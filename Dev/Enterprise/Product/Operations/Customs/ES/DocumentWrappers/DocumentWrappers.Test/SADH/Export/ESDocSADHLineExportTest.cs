using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
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
	[MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Spain)]
	sealed class ESDocSADHLineExportTest : ESDocSADHLineTest
	{
		public void TestBox42ItemPrice()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1234.00m;

			CombineAssertions(() =>
			{
				Assert("Precondition: declaration.DoMerge()", declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer()));
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
				var entryLine = entryHeader.MergedLines[0];

				ESDocSADHLineExport line = ESDocSADHLineExport.New(entryLine, Factory);
				AssertEquals("Box42ItemPrice correct format", 1234.00m, line.Box42ItemPrice);
			});
		}

		public void TestBox44Contents_WithEntryStatus()
		{
			SetUpMapData();

			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "AA";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
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
				var expected = "XS01: ES36000000S1 12-03-2020; XS02: ES36000000S2; XS03: ES36000000S3 31-12-2020; " +
				"XA01: ES36000000A1; XA02: ES36000000A2; " +
				"CSAS: InvoiceLine1; C506: InvoiceLine2";
				AssertEquals("Box44AddInfoAndDocuments correct format", expected, line.Box44AddInfoAndDocuments);

				for (var i = 0; i < 30; i++)
				{
					AddSupDoc(entryLine.PK, entryLine.TablePrefix, "AA" + i, "ES36000000S1", new ZDateTime(2020, 03, 12), DocumentStatus.Accepted);
					AddAddInf(entryLine.PK, entryLine.TablePrefix, "AA" + i, "ES36000000A1", status: DocumentStatus.Accepted);
				}

				line = GetNewDocSADHLine(entryLine);

				expected = "XS01: ES36000000S1 12-03-2020; XS02: ES36000000S2; XS03: ES36000000S3 31-12-2020; AA0: ES36000000S1 12-03-2020; AA1: ES36000000S1 12-03-2020; AA2: ES36000000S1 12-03-2020; AA3: ES36000000S1 12-03-2020; AA4: ES36000000S1 12-03-2020; AA5: ES36000000S1 12-03-2020; AA6: ES36000000S1 12-03-2020; AA7: ES36000000S1 12-03-2020; AA8: ES36000000S1 12-03-2020; AA9: ES36000000S1 12-03-2020; AA10: ES36000000S1 12-03-2020; AA11: ES36000000S1 12-03-2020; AA12: ES36000000S1 12-03-2020; AA13: ES36000000S1 12-03-2020; AA14: ES36000000S1 12-03-2020; AA15: ES36000000S1 12-03-2020; AA16: ES36000000S1 12-03-2020; AA17: ES36000000S1 12-03-2020; AA18: ES36000000S1 12-03-2020; AA19: ES36000000S1 12-03-2020; AA20: ES36000000S1 12-03-2020; AA21: ES36000000S1 12-03-2020; AA22: ES36000000S1 12-03-2020; AA23: ES36000000S1 12-03-2020; AA24: ES36000000S1 12-03-2020; AA25: ES36000000S1 12-03-2020; AA26: ES36000000S1 12-03-2020; AA27: ES36000000S1 12-03-2020; AA28: ES36000000S1 12-03-2020";
				AssertEquals("Box44AddInfoAndDocuments correct format and max length", expected, line.Box44AddInfoAndDocuments);
			});
		}

		public void TestBox44Contents_WithEntryStatus_T2L()
		{
			SetUpMapData();

			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "AA";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList.Codes.T2L;
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
					AddAddInf(entryLine.PK, entryLine.TablePrefix, "AA" + i, "ES36000000A1", status: DocumentStatus.Accepted);
				}

				line = GetNewDocSADHLine(entryLine);

				expected = "XS01: ES36000000S1 12-03-2020; XS02: ES36000000S2; XS03: ES36000000S3 31-12-2020; AA0: ES36000000S1 12-03-2020; AA1: ES36000000S1 12-03-2020; AA2: ES36000000S1 12-03-2020; AA3: ES36000000S1 12-03-2020; AA4: ES36000000S1 12-03-2020; AA5: ES36000000S1 12-03-2020; AA6: ES36000000S1 12-03-2020; AA7: ES36000000S1 12-03-2020; AA8: ES36000000S1 12-03-2020; AA9: ES36000000S1 12-03-2020; AA10: ES36000000S1 12-03-2020; AA11: ES36000000S1 12-03-2020; AA12: ES36000000S1 12-03-2020; AA13: ES36000000S1 12-03-2020; AA14: ES36000000S1 12-03-2020; AA15: ES36000000S1 12-03-2020; AA16: ES36000000S1 12-03-2020; AA17: ES36000000S1 12-03-2020; AA18: ES36000000S1 12-03-2020; AA19: ES36000000S1 12-03-2020; AA20: ES36000000S1 12-03-2020; AA21: ES36000000S1 12-03-2020; AA22: ES36000000S1 12-03-2020; AA23: ES36000000S1 12-03-2020; AA24: ES36000000S1 12-03-2020; AA25: ES36000000S1 12-03-2020; AA26: ES36000000S1 12-03-2020; AA27: ES36000000S1 12-03-2020; AA28: ES36000000S1 12-03-2020";
				AssertEquals("Box44AddInfoAndDocuments correct format and max length", expected, line.Box44AddInfoAndDocuments);
			});
		}

		public void TestBox44Contents_WithEntryStatus_EXS()
		{
			SetUpMapData();

			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "AA";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Enterprise.Customs.ES.Business.Declaration.ExsEntrySubStyleList.Codes.EXS;
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
					AddAddInf(entryLine.PK, entryLine.TablePrefix, "AA" + i, "ES36000000A1", status: DocumentStatus.Accepted);
				}

				line = GetNewDocSADHLine(entryLine);

				expected = "XS01: ES36000000S1 12-03-2020; XS02: ES36000000S2; XS03: ES36000000S3 31-12-2020; AA0: ES36000000S1 12-03-2020; AA1: ES36000000S1 12-03-2020; AA2: ES36000000S1 12-03-2020; AA3: ES36000000S1 12-03-2020; AA4: ES36000000S1 12-03-2020; AA5: ES36000000S1 12-03-2020; AA6: ES36000000S1 12-03-2020; AA7: ES36000000S1 12-03-2020; AA8: ES36000000S1 12-03-2020; AA9: ES36000000S1 12-03-2020; AA10: ES36000000S1 12-03-2020; AA11: ES36000000S1 12-03-2020; AA12: ES36000000S1 12-03-2020; AA13: ES36000000S1 12-03-2020; AA14: ES36000000S1 12-03-2020; AA15: ES36000000S1 12-03-2020; AA16: ES36000000S1 12-03-2020; AA17: ES36000000S1 12-03-2020; AA18: ES36000000S1 12-03-2020; AA19: ES36000000S1 12-03-2020; AA20: ES36000000S1 12-03-2020; AA21: ES36000000S1 12-03-2020; AA22: ES36000000S1 12-03-2020; AA23: ES36000000S1 12-03-2020; AA24: ES36000000S1 12-03-2020; AA25: ES36000000S1 12-03-2020; AA26: ES36000000S1 12-03-2020; AA27: ES36000000S1 12-03-2020; AA28: ES36000000S1 12-03-2020";
				AssertEquals("Box44AddInfoAndDocuments correct format and max length", expected, line.Box44AddInfoAndDocuments);
			});
		}

		public void TestBox44Contents_DifferencesBetweenFirstAndSecondEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
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

			var expected1 = "AA0: ES36000000S1 12-03-2020; AA1: ES36000000S1 12-03-2020; AA2: ES36000000S1 12-03-2020; AA3: ES36000000S1 12-03-2020; AA4: ES36000000S1 12-03-2020; AA5: ES36000000S1 12-03-2020; AA6: ES36000000S1 12-03-2020; AA7: ES36000000S1 12-03-2020; AA8: ES36000000S1 12-03-2020; AA9: ES36000000S1 12-03-2020; AA10: ES36000000S1 12-03-2020; AA11: ES36000000S1 12-03-2020; AA12: ES36000000S1 12-03-2020; AA13: ES36000000S1 12-03-2020; AA14: ES36000000S1 12-03-2020; AA15: ES36000000S1 12-03-2020; AA16: ES36000000S1 12-03-2020; AA17: ES36000000S1 12-03-2020; AA18: ES36000000S1 12-03-2020; AA19: ES36000000S1 12-03-2020; AA20: ES36000000S1 12-03-2020; AA21: ES36000000S1 12-03-2020; AA22: ES36000000S1 12-03-2020; AA23: ES36000000S1 12-03-2020; AA24: ES36000000S1 12-03-2020; AA25: ES36000000S1 12-03-2020; AA26: ES36000000S1 12-03-2020; AA27: ES36000000S1 12-03-2020; AA28: ES36000000S1 12-03-2020; AA29: ES36000000S1 12-03-2020; AA30: ES36000000S1 12-03-2020; AA31: ES36000000S1 12-03-2020";
			var expected2 = "AA0: ES36000000S1 12-03-2020; AA1: ES36000000S1 12-03-2020; AA2: ES36000000S1 12-03-2020; AA3: ES36000000S1 12-03-2020; AA4: ES36000000S1 12-03-2020; AA5: ES36000000S1 12-03-2020; AA6: ES36000000S1 12-03-2020; AA7: ES36000000S1 12-03-2020; AA8: ES36000000S1 12-03-2020; AA9: ES36000000S1 12-03-2020; AA10: ES36000000S1 12-03-2020; AA11: ES36000000S1 12-03-2020; AA12: ES36000000S1 12-03-2020; AA13: ES36000000S1 12-03-2020; AA14: ES36000000S1 12-03-2020; AA15: ES36000000S1 12-03-2020; AA16: ES36000000S1 12-03-2020; AA17: ES36000000S1 12-03-2020; AA18: ES36000000S1 12-03-2020; AA19: ES36000000S1 12-03-2020; AA20: ES36000000S1 12-03-2020; AA21: ES36000000S1 12-03-2020; AA22: ES36000000S1 12-03-2020; AA23: ES36000000S1 12-03-2020; AA24: ES36000000S1 12-03-2020; AA25: ES36000000S1 12-03-2020; AA26: ES36000000S1 12-03-2020; AA27: ES36000000S1 12-03-2020";

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
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS01", "DeclarationSDoc1", ZDateTime.Empty, ZString.Empty));
			declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA01", "DeclarationADoc1"));

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS01", "HeaderSDoc1", new ZDateTime(2020, 03, 12), ZString.Empty));
			invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA01", "HeaderADoc1"));

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList.Codes.A;
			entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "OTE", "EntryInstruction1"));
			entryInstruction.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, entryInstruction.TablePrefix, holder.PK, "AAA", "EntryInstruction2"));
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "LS01", "LineSDoc1", new ZDateTime(2020, 03, 12), ZString.Empty));
			invoiceLine.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceLine.TablePrefix, "LS02", "LineSDoc2", ZDateTime.Empty, ZString.Empty));
			invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "LA01", "LineADoc1"));
			invoiceLine.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceLine.TablePrefix, "LA02", "LineADoc2"));
			invoiceLine.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine.TablePrefix, holder.PK, "SAS", "InvoiceLine1"));
			invoiceLine.CusAuthorizationUsages.Add(AddAuthorization(ZGuid.Empty, invoiceLine.TablePrefix, holder.PK, "DPO", "InvoiceLine2"));

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
				"XS03: ES36000000S3 31-12-2020; XS04: ES36000000S4 31-03-2020; " +
				"DA01: DeclarationADoc1; " +
				"HA01: HeaderADoc1; " +
				"LA01: LineADoc1; LA02: LineADoc2; " +
			"CES: EntryInstruction1; AAA: EntryInstruction2; " +
			"CSAS: InvoiceLine1; C506: InvoiceLine2";
			AssertEquals("Box44AddInfoAndDocuments correct format", expected, line.Box44AddInfoAndDocuments);
		}

		public void TestBox44Contents_NoEntryStatus_T2L()
		{
			SetUpMapData();

			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "AA";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS01", "DeclarationSDoc1", ZDateTime.Empty, ZString.Empty));
			declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA01", "DeclarationADoc1"));

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS01", "HeaderSDoc1", new ZDateTime(2020, 03, 12), ZString.Empty));
			invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA01", "HeaderADoc1"));

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList.Codes.T2L;
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

		public void TestBox44Contents_NoEntryStatus_EXS()
		{
			SetUpMapData();

			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "AA";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, declaration.TablePrefix, "DS01", "DeclarationSDoc1", ZDateTime.Empty, ZString.Empty));
			declaration.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, declaration.TablePrefix, "DA01", "DeclarationADoc1"));

			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.SupportingDocuments.Add(AddSupDoc(ZGuid.Empty, invoiceHeader.TablePrefix, "HS01", "HeaderSDoc1", new ZDateTime(2020, 03, 12), ZString.Empty));
			invoiceHeader.AdditionalInfos.Add(AddAddInf(ZGuid.Empty, invoiceHeader.TablePrefix, "HA01", "HeaderADoc1"));

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = Enterprise.Customs.ES.Business.Declaration.ExsEntrySubStyleList.Codes.EXS;
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

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			return ESDocSADHLineExport.New(entryLine, Factory);
		}

		protected override DocSADHLine GetNewDocSADHLine(EU.Business.Declaration.CusEntryLine entryLine)
		{
			return ESDocSADHLineExport.New((ESCusEntryLine)entryLine, Factory);
		}

		protected override void SetUpForOutwardProcedure()
		{
			zzzDataGrouping = "ES";
			procedureCode = "71";
			previousProcedureCode = "40";
			concession = "C33";
			country = Core.Constants.CountryCodes.Spain;
			customsRegNo = "A12345678ES";
			shipmentType = "EXP";
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
			shipmentType = "EXP";
			group = "ES";
		}

		protected override ZString ExpectedBox41SupplementaryUnitsWithDecimals => "1,005.346566 ABC";
		protected override ZString ExpectedBox41SupplementaryQtyWithDecimals => "1,005.346566";
		protected override ZString ExpectedCommodityCode => "1234567890";
		protected override ZString ExpectedBox49WarehouseInward => "A12345678ES";
		protected override ZString ExpectedBox49WarehouseOutward => "A12345678ES";
	}
}
