using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Module
{
	public class StatementFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string AlwaysAppliedCompany = "Always Applied Company";
			public const string AlwaysAppliedType = "Always Applied Type";
			public const string Status = "Status";
			public const string RepresentativeId = "Representative ID";
			public const string RepresentativeOrganization = "Representative Organization";
			public const string DueDate = "Due Date";
			public const string Profile = "Profile";
			public const string Type = "Type";
			public const string ProcessDate = "Process Date";
			public const string StatementNumber = "Statement Number";
			public const string ReferenceNumber = "Reference Number";
			public const string EntryNumber = "Entry Number";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var companyFilter = result.AddGuidFilter(Schema.AlwaysAppliedCompany, ModuleIDs.GlbCompany, GetAlwaysAppliedCompanyQuery, Lookups.Companies);
			companyFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			var alwaysAppliedTypeFilter = result.AddTextFilter(Schema.AlwaysAppliedType, GetAlwaysAppliedTypeQuery, Lookups.DirectionList);
			alwaysAppliedTypeFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			var statusFilter = result.AddTextFilter(Schema.Status, CusStatementHeaderSchema.B2_Status, Lookups.StatusList);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("3849B130-1A06-447A-B8C8-0CCAB65E37D0", Schema.Status);

			var representativeIdFilter = result.AddNumberFilter(Schema.RepresentativeId, CusStatementHeaderSchema.B2_ImporterCustomsID);
			representativeIdFilter.MultilingualDescription = ResString.GetMultilingualString("B34214BF-8D37-412B-AE75-8C9D7C9758B8", Schema.RepresentativeId);

			var representativeOrganizationFilter = result.AddGuidFilter(Schema.RepresentativeOrganization, ModuleIDs.Organisation, CusStatementHeaderSchema.B2_OH_Importer, Lookups.Consignees);
			representativeOrganizationFilter.MultilingualDescription = ResString.GetMultilingualString("2A35AEDC-F714-4F22-92D4-03A8BF1EE8D2", Schema.RepresentativeOrganization);

			var dueDateFilter = result.AddDateFilter(Schema.DueDate, CusStatementHeaderSchema.B2_DueDate);
			dueDateFilter.MultilingualDescription = ResString.GetMultilingualString("56582C76-EA9B-455B-A016-197014C9A4AC", Schema.DueDate);

			var profileFilter = result.AddNumberFilter(Schema.Profile, CusStatementHeaderSchema.B2_EntryFilerCode);
			profileFilter.MultilingualDescription = ResString.GetMultilingualString("758C0F0A-4EC8-4172-823E-04ECECC0D3EF", Schema.Profile);

			var typeFilter = result.AddTextFilter(Schema.Type, CusStatementHeaderSchema.B2_BranchDesignation, Lookups.DirectionList);
			typeFilter.Category = FilterCategories.ModesAndTypes;
			typeFilter.MultilingualDescription = ResString.GetMultilingualString("52675DCD-0E58-4F2B-8630-F4766CFBC7D2", Schema.Type);

			var processDateFilter = result.AddDateFilter(Schema.ProcessDate, CusStatementHeaderSchema.B2_ProcessDate);
			processDateFilter.MultilingualDescription = ResString.GetMultilingualString("0337A963-6FEB-4E55-99E0-ED3F0CFBDFEE", Schema.ProcessDate);

			var statementNumberFilter = result.AddNumberFilter(Schema.StatementNumber, CusStatementHeaderSchema.B2_StatementNumber);
			statementNumberFilter.MultilingualDescription = ResString.GetMultilingualString("7790FE04-F807-4FF6-8971-764571372094", Schema.StatementNumber);

			var referenceNumberFilter = result.AddNumberFilter(Schema.ReferenceNumber, CusStatementLineSchema.B3_BrokerReference);
			referenceNumberFilter.SubGroup = DCGStatementLineSubGroup;
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("9668C3A1-3F92-499B-ABF1-C1FED20CA1D9", Schema.ReferenceNumber);

			var entryNumberFilter = result.AddNumberFilter(Schema.EntryNumber, CusStatementLineSchema.B3_EntryNum);
			entryNumberFilter.SubGroup = DCGStatementLineSubGroup;
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("52EBBAB1-8E65-433C-9FD3-BCC0968FAB6A", Schema.EntryNumber);

			return result;
		}

		ZQuery GetAlwaysAppliedCompanyQuery(ZGuid value)
		{
			return new ZQuery(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
		}

		ZQuery GetAlwaysAppliedTypeQuery(ZString value)
		{
			return new ZQuery(CusStatementHeaderSchema.B2_BranchDesignation, Lookups.DirectionList.GetAllCodes());
		}

		public DCGStatementLineSubGroup DCGStatementLineSubGroup => dcgStatementEntrySubGroup ?? (dcgStatementEntrySubGroup = new DCGStatementLineSubGroup());
		DCGStatementLineSubGroup dcgStatementEntrySubGroup;

		public StatementFilterStripLookups Lookups => lookups ?? (lookups = new StatementFilterStripLookups(Factory));
		StatementFilterStripLookups lookups;
	}
}
