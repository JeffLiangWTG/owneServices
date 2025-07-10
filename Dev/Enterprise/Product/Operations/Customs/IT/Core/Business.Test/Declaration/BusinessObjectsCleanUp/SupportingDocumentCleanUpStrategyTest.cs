using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class SupportingDocumentCleanUpStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When supportingDocument is null", () => new SupportingDocumentCleanUpStrategy(supportingDocument: null));
		AssertExceptionThrown<ArgumentNullException>("When supportingDocument.Declaration is null", () => new SupportingDocumentCleanUpStrategy(Factory.New<SupportingDocument>()));
	}

	public void TestCleanUpCSI_Value()
	{
		supportingDocument.CSI_Value = 123m;
		declaration.JE_MessageType = "XXX";
		strategy.CleanUp();
		AssertEquals("CSI_Value", 0m, supportingDocument.CSI_Value);
	}

	public void TestCleanUpCSI_ReferenceNumber2()
	{
		supportingDocument.CSI_ReferenceNumber2 = "123";
		declaration.JE_MessageType = "XXX";
		strategy.CleanUp();
		AssertEquals("CSI_ReferenceNumber2", "", supportingDocument.CSI_ReferenceNumber2);
	}

	public void TestCleanUpCSI_RX_NKCurrency()
	{
		supportingDocument.CSI_RX_NKCurrency = "EUR";
		declaration.JE_MessageType = "XXX";
		strategy.CleanUp();
		AssertEquals("CSI_RX_NKCurrency", "", supportingDocument.CSI_RX_NKCurrency);
	}

	public void TestCleanUpCSI_DateOfExpiry()
	{
		supportingDocument.CSI_DateOfExpiry = new ZDateTime(2022, 01, 01);
		declaration.JE_MessageType = "XXX";
		strategy.CleanUp();
		AssertEquals("CSI_DateOfExpiry", ZDate.Empty, supportingDocument.CSI_DateOfExpiry);
	}

	public void TestCleanUpCSI_Status()
	{
		supportingDocument.CSI_Status = "AAA";

		declaration.JE_MessageType = "EXP";
		strategy.CleanUp();
		AssertEquals("CSI_Status", "AAA", supportingDocument.CSI_Status);

		declaration.JE_MessageType = "XXX";
		strategy.CleanUp();
		AssertEquals("CSI_Status", "", supportingDocument.CSI_Status);
	}

	public void TestCleanUpCSI_LineNo()
	{
		supportingDocument.CSI_LineNo = 3;

		declaration.JE_MessageType = "EXP";
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: true))
		{
			strategy.CleanUp();
			AssertEquals("CSI_LineNo", 3, supportingDocument.CSI_LineNo);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isUCC6: false))
		{
			strategy.CleanUp();
			AssertEquals("CSI_LineNo", 0, supportingDocument.CSI_LineNo);
		}
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isUCC6)
		=> EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		supportingDocument = declaration.SupportingDocuments.AddNew();
		strategy = new SupportingDocumentCleanUpStrategy(supportingDocument);
	}

	JobDeclaration declaration;
	SupportingDocument supportingDocument;
	ICleanUpStrategy strategy;
}
