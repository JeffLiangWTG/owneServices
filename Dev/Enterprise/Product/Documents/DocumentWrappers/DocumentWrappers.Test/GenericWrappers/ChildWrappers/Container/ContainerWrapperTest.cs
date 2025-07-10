using CargoWise.Types;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class ContainerWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (ContainerWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Mode.Code", ZString.Empty, wrapperEmpty.Mode.Code);
			AssertEquals("wrapperEmpty.Type.Code", ZString.Empty, wrapperEmpty.Type.Code);
			AssertEquals("wrapperEmpty.SetPointTemperature", ZString.Empty, wrapperEmpty.SetPointTemperature.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.VolumeGoods.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.VolumeGoods.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightTare.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.WeightTare.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightGoods.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.WeightGoods.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightDunnage.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.WeightDunnage.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.WeightGross.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.WeightGross.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.ContainerNo", ZString.Empty, wrapperEmpty.ContainerNo);
			AssertEquals("wrapperEmpty.SealNo", ZString.Empty, wrapperEmpty.SealNo);
			AssertEquals("wrapperEmpty.SealNo2", ZString.Empty, wrapperEmpty.SealNo2);
			AssertEquals("wrapperEmpty.SealNo3", ZString.Empty, wrapperEmpty.SealNo3);
			AssertEquals("wrapperEmpty.ReleaseNumber", ZString.Empty, wrapperEmpty.ReleaseNumber);
			AssertEquals("wrapperEmpty.ArrivalEstimatedDelivery", ZDateTime.Empty, wrapperEmpty.ArrivalEstimatedDelivery);
			AssertEquals("wrapperEmpty.ArrivalReleaseNumber", ZString.Empty, wrapperEmpty.ArrivalReleaseNumber);
			AssertEquals("wrapperEmpty.Services", 0, wrapperEmpty.Services.Count);
			AssertEquals("wrapperEmpty.PackCount.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.PackCount.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.EmptyReturnedBy", ZDateTime.Empty, wrapperEmpty.EmptyReturnedBy);
			AssertEquals("wrapperEmpty.ContainerYardEmptyReturnGateIn", ZDateTime.Empty, wrapperEmpty.ContainerYardEmptyReturnGateIn);
			AssertEquals("wrapperEmpty.ExportDepotCustomsReference", ZString.Empty, wrapperEmpty.ExportDepotCustomsReference);
			AssertEquals("wrapperEmpty.EmptyReadyForReturn", ZDateTime.Empty, wrapperEmpty.EmptyReadyForReturn);
			AssertEquals("wrapperEmpty.EmptyRequired", ZDateTime.Empty, wrapperEmpty.EmptyRequired);
			AssertEquals("wrapperEmpty.EmptyReturnReference", ZString.Empty, wrapperEmpty.EmptyReturnReference);
			AssertEquals("wrapperEmpty.WharfGateOut", ZDateTime.Empty, wrapperEmpty.WharfGateOut);
			AssertEquals("wrapperEmpty.DepartureEstimatedPickup", ZDateTime.Empty, wrapperEmpty.DepartureEstimatedPickup);
			AssertEquals("wrapperEmpty.Length", ZDecimal.Zero, wrapperEmpty.Length);
			AssertEquals("wrapperEmpty.Width", ZDecimal.Zero, wrapperEmpty.Width);
			AssertEquals("wrapperEmpty.Height", ZDecimal.Zero, wrapperEmpty.Height);
			AssertEquals("wrapper.ContainerJobID", ZString.Empty, wrapperEmpty.ContainerJobID);
			Assert("wrapperEmpty.Damaged", !wrapperEmpty.Damaged);
			Assert("wrapperEmpty.Frozen", !wrapperEmpty.Frozen);
			Assert("wrapperEmpty.Chilled", !wrapperEmpty.Chilled);
			AssertEquals("wrapperEmpty.HumidityPercentage", ZByte.Zero, wrapperEmpty.HumidityPercentage);
			AssertEquals("wrapperEmpty.AirVentFlow.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapperEmpty.AirVentFlow.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.ClipOnUnit", ZString.Empty, wrapperEmpty.ClipOnUnit);
			AssertEquals("wrapperEmpty.UNDGNumbers.Count", 0, wrapperEmpty.UNDGSubstances.Count);
			AssertEquals("wrapperEmpty.DepartureContainerYardAddress.CompanyName", ZString.Empty, wrapperEmpty.DepartureContainerYardAddress.CompanyName);
			AssertEquals("wrapperEmpty.IsChargeable", "No", wrapperEmpty.IsChargeable);
			AssertEquals("wrapperEmpty.IsPalletized", "No", wrapperEmpty.IsPalletized);
			AssertEquals("wrapperEmpty.Packages", "0", wrapperEmpty.Packages);
			AssertEquals("wrapperEmpty.Pallets", "0", wrapperEmpty.Pallets);
			AssertEquals("wrapperEmpty.StowagePosition", ZString.Empty, wrapperEmpty.StowagePosition);
		}

		public abstract void TestWrapperMappingFull();

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Container                                 (Default Field: ContainerNo)
======================================================================
Name                                    Type
----------------------------------------------------------------------
ArrivalContainerYardAddress             Address
DepartureContainerYardAddress           Address
VGMVerifiedByAddress                    Address
ContainerQuality                        CodeAndDescription
DeliveryMode                            CodeAndDescription
Mode                                    CodeAndDescription
Status                                  CodeAndDescription
VGMMethod                               CodeAndDescription
Type                                    ContainerType
ExportDetention                         Detention Information
ImportDetention                         Detention Information
FreightJob                              Freight
OffHirePort                             Location
OnHirePort                              Location
CFSClient                               Organisation
Owner                                   Organisation
AirVentFlow                             ValueAndUnit
PackCount                               ValueAndUnit
SetPointTemperature                     ValueAndUnit
VolumeGoods                             Volume
WeightDunnage                           Weight
WeightGoods                             Weight
WeightGross                             Weight
WeightTare                              Weight
AMSNumber                               String
ArrivalCartageRef                       String
ArrivalEstimatedDelivery                DateTime
ArrivalReleaseNumber                    String
ArrivalSlotReference                    String
ArrivalSlotTime                         DateTime
BookingReference                        String
Chilled                                 Bool
ClipOnUnit                              String
ContainerCount                          Int
ContainerJobID                          String
ContainerNo                             String
ContainerNumberOrTypeCount              String
ContainerYardEmptyReturnGateIn          DateTime
ControlledAtmosphere                    Bool
CreatedByUserName                       String
Damaged                                 Bool
DepartureCartageRef                     String
DepartureEstimatedPickup                DateTime
DepartureSlotReference                  String
DepartureSlotTime                       DateTime
EmptyReadyForReturn                     DateTime
EmptyRequired                           DateTime
EmptyReturnedBy                         DateTime
EmptyReturnReference                    String
ExportDepotCustomsReference             String
Frozen                                  Bool
GateInDate                              DateTime
GateOutDate                             DateTime
Height                                  Decimal
HumidityPercentage                      Byte
IsChargeable                            String
IsHazardous                             Bool
IsPalletized                            String
IsReefer                                Bool
ITReferenceNumber                       String
Length                                  Decimal
ManufactureDate                         DateTime
OffHireDate                             DateTime
OnHireDate                              DateTime
Packages                                String
Pallets                                 String
PrintTACImage                           Bool
ReleaseNumber                           String
SealNo                                  String
SealNo2                                 String
SealNo3                                 String
StowagePosition                         String
UnpackShed                              String
VGMVerifiedDate                         DateTime
WharfGateOut                            DateTime
Width                                   Decimal

Commodities                             Commodity Collection
Services                                ContainerService Collection
CustomsEntries                          CustomsEntry Collection
UNDGSubstances                          UNDGSubstance Collection
";
			}
		}
	}
}
