using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ReportsGridUserControlMenuProvider : EU.ExitControl.GUI.ReportsGridUserControlMenuProvider
	{
		public ReportsGridUserControlMenuProvider(IReportsGridUserControlProvider provider) : base(provider)
		{
		}

		protected override ZMenuItem[] GetAdditionalReportsGridMenuItems()
		{
			return new[] {
				new SelectReportItemMenuItemCreator(provider).Create(),
				new ExitControlUploadSupportingDocumentsMenuCreator(provider).Create(),
			};
		}

		protected override ZMenuItem[] GetAdditionalMenuItemsForMainForm()
		{
			return new[] {
				new SelectReportItemMenuItemCreator(provider).Create(),
				new ExitControlUploadSupportingDocumentsMenuCreator(provider).Create(),
			};
		}
	}
}
