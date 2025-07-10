using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class CargoSelectivityResultTypeCodeListTest : NUnit.Framework.TestCase
	{
		public void TestToBeInspected()
		{
			var list = new CargoSelectivityResultTypeCodeList();
			AssertEquals("If more codes are added, please see the definition of ToBeInspected and update it accordingly", 3, list.Count);

			foreach (CodeDescriptionPair pair in list)
			{
				if (pair.Code != CargoSelectivityResultTypeCodeList.Codes.S)
				{
					Assert("To be inspected:" + pair.Code, CargoSelectivityResultTypeCodeList.ToBeInspected(pair.Code));
				}
				else
				{
					Assert("To be inspected:" + pair.Code, !CargoSelectivityResultTypeCodeList.ToBeInspected(pair.Code));
				}
			}
		}

		public void TestCodesInOrderOfImportance()
		{
			var list = new CargoSelectivityResultTypeCodeList();
			AssertEquals("If more codes are added, please see the definition of CodesInOrderOfImportance and update it accordingly", 3, list.Count);

			var orderedList = CargoSelectivityResultTypeCodeList.CodesInOrderOfImportance();
			AssertEquals(CargoSelectivityResultTypeCodeList.Codes.F, orderedList[0]);
			AssertEquals(CargoSelectivityResultTypeCodeList.Codes.Y, orderedList[1]);
			AssertEquals(CargoSelectivityResultTypeCodeList.Codes.S, orderedList[2]);
		}
	}
}
