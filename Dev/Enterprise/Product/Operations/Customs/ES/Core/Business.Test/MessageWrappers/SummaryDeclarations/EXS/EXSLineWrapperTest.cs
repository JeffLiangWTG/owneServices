using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	class EXSLineWrapperTest : WrapperHelperTest<EXSLineWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("EntryLine null", () => GetWrapper(null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("Empty InvoiceLines", () => GetWrapper(Factory.New<CusEntryLine>()));
			});
		}

		public void TestLineNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals("Expected filled LineNumber", 2, wrapper.LineNumber);
		}

		public void TestGoodsDescription()
		{
			invoiceLine.JI_Description = EntryLineData.GoodsDescription;
			AssertEquals("Expected filled GoodsDescription", EntryLineData.GoodsDescription, wrapper.GoodsDescription);
		}

		public void TestGrossWeight()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_Weight = 200.445M;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("Expected filled GrossWeightInKG when weight > 1 rounded to the upper integer unit", 200.445M, wrapper.GrossWeight);

				invoiceLine.JI_Weight = 100.9938M;
				invoiceLine.JI_WeightUQ = Core.Constants.Weight.Grams;
				AssertEquals("Expected filled GrossWeightInKG when weight < 1", 0.100994M, wrapper.GrossWeight);

				var invLine1 = entryLine.InvoiceLines.AddNew();
				invLine1.JI_Weight = 4;
				invLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
				AssertEquals("Expected filled GrossWeightInKG with the sum of gross weights in all invoice lines in entryline", 4.100994M, wrapper.GrossWeight);
			});
		}

		public void TestMethodOfPayment()
		{
			CombineAssertions(() =>
			{
				invoice.ZG_TransportChargesMethodOfPayment = MethodOfPaymentList.Codes.A;
				wrapper = GetWrapper(entryLine);
				AssertEquals("Expected MethodOfPayment when send true in shouldDeclareMopValueinLine", MethodOfPaymentList.Codes.A, wrapper.MethodOfPayment);
				wrapper = GetWrapper(entryLine, shouldDeclareMopValueinLine: false);
				AssertEquals("Expected MethodOfPayment when send false in shouldDeclareMopValueinLine", ZString.Empty, wrapper.MethodOfPayment);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;
				var invoiceHeader1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction1.PK;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;
				var invoiceHeader2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;

				var entryHeader1 = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
				entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
				var entryLine1 = entryHeader1.AllEntryLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				invoiceLine1.JI_CL = entryLine1.PK;

				var entryLine2 = entryHeader1.AllEntryLines.AddNew();
				entryLine2.CL_LineNumber = 2;
				invoiceLine2.JI_CL = entryLine2.PK;

				invoiceHeader1.ZG_TransportChargesMethodOfPayment = MethodOfPaymentList.Codes.A;
				var wrapper1 = GetWrapper(entryLine1);
				AssertEquals("Expected MethodOfPayment when send true in shouldDeclareMopValueinLine and two invoice (invoice 1)", MethodOfPaymentList.Codes.A, wrapper1.MethodOfPayment);
				invoiceHeader2.ZG_TransportChargesMethodOfPayment = MethodOfPaymentList.Codes.J;
				var wrapper2 = GetWrapper(entryLine2);
				AssertEquals("Expected MethodOfPayment when send true in shouldDeclareMopValueinLine and two invoice (invoice 2)", MethodOfPaymentList.Codes.J, wrapper2.MethodOfPayment);
			});
		}

		public void TestUNDangerousCode()
		{
			var code = "001";
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = code;
			subs.DG_Variant = "A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			invoiceLine.UNDGs.FirstItemForBinding[0].LinkDefault(subs);
			wrapper = GetWrapper(entryLine);
			AssertEquals("Expected filled DangerousGoodsCode", code + "A", wrapper.UNDangerousCode);
		}

		public void TestReferenceNumber()
		{
			invoiceLine.ZG_CommercialReference = "AAAA";
			AssertEquals("Expected filled ReferenceNumber", "AAAA", wrapper.ReferenceNumber);
		}

		public void TestCertificates()
		{
			CombineAssertions(() =>
			{
				var certificates = wrapper.Certificates;
				AssertEquals("Expected filled Certificates when no data", 0, certificates.Count);

				declaration.SupportingDocuments.Add(GetSupportingDoc("AAA", "REF111"));
				declaration.SupportingDocuments.Add(GetSupportingDoc("N380", "REF222"));
				declaration.SupportingDocuments.Add(GetSupportingDoc("N325", "REF333"));
				entryInstruction.SupportingDocuments.Add(GetSupportingDoc("D005", "REF444"));
				entryInstruction.SupportingDocuments.Add(GetSupportingDoc("D008", "REF555"));
				entryInstruction.SupportingDocuments.Add(GetSupportingDoc("N935", "REF666"));
				invoice.SupportingDocuments.Add(GetSupportingDoc("D005", "REF777"));
				invoice.SupportingDocuments.Add(GetSupportingDoc("D008", "REF888"));
				invoice.SupportingDocuments.Add(GetSupportingDoc("N935", "REF999"));
				invoiceLine.SupportingDocuments.Add(GetSupportingDoc("1001", "REF100"));
				invoiceLine.SupportingDocuments.Add(GetSupportingDoc("1003", "REF200"));
				invoiceLine.SupportingDocuments.Add(GetSupportingDoc("1004", "REF300"));

				var supdocEntry1 = GetSupportingDoc("1003", "REF888");
				supdocEntry1.CSI_ParentID = entryLine.PK;
				supdocEntry1.CSI_ParentTableCode = entryLine.TablePrefix;

				wrapper = GetWrapper(entryLine);
				certificates = wrapper.Certificates;

				AssertEquals("Expected filled Certificates", 12, certificates.Count);
				AssertSame("Cached Certificates", wrapper.Certificates, certificates);
			});
		}

		public void TestPreviousDocument()
		{
			CombineAssertions(() =>
			{
				AssertEquals("For empty previous document expected null", null, wrapper.PreviousDocument);

				var previousDoc = invoiceLine.PreviousDocuments.AddNew();
				previousDoc.CSI_SubType = EntryLineData.PrevDocSubType;
				previousDoc.CSI_ReferenceNumber = "Reference";
				previousDoc.CSI_Code = "SUM";
				previousDoc.CSI_SubType = "Y";
				previousDoc.CSI_LineNo = 1;
				wrapper = GetWrapper(entryLine);
				var previousDocument = wrapper.PreviousDocument;
				AssertEquals("For PRE previous documents code", "YSUM", previousDocument.Name);
				AssertEquals("For PRE previous documents reference", "Reference00001", previousDocument.Number);
				AssertEquals("For PRE previous documents line no", ZString.Empty, previousDocument.LineNumber);

				AssertSame("Cached PreviousDocument", wrapper.PreviousDocument, previousDocument);
			});
		}

		public void TestConsignor()
		{
			CombineAssertions(() =>
			{
				var orgHeader1 = createOrgHeader("NIF22222222", "bbbb@aaaa.com", "Address NIF22222222");
				var orgAddressOther1 = Factory.New<OrgAddress>();
				orgAddressOther1.OA_OH = orgHeader1.PK;
				orgAddressOther1.OA_Address1 = "Address Other 1";

				var orgHeader2 = createOrgHeader("NIF33333333", "cccc@dddd.com", "Address NIF33333333", "2");
				var orgAddressOther2 = Factory.New<OrgAddress>();
				orgAddressOther2.OA_OH = orgHeader2.PK;
				orgAddressOther2.OA_Address1 = "Address Other 2";

				declaration.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				invoice.JZ_OH_Supplier = ZGuid.Empty;
				invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
				var wrapper = GetWrapper(entryLine);
				AssertEquals("Expected when no consignor in declaration and no lines with consignor", null, wrapper.Consignor);

				invoice.JZ_OA_SupplierAddress = orgAddressOther1.PK;
				wrapper = GetWrapper(entryLine, shouldDeclareSupplierInLine: false);
				AssertEquals("Expected when no consignor in declaration and consignor in line", null, wrapper.Consignor);

				declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddressOther1.PK;
				invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
				wrapper = GetWrapper(entryLine);
				AssertEquals("Expected when consignor in declaration and no lines with consignor (Name)", orgHeader1.OH_FullName, wrapper.Consignor.Name);
				AssertEquals("Expected when consignor in declaration and no lines with consignor (Address)", orgAddressOther1.OA_Address1, wrapper.Consignor.Address);

				invoice.JZ_OA_SupplierAddress = orgAddressOther2.PK;
				wrapper = GetWrapper(entryLine, shouldDeclareSupplierInLine: false);
				AssertEquals("Expected when consignor in declaration and consignor in line are different", null, wrapper.Consignor);

				var entryLine2 = declaration.CustomsEntryHeaders[0].MergedLines[0];
				var invoice2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice2.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;

				invoice2.JZ_OA_SupplierAddress = orgAddressOther1.PK;
				var wrapper1 = GetWrapper(entryLine);
				AssertEquals("Expected when consignor different in declaration and two lines and equals consignors (line 1)(Name)", orgHeader2.OH_FullName, wrapper1.Consignor.Name);
				AssertEquals("Expected when consignor different in declaration and two lines and equals consignors (line 1)(Address)", orgAddressOther2.OA_Address1, wrapper1.Consignor.Address);
				var wrapper2 = GetWrapper(entryLine2);
				AssertEquals("Expected when consignor different in declaration and two lines and equals consignors (line 2)(Name)", orgHeader2.OH_FullName, wrapper2.Consignor.Name);
				AssertEquals("Expected when consignor different in declaration and two lines and equals consignors (line 2)(Address)", orgAddressOther2.OA_Address1, wrapper2.Consignor.Address);

				invoice.JZ_OH_Supplier = orgHeader2.PK;
				invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
				wrapper1 = GetWrapper(entryLine);
				AssertEquals("Expected when consignor different in declaration and two lines and equals consignors (line 1)(Name)", orgHeader2.OH_FullName, wrapper1.Consignor.Name);
				AssertEquals("Expected when consignor different in declaration and two lines and equals consignors (line 1)(Address Main)", "Address NIF33333333", wrapper1.Consignor.Address);
				wrapper2 = GetWrapper(entryLine2);
				AssertEquals("Expected when consignor different in declaration and two lines and equals consignors (line 2)(Name)", orgHeader2.OH_FullName, wrapper2.Consignor.Name);
				AssertEquals("Expected when consignor different in declaration and two lines and equals consignors (line 2)(Address Main)", "Address NIF33333333", wrapper2.Consignor.Address);

				invoice.JZ_OA_SupplierAddress = orgAddressOther1.PK;
				wrapper1 = GetWrapper(entryLine, shouldDeclareSupplierInLine: false);
				AssertEquals("Expected when consignor equals in declaration and two lines and equals consignors", null, wrapper1.Consignor);
			});
		}

		public void TestCommodityCode()
		{
			{
				invoiceLine.JI_Tariff = EntryLineData.Tariff;
				AssertEquals("Expected filled CommodityCode with 10 characters", EntryLineData.TariffShort, wrapper.CommodityCode);
				invoiceLine.JI_Tariff = EntryLineData.TariffShort;
				AssertEquals("Expected filled CommodityCode with 8 characters", EntryLineData.TariffShort, wrapper.CommodityCode);
			}
		}

		public void TestConsignee()
		{
			CombineAssertions(() =>
			{
				var orgHeader1 = createOrgHeader("NIF22222222", "bbbb@aaaa.com", "Address NIF22222222");
				var orgAddressOther1 = Factory.New<OrgAddress>();
				orgAddressOther1.OA_OH = orgHeader1.PK;
				orgAddressOther1.OA_Address1 = "Address Other 1";

				var orgHeader2 = createOrgHeader("NIF33333333", "cccc@dddd.com", "Address NIF33333333", "2");
				var orgAddressOther2 = Factory.New<OrgAddress>();
				orgAddressOther2.OA_OH = orgHeader2.PK;
				orgAddressOther2.OA_Address1 = "Address Other 2";

				declaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				invoice.JZ_OH_Buyer = ZGuid.Empty;
				invoice.JZ_OA_BuyerAddress = ZGuid.Empty;
				var wrapper = GetWrapper(entryLine);
				AssertEquals("Expected when no consignee in declaration and no lines with consignee", null, wrapper.Consignee);

				invoice.JZ_OA_BuyerAddress = orgAddressOther1.PK;
				wrapper = GetWrapper(entryLine, shouldDeclareImporterInLine: false);
				AssertEquals("Expected when no consignee in declaration and consignee in line", null, wrapper.Consignee);

				declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddressOther1.PK;
				invoice.JZ_OA_BuyerAddress = ZGuid.Empty;
				wrapper = GetWrapper(entryLine);
				AssertEquals("Expected when consignee in declaration and no lines with consignee (Name)", orgHeader1.OH_FullName, wrapper.Consignee.Name);
				AssertEquals("Expected when consignee in declaration and no lines with consignee (Address)", orgAddressOther1.OA_Address1, wrapper.Consignee.Address);

				invoice.JZ_OA_BuyerAddress = orgAddressOther2.PK;
				wrapper = GetWrapper(entryLine, shouldDeclareImporterInLine: false);
				AssertEquals("Expected when consignee in declaration and consignee in line are different", null, wrapper.Consignee);

				var entryLine2 = declaration.CustomsEntryHeaders[0].MergedLines[0];
				var invoice2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice2.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine2.PK;

				invoice2.JZ_OA_BuyerAddress = orgAddressOther1.PK;
				var wrapper1 = GetWrapper(entryLine);
				AssertEquals("Expected when consignee different in declaration and two lines and equals consignees (line 1)(Name)", orgHeader2.OH_FullName, wrapper1.Consignee.Name);
				AssertEquals("Expected when consignee different in declaration and two lines and equals consignees (line 1)(Address)", orgAddressOther2.OA_Address1, wrapper1.Consignee.Address);
				var wrapper2 = GetWrapper(entryLine2);
				AssertEquals("Expected when consignee different in declaration and two lines and equals consignees (line 2)(Name)", orgHeader2.OH_FullName, wrapper2.Consignee.Name);
				AssertEquals("Expected when consignee different in declaration and two lines and equals consignees (line 2)(Address)", orgAddressOther2.OA_Address1, wrapper2.Consignee.Address);

				invoice.JZ_OH_Buyer = orgHeader2.PK;
				invoice.JZ_OA_BuyerAddress = ZGuid.Empty;
				wrapper1 = GetWrapper(entryLine);
				AssertEquals("Expected when consignee different in declaration and two lines and equals consignees (line 1)(Name)", orgHeader2.OH_FullName, wrapper1.Consignee.Name);
				AssertEquals("Expected when consignee different in declaration and two lines and equals consignees (line 1)(Address Main)", "Address NIF33333333", wrapper1.Consignee.Address);
				wrapper2 = GetWrapper(entryLine2);
				AssertEquals("Expected when consignee different in declaration and two lines and equals consignees (line 2)(Name)", orgHeader2.OH_FullName, wrapper2.Consignee.Name);
				AssertEquals("Expected when consignee different in declaration and two lines and equals consignees (line 2)(Address Main)", "Address NIF33333333", wrapper2.Consignee.Address);

				invoice.JZ_OA_BuyerAddress = orgAddressOther1.PK;
				wrapper1 = GetWrapper(entryLine, shouldDeclareImporterInLine: false);
				AssertEquals("Expected when consignee equals in declaration and two lines and equals consignees", null, wrapper1.Consignee);
			});
		}

		public void TestContainers()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			foreach (var tag in ContainerTags)
			{
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = tag;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(tag).IsForInvoiceLine = true;
				invoiceLine2.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(tag).IsForInvoiceLine = true;
			}

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = GetWrapper(entryLine);
			var containers = wrapper.Containers;
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("Expected filled Containers", ContainerTags, containers.ToArray());
				AssertSame("Cached Containers", wrapper.Containers, containers);
			});
		}

		public void TestPackages()
		{
			var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
			packageInfo1.CW_PackType = InternalPackage1.Type;
			pack1.CHC_CW = packageInfo1.PK;
			invoiceLine.PackagesPivot.Add(pack1);

			var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
			packageInfo2.CW_PackType = InternalPackage2.Type;
			pack2.CHC_CW = packageInfo2.PK;
			invoiceLine.PackagesPivot.Add(pack2);

			var packages = wrapper.Packages;
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled Packages", 2, packages.Count);
				AssertSame("Cached Packages", wrapper.Packages, packages);
			});
		}

		public void TestCusCode()
		{
			{
				invoiceLine.ZG_CusNumber = "000258-5";
				AssertEquals("Expected filled CusCode", "000258-5", wrapper.CusCode);
			}
		}

		public void TestAdditionalActors()
		{
			CombineAssertions(() =>
			{
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var referenceEntryInstructionAA = entryInstruction.CusSupplyChainActorReferences.AddNew();
				referenceEntryInstructionAA.CFR_Code = "AA";
				referenceEntryInstructionAA.CFR_Reference = "REFAA";
				AssertEquals("When AdditionalActors in EntryInstruction with CFR_Code AA", 0, wrapper.AdditionalActors.Count);

				var referenceAA = invoiceLine.CusSupplyChainActorReferences.AddNew();
				referenceAA.CFR_Code = "AA";
				referenceAA.CFR_Reference = "REFAA";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalActors in InvoiceLine with CFR_Code AA", 0, wrapper.AdditionalActors.Count);

				var referenceMF = invoiceLine.CusSupplyChainActorReferences.AddNew();
				referenceMF.CFR_Code = "MF";
				referenceMF.CFR_Reference = "REFMF";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalActors in InvoiceLine with CFR_Code MF", 1, wrapper.AdditionalActors.Count);

				var referenceCS = invoiceLine.CusSupplyChainActorReferences.AddNew();
				referenceCS.CFR_Code = "CS";
				referenceCS.CFR_Reference = "REFCS";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalActors in InvoiceLine with CFR_Code MF and CS", 2, wrapper.AdditionalActors.Count);

				var referenceFW = invoiceLine.CusSupplyChainActorReferences.AddNew();
				referenceFW.CFR_Code = "FW";
				referenceFW.CFR_Reference = "REFFW";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalActors in InvoiceLine with CFR_Code MF, CS and FW", 3, wrapper.AdditionalActors.Count);

				var referenceWH = invoiceLine.CusSupplyChainActorReferences.AddNew();
				referenceWH.CFR_Code = "WH";
				referenceWH.CFR_Reference = "REFWH";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalActors in InvoiceLine with CFR_Code MF, CS, FW and WH", 4, wrapper.AdditionalActors.Count);

				wrapper = GetWrapper(entryLine, showActorInLine: false);
				AssertEquals("If change showActorInLine to false in LineWrapper is not sent", 0, wrapper.AdditionalActors.Count);

				invoiceLine.CusSupplyChainActorReferences.RemoveAndDeleteAll();
				wrapper = GetWrapper(entryLine);
				AssertEquals("If change RemoveAndDeleteAll in LineWrapper count = 0", 0, wrapper.AdditionalActors.Count);

				var referenceMF1 = invoiceLine.CusSupplyChainActorReferences.AddNew();
				referenceMF1.CFR_Code = "MF";
				referenceMF1.CFR_Reference = "REFMF";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalActors in InvoiceLine with CFR_Code MF", 1, wrapper.AdditionalActors.Count);

				var referenceEntryInstructionMF = entryInstruction.CusSupplyChainActorReferences.AddNew();
				referenceEntryInstructionMF.CFR_Code = "MF";
				referenceEntryInstructionMF.CFR_Reference = "ENTRYMF";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalActors in InvoiceLine and EntryInstruction with CFR_Code MF, only one", 1, wrapper.AdditionalActors.Count);

				var referenceEntryInstructionCS = entryInstruction.CusSupplyChainActorReferences.AddNew();
				referenceEntryInstructionCS.CFR_Code = "CS";
				referenceEntryInstructionCS.CFR_Reference = "ENTRYCS";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalActors only in EntryInstruction with CFR_Code CS", 2, wrapper.AdditionalActors.Count);
			});
		}

		public void TestAdditionalInfo()
		{
			CombineAssertions(() =>
			{
				AssertType<AdditionalInfoCollection>(invoiceLine.AdditionalInfos);

				var addInfo1 = invoice.AdditionalInfos.AddNew();
				addInfo1.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;
				addInfo1.CSI_Code = "AAA";
				addInfo1.CSI_ReferenceNumber = "AAAAAAA";

				var addInfo2 = invoiceLine.AdditionalInfos.AddNew();
				addInfo2.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;
				addInfo2.CSI_Code = "BBB";
				addInfo2.CSI_ReferenceNumber = "BBBBBBB";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalInfos in invoice and invoiceLine with subtype " + AdditionalDocList.Codes.AdditionalInformation, 2, wrapper.AdditionalInfo.Count);

				wrapper = GetWrapper(entryLine, showINFAddInfoInEntryLines: false);
				AssertEquals("When AdditionalInfo with showINFAddInfoInEntryLines = false", 0, wrapper.AdditionalInfo.Count);

				addInfo2.CSI_SubType = "AA";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalInfo in invoice has subtype INF and the one in invoiceLine has subtype AA", 1, wrapper.AdditionalInfo.Count);

				addInfo1.CSI_SubType = "TRA";
				wrapper = GetWrapper(entryLine);
				AssertEquals("When no AdditionalInfo (invoice or invoiceLine) has subtype INF", 0, wrapper.AdditionalInfo.Count);

				addInfo2.CSI_SubType = AdditionalDocList.Codes.AdditionalInformation;
				wrapper = GetWrapper(entryLine);
				AssertEquals("When AdditionalInfo in invoice has subtype TRA and the one in invoiceLine has subtype INF", 1, wrapper.AdditionalInfo.Count);
			});
		}

		public OrgHeader createOrgHeader(ZString nif, ZString email, ZString address, string extension = null)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = HeaderData.ImporterCode + extension;
			orgHeader.OH_FullName = OrgHeaderData.Name + extension;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, nif);
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			var orgAddress = orgHeader.MainAddress;

			orgAddress.OA_Email = email;
			orgAddress.Address1 = address;
			return orgHeader;
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

			wrapper = GetWrapper(entryLine);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		EXSLineWrapper wrapper;

		EXSLineWrapper GetWrapper(CusEntryLine cusEntryLine,
									bool shouldDeclareMopValueinLine = true,
									bool shouldDeclareImporterInLine = true,
									bool shouldDeclareSupplierInLine = true,
									bool showINFAddInfoInEntryLines = true,
									bool showActorInLine = true)
					=> new EXSLineWrapper(cusEntryLine,
											shouldDeclareMopValueinLine,
											shouldDeclareImporterInLine,
											shouldDeclareSupplierInLine,
											showINFAddInfoInEntryLines,
											showActorInLine);

		protected override EXSLineWrapper GetProvider() => wrapper;
	}
}
