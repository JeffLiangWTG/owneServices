using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Module;

public class EntryHeaderFilterBusinessObject : EU.Module.EntryHeaderFilterBusinessObject
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant Values for IT Filter")]
	public static class ITFilterConstants
	{
		public const string ControlChannel = "Control Channel";
		public const string ElectronicDocumentsUploadRequired = "Electronic Documents Upload Required";
		public const string EntryInvoiceCurrency = "Entry Invoice Currency";
		public const string EntryInvoiceCurrencyAmount = "Entry Invoice Amount";
		public const string ReleaseCode = "Release Code";
		public const string RegistrationNumber = "Registration Number";
		public const string ExitDateFilter = "IVISTO Exit Date";
		public const string ExitProcessingDateFilter = "IVISTO Exit Processing Date";
		public const string ExitOfficeFilter = "IVISTO Exit Office";
		public const string ExitStatusFilter = "IVISTO Exit Status";
		public const string ArrivalDateFilter = "IRILDES Arrival Date";
		public const string ArrivalOfficeFilter = "IRILDES Arrival Office";
		public const string ArrivalStatusFilter = "IRILDES Arrival Status";
		public const string MessageNumber = "Message Number";
	}

	public new EntryHeaderFilterLookups Lookups => (EntryHeaderFilterLookups)base.Lookups;

	protected override Customs.Module.EntryHeaderFilterLookups GetNewLookups() => new EntryHeaderFilterLookups(this);

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var filters = base.GetModuleFiltersCore();
		AddControlChannelFilter(filters);
		AddElectronicDocumentsUploadRequiredFilter(filters);
		AddEntryInvoiceCurrencyFilter(filters);
		AddEntryInvoiceCurrencyAmountFilter(filters);
		new EntryHeaderEntryNumberFilterApplier(this).AddFilters(filters);

		var messageNumberFilter = new EntryHeaderMessageNumberFilterGenerator()
			.Generate(ITFilterConstants.MessageNumber, FilterCategories.Other, messageNumberMultilingualDescription);
		filters.AddFilter(messageNumberFilter);

		return filters;
	}
	#region Implementation

	void AddControlChannelFilter(ModuleFilterCollection filters)
	{
		var controlChannelFilter = filters.AddTextFilter(ITFilterConstants.ControlChannel, GetControlChannelFilterQuery, Lookups.ControlChannelList);
		controlChannelFilter.Category = FilterCategories.StatusAndFlags;
		controlChannelFilter.MultilingualDescription = ResString.GetMultilingualString("D83347C0-48F8-45FF-AADA-E964BE9FCBD0", ITFilterConstants.ControlChannel);
	}

	ZQuery GetControlChannelFilterQuery(SQLComparisonOperator comparisonOperator, ZString value) => QueryHelper.GetQueryHandlingBlanks(CusEntryHeader.GenAddOnColumnConstants.CustomsChannelColumnName, comparisonOperator, value);

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
	void AddElectronicDocumentsUploadRequiredFilter(ModuleFilterCollection filters)
	{
		var flagNames = new string[] { Res.GetString("40C95D7C-04BD-49D6-BF8D-DFED1DBAA10A", ITFilterConstants.ElectronicDocumentsUploadRequired) };
		var queryDelegates = new GetFlagsQuery[] { GetElectronicDocumentsFilterQuery };
		var filter = filters.AddFlagsFilter(ITFilterConstants.ElectronicDocumentsUploadRequired, flagNames, queryDelegates);
		filter.MultilingualDescription = ResString.GetMultilingualString("40C95D7C-04BD-49D6-BF8D-DFED1DBAA10A", ITFilterConstants.ElectronicDocumentsUploadRequired);
	}

	ZQuery GetElectronicDocumentsFilterQuery(ZBool filterApplied)
	{
		var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
		var entryInstructionQuery = new ZDBOnlySubQuery(typeof(CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction);

		var genAddOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID, notIn: !filterApplied);
		genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, ZBool.True.ToString());
		genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, CusEntryInstruction.GenAddOnColumnConstants.UseElectronicDocumentsColumnName);
		genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
		entryInstructionQuery.AddSubQuery(genAddOnQuery, JoinCondition.And);
		entryHeaderQuery.AddSubQuery(entryInstructionQuery, JoinCondition.And);
		return entryHeaderQuery;
	}

	void AddEntryInvoiceCurrencyFilter(ModuleFilterCollection filters)
	{
		var entryInvoiceCurrencyFilter = filters.AddTextFilter(ITFilterConstants.EntryInvoiceCurrency, GetEntryInvoiceCurrencyFilterQuery, Lookups.CurrencyList);
		entryInvoiceCurrencyFilter.Category = FilterCategories.NumbersAndReferences;
		entryInvoiceCurrencyFilter.MultilingualDescription = ResString.GetMultilingualString("DF8C4F2C-471A-4EE0-9AB5-DB96AC4EB063", ITFilterConstants.EntryInvoiceCurrency);
	}

	ZQuery GetEntryInvoiceCurrencyFilterQuery(SQLComparisonOperator comparisonOperator, ZString value) => QueryHelper.GetQueryHandlingBlanks(CusEntryHeader.GenAddOnColumnConstants.InvoiceAmountCurrencyColumnName, comparisonOperator, value);

	void AddEntryInvoiceCurrencyAmountFilter(ModuleFilterCollection filters)
	{
		var entryInvoiceCurrencyAmountFilter = filters.AddNumberRangeFilter(ITFilterConstants.EntryInvoiceCurrencyAmount, GetEntryInvoiceCurrencyAmountFilterQuery);
		entryInvoiceCurrencyAmountFilter.Category = FilterCategories.NumbersAndReferences;
		entryInvoiceCurrencyAmountFilter.MultilingualDescription = ResString.GetMultilingualString("A9E7CACD-775E-4EF7-B1A0-2650BFFDC048", ITFilterConstants.EntryInvoiceCurrencyAmount);
	}

	ZQuery GetEntryInvoiceCurrencyAmountFilterQuery(INumericZType value1, INumericZType value2) => QueryHelper.GetQueryOnGenAddOnColumn(CusEntryHeader.GenAddOnColumnConstants.InvoiceAmountColumnName, value1, value2);

	GenAddOnColumnQueryHelper QueryHelper => queryHelper ?? (queryHelper = new GenAddOnColumnQueryHelper(typeof(CusEntryHeader)));
	GenAddOnColumnQueryHelper queryHelper;

	MultilingualString messageNumberMultilingualDescription =>
		ResString.GetMultilingualString("27181133-C642-4892-BD23-1E7FFC3C6A84", ITFilterConstants.MessageNumber);

	#endregion
}
