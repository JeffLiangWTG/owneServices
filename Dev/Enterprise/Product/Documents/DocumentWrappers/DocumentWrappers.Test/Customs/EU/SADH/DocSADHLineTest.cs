using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;
using Enterprise.MasterFiles.Integration;
using Biz = Enterprise.Customs.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	public abstract class DocSADHLineTest : DocumentWrappers.Testing.DocBaseWrapperTest
	{
		public void TestBox1bSubStyle()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = "C";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var docLine = GetNewDocSADHLine(entryLine);
			AssertEquals("C", docLine.Box1bSubStyle);
		}

		public void TestBox15aExportCountry()
		{
			var declaration = CreateDeclarationForBox15aExportCountryTest();
			declaration.JE_MessageType = "EXP";
			declaration.JE_GoodsOrigin = "IE";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_RN_NKCountryOfExport = "FR";
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var line1 = GetNewDocSADHLine(entryLine1);
			var line2 = GetNewDocSADHLine(entryLine2);
			AssertEquals("Line 1 take value from declaration", "IE", line1.Box15aExportCountry);
			AssertEquals("Line 2 take value from invoice line", GetExportCountryFromInvoiceLine ? "FR" : "IE", line2.Box15aExportCountry);
		}

		protected virtual JobDeclaration CreateDeclarationForBox15aExportCountryTest() => Factory.New<JobDeclaration>();

		protected virtual bool GetExportCountryFromInvoiceLine => false;

		public void TestBox17aDestinationCountry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			declaration.JE_GoodsDestination = "HK";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.ZG_CountryOfDestination = "CH";
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine2.JI_CL = entryLine2.PK;

			var line1 = GetNewDocSADHLine(entryLine1);
			var line2 = GetNewDocSADHLine(entryLine2);
			AssertEquals("Line 1 take value from invoice line", "CH", line1.Box17aDestinationCountry);
			AssertEquals("Line 2 take value from declaration", "HK", line2.Box17aDestinationCountry);
		}

		public void TestShowBox41SupplementaryUnits()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var lineWrapper = GetNewDocSADHLine(entryLine);
			AssertEquals(ExpectedShowBox41SupplementaryUnits, lineWrapper.ShowBox41SupplementaryUnits);
		}

		public void TestShowBox46StatisticalValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var lineWrapper = GetNewDocSADHLine(entryLine);
			AssertEquals(true, lineWrapper.ShowBox46StatisticalValue);
		}

		public void TestBox49Warehouse_Inward()
		{
			SetUpForInwardProcedure();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(zzzDataGrouping, "IM", procedureCode, previousProcedureCode, concession, "", shipmentType, group);

			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			Factory.Save();

			var warehouse = Factory.New<OrgHeader>();
			var warehouseCustomsCode = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, customsRegNo, RefCountry.LoadFromCountryCode(Factory, country));
			warehouseCustomsCode.OK_OA_PremisesAddress = warehouse.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = shipmentType;
			declaration.JE_ApplicationCode = zzzDataGrouping;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

			var warehouseAddress = warehouse.MainAddress;
			var cusAuthorisationHeader = Factory.NewWithValidTestData<Biz.CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_RN_NKCountryCode = country;
			cusAuthorisationHeader.CPH_Type = "CWP";
			cusAuthorisationHeader.CPH_OH_PermitHolder = warehouseAddress.Header?.PK ?? ZGuid.Empty;
			cusAuthorisationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;
			cusAuthorisationHeader.CPH_Number = customsRegNo;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = procedureCode + previousProcedureCode + concession;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var sadhLine = GetNewDocSADHLine(entryLine);

			AssertEquals(ExpectedBox49WarehouseInward, sadhLine.Box49Warehouse);
		}
		public void TestBox49Warehouse_Outward()
		{
			SetUpForOutwardProcedure();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(zzzDataGrouping, "IM", procedureCode, previousProcedureCode, concession, "", shipmentType, group);

			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			Factory.Save();

			var warehouse = Factory.New<OrgHeader>();
			var warehouseCustomsCode = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, customsRegNo, RefCountry.LoadFromCountryCode(Factory, country));
			warehouseCustomsCode.OK_OA_PremisesAddress = warehouse.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = shipmentType;
			declaration.JE_ApplicationCode = zzzDataGrouping;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_OA_Warehouse = warehouse.MainAddress.PK;

			var warehouseAddress = warehouse.MainAddress;
			var cusAuthorisationHeader = Factory.NewWithValidTestData<Biz.CusAuthorisationHeader>();
			cusAuthorisationHeader.CPH_RN_NKCountryCode = country;
			cusAuthorisationHeader.CPH_Type = "CWP";
			cusAuthorisationHeader.CPH_OH_PermitHolder = warehouseAddress.Header?.PK ?? ZGuid.Empty;
			cusAuthorisationHeader.CPH_OA_AppliesTo = warehouseAddress.PK;
			cusAuthorisationHeader.CPH_Number = customsRegNo;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = procedureCode + previousProcedureCode + concession;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var sadhLine = GetNewDocSADHLine(entryLine);

			AssertEquals(ExpectedBox49WarehouseOutward, sadhLine.Box49Warehouse);
		}

		protected virtual void SetUpForInwardProcedure()
		{
			zzzDataGrouping = "CDS";
			procedureCode = "10";
			previousProcedureCode = "71";
			concession = "F61";
			country = Core.Constants.CountryCodes.UnitedKingdom;
			customsRegNo = "A12345678GB";
			shipmentType = "IMP";
			group = "10P";
		}

		protected virtual void SetUpForOutwardProcedure()
		{
			zzzDataGrouping = "CDS";
			procedureCode = "42";
			previousProcedureCode = "71";
			concession = "C33";
			country = Core.Constants.CountryCodes.UnitedKingdom;
			customsRegNo = "A12345678GB";
			shipmentType = "IMP";
			group = "42P";
		}

		protected ZString zzzDataGrouping;
		protected ZString procedureCode;
		protected ZString previousProcedureCode;
		protected ZString concession;
		protected ZString country;
		protected ZString customsRegNo;
		protected ZString shipmentType;
		protected ZString group;

		protected virtual ZString ExpectedBox49WarehouseInward => "A12345678GB";
		protected virtual ZString ExpectedBox49WarehouseOutward => "A12345678GB";
		protected virtual ZString ExpectedProcedure => "1000000";
		protected virtual ZString ExpectedAdditionalProcedure => ZString.Empty;
		protected virtual ZBool ExpectedShowBox41SupplementaryUnits => false;
		protected virtual ZString ExpectedSupplementaryUnits => "15.340[ABC]";
		protected virtual ZString ExpectedSupplementaryQuantity => "15.340";
		protected virtual ZString ExpectedSupplementaryUQDescription => "ABC DESC";

		protected virtual void AssertCountrySpecificFields(DocSADHLine line)
		{
		}

		public virtual void TestBox44FiscalReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = "C";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			var docLine = GetNewDocSADHLine(entryLine);
			AssertType<GenericWrappers.OrganisationWrapper>(docLine.Box44FiscalReference);

			AssertEquals(ZString.Empty, docLine.Box44FiscalReferenceNumber);
		}

		public virtual void TestBox44_1ProducedDocumentsCertificates()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			var supportingDocument1 = invoiceLine.SupportingDocuments.AddNew();
			var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
			var supportingDocument3 = declaration.SupportingDocuments.AddNew();

			supportingDocument1.CSI_Code = "N380";
			supportingDocument1.CSI_ReferenceNumber = "A0023";
			supportingDocument1.CSI_RN_NKCountryCode = "IT";
			supportingDocument1.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
			supportingDocument1.CSI_Quantity = 343.43m;

			supportingDocument2.CSI_Code = "C601";
			supportingDocument2.CSI_ReferenceNumber = "A0050";
			supportingDocument2.CSI_RN_NKCountryCode = "DE";
			supportingDocument2.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
			supportingDocument2.CSI_Quantity = 200.43m;
			supportingDocument2.CSI_UnitOfQuantity = "KGM";

			supportingDocument3.CSI_Code = "C647";
			supportingDocument3.CSI_ReferenceNumber = "A0067";
			supportingDocument3.CSI_RN_NKCountryCode = "FR";
			supportingDocument3.CSI_DateOfIssue = new ZDateTime(2000, 1, 4);
			supportingDocument3.CSI_Quantity = 233.43m;

			var lineWrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals(nameof(DocSADHLine.Box44_1ProducedDocumentsCertificates), "C647 A0067 Q=233.43, N380 A0023 Q=343.43, C601 A0050 Q=200.43", lineWrapper.Box44_1ProducedDocumentsCertificates);

			entryLine.CL_LineNumber = 2;
			lineWrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals(nameof(DocSADHLine.Box44_1ProducedDocumentsCertificates), "N380 A0023 Q=343.43, C601 A0050 Q=200.43", lineWrapper.Box44_1ProducedDocumentsCertificates);
		}

		public virtual void TestBox47Taxes()
		{
			var docLine = GetSADHLineForBox47Taxes();
			AssertBox47Taxes(docLine.Box47Taxes);
		}

		protected virtual DocSADHLine GetSADHLineForBox47Taxes()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Biz.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			var invHeader = declaration.Invoices.AddNew();
			var invLine1 = invHeader.InvoiceLines.AddNew();
			var invLine2 = invHeader.InvoiceLines.AddNew();
			invLine1.JI_Tariff = "abc";
			invLine2.JI_Tariff = "abc";

			var tax1Vat = invLine1.Taxes.AddNew();
			var tax1Dty = invLine1.Taxes.AddNew();
			var tax2Vat = invLine2.Taxes.AddNew();
			var tax2Dty = invLine2.Taxes.AddNew();
			tax1Vat.Data.G4_Type = "B00";
			tax1Dty.Data.G4_Type = "A00";
			tax2Vat.Data.G4_Type = "B00";
			tax2Dty.Data.G4_Type = "A00";
			invLine1.JI_LinePrice = 1000m;
			invLine2.JI_LinePrice = 105.89m;
			declaration.DoMerge(new Biz.SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];

			entryHeader.MergedLines[0].CL_CustomsValue = 1105.89m;

			tax1Vat.Data.G4_Amount = "200.34";
			tax1Vat.Data.G4_BaseAmount = 1144.81m;

			tax2Vat.Data.G4_Amount = "21.21";
			tax2Vat.Data.G4_BaseAmount = 121.22m;

			tax1Dty.Data.G4_Amount = "26.99";
			tax1Dty.Data.G4_BaseAmount = 1000m;

			tax2Dty.Data.G4_Amount = "2.86";
			tax2Dty.Data.G4_BaseAmount = 105.89m;

			return GetNewDocSADHLine(entryHeader.MergedLines[0]);
		}

		protected virtual void AssertBox47Taxes(DocSADHLineTaxCollection taxCollection)
		{
			AssertNotNull(taxCollection);
			AssertEquals("TaxCollection count", 2, taxCollection.Count);
			AssertEquals("TaxCollection[1].G4_Type", "B00", taxCollection[1].G4_Type);
			AssertEquals("TaxCollection[1].Box47b", "1266.03", taxCollection[1].Box47b);
			AssertEquals("TaxCollection[1].G4_Amount_InDeclarationCurrency", "221.55", taxCollection[1].G4_Amount_InDeclarationCurrency);

			AssertEquals("TaxCollection[0].G4_Type", "A00", taxCollection[0].G4_Type);
			AssertEquals("TaxCollection[0].Box47b", "1105.89", taxCollection[0].Box47b);
			AssertEquals("TaxCollection[0].G4_Amount_InDeclarationCurrency", "29.85", taxCollection[0].G4_Amount_InDeclarationCurrency);
		}

		public virtual void TestSortBox47Taxes()
		{
			var lineWrapper = GetSADHLineForBox47TaxesOrder();
			var box47TaxTypes = lineWrapper.Box47Taxes.Cast<DocSADHLineTax>().Select(x => x.G4_Type).ToArray();
			AssertArrayEqualsByElements(ExpectedTaxTypesOrder, box47TaxTypes);
		}

		protected virtual DocSADHLine GetSADHLineForBox47TaxesOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			foreach (var taxType in TaxTypesToTestOrder)
			{
				invoiceLine.Taxes.AddNew().G4_Type = taxType;
			}
			return DocSADHLine.New(entryLine, Factory);
		}

		protected virtual ZString[] TaxTypesToTestOrder => new ZString[] { "B10", "D10", "A00" };
		protected virtual ZString[] ExpectedTaxTypesOrder => new ZString[] { "A00", "D10", "B10" };

		public void TestBox31_1NumberOfPackagesPiecesMarksAndNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var declarationBill = declaration.Bills.AddNew();

			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "CT";
			package.CW_MarksAndNos = "IND";

			var packagesPivotCollection = invoiceLine.PackagesPivot;
			var packagePivot = packagesPivotCollection.AddNew();
			packagePivot.CHC_CW = package.PK;
			packagePivot.CHC_NumberOfPacks = 1;
			packagePivot.CHC_JE = invoiceLine.Declaration.PK;

			var wrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals("Box31 formatted value in English", "Marks & Number=IND Number=1 Package type=CT", wrapper.Box31_1NumberOfPackagesPiecesMarksAndNumbers);
		}

		public void TestBox31_2DescriptionOfGoodsHasNoRN()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_Description = @"PRODUITS PHARMACEUTIQUES PRÉPARATIONS ET ARTICLES

