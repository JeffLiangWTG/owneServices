using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDCusContainerValidationTest : CusContainerValidationTest
	{
		public void TestCO_FCL_LCL_AIR()
		{
			container.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("SEA", true, container.JobDeclaration.IsSea);

			container.CO_FCL_LCL_AIR = "";
			AssertEquals("Is mandatory for this job", true, container.CO_FCL_LCL_AIRInfo.HasMessageErrors());

			container.JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Is AIR", false, container.JobDeclaration.IsSea);

			container.CO_FCL_LCL_AIR = "";
			AssertEquals("Is not mandatory for Air job", false, container.CO_FCL_LCL_AIRInfo.HasMessageErrors());

			container.CO_FCL_LCL_AIR = container.Lookups.CO_FCL_LCL_NCT_List[0].Code;
			AssertEquals("Is mandatory for this job", false, container.CO_FCL_LCL_AIRInfo.HasMessageErrors());

			container.CO_FCL_LCL_AIR = "XXX";
			AssertEquals("Is not in the list", true, container.CO_FCL_LCL_AIRInfo.HasMessageErrors());
		}

		public void TestValidateCO_ContainerNumberForMail()
		{
			container.Validation.ValidateCO_ContainerNumber();
			AssertEquals("Should have an error", true, container.CO_ContainerNumberInfo.HasMessageErrors());
		}

		public void TestValidateCO_ContainerNumWithNoPacking()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.DisableDefaultPackingInformation = true;
			testDec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_MessageSubType = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.MessageSubType.FormalEntry;
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 11, 01);

			CusContainer container1 = testDec.CusContainers.AddNew();
			Package package = testDec.Packages.AddNew();
			package.CW_PackQty = 12;
			package.CW_OuterPacks = 13;
			package.CW_InBondPackQty = 14;
			container1.CO_ContainerNumber = "CZUP3352349";
			package.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;

			Customs.Business.Bill masterBill1 = testDec.Bills.AddNew();
			masterBill1.CU_MasterBill = "AQT1";

			Customs.Business.Bill houseBill1 = testDec.Bills.AddNew();
			houseBill1.CU_MasterBill = "AQT1";
			houseBill1.CU_HouseBill = "AQT1HBL";

			BaseCusContainer container2 = testDec.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CTZU3324527";
			container2.PackingGroups.RemoveAndDeleteAll();

			Assert(!container1.CO_ContainerNumberInfo.GetMessageErrors().Contains("This container is not selected in the packing section. It will not be sent in a message."));
			Assert(container2.CO_ContainerNumberInfo.GetMessageErrors().Contains("This container is not selected in the packing section. It will not be sent in a message."));

			testDec.JE_MessageSubType = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.MessageSubType.SelfAssessedClearance;
			BaseCusContainer container3 = testDec.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CTZU3324527";
			container3.PackingGroups.RemoveAndDeleteAll();
			Assert(!container3.CO_ContainerNumberInfo.GetMessageErrors().Contains("This container is not selected in the packing section. It will not be sent in a message."));

			testDec.JE_MessageSubType = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.MessageSubType.SelfAssessedClearanceWithLines;
			BaseCusContainer container4 = testDec.CusContainers.AddNew();
			container4.CO_ContainerNumber = "CTZU3324527";
			container4.PackingGroups.RemoveAndDeleteAll();
			Assert(!container4.CO_ContainerNumberInfo.GetMessageErrors().Contains("This container is not selected in the packing section. It will not be sent in a message."));

			testDec.JE_MessageSubType = Enterprise.Customs.AU.Declaration.Business.JobDeclaration.MessageSubType.FormalEntry;
			BaseCusContainer container5 = testDec.CusContainers.AddNew();
			container5.CO_ContainerNumber = "CTZU3324527";
			container5.PackingGroups.RemoveAndDeleteAll();
			Assert(container5.CO_ContainerNumberInfo.GetMessageErrors().Contains("This container is not selected in the packing section. It will not be sent in a message."));
		}

		public void TestCheckCO_FCL_LCL_AIR()
		{
			container.Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			container.Validation.ValidateCO_FCL_LCL_AIR();
			AssertHasMessageErrors(container.CO_FCL_LCL_AIRInfo);
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertNoMessageErrors(container.CO_FCL_LCL_AIRInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_HouseBill = "1";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
		}

		CusContainer container;
		JobDeclaration declaration;

		#endregion

	}
}
