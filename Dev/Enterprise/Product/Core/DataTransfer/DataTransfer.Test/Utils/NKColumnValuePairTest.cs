using System;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class NKColumnValuePairTest : TestCase
	{
		public void TestNew()
		{
			foreach (ForeignKeyType type in Enum.GetValues(typeof(ForeignKeyType)))
			{
				if (type != ForeignKeyType.None)
				{
					NKColumnValuePair pair = NKColumnValuePair.New("splaty", type);
					AssertNotNull(pair.BizType);
					AssertNotNull(pair.NKColumn);
					AssertNotNull(pair.Value);
				}
			}
		}

		public void TestContainerCodeNK()
		{
			NKColumnValuePair pair = NKColumnValuePair.New("20GP", ForeignKeyType.ContainerCodeNK);
			AssertEquals(RefContainerSchema.RC_Code, pair.NKColumn);
			AssertEquals("20GP", pair.Value);
			AssertEquals(typeof(RefContainer), pair.BizType);
		}

		public void TestPortMatchWithUNLOCOFirstNK()
		{
			NKColumnValuePair pair = NKColumnValuePair.New("AUSYD", ForeignKeyType.PortNK);
			AssertEquals(RefUNLOCOSchema.RL_PortName, pair.NKColumn);
			AssertEquals("AUSYD", pair.Value);
			AssertEquals(typeof(RefUNLOCO), pair.BizType);
		}

		public void TestMatchWithIntZoneNK()
		{
			NKColumnValuePair pair = NKColumnValuePair.New("BBBB", ForeignKeyType.IntZoneNK);
			AssertEquals(RefZoneHeaderSchema.FZ_Code, pair.NKColumn);
			AssertEquals("BBBB", pair.Value);
			AssertEquals(typeof(RefZoneHeader), pair.BizType);
		}
	}
}
