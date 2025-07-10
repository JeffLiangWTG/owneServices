using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using CodeList = Enterprise.Customs.Universal.CodeDescriptionPairLists;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationAmendmentMessageSendingObjectLookups : JobDeclarationMiscMessageSendingObjectCoreLookups
	{
		public JobDeclarationAmendmentMessageSendingObjectLookups(JobDeclarationAmendmentMessageSendingObject parent) : base(parent)
		{
		}
		new JobDeclarationAmendmentMessageSendingObject Parent => (JobDeclarationAmendmentMessageSendingObject)base.Parent;

		public CodeDescriptionPairList YNCodeList => Factory.GetCachedValue<CodeList.YesNoList>();

		public CodeDescriptionPairList RefundTypeList => Factory.GetCachedValue<RefundTypeList>();

		public CodeDescriptionPairList RefundCauseCodeList => Factory.GetCachedValue<RefundCauseCodeList>();

		public CodeDescriptionPairList RefundReasonCodeList => Factory.GetCachedValue<RefundReasonCodeList>();

		public ZZRefCusCodeListCombinedCollection TaxOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.TaxOffice, ZDateTime.Today);
		public CodeDescriptionPairList DutyPenaltyExemptionCodeList
		{
			get
			{
				var amendmentType = Parent.AmendmentType.SubstringSafe(0, 1);
				return Factory.GetCachedValue("DutyPenaltyExemptionCodeList_GetCodeListForAmendmentType:" + amendmentType, () =>
				{
					var result = new CodeDescriptionPairList();
					if (DutyTaxCorrectionCodeList.Is5UARelevant(amendmentType))
					{
						result.AddPair(Messaging.DutyPenaltyExemptionCodeList.Codes.Y, Messaging.DutyPenaltyExemptionCodeList.Descriptions.Y);
						result.AddPair(Messaging.DutyPenaltyExemptionCodeList.Codes.N, Messaging.DutyPenaltyExemptionCodeList.Descriptions.N);
					}
					else
					{
						result.AddPair(Messaging.DutyPenaltyExemptionCodeList.Codes.X, Messaging.DutyPenaltyExemptionCodeList.Descriptions.X);
					}
					return result;
				});
			}
		}
		public CodeDescriptionPairList AdditiveTaxExemptionReasonCodeList => Factory.GetCachedValue<PenaltyExemptionReasonCodeList>();
		public CodeDescriptionPairList DomesticTaxPenaltyTypeCodeList => Factory.GetCachedValue<TaxPenaltyTypeCodeList>();
		public CodeDescriptionPairList DutyPenaltyTypeCodeList => Factory.GetCachedValue<TaxPenaltyTypeCodeList>();
		public CodeDescriptionPairList DutyPenaltyReducedYNCodeList => Factory.GetCachedValue<DutyPenaltyReducedYNCodeList>();
		public ZZRefCusCodeListCombinedCollection DetectionPatternCodeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.KR006, ZDateTime.Today);
		public CodeDescriptionPairList RefundRequestSubmissionYNCodeList => Factory.GetCachedValue<YesNoList>();
	}
}