PHARMACEUTIQUES VISÉS À LA NOTE 4 DU PRÉSENT CHAPITRE PLACEBOS ET TROUSSES POUR ESSAIS CLINIQUES MASQUÉS (OU À DOUBLE INSU), DESTINÉS À UN ESSAI CLINIQUE RECONNU, PRÉSENTÉS SOUS FORME DE DOSES";

			var wrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals(@"Box31_2DescriptionOfGoods has no '\r\n'", false, wrapper.Box31_2DescriptionOfGoods.Contains("\r\n"));
			AssertEquals(@"Box31_2DescriptionOfGoods has no '\r'", false, wrapper.Box31_2DescriptionOfGoods.Contains("\r"));
			AssertEquals(@"Box31_2DescriptionOfGoods has no '\n'", false, wrapper.Box31_2DescriptionOfGoods.Contains("\n"));
		}

		public void TestBox313ContainerNumbers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER1";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER2";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER3";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = entryLine.InvoiceLines.AddNew();
			var invoiceLine2 = entryLine.InvoiceLines.AddNew();
			var invoiceLine3 = entryLine.InvoiceLines.AddNew();
			var lineWrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals(ZString.Empty, lineWrapper.Box31_3ContainerNumbers);

			invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;
			invoiceLine1.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[0].PK;
			invoiceLine2.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[1].PK;
			invoiceLine3.ContainersPivot.AddNew().C2_CO = declaration.CusContainers[2].PK;

			lineWrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals("CONTAINER1, CONTAINER2, CONTAINER3", lineWrapper.Box31_3ContainerNumbers);
		}

		public void TestBoxes2And8PassThrough()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			OrgHeader sup = Factory.New<OrgHeader>();
			OrgHeader imp = Factory.New<OrgHeader>();

			declaration.JE_OH_Importer = imp.PK;
			declaration.JE_OH_Supplier = sup.PK;

			DocSADHLine line = DocSADHLine.New(entryLine, Factory);
			AssertEquals("entryLine.Box2", sup.OH_FullName, line.Box2Consignor);
			AssertEquals("entryLine.Box8", imp.OH_FullName, line.Box8Consignee);
		}

		protected virtual CusEntryLine SetUpEntryLine(CusEntryHeader entryHeader)
		{
			return entryHeader.MergedLines[0];
		}

		public virtual void TestLineFieldsForTheC88()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(GlbCompany.CurrentCompany.Country.Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreatePreferenceForCountryAndGrouping("100", "Normal Third Country Tariff Duty (Including Ceilings)", GlbCompany.CurrentCompany.Country.Code, "EUN");
			Factory.Save();

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var warehouseID = warehouse.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "A1234567GB", RefCountry.LoadFromCountryCode(Factory, GlbCompany.CurrentCompany.Country.Code));
			warehouseID.OK_OA_PremisesAddress = warehouse.MainAddress.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_PaymentMethod = "A";
			declaration.JE_DefermentAccountNumber = "1234567";
			declaration.ZG_VATDeferType = "B";
			declaration.ZG_VATDeferNumber = "9876543";
			declaration.JE_ApplicationCode = "BLT";

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 1234.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.JI_CountryOfOrigin = "AU";
			invoiceLine.JI_StateOrRegionOfOrigin = "20";
			invoiceLine.JI_Weight = 100.00m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Pounds;
			invoiceLine.JI_PrimaryPreference = "100";
			invoiceLine.JI_Procedure = "1000000";
			invoiceLine.AdditionalProcedureCodes.AddNew("456F89");
			invoiceLine.JI_NetWeight = 50.00m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Ounces;
			invoiceLine.JI_CustomsQuantity = 2.5m;
			invoiceLine.JI_ConcessionOrder = "YAY";
			invoiceLine.JI_CustomsSecondQuantity = 15.34m;
			invoiceLine.JI_CustomsSecondUnitQty = "ABC";
			invoiceLine.JI_SupplementaryCode1 = "EC1";
			invoiceLine.JI_SupplementaryCode2 = "EC2";
			invoiceLine.JI_LinePrice = 1234.00m;
			invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
			invoiceLine.ZG_ValueAdjustmentCode = "A";
			invoiceLine.ZG_StatisticalValueManualOverride = true;
			invoiceLine.ZG_StatisticalValue = 123.45m;
			declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
			invoiceLine.ZG_PrincipalsRepresentativeName = "FREDDY MERCURY";
			invoiceLine.ZG_RL_NKPrincipalsRepresentativeCity = "GBLON";
			invoiceLine.JI_ValuationMarkup = 12.4m;

			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new Biz.SendsMessagesToCustomsShutterUpperer()));
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			CusEntryLine entryLine = SetUpEntryLine(entryHeader);

			var line = GetNewDocSADHLine(entryLine);
			CombineAssertions("Assert properties", () =>
			{
				AssertEquals("32 Item Number", 1, line.Box32ItemNumber);
				AssertEquals("33 Commodity Code", ExpectedCommodityCode, line.Box33CommodityCode);
				AssertEquals("33 EC Supplement", ExpectedECSupplement, line.Box33ECSupplement);
				AssertEquals("33 EC Supplement2", ExpectedECSupplement2, line.Box33ECSupplement2);
				AssertEquals("34a Country of Origin", "AU", line.Box34CountryOfOrigin);
				AssertEquals("34b State of Origin", "20", line.Box34StateOfOrigin);
				AssertEquals("35 Gross Mass in KGs", ExpectedGrossMass, line.Box35GrossWeightInKG);
				AssertEquals("35 Gross Mass in KGs for commercial", ExpectedGrossMassForCommericalPurposesOnly, line.Box35GrossWeightInKGForCommericalPurposesOnly);
				AssertEquals("36 Preference", "100", line.Box36Preference);
				AssertEquals("37 Procedure Code", ExpectedProcedure, line.Box37Procedure);
				AssertEquals("37_2 Procedure Code", ExpectedAdditionalProcedure, line.Box37_2Procedure);
				AssertEquals("38 Net Mass in KGs", ExpectedNetMass, line.Box38NetWeightInKG);
				AssertEquals("39 Quota", "YAY", line.Box39Quota);
				AssertEquals("41 Supplementary Units", ExpectedSupplementaryUnits, line.Box41SupplementaryUnits);
				AssertEquals("41 Supplementary Quantity", ExpectedSupplementaryQuantity, line.Box41SupplementaryQty);
				AssertEquals("41 Supplementary UQ", ExpectedSupplementaryUQDescription, line.Box41SupplementaryUQDescription);
				AssertEquals("42 Item Price", new Money(1234.00, invoiceHeader.Invoice_Currency).ToString(), line.Box42ItemPrice);// Appears to be the Customs Value, not the raw Invoice Price. Not sure.
				AssertEquals("43 Valuation Method", ValuationMethodList.Codes._1, line.Box43ValuationMethod);
				AssertEquals("46 Statistical Value", ExpectedStatisticalValue, line.Box46StatisticalValue);
				AssertEquals("46 Statistical Value Currency Code", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, line.Box46CurrencyCode);
				AssertEquals("49 Warehouse", ExpectedBox49WarehouseForC88, line.Box49Warehouse);
				AssertEquals("50 Pricipals Representative Name", "FREDDY MERCURY", line.Box50PrincipalsRepresentativeName);
				AssertEquals("50 Pricipals Representative City", "GBLON", line.Box50PrincipalsRepresentativeCity);

				AssertCountrySpecificFields(line);
			});

			invoiceLine.JI_Weight = 0;
			entryLine = entryHeader.MergedLines[0];
			line = GetNewDocSADHLine(entryLine);
			AssertEquals("35 Gross Mass in KGs should be blank not zero", "", line.Box35GrossWeightInKG);
		}

		protected virtual ZString ExpectedCommodityCode => "1234567890";
		protected virtual ZDecimal ExpectedStatisticalValue => 123.45m;
		protected virtual ZString ExpectedBox49WarehouseForC88 => "A1234567GB";
		protected virtual ZString ExpectedECSupplement => "EC1";
		protected virtual ZString ExpectedECSupplement2 => "EC2";
		protected virtual ZString ExpectedGrossMass => "45.359";
		protected virtual ZString ExpectedGrossMassForCommericalPurposesOnly => "45.359";
		protected virtual ZString ExpectedNetMass => "2.5";

		public virtual void TestBox40Contents()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Biz.DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var documentOnHeader = invoiceHeader.PreviousDocuments.AddNew();
			documentOnHeader.CSI_SubType = "X";
			documentOnHeader.CSI_Code = "280";
			documentOnHeader.CSI_ReferenceNumber = "ABCDEFG";

			var documentOnFirstLine = invoiceLine.PreviousDocuments.AddNew();
			documentOnFirstLine.CSI_SubType = PreviousDocumentClassList.Codes.PreviousDocument;
			documentOnFirstLine.CSI_Code = "380";
			documentOnFirstLine.CSI_ReferenceNumber = "3421789012";
			documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);

			var documentOnSecondLine = invoiceLine.PreviousDocuments.AddNew();
			documentOnSecondLine.CSI_SubType = "Y";
			documentOnSecondLine.CSI_Code = "CLE";
			documentOnSecondLine.CSI_ReferenceNumber = "20070701-120-A12345E";
			documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);

			var documentOnGroup = declaration.PreviousDocuments.AddNew();
			documentOnGroup.CSI_SubType = "A";
			documentOnGroup.CSI_Code = "123";
			documentOnGroup.CSI_ReferenceNumber = "987654321";

			var line = DocSADHLine.New(entryLine, Factory);
			AssertEquals("entryLine.Box40PreviousDocuments", "A-123-987654321; X-280-ABCDEFG; Z-380-3421789012-02/01/2000; Y-CLE-20070701-120-A12345E-03/01/2000", line.Box40PreviousDocuments);
		}

		public void TestBox41SupplementaryQty_WhenEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var wrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals(ZString.Empty, wrapper.Box41SupplementaryQty);
			AssertEquals(ZString.Empty, wrapper.Box41SupplementaryUQDescription);
		}

		public virtual void TestBox45Adjustment()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.InvoiceLines.Add(invoiceLine1);
			invoiceLine1.JI_ValuationMarkup = 12.4m;

			var line = DocSADHLine.New(entryLine1, Factory);
			AssertEquals("45 Adjustment", "12.4%", line.Box45Adjustment);
		}

		public virtual void TestBox48DeferredPayment()
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
			var line = DocSADHLine.New(entryLine, Factory);

			declaration.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			AssertEquals(declaration.CountryCode, Core.Constants.CountryCodes.Germany);

			declaration.JE_PaymentMethod = "A";
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "Declarant DAN");
			line = DocSADHLine.New(entryLine, Factory);
			AssertEquals("A Declarant DAN", line.Box48DeferredPayment);

			declaration.JE_PaymentMethod = "B";
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber, "Importer DAN");
			line = DocSADHLine.New(entryLine, Factory);
			AssertEquals("B Importer DAN", line.Box48DeferredPayment);
		}

		public virtual void TestBox31Contents()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Description = "FULL-LENGTH OR KNEE-LENGTH STOCKINGS, SOCKS AND OTHER HOSIERY, INCL. FOOTWEAR WITHOUT APPLIED SOLES, OF WOOL OR FINE ANIMAL HAIR, KNITTED OR CROCHETED (EXCL. GRADUATED COMPRESSION HOSIERY, PANTYHOSE AND TIGHTS, WOMEN''S FULL-LENGTH OR KNEE-LENGTH STOCKINGS, MEASURING PER SINGLE YARN < 67 DECITEX, AND HOSIERY FOR BABIES)";

			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new Biz.SendsMessagesToCustomsShutterUpperer()));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			declaration.CusContainers.AddNew().CO_ContainerNumber = "OOCL3219032";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "OOCL3127895";

			foreach (Biz.NonPersistentCusContainer container in invoiceLine.ContainersForInvoiceLinesForBindingOnly)
			{
				container.IsForInvoiceLine = true;
			}
			declaration.JE_MasterBill = "X";
			var bill = declaration.PrimaryMasterBill;
			var cw1 = bill.PackingGroups[0].Packages[0];
			cw1.CW_PackQty = 10;
			cw1.CW_PackType = "PK";
			cw1.CW_MarksAndNos = "RED";
			var cw2 = bill.PackingGroups[0].Packages.AddNew();
			cw2.CW_PackQty = 11;
			cw2.CW_PackType = "BA";
			cw2.CW_MarksAndNos = "BLUE";

			var packing1 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 6;

			var packing2 = invoiceLine.PackagesForInvoiceLinesForBindingOnly[1];
			packing2.IsLinked = true;
			packing2.PackQty = 7;

			var line = DocSADHLine.New(entryLine, Factory);

			// Note only first 280 chars of desc are shown
			string expected = @"FULL-LENGTH OR KNEE-LENGTH STOCKINGS, SOCKS AND OTHER HOSIERY, INCL. FOOTWEAR WITHOUT APPLIED SOLES, OF WOOL OR FINE ANIMAL HAIR, KNITTED OR CROCHETED (EXCL. GRADUATED COMPRESSION HOSIERY, PANTYHOSE AND TIGHTS, WOMEN''S FULL-LENGTH OR KNEE-LENGTH STOCKINGS, MEASURING PER SINGLE Y
