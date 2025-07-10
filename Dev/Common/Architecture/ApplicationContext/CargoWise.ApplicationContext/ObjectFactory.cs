using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using CargoWise.Application.Exceptions;
using CargoWise.Application.InversionOfControl;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;
using static System.FormattableString;

namespace CargoWise.Application
{
	public static class ObjectFactory
	{
		[ThreadSafe]
		static readonly ConcurrentDictionary<string, ObjectDefinitions> objectDefinitionsCache = new ConcurrentDictionary<string, ObjectDefinitions>();

		[ThreadSafe]
		static readonly ConcurrentDictionary<string, ObjectDefinition> objectDefinitionCache = new ConcurrentDictionary<string, ObjectDefinition>();

		#region Configuration

		/// <summary>
		/// Configures the object factory using a specific configuration resource from assembly.
		/// </summary>
		/// <param name="assemblyFilePath">The assembly file path.</param>
		/// <param name="configurationResourceName">The configuration resource name in the assembly.</param>
		public static void ConfigureUsingFile(string assemblyFilePath, string configurationResourceName)
		{
			var definitions = ObjectDefinitions.CreateUsingFile(assemblyFilePath, configurationResourceName);
			Configure(configurationResourceName, definitions);
		}

		/// <summary>
		/// Configures the object factory using a specific configuration resource file.
		/// </summary>
		/// <param name="configurationResourceFileUri">The URI of the configuration resource file.</param>
		public static void Configure(string configurationResourceFileUri)
		{
			Argument.NotNullOrEmpty(configurationResourceFileUri, nameof(configurationResourceFileUri));
			var definitions = ObjectDefinitions.Create(configurationResourceFileUri);
			Configure(configurationResourceFileUri, definitions);
		}

		static void Configure(string configurationResourceFileUri, ObjectDefinitions definitions)
		{
			Argument.NotNull(definitions, nameof(definitions));

			if (definitions.Definitions == null)
			{
				return;
			}

			foreach (var definition in definitions.Definitions)
			{
				objectDefinitionCache.AddOrUpdate(definition.Name, definition, (key, oldValue) =>
				{
					var newValue = definition;
					var result = newValue;
					if (newValue.PropertyDefinitions?.FirstOrDefault() is ObjectPropertyDefinition propertyDefinition &&
						oldValue.PropertyDefinitions?.FirstOrDefault() is ObjectPropertyDefinition oldPropertyDefinition)
					{
						var propertyDefinitionIsSubSet = propertyDefinition.IsSubSet;
						var oldPropertyDefinitionIsSubSet = oldPropertyDefinition.IsSubSet;
						if (propertyDefinitionIsSubSet != oldPropertyDefinitionIsSubSet)
						{
							throw new InvalidOperationException($"'{key}' in {configurationResourceFileUri} has SubSet set to {propertyDefinitionIsSubSet} however another configuration has it set {oldPropertyDefinitionIsSubSet}.");
						}
						if (propertyDefinitionIsSubSet)
						{
							if (oldPropertyDefinition.DictionaryValues != null || propertyDefinition.DictionaryValues != null)
							{
								result = oldValue.Clone();
								var clonedPropertyDefinition = result.PropertyDefinitions[0];
								clonedPropertyDefinition.DictionaryValues = Combine(configurationResourceFileUri, key, "SourceDictionary", oldPropertyDefinition.DictionaryValues, propertyDefinition.DictionaryValues, (x) => x.Key.Value, x => x.Clone());
								clonedPropertyDefinition.EnableParallelInit = clonedPropertyDefinition.EnableParallelInit || propertyDefinition.EnableParallelInit;
							}
							else if (oldPropertyDefinition.ListValues != null || propertyDefinition.ListValues != null)
							{
								result = oldValue.Clone();
								var clonedPropertyDefinition = result.PropertyDefinitions[0];
								clonedPropertyDefinition.ListValues = Combine(configurationResourceFileUri, key, "SourceList", oldPropertyDefinition.ListValues, propertyDefinition.ListValues, (x) => x.ObjectName, x => x.Clone());
								clonedPropertyDefinition.EnableParallelInit = clonedPropertyDefinition.EnableParallelInit || propertyDefinition.EnableParallelInit;
							}
							else
							{
								throw new InvalidOperationException($"'{key}' in {configurationResourceFileUri} is configured to use SubSet however SubSet can only be used for either SourceList or SourceDictionary.");
							}
						}
					}
					return result;
				});
			}

			objectDefinitionsCache.AddOrUpdate(configurationResourceFileUri, definitions, (key, oldValue) => definitions);
		}

