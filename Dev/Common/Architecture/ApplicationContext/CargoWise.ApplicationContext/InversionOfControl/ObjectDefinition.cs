using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Xml.Serialization;
using CargoWise.Application.Exceptions;
using static System.FormattableString;

namespace CargoWise.Application.InversionOfControl
{
	/// <summary>
	/// Defines an object definition.
	/// </summary>
	[XmlRoot("object", Namespace = "http://www.springframework.net")]
	[XmlSerializerAssembly("CargoWise.ApplicationContext.XmlSerializers")]
	public sealed class ObjectDefinition
	{
		ConcurrentDictionary<Type, IState> cache;

		public ObjectDefinition()
		{
		}

		/// <summary>
		/// Gets/Sets the specified constructor arguments.
		/// </summary>
		[XmlElement("constructor-arg")]
		public ObjectConstructorArgumentDefinition[] ConstructorArguments { get; set; }

		/// <summary>
		/// Gets the default constructor (if exists)
		/// </summary>
		public Func<object> GetParameterLessConstructor(Type requestedType)
		{
			return GetState(requestedType).GenericParameterlessConstructor;
		}

		ConcurrentDictionary<Type, IState> GetCache()
		{
			if (cache == null)
			{
				// Dictionary will only be created if needed, and if needed will most likely need capacity only 1 (to reduce memory impact)
				Interlocked.CompareExchange(ref cache, new ConcurrentDictionary<Type, IState>(concurrencyLevel: 1, capacity: 1), null);
			}

			return cache;
		}

		Func<object> GetParameterlessConstructorCore(Type requestedType)
		{
			var type = GetType(requestedType);
			var constructorInfo = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
			return constructorInfo == null
				? throw new CannotCreateObjectException(string.Format(CultureInfo.InvariantCulture, "Unable to find a constructor with no arguments for target type \"{0}\".", type.AssemblyQualifiedName))
				: Expression.Lambda<Func<object>>(Expression.New(constructorInfo)).Compile();
		}

		class TypeArrayEqualityComparer : IEqualityComparer<Type[]>
		{
			public bool Equals(Type[] x, Type[] y)
			{
				if (x.Length != y.Length)
				{
					return false;
				}

				for (int i = 0; i < x.Length; i++)
				{
					if (x[i] != y[i])
					{
						return false;
					}
				}

				return true;
			}

			public int GetHashCode(Type[] array)
			{
				var result = 0;
				unchecked
				{
					foreach (var type in array)
					{
						if (type != null)
						{
							result += type.GetHashCode();
						}
					}
				}
				return result;
			}
		}

		class TypeEqualityComparer : IEqualityComparer<Type>
		{
			public bool Equals(Type x, Type y)
			{
				if (x == null)
				{
					return false;
				}
				else if (y == null)
				{
					return IsNullableType(x);
				}
				else
				{
					return x.IsAssignableFrom(y);
				}
			}

			public int GetHashCode(Type obj)
			{
				return obj != null ? obj.GetHashCode() : 0;
			}

			static bool IsNullableType(Type type)
			{
				return type.IsClass || type.IsInterface || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
			}
		}

		static bool IsNormalParameter(ParameterInfo parameter)
		{
			return !parameter.IsOptional && !parameter.IsOut && !parameter.ParameterType.IsByRef;
		}

		public Func<object[], object> GetConstructorBasedOnArguments(Type requestedType, object[] arguments)
		{
			var type = GetType(requestedType);
			var argTypeList = arguments.Select(arg => arg?.GetType()).ToArray();
			return GetState(type).ConstructorCache.GetOrAdd(argTypeList, (argTypeCollection) => GetConstructorBasedOnArgumentsCore(requestedType, argTypeCollection));
		}

		Func<object[], object> GetConstructorBasedOnArgumentsCore(Type requestedType, Type[] argTypeList)
		{
			var type = GetType(requestedType);
			var compatibleConstructors =
			(
				from ci in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				let parameters = ci.GetParameters()
				where
					parameters.All(IsNormalParameter) &&
					parameters.Select(pi => pi.ParameterType).SequenceEqual(argTypeList, new TypeEqualityComparer())
				select ci
			).ToArray();

			ConstructorInfo compatibleConstructor;

			if (compatibleConstructors.Length == 1)
			{
				compatibleConstructor = compatibleConstructors[0];
			}
			// The null check on argument types is needed as GetConstructor throws an Exception on null values.
			else if (compatibleConstructors.Length > 1 && argTypeList.All(a => a != null))
			{
				// Type.GetConstructor is used to resolve ambiguous cases.
				// Consider the following case:
				// *   constructor 1 with an argument taking class T
				// *   constructor 2 with an argument taking class U: T
				// Type.GetConstructor will choose constructor 1 in this case, which is what we want
				compatibleConstructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, argTypeList, null);
			}
			else
			{
				compatibleConstructor = null;
			}

