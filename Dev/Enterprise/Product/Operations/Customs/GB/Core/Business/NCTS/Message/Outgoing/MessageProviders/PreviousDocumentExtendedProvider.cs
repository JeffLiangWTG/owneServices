using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class PreviousDocumentExtendedProvider : PreviousDocumentProvider, IPreviousDocumentExtended
	{
		public PreviousDocumentExtendedProvider(CusSupportingInfo document, bool isInPhase5TransitionPeriod) : base(document, isInPhase5TransitionPeriod)
		{
		}

		public int? GoodsItemNumber => !document.CSI_ItemNumber.IsDefault ? document.CSI_ItemNumber : null;

		public string TypeOfPackages => shouldOutputPackageData ? (string)document.CSI_UnitOfQuantity2 : null;

		public int? NumberOfPackages => shouldOutputPackageData ? document.CSI_Quantity2.ToZInt() : null;

		public string MeasurementUnitAndQualifier => shouldOutputPackageData ? (string)document.CSI_UnitOfQuantity : null;

		public decimal? Quantity => shouldOutputPackageData ? document.CSI_Quantity : null;

		bool shouldOutputPackageData => !document.CSI_UnitOfQuantity2.IsEmpty && !document.CSI_Quantity2.IsEmpty;
	}
}
