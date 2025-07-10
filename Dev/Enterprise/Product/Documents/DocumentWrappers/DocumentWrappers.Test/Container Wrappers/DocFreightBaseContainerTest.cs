using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.Testing.Container_Wrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.ContainerWrappers
{
	sealed class DocFreightBaseContainerTest : TestCaseWithFactory
	{
		#region Overrides

		public void TestToString()
		{
			BaseContainer.JC_ContainerNum = "ContNum";
			AssertEquals("ToString()", BaseContainer.JC_ContainerNum, BaseContainerWrapper.ToString());
		}

		public void TestSetTemplateConstants()
		{
			AssertEquals("MarksAndNumbersWidth", 1, BaseContainerWrapper.MarksAndNumbersWidth);
			AssertEquals("GoodsDescWidth", 1, BaseContainerWrapper.GoodsDescWidth);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.MarksAndNumbersWidth, 7);
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.GoodsDescriptionWidth, 16);

			BaseContainerWrapper.SetTemplateConstants(constants);
			AssertEquals("MarksAndNumbersWidth", 7, BaseContainerWrapper.MarksAndNumbersWidth);
			AssertEquals("GoodsDescWidth", 16, BaseContainerWrapper.GoodsDescWidth);
		}

		#endregion

		#region Virtual Fields

		public void TestPortOfDischarge()
		{
			AssertNull(BaseContainerWrapper.PortOfDischarge);

			JobVoyage voyage = Factory.New<JobVoyage>();

			voyage.Origins.AddNew();
			voyage.Origins[0].JA_RL_NKPortOfLoading = "HKHKG";

			voyage.Destinations.AddNew();
			voyage.Destinations[0].JB_RL_NKPortOfDischarge = "USSFO";

			BaseContainer.JC_JK = ZGuid.Empty;
			BaseContainer.JC_JX = voyage.Sailings[0].PK;
			AssertEquals("USSFO", BaseContainerWrapper.PortOfDischarge.Code);
		}

		public void TestPortOfLoading()
		{
			AssertNull(BaseContainerWrapper.PortOfLoading);

			JobVoyage voyage = Factory.New<JobVoyage>();

			voyage.Origins.AddNew();
			voyage.Origins[0].JA_RL_NKPortOfLoading = "HKHKG";

			voyage.Destinations.AddNew();
			voyage.Destinations[0].JB_RL_NKPortOfDischarge = "USSFO";

			BaseContainer.JC_JK = ZGuid.Empty;
			BaseContainer.JC_JX = voyage.Sailings[0].PK;
			AssertEquals("HKHKG", BaseContainerWrapper.PortOfLoading.Code);
		}

		#region Cartage Advice Fields

		#region Addresses

		public void TestJourneyOneDeliverToAddress()
		{
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOneDeliverToAddress", BaseContainerWrapper.JourneyOneDeliverToAddressForExport, BaseContainerWrapper.JourneyOneDeliverToAddress);

			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOneDeliverToAddress", BaseContainerWrapper.JourneyOneDeliverToAddressForImport, BaseContainerWrapper.JourneyOneDeliverToAddress);
		}

		public void TestJourneyTwoPickUpAddress()
		{
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoPickUpAddress", BaseContainerWrapper.JourneyTwoPickUpAddressForExport, BaseContainerWrapper.JourneyTwoPickUpAddress);

			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyTwoPickUpAddress", BaseContainerWrapper.JourneyTwoPickUpAddressForImport, BaseContainerWrapper.JourneyTwoPickUpAddress);
		}

		#endregion

		#region Contacts

		public void TestJourneyOnePickUpContactDetails()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);

			AssertEquals("ContactName", "AAA", baseContainerWrapperForCartageAdvice.JourneyOnePickUpContactName);
			AssertEquals("Phone", "111", baseContainerWrapperForCartageAdvice.JourneyOnePickUpContactPhone);

			baseContainerWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", baseContainerWrapperForCartageAdvice.JourneyOnePickUpContactName);
			AssertEquals("Phone", "222", baseContainerWrapperForCartageAdvice.JourneyOnePickUpContactPhone);

			baseContainerWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, baseContainerWrapperForCartageAdvice.JourneyOnePickUpContactName);
			AssertEquals("Phone", "333", baseContainerWrapperForCartageAdvice.JourneyOnePickUpContactPhone);
		}

		public void TestJourneyOneDeliverToContactDetails()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);

			baseContainerWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ContactName", "AAA", baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactName);
			AssertEquals("Phone", "111", baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);

			baseContainerWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactName);
			AssertEquals("Phone", "222", baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);

			baseContainerWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactName);
			AssertEquals("Phone", "333", baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);

			baseContainerWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactName);
			AssertEquals("Phone", "333", baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);
		}

		public void TestJourneyTwoPickUpContactDetails()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);

			baseContainerWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ContactName", "AAA", baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactName);
			AssertEquals("Phone", "111", baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);

			baseContainerWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactName);
			AssertEquals("Phone", "222", baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);

			baseContainerWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactName);
			AssertEquals("Phone", "333", baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);

			baseContainerWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactName);
			AssertEquals("Phone", "333", baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);
		}

		public void TestJourneyTwoDeliverToContactDetails()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);

			AssertEquals("ContactName", "AAA", baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToContactName);
			AssertEquals("Phone", "111", baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToContactPhone);

			baseContainerWrapperForCartageAdvice.LocalTransportContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", "BBB", baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToContactName);
			AssertEquals("Phone", "222", baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToContactPhone);

			baseContainerWrapperForCartageAdvice.AllTypeContact.Documents[0].OD_DefaultContact = false;
			AssertEquals("ContactName", ContactType.LocalTransport.DefaultName, baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToContactName);
			AssertEquals("Phone", "333", baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToContactPhone);
		}

		#region JourneyOne

		public void TestJourneyOnePickUpContactName()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);
			JobDocAddress journeyOnePickupDocAddress = (JobDocAddress)baseContainerWrapperForCartageAdvice.JourneyOnePickUpAddress.WrappedObject;
			journeyOnePickupDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyOnePickUpContactName", "AAA", baseContainerWrapperForCartageAdvice.JourneyOnePickUpContactName);
			journeyOnePickupDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyOnePickUpContactName", "BBB", baseContainerWrapperForCartageAdvice.JourneyOnePickUpContactName);
		}

		public void TestJourneyOnePickUpContactPhone()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);
			JobDocAddress journeyOnePickupDocAddress = (JobDocAddress)baseContainerWrapperForCartageAdvice.JourneyOnePickUpAddress.WrappedObject;
			journeyOnePickupDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyOnePickUpContactPhone", "111", baseContainerWrapperForCartageAdvice.JourneyOnePickUpContactPhone);
			journeyOnePickupDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyOnePickUpContactPhone", "222", baseContainerWrapperForCartageAdvice.JourneyOnePickUpContactPhone);
		}

		public void TestJourneyOneDeliverToContactName()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);
			baseContainerWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			JobDocAddress journeyOneDeliverToDocAddress = (JobDocAddress)baseContainerWrapperForCartageAdvice.JourneyOneDeliverToAddress.WrappedObject;
			journeyOneDeliverToDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyOneDeliverToContactName", "AAA", baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactName);
			journeyOneDeliverToDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyOneDeliverToContactName", "BBB", baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactName);
		}

		public void TestJourneyOneDeliverToContactPhone()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);
			baseContainerWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			JobDocAddress journeyOneDeliverToDocAddress = (JobDocAddress)baseContainerWrapperForCartageAdvice.JourneyOneDeliverToAddress.WrappedObject;
			journeyOneDeliverToDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyOneDeliverToContactPhone", "111", baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);
			journeyOneDeliverToDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyOneDeliverToContactPhone", "222", baseContainerWrapperForCartageAdvice.JourneyOneDeliverToContactPhone);
		}
		#endregion

		#region Journey Two

		public void TestJourneyTwoPickUpContactName()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);
			baseContainerWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			JobDocAddress journeyTwoPickupDocAddress = (JobDocAddress)baseContainerWrapperForCartageAdvice.JourneyTwoPickUpAddress.WrappedObject;
			journeyTwoPickupDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyTwoPickUpContactName", "AAA", baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactName);
			journeyTwoPickupDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyTwoPickUpContactName", "BBB", baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactName);
		}

		public void TestJourneyTwoPickUpContactPhone()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);
			baseContainerWrapperForCartageAdvice.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			JobDocAddress journeyTwoPickupDocAddress = (JobDocAddress)baseContainerWrapperForCartageAdvice.JourneyTwoPickUpAddress.WrappedObject;
			journeyTwoPickupDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyTwoPickUpContactPhone", "111", baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);
			journeyTwoPickupDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyTwoPickUpContactPhone", "222", baseContainerWrapperForCartageAdvice.JourneyTwoPickUpContactPhone);
		}

		public void TestJourneyTwoDeliverToContactName()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);
			JobDocAddress journeyTwoDeliverToDocAddress = (JobDocAddress)baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToAddress.WrappedObject;
			journeyTwoDeliverToDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyTwoDeliverToContactName", "AAA", baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToContactName);
			journeyTwoDeliverToDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyTwoDeliverToContactName", "BBB", baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToContactName);
		}

		public void TestJourneyTwoDeliverToContactPhone()
		{
			DocFreightBaseContainerHelperClassForCartageAdvice baseContainerWrapperForCartageAdvice = new DocFreightBaseContainerHelperClassForCartageAdvice(BaseContainer, Factory);
			JobDocAddress journeyTwoDeliverToDocAddress = (JobDocAddress)baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToAddress.WrappedObject;
			journeyTwoDeliverToDocAddress.E2_Contact = "AAA";
			AssertEquals("JourneyTwoDeliverToContactPhone", "111", baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToContactPhone);
			journeyTwoDeliverToDocAddress.E2_Contact = "BBB";
			AssertEquals("JourneyTwoDeliverToContactPhone", "222", baseContainerWrapperForCartageAdvice.JourneyTwoDeliverToContactPhone);
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region ZString

		public void TestContainerNumberOrTypeCount()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonConsol consol1 = shipment.Consols.AddNew();
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			var containerType1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container1.JC_RC = containerType1.PK;
			PackLine aPackLine = shipment.OuterPackLines.AddNew();
			aPackLine.SetContainer(consol1, container1);
			aPackLine.CurrentConsol = consol1;
			DocContainer container1Wrapper = DocContainer.New(container1, Factory);
			AssertEquals("ContainerCode should have container 1 details: ", "20GP (2)", container1Wrapper.ContainerNumberOrTypeCount);
			container1.JC_ContainerNum = "111";
			AssertEquals("ContainerCode should have container 1 number: ", "111", container1Wrapper.ContainerNumberOrTypeCount);
		}

		public void TestTotalVolumeUnit()
		{
			AssertEquals("TotalVolumeUnit", BaseContainer.JC_Calc_TotalVolumeUnit, BaseContainerWrapper.TotalVolumeUnit);
		}

		public void TestTotalWeightUnit()
		{
			AssertEquals("TotalWeightUnit", BaseContainer.JC_Calc_TotalWeightUnit, BaseContainerWrapper.TotalWeightUnit);
		}

		public void TestTotalPackagesUnit()
		{
			AssertEquals("TotalPackagesUnit", BaseContainer.JC_Calc_TotalPackagesUnit, BaseContainerWrapper.TotalPackagesUnit);
		}

		public void TestAdditionalSealNum()
		{
			ZString additionalSealNum = new ZString("AdditionalSealNum");
			BaseContainer.JC_AdditionalSealNum = additionalSealNum;
			AssertEquals("AdditionalSealNum", additionalSealNum, BaseContainerWrapper.AdditionalSealNum);
		}

		public void TestAirVent()
		{
			BaseContainer.JC_AirVentFlow = 12;
			BaseContainer.JC_AirVentFlowRateUnit = "MQM";
			AssertEquals("AirVent 12 MQM", "12 MQM", BaseContainerWrapper.AirVent);

			BaseContainer.JC_AirVentFlow = 0;
			AssertEquals("AirVent is closed", "CLOSED", BaseContainerWrapper.AirVent);
		}

		public void TestArrivalTruckDriversLicense()
		{
			ZString arrivalTruckDriversLicense = new ZString("15CHARACTERSMAX");
			BaseContainer.DestinationCFSArrival.EU_DriversLicence = arrivalTruckDriversLicense;
			AssertEquals("ArrivalTruckDriversLicense", arrivalTruckDriversLicense, BaseContainerWrapper.ArrivalTruckDriversLicense);
		}

		public void TestArrivalTruckRegistration()
		{
			ZString arrivalTruckRegistration = new ZString("ArrReg");
			BaseContainer.DestinationCFSArrival.EU_VehicleRegistration = arrivalTruckRegistration;
			AssertEquals("ArrivalTruckRegistration", arrivalTruckRegistration, BaseContainerWrapper.ArrivalTruckRegistration);
		}

		public void TestClipOnUnit()
		{
			ZString clipOnUnit = new ZString("CLIP01");
			BaseContainer.JC_RefrigGeneratorID = clipOnUnit;
			AssertEquals("ClipOnUnit", clipOnUnit, BaseContainerWrapper.ClipOnUnit);
		}

		public void TestContainerJobID()
		{
			ZString containerJobID = new ZString("ContainerJobID");
			BaseContainer.JC_ContainerJobID = containerJobID;
			AssertEquals("ContainerJobID", containerJobID, BaseContainerWrapper.ContainerJobID);
		}

		public void TestContainerMode()
		{
			ZString containerMode = new ZString("CCC");
			BaseContainer.JC_ContainerMode = containerMode;
			AssertEquals("ContainerMode", containerMode, BaseContainerWrapper.ContainerMode);
		}

		public void TestContainerNum()
		{
			ZString containerNum = "FCRU3939382";
			BaseContainer.JC_ContainerNum = containerNum;
			AssertEquals("ContainerNum", containerNum, BaseContainerWrapper.ContainerNumber);
		}

		public void TestContainerRating()
		{
			ZString containerRating = new ZString("Cont");
			BaseContainer.JC_ContainerRating = containerRating;
			AssertEquals("ContainerRating", containerRating, BaseContainerWrapper.ContainerRating);
		}

		public void TestContainerStatus()
		{
			ZString containerStatus = new ZString("SSS");
			BaseContainer.JC_ContainerStatus = containerStatus;
			AssertEquals("ContainerStatus", containerStatus, BaseContainerWrapper.ContainerStatus);
		}

		public void TestDeliveryMode()
		{
			var registryDeliveryList = FreightDataRegistry.Instance.ContainerDeliveryModeList.Value;
			registryDeliveryList.Add(new DeliveryMode { Code = "XYZ", UserDefinedCode = "UXYZ", Description = (NoResString)"XYZ DESC", UserDefinedDescription = (NoResString)"USER DESC" });

			using (FreightDataRegistry.Instance.ContainerDeliveryModeList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDeliveryList))
			{
				BaseContainer.JC_DeliveryMode = Core.Constants.DeliveryModes.Descriptions.CY_CFS;
				AssertEquals("DeliveryMode", Core.Constants.DeliveryModes.Descriptions.CY_CFS, BaseContainerWrapper.DeliveryMode);

				BaseContainer.JC_DeliveryMode = "XY";
				AssertEquals("DeliveryMode", "XY", BaseContainerWrapper.DeliveryMode);
			}
		}

		public void TestDeliveryModeDescription()
		{
			var registryDeliveryList = FreightDataRegistry.Instance.ContainerDeliveryModeList.Value;
			registryDeliveryList.Add(new DeliveryMode { Code = "ABC", UserDefinedCode = "ABC" });
			registryDeliveryList.Add(new DeliveryMode { Code = "EFG", UserDefinedCode = "EFG", Description = (NoResString)"Blah blah blah", UserDefinedDescription = (NoResString)"Blah blah blah" });
			registryDeliveryList.Add(new DeliveryMode { Code = "XYZ", UserDefinedCode = "UXYZ", Description = (NoResString)"XYZ DESC", UserDefinedDescription = (NoResString)"USER DESC" });

			using (FreightDataRegistry.Instance.ContainerDeliveryModeList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDeliveryList))
			{
				CommonConsol consol = Factory.New<CommonConsol>();
				BaseContainer.JC_JK = consol.PK;
				consol.JK_TransportMode = Core.Constants.TransportModes.Rail;

				BaseContainer.JC_DeliveryMode = "";
				AssertEquals("Delivery Mode Description", ZString.Empty, BaseContainerWrapper.DeliveryModeDescription);

				BaseContainer.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
				AssertEquals("Delivery Mode Description", Core.Constants.DeliveryModes.Descriptions.CY_CY, BaseContainerWrapper.DeliveryModeDescription);

				BaseContainer.JC_DeliveryMode = "ABC";
				AssertEquals("Delivery Mode Description", "ABC", BaseContainerWrapper.DeliveryModeDescription);

				BaseContainer.JC_DeliveryMode = "EFG";
				AssertEquals("Delivery Mode Description", "Blah blah blah", BaseContainerWrapper.DeliveryModeDescription);

				BaseContainer.JC_DeliveryMode = "XYZ";
				AssertEquals("Delivery Mode Description", "XYZ DESC", BaseContainerWrapper.DeliveryModeDescription);
			}
		}

		public void TestDepartureTruckDriversLicense()
		{
			ZString departureTruckDriversLicense = new ZString("15CHARACTERSMAX");
			BaseContainer.DestinationCFSDeparture.EU_DriversLicence = departureTruckDriversLicense;
			AssertEquals("DepartureTruckDriversLicense", departureTruckDriversLicense, BaseContainerWrapper.DepartureTruckDriversLicense);
		}

		public void TestDepartureTruckRegistration()
		{
			ZString departureTruckRegistration = new ZString("Depart");
			BaseContainer.DestinationCFSDeparture.EU_VehicleRegistration = departureTruckRegistration;
			AssertEquals("DepartureTruckRegistration", departureTruckRegistration, BaseContainerWrapper.DepartureTruckRegistration);
		}

		public void TestDetentionLiabilityWarningText()
		{
			ZString warning = "Detention Liablility Warning Text";
			DocumentsDataRegistry.Instance.ContainerLiabilityStatementLiabilityWarningText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, warning);
			AssertEquals("DetentionLiabilityWarningText", warning, BaseContainerWrapper.DetentionLiabilityWarningText);
		}

		public void TestDetentionLiabilityAcceptanceText()
		{
			ZString acceptance = "Detention Liablility Acceptance Text";
			DocumentsDataRegistry.Instance.ContainerLiabilityAcceptanceText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, acceptance);
			AssertEquals("DetentionLiabilityAcceptanceText", acceptance, BaseContainerWrapper.DetentionLiabilityAcceptanceText);
		}

		public void TestExportDepotCustomsReference()
		{
			ZString exportDepotCustomsReference = new ZString("ExportDepotCustoms");
			BaseContainer.JC_ExportDepotCustomsReference = exportDepotCustomsReference;
			AssertEquals("ExportDepotCustomsReference", exportDepotCustomsReference, BaseContainerWrapper.ExportDepotCustomsReference);
		}

		public void TestGrossWeightUQ()
		{
			ZString grossWeightUQ = new ZString("UQ");
			BaseContainer.JC_GrossWeightUQ = grossWeightUQ;
			AssertEquals("GrossWeightUQ", "KG", BaseContainerWrapper.GrossWeightUQ);
		}

		public void TestTareWeightUQ()
		{
			ZString grossWeightUQ = new ZString("UQ");
			BaseContainer.JC_GrossWeightUQ = grossWeightUQ;
			AssertEquals("TareWeightUQ should use Gross weight UQ", "KG", BaseContainerWrapper.TareWeightUQ);
		}

		public void TestWeightUQ()
		{
			BaseContainer.JC_GrossWeightUQ = Constants.Weight.Tonnes;
			AssertEquals("WeightUQ should be Tonnes", Constants.Weight.Tonnes, BaseContainerWrapper.WeightUQ);

			BaseContainer.JC_GrossWeightUQ = Constants.Weight.Ounces;
			AssertEquals("WeightUQ should be Ounces", Constants.Weight.Ounces, BaseContainerWrapper.WeightUQ);

			BaseContainer.JC_GrossWeightUQ = "XX";
			AssertEquals("WeightUQ should be Kilograms", Constants.Weight.Kilograms, BaseContainerWrapper.WeightUQ);
		}

		public void TestMasterBillNumber()
		{
			ZString masterBillNumber = "MasterBillNumber";
			AssertEquals("MasterBillNumber", true, BaseContainerWrapper.MasterBillNumber.IsEmpty);
			Consol.JK_MasterBillNum = masterBillNumber;
			AssertEquals("MasterBillNumber after attaching to consol", masterBillNumber, BaseContainerWrapper.MasterBillNumber);
		}

		public void TestSealNumber()
		{
			BaseContainer.JC_SealNum = "SealNum1";
			AssertEquals("SealNumber", "SealNum1", BaseContainerWrapper.SealNumber);
		}

		public void TestSealNumber2()
		{
			BaseContainer.JC_AdditionalSealNum = "SealNum2";
			AssertEquals("SealNumber2", "SealNum2", BaseContainerWrapper.SealNumber2);
		}

		public void TestSealNumber3()
		{
			BaseContainer.JC_Additional2SealNum = "SealNum3";
			AssertEquals("SealNumber3", "SealNum3", BaseContainerWrapper.SealNumber3);
		}

		public void TestSetPointTemp()
		{
			BaseContainer.JC_SetPointTemp = -4.4m;
			AssertEquals("SetPointTemp", -4.4m, BaseContainerWrapper.SetPointTemp);
		}

		public void TestSetPointTempUnit()
		{
			BaseContainer.JC_SetPointTempUnit = "C";
			AssertEquals("SetPointTempUnit", "C", BaseContainerWrapper.SetPointTempUnit);
		}

		public void TestTempRecorderSerialNo()
		{
			ZString tempRecorderSerialNo = "12345678910";
			BaseContainer.JC_TempRecorderSerialNo = tempRecorderSerialNo;
			AssertEquals("TempRecorderSerialNo", tempRecorderSerialNo, BaseContainerWrapper.TempRecorderSerialNo);
		}

		public void TestSlotReference()
		{
			ZString slotReference = new ZString("SlotReference");
			BaseContainer.JC_ArrivalSlotReference = slotReference;
			AssertEquals("SlotReference", slotReference, BaseContainerWrapper.SlotReference);
		}

		public void TestPurpose()
		{
			ZString purpose = new ZString("PPP");
			BaseContainer.JC_Purpose = purpose;
			AssertEquals("Purpose", purpose, BaseContainerWrapper.Purpose);
		}

		public void TestUnpackGang()
		{
			ZString unpackGang = new ZString("UnpackGang");
			BaseContainer.JC_UnpackGang = unpackGang;
			AssertEquals("UnpackGang", unpackGang, BaseContainerWrapper.UnpackGang);
		}

		public void TestVolumeCapacityUQ()
		{
			ZString volumeCapacityUQ = new ZString("UQ");
			BaseContainer.JC_VolumeCapacityUQ = volumeCapacityUQ;
			AssertEquals("VolumeCapacityUQ", volumeCapacityUQ, BaseContainerWrapper.VolumeCapacityUQ);
		}

		public void TestWeightCapacityUQ()
		{
			ZString weightCapacityUQ = new ZString("UQ");
			BaseContainer.JC_WeightCapacityUQ = weightCapacityUQ;
			AssertEquals("WeightCapacityUQ", weightCapacityUQ, BaseContainerWrapper.WeightCapacityUQ);
		}

		public void TestReleaseNum()
		{
			ZString releaseNum = new ZString("ReleaseNum");

			BaseContainer.@JC_ReleaseNum = releaseNum;
			AssertEquals("ReleaseNum", releaseNum, BaseContainerWrapper.ReleaseNum);
		}

		public void TestArrivalReleaseNum()
		{
			ZString arrivalReleaseNum = "ArrRelNo";

			BaseContainer.JC_ContainerImportDORelease = arrivalReleaseNum;
			AssertEquals("ArrivalReleaseNum", arrivalReleaseNum, BaseContainerWrapper.ArrivalReleaseNum);
		}

		public void TestTotalWeight()
		{
			AssertEquals("TotalWeight", BaseContainer.JC_Calc_TotalWeight.ToString(), BaseContainerWrapper.TotalWeight);
		}

		public void TestBBKTotalWeight()
		{
			AssertEquals("BBKTotalWeight", "", BaseContainerWrapper.BBKTotalWeight);

			var shipment = Consol.Shipments.AddNew();
			var pack = shipment.OuterPackLines.AddNew();
			pack.SetContainer(Consol, BaseContainer);

			AssertEquals("BBKTotalWeight", "", BaseContainerWrapper.BBKTotalWeight);

			pack.JL_ActualWeight = 2357;
			pack.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("BBKTotalWeight", "", BaseContainerWrapper.BBKTotalWeight);

			BaseContainerWrapper = new DocFreightBaseContainerHelperClass(BaseContainer, Factory);
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Bulk;
			AssertEquals("BBKTotalWeight", "2.357", BaseContainerWrapper.BBKTotalWeight);

			BaseContainerWrapper = new DocFreightBaseContainerHelperClass(BaseContainer, Factory);
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("BBKTotalWeight", "2.357", BaseContainerWrapper.BBKTotalWeight);

			BaseContainerWrapper = new DocFreightBaseContainerHelperClass(BaseContainer, Factory);
			Consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals("BBKTotalWeight", "2.357", BaseContainerWrapper.BBKTotalWeight);
		}

		public void TestReportName()
		{
			BaseContainerWrapper.SetReportNameForTesting(ZString.Empty);
			AssertEquals("ReportName", ZString.Empty, BaseContainerWrapper.ReportName);

			BaseContainerWrapper.SetReportNameForTesting("TestName");
			AssertEquals("ReportName", "TestName", BaseContainerWrapper.ReportName);
		}

		public void TestDocumentDirection()
		{
			BaseContainerWrapper.SetDocumentDirectionForTesting(ZString.Empty);
			AssertEquals("DocumentDirection", ZString.Empty, BaseContainerWrapper.DocumentDirection);

			BaseContainerWrapper.SetDocumentDirectionForTesting("TestName");
			AssertEquals("DocumentDirection", "TestName", BaseContainerWrapper.DocumentDirection);
		}

		public void TestCommodityDescription()
		{
			AssertEquals("No Commodity", "", BaseContainerWrapper.CommodityDescription);

			BaseContainer.JC_RH_NKContainerCommodityCode = Core.Constants.CargoTypes.General;
			AssertEquals("Commodity General", "General", BaseContainerWrapper.CommodityDescription);

			BaseContainer.JC_RH_NKContainerCommodityCode = "TST";
			AssertEquals("Commodity Code only", "TST", BaseContainerWrapper.CommodityDescription);
		}

		public void TestUnpackShed()
		{
			ZString testUnpackShedValue = "Test";
			BaseContainer.JC_UnpackShed = testUnpackShedValue;
			AssertEquals("Unpack Shed", testUnpackShedValue, BaseContainerWrapper.UnpackShed);
		}

		public void TestContainerStorageLocation()
		{
			ZString testLocation = "Test";
			BaseContainer.JC_ContainerStorageLocation = testLocation;
			AssertEquals("Storage Location", testLocation, BaseContainerWrapper.ContainerStorageLocation);
		}

		public void TestSize()
		{
			RefContainer @ref = Factory.New<RefContainer>();
			BaseContainer.JC_RC = @ref.PK;
			@ref.RC_Code = "20FR";
			AssertEquals("Size should be 20FR", "20FR", BaseContainerWrapper.Size);
		}

		#endregion

		#region ZDecimal

		public void TestTotalVolume()
		{
			AssertEquals("TotalVolume", BaseContainer.JC_Calc_TotalVolume, BaseContainerWrapper.TotalVolume);
		}

		public void TestContainerCapacity()
		{
			AssertEquals("ContainerCapacity", BaseContainer.JC_Calc_ContainerCapacity, BaseContainerWrapper.ContainerCapacity);
		}

		public void TestMaxGrossWeight()
		{
			AssertEquals("MaxGrossWeight", BaseContainer.JC_Calc_MaxGrossWeight, BaseContainerWrapper.MaxGrossWeight);
		}

		public void TestCalcTareWeight()
		{
			AssertEquals("CalcTareWeight", BaseContainer.JC_Calc_TareWeight, BaseContainerWrapper.CalcTareWeight);
		}

		public void TestNetWeight()
		{
			AssertEquals("NetWeight", BaseContainer.JC_Calc_NetWeight, BaseContainerWrapper.NetWeight);
		}

		public void TestLength()
		{
			AssertEquals("Length", BaseContainer.JC_Calc_Length, BaseContainerWrapper.Length);
		}

		public void TestWidth()
		{
			AssertEquals("Width", BaseContainer.JC_Calc_Width, BaseContainerWrapper.Width);
		}

		public void TestHeight()
		{
			AssertEquals("Height", BaseContainer.JC_Calc_Height, BaseContainerWrapper.Height);
		}

		public void TestOLength()
		{
			RefContainer @ref = Factory.New<RefContainer>();
			BaseContainer.JC_RC = @ref.PK;
			@ref.RC_Length = 10;
			BaseContainer.JC_TotalLength = 10;
			AssertEquals("OLength", "", BaseContainerWrapper.OLength);

			BaseContainer.JC_TotalLength = 8;
			AssertEquals("OLength", "", BaseContainerWrapper.OLength);

			BaseContainer.JC_TotalLength = 13;
			AssertEquals("OLength", "91.4", BaseContainerWrapper.OLength);
		}

		public void TestOWidth()
		{
			RefContainer @ref = Factory.New<RefContainer>();
			BaseContainer.JC_RC = @ref.PK;
			@ref.RC_Width = 15;
			BaseContainer.JC_TotalWidth = 15;
			AssertEquals("OWidth", "", BaseContainerWrapper.OWidth);

			BaseContainer.JC_TotalWidth = 11;
			AssertEquals("OWidth", "", BaseContainerWrapper.OWidth);

			BaseContainer.JC_TotalWidth = 17;
			AssertEquals("OWidth", "61.0", BaseContainerWrapper.OWidth);
		}

		public void TestOHeight()
		{
			RefContainer @ref = Factory.New<RefContainer>();
			BaseContainer.JC_RC = @ref.PK;
			@ref.RC_Height = 21;
			BaseContainer.JC_TotalHeight = 21;
			AssertEquals("OHeight", "", BaseContainerWrapper.OHeight);

			BaseContainer.JC_TotalHeight = 19;
			AssertEquals("OHeight", "", BaseContainerWrapper.OHeight);

			BaseContainer.JC_TotalHeight = 30;
			AssertEquals("OHeight", "274.3", BaseContainerWrapper.OHeight);
		}

		public void TestTareWeight()
		{
			ZDecimal tareWeight = new ZDecimal(1);
			BaseContainer.JC_TareWeight = tareWeight;
			AssertEquals("TareWeight", tareWeight, BaseContainerWrapper.TareWeight);
		}

		public void TestDemurrage()
		{
			ZDecimal demurrage = new ZDecimal(1);
			BaseContainer.ArrivalTruckWaitCost = demurrage;
			AssertEquals("Demurrage", demurrage, BaseContainerWrapper.Demurrage);
		}

		public void TestDunnageWeight()
		{
			ZDecimal dunnageWeight = new ZDecimal(1);
			BaseContainer.JC_DunnageWeight = dunnageWeight;
			AssertEquals("DunnageWeight", dunnageWeight, BaseContainerWrapper.DunnageWeight);
		}

		public void TestGrossWeight()
		{
			ZDecimal grossWeight = new ZDecimal(1);
			BaseContainer.JC_GrossWeight = grossWeight;
			AssertEquals("GrossWeight", grossWeight, BaseContainerWrapper.GrossWeight);
		}

		public void TestTotalHeight()
		{
			ZDecimal totalHeight = new ZDecimal(1);
			BaseContainer.JC_TotalHeight = totalHeight;
			AssertEquals("TotalHeight", totalHeight, BaseContainerWrapper.TotalHeight);
		}

		public void TestTotalLength()
		{
			ZDecimal totalLength = new ZDecimal(1);
			BaseContainer.JC_TotalLength = totalLength;
			AssertEquals("TotalLength", totalLength, BaseContainerWrapper.TotalLength);
		}

		public void TestTotalWidth()
		{
			ZDecimal totalWidth = new ZDecimal(1);
			BaseContainer.JC_TotalWidth = totalWidth;
			AssertEquals("TotalWidth", totalWidth, BaseContainerWrapper.TotalWidth);
		}

		public void TestVolumeCapacity()
		{
			ZDecimal volumeCapacity = new ZDecimal(1);
			BaseContainer.JC_VolumeCapacity = volumeCapacity;
			AssertEquals("VolumeCapacity", volumeCapacity, BaseContainerWrapper.VolumeCapacity);
		}

		public void TestWeightCapacity()
		{
			ZDecimal weightCapacity = new ZDecimal(1);
			BaseContainer.JC_WeightCapacity = weightCapacity;
			AssertEquals("WeightCapacity", weightCapacity, BaseContainerWrapper.WeightCapacity);
		}

		#endregion

		#region ZInt

		public void TestTotalPackages()
		{
			AssertEquals("TotalPackages", BaseContainer.JC_Calc_TotalPackages, BaseContainerWrapper.TotalPackages);
		}

		public void TestQuantityCount()
		{
			AssertEquals("Initial container count", 1, BaseContainerWrapper.QuantityCount);

			BaseContainer.JC_ContainerCount = 10;
			BaseContainerWrapper = new DocFreightBaseContainerHelperClass(BaseContainer, Factory);
			AssertEquals("Container count", 10, BaseContainerWrapper.QuantityCount);
		}

		#endregion

		#region ZDateTime Fields

		#region Cartage Advice Fields

		public void TestDropOffEmpty()
		{
			AssertEquals("DropOffEmpty", ZDateTime.Empty, BaseContainerWrapper.DropOffEmpty);

			BaseContainer.JC_EmptyRequired = new ZDateTime(2004, 04, 04);
			AssertEquals("DropOffEmpty", ZDateTime.Empty, BaseContainerWrapper.DropOffEmpty);

			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("DropOffEmpty", BaseContainer.JC_EmptyRequired, BaseContainerWrapper.DropOffEmpty);

			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("DropOffEmpty", ZDateTime.Empty, BaseContainerWrapper.DropOffEmpty);
		}

		public void TestReturnEmpty()
		{
			AssertEquals("ReturnEmpty", ZDateTime.Empty, BaseContainerWrapper.ReturnEmpty);

			BaseContainer.JC_EmptyReturnedBy = new ZDateTime(2004, 04, 04);
			AssertEquals("ReturnEmpty", ZDateTime.Empty, BaseContainerWrapper.ReturnEmpty);

			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ReturnEmpty", BaseContainer.JC_EmptyReturnedBy, BaseContainerWrapper.ReturnEmpty);

			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("ReturnEmpty", ZDateTime.Empty, BaseContainerWrapper.ReturnEmpty);
		}

		public void TestPickUpFull()
		{
			AssertEquals("PickUpFull", ZDateTime.Empty, BaseContainerWrapper.PickUpFull);

			BaseContainer.JC_DepartureEstimatedPickup = new ZDateTime(2004, 02, 02);
			AssertEquals("PickUpFull", ZDateTime.Empty, BaseContainerWrapper.PickUpFull);

			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("PickUpFull", BaseContainer.JC_DepartureEstimatedPickup, BaseContainerWrapper.PickUpFull);

			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("PickUpFull", ZDateTime.Empty, BaseContainerWrapper.PickUpFull);
		}

		#endregion

		public void TestArrivalCartageAdvised()
		{
			ZDateTime arrivalCartageAdvised = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_ArrivalCartageAdvised = arrivalCartageAdvised;
			AssertEquals("ArrivalCartageAdvised", arrivalCartageAdvised, BaseContainerWrapper.ArrivalCartageAdvised);
		}

		public void TestArrivalCartageComplete()
		{
			ZDateTime arrivalCartageComplete = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_ArrivalCartageComplete = arrivalCartageComplete;
			AssertEquals("ArrivalCartageComplete", arrivalCartageComplete, BaseContainerWrapper.ArrivalCartageComplete);
		}

		public void TestContainerAvailable()
		{
			ZDateTime containerAvailable = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_FCLAvailable = containerAvailable;
			AssertEquals("ContainerAvailable", containerAvailable, BaseContainerWrapper.ContainerAvailable);
		}

		public void TestDepartureCartageAdvised()
		{
			ZDateTime departureCartageAdvised = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_DepartureCartageAdvised = departureCartageAdvised;
			AssertEquals("DepartureCartageAdvised", departureCartageAdvised, BaseContainerWrapper.DepartureCartageAdvised);
		}

		public void TestDepartureCartageComplete()
		{
			ZDateTime departureCartageComplete = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_DepartureCartageComplete = departureCartageComplete;
			AssertEquals("DepartureCartageComplete", departureCartageComplete, BaseContainerWrapper.DepartureCartageComplete);
		}

		public void TestDepartureEstimatedPickup()
		{
			AssertEquals("DepartureEstimatedPickup", ZDateTime.Empty, BaseContainerWrapper.DepartureEstimatedPickup);

			BaseContainer.JC_DepartureEstimatedPickup = new ZDateTime(2004, 02, 02);
			AssertEquals("DepartureEstimatedPickup", BaseContainer.JC_DepartureEstimatedPickup, BaseContainerWrapper.DepartureEstimatedPickup);
		}

		public void TestContainerParkEmptyReturnGateIn()
		{
			ZDateTime containerParkEmptyReturnGateIn = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_ContainerYardEmptyReturnGateIn = containerParkEmptyReturnGateIn;
			AssertEquals("ContainerParkEmptyReturnGateIn", containerParkEmptyReturnGateIn, BaseContainerWrapper.ContainerParkEmptyReturnGateIn);
		}

		public void TestEmptyRequired()
		{
			ZDateTime emptyRequired = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_EmptyRequired = emptyRequired;
			AssertEquals("EmptyRequired", emptyRequired, BaseContainerWrapper.EmptyRequired);
		}

		public void TestEmptyReturnedBy()
		{
			ZDateTime emptyReturnedBy = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_EmptyReturnedBy = emptyReturnedBy;
			AssertEquals("EmptyReturnedBy", emptyReturnedBy, BaseContainerWrapper.EmptyReturnedBy);
		}

		public void TestEstimatedDelivery()
		{
			ZDateTime estimatedDelivery = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_ArrivalEstimatedDelivery = estimatedDelivery;
			AssertEquals("EstimatedDelivery", estimatedDelivery, BaseContainerWrapper.EstimatedDelivery);
		}

		public void TestFullPickDate()
		{
			ZDateTime fullPickDate = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_DepartureEstimatedPickup = fullPickDate;
			AssertEquals("FullPickDate", fullPickDate, BaseContainerWrapper.FullPickDate);
		}

		public void TestLCLAvailable()
		{
			ZDateTime lCLAvailable = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_LCLAvailable = lCLAvailable;
			AssertEquals("LCLAvailable", lCLAvailable, BaseContainerWrapper.LCLAvailable);
		}

		public void TestLCLStorageCommences()
		{
			ZDateTime lCLStorageCommences = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_LCLStorageCommences = lCLStorageCommences;
			AssertEquals("LCLStorageCommences", lCLStorageCommences, BaseContainerWrapper.LCLStorageCommences);
		}

		public void TestLCLUnpack()
		{
			ZDateTime lCLUnpack = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_LCLUnpack = lCLUnpack;
			AssertEquals("LCLUnpack", lCLUnpack, BaseContainerWrapper.LCLUnpack);
		}

		public void TestPackDate()
		{
			ZDateTime packDate = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_PackDate = packDate;
			AssertEquals("PackDate", packDate, BaseContainerWrapper.PackDate);
		}

		public void TestSlotDate()
		{
			ZDateTime slotDate = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_ArrivalSlotDateTime = slotDate;
			AssertEquals("SlotDate", slotDate, BaseContainerWrapper.SlotDate);
		}

		public void TestArrivalSlotDate()
		{
			ZDateTime arrivalSlotDate = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_ArrivalSlotDateTime = arrivalSlotDate;
			AssertEquals("ArrivalSlotDate", arrivalSlotDate, BaseContainerWrapper.ArrivalSlotDate);
		}

		public void TestDepartureSlotDate()
		{
			ZDateTime departureSlotDate = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_DepartureSlotDateTime = departureSlotDate;
			AssertEquals("DepartureSlotDate", departureSlotDate, BaseContainerWrapper.DepartureSlotDate);
		}

		public void TestStorageCommences()
		{
			ZDateTime storageCommences = new ZDateTime(2004, 02, 02);
			BaseContainer.JC_ArrivalCTOStorageStartDate = storageCommences;
			AssertEquals("StorageCommences", storageCommences, BaseContainerWrapper.StorageCommences);
		}

		#endregion

		#region ZShort

		public void TestContainerCount()
		{
			BaseContainer.JC_ContainerCount = 1;
			AssertEquals("ContainerCount", "1", BaseContainerWrapper.ContainerCount.ToString());
		}

		#endregion

		#region ZByte Fields

		public void TestHumidityPercent()
		{
			BaseContainer.JC_HumidityPercent = 1;
			AssertEquals("HumidityPercent", "1", BaseContainerWrapper.HumidityPercent.ToString());
		}

		#endregion

		#region ZBool Fields

		#region Cartage Advice

		public void TestIsCFSCartageAdvice()
		{
			BaseContainerWrapper.SetReportNameForTesting(ZString.Empty);
			Assert("!IsCFSCartageAdvice", !BaseContainerWrapper.IsCFSCartageAdvice);

			BaseContainerWrapper.SetReportNameForTesting("CFS Cartage Advice");
			Assert("IsCFSCartageAdvice", BaseContainerWrapper.IsCFSCartageAdvice);
		}

		#endregion

		public void TestIsCFSRegistered()
		{
			BaseContainer.JC_IsCFSRegistered = ZBool.False;
			Assert("!IsCFSRegistered", !BaseContainerWrapper.IsCFSRegistered);

			BaseContainer.JC_IsCFSRegistered = ZBool.True;
			Assert("IsCFSRegistered", BaseContainerWrapper.IsCFSRegistered);
		}

		public void TestIsChiller()
		{
			BaseContainer.IsChiller = ZBool.False;
			Assert("!IsChiller", !BaseContainerWrapper.IsChiller);

			BaseContainer.IsChiller = ZBool.True;
			Assert("IsChiller", BaseContainerWrapper.IsChiller);
		}

		public void TestIsCleaningRequired()
		{
			Assert("!IsCleaningRequired", !BaseContainerWrapper.IsCleaningRequired);

			JobService cleaning = BaseContainer.Services.AddNew();
			cleaning.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Cleaning;
			Assert("IsCleaningRequired", BaseContainerWrapper.IsCleaningRequired);
		}

		public void TestIsControlledAtmosphere()
		{
			BaseContainer.JC_IsControlledAtmosphere = ZBool.False;
			Assert("!IsControlledAtmosphere", !BaseContainerWrapper.IsControlledAtmosphere);

			BaseContainer.JC_IsControlledAtmosphere = ZBool.True;
			Assert("IsControlledAtmosphere", BaseContainerWrapper.IsControlledAtmosphere);
		}

		public void TestIsCustomsHold()
		{
			Assert("!IsCustomsHold", !BaseContainerWrapper.IsCustomsHold);

			JobService customsHold = BaseContainer.Services.AddNew();
			customsHold.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.CustomsHold;
			Assert("IsCustomsHold", BaseContainerWrapper.IsCustomsHold);
		}

		public void TestIsDamaged()
		{
			BaseContainer.JC_IsDamaged = ZBool.False;
			Assert("!IsDamaged", !BaseContainerWrapper.IsDamaged);

			BaseContainer.JC_IsDamaged = ZBool.True;
			Assert("IsDamaged", BaseContainerWrapper.IsDamaged);
		}

		public void TestIsEmptyContainer()
		{
			BaseContainer.JC_IsEmptyContainer = ZBool.False;
			Assert("!IsEmptyContainer", !BaseContainerWrapper.IsEmptyContainer);

			BaseContainer.JC_IsEmptyContainer = ZBool.True;
			Assert("IsEmptyContainer", BaseContainerWrapper.IsEmptyContainer);
		}

		public void TestIsExtraInspectionRequired()
		{
			Assert("!IsExtraInspectionRequired", !BaseContainerWrapper.IsExtraInspectionRequired);

			JobService extraInspection = BaseContainer.Services.AddNew();
			extraInspection.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.ExtraInspection;
			Assert("IsExtraInspectionRequired", BaseContainerWrapper.IsExtraInspectionRequired);
		}

		public void TestIsFrozen()
		{
			BaseContainer.IsFreezer = ZBool.False;
			Assert("!IsFrozen", !BaseContainerWrapper.IsFrozen);

			BaseContainer.IsFreezer = ZBool.True;
			Assert("IsFrozen", BaseContainerWrapper.IsFrozen);
		}

		public void TestIsFumigationRequired()
		{
			Assert("!IsFumigationRequired", !BaseContainerWrapper.IsFumigationRequired);

			JobService fumigation = BaseContainer.Services.AddNew();
			fumigation.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			Assert("IsFumigationRequired", BaseContainerWrapper.IsFumigationRequired);
		}

		public void TestIsQuarantineRequired()
		{
			Assert("!IsQuarantineRequired", !BaseContainerWrapper.IsQuarantineRequired);

			JobService quarantine = BaseContainer.Services.AddNew();
			quarantine.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			Assert("IsQuarantineRequired", BaseContainerWrapper.IsQuarantineRequired);
		}

		public void TestIsQuarantineUnpackRequired()
		{
			Assert("!IsQuarantineUnpackRequired", !BaseContainerWrapper.IsQuarantineUnpackRequired);

			JobService unpack = BaseContainer.Services.AddNew();
			unpack.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.QuarantineUnpack;
			Assert("IsQuarantineUnpackRequired", BaseContainerWrapper.IsQuarantineUnpackRequired);
		}

		public void TestIsSealOk()
		{
			BaseContainer.JC_IsSealOk = ZBool.False;
			Assert("!IsSealOk", !BaseContainerWrapper.IsSealOk);

			BaseContainer.JC_IsSealOk = ZBool.True;
			Assert("IsSealOk", BaseContainerWrapper.IsSealOk);
		}

		public void TestIsSteamCleanRequired()
		{
			Assert("!IsSteamCleanRequired", !BaseContainerWrapper.IsSteamCleanRequired);

			JobService steamClean = BaseContainer.Services.AddNew();
			steamClean.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.SteamCleaning;
			Assert("IsSteamCleanRequired", BaseContainerWrapper.IsSteamCleanRequired);
		}

		public void TestIsTailgateRequired()
		{
			Assert("!IsTailgateRequired", !BaseContainerWrapper.IsTailgateRequired);

			JobService tailgate = BaseContainer.Services.AddNew();
			tailgate.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Tailgate;
			Assert("IsTailgateRequired", BaseContainerWrapper.IsTailgateRequired);
		}

		public void TestIsWashingRequired()
		{
			Assert("!IsWashingRequired", !BaseContainerWrapper.IsWashingRequired);

			JobService washing = BaseContainer.Services.AddNew();
			washing.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Washing;
			Assert("IsWashingRequired", BaseContainerWrapper.IsWashingRequired);
		}

		public void TestPrintAsContainer()
		{
			AssertEquals("PrintAsContainer", ZBool.True, BaseContainerWrapper.PrintAsContainers);
		}
		#endregion

		#region Wrapper Fields

		public void TestConsol()
		{
			BaseContainer.JC_JK = ZGuid.Empty;
			AssertNull("Consol", BaseContainerWrapper.Consol);

			BaseContainer.JC_JK = Factory.New(typeof(ForwardingConsol)).PK;
			AssertNotNull("Consol", BaseContainerWrapper.Consol);
			AssertEquals("Consol is of type DocForwardingConsol", typeof(DocForwardingConsol), BaseContainerWrapper.Consol.GetType());
		}

		public void TestSailing()
		{
			Consol.Containers.Remove(BaseContainer);
			BaseContainer.JC_JX = ZGuid.Empty;
			AssertNull("Sailing", BaseContainerWrapper.Sailing);

			BaseContainer.JC_JX = Factory.New(typeof(JobSailing)).PK;
			AssertNotNull("Sailing", BaseContainerWrapper.Sailing);
			AssertEquals("Sailing is of type DocSailing", typeof(DocSailing), BaseContainerWrapper.Sailing.GetType());
		}

		public void TestArrivalContainerParkAddress()
		{
			BaseContainer.JC_OA_ArrivalContainerYardAddress = ZGuid.Empty;
			AssertNull("ArrivalContainerParkAddress", BaseContainerWrapper.ArrivalContainerParkAddress);

			BaseContainer.JC_OA_ArrivalContainerYardAddress = Factory.New(typeof(OrgAddress)).PK;
			AssertNotNull("ArrivalContainerParkAddress", BaseContainerWrapper.ArrivalContainerParkAddress);
			AssertEquals("ArrivalContainerParkAddress is of type DocDocAddress", typeof(DocDocAddress), BaseContainerWrapper.ArrivalContainerParkAddress.GetType());
		}

		public void TestArrivalCTOAddress()
		{
			Consol.JK_OA_ArrivalCTOAddress = ZGuid.Empty;
			AssertNull("ArrivalCTOAddress", BaseContainerWrapper.ArrivalCTOAddress);
			var testCTO = Factory.New<OrgHeader>();
			testCTO.OH_FullName = "Test CTO";
			testCTO.MainAddress.OA_Address1 = "The Docks";
			testCTO.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			Consol.JK_OA_ArrivalCTOAddress = testCTO.MainAddress.PK;
			AssertNotNull("ArrivalCTOAddress", BaseContainerWrapper.ArrivalCTOAddress);
			AssertEquals("ArrivalCTOAddress is of type DocDocAddress", typeof(DocDocAddress), BaseContainerWrapper.ArrivalCTOAddress.GetType());
		}

		public void TestArrivalUnpackAddress()
		{
			Consol.JK_OA_UnpackDepotAddress = ZGuid.Empty;
			AssertNull("ArrivalUnpackAddress", BaseContainerWrapper.ArrivalUnpackAddress);

			var testUnpackDepot = Factory.New<OrgHeader>();
			testUnpackDepot.OH_FullName = "Test Unpack Depot";
			testUnpackDepot.MainAddress.OA_Address1 = "Away from The Docks";
			testUnpackDepot.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			Consol.JK_OA_UnpackDepotAddress = testUnpackDepot.MainAddress.PK;
			AssertNotNull("ArrivalUnpackAddress", BaseContainerWrapper.ArrivalUnpackAddress);
			AssertEquals("ArrivalUnpackAddress is of type DocDocAddress", typeof(DocDocAddress), BaseContainerWrapper.ArrivalUnpackAddress.GetType());
		}

		public void TestDepartureContainerParkAddress()
		{
			BaseContainer.JC_OA_DepartureContainerYardAddress = ZGuid.Empty;
			AssertNull("DepartureContainerParkAddress", BaseContainerWrapper.DepartureContainerParkAddress);

			BaseContainer.JC_OA_DepartureContainerYardAddress = Factory.New(typeof(OrgAddress)).PK;
			AssertNotNull("DepartureContainerParkAddress", BaseContainerWrapper.DepartureContainerParkAddress);
			AssertEquals("DepartureContainerParkAddress is of type DocDocAddress", typeof(DocDocAddress), BaseContainerWrapper.DepartureContainerParkAddress.GetType());
		}

		public void TestDepartureCTOAddress()
		{
			BaseContainer.Consol.JK_OA_DepartureCTOAddress = ZGuid.Empty;
			AssertNull("DepartureCTOAddress", BaseContainerWrapper.DepartureCTOAddress);

			BaseContainer.Consol.JK_OA_DepartureCTOAddress = Factory.New(typeof(OrgAddress)).PK;
			AssertNotNull("DepartureCTOAddress", BaseContainerWrapper.DepartureCTOAddress);
			AssertEquals("DepartureCTOAddress is of type DocDocAddress", typeof(DocDocAddress), BaseContainerWrapper.DepartureCTOAddress.GetType());
		}

		public void TestDeparturePackAddress()
		{
			BaseContainer.Consol.JK_OA_PackDepotAddress = ZGuid.Empty;
			AssertNull("DeparturePackAddress", BaseContainerWrapper.DeparturePackAddress);

			var packDepotAddress = Factory.New<OrgAddress>();
			BaseContainer.Consol.JK_OA_PackDepotAddress = packDepotAddress.PK;
			packDepotAddress.OA_Address1 = "anything";
			AssertEquals("DeparturePackAddress is PackDepotAddress", packDepotAddress.OA_Address1, BaseContainerWrapper.DeparturePackAddress.Address1);
			AssertNotNull("DeparturePackAddress", BaseContainerWrapper.DeparturePackAddress);
			AssertEquals("DeparturePackAddress is of type DocDocAddress", typeof(DocDocAddress), BaseContainerWrapper.DeparturePackAddress.GetType());
		}

		public void TestArrivalTransport()
		{
			BaseContainer.DestinationCFSArrival.TransportCoPK = ZGuid.Empty;
			AssertNull("ArrivalTransport", BaseContainerWrapper.ArrivalTransport);

			BaseContainer.DestinationCFSArrival.TransportCoPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNotNull("ArrivalTransport", BaseContainerWrapper.ArrivalTransport);
			AssertEquals("ArrivalTransport is of type DocOrganisation", typeof(DocOrganisation), BaseContainerWrapper.ArrivalTransport.GetType());
		}

		public void TestDepartureTransport()
		{
			//the unit test fot the departure transport method
			OrgHeader departLegTransportCo = Factory.NewWithValidTestData<OrgHeader>();
			departLegTransportCo.OH_FullName = "Depart Leg TransportCo";
			BaseContainer.DestinationCFSDeparture.TransportCoPK = departLegTransportCo.PK;
			AssertEquals("Should return delivery cartage co for import documents because DepartLeg._OH_TransportCo is empty", departLegTransportCo.OH_FullName, BaseContainerWrapper.DepartureTransport.Name);

			AssertEquals("DepartureTransport is of type DocOrganisation", typeof(DocOrganisation), BaseContainerWrapper.DepartureTransport.GetType());
		}

		public void TestCFSClient()
		{
			BaseContainer.JC_OH_CFSClient = ZGuid.Empty;
			AssertNull("CFSClient", BaseContainerWrapper.CFSClient);

			BaseContainer.JC_OH_CFSClient = Factory.New(typeof(OrgHeader)).PK;
			AssertNotNull("CFSClient", BaseContainerWrapper.CFSClient);
			AssertEquals("CFSClient is of type DocOrganisation", typeof(DocOrganisation), BaseContainerWrapper.CFSClient.GetType());
		}

		public void TestShippingLine()
		{
			BaseContainer.JC_OH_ShippingLine = ZGuid.Empty;
			AssertNull("ShippingLine", BaseContainerWrapper.ShippingLine);

			OrgHeader shippingLine = Factory.New<OrgHeader>();
			Consol.SetDefaultShippingLineAddress(ZGuid.Empty);
			BaseContainer.JC_OH_ShippingLine = shippingLine.PK;
			AssertNull("ShippingLine", BaseContainerWrapper.ShippingLine);

			Consol.SetDefaultShippingLineAddress(shippingLine);
			AssertEquals("ShippingLine", shippingLine.PK, ((BusinessObject)((OrgHeaderSource)BaseContainerWrapper.ShippingLine.WrappedObject).WrappedObject).PK);
			AssertEquals("ShippingLine is of type DocOrganisation", typeof(DocOrganisation), BaseContainerWrapper.ShippingLine.GetType());
		}

		public void TestContainer()
		{
			BaseContainer.JC_RC = ZGuid.Empty;
			AssertNull("RefContainer", BaseContainerWrapper.Container);

			BaseContainer.JC_RC = Factory.New(typeof(RefContainer)).PK;
			AssertNotNull("RefContainer", BaseContainerWrapper.Container);
			AssertEquals("RefContainer is of type DocRefContainer", typeof(DocRefContainer), BaseContainerWrapper.Container.GetType());
		}

		public void TestCommodity()
		{
			BaseContainer.JC_RH_NKContainerCommodityCode = "";
			AssertNull("Commodity", BaseContainerWrapper.Commodity);

			var commCode = Factory.New<RefCommodityCode>();
			commCode.RH_Code = "TEST";
			BaseContainer.JC_RH_NKContainerCommodityCode = commCode.RH_Code;
			AssertNotNull("Commodity", BaseContainerWrapper.Commodity);
			AssertEquals("Commodity is of type DocCommodity", typeof(DocCommodity), BaseContainerWrapper.Commodity.GetType());
		}

		#endregion

		#region IDocContainer

		#region Headings

		public void TestJourneyOnePickUpHeading()
		{
			//No Dates
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOnePickUpHeading: Export, No date", "PICKUP EMPTY", BaseContainerWrapper.JourneyOnePickUpHeading);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOnePickUpHeading: Import, No date", "PICKUP FULL", BaseContainerWrapper.JourneyOnePickUpHeading);

			//+ Dates
			BaseContainer.JC_ReleaseNum = "12345";
			BaseContainer.JC_ArrivalSlotReference = "67890";
			BaseContainer.JC_ArrivalSlotDateTime = ZDateTime.Now;
			BaseContainer.JC_DepartureEstimatedPickup = ZDateTime.Now.AddDays(1);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOnePickUpHeading: LCL Export", "PICKUP EMPTY REF. 12345", BaseContainerWrapper.JourneyOnePickUpHeading);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOnePickUpHeading: LCL Import + DATE", "PICKUP FULL SLOT REF. 67890 / " + BaseContainer.JC_ArrivalSlotDateTime.ToLongTimeString(), BaseContainerWrapper.JourneyOnePickUpHeading);
		}

		public void TestJourneyOneDeliverToHeading()
		{
			//No Dates
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOneDeliveryToHeading: Export, No date", "DELIVER TO EMPTY", BaseContainerWrapper.JourneyOneDeliverToHeading);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOneDeliveryToHeading: Import, No date", "DELIVER TO FULL", BaseContainerWrapper.JourneyOneDeliverToHeading);

			//+ Dates
			BaseContainer.JC_EmptyRequired = ZDateTime.Now;
			BaseContainer.JC_ArrivalEstimatedDelivery = ZDateTime.Now.AddDays(1);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyOneDeliveryToHeading: LCL Export + Date", "DELIVER TO EMPTY DATE " + BaseContainer.JC_EmptyRequired.ToLongTimeString(), BaseContainerWrapper.JourneyOneDeliverToHeading);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyOneDeliveryToHeading: LCL Import + Date", "DELIVER TO FULL DATE " + BaseContainer.JC_ArrivalEstimatedDelivery.ToLongTimeString(), BaseContainerWrapper.JourneyOneDeliverToHeading);
		}

		public void TestJourneyTwoPickUpHeading()
		{
			//No Dates
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoPickUpHeading: Export, No date", "PICKUP FULL", BaseContainerWrapper.JourneyTwoPickUpHeading);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyTwoPickUpHeading: Import, No date", "PICKUP EMPTY", BaseContainerWrapper.JourneyTwoPickUpHeading);

			//+ Dates
			BaseContainer.JC_DepartureEstimatedPickup = ZDateTime.Now;
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoPickUpHeading: Export + Date", "PICKUP FULL DATE " + BaseContainer.JC_DepartureEstimatedPickup.ToLongTimeString(), BaseContainerWrapper.JourneyTwoPickUpHeading);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyTwoPickUpHeading: Import + Date", "PICKUP EMPTY", BaseContainerWrapper.JourneyTwoPickUpHeading);
		}

		public void TestJourneyTwoDeliverToHeading()
		{
			//No Dates
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoDeliverToHeading: Export, No date", "DELIVER TO FULL", BaseContainerWrapper.JourneyTwoDeliverToHeading);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyTwoDeliverToHeading: Import, No date", "DELIVER TO EMPTY", BaseContainerWrapper.JourneyTwoDeliverToHeading);

			//+ Dates
			BaseContainer.JC_EmptyReturnedBy = ZDateTime.Now;
			BaseContainer.JC_DepartureSlotReference = "67890";
			BaseContainer.JC_DepartureSlotDateTime = ZDateTime.Now.AddDays(1);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoDeliverToHeading: LCL Export + Date", "DELIVER TO FULL SLOT REF. 67890 / " + BaseContainer.JC_DepartureSlotDateTime.ToLongTimeString(), BaseContainerWrapper.JourneyTwoDeliverToHeading);
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("JourneyTwoDeliverToHeading: LCL Import + Date", "DELIVER TO EMPTY DATE " + BaseContainer.JC_EmptyReturnedBy.ToLongTimeString(), BaseContainerWrapper.JourneyTwoDeliverToHeading);
		}

		#endregion

		public void TestIsEmptyLeg()
		{
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Journey One, Export: Should be True", true, BaseContainerWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, Export: Should be False", false, BaseContainerWrapper.IsEmptyLeg(false));

			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Journey One, Import: Should be False", false, BaseContainerWrapper.IsEmptyLeg(true));
			AssertEquals("Journey Two, Import: Should be True", true, BaseContainerWrapper.IsEmptyLeg(false));
		}

		public void TestIsFullLeg()
		{
			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Journey One, Export: Should be True", false, BaseContainerWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, Export: Should be False", true, BaseContainerWrapper.IsFullLeg(false));

			BaseContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Journey One, Import: Should be False", true, BaseContainerWrapper.IsFullLeg(true));
			AssertEquals("Journey Two, Import: Should be True", false, BaseContainerWrapper.IsFullLeg(false));
		}

		public void TestConsignee()
		{
			AssertNull("No Consignee", BaseContainerWrapper.Consignee);
		}

		public void TestConsignor()
		{
			AssertNull("No Consignor", BaseContainerWrapper.Consignor);

			BaseContainer.JC_OH_CFSClient = ZGuid.Empty;
			BaseContainerWrapper.SetReportNameForTesting("CFS blah");
			AssertNull("CFSClient", BaseContainerWrapper.CFSClient);

			OrgHeader cFSClient = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			cFSClient.OH_FullName = "CFSClient";
			BaseContainer.JC_OH_CFSClient = cFSClient.PK;
			AssertNotNull("CFSClient", BaseContainerWrapper.CFSClient);
			AssertEquals("CFSClient is of type DocOrganisation", typeof(DocOrganisation), BaseContainerWrapper.CFSClient.GetType());
			AssertEquals("Consignor should be CFSClient", "CFSClient", BaseContainerWrapper.Consignor.Name);
		}

		public void TestTareWeightWithUQ()
		{
			ZString result = BaseContainerWrapper.TareWeight.ToStringTrimZeros() + " " + BaseContainerWrapper.GrossWeightUQ;
			AssertEquals("TareWeightWithUQ", result, BaseContainerWrapper.TareWeightWithUQ);

			BaseContainer.JC_TareWeight = 123.4819m;
			BaseContainer.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals("TareWeightWithUQ Rounded to 3 decimals", "123.482 LB", BaseContainerWrapper.TareWeightWithUQ);
		}

		public void TestGrossWeightWithUQ()
		{
			ZString result = BaseContainerWrapper.GrossWeight.ToStringTrimZeros() + " " + BaseContainerWrapper.GrossWeightUQ;
			AssertEquals("TareWeightWithUQ", result, BaseContainerWrapper.GrossWeightWithUQ);

			BaseContainer.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;
			BaseContainer.JC_GrossWeight = 123.4819m;
			AssertEquals("TareWeightWithUQ Rounded to 3 decimals", "123.482 LB", BaseContainerWrapper.GrossWeightWithUQ);
		}

		public void TestContainerPacklinesDescription()
		{
			BaseContainer.PackLines.RemoveAll();
			AssertEquals("ContainerPacklinesDescription is empty if no packlines", "", BaseContainerWrapper.ContainerPacklinesDescription);

			CommonShipment shipment = BaseContainer.Consol.Shipments.AddNew();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			PackLine packLine2 = shipment.OuterPackLines.AddNew();

			BaseContainer.AddPackLine(packLine1);

			packLine1.JL_PackageCount = 1;
			packLine1.JL_Description = "desc1";
			packLine1.JL_HarmonisedCode = "harmonised1";
			AssertEquals("ContainerPacklinesDescription for one packline", "1 desc1 harmonised1\n", BaseContainerWrapper.ContainerPacklinesDescription);

			BaseContainer.AddPackLine(packLine2);

			packLine2.JL_PackageCount = 2;
			packLine2.JL_Description = "desc2";
			packLine2.JL_HarmonisedCode = "harmonised2";
			AssertEquals("ContainerPacklinesDescription for two packlines", "1 desc1 harmonised1\n2 desc2 harmonised2\n", BaseContainerWrapper.ContainerPacklinesDescription);
		}

		#endregion

		#region Implementation

		CommonContainer BaseContainer;
		ForwardingConsol Consol;
		DocFreightBaseContainerHelperClass BaseContainerWrapper;

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			Consol = Factory.New<ForwardingConsol>();
			BaseContainer = Consol.Containers.AddNew();
			BaseContainerWrapper = new DocFreightBaseContainerHelperClass(BaseContainer, Factory);
			base.SetUp();
		}

		#endregion
	}
}
