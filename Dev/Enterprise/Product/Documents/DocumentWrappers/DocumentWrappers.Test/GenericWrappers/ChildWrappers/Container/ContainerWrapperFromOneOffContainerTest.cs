using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerWrapperFromOneOffContainer))]
	sealed class ContainerWrapperFromOneOffContainerTest : ContainerWrapperTest
	{
		public void TestFreightJob()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.TH_QuoteNumber = "55554";
			RateOneOffContainers container = quote.CurrentOneOffQuote.Containers.AddNew();
			ContainerWrapperFromOneOffContainer wrapper = new ContainerWrapperFromOneOffContainer(container, Factory);
			AssertEquals("Wrapped business object should be Quote", "55554", wrapper.FreightJob.JobNumber);
		}

		public override void TestWrapperMappingFull()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			quote.CurrentOneOffQuote.TT_RH_NKCommodity = "IRON";

			var container = quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 5;
			container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			Factory.Save();

			var wrapper = new ContainerWrapperFromOneOffContainer(container, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("wrapper.Mode.Code", "", wrapper.Mode.Code);
				AssertEquals("wrapper.Type.Code", "20GP", wrapper.Type.Code);
				AssertEquals("wrapper.Type.Description", "Twenty foot general purpose", wrapper.Type.Description);
				AssertEquals("wrapper.VolumeGoods.Value", 0m, wrapper.VolumeGoods.Value);
				AssertEquals("wrapper.VolumeGoods.Unit.Code", "M3", wrapper.VolumeGoods.Unit.Code);
				AssertEquals("wrapper.WeightTare.Value", 2280m * 5, wrapper.WeightTare.Value);
				AssertEquals("wrapper.WeightTare.Unit.Code", "KG", wrapper.WeightTare.Unit.Code);
				AssertEquals("wrapper.WeightGoods.Value", 0m, wrapper.WeightGoods.Value);
				AssertEquals("wrapper.WeightGoods.Unit.Code", "KG", wrapper.WeightGoods.Unit.Code);
				AssertEquals("wrapper.WeightDunnage.Value", 0m, wrapper.WeightDunnage.Value);
				AssertEquals("wrapper.WeightDunnage.Unit.Code", "KG", wrapper.WeightDunnage.Unit.Code);
				AssertEquals("wrapper.WeightGross.Value", 2280m * 5m, wrapper.WeightGross.Value);
				AssertEquals("wrapper.WeightGross.Unit.Code", "KG", wrapper.WeightGross.Unit.Code);
				AssertEquals("wrapper.SetPointTemperature.Value", 0m, wrapper.SetPointTemperature.Value);
				AssertEquals("wrapper.SetPointTemperature.Unit.Code", "", wrapper.SetPointTemperature.Unit.Code);
				AssertEquals("wrapper.ContainerNo", "", wrapper.ContainerNo);
				AssertEquals("wrapper.ContainerNumberOrTypeCount", "20GP (5)", wrapper.ContainerNumberOrTypeCount);
				AssertEquals("wrapper.SealNo", "", wrapper.SealNo);
				AssertEquals("wrapper.SealNo2", "", wrapper.SealNo2);
				AssertEquals("wrapper.SealNo3", "", wrapper.SealNo3);
				AssertEquals("wrapper.ReleaseNumber", "", wrapper.ReleaseNumber);
				AssertEquals("wrapper.ArrivalEstimatedDeliver", ZDateTime.Empty, wrapper.ArrivalEstimatedDelivery);
				AssertEquals("wrapper.ArrivalReleaseNumber", "", wrapper.ArrivalReleaseNumber);
				AssertEquals("wrapper.Services.Count", 0, wrapper.Services.Count);
				AssertEquals("wrapper.EmptyReturnedBy", ZDateTime.Empty, wrapper.EmptyReturnedBy);
				AssertEquals("wrapper.ContainerYardEmptyReturnGateIn", ZDateTime.Empty, wrapper.ContainerYardEmptyReturnGateIn);
				AssertEquals("wrapper.PackCount.ValueAndUnitCodeBlankIfZero", "", wrapper.PackCount.ValueAndUnitCodeBlankIfZero);
				AssertEquals("wrapper.Commodity.Count", 1, wrapper.Commodities.Count);
				AssertEquals("wrapper.BookingReference", "", wrapper.BookingReference);
				AssertEquals("wrapper.ArrivalSlotReference", "", wrapper.ArrivalSlotReference);
				AssertEquals("wrapper.DepartureSlotReference", "", wrapper.DepartureSlotReference);
				AssertEquals("wrapper.ArrivalSlotTime", ZDateTime.Empty, wrapper.ArrivalSlotTime);
				AssertEquals("wrapper.DepartureSlotTime", ZDateTime.Empty, wrapper.DepartureSlotTime);
				AssertEquals("wrapper.DeliveryMode.Description", "", wrapper.DeliveryMode.Description);
				AssertEquals("wrapper.ExportDepotCustomsReference", "", wrapper.ExportDepotCustomsReference);
				AssertEquals("wrapper.EmptyReadyForReturn", ZDateTime.Empty, wrapper.EmptyReadyForReturn);
				AssertEquals("wrapper.EmptyRequired", ZDateTime.Empty, wrapper.EmptyRequired);
				AssertEquals("wrapper.WharfGateOut", ZDateTime.Empty, wrapper.WharfGateOut);
				AssertEquals("wrapper.DepartureEstimatedPickup", ZDateTime.Empty, wrapper.DepartureEstimatedPickup);
				AssertEquals("wrapper.Length", 0m, wrapper.Length);
				AssertEquals("wrapper.Width", 0m, wrapper.Width);
				AssertEquals("wrapper.Height", 0m, wrapper.Height);
				AssertEquals("wrapper.Damaged", false, wrapper.Damaged);
				AssertEquals("wrapper.Frozen", false, wrapper.Frozen);
				AssertEquals("wrapper.Chilled", false, wrapper.Chilled);
				AssertEquals("wrapper.ControlledAtmosphere", false, wrapper.ControlledAtmosphere);
				AssertEquals("wrapper.HumidityPercentage", ZByte.Zero, wrapper.HumidityPercentage);
				AssertEquals("wrapper.AirVentFlow.ValueAndUnitCodeBlankIfZero", "", wrapper.AirVentFlow.ValueAndUnitCodeBlankIfZero);
				AssertEquals("wrapper.ClipOnUnit", ZString.Empty, wrapper.ClipOnUnit);
				AssertEquals("wrapper.UNDGSubstances.Count", 0, wrapper.UNDGSubstances.Count);
				AssertEquals("wrapper.DepartureContainerYardAddress.CompanyName", "", wrapper.DepartureContainerYardAddress.CompanyName);
				AssertEquals("wrapper.ContainerJobID", "", wrapper.ContainerJobID);
				AssertEquals("wrapper.IsChargeable", "No", wrapper.IsChargeable);
				AssertEquals("wrapper.IsPalletized", "No", wrapper.IsPalletized);
				AssertEquals("wrapper.Items", "0", wrapper.Packages);
				AssertEquals("wrapper.Pallets", "0", wrapper.Pallets);
				AssertEquals("wrapper.ContainerQuality", "", wrapper.ContainerQuality.Description);
				AssertEquals("wrapper.IsReefer", false, wrapper.IsReefer);
				AssertEquals("wrapper.PrintTACImage", ZBool.False, wrapper.PrintTACImage);
				AssertEquals("wrapper.ImportDetention.Released", ZDateTime.Empty, wrapper.ImportDetention.Released);
				AssertEquals("wrapper.ExportDetention.Released", ZDateTime.Empty, wrapper.ExportDetention.Released);
				AssertEquals("wrapper.CFSClient", null, wrapper.CFSClient);
				AssertEquals("wrapper.UnpackShed", ZString.Empty, wrapper.UnpackShed);
			});
		}

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			Quote quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			RateOneOffContainers container = quote.CurrentOneOffQuote.Containers.AddNew();
			return new ContainerWrapperFromOneOffContainer(container, Factory);
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedButNotAccepted);
			var container = quote.CurrentOneOffQuote.Containers.AddNew();
			container.TC_ContainerCount = 5;
			container.TC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			Factory.Save();

			return new ContainerWrapperFromOneOffContainer(container, Factory);
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
FreightJob : 1000
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
WeightGross : 11400.000 KG
WeightTare : 11400.000 KG
";
			}
		}

		#endregion
	}
}
