using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class PKDataTest : TestCaseWithFactory
	{
		public void TestEquals()
		{
			var pkData1 = new PKData() { PK = pkGuid, Data = dataGuid };
			var pkData2 = new PKData() { PK = pkGuid, Data = dataGuid };
			Assert(pkData1.Equals(pkData2));
			var pkData3 = new PKData() { PK = pkGuid, Data = ZGuid.NewZGuid() };
			Assert(!pkData1.Equals(pkData3));
			var pkData4 = new PKData() { PK = ZGuid.NewZGuid(), Data = dataGuid };
			Assert(!pkData1.Equals(pkData4));
			var pkData5 = new PKData() { PK = ZGuid.NewZGuid(), Data = ZGuid.NewZGuid() };
			Assert(!pkData1.Equals(pkData5));
		}

		public void TestEqualsOperator()
		{
			var pkData1 = new PKData() { PK = pkGuid, Data = dataGuid };
			var pkData2 = new PKData() { PK = pkGuid, Data = dataGuid };
			Assert(pkData1 == pkData2);
		}

		public void TestNotEqualsOperator()
		{
			var pkData1 = new PKData() { PK = pkGuid, Data = dataGuid };
			var pkData2 = new PKData() { PK = pkGuid, Data = ZGuid.NewZGuid() };
			Assert(pkData1 != pkData2);
			var pkData3 = new PKData() { PK = ZGuid.NewZGuid(), Data = dataGuid };
			Assert(pkData1 != pkData3);
			var pkData4 = new PKData() { PK = ZGuid.NewZGuid(), Data = ZGuid.NewZGuid() };
			Assert(pkData1 != pkData4);
		}

		public void TestGetHashCode()
		{
			var pkData1 = new PKData() { PK = ZGuid.BrettsGuid, Data = ZDateTime.BrettsBirthday };
			// NOTE: The GetHashCode code functionality can differ based on the framework used
#if NET
			AssertEquals(876542450, pkData1.GetHashCode());
#else
			AssertEquals(620625158, pkData1.GetHashCode());
#endif
			var pkData2 = new PKData();
			AssertEquals(0, pkData2.GetHashCode());
			var pkData3 = new PKData() { PK = ZGuid.Empty, Data = dataGuid };
			AssertEquals(0, pkData3.GetHashCode());
			var pkData4 = new PKData() { PK = ZGuid.BrettsGuid };
			AssertEquals(0, pkData4.GetHashCode());
			var pkData5 = new PKData() { Data = dataGuid };
			AssertEquals(0, pkData5.GetHashCode());
		}

		protected override void SetUp()
		{
			base.SetUp();
			pkGuid = ZGuid.NewZGuid();
			dataGuid = ZGuid.NewZGuid();
		}
		ZGuid pkGuid;
		ZGuid dataGuid;
	}
}
