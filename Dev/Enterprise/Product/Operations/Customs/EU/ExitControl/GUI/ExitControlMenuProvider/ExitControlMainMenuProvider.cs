using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class ExitControlMainMenuProvider : IExitControlMainMenuProvider
	{
		public ExitControlMainMenuProvider(CusExitHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}
		protected readonly CusExitHeader header;

		IReadOnlyList<ZMenuItem> IExitControlMainMenuProvider.AdditionalMainMenuItems => additionalMainMenuItems ??= GetAdditionalMainMenuItems();
		ZMenuItem[] additionalMainMenuItems;

		ZMenuItem[] GetAdditionalMainMenuItems() => GetAdditionalMainMenuItemsCore();
		protected virtual ZMenuItem[] GetAdditionalMainMenuItemsCore() => new[] {
			new CreateExitReportMenuItemCreator(null).Create(),
			new ExitControlSendToCustomsMenuCreator(header).Create()
		};
	}
}
