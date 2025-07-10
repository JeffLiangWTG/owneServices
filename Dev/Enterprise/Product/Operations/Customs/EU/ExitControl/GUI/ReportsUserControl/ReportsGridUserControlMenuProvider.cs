using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class ReportsGridUserControlMenuProvider : IReportsGridUserControlMenuProvider
	{
		public ReportsGridUserControlMenuProvider(IReportsGridUserControlProvider provider)
		{
			this.provider = Argument.NotNull(provider, nameof(provider));
		}
		protected readonly IReportsGridUserControlProvider provider;

		IReadOnlyList<ZMenuItem> IReportsGridUserControlMenuProvider.AdditionalReportsGridMenuItems => additionalReportsGridMenuItems ?? (additionalReportsGridMenuItems = GetAdditionalReportsGridMenuItems());
		ZMenuItem[] additionalReportsGridMenuItems;

		protected virtual ZMenuItem[] GetAdditionalReportsGridMenuItems() => new[] { new SelectReportItemMenuItemCreator(provider).Create() };

		IReadOnlyList<ZMenuItem> IReportsGridUserControlMenuProvider.AdditionalMenuItemsForMainForm => additionalMenuItemsForMainForm ?? (additionalMenuItemsForMainForm = GetAdditionalMenuItemsForMainForm());
		ZMenuItem[] additionalMenuItemsForMainForm;

		protected virtual ZMenuItem[] GetAdditionalMenuItemsForMainForm() => new[] { new SelectReportItemMenuItemCreator(provider).Create() };
	}
}
