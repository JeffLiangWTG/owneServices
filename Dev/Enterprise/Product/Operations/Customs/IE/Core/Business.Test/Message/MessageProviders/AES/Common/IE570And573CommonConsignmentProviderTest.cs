using System.Linq;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE570And573CommonConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE570And573CommonConsignmentProvider>
	{
		public void TestContainerIndicator()
		{
			var provider = GetProvider();
			AssertEquals("No for no elements in Containers", AESFlagCodeList.Codes.No, provider.ContainerIndicator);
			declaration.CusContainers.AddNew();
			provider = GetProvider();
			AssertEquals("Yes for elements in Containers", AESFlagCodeList.Codes.Yes, provider.ContainerIndicator);
		}

		public void TestCarrier()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111");
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222");
			declaration.JE_OH_ShippingLine = orgHeader.PK;

			AssertEquals("Carrier.CarrierId", "IEREG222", Provider.Carrier.CarrierId);
		}

		public void TestAdditionalSupplyChainActors()
		{
			instruction.CusSupplyChainActorReferences.AddNew();
			AssertType<AdditionalSupplyChainActorProvider>(Provider.AdditionalSupplyChainActors.FirstOrDefault());
		}

		public void TestTransportEquipments()
		{
			AssertEquals("TransportEquipment being empty for now.", 0, Provider.TransportEquipments.Count);
		}

		public void TestSupportingDocuments()
		{
			var supportingDocument1 = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "C1";
			supportingDocument1.CSI_ReferenceNumber = "R1";
			var supportingDocument2 = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "C2";
			supportingDocument2.CSI_ReferenceNumber = "R2";

			var supportingDocuments = Provider.SupportingDocuments.ToList();
			CombineAssertions(() =>
			{
				AssertEquals("Count", 2, supportingDocuments.Count);
				AssertEquals("Supporting Document 1", "C1", supportingDocuments[0].Type);
				AssertEquals("Supporting Document 1", "R1", supportingDocuments[0].Reference);
				AssertEquals("Supporting Document 2", "C2", supportingDocuments[1].Type);
				AssertEquals("Supporting Document 2", "R2", supportingDocuments[1].Reference);
			});
		}

		public void TestPreviousDocuments()
		{
			var previousDocument1 = invoiceHeader.PreviousDocuments.AddNew();
			previousDocument1.CSI_Code = "C1";
			previousDocument1.CSI_ReferenceNumber = "R1";
			var previousDocument2 = invoiceHeader.PreviousDocuments.AddNew();
			previousDocument2.CSI_Code = "C2";
			previousDocument2.CSI_ReferenceNumber = "R2";

			CombineAssertions(() =>
			{
				var previousDocuments = Provider.PreviousDocuments.ToList();
				AssertEquals("Count", 2, previousDocuments.Count);
				AssertEquals("Previous Document 1", "C1", previousDocuments[0].Type);
				AssertEquals("Previous Document 1", "R1", previousDocuments[0].Reference);
				AssertEquals("Previous Document 2", "C2", previousDocuments[1].Type);
				AssertEquals("Previous Document 2", "R2", previousDocuments[1].Reference);
			});
		}

		public void TestTransportDocument()
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, codes: "TD1");

			var ref1 = invoiceHeader.AdditionalInfos.AddNew();
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
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, codes: new[] { "9001", "9002" });

			var additionalInfo1 = invoiceHeader.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "9001";
			additionalInfo1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo1.CSI_ReferenceNumber = "9001001";
			var additionalInfo2 = invoiceHeader.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "9002";
			additionalInfo2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalReference;
			additionalInfo2.CSI_ReferenceNumber = "9002001";

			CombineAssertions(() =>
			{
				var additionalReferences = Provider.AdditionalReferences.ToList();
				AssertEquals("Count", 2, additionalReferences.Count);
				AssertEquals("Additional References 1", "9001", additionalReferences[0].Type);
				AssertEquals("Additional References 1", "9001001", additionalReferences[0].Reference);
				AssertEquals("Additional References 2", "9002", additionalReferences[1].Type);
				AssertEquals("Additional References 2", "9002001", additionalReferences[1].Reference);
			});
		}

		public void TestAdditionalInformations()
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, new[] { ("9001", "9001 DES"), ("9002", "9002 DES") });

			var additionalInfo1 = invoiceHeader.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "9001";
			additionalInfo1.CSI_Description = "9001001";
			additionalInfo1.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var additionalInfo2 = invoiceHeader.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "9002";
			additionalInfo2.CSI_Description = "9002001";
			additionalInfo2.CSI_SubType = EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			CombineAssertions(() =>
			{
				var additionalInformations = Provider.AdditionalInformations.ToList();
				AssertEquals("Count", 2, additionalInformations.Count);
				AssertEquals("AdditionalInfo 1", "9001", additionalInformations[0].Code);
				AssertEquals("AdditionalInfo 1", "9001001", additionalInformations[0].Text);
				AssertEquals("AdditionalInfo 2", "9002", additionalInformations[1].Code);
				AssertEquals("AdditionalInfo 2", "9002001", additionalInformations[1].Text);
			});
		}

		public void TestLocationOfGoods()
		{
			AssertSame("GoodsLocation", Provider, Provider.LocationOfGoods);
		}

		public void TestConsignmentItems()
		{
			AssertType<IE570And573CommonConsignmentItemProvider>(Provider.ConsignmentItems.FirstOrDefault());
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

		protected override IE570And573CommonConsignmentProvider GetProvider() => new IE570And573CommonConsignmentProvider(entryHeaderWrapper);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			instruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_CL = entryLine.PK;
			entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
		}
		EntryHeaderWrapper entryHeaderWrapper;
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		CusEntryInstruction instruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