		static T[] Combine<T>(string configurationResourceFileUri, string definitionName, string valueType, T[] oldValues, T[] newValues, Func<T, string> getKey, Func<T, T> getClone)
		{
			if (oldValues == null)
			{
				throw new InvalidOperationException($"'{definitionName}' in {configurationResourceFileUri} is configured to use SubSet {valueType} however another configuration file isn't using {valueType}.");
			}
			else if (newValues == null)
			{
				throw new InvalidOperationException($"'{definitionName}' in {configurationResourceFileUri} is not configured to use SubSet {valueType} however another configuration file is using {valueType}.");
			}
			var dictionaryValues = oldValues.ToDictionary(x => getKey(x), y => y);
			newValues.ForEach(x => dictionaryValues[getKey(x)] = x);
			return dictionaryValues.Values.Select(x => getClone(x)).ToArray();
		}

		/// <summary>
		/// Gets whether the object factory is configured.
		/// </summary>		
		public static bool IsConfigured => !objectDefinitionsCache.IsEmpty;

		/// <summary>
		/// Unconfigures an existing configuration file off the object factory.
		/// </summary>
		/// <param name="configurationFile">The configuration file.</param>
		public static void UnconfigureUsingFile(string configurationFile) => UnconfigureCore(configurationFile);

		/// <summary>
		/// Unconfigures an existing configuration resource file off the object factory.
		/// </summary>
		/// <param name="configurationResourceFileUri">The URI of the configuration resource file.</param>
		public static void Unconfigure(string configurationResourceFileUri) => UnconfigureCore(configurationResourceFileUri);

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Racing removers cannot cause incorrect behaviour")]
		static void UnconfigureCore(string configurationResourceFileUri)
		{
			Argument.NotNullOrEmpty(configurationResourceFileUri, nameof(configurationResourceFileUri));

			if (objectDefinitionsCache.TryGetValue(configurationResourceFileUri, out var objectDefinitionsToBeRemoved))
			{
				if (objectDefinitionsToBeRemoved != null && objectDefinitionsToBeRemoved.Definitions != null)
				{
					foreach ((string key, ObjectDefinition definition) in objectDefinitionsToBeRemoved.Definitions.Select(d => (d.Name, d)))
					{
						if (objectDefinitionCache.TryRemove(key, out var cachedValue)
							&& cachedValue.PropertyDefinitions?.FirstOrDefault() is ObjectPropertyDefinition cachedPropertyDefinition
							&& definition.PropertyDefinitions?.FirstOrDefault() is ObjectPropertyDefinition propertyDefinition && propertyDefinition.IsSubSet)
						{
							if (GetCloneObjectDefinitionToReAdd(cachedValue, cachedPropertyDefinition, propertyDefinition, (d) => d.DictionaryValues, (d, values) => d.DictionaryValues = values, x => x.Key.Value, x => x.Clone())
								is ObjectDefinition newValueWithRemovedDictionaryValues)
							{
								objectDefinitionCache.AddOrUpdate(key, newValueWithRemovedDictionaryValues, (key, oldValue) => newValueWithRemovedDictionaryValues);
							}
							else if (GetCloneObjectDefinitionToReAdd(cachedValue, cachedPropertyDefinition, propertyDefinition, (d) => d.ListValues, (d, values) => d.ListValues = values, x => x.ObjectName, x => x.Clone())
								is ObjectDefinition newValueWithRemovedListValues)
							{
								objectDefinitionCache.AddOrUpdate(key, newValueWithRemovedListValues, (key, oldValue) => newValueWithRemovedListValues);
							}
						}
					}
				}

				objectDefinitionsCache.TryRemove(configurationResourceFileUri, out _);
			}
		}

