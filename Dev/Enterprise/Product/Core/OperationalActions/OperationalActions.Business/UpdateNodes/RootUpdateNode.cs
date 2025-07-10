using System;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerDisplay("Root Type = {rootType.Name}")]
	public sealed partial class RootUpdateNode : UpdateNode
	{
		public RootUpdateNode(Type rootType, IOperationalActionSectionLog log = null)
			: base(log)
		{
			this.expressionCache = new Dictionary<string, IFilterExpression>(new FieldNameComparer());
			this.rootType = rootType;
		}

		public void AddRange(IEnumerable<IOperationalActionFieldValuePair> pairs)
		{
			foreach (IOperationalActionFieldValuePair pair in pairs)
			{
				Add(pair);
			}
		}
		public void Add(IOperationalActionFieldValuePair pair)
		{
			if (pair == null)
			{
				throw new ArgumentNullException(nameof(pair));
			}

			if (pair.Field == null)
			{
				throw new ArgumentException("Field property of pair cannot be null", nameof(pair));
			}

			PropertyInfo[] path = ReflectionHelper.FieldTextToPath(rootType, pair.Field.Field);

			if (path != null && path.Length > 0)
			{
				IFilterExpression[] filters = FilterTools.SplitByInfoChain(pair.Filter, path);
				Normalise(filters);
				Add(path, filters, pair);
			}
			else
			{
				Add(pair.Field.Field, pair);
			}
		}

		void Add(PropertyInfo[] infos, IFilterExpression[] filters, IOperationalActionFieldValuePair pair)
		{
			int lastIndex = infos.Length - 1;

			UpdateNode current = this;

			for (int i = 0; i < lastIndex; i++)
			{
				IFilterExpression filter = filters[i];
				PropertyInfo info = infos[i];

				if (!filter.IsEmpty)
				{
					current = FindOrCreateChild(current, info.ReflectedType, filter);
				}

				current = FindOrCreateChild(current, info);
			}

			{
				IFilterExpression filter = filters[lastIndex];
				PropertyInfo info = infos[lastIndex];

				if (!filter.IsEmpty)
				{
					current = FindOrCreateChild(current, info.ReflectedType, filter);
				}

				AddTo(current, infos[lastIndex], pair.GetValue(infos[lastIndex].PropertyType));
			}
		}

		void Add(string field, IOperationalActionFieldValuePair pair)
		{
			AddTo(this, field, (pair as RunnerField)?.Caption, pair.GetValue(null));
		}

		static StepUpdateNode FindOrCreateChild(UpdateNode parent, PropertyInfo info)
		{
			StepUpdateNode result = parent.FindByInfoName(info.Name);

			if (result == null)
			{
				result = NewNode(info);
				AddTo(parent, result);
			}

			return result;
		}
		static FilterUpdateNode FindOrCreateChild(UpdateNode parent, Type expectedType, IFilterExpression filter)
		{
			FilterUpdateNode result = parent.FindByFilter(filter);

			if (result == null)
			{
				result = NewNode(expectedType, filter);
				AddTo(parent, result);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Diagnostic Information")]
		static StepUpdateNode NewNode(PropertyInfo info)
		{
			switch (ReflectionHelper.Classify(info))
			{
				case PropertyClassification.FollowSingle:
					return new PropertyUpdateNode(info);
				case PropertyClassification.FollowCollection:
					return new CollectionUpdateNode(info);
				case PropertyClassification.FollowView:
					return new ViewUpdateNode(info);

				default:
					const string messageFormat =
						"And what do you expect me to do with this?\n" +
						"Property: {0}.{1}\n" +
						"Property Type: {3}\n" +
						"Overridden Type: {2}" +
						"";

					string message = string.Format(messageFormat,
						info.DeclaringType.FullName,
						info.Name,
						info.PropertyType.FullName,
						ActionFieldFollowAttribute.GetReturnType(info).FullName);

					throw new InvalidOperationException(message);
			}
		}
		static FilterUpdateNode NewNode(Type expectedType, IFilterExpression filter)
		{
			return new FilterUpdateNode(expectedType, filter);
		}

		void Normalise(IFilterExpression[] expressions)
		{
			for (int i = 0; i < expressions.Length; i++)
			{
				IFilterExpression current = expressions[i];
				IFilterExpression cached;
				string key = current.ToString();

				if (expressionCache.TryGetValue(key, out cached))
				{
					expressions[i] = cached;
				}
				else
				{
					expressionCache[key] = current;
				}
			}
		}

		readonly Dictionary<string, IFilterExpression> expressionCache;
		readonly Type rootType;
	}
}

#region Test
#if DEBUG

#region DisplayProxy

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerTypeProxy(typeof(DisplayProxy<UpdateNode>))]
	partial class RootUpdateNode
	{
	}
}

#endregion

#endif
#endregion
