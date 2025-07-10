using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using ECB = Enterprise.Customs.Business;
using UniversalConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CusSupportingDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSupportingDocumentsList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Import", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Export", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DisplayCode", "Desc.", UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, Core.Constants.CountryCodes.China, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments);
			var code1 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD1", "Code 1");
			code1.Attributes.AddNew("Import", ZString.Empty);
			code1.Attributes.AddNew("DisplayCode", "X");
			var code3 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CDB", "Code Both");
			code3.Attributes.AddNew("Export", ZString.Empty);
			code3.Attributes.AddNew("Import", ZString.Empty);
			code3.Attributes.AddNew("DisplayCode", "b");
			var code2 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "CD2", "Code 2");
			code2.Attributes.AddNew("Export", "");
			code2.Attributes.AddNew("DisplayCode", "Y");
			var code4 = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, UniversalConstants.RefCusCodeListTypes.Codes.CNRequiredDocuments, "1Y", "Certificate of Origin");
			code4.Attributes.AddNew("Import", ZString.Empty);
			code4.Attributes.AddNew("Export", ZString.Empty);
			code4.Attributes.AddNew("DisplayCode", "1Y");
			var codeIgnore = CNCusEntryHeaderHelper.CreateAndSaveNewRefCusCode(Factory, "CUSAB", "CD3", "Code 3");
			codeIgnore.Attributes.AddNew("Import", ZString.Empty);
			codeIgnore.Attributes.AddNew("DisplayCode", "Z");
			Factory.Save();
			testDeclaration.JE_MessageType = ECB.JobMessageTypeList.Codes.Import;
			var list = testItem.Lookups.SupportingDocumentsList;
			AssertEquals("Should have one and only one item.", 2, list.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "CD1", "CDB" }, list.GetAllCodes());
			AssertArrayEqualsByElements(new[] { "X.Code 1", "b.Code Both" }, list.ToArray().Select(x => x.Description).ToArray());
			testDeclaration.JE_MessageType = ECB.JobMessageTypeList.Codes.Export;
			list = testItem.Lookups.SupportingDocumentsList;
			AssertEquals("Should have one and only one item.", 2, list.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "CD2", "CDB" }, list.GetAllCodes());
			AssertArrayEqualsByElements(new[] { "Y.Code 2", "b.Code Both" }, (list.ToArray().Select(x => x.Description)).ToArray());
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDeclaration = Factory.New<JobDeclaration>();
			testDeclaration.JE_MessageType = ECB.JobMessageTypeList.Codes.Import;
			var invoice = testDeclaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			testItem = invoiceLine.CusSupportingDocuments.AddNew();
		}

		JobDeclaration testDeclaration;
		CusSupportingDocument testItem;
	}
}
