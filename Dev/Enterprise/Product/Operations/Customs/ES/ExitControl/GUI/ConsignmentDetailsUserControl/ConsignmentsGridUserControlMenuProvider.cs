using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public class ConsignmentsGridUserControlMenuProvider : EU.ExitControl.GUI.ConsignmentsGridUserControlMenuProvider
	{
		public ConsignmentsGridUserControlMenuProvider(IConsignmentsGridUserControlProvider provider) : base(provider)
		{
		}

		protected override ZMenuItem GetCreateExitReportMenuItem() => new CreateExitReportMenuItemCreator(Provider).Create();
	}
}