		static ObjectDefinition GetCloneObjectDefinitionToReAdd<T>(ObjectDefinition cachedValue, ObjectPropertyDefinition cachedPropertyDefinition, ObjectPropertyDefinition propertyDefinition, Func<ObjectPropertyDefinition, T[]> getValues, Action<ObjectPropertyDefinition, T[]> setValues, Func<T, string> getKey, Func<T, T> getClone)
		{
			ObjectDefinition result = null;
			var values = getValues(propertyDefinition);
			var cachedValues = getValues(cachedPropertyDefinition);
			if (values != null
				&& cachedValues != null
				&& values.Length != cachedValues.Length)
			{
				var dictionaryValues = cachedValues.ToDictionary(x => getKey(x), y => y);
				values.Select(x => getKey(x)).ForEach(key => dictionaryValues.Remove(key));
				if (dictionaryValues.Count > 0)
				{
					result = cachedValue.Clone();
					var clonedPropertyDefinition = result.PropertyDefinitions[0];
					setValues(clonedPropertyDefinition, dictionaryValues.Values.Select(x => getClone(x)).ToArray());
				}
			}
			return result;
		}

		#endregion

		#region Helpers
		static ObjectDefinition GetObjectDefinition(string name)
		{
			Argument.NotNull(name, nameof(name));
			objectDefinitionCache.TryGetValue(name, out ObjectDefinition result);
			return result;
		}

		/// <summary>
		/// Gets the object definition name of a specific type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The type name.</returns>
		internal static string GetObjectDefinitionName(Type type)
		{
			Argument.NotNull(type, nameof(type));

			if (!type.IsGenericType)
			{
				return type.DeclaringType?.DeclaringType != null ? (type.DeclaringType.Name + "." + type.Name) : type.Name;
			}

			var definitionName = GetGenericTypeDefinitionName(type.GetGenericTypeDefinition().Name);
			return definitionName;
		}

		static string GetGenericTypeDefinitionName(string name)
		{
			return name.Split('`')[0];
		}

		#endregion

		#region Type Resolution
		/// <summary>
		/// Gets the implementation type of a specific service type.
		/// </summary>
		/// <typeparam name="T">The service type.</typeparam>
		/// <returns>The implementation type.</returns>
		public static Type GetType<T>()
		{
			var name = GetObjectDefinitionName(typeof(T));
			return GetTypeWithoutSecurityCheck<T>(name);
		}

		/// <summary>
		/// Gets the implementation type of a specific service type.
		/// </summary>
		/// <param name="type">The service type.</param>
		/// <returns>The implementation type.</returns>
		public static Type GetType(Type type)
		{
			Argument.NotNull(type, nameof(type));
			var name = GetObjectDefinitionName(type);
			return GetTypeWithoutSecurityCheck(type, name);
		}

		/// <summary>
		/// This method is intended to be used internally.
		/// DO NOT CALL THIS METHOD DIRECTLY
		/// </summary>
		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Type GetType(string name, Assembly callingAssembly)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			return GetTypeWithoutSecurityCheck<EmptyType>(name);
		}

		/// <summary>
		/// This method is intended to be used internally.
		/// DO NOT CALL THIS METHOD DIRECTLY
		/// </summary>
		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Type GetType(Type type, Assembly callingAssembly)
		{
			Argument.NotNull(type, nameof(type));
			var name = GetObjectDefinitionName(type);
			return GetTypeWithoutSecurityCheck(type, name);
		}

		/// <summary>
		/// Gets the implementation type by its associated name.
		/// </summary>
		/// <param name="name">The associated name.</param>
		/// <returns>The implementation type.</returns>
		public static Type GetType(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			return GetTypeWithoutSecurityCheck<EmptyType>(name);
		}

