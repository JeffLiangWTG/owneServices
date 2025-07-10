using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukGenralMessageFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string Pima = "PIMA";
			public const string LocalProfile = "Local Profile";
			public const string Status = "Status";
			public const string Direction = "Direction";
			public const string DateCreated = "Date created";
			public const string HasRelatedMessage = "Has related message";
			public const string Purpose = "Purpose";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter(FilterConstants.Pima, EDIMessageSchema.EM_ApplicationReference)
				.MultilingualDescription = ResString.GetMultilingualString("7F2FAEE7-56F1-4D30-AED1-8ED079F4E685", FilterConstants.Pima);
			result.AddTextFilter(FilterConstants.LocalProfile, EDIMessageSchema.EM_MessageOwner, () => CusHAWBLookups.GetProfilesList(null, Factory))
				.MultilingualDescription = ResString.GetMultilingualString("6B9B74CD-A25C-4290-BE82-C72B6796DF5F", FilterConstants.LocalProfile);
			result.AddTextFilter(FilterConstants.Status, EDIMessageSchema.EM_Status, () => new EDIMessageStatusList())
				.MultilingualDescription = ResString.GetMultilingualString("9EADDD94-8A22-4BE9-8239-A8B0A36C799A", FilterConstants.Status);
			result.AddTextFilter(FilterConstants.Direction, EDIMessageSchema.EM_ReceiveTransmit, new ReceiveTransmitList())
				.MultilingualDescription = ResString.GetMultilingualString("FD95EDD3-4483-4683-92DC-9D9D7163E161", FilterConstants.Direction);
			result.AddDateFilter(FilterConstants.DateCreated, EDIMessageSchema.EM_SystemCreateTimeUtc, convertFromLocalToUTC: true)
				.MultilingualDescription = ResString.GetMultilingualString("8098F96B-084B-4924-BFC8-BF117C083488", FilterConstants.DateCreated);
			result.AddFlagsFilter(FilterConstants.HasRelatedMessage, ["Yes"], new GetFlagsQuery[] { GetHasLinkedMessageQuery })
				.MultilingualDescription = ResString.GetMultilingualString("522A89C0-03F1-41AF-89CE-B946C793A184", FilterConstants.HasRelatedMessage);
			result.AddTextFilter(FilterConstants.Purpose, EDIMessageSchema.EM_MessageSubType, () => new GenralPurpose())
				.MultilingualDescription = ResString.GetMultilingualString("C8D3F22C-2377-4FC0-B995-186069B04B84", FilterConstants.Purpose);
			return result;
		}

		ZQuery GetHasLinkedMessageQuery(ZBool value)
		{
			var comparison = value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
			return new ZQuery(EDIMessageSchema.EM_LinkTable, comparison, EDIMessageSchema.Constants.TableName);
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
