using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ScreeningMethodLookups : CusSupportingInfoLookups
	{
		public ScreeningMethodLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListType.Code.Code_IC2SM, ZDateTime.Today);
	}
}
