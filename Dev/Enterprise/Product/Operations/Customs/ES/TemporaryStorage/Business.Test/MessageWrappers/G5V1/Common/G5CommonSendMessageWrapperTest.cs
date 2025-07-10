using System.Linq;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5CommonSendMessageWrapperTest : WrapperHelperTest<G5CommonSendMessageWrapper>
	{
		public void TestHeader()
		{
			var header = wrapper.Header;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Header", header);
				AssertSame("Cached Header", wrapper.Header, header);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 0 Lines", 0, wrapper.Lines.Count);

				var packedItem1 = bill.PackedItems.AddNew();
				var packedItem2 = bill.PackedItems.AddNew();
				var packedItem3 = bill.PackedItems.AddNew();
				packedItem1.IsMissing = true;

				wrapper = new G5CommonSendMessageWrapper(header, Certificate);
				var lines = wrapper.Lines;
				AssertEquals("Expected 2 Lines", 2, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TemporaryStorageHeader>();
			bill = header.Bills.FirstOrDefault() ?? header.Bills.AddNew();
			wrapper = new G5CommonSendMessageWrapper(header, Certificate);
		}

		TemporaryStorageHeader header;
		TemporaryStorageBill bill;
		G5CommonSendMessageWrapper wrapper;

		protected override G5CommonSendMessageWrapper GetProvider() => wrapper;
	}
}