			if (compatibleConstructor == null)
			{
				throw new CannotCreateObjectException(string.Format(CultureInfo.InvariantCulture, "Unable to find a unique compatible constructor on the target type \"{0}\".", type.AssemblyQualifiedName));
			}

			var (constructorArguments, lambdaParameter) = GenerateParameterExpression(compatibleConstructor);
			var lambda = Expression.Lambda<Func<object[], object>>(Expression.New(compatibleConstructor, constructorArguments), lambdaParameter);
			// lambda contains an expression tree equivalent to the following code:
			// (object[] args) => new T((U) args[0], (V) args[1], ... );
			return lambda.Compile();
		}

		public Func<object[], object> GetSinglePublicConstructorForFactoryNew(Type requestedType)
		{
			return GetState(requestedType).SinglePublicConstructorForFactoryNew;
		}

		Func<object[], object> GetSinglePublicConstructorForFactoryNewCore(Type requestedType)
		{
			var type = GetType(requestedType);
			var constructors = type.GetConstructors();
			if (constructors.Length != 1)
			{
				throw new CannotLoadObjectTypeException(string.Format(CultureInfo.InvariantCulture, "New<T> can only be used where there is only 1 public constructor. TypeName [{0}] has {1} constructors.", type.FullName, constructors.Length));
			}

			var constructor = constructors[0];
			var (constructorArguments, lambdaParameter) = GenerateParameterExpression(constructor);
			var lambda = Expression.Lambda<Func<object[], object>>(Expression.New(constructor, constructorArguments), lambdaParameter);
			// lambda contains an expression tree equivalent to the following code:
			// (object[] args) => new T((U) args[0], (V) args[1], ... );
			return lambda.Compile();
		}

		/// <summary>
		/// Gets the factory method (if exists)
		/// Throws AmbiguousMatchException if there are multiple methods with the factory method name.
		/// Throws Exception if there is no method with the factory method name.
		/// </summary>
		public Func<object[], object> GetFactoryMethod(Type requestedType)
		{
			return GetState(requestedType).FactoryMethod;
		}