		/// <summary>
		/// Gets the implementation type by its associated name in designer.
		/// </summary>
		/// <typeparam name="T">The service type.</typeparam>
		/// <returns>The implementation type.</returns>
		public static Type GetTypeDesignerSafe<T>()
		{
			var type = GetObjectDefinitionName(typeof(T));
			return GetTypeDesignerSafe(type, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Gets the implementation type by its associated name in designer.
		/// </summary>
		/// <param name="name">The associated name.</param>
		/// <returns>The implementation type.</returns>
		public static Type GetTypeDesignerSafe(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			return GetTypeDesignerSafe(name, Assembly.GetCallingAssembly());
		}

		/// <summary>
		/// Gets the implementation type by its associated name in designer.
		/// </summary>
		/// <param name="name">The associated name.</param>
		/// <param name="callingAssembly">The calling assembly.</param>
		/// <returns>The implementation type.</returns>
		public static Type GetTypeDesignerSafe(string name, Assembly callingAssembly)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			return DesignTimeObjectFactory.GetTypeDesignerSafe(name);
		}

		/// <summary>
		/// Gets the implementation type by its associated name without any security check.
		/// </summary>
		/// <param name="name">The associated name.</param>
		/// <returns>The implementation type.</returns>
		internal static Type GetTypeWithoutSecurityCheck<T>(string name, params object[] args)
		{
			return GetTypeWithoutSecurityCheck(typeof(T), name, args);
		}

		internal static Type GetTypeWithoutSecurityCheck(Type type, string name, params object[] args)
		{
			Argument.NotNull(name, nameof(name));
#if DEBUG
			if (substitutions.TryGetValue(name, out var factory))
			{
				return factory.Invoke(args).GetType();
			}
#endif
			var objectDefinition = GetObjectDefinition(name);
			return objectDefinition == null
				? throw new NoSuchObjectDefinitionException(name)
				: objectDefinition.GetType(type);
		}
		#endregion

		#region Object Resolution
		/// <summary>
		/// Gets an object of a specific type.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>The object.</returns>
		public static T Get<T>()
		{
			var type = typeof(T);
			var name = GetObjectDefinitionName(type);
			var obj = GetWithoutSecurityCheckCore(type, name);
			return (T)obj;
		}

		/// <summary>
		/// Gets an object of a specific type.
		/// </summary>
		/// <param name="serviceType">The object type.</typeparam>
		/// <returns>The object.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static object Get(Type serviceType)
		{
			var name = GetObjectDefinitionName(serviceType);
			var obj = GetWithoutSecurityCheckCore(serviceType, name);
			return obj;
		}

		/// <summary>
		/// Gets an object of a specific type associated with a specific name.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		/// <param name="name">The associated name.</param>
		/// <returns>The object.</returns>
		public static T Get<T>(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			var obj = GetWithoutSecurityCheckCore(typeof(T), name);
			return (T)obj;
		}

		/// <summary>
		/// Gets an object of a specific type associated with a specific name.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		/// <param name="name">The associated name.</param>
		/// <param name="arguments">The arguments passed to the object constructor.</param>
		/// <returns>The object.</returns>
		public static T Get<T>(string name, params object[] arguments)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			var obj = GetWithoutSecurityCheckCore(typeof(T), name, arguments);

			return (T)obj;
		}

		/// <summary>
		/// Gets an object with an associated name.
		/// </summary>
		/// <param name="name">The associated name.</param>
		/// <returns>The object.</returns>
		public static object Get(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			return GetWithoutSecurityCheck(name);
		}

