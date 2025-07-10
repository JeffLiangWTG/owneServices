using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusSeaManOBLDetailCargoLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCargoType()
		{
			line.Header.BO_HeaderCargoType = "E";
			line.BD_LineCargoType = Core.Constants.ContainerModes.Bulk;
			AssertHasMessageErrors("when set to E and BLK", line.BD_LineCargoTypeInfo);

			line.BD_LineCargoType = Core.Constants.ContainerModes.FCL;
			AssertNoMessageErrors("when set to E and FCL", line.BD_LineCargoTypeInfo);

			line.Header.BO_HeaderCargoType = "X";
			line.BD_LineCargoType = Core.Constants.ContainerModes.Bulk;
			AssertNoMessageErrors("when set to X and BLK", line.BD_LineCargoTypeInfo);

			line.BD_LineCargoType = Core.Constants.ContainerModes.FCL;
			AssertNoMessageErrors("when set to X and FCL", line.BD_LineCargoTypeInfo);

			line.BD_LineCargoType = ZString.Empty;
			AssertHasMessageErrors("when empty", line.BD_LineCargoTypeInfo);

			line.BD_LineCargoType = "BBK";
			AssertHasError(line.BD_LineCargoTypeInfo, "Enter a valid Cargo Type.");

			line.BD_LineCargoType = "B/B";
			AssertNoError(line.BD_LineCargoTypeInfo, "Enter a valid Cargo Type.");
		}

		public void TestValidatePackageType()
		{
			line.BD_PackType = ZString.Empty;
			AssertNoMessageErrors("when empty", line.BD_PackTypeInfo);

			line.BD_LineCargoType = CMRCargoTypes.Codes.BreakBulk;
			line.BD_PackType = ZString.Empty;
			AssertHasMessageErrors("when b/b and not entered", line.BD_PackTypeInfo);

			line.BD_PackType = CMRPackageTypes.Codes.BeerCrate;
			AssertNoMessageErrors("when b/b and entered", line.BD_PackTypeInfo);

			line.BD_LineCargoType = Core.Constants.ContainerModes.FCL;
			line.BD_PackType = ZString.Empty;
			AssertNoMessageErrors("when fcl and empty", line.BD_PackTypeInfo);

			line.BD_PackType = "?";
			AssertHasMessageErrorContaining(line.BD_PackTypeInfo, "The code you have selected is not in the list");
			line.BD_PackType = CMRPackageTypes.Codes.UnpackedOrPacked;
			AssertNoMessageErrorContaining(line.BD_PackTypeInfo, "The code you have selected is not in the list");
		}

		public void TestValidateNumberOfPackages()
		{
			line.BD_NoOfPacks = 0;
			AssertNoMessageErrors("when empty", line.BD_NoOfPacksInfo);

			line.BD_LineCargoType = Core.Constants.ContainerModes.Bulk;
			line.BD_NoOfPacks = 0;
			AssertNoMessageErrors("when blk and 0", line.BD_NoOfPacksInfo);

			line.BD_NoOfPacks = 1;
			AssertHasMessageErrors("when blk and 1", line.BD_NoOfPacksInfo);

			line.BD_LineCargoType = CMRCargoTypes.Codes.BreakBulk;
			line.BD_NoOfPacks = 0;
			AssertHasMessageErrors("when b/b and 0", line.BD_NoOfPacksInfo);

			line.BD_NoOfPacks = 1;
			AssertNoMessageErrors("when b/b and 1", line.BD_NoOfPacksInfo);

			line.BD_LineCargoType = Core.Constants.ContainerModes.FCL;
			line.BD_NoOfPacks = 0;
			AssertNoMessageErrors("when fcl and 0", line.BD_NoOfPacksInfo);

			line.BD_NoOfPacks = 1;
			AssertHasMessageErrors("when fcl and 1", line.BD_NoOfPacksInfo);
		}

		public void TestValidateCargoIdentifier()
		{
			CusSeaManOBLHeaderCargoLine header2 = line.Header.Port.CargoLines.AddNew();
			CusSeaManOBLDetailCargoLine line2 = header2.Detail;

			line.BD_ContainerNumber = ZString.Empty;
			AssertHasMessageErrors("when empty", line.BD_ContainerNumberInfo);

			line.BD_ContainerNumber = "blah";
			AssertNoMessageErrors("when filled", line.BD_ContainerNumberInfo);

			line2.BD_ContainerNumber = "blAh";
			AssertHasErrorContaining(line2.BD_ContainerNumberInfo, "This Cargo Identifier has already been used on another line of this Cargo List.");

			line2.BD_ContainerNumber = "blaha";
			AssertNoErrorContaining(line2.BD_ContainerNumberInfo, "This Cargo Identifier has already been used on another line of this Cargo List.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusSeaManArrivalPort port = Factory.New<CusSeaManArrivalPort>();
			port.BA_RL_NKArrivalPort = "AUSYD";
			CusSeaManOBLHeaderCargoLine header = port.CargoLines.AddNew();
			line = header.Detail;
		}
		CusSeaManOBLDetailCargoLine line;
	}
}
