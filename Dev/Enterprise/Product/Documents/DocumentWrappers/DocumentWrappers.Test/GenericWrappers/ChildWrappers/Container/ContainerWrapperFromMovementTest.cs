using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperFromMovement))]
	sealed class ContainerWrapperFromMovementTest : ContainerWrapperTest
	{
		public void TestFreightJob()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("NORDCLOUD", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "018N";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_UniqueConsignRef = "S18181818";
			shipment.JS_JX = voyage.Sailings[0].PK;

			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "TEST4100013";

			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			stock.R6_ContainerNum = "TEST4100013";

			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementDate = ZDateTime.Now;
			movement.E9_MovementType = ContainerMovementTypes.Codes.WharfGateIn;
			movement.E9_JV = voyage.PK;

			Factory.Save();
			ContainerWrapperFromMovement wrapper = new ContainerWrapperFromMovement(movement, Factory);
			AssertEquals("Wrapped business object should be AgencyShipment", "S18181818", wrapper.FreightJob.JobNumber);
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

		public void TestImportDetention()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 5 }))
			{
				ZDateTime now = ZDateTime.Now.ToSmallDateTime();

				JobVoyage voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";

				VoyageDestination destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "SGSIN";
				destination.JB_AvailabilityDate = now.AddDays(-15);

				BillOfLading bill = Factory.New<BillOfLading>();
				bill.JS_JX = voyage.Sailings[0].PK;

				BillOfLadingContainer container = bill.RealContainers.AddNew();
				container.JC_ContainerNum = "TEST4100013";
				container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				container.JC_EmptyReturnedBy = now.AddDays(-6);

				Factory.Save();

				RefContainerStock stock = container.Stock;

				ContainerMovement movement = stock.Movements.AddNew();
				movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
				movement.E9_MovementDate = now.AddDays(-1);
				movement.E9_JV = voyage.PK;

				ContainerWrapperFromMovement wrapper = new ContainerWrapperFromMovement(movement, Factory);

				AssertEquals("wrapper.ImportDetention.Released", now.AddDays(-15), wrapper.ImportDetention.Released);
				AssertEquals("wrapper.ImportDetention.LastFreeDay", now.AddDays(-6), wrapper.ImportDetention.LastFreeDay);
				AssertEquals("wrapper.ImportDetention.Returned", now.AddDays(-1), wrapper.ImportDetention.Returned);

				AssertEquals("wrapper.ImportDetention.FreeDays", 10, wrapper.ImportDetention.FreeDays);
				AssertEquals("wrapper.ImportDetention.DetentionDays", 5, wrapper.ImportDetention.DetentionDays);
			}
		}

		public void TestExportDetention()
		{
			using (FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = 10 }))
			{
				ZDateTime now = ZDateTime.Now.ToSmallDateTime();

				RefContainerStock stock = Factory.New<RefContainerStock>();
				stock.R6_ContainerNum = "TEST4100013";
				stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

				ContainerMovement movement1 = stock.Movements.AddNew();
				movement1.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
				movement1.E9_MovementDate = now.AddDays(-15);

				ContainerMovement movement2 = stock.Movements.AddNew();
				movement2.E9_MovementType = ContainerMovementTypes.Codes.ReturnedUnshipped;
				movement2.E9_MovementDate = now.AddDays(-1);

				ContainerWrapperFromMovement wrapper = new ContainerWrapperFromMovement(movement2, Factory);

				AssertEquals("wrapper.ExportDetention.Released", now.AddDays(-15), wrapper.ExportDetention.Released);
				AssertEquals("wrapper.ExportDetention.LastFreeDay", now.AddDays(-6), wrapper.ExportDetention.LastFreeDay);
				AssertEquals("wrapper.ExportDetention.Returned", now.AddDays(-1), wrapper.ExportDetention.Returned);

				AssertEquals("wrapper.ExportDetention.FreeDays", 10, wrapper.ExportDetention.FreeDays);
				AssertEquals("wrapper.ExportDetention.DetentionDays", 5, wrapper.ExportDetention.DetentionDays);
			}
		}

		public override void TestWrapperMappingFull()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "client";

			var principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "principal";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("MAJAPAHIT", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "018N";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();

			var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = containerType.PK;

			var movement = stock.Movements.AddNew();
			movement.E9_MovementDate = new ZDateTime(2009, 05, 17);
			movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			movement.E9_JV = voyage.PK;

			var bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;

			var container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "TEST4100013";

			var detention = Factory.New<ContainerDetention>();
			detention.NC_OH_Client = client.PK;
			detention.NC_OH_Principal = principal.PK;
			detention.Movements.Add(movement);

			Factory.Save();

			var wrapperFull = new ContainerWrapperFromMovement(movement, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("wrapperFull.Hazardous", false, wrapperFull.IsHazardous);
				AssertEquals("wrapperFull.Mode.Code", "", wrapperFull.Mode.Code);
				AssertEquals("wrapperFull.Type.Code", "20GP", wrapperFull.Type.Code);
				AssertEquals("wrapperFull.Type.Description", "Twenty foot general purpose", wrapperFull.Type.Description);
				AssertEquals("wrapperFull.VolumeGoods.Value", 0m, wrapperFull.VolumeGoods.Value);
				AssertEquals("wrapperFull.VolumeGoods.Unit.Code", "M3", wrapperFull.VolumeGoods.Unit.Code);
				AssertEquals("wrapperFull.WeightTare.Value", 0m, wrapperFull.WeightTare.Value);
				AssertEquals("wrapperFull.WeightTare.Unit.Code", "KG", wrapperFull.WeightTare.Unit.Code);
				AssertEquals("wrapperFull.WeightGoods.Value", 0m, wrapperFull.WeightGoods.Value);
				AssertEquals("wrapperFull.WeightGoods.Unit.Code", "KG", wrapperFull.WeightGoods.Unit.Code);
				AssertEquals("wrapperFull.WeightDunnage.Value", 0m, wrapperFull.WeightDunnage.Value);
				AssertEquals("wrapperFull.WeightDunnage.Unit.Code", "KG", wrapperFull.WeightDunnage.Unit.Code);
				AssertEquals("wrapperFull.WeightGross.Value", 0m, wrapperFull.WeightGross.Value);
				AssertEquals("wrapperFull.WeightGross.Unit.Code", "KG", wrapperFull.WeightGross.Unit.Code);
				AssertEquals("wrapperFull.SetPointTemperature.Value", 0m, wrapperFull.SetPointTemperature.Value);
				AssertEquals("wrapperFull.SetPointTemperature.Unit.Code", "", wrapperFull.SetPointTemperature.Unit.Code);
				AssertEquals("wrapperFull.ContainerNo", "TEST4100013", wrapperFull.ContainerNo);
				AssertEquals("wrapperFull.ContainerNumberOrTypeCount", "TEST4100013", wrapperFull.ContainerNumberOrTypeCount);
				AssertEquals("wrapperFull.SealNo", "", wrapperFull.SealNo);
				AssertEquals("wrapperFull.SealNo2", "", wrapperFull.SealNo2);
				AssertEquals("wrapperFull.SealNo3", "", wrapperFull.SealNo3);
				AssertEquals("wrapperFull.ReleaseNumber", "", wrapperFull.ReleaseNumber);
				AssertEquals("wrapperFull.ArrivalEstimatedDelivery", ZDateTime.Empty, wrapperFull.ArrivalEstimatedDelivery);
				AssertEquals("wrapperFull.ArrivalReleaseNumber", "", wrapperFull.ArrivalReleaseNumber);
				AssertEquals("wrapperFull.Services.Count", 0, wrapperFull.Services.Count);
				AssertEquals("wrapperFull.EmptyReturnedBy", ZDateTime.Empty, wrapperFull.EmptyReturnedBy);
				AssertEquals("wrapperFull.ContainerYardEmptyReturnGateIn", ZDateTime.Empty, wrapperFull.ContainerYardEmptyReturnGateIn);
				AssertEquals("wrapperFull.PackCount.ValueAndUnitCodeBlankIfZero", "", wrapperFull.PackCount.ValueAndUnitCodeBlankIfZero);
				AssertEquals("wrapperFull.Commodity.Count", 0, wrapperFull.Commodities.Count);
				AssertEquals("wrapperFull.BookingReference", "", wrapperFull.BookingReference);
				AssertEquals("wrapperFull.ArrivalSlotReference", "", wrapperFull.ArrivalSlotReference);
				AssertEquals("wrapperFull.DepartureSlotReference", "", wrapperFull.DepartureSlotReference);
				AssertEquals("wrapperFull.ArrivalSlotTime", ZDateTime.Empty, wrapperFull.ArrivalSlotTime);
				AssertEquals("wrapperFull.DepartureSlotTime", ZDateTime.Empty, wrapperFull.DepartureSlotTime);
				AssertEquals("wrapperFull.DeliveryMode.Description", "", wrapperFull.DeliveryMode.Description);
				AssertEquals("wrapperFull.ExportDepotCustomsReference", "", wrapperFull.ExportDepotCustomsReference);
				AssertEquals("wrapperFull.EmptyReadyForReturn", ZDateTime.Empty, wrapperFull.EmptyReadyForReturn);
				AssertEquals("wrapperFull.EmptyRequired", ZDateTime.Empty, wrapperFull.EmptyRequired);
				AssertEquals("wrapperFull.WharfGateOut", ZDateTime.Empty, wrapperFull.WharfGateOut);
				AssertEquals("wrapperFull.DepartureEstimatedPickup", ZDateTime.Empty, wrapperFull.DepartureEstimatedPickup);
				AssertEquals("wrapperFull.Length", 20m, wrapperFull.Length);
				AssertEquals("wrapperFull.Width", 8m, wrapperFull.Width);
				AssertEquals("wrapperFull.Height", 8.5m, wrapperFull.Height);
				AssertEquals("wrapperFull.Damaged", false, wrapperFull.Damaged);
				AssertEquals("wrapperFull.Frozen", false, wrapperFull.Frozen);
				AssertEquals("wrapperFull.Chilled", false, wrapperFull.Chilled);
				AssertEquals("wrapperFull.ControlledAtmosphere", false, wrapperFull.ControlledAtmosphere);
				AssertEquals("wrapperFull.HumidityPercentage", ZByte.Zero, wrapperFull.HumidityPercentage);
				AssertEquals("wrapperFull.AirVentFlow.ValueAndUnitCodeBlankIfZero", "", wrapperFull.AirVentFlow.ValueAndUnitCodeBlankIfZero);
				AssertEquals("wrapperFull.ClipOnUnit", ZString.Empty, wrapperFull.ClipOnUnit);
				AssertEquals("wrapperFull.UNDGSubstances.Count", 0, wrapperFull.UNDGSubstances.Count);
				AssertEquals("wrapperFull.DepartureContainerYardAddress.CompanyName", "", wrapperFull.DepartureContainerYardAddress.CompanyName);
				AssertEquals("wrapperFull.ArrivalContainerYardAddress.CompanyName", "", wrapperFull.ArrivalContainerYardAddress.CompanyName);
				AssertEquals("wrapperFull.ContainerJobID", "", wrapperFull.ContainerJobID);
				AssertEquals("wrapperFull.IsChargeable", "No", wrapperFull.IsChargeable);
				AssertEquals("wrapperFull.IsPalletized", "No", wrapperFull.IsPalletized);
				AssertEquals("wrapperFull.Items", "0", wrapperFull.Packages);
				AssertEquals("wrapperFull.Pallets", "0", wrapperFull.Pallets);
				AssertEquals("wrapperFull.ContainerQuality", ZString.Empty, wrapperFull.ContainerQuality.Code);
				AssertEquals("wrapperFull.PrintTACImage", ZBool.False, wrapperFull.PrintTACImage);
				AssertEquals("wrapperFull.CFSClient", null, wrapperFull.CFSClient);
				AssertEquals("wrapperFull.UnpackShed", ZString.Empty, wrapperFull.UnpackShed);
			});
		}

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerWrapperFromMovement(null, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			principal.OH_Code = "prinicpal";

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_Code = "client";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();

			RefContainer containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = containerType.PK;

			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_MovementDate = new ZDateTime(2009, 05, 17);
			movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateOut;
			movement.E9_JV = voyage.PK;

			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;

			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "TEST4100013";

			ContainerDetention detention = Factory.New<ContainerDetention>();

			detention.Movements.Add(movement);
			detention.NC_OH_Client = client.PK;
			detention.NC_OH_Principal = principal.PK;

			Factory.Save();

			return new ContainerWrapperFromMovement(movement, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
AirVentFlow : 
ArrivalContainerYardAddress : 
CFSClient :  is null
ContainerQuality : 
DeliveryMode : 
DepartureContainerYardAddress : 
ExportDetention : 
FreightJob : V00001000
ImportDetention : 
Mode : 
OffHirePort : 
OnHirePort : 
Owner : 
PackCount : 
Registry : (No Default Field Value Available on Registry)
SetPointTemperature : 
Status : 
Type : 20GP - Twenty foot general purpose
VGMMethod : 
VGMVerifiedByAddress : 
VolumeGoods : 
WeightDunnage : 
WeightGoods : 
WeightGross : 
WeightTare :
";
			}
		}

		#endregion
	}
}
