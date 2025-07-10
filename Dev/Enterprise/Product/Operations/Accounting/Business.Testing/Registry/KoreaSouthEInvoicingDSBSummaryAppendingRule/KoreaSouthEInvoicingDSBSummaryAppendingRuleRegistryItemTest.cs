using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItem))]
	class KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItemTest : StronglyTypedRegistryItemTestCase<KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection>
	{
		protected override StronglyTypedRegistryItem<KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection, KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection> GetNewRegistryItem()
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}

	[TestedType(typeof(KoreaSouthEInvoicingDSBSummaryAppendingRuleDataType))]
	class KoreaSouthEInvoicingDSBSummaryAppendingRuleDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<KoreaSouthEInvoicingDSBSummaryAppendingRuleDataType>
	{
		#region Implementation

		protected override KoreaSouthEInvoicingDSBSummaryAppendingRuleDataType GetNewDataType()
		{
			return new KoreaSouthEInvoicingDSBSummaryAppendingRuleDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var taxRate1 = factory.New<AccTaxRate>();
			taxRate1.FillWithValidTestData();
			taxRate1.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			var taxRate2 = factory.New<AccTaxRate>();
			taxRate2.FillWithValidTestData();
			taxRate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;

			factory.Save();

			var sample1 = new KoreaSouthEInvoicingDSBSummaryAppendingRule(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), factory);
			sample1.TaxIdPK = taxRate1.PK;
			sample1.Order = 0;

			var sample2 = new KoreaSouthEInvoicingDSBSummaryAppendingRule(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), factory);
			sample2.TaxIdPK = taxRate2.PK;
			sample2.Order = 1;

			var collection1 = new KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), factory)
			{
				sample1
			};

			var collection2 = new KoreaSouthEInvoicingDSBSummaryAppendingRuleCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), factory)
			{
				sample2
			};

			var expectedXmlValue_collection1 = $"<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfKoreaSouthEInvoicingDSBSummaryAppendingRule xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><KoreaSouthEInvoicingDSBSummaryAppendingRule><TaxIdPK>{taxRate1.PK}</TaxIdPK><Order>0</Order></KoreaSouthEInvoicingDSBSummaryAppendingRule></ArrayOfKoreaSouthEInvoicingDSBSummaryAppendingRule>";

			var expectedXmlValue_collection2 = $"<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfKoreaSouthEInvoicingDSBSummaryAppendingRule xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><KoreaSouthEInvoicingDSBSummaryAppendingRule><TaxIdPK>{taxRate2.PK}</TaxIdPK><Order>1</Order></KoreaSouthEInvoicingDSBSummaryAppendingRule></ArrayOfKoreaSouthEInvoicingDSBSummaryAppendingRule>";

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, Encoding.Unicode.GetBytes(expectedXmlValue_collection1)),
				new ValidSampleAndBinaryValueInDB(collection2, Encoding.Unicode.GetBytes(expectedXmlValue_collection2))
			};
		}

		#endregion Implementation
	}
}
