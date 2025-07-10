using CargoWise.EntityFramework;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ActivitySubtypeAssignmentLookups : ZLookups
	{
		public ActivitySubtypeAssignmentLookups(ActivitySubtypeAssignment parent)
		: base(parent) { }

		#region Implementation

		protected new ActivitySubtypeAssignment Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ActivitySubtypeAssignment)base.Parent; }
		}

		#endregion

		protected override BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;

		public CodeDescriptionPairList ActivitySubtypeList
		{
			get
			{
				var tree = ProcessManagementRegistry.Instance.WorkItemTypeTree.Value;
				var list = new CodeDescriptionPairList();

				for (int i = 0;i < tree.Count; i++)
				{
					var node = tree[i];
					if (tree.GetDepth(node) == 4)
					{
						if (!list.ContainsCode(node.Code))
						{
							list.AddPair(node.Code, node.Description);
						}
					}
				}
				return list;
			}
		}
	}
}
