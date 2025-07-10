using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class AdditionalInfoLookups : EU.ExitControl.Business.AdditionalInfoLookups
	{
		public AdditionalInfoLookups(AdditionalInfo parent)
			: base(parent)
		{
		}

		public override ICollection CodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Ireland,
			EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument, ZDateTime.Today);
	}
}