		/// <summary>
		/// Gets an object with an associated name and returns null when type not found.
		/// </summary>
		/// <param name="name">The associated name.</param>
		/// <returns>The object.</returns>
		public static object TryGet(string name)
		{
			try
			{
				return Get(name);
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Creates an instance of a subclass of <typeparamref name="T"/> for a specified country or
		/// an instance of <typeparamref name="T"/> if no such subclass found.
		/// </summary>
		/// <typeparam name="T">The base (default) class.</typeparam>
		/// <param name="countryCode">Two letter country code.</param>
		/// <param name="arguments">Constructor arguments.</param>
		/// <returns>The instance of <typeparamref name="T"/> or its subclass.</returns>
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "just adding a character to a string")]
		public static T GetCountrySpecificOrDefault<T>(string countryCode, params object[] arguments) where T : class
		{
			Argument.NotNull(countryCode, nameof(countryCode));
			var baseType = typeof(T);
			var allTypesTableName = GetObjectDefinitionName(baseType) + "s";
			var allTypes = (Hashtable)GetWithoutSecurityCheckCore(typeof(T), allTypesTableName);
			var actualType = allTypes.ContainsKey(countryCode) ? (Type)allTypes[countryCode] : baseType;
			return (T)Activator.CreateInstance(actualType, arguments);
		}

		/// <summary>
		/// Get an instance of the implementing class for the given interface, similarly to Get&lt;T&gt;.
		/// If the process is devenv.exe, the configuration xml file is parsed manually and Activator.CreateInstance
		/// or factory method invoke is performed manually, if possible, using CargoWise.Common.AssemblyLoader.
		/// </summary>
		public static T GetDesignerSafe<T>()
		{
			return GetDesignerSafe<T>(Assembly.GetCallingAssembly());
		}

		public static T GetDesignerSafe<T>(Assembly callingAssembly)
		{
			var name = GetObjectDefinitionName(typeof(T));
			return GetDesignerSafe<T>(name, callingAssembly);
		}

		/// <summary>
		/// Get an instance of the implementing class for the given interface, similarly to Get&lt;T&gt;.
		/// If the process is devenv.exe, the configuration xml file is parsed manually and Activator.CreateInstance
		/// or factory method invoke is performed manually, if possible, using CargoWise.Common.AssemblyLoader.
		/// </summary>
		public static T GetDesignerSafe<T>(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			return GetDesignerSafe<T>(name, Assembly.GetCallingAssembly());
		}

		public static T GetDesignerSafe<T>(string name, Assembly callingAssembly)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			object result = DesignTimeObjectFactory.GetDesignerSafe(name);