		Func<object[], object> GetFactoryMethodCore(Type requestedType)
		{
			var type = GetType(requestedType);
			if (!string.IsNullOrEmpty(FactoryMethodName))
			{
				var factoryMethodInfo = type.GetMethod(FactoryMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				if (factoryMethodInfo == null)
				{
					throw new CannotCreateObjectException(string.Format(CultureInfo.InvariantCulture, "object with id '{0}' has a factory-method of '{1}' which does not exist on type '{2}'", Name, FactoryMethodName, type.Name));
				}
				else
				{
					var (parameters, lambdaParameter) = GenerateParameterExpression(factoryMethodInfo);
					var lambda = Expression.Lambda<Func<object[], object>>(Expression.Call(factoryMethodInfo, parameters), lambdaParameter);
					// lambda contains an expression tree equivalent to the following code:
					// (object[] args) => function((T) args[0], (U) args[1], ... );
					return lambda.Compile();
				}
			}

			return null;
		}

		(UnaryExpression[], ParameterExpression) GenerateParameterExpression(MethodBase method)
		{
			var lambdaParameter = Expression.Parameter(typeof(object[]));
			var parameters = method.GetParameters().Select((parameter, index) =>
				Expression.Convert(
					Expression.ArrayIndex(
						lambdaParameter,
						Expression.Constant(index)
					),
					parameter.ParameterType
				)
			).ToArray();

			return (parameters, lambdaParameter);
		}

		/// <summary>
		/// Gets/Sets the factory method name.
		/// </summary>
		[XmlAttribute("factory-method")]
		public string FactoryMethodName { get; set; }

		/// <summary>
		/// Gets/Sets the object name.
		/// </summary>
		[XmlAttribute("id")]
		public string Name { get; set; }

		/// <summary>
		/// Gets/Sets whether the object is singleton.
		/// </summary>
		[XmlAttribute("singleton")]
		public bool IsSingleton { get; set; }

		/// <summary>
		/// Gets/Sets the object property definitions.
		/// </summary>
		[XmlElement("property")]
		public ObjectPropertyDefinition[] PropertyDefinitions { get; set; }

		public ObjectDefinition Clone()
		{
			var result = new ObjectDefinition();
			result.ConstructorArguments = ConstructorArguments?.Select(x => x?.Clone()).ToArray();
			result.FactoryMethodName = FactoryMethodName;
			result.Name = Name;
			result.IsSingleton = IsSingleton;
			result.PropertyDefinitions = PropertyDefinitions?.Select(x => x?.Clone()).ToArray();
			result.Singleton = Singleton;
			result.TypeName = TypeName;
			return result;
		}

		/// <summary>
		/// Gets the factory method (if exists)
		/// Throws AmbiguousMatchException if there are multiple methods with the factory method name.
		/// </summary>
		public Action<object> GetPropertySetter(Type requestedType)
		{
			return GetState(requestedType).PropertySetter;
		}

		Action<object> GetPropertySetterCore(Type requestedType)
		{
			var type = GetType(requestedType);
			if (PropertyDefinitions != null && PropertyDefinitions.Any())
			{
				var allAssignExpressions = new List<Expression>();
				var instanceParameter = Expression.Parameter(typeof(object));

				// cast to its actual type
				var instanceVariable = Expression.Variable(type);
				allAssignExpressions.Add(
					Expression.Assign(
						instanceVariable,
						Expression.Convert(instanceParameter, type)
					)
				);

				// generate property/field setters
				foreach (ObjectPropertyDefinition propertyDefinition in PropertyDefinitions)
				{
					var propertyName = propertyDefinition.Name;

					if (string.IsNullOrEmpty(propertyName))
					{
						// Ignore a property without a name.
						continue;
					}

					var (memberExpression, memberType) = GetMemberExpressionAndMemberType(requestedType, instanceVariable, propertyName);

					if (!string.IsNullOrEmpty(propertyDefinition.Value))
					{
						allAssignExpressions.Add(GetAssignMemberToPropertyDefinitionValueExpression(propertyDefinition, memberExpression, memberType));
					}
					else if (propertyDefinition.ObjectValue != null)
					{
						allAssignExpressions.Add(
							Expression.Assign(
								memberExpression,
								Expression.Convert(
									Expression.Call(
										GetWithoutSecurityCheckForDefinitionMethodInfo.Value,
										Expression.Constant(memberType),
										Expression.Constant(propertyDefinition.ObjectValue),
										Expression.Constant(Array.Empty<object>())
									),
									memberType
								)
							)
						);
					}
					else if (propertyDefinition.ListValues != null && propertyDefinition.ListValues.Any())
					{
						allAssignExpressions.Add(GetAssignMemberToPropertyDefinitionListExpression(requestedType, propertyDefinition, memberExpression, memberType));
					}
					else if (propertyDefinition.DictionaryValues != null && propertyDefinition.DictionaryValues.Any())
					{
						allAssignExpressions.Add(GetAssignMemberToPropertyDefinitionDictionaryExpression(requestedType, propertyDefinition, memberExpression, memberType));
					}
				}

				var block = Expression.Block(new[] { instanceVariable }, allAssignExpressions);
				var lambda = Expression.Lambda<Action<object>>(block, instanceParameter);
				// lambda contains an expression tree equivalent to the following code, one of each type of property that it can handle is demonstrated:
				// (object instance) => {
				//     InstanceType instanceVariable = (InstanceType)instance;
				//     instanceVariable.foo = "Some String";
				//     instanceVariable.bar = typeof(SomeType);
				//     instanceVariable.baz = (PropertyType) GetWithoutSecurityCheck("TypeSpecifiedByString");
				//     instanceVariable.qux = (PropertyType) GetWithoutSecurityCheckForDefinition(objectDefinition);
				//
				// IF propertyDefinition.EnableParallelInit THEN
				//    new List<string> { "TypeSpecifiedByString", "TypeSpecifiedByString2" }.AsParallel().Select(param => param.GetWithoutSecurityCheck(memberType, param, new[] { }));
				// ELSE
				//     instanceVariable.someList = new ArrayList() { GetWithoutSecurityCheck("TypeSpecifiedByString"), GetWithoutSecurityCheck("TypeSpecifiedByString2"), ... };
				// ENDIF
				//     {
				//         var hashtable = new Hashtable();
				//         hashtable.Add("Key1", "Value1");
				//         hashtable.Add("Key2", "Value2");
				//         ...
				//         instanceVariable.someDictionary = hashtable;
				//     }
				//     ...
				// }
				return lambda.Compile();
			}

			return null;
		}

		(MemberExpression Expression, Type Type) GetMemberExpressionAndMemberType(Type requestedType, ParameterExpression instanceVariable, string propertyName)
		{
			var type = GetType(requestedType);
			(MemberExpression, Type) result;

			var property = type.GetProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName));
			if (property == null)
			{
				var field = type.GetFields().FirstOrDefault(f => string.Equals(f.Name, propertyName))
							?? throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The property or field name \"{0}\" does not exist in the type \"{1}\".", propertyName, type.FullName));

				result = (Expression.Field(instanceVariable, field), field.FieldType);
			}
			else
			{
				result = (Expression.Property(instanceVariable, property), property.PropertyType);
			}

			return result;
		}

		BinaryExpression GetAssignMemberToPropertyDefinitionValueExpression(ObjectPropertyDefinition propertyDefinition, MemberExpression memberExpression, Type memberType)
		{
			BinaryExpression result;

			if (memberType == typeof(string))
			{
				result = Expression.Assign(memberExpression, Expression.Constant(propertyDefinition.Value));
			}
			else if (memberType == typeof(Type))
			{
				result = Expression.Assign(memberExpression, Expression.Constant(propertyDefinition.ValueOfType));
			}
			// More property value type supports can be added here.
			else
			{
				result = Expression.Assign(
					memberExpression,
					Expression.Convert(
						Expression.Call(
							GetWithoutSecurityCheckMethodInfo.Value,
							Expression.Constant(memberType),
							Expression.Constant(propertyDefinition.Value),
							Expression.Constant(Array.Empty<object>())
						),
						memberType
					)
				);
			}

			return result;
		}

		BinaryExpression GetAssignMemberToPropertyDefinitionListExpression(Type requestedType, ObjectPropertyDefinition propertyDefinition, MemberExpression memberExpression, Type memberType)
		{
			if (!typeof(IList).IsAssignableFrom(memberType))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The type of the property or field \"{0}\" of type \"{1}\" must be a subclass of IList.", propertyDefinition.Name, GetType(requestedType).FullName));
			}

			if (propertyDefinition.EnableParallelInit)
			{
				var arrayElements = propertyDefinition.ListValues.Select(plv =>
										Expression.ElementInit(
											AddOnListMethodInfo.Value,
											Expression.Constant(plv.ObjectName)
										)
									).ToArray();

				var param = Expression.Parameter(typeof(string));
				var lambda = Expression.Lambda<Func<string, object>>(
									Expression.Call(
										GetWithoutSecurityCheckMethodInfo.Value,
										Expression.Constant(memberType),
										param,
										Expression.Constant(Array.Empty<object>())
									),
									param
								);
				var parallel = Expression.Call(
									ParallelEnumerableAsParallelMethodInfo.Value,
									Expression.ListInit(
										Expression.New(typeof(List<string>)),
										arrayElements
									)
								);
				var toArrayArgs = Expression.Call(
										ParallelEnumerableSelectMethodInfo.Value,
										parallel,
										lambda
									);

				return Expression.Assign(
					memberExpression,
					Expression.New(
						typeof(ArrayList).GetConstructor(new[] { typeof(ICollection) }),
						Expression.Call(
							ParallelEnumerableToArrayMethodInfo.Value,
							toArrayArgs
						)
					)
				);
			}
			else
			{
				var arrayElements = propertyDefinition.ListValues.Select(plv =>
										Expression.ElementInit(
											AddOnArrayListMethodInfo.Value,
											Expression.Call(
												GetWithoutSecurityCheckMethodInfo.Value,
												Expression.Constant(memberType),
												Expression.Constant(plv.ObjectName),
												Expression.Constant(Array.Empty<object>())
											)
										)
									).ToArray();

				return Expression.Assign(
						memberExpression,
						Expression.ListInit(
							Expression.New(typeof(ArrayList)),
							arrayElements
						)
					);
			}
		}

		BinaryExpression GetAssignMemberToPropertyDefinitionDictionaryExpression(Type requestedType, ObjectPropertyDefinition propertyDefinition, MemberExpression memberExpression, Type memberType)
		{
			if (!typeof(IDictionary).IsAssignableFrom(memberType))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The type of the property or field \"{0}\" of type \"{1}\" must be a subclass of IDictionary.", propertyDefinition.Name, GetType(requestedType).FullName));
			}

			var pairs = propertyDefinition.DictionaryValues.Select(pair =>
				Expression.ElementInit(
					AddOnIDictionaryMethodInfo.Value,
					Expression.Constant(pair.Key.Value),
					Expression.Constant(pair.Value)
				)
			).ToArray();

			return Expression.Assign(
				memberExpression,
				Expression.ListInit(
					Expression.New(typeof(Hashtable)),
					pairs
				)
			);
		}

		static readonly Lazy<MethodInfo> GetWithoutSecurityCheckMethodInfo = new Lazy<MethodInfo>(() => typeof(ObjectFactory).GetMethod(nameof(ObjectFactory.GetWithoutSecurityCheckCore), BindingFlags.NonPublic | BindingFlags.Static));
		static readonly Lazy<MethodInfo> GetWithoutSecurityCheckForDefinitionMethodInfo = new Lazy<MethodInfo>(() => typeof(ObjectFactory).GetMethod(nameof(ObjectFactory.GetWithoutSecurityCheckForDefinitionCore), BindingFlags.NonPublic | BindingFlags.Static));
		static readonly Lazy<MethodInfo> AddOnIDictionaryMethodInfo = new Lazy<MethodInfo>(() => typeof(IDictionary).GetMethod(nameof(IDictionary.Add)));
		static readonly Lazy<MethodInfo> AddOnArrayListMethodInfo = new Lazy<MethodInfo>(() => typeof(ArrayList).GetMethod(nameof(ArrayList.Add)));
		static readonly Lazy<MethodInfo> AddOnListMethodInfo = new Lazy<MethodInfo>(() => typeof(List<string>).GetMethod(nameof(List<string>.Add), new[] { typeof(string) }));
		static readonly Lazy<MethodInfo> ParallelEnumerableAsParallelMethodInfo = new Lazy<MethodInfo>(() => typeof(ParallelEnumerable).GetMethods().Where(m => m.Name == nameof(ParallelEnumerable.AsParallel)).First().MakeGenericMethod(new[] { typeof(string) }));
		static readonly Lazy<MethodInfo> ParallelEnumerableSelectMethodInfo = new Lazy<MethodInfo>(() => typeof(ParallelEnumerable).GetMethods().Where(m => m.Name == nameof(ParallelEnumerable.Select)).First().MakeGenericMethod(new[] { typeof(string), typeof(object) }));
		static readonly Lazy<MethodInfo> ParallelEnumerableToArrayMethodInfo = new Lazy<MethodInfo>(() => typeof(ParallelEnumerable).GetMethods().Where(m => m.Name == nameof(ParallelEnumerable.ToArray)).First().MakeGenericMethod(new[] { typeof(object) }));

		/// <summary>
		/// Gets the object type.
		/// </summary>
		public Type GetType(Type requestedType)
		{
			return GetType(requestedType, this);
		}

		static Type GetType(Type requestedType, ObjectDefinition definition)
		{
			return GetTypeWithCache(requestedType, definition);
		}

		static Type TryGetType(Type requestedType, ObjectDefinition definition)
		{
			try
			{
				var type = Type.GetType(definition.TypeName, true, true);
				if (requestedType.IsGenericType)
				{
					type = type.MakeGenericType(requestedType.GetGenericArguments());
				}

				return type;
			}
			catch (Exception ex)
			{
				var message = Invariant($"The type associated with the name '{definition.Name}' cannot be resolved. TypeName is '{definition.TypeName}'.");
				throw new CannotLoadObjectTypeException(message, ex);
			}
		}

		#region Caching

		internal class ResolvedTypeCache
		{
			public ResolvedTypeCache(string key, Type resolvedType)
			{
				Key = key;
				ResolvedType = resolvedType;
			}

			public string Key { get; }
			public Type ResolvedType { get; }
		}

		ResolvedTypeCache LastResolvedTypeCache { get; set; }

