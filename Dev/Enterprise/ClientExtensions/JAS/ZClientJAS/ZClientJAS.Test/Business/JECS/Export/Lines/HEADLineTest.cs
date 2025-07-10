using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC.Testing;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class HEADLineTest : MessageLineTestCase
	{
		public void TestLineAsString()
		{
			HeaderData.SetDestinationForwarder("OFF01", "NET01");
			HeaderData.SetSendingForwarder("OFF02", "NET02");
			HeaderData.FreightDest = "DEFRA";
			AssertEquals("HEAD3100;OFF01;OFF02;NET01;NET02;DEFRA", Line.LineAsString);
			HeaderData.SetDestinationForwarder("OFF11", "NET11");
			HeaderData.SetSendingForwarder("OFF12", "NET12");
			HeaderData.FreightDest = "IDJKT";
			AssertEquals("HEAD3100;OFF11;OFF12;NET11;NET12;IDJKT", Line.LineAsString);
		}

		public void TestLineAsString_NoForwarderOrgs()
		{
			HeaderData.FreightDest = "DEFRA";
			AssertEquals("HEAD3100;NONET;NONET;NONET;NONET;DEFRA", Line.LineAsString);
			HeaderData.SendingForwarder = Factory.New<JASOrgHeader>();
			AssertEquals("HEAD3100;NONET;;NONET;;DEFRA", Line.LineAsString);
			HeaderData.SendingForwarder = null;
			HeaderData.ReceivingForwarder = Factory.New<JASOrgHeader>();
			AssertEquals("HEAD3100;;NONET;;NONET;DEFRA", Line.LineAsString);
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return "HEAD";
			}
		}

		protected override MessageLine GetMessageLine()
		{
			return new HEADLine(HeaderData);
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return 5;
			}
		}

		#region Implementation
		JXCHeaderForTest HeaderData
		{
			get
			{
				if (fHeaderData == null)
				{
					fHeaderData = new JXCHeaderForTest();
				}

				return fHeaderData;
			}
		}

		JXCHeaderForTest fHeaderData;
		#endregion
	}
}