			return (T)result;
		}

		public static object GetWithoutSecurityCheck(string name, params object[] arguments)
		{
			return GetWithoutSecurityCheckCore(typeof(EmptyType), name, arguments);
		}

		/// <summary>
		/// Gets an object with an associated name without security check.
		/// </summary>
		/// <param name="name">The associated name.</param>
		/// <param name="arguments">The arguments passed into the type constructor.</param>
		/// <returns>The object.</returns>
		internal static object GetWithoutSecurityCheckCore(Type requestedType, string name, params object[] arguments)
		{
			Argument.NotNull(name, nameof(name));
#if DEBUG
			if (substitutions.TryGetValue(name, out var factory))
			{
				return factory.Invoke(arguments);
			}
#endif
			var objectDefinition = GetObjectDefinition(name) ?? throw new NoSuchObjectDefinitionException(name);
			var obj = GetWithoutSecurityCheckForDefinitionCore(requestedType, objectDefinition, arguments);
			return obj;
		}

		/// <summary>
		/// Gets an object for a specific definition.
		/// </summary>
		/// <param name="objectDefinition">The object definition.</param>
		/// <param name="arguments">The arguments passed into the type constructor.</param>
		/// <returns>The object.</returns>
		internal static object GetWithoutSecurityCheckForDefinitionCore(Type requestedType, ObjectDefinition objectDefinition, params object[] arguments)
		{
			Argument.NotNull(objectDefinition, nameof(objectDefinition));
			if (objectDefinition.Singleton != null)
			{
				return objectDefinition.Singleton;
			}

			// Apply predefined arguments if not overriden.
			if (arguments == null || !arguments.Any())
			{
				var newArguments = GetNewArgumentsBasedOnConstructorArguments(requestedType, objectDefinition);
				if (newArguments != null)
				{
					arguments = newArguments;
				}
			}

			// Create instance.
			object instance;

			if (!string.IsNullOrEmpty(objectDefinition.FactoryMethodName))
			{
				instance = objectDefinition.GetFactoryMethod(requestedType).Invoke(arguments);
			}
			else if (arguments == null || arguments.Length == 0)
			{
				instance = objectDefinition.GetParameterLessConstructor(requestedType).Invoke();
			}
			else
			{
				instance = objectDefinition.GetConstructorBasedOnArguments(requestedType, arguments).Invoke(arguments);
			}

			// Set properties.
			objectDefinition.GetPropertySetter(requestedType)?.Invoke(instance);

			// Return instance.
			if (objectDefinition.IsSingleton)
			{
				objectDefinition.Singleton = instance;
#if DEBUG
				singletons.Push(objectDefinition);
#endif
			}
			return instance;
		}

		static object[] GetNewArgumentsBasedOnConstructorArguments(Type requestedType, ObjectDefinition objectDefinition)
		{
			var constructorArgumentDefinitions = objectDefinition.ConstructorArguments;
			if (constructorArgumentDefinitions != null && constructorArgumentDefinitions.Any())
			{
				var argumentList = new List<object>();
				foreach (var constructorArgumentDefinition in constructorArgumentDefinitions)
				{
					if (constructorArgumentDefinition != null)
					{
						if (constructorArgumentDefinition.ListValues == null || !constructorArgumentDefinition.ListValues.Any())
						{
							argumentList.Add(GetWithoutSecurityCheck(constructorArgumentDefinition.ObjectName));
						}
						else
						{
							var argumentOfCollection = new List<object>();
							foreach (var listValueDefinition in constructorArgumentDefinition.ListValues)
							{
								argumentOfCollection.Add(GetWithoutSecurityCheck(listValueDefinition.ObjectName));
							}
							argumentList.Add(argumentOfCollection);
						}
					}
				}

				return argumentList.ToArray();
			}

			return null;
		}

		/// <summary>
		/// Creates a new object of a specific type associated with a specific name.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		/// <param name="arguments">The arguments passed to the object constructor.</param>
		/// <returns>The object.</returns>
		public static T New<T>(params object[] arguments)
		{
			var name = GetObjectDefinitionName(typeof(T));
#if DEBUG
			if (substitutions.TryGetValue(name, out var factory))
			{
				return (T)factory.Invoke(arguments);
			}
#endif
			var objectDefinition = GetObjectDefinition(name);
			return objectDefinition == null
				? throw new NoSuchObjectDefinitionException(name)
				: (T)objectDefinition.GetSinglePublicConstructorForFactoryNew(typeof(T))(arguments);
		}
		#endregion

		#region Registration Checks
		/// <summary>
		/// Checks whether the name is contained in the object factory.
		/// </summary>
		/// <param name="name">The name to be checked.</param>
		/// <returns>Whether the name is contained in the object factory.</returns>
		public static bool Contains(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));

			var result = objectDefinitionCache.ContainsKey(name);
#if DEBUG
			result = result || substitutions.ContainsKey(name);
#endif
			return result;
		}

		/// <summary>
		/// Checks whether the service type is contained in the object factory.
		/// </summary>
		/// <param name="type">The service type.</param>
		/// <returns>Whether the service type is contained in the object factory.</returns>
		public static bool Contains(Type type)
		{
			Argument.NotNull(type, nameof(type));
			return Contains(GetObjectDefinitionName(type));
		}

		public static ObjectDefinition[] GetObjectDefinitions()
		{
			return objectDefinitionCache.Values.ToArray();
		}
		#endregion

		public interface EmptyType { }

		#region For Testing
