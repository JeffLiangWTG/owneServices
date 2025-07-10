using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ExitControlMainMenuProvider : EU.ExitControl.GUI.ExitControlMainMenuProvider
	{
		public ExitControlMainMenuProvider(CusExitHeader header) : base(header)
		{
		}

		new CusExitHeader header => (CusExitHeader)base.header;

		protected override ZMenuItem[] GetAdditionalMainMenuItemsCore()
		{
			return new[] {
				new EU.ExitControl.GUI.CreateExitReportMenuItemCreator(null).Create(),
				new EU.ExitControl.GUI.SelectReportItemMenuItemCreator(null).Create(),
				EU.ExitControl.GUI.ExitControlMenuItem.CreateSendToCustomsMenuItemSeparatorMenuItem(),
				new ExitControlSendToCustomsMenuCreator(header).Create(),
				new ExitControlUploadSupportingDocumentsMenuCreator(null).Create(),
			};
		}
	}
}
