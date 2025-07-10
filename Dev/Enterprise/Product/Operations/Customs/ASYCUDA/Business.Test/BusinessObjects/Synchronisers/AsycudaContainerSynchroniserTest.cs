using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaContainerSynchroniser))]
	sealed class AsycudaContainerSynchroniserTest : TestCaseWithFactory
	{
		public void TestAsycudaPackHasEmptyHeader()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var sourceContainer = consol.Containers.AddNew();
			sourceContainer.JC_ContainerNum = "123";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 3;
			sourceContainer.PackLines.Add(packLine);

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			Factory.Save();

			var destinationContainer = manifestHeader.Containers.AddNew();
			destinationContainer.ACN_ContainerNumber = "123";
			destinationContainer.ACN_AMA_Manifest = ZGuid.Empty;
			var synchroniser = new AsycudaContainerSynchroniser(destinationContainer, sourceContainer, shipment);
			AssertNoExceptionThrown(() => synchroniser.Synchronise(true));
		}

		public void TestSynchroniseOfSealPartyTypes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.STYPE, MapDirectionList.Codes.BTH, "Sealing Party Type Mapping", false);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.Customs, Core.Constants.ContainerSealParties.Codes.Customs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap(RefCusMapTypeList.Codes.STYPE, Core.Constants.ContainerSealParties.Codes.Terminal, Core.Constants.ContainerSealParties.Codes.Terminal, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();

			var departurePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.SouthAfrica));
			var destinationPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.SouthAfrica));

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = departurePort.Code;
			shipment.JS_RL_NKDestination = destinationPort.Code;

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKLoadPort = departurePort.Code;
			consol.JK_RL_NKDischargePort = destinationPort.Code;

			var sourceContainer = consol.Containers.AddNew();
			sourceContainer.JC_ContainerNum = "123";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 3;
			sourceContainer.PackLines.Add(packLine);
			var manifestHeader = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
			manifestHeader.SetParent(consol);
			var destinationContainer = manifestHeader.Containers.AddNew();
			destinationContainer.ACN_ContainerNumber = "123";
			Factory.Save();

			AssertNoExceptionThrown(() => new AsycudaContainerSynchroniser(destinationContainer, sourceContainer, shipment).Synchronise(true));

			destinationContainer.ACN_SealingPartyType = Core.Constants.ContainerSealParties.Codes.Quarantine;
			destinationContainer.ACN_SealType1 = SealTypeList.Codes.MechanicalSeal;
			destinationContainer.ACN_SealingPartyName = "ABC";
			sourceContainer.JC_SealParty = Core.Constants.ContainerSealParties.Codes.Quarantine;
			AssertEquals(ZString.Empty, destinationContainer.ACN_SealingPartyType);
			AssertEquals(SealTypeList.Codes.MechanicalSeal, destinationContainer.ACN_SealType1);
			AssertEquals("ABC", destinationContainer.ACN_SealingPartyName);
			AssertEquals("ACN_SealingPartyTypeInfo.ReadOnly", false, destinationContainer.ACN_SealingPartyTypeInfo.ReadOnly);

			destinationContainer.ACN_SealingPartyType2 = Core.Constants.ContainerSealParties.Codes.Quarantine;
			destinationContainer.ACN_SealType2 = SealTypeList.Codes.MechanicalSeal;
			sourceContainer.JC_AdditionalSealParty = Core.Constants.ContainerSealParties.Codes.Quarantine;
			AssertEquals(ZString.Empty, destinationContainer.ACN_SealingPartyType2);
			AssertEquals(SealTypeList.Codes.MechanicalSeal, destinationContainer.ACN_SealType2);
			AssertEquals("ACN_SealingPartyType2Info.ReadOnly", false, destinationContainer.ACN_SealingPartyType2Info.ReadOnly);

			destinationContainer.ACN_SealingPartyType3 = Core.Constants.ContainerSealParties.Codes.Quarantine;
			destinationContainer.ACN_SealType3 = SealTypeList.Codes.MechanicalSeal;
			sourceContainer.JC_Additional2SealParty = Core.Constants.ContainerSealParties.Codes.Quarantine;
			AssertEquals(ZString.Empty, destinationContainer.ACN_SealingPartyType3);
			AssertEquals(SealTypeList.Codes.MechanicalSeal, destinationContainer.ACN_SealType3);
			AssertEquals("ACN_SealingPartyType3Info.ReadOnly", false, destinationContainer.ACN_SealingPartyType3Info.ReadOnly);

			destinationContainer.ACN_SealingPartyType = Core.Constants.ContainerSealParties.Codes.Quarantine;
			destinationContainer.ACN_SealType1 = SealTypeList.Codes.ElectronicSeal;
			destinationContainer.ACN_SealingPartyName = "XYZ";
			sourceContainer.JC_SealParty = Core.Constants.ContainerSealParties.Codes.Customs;
			AssertEquals(Core.Constants.ContainerSealParties.Codes.Customs, destinationContainer.ACN_SealingPartyType);
			AssertEquals(SealTypeList.Codes.ElectronicSeal, destinationContainer.ACN_SealType1);
			AssertEquals("XYZ", destinationContainer.ACN_SealingPartyName);
			AssertEquals("ACN_SealingPartyTypeInfo.ReadOnly", false, destinationContainer.ACN_SealingPartyTypeInfo.ReadOnly);

			destinationContainer.ACN_SealingPartyType2 = Core.Constants.ContainerSealParties.Codes.Quarantine;
			destinationContainer.ACN_SealType2 = SealTypeList.Codes.ElectronicSeal;
			sourceContainer.JC_AdditionalSealParty = Core.Constants.ContainerSealParties.Codes.CarrierShippingLine;
			AssertEquals(Core.Constants.ContainerSealParties.Codes.CarrierShippingLine, destinationContainer.ACN_SealingPartyType2);
			AssertEquals(SealTypeList.Codes.ElectronicSeal, destinationContainer.ACN_SealType2);
			AssertEquals("ACN_SealingPartyType2Info.ReadOnly", false, destinationContainer.ACN_SealingPartyType2Info.ReadOnly);

			destinationContainer.ACN_SealingPartyType3 = Core.Constants.ContainerSealParties.Codes.Quarantine;
			destinationContainer.ACN_SealType3 = SealTypeList.Codes.ElectronicSeal;
			sourceContainer.JC_Additional2SealParty = Core.Constants.ContainerSealParties.Codes.Terminal;
			AssertEquals(Core.Constants.ContainerSealParties.Codes.Terminal, destinationContainer.ACN_SealingPartyType3);
			AssertEquals(SealTypeList.Codes.ElectronicSeal, destinationContainer.ACN_SealType3);
			AssertEquals("ACN_SealingPartyType3Info.ReadOnly", false, destinationContainer.ACN_SealingPartyType3Info.ReadOnly);
		}
	}
}
