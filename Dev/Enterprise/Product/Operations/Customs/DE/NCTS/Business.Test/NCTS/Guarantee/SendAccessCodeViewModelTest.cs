using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(SendAccessCodeViewModel))]
	sealed class SendAccessCodeViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGuaranteeNumber()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			guarantee.CPH_Number = "123";
			var viewModel = new SendAccessCodeViewModel(guarantee);

			CombineAssertions(() =>
			{
				AssertEquals("Value", "123", viewModel.GuaranteeNumber);
				AssertEquals("Read only", true, viewModel.GuaranteeNumberInfo.ReadOnly);
			});
		}

		public void TestOfficeOfGuarantee()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			var viewModel = new SendAccessCodeViewModel(guarantee);
			AssertEquals(8, viewModel.OfficeOfGuaranteeInfo.MaxLength);
		}

		public void TestNewMainAccessCode()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			var viewModel = new SendAccessCodeViewModel(guarantee);
			AssertEquals(4, viewModel.NewMainAccessCodeInfo.MaxLength);
		}

		public void TestSend()
		{
			var guarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var viewModel = new SendAccessCodeViewModel(guarantee);
			AssertEquals(true, viewModel.Send());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			return new SendAccessCodeViewModel(guarantee);
		}
	}
}
