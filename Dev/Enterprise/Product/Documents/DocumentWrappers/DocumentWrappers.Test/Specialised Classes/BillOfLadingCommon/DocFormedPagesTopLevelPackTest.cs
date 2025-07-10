using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocFormedPagesTopLevelPack))]
	sealed class DocFormedPagesTopLevelPackTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCtor()
		{
			DocFormedPagesTopLevelPack docVehicle = new DocFormedPagesTopLevelPack(CreatePopulatedRORContainer());
			CombineAssertions(() =>
			{
				AssertEquals(Constants.ContainerModes.RollOnRollOff, docVehicle.Mode);
				AssertEquals("ABC123", docVehicle.ReferenceNumber);
				AssertEquals("CCC", docVehicle.PackType);

				AssertEquals("Red", docVehicle.VehicleColour);
				AssertEquals("Honda", docVehicle.VehicleMake);
				AssertEquals("CRX", docVehicle.VehicleModel);
				AssertEquals((ZByte)2, docVehicle.VehicleNumberOfDoors);
				AssertEquals("MAN", docVehicle.VehicleTransmission);
				AssertEquals((ZShort)1989, docVehicle.VehicleYear);

				AssertEquals(10, docVehicle.Count);
				AssertEquals("Description of the Vehicle", docVehicle.Description);
				AssertEquals("M&N of the Vehicle", docVehicle.MarksAndNumbers);
				AssertEquals("AAA", docVehicle.Commodity.Code);
				AssertEquals("AAA Description", docVehicle.Commodity.Description);
				AssertEquals("BBB", docVehicle.HarmonisedCode);

				AssertEquals(10m, docVehicle.Volume);
				AssertEquals(Constants.Volume.CubicMetres, docVehicle.VolumeUnit);

				AssertEquals(1000m, docVehicle.Weight);
				AssertEquals(Constants.Weight.Kilograms, docVehicle.WeightUnit);

				AssertEquals(1.1m, docVehicle.Length);
				AssertEquals(1.2m, docVehicle.Height);
				AssertEquals(1.3m, docVehicle.Width);
				AssertEquals(Constants.Length.Metres, docVehicle.DimensionUnit);

				AssertEquals(1, docVehicle.UNDGs.Length);
				AssertEquals("FOOBR UNDG Subtance Description", docVehicle.UNDGs[0].ProperShippingName);
				AssertNotNull("DG Contact should be blank, not null", docVehicle.UNDGs[0].DGContact);
			});
		}

		public void TestIDocPackageDetails()
		{
			DocFormedPagesTopLevelPack docVehicle = new DocFormedPagesTopLevelPack(CreatePopulatedRORContainer());
			var packageDetails = docVehicle as IDocPackageDetails;

			CombineAssertions(() =>
			{
				AssertEquals("ABC123", packageDetails.ReferenceNumber);
				AssertEquals("Red", packageDetails.VehicleColour);
				AssertEquals("Honda", packageDetails.VehicleMake);
				AssertEquals("CRX", packageDetails.VehicleModel);
				AssertEquals((ZByte)2, packageDetails.VehicleNumberOfDoors);
				AssertEquals("MAN", packageDetails.VehicleTransmission);
				AssertEquals((ZShort)1989, packageDetails.VehicleYear);

				AssertEquals(10, packageDetails.Count);
				AssertEquals("CCC", packageDetails.PackType);
				AssertEquals("Description of the Vehicle", packageDetails.Description);
				AssertEquals("Description of the Vehicle", packageDetails.DetailedDescription);
				AssertEquals("M&N of the Vehicle", packageDetails.MarksAndNumbers);

				AssertEquals(10m, packageDetails.Volume);
				AssertEquals(Constants.Volume.CubicMetres, packageDetails.VolumeUnit);

				AssertEquals(1000m, packageDetails.Weight);
				AssertEquals(Constants.Weight.Kilograms, packageDetails.WeightUnit);

				AssertEquals(1.1m, packageDetails.Length);
				AssertEquals(1.2m, packageDetails.Height);
				AssertEquals(1.3m, packageDetails.Width);
				AssertEquals(Constants.Length.Metres, packageDetails.DimensionUnit);

				AssertEquals("AAA", packageDetails.Commodity.Code);
				AssertEquals("AAA Description", packageDetails.Commodity.Description);
				AssertEquals("BBB", packageDetails.HarmonisedCode);
				AssertEquals(1, packageDetails.UNDGs.Length);
				AssertEquals("FOOBR UNDG Subtance Description", packageDetails.UNDGs[0].ProperShippingName);
			});
		}

		#region Implementation

		AgencyShipmentContainer CreatePopulatedRORContainer()
		{
			RefCommodityCode commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AAA";
			commodity.RH_Description = "AAA Description";

			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;

			var container = shipment.ShippingContainers.AddNew();
			container.JC_ContainerNum = "ABC123";
			container.JC_VehicleColor = "Red";
			container.JC_VehicleMake = "Honda";
			container.JC_VehicleModel = "CRX";
			container.JC_VehicleNumberOfDoors = 2;
			container.JC_VehicleTransmission = "MAN";
			container.JC_VehicleYear = 1989;

			container.JC_ContainerCount = 10;
			container.JC_Description = "Description of the Vehicle";
			container.JC_MarksAndNumbers = "M&N of the Vehicle";
			container.JC_RH_NKContainerCommodityCode = commodity.RH_Code;
			container.JC_HarmonisedCode = "BBB";
			container.JC_F3_NKPackType = "CCC";

			container.JC_TotalUnitOfMeasure = Constants.Length.Metres;
			container.JC_TotalLength = 1.1;
			container.JC_TotalHeight = 1.2;
			container.JC_TotalWidth = 1.3;

			container.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
			container.JC_GrossVolume = 10m;

			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container.JC_GrossWeight = 1000m;

			UNDGSubstance undgSubtance = Factory.New<UNDGSubstance>();
			undgSubtance.DG_Code = "FOOBR";
			undgSubtance.DG_PSN = "FOOBR UNDG Subtance Description";
			undgSubtance.DG_UNNO = "FOOB";
			undgSubtance.DG_Variant = "R";

			UNDGDataItem undg = Factory.New<UNDGDataItem>();
			undg.DI_DG = undgSubtance.PK;

			container.UNDGs.Add(undg);

			return container;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			AgencyShipmentContainer container = Factory.New<AgencyShipmentContainer>();
			return new DocFormedPagesTopLevelPack(container);
		}

		#endregion
	}
}
