using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC045C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC045CProviderTest : TestCaseWithFactory
	{
		public void TestEmptyValue()
		{
			var emptyProvider = new CC045CProvider(new Cc045CType());
			CombineAssertions(() =>
			{
				AssertEquals("MRN", ZString.Empty, emptyProvider.MRN);
				AssertEquals("WriteOffDate", ZDateTime.Empty, emptyProvider.WriteOffDate);
			});
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "19AA12345678901230", provider.MRN);
		}

		public void TestWriteOffDate()
		{
			AssertEquals("WriteOffDate", new ZDate(2023, 2, 15), provider.WriteOffDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC045CProvider(new Cc045CType
			{
				TransitOperation = new TransitOperationType16
				{
					Mrn = "19AA12345678901230",
					WriteOffDate = new DateTime(2023, 2, 15, 22, 35, 16)
				}
			});
		}
		CC045CProvider provider;
	}
}
