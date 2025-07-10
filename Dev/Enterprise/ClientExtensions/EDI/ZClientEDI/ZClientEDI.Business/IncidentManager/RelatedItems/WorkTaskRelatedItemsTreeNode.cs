using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class WorkTaskRelatedItemsTreeNode : ZNode<WorkTaskRelatedItemsTreeWrapper>
	{
		public WorkTaskRelatedItemsTreeNode(WorkTaskRelatedItemsTreeModel treeModel, WorkTaskRelatedItemsTreeWrapper bizObj)
				: base(treeModel, bizObj)
		{
		}

		public new WorkTaskRelatedItemsTreeModel TreeModel => (WorkTaskRelatedItemsTreeModel)base.TreeModel;

		#region	Properties

		public ZString Group => BizObjForBinding.Group;
		public ZString StatusDescription => BizObjForBinding.StatusDescription;
		public ZString Type => BizObjForBinding.Type;
		public ZString Description => BizObjForBinding.Description;
		public ZString OrganisationCode => BizObjForBinding.OrganisationCode;
		public ZString OrganisationName => BizObjForBinding.OrganisationName;
		public ZDateTime AgreedDeliveryDate => BizObjForBinding.AgreedDeliveryDate;
		public ZString CurrentTaskStatus => BizObjForBinding.CurrentTaskStatus;
		public ZString CurrentTaskDescription => BizObjForBinding.CurrentTaskDescription;
		public ZString CurrentTaskAssigned => BizObjForBinding.CurrentTaskAssigned;
		public ZString CurrentTaskCapabilityCodeDescription => BizObjForBinding.CurrentTaskCapabilityCodeDescription;
		public ZString SelectionCriterion1Code => BizObjForBinding.SelectionCriterion1Code;
		public ZString SelectionCriterion2Code => BizObjForBinding.SelectionCriterion2Code;
		public ZString SelectionCriterion3Code => BizObjForBinding.SelectionCriterion3Code;
		public ZString SelectionCriterion4Code => BizObjForBinding.SelectionCriterion4Code;
		public ZString SelectionCriterion5Code => BizObjForBinding.SelectionCriterion5Code;
		public ZBool IsClosedOrCancelled => BizObjForBinding.IsClosedOrCancelled;

		#endregion

		#region Overrides

		protected override ChangeParentOnBizObjResult ChangeParentOnBizObj(WorkTaskRelatedItemsTreeWrapper previousParent, WorkTaskRelatedItemsTreeWrapper newParent, bool checkValid) =>
			throw new NotImplementedException("Not required as does not support re-ordering");

		protected override IEnumerable<WorkTaskRelatedItemsTreeWrapper> LoadChildBizObjs() =>
			BizObjForBinding.Children ?? Enumerable.Empty<WorkTaskRelatedItemsTreeWrapper>();

		protected override WorkTaskRelatedItemsTreeWrapper LoadParentBizObj() => null;

		#endregion
	}
}
