using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMBoardLookups : AutoBMBoardLookups
	{
		public BMBoardLookups(AutoBMBoard parent)
			: base(parent)
		{
		}

		#region Systems

		public virtual BMSystemCollection Systems
		{
			get { return new BMSystemCollection(Factory); }
		}

		#endregion

		#region Release Groups

		public ActiveBusinessObjectCollection<GlbGroup> SystemReleaseGroups => Factory.GetCachedValue("ActiveBusinessObjectCollection.SystemReleaseGroups", () => new GlbGroupActiveBusinessObjectCollection(Factory, staffGroupOnly: true));

		#endregion

		#region Customised Cards

		public BMControlCustomisationCollection DetailedCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.DetailedCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.DetailedCard)); }
		}

		public BMControlCustomisationCollection SummaryCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.SummaryCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.TaskCard)); }
		}

		public BMControlCustomisationCollection WorkflowDetailedCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.WorkflowDetailedCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.WorkflowDetailedCard)); }
		}

		public BMControlCustomisationCollection WorkflowSummaryCards
		{
			get { return Factory.GetCachedValue("BMControlCustomisationCollection.WorkflowSummaryCards", () => new BMControlCustomisationCollection(Factory, CustomisedControlTypeList.Codes.WorkflowSummaryCard)); }
		}

		#endregion
	}
}
