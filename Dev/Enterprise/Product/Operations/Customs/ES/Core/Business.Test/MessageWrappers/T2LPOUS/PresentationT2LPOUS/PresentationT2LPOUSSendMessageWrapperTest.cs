using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class PresentationT2LPOUSSendMessageWrapperTest : WrapperHelperTest<PresentationT2LPOUSSendMessageWrapper>
	{
		public void TestPreviousMRN()
		{
			entryHeader.MovementReferenceNumber = "MRNNumber";
			AssertEquals("Expected filled PreviousMRN", "MRNNumber", wrapper.PreviousMRN);
		}

		public void TestLocationOfGoods()
		{
			CombineAssertions(() =>
			{
				declaration.JE_LocationOfGoods = "123456789";
				AssertEquals("Expected filled LocationOfGoods less than 10", "123456789", wrapper.LocationOfGoods);

				declaration.JE_LocationOfGoods = "12345678901234";
				AssertEquals("Expected filled LocationOfGoods more than 10 trim first 4 chrs", "5678901234", wrapper.LocationOfGoods);
			});
		}

		public void TestContainerIndication()
		{
			CombineAssertions(() =>
			{
				var containerIndication = wrapper.ContainerIndication;
				AssertNotNull("Expected filled ContainerIndication", containerIndication);
				AssertSame("Cached ContainerIndication", wrapper.ContainerIndication, containerIndication);
				AssertEquals("Expected false ContainerIndication.IsContainerised", false, containerIndication.IsContainerised);

				var containerTag = "CONTAINER";
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = containerTag;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerTag).IsForInvoiceLine = true;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected true IsContainerised", true, wrapper.ContainerIndication.IsContainerised);
			});
		}

		public void TestTransportEquipment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty TransportEquipment", 0, wrapper.TransportEquipment.Count);

				declaration.JE_ContainerMode = "ULD";
				var package1 = declaration.Packages.AddNew();
				var container1 = declaration.CusContainers.AddNew();
				container1.CO_ContainerNumber = "CONT1";
				package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
				invoiceLine.PackagesPivot.AddPivotFor(package1);
				wrapper = GetWrapper(entryHeader);
				var transportEquipment = wrapper.TransportEquipment;
				AssertEquals("Expected filled TransportEquipment", 1, transportEquipment.Count);
				AssertSame("Cached TransportEquipment", wrapper.TransportEquipment, transportEquipment);
			});
		}

		public void TestGoodItems()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 GoodItem (mandatory at least one)", 1, wrapper.GoodItems.Count);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2203001011";

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_Tariff = "2203001012";

				AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

				entryHeader = declaration.CustomsEntryHeaders[0];
				wrapper = GetWrapper(entryHeader);

				var goodItems = wrapper.GoodItems;

				AssertEquals("Expected 3 GoodItems", 3, goodItems.Count);
				AssertSame("Cached GoodItems", wrapper.GoodItems, goodItems);
			});
		}

		public void TestPersonReqPres()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				var customCode = orgHeader.CustomsCodes.AddNew();
				customCode.OK_CodeType = "PAS";
				customCode.OK_CustomsRegNo = "12456789I";
				customCode.OK_RN_NKCodeCountry = "ES";

				var orgAddress = orgHeader.MainAddress;
				orgAddress.OA_RN_NKCountryCode = "ES";
				declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				declaration.JE_OH_Importer = orgHeader.PK;

				var personReqPres = wrapper.PersonReqPres;

				AssertNotNull("Expected filled PersonReqPres", personReqPres);
				AssertSame("Cached PersonReqPres", wrapper.PersonReqPres, personReqPres);
				AssertEquals("Uses the Importer if Declarant or Representative don't exist", "ES", personReqPres.Address.Country);
				AssertEquals("PersonReqPres Contact Email is the one in Misc", "wisetech@wisetechglobal.com", personReqPres.ContactPerson.Email);

				wrapper = GetWrapper(entryHeader);

				var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader2.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				var customCode2 = orgHeader2.CustomsCodes.AddNew();
				customCode2.OK_CodeType = "PAS";
				customCode2.OK_CustomsRegNo = "12456789P";
				customCode2.OK_RN_NKCodeCountry = "PL";

				var orgAddress2 = orgHeader2.MainAddress;
				orgAddress2.OA_RN_NKCountryCode = "PL";
				declaration.JE_OA_DeclarantAddress = orgAddress2.PK;

				var personReqPres2 = wrapper.PersonReqPres;

				AssertNotNull("Expected filled personReqPres2", personReqPres2);
				AssertNotSame("Not equal to old Cached PersonReqPres2", wrapper.PersonReqPres, personReqPres);
				AssertSame("Cached personReqPres2", wrapper.PersonReqPres, personReqPres2);
				AssertEquals("Uses Declarant if exists instead of Importer", "PL", personReqPres2.Address.Country);

				wrapper = GetWrapper(entryHeader);

				var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader3.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				var customCode3 = orgHeader3.CustomsCodes.AddNew();
				customCode3.OK_CodeType = "PAS";
				customCode3.OK_CustomsRegNo = "12456789I";
				customCode3.OK_RN_NKCodeCountry = "IT";

				var orgAddress3 = orgHeader3.MainAddress;
				orgAddress3.OA_RN_NKCountryCode = "IT";
				declaration.JE_OA_Representative = orgAddress3.PK;

				var personReqPres3 = wrapper.PersonReqPres;

				AssertNotNull("Expected filled personReqPres3", personReqPres3);
				AssertNotSame("Not equal to old Cached PersonReqPres", wrapper.PersonReqPres, personReqPres);
				AssertNotSame("Not equal to old Cached PersonReqPres2", wrapper.PersonReqPres, personReqPres2);
				AssertSame("Cached personReqPres3", wrapper.PersonReqPres, personReqPres3);
				AssertEquals("Uses Representative if exists instead of Declarant or Importer", "IT", personReqPres3.Address.Country);
			});
		}

		public void TestSendEmailL()
		{
			AssertEquals("SendEmailL is implemented in each child class", "S", wrapper.SendEmailL);
		}

		public void TestSendEmailU()
		{
			AssertEquals("SendEmailU is implemented in each child class", ZString.Empty, wrapper.SendEmailU);
		}

		public void TestSendEmailExp()
		{
			AssertEquals("SendEmailExp is implemented in each child class", ZString.Empty, wrapper.SendEmailExp);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge failed", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			var contactEmail = "wisetech@wisetechglobal.com";
			using (RegistryTemporarySetterHelper.SetCustomsClearanceEmailRecipient(contactEmail))
			{
				wrapper = GetWrapper(entryHeader);
			}
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		PresentationT2LPOUSSendMessageWrapper wrapper;

		PresentationT2LPOUSSendMessageWrapper GetWrapper(CusEntryHeader entryHeader) => new PresentationT2LPOUSSendMessageWrapper(entryHeader, Certificate);

		protected override PresentationT2LPOUSSendMessageWrapper GetProvider() => wrapper;
	}
}
