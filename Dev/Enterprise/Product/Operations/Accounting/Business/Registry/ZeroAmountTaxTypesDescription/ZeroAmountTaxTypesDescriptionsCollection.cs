using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ZeroAmountTaxTypesDescriptionsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new ZeroAmountTaxTypesDescriptions this[int index]
		{
			get { return (ZeroAmountTaxTypesDescriptions)Elements[index]; }
		}

		public new ZeroAmountTaxTypesDescriptions AddNew()
		{
			return (ZeroAmountTaxTypesDescriptions)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ZeroAmountTaxTypesDescriptions();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ZeroAmountTaxTypesDescriptionsCollection();
		}

		#endregion

		public static ZeroAmountTaxTypesDescriptionsCollection GetDefault()
		{
			var result = new ZeroAmountTaxTypesDescriptionsCollection();

			AddDefaultZeroAmountTaxTypesDescriptions(
				result.AddNew(),
				AccTaxRate.Types.Rated,
				ZeroAmountTaxTypeDescription.ZeroRate,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.Rated));
			AddDefaultZeroAmountTaxTypesDescriptions(
				result.AddNew(),
				AccTaxRate.Types.Exempt,
				ZeroAmountTaxTypeDescription.Exempt,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.Exempt));
			AddDefaultZeroAmountTaxTypesDescriptions(
				result.AddNew(),
				AccTaxRate.Types.NotReportable,
				ZeroAmountTaxTypeDescription.NotReportable,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.NotReportable));
			AddDefaultZeroAmountTaxTypesDescriptions(
				result.AddNew(),
				AccTaxRate.Types.ReverseRated,
				ZeroAmountTaxTypeDescription.Reverse,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.ReverseRated));
			AddDefaultZeroAmountTaxTypesDescriptions(
				result.AddNew(),
				AccTaxRate.Types.Suspended,
				ZeroAmountTaxTypeDescription.Suspended,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.Suspended));
			AddDefaultZeroAmountTaxTypesDescriptions(
				result.AddNew(),
				AccTaxRate.Types.ReportableUnderBusinessTax,
				ZeroAmountTaxTypeDescription.ReportableUnderBusinessTax,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.ReportableUnderBusinessTax));
			AddDefaultZeroAmountTaxTypesDescriptions(
				result.AddNew(),
				AccTaxRate.Types.ExcludedFromTheTaxBase,
				ZeroAmountTaxTypeDescription.ExcludedFromTheTaxBase,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.ExcludedFromTheTaxBase));
			AddDefaultZeroAmountTaxTypesDescriptions(
				result.AddNew(),
				AccTaxRate.Types.CapitalRated,
				ZeroAmountTaxTypeDescription.ZeroRatedCapital,
				AccountingConstants.ZeroAmountTaxTypesDescriptionList.GetMultilingualDescriptionFromCode(AccTaxRate.Types.CapitalRated));

			return result;
		}

		static void AddDefaultZeroAmountTaxTypesDescriptions(ZeroAmountTaxTypesDescriptions zeroAmountTaxTypesDescriptions, string taxType, string description, MultilingualString defaultValue)
		{
			zeroAmountTaxTypesDescriptions.TaxType = taxType;
			zeroAmountTaxTypesDescriptions.Description = description;
			zeroAmountTaxTypesDescriptions.DefaultValue = defaultValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded description")]
		public static class ZeroAmountTaxTypeDescription
		{
			public const string ZeroRate = "Zero Rated tax treatments are RAT (Rated) Tax types with a 0% tax rate.";
			public const string Exempt = "Exempt Tax ID’s are used when charges are classified as Exempt from VAT/GST under the laws of a country.";
			public const string NotReportable = "Not Reportable Tax ID’s are used to identify ‘Out of Scope’ supplies.";
			public const string Reverse = @"RVS (Reverse Rated) Tax types shift the VAT/ GST collection and reporting obligation onto the customer when used on an Accounts Receivable Transaction.
When used on an Accounts Payable transaction, Reverse Rated Tax ID’s recognize that the VAT / GST collection and reporting obligation for the sale has been shifted to the Login Company.";
			public const string Suspended = "SUS (Suspended) Tax types identify charges where VAT/GST was calculated but not charged. This tax type is used in Sri Lanka.";
			public const string ReportableUnderBusinessTax = "BST (Business Tax) is an obsolete tax type relevant in China while the China government migrates the country to a full VAT tax structure";
			public const string ExcludedFromTheTaxBase = "Excluded Tax ID’s are used when charges are classified as excluded from the VAT/GST reporting base  under the laws of the country.";
			public const string ZeroRatedCapital = @"Zero Rated ‘Capital’ acquisitions / sales are CAP (Capital) Tax Types with a 0% tax rate.";
		}
	}
}
