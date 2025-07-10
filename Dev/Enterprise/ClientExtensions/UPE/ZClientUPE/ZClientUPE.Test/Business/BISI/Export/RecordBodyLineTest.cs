using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class RecordBodyLineTest : TestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", LineKey, Line.LineKey);
		}

		public void TestLineAsString()
		{
			AssertEquals("SHP10001   IDTESTLI2005-11-10ADD004400002004-05-0509:30:12  ", Line.LineAsString);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			InitialPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "IDJKT";
		}

		protected override void TearDown()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = InitialPort;
			base.TearDown();
		}

		string InitialPort;
		#region RecordBodyLineForTest
		RecordBodyLineForTest Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = new RecordBodyLineForTest(LineKey);
				}

				return fLine;
			}
		}

		LineKeyForTest LineKey
		{
			get
			{
				if (fLineKey == null)
				{
					fLineKey = new LineKeyForTest();
					fLineKey.ShipmentRef = "SHP10001";
					fLineKey.ImportDate = new ZDateTime(2005, 11, 10);
				}

				return fLineKey;
			}
		}

		RecordBodyLineForTest fLine;
		LineKeyForTest fLineKey;
		class RecordBodyLineForTest : RecordBodyLine
		{
			public RecordBodyLineForTest(ILineKey lineKey) : base(lineKey)
			{
			}

			protected override void AppendContentFields(ZStringBuilder lineBuilder)
			{
				AppendFixedLengthField(lineBuilder, 44m, 4, 8);
				AppendFixedLengthField(lineBuilder, new ZDateTime(2004, 5, 5), false, 10);
				AppendFixedLengthField(lineBuilder, new ZDateTime(2004, 5, 5, 9, 30, 12), true, 10);
			}

			protected override ZString LineType
			{
				get
				{
					return "TESTLINE";
				}
			}
		}
		#endregion
		#endregion
	}
}
