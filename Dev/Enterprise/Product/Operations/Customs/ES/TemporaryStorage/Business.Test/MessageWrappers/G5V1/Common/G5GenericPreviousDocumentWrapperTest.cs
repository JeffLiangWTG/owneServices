using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5GenericPreviousDocumentWrapperTest : WrapperHelperTest<G5GenericPreviousDocumentWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<NullReferenceException>("Null Document", () => new G5GenericPreviousDocumentWrapper(null));
		}

		public void TestGoodsItemId()
		{
			CombineAssertions(() =>
			{
				doc.CSI_LineNo = 5;
				AssertEquals("Expected filled GoodsItemId", "5", wrapper.GoodsItemId);

				doc.CSI_LineNo = 0;
				AssertEquals("Expected empty GoodsItemId when 0", ZString.Empty, wrapper.GoodsItemId);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			doc = Factory.New<TemporaryStoragePreviousDocument>();
			wrapper = new G5GenericPreviousDocumentWrapper(doc);
		}

		TemporaryStoragePreviousDocument doc;
		G5GenericPreviousDocumentWrapper wrapper;

		protected override G5GenericPreviousDocumentWrapper GetProvider() => wrapper;
	}
}
