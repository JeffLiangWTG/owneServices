using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC028C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC028CProviderTest : TestCaseWithFactory
	{
		public void TestNullValue()
		{
			CombineAssertions(() =>
			{
				var emptyProvider = new CC028CProvider(new Cc028CType());
				AssertEquals("Null value should return empty", ZString.Empty, emptyProvider.MRN);
				AssertEquals("Null value should return empty", ZDateTime.Empty, emptyProvider.DeclarationAcceptanceDate);
			});
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "19AA12345678901230", provider.MRN);
		}

		public void TestDeclarationAcceptanceDate()
		{
			AssertEquals("DeclarationAcceptanceDate", new ZDate(2023, 1, 30), provider.DeclarationAcceptanceDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC028CProvider(new Cc028CType
			{
				TransitOperation = new TransitOperationType11
				{
					Mrn = "19AA12345678901230",
					DeclarationAcceptanceDate = new DateTime(2023, 1, 30, 15, 30, 0)
				}
			});
		}
		CC028CProvider provider;
	}
}
