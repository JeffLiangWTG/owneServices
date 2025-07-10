using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocument))]
	public class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
	{
		public void TestCloneActionsAndAvailability()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
				new string[] { importCodeType, exportCodeType }, "TEST1", "Test 1", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();
			supportingDocument.CSI_Code = "TEST1";

			var clonedSupportingDocument = (SupportingDocument)supportingDocument.Clone();

			AssertEquals("A", clonedSupportingDocument.CSI_Availability);
			AssertEquals("C", clonedSupportingDocument.CSI_Actions);
		}

		public void TestSetDescriptionToCDSWaiverWhen999L()
		{
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var eunCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(eunCountryCode, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "United Kingdom", eun);
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, new[] { importCodeType, exportCodeType },
				"999L", "Goods qualify for a document waiver or exemption from the measure shown against a specific commodity code",
				new Dictionary<string, string[]>
				{
					["LEVEL"] = new[] { "ITEM" },
					["StatementText"] = new[] { "CDS Waiver" }
				},
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;

			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var suppDoc = invoiceLine.SupportingDocuments.AddNew();
			suppDoc.CSI_Code = "999L";
			AssertEquals("Chief Declarations should not default CSI_Description to 'CDS Waiver' when CSI_Code = '999L'", "", suppDoc.CSI_Description);

			suppDoc.CSI_Code = "";
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			suppDoc.CSI_Code = "999L";
			AssertEquals("CDS Declarations should default CSI_Description to 'CDS Waiver' when CSI_Code = '999L'", "CDS Waiver", suppDoc.CSI_Description);
		}

		public void TestDefaultActionsAndAvailability()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			//No codes valid
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
				new string[] { importCodeType, exportCodeType }, "TEST1", "Test 1", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			//JE not fine, but others are
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC", "AE" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
				new string[] { importCodeType, exportCodeType }, "TEST2", "Test 2", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			//JE is fine
			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC", "AE", "AF", "AG", "AT", "GE", "JE", "JS" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
				new string[] { importCodeType, exportCodeType }, "TEST3", "Test 3", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			//J and E are present but not a valid combo
			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC", "AE", "AF", "AG", "AT", "GE", "JP", "JS" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
				new string[] { importCodeType, exportCodeType }, "TEST4", "Test 4", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			supportingDocument.CSI_Code = "TEST1";

			AssertEquals(ZString.Empty, supportingDocument.CSI_Availability);
			AssertEquals(ZString.Empty, supportingDocument.CSI_Actions);

			supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "TEST2";

			AssertEquals("A", supportingDocument.CSI_Availability);
			AssertEquals("C", supportingDocument.CSI_Actions);

			supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "TEST3";

			AssertEquals("J", supportingDocument.CSI_Availability);
			AssertEquals("E", supportingDocument.CSI_Actions);

			supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "TEST4";

			AssertEquals("J", supportingDocument.CSI_Availability);
			AssertEquals("P", supportingDocument.CSI_Actions);
		}

		public void TestReadOnlyStatusOfWriteOffFields()
		{
			string countryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeUK))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingDataGrouping(countryCodeUK, "United Kingdom");
				helper.CreateNewOrGetExistingDataGrouping("CDS", "CDS Grouping");
				var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				helper.CreateNewOrGetExistingCusCodeType(importCodeType, "Document Type (EU Box 44 Imports)");
				helper.CreateNewOrGetExistingCusCodeType(exportCodeType, "Document Type (EU Box 44 Exports)");

				var attributeNameValuePairs = new Dictionary<string, string[]>();
				attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes("CDS", new string[] { importCodeType, exportCodeType }, "C601", "C601 - Description",
					attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				attributeNameValuePairs.Clear();
				attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
				helper.CreateCusCodeListsForMultipleTypesWithAttributes("CDS", new string[] { importCodeType, exportCodeType }, "Y040", "Y040 - Description",
					attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				var dec = Factory.New<JobDeclaration>();
				dec.JE_ApplicationCode = "CDS";

				var invHeader = dec.Invoices.AddNew();
				var invLine = invHeader.InvoiceLines.AddNew();
				var suppDoc = Factory.New<SupportingDocumentForTest>();

				var msgCDS_ReadOnly = "Write-off fields should be read only when CDS and Supporting Document type is not an ITEM level type.";
				var msgCDS_Active = "Write-off fields should not be read only when CDS and Supporting Document type is an ITEM level type.";

				suppDoc.SetParentForTesting(dec);
				suppDoc.CSI_Code = "C601";
				CheckReadOnlyStatusOfTheWriteOffFields(msgCDS_ReadOnly, true, suppDoc);

				suppDoc.CSI_Code = "Y040";
				CheckReadOnlyStatusOfTheWriteOffFields(msgCDS_ReadOnly, true, suppDoc);

				suppDoc.SetParentForTesting(invHeader);
				suppDoc.CSI_Code = "C601";
				CheckReadOnlyStatusOfTheWriteOffFields(msgCDS_Active, false, suppDoc);

				suppDoc.CSI_Code = "Y040";
				CheckReadOnlyStatusOfTheWriteOffFields(msgCDS_ReadOnly, true, suppDoc);

				suppDoc.SetParentForTesting(invLine);
				suppDoc.CSI_Code = "C601";
				CheckReadOnlyStatusOfTheWriteOffFields(msgCDS_Active, false, suppDoc);

				suppDoc.CSI_Code = "Y040";
				CheckReadOnlyStatusOfTheWriteOffFields(msgCDS_ReadOnly, true, suppDoc);

				dec.JE_ApplicationCode = "CHF";
				var msgChief = "Write-off fields should be read only for Chief.";

				suppDoc.SetParentForTesting(dec);
				suppDoc.CSI_Code = "C601";
				CheckReadOnlyStatusOfTheWriteOffFields(msgChief, true, suppDoc);

				suppDoc.CSI_Code = "Y040";
				CheckReadOnlyStatusOfTheWriteOffFields(msgChief, true, suppDoc);

				suppDoc.SetParentForTesting(invHeader);
				suppDoc.CSI_Code = "C601";
				CheckReadOnlyStatusOfTheWriteOffFields(msgChief, true, suppDoc);

				suppDoc.CSI_Code = "Y040";
				CheckReadOnlyStatusOfTheWriteOffFields(msgChief, true, suppDoc);

				suppDoc.SetParentForTesting(invLine);
				suppDoc.CSI_Code = "C601";
				CheckReadOnlyStatusOfTheWriteOffFields(msgChief, true, suppDoc);

				suppDoc.CSI_Code = "Y040";
				CheckReadOnlyStatusOfTheWriteOffFields(msgChief, true, suppDoc);
			}
		}

		public void TestUnitOfQUantityFieldType()
		{
			AssertEquals(nameof(FieldType.TextDropEdit), Factory.New<SupportingDocument>().UnitOfQuantityFieldType);
		}

		void CheckReadOnlyStatusOfTheWriteOffFields(string msg, bool expectedValue, SupportingDocumentForTest suppDoc)
		{
			AssertEquals(msg, expectedValue, suppDoc.IsWriteOffFieldsReadOnly);
			AssertEquals(msg, expectedValue, suppDoc.CSI_ReferenceNumber2Info.ReadOnly);
			AssertEquals(msg, expectedValue, suppDoc.CSI_DateOfIssueInfo.ReadOnly);
			AssertEquals(msg, expectedValue, suppDoc.CSI_UnitOfQuantityInfo.ReadOnly);
			AssertEquals(msg, expectedValue, suppDoc.CSI_UnitOfQuantity2Info.ReadOnly);
		}

		public void TestSetPropertiesFromCusAuthorisationHeader()
		{
			var header = Factory.New<Customs.Business.CusAuthorisationHeader>();
			header.CPH_RN_NKCountryCode = "GB";
			header.CPH_Type = "ABC";
			header.CPH_Number = "123";
			AssertSetPropertiesFromCusAuthorisationHeader(header, "GBABC123");

			var rule = Factory.New<Customs.Business.CusAuthorisationRule>();
			rule.CPR_CPH_PermitHeader = header.PK;
			rule.CPR_RuleCode = GBCusAuthorisationRuleTypeBaseList.Codes.CTY;
			rule.CPR_ValueFrom = GBCusAuthorisationCountryCodePrefixList.Codes.XI;
			header.CusAuthorisationRules.Add(rule);
			AssertSetPropertiesFromCusAuthorisationHeader(header, "XIABC123");
		}

		void AssertSetPropertiesFromCusAuthorisationHeader(Customs.Business.CusAuthorisationHeader header, string expectedRefNumber)
		{
			var suppDoc = Factory.New<SupportingDocumentForTest>();
			suppDoc.SetPropertiesFromCusAuthorisationHeader(header, "XYZ");
			CombineAssertions(() =>
			{
				AssertEquals("Value of CSI_Code", "XYZ", suppDoc.CSI_Code);
				AssertEquals("Value of CSI_ReferenceNumber", expectedRefNumber, suppDoc.CSI_ReferenceNumber);
			});
		}

		#region Implementation

		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.SupportingDocuments.AddNew();
			var invoice = declaration.Invoices.AddNew();
			yield return invoice.SupportingDocuments.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.SupportingDocuments.AddNew();
			var product = factory.New<OrgSupplierPart>();
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.Both;
			yield return pivot.SupportingDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.SupportingDocuments.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			invoiceHeader = declaration.Invoices.AddNew();
			supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
		}

		SupportingDocument supportingDocument;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;

		#endregion
	}

	class SupportingDocumentForTest : SupportingDocument
	{
		public SupportingDocumentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void SetParentForTesting(BusinessObject parent)
		{
			base.SetParent(parent);
		}

		public bool IsWriteOffFieldsReadOnly => base.IsNotItemLevelCode;
	}
}
