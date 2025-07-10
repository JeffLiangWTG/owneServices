using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class ShipmentDetailsLineTest : TestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be initialised in the constructor", ShipmentData, Line.ShipmentData);
		}

		public void TestLineAsString()
		{
			string expected = "S0015678   GB1000002004-10-12ADDS0015678   IMPACC101 AB08190837222   00000000034780000000009821SGD0000709827001SSDBL123459809 2004-10-13O14098364537   1345678090982123" + new string(' ', 133);
			AssertEquals(expected, Line.LineAsString);
		}

		public void TestLineAsString_CustomsEntryDateFilledWithEmptyStringWhenZDateTimeIsEmpty()
		{
			ShipmentData.CustomsEntryDate = ZDateTime.Empty;
			string expected = "S0015678   GB1000002004-10-12ADDS0015678   IMPACC101 AB08190837222   00000000034780000000009821SGD0000709827001SSDBL123459809           O14098364537   1345678090982123" + new string(' ', 133);
			AssertEquals(expected, Line.LineAsString);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			InitialPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "GBLON";
		}

		protected override void TearDown()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = InitialPort;
			base.TearDown();
		}

		ShipmentDetailsLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = new ShipmentDetailsLine(ShipmentData);
				}

				return fLine;
			}
		}

		ShipmentDataForTest ShipmentData
		{
			get
			{
				if (fShipmentData == null)
				{
					fShipmentData = new ShipmentDataForTest();
					PopulateShipmentDataWithTestValues();
				}

				return fShipmentData;
			}
		}

		void PopulateShipmentDataWithTestValues()
		{
			ShipmentData.ShipmentRef = "S0015678";
			ShipmentData.ImportDate = new ZDateTime(2004, 10, 12);
			ShipmentData.ImporterAccountNumber = "IMPACC101";
			ShipmentData.DutyType = "AB";
			ShipmentData.MasterBillNumber = "08190837222";
			ShipmentData.CustomsValue = 34.78m;
			ShipmentData.StatisticalValue = 98.21m;
			ShipmentData.DVCCurrencyCode = "SGD";
			ShipmentData.CustomsExchangeRate = 0.709827001m;
			ShipmentData.BISICustomsEntryStatus = "SS";
			ShipmentData.EntryType = "DB";
			ShipmentData.CustomsEntryNumber = "L123459809";
			ShipmentData.CustomsEntryDate = new ZDateTime(2004, 10, 13);
			ShipmentData.CustomsOfficeNumber = "O14";
			ShipmentData.VATNumber = "098364537";
			ShipmentData.ImporterVATDefermentNumber = "13456780";
			ShipmentData.SplitDutyDefermentNumber = "90982123";
		}

		ShipmentDetailsLine fLine;
		ShipmentDataForTest fShipmentData;
		string InitialPort;
		#endregion
	}
}
