using System;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ParameterNameFactoryTest : TransactionedTestCase
	{
		public void TestGetNextParameterNameReturnsIncrementingName()
		{
			var factory = new ParameterNameFactory();
			var parameter1 = factory.GetParameterName(ZSqlParameter.New("", String.Empty, DummyBizoSchema.Z0_Code));
			var parameter2 = factory.GetParameterName(ZSqlParameter.New("", String.Empty, DummyBizoSchema.Z0_Code));
			Assert("Parameters should be different, but were the same", parameter1 != parameter2);
		}

		public void TestGetNextParameterNameReturnsSameForSameParameter()
		{
			var factory = new ParameterNameFactory();
			var parameter = ZSqlParameter.New("@P1", string.Empty, DummyBizoSchema.Z0_Code);
			var firstName = factory.GetParameterName(parameter);
			var secondName = factory.GetParameterName(parameter);
			AssertEquals("Parameters should be same", firstName, secondName);
		}

		public void TestInvalidParameterName()
		{
			AssertExceptionThrown<ArgumentException>(() =>
			{
				var factory = new ParameterNameFactory("@@invalid");
			});
		}
	}
}
