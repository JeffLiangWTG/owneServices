using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE513And515CommonGoodsItemProviderTest : Customs.Business.Testing.DataProviderTestCase<IE513And515CommonGoodsItemProvider>
	{
		public void TestNatureOfTransaction()
		{
			instruction.CEI_SubStyle = "B";
			invoice.JZ_ValuationCode = NatureOfTransactionList.Codes._11;
			var provider = GetProvider();
			AssertEquals("MethodOfPayment", string.Empty, provider.NatureOfTransaction);

			instruction.CEI_SubStyle = "A";
			provider = GetProvider();
			AssertEquals("MethodOfPayment", "11", provider.NatureOfTransaction);
		}

		public void TestPreviousDocuments_IsTransitionPeriodAES30()
		{
			using (TemporarilySetAESTransitionPeriod(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine1 = entryHeader.MergedLines.AddNew();
				var entryLine2 = entryHeader.MergedLines.AddNew();

				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_CL = entryLine1.PK;

				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_CL = entryLine2.PK;

				var invoicePreviousDocument1 = invoice.PreviousDocuments.AddNew();
				invoicePreviousDocument1.CSI_Code = "C1";
				var invoicePreviousDocument2 = invoice.PreviousDocuments.AddNew();
				invoicePreviousDocument2.CSI_Code = "C2";
				var instructionPreviousDocument1 = entryInstruction.PreviousDocuments.AddNew();
				instructionPreviousDocument1.CSI_Code = "C2";
				var instructionPreviousDocument2 = entryInstruction.PreviousDocuments.AddNew();
				instructionPreviousDocument2.CSI_Code = "C3";

				var invoiceLine1PreviousDocument1 = invoiceLine1.PreviousDocuments.AddNew();
				invoiceLine1PreviousDocument1.CSI_Code = "C3";
				var invoiceLine1PreviousDocument2 = invoiceLine1.PreviousDocuments.AddNew();
				invoiceLine1PreviousDocument2.CSI_Code = "C4";

				var invoiceLine2PreviousDocument1 = invoiceLine2.PreviousDocuments.AddNew();
				invoiceLine2PreviousDocument1.CSI_Code = "C4";
				var invoiceLine2PreviousDocument2 = invoiceLine2.PreviousDocuments.AddNew();
				invoiceLine2PreviousDocument2.CSI_Code = "C5";

				var provider = new IE515MessageProvider(entryHeader);
				CombineAssertions(() =>
				{
					AssertEquals("Count", 0, provider.PreviousDocuments.Count);
					AssertEquals("Count", 2, provider.GoodsItems.Count);
					var goodsItems = provider.GoodsItems.ToArray();
					AssertEquals("Count", 4, goodsItems[0].PreviousDocuments.Count);
					AssertArrayEqualsByElements(new[] { "C1", "C2", "C3", "C4" }, goodsItems[0].PreviousDocuments.Cast<PreviousDocumentLineProvider>().Select(p => p.Type).OrderBy(p => p).ToArray());
					AssertEquals("Count", 5, goodsItems[1].PreviousDocuments.Count);
					AssertArrayEqualsByElements(new[] { "C1", "C2", "C3", "C4", "C5" }, goodsItems[1].PreviousDocuments.Cast<PreviousDocumentLineProvider>().Select(p => p.Type).OrderBy(p => p).ToArray());
				});
			}
		}

		public void TestPreviousDocuments()
		{
			var doc1 = invoiceLine.PreviousDocuments.AddNew();
			doc1.CSI_Code = "PD1";
			doc1.CSI_ReferenceNumber = "DOC001";

			var pd1 = Provider.PreviousDocuments.FirstOrDefault();
			AssertType<PreviousDocumentLineProvider>("Type of PreviousDocuments", pd1);
		}

		public void TestSupportingDocuments()
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: false, codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "SD1");

			var doc1 = invoiceLine.SupportingDocuments.AddNew();
			doc1.CSI_Code = "SD1";
			doc1.CSI_ReferenceNumber = "DOC002";

			var sd1 = Provider.SupportingDocuments.FirstOrDefault();
			AssertType<SupportingDocumentLineProvider>("Type of PreviousDocuments", sd1);
		}

		public void TestSupportingDocuments_IsTransitionPeriodAES30()
		{
			using (TemporarilySetAESTransitionPeriod(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
				var supportingDocument1 = entryHeaderWrapper.RandomInvoiceHeader.SupportingDocuments.AddNew();
				supportingDocument1.CSI_Code = "SD1";
				supportingDocument1.CSI_ReferenceNumber = "SD001";

				MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: false, codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "SD1");

				var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
				supportingDocument2.CSI_Code = "SD2";
				supportingDocument2.CSI_ReferenceNumber = "SD002";

				var provider = new IE513And515CommonGoodsItemProvider(entryLine, entryHeaderWrapper, instruction.IsSubStyle_B_C_E_F, false);

				var supportingDocuments = provider.SupportingDocuments.OrderBy(x => x.Type).ToArray();
				AssertEquals("Count", 2, supportingDocuments.Length);
				AssertEquals("Type", "SD1", supportingDocuments[0].Type);
				AssertEquals("Reference", "SD001", supportingDocuments[0].Reference);
				AssertEquals("Type", "SD2", supportingDocuments[1].Type);
				AssertEquals("Reference", "SD002", supportingDocuments[1].Reference);
			}
		}

		public void TestAdditionalReferences()
		{
			var ar1 = invoiceLine.AdditionalInfos.AddNew();
			ar1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ar1.CSI_Code = "AR1";
			ar1.CSI_ReferenceNumber = "AR001";

			var ar1Result = Provider.AdditionalReferences.FirstOrDefault();
			AssertType<AdditionalReferenceProvider>("Type of AdditionalReferences", ar1Result);
			AssertEquals("Type", "AR1", ar1Result.Type);
			AssertEquals("Reference", "AR001", ar1Result.Reference);
		}

		public void TestAdditionalReferences_IsTransitionPeriodAES30()
		{
			using (TemporarilySetAESTransitionPeriod(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				entryLine.CL_LineNumber = 1;
				invoiceLine.JI_CL = entryLine.PK;
				var entryLine2 = entryHeader.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_CL = entryLine2.PK;
				var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);

				MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, codes: new[] { "9001", "9002" });

				var additionalInfo1 = invoice.AdditionalInfos.AddNew();
				additionalInfo1.CSI_Code = "9001";
				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				var additionalInfo2 = invoice.AdditionalInfos.AddNew();
				additionalInfo2.CSI_Code = "9002";
				additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				var ar1D23 = invoice.AdditionalInfos.AddNew();
				ar1D23.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				ar1D23.CSI_Code = "1D23";
				ar1D23.CSI_ReferenceNumber = "AR123";

				var additionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
				additionalInfo3.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo3.CSI_Code = "9002";
				var additionalInfo4 = invoiceLine.AdditionalInfos.AddNew();
				additionalInfo4.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo4.CSI_Code = "9003";

				var additionalInfo5 = invoiceLine2.AdditionalInfos.AddNew();
				additionalInfo5.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				additionalInfo5.CSI_Code = "9005";

				var provider = new IE513And515CommonGoodsItemProvider(entryLine, entryHeaderWrapper, entryInstruction.IsSubStyle_B_C_E_F, false);
				var additionalReferences = provider.AdditionalReferences.ToList();
				AssertEquals("Count", 4, additionalReferences.Count);
				AssertEquals("AdditionalInfo 1", "9001", additionalReferences[0].Type);
				AssertEquals("AdditionalInfo 2", "9002", additionalReferences[1].Type);
				AssertEquals("AdditionalInfo 3", "1D23", additionalReferences[2].Type);
				AssertEquals("AdditionalInfo 4", "9003", additionalReferences[3].Type);
				provider = new IE513And515CommonGoodsItemProvider(entryLine2, entryHeaderWrapper, entryInstruction.IsSubStyle_B_C_E_F, false);
				additionalReferences = provider.AdditionalReferences.ToList();
				AssertEquals("Only first line should include 1D23", 3, additionalReferences.Count);
				AssertEquals("AdditionalInfo 1", "9001", additionalReferences[0].Type);
				AssertEquals("AdditionalInfo 2", "9002", additionalReferences[1].Type);
				AssertEquals("AdditionalInfo 5", "9005", additionalReferences[2].Type);
			}
		}

		public void TestAdditionalInformations()
		{
			var ai1 = invoiceLine.AdditionalInfos.AddNew();
			ai1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			ai1.CSI_Code = "AR1";
			ai1.CSI_ReferenceNumber = "AR001";

			var ai1Result = Provider.AdditionalInformations.FirstOrDefault();
			AssertType<AdditionalInformationProvider>("Type of AdditionalInformations", ai1Result);
		}

		public void TestAdditionalInformations_IsTransitionPeriodAES30()
		{
			using (TemporarilySetAESTransitionPeriod(true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);

				MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, new[] { ("9001", "9001 DES"), ("9002", "9002 DES") });

				var additionalInfo1 = invoice.AdditionalInfos.AddNew();
				additionalInfo1.CSI_Code = "9001";
				additionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				var additionalInfo2 = invoice.AdditionalInfos.AddNew();
				additionalInfo2.CSI_Code = "9002";
				additionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

				var additionalInfo3 = invoiceLine.AdditionalInfos.AddNew();
				additionalInfo3.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo3.CSI_Code = "9003";

				var additionalInfo4 = invoiceLine.AdditionalInfos.AddNew();
				additionalInfo4.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo4.CSI_Code = "9002";

				var provider = new IE513And515CommonGoodsItemProvider(entryLine, entryHeaderWrapper, instruction.IsSubStyle_B_C_E_F, true);

				var additionalInformations = provider.AdditionalInformations.OrderBy(x => x.Code).ToArray();
				AssertEquals("Count", 3, additionalInformations.Length);
				AssertEquals("AdditionalInfo 1", "9001", additionalInformations[0].Code);
				AssertEquals("AdditionalInfo 2", "9002", additionalInformations[1].Code);
				AssertEquals("AdditionalInfo 3", "9003", additionalInformations[2].Code);
			}
		}

		public void TestAdditionalSupplyChainActors()
		{
			var ca1 = invoiceLine.CusSupplyChainActorReferences.AddNew();
			ca1.CFR_Code = "CA1";
			ca1.CFR_Reference = "CA001";

			AssertType<AdditionalSupplyChainActorProvider>(Provider.AdditionalSupplyChainActors.FirstOrDefault());
		}

		public void TestStatisticalValue()
		{
			entry.EntryInstruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			entryLine.CL_StatisticalValue = 20m;
			AssertEquals("StatisticalValue", 20m, Provider.StatisticalValue);
		}

		public void TestCountryOfExport()
		{
			invoiceLine.JI_RN_NKCountryOfExport = "US";
			AssertEquals("CountryOfExport", "US", Provider.CountryOfExport);
		}

		public void TestCountryOfDestination()
		{
			invoiceLine.ZG_CountryOfDestination = Core.Constants.CountryCodes.China;
			AssertEquals("CountryOfDestination", Core.Constants.CountryCodes.China, Provider.CountryOfDestination);
		}

		public void TestProcedure()
		{
			AssertType<ProcedureProvider>("Type of Procedure.", Provider.Procedure);
		}

		public void TestOrigin()
		{
			AssertType<OriginProvider>("Type of Origin.", Provider.Origin);
		}

		public void TestTransportDocuments()
		{
			var ref0 = instruction.AdditionalInfos.AddNew();
			ref0.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref0.CSI_Code = "TD0";
			ref0.CSI_ReferenceNumber = "REF1";
			ref0.CSI_Description = "Description0";
			var ref1 = invoice.AdditionalInfos.AddNew();
			ref1.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref1.CSI_Code = "TD1";
			ref1.CSI_ReferenceNumber = "REF1";
			ref1.CSI_Description = "Description1";
			var ref2 = invoiceLine.AdditionalInfos.AddNew();
			ref2.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref2.CSI_Code = "TD2";
			ref2.CSI_ReferenceNumber = "REF1";
			ref2.CSI_Description = "Description2";
			var ref3 = invoiceLine.AdditionalInfos.AddNew();
			ref3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref3.CSI_Code = "TD4";
			ref3.CSI_ReferenceNumber = "REF1";
			ref3.CSI_Description = "Description4";
			var ref4 = invoiceLine.AdditionalInfos.AddNew();
			ref4.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref4.CSI_Code = "TD2";
			ref4.CSI_ReferenceNumber = "REF1";
			ref4.CSI_Description = "Description5";

			var ref5 = invoice.AdditionalInfos.AddNew();
			ref5.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref5.CSI_Code = "TD4";
			ref5.CSI_ReferenceNumber = "REF1";
			ref5.CSI_Description = "Description4";
			var ref6 = invoiceLine.AdditionalInfos.AddNew();
			ref6.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref6.CSI_Code = "TD5";
			ref6.CSI_ReferenceNumber = "REF1";
			ref6.CSI_Description = "Description2";

			AssertContainsExactElementsInAnyOrder("After Transition Period, only invoice line transport documents are included on goods item", new[] { "TD2|REF1", "TD4|REF1", "TD5|REF1" }, Provider.TransportDocuments.Select(x => x.Type + "|" + x.Reference));

			using (TemporarilySetAESTransitionPeriod(true))
			{
				AssertContainsExactElementsInAnyOrder("During Transition Period, all transport documents are included on goods item", new[] { "TD0|REF1", "TD1|REF1", "TD2|REF1", "TD4|REF1", "TD5|REF1" }, GetProvider().TransportDocuments.Select(x => x.Type + "|" + x.Reference));
			}
		}

		public void TestGoodsItemNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals("LineNumber", (short)2, Provider.GoodsItemNumber);
		}

		public void TestConsignor()
		{
			var orgOnJob = Factory.New<OrgHeader>();
			orgOnJob.OH_FullName = "Consignor on Job";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgOnJob.MainAddress.PK;
			AssertEquals("Should populate Consignor from Job.", "Consignor on Job", GetProvider().Consignor.Name);

			var orgOnHeader = Factory.New<OrgHeader>();
			orgOnHeader.OH_FullName = "Consignor on Header";
			invoice.JZ_OA_SupplierAddress = orgOnHeader.MainAddress.PK;
			AssertEquals("Should populate Consignor from Header.", "Consignor on Header", GetProvider().Consignor.Name);

			var orgOnLine = Factory.New<OrgHeader>();
			orgOnLine.OH_FullName = "Consignor on Line";
			invoiceLine.JI_OA_ExporterAddress = orgOnLine.MainAddress.PK;
			AssertEquals("Should populate Consignor from Line.", "Consignor on Line", GetProvider().Consignor.Name);
		}

		public void TestConsignee()
		{
			var orgOnJob = Factory.New<OrgHeader>();
			orgOnJob.OH_FullName = "Consignee on Job";
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgOnJob.MainAddress.PK;
			AssertEquals("Should populate Consignee from Job.", "Consignee on Job", GetProvider().Consignee.Name);

			var orgOnHeader = Factory.New<OrgHeader>();
			orgOnHeader.OH_FullName = "Consignee on Header";
			invoice.JZ_OA_BuyerAddress = orgOnHeader.MainAddress.PK;
			AssertEquals("Should populate Consignee from Header.", "Consignee on Header", GetProvider().Consignee.Name);

			var orgOnLine = Factory.New<OrgHeader>();
			orgOnLine.OH_FullName = "Consignee on Line";
			invoiceLine.JI_OA_ConsigneeAddress = orgOnLine.MainAddress.PK;
			AssertEquals("Should populate Consignee from Line.", "Consignee on Line", GetProvider().Consignee.Name);
		}

		public void TestReferenceNumberUCR()
		{
			declaration.JE_UCR = "UCR";
			invoice.JZ_UCR = "";
			var provider = GetProvider();
			AssertEquals("ReferenceNumberUCR", string.Empty, provider.ReferenceNumberUCR);

			invoice.JZ_UCR = "UCR2";
			provider = GetProvider();
			AssertEquals("ReferenceNumberUCR", "UCR2", provider.ReferenceNumberUCR);

			declaration.ZG_SpecificCircumstanceIndicator = "A20";
			provider = GetProvider();
			AssertEquals("ReferenceNumberUCR", string.Empty, provider.ReferenceNumberUCR);
		}

		public void TestMethodOfPayment()
		{
			instruction.CEI_SubStyle = "A";
			invoice.ZG_TransportChargesMethodOfPayment = "A";
			var provider = GetProvider();
			AssertEquals("Transport Charges MoP", "A", provider.MethodOfPayment);

			instruction.CEI_SubStyle = "B";
			provider = GetProvider();
			AssertEquals("Transport Charges MoP not included", string.Empty, provider.MethodOfPayment);
		}

		public void TestCommodity()
		{
			AssertType<CommodityTypeWithSupplementaryUnitsProvider>("Expected type of Commodity", Provider.Commodity);
		}

		public void TestCommodity_GrossMassAndNetMassOnlyOnMainPackEntryLine()
		{
			var (secondEntryLineWrapper, secondInvoiceLine) = MessageProviderTestHelper.SetupSecondLine(entryHeaderWrapper);
			entryLine.CL_LineNumber = 1;
			var secondEntryLine = secondEntryLineWrapper.EntryLine;
			secondEntryLine.CL_LineNumber = 2;
			declaration.JE_TotalNoOfPacks = 1;
			var packingGroup = declaration.PackingGroups[0];
			packingGroup.Packages.RemoveAndDeleteAll();
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			secondInvoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			invoiceLine.JI_Weight = 40m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = 35m;
			invoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			secondInvoiceLine.JI_Weight = 60m;
			secondInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			secondInvoiceLine.JI_CustomsQuantity = 55m;
			secondInvoiceLine.JI_CustomsUnitQty = Core.Constants.Weight.Kilograms;
			invoiceLine.ZG_IsMainPack = true;
			secondInvoiceLine.ZG_IsMainPack = false;
			var provider1 = new IE513And515CommonGoodsItemProvider(entryLine, entryHeaderWrapper, false, true);
			var provider2 = new IE513And515CommonGoodsItemProvider(secondEntryLine, entryHeaderWrapper, false, true);
			CombineAssertions(() =>
			{
				AssertGrossMassAndNetMass(provider1.Commodity, 100m, 35m);
				AssertGrossMassAndNetMass(provider2.Commodity, 0m, 55m);
			});
		}

		void AssertGrossMassAndNetMass(ICommodityTypeWithGrossNetMassAndTaxes commodity, decimal grossMass, decimal netMass)
		{
			AssertEquals("GrossMass", grossMass, commodity.GrossMass);
			AssertEquals("NetMass", netMass, commodity.NetMass);
		}

		public void TestPackages()
		{
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			AssertType<PackagingProvider>("Type of elements in Packages", Provider.Packages.First());
		}

		public void TestPackages_NumberOnlyOnFirstEntryLine()
		{
			var (secondEntryLineWrapper, secondInvoiceLine) = MessageProviderTestHelper.SetupSecondLine(entryHeaderWrapper);
			entryLine.CL_LineNumber = 1;
			var secondEntryLine = secondEntryLineWrapper.EntryLine;
			secondEntryLine.CL_LineNumber = 2;
			declaration.JE_TotalNoOfPacks = 1;
			var packingGroup = declaration.PackingGroups[0];
			packingGroup.Packages.RemoveAndDeleteAll();
			var package1 = declaration.Packages.AddNew();
			package1.CW_PackType = "1A";
			package1.CW_PackQty = 1;
			var package2 = declaration.Packages.AddNew();
			package2.CW_PackType = "2A";
			package2.CW_PackQty = 2;
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			secondInvoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			var provider1 = new IE513And515CommonGoodsItemProvider(entryLine, entryHeaderWrapper, false, true);
			var provider2 = new IE513And515CommonGoodsItemProvider(secondEntryLine, entryHeaderWrapper, false, true);
			var packages = provider1.Packages.ToArray();
			AssertEquals("Line 1 - Length", 2, packages.Length);
			AssertEquals("Line 1 - Pack 1 - PackageQuantity", 1, packages[0].PackageQuantity);
			AssertEquals("Line 1 - Pack 2 - PackageQuantity", 2, packages[1].PackageQuantity);
			packages = provider2.Packages.ToArray();
			AssertEquals("Line 2 - Length", 2, packages.Length);
			AssertEquals("Line 2 - Pack 1 - PackageQuantity", 0, packages[0].PackageQuantity);
			AssertEquals("Line 2 - Pack 2 - PackageQuantity", 0, packages[1].PackageQuantity);
		}

		public void TestAuthorisations()
		{
			var cusAuthorizationUsage1 = invoiceLine.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage1.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			var cusAuthorizationUsage2 = invoiceLine.CusAuthorizationUsages.AddNew();
			cusAuthorizationUsage2.AGC_Code = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
			AssertContainsExactElementsInAnyOrder("Authorisations", new[] { "CUSDE", "CUCCL" }, Provider.Authorisations.Select(x => x.IdentificationType));
		}

		protected override IE513And515CommonGoodsItemProvider GetProvider() => new IE513And515CommonGoodsItemProvider(entryLine, entryHeaderWrapper, instruction.IsSubStyle_B_C_E_F, true);

		protected override void SetUp()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "SDE", "CUSDE", startDate, endDate, "EUN");
			helper.CreateCusMap("EUNAU", "CCL", "CUCCL", startDate, endDate, "EUN");
			Factory.Save();

			base.SetUp();
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			entryHeaderWrapper = testBizObjs.entryHeaderWrapper;
			declaration = entryHeaderWrapper.Declaration;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_TotalNoOfPacks = 1;
			instruction = entryHeaderWrapper.Instruction;
			invoice = entryHeaderWrapper.RandomInvoiceHeader;
			var entryLineWrapper = testBizObjs.entryLineWrapper;
			invoiceLine = entryLineWrapper.RandomInvoiceLine;

			entry = entryHeaderWrapper.EntryHeader;
			entryLine = entryLineWrapper.EntryLine;
		}
		EntryHeaderWrapper entryHeaderWrapper;
		JobDeclaration declaration;
		CusEntryInstruction instruction;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entry;
		CusEntryLine entryLine;

		System.IDisposable TemporarilySetAESTransitionPeriod(bool isTransitionPeriod) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
			Universal.Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Ireland, ZDate.Today, isTransitionPeriod);
	}
}
