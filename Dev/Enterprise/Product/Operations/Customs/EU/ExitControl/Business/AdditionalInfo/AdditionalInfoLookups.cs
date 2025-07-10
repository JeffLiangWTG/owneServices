using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class AdditionalInfoLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
	{
		public AdditionalInfoLookups(AdditionalInfo parent)
			: base(parent)
		{
		}

		AdditionalInfo ExitReportItemAdditionalInfo => (AdditionalInfo)Parent;

		CusExitReportItem ExitReportItem => ExitReportItemAdditionalInfo.Parent as CusExitReportItem;

		CusExitReport ExitReport => ExitReportItem?.Report;

		public override ICollection CodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(
			Factory,
			ExitReport?.CountryCode ?? Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument,
			ZDateTime.Today
		);
	}
}
