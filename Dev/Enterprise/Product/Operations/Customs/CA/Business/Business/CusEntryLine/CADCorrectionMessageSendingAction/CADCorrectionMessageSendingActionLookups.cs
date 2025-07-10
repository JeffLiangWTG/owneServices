using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CADCorrectionMessageSendingActionLookups : CusSupportingInfoLookups
	{
		public CADCorrectionMessageSendingActionLookups(CADCorrectionMessageSendingAction parent)
			: base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection CARMChangeReasonCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CARMChangeReasonCode, ZDateTime.Today);

		public CodeDescriptionPairList CARMAppealsProgramCodeList
		{
			get
			{
				var date = ZDateTime.Today;
				return Factory.GetCachedValue("CARMAppealsProgramCodeList" + date.ToISO8601ShortDateString(), () =>
				{
					var result = new CodeDescriptionPairList();
					var coll = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CARMAppealsProgramCode, date);
					foreach (var codeList in coll)
					{
						result.AddPair(codeList.ZZD_Code, codeList.ZZD_Description);
					}
					result.Sort();
					return result;
				});
			}
		}
	}
}
