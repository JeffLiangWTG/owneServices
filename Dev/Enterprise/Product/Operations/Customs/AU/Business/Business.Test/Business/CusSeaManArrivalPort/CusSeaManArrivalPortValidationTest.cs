using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManArrivalPortValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateBA_IsFirstArrival()
		{
			CusSeaManArrivalPort port = GetNewPort();

			port.Validation.ValidateAll();
			AssertHasMessageErrors("by default, when not selected", port.BA_IsFirstArrivalInfo);

			port.BA_IsFirstArrival = true;
			AssertNoNotifications("when selected", port.BA_IsFirstArrivalInfo);

			CusSeaManArrivalPort port2 = port.Header.Arrivals.AddNew();
			port.BA_IsFirstArrival = false;

			AssertHasMessageErrors("when not selected with two ports", port.BA_IsFirstArrivalInfo);
			AssertNoNotifications("when not selected with two ports", port2.BA_IsFirstArrivalInfo);
			port2.Validation.ValidateAll();
			AssertHasMessageErrors("when not selected with two ports", port2.BA_IsFirstArrivalInfo);

			port2.BA_IsFirstArrival = true;

			AssertHasMessageErrors("when selected with two ports", port.BA_IsFirstArrivalInfo);
			port.Validation.ValidateAll();
			AssertNoNotifications("when selected with two ports", port.BA_IsFirstArrivalInfo);
			AssertNoNotifications("when selected with two ports", port2.BA_IsFirstArrivalInfo);
		}

		public void TestValidateBA_RL_NKArrivalPort()
		{
			CusSeaManArrivalPort port = GetNewPort();

			port.Validation.ValidateBA_RL_NKArrivalPort();
			AssertHasErrors("by default", port.BA_RL_NKArrivalPortInfo);

			port.BA_RL_NKArrivalPort = "~~~";
			AssertHasErrors("when invalid", port.BA_RL_NKArrivalPortInfo);

			port.BA_RL_NKArrivalPort = (Factory.NewWithValidTestData<RefUNLOCO>()).RL_Code;
			AssertNoNotifications("when valid", port.BA_RL_NKArrivalPortInfo);
		}

		public void TestValidateBA_ArrivalPortATA()
		{
			CusSeaManArrivalPort port = GetNewPort();

			port.Validation.ValidateBA_ArrivalPortATA();
			AssertNoNotifications("by default", port.BA_ArrivalPortATAInfo);

			port.BA_ArrivalPortATA = ZDateTime.Now;
			AssertNoNotifications("when set", port.BA_ArrivalPortATAInfo);

			port.Header.ImpendingArrivalResponseStatus.Code = CMRBaseStatuses.Codes.OriginalAccepted;
			port.BA_ArrivalPortATA = ZDateTime.Empty;
			AssertHasMessageErrors("when iar accepted and empty", port.BA_ArrivalPortATAInfo);

			port.BA_ArrivalPortATA = ZDateTime.Now;
			AssertNoNotifications("when iar accepted and set", port.BA_ArrivalPortATAInfo);

			port.Header.ImpendingArrivalResponseStatus.Code = CMRBaseStatuses.Codes.AmendmentAccepted;
			port.BA_ArrivalPortATA = ZDateTime.Empty;
			AssertHasMessageErrors("when iar ammendment accepted and empty", port.BA_ArrivalPortATAInfo);

			port.BA_ArrivalPortATA = ZDateTime.Now;
			AssertNoNotifications("when iar ammendment accepted and set", port.BA_ArrivalPortATAInfo);
		}

		public void TestValidateBA_ArrivalPortETA()
		{
			CusSeaManArrivalPort port = GetNewPort();

			port.Validation.ValidateBA_ArrivalPortETA();
			AssertHasMessageErrors("by default", port.BA_ArrivalPortETAInfo);

			port.BA_ArrivalPortETA = ZDateTime.Now;
			AssertNoNotifications("when set", port.BA_ArrivalPortETAInfo);
		}

		public void TestValidateBA_OA_CTOAddress()
		{
			CusSeaManArrivalPort port = GetNewPort();
			port.BA_RL_NKArrivalPort = "AUSYD";

			port.BA_OA_CTOAddress = ZGuid.Invalid;
			AssertHasErrors("when invalid", port.BA_OA_CTOAddressInfo);

			port.BA_OA_CTOAddress = ZGuid.Empty;
			AssertHasMessageErrors("when empty", port.BA_OA_CTOAddressInfo);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			port.BA_OA_CTOAddress = org.MainAddress.PK;
			org.OH_IsSeaCTO = true;
			org.OH_IsMiscFreightServices = true;
			AssertNoNotifications("Closest port of the org does not match Arrival Port", port.BA_OA_CTOAddressInfo);
			org.OH_RL_NKClosestPort = "AUSYD";
			port.BA_OA_CTOAddress = org.MainAddress.PK; //Recalling the validation
			AssertNoNotifications("when set", port.BA_OA_CTOAddressInfo);
		}

		public void TestValidateBA_BerthCode()
		{
			CusSeaManArrivalPort port = GetNewPort();

			port.Validation.ValidateBA_BerthCode();
			AssertHasMessageErrors("by default", port.BA_BerthCodeInfo);

			port.BA_BerthCode = "Cheese";
			AssertNoNotifications("when set", port.BA_BerthCodeInfo);
		}

		public void TestValidateCTOEstablishmentID()
		{
			CusSeaManArrivalPort port = GetNewPort();

			port.Validation.ValidateAll();
			AssertHasMessageErrors("by default", port.CTOEstablishmentIDInfo);

			OrgHeader header = Factory.New<OrgHeader>();
			port.BA_OA_CTOAddress = header.Addresses[0].PK;
			port.CTOAddress.LocalControlledPremisesID = "54321";
			port.Validation.ValidateAll();
			AssertNoNotifications("when set", port.CTOEstablishmentIDInfo);
		}

		public void TestValidateStevedoreID()
		{
			CusSeaManArrivalPort port = GetNewPort();

			port.Validation.ValidateAll();
			AssertHasMessageErrors("by default", port.StevedoreIDInfo);

			OrgHeader header = Factory.New<OrgHeader>();
			port.BA_OA_CTOAddress = header.Addresses[0].PK;
			port.CTOAddress.Header.PrimaryRegistrationNumber.Number = "54321";
			port.Validation.ValidateAll();
			AssertNoNotifications("when set", port.StevedoreIDInfo);
		}

		public void TestValidateBA_DischargeIndicator()
		{
			CusSeaManArrivalPort port1 = GetNewPort();

			port1.Validation.ValidateAll();
			AssertNoNotifications("by default", port1.BA_DischargeIndicatorInfo);

			CusSeaManOBLHeader oceanBill = head.OceanBills.AddNew();
			oceanBill.BO_RL_NKDischargePort = "AUSYD";
			port1.BA_RL_NKArrivalPort = "AUSYD";
			port1.Validation.ValidateAll();
			AssertHasMessageErrors("when has discharging oceanbill", port1.BA_DischargeIndicatorInfo);

			port1.BA_DischargeIndicator = true;
			AssertNoNotifications("when set to true", port1.BA_DischargeIndicatorInfo);

			CusSeaManArrivalPort port2 = GetNewPort();
			port2.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("precondition", true, port1.BA_DischargeIndicator);
			AssertEquals("precondition", false, port2.BA_DischargeIndicator);

			port1.Validation.ValidateAll();
			port2.Validation.ValidateAll();
			AssertNoNotifications(port1.BA_DischargeIndicatorInfo);
			AssertNoNotifications(port2.BA_DischargeIndicatorInfo);

			port1.BA_DischargeIndicator = false;
			port1.Validation.ValidateAll();
			port2.Validation.ValidateAll();
			AssertHasMessageErrors(port1.BA_DischargeIndicatorInfo);
			AssertHasMessageErrors(port2.BA_DischargeIndicatorInfo);
		}

		#region Implementation

		CusSeaManArrivalPort GetNewPort()
		{
			return head.Arrivals.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			head = Factory.New<CusSeaManTranHead>();
		}

		CusSeaManTranHead head;

		#endregion
	}
}
