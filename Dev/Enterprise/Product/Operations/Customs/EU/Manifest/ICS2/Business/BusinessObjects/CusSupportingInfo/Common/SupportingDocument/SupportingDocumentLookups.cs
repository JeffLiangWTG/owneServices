using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants.Customs.Universal;
using RefCusCodeListType = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusCodeListType;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SupportingDocumentLookups : CusSupportingInfoLookups
	{
		public SupportingDocumentLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListType.Code.Code_IC2DT, ZDateTime.Today);
	}
}