Marks & Number=RED Number=6 Package type=PK; Marks & Number=BLUE Number=7 Package type=BA; CN=OOCL3219032, OOCL3127895";
			AssertEquals("entryLine.Box31Contents", expected, line.Box31PackagesAndDescriptionOfGoods);

			packing2.IsLinked = false;
			line = DocSADHLine.New(entryLine, Factory);
			AssertContains("Marks & Number=RED Number=6 Package type=PK; CN=", line.Box31PackagesAndDescriptionOfGoods);

			packing1.PackQty = 0;
			line = DocSADHLine.New(entryLine, Factory);
			AssertContains("Marks & Number=RED Number=0 Package type=PK; CN=", line.Box31PackagesAndDescriptionOfGoods);
		}

		public void TestBox44CustomsSupervisingOfficeDefaultsFromDeclarationHeader()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			var orgOnDeclaration = Factory.New<OrgHeader>();
			orgOnDeclaration.MainAddress.OA_Address1 = "Add1";
			orgOnDeclaration.MainAddress.OA_City = "City";
			orgOnDeclaration.MainAddress.OA_PostCode = "PCODE";
			orgOnDeclaration.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			declaration.DocAddresses.AddNew(orgOnDeclaration.MainAddress, DocAddressType.CustomsSupervisingOffice);
			AssertEquals(declaration.SupervisingOfficeDocAddress.Address.PK, orgOnDeclaration.MainAddress.PK);

			DocSADHLine line = DocSADHLine.New(entryLine, Factory);
			ZString expected = "SPOFF - Add1, City, PCODE, United Kingdom";
			AssertEquals("entryLine.Box44Contents", expected, line.Box44AddInfoAndDocuments);

			var orgLine = Factory.New<OrgHeader>();
			orgLine.MainAddress.OA_Address1 = "Add2";
			orgLine.MainAddress.OA_City = "City2";
			orgLine.MainAddress.OA_PostCode = "PCODE2";
			orgLine.MainAddress.OA_RL_NKRelatedPortCode = "GBLON";
			invoiceLine.JI_OA_SupervisingOffice = orgLine.MainAddress.PK;
			expected = "SPOFF - Add2, City2, PCODE2, United Kingdom";
			AssertEquals("entryLine.Box44Contents", expected, line.Box44AddInfoAndDocuments);
		}

		public void TestBox44ContentsContainThirdQuantity()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CustomsThirdQuantity = 4.53;

			DocSADHLine line = DocSADHLine.New(entryLine, Factory);
			ZString expected = "4.53- THRDQ";
			AssertEquals("entryLine.Box44Contents", expected, line.Box44AddInfoAndDocuments);
		}

		public void TestBox44UnitedNationsDangerousGoods()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();

			invoiceLine1.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			invoiceLine1.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2872", "b", "IMO").First().PK;
			invoiceLine2.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0313", "b", "IMO").First().PK;
			invoiceLine2.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0107", "", "IMO").First().PK;

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.InvoiceLines.Add(invoiceLine1);

			CusEntryLine entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.InvoiceLines.Add(invoiceLine2);

			DocSADHLine line1 = DocSADHLine.New(entryLine1, Factory);
			ZString expected1 = "0004, 2872";
			AssertEquals("entryLine.Box44Contents.UNDG", expected1, line1.Box44_4UnDangerousGoods);

			DocSADHLine line2 = DocSADHLine.New(entryLine2, Factory);
			ZString expected2 = "0313, 0107";
			AssertEquals("entryLine.Box44Contents.UNDG", expected2, line2.Box44_4UnDangerousGoods);
		}

		public void TestConstructor()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			AssertNull(DocSADHLine.New(entryLine: null, Factory));
			AssertNotNull(DocSADHLine.New(entryLine, Factory));
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			return DocSADHLine.New(entryLine, Factory);
		}

		protected virtual DocSADHLine GetNewDocSADHLine(CusEntryLine entryLine) => DocSADHLine.New(entryLine, Factory);

		public virtual void TestBox44FiscalReferenceNumberLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var wrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals(ZString.Empty, wrapper.Box44FiscalReferenceNumberLabel);
		}

		public virtual void TestBox44FiscalReferenceNumberSummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var wrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals(ZString.Empty, wrapper.Box44FiscalReferenceNumberSummary);
		}

		public void TestBoxS28Seals_WhenEntryLineHasSecondSealFilled()
		{
			var declaration = Factory.New<JobDeclaration>();

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();

			var invoiceLine = declaration
				.Invoices.AddNew()
				.InvoiceLines.AddNew();

			invoiceLine.JI_CL = entryLine.PK;

			AddSeal("S0001", "SND01");
			AddSeal("S0001", "SND02");

			invoiceLine.ContainersForInvoiceLinesForBindingOnly
				.Cast<Biz.NonPersistentCusContainer>()
				.ForEach(c => c.IsForInvoiceLine = true);

			var lineWrapper = DocSADHLine.New(entryLine, Factory);
			AssertEquals("S0001, SND01, SND02", lineWrapper.BoxS28SealsLine);

			void AddSeal(string seal1, string seal2)
			{
				var cnt = declaration.CusContainers.AddNew();
				cnt.CO_Seal = seal1;
				cnt.CO_SecondSeal = seal2;
			}
		}
	}
}
