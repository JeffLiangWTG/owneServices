using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class ConsignmentsGridUserControlMenuProvider : IConsignmentsGridUserControlMenuProvider
	{
		public ConsignmentsGridUserControlMenuProvider(IConsignmentsGridUserControlProvider provider)
		{
			Provider = Argument.NotNull(provider, nameof(provider));
		}
		protected IConsignmentsGridUserControlProvider Provider { get; }

		IReadOnlyList<ZMenuItem> IConsignmentsGridUserControlMenuProvider.AdditionalConsignmentsGridMenuItems => additionalConsignmentsGridMenuItems ?? (additionalConsignmentsGridMenuItems = GetAdditionalConsignmentsGridMenuItems());
		ZMenuItem[] additionalConsignmentsGridMenuItems;

		protected virtual ZMenuItem[] GetAdditionalConsignmentsGridMenuItems() => new[] { GetCreateExitReportMenuItem() };

		IReadOnlyList<ZMenuItem> IConsignmentsGridUserControlMenuProvider.AdditionalMenuItemsForMainForm => additionalMenuItemsForMainForm ?? (additionalMenuItemsForMainForm = new[] { GetCreateExitReportMenuItem() });
		ZMenuItem[] additionalMenuItemsForMainForm;

		protected virtual ZMenuItem GetCreateExitReportMenuItem() => new CreateExitReportMenuItemCreator(Provider).Create();
	}
}
