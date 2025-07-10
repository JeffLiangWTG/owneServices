using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class SHMKLineTest : MessageLineTestCase
	{
		public void TestLineAsString()
		{
			AssertEquals("SHMK3100;ASDFASDFKJASDFKASDFJASDF;ASDFASDFKJASDFKASDFJASDF;20", Line.LineAsString);
			((SHMKLine)Line).Shipment.JS_MarksAndNumbers = "";
			AssertEquals("SHMK3100;N/A;N/A;20", Line.LineAsString);
		}

		public void TestLineAsString_FieldsExceedMaxLength()
		{
			((SHMKLine)Line).Shipment.JS_MarksAndNumbers = new string('X', 300) + "YYY";
			AssertEquals("SHMK3100;" + new string('X', 60) + ";" + new string('X', 250) + ";20", Line.LineAsString);
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return JXCConstants.SHMKFieldCount;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.SHMK;
			}
		}

		protected override MessageLine GetMessageLine()
		{
			JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
			shipment.JS_MarksAndNumbers = "ASDFASDFKJASDFKASDFJASDF";
			shipment.JS_OuterPacks = 20;
			return new SHMKLine(shipment);
		}
	}
}
