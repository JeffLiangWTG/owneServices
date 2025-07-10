using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Module
{
	public class StandAloneFsrEnquiryFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddNumberFilter("Reference Number", EDIMessageSchema.EM_ApplicationReference)
				.MultilingualDescription = ResString.GetMultilingualString("DFB58564-0CC7-4648-BDDC-09D61A25EACF", "Reference Number");
			result.AddDateFilter("Date Created", EDIMessageSchema.EM_SystemCreateTimeUtc, convertFromLocalToUTC: true)
				.MultilingualDescription = ResString.GetMultilingualString("29E287AC-8895-4A84-AD76-8D61C7AAA938", "Date Created");
			result.AddTextFilter("Querying Profile", EDIMessageSchema.EM_MessageOwner, () => CusHAWBLookups.GetProfilesList(null, Factory))
				.MultilingualDescription = ResString.GetMultilingualString("6BE19811-86D0-4A81-9641-50D4AFCFC35E", "Querying Profile");
			result.AddTextFilter("Response Text", new GetTextQueryWithOperator(GetResponseTextQuery))
				.MultilingualDescription = ResString.GetMultilingualString("6E7B8836-9614-41C9-926B-C531BD872E4A", "Response Text");
			result.AddFlagsFilter("Has Response?", ["Ticked for yes, unticked for no"], new GetFlagsQuery[] { GetHasResponseQuery })
				.MultilingualDescription = ResString.GetMultilingualString("9F52F67B-102D-42B7-8539-89F5949BB37C", "Has Response?");

			return result;
		}

		ZQuery GetResponseTextQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			var addOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			addOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry.Subcode);
			addOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, EDIMessageSchema.Constants.Prefix);
			addOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);
			messageQuery.AddSubQuery(addOnQuery, JoinCondition.And);
			return messageQuery;
		}

		ZQuery GetHasResponseQuery(ZBool value)
		{
			var query = new ZQuery();
			if (!value)
			{
				query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, SQLComparisonOperator.Equal, DBNull.Value);
				query.AddToFilter(EDIMessageSchema.EM_LinkTable, SQLComparisonOperator.NotEqual, EDIMessage.Schema.TableName);
			}
			else
			{
				query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, SQLComparisonOperator.NotEqual, DBNull.Value);
				query.AddToFilter(EDIMessageSchema.EM_LinkTable, SQLComparisonOperator.Equal, EDIMessage.Schema.TableName);
			}
			return query;
		}

		public override ZQuery Filter
		{
			get
			{
				var f = base.Filter;
				var branchPks = (from GlbBranch b in GlbCompany.CurrentCompany.Branches select b.PK).ToArray();
				f.AddToFilter(EDIMessageSchema.EM_GB, branchPks);
				return f;
			}
		}
	}
}

