using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public class CreateExitReportMenuItemCreator : EU.ExitControl.GUI.CreateExitReportMenuItemCreator
	{
		public CreateExitReportMenuItemCreator(IConsignmentsGridUserControlProvider provider) : base(provider)
		{
		}

		protected override void CreateExitReport(EU.ExitControl.Business.CusExitConsignment consignment)
		{
			((CusExitConsignment)consignment).CreateOrUpdateReport();
		}
	}
}
