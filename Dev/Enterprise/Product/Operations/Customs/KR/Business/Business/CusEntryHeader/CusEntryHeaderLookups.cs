using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class CusEntryHeaderLookups : Customs.Business.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent)
			: base(parent)
		{
		}

		public CusEntryHeader EntryHeader
		{
			get { return Parent; }
		}

		protected new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		public override CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<CustomsMessageStatusTypeList>();

		public override CodeDescriptionPairList CH_MessageTypeList
		{
			get
			{
				return Factory.GetCachedValue("CusEntryHeaderCHMessageTypeList", delegate
				{
					var result = new CodeDescriptionPairList();
					result = new CodeDescriptionPairList();
					result.AddPair(JobMessageTypeList.Codes.Export, JobMessageTypeList.Codes.Export);
					result.AddPair(JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.Import);
					result.AddPair(ElectronicDocumentTypeList.Codes._5DQ, ElectronicDocumentTypeList.Descriptions._5DQ);
					result.AddPair(ElectronicDocumentTypeList.Codes._5DP, ElectronicDocumentTypeList.Descriptions._5DP);
					result.AddPair(ElectronicDocumentTypeList.Codes._D87, ElectronicDocumentTypeList.Descriptions._D87);
					return result;
				});
			}
		}

		public override CodeDescriptionPairList CH_EntryStatusList => Factory.GetCachedValue<CustomsEntryStatusTypeList>();

		public ZZRefCusCodeListCombinedCollection CountryOfOriginsReqDHRList
		{
			get
			{
				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.DetailedFTACountries, ZDateTime.Today);
				collection.Load();
				return collection;
			}
		}
		public CusStatementHeaderCollection Statements => new CusStatementHeaderCollection(Factory);
	}
}
