using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.FSH.TsManifest.Lines.Testing
{
	sealed class ContainerFieldTest : BaseLineTest
	{
		public void TestProperties()
		{
			AssertEquals("CargoSequenceNumber", 1, ContainerField.CargoSequenceNumber);
			AssertEquals("ContainerNumber", "CCLU4398759", ContainerField.ContainerNumber);
			AssertEquals("ContainerMode", Xsd.ContainerMode.FCL, ContainerField.ContainerMode);
			AssertEquals("SealNumber", "D851255", ContainerField.SealNumber);
			AssertEquals("ContainerSizeOrISOCode", "42G1", ContainerField.ContainerSizeOrISOCode);
			AssertEquals("NumberOfPackages", "64", ContainerField.NumberOfPackages);
			AssertEquals("NetWeight", 22832.7m, ContainerField.NetWeight);
			AssertEquals("Volume", 52.2m, ContainerField.Volume);
			AssertEquals("ShipperOwned", "N", ContainerField.ShipperOwned);
			AssertNotNull(ContainerField.OceanBill);
		}

		public void TestCargoFields()
		{
			AssertEquals("Precondition: no cargo fields", 0, ContainerField.CargoFields.Count);
			ContainerField.AttachCargoField(CargoField);
			AssertEquals(1, ContainerField.CargoFields.Count);
			AssertEquals(CargoField, ContainerField.CargoFields[0]);
		}
	}
}
