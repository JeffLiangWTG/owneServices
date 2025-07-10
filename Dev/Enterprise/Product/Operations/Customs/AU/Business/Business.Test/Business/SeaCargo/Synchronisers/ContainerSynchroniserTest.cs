using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class ContainerSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniser()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerMode = ZString.Empty;

			CusSCAContainer sCAContainer = Factory.New<CusSCAContainer>();

			ContainerSynchroniser synchroniser = new ContainerSynchroniser(sCAContainer, container, consol);
			synchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Start));

			container.JC_ContainerNum = TestContainerNumber;
			AssertEquals("CusSCAContainer CN_ContainerNubmer", TestContainerNumber, sCAContainer.CN_ContainerNumber);

			container.JC_SealNum = TestSealNumber;
			AssertEquals("CusSCAContainer CN_SealNumber", TestSealNumber, sCAContainer.CN_SealNumber);

			var fortyFootGPContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, TestContainerTypeNK);
			container.JC_RC = fortyFootGPContainer.PK;

			AssertEquals("CusSCAContainer CN_RC_NKContainerType", TestContainerTypeNK, sCAContainer.CN_RC_NKContainerType);

			container.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertEquals("CusSCAContainer CN_ContainerMode", Enterprise.Core.Constants.ContainerModes.FCL, sCAContainer.CN_ContainerMode);
		}

		public void TestContainerModeSynchronisation()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer container = consol.Containers.AddNew();
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;

			CusSCAContainer sCAContainer = oceanBill.Containers[0];

			consol.JK_ConsolMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;
			container.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;

			AssertEquals("Buyers Consol should come across as FCX", Enterprise.Core.Constants.ContainerModes.FCLMixedShipper, sCAContainer.CN_ContainerMode);

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BCNU8833928";
			container2.JC_ContainerMode = Enterprise.Core.Constants.ContainerModes.BuyersConsol;

			AssertEquals("Buyers Consol should come across as FCX", Enterprise.Core.Constants.ContainerModes.FCLMixedShipper, sCAContainer.CN_ContainerMode);
		}

		[ExpectNoExceptions()]
		public void TestContainerNotFoundException()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer container = consol.Containers.AddNew();
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;

			CusSCAContainer sCAContainer = oceanBill.Containers[0];
			consol.Containers.Remove(container);
			//SCAContainer.Delete();
			container.JC_ContainerNum = "NLYU9282989";
		}

		public void TestContainerNotFoundException2()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer container = consol.Containers.AddNew();
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;

			CusSCAContainer sCAContainer = oceanBill.Containers[0];
			sCAContainer.Delete();
			container.JC_ContainerNum = "NLYU9282989";
			AssertEquals("SCAContainer.IsDeleted", true, sCAContainer.IsDeleted);
		}

		/// <summary>
		/// 
		///	  Not Synchronised CN_ShipperOwnedContainer
		///	  Not Synchronised CN_ContainerStatus
		///	  Not Synchronised CN_UnderbondStatus
		///	  Not Synchronised CN_UnderbondBySea
		///	  Not Synchronised CN_UnderbondVoyage
		///	  Not Synchronised CN_UnderbondVesselName
		///	  Not Synchronised CN_TimeupUnderbondMove
		/// 
		/// </summary>
		public void TestSynchronisationOfUnderbondFields()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer container = consol.Containers.AddNew();
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;

			OrgHeader arrivalCTO = GetArrivalCTO();
			CusSCAContainer sCAContainer = oceanBill.Containers[0];

			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.Addresses[0].PK;
			AssertEquals("Synched Container CTO", TestCTOCode, sCAContainer.CN_MoveUnderbondFrom);
			AssertEquals("Synched Container CTO Address", arrivalCTO.Addresses[0].PK, sCAContainer.CN_OA_UnderbondFrom);

			OrgHeader unpackCFS = GetUnpackCFS();
			consol.JK_OA_UnpackDepotAddress = unpackCFS.Addresses[0].PK;
			AssertEquals("Synched Container Depot", TestDepotCode, sCAContainer.CN_MoveUnderbondTo);
			AssertEquals("Synched Container Depot Address", unpackCFS.Addresses[0].PK, sCAContainer.CN_OA_UnderbondTo);
		}

		public void TestSynchroniserReadOnlyState()
		{
			ForwardingConsol consol = CreateFCLConsol();
			CommonContainer freightContainer = consol.Containers.AddNew();
			SeaCargoSynchroniser synchroniser = GetSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			CusSCAContainer container = oceanBill.Containers[0];

			Assert(!oceanBill.OverrideFreightDefaults);
			AssertEquals("ReadOnly CN_ContainerNumberInfo", true, container.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("ReadOnly CN_SealNumberInfo", true, container.CN_SealNumberInfo.ReadOnly);
			AssertEquals("ReadOnly CN_RC_NKContainerTypeInfo", true, container.CN_RC_NKContainerTypeInfo.ReadOnly);
			AssertEquals("ReadOnly CN_ContainerModeInfo", true, container.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("ReadOnly CN_OA_UnderbondFromInfo", true, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("ReadOnly CN_OA_UnderbondToInfo", true, container.CN_OA_UnderbondToInfo.ReadOnly);

			oceanBill.OverrideFreightDefaults = true;
			AssertEquals("ReadOnly CN_ContainerNumberInfo", false, container.CN_ContainerNumberInfo.ReadOnly);
			AssertEquals("ReadOnly CN_SealNumberInfo", false, container.CN_SealNumberInfo.ReadOnly);
			AssertEquals("ReadOnly CN_RC_NKContainerTypeInfo", false, container.CN_RC_NKContainerTypeInfo.ReadOnly);
			AssertEquals("ReadOnly CN_ContainerModeInfo", false, container.CN_ContainerModeInfo.ReadOnly);
			AssertEquals("ReadOnly CN_OA_UnderbondFromInfo", false, container.CN_OA_UnderbondFromInfo.ReadOnly);
			AssertEquals("ReadOnly CN_OA_UnderbondToInfo", false, container.CN_OA_UnderbondToInfo.ReadOnly);
		}

		#region Implementation

		const string TestCTOCode = "A077J";
		const string TestDepotCode = "AP21J";

		OrgHeader GetUnpackCFS()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "Unpack Container Freight Station";
			result.MainAddress.OA_Address1 = "Somewhere not near the docks";
			result.OH_RL_NKClosestPort = "AUSYD";
			result.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, TestDepotCode);
			result.OH_IsUnpackDepot = true;
			return result;
		}

		OrgHeader GetArrivalCTO()
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = "Synch CTO Test Object";
			result.MainAddress.OA_Address1 = "Somewhere On the Docks";
			result.OH_RL_NKClosestPort = "AUSYD";
			result.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, TestCTOCode);
			result.OH_IsSeaCTO = true;
			return result;
		}

		protected virtual SeaCargoSynchroniser GetSeaCargoSynchroniser(ForwardingConsol consol)
		{
			return new CMRSeaCargoSynchroniser(consol);
		}

		#endregion

	}
}
