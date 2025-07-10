using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	internal static class ParserHelper
	{
		public static ForwardingConsol FindBritishConsolWithThisMawbNumber(BusinessObjectFactory factory, string mawbNo)
		{
			var query = new ZDBOnlyQuery(typeof(ForwardingConsol));
			query.AddToFilter(JobConsolSchema.JK_MasterBillNum, mawbNo);
			query.AddToFilter(JobConsolSchema.JK_TransportMode, Core.Constants.TransportModes.Air);
			query.AddToFilter(JobConsolSchema.JK_RL_NKDischargePort, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.UnitedKingdom);
			query.AddToFilter(JobConsolSchema.JK_IsCancelled, Core.Constants.BooleanFalseString);
			var clonedDatelessQuery = query.DeepClone();
			AddMasterBillDateFilter(query, JobConsolSchema.JK_MasterBillIssueDate);
			var result = factory.LoadTop1<ForwardingConsol>(query);
			if (result == null)
			{
				AddMasterBillDateFilter(clonedDatelessQuery, JobConsolSchema.JK_SystemCreateTimeUtc);
				result = factory.LoadTop1<ForwardingConsol>(clonedDatelessQuery);
			}
			return result;
		}

		public static void AddMasterBillDateFilter(ZQuery query, SchemaDateTimeColumn column)
		{
			var dateQuery = new ZDBOnlyQuery(typeof(ForwardingConsol));
			dateQuery.AddToFilter(JobConsolSchema.JK_MasterBillIssueDate, DBNull.Value);
			dateQuery.AddToFilter(JoinCondition.Or, column, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Now.AddMonths(-12));
			query.AddToFilter(dateQuery);
		}

		public static ForwardingShipment FindBritishShipmentWithThisHawbNumberAndOnThisMasterAndMaybeOriginToo(string hawbNo, string mawbNo, string originPort, EDIMessage ediMsg)
		{
			var query = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZDBOnlyQuery queryClonedForUseWithoutMaster;
			ZDBOnlySubQuery consolShipmentLinkQuery;
			ZDBOnlySubQuery consolSubSubQuery;
			PrepareMainQueryForFindBritishShipmentWithThisHawbNumberAndOnThisMasterAndIfWereReallyStuckThenOriginToo(hawbNo, mawbNo, query, out queryClonedForUseWithoutMaster, out consolShipmentLinkQuery, out consolSubSubQuery);
			var results = ediMsg.Factory.Load<ForwardingShipment>(query);
			ForwardingShipment foundSingleShipment = null;
			if (results.Length == 1)
			{
				foundSingleShipment = results[0];  // Found one house on the consol, great
			}
			else if (results.Length > 1)
			{   // Found many shipments with this mawb and hawb, now apply origin to limit a bit more
				query.AddToFilter(JobShipmentSchema.JS_RL_NKOrigin, SQLComparisonOperator.EndsWith, originPort);
				results = ediMsg.Factory.Load<ForwardingShipment>(query);
				if (results.Length == 1)
				{
					foundSingleShipment = results[0];
				}
				else if (results.Length > 1)
				{
					// Finally, let's make the mawb filter more tight using dates
					AddMasterBillDateFilter(consolSubSubQuery, JobConsolSchema.JK_MasterBillIssueDate);
					consolShipmentLinkQuery.AddSubQuery(consolSubSubQuery, JoinCondition.And);
					queryClonedForUseWithoutMaster.AddSubQuery(consolShipmentLinkQuery, JoinCondition.And);
					foundSingleShipment = FirstShipmentFromQueryOrWarnOfFindingTooManyShipments(queryClonedForUseWithoutMaster, hawbNo, mawbNo, ediMsg);
				}
			}
			return foundSingleShipment;
		}

		static void PrepareMainQueryForFindBritishShipmentWithThisHawbNumberAndOnThisMasterAndIfWereReallyStuckThenOriginToo(string hawbNo, string mawbNo, ZDBOnlyQuery query, out ZDBOnlyQuery queryClonedForUseWithoutMaster, out ZDBOnlySubQuery consolShipmentLinkQuery, out ZDBOnlySubQuery consolSubSubQuery)
		{
			var dateQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			dateQuery.AddToFilter(JobShipmentSchema.JS_SystemCreateTimeUtc, DBNull.Value);
			dateQuery.AddToFilter(JoinCondition.Or, JobShipmentSchema.JS_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Now.AddMonths(-12));
			query.AddToFilter(dateQuery);
			query.AddToFilter(JobShipmentSchema.JS_HouseBill, hawbNo);
			query.AddToFilter(JobShipmentSchema.JS_TransportMode, Core.Constants.TransportModes.Air);
			query.AddToFilter(JobShipmentSchema.JS_RL_NKDestination, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.UnitedKingdom);
#if DEBUG
			query.OrderBy = JobShipmentSchema.JS_UniqueConsignRef.Name;  // so we can convince ourselves in the test that we only get one hit
#endif
			queryClonedForUseWithoutMaster = (ZDBOnlyQuery)query.DeepClone();

			consolShipmentLinkQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			consolSubSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
			consolSubSubQuery.AddToFilter(JobConsolSchema.JK_MasterBillNum, mawbNo);
			consolShipmentLinkQuery.AddSubQuery(consolSubSubQuery, JoinCondition.And);
			query.AddSubQuery(consolShipmentLinkQuery, JoinCondition.And);
		}

		static ForwardingShipment FirstShipmentFromQueryOrWarnOfFindingTooManyShipments(ZQuery query, string hawbNo, string mawbNo, EDIMessage ediMsg)
		{
			var results = ediMsg.Factory.Load<ForwardingShipment>(query);
			if (results.Length == 1)
			{
				return results[0];
			}
			SendEmailWarning(string.Format(CultureInfo.InvariantCulture, "When processing an incoming record from CCSUK, too many matching shipments were found.  A new Hawb record has been created but no shipment has been linked to it for this reason. Mawb={0}, Hawb={1}, MessageNumber={2}. Technical details for support: Query=[{3}]", mawbNo, hawbNo, ediMsg.EM_MessageNum, query.LiteralTextSqlFormatted), null, ediMsg.Factory);
			return null;
		}

		public static void SendEmailWarning(string msg, ICcsukCusAwb job, BusinessObjectFactory factory)
		{
			var company = Guid.Empty;
			var branch = Guid.Empty;
			if (job != null)
			{
				company = job.Branch.PK.ToGuid();
				branch = job.Branch.Company.PK.ToGuid();
			}
			new CcsukEmailSender(factory, CcsukEmailSender.ToWhom.CustomsGroupOnly, (BusinessObject)job).SendEmail("Problem processing inbound FRI data", msg,
				GBCustomsDataRegistry.Instance.NotificationCcsukCuscarFri, "", branch, company, Guid.Empty);
		}
	}
}
