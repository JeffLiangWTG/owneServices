using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconDeclarationLookups : Customs.Business.CusReconDeclarationLookups
	{
		public CusReconDeclarationLookups(CusReconDeclaration parent) : base(parent)
		{
		}
		public override CodeDescriptionPairList MessageStatusList => CustomsMessageStatusTypeList.GetStatusListForRefundDeclaration(Factory);

		public CodeDescriptionPairList EntryStatusList => CustomsEntryStatusTypeList.GetStatusListForRefundDeclaration(Factory);

		public CodeDescriptionPairList RefundTypeList => Factory.GetCachedValue<RefundTypeList>();

		public CodeDescriptionPairList RefundCauseCodeList => Factory.GetCachedValue<RefundCauseCodeList>();

		public CodeDescriptionPairList RefundReasonCodeList => Factory.GetCachedValue<RefundReasonCodeList>();

		public new ZZRefCusCodeListCombinedCollection CustomsOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection CustomsDivisionList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsDepartment, ZDateTime.Today);

		public ZZRefCusCodeListCombinedCollection TaxOfficeList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.TaxOffice, ZDateTime.Today);

		public CodeDescriptionPairList BankTypeList => Factory.GetCachedValue<BankTypeList>();

		public OrgHeaderCollection Organisations => new OrgHeaderCollection(Factory);
	}
}
