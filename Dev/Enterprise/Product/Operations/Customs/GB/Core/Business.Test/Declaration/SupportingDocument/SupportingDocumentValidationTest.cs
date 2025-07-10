using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestThatC600AndC601TypeCodesAllowBlankBothActionsAndAvailability()
		{
			supportingDocument.CSI_Code = "C600";
			supportingDocument.CSI_Actions = string.Empty;
			AssertNoMessageErrors(supportingDocument.CSI_ActionsInfo);

			supportingDocument.CSI_Code = "C600";
			supportingDocument.CSI_Availability = string.Empty;
			AssertNoMessageErrors(supportingDocument.CSI_AvailabilityInfo);

			supportingDocument.CSI_Code = "C601";
			supportingDocument.CSI_Actions = string.Empty;
			AssertNoMessageErrors(supportingDocument.CSI_ActionsInfo);

			supportingDocument.CSI_Code = "C601";
			supportingDocument.CSI_Availability = string.Empty;
			AssertNoMessageErrors(supportingDocument.CSI_AvailabilityInfo);
		}

		public void TestValidActionsAndAvailability()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, "Action Pumpo");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "Sminky Pinky Bang Bang");
			var actionD = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, "D", "Dog", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var availD = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "D", "Daniel", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AE", "XX", "DD", "QQ" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.CountryCodes.UnitedKingdom,
				new string[] { importCodeType, exportCodeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			supportingDocument.CSI_Code = "9001";
			supportingDocument.CSI_Availability = "A";
			supportingDocument.CSI_Actions = "E";
			AssertNoMessageErrors(supportingDocument.CSI_AvailabilityInfo);
			AssertNoMessageErrors(supportingDocument.CSI_ActionsInfo);

			supportingDocument.CSI_Actions = "X";
			supportingDocument.Validation.ValidateCSI_Availability();
			AssertHasMessageErrorContaining(supportingDocument.CSI_AvailabilityInfo, validAvailabilityError);
			AssertHasMessageErrorContaining(supportingDocument.CSI_ActionsInfo, validActionError);

			supportingDocument.CSI_Availability = "X";
			supportingDocument.CSI_Actions = "E";
			supportingDocument.Validation.ValidateCSI_Availability();
			AssertHasMessageErrorContaining(supportingDocument.CSI_AvailabilityInfo, validAvailabilityError);
			AssertHasMessageErrorContaining(supportingDocument.CSI_ActionsInfo, validActionError);

			supportingDocument.CSI_Availability = "";
			supportingDocument.CSI_Actions = "";
			supportingDocument.CSI_Actions = "D";
			supportingDocument.CSI_Availability = "D";
			AssertNoMessageErrorContaining(supportingDocument.CSI_AvailabilityInfo, validAvailabilityError);
			AssertNoMessageErrorContaining(supportingDocument.CSI_ActionsInfo, validActionError);

			AssertEquals(expected: true, supportingDocument.Lookups.ActionList.ContainsCode("D"));
			AssertEquals("Dog", supportingDocument.Lookups.ActionList["D"].Description);
			AssertEquals(expected: true, supportingDocument.Lookups.AvailabilityList.ContainsCode("D"));
			AssertEquals("Daniel", supportingDocument.Lookups.AvailabilityList["D"].Description);

			AssertEquals("'Q' from the ACTAV attributes should be OK even if there is no refcuscodelist explaining it", expected: true, supportingDocument.Lookups.ActionList.ContainsCode("Q"));
			AssertEquals("Action Q", supportingDocument.Lookups.ActionList["Q"].Description);
			AssertEquals(expected: true, supportingDocument.Lookups.AvailabilityList.ContainsCode("Q"));
			AssertEquals("Availability Q", supportingDocument.Lookups.AvailabilityList["Q"].Description);
		}

		public void TestWritingOffFields()
		{
			string countryCodeUK = Core.Constants.CountryCodes.UnitedKingdom;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCodeUK))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(countryCodeUK, "United Kingdom");

				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsTaxUQ, "Tax Unit of Quantity");
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Unit of Quantity");

				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsTaxUQ, "KGM", "Kilogram", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsTaxUQ, "LTR", "Liter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsTaxUQ, "WAT", "Watt", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "GBP", "UK Pound", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MTR", "Metre", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateCusCodeList(countryCodeUK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MTQ", "Cubic Metres", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

				Factory.Save();

				var dec = Factory.New<JobDeclaration>();
				dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				var sDataGroupCode = dec.GetDefaultDataGroupingCode();

				AssertEquals(countryCodeUK, dec.CountryCode);
				AssertEquals(countryCodeUK, sDataGroupCode);

				var invoice = dec.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();

				var arrSupDoc = new List<SupportingDocument>();
				arrSupDoc.Add(dec.SupportingDocuments.AddNew());
				arrSupDoc.Add(invoice.SupportingDocuments.AddNew());
				arrSupDoc.Add(invoiceLine.SupportingDocuments.AddNew());

				const string errMsgNotInList = "The code you have selected is not in the list.";

				foreach (SupportingDocument supDoc in arrSupDoc)
				{
					AssertEquals(countryCodeUK, supDoc.ImportExportParent.TrueCountryCode);
					AssertEquals(dec.GetDefaultDataGroupingCode(), supDoc.ImportExportParent.DataGroupingCode);
					AssertEquals(70, SupportingDocument.Schema.CSI_ReferenceNumber2MaxLength);

					var dtNow = ZDateTime.Now;

					supDoc.CSI_ReferenceNumber2 = "Issuing Authority";
					supDoc.CSI_DateOfIssue = dtNow;
					supDoc.CSI_UnitOfQuantity = "KGM";
					supDoc.CSI_UnitOfQuantity2 = "G";

					AssertNoErrors(supDoc.CSI_ReferenceNumber2Info);
					AssertNoErrors(supDoc.CSI_DateOfIssueInfo);
					AssertNoErrors(supDoc.CSI_UnitOfQuantityInfo);
					AssertNoErrors(supDoc.CSI_UnitOfQuantity2Info);

					AssertNoMessageErrorContaining(supDoc.CSI_UnitOfQuantityInfo, errMsgNotInList);

					AssertEquals("Issuing Authority", supDoc.CSI_ReferenceNumber2);
					AssertEquals(dtNow, supDoc.CSI_DateOfIssue);
					AssertEquals("KGM", supDoc.CSI_UnitOfQuantity);
					AssertEquals("G", supDoc.CSI_UnitOfQuantity2);

					supDoc.CSI_UnitOfQuantity = "XYZ";
					AssertHasMessageErrorContaining(supDoc.CSI_UnitOfQuantityInfo, errMsgNotInList);

					supDoc.CSI_UnitOfQuantity = "";
					AssertNoErrors(supDoc.CSI_UnitOfQuantityInfo);
					AssertNoMessageErrorContaining(supDoc.CSI_UnitOfQuantityInfo, errMsgNotInList);
				}
			}
		}

		public void TestSupportingDocsNoActionsOrAvailability_NoError()
		{
			Assert(supportingDocument.ImportExportParent.IsExport);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom", eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, "Sup Doc Action");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "Sup Doc Availability");
			var docNorm = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DOC1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var docNone = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DOC2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var availNorm = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAvailability, "U", "Usual", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var actionNorm = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GbBox44SupportingDocumentAction, "N", "Norm", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ACTAV", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, Core.Constants.CountryCodes.UnitedKingdom);
			docNorm.Attributes.AddNew("Direction", "IMPORT");
			docNorm.Attributes.AddNew("Direction", "EXPORT");
			docNorm.Attributes.AddNew("Level", "ITEM");
			docNorm.Attributes.AddNew("ACTAV", "UN");

			docNone.Attributes.AddNew("Direction", "IMPORT");
			docNone.Attributes.AddNew("Direction", "EXPORT");
			docNone.Attributes.AddNew("Level", "ITEM");

			Factory.Save();

			supportingDocument.CSI_Code = "DOC1";
			supportingDocument.CSI_Actions = ZString.Empty;
			supportingDocument.CSI_Availability = ZString.Empty;
			supportingDocument.Validation.ValidateCSI_Availability();
			supportingDocument.Validation.ValidateCSI_Actions();
			AssertHasMessageErrorContaining(supportingDocument.CSI_AvailabilityInfo, "Please enter an Availability.");
			AssertHasMessageErrorContaining(supportingDocument.CSI_ActionsInfo, "Please enter an Action.");
			supportingDocument.CSI_Actions = "N";
			supportingDocument.CSI_Availability = "U";
			supportingDocument.Validation.ValidateCSI_Availability();
			supportingDocument.Validation.ValidateCSI_Actions();
			AssertNoMessageErrorContaining(supportingDocument.CSI_AvailabilityInfo, "Please enter an Availability.");
			AssertNoMessageErrorContaining(supportingDocument.CSI_ActionsInfo, "Please enter an Action.");

			supportingDocument.CSI_Actions = ZString.Empty;
			supportingDocument.CSI_Availability = ZString.Empty;
			supportingDocument.CSI_Code = "DOC2";

			Assert(supportingDocument.Lookups.AvailabilityList.Count == 0);
			Assert(supportingDocument.Lookups.ActionList.Count == 0);

			supportingDocument.Validation.ValidateCSI_Availability();
			supportingDocument.Validation.ValidateCSI_Actions();
			AssertNoMessageErrorContaining(supportingDocument.CSI_AvailabilityInfo, "Please enter an Availability.");
			AssertNoMessageErrorContaining(supportingDocument.CSI_ActionsInfo, "Please enter an Action.");
		}

		public void TestSupportingDocumentReferenceNumberAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom"));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "DC44I");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DC44E");

			var doc1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "DOC1", "DOC 1", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(doc1.PK, GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReferenceNumber, "Y");
			var doc2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DOC2", "DOC 2", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(doc2.PK, GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReferenceNumber, "Y");
			var doc3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DOC3", "DOC 3", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				supportingDocument.CSI_Code = doc1.ZZD_Code;
				supportingDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertRefCusCodeHasAttributeAndValue(GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReferenceNumber, "Y");
				AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, referenceNumberError);

				supportingDocument.CSI_ReferenceNumber = "AR1";
				supportingDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoError(supportingDocument.CSI_ReferenceNumberInfo, referenceNumberError);
				supportingDocument.CSI_ReferenceNumber = ZString.Empty;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				supportingDocument.CSI_Code = doc2.ZZD_Code;
				AssertRefCusCodeHasAttributeAndValue(GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReferenceNumber, "Y");
				supportingDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertHasMessageErrorContaining(supportingDocument.CSI_ReferenceNumberInfo, referenceNumberError);
				supportingDocument.CSI_Code = doc3.ZZD_Code;
				supportingDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertEquals("Does not have ReferenceNumber attribute", expected: false, supportingDocument.RefCusCode.HasAttribute(GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReferenceNumber));
				AssertNoError(supportingDocument.CSI_ReferenceNumberInfo, referenceNumberError);
			});
		}

		public void TestSupportingDocumentReasonAttribute()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom"));
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "DC44I");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DC44E");

			var doc1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "DOC1", "DOC 1", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(doc1.PK, GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReason, "Y");
			var doc2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DOC2", "DOC 2", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateNewOrGetExistingCusCodeListAttribute(doc2.PK, GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReason, "Y");
			var doc3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "DOC3", "DOC 3", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				supportingDocument.CSI_Code = doc1.ZZD_Code;
				supportingDocument.Validation.ValidateCSI_Description();
				AssertRefCusCodeHasAttributeAndValue(GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReason, "Y");
				AssertHasMessageErrorContaining(supportingDocument.CSI_DescriptionInfo, reasonError);

				supportingDocument.CSI_Description = "AR1";
				supportingDocument.Validation.ValidateCSI_Description();
				AssertNoError(supportingDocument.CSI_DescriptionInfo, reasonError);
				supportingDocument.CSI_Description = ZString.Empty;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				supportingDocument.CSI_Code = doc2.ZZD_Code;
				AssertRefCusCodeHasAttributeAndValue(GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReason, "Y");
				supportingDocument.Validation.ValidateCSI_Description();
				AssertHasMessageErrorContaining(supportingDocument.CSI_DescriptionInfo, reasonError);
				supportingDocument.CSI_Code = doc3.ZZD_Code;
				supportingDocument.Validation.ValidateCSI_Description();
				AssertEquals("Does not have Reason attribute", expected: false, supportingDocument.RefCusCode.HasAttribute(GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReason));
				AssertNoError(supportingDocument.CSI_DescriptionInfo, reasonError);
			});
		}

		void AssertRefCusCodeHasAttributeAndValue(ZString attribute, ZString value)
		{
			AssertEquals("Has attribute", expected: true, supportingDocument.RefCusCode.HasAttribute(attribute));
			AssertEquals("Has attribute value", value, supportingDocument.RefCusCode.GetAttribute(attribute));
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		}

		SupportingDocument supportingDocument;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;
		#endregion

		const string validAvailabilityError = "This availability cannot be used with the selected action";
		const string validActionError = "This action cannot be used with the selected availability";
		const string referenceNumberError = "This supporting document type requires a reference/ID";
		const string reasonError = "This supporting document type requires a reason/description";
	}
}
