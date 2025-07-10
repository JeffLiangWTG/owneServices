using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using static Enterprise.Customs.Business.BaseCusClassification;
using OrgSupplierPart = Enterprise.Customs.FR.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocument))]
	sealed class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
	{
		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			AssertType<SupportingDocumentValidation>(supportingDocument.Validation);

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			AssertType<DeltaIESupportingDocumentValidation>(supportingDocument.Validation);
		}

		public void TestUnitOfQuantityFieldType()
		{
			SetUpRefDataForSupportDocumentUnitOfQuantityTest(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = string.Empty;
				AssertEquals("UnitOfQuantityFieldType shoule be Text if CSI_Code is empty", nameof(FieldType.Text), supportingDocument.UnitOfQuantityFieldType);
				Assert("ShowDropEditForUnitOfQuantity should be false if CSI_Code is empty", !supportingDocument.ShowDropEditForUnitOfQuantity);
				Assert("ShowTextBoxForUnitOfQuantiy should be true if CSI_Code is empty", supportingDocument.ShowTextBoxForUnitOfQuantiy);

				supportingDocument.CSI_Code = "CODE1";
				AssertEquals("CODE1: UnitOfQuantityFieldType shoule be DropEdit if CSI_Code has UQMapType attribute", nameof(FieldType.TextDropEdit), supportingDocument.UnitOfQuantityFieldType);
				Assert("CODE1: ShowDropEditForUnitOfQuantity shoule be true if CSI_Code has UQMapType attribute", supportingDocument.ShowDropEditForUnitOfQuantity);
				Assert("CODE1: ShowTextBoxForUnitOfQuantiy shoule be false if CSI_Code has UQMapType attribute", !supportingDocument.ShowTextBoxForUnitOfQuantiy);

				supportingDocument.CSI_Code = "CODE2";
				AssertEquals("CODE2: UnitOfQuantityFieldType shoule be DropEdit if CSI_Code has UQMapType attribute", nameof(FieldType.TextDropEdit), supportingDocument.UnitOfQuantityFieldType);
				Assert("CODE2: ShowDropEditForUnitOfQuantity shoule be true if CSI_Code has UQMapType attribute", supportingDocument.ShowDropEditForUnitOfQuantity);
				Assert("CODE2: ShowTextBoxForUnitOfQuantiy shoule be false if CSI_Code has UQMapType attribute", !supportingDocument.ShowTextBoxForUnitOfQuantiy);

				supportingDocument.CSI_Code = "CODE3";
				AssertEquals("CODE3: UnitOfQuantityFieldType shoule be Text if CSI_Code doesn't have UQMapType attribute", nameof(FieldType.Text), supportingDocument.UnitOfQuantityFieldType);
				Assert("CODE3: ShowDropEditForUnitOfQuantity should be false if CSI_Code doesn't have UQMapType attribute", !supportingDocument.ShowDropEditForUnitOfQuantity);
				Assert("CODE3: ShowTextBoxForUnitOfQuantiy should be true if CSI_Code doesn't have UQMapType attribute", supportingDocument.ShowTextBoxForUnitOfQuantiy);
			});
		}

		public void TestShowCalcEditItemNumber()
		{
			CombineAssertions("When declaration is UCC6 and is Import ShowCalcEditItemNumber should return true otherwise false.", () =>
			{
				AssertShowCalcEditItemNumber(isUCC66: true, messageType: EUJobMessageTypeList.Codes.Import, expectedShowCalcEditItemNumber: true);
				AssertShowCalcEditItemNumber(isUCC66: true, messageType: EUJobMessageTypeList.Codes.Export, expectedShowCalcEditItemNumber: false);
				AssertShowCalcEditItemNumber(isUCC66: false, messageType: EUJobMessageTypeList.Codes.Import, expectedShowCalcEditItemNumber: false);
				AssertShowCalcEditItemNumber(isUCC66: false, messageType: EUJobMessageTypeList.Codes.Export, expectedShowCalcEditItemNumber: false);
			});

			void AssertShowCalcEditItemNumber(bool isUCC66, string messageType, bool expectedShowCalcEditItemNumber)
			{
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC66))
				{
					declaration.JE_MessageType = messageType;
					var supportingDocument = declaration.SupportingDocuments.AddNew();
					AssertEquals(expectedShowCalcEditItemNumber, supportingDocument.ShowCalcEditItemNumber);
				}
			}
		}

		public void TestCSI_UnitOfQuantityForMessageSending()
		{
			SetUpRefDataForSupportDocumentUnitOfQuantityTest(Factory);
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = "CODE1";
				supportingDocument.CSI_UnitOfQuantity = "KGM";
				AssertEquals("Send mapped unit value if CSI_UnitOfQuantity is in the list.", "kilo", supportingDocument.CSI_UnitOfQuantityForMessageSending);

				supportingDocument.CSI_Code = "CODE1";
				supportingDocument.CSI_UnitOfQuantity = "LTR";
				AssertEquals("Send not mapped code if CSI_UnitOfQuantity is not in the list.", "LTR", supportingDocument.CSI_UnitOfQuantityForMessageSending);

				supportingDocument.CSI_Code = "CODE2";
				supportingDocument.CSI_UnitOfQuantity = "LTR";
				AssertEquals("Send mapped unit value if CSI_UnitOfQuantity is in the list.", "litre", supportingDocument.CSI_UnitOfQuantityForMessageSending);

				supportingDocument.CSI_Code = "CODE3";
				supportingDocument.CSI_UnitOfQuantity = "TNE";
				AssertEquals("Send not mapped code if the code has no UQMapType attribute.", "TNE", supportingDocument.CSI_UnitOfQuantityForMessageSending);
			});
		}

		internal static void SetUpRefDataForSupportDocumentUnitOfQuantityTest(BusinessObjectFactory factory)
		{
			var config = new RefDataConfig(
				dataGroupings:
				[
						new(code: "FR", description: "France", parent: "")
				],
				cusCodeTypes:
				[
					new(
						typeCode: "DC44E", description: "Supporting Doc. Type", attributeTypes: [new(name: "UQMapType", dataGrouping: "FR")],
						cusCodes:
						[
							new(code: "CODE1", dataGrouping: "FR", attributes: [new(name: "UQMapType", value: "SGLUQ")]),
							new(code: "CODE2", dataGrouping: "FR", attributes: [new(name: "UQMapType", value: "CALUQ")]),
							new (code: "CODE3", dataGrouping: "FR", attributes: [new(name: "", value: "")]),
							new(code: "CODE4", dataGrouping: "FR", attributes: [new(name: "UQMapType", value: "OTH")]),
						]
					),
					new(
						typeCode: "CUSUQ", description: "Customs Unit Of Quantity", attributeTypes: [new(name: "", dataGrouping: "")],
						cusCodes:
						[
							new(code: "KGM", dataGrouping: "FR", attributes: [new(name: "", value: "")]),
							new(code: "TNE", dataGrouping: "FR", attributes: [new(name: "", value: "")]),
							new(code: "LTR", dataGrouping: "FR", attributes: [new(name: "", value: "")]),
						]
					)
				],
				mapTypes:
				[
					new(type: "SGLUQ"),
					new(type: "CALUQ")
				],
				maps:
				[
					new(type: "SGLUQ", cw1Value: "TNE", customsValue: "tonne", dataGrouping: "FR"),
					new(type: "CALUQ", cw1Value: "LTR", customsValue: "litre", dataGrouping: "FR"),
					new(type: "SGLUQ", cw1Value: "KGM", customsValue: "kilo", dataGrouping: "FR"),
					new(type: "SGLUQ", cw1Value: "MTK", customsValue: "mètre carré", dataGrouping: "FR"),
				]
			);
			EUUniversalTestDataHelper.SetUpTestRefData(factory, config);
		}

		public void TestCSI_AdditionalDescriptionCaption()
		{
			AssertEquals("Product Reference No", DataBoundResourceStrings.GetDataForProperty(Factory.New<SupportingDocument>().CSI_AdditionalDescriptionInfo).Caption);
			AssertEquals("Product No", DataBoundResourceStrings.GetDataForProperty(Factory.New<SupportingDocument>().CSI_AdditionalDescriptionInfo).ShortCaption);
		}

		public void TestCSI_LineNoCaption()
		{
			AssertEquals("License Line No", DataBoundResourceStrings.GetDataForProperty(Factory.New<SupportingDocument>().CSI_LineNoInfo).Caption);
			AssertEquals("Line No", DataBoundResourceStrings.GetDataForProperty(Factory.New<SupportingDocument>().CSI_LineNoInfo).ShortCaption);
		}

		public void TestCSI_ReferenceNumber2()
		{
			AssertEquals("Issuing Authority", DataBoundResourceStrings.GetDataForProperty(Factory.New<SupportingDocument>().CSI_ReferenceNumber2Info).Caption);
		}

		public void TestShowTextBoxForReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocuments = declaration.SupportingDocuments.AddNew();
			AssertEquals(true, supportingDocuments.ShowTextBoxForReferenceNumber);
		}

		public void TestIsLineCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocuments = declaration.SupportingDocuments.AddNew();
			AssertEquals(true, supportingDocuments.IsLine);
		}

		public void TestIsLineOnlyCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "DG";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var miscSupportingDocument = declaration.SupportingDocuments.AddNew();
			miscSupportingDocument.CSI_Code = "MDOC";
			miscSupportingDocument.CSI_ReferenceNumber = "DECLARATION_DOCUMENT";
			miscSupportingDocument.CSI_DateOfIssue = ZDate.Today;

			var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();
			invoiceSupportingDocument.CSI_Code = "HDOC";
			invoiceSupportingDocument.CSI_ReferenceNumber = "INVOICEHEADER_DOCUMENT";
			invoiceSupportingDocument.CSI_DateOfIssue = ZDate.Today;

			var invoiceLineSupportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			invoiceLineSupportingDocument1.CSI_Code = "LDOC";
			invoiceLineSupportingDocument1.CSI_ReferenceNumber = "INVOICELINE_DOCUMENT";
			invoiceLineSupportingDocument1.CSI_DateOfIssue = ZDate.Today;

			invoiceLine.JI_SupplementaryCode1 = "CAC1";
			invoiceLine.JI_SupplementaryCode2 = "CAC2";
			Factory.Save();

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			AssertEquals(true, miscSupportingDocument.IsLineOnly);
			AssertEquals(true, invoiceSupportingDocument.IsLineOnly);
			AssertEquals(true, invoiceLineSupportingDocument1.IsLineOnly);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "MDOC" }, entryHeader.SupportingDocuments.Select(x => x.CSI_Code));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "HDOC", "LDOC" }, entryLine.SupportingDocuments.Select(x => x.CSI_Code));

			declaration.JE_ApplicationCode = "DI";
			Factory.Save();
			merger.DoMerge();

			entryHeader = declaration.CustomsEntryHeaders[0];
			entryLine = entryHeader.MergedLines[0];
			AssertEquals(false, miscSupportingDocument.IsLineOnly);
			AssertEquals(false, invoiceSupportingDocument.IsLineOnly);
			AssertEquals(false, invoiceLineSupportingDocument1.IsLineOnly);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "MDOC", "HDOC" }, entryHeader.SupportingDocuments.Select(x => x.CSI_Code));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "LDOC" }, entryLine.SupportingDocuments.Select(x => x.CSI_Code));
		}

		void PrepareIsDTPData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsDTP, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsDTP, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2019, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsDTP, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();
		}

		[TestDate(2020, 6, 5)]
		public void TestIsDTP()
		{
			PrepareIsDTPData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Style = "11";
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			declaration.SupportingDocuments.AddNew();
			var jobComInvoiceHeader = declaration.Invoices.AddNew();
			jobComInvoiceHeader.SupportingDocuments.AddNew();
			var jobComInvoiceLine = declaration.InvoiceLines.AddNew();
			jobComInvoiceLine.JI_CEI = entryInstruction.PK;
			var lineSupportingDocument = jobComInvoiceLine.SupportingDocuments.AddNew();

			lineSupportingDocument.CSI_Code = "0001";
			AssertEquals("IMP: JE_DateOfArrival and CEI_DateForDuty are both empty,so the current time is used for the date", true, lineSupportingDocument.IsDTP);
			lineSupportingDocument.CSI_Code = "2044";
			AssertEquals("IMP: Not D48 but Designation", false, lineSupportingDocument.IsDTP);
			lineSupportingDocument.CSI_Code = "0003";
			AssertEquals("IMP: No related REF data and not DTP", false, lineSupportingDocument.IsDTP);

			declaration.JE_DateOfArrival = new ZDateTime(2070, 6, 6);
			lineSupportingDocument.CSI_Code = "0001";
			AssertEquals("IMP: CEI_DateForDuty is empty, JE_DateOfArrival is now used for the date and is not in the valid time range of 0001", false, lineSupportingDocument.IsDTP);
			declaration.JE_DateOfArrival = new ZDateTime(2023, 6, 6);
			lineSupportingDocument.CSI_Code = "0001";
			AssertEquals("IMP: JE_DateOfArrival is now used for the date and is in the valid time range of 0001", true, lineSupportingDocument.IsDTP);

			entryInstruction.CEI_DateForDuty = new ZDateTime(2070, 1, 21);
			lineSupportingDocument.CSI_Code = "0001";
			AssertEquals("IMP: CEI_DateForDuty is now used for the date and is not in the valid time range of 0001", false, lineSupportingDocument.IsDTP);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 1, 21);
			lineSupportingDocument.CSI_Code = "0001";
			AssertEquals("IMP: CEI_DateForDuty is now used for the date and is in the valid time range of 0001", true, lineSupportingDocument.IsDTP);

			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			lineSupportingDocument.CSI_Code = "0001";
			AssertEquals("EXP: JE_ExportDate and CEI_DateForDuty are both empty,so the current time is used for the date", true, lineSupportingDocument.IsDTP);
			lineSupportingDocument.CSI_Code = "2044";
			AssertEquals("EXP: Not DTP due to No related REF data for EXP", false, lineSupportingDocument.IsDTP);
			lineSupportingDocument.CSI_Code = "0003";
			AssertEquals("EXP: Not DTP due to not in valid date range", false, lineSupportingDocument.IsDTP);
			lineSupportingDocument.CSI_Code = "2045";
			AssertEquals("EXP: Is Not DTP but Designation", false, lineSupportingDocument.IsDTP);

			declaration.JE_ExportDate = new ZDateTime(2070, 6, 6);
			lineSupportingDocument.CSI_Code = "0001";
			AssertEquals("EXP: CEI_DateForDuty is empty, JE_ExportDate is now used for the date and is not in the valid time range of 0001", false, lineSupportingDocument.IsDTP);
			declaration.JE_ExportDate = new ZDateTime(2023, 6, 6);
			lineSupportingDocument.CSI_Code = "0001";
			AssertEquals("EXP: JE_ExportDate is now used for the date and is in the valid time range of 0001", true, lineSupportingDocument.IsDTP);

			entryInstruction.CEI_DateForDuty = new ZDateTime(2070, 1, 21);
			lineSupportingDocument.CSI_Code = "0001";
			AssertEquals("EXP: CEI_DateForDuty is now used for the date and is not in the valid time range of 0001", false, lineSupportingDocument.IsDTP);
			entryInstruction.CEI_DateForDuty = new ZDateTime(2023, 1, 21);
			lineSupportingDocument.CSI_Code = "0001";
			AssertEquals("EXP: CEI_DateForDuty is now used for the date and is in the valid time range of 0001", true, lineSupportingDocument.IsDTP);
		}

		public void TestIsDTPIfParentIsCusClassPartPivot()
		{
			PrepareIsDTPData();

			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			var supportingDocument = pivot.SupportingDocuments.AddNew();
			pivot.CI_ChildType = ClassificationType.IMP;
			supportingDocument.CSI_Code = "0001";
			Assert("IsDTP should be true when matching IMP document has IsDTP attribute set to Y and date range within boundaries.", supportingDocument.CSI_IsDTP);

			supportingDocument.CSI_Code = "2044";
			Assert("IsDTP should be false when matching IMP document has no IsDTP attribute.", !supportingDocument.CSI_IsDTP);

			pivot.CI_ChildType = ClassificationType.EXP;
			supportingDocument.CSI_Code = "0001";
			Assert("IsDTP should be true when matching EXP document has IsDTP attribute set to Y and date range within boundaries.", supportingDocument.CSI_IsDTP);

			supportingDocument.CSI_Code = "0003";
			Assert("IsDTP should be false when no EXP document with date range within boundaries can be found.", !supportingDocument.CSI_IsDTP);

			supportingDocument.CSI_Code = "2045";
			Assert("IsDTP should be false when matching EXP document has no IsDTP attribute", !supportingDocument.CSI_IsDTP);
		}

		public void TestIsD48()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0001", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "0003", "attestation  produite par l'ONU ou une de ses institutions spécialisées", new ZDateTime(1900, 1, 1), new ZDateTime(2019, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "2045", "Demande d'autorisation d'exportation de radionucléides (DAE) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			var jobComInvoiceHeader = declaration.Invoices.AddNew();
			var headerSupportingDocument = jobComInvoiceHeader.SupportingDocuments.AddNew();
			var jobComInvoiceLine = declaration.InvoiceLines.AddNew();
			var lineSupportingDocument = jobComInvoiceLine.SupportingDocuments.AddNew();

			AssertIsD48(supportingDocument, true);
			AssertIsD48(headerSupportingDocument, true);
			AssertIsD48(lineSupportingDocument, true);

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertIsD48(supportingDocument, false);
			AssertIsD48(headerSupportingDocument, false);
			AssertIsD48(lineSupportingDocument, false);
		}

		void AssertIsD48(SupportingDocument sd, bool isImport)
		{
			if (isImport)
			{
				sd.CSI_Code = "0001";
				AssertEquals("IMP: Is D48", true, sd.IsD48);

				sd.CSI_Code = "2044";
				AssertEquals("IMP: Not D48 but Designation", false, sd.IsD48);

				sd.CSI_Code = "0003";
				AssertEquals("IMP: No related REF data and not D48", false, sd.IsD48);
			}
			else
			{
				sd.CSI_Code = "0001";
				AssertEquals("EXP: Is D48", true, sd.IsD48);

				sd.CSI_Code = "2044";
				AssertEquals("EXP: Not D48 due to No related REF data for EXP", false, sd.IsD48);

				sd.CSI_Code = "0003";
				AssertEquals("EXP: Not D48 due to not in valid date range", false, sd.IsD48);

				sd.CSI_Code = "2045";
				AssertEquals("EXP: Is Not D48 but Designation", false, sd.IsD48);
			}
		}

		public void TestIsD48AndNotClosed()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "0001", "statut juridique", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsD48, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "2044", "Demande d'autorisation d'importation de radionucléides (DAI) visée par l'IRSN", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Designation, YesNoList.Codes.Yes);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "0001";
			supportingDocument.CSI_Value = 400.00m;
			supportingDocument.CSI_Quantity3 = 2.00m;
			CombineAssertions(() =>
			{
				Assert("should be true", supportingDocument.IsD48AndNotClosed);

				supportingDocument.CSI_Code = "0002";
				Assert("should be false because not D48", !supportingDocument.IsD48AndNotClosed);

				supportingDocument.CSI_Code = "0001";
				supportingDocument.CSI_Value = 0m;
				Assert("should be false because CSI_Value <= 0", !supportingDocument.IsD48AndNotClosed);

				supportingDocument.CSI_Value = 400.00m;
				supportingDocument.CSI_Quantity3 = 0m;
				Assert("should be false because CSI_Quantity3 <= 0", !supportingDocument.IsD48AndNotClosed);
			});
		}

		public void TestDecimalPlaces()
		{
			const int expectedDecimalPlaces = 4;
			var supportingDocument = Factory.New<SupportingDocument>();
			AssertEquals(expectedDecimalPlaces, supportingDocument.QuantityDecimalPlaces);
			AssertEquals(expectedDecimalPlaces, supportingDocument.Quantity2DecimalPlaces);
			AssertHasCustomAttribute<DecimalPlacesAttribute>(typeof(SupportingDocument), nameof(SupportingDocument.CSI_Quantity2), false, x => x.DecimalPlacesMember == "Quantity2DecimalPlaces");
		}

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

		void PrepareIsODSData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "Supporting Document of Import Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "L100", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, "E013", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "Supporting Document of Export Direction");
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "L100", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			helper.CreateCusCodeListWithAttribute("FR", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "E013", "The document has ODS requirements", new ZDateTime(1900, 1, 1), new ZDateTime(2059, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.IsODS, YesNoList.Codes.Yes);
			Factory.Save();
		}

		public void TestIsODS()
		{
			PrepareIsODSData();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			var decSupportingDocument = declaration.SupportingDocuments.AddNew();
			var jobComInvoiceHeader = declaration.Invoices.AddNew();
			var invHeaderSupportingDocument = jobComInvoiceHeader.SupportingDocuments.AddNew();
			var jobComInvoiceLine = declaration.InvoiceLines.AddNew();
			var lineSupportingDocument = jobComInvoiceLine.SupportingDocuments.AddNew();

			decSupportingDocument.CSI_Code = "L100";
			AssertEquals("IMP: Supporting document L100 in declaration", true, decSupportingDocument.IsODS);
			decSupportingDocument.CSI_Code = "E013";
			AssertEquals("IMP: Supporting document E013 in declaration", true, decSupportingDocument.IsODS);
			decSupportingDocument.CSI_Code = "0003";
			AssertEquals("IMP: Supporting document 0003 in declaration", false, decSupportingDocument.IsODS);

			invHeaderSupportingDocument.CSI_Code = "L100";
			AssertEquals("IMP: Supporting document L100 in invoice header", true, invHeaderSupportingDocument.IsODS);
			invHeaderSupportingDocument.CSI_Code = "E013";
			AssertEquals("IMP: Supporting document E013 in invoice header", true, invHeaderSupportingDocument.IsODS);
			invHeaderSupportingDocument.CSI_Code = "0003";
			AssertEquals("IMP: Supporting document 0003 in invoice header", false, invHeaderSupportingDocument.IsODS);

			lineSupportingDocument.CSI_Code = "L100";
			AssertEquals("IMP: Supporting document L100 in invoice line", true, lineSupportingDocument.IsODS);
			lineSupportingDocument.CSI_Code = "E013";
			AssertEquals("IMP: Supporting document E013 in invoice line", true, lineSupportingDocument.IsODS);
			lineSupportingDocument.CSI_Code = "0003";
			AssertEquals("IMP: Supporting document 0003 in invoice line", false, lineSupportingDocument.IsODS);

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			decSupportingDocument.CSI_Code = "L100";
			AssertEquals("IMP: Supporting document L100 in declaration", true, decSupportingDocument.IsODS);
			decSupportingDocument.CSI_Code = "E013";
			AssertEquals("IMP: Supporting document E013 in declaration", true, decSupportingDocument.IsODS);
			decSupportingDocument.CSI_Code = "0003";
			AssertEquals("IMP: Supporting document 0003 in declaration", false, decSupportingDocument.IsODS);

			invHeaderSupportingDocument.CSI_Code = "L100";
			AssertEquals("IMP: Supporting document L100 in invoice header", true, invHeaderSupportingDocument.IsODS);
			invHeaderSupportingDocument.CSI_Code = "E013";
			AssertEquals("IMP: Supporting document E013 in invoice header", true, invHeaderSupportingDocument.IsODS);
			invHeaderSupportingDocument.CSI_Code = "0003";
			AssertEquals("IMP: Supporting document 0003 in invoice header", false, invHeaderSupportingDocument.IsODS);

			lineSupportingDocument.CSI_Code = "L100";
			AssertEquals("IMP: Supporting document L100 in invoice line", true, lineSupportingDocument.IsODS);
			lineSupportingDocument.CSI_Code = "E013";
			AssertEquals("IMP: Supporting document E013 in invoice line", true, lineSupportingDocument.IsODS);
			lineSupportingDocument.CSI_Code = "0003";
			AssertEquals("IMP: Supporting document 0003 in invoice line", false, lineSupportingDocument.IsODS);
		}
	}
}
