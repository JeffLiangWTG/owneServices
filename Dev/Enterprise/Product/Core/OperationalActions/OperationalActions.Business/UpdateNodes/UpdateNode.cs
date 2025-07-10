using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	public abstract partial class UpdateNode
	{
		internal UpdateNode(IOperationalActionSectionLog log = null)
		{
			children = new List<UpdateNode>();
			fields = new List<InfoValuePair>();
			this.log = log;
		}

		public virtual IEnumerable<BusinessObject> Apply(IEnumerable<BusinessObject> targets)
		{
			foreach (var leaf in fields)
			{
				HandleLeafForBizOs(leaf, targets);
			}

			var affectedTargets = targets.ToDictionary(t => t.PK);

			foreach (var child in children)
			{
				foreach (var childTarget in child.Apply(targets))
				{
					if (!affectedTargets.ContainsKey(childTarget.PK))
					{
						affectedTargets.Add(childTarget.PK, childTarget);
					}
				}
			}

			return affectedTargets.Values;
		}

		protected virtual void HandleLeafForBizOs(InfoValuePair leaf, IEnumerable<BusinessObject> targets)
		{
			CheckIfAnyTargetIsReadOnly(leaf, targets);

			foreach (BusinessObject target in targets)
			{
				if (leaf.Info != null)
				{
					if (HasReadOnlyErrorMessage(target, leaf))
					{
						continue;
					}

					if (leaf.Value is ZDateTimeOffset)
					{
						leaf.Info.SetValue(target, ZDateTimeOffset.ValueForUpdate((ZDateTimeOffset)leaf.Info.GetValue(target), (ZDateTimeOffset)leaf.Value), null);
					}
					else
					{
						leaf.ApplyToBusinessObject(target);
					}
				}
				else if (leaf.Field != null)
				{
					var customBizo = (target as ICustomFieldProvider)?.GetCustomBusinessObject();

					if (customBizo != null && customBizo.HasPossiblyCustomProperty(leaf.Field))
					{
						customBizo[leaf.Field] = leaf.Value;
					}
					else if (log != null)
					{
						log.Notify(OperationalActionLogErrorLevel.Warning, Res.GetString("f9dc3fc2-3cfc-4e5c-b4f7-e5a4f6590ea1", "Custom field '{0}' was not found on {1}.", leaf.FieldName ?? leaf.Field, target.HumanReadableName));
					}
				}
			}
		}

		void CheckIfAnyTargetIsReadOnly(InfoValuePair leaf, IEnumerable<BusinessObject> targets)
		{
			foreach (BusinessObject target in targets)
			{
				if (leaf.Info != null)
				{
					var targetPropertyInfo = target.FindPropertyInfo(leaf.Info.Name);

					if (IsReadOnly(target, targetPropertyInfo))
					{
						target.AddRowError(GetReadOnlyErrorMessage(targetPropertyInfo));
					}
				}
			}
		}

		string GetReadOnlyErrorMessage(ZPropertyInfo targetPropertyInfo)
		{
			return Res.GetString("3116a577-22ac-4a8b-919f-9467369aadd6",
				"{0} is a read-only field in the target object.", targetPropertyInfo.Name);
		}

		bool IsReadOnly(BusinessObject bizo, ZPropertyInfo info)
		{
			if (info == null)
			{
				return false;
			}

			var oaReadOnlyAttribute = bizo.GetType().GetProperty(info.Name).GetCustomAttribute<OperationActionReadOnlyMemberAttribute>();
			if (oaReadOnlyAttribute != null)
			{
				var readOnlyProperty = bizo.GetType().GetProperty(oaReadOnlyAttribute.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				return (bool)readOnlyProperty.GetValue(bizo);
			}

			return info.ReadOnly;
		}

		bool HasReadOnlyErrorMessage(BusinessObject target, InfoValuePair leaf)
		{
			var targetPropertyInfo = target.FindPropertyInfo(leaf.Info.Name);
			if (targetPropertyInfo == null)
			{
				return false;
			}
			return target.Notifications.Any(_ => _.Message.Contains(GetReadOnlyErrorMessage(targetPropertyInfo)));
		}

		public StepUpdateNode FindByInfoName(string name)
		{
			foreach (UpdateNode child in children)
			{
				StepUpdateNode stepNode = child as StepUpdateNode;

				if (stepNode != null && stepNode.Info.Name == name)
				{
					return stepNode;
				}
			}

			return null;
		}

		public FilterUpdateNode FindByFilter(IFilterExpression filter)
		{
			foreach (UpdateNode child in children)
			{
				FilterUpdateNode filterNode = child as FilterUpdateNode;

				if (filterNode != null && filterNode.Filter == filter)
				{
					return filterNode;
				}
			}

			return null;
		}

		public IEnumerable<UpdateNode> GetChildren()
		{
			return children;
		}

		public IEnumerable<InfoValuePair> GetFields()
		{
			return fields;
		}

		protected static void AddTo(UpdateNode parent, PropertyInfo info, IZType value)
		{
			parent.fields.Add(new InfoValuePair(info, value));
		}

		protected static void AddTo(UpdateNode parent, string field, string fieldName, IZType value)
		{
			parent.fields.Add(new InfoValuePair(field, fieldName, value));
		}

		protected static void AddTo(UpdateNode parent, UpdateNode child)
		{
			parent.children.Add(child);
		}

		readonly List<UpdateNode> children;
		readonly List<InfoValuePair> fields;
		readonly IOperationalActionSectionLog log;
	}
}

#region Test
#if DEBUG

#region DisplayProxy

namespace Enterprise.Services.OperationalActions.Business
{
	[System.Diagnostics.DebuggerTypeProxy(typeof(DisplayProxy<UpdateNode>))]
	partial class UpdateNode
	{
		protected class DisplayProxy<T>
			where T : UpdateNode
		{
			public DisplayProxy(T parent)
			{
				this.parent = parent;
			}

			public UpdateNode[] Children
			{
				get { return parent.children.ToArray(); }
			}

			public InfoValuePair[] Fields
			{
				get { return parent.fields.ToArray(); }
			}

			protected readonly T parent;
		}
	}
}
#endregion
#endif
#endregion
