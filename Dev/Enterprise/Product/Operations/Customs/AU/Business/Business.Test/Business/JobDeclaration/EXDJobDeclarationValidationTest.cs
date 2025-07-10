using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDJobDeclarationValidationTest : ExportJobDeclarationValidationTest
	{
		public void TestCheckJE_ExportDate()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = "NCF";
			declaration.Validation.ValidateJE_ExportDate();
			AssertHasMessageErrors("Export Date", declaration.JE_ExportDateInfo);

			declaration.JE_ExportDate = ZDateTime.Today;
			AssertNoMessageErrors("Export Date", declaration.JE_ExportDateInfo);

			declaration.JE_ExportDate = ZDateTime.Today.AddMonths(6);
			AssertNoMessageErrors("Export Date", declaration.JE_ExportDateInfo);

			declaration.JE_ExportDate = ZDateTime.Today.AddMonths(6).AddDays(1);
			AssertHasMessageErrors("Export Date", declaration.JE_ExportDateInfo);

			declaration.JE_ExportDate = ZDateTime.Today;
			AssertNoMessageErrors("Export Date", declaration.JE_ExportDateInfo);

			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-1);
			AssertHasMessageErrors("Export Date", declaration.JE_ExportDateInfo);

			declaration.JE_MessageSubType = "CFM";
			declaration.Validation.ValidateJE_ExportDate();
			AssertNoMessageErrors("Export Date", declaration.JE_ExportDateInfo);

			declaration.JE_ExportDate = ZDateTime.Today.AddDays(14);
			AssertNoMessageErrors("Export Date", declaration.JE_ExportDateInfo);
		}

		[TestDate(2005, 10, 16)]
		public void TestExportDateForConfirmingEntry()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = "CFM";
			declaration.JE_ExportDate = new ZDateTime(2005, 10, 01);
			AssertHasMessageErrors("Export Date", declaration.JE_ExportDateInfo);
		}

		public void TestValidateJE_MergeBy()
		{
			declaration.JE_MergeBy = ZString.Empty;
			AssertEquals("Empty is not invalid for AU export", false, declaration.JE_MergeByInfo.HasErrors());
		}

		public void TestValidate_JE_ContainerCount()
		{
			// Conditional
			// doesn't validate (clears all notifications : see below) when Mode of Transport = AIR 
			// Must be = 0 when Cargo Type is Non-Containerised or Bulk
			// Must be > 0 when Cargo Type is Containerised or Combination
			// Must be = 0 When Export Goods Type is Postal, Own Power or Accompanied
			declaration.JE_ContainerCount = 1;
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("JE_ContainerCount Notifications", true, declaration.JE_ContainerCountInfo.HasNotifications());
			AssertHasWarning(declaration.JE_ContainerCountInfo, "Total Number of Containers entered does not tally with the containers entered on Containers tab: (0).");

			declaration.CusContainers.AddNew();
			declaration.JE_ContainerCount = 1;
			AssertNoWarning(declaration.JE_ContainerCountInfo, "Total Number of Containers entered does not tally with the containers entered on Containers tab: (0).");
			AssertEquals("JE_ContainerCount Notifications", false, declaration.JE_ContainerCountInfo.HasNotifications());
			//The control is not visible any more if it's Air
			//			Declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			//			AssertEquals("JE_ContainerCount Message Errors", true, Declaration.JE_ContainerCountInfo.HasMessageErrors());
			//			Declaration.JE_ContainerCount = 0;
			//			AssertEquals("JE_ContainerCount Notifications", false, Declaration.JE_ContainerCountInfo.HasNotifications());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Combination;
			declaration.JE_ContainerCount = 0;
			AssertEquals("JE_ContainerCount Message Errors", true, declaration.JE_ContainerCountInfo.HasMessageErrors());
			declaration.JE_ContainerCount = 1;
			AssertEquals("JE_ContainerCount Notifications", false, declaration.JE_ContainerCountInfo.HasNotifications());
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("JE_ContainerCount Message Errors", true, declaration.JE_ContainerCountInfo.HasMessageErrors());
			declaration.JE_ContainerCount = 0;
			AssertHasWarning(declaration.JE_ContainerCountInfo, "Total Number of Containers entered does not tally with the containers entered on Containers tab: (1).");
			declaration.CusContainers.RemoveAll();
			declaration.JE_ContainerCount = 0;
			AssertEquals("JE_ContainerCount Notifications", false, declaration.JE_ContainerCountInfo.HasNotifications());

			declaration.JE_ContainerMode = "";
			declaration.JE_ExportGoodsType = "PO";
			AssertEquals("JE_ContainerCount Notifications", false, declaration.JE_ContainerCountInfo.HasNotifications());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.CusContainers.AddNew();
			declaration.JE_ContainerCount = 1;
			AssertEquals("JE_ContainerCount Message Errors", true, declaration.JE_ContainerCountInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "OT";
			AssertEquals("JE_ContainerCount Notifications", false, declaration.JE_ContainerCountInfo.HasNotifications());

			declaration.JE_ExportGoodsType = "OP";
			AssertEquals("JE_ContainerCount Message Errors", true, declaration.JE_ContainerCountInfo.HasMessageErrors());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = "";
			declaration.CusContainers.RemoveAll();
			declaration.JE_ContainerCount = 0;
			AssertEquals("JE_ContainerCount Notifications", false, declaration.JE_ContainerCountInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "AB";
			AssertEquals("JE_ContainerCount Notifications", false, declaration.JE_ContainerCountInfo.HasNotifications());
		}

		public void TestValidateJE_TransportMode()
		{
			// Mandatory when Export Goods Type is not postal
			AssertEquals(false, declaration.HasNotifications());
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			declaration.JE_ExportGoodsType = "OP";
			declaration.JE_TransportMode = "";
			AssertEquals(true, declaration.JE_TransportModeInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "PO";
			AssertEquals(false, declaration.JE_TransportModeInfo.HasNotifications());
			declaration.JE_TransportMode = "JNK";
			AssertEquals(true, declaration.JE_TransportModeInfo.HasErrors());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals(false, declaration.JE_TransportModeInfo.HasNotifications());
			declaration.JE_TransportMode = "";
			AssertEquals(true, declaration.JE_TransportModeInfo.HasMessageErrors());

			declaration.JE_ExportGoodsType = "SP";
			AssertEquals(true, declaration.JE_TransportModeInfo.HasMessageErrors());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals(false, declaration.JE_TransportModeInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "OT";
			AssertEquals(false, declaration.JE_TransportModeInfo.HasNotifications());
			declaration.JE_TransportMode = "";
			AssertEquals(true, declaration.JE_TransportModeInfo.HasMessageErrors());

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ExportGoodsType = "OP";
			AssertEquals(false, declaration.JE_TransportModeInfo.HasNotifications());
			declaration.JE_TransportMode = "";
			AssertEquals(true, declaration.JE_TransportModeInfo.HasMessageErrors());

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ExportGoodsType = "AB";
			AssertEquals(false, declaration.JE_TransportModeInfo.HasNotifications());
		}

		public void TestChangeToAirClearsContainerCountNotification()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Combination;
			declaration.JE_ContainerCount = 0;
			AssertEquals("JE_ContainerCount Message Errors", true, declaration.JE_ContainerCountInfo.HasMessageErrors());

			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			AssertEquals("JE_ContainerCount No Errors", false, declaration.JE_ContainerCountInfo.HasMessageErrors());
		}

		public void TestValidateJE_TotalNoOfPacks()
		{
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_TotalNoOfPacks = 10;
			AssertEquals("HasMessageErrors", false, declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
			declaration.JE_TotalNoOfPacks = 10000000;
			AssertEquals("HasMessageErrors", true, declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
		}

		public void TestValidateJE_ExportGoodsType()
		{
			AssertEquals(false, declaration.HasNotifications());
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;

			declaration.JE_ExportGoodsType = "XX";
			AssertEquals("Invalid Export Goods Type", true, declaration.JE_ExportGoodsTypeInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "OT";
			AssertEquals("Valid Export Goods Type", false, declaration.JE_ExportGoodsTypeInfo.HasNotifications());
		}

		public void TestValidateTotalNumberOfPackages()
		{
			// Conditional
			// Must be > 0 when Mode of Transport = AIR 
			// Must be > 0 when Cargo Type is Non-Containerised or Combination
			// Must be = 0 when Cargo Type is Containerised or Bulk
			// Must be > 0 When Total Number Of Containers = 0
			AssertEquals("JE_TotalNoOfPacks Notifications", false, declaration.JE_TotalNoOfPacksInfo.HasNotifications());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_ContainerMode = Core.Constants.TransportModes.Air;
			declaration.JE_TotalNoOfPacks = 2;
			AssertEquals("JE_TotalNoOfPacks Notifications", false, declaration.JE_TotalNoOfPacksInfo.HasNotifications());
			declaration.JE_ContainerCount = 1;
			declaration.JE_TotalNoOfPacks = 0;
			AssertEquals("JE_TotalNoOfPacks Message Errors", true, declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			declaration.JE_TotalNoOfPacksPackType = "";
			AssertEquals("JE_TotalNoOfPacks Notifications", false, declaration.JE_TotalNoOfPacksInfo.HasNotifications());
			declaration.JE_TotalNoOfPacks = 4;
			AssertEquals("JE_TotalNoOfPacks Message Errors", false, declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
			declaration.JE_TotalNoOfPacks = 0;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
			AssertEquals("JE_TotalNoOfPacks Notifications", false, declaration.JE_TotalNoOfPacksInfo.HasNotifications());
			declaration.JE_TotalNoOfPacks = 4;
			AssertEquals("JE_TotalNoOfPacks Message Errors", false, declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
			declaration.JE_TotalNoOfPacks = 0;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_TotalNoOfPacks = 0;
			AssertEquals("JE_TotalNoOfPacks Message Errors", false, declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
			declaration.JE_TotalNoOfPacks = 1;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_ContainerCount = 0;
			AssertEquals("JE_TotalNoOfPacks Notifications", false, declaration.JE_TotalNoOfPacksInfo.HasNotifications());
			declaration.JE_ContainerMode = Core.Constants.TransportModes.Air;
			declaration.JE_TotalNoOfPacks = 0;
			AssertEquals("JE_TotalNoOfPacks Message Errors", true, declaration.JE_TotalNoOfPacksInfo.HasMessageErrors());
			declaration.JE_TotalNoOfPacks = 1;
			AssertEquals("JE_TotalNoOfPacks Notifications", false, declaration.JE_TotalNoOfPacksInfo.HasNotifications());
		}

		public void TestValidateJE_OH_Importer()
		{
			// Conditional on 'Export Goods Type' - Mandatory when Export Goods Type is not Stores or Spares
			OrgHeader importer = GetValidImporter();
			importer.OH_RL_NKClosestPort = "USLAX";
			ZGuid validImporterPK = importer.PK;

			AssertEquals(false, declaration.JE_OH_ImporterInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "ST";
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(false, declaration.JE_OH_ImporterInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "SP";
			AssertEquals(false, declaration.JE_OH_ImporterInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "OT";
			AssertEquals(true, declaration.JE_OH_ImporterInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "OP";
			AssertEquals(true, declaration.JE_OH_ImporterInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "AB";
			AssertEquals(true, declaration.JE_OH_ImporterInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "PO";
			AssertEquals(true, declaration.JE_OH_ImporterInfo.HasMessageErrors());
			declaration.JE_OH_Importer = validImporterPK;
			AssertEquals(false, declaration.JE_OH_ImporterInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "OP";
			AssertEquals(false, declaration.JE_OH_ImporterInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "AB";
			AssertEquals(false, declaration.JE_OH_ImporterInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "PO";
			AssertEquals(false, declaration.JE_OH_ImporterInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "OP";
			declaration.JE_OH_Importer = ZGuid.Empty;
			AssertEquals(true, declaration.JE_OH_ImporterInfo.HasMessageErrors());
			//			Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_ExportGoodsType = "AB";
			declaration.JE_OH_Importer = validImporterPK;
			AssertEquals(false, declaration.JE_OH_ImporterInfo.HasMessageErrors());
		}

		public void TestValidateJE_OH_ImporterRequiredUNLOCO()
		{
			OrgHeader importer = GetValidImporter();
			importer.OH_RL_NKClosestPort = ZString.Empty;
			importer.MainAddress.OA_City = ZString.Empty;

			ZGuid validImporterPK = importer.PK;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(true, declaration.JE_OH_ImporterInfo.HasNotifications());
			importer.MainAddress.OA_City = "NEW York";

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(false, declaration.JE_OH_ImporterInfo.HasNotifications());

			importer.MainAddress.OA_City = ZString.Empty;
			importer.OH_RL_NKClosestPort = "USLAX";

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals(false, declaration.JE_OH_ImporterInfo.HasNotifications());
		}

		public void TestValidateJE_OH_Supplier()
		{
			// Mandatory
			OrgHeader goodSupplier = Factory.New<OrgHeader>();
			AssertEquals(false, declaration.JE_OH_SupplierInfo.HasNotifications());
			declaration.JE_OH_Supplier = ZGuid.Empty;
			AssertEquals(true, declaration.JE_OH_SupplierInfo.HasMessageErrors());
			goodSupplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, "W123N");
			declaration.JE_OH_Supplier = goodSupplier.PK;
			AssertEquals(false, declaration.JE_OH_SupplierInfo.HasMessageErrors());
		}

		public void TestValidateJE_VoyageFlightNo_SEA()
		{
			// Conditional when Mode of Transport = SEA && ExportGoodsType is Stores Or Spares
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.Validation.ValidateJE_VoyageFlightNo();
			AssertEquals("JE_VoyageFlightNo Notifications", false, declaration.JE_VoyageFlightNoInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "ST";
			AssertEquals("JE_VoyageFlightNo Message Error", true, declaration.JE_VoyageFlightNoInfo.HasNotifications());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("JE_VoyageFlightNo Message Error", true, declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "PO";
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("JE_VoyageFlightNo Notifications", false, declaration.JE_VoyageFlightNoInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "SP";
			declaration.JE_VoyageFlightNo = "EF4321";
			AssertEquals("JE_VoyageFlightNo Notifications", false, declaration.JE_VoyageFlightNoInfo.HasNotifications());
			declaration.JE_VoyageFlightNo = "";
			AssertEquals("JE_VoyageFlightNo Message Error", true, declaration.JE_VoyageFlightNoInfo.HasMessageErrors());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_VoyageFlightNo = "QF1234";
			AssertEquals("JE_VoyageFlightNo Notifications", false, declaration.JE_VoyageFlightNoInfo.HasNotifications());
		}

		public void TestValidateJE_VesselName()
		{
			// Conditional when Mode of Transport = SEA && ExportGoodsType is Stores Or Spares
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			declaration.JE_VesselName = "";
			AssertEquals("Notifications", false, declaration.JE_VesselNameInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "ST";
			AssertEquals("Notifications", false, declaration.JE_VesselNameInfo.HasNotifications());
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertEquals("Message Error", true, declaration.JE_VesselNameInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "PO";
			AssertEquals("Notifications", false, declaration.JE_VesselNameInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "SP";
			declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			declaration.JE_VesselName = GetValidVessel().RV_Code;
			AssertEquals("Notifications", false, declaration.JE_VesselNameInfo.HasNotifications());
			declaration.JE_VesselName = "";
			AssertEquals("Message Error", true, declaration.JE_VesselNameInfo.HasMessageErrors());
		}

		public void TestJE_DateOfArrival()
		{
			declaration.JE_ExportDate = ZDateTime.Today;
			declaration.JE_RL_NKPortOfArrival = "AUSYD";
			declaration.JE_RL_NKPortOfFirstArrival = "AUMEL";
			declaration.JE_DateOfFirstArrival = declaration.JE_ExportDate.AddDays(3);
			declaration.JE_DateOfArrival = declaration.JE_ExportDate.AddDays(6);
			AssertEquals("JE_DateOfArrival should have no notifications", false, declaration.JE_DateOfArrivalInfo.HasNotifications());

			declaration.JE_DateOfArrival = declaration.JE_ExportDate.AddDays(2);
			AssertEquals("JE_DateOfArrival should have message errors", true, declaration.JE_DateOfArrivalInfo.HasMessageErrors());
			declaration.JE_DateOfArrival = declaration.JE_ExportDate.AddDays(5);
			AssertEquals("JE_DateOfArrival should have no notifications", false, declaration.JE_DateOfArrivalInfo.HasNotifications());
			declaration.JE_DateOfArrival = ZDateTime.Invalid;
			AssertEquals("JE_DateOfArrival should have errors", true, declaration.JE_DateOfArrivalInfo.HasErrors());
		}

		public void TestSupplier()
		{
			declaration.JE_OH_Supplier = ZGuid.Empty;
			Assert("MessageErrors", declaration.JE_OH_SupplierInfo.HasMessageErrors());
			declaration.JE_OH_Supplier = Factory.New(typeof(OrgHeader)).PK;
			Assert("MessageErrors", declaration.JE_OH_SupplierInfo.HasMessageErrors());
			declaration.Supplier.SetLocalCustomsCode(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			declaration.JE_OH_Supplier = declaration.JE_OH_Supplier;
			Assert("!MessageErrors", !declaration.JE_OH_SupplierInfo.HasMessageErrors());
		}

		public void TestValidateCargoAndGoodsTypeForPostal()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			Assert(declaration.JE_ContainerModeInfo.HasMessageErrors());
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			Assert(!declaration.JE_ContainerModeInfo.HasMessageErrors());

			declaration.JE_ExportGoodsType = JobDeclaration.ExportGoodsType.Stores;
			Assert(declaration.JE_ExportGoodsTypeInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = JobDeclaration.ExportGoodsType.Postal;
			Assert(!declaration.JE_ExportGoodsTypeInfo.HasMessageErrors());
		}

		public void TestValidateJE_RL_FinalDestination()
		{
			// Conditional on 'Export Goods Type' - Mandatory when Export Goods Type is not Stores or Spares
			declaration.JE_ExportDate = ZDateTime.Today;

			//AssertEquals(false, Declaration.JE_RL_NKFinalDestinationInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "ST";
			declaration.JE_RL_NKFinalDestination = "";
			Assert("Ships stores should not have errors when final destination is empty", !declaration.JE_RL_NKFinalDestinationInfo.HasNotifications());
			declaration.JE_RL_NKFinalDestination = "USDEN";
			AssertEquals(false, declaration.JE_RL_NKFinalDestinationInfo.HasNotifications());
			declaration.JE_RL_NKFinalDestination = "";
			AssertEquals(false, declaration.JE_RL_NKFinalDestinationInfo.HasNotifications());
			declaration.JE_ExportGoodsType = "SP";
			AssertEquals(false, declaration.JE_RL_NKFinalDestinationInfo.HasNotifications());

			declaration.JE_ExportGoodsType = "OT";
			AssertEquals(true, declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "OP";
			AssertEquals(true, declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "AB";
			AssertEquals(true, declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "PO";
			AssertEquals(true, declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
			declaration.JE_RL_NKFinalDestination = "USDEN";
			AssertEquals(false, declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "OP";
			AssertEquals(false, declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "AB";
			AssertEquals(false, declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "PO";
			AssertEquals(false, declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "OP";
			declaration.JE_RL_NKFinalDestination = "";
			AssertEquals(true, declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
			declaration.JE_ExportGoodsType = "AB";
			declaration.JE_RL_NKFinalDestination = "USDEN";
			AssertEquals(false, declaration.JE_RL_NKFinalDestinationInfo.HasMessageErrors());
		}

		#region Implementation

		protected override JobDeclarationValidation GetNewValidationProvider(JobDeclaration jobDeclaration)
		{
			return new EXDJobDeclarationValidation(jobDeclaration);
		}

		#endregion

	}
}
