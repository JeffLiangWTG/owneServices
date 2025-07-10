using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs;
using NUnit.Framework;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(DocReleaseStatus))]
	sealed class CADocReleaseStatusTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			CACSubLocationTest.CreateSubLocation(Factory, "3072", "ADAMS CARGO LTD");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0497", "Toronto International Airport (Pearson)", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var wrapper = DocReleaseStatus.New(GetReleaseStatus(Factory), Factory);
			AssertEquals("ServiceOptionDescription", "257 RMD, EDI", wrapper.ServiceOptionDescription);
			AssertEquals("TransactionNumberFormatted", "10207 40000406 8", wrapper.TransactionNumberFormatted);
			AssertEquals("ReleaseStatusDescription", "4 - Goods Released", wrapper.ReleaseStatusDescription.ToString());
			AssertEquals("ReleaseDate", new ZDateTime(2010, 11, 25, 8, 20, 0), wrapper.ReleaseDate);
			AssertEquals("Processing Date", ZDateTime.Empty, wrapper.ProcessingDate);
			AssertEquals("CargoControlNumber", "37132536987", wrapper.CargoControlNumber);
			AssertEquals("DeliveryInstructions", "DELIVERY INSTRUCTIONS LINE 1\r\nLINE 2", wrapper.DeliveryInstructions);
			AssertEquals("ReleaseOfficeCodeDescription", "0497 - Toronto International Airport (Pearson)", wrapper.ReleaseOfficeCodeDescription);
			AssertEquals("WarehouseCodeDescription", "3072 - ADAMS CARGO LTD", wrapper.WarehouseCodeDescription);
			AssertEquals("Containers", "CONTAINER1, CONTAINER2, CONTAINER3", wrapper.Containers);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return DocReleaseStatus.New(GetReleaseStatus(Factory), Factory);
		}

		internal static IReleaseStatus GetReleaseStatus(BusinessObjectFactory factory)
		{
			var helper = new DeclarationTestHelper(factory, true);
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "37132536987";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER1";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER2";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CONTAINER3";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("37132536987", "1"));
			return declaration.ReleaseStatusesToPrint[0];
		}
	}
}
