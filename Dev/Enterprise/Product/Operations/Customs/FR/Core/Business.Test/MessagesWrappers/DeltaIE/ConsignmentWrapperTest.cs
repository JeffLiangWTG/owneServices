using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class ConsignmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<ConsignmentWrapper>
	{
		protected override ConsignmentWrapper GetProvider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceline = invoice.InvoiceLines.AddNew();
			invoiceline.FillWithValidTestData();
			var invoiceline2 = invoice.InvoiceLines.AddNew();
			invoiceline2.FillWithValidTestData();
			var additionalInfo = invoice.AdditionalInfos.AddNew();
			additionalInfo.CSI_ReferenceNumber = "reference";
			additionalInfo.CSI_Code = PreviousDocumentCodeList.Codes.ZZZ;
			declaration.JE_ContainerMode = Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH;
			declaration.JE_TotalWeight = 42m;
			declaration.JE_TransportModeInland = "Inl";
			declaration.JE_TransportMode = "Mod";
			declaration.JE_UCR = "1";
			declaration.ZG_Box18TransportNationality = Core.Constants.CountryCodes.France;
			declaration.ZG_Box18TransportID = "ID";
			declaration.ZG_Box18TransportType = 2;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();

			var doc = declaration.AdditionalInfos.AddNew();
			doc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			doc.CSI_ReferenceNumber = "ref";
			doc.CSI_Code = "cod";

			var doc3 = declaration.AdditionalInfos.AddNew();
			doc3.CSI_SubType = "ZZZ";
			doc3.CSI_ReferenceNumber = "ref2";
			doc3.CSI_Code = "co2";

			instruction.FillWithValidTestData();
			invoiceline.JI_CEI = instruction.PK;
			invoiceline2.JI_CEI = instruction.PK;

			var equipment = declaration.Equipments.AddNew();
			equipment.CEQ_IdentificationNumber = "E1";

			var equipment2 = declaration.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "E2";

			var goodLocation = instruction.GoodsLocation;
			goodLocation.FillWithValidTestData();
			goodLocation.CGL_AdditionalIdentifier = "adID";

			var entryline = entryHeader.MergedLines.AddNew();
			var entryline2 = entryHeader.MergedLines.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "MSCU6767675";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "MSCU7897890";

			var package1 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_ContainerNoOrEquipmentNo = "MSCU6767675";
			package1.CW_PackType = "AA";
			package1.CW_MarksAndNos = "A";

			var package2 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_ContainerNoOrEquipmentNo = "MSCU7897890";
			package2.CW_PackType = "PK";
			package2.CW_MarksAndNos = "B";

			var package3 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package3.CW_PackQty = 3;
			package3.CW_PackType = "PK";
			package3.CW_MarksAndNos = "C";

			var package4 = (EU.Business.Declaration.Package)declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			package4.CW_PackQty = 3;
			package4.CW_PackType = "PK";
			package4.CW_MarksAndNos = "C";
			package4.CW_ContainerNoOrEquipmentNo = "MSCU6767675";

			var packing1 = invoiceline.PackagesForInvoiceLinesForBindingOnly[0];
			packing1.IsLinked = true;
			packing1.PackQty = 1;

			var packing2 = invoiceline.PackagesForInvoiceLinesForBindingOnly[1];
			packing2.IsLinked = true;
			packing2.PackQty = 2;

			var packing3 = invoiceline.PackagesForInvoiceLinesForBindingOnly[2];
			packing3.IsLinked = true;
			packing3.PackQty = 3;

			var packing4 = invoiceline.PackagesForInvoiceLinesForBindingOnly[3];
			packing4.IsLinked = true;
			packing4.PackQty = 2;

			var packing5 = invoiceline2.PackagesForInvoiceLinesForBindingOnly[3];
			packing5.IsLinked = true;
			packing5.PackQty = 1;

			invoiceline.JI_CL = entryline.PK;
			entryline.CL_LineNumber = 1;
			entryline.InvoiceLines.Add(invoiceline);

			invoiceline2.JI_CL = entryline2.PK;
			entryline2.CL_LineNumber = 2;
			entryline2.InvoiceLines.Add(invoiceline2);

			return ConsignmentWrapper.New(entryHeader);
		}

		public void TestActiveBorderTransportMeans()
		{
			AssertEquals("ActiveBorderTransportMeans.Nationality should be equal to ZG_Box18TransportNationality.", Core.Constants.CountryCodes.France, Provider.ActiveBorderTransportMeans.Nationality);
		}

		public void TestArrivalTransportMeans()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				declaration.JE_VesselName = "MYVESSEL";

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;

				var consignmentWrapper = ConsignmentWrapper.New(entryHeader);

				AssertEquals("ArrivalTransportMeans.IdentificationNumber should be equal to JE_VesselName when transport mode is Rail.", "MYVESSEL", consignmentWrapper.ArrivalTransportMeans.IdentificationNumber);
				AssertEquals("ArrivalTransportMeans.TypeOfIdentification should be equal to JE_TransportMeans.", "20", consignmentWrapper.ArrivalTransportMeans.TypeOfIdentification);
			}
		}

		public void TestContainerIndicator()
		{
			AssertcontainerTraandContainerMode("0", false, ZString.Empty);
			AssertcontainerTraandContainerMode("0", true, ZString.Empty);
			AssertcontainerTraandContainerMode("1", true, "container");
		}

		void AssertcontainerTraandContainerMode(string containerTra, bool shouldCreateContainer, string containerNumber)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryline = entry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryline.PK;

			if (shouldCreateContainer)
			{
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = containerNumber;
			}

			var consignementWrapper = ConsignmentWrapper.New(entry);
			declaration.JE_ContainerMode = ZString.Empty;
			AssertEquals("0", consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("0", consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("0", consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.RollOnRollOff;
			AssertEquals("0", consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("0", consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Loose;
			AssertEquals("0", consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LTL;
			AssertEquals("0", consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FTL;
			AssertEquals("0", consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("0", consignementWrapper.ContainerIndicator);

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(containerTra, consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals(containerTra, consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.ULD;
			AssertEquals(containerTra, consignementWrapper.ContainerIndicator);
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals(containerTra, consignementWrapper.ContainerIndicator);
		}

		public void TestGrossMass()
		{
			AssertEquals("GrossMass should be equal to JE_TotalWeight.", 42d, Provider.GrossMass);
		}

		public void TestInlandModeOfTransport_TranslatedValueExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportModeInland = "AIR";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var consignementWrapper = ConsignmentWrapper.New(entry);

			AssertEquals("InlandModeOfTransport should be equal to the value of WCO code translated from JE_TransportModeInland.", "4", consignementWrapper.InlandModeOfTransport);
		}

		public void TestInlandModeOfTransport_TranslatedValueNotExist()
		{
			AssertEquals("InlandModeOfTransport should be equal to JE_TransportModeInland.", "Inl", Provider.InlandModeOfTransport);
		}

		public void TestLocationOfGoods()
		{
			AssertEquals("LocationOfGoods.AdditionalIdentifier should be equal to CGL_AdditionalIdentifier.", "adID", Provider.LocationOfGoods.AdditionalIdentifier);
		}

		public void TestModeOfTransportAtTheBorder_TranslatedValueExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "AIR";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var consignementWrapper = ConsignmentWrapper.New(entry);
			AssertEquals("ModeOfTransportAtTheBorder should be equal to the value of WCO code translated from JE_TransportMode.", "4", consignementWrapper.ModeOfTransportAtTheBorder);
		}

		public void TestModeOfTransportAtTheBorder_TranslatedValueNotExist()
		{
			AssertEquals("ModeOfTransportAtTheBorder should be equal to JE_TransportMode.", "Mod", Provider.ModeOfTransportAtTheBorder);
		}

		public void TestReferenceNumberUCR()
		{
			AssertEquals("ReferenceNumberUCR should be equal to JE_UCR.", "1", Provider.ReferenceNumberUCR);
		}

		public void TestTransportDocument()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();

			var wrapper = ConsignmentWrapper.New(entryHeader);
			AssertNull("TransportDocument should be null as there is no additionalinfo.", wrapper.TransportDocument);

			var doc = declaration.AdditionalInfos.AddNew();
			doc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			doc.CSI_ReferenceNumber = "ref";
			doc.CSI_Code = "cod";

			var doc2 = declaration.AdditionalInfos.AddNew();
			doc2.CSI_SubType = "ZZZ";
			doc2.CSI_ReferenceNumber = "ref2";
			doc2.CSI_Code = "co2";

			AssertEquals("TransportDocument.Count should be equal to 1.", 1, wrapper.TransportDocument.Count);
			AssertEquals("ReferenceNumber should be equal to doc.CSI_Referencenumber.", "ref", wrapper.TransportDocument.ElementAt(0).ReferenceNumber);
			AssertEquals("Type should be equal to doc.CSI_Code.", "cod", wrapper.TransportDocument.ElementAt(0).Type);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.FillWithValidTestData();

			wrapper = ConsignmentWrapper.New(entryHeader);
			AssertNull("TransportDocument.Count should be null as there is no additionalinfo.", wrapper.TransportDocument);

			doc = declaration.AdditionalInfos.AddNew();
			doc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			doc.CSI_ReferenceNumber = "ref";
			doc.CSI_Code = "cod";

			doc2 = declaration.AdditionalInfos.AddNew();
			doc2.CSI_SubType = "ZZZ";
			doc2.CSI_ReferenceNumber = "ref2";
			doc2.CSI_Code = "co2";

			var doc3 = declaration.AdditionalInfos.AddNew();
			doc3.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			doc3.CSI_ReferenceNumber = "ref2";
			doc3.CSI_Code = "co2";

			AssertEquals("TransportDocument.Count should be equal to 2.", 2, wrapper.TransportDocument.Count);
		}

		public void TestTransportEquipment()
		{
			AssertEquals("TransportEquipment count should be two as there is 3 containers link to the entrylines.", 2, Provider.TransportEquipment.Count);

			CombineAssertions("value of the MSCU6767675 transport equipment, entryLine 1 + 2:", () =>
			{
				var firstEquipment = Provider.TransportEquipment.ElementAt(0);

				AssertEquals("ContainerIdentificationNumber should be equal entryline's first linked package container number.", "MSCU6767675", firstEquipment.ContainerIdentificationNumber);
				AssertEquals("There should be 2 goods Reference as there is two entryline with the same container number.", 2, firstEquipment.GoodsReference.Count);
				AssertEquals("DeclarationGoodsItemNumber should be equal to entryline line number, first entryline.", "1", firstEquipment.GoodsReference.ElementAt(0).DeclarationGoodsItemNumber);
				AssertEquals("DeclarationGoodsItemNumber should be equal to entryline line number, second entryline.", "2", firstEquipment.GoodsReference.ElementAt(1).DeclarationGoodsItemNumber);
			});

			CombineAssertions("value of the second transportEquipment, entryLine 1:", () =>
			{
				var secondEquipment = Provider.TransportEquipment.ElementAt(1);

				AssertEquals("ContainerIdentificationNumber should be equal entryline's 2nd linked package container number.", "MSCU7897890", secondEquipment.ContainerIdentificationNumber);
				AssertEquals("DeclarationGoodsItemNumber should be equal to entryline line number.", "1", secondEquipment.GoodsReference.ElementAt(0).DeclarationGoodsItemNumber);
			});
		}
	}
}
