using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSeaManOBLHeaderCargoLine))]
	public class CusSeaManOBLHeaderCargoLineTest : BaseCusSeaManOBLHeaderTest
	{
		#region Proxied Properties

		public void TestCargoType()
		{
			line.Detail.BD_LineCargoType = "FOO";
			AssertEquals("proxied get", "FOO", line.CargoType);

			line.CargoType = "BAR";
			AssertEquals("proxied set", "BAR", line.Detail.BD_LineCargoType);

			AssertNotNull("info", line.CargoTypeInfo);
			AssertEquals("info maxlen", line.Detail.BD_LineCargoTypeInfo.MaxLength, line.CargoTypeInfo.MaxLength);
		}

		public void TestCargoIdentifier()
		{
			line.Detail.BD_ContainerNumber = "FOO";
			AssertEquals("proxied get", "FOO", line.CargoIdentifier);

			line.CargoIdentifier = "BAR";
			AssertEquals("proxied set", "BAR", line.Detail.BD_ContainerNumber);

			AssertNotNull("info", line.CargoIdentifierInfo);
			AssertEquals("info maxlen", 35, line.CargoIdentifierInfo.MaxLength);
		}

		public void TestPackageType()
		{
			line.Detail.BD_PackType = "FOO";
			AssertEquals("proxied get", "FOO", line.PackageType);

			line.PackageType = "BAR";
			AssertEquals("proxied set", "BAR", line.Detail.BD_PackType);

			AssertNotNull("info", line.PackageTypeInfo);
			AssertEquals("info maxlen", line.Detail.BD_PackTypeInfo.MaxLength, line.CargoTypeInfo.MaxLength);
		}

		public void TestNumberOfPackages()
		{
			line.Detail.BD_NoOfPacks = 1;
			AssertEquals("proxied get", 1, line.NumberOfPackages);

			line.NumberOfPackages = 2;
			AssertEquals("proxied set", 2, line.Detail.BD_NoOfPacks);

			AssertNotNull("info", line.NumberOfPackagesInfo);
		}

		#endregion

		#region Overrides

		#region Properties

		public void TestBO_RL_NKDischargePort()
		{
			port.BA_RL_NKArrivalPort = "AUSYD";
			AssertEquals("defaults to discharge port when first set", "AUSYD", line.BO_RL_NKDestinationPort);
			port.BA_RL_NKArrivalPort = "AUMEL";
			AssertEquals("doesn't change after having been set", "AUSYD", line.BO_RL_NKDestinationPort);
			line.BO_RL_NKDestinationPort = ZString.Empty;
			port.BA_RL_NKArrivalPort = "AUADL";
			AssertEquals("doesn't change after having been set", ZString.Empty, line.BO_RL_NKDestinationPort);
		}

		public void TestDetails()
		{
			CusSeaManOBLHeaderCargoLine header = Factory.New<CusSeaManOBLHeaderCargoLine>();
			AssertEquals(typeof(CusSeaManOBLDetailCargoLineCollection), header.Details.GetType());
		}

		public void TestBO_HeaderCargoType()
		{
			AssertEquals("by default", CMRCargoTypes.Codes.FullContainerLoad, line.CargoType);
			line.CargoType = CMRCargoTypes.Codes.Bulk;
			line.BO_HeaderCargoType = CMRCargoCodes.Codes.Export;
			AssertEquals("when set to bulk then export", CMRCargoTypes.Codes.Bulk, line.CargoType);
			line.BO_HeaderCargoType = CMRCargoCodes.Codes.Empty;
			AssertEquals("when set to empty", CMRCargoTypes.Codes.FullContainerLoad, line.CargoType);
		}

		#endregion

		public void TestDefaultValues()
		{
			AssertEquals("Header type set to E by default", CMRCargoCodes.Codes.Empty, line.BO_HeaderCargoType);
		}

		public void TestLookups()
		{
			AssertEquals(typeof(CusSeaManOBLHeaderCargoLineLookups), line.Lookups.GetType());
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusSeaManOBLHeaderCargoLineValidation), line.Validation.GetType());
		}

		public void TestHumanReadableName()
		{
			line.BO_HeaderCargoType = ZString.Empty;
			AssertEquals("precondition", ZString.Empty, line.BO_HeaderCargoType);
			AssertEquals("Cargo List", line.HumanReadableName);

			line.BO_HeaderCargoType = CMRImportCargoCodes.Codes.Cabotage;
			AssertEquals("Cargo List", line.HumanReadableName);

			line.CargoIdentifier = "foo";
			AssertEquals("Cargo List foo", line.HumanReadableName);

			line.CargoIdentifier = "bar";
			AssertEquals("Cargo List bar", line.HumanReadableName);
		}

		public void TestICMRMessageRespondee_Details()
		{
			AssertEquals(@"Vessel: 
Voyage: 
Cargo List: 
", ((ICMRMessageRespondee)line).Details);

			transportHeader.BT_VesselName = "ADMIRALENGRACHT";
			transportHeader.BT_VoyageNum = "4365";
			line.CargoIdentifier = "foo";

			AssertEquals(@"Vessel: ADMIRALENGRACHT
Voyage: 4365
Cargo List: foo
", ((ICMRMessageRespondee)line).Details);
		}

		public void TestICMRMessageRespondee_ShortDescription()
		{
			AssertEquals("Voyage:  Cargo List: ", ((ICMRMessageRespondee)line).ShortDescription);

			transportHeader.BT_VesselName = "ADMIRALENGRACHT";
			transportHeader.BT_VoyageNum = "4365";
			line.CargoIdentifier = "foo";

			AssertEquals("Voyage: 4365 Cargo List: foo", ((ICMRMessageRespondee)line).ShortDescription);
		}

		#endregion

		#region Properties

		public void TestPort()
		{
			AssertEquals(port, line.Port);
		}

		public void TestDetail()
		{
			AssertNotNull("when new", line.Detail);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			CusSeaManOBLHeaderCargoLine line2 = factory2.Load<CusSeaManOBLHeaderCargoLine>(line.PK);
			AssertEquals("when loaded", line.Detail.PK, line2.Detail.PK);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(CusSeaManOBLHeaderCargoLine));
		}

		protected override void SetUp()
		{
			base.SetUp();

			transportHeader = Factory.New<CusSeaManTranHead>();
			port = transportHeader.Arrivals.AddNew();
			line = port.CargoLines.AddNew();
		}

		CusSeaManTranHead transportHeader;
		CusSeaManArrivalPort port;
		CusSeaManOBLHeaderCargoLine line;

		#endregion
	}
}
