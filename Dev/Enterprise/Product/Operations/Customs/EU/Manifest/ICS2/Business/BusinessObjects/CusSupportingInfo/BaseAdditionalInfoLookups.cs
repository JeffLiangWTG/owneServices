using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class BaseAdditionalInfoLookups : CusSupportingInfoLookups
	{
		public BaseAdditionalInfoLookups(CusSupportingInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList
		{
			get
			{
				var codeList = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListType.Code.Code_IC2AI, ZDateTime.Today);
				if (!codeList.IsLoaded)
				{
					codeList.Load();
					codeList.Sort(RefCusCodeListSchema.Constants.ZZD_Code);
				}

				return codeList;
			}
		}

		public override CodeDescriptionPairList SubTypeList => new EUICS2HRCMAdditionalInfoTypes();
	}
}
