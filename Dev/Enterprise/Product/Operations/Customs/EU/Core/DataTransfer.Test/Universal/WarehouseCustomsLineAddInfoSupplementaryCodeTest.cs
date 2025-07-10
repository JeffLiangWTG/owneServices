using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	class WarehouseCustomsLineAddInfoSupplementaryCodeTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => _ = new WarehouseCustomsLineAddInfoSupplementaryCode(null));
		}

		public void TestType()
		{
			AssertEquals("SUP", addInfoSupplementaryCode.Type);
		}

		public void TestAddInfoData()
		{
			AssertEquals("Code=1013*Order=3", addInfoSupplementaryCode.AddInfoData);
		}

		public void TestNAddInfoData()
		{
			AssertEquals("", addInfoSupplementaryCode.NAddInfoData);
		}

		protected override void SetUp()
		{
			base.SetUp();

			addInfoSupplementaryCode = new WarehouseCustomsLineAddInfoSupplementaryCode(new CustomsReference
			{
				Type = new CodeDescriptionPair
				{
					Code = "SUP"
				},
				SubType = new CodeDescriptionPair35Char
				{
					Code = "1013"
				},
				Order = 3
			});
		}

		WarehouseCustomsLineAddInfoSupplementaryCode addInfoSupplementaryCode;
	}
}
