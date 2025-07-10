using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.Business.OrgSupplierPart;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : ImportExportAwareSupportingInfoTest<SupportingDocument>
	{
		public void TestIsCodeAPermitType()
		{
			Factory.CreateSupportingDocumentCodeLists(
				new TestSupportingDocumentCodeList("AAA", isImport: true, hasPermitAttribute: true),
				new TestSupportingDocumentCodeList("AAA", isImport: false, hasPermitAttribute: false));
			Factory.Save();

			var supportingDocument1 = declaration.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "AAA";

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			Assert("Current document is permit type because AAA of import type has a permit attribute", supportingDocument1.IsCodeAPermitType);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			Assert("Current document is permit type because AAA of export type doesn't have a permit attribute", !supportingDocument1.IsCodeAPermitType);
		}

		public void TestSupportingDocumentQtyValue()
		{
			supportingDocument.CSI_Quantity = 12345678901.123;
			AssertEquals("Qty should allow decimal 11.3 - 12345678901.123", 12345678901.123m, supportingDocument.CSI_Quantity);
		}

		public void TestCSI_Quantity_DecimalPlaces()
		{
			AssertEquals(6, supportingDocument.QuantityDecimalPlaces);
		}

		public void TestKeyToDeterimeUniqueness()
		{
			var sd = (SupportingDocument)GetNewBusinessObject();
			sd.CSI_Code = "N123";
			sd.CSI_ReferenceNumber = "FOO";
			AssertEquals("N123FOO", sd.KeyToDeterimeUniqueness);
		}

		public void TestParentDirection()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export", supportingDocument.ParentDirection);
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Import", supportingDocument.ParentDirection);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var product = (MasterFiles.OrgSupplierPart)OrgSupplierPart.New(Factory);
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = org.PK;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Enterprise.Customs.Business.BaseCusClassification.ClassificationType.Both;
			var document = pivot.SupportingDocuments.AddNew();
			AssertEquals("Both", document.ParentDirection);
		}

		public void TestRefCusCode()
		{
			var latviaCountryCode = Core.Constants.CountryCodes.Latvia;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(latviaCountryCode))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eunCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
				var eun = helper.CreateNewOrGetExistingDataGrouping(eunCountryCode, "European Union");
				helper.CreateNewOrGetExistingDataGrouping(latviaCountryCode, "Latvia", eun);

				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(eunCountryCode, new string[] { exportCodeType }, "SD01", "SD01 DES"
				, new Dictionary<string, string[]>(), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.Save();

				var sd = (SupportingDocument)GetNewBusinessObject();
				sd.CSI_Code = "N123";
				AssertNull(sd.RefCusCode);
				sd.CSI_Code = "SD01";
				AssertEquals("SD01", sd.RefCusCode.ZZD_Code);
			}
		}

		public void TestSupportingDocumentLevel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "9001", "9001 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "9002", "9002 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "9003", "9003 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			CombineAssertions(() =>
			{
				var data = supportingDocument;
				data.CSI_Code = "9001";
				Assert($"IsLine CSI_Code: {data.CSI_Code}", data.IsLine);
				Assert($"!IsHeaderOnly CSI_Code: {data.CSI_Code}", !data.IsHeaderOnly);
				Assert($"IsLineOnly CSI_Code: {data.CSI_Code}", data.IsLineOnly);
				AssertEquals($"IsEffectiveSupportingDocumentsForLine CSI_Code: {data.CSI_Code}", data.IsEffectiveSupportingDocumentsForLine, data.IsLineOnly);

				data = supportingDocumentDec;
				data.CSI_Code = "9002";
				Assert($"!IsLine CSI_Code: {data.CSI_Code}", !data.IsLine);
				Assert($"IsHeaderOnly CSI_Code: {data.CSI_Code}", data.IsHeaderOnly);
				Assert($"IsHeader CSI_Code: {data.CSI_Code}", data.IsHeader);
				Assert($"!IsLineOnly CSI_Code: {data.CSI_Code}", !data.IsLineOnly);
				AssertEquals($"IsEffectiveSupportingDocumentsForLine CSI_Code: {data.CSI_Code}", data.IsEffectiveSupportingDocumentsForLine, data.IsLineOnly);

				data = supportingDocument;
				data.CSI_Code = "9003";
				Assert($"IsLine CSI_Code: {data.CSI_Code}", data.IsLine);
				Assert($"!IsHeaderOnly CSI_Code: {data.CSI_Code}", !data.IsHeaderOnly);
				Assert($"IsHeader CSI_Code: {data.CSI_Code}", data.IsHeader);
				Assert($"!IsLineOnly CSI_Code: {data.CSI_Code}", !data.IsLineOnly);
				AssertEquals($"IsEffectiveSupportingDocumentsForLine CSI_Code: {data.CSI_Code}", data.IsEffectiveSupportingDocumentsForLine, data.IsLineOnly);
			});
		}

		public void TestSuppportingDocumentHumanReadableName()
		{
			AssertEquals("Supporting Document", supportingDocument.HumanReadableName);
		}

		public void TestCSI_ReferenceNumber()
		{
			var permitHeader = Factory.NewWithValidTestData<CusPermitHeader>();
			permitHeader.CPH_Number = "P100";
			permitHeader.CPH_UnitOfMeasure = "KG";
			Factory.Save();

			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_UnitOfQuantity = "";
			supportingDocument.CSI_ReferenceNumber = "P100";
			AssertEquals("When CSI_ReferenceNumber is set to some permit number, we default the unit to CPH_UnitOfMeasure.", "KG", supportingDocument.CSI_UnitOfQuantity);

			supportingDocument.CSI_UnitOfQuantity = "";
			supportingDocument.CSI_ReferenceNumber = "P200";
			AssertEquals("If CSI_ReferenceNumber is not a permit number, we do nothing.", "", supportingDocument.CSI_UnitOfQuantity);

			var supportingDocumentNotSupportPermitIntegration = Factory.New<SupportingDocumentNotSupportPermitIntegration>();
			supportingDocumentNotSupportPermitIntegration.CSI_UnitOfQuantity = "";
			supportingDocumentNotSupportPermitIntegration.CSI_ReferenceNumber = "P100";
			AssertEquals("If the supporting document doesn't support permit integration, we should not default the unit.", "", supportingDocumentNotSupportPermitIntegration.CSI_UnitOfQuantity);
		}

		public void TestSavingForTemporaryAggregation()
		{
			var factory1 = Factory.CreateNewFactory();
			var factory2 = Factory.CreateNewFactory();
			var factory3 = Factory.CreateNewFactory();

			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;
			factory3.RefreshEnabled = false;

			var dec = factory1.New<JobDeclaration>();
			var supDoc = dec.SupportingDocuments.AddNew();

			supDoc.IsUsedForTemporaryAggregation = true;
			supDoc.CSI_Code = "ABC";

			factory1.Save();

			var loadedDec = factory2.Load<JobDeclaration>(dec.PK);
			AssertNotNull(loadedDec);
			AssertEquals(0, loadedDec.SupportingDocuments.Count);

			supDoc.IsUsedForTemporaryAggregation = false;
			supDoc.CSI_Code = "XYZ";

			factory1.Save();

			var loadedDec2 = factory3.Load<JobDeclaration>(dec.PK);
			AssertNotNull(loadedDec2);
			AssertEquals(1, loadedDec2.SupportingDocuments.Count);
		}

		public void TestIsCodeAnInvoiceType()
		{
			supportingDocument.CSI_Code = "N380";
			AssertEquals("IsCodeAnInvoiceType", ZBool.True, supportingDocument.IsCodeAnInvoiceType);

			supportingDocument.CSI_Code = "XXX";
			AssertEquals("IsCodeAnInvoiceType", ZBool.False, supportingDocument.IsCodeAnInvoiceType);

			declaration.JE_MessageType = "IMP";
			supportingDocument.CSI_Code = "D005";
			AssertEquals("IsCodeAnInvoiceType", ZBool.True, supportingDocument.IsCodeAnInvoiceType);

			declaration.JE_MessageType = "EXP";
			AssertEquals("IsCodeAnInvoiceType", ZBool.False, supportingDocument.IsCodeAnInvoiceType);

			var supporingDocumentNoParent = Factory.New<SupportingDocument>();
			supporingDocumentNoParent.CSI_Code = "N380";
			AssertEquals("IsCodeAnInvoiceType", ZBool.False, supportingDocument.IsCodeAnInvoiceType);
		}

		public void TestReferencedPermit()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Number = "ABC";
			supportingDocument.CSI_ReferenceNumber = "ABC";
			AssertNull("ReferencedPermit should not load the guarantee.", supportingDocument.ReferencedPermit);

			var permit = Factory.New<CusPermitHeader>();
			permit.CPH_Number = "DEF";
			supportingDocument.CSI_ReferenceNumber = "DEF";
			AssertSame("ReferencedPermit should load the permit.", permit, supportingDocument.ReferencedPermit);
		}

		[ExpectNoExceptions]
		public void TestSetCSI_CodeTriggersReferenceNumberValidation()
		{
			var mockSupportingDocument = Factory.NewMoq<SupportingDocument>();
			var mockSupportingDocumentValidation = new Mock<SupportingDocumentValidation>(mockSupportingDocument.Object);
			mockSupportingDocument.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation").Returns(mockSupportingDocumentValidation.Object);
			mockSupportingDocumentValidation.Protected().Setup("CheckCSI_ReferenceNumber");

			mockSupportingDocument.Object.CSI_Code = "XXX";
			mockSupportingDocumentValidation.VerifyAll();

			using (mockSupportingDocument.Object.GetValidationSuspender())
			{
				mockSupportingDocumentValidation.Reset();
				mockSupportingDocument.Object.CSI_Code = "YYY";
				mockSupportingDocumentValidation.VerifyAll();
				mockSupportingDocumentValidation.Protected().Verify("CheckCSI_ReferenceNumber", Times.Never());
			}
		}

		public void TestSetCSI_Code_DefaultReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals(declaration.PK, supportingDocument.Parent.PK);

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				declaration.JE_HouseBill = "HouseBill";
				declaration.JE_MasterBill = "MasterBill";
				supportingDocument.CSI_ReferenceNumber = "123123";

				supportingDocument.CSI_Code = "N740";
				AssertEquals("CSI_Reference not equals housebill nor masterbill", "123123", supportingDocument.CSI_ReferenceNumber);

				supportingDocument.CSI_Code = "N741";
				AssertEquals("CSI_Reference not equals housebill nor masterbill", "123123", supportingDocument.CSI_ReferenceNumber);

				supportingDocument.CSI_Code = "N704";
				AssertEquals("CSI_Reference equals masterbill", declaration.JE_MasterBill, supportingDocument.CSI_ReferenceNumber);

				supportingDocument.CSI_Code = "N705";
				AssertEquals("CSI_Reference equals housebill", declaration.JE_HouseBill, supportingDocument.CSI_ReferenceNumber);

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				supportingDocument.CSI_ReferenceNumber = "123123";

				supportingDocument.CSI_Code = "N704";
				AssertEquals("CSI_Reference not equals housebill nor masterbill", "123123", supportingDocument.CSI_ReferenceNumber);

				supportingDocument.CSI_Code = "N705";
				AssertEquals("CSI_Reference not equals housebill nor masterbill", "123123", supportingDocument.CSI_ReferenceNumber);

				supportingDocument.CSI_Code = "N740";
				AssertEquals("CSI_Reference equals housebill", declaration.JE_HouseBill, supportingDocument.CSI_ReferenceNumber);

				supportingDocument.CSI_Code = "N741";
				AssertEquals("CSI_Reference equals masterbill", declaration.JE_MasterBill, supportingDocument.CSI_ReferenceNumber);
			});
		}

		public void TestSetCSI_DescriptionDefaultValue()
		{
			SupportingDocumentTestHelper.SetupRefCusCodeList(Factory);
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var supportingDocument = (SupportingDocument)GetNewBusinessObject();
				supportingDocument.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
				supportingDocument.Declaration.JE_ApplicationCode = "CHF";

				supportingDocument.CSI_Code = "Y057";
				AssertEquals("Import licence not required", supportingDocument.CSI_Description);

				supportingDocument.CSI_Code = "Y922";
				AssertEquals(ZString.Empty, supportingDocument.CSI_Description);

				supportingDocument.CSI_Code = "T123";
				AssertEquals(ZString.Empty, supportingDocument.CSI_Description);
			}
		}

		public void TestUnitOfQuantityFieldType()
		{
			AssertEquals(nameof(FieldType.Text), Factory.New<SupportingDocument>().UnitOfQuantityFieldType);
		}

		public void TestUnitOfQuantity2FieldType()
		{
			AssertEquals(nameof(FieldType.Text), Factory.New<SupportingDocument>().UnitOfQuantity2FieldType);
		}

		public void TestCSI_CodeDescription()
		{
			var countryCode = Core.Constants.CountryCodes.Turkey;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var eunCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
				var eun = helper.CreateNewOrGetExistingDataGrouping(eunCountryCode, "European Union");
				helper.CreateNewOrGetExistingDataGrouping(countryCode, "Turkey", eun);

				var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(eunCountryCode, new string[] { exportCodeType }, "SD01", "SD01 DES"
				, new Dictionary<string, string[]>(), ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.Save();

				var sd = (SupportingDocument)GetNewBusinessObject();
				sd.CSI_Code = "N123";
				AssertNull(sd.RefCusCode);
				sd.CSI_Code = "SD01";
				AssertEquals("SD01", sd.RefCusCode.ZZD_Code);
				AssertEquals("SD01 DES", sd.RefCusCode.ZZD_Description);
				AssertEquals("SD01 DES", sd.CSI_CodeDescription);
			}
		}

		public void TestCSI_ItemNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals(5, supportingDocument.GetPossiblyCustomPropertyMaxLength(SupportingDocument.Schema.CSI_ItemNumber));

				var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_ItemNumberInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption of CSI_ItemNumber", "Document Line No.", data.Caption);
				AssertEquals("Full Caption of CSI_ItemNumber", "[12 03 013 000] Document Line Item Number", data.FullDescription);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_ItemNumberInfo, declaration.MultipleKeysToUse);
				AssertEquals("Caption of CSI_ItemNumber", "Document Line No.", data.Caption);
				AssertEquals("Full Caption of CSI_ItemNumber", string.Empty, data.FullDescription);
			});
		}

		public void TestCSI_AdditionalDescription_Caption()
		{
			CombineAssertions(() =>
			{
				var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_AdditionalDescriptionInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption of CSI_AdditionalDescription", "Issuing Authority Name", data.Caption);
				AssertEquals("Short Caption of CSI_AdditionalDescription", "Authority Name", data.ShortCaption);
				AssertEquals("Full Caption of CSI_AdditionalDescription", "[12 03 010 000] Issuing Authority Name", data.FullDescription);

				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_AdditionalDescriptionInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("ImportUCC6, Caption of CSI_AdditionalDescription", "Issuing Authority Name", data.Caption);
				AssertEquals("ImportUCC6, Short Caption of CSI_AdditionalDescription", "Authority Name", data.ShortCaption);
				AssertEquals("ImportUCC6, Full Caption of CSI_AdditionalDescription", "[12 03 010 000] Supporting Documents < Issuing Authority name", data.FullDescription);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_AdditionalDescriptionInfo, declaration.MultipleKeysToUse);
				AssertEquals("Caption of CSI_AdditionalDescription", "Issuing Authority Name", data.Caption);
				AssertEquals("Short Caption of CSI_AdditionalDescription", "Authority Name", data.ShortCaption);
				AssertEquals("Full Caption of CSI_AdditionalDescription", string.Empty, data.FullDescription);
			});
		}

		public void TestCSI_AdditionalDescription_MaxLength()
		{
			AssertEquals(300, supportingDocument.GetPossiblyCustomPropertyMaxLength(SupportingDocument.Schema.CSI_AdditionalDescription));
		}

		public void TestCSI_ReferenceNumber_Caption()
		{
			CombineAssertions(() =>
			{
				var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_ReferenceNumberInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption of CSI_ReferenceNumber", "Reference", data.Caption);
				AssertEquals("Full Caption of CSI_ReferenceNumber", "[12 03 001 000] Reference Number", data.FullDescription);

				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_ReferenceNumberInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("ImportUCC6, Caption of CSI_ReferenceNumber", "Reference", data.Caption);
				AssertEquals("ImportUCC6, Full Caption of CSI_ReferenceNumber", "[12 03 001 000] Supporting Documents < Reference Number", data.FullDescription);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_ReferenceNumberInfo, declaration.MultipleKeysToUse);
				AssertEquals("Caption of CSI_ReferenceNumber", "Reference", data.Caption);
				AssertEquals("Full Caption of CSI_ReferenceNumber", string.Empty, data.FullDescription);
			});
		}

		public void TestCSI_Quantity_Caption()
		{
			CombineAssertions(() =>
			{
				var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_QuantityInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption of CSI_Quantity", "Quantity", data.Caption);
				AssertEquals("Short Caption of CSI_Quantity", "Qty", data.ShortCaption);
				AssertEquals("Full Caption of CSI_Quantity", "[12 03 006 000] Quantity", data.FullDescription);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_QuantityInfo, declaration.MultipleKeysToUse);
				AssertEquals("Caption of CSI_Quantity", "Quantity", data.Caption);
				AssertEquals("Short Caption of CSI_Quantity", "Qty", data.ShortCaption);
				AssertEquals("Full Caption of CSI_Quantity", string.Empty, data.FullDescription);
			});
		}

		public void TestCSI_Code_Caption()
		{
			CombineAssertions(() =>
			{
				var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_CodeInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption of CSI_Code", "Type", data.Caption);
				AssertEquals("Full Caption of CSI_Code", "[12 03 002 000] Type", data.FullDescription);

				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_CodeInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("Caption of CSI_Code", "Type", data.Caption);
				AssertEquals("Full Caption of CSI_Code", "[12 03 002 000] Supporting Documents < Type", data.FullDescription);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_CodeInfo, declaration.MultipleKeysToUse);
				AssertEquals("Caption of CSI_Code", "Type", data.Caption);
				AssertEquals("Full Caption of CSI_Code", string.Empty, data.FullDescription);
			});
		}

		public void TestCSI_DateOfExpiry_Caption()
		{
			CombineAssertions(() =>
			{
				var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_DateOfExpiryInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption of CSI_DateOfExpiry", "Date of Validity", data.Caption);
				AssertEquals("Full Caption of CSI_DateOfExpiry", "[12 03 011 000] Date of Validity", data.FullDescription);

				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_DateOfExpiryInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("Caption of CSI_DateOfExpiry", "Date of Validity", data.Caption);
				AssertEquals("Full Caption of CSI_DateOfExpiry", "[12 03 011 000] Supporting Documents < Date of validity", data.FullDescription);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_DateOfExpiryInfo, declaration.MultipleKeysToUse);
				AssertEquals("Caption of CSI_DateOfExpiry", "Date of Expiry", data.Caption);
				AssertEquals("Full Caption of CSI_DateOfExpiry", string.Empty, data.FullDescription);
			});
		}

		public void TestCSI_Value_Caption()
		{
			CombineAssertions(() =>
			{
				var data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_ValueInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption of CSI_Value", "Amount", data.Caption);
				AssertEquals("Full Caption of CSI_Value", "[12 03 014 000] Amount", data.FullDescription);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				data = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(supportingDocument.CSI_ValueInfo, declaration.MultipleKeysToUse);
				AssertEquals("Caption of CSI_Value", "Value", data.Caption);
				AssertEquals("Full Caption of CSI_Value", string.Empty, data.FullDescription);
			});
		}

		public void TestSetPropertiesFromCusAuthorisationHeader()
		{
			var header = Factory.New<CusAuthorisationHeader>();
			header.CPH_Number = "123";

			var suppDoc = Factory.New<SupportingDocument>();

			suppDoc.SetPropertiesFromCusAuthorisationHeader(header, "XYZ");

			CombineAssertions(() =>
			{
				AssertEquals("Value of CSI_Code", "XYZ", suppDoc.CSI_Code);
				AssertEquals("Value of CSI_ReferenceNumber", "123", suppDoc.CSI_ReferenceNumber);
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
			var product = factory.New<MasterFiles.OrgSupplierPart>();
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
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
			invoiceHeader = declaration.Invoices.AddNew();
			supportingDocumentDec = declaration.SupportingDocuments.AddNew();
			supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
		}

		SupportingDocument supportingDocumentDec;
		SupportingDocument supportingDocument;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;

		#endregion
	}

	class SupportingDocumentNotSupportPermitIntegration : SupportingDocument
	{
		public SupportingDocumentNotSupportPermitIntegration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsPermitIntegration => false;
	}
}
