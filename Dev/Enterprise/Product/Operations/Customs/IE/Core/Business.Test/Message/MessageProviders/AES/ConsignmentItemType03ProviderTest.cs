using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class ConsignmentItemType03ProviderTest : DataProviderTestCase<ConsignmentItemType03Provider>
	{
		public void TestGoodsItemNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals("GoodsItemNumber", (short)2, Provider.GoodsItemNumber);
		}

		public void TestConsignor()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "ORG NAME";
			var address = Factory.New<OrgAddress>();
			invoiceLine.JI_OA_ExporterAddress = address.PK;
			address.OA_OH = org.PK;
			AssertEquals("Consignor", "ORG NAME", Provider.Consignor.Name);
		}

		public void TestConsignee()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "ORG NAME";
			var address = Factory.New<OrgAddress>();
			invoiceLine.JI_OA_ConsigneeAddress = address.PK;
			address.OA_OH = org.PK;
			AssertEquals("Consignee", "ORG NAME", Provider.Consignee.Name);
		}

		public void TestAdditionalSupplyChainActors()
		{
			invoiceLine.CusSupplyChainActorReferences.AddNew();
			AssertType<AdditionalSupplyChainActorProvider>("AdditionalSupplyChainActors", Provider.AdditionalSupplyChainActors.First());
		}

		public void TestCommodity()
		{
			AssertType<CommodityTypeWithGrossMassProvider>("Expected type of Commodity", Provider.Commodity);
		}

		public void TestCommodity_GrossMassOnlyOnMainPackEntryLine()
		{
			(var secondEntryLineWrapper, var secondInvoiceLine) = MessageProviderTestHelper.SetupSecondLine(entryHeaderWrapper);
			entryLine.CL_LineNumber = 1;
			var secondEntryLine = secondEntryLineWrapper.EntryLine;
			secondEntryLine.CL_LineNumber = 2;
			var packingGroup = declaration.PackingGroups[0];
			packingGroup.Packages.RemoveAndDeleteAll();
			var package1 = declaration.Packages.AddNew();
			var package2 = declaration.Packages.AddNew();
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			secondInvoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			invoiceLine.JI_Weight = 40m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 35m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			secondInvoiceLine.JI_Weight = 0.060m;
			secondInvoiceLine.JI_WeightUQ = Core.Constants.Weight.Tonnes;
			secondInvoiceLine.JI_NetWeight = 55m;
			secondInvoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.ZG_IsMainPack = ZBool.True;
			secondInvoiceLine.ZG_IsMainPack = ZBool.False;
			var provider1 = new IE513And515CommonGoodsItemProvider(entryLine, entryHeaderWrapper, false, true);
			var provider2 = new IE513And515CommonGoodsItemProvider(secondEntryLine, entryHeaderWrapper, false, true);
			AssertEquals("1 - GrossMass", 100m, provider1.Commodity.GrossMass);
			AssertEquals("2 - GrossMass", 0m, provider2.Commodity.GrossMass);
		}

		public void TestPackages()
		{
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<InvoiceLineCusLinkPackage>().ForEach(link => link.IsLinked = true);
			AssertType<PackagingProvider>("Type of elements in Packages", Provider.Packages.First());

			invoice.JobDeclaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicatorForUCCList.Codes.A20;
			AssertEquals("Empty Packages for A20.", false, GetProvider().Packages.Any());
		}

		public void TestPackages_NumberOnlyOnFirstEntryLine()
		{
			(var secondEntryLineWrapper, var secondInvoiceLine) = MessageProviderTestHelper.SetupSecondLine(entryHeaderWrapper);
			entryLine.CL_LineNumber = 1;
			var secondEntryLine = secondEntryLineWrapper.EntryLine;
			secondEntryLine.CL_LineNumber = 2;
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
			var provider1 = new ConsignmentItemType03Provider(entryLine, entryHeaderWrapper);
			var provider2 = new ConsignmentItemType03Provider(secondEntryLine, entryHeaderWrapper);
			var packages = provider1.Packages.ToArray();
			AssertEquals("Line 1 - Length", 2, packages.Length);
			AssertEquals("Line 1 - Pack 1 - PackageQuantity", 1, packages[0].PackageQuantity);
			AssertEquals("Line 1 - Pack 2 - PackageQuantity", 2, packages[1].PackageQuantity);
			packages = provider2.Packages.ToArray();
			AssertEquals("Line 2 - Length", 2, packages.Length);
			AssertEquals("Line 2 - Pack 1 - PackageQuantity", 0, packages[0].PackageQuantity);
			AssertEquals("Line 2 - Pack 2 - PackageQuantity", 0, packages[1].PackageQuantity);
		}

		public void TestAdditionalReferences()
		{
			var info1 = invoiceLine.AdditionalInfos.AddNew();
			info1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			info1.CSI_Code = "AR1";
			info1.CSI_ReferenceNumber = "AR001";

			var info2 = invoiceLine.AdditionalInfos.AddNew();
			info2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			info2.CSI_Code = "AI1";
			info2.CSI_Description = "AI001";

			AssertEquals("Count", 1, Provider.AdditionalReferences.Count);
			var ar1 = Provider.AdditionalReferences.First();
			AssertType<AdditionalReferenceProvider>("Type of AdditionalReference", ar1);
			AssertEquals("Type", "AR1", ar1.Type);
			AssertEquals("Reference", "AR001", ar1.Reference);
		}

		public void TestAdditionalReferences_IsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine1 = entryHeader.MergedLines.AddNew();
				entryLine1.CL_LineNumber = 1;
				var entryLine2 = entryHeader.MergedLines.AddNew();
				entryLine2.CL_LineNumber = 2;

				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_CL = entryLine1.PK;

				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_CL = entryLine2.PK;

				var ar1 = invoice.AdditionalInfos.AddNew();
				ar1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				ar1.CSI_Code = "AR1";
				ar1.CSI_ReferenceNumber = "AR001";
				var ar4 = invoice.AdditionalInfos.AddNew();
				ar4.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				ar4.CSI_Code = "AR3";
				ar4.CSI_ReferenceNumber = "AR003";
				var ar1D23 = invoice.AdditionalInfos.AddNew();
				ar1D23.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				ar1D23.CSI_Code = "1D23";
				ar1D23.CSI_ReferenceNumber = "AR123";

				var ar2 = invoiceLine1.AdditionalInfos.AddNew();
				ar2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				ar2.CSI_Code = "AR2";
				ar2.CSI_ReferenceNumber = "AR002";
				var ar5 = invoiceLine1.AdditionalInfos.AddNew();
				ar5.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				ar5.CSI_Code = "AR3";
				ar5.CSI_ReferenceNumber = "AR003";

				var ar3 = invoiceLine2.AdditionalInfos.AddNew();
				ar3.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				ar3.CSI_Code = "AR3";
				ar3.CSI_ReferenceNumber = "AR003";
				var ar6 = invoiceLine2.AdditionalInfos.AddNew();
				ar6.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				ar6.CSI_Code = "AR4";
				ar6.CSI_ReferenceNumber = "AR004";

				var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
				var provider = new ConsignmentItemType03Provider(entryLine1, entryHeaderWrapper);

				AssertEquals("Count", 4, provider.AdditionalReferences.Count);
				var additionalReferences = provider.AdditionalReferences.ToArray();
				AssertEquals("Code", "AR1", additionalReferences[0].Type);
				AssertEquals("Text", "AR001", additionalReferences[0].Reference);
				AssertEquals("Code", "AR3", additionalReferences[1].Type);
				AssertEquals("Text", "AR003", additionalReferences[1].Reference);
				AssertEquals("Code", "1D23", additionalReferences[2].Type);
				AssertEquals("Text", "AR123", additionalReferences[2].Reference);
				AssertEquals("Code", "AR2", additionalReferences[3].Type);
				AssertEquals("Text", "AR002", additionalReferences[3].Reference);

				provider = new ConsignmentItemType03Provider(entryLine2, entryHeaderWrapper);
				AssertEquals("Only first line should include 1D23", 3, provider.AdditionalReferences.Count);
				additionalReferences = provider.AdditionalReferences.ToArray();
				AssertEquals("Code", "AR1", additionalReferences[0].Type);
				AssertEquals("Text", "AR001", additionalReferences[0].Reference);
				AssertEquals("Code", "AR3", additionalReferences[1].Type);
				AssertEquals("Text", "AR003", additionalReferences[1].Reference);
				AssertEquals("Code", "AR4", additionalReferences[2].Type);
				AssertEquals("Text", "AR004", additionalReferences[2].Reference);
			}
		}

		public void TestAdditionalInformations()
		{
			var info1 = invoiceLine.AdditionalInfos.AddNew();
			info1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			info1.CSI_Code = "AR1";
			info1.CSI_Description = "AR001";

			var info2 = invoiceLine.AdditionalInfos.AddNew();
			info2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			info2.CSI_Code = "AI1";
			info2.CSI_Description = "AI001";

			AssertEquals("Count", 1, Provider.AdditionalInformations.Count);
			var ai1 = Provider.AdditionalInformations.First();
			AssertType<AdditionalInformationProvider>("Type of AdditionalInformation", ai1);
			AssertEquals("Code", "AI1", ai1.Code);
			AssertEquals("Text", "AI001", ai1.Text);
		}

		public void TestAdditionalInformations_IsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine1 = entryHeader.MergedLines.AddNew();

				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_CL = entryLine1.PK;

				var ai1 = invoice.AdditionalInfos.AddNew();
				ai1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				ai1.CSI_Code = "AI1";
				ai1.CSI_Description = "AI001";
				var ai3 = invoice.AdditionalInfos.AddNew();
				ai3.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				ai3.CSI_Code = "AI3";
				ai3.CSI_Description = "AI003";

				var ai2 = invoiceLine1.AdditionalInfos.AddNew();
				ai2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				ai2.CSI_Code = "AI2";
				ai2.CSI_Description = "AI002";
				var ai4 = invoiceLine1.AdditionalInfos.AddNew();
				ai4.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				ai4.CSI_Code = "AI3";
				ai4.CSI_Description = "AI003";

				var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
				var provider = new ConsignmentItemType03Provider(entryLine1, entryHeaderWrapper);

				var additionalInformations = provider.AdditionalInformations.OrderBy(x => x.Code).ToArray();
				AssertEquals("Count", 3, additionalInformations.Length);
				AssertEquals("Code", "AI1", additionalInformations[0].Code);
				AssertEquals("Text", "AI001", additionalInformations[0].Text);
				AssertEquals("Code", "AI2", additionalInformations[1].Code);
				AssertEquals("Text", "AI002", additionalInformations[1].Text);
				AssertEquals("Code", "AI3", additionalInformations[2].Code);
				AssertEquals("Text", "AI003", additionalInformations[2].Text);
			}
		}

		public void TestPreviousDocuments()
		{
			var doc1 = invoiceLine.PreviousDocuments.AddNew();
			doc1.CSI_Code = "PD1";
			doc1.CSI_ReferenceNumber = "DOC001";

			var pd1 = Provider.PreviousDocuments.FirstOrDefault();
			AssertType<PreviousDocumentProvider>("Type of PreviousDocument", pd1);
			AssertEquals("Type", "PD1", pd1.Type);
			AssertEquals("Reference", "DOC001", pd1.Reference);
		}

		public void TestPreviousDocuments_IsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
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

				var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
				var provider = new IE613And615ConsignmentProvider(entryHeaderWrapper);

				CombineAssertions(() =>
				{
					AssertEquals("Count", 0, provider.PreviousDocuments.Count);
					AssertEquals("Count", 2, provider.ConsignmentItem.Count);
					var consignmentItems = provider.ConsignmentItem.ToArray();
					AssertEquals("Count", 4, consignmentItems[0].PreviousDocuments.Count);
					AssertArrayEqualsByElements(new[] { "C1", "C2", "C3", "C4" }, consignmentItems[0].PreviousDocuments.Cast<PreviousDocumentProvider>().Select(p => p.Type).OrderBy(p => p).ToArray());
					AssertEquals("Count", 5, consignmentItems[1].PreviousDocuments.Count);
					AssertArrayEqualsByElements(new[] { "C1", "C2", "C3", "C4", "C5" }, consignmentItems[1].PreviousDocuments.Cast<PreviousDocumentProvider>().Select(p => p.Type).OrderBy(p => p).ToArray());
				});
			}
		}

		public void TestSupportingDocuments()
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: false, codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection, "SD1");

			var doc1 = invoiceLine.SupportingDocuments.AddNew();
			doc1.CSI_Code = "SD1";
			doc1.CSI_ReferenceNumber = "DOC002";

			var sd1 = Provider.SupportingDocuments.FirstOrDefault();
			AssertType<SupportingDocumentProvider>("Type of SupportingDocument", sd1);
			AssertEquals("Type", "SD1", sd1.Type);
			AssertEquals("Reference", "DOC002", sd1.Reference);

			invoice.JobDeclaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicatorForUCCList.Codes.A20;
			AssertEquals("Empty SupportingDocuments for A20.", false, GetProvider().SupportingDocuments.Any());
		}

		public void TestSupportingDocuments_IsTransitionPeriodAES30()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						Core.Constants.CountryCodes.Ireland, ZDate.Today, true))
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

				var provider = new ConsignmentItemType03Provider(entryLine, entryHeaderWrapper);

				var supportingDocuments = provider.SupportingDocuments.OrderBy(x => x.Type).ToArray();
				AssertEquals("Count", 2, supportingDocuments.Length);
				AssertEquals("Type", "SD1", supportingDocuments[0].Type);
				AssertEquals("Reference", "SD001", supportingDocuments[0].Reference);
				AssertEquals("Type", "SD2", supportingDocuments[1].Type);
				AssertEquals("Reference", "SD002", supportingDocuments[1].Reference);
			}
		}

		public void TestReferenceNumberUCR()
		{
			invoice.JZ_UCR = "UCR001";
			AssertEquals("ReferenceNumberUCR", "UCR001", Provider.ReferenceNumberUCR);
		}

		public void TestTransportMOP()
		{
			invoice.ZG_TransportChargesMethodOfPayment = "A";
			AssertEquals("TransportMOP", "A", Provider.TransportMOP);
		}

		protected override ConsignmentItemType03Provider GetProvider() => new ConsignmentItemType03Provider(entryLine, entryHeaderWrapper);

		protected override void SetUp()
		{
			base.SetUp();
			(entryHeaderWrapper, entryLineWrapper) = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			declaration = entryHeaderWrapper.Declaration;
			declaration.JE_MasterBill = "MB1";
			declaration.JE_TotalNoOfPacks = 1;
			invoice = entryLineWrapper.RandomInvoiceHeader;
			invoiceLine = entryLineWrapper.RandomInvoiceLine;
			entryLine = entryLineWrapper.EntryLine;
		}
		EntryHeaderWrapper entryHeaderWrapper;
		EntryLineWrapper entryLineWrapper;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryLine entryLine;
		JobDeclaration declaration;
	}
}
