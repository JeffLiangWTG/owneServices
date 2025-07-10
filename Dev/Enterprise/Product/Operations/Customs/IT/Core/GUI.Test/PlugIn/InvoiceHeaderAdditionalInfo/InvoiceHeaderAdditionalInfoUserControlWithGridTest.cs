using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class InvoiceHeaderAdditionalInfoUserControlWithGridTest : TestCaseWithFactory
{
	public void TestCreateNewInvoiceHeaderAdditionalInformationDetailsLayout()
	{
		using (var control = new InvoiceHeaderAdditionalInfoUserControlWithGridForTest())
		{
			AssertType<InvoiceHeaderAdditionalInfoDetailsLayout>("Layout Type", control.CreateNewInvoiceHeaderAdditionalInformationDetailsLayoutExposed());
		}
	}

	class InvoiceHeaderAdditionalInfoUserControlWithGridForTest : InvoiceHeaderAdditionalInfoUserControlWithGrid
	{
		public IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayoutExposed() => base.CreateNewInvoiceHeaderAdditionalInformationDetailsLayout();
	}
}
