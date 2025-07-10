using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class RefContainerCodesCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new RefContainerCodesCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			CreateNewContainerType("20SEA", "SEA", 1);
			CreateNewContainerType("20SXX", "SEA", 0);
			CreateNewContainerType("20ROA", "ROA", 1);
			CreateNewContainerType("20RXX", "ROA", 0);
			CreateNewContainerType("20AIR", "AIR", 0);
			factory.Save();

			var containerTypeList = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			AssertEquals(true, containerTypeList.ContainsCode("20SEA"));
			AssertEquals(true, containerTypeList.ContainsCode("20ROA"));
			AssertEquals(false, containerTypeList.ContainsCode("20SXX"));
			AssertEquals(false, containerTypeList.ContainsCode("20RXX"));
			AssertEquals(false, containerTypeList.ContainsCode("20AIR"));
		}

		void CreateNewContainerType(string code, string shippingMode, int teu)
		{
			var containerType = factory.New<RefContainer>();
			containerType.RC_Code = code;
			containerType.RC_Description = code + " Description";
			containerType.RC_ShippingMode = shippingMode;
			containerType.RC_TEU = teu;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			factory = new BusinessObjectFactory();
		}

		BusinessObjectFactory factory;

		#endregion
	}
}
