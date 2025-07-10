using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusStatementHeaderLookups : Customs.Business.CusStatementHeaderLookups
	{
		public CusStatementHeaderLookups(CusStatementHeader parent)
			: base(parent)
		{
		}

		public CusStatementHeaderCollection Statements => new CusStatementHeaderCollection(Factory);

		public OrgHeaderCollection Organisations => new OrgHeaderCollection(Factory);

		public ConsigneeCollection ImportersList => new ConsigneeCollection(Factory);

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList
			=> ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, ZDateTime.Today);

		public CodeDescriptionPairList PaymentStatusList => Factory.GetCachedValue<StatementHeaderPaymentStatusList>();

		public CodeDescriptionPairList StatementTypeList => Factory.GetCachedValue<StatementHeaderTypeList>();

		public CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<PaymentPartyList>();
		public CodeDescriptionPairList PaymentTypeList => Factory.GetCachedValue<StatementTypeList>();
		public CodeDescriptionPairList BillTypeList => Factory.GetCachedValue<StatementHeaderStatusList>();
	}
}
