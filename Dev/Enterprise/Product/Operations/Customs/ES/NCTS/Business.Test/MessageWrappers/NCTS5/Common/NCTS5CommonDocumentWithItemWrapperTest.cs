using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NCTS5CommonDocumentWithItemWrapperTest : WrapperHelperTest<NCTS5CommonDocumentWithItemWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if doc is null", typeof(NullReferenceException),
			"Object reference not set to an instance of an object.", () => new NCTS5CommonDocumentWithItemWrapper(null, 0));
		}

		public void TestGoodsItemNumber()
		{
			document.CSI_ItemNumber = ZShort.Zero;
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty GoodsItemNumber", ZString.Empty, wrapper.GoodsItemNumber);

				document.CSI_ItemNumber = 1;
				AssertEquals("Expected filled GoodsItemNumber", "1", wrapper.GoodsItemNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			document = Factory.New<CusSupportingInfo>();
			wrapper = new NCTS5CommonDocumentWithItemWrapper(document, 1);
		}

		CusSupportingInfo document;
		NCTS5CommonDocumentWithItemWrapper wrapper;

		protected override NCTS5CommonDocumentWithItemWrapper GetProvider() => wrapper;
	}
}
