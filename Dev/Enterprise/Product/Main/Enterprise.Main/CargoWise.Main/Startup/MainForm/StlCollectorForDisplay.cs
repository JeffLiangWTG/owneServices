using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class StlCollectorForDisplay : NonPersistentBusinessObject
	{
		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|FeatureCode", Caption = "Feature Code")]
		public ZString FeatureCode { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|RoleName", Caption = "Role Name")]
		public ZString RoleName { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|ModuleName", Caption = "Module Name")]
		public ZString ModuleName { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|FunctionName", Caption = "Function Name")]
		public ZString FunctionName { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|FeatureName", Caption = "Feature Name")]
		public ZString FeatureName { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|DataGranularity", Caption = "Data Granularity")]
		public ZString DataGranularity { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|CompanyCode", Caption = "Company Code")]
		public ZString CompanyCode { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|BranchCode", Caption = "Branch Code")]
		public ZString BranchCode { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|TransactionDateUtc", Caption = "Transaction Date Coordinated Universal Time")]
		public ZString TransactionDateUtc { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|CreatingUserCode", Caption = "Creating User Code")]
		public ZString CreatingUserCode { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|GuidReference", Caption = "Guid Reference")]
		public ZString GuidReference { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|BillingReference1", Caption = "Billing Reference 1")]
		public ZString BillingReference1 { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|BillingReference2", Caption = "Billing Reference 2")]
		public ZString BillingReference2 { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|BillingReference3", Caption = "Billing Reference 3")]
		public ZString BillingReference3 { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|BillingReference4", Caption = "Billing Reference 4")]
		public ZString BillingReference4 { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|AdditionalRefs", Caption = "Additional References")]
		public ZString AdditionalRefs { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|TransactionCount", Caption = "Transaction Count")]
		public ZString TransactionCount { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|PreparationScript", Caption = "Preparation Script")]
		public ZString PreparationScript { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|FromClause", Caption = "From Clause")]
		public ZString FromClause { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|WhereClause", Caption = "Where Clause")]
		public ZString WhereClause { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|WithOptionRecompile", Caption = "With Option Recompile")]
		public ZBool WithOptionRecompile { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|UsedInBilling", Caption = "Used In Billing")]
		public ZBool UsedInBilling { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|ActiveOn", Caption = "Active On")]
		public ZString ActiveOn { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|MinCW1Version", Caption = "Min CW1 Version")]
		public ZString MinCW1Version { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|MaxCW1Version", Caption = "Max CW1 Version")]
		public ZString MaxCW1Version { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|DateType", Caption = "Date Type")]
		public ZString DateType { get; set; }

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Integration.Billing|CollectionStartDateUtc", Caption = "Collection Start Date Coordinated Universal Time")]
		public ZString CollectionStartDateUtc { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is string representation of an STL Collector definition")]
		public override string ToString()
		{
			return $@"FeatureCode: {FeatureCode}, RoleName: {RoleName}, ModuleName: {ModuleName}, FunctionName: {FunctionName}, FeatureName: {FeatureName},
						DataGranularity: {DataGranularity}, CompanyCode: {CompanyCode}, BranchCode: {BranchCode}, TransactionDateUtc: {TransactionDateUtc},
						CreatingUserCode: {CreatingUserCode}, GuidReference: {GuidReference}, BillingReference1: {BillingReference1},
						BillingReference2: {BillingReference2}, BillingReference3: {BillingReference3}, BillingReference4: {BillingReference4},
						AdditionalRefs: {AdditionalRefs}, TransactionCount: {TransactionCount}, PreparationScript: {PreparationScript},
						FromClause: {FromClause}, WhereClause: {WhereClause}, WithOptionRecompile: {WithOptionRecompile},
						UsedInBilling: {UsedInBilling}, ActiveOn: {ActiveOn}, MinCW1Version: {MinCW1Version}, MaxCW1Version: {MaxCW1Version},
						DateType: {DateType}, CollectionStartDateUtc: {CollectionStartDateUtc}";
		}
	}
}
