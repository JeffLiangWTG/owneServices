using Enterprise.Customs.FR.Registry;
using Enterprise.Services.OperationalActions.Business;

namespace Enterprise.Customs.FR.Module
{
	public class FilterIsDeltaIEEnabledForImportsOrExportsConstraint : IFilterConstraint
	{
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get => "IsDeltaIEEnabledForImportsOrExports";
		}

		public string SingularValueName => Res.GetString("6717E4CC-22D9-4F2A-8E1B-DFCBE48B28BA", "value");

		public string PluralValueName => Res.GetString("B7B9EDDA-26A3-499A-AB20-CFCD3FDBC67A", "values");

		public string Description => Res.GetString("OperationalActionsFilter|IsDeltaIEEnabledForImportsOrExports|Description",
				"Matches if at least one of the registry item Customs > France > Enable Delta I/E for Imports or Customs > France > Enable Delta I/E for Exports is turned on.\r\ne.g. '{0} == \"Y\"' will only match if at least one of the registry item Customs > France > Enable Delta I/E for Imports or Customs > France > Enable Delta I/E for Exports is turned on.",
				Name);

		public string GetDefaultStringValue()
		{
			return FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.Value || FRCustomsDataRegistry.Instance.EnableDeltaIEForExports.Value ? "Y" : "N";
		}

		public object GetValue()
		{
			return GetDefaultStringValue();
		}
	}
}
