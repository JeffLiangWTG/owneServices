using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ExportCustomsManifestHeader))]
	sealed class ExportCustomsManifestHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_BGMReference = "K00004713";
			AssertEquals("Customs Export Manifest K00004713", header.HumanReadableName);
		}

		public void TestCannotBeDeletedWhenMessages()
		{
			ExportCustomsManifestHeader header1 = Factory.New<ExportCustomsManifestHeader>();
			header1.ED_ManifestType = "EMM";
			header1.ED_DocumentStatus = "CLR";
			header1.ED_DepartureReportStatus = "CLR";
			bool exceptionThrown = false;
			try
			{
				header1.Delete();
			}
			catch (CannotDeleteException)
			{
				exceptionThrown = true;
			}
			Assert("Should not have thrown a CannotDeleteException", !exceptionThrown);

			ExportCustomsManifestHeader header2 = Factory.New<ExportCustomsManifestHeader>();
			header2.ED_ManifestType = "EMM";
			header2.ED_DocumentStatus = "CLR";
			header2.ED_DepartureReportStatus = "CLR";
			EDIMessage message = header2.Messages.AddNew();
			exceptionThrown = false;
			try
			{
				header2.Delete();
			}
			catch (CannotDeleteException)
			{
				exceptionThrown = true;
			}
			Assert("Should have thrown a CannotDeleteException", exceptionThrown);
		}

		public void TestDefaultValues()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			AssertEquals("Should default to sea (Air CTO will override to default to air but this needs to be sea)", Core.Constants.TransportModes.Sea, header.ED_TransportMode);
		}

		public void TestIsWaitingForResponseWithUnorderedMessages()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_ManifestType = "EMM";
			header.ED_DocumentStatus = "CLR";
			header.ED_DepartureReportStatus = "CLR";

			Assert(!header.ManifestStatus.Contains("Waiting for response"));
			Assert(!header.DepartureStatus.Contains("Waiting for response"));

			AssertEquals("Not waiting for manifest response", false, header.IsWaitingForManifestResponse);
			AssertEquals("Not waiting for departure response", false, header.IsWaitingForDepartureReportResponse);

			EDIMessage dEPSubmitMessage = header.Messages.AddNew();
			dEPSubmitMessage.EM_MessageType = "DEP";
			dEPSubmitMessage.EM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			dEPSubmitMessage.EM_MessageSubType = "ORG";
			dEPSubmitMessage.EM_SystemCreateTimeUtc = new ZDateTime(2005, 2, 2, 8, 10, 10);
			dEPSubmitMessage.EM_ReceiveTransmit = "TRX";

			EDIMessage eMMSubmitMessage = header.Messages.AddNew();
			eMMSubmitMessage.EM_MessageType = "EMM";
			eMMSubmitMessage.EM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			eMMSubmitMessage.EM_MessageSubType = "ORG";
			eMMSubmitMessage.EM_SystemCreateTimeUtc = new ZDateTime(2005, 2, 2, 8, 10, 10);
			eMMSubmitMessage.EM_ReceiveTransmit = "TRX";

			Assert(header.ManifestStatus.Contains("Waiting for response"));
			Assert(header.DepartureStatus.Contains("Waiting for response"));
			AssertEquals("Waiting for manifest response", true, header.IsWaitingForManifestResponse);
			AssertEquals("Waiting for departure response", true, header.IsWaitingForDepartureReportResponse);

			EDIMessage eMMClearMessage = header.Messages.AddNew();
			eMMClearMessage.EM_MessageType = "EMM";
			eMMClearMessage.EM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			eMMClearMessage.EM_MessageSubType = "CLR";
			eMMClearMessage.EM_SystemCreateTimeUtc = new ZDateTime(2005, 2, 2, 10, 10, 10);
			eMMClearMessage.EM_ReceiveTransmit = "RCV";

			Assert(!header.ManifestStatus.Contains("Waiting for response"));
			Assert(header.DepartureStatus.Contains("Waiting for response"));
			AssertEquals("Not waiting for manifest response", false, header.IsWaitingForManifestResponse);
			AssertEquals("Waiting for departure response", true, header.IsWaitingForDepartureReportResponse);

			EDIMessage dEPClearMessage = header.Messages.AddNew();
			dEPClearMessage.EM_MessageType = "DEP";
			dEPClearMessage.EM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			dEPClearMessage.EM_MessageSubType = "CLR";
			dEPClearMessage.EM_SystemCreateTimeUtc = new ZDateTime(2005, 2, 2, 10, 10, 10);
			dEPClearMessage.EM_ReceiveTransmit = "RCV";

			Assert(!header.ManifestStatus.Contains("Waiting for response"));
			Assert(!header.DepartureStatus.Contains("Waiting for response"));
			AssertEquals("Not waiting for manifest response", false, header.IsWaitingForManifestResponse);
			AssertEquals("Not waiting for manifest response", false, header.IsWaitingForDepartureReportResponse);
		}

		public void TestValidation()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			AssertEquals("ValidationType", typeof(ExportCustomsManifestHeaderValidation), header.Validation.GetType());
		}

		public void TestLookups()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			AssertEquals("LookupsType", typeof(ExportCustomsManifestHeaderLookups), header.Lookups.GetType());
		}

		public void TestIsAir()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			Assert(header.IsAir);
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			Assert(!header.IsAir);
		}

		public void TestIsSea()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			Assert(header.IsSea);
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			Assert(!header.IsSea);
		}

		public void TestIsMainManifest()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			Assert(header.IsMainManifest);
			header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			Assert(!header.IsMainManifest);
		}

		public void TestIsConsolidation()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			Assert(header.IsConsolidation);
			header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			Assert(!header.IsConsolidation);
		}

		public void TestIsSlot()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			Assert(header.IsSlot);
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			Assert(!header.IsSlot);
		}

		public void TestIsDeparture()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			AssertEquals(false, header.IsDeparture);
			header.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			AssertEquals(true, header.IsDeparture);
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertEquals(false, header.IsDeparture);
		}

		public void TestIsOld()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			AssertEquals(false, header.IsOld);

			header.ED_DocumentStatus = ZString.Empty;
			header.ED_DepartureReportStatus = ZString.Empty;
			AssertEquals(false, header.IsOld);

			header.ED_DocumentStatus = "foo";
			header.ED_DepartureReportStatus = ZString.Empty;
			AssertEquals(false, header.IsOld);

			header.ED_DocumentStatus = ZString.Empty;
			header.ED_DepartureReportStatus = "bar";
			AssertEquals(false, header.IsOld);

			header.ED_DocumentStatus = "foo";
			header.ED_DepartureReportStatus = "bar";
			AssertEquals(true, header.IsOld);
		}

		public void TestCarrierPartyID()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(Enterprise.MasterFiles.Business.OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, ZString.Empty);
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(Enterprise.MasterFiles.Business.OrgCusCode.CodeTypes.CustomsClientCode, ZString.Empty);
			AssertEquals("CarrierPartyID", ZString.Empty, header.CarrierPartyID);
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(Enterprise.MasterFiles.Business.OrgCusCode.CodeTypes.CustomsClientCode, "12345");
			AssertEquals("CarrierPartyID", "12345", header.CarrierPartyID);
			GlbCompany.CurrentCompany.OrgProxy.SetLocalCustomsCode(Enterprise.MasterFiles.Business.OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "54321");
			AssertEquals("CarrierPartyID", "54321", header.CarrierPartyID);
		}

		public void TestCanImport()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			Assert("CanImport", header.CanImport);
			header.Lines.AddNew();
			Assert("!CanImport", !header.CanImport);
		}

		public void TestCCANIsReadonlyIfWeHaveACAN()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			Assert(!header.ED_CCANInfo.ReadOnly);
			header.ED_CAN = "123456789";
			Assert(header.ED_CCANInfo.ReadOnly);
		}

		public void TestBlankContainersIfWeChangeToAir()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header.ED_NoOfContainer = 100;
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ContainerCount", (short)0, header.ED_NoOfContainer);
		}

		public void TestBlankEmptyContainersIfWeChangeToAir()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header.ED_NoOfEmptyContainers = 100;
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("EmptyContainerCount", (short)0, header.ED_NoOfEmptyContainers);
		}

		public void TestManifestTypeReadonlyIfWeHaveACAN()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			AssertEquals("Readonly", false, header.ED_ManifestTypeInfo.ReadOnly);
			header.ED_CAN = "123456789";
			AssertEquals("Readonly", true, header.ED_ManifestTypeInfo.ReadOnly);
			Factory.Save();
			BusinessObjectFactory secondFactory = new BusinessObjectFactory();
			ExportCustomsManifestHeader secondHeader = secondFactory.Load<ExportCustomsManifestHeader>(header.PK);
			AssertEquals("LoadedHeaderReadonly", true, secondHeader.ED_ManifestTypeInfo.ReadOnly);
		}

		public void TestLoadFromVesselEtc()
		{
			RefVessel vessel1 = Factory.New<RefVessel>();
			vessel1.RV_Code = "VES1";
			RefVessel vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Code = "VES2";
			ExportCustomsManifestHeader header1 = Factory.New<ExportCustomsManifestHeader>();
			header1.ED_VesselName = vessel1.RV_Code;
			header1.ED_VoyageNumber = "123";
			header1.ED_RL_NKPortOfDeparture = "AUSYD";
			header1.ED_RN_NKCountryOfDestination = "US";
			header1.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			ExportCustomsManifestHeader header2 = Factory.New<ExportCustomsManifestHeader>();
			header2.ED_VesselName = vessel1.RV_Code;
			header2.ED_VoyageNumber = "123";
			header2.ED_RL_NKPortOfDeparture = "AUSYD";
			header2.ED_RN_NKCountryOfDestination = "GB";
			header2.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			Factory.Save();
			ExportCustomsManifestHeader header3 = Factory.New<ExportCustomsManifestHeader>();
			header3.ED_VesselName = vessel1.RV_Code;
			header3.ED_VoyageNumber = "123";
			header3.ED_RL_NKPortOfDeparture = "AUSYD";
			header3.ED_RN_NKCountryOfDestination = "GB";
			header3.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertEquals("1 other found", 1, header3.LoadFromVesselVoyageDepartureDestination().Length);
			header3.ED_RN_NKCountryOfDestination = "NZ";
			AssertEquals("no other found", 0, header3.LoadFromVesselVoyageDepartureDestination().Length);
			Factory.Save();
			AssertEquals("still no other found", 0, header3.LoadFromVesselVoyageDepartureDestination().Length);
			header3.ED_RN_NKCountryOfDestination = "US";
			AssertEquals("1 other found", 1, header3.LoadFromVesselVoyageDepartureDestination().Length);
			header3.ED_RL_NKPortOfDeparture = "AUMEL";
			AssertEquals("no other found", 0, header3.LoadFromVesselVoyageDepartureDestination().Length);
			header3.ED_RL_NKPortOfDeparture = "AUSYD";
			header3.ED_VoyageNumber = "123x";
			AssertEquals("no other found", 0, header3.LoadFromVesselVoyageDepartureDestination().Length);
			header3.ED_VoyageNumber = "123";
			header3.ED_VesselName = vessel2.RV_Code;
			AssertEquals("no other found", 0, header3.LoadFromVesselVoyageDepartureDestination().Length);
			header3.ED_VesselName = vessel1.RV_Code;
			header3.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			AssertEquals("no other found", 0, header3.LoadFromVesselVoyageDepartureDestination().Length);
			header3.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertEquals("1 other found", 1, header3.LoadFromVesselVoyageDepartureDestination().Length);
		}

		public void TestManifestTypeAndTransportModeReadOnlyConditions()
		{
			AssertEquals(false, Header.ED_ManifestTypeInfo.ReadOnly);
			AssertEquals(false, Header.ED_TransportModeInfo.ReadOnly);
			AssertEquals(false, Header.IsWaitingForDepartureReportResponse);
			AssertEquals(false, Header.IsWaitingForManifestResponse);
			AssertEquals(false, Header.IsDepartureReportDeclaredAtCustoms);
			AssertEquals(false, Header.IsManifestDeclaredAtCustoms);
			AssertEquals("HaveMessagesBeenSentToCustoms should be False", false, Header.HaveMessagesBeenSentToCustoms);

			CMRDEPARTMessage departMessage = Factory.New<CMRDEPARTMessage>();
			Header.Messages.Add(departMessage);

			AssertEquals(true, Header.ED_ManifestTypeInfo.ReadOnly);
			AssertEquals(true, Header.ED_TransportModeInfo.ReadOnly);
			AssertEquals(true, Header.IsWaitingForDepartureReportResponse);
			AssertEquals(false, Header.IsWaitingForManifestResponse);
			AssertEquals(false, Header.IsDepartureReportDeclaredAtCustoms);
			AssertEquals(false, Header.IsManifestDeclaredAtCustoms);
			AssertEquals("HaveMessagesBeenSentToCustoms should be True", true, Header.HaveMessagesBeenSentToCustoms);

			Header.Messages.RemoveAll();
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			CMREMMMessage manifestMessage = Factory.New<CMREMMMessage>();
			Header.Messages.Add(manifestMessage);

			AssertEquals(true, Header.ED_ManifestTypeInfo.ReadOnly);
			AssertEquals(true, Header.ED_TransportModeInfo.ReadOnly);
			AssertEquals(false, Header.IsWaitingForDepartureReportResponse);
			AssertEquals(true, Header.IsWaitingForManifestResponse);
			AssertEquals(false, Header.IsDepartureReportDeclaredAtCustoms);
			AssertEquals(false, Header.IsManifestDeclaredAtCustoms);
			AssertEquals("HaveMessagesBeenSentToCustoms should be True", true, Header.HaveMessagesBeenSentToCustoms);

			Header.Messages.RemoveAll();
			Header.ED_CAN = "foosh";

			AssertEquals(true, Header.ED_ManifestTypeInfo.ReadOnly);
			AssertEquals(true, Header.ED_TransportModeInfo.ReadOnly);
			AssertEquals(false, Header.IsWaitingForDepartureReportResponse);
			AssertEquals(false, Header.IsWaitingForManifestResponse);
			AssertEquals(false, Header.IsDepartureReportDeclaredAtCustoms);
			AssertEquals(true, Header.IsManifestDeclaredAtCustoms);
			AssertEquals("HaveMessagesBeenSentToCustoms should be True", true, Header.HaveMessagesBeenSentToCustoms);

			Header.ED_CAN = ZString.Empty;
			Header.Messages.Add(departMessage);
			CMRDEPARTRMessage departResponseMessage = Factory.New<CMRDEPARTRMessage>();
			departResponseMessage.EM_MessageSubType = CMRMessage.ManifestResponseSubTypes.Clear;
			Header.Messages.Add(departResponseMessage);

			AssertEquals(true, Header.ED_ManifestTypeInfo.ReadOnly);
			AssertEquals(true, Header.ED_TransportModeInfo.ReadOnly);
			AssertEquals(false, Header.IsWaitingForDepartureReportResponse);
			AssertEquals(false, Header.IsWaitingForManifestResponse);
			AssertEquals(true, Header.IsDepartureReportDeclaredAtCustoms);
			AssertEquals(false, Header.IsManifestDeclaredAtCustoms);
			AssertEquals("HaveMessagesBeenSentToCustoms should be True", true, Header.HaveMessagesBeenSentToCustoms);
		}

		public void TestMessageManager()
		{
			ExportCustomsManifestHeader header = (ExportCustomsManifestHeader)GetNewBusinessObject();
			AssertNotNull("MessageManager", header.MessageManager.GetType());
		}

		public void TestLoad()
		{
			ExportCustomsManifestHeader header = (ExportCustomsManifestHeader)GetNewBusinessObject();
			header.ED_BGMReference = "123";
			AssertNull(ExportCustomsManifestHeader.Load(Factory, "321"));
			header.ED_BGMReference = "321";
			AssertNotNull(ExportCustomsManifestHeader.Load(Factory, "321"));
		}

		public void TestShortDescription()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			AssertEquals("ShortDescription is Empty", ZString.Empty, header.ShortDescription);
			header.ED_CAN = "123456789";
			AssertEquals("ShortDescriptiony", "CAN: 123456789", header.ShortDescription);
		}

		public void TestCalculateTotalPackagesFromLines()
		{
			AssertEquals(0, Header.ED_NoOfPacks);
			ExportCustomsManifestLines line1 = Header.Lines.AddNew();
			AssertEquals(0, Header.ED_NoOfPacks);
			line1.EL_NumberOfPackages = 4;
			AssertEquals(4, Header.ED_NoOfPacks);

			ExportCustomsManifestLines line2 = Header.Lines.AddNew();
			AssertEquals(4, Header.ED_NoOfPacks);
			line2.EL_NumberOfPackages = 5;
			AssertEquals(9, Header.ED_NoOfPacks);

			line1.EL_NumberOfPackages = 3;
			AssertEquals(8, Header.ED_NoOfPacks);

			Header.Lines.RemoveAndDelete(line2);
			AssertEquals(3, Header.ED_NoOfPacks);
			AssertEquals((short)0, Header.ED_NoOfContainer);
		}

		public void TestCalculateTotalContainersFromLines()
		{
			AssertEquals((short)0, Header.ED_NoOfContainer);
			ExportCustomsManifestLines line1 = Header.Lines.AddNew();
			AssertEquals((short)0, Header.ED_NoOfContainer);
			line1.EL_NumberOfContainers = 4;
			AssertEquals((short)4, Header.ED_NoOfContainer);

			ExportCustomsManifestLines line2 = Header.Lines.AddNew();
			AssertEquals((short)4, Header.ED_NoOfContainer);
			line2.EL_NumberOfContainers = 5;
			AssertEquals((short)9, Header.ED_NoOfContainer);

			line1.EL_NumberOfContainers = 3;
			AssertEquals((short)8, Header.ED_NoOfContainer);

			Header.Lines.RemoveAndDelete(line2);
			AssertEquals((short)3, Header.ED_NoOfContainer);
			AssertEquals(0, Header.ED_NoOfPacks);

			Header.ED_NoOfEmptyContainers = 7;
			AssertEquals((short)3, Header.ED_NoOfContainer);
			AssertEquals(0, Header.ED_NoOfPacks);
		}

		public void TestIsCTO()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			AssertEquals(false, header.IsCTO);

			header.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			AssertEquals(true, header.IsCTO);

			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertEquals(false, header.IsCTO);
		}

		public void TestIsAirCTO()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			AssertEquals(false, header.IsAirCTOHeader);
		}

		public void TestSingleBusinessObjectAroundARow()
		{
			AssertEquals(1, typeof(ExportCustomsManifestHeader).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
		}

		public void TestMessagesRegistersAsChildEditable()
		{
			ExportCustomsManifestHeader header = Factory.New<ExportCustomsManifestHeader>();
			var collection = header.Messages;
			AssertEquals("Touching messages should register as child editable", true, header.IsRegisteredEditableChildObject(collection));
		}

		public void TestCreateLinesFromJobDeclarations()
		{
			var consignorOrg = Factory.New<OrgHeader>();
			consignorOrg.OH_Code = "Sup1";
			consignorOrg.OH_FullName = "Supplier1";
			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_Code = "Imp1";
			consigneeOrg.OH_FullName = "Importer1";

			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec1.JE_DeclarationReference = "B001";
			dec1.DeclarationNumber = "";
			dec1.JE_HouseBill = "HB1";
			dec1.JE_GoodsDescription = "MAIN STUFF";
			dec1.JE_RL_NKFinalDestination = "AUSYD";
			dec1.JE_TotalVolume = 4.321m;
			dec1.JE_TotalVolumeUnit = "M3";
			dec1.JE_TotalWeight = 33.333m;
			dec1.JE_TotalWeightUnit = "KG";
			dec1.JE_TotalNoOfPacks = 25;
			dec1.JE_ContainerCount = 2;

			dec1.JE_OH_Supplier = consignorOrg.PK;
			dec1.JE_OH_Importer = consigneeOrg.PK;

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec2.JE_DeclarationReference = "B002";
			dec2.DeclarationNumber = "12345678901234567890";
			dec2.JE_HouseBill = "HB2";
			dec2.JE_GoodsDescription = "OTHER STUFF";
			dec2.JE_RL_NKFinalDestination = "AUSYD";
			dec2.JE_TotalVolume = 4.321m;
			dec2.JE_TotalVolumeUnit = "M3";
			dec2.JE_TotalWeight = 33.333m;
			dec2.JE_TotalWeightUnit = "KG";
			dec2.JE_TotalNoOfPacks = 22;
			dec2.JE_ContainerCount = 2;

			var cn3 = dec2.CusContainers.AddNew();
			cn3.CO_ContainerNumber = "CN3";
			var cn4 = dec2.CusContainers.AddNew();
			cn4.CO_ContainerNumber = "CN4";

			dec2.JE_OH_Supplier = consignorOrg.PK;
			dec2.JE_OH_Importer = consigneeOrg.PK;

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec3.JE_DeclarationReference = "B003";
			dec3.DeclarationNumber = "12345678901234567890";
			dec3.JE_HouseBill = "HB3";
			dec3.JE_GoodsDescription = "DUPLICATE DEC";

			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec4.JE_DeclarationReference = "B004";
			dec4.DeclarationNumber = "";
			dec4.JE_HouseBill = "HB4";
			dec4.JE_GoodsDescription = "SECOND EMPTY CAN DEC";

			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec5.JE_DeclarationReference = "B005";
			dec5.DeclarationNumber = "12345";
			dec5.JE_HouseBill = "HB5";
			dec5.JE_GoodsDescription = "EXISTING CAN DEC";

			var header = Factory.New<ExportCustomsManifestHeader>();
			var line1 = header.Lines.AddNew();
			line1.EL_CAN = "12345";
			var line2 = header.Lines.AddNew();
			line2.EL_CAN = "12345";
			Factory.Save();

			header.CreateLinesFromJobDeclarations(new JobDeclaration[] { dec1, dec2, dec3, dec4, dec5 });

			var line3 = header.Lines[2];
			CombineAssertions(() =>
			{
				AssertEquals("EL_TypeOfCAN", CANType.Exemptions.EXLV.Code, line3.EL_TypeOfCAN);
				AssertEquals("EL_CAN", ZString.Empty, line3.EL_CAN);
				AssertEquals("EL_AirWayBill", "HB1", line3.EL_AirWayBill);
				AssertEquals("EL_GoodsDescription", "MAIN STUFF", line3.EL_GoodsDescription);
				AssertEquals("EL_RN_NKCountryOfDestination", "AU", line3.EL_RN_NKCountryOfDestination);
				AssertEquals("EL_Volume", 4.321m, line3.EL_Volume);
				AssertEquals("EL_VolumeUQ", "M3", line3.EL_VolumeUQ);
				AssertEquals("EL_Weight", 33.333m, line3.EL_Weight);
				AssertEquals("EL_WeightUQ", "KG", line3.EL_WeightUQ);
				AssertEquals("EL_NumberOfPackages", 25, line3.EL_NumberOfPackages);
				AssertEquals("EL_NumberOfContainers", (short)2, line3.EL_NumberOfContainers);

				AssertEquals("Consignor", consignorOrg.PK, line3.ConsignorDocumentaryAddress.OrganisationPK);
				AssertEquals("Consignee", consigneeOrg.PK, line3.ConsigneeDocumentaryAddress.OrganisationPK);
				AssertEquals("EL_OH_Owner", consignorOrg.PK, line3.EL_OH_Owner);
				AssertEquals("EL_GoodsOwner", "Supplier1", line3.EL_GoodsOwner);
			});

			var line4 = header.Lines[3];
			CombineAssertions(() =>
			{
				AssertEquals("EL_TypeOfCAN", CANType.CustomsAuthorityNumber.Code, line4.EL_TypeOfCAN);
				AssertEquals("EL_CAN", "123456789012345", line4.EL_CAN);
				AssertEquals("EL_AirWayBill", "HB2", line4.EL_AirWayBill);
				AssertEquals("EL_GoodsDescription", "OTHER STUFF", line4.EL_GoodsDescription);
				AssertEquals("EL_RN_NKCountryOfDestination", "AU", line4.EL_RN_NKCountryOfDestination);
				AssertEquals("EL_Volume", 4.321m, line4.EL_Volume);
				AssertEquals("EL_VolumeUQ", "M3", line4.EL_VolumeUQ);
				AssertEquals("EL_Weight", 33.333m, line4.EL_Weight);
				AssertEquals("EL_WeightUQ", "KG", line4.EL_WeightUQ);
				AssertEquals("EL_NumberOfPackages", 22, line4.EL_NumberOfPackages);
				AssertEquals("EL_NumberOfContainers", (short)2, line4.EL_NumberOfContainers);

				AssertEquals("Consignor", consignorOrg.PK, line4.ConsignorDocumentaryAddress.OrganisationPK);
				AssertEquals("Consignee", consigneeOrg.PK, line4.ConsigneeDocumentaryAddress.OrganisationPK);
				AssertEquals("EL_OH_Owner", ZGuid.Empty, line4.EL_OH_Owner);
				AssertEquals("EL_GoodsOwner", ZString.Empty, line4.EL_GoodsOwner);
			});

			var line5 = header.Lines[4];
			CombineAssertions(() =>
			{
				AssertEquals("EL_TypeOfCAN", CANType.Exemptions.EXLV.Code, line5.EL_TypeOfCAN);
				AssertEquals("EL_CAN", ZString.Empty, line5.EL_CAN);
				AssertEquals("EL_AirWayBill", "HB4", line5.EL_AirWayBill);
				AssertEquals("EL_GoodsDescription", "SECOND EMPTY CAN DEC", line5.EL_GoodsDescription);
			});

			AssertEquals("Dec3 and Dec5 are not added because CANs already exist", 5, header.Lines.Count);

			var logEntry = header.Logs.MostRecentLog;
			AssertEquals("Duplicate CANs prevented some Declarations being imported: B003 (123456789012345), B005 (12345)", logEntry.SL_Reference);
			AssertEquals(Events.EditedARecord.Code, logEntry.SL_SE_NKEvent);
		}

		ExportCustomsManifestHeader header;
		ExportCustomsManifestHeader Header => header ?? (header = Factory.NewWithValidTestData<ExportCustomsManifestHeader>());
	}
}
