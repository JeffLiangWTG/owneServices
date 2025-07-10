using System.Collections;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionFieldDescriptorLookups : ZLookups
	{
		public OperationalActionFieldDescriptorLookups(OperationalActionFieldDescriptor parent)
			: base(parent) { }

		public CodeDescriptionPairList FieldNames
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				OperationalActionContext context = Parent.Context;

				if (context != null)
				{
					foreach (OperationalActionFieldSupporter field in context.FieldSupporters)
					{
						result.Add(field);
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList EmptyBehaviour_List
		{
			get { return emptyBehaviour_List ?? (emptyBehaviour_List = new EmptyBehaviourList()); }
		}
		CodeDescriptionPairList emptyBehaviour_List;

		public FieldDefaultingStrategyList DefaultStrategy_List
		{
			get
			{
				if (defaultStrategy_List == null || defaultStrategy_List.FieldSupporter != Parent.FieldSupporter)
				{
					if (Parent.FieldSupporter == null)
					{
						defaultStrategy_List = new FieldDefaultingStrategyList();
					}
					else
					{
						defaultStrategy_List = Parent.FieldSupporter.GetDefaultingStrategies();
					}
				}
				return defaultStrategy_List;
			}
		}
		FieldDefaultingStrategyList defaultStrategy_List;

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList DefaultValue_List
		{
			get { return defaultStrategy_List.GetBoundCollection(Parent.DefaultingStrategy, Factory); }
		}

		new OperationalActionFieldDescriptor Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (OperationalActionFieldDescriptor)base.Parent; }
		}
	}
}
