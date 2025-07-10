using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class LayoutSupportingDocumentsFieldsControlTest : TestCaseWithFactory
{
	public void TestGetLayoutUcc6()
	{
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		using (var control = new LayoutSupportingDocumentsFieldsControlForTest(declaration))
		{
			AssertType<Ucc6SupportingDocumentFieldsLayout>("Layout", control.GetLayoutExposed());
		}
	}

	public void TestGetLayoutNonUcc6()
	{
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		using (var control = new LayoutSupportingDocumentsFieldsControlForTest(declaration))
		{
			AssertType<NonUcc6SupportingDocumentFieldsLayout>("Layout", control.GetLayoutExposed());
		}
	}

	class LayoutSupportingDocumentsFieldsControlForTest : LayoutSupportingDocumentsFieldsControl
	{
		public LayoutSupportingDocumentsFieldsControlForTest(JobDeclaration declaration) : base(declaration)
		{
		}

		public IPanelLayoutProvider GetLayoutExposed() => base.GetLayout();
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}

	JobDeclaration declaration;
}
