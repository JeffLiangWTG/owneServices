using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	public class K84MessageCollection : Enterprise.Messaging.Business.NonDependentEDIMessageCollection
	{
		public K84MessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new EDIMessage AddNew()
		{
			return (EDIMessage)base.AddNew();
		}

		public new EDIMessage this[int index]
		{
			get { return (EDIMessage)base[index]; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			var k84Query = new ZQuery();
			k84Query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
			k84Query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.K84Report);
			result.AddToFilter(k84Query, JoinCondition.Or);

			var arlQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			var genAddOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, EDIMessageSchema.Constants.Prefix);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, EDIMessage.Schema.XMLCustomsMessageType);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.NotEqual, ZString.Empty);
			arlQuery.AddSubQuery(genAddOnQuery, JoinCondition.And);
			arlQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, XmlEDIMessage.ApplicationCodes.UniversalDataMessaging);
			arlQuery.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			arlQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch);

			var dnsoaQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			dnsoaQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, XmlEDIMessage.ApplicationCodes.UniversalDataMessaging);
			dnsoaQuery.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			dnsoaQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, new[] { ARLMessageTypes.Codes.StatementOfAccount, ARLMessageTypes.Codes.DailyNotice });
			result.AddToFilter(arlQuery, JoinCondition.Or);
			result.AddToFilter(dnsoaQuery, JoinCondition.Or);
			return result;
		}
	}
}
