using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LPOUSCommonGoodsItemWrapperTest : WrapperHelperTest<T2LPOUSCommonGoodsItemWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if entryLine is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryLine"), () => GetWrapper(null));

				var entryLine = Factory.New<CusEntryLine>();
				AssertExceptionThrown("Constructor Throws Exception if InvoiceLines is null", typeof(ArgumentOutOfRangeException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value '0' cannot be less than or equal to 0.","InvoiceLines"), () => GetWrapper(entryLine));
			});
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("Expected filled GoodsItemNumber", 3, wrapper.GoodsItemNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_LineNumber = 3;

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_CL = entryLine.PK;

			wrapper = GetWrapper(entryLine);
		}
		CusEntryLine entryLine;
		T2LPOUSCommonGoodsItemWrapper wrapper;

		T2LPOUSCommonGoodsItemWrapper GetWrapper(CusEntryLine entryLine) => new T2LPOUSCommonGoodsItemWrapper(entryLine);

		protected override T2LPOUSCommonGoodsItemWrapper GetProvider() => wrapper;
	}
}
