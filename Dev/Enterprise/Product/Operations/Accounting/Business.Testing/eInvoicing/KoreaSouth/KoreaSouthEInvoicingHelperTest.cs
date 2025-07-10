using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.EInvoicing.KoreaSouth.Testing
{
	public class KoreaSouthEInvoicingHelperTest : TestCaseWithFactory
	{
		public void TestGetIssueIDFromInvoice()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			AssertNullOrEmpty(KoreaSouthEInvoicingHelper.GetIssueIDFromInvoice(arInvoice));

			var referenceKRI = Factory.New<AccTransactionHeaderReference>();
			referenceKRI.AH1_AH = arInvoice.PK;
			referenceKRI.AH1_Type = AccTransactionHeaderReferenceTypes.KRI;
			referenceKRI.AH1_Reference = "TEST001";
			Factory.Save();
			AssertEquals("TEST001", KoreaSouthEInvoicingHelper.GetIssueIDFromInvoice(arInvoice));
		}

		public void TestGetTaxInvoiceDocumentTypeCode()
		{
			var registryItem = AccountingMasterFilesRegistry.Instance.TaxTypeToTaxInvoiceDocumentTypeCodeMapping;
			var newValueForRegistry = registryItem.DefaultValue.Clone(null, null) as CodeDescriptionWithGroupCollection;

			var taxType = AccountingMasterFilesConstants.NullTaxRateType.Code;

			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == taxType).Group = AccountingMasterFilesConstants.KoreaEInvoicingTypeCodeCategory.NotApplicable;
			newValueForRegistry.Cast<CodeDescriptionWithGroup>().Single(x => x.Code == "NOT").Group = AccountingMasterFilesConstants.KoreaEInvoicingTypeCodeCategory.NotApplicable;
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValueForRegistry);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.KoreaSouth))
			{
				var mockIFeatureControlManager = new FeatureControlTestDataFactory().CreateKoreaSouthComplianceSubTypeFeatureControlMock(true);

				using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
				{
					var result = KoreaSouthEInvoicingHelper.GetTaxInvoiceDocumentTypeCode("", new List<ZString>() { "NOT" }, () => false, 1m);
					AssertEquals(string.Empty, result);

					result = KoreaSouthEInvoicingHelper.GetTaxInvoiceDocumentTypeCode("101", new List<ZString>() { "NOT" }, () => false, 1m);
					AssertEquals("0101", result);
				}

				mockIFeatureControlManager = new FeatureControlTestDataFactory().CreateKoreaSouthComplianceSubTypeFeatureControlMock(false);

				using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
				{
					var result = KoreaSouthEInvoicingHelper.GetTaxInvoiceDocumentTypeCode("101", new List<ZString>() { "NOT" }, () => false, 1m);
					AssertEquals(string.Empty, result);
				}
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
