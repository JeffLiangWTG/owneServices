using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
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
				new ExitControlSendToCustomsMenuCreator(header).Create(),
				new ExitControlDocRequestMenuCreator(header).Create(),
				new ExitControlLockUnlockForEditMenuCreator(header).Create(),
			};
		}
	}
}
