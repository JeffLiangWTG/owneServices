using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Biz = Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.DocumentWrappers.Testing
{
	[TestedType(typeof(DocSADHLine))]
	class DocSADHLineTest : Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHLineTest
	{
		public new void TestBox40Contents()
		{
			Assert(true);
		}

		public void TestBox44Contents()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var gbGroup = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.UnitedKingdom);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var addInfCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { importCodeType, exportCodeType }, "HDR1", "COZ WE WANT TO", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { importCodeType, exportCodeType }, "HNY1", "HONEY TO THE BEE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { addInfCodeType }, "HIT99", "RANDOM SONG TITLE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { importCodeType, exportCodeType }, "9100", "I HAVE NO CLUE", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { importCodeType, exportCodeType }, "9120", "9120 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			var suppDocHeader = invoiceHeader.SupportingDocuments.AddNew();
			suppDocHeader.CSI_Code = "HDR1";
			suppDocHeader.CSI_Availability = "A";
			suppDocHeader.CSI_Actions = "A";
			suppDocHeader.CSI_ReferenceNumber = "BILLY";
			suppDocHeader.CSI_SubType = "X";
			suppDocHeader.CSI_Quantity = 99;
			suppDocHeader.CSI_Description = "COZ WE WANT TO";

			var addInfoHeader = invoiceHeader.AdditionalInfos.AddNew();
			addInfoHeader.CSI_Code = "HIT99";
			addInfoHeader.CSI_Description = "RANDOM SONG TITLE";

			var suppDocGroup = declaration.SupportingDocuments.AddNew();
			suppDocGroup.CSI_Code = "HNY1";
			suppDocGroup.CSI_Availability = "B";
			suppDocGroup.CSI_Actions = "H";
			suppDocGroup.CSI_ReferenceNumber = "PIPER";
			suppDocGroup.CSI_SubType = "2";
			suppDocGroup.CSI_Quantity = 22;
			suppDocGroup.CSI_Description = "HONEY TO THE BEE";

			var addInfoGroup = declaration.AdditionalInfos.AddNew();
			addInfoGroup.CSI_Code = "BOB21";
			addInfoGroup.CSI_Description = "MADE SENSE AT THE TIME";

			var addInfo1 = invoiceLine.AdditionalInfos.AddNew();
			addInfo1.CSI_Code = "GEN13";
			addInfo1.CSI_Description = "PLENTY OF COATS";

			var addInfo2 = invoiceLine.AdditionalInfos.AddNew();
			addInfo2.CSI_Code = "PAL01";

			var document1 = invoiceLine.SupportingDocuments.AddNew();
			document1.CSI_Code = "9100";
			document1.CSI_Availability = "E";
			document1.CSI_Actions = "P";
			document1.CSI_ReferenceNumber = "3278923";
			document1.CSI_SubType = "1";
			document1.CSI_Quantity = 12;
			document1.CSI_Description = "I HAVE NO CLUE";

			var document2 = invoiceLine.SupportingDocuments.AddNew();
			document2.CSI_Code = "9120";
			document2.CSI_Availability = "S";
			document2.CSI_Actions = "F";
			document2.CSI_ReferenceNumber = "45982309";
			document2.CSI_Description = "9120 Test";

			var line = DocSADHLine.New(entryLine, Factory);
			string expected = @"BOB21-MADE SENSE AT THE TIME; HIT99-RANDOM SONG TITLE; GEN13-PLENTY OF COATS; PAL01; HNY1-[BH] PIPER P=2 Q=22 ""HONEY TO THE BEE""; HDR1-[AA] BILLY P=X Q=99 ""COZ WE WANT TO""; 9100-[EP] 3278923 P=1 Q=12 ""I HAVE NO CLUE""; 9120-[SF] 45982309 ""9120 Test""";
			AssertEquals("entryLine.Box44Contents", expected, line.Box44AddInfoAndDocuments);
		}

		public override void TestBox44_1ProducedDocumentsCertificates()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var gbGroup = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.UnitedKingdom);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { importCodeType, exportCodeType }, "2154", "2154 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { importCodeType, exportCodeType }, "1920", "1920 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { importCodeType, exportCodeType }, "1968", "1968 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			var documentHead1 = declaration.SupportingDocuments.AddNew();
			documentHead1.CSI_Code = "2154";
			documentHead1.CSI_Availability = "A";
			documentHead1.CSI_Actions = "G";
			documentHead1.CSI_ReferenceNumber = "YOU FAT BASTARD";

			var documentLine1 = invoiceLine.SupportingDocuments.AddNew();
			documentLine1.CSI_Code = "1920";
			documentLine1.CSI_Availability = "S";
			documentLine1.CSI_Actions = "F";
			documentLine1.CSI_ReferenceNumber = "ALOTTA FAJINA";

			var documentLine2 = invoiceLine.SupportingDocuments.AddNew();
			documentLine2.CSI_Code = "1968";
			documentLine2.CSI_Availability = "C";
			documentLine2.CSI_Actions = "B";
			documentLine2.CSI_ReferenceNumber = "FANNY MCMUFFIN";

			var lineWrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals(nameof(DocSADHLine.Box44_1ProducedDocumentsCertificates), "2154-[AG] YOU FAT BASTARD, 1920-[SF] ALOTTA FAJINA, 1968-[CB] FANNY MCMUFFIN", lineWrapper.Box44_1ProducedDocumentsCertificates);

			entryLine.CL_LineNumber = 2;
			lineWrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals(nameof(DocSADHLine.Box44_1ProducedDocumentsCertificates), "1920-[SF] ALOTTA FAJINA, 1968-[CB] FANNY MCMUFFIN", lineWrapper.Box44_1ProducedDocumentsCertificates);
		}

		public void TestBoxes2And8ForEADPassThrough()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				var declaration = Factory.New<EU.Business.Declaration.JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entryHeader.MergedLines.AddNew();
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var consignee = Factory.New<OrgHeader>();
				consignee.OH_FullName = "Consignee";
				consignee.MainAddress.Address1 = "Consignee address1";
				consignee.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "CNSEE123");
				var consignor = Factory.New<OrgHeader>();
				consignor.OH_FullName = "Consignor";
				consignor.MainAddress.Address1 = "Consignor address1";
				consignor.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "CNSOR123");

				invoiceLine.JI_OA_ConsigneeAddress = consignee.MainAddress.PK;
				invoiceLine.JI_OA_ExporterAddress = consignor.MainAddress.PK;

				var line = DocSADHLine.New(entryLine, Factory);
				AssertEquals(@"Consignee
Consignee address1
IECNSEE123", line.Box8ConsigneeForEAD);
				AssertEquals(@"Consignor
Consignor address1
IECNSOR123", line.Box2ConsignorForEAD);
			}
		}

		public void TestParentAddInfosAndDUCRAndMUCRGetAddedToLine1()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var gbGroup = helper.CreateNewOrGetExistingDataGrouping(CountryCodes.UnitedKingdom);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "HEADER" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { importCodeType, exportCodeType }, "2154", "2154 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { importCodeType, exportCodeType }, "1920", "1920 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			helper.CreateCusCodeListsForMultipleTypesWithAttributes(CountryCodes.UnitedKingdom, new string[] { importCodeType, exportCodeType }, "1968", "1968 Test", attributeNameValuePairs, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var addInfoHead1 = declaration.AdditionalInfos.AddNew();
			addInfoHead1.CSI_Code = "CATNP";
			addInfoHead1.CSI_Description = "EAGER BEAVER";

			var documentHead1 = declaration.SupportingDocuments.AddNew();
			documentHead1.CSI_Code = "2154";
			documentHead1.CSI_Availability = "A";
			documentHead1.CSI_Actions = "G";
			documentHead1.CSI_ReferenceNumber = "YOU FAT BASTARD";

			var addInfoLine1 = invoiceLine1.AdditionalInfos.AddNew();
			addInfoLine1.CSI_Code = "DOGBA";
			addInfoLine1.CSI_Description = "SMALL TURBAN";

			var documentLine1 = invoiceLine1.SupportingDocuments.AddNew();
			documentLine1.CSI_Code = "1920";
			documentLine1.CSI_Availability = "S";
			documentLine1.CSI_Actions = "F";
			documentLine1.CSI_ReferenceNumber = "ALOTTA FAJINA";

			var addInfoLine2 = invoiceLine2.AdditionalInfos.AddNew();
			addInfoLine2.CSI_Code = "LINE2";
			addInfoLine2.CSI_Description = "A PARAQUETE";

			var documentLine2 = invoiceLine2.SupportingDocuments.AddNew();
			documentLine2.CSI_Code = "1968";
			documentLine2.CSI_Availability = "C";
			documentLine2.CSI_Actions = "B";
			documentLine2.CSI_ReferenceNumber = "FANNY MCMUFFIN";

			declaration.JE_MasterUCR = "A:08145678901";
			entryHeader.CH_BGMReference = "GB332278957000-B00001258/121";

			var line = DocSADHLine.New(entryLine1, Factory);
			string expected = @"CATNP-EAGER BEAVER; DOGBA-SMALL TURBAN; 9DCR-GB332278957000-B00001258 Part:121; 9MCR-A:08145678901; 2154-[AG] YOU FAT BASTARD; 1920-[SF] ALOTTA FAJINA";
			AssertEquals("entryLine.Box44Contents", expected, line.Box44AddInfoAndDocuments);

			line = DocSADHLine.New(entryLine2, Factory);
			expected = @"LINE2-A PARAQUETE; 1968-[CB] FANNY MCMUFFIN";
			AssertEquals("entryLine.Box44Contents", expected, line.Box44AddInfoAndDocuments);  //does not contain header-only AI statements

			var org = Factory.New<OrgHeader>();
			org.MainAddress.OA_Address1 = "Add1";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_PostCode = "PCODE";
			org.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			invoiceLine2.JI_OA_SupervisingOffice = org.MainAddress.PK;

			line = DocSADHLine.New(entryLine2, Factory);
			expected = @"LINE2-A PARAQUETE; SPOFF - Add1, City, PCODE, United Kingdom; 1968-[CB] FANNY MCMUFFIN";
			AssertEquals("entryLine.Box44Contents", expected, line.Box44AddInfoAndDocuments);
		}

		public void TestBoxes2And8ForEADPassThrough_GB()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var sup = Factory.New<OrgHeader>();
			sup.OH_FullName = "supplier";
			sup.MainAddress.Address1 = "supplier address1";
			sup.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "supplier123");
			var imp = Factory.New<OrgHeader>();
			imp.OH_FullName = "importer";
			imp.MainAddress.Address1 = "importer address1";
			imp.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "importer123");

			declaration.JE_OH_Importer = imp.PK;
			declaration.JE_OH_Supplier = sup.PK;

			var line = DocSADHLine.New(entryLine, Factory);
			AssertEquals(@"supplier
supplier address1
GBSUPPLIER123", line.Box2ConsignorForEAD);
			AssertEquals(@"importer
importer address1
GBIMPORTER123", line.Box8ConsigneeForEAD);
		}

		public override void TestBox48DeferredPayment()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Biz.DeclarationApplicationCodeList.Codes.Builtin;

			var declarant = declaration.Declarant.Header;
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new Biz.SendsMessagesToCustomsShutterUpperer()));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			declaration.JE_PaymentMethod = "A";
			declaration.JE_DefermentAccountNumber = "1";
			var line = DocSADHLine.New(entryLine, Factory);
			AssertEquals("A 1", line.Box48DeferredPayment);

			declaration.JE_PaymentMethod = ZString.Empty;
			declaration.JE_DefermentAccountNumber = ZString.Empty;

			declaration.ZG_VATDeferType = "B";
			declaration.ZG_VATDeferNumber = "2";
			line = DocSADHLine.New(entryLine, Factory);
			AssertEquals("B 2", line.Box48DeferredPayment);

			declaration.JE_PaymentMethod = "A";
			declaration.JE_DefermentAccountNumber = "1";

			line = DocSADHLine.New(entryLine, Factory);
			AssertEquals("A 1; B 2", line.Box48DeferredPayment);
		}

		protected override void AssertCountrySpecificFields(Enterprise.DocumentWrappers.Customs.EU.DocSADHLine line)
		{
			AssertEquals("45 Adjustment", "[A] 12.4%", line.Box45Adjustment);
		}

		public void TestBox49Warehouse_GB_CHIEF()
		{
			var warehouse = Factory.New<OrgHeader>();
			var warehouseId = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "A12345678GB", RefCountry.LoadFromCountryCode(Factory, CountryCodes.UnitedKingdom));
			warehouseId.OK_OA_PremisesAddress = warehouse.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			declaration.DoMerge(new Biz.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			var docLine = DocSADHLine.New(entryLine, Factory);
			AssertEquals("A12345678GB", docLine.Box49Warehouse);
		}

		public void TestBox49Warehouse_GB_CDS_Inward()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "IM", "10", "71", "F61", "", MessageTypeList.Codes.Import, "10P");
			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			Factory.Save();

			var warehouse = Factory.New<OrgHeader>();
			var warehouseId = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "A12345678GB", RefCountry.LoadFromCountryCode(Factory, CountryCodes.UnitedKingdom));
			warehouseId.OK_OA_PremisesAddress = warehouse.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "1071F61";

			declaration.DoMerge(new Biz.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			var docLine = DocSADHLine.New(entryLine, Factory);
			AssertEquals("A12345678GB", docLine.Box49Warehouse);
		}

		public void TestBox49Warehouse_GB_CDS_Outward()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, "IM", "42", "71", "C33", "", MessageTypeList.Codes.Import, "42P");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			Factory.Save();

			var warehouse = Factory.New<OrgHeader>();
			var warehouseId = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "A12345678GB", RefCountry.LoadFromCountryCode(Factory, CountryCodes.UnitedKingdom));
			warehouseId.OK_OA_PremisesAddress = warehouse.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "4271C33";

			declaration.DoMerge(new Biz.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			var docLine = DocSADHLine.New(entryLine, Factory);
			AssertEquals("A12345678GB", docLine.Box49Warehouse);
		}

		protected override bool GetExportCountryFromInvoiceLine => true;
	}
}
