using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;
using JobDeclaration = Enterprise.Customs.ES.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Spain)]
	sealed class ESDocSADHLineImportTest : ESDocSADHLineTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			return ESDocSADHLineImport.New(entryLine, Factory);
		}

		public void TestBox33CommodityCodeImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];
				ESDocSADHLineImport line = ESDocSADHLineImport.New(entryLine, Factory);

				AssertEquals("Box33ab Empty", ZString.Empty, line.Box33CommodityCode);

				invoiceLine.JI_Tariff = "6501000000";
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Box33ab Only Tariff", "6501000000", line.Box33CommodityCode);

				invoiceLine.JI_SupplementaryCode1 = "2222";
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Box33ab Tariff+SupplementaryCode1", "65010000002222", line.Box33CommodityCode);
			});
		}

		protected override void SetUpForOutwardProcedure()
		{
			zzzDataGrouping = "ES";
			procedureCode = "71";
			previousProcedureCode = "40";
			concession = "C33";
			country = Core.Constants.CountryCodes.Spain;
			customsRegNo = "A12345678ES";
			shipmentType = "IMP";
			group = "ES";
		}

		protected override void SetUpForInwardProcedure()
		{
			zzzDataGrouping = "ES";
			procedureCode = "71";
			previousProcedureCode = "40";
			concession = "C33";
			country = Core.Constants.CountryCodes.Spain;
			customsRegNo = "A12345678ES";
			shipmentType = "IMP";
			group = "ES";
		}

		public void TestBox33ECSupplementImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			invoiceLine.JI_SupplementaryCode2 = "6666";
			ESDocSADHLineImport line = ESDocSADHLineImport.New(entryLine, Factory);

			AssertEquals("Box33c SupplementaryCode2", "6666", line.Box33ECSupplement);
		}

		public void TestBox33ECSupplement2Import()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];
				ESDocSADHLineImport line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Box33d Empty", ZString.Empty, line.Box33ECSupplement2);

				invoiceLine.ZG_ExciseCode = "0F1";
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Box33d No ZG_ExciseExemption", "0F10", line.Box33ECSupplement2);

				invoiceLine.ZG_ExciseExemption = "S";
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Box33d No ZG_ExciseExemption", "0F1S", line.Box33ECSupplement2);
			});
		}

		public override void TestBox45Adjustment()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];

				ESDocSADHLineImport line = ESDocSADHLineImport.New(entryLine, Factory);

				AssertEquals("Test Box 45 Complete test without values", "+0 -0", line.Box45Adjustment);

				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
				var groupHeader = invoiceHeader.GroupHeader;

				var oNS = groupHeader.Charges.AddNew(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, 30m, declaration.LocalCurrencyCode);
				oNS.J7_IsDutiable = true;
				oNS.J7_IsIncludedInITOT = false;
				var oFT = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 2000.05m, declaration.LocalCurrencyCode);
				oFT.J7_IsDutiable = false;
				oFT.J7_IsIncludedInITOT = true;
				var oL1 = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, 3m, declaration.LocalCurrencyCode);
				oL1.J7_IsDutiable = true;
				oL1.J7_IsIncludedInITOT = false;
				var oL2 = invoiceLine.Charges.AddNew(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge, 2m, declaration.LocalCurrencyCode);
				oL2.J7_IsDutiable = false;
				oL2.J7_IsIncludedInITOT = true;

				invoiceHeader.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoiceHeader.JZ_InvoiceAmount = 100m;
				invoiceLine.JI_LinePrice = 100m;
				declaration.ResumeApportionment();

				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Test Box 45 Complete test with values", "+33 -2,002.05", line.Box45Adjustment);
			});
		}

		public void TestBox44Contents_WithEntryStatus()
		{
			SetUpMapData();

			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "AA";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList.Codes.A;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "LS01", "LineSDoc1", new ZDateTime(2020, 03, 12), ZString.Empty));
			invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "LS02", "LineSDoc2", new ZDateTime(2020, 12, 31), ZString.Empty));
			invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "LA01", "LineADoc1"));
			invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "LA02", "LineADoc2"));
			invoiceLine.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine.TablePrefix, holder.PK, "SAS", "InvoiceLine1"));
			invoiceLine.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine.TablePrefix, holder.PK, "DPO", "InvoiceLine2"));

			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = "CLP";

			var entryLine = entryHeader.AllEntryLines[0];

			AddSupDoc(entryLine.PK, entryLine.TablePrefix, "XS01", "ES36000000S1", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
			AddSupDoc(entryLine.PK, entryLine.TablePrefix, "XS02", "ES36000000S2", ZDateTime.Empty, DocumentStatus.Accepted);
			AddSupDoc(entryLine.PK, entryLine.TablePrefix, "XS03", "ES36000000S3", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted);
			AddSupDoc(entryLine.PK, entryLine.TablePrefix, "XS04", "ES36000000S4", new ZDateTime(2020, 12, 31), DocumentStatus.Cancelled);

			AddAddInf(entryLine.PK, entryLine.TablePrefix, "XA01", "ES36000000A1", status: DocumentStatus.Accepted);
			AddAddInf(entryLine.PK, entryLine.TablePrefix, "XA02", "ES36000000A2", status: DocumentStatus.Accepted);
			AddAddInf(entryLine.PK, entryLine.TablePrefix, "XA03", "ES36000000A3", subtype: "INF", status: DocumentStatus.Accepted);
			AddAddInf(entryLine.PK, entryLine.TablePrefix, "XA04", "ES36000000A4", status: DocumentStatus.Cancelled);

			CombineAssertions(() =>
			{
				var line = GetNewDocSADHLine(entryLine);
				var expected = "XS01: ES36000000S1 12-03-2020; XS02: ES36000000S2; XS03: ES36000000S3 31-12-2020";
				AssertEquals("Box44AddInfoAndDocuments correct format", expected, line.Box44AddInfoAndDocuments);

				for (var i = 0; i < 30; i++)
				{
					AddSupDoc(entryLine.PK, entryLine.TablePrefix, "AA" + i, "ES36000000S1", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
					AddAddInf(entryLine.PK, entryLine.TablePrefix, "AA" + i, "ES36000000A1", DocumentStatus.Accepted);
				}

				line = GetNewDocSADHLine(entryLine);

				expected = "XS01: ES36000000S1 12-03-2020; XS02: ES36000000S2; XS03: ES36000000S3 31-12-2020; AA0: ES36000000S1 12-03-2020; AA1: ES36000000S1 12-03-2020; AA2: ES36000000S1 12-03-2020; AA3: ES36000000S1 12-03-2020; AA4: ES36000000S1 12-03-2020; AA5: ES36000000S1 12-03-2020; AA6: ES36000000S1 12-03-2020; AA7: ES36000000S1 12-03-2020; AA8: ES36000000S1 12-03-2020; AA9: ES36000000S1 12-03-2020; AA10: ES36000000S1 12-03-2020; AA11: ES36000000S1 12-03-2020; AA12: ES36000000S1 12-03-2020; AA13: ES36000000S1 12-03-2020; AA14: ES36000000S1 12-03-2020; AA15: ES36000000S1 12-03-2020; AA16: ES36000000S1 12-03-2020; AA17: ES36000000S1 12-03-2020; AA18: ES36000000S1 12-03-2020; AA19: ES36000000S1 12-03-2020; AA20: ES36000000S1 12-03-2020; AA21: ES36000000S1 12-03-2020; AA22: ES36000000S1 12-03-2020; AA23: ES36000000S1 12-03-2020; AA24: ES36000000S1 12-03-2020; AA25: ES36000000S1 12-03-2020";
				AssertEquals("Box44AddInfoAndDocuments correct format and max length", expected, line.Box44AddInfoAndDocuments);
			});
		}

		public void TestBox44Contents_DifferencesBetweenFirstAndSecondEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList.Codes.A;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;

			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = "CLP";

			var entryLine1 = entryHeader.AllEntryLines[0];
			var entryLine2 = entryHeader.AllEntryLines[1];

			for (var i = 0; i < 50; i++)
			{
				AddSupDoc(entryLine1.PK, entryLine1.TablePrefix, "AA" + i, "ES36000000S1", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
				AddSupDoc(entryLine2.PK, entryLine2.TablePrefix, "AA" + i, "ES36000000S1", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
			}

			var line1 = GetNewDocSADHLine(entryLine1);
			var line2 = GetNewDocSADHLine(entryLine2);

			var expected1 = "AA0: ES36000000S1 12-03-2020; AA1: ES36000000S1 12-03-2020; AA2: ES36000000S1 12-03-2020; AA3: ES36000000S1 12-03-2020; AA4: ES36000000S1 12-03-2020; AA5: ES36000000S1 12-03-2020; AA6: ES36000000S1 12-03-2020; AA7: ES36000000S1 12-03-2020; AA8: ES36000000S1 12-03-2020; AA9: ES36000000S1 12-03-2020; AA10: ES36000000S1 12-03-2020; AA11: ES36000000S1 12-03-2020; AA12: ES36000000S1 12-03-2020; AA13: ES36000000S1 12-03-2020; AA14: ES36000000S1 12-03-2020; AA15: ES36000000S1 12-03-2020; AA16: ES36000000S1 12-03-2020; AA17: ES36000000S1 12-03-2020; AA18: ES36000000S1 12-03-2020; AA19: ES36000000S1 12-03-2020; AA20: ES36000000S1 12-03-2020; AA21: ES36000000S1 12-03-2020; AA22: ES36000000S1 12-03-2020; AA23: ES36000000S1 12-03-2020; AA24: ES36000000S1 12-03-2020; AA25: ES36000000S1 12-03-2020; AA26: ES36000000S1 12-03-2020; AA27: ES36000000S1 12-03-2020; AA28: ES36000000S1 12-03-2020";
			var expected2 = "AA0: ES36000000S1 12-03-2020; AA1: ES36000000S1 12-03-2020; AA2: ES36000000S1 12-03-2020; AA3: ES36000000S1 12-03-2020; AA4: ES36000000S1 12-03-2020; AA5: ES36000000S1 12-03-2020; AA6: ES36000000S1 12-03-2020; AA7: ES36000000S1 12-03-2020; AA8: ES36000000S1 12-03-2020; AA9: ES36000000S1 12-03-2020; AA10: ES36000000S1 12-03-2020; AA11: ES36000000S1 12-03-2020; AA12: ES36000000S1 12-03-2020; AA13: ES36000000S1 12-03-2020; AA14: ES36000000S1 12-03-2020; AA15: ES36000000S1 12-03-2020; AA16: ES36000000S1 12-03-2020; AA17: ES36000000S1 12-03-2020; AA18: ES36000000S1 12-03-2020; AA19: ES36000000S1 12-03-2020; AA20: ES36000000S1 12-03-2020; AA21: ES36000000S1 12-03-2020; AA22: ES36000000S1 12-03-2020; AA23: ES36000000S1 12-03-2020; AA24: ES36000000S1 12-03-2020; AA25: ES36000000S1 12-03-2020";

			CombineAssertions(() =>
			{
				AssertEquals("Box44AddInfoAndDocuments for First EntryLine", expected1, line1.Box44AddInfoAndDocuments);
				AssertEquals("Box44AddInfoAndDocuments for Second EntryLine", expected2, line2.Box44AddInfoAndDocuments);
			});
		}

		public void TestBox44Contents_NoEntryStatus()
		{
			SetUpMapData();

			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "AA";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS01", "DeclarationSDoc1", ZDateTime.Empty, ZString.Empty));
			declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA01", "DeclarationADoc1"));

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS01", "HeaderSDoc1", new ZDateTime(2020, 03, 12), ZString.Empty));
			invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA01", "HeaderADoc1"));

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList.Codes.A;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "LS01", "LineSDoc1", new ZDateTime(2020, 03, 12), ZString.Empty));
			invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "LS02", "LineSDoc2", ZDateTime.Empty, ZString.Empty));
			invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "LA01", "LineADoc1"));
			invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "LA02", "LineADoc2"));

			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_EntryStatus = ZString.Empty;

			var entryLine = entryHeader.AllEntryLines[0];

			AddSupDoc(entryLine.PK, entryLine.TablePrefix, "XS01", "ES36000000S1", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
			AddSupDoc(entryLine.PK, entryLine.TablePrefix, "XS02", "ES36000000S2", ZDateTime.Empty, DocumentStatus.Accepted);
			AddSupDoc(entryLine.PK, entryLine.TablePrefix, "XS03", "ES36000000S3", new ZDateTime(2020, 12, 31), DocumentStatus.Accepted, SupportingDocumentSubType.LIQ);
			AddSupDoc(entryLine.PK, entryLine.TablePrefix, "XS04", "ES36000000S4", new ZDateTime(2020, 03, 31), DocumentStatus.Cancelled, SupportingDocumentSubType.LIQ);

			AddAddInf(entryLine.PK, entryLine.TablePrefix, "XA01", "ES36000000A1", status: DocumentStatus.Accepted);
			AddAddInf(entryLine.PK, entryLine.TablePrefix, "XA02", "ES36000000A2", status: DocumentStatus.Accepted);
			AddAddInf(entryLine.PK, entryLine.TablePrefix, "XA03", "ES36000000A3", subtype: "INF", status: DocumentStatus.Accepted);
			AddAddInf(entryLine.PK, entryLine.TablePrefix, "XA04", "ES36000000A4", status: DocumentStatus.Cancelled);

			var line = GetNewDocSADHLine(entryLine);

			var expected = "DS01: DeclarationSDoc1; " +
				"HS01: HeaderSDoc1 12-03-2020; " +
				"LS01: LineSDoc1 12-03-2020; LS02: LineSDoc2; " +
				"XS03: ES36000000S3 31-12-2020; XS04: ES36000000S4 31-03-2020";
			AssertEquals("Box44AddInfoAndDocuments correct format", expected, line.Box44AddInfoAndDocuments);
		}

		public void TestBox44VatInfoCalculation()
		{
			CanaryIslandSetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				var entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];
				ESDocSADHLineImport line = ESDocSADHLineImport.New(entryLine, Factory);

				AssertEquals("Box44VatInfoCalculation all the data is empty", "B. IVA: + 0 + 0 = 0", line.Box44VatInfoCalculation);

				entryLine.CL_CustomsValue = 50m;
				var feeB00 = entryLine.Fees.AddNew();
				feeB00.CF_ChargeType = "B00";
				feeB00.CF_BaseValue = 1524.55m;
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Box44VatInfoCalculation only customs value and result", "B. IVA: 50 + 0 + 0 = 1,524.55", line.Box44VatInfoCalculation);

				var suppDoc1 = invoiceLine.SupportingDocuments.AddNew();
				suppDoc1.CSI_Code = SupportingDocumentType.TransformedRPP;
				suppDoc1.CSI_ReferenceNumber = "25,5";
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("7009", "B. IVA: 50 - 25.5 + 0 + 0 = 1,524.55", line.Box44VatInfoCalculation);

				suppDoc1.CSI_Code = SupportingDocumentType.REARebate;
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("7003", "B. IVA: 50 - 25.5 + 0 + 0 = 1,524.55", line.Box44VatInfoCalculation);

				var feeA00 = entryLine.Fees.AddNew();
				feeA00.CF_ChargeType = "A00";
				feeA00.CF_ChargeAmount = 250.05m;
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Only type taxes with A", "B. IVA: 50 - 25.5 + 250.05 + 0 = 1,524.55", line.Box44VatInfoCalculation);

				var specialTax = entryLine.Fees.AddNew();
				specialTax.CF_ChargeType = "4FG";
				specialTax.CF_ChargeAmount = 250m;
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Special taxes and Taxes with A", "B. IVA: 50 - 25.5 + 500.05 + 0 = 1,524.55", line.Box44VatInfoCalculation);

				var charge3 = invoiceLine.Charges.AddNew();
				charge3.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
				charge3.J7_IsDutiable = false;
				charge3.J7_IsIncludedInITOT = true;
				charge3.J7_IsGSTApplicable = true;
				charge3.J7_Amount = 1500m;
				charge3.J7_RX_NKCurrency = declaration.LocalCurrencyCode;

				var charge4 = invoiceLine.Charges.AddNew();
				charge4.J7_ChargeType = UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge;
				charge4.J7_IsDutiable = true;
				charge4.J7_IsIncludedInITOT = false;
				charge4.J7_IsGSTApplicable = false;
				charge4.J7_Amount = 500m;
				charge4.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Vat Additions", "B. IVA: 50 - 25.5 + 500.05 + 1,000 = 1,524.55", line.Box44VatInfoCalculation);

				declaration.ZG_DestinationState = Enterprise.Customs.ES.Business.CustomsFiscalTerritoriesList.GetCanaryIslandsList(Factory).GetAllCodes()[0];
				line = ESDocSADHLineImport.New(entryLine, Factory);
				AssertEquals("Canary Islands", "B. IGIC: 50 - 25.5 + 500.05 + 1,000 = 1,524.55", line.Box44VatInfoCalculation);
			});
		}

		protected override DocSADHLine GetSADHLineForBox47Taxes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var feeA00 = entryLine.Fees.AddNew();
			feeA00.CF_ChargeType = "A00";
			feeA00.CF_ChargeAmount = 29.85m;
			feeA00.CF_BaseValue = 1105.89m;

			var feeB00 = entryLine.Fees.AddNew();
			feeB00.CF_ChargeType = "B00";
			feeB00.CF_ChargeAmount = 221.55m;
			feeB00.CF_BaseValue = 1266.03m;

			return GetNewDocSADHLine(entryLine);
		}

		protected override DocSADHLine GetSADHLineForBox47TaxesOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];

			foreach (var taxType in TaxTypesToTestOrder)
			{
				entryLine.Fees.AddNew().G4_Type = taxType;
			}

			return GetNewDocSADHLine(entryLine);
		}

		protected override ZString ExpectedBox41SupplementaryUnitsWithDecimals => "1,005.347 ABC";
		protected override ZString ExpectedBox41SupplementaryQtyWithDecimals => "1,005.347";
		protected override ZString[] TaxTypesToTestOrder => new ZString[] { "B10", "D10", "B00", "A00", "3RM", "3IG" };
		protected override ZString[] ExpectedTaxTypesOrder => new ZString[] { "A00", "3IG", "3RM", "B00", "D10", "B10" };
		protected override ZString ExpectedCommodityCode => "1234567890EC1";
		protected override ZString ExpectedECSupplement => "EC2";
		protected override ZString ExpectedECSupplement2 => "";
		protected override ZString ExpectedBox49WarehouseInward => "A12345678ES";
		protected override ZString ExpectedBox49WarehouseOutward => "A12345678ES";
		protected override ZString ExpectedGrossMassForCommericalPurposesOnly => "45.36";
		protected override DocSADHLine GetNewDocSADHLine(EU.Business.Declaration.CusEntryLine entryLine)
		{
			return ESDocSADHLineImport.New((ESCusEntryLine)entryLine, Factory);
		}

		protected override void AssertBox47Taxes(DocSADHLineTaxCollection taxCollection)
		{
			AssertNotNull(taxCollection);
			AssertEquals("TaxCollection count", 2, taxCollection.Count);
			AssertEquals("TaxCollection[1].G4_Type", "B00", taxCollection[1].G4_Type);
			AssertEquals("TaxCollection[1].Box47b", "1,266.03", taxCollection[1].Box47b);
			AssertEquals("TaxCollection[1].G4_Amount_InDeclarationCurrency", "221.55", taxCollection[1].G4_Amount_InDeclarationCurrency);

			AssertEquals("TaxCollection[0].G4_Type", "A00", taxCollection[0].G4_Type);
			AssertEquals("TaxCollection[0].Box47b", "1,105.89", taxCollection[0].Box47b);
			AssertEquals("TaxCollection[0].G4_Amount_InDeclarationCurrency", "29.85", taxCollection[0].G4_Amount_InDeclarationCurrency);
		}

		void CanaryIslandSetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "State and Territories");

			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain + "C", parent: grouping);
			helper.CreateCusCodeList("ESC", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsFiscalTerritories, "64", "Test 61", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}
	}
}