#if DEBUG
		[ThreadSafe]
		static readonly ConcurrentStack<ObjectDefinition> singletons = new ConcurrentStack<ObjectDefinition>();

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Interlocked)]
		static ImmutableDictionary<string, Func<object[], object>> substitutions = ImmutableDictionary<string, Func<object[], object>>.Empty;

		public static IDisposable Substitute<T>(T obj)
		{
			ValidateTypeIsNotMock(typeof(T));
			return Substitute(GetObjectDefinitionName(typeof(T)), obj);
		}

		public static IDisposable Substitute<T>(Func<T> factory)
		{
			ValidateTypeIsNotMock(typeof(T));
			return Substitute(GetObjectDefinitionName(typeof(T)), factory);
		}

		public static IDisposable Substitute<T>(Func<object[], T> factory)
		{
			ValidateTypeIsNotMock(typeof(T));
			return Substitute(GetObjectDefinitionName(typeof(T)), args => factory(args));
		}

		public static IDisposable Substitute(string name, object obj)
		{
			ValidateTypeIsNotMock(obj.GetType());
			return Substitute(name, args => obj);
		}

		static void ValidateTypeIsNotMock(Type type)
		{
			if (type.IsGenericType && GetGenericTypeDefinitionName(type.GetGenericTypeDefinition().FullName) == "Moq.Mock")
			{
				throw new ArgumentException(
@"Should not substitute with type 'Mock<T>' as this will not correctly configure the mock for ObjectFactory.Get<T>().
Please use the Mock instance's '.Object' attribute instead.");
			}
		}

		public static IDisposable Substitute(string name, Delegate factory)
		{
			return Substitute(name, args => factory.DynamicInvoke());
		}

		public static IDisposable Substitute(string name, Func<object[], object> factory)
		{
			Argument.NotNullOrEmpty(name, nameof(name));

			UpdateField(ref substitutions, (ImmutableDictionary<string, Func<object[], object>> subs) =>
			{
				if (HasBeenSubstituted(name))
				{
					throw new InvalidOperationException(Invariant($"ObjectName name is already registered: {name}."));
				}
				return subs.Add(name, factory);
			});

			return new DisposableAction(() =>
			{
				UpdateField(ref substitutions, (ImmutableDictionary<string, Func<object[], object>> subs) =>
					{
						if (subs.ContainsKey(name))
						{
							subs = subs.Remove(name);
						}
						return subs;
					});
			});
		}

		public static bool HasBeenSubstituted<T>()
		{
			return HasBeenSubstituted(GetObjectDefinitionName(typeof(T)));
		}

		public static bool HasBeenSubstituted(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			return substitutions.ContainsKey(name);
		}

		public static void DisposeSubstitutions()
		{
			substitutions = ImmutableDictionary<string, Func<object[], object>>.Empty;
		}

		public static void DisposeSingletons()
		{
			while (singletons.TryPop(out var objectDefinition))
			{
				objectDefinition.Singleton = null;
			}
		}

		public static IEnumerable<string> TypeFactoryAnnotationUsingFile(string assemblyFilePath, string resourceName)
		{
			var definitions = ObjectDefinitions.CreateUsingFile(assemblyFilePath, resourceName);
			return TypeFactoryAnnotation(definitions);
		}

		public static IEnumerable<string> TypeFactoryAnnotation(string configurationResourceFileUri)
		{
			Argument.NotNullOrEmpty(configurationResourceFileUri, nameof(configurationResourceFileUri));
			return TypeFactoryAnnotation(ObjectDefinitions.Create(configurationResourceFileUri));
		}

		static IEnumerable<string> TypeFactoryAnnotation(ObjectDefinitions objectDefinitions)
		{
			Argument.NotNull(objectDefinitions, nameof(objectDefinitions));
			return (objectDefinitions.Definitions ?? Array.Empty<ObjectDefinition>()).Where(d => d.TypeName != null && d.TypeName.Contains(',')).Select(d => d.TypeName);
		}

		static void UpdateField<T>(ref T field, Func<T, T> operation) where T : class
		{
			Argument.NotNull(operation, nameof(operation));

			var original = field;
			if (Interlocked.CompareExchange(ref field, operation(original), original) != original)
			{
				var spinner = new SpinWait();
				do
				{
					spinner.SpinOnce();
					original = field;
				}
				while (Interlocked.CompareExchange(ref field, operation(original), original) != original);
			}
		}

#endif
		#endregion
	}
}
