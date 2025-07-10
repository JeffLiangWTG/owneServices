using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Common
{
	/// <summary>
	/// Property Visitation state
	/// </summary>
	public enum PropertyVisitState
	{
		/// <summary>
		/// Before visiting the item
		/// </summary>
		Start,

		/// <summary>
		/// Between properties in a property bag or collection
		/// </summary>
		ItemBetween,

		/// <summary>
		/// After visiting the item 
		/// </summary>
		End
	}

	/// <summary>
	/// Type of property about to be visited
	/// </summary>
	public enum PropertyVisitType
	{
		/// <summary>
		/// Leaf node of class, no deeper traversel will occur
		/// </summary>
		Leaf,

		/// <summary>
		/// Collection of properties such as struct or class
		/// </summary>
		PropertyBag,

		/// <summary>
		/// Typed collection such as a typed List or Array
		/// </summary>
		TypedEnumerable,

		/// <summary>
		/// Untyped collection such as an IEnumerable or IList
		/// </summary>
		UntypedEnumerable
	}

	/// <summary>
	/// Object Property Visitor
	/// </summary>
	public interface IPropertyVisitor
	{
		/// <summary>
		/// Whether to visit a specific property of name and type
		/// Called before the getter is accessed.
		/// </summary>
		/// <param name="propertyVisitType">PropertyVisitor type of the upcoming property</param>
		/// <param name="type">Type of the upcoming property</param>
		/// <param name="typeFullName">Types full name</param>
		/// <param name="propertyFullName">Properties full name</param>
		/// <param name="propertyName">Name of the upcoming property</param>
		/// <returns>True if should visit, otherwise false</returns>
		bool ShouldVisit(PropertyVisitType propertyVisitType, Type type, string typeFullName, string propertyFullName, string propertyName);

		/// <summary>
		/// Visit a leaf property
		/// </summary>
		/// <typeparam name="T">Property type</typeparam>
		/// <param name="propertyName">Property name</param>
		/// <param name="item">Actual property</param>
		void VisitLeaf<T>(string propertyName, T item);

		/// <summary>
		/// Visit a bag of properties (e.g. a struct or class)
		/// </summary>
		/// <typeparam name="T">Type of the property bag</typeparam>
		/// <param name="propertyName">The properties names</param>
		/// <param name="item">Actual state</param>
		/// <param name="state">The state of the visitation</param>
		/// <returns>False to skip processing of items in the bag, true to visit them</returns>
		bool VisitPropertyBag<T>(string propertyName, T item, PropertyVisitState state);

		/// <summary>
		/// Visit a collection of typed properties (array, list, dictionary etc)
		/// </summary>
		/// <typeparam name="T">Type of the individual items</typeparam>
		/// <param name="propertyName">Name of the property</param>
		/// <param name="enumerable">Actual collection</param>
		/// <param name="state">The state of the visitation</param>
		/// <returns>False to skip processing of items in the collection, true to visit them</returns>
		bool VisitEnumerable<T>(string propertyName, IEnumerable<T> enumerable, PropertyVisitState state);

		/// <summary>
		/// Visit a collection of untyped properties (e.g. IEnumerable)
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <param name="enumerable">Actual collection</param>
		/// <param name="state">The state of the visitation</param>
		/// <returns>False to skip processing of items in the collection, true to visit them</returns>
		bool VisitEnumerable(string propertyName, IEnumerable enumerable, PropertyVisitState state);
	}

	/// <summary>
	/// Filter out properties to not visit while printing
	/// </summary>
	/// <param name="propertyType">A property type</param>
	/// <param name="propertyName">A property name</param>
	/// <returns>True to ignore this property, otherwise false</returns>
	public delegate bool IgnoreProperties(Type propertyType, string propertyName);

	/// <summary>
	/// Object property traversal
	/// </summary>
	public static class ObjectPropertyVisitExtensions
	{
		[ThreadSafe]
		static readonly ConcurrentDictionary<Type, object> visitors = new ConcurrentDictionary<Type, object>();

		/// <summary>
		/// Formats the provided object as a string for logging purposes. 
		/// All public getters names and values are printed. Any property type supporting IEnumerable is also deeply printed.
		/// </summary>
		/// <typeparam name="T">A type to be logged</typeparam>
		/// <param name="logableItem">Instance to be logged</param>
		/// <param name="ignoreProperties">Optional delegate to specify properties to ignore</param>
		/// <remarks>
		///		Use caution, getters with side affects will cause problems.
		///		This class caches a compiled expression tree for the type, so after the initial reflection performance is on 
		///		par with hand crafted logging.
		///		Types coming from the System... namespace that are not Value types are not reflected. 
		///		Properties returning null are handled without performance concerns.
		///		Exceptions caused from accessing getters are ignored but can severly impact performance.
		///		Recursion is handled via a maximum recursion depth of 10.
		/// </remarks>
		/// <returns>The formatted string</returns>
		public static string Print<T>(this T logableItem, IgnoreProperties ignoreProperties = null)
		{
			var sb = new StringBuilder();
			logableItem.Visit(new PropertyPrintVisitor(sb, ignoreProperties));
			return sb.ToString();
		}

		/// <summary>
		/// Visit all of the public properties for an object based on a given type.
		/// </summary>
		/// <typeparam name="T">Type to obtain properties from</typeparam>
		/// <param name="item">object to traverse</param>
		/// <param name="propertyVisitor">Property visitor</param>
		/// <remarks>
		///		This is a depth first recursive traversal. The implementation of 
		///		IPropertyVisitor will need to handle property cycles if applicable.
		/// </remarks>
		public static void Visit<T>(this T item, IPropertyVisitor propertyVisitor)
		{
			item.Visit(new Context(), propertyVisitor);
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		static void Visit<T>(
			this T logableItem,
			Context context,
			IPropertyVisitor propertyVisitor)
		{
			if (!visitors.TryGetValue(typeof(T), out var cachedVisitor))
			{
				var visitor = BuildVisitor<T>();
				visitors.TryAdd(typeof(T), visitor);
				cachedVisitor = visitor;
			}

			((Action<T, Context, IPropertyVisitor>)cachedVisitor).Invoke(logableItem, context, propertyVisitor);
		}

		static Action<T, Context, IPropertyVisitor> BuildVisitor<T>()
		{
			var itemParameter = Expression.Parameter(typeof(T), "item");
			var contextParameter = Expression.Parameter(typeof(Context), "context");
			var propertyVisitorParameter = Expression.Parameter(typeof(IPropertyVisitor), "propertyVisitor");

			var typesToReflect = new List<Type>() { typeof(T) };
			typesToReflect.AddRange(typeof(T).GetInterfaces().Where(type => !type.Namespace.StartsWith("System", StringComparison.Ordinal) && !type.Namespace.StartsWith("CargoWise.Types", StringComparison.Ordinal)));
			List<Expression> propertyVisitors = new List<Expression>();

			if ((typeof(T).Namespace.StartsWith("System", StringComparison.Ordinal) && !typeof(T).IsValueType) ||
				(typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Length == 0 && typesToReflect.Count == 1) ||
				typeof(T).IsArray ||
				typeof(T).Namespace.StartsWith("CargoWise.Types", StringComparison.Ordinal))
			{
				propertyVisitors.Add(
					GetBodyExpression(
						typeof(T),
						typeof(T),
						string.Empty,
						itemParameter,
						itemParameter,
						contextParameter,
						propertyVisitorParameter));
			}
			else
			{
				foreach (var propertyDetails in GetProperties(typesToReflect, typeof(T)))
				{
					var aType = propertyDetails.ParentType;
					var propertyName = propertyDetails.PropertyName;
					var propertyInfo = propertyDetails.PropertyInfo;

					var castExpression = Expression.Convert(itemParameter, aType);
					var member = aType != typeof(T) ? Expression.Property(castExpression, propertyInfo.Name) : Expression.Property(itemParameter, propertyInfo.Name);
					propertyVisitors.Add(
						GetBodyExpression(
							propertyInfo.PropertyType,
							typeof(T),
							propertyName,
							member,
							itemParameter,
							contextParameter,
							propertyVisitorParameter));
				}

				if (propertyVisitors.Count == 0)
				{
					propertyVisitors.Add(
						GetBodyExpression(
							typeof(T),
							typeof(T),
							string.Empty,
							itemParameter,
							itemParameter,
							contextParameter,
							propertyVisitorParameter));
				}
			}

			Action<T, Context, IPropertyVisitor> printer;
			var arrayExpression = Expression.NewArrayInit(typeof(bool), propertyVisitors);
			var func = Expression.Lambda<
				Func<T, Context, IPropertyVisitor, bool[]>>(
						arrayExpression,
						itemParameter,
						contextParameter,
						propertyVisitorParameter).Compile();

			printer = (logMe, context, propertyVisitor) =>
			{
				_ = func.Invoke(logMe, context, propertyVisitor);
			};

			return printer;
		}

		static Expression GetBodyExpression(
			Type itemType,
			Type parentType,
			string keyName,
			Expression memberParameter,
			ParameterExpression itemParameter,
			ParameterExpression contextParameter,
			ParameterExpression propertyVisitorParameter)
		{
			MethodCallExpression valueExpression;
			PropertyVisitType propertyVisitType;

			// We have to process arrays first or they will match enumerable but not have the type
			if (itemType.IsArray)
			{
				valueExpression = Expression.Call(
					typeof(ObjectPropertyVisitExtensions), "ProcessEnumerable",
					new[] { itemType.GetElementType() },
					Expression.Constant(keyName),
					memberParameter,
					contextParameter,
					propertyVisitorParameter);

				propertyVisitType = PropertyVisitType.TypedEnumerable;
			}
			else if (itemType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Length > 0 &&
					(!itemType.Namespace.StartsWith("System", StringComparison.Ordinal) || itemType.IsValueType) &&
					!itemType.Namespace.StartsWith("CargoWise.Types", StringComparison.Ordinal))
			{
				valueExpression = Expression.Call(
					typeof(ObjectPropertyVisitExtensions),
					nameof(ProcessItem),
					new[] { itemType },
					Expression.Constant(keyName),
					memberParameter,
					contextParameter,
					propertyVisitorParameter);

				propertyVisitType = PropertyVisitType.PropertyBag;
			}
			else if (itemType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(itemType))
			{
				var enumerableType = itemType.Name == "IEnumerable`1" ? itemType : itemType.GetInterfaces().Where(x => x.Name == "IEnumerable`1").FirstOrDefault();

				if (enumerableType == null)
				{
					valueExpression = Expression.Call(
						typeof(ObjectPropertyVisitExtensions),
						nameof(ProcessEnumerable),
						null,
						Expression.Constant(keyName),
						memberParameter,
						contextParameter,
						propertyVisitorParameter);

					propertyVisitType = PropertyVisitType.UntypedEnumerable;
				}
				else
				{
					valueExpression = Expression.Call(
						typeof(ObjectPropertyVisitExtensions),
						nameof(ProcessEnumerable),
						new[] { enumerableType.GenericTypeArguments[0] },
						Expression.Constant(keyName),
						memberParameter,
						contextParameter,
						propertyVisitorParameter);

					propertyVisitType = PropertyVisitType.TypedEnumerable;
				}
			}
			else
			{
				valueExpression = Expression.Call(
					typeof(ObjectPropertyVisitExtensions),
					nameof(ProcessLeaf),
					new[] { itemType },
					Expression.Constant(keyName),
					memberParameter,
					contextParameter,
					propertyVisitorParameter);

				propertyVisitType = PropertyVisitType.Leaf;
			}

			var exceptionParameter = Expression.Parameter(typeof(Exception));
			var exceptionHandlingExpression =
				Expression.Call(
					typeof(ObjectPropertyVisitExtensions),
					nameof(ProcessException),
					null,
					exceptionParameter);

			var tryCatchExpression =
				Expression.TryCatch(
					Expression.Block(valueExpression),
					Expression.Catch(
						exceptionParameter,
						exceptionHandlingExpression));

			var shouldLogExpression =
				Expression.Call(
					typeof(ObjectPropertyVisitExtensions),
					nameof(ShouldProcess),
					new[] { itemType },
					Expression.Constant(propertyVisitType),
					Expression.Constant(itemType.FullName),
					Expression.Constant(string.Join(".", parentType.FullName, keyName)),
					Expression.Constant(keyName),
					propertyVisitorParameter);

			var conditionalExpression = Expression.AndAlso(
				shouldLogExpression,
				Expression.Call(
					typeof(ObjectPropertyVisitExtensions),
					nameof(PropertyBagProcessing),
					new[] { parentType },
					itemParameter,
					contextParameter,
					propertyVisitorParameter,
					Expression.Constant(PropertyVisitState.ItemBetween)));

			return Expression.AndAlso(conditionalExpression, tryCatchExpression);
		}

		static bool PropertyBagProcessing<T>(
			T item,
			Context context,
			IPropertyVisitor propertyVisitor,
			PropertyVisitState state)
		{
			if (context.ProcessedFirst)
			{
				return propertyVisitor.VisitPropertyBag("", item, state);
			}

			return true;
		}

		static IEnumerable<(string PropertyName, Type ParentType, PropertyInfo PropertyInfo)> GetProperties(IEnumerable<Type> typesToReflect, Type mainType)
		{
			var properties = new Dictionary<string, (Type ParentType, PropertyInfo PropertyInfo)>();
			var restrictedPropertyNames = new HashSet<string>();
			var mainTypePropertyNames = mainType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetIndexParameters().Length == 0).Select(x => x.Name).ToHashSet();

			foreach (var aType in typesToReflect)
			{
				var propertyInfos = aType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(
					x => x.GetIndexParameters().Length == 0 && (aType == mainType || !mainTypePropertyNames.Contains(x.Name)));
				foreach (var propertyInfo in propertyInfos)
				{
					TryAddProperty(propertyInfo.Name, propertyInfo);
				}
			}

			void TryAddProperty(string propertyName, PropertyInfo propertyInfo)
			{
				if (restrictedPropertyNames.Contains(propertyName))
				{
					var currentFullyQualifiedName = string.Join(".", propertyInfo.DeclaringType.FullName, propertyInfo.Name);
					properties.Add(currentFullyQualifiedName, (propertyInfo.DeclaringType, propertyInfo));

					if (properties.TryGetValue(propertyName, out var clashingProperty))
					{
						var clashingPropertyFullyQualifiedName = string.Join(".", clashingProperty.ParentType.FullName, clashingProperty.PropertyInfo.Name);
						properties.Remove(propertyName);
						properties.Add(clashingPropertyFullyQualifiedName, clashingProperty);
					}
				}
				else
				{
					properties.Add(propertyName, (propertyInfo.DeclaringType, propertyInfo));
					restrictedPropertyNames.Add(propertyName);
				}
			}

			return properties.Select(property => (property.Key, property.Value.ParentType, property.Value.PropertyInfo));
		}

		static bool ShouldProcess<T>(
			PropertyVisitType propertyVisitType,
			string typeFullName,
			string propertyFullName,
			string propertyName,
			IPropertyVisitor propertyVisitor)
		{
			return propertyVisitor.ShouldVisit(propertyVisitType, typeof(T), typeFullName, propertyFullName, propertyName);
		}

		static bool ProcessLeaf<T>(
			string name,
			T item,
			Context context,
			IPropertyVisitor propertyVisitor)
		{
			propertyVisitor.VisitLeaf(name, item);
			context.ProcessedFirst = true;
			return true;
		}

		static bool ProcessItem<T>(
			string name,
			T item,
			Context context,
			IPropertyVisitor propertyVisitor)
		{
			bool result = propertyVisitor.VisitPropertyBag(name, item, PropertyVisitState.Start);
			var oldProcessed = context.ProcessedFirst;
			context.ProcessedFirst = false;

			if (result)
			{
				if (item != null)
				{
					item.Visit(context, propertyVisitor);
				}
				else
				{
					ProcessLeaf("", item, context, propertyVisitor);
				}
			}

			result = propertyVisitor.VisitPropertyBag(name, item, PropertyVisitState.End) & result;
			context.ProcessedFirst = oldProcessed;
			return result;
		}

		static bool ProcessEnumerable<T>(
			string name,
			IEnumerable<T> enumerable,
			Context context,
			IPropertyVisitor propertyVisitor)
		{
			bool result = propertyVisitor.VisitEnumerable(name, enumerable, PropertyVisitState.Start);

			if (result && enumerable != null)
			{
				using (var en = enumerable.GetEnumerator())
				{
					if (en.MoveNext())
					{
						ProcessItem("", en.Current, context, propertyVisitor);

						while (en.MoveNext())
						{
							propertyVisitor.VisitEnumerable(name, enumerable, PropertyVisitState.ItemBetween);
							ProcessItem("", en.Current, context, propertyVisitor);
						}
					}
				}
			}

			return propertyVisitor.VisitEnumerable(name, enumerable, PropertyVisitState.End) & result;
		}

		static bool ProcessEnumerable(
			string name,
			IEnumerable enumerable,
			Context context,
			IPropertyVisitor propertyVisitor)
		{
			bool result = propertyVisitor.VisitEnumerable(name, enumerable, PropertyVisitState.Start);

			if (result && enumerable != null)
			{
				var en = enumerable.GetEnumerator();

				if (en.MoveNext())
				{
					ProcessItem("", en.Current, context, propertyVisitor);

					while (en.MoveNext())
					{
						propertyVisitor.VisitEnumerable(name, enumerable, PropertyVisitState.ItemBetween);
						ProcessItem("", en.Current, context, propertyVisitor);
					}
				}
			}

			return propertyVisitor.VisitEnumerable(name, enumerable, PropertyVisitState.End) & result;
		}

		static bool ProcessException(Exception ex)
		{
			if (ex.IsCriticalException())
			{
				throw ex;
			}

			return true;
		}

		struct PropertyPrintVisitor : IPropertyVisitor
		{
			public const int MaximumCallDepth = 10;

			int callDepth;
			readonly StringBuilder stringBuilder;
			readonly IgnoreProperties ignoreProperty;

			public PropertyPrintVisitor(StringBuilder stringBuilder, IgnoreProperties ignoreProperty = null)
			{
				this.callDepth = 0;
				this.stringBuilder = stringBuilder;
				this.ignoreProperty = ignoreProperty;
			}

			public bool VisitEnumerable<T>(string propertyName, IEnumerable<T> enumerable, PropertyVisitState state)
			{
				return CollectionMarkers(propertyName, state);
			}

			public bool VisitEnumerable(string propertyName, IEnumerable enumerable, PropertyVisitState state)
			{
				return CollectionMarkers(propertyName, state);
			}

			public void VisitLeaf<T>(string propertyName, T item)
			{
				if (!string.IsNullOrEmpty(propertyName))
				{
					stringBuilder.Append(propertyName);
					stringBuilder.Append(": ");
				}

				if (item == null)
				{
					stringBuilder.Append("null");
					return;
				}

				if (item is IFormattable format)
				{
					stringBuilder.Append(format.ToString(null, CultureInfo.InvariantCulture));
				}
				else
				{
					stringBuilder.Append(item.ToString());
				}
			}

			public bool VisitPropertyBag<T>(string propertyName, T item, PropertyVisitState state)
			{
				return PropertyBagMarkers(propertyName, state);
			}

			public bool ShouldVisit(PropertyVisitType propertyVisitType, Type type, string typeFullName, string propertyFullName, string propertyName)
			{
				if (propertyVisitType == PropertyVisitType.PropertyBag &&
					(typeFullName.Contains(".BusinessObject") ||
					typeFullName.Contains(".EnterpriseBusinessObject") ||
					typeFullName.Contains(".IBusiness")))
				{
					return false;
				}
				else if (ignoreProperty != null)
				{
					return !ignoreProperty(type, propertyName);
				}
				else
				{
					return true;
				}
			}

			bool CollectionMarkers(string propertyName, PropertyVisitState state)
			{
				switch (state)
				{
					case PropertyVisitState.Start:
						callDepth++;
						if (!string.IsNullOrEmpty(propertyName))
						{
							stringBuilder.Append(propertyName);
							stringBuilder.Append(" ");
						}

						stringBuilder.Append("{ ");
						break;
					case PropertyVisitState.ItemBetween:
						stringBuilder.Append(", ");
						break;
					case PropertyVisitState.End:
						stringBuilder.Append(" }");
						callDepth--;
						break;
				}

				return callDepth < MaximumCallDepth;
			}

			bool PropertyBagMarkers(string propertyName, PropertyVisitState state)
			{
				switch (state)
				{
					case PropertyVisitState.Start:
						callDepth++;
						if (!string.IsNullOrEmpty(propertyName))
						{
							stringBuilder.Append(propertyName);
							stringBuilder.Append(" ");
						}

						stringBuilder.Append("( ");
						break;
					case PropertyVisitState.ItemBetween:
						stringBuilder.Append(", ");
						break;
					case PropertyVisitState.End:
						stringBuilder.Append(" )");
						callDepth--;
						break;
				}

				return callDepth < MaximumCallDepth;
			}
		}

		class Context
		{
			public bool ProcessedFirst;
		}
	}
}