#if DEBUG

		internal ResolvedTypeCache LastResolvedTypeCacheForTest
		{
			get => LastResolvedTypeCache;
			set => LastResolvedTypeCache = value;
		}

#endif

		static Type GetTypeWithCache(Type requestedType, ObjectDefinition definition)
		{
			var cacheKey = GetTypeDefinitionCacheKey(requestedType, definition);

			var lastResolvedType = definition.LastResolvedTypeCache;

			if (lastResolvedType != null && lastResolvedType.Key.Equals(cacheKey))
			{
				return lastResolvedType.ResolvedType;
			}

			var resolvedType = TryGetType(requestedType, definition);
			if (resolvedType != null)
			{
				definition.LastResolvedTypeCache = new ResolvedTypeCache(cacheKey, resolvedType);
			}

			return resolvedType;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Cache Key")]
		internal static string GetTypeDefinitionCacheKey(Type requestedType, ObjectDefinition definition)
		{
			var cacheKey = definition.TypeName;

			if (requestedType.IsGenericType)
			{
				cacheKey += "|Args:" + string.Join(",", requestedType.GetGenericArguments().Select(t => t.FullName));
			}

			return cacheKey;
		}

		#endregion

		/// <summary>
		/// Gets/Sets the singleton instance.
		/// </summary>
		[XmlIgnore]
		public object Singleton { get; set; }

		/// <summary>
		/// Gets/Sets the object type name.
		/// </summary>
		[XmlAttribute("type")]
		public string TypeName { get; set; }

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Multiple State creation in the advent of the race is fine")]
		IState GetState(Type type)
		{
			var stateCache = GetCache();
			if (!stateCache.TryGetValue(type, out var state))
			{
				state = new State(this, type);
				stateCache.TryAdd(type, state);
			}

			return state;
		}

		interface IState
		{
			Func<object> GenericParameterlessConstructor { get; }
			Func<object[], object> SinglePublicConstructorForFactoryNew { get; }
			Func<object[], object> FactoryMethod { get; }
			Type Type { get; }
			Action<object> PropertySetter { get; }
			ConcurrentDictionary<Type[], Func<object[], object>> ConstructorCache { get; }
		}

		class State : IState
		{
			readonly Lazy<Func<object>> genericParameterlessConstructor;
			readonly Lazy<Func<object[], object>> singlePublicConstructorForFactoryNew;
			readonly Lazy<Func<object[], object>> factoryMethod;
			readonly Lazy<Type> type;
			readonly Lazy<Action<object>> propertySetter;

			public State(ObjectDefinition definition, Type requestedType)
			{
				type = new Lazy<Type>(() => ObjectDefinition.GetType(requestedType, definition), LazyThreadSafetyMode.PublicationOnly);
				genericParameterlessConstructor = new Lazy<Func<object>>(() => definition.GetParameterlessConstructorCore(requestedType), LazyThreadSafetyMode.PublicationOnly);
				singlePublicConstructorForFactoryNew = new Lazy<Func<object[], object>>(() => definition.GetSinglePublicConstructorForFactoryNewCore(requestedType), LazyThreadSafetyMode.PublicationOnly);
				factoryMethod = new Lazy<Func<object[], object>>(() => definition.GetFactoryMethodCore(requestedType), LazyThreadSafetyMode.PublicationOnly);
				propertySetter = new Lazy<Action<object>>(() => definition.GetPropertySetterCore(requestedType), LazyThreadSafetyMode.PublicationOnly);
			}

			public Func<object> GenericParameterlessConstructor => genericParameterlessConstructor.Value;
			public Func<object[], object> SinglePublicConstructorForFactoryNew => singlePublicConstructorForFactoryNew.Value;
			public Func<object[], object> FactoryMethod => factoryMethod.Value;
			public Type Type => type.Value;
			public Action<object> PropertySetter => propertySetter.Value;
			public ConcurrentDictionary<Type[], Func<object[], object>> ConstructorCache { get; } = new ConcurrentDictionary<Type[], Func<object[], object>>(new TypeArrayEqualityComparer());
		}
	}
}
