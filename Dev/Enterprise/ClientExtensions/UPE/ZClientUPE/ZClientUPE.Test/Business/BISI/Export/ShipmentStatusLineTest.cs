using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class ShipmentStatusLineTest : TestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be initialised in the constructor", ShipmentStatusData, Line.ShipmentStatusData);
		}

		public void TestLineAsString()
		{
			string expected = "GG19028    HK2000002005-01-01ADDBSRE012005-01-03X1234567890  BRKThis is the remarks for this exception                                LLJJ" + new string(' ', 130);
			AssertEquals(expected, Line.LineAsString);
		}

		public void TestLineAsString_ImportReleasedDateFilledWithEmptyStringWhenZDateTimeIsEmpty()
		{
			ShipmentStatusData.ImportReleaseDate = ZDateTime.Empty;
			string expected = "GG19028    HK2000002005-01-01ADDBSRE01          X1234567890  BRKThis is the remarks for this exception                                LLJJ" + new string(' ', 130);
			AssertEquals(expected, Line.LineAsString);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			InitialPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "HKHKG";
		}

		protected override void TearDown()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = InitialPort;
			base.TearDown();
		}

		ShipmentStatusLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = new ShipmentStatusLine(ShipmentStatusData);
				}

				return fLine;
			}
		}

		ShipmentStatusDataForTest ShipmentStatusData
		{
			get
			{
				if (fShipmentStatusData == null)
				{
					fShipmentStatusData = new ShipmentStatusDataForTest();
					PopulateShipmentStatusDataWithTestValues();
				}

				return fShipmentStatusData;
			}
		}

		void PopulateShipmentStatusDataWithTestValues()
		{
			ShipmentStatusData.ShipmentRef = "GG19028";
			ShipmentStatusData.ImportDate = new ZDateTime(2005, 1, 1);
			ShipmentStatusData.ShipmentStatus = "BS";
			ShipmentStatusData.HoldReasonCode = "REST";
			ShipmentStatusData.InspectIndicator = false;
			ShipmentStatusData.AddressCorrectionIndicator = true;
			ShipmentStatusData.ImportReleaseDate = new ZDateTime(2005, 1, 3);
			ShipmentStatusData.CustomsRefNo = "X1234567890";
			ShipmentStatusData.BrokerCode = "BRK19238";
			ShipmentStatusData.Remarks = "This is the remarks for this exception";
			ShipmentStatusData.ExceptionStatusCode = "LL";
			ShipmentStatusData.ExceptionResolutionCode = "JJ";
		}

		ShipmentStatusLine fLine;
		ShipmentStatusDataForTest fShipmentStatusData;
		string InitialPort;
		#endregion
	}
}
