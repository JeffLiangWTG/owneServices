using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE613And615ConsignmentProviderTest : DataProviderTestCase<IE613And615ConsignmentProvider>
	{
		public void TestContainerIndicator()
		{
			var provider = GetProvider();
			AssertEquals("No for no elements in Containers", AESFlagCodeList.Codes.No, provider.ContainerIndicator);
			declaration.CusContainers.AddNew();
			provider = GetProvider();
			AssertEquals("Yes for elements in Containers", AESFlagCodeList.Codes.Yes, provider.ContainerIndicator);

			declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicatorForUCCList.Codes.A20;
			AssertEquals("Empty for A20.", string.Empty, GetProvider().ContainerIndicator);
		}

		public void TestGrossMass()
		{
			var line1 = invoiceLines[0];
			line1.JI_Weight = 10m;
			line1.JI_WeightUQ = Core.Constants.Weight.Tonnes;

			var line2 = invoiceLines[1];
			line2.JI_Weight = 100m;
			line2.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			AssertEquals("GrossMass", 10100m, Provider.GrossMass);
		}

		public void TestCarrier()
		{
			var orgHeader = Factory.New<OrgHeader>();
			declaration.JE_OH_ShippingLine = orgHeader.PK;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");

			AssertEquals("Carrier.CarrierId", "IEREG222", Provider.Carrier.CarrierId);
		}

		public void TestTransportEquipment()
		{
			var package1 = declaration.Packages.AddNew();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT3";
			container1.CO_Seal = "SLA1";
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			invoiceLines[0].PackagesPivot.AddPivotFor(package1);
			AssertEquals("TransportEquipment should have element for empty ZG_SpecificCircumstanceIndicator.", true, GetProvider().TransportEquipment.Any());

			declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicatorForUCCList.Codes.A20;
			AssertEquals("Empty TransportEquipment for A20.", false, GetProvider().TransportEquipment.Any());
		}

		public void TestLocationOfGoods()
		{
			AssertSame("LocationOfGoods", Provider, Provider.LocationOfGoods);
		}

		public void TestCountryOfRoutingConsignment()
		{
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			AssertArrayEqualsByElements("CountryOfRoutingConsignment", new[] { "AU" }, Provider.CountryOfRoutingConsignment.ToArray());
		}

		public void TestCountryOfRoutingConsignment_GetDefaultTerritory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);

			var countryTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Spain, "EUSFT", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var europeTradeGroup = helper.CreateTradeGroup("EUN", "EUSFR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.AddCountry(countryTradeGroup, "AB", ZDate.BrettsBirthday, ZDate.Today.AddMonths(2));
			helper.AddCountry(europeTradeGroup, Core.Constants.CountryCodes.Spain, ZDate.BrettsBirthday, ZDate.Today.AddMonths(2));
			Factory.Save();

			declaration.JE_RL_NKPortOfArrival = "ABCD";
			AssertArrayEqualsByElements("CountryOfRoutingConsignment", new[] { Core.Constants.CountryCodes.Spain }, Provider.CountryOfRoutingConsignment.ToArray());
		}

		public void TestActiveTransportMeans()
		{
			AssertType<ActiveTransportMeansProvider>("ActiveTransportMeans", Provider.ActiveTransportMeans);
		}

		public void TestAdditionalSupplyChainActors()
		{
			var ca1 = instruction.CusSupplyChainActorReferences.AddNew();
			ca1.CFR_Code = "CA1";
			ca1.CFR_Reference = "CA001";

			AssertType<AdditionalSupplyChainActorProvider>(Provider.AdditionalSupplyChainActors.FirstOrDefault());
		}

		public void TestTransportDocument()
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, codes: "TD1");

			var ref1 = invoice.AdditionalInfos.AddNew();
			ref1.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo;
			ref1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument;
			ref1.CSI_ReferenceNumber = "REF1";
			ref1.CSI_Code = "TD1";
			AssertEquals(1, Provider.TransportDocument.Count);
			var transportDocument = Provider.TransportDocument.FirstOrDefault();
			AssertEquals("TransportDocument.Type", "TD1", transportDocument.Type);
			AssertEquals("TransportDocument.Reference", "REF1", transportDocument.Reference);
		}

		public void TestAdditionalReferences()
		{
			var ar1 = invoice.AdditionalInfos.AddNew();
			ar1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			ar1.CSI_Code = "AR1";
			ar1.CSI_ReferenceNumber = "AR001";

			var ar1Result = Provider.AdditionalReferences.FirstOrDefault();
			AssertType<AdditionalReferenceProvider>("Type of AdditionalReferences", ar1Result);
			AssertEquals("Type", "AR1", ar1Result.Type);
			AssertEquals("Reference", "AR001", ar1Result.Reference);
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
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;

				var ar1 = invoice.AdditionalInfos.AddNew();
				ar1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
				ar1.CSI_Code = "AR1";
				ar1.CSI_ReferenceNumber = "AR001";

				var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
				var provider = new IE613And615ConsignmentProvider(entryHeaderWrapper);

				AssertEquals("Count", 0, provider.AdditionalReferences.Count);
			}
		}

		public void TestAdditionalInformations()
		{
			var ai1 = invoice.AdditionalInfos.AddNew();
			ai1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			ai1.CSI_Code = "AR1";
			ai1.CSI_ReferenceNumber = "AR001";

			var ai1Result = Provider.AdditionalInformations.FirstOrDefault();
			AssertType<AdditionalInformationProvider>("Type of AdditionalInformations", ai1Result);
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
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				var ai1 = invoice.AdditionalInfos.AddNew();
				ai1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				ai1.CSI_Code = "AR1";
				ai1.CSI_ReferenceNumber = "AR001";
				var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
				var provider = new IE613And615ConsignmentProvider(entryHeaderWrapper);
				AssertEquals("Count", 0, provider.AdditionalInformations.Count);
			}
		}

		public void TestPreviousDocuments()
		{
			var doc1 = invoice.PreviousDocuments.AddNew();
			doc1.CSI_Code = "PD1";
			doc1.CSI_ReferenceNumber = "DOC001";

			AssertType<PreviousDocumentProvider>("Type of PreviousDocuments", Provider.PreviousDocuments.Single());
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
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				var entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				var previousDocument1 = invoice.PreviousDocuments.AddNew();
				previousDocument1.CSI_Code = "C1";
				var previousDocument2 = invoice.PreviousDocuments.AddNew();
				previousDocument2.CSI_Code = "C2";

				var entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
				var provider = new IE613And615ConsignmentProvider(entryHeaderWrapper);
				AssertEquals("Count", 0, provider.PreviousDocuments.Count);
			}
		}

		public void TestSupportingDocuments()
		{
			var supportingDocument1 = entryHeaderWrapper.RandomInvoiceHeader.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "SD1";
			supportingDocument1.CSI_ReferenceNumber = "SD001";

			AssertType<SupportingDocumentProvider>("Type of SupportingDocuments", Provider.SupportingDocuments.Single());

			declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicatorForUCCList.Codes.A20;
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
				var provider = new IE613And615ConsignmentProvider(entryHeaderWrapper);
				AssertEquals("Count", 0, provider.SupportingDocuments.Count);
			}
		}

		public void TestConsignmentItem()
		{
			AssertType<ConsignmentItemType03Provider>("ConsignmentItem should have a valid object.", Provider.ConsignmentItem.First());
		}

		#region ILocationOfGoods Members

		public void TestLocationCodeType()
		{
			declaration.JE_LocationOtherInformation = "KD";
			AssertEquals("LocationCodeType", "KD", Provider.LocationCodeType);
		}

		public void TestUNLocode()
		{
			declaration.JE_LocationOfGoods = "KD";
			AssertEquals("UNLocode", "KD", Provider.UNLocode);
		}

		#endregion

		protected override IE613And615ConsignmentProvider GetProvider() => new IE613And615ConsignmentProvider(entryHeaderWrapper);

		protected override void SetUp()
		{
			base.SetUp();
			(entryHeaderWrapper, _, invoiceLines) = MessageProviderTestHelper.SetupBasicTestBizObjsMultipleInvoiceLinesSingleEntryLine(Factory);
			declaration = entryHeaderWrapper.Declaration;
			invoice = entryHeaderWrapper.RandomInvoiceHeader;
			instruction = entryHeaderWrapper.Instruction;
		}

		EntryHeaderWrapper entryHeaderWrapper;
		JobComInvoiceLine[] invoiceLines;
		JobDeclaration declaration;
		CusEntryInstruction instruction;
		JobComInvoiceHeader invoice;
	}
}
