using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class REFRLineTest : MessageLineTestCase
	{
		public void TestLineAsString()
		{
			AssertEquals("REFR3100;HB102;S", Line.LineAsString);
			ReferenceText = "NEWREFERENCE";
			ReferenceFrom = REFRLine.ReferenceFrom.Consignee;
			Line = GetMessageLine();
			AssertEquals("REFR3100;NEWREFERENCE;C", Line.LineAsString);
		}

		public void TestLineAsStringWhenFieldExceedMaxLength()
		{
			ReferenceText = "123456789012345678901234567890123456789012345678901234567890123456";
			Line = GetMessageLine();
			AssertEquals("REFR3100;123456789012345678901234567890123456789012345678901234567890;S", Line.LineAsString);
		}

		#region Implementation
		protected override int ExpectedFieldCount
		{
			get
			{
				return 2;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return "REFR";
			}
		}

		protected override MessageLine GetMessageLine()
		{
			return new REFRLine(ReferenceText, ReferenceFrom);
		}

		ZString ReferenceText = "HB102";
		REFRLine.ReferenceFrom ReferenceFrom;
		#endregion
	}
}
