using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class CusGoodsLocationValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidateInnerGoodsLocation_EdgeCases()
		{
			AssertExceptionThrown<ArgumentNullException>("Exception expected", () => CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(null));

			var goodsLocationProviderMock = new Mock<ICusGoodsLocationProvider>();
			goodsLocationProviderMock.Setup(x => x.GoodsLocation).Returns(value: null);

			AssertNoExceptionThrown(() => CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(goodsLocationProviderMock.Object));
		}

		public void TestValidateInnerGoodsLocation_WithMultipleErrorLevels()
		{
			var dummyBizOForTest = Factory.New<DummyBizOForTesting>();

			const string notificationText = "There are errors within the 'Location of Goods', please click on 'More..' to view the error information.";

			dummyBizOForTest.Validation.ValidateZ0_Description();
			AssertHasErrorContaining(dummyBizOForTest.GoodsLocationDescriptionInfo, notificationText);
		}

		#region DummyBizOForTest

		sealed class DummyBizOForTesting : DummyBusinessObject, ICusGoodsLocationProvider
		{
			public DummyBizOForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public CusGoodsLocation GoodsLocation => goodsLocation ??= Factory.New<CusGoodsLocation>();
			CusGoodsLocation goodsLocation;

			public ZString GoodsLocationDescription => Z0_Description;

			public ZPropertyInfo GoodsLocationDescriptionInfo => Z0_DescriptionInfo;

			public ZString ProviderKey => "Key";

			public void ValidateGoodsLocationDescription()
			{
			}

			protected override DummyBizoValidation GetNewValidation()
				=> new DummyBizOValidationForTest(this);
		}

		sealed class DummyBizOValidationForTest : DummyBizoValidation
		{
			public DummyBizOValidationForTest(DummyBusinessObject parent) : base(parent)
			{
			}

			protected override void CheckZ0_Description()
			{
				base.CheckZ0_Description();

				CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(Parent as DummyBizOForTesting);
			}
		}

		#endregion
	}
}
