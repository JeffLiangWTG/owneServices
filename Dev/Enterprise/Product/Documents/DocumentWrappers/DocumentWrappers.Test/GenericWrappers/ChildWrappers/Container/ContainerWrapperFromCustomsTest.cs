using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperFromCustoms))]
	sealed class ContainerWrapperFromCustomsTest : ContainerWrapperTest
	{
		public void TestArrivalReleaseNumberAndArrivalCartageRef()
		{
			var container = Factory.New<BaseCusContainer>();
			var jobContainer = container.JobContainer as ForwardingContainer;
			var wrapper = new ContainerWrapperFromCustoms(container, Factory);
			jobContainer.JC_ArrivalPickupByRail = true;
			jobContainer.AMSNumber = "MB2";
			jobContainer.ITReferenceNumber = "V2";
			AssertEquals("Release number", "V2", wrapper.ITReferenceNumber);
			AssertEquals("Cartage Ref", "MB2", wrapper.AMSNumber);
		}

		public void TestFreightJob()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00001845";
			BaseCusContainer container = declaration.CusContainers.AddNew();
			ContainerWrapperFromCustoms wrapper = new ContainerWrapperFromCustoms(container, Factory);
			AssertEquals("Wrapped business object should be BaseJobDeclaration", "B00001845", wrapper.FreightJob.JobNumber);
		}

		public void TestIsReefer()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonContainer container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			ContainerWrapperFromCartage wrapper = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);
			AssertEquals("Container: IsReefer", true, wrapper.IsReefer);
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AssertEquals("Container: IsNotReefer", false, wrapper.IsReefer);
		}

		public void TestIsHazardous()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			var packline = shipment.OuterPackLines.AddNew();
			packline.SetContainer(consol, container);

			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_ParentID = shipment.PK;
			var move = Factory.New<CommonBookedCtgMove>();
			move.EW_JC_Container = container.PK;
			cartage.ContainerBookedMoves.Add(move);

			ContainerWrapperFromCartage wrapper = new ContainerWrapperFromCartage(new FreightWrapperFromCartage(cartage, Factory), container, Factory);

			container.JC_RH_NKContainerCommodityCode = "HAZ";
			AssertEquals("Container: IsHazardous", true, wrapper.IsHazardous);
			container.JC_RH_NKContainerCommodityCode = "GEN";
			AssertEquals("Container: IsNotHazardous", false, wrapper.IsHazardous);
			packline.JL_RH_NKCommodityCode = "HAZ";
			AssertEquals("PackLine: IsHazardous", true, wrapper.IsHazardous);
			packline.JL_RH_NKCommodityCode = "GEN";
			AssertEquals("PackLine: IsNotHazardous", false, wrapper.IsHazardous);
			container.JC_RH_NKContainerCommodityCode = "";
			packline.JL_RH_NKCommodityCode = "";
			AssertEquals("Container & PackLine: IsEmpty", false, wrapper.IsHazardous);
		}

		public void TestCorrectPackageCountIsObtained()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TotalNoOfPacks = 80;
			declaration.JE_TotalNoOfPacksPackType = Constants.PkgUnit.Package;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "BEXU1234561";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CAXU1234563";

			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "AAXU1234563";

			const string houseBill = "HB:HB1";
			AddCusPackage(declaration, container1.CO_ContainerNumber, 25, Constants.PkgUnit.Pallet, houseBill);
			AddCusPackage(declaration, container1.CO_ContainerNumber, 13, Constants.PkgUnit.Drum, houseBill);
			AddCusPackage(declaration, container2.CO_ContainerNumber, 25, Constants.PkgUnit.Drum, houseBill);
			AddCusPackage(declaration, container2.CO_ContainerNumber, 30, Constants.PkgUnit.Drum, "HB:HB2");
			AddCusPackage(declaration, container3.CO_ContainerNumber, 20, string.Empty, houseBill);

			var wrapper = new ContainerWrapperFromCustoms(container1, Factory);
			AssertEquals("The total number of packages in container1 - BEXU1234561", "38 " + Constants.PkgUnit.Piece, wrapper.PackCount.ToString());

			wrapper = new ContainerWrapperFromCustoms(container2, Factory);
			AssertEquals("The total number of packages in container2 - CAXU1234563", "55 " + Constants.PkgUnit.Drum, wrapper.PackCount.ToString());

			wrapper = new ContainerWrapperFromCustoms(container3, Factory);
			AssertEquals("The total number of packages in container3 - AAXU1234563", "20 " + Constants.PkgUnit.Piece, wrapper.PackCount.ToString());
		}

		public void TestContainerNumberOrTypeCount()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			RefContainer containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000011";
			container.CO_RC = containerType.PK;

			CommonContainer jobContainer = container.JobContainer;
			jobContainer.JC_ContainerCount = 2;

			ContainerWrapper wrapperFull = new ContainerWrapperFromCustoms(container, Factory);
			AssertEquals("wrapperFull.ContainerNo", "OOCL0000011", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerNumberOrTypeCount", "OOCL0000011", wrapperFull.ContainerNumberOrTypeCount);

			container.CO_ContainerNumber = "";
			wrapperFull = new ContainerWrapperFromCustoms(container, Factory);
			AssertEquals("wrapperFull.ContainerNo", "", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerNumberOrTypeCount", "ZZGP (2)", wrapperFull.ContainerNumberOrTypeCount);

			containerType.RC_Code = "";
			wrapperFull = new ContainerWrapperFromCustoms(container, Factory);
			AssertEquals("wrapperFull.Type.Code", "", wrapperFull.Type.Code);
			AssertEquals("wrapperFull.ContainerNumberOrTypeCount", "", wrapperFull.ContainerNumberOrTypeCount);
		}

		public override void TestWrapperMappingFull()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_ISOType = "2340";
			containerType.RC_TareWeight = 1450m;

			var departureAddress = Factory.New<JobDocAddress>();
			departureAddress.E2_CompanyName = "TEST DEPARTURE CONTAINER YARD ADDRESS";
			var arrivalAddress = Factory.New<OrgHeader>();
			arrivalAddress.OH_FullName = "TEST ARRIVAL CONTAINER YARD ADDRESS";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000011";
			container.CO_RC = containerType.PK;
			container.CO_FCL_LCL_AIR = "FCX";
			container.CO_Seal = "SEAL_ROCKS";
			container.CO_SecondSeal = "SEAL_ROCKS_SUCK";
			container.CO_ContainerSize = "40";
			container.EmptyReturnedBy = new ZDateTime(2008, 2, 14);
			container.EmptyRequired = new ZDateTime(2008, 2, 11);
			container.ContainerYardEmptyReturnGateIn = new ZDateTime(2007, 3, 24);
			container.ArrivalEstimatedDelivery = new ZDateTime(2011, 4, 1);
			container.ArrivalSlotReference = "MKJHU12874L";
			container.DepartureSlotReference = "PDFGU9832M";
			container.ArrivalSlotDateTime = new ZDateTime(2007, 8, 24);
			container.DepartureSlotDateTime = new ZDateTime(2007, 8, 25);
			container.DepartureEstimatedPickup = new ZDateTime(2007, 9, 23);
			container.TotalLength = 89.34m;
			container.TotalWidth = 12.90m;
			container.TotalHeight = 8.35m;
			container.SetPointTemp = 23.9m;
			container.SetPointTempUnit = "C";
			container.IsControlledAtmosphere = true;
			container.HumidityPercent = new ZByte(23);
			container.AirVentFlow = 12;
			container.AirVentFlowRateUnit = "2L";
			container.RefrigGeneratorID = "REFRIG1";

			var package1 = declaration.Packages.AddNew();
			package1.CW_ContainerNoOrEquipmentNo = "OOCL0000011";
			package1.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3398", "a", "IMO").First().PK;

			CommonContainer jobContainer = container.JobContainer;
			jobContainer.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CY;
			jobContainer.JC_EmptyReadyForReturn = new ZDateTime(2011, 4, 5);
			jobContainer.JC_EmptyReturnReference = "REEE123";
			jobContainer.JC_ExportDepotCustomsReference = "REF.213ADHJ";
			jobContainer.JC_IsDamaged = true;
			jobContainer.JC_OA_DepartureContainerYardAddress = departureAddress.PK;
			jobContainer.JC_OA_ArrivalContainerYardAddress = arrivalAddress.MainAddress.PK;
			jobContainer.JC_ContainerJobID = "JobID";
			jobContainer.JC_ReleaseNum = "ReleaseNo181";
			jobContainer.JC_ContainerImportDORelease = "ImpRelease123";
			jobContainer.JC_FCLWharfGateOut = new ZDateTime(2008, 2, 20);
			jobContainer.JC_StowagePosition = "ABC1";
			jobContainer.JC_ArrivalCartageRef = "ARV111";
			jobContainer.JC_DepartureCartageRef = "DEP222";

			var packLine = Factory.New<PackLine>();
			packLine.JL_ActualVolume = 23.32m;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			packLine.JL_PackageCount = 24;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			packLine.UNDGs.AddNew().DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			jobContainer.AddPackLine(packLine);
			AssertEquals("Precondition: jobContainer.JC_Calc_TotalVolume", 23.32m, jobContainer.JC_Calc_TotalVolume);
			AssertEquals("Precondition: jobContainer.JC_Calc_TotalVolumeUnit", Core.Constants.Volume.CubicMetres, jobContainer.JC_Calc_TotalVolumeUnit);

			container.CO_Weight = 23630m;
			container.DunnageWeight = 340m;
			AssertEquals("Precondition: container.GrossWeight", 25420m, container.GrossWeight);
			AssertEquals("Precondition: container.GrossWeightUQ", Core.Constants.Weight.Kilograms, container.GrossWeightUQ);

			var service = container.JobContainer.Services.AddNew();
			service.ES_ServiceCode = "QIN";

			var wrapperFull = new ContainerWrapperFromCustoms(container, Factory);
			AssertEquals("wrapperFull.Mode.Code", "FCX", wrapperFull.Mode.Code);
			AssertEquals("wrapperFull.Type.Code", "ZZGP", wrapperFull.Type.Code);
			AssertEquals("wrapperFull.Type.Description", "General Purpose Box", wrapperFull.Type.Description);
			AssertEquals("wrapperFull.VolumeGoods.Value", 0m, wrapperFull.VolumeGoods.Value);
			AssertEquals("wrapperFull.VolumeGoods.Unit.Code", string.Empty, wrapperFull.VolumeGoods.Unit.Code);
			AssertEquals("wrapperFull.WeightTare.Value", 1450m, wrapperFull.WeightTare.Value);
			AssertEquals("wrapperFull.WeightTare.Unit.Code", "KG", wrapperFull.WeightTare.Unit.Code);
			AssertEquals("wrapperFull.WeightGoods.Value", 23630m, wrapperFull.WeightGoods.Value);
			AssertEquals("wrapperFull.WeightGoods.Unit.Code", "KG", wrapperFull.WeightGoods.Unit.Code);
			AssertEquals("wrapperFull.WeightDunnage.Value", 340m, wrapperFull.WeightDunnage.Value);
			AssertEquals("wrapperFull.WeightDunnage.Unit.Code", "KG", wrapperFull.WeightDunnage.Unit.Code);
			AssertEquals("wrapperFull.WeightGross.Value", 25420m, wrapperFull.WeightGross.Value);
			AssertEquals("wrapperFull.WeightGross.Unit.Code", "KG", wrapperFull.WeightGross.Unit.Code);
			AssertEquals("wrapperFull.SetPointTemperature.Value", 23.9m, wrapperFull.SetPointTemperature.Value);
			AssertEquals("wrapperFull.SetPointTemperature.Unit.Code", "C", wrapperFull.SetPointTemperature.Unit.Code);
			AssertEquals("wrapperFull.ContainerNo", "OOCL0000011", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerNumberOrTypeCount", "OOCL0000011", wrapperFull.ContainerNumberOrTypeCount);
			AssertEquals("wrapperFull.SealNo", "SEAL_ROCKS", wrapperFull.SealNo);
			AssertEquals("wrapperFull.SealNo2", "SEAL_ROCKS_SUCK", wrapperFull.SealNo2);
			AssertEquals("wrapperFull.SealNo3", ZString.Empty, wrapperFull.SealNo3);
			AssertEquals("wrapperFull.ReleaseNumber", "ReleaseNo181", wrapperFull.ReleaseNumber);
			AssertEquals("wrapperFull.ArrivalReleaseNumber", "ImpRelease123", wrapperFull.ArrivalReleaseNumber);
			AssertEquals("wrapperFull.Services.Count", 1, wrapperFull.Services.Count);
			AssertEquals("wrapperFull.Services[0].Type.Code", "QIN", wrapperFull.Services[0].Type.Code);
			AssertEquals("wrapperFull.EmptyReadyForReturn", new ZDateTime(2011, 4, 5), wrapperFull.EmptyReadyForReturn);
			AssertEquals("wrapperFull.EmptyReturnReference", "REEE123", wrapperFull.EmptyReturnReference);
			AssertEquals("wrapperFull.EmptyReturnedBy", new ZDateTime(2008, 2, 14), wrapperFull.EmptyReturnedBy);
			AssertEquals("wrapperFull.WharfGateOut", new ZDateTime(2008, 2, 20), wrapperFull.WharfGateOut);
			AssertEquals("wrapperFull.StowagePosition", "ABC1", wrapperFull.StowagePosition);
			AssertEquals("wrapperFull.ContainerYardEmptyReturnGateIn", new ZDateTime(2007, 3, 24), wrapperFull.ContainerYardEmptyReturnGateIn);
			AssertEquals("wrapperFull.PackCount.ValueAndUnitCodeBlankIfZero", "", wrapperFull.PackCount.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperFull.ArrivalEstimatedDelivery", new ZDateTime(2011, 4, 1), wrapperFull.ArrivalEstimatedDelivery);
			AssertEquals("wrapperFull.ArrivalSlotReference", "MKJHU12874L", wrapperFull.ArrivalSlotReference);
			AssertEquals("wrapperFull.DepartureSlotReference", "PDFGU9832M", wrapperFull.DepartureSlotReference);
			AssertEquals("wrapperFull.ArrivalSlotTime", new ZDateTime(2007, 8, 24), wrapperFull.ArrivalSlotTime);
			AssertEquals("wrapperFull.DepartureSlotTime", new ZDateTime(2007, 8, 25), wrapperFull.DepartureSlotTime);
			AssertEquals("ArrivalCartageRef", "ARV111", wrapperFull.ArrivalCartageRef);
			AssertEquals("DepartureCartageRef", "DEP222", wrapperFull.DepartureCartageRef);
			AssertEquals("wrapperFull.DeliveryMode.Description", "CFS/CY", wrapperFull.DeliveryMode.Description);
			AssertEquals("wrapperFull.ExportDepotCustomsReference", "REF.213ADHJ", wrapperFull.ExportDepotCustomsReference);
			AssertEquals("wrapperFull.EmptyRequired", new ZDateTime(2008, 2, 11), wrapperFull.EmptyRequired);
			AssertEquals("wrapperFull.DepartureEstimatedPickup", new ZDateTime(2007, 9, 23), wrapperFull.DepartureEstimatedPickup);
			AssertEquals("wrapperFull.Length", 89.34m, wrapperFull.Length);
			AssertEquals("wrapperFull.Width", 12.90m, wrapperFull.Width);
			AssertEquals("wrapperFull.Height", 8.35m, wrapperFull.Height);
			Assert("wrapperFull.Damaged", wrapperFull.Damaged);
			Assert("wrapperFull.Frozen", !wrapperFull.Frozen);
			Assert("wrapperFull.Chilled", wrapperFull.Chilled);
			Assert("wrapperFull.ControlledAtmosphere", wrapperFull.ControlledAtmosphere);
			AssertEquals("wrapperFull.HumidityPercentage", new ZByte(23), wrapperFull.HumidityPercentage);
			AssertEquals("wrapperFull.AirVentFlow.Value", 12m, wrapperFull.AirVentFlow.Value);
			AssertEquals("wrapperFull.AirVentFlow.Unit.Code", "2L", wrapperFull.AirVentFlow.Unit.Code);
			AssertEquals("wrapperFull.ClipOnUnit", "REFRIG1", wrapperFull.ClipOnUnit);
			AssertEquals("wrapperFull.UNDGSubstances.Count", 1, wrapperFull.UNDGSubstances.Count);
			AssertEquals("wrapperFull.UNDGSubstances[0].UNNumber", "3398", wrapperFull.UNDGSubstances[0].UNNumber);
			AssertEquals("wrapperFull.DepartureContainerYardAddress.CompanyName", "TEST DEPARTURE CONTAINER YARD ADDRESS", wrapperFull.DepartureContainerYardAddress.CompanyName);
			AssertEquals("wrapperFull.ArrivalContainerYardAddress.CompanyName", "TEST ARRIVAL CONTAINER YARD ADDRESS", wrapperFull.ArrivalContainerYardAddress.CompanyName);
			AssertEquals("wrapperFull.ContainerJobID", "JobID", wrapperFull.ContainerJobID);
			AssertEquals("wrapperFull.Commodity.Count", 0, wrapperFull.Commodities.Count);
			AssertEquals("wrapperFull.IsChargeable", "No", wrapperFull.IsChargeable);
			AssertEquals("wrapperFull.IsPalletized", "No", wrapperFull.IsPalletized);
			AssertEquals("wrapperFull.Items", "0", wrapperFull.Packages);
			AssertEquals("wrapperFull.Pallets", "0", wrapperFull.Pallets);
			AssertEquals("wrapperFull.ContainerQuality", ZString.Empty, wrapperFull.ContainerQuality.Code);
			AssertEquals("wrapperFull.PrintTACImage", ZBool.False, wrapperFull.PrintTACImage);
			AssertEquals("wrapperFull.ExportDetention.Released", ZDateTime.Empty, wrapperFull.ExportDetention.Released);
			AssertEquals("wrapperFull.ImportDetention.Released", ZDateTime.Empty, wrapperFull.ImportDetention.Released);
			AssertEquals("wrapperFull.CFSClient", null, wrapperFull.CFSClient);
			AssertEquals("wrapperFull.UnpackShed", ZString.Empty, wrapperFull.UnpackShed);
		}

		public void TestFrozen()
		{
			BaseCusContainer container = Factory.New<BaseCusContainer>();
			CommonContainer jobContainer = container.JobContainer;
			jobContainer.JC_IsControlledAtmosphere = true;
			jobContainer.JC_SetPointTemp = -5.3m;

			ContainerWrapper wrapperFull = new ContainerWrapperFromCustoms(container, Factory);
			Assert("wrapperFull.Frozen", wrapperFull.Frozen);
			Assert("wrapperFull.Chilled", !wrapperFull.Chilled);
		}

		public void TestChilled()
		{
			BaseCusContainer container = Factory.New<BaseCusContainer>();
			CommonContainer jobContainer = container.JobContainer;
			jobContainer.JC_IsControlledAtmosphere = true;
			jobContainer.JC_SetPointTemp = 8.4m;

			ContainerWrapper wrapperFull = new ContainerWrapperFromCustoms(container, Factory);
			Assert("wrapperFull.Chilled", wrapperFull.Chilled);
			Assert("wrapperFull.Frozen", !wrapperFull.Frozen);
		}

		public void TestImportDetention()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZDateTime now = ZDateTime.Now;

				BaseCusContainer container = Factory.New<BaseCusContainer>();

				CommonContainer jobContainer = container.JobContainer;
				jobContainer.JC_FCLAvailable = now.AddDays(-15);
				jobContainer.JC_EmptyReturnedBy = now.AddDays(-6);
				jobContainer.JC_ContainerYardEmptyReturnGateIn = now.AddDays(-1);

				ContainerWrapperFromCustoms wrapper = new ContainerWrapperFromCustoms(container, Factory);

				AssertEquals("wrapper.ImportDetention.Released", now.AddDays(-15), wrapper.ImportDetention.Released);
				AssertEquals("wrapper.ImportDetention.LastFreeDay", now.AddDays(-6), wrapper.ImportDetention.LastFreeDay);
				AssertEquals("wrapper.ImportDetention.Returned", now.AddDays(-1), wrapper.ImportDetention.Returned);

				AssertEquals("wrapper.ImportDetention.FreeDays", 10, wrapper.ImportDetention.FreeDays);
				AssertEquals("wrapper.ImportDetention.DetentionDays", 5, wrapper.ImportDetention.DetentionDays);
			}
		}

		[TestDate(2016, 1, 2, 3, 4, 5)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestVGM()
		{
			var container = Factory.New<BaseCusContainer>();

			var jobContainer = container.JobContainer;
			jobContainer.JC_GrossWeight = 456;
			jobContainer.JC_GrossWeightUQ = "KG";
			jobContainer.JC_GrossWeightVerificationType = "PKG";

			var vgmAddress = jobContainer.DocAddresses.FindByDocAddressType(DocAddressType.GrossWeightVerifiedBy);
			vgmAddress.E2_AddressOverride = true;
			vgmAddress.E2_Contact = "Mr Robot";
			vgmAddress.E2_CompanyName = "Evil Corp";
			vgmAddress.E2_AddressType = "VGM";

			var wrapper = new ContainerWrapperFromFreight(jobContainer, Factory);
			AssertEquals(456m, wrapper.WeightGross.Value);
			AssertEquals("KG", wrapper.WeightGross.Unit.Code);
			AssertEquals("02-Jan-16 03:04", wrapper.VGMVerifiedDate.ToString("dd-MMM-yy HH:mm"));
			AssertEquals("Method 2 - Packages", wrapper.VGMMethod.Description);
			AssertEquals("Mr Robot", wrapper.VGMVerifiedByAddress.ContactName);
			AssertEquals("EVIL CORP", wrapper.VGMVerifiedByAddress.CompanyName);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerWrapperFromCustoms(null, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AirVentFlow : 12 2L
ArrivalContainerYardAddress : 
CFSClient :  is null
ContainerQuality : 
DeliveryMode : CFS/CY
DepartureContainerYardAddress : TEST DEPARTURE CONTAINER YARD ADDRESS
ExportDetention : 
FreightJob : 
ImportDetention : 
Mode : FCX - Full Container Load - Multiple Bills
OffHirePort : 
OnHirePort : 
Owner : 
PackCount : 
Registry : (No Default Field Value Available on Registry)
SetPointTemperature : 23.9 C
Status : 
Type : ZZGP - General Purpose Box
VGMMethod : NON - Not Verified
VGMVerifiedByAddress : 
VolumeGoods : 
WeightDunnage : 340.000 KG
WeightGoods : 23630.000 KG
WeightGross : 25420.000 KG
WeightTare : 1450.000 KG
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.E2_CompanyName = "TEST DEPARTURE CONTAINER YARD ADDRESS";

			var declaration = Factory.New<BaseJobDeclaration>();

			var containerType = Factory.New<RefContainer>();
			containerType.RC_Code = "ZZGP";
			containerType.RC_Description = "General Purpose Box";
			containerType.RC_TareWeight = 1450m;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000011";
			container.CO_RC = containerType.PK;
			container.CO_FCL_LCL_AIR = "FCX";
			container.SetPointTemp = 23.9m;
			container.SetPointTempUnit = "C";
			container.AirVentFlow = 12;
			container.AirVentFlowRateUnit = "2L";

			CommonContainer jobContainer = container.JobContainer;
			jobContainer.JC_DeliveryMode = Constants.DeliveryModes.Codes.CFS_CY;
			jobContainer.JC_OA_DepartureContainerYardAddress = docAddress.PK;
			var packLine = Factory.New<PackLine>();
			packLine.JL_ActualVolume = 23.32m;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			packLine.JL_PackageCount = 24;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Package;
			jobContainer.AddPackLine(packLine);

			container.CO_Weight = 23630m;
			container.DunnageWeight = 340m;

			return new ContainerWrapperFromCustoms(container, Factory);
		}

		#region Implementation

		internal static void AddCusPackage(BaseJobDeclaration declaration, string containerNo, int packQty, string packType, string houseBill)
		{
			var package1 = declaration.Packages.AddNew();
			package1.CW_ContainerNoOrEquipmentNo = containerNo;
			package1.CW_PackQty = packQty;
			package1.CW_PackType = packType;
			package1.CW_HouseBill = houseBill;
		}

		#endregion
	}
}
