using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class PreviousDocumentCleanUpStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When previousDocument is null", () => new PreviousDocumentCleanUpStrategy(previousDocument: null));
		AssertExceptionThrown<ArgumentNullException>("When previousDocument.Declaration is null", () => new PreviousDocumentCleanUpStrategy(Factory.New<PreviousDocument>()));
	}

	public void TestCleanUpCSI_PackType()
	{
		previousDocument.CSI_PackType = "AA";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("CSI_PackType", "AA", previousDocument.CSI_PackType);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("CSI_PackType", "", previousDocument.CSI_PackType);
		}
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		previousDocument = declaration.PreviousDocuments.AddNew();
		strategy = new PreviousDocumentCleanUpStrategy(previousDocument);
	}

	JobDeclaration declaration;
	PreviousDocument previousDocument;
	ICleanUpStrategy strategy;
}
