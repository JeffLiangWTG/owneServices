using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class ShipmentChargeLineTest : TestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be initialised in the constructor", ShipmentData, Line.ShipmentData);
			AssertEquals("Should be initialised in the constructor", ChargeData, Line.ChargeData);
		}

		public void TestLineAsString()
		{
			string expected = "SS102393   US5000002005-04-12ADD20600000001889890000AUD" + new string(' ', 245);
			AssertEquals(expected, Line.LineAsString);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			InitialPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "USHON";
		}

		protected override void TearDown()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = InitialPort;
			base.TearDown();
		}

		ShipmentChargeLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = new ShipmentChargeLine(ShipmentData, ChargeData);
				}

				return fLine;
			}
		}

		ShipmentChargeData ChargeData
		{
			get
			{
				if (fChargeData == null)
				{
					fChargeData = new ShipmentChargeData(ShipmentChargeTypeCode.GST, 1889.89m, Core.Constants.CurrencyCodes.Australia);
				}

				return fChargeData;
			}
		}

		ShipmentDataForTest ShipmentData
		{
			get
			{
				if (fShipmentData == null)
				{
					fShipmentData = new ShipmentDataForTest();
					fShipmentData.ShipmentRef = "SS102393";
					fShipmentData.ImportDate = new ZDateTime(2005, 4, 12);
				}

				return fShipmentData;
			}
		}

		ShipmentChargeLine fLine;
		ShipmentChargeData fChargeData;
		ShipmentDataForTest fShipmentData;
		string InitialPort;
		#endregion
	}
}
