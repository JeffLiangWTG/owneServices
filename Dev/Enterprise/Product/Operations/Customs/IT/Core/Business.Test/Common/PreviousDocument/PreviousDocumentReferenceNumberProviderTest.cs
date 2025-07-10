using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Declaration.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PreviousDocumentReferenceNumberProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when previousDocument parameter is null", () => new PreviousDocumentReferenceNumberProvider(null));
	}

	public void TestIsCIM()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_Procedure = "CIM";
			AssertEquals($@"If {nameof(referenceNumberProvider.Procedure)}=""{referenceNumberProvider.Procedure}"" -> {nameof(referenceNumberProvider.IsCIM)}:", true, referenceNumberProvider.IsCIM);
			previousDocument.CSI_Procedure = "";
			AssertEquals($@"If {nameof(referenceNumberProvider.Procedure)}=""{referenceNumberProvider.Procedure}"" -> {nameof(referenceNumberProvider.IsCIM)}:", false, referenceNumberProvider.IsCIM);
			previousDocument.CSI_Procedure = "A3";
			AssertEquals($@"If {nameof(referenceNumberProvider.Procedure)}=""{referenceNumberProvider.Procedure}"" -> {nameof(referenceNumberProvider.IsCIM)}:", false, referenceNumberProvider.IsCIM);
		});
	}

	public void TestIsManualA3()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_Procedure = "A3";
			previousDocument.CSI_Status = "";
			AssertEquals("When Procedure is A3 but Status is Empty, IsManualA3 = false", false, referenceNumberProvider.IsManualA3);
			previousDocument.CSI_Status = "M";
			AssertEquals("When Procedure is A3 and Status is M, IsManualA3 = true", true, referenceNumberProvider.IsManualA3);
			previousDocument.CSI_Procedure = "CIM";
			AssertEquals("When Procedure is different from A3, IsManualA3 = false", false, referenceNumberProvider.IsManualA3);
		});
	}

	public void TestReferenceNumberWithoutCin()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberWithoutCin)}:", ZString.Empty, referenceNumberProvider.ReferenceNumberWithoutCin);

			previousDocument.CSI_ReferenceNumber = "1";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberWithoutCin)}:", "1", referenceNumberProvider.ReferenceNumberWithoutCin);

			previousDocument.CSI_ReferenceNumber = "12345X";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberWithoutCin)}:", "12345", referenceNumberProvider.ReferenceNumberWithoutCin);
		});
	}

	public void TestReferenceNumberWithoutCin_ForManualA3()
	{
		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_Status = "M";
		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumber = "1";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberWithoutCin)}:", "1", referenceNumberProvider.ReferenceNumberWithoutCin);
			previousDocument.CSI_ReferenceNumber = "12345";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberWithoutCin)}:", "12345", referenceNumberProvider.ReferenceNumberWithoutCin);
		});
	}

	public void TestReferenceNumberWithoutCin_ForCIM()
	{
		previousDocument.CSI_Procedure = "CIM";
		previousDocument.CSI_Status = "M";
		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumber = "";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberWithoutCin)}:", "", referenceNumberProvider.ReferenceNumberWithoutCin);
			previousDocument.CSI_ReferenceNumber = "1";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberWithoutCin)}:", "1", referenceNumberProvider.ReferenceNumberWithoutCin);
			previousDocument.CSI_ReferenceNumber = "12345";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberWithoutCin)}:", "12345", referenceNumberProvider.ReferenceNumberWithoutCin);
		});
	}
	public void TestReferenceNumberCin_ForManualA3()
	{
		previousDocument.CSI_Procedure = "A3";
		previousDocument.CSI_Status = "M";
		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumber = "1";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberCin)}:", "", referenceNumberProvider.ReferenceNumberCin);
			previousDocument.CSI_ReferenceNumber = "12345";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberCin)}:", "", referenceNumberProvider.ReferenceNumberCin);
		});
	}

	public void TestReferenceNumberCin_ForCIM()
	{
		previousDocument.CSI_Procedure = "CIM";
		previousDocument.CSI_Status = "M";
		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumber = "";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberCin)}:", "", referenceNumberProvider.ReferenceNumberCin);
			previousDocument.CSI_ReferenceNumber = "1";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberCin)}:", "", referenceNumberProvider.ReferenceNumberCin);
			previousDocument.CSI_ReferenceNumber = "12345";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberCin)}:", "", referenceNumberProvider.ReferenceNumberCin);
		});
	}

	public void TestReferenceNumberCin()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberCin)}:", ZString.Empty, referenceNumberProvider.ReferenceNumberCin);

			previousDocument.CSI_ReferenceNumber = "1";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberCin)}:", ZString.Empty, referenceNumberProvider.ReferenceNumberCin);

			previousDocument.CSI_ReferenceNumber = "12345X";
			AssertEquals($@"If {nameof(referenceNumberProvider.ReferenceNumber)}=""{referenceNumberProvider.ReferenceNumber}"" -> {nameof(referenceNumberProvider.ReferenceNumberCin)}:", "X", referenceNumberProvider.ReferenceNumberCin);
		});
	}

	public void TestDocumentType()
	{
		CombineAssertions(() =>
		{
			previousDocument.CSI_Code = ZString.Empty;
			AssertEquals("Code when Empty", ZString.Empty, referenceNumberProvider.DocumentType);

			previousDocument.CSI_Code = "N337";
			AssertEquals("Code when N337", "N337", referenceNumberProvider.DocumentType);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		previousDocument = Factory.New<PreviousDocumentForTest>();
		Factory.New<JobDeclaration>().Invoices.AddNew().PreviousDocuments.Add(previousDocument);
		referenceNumberProvider = new PreviousDocumentReferenceNumberProvider(previousDocument);
	}
	EU.Business.Declaration.MultiLineAddInfos.PreviousDocument previousDocument;
	PreviousDocumentReferenceNumberProvider referenceNumberProvider;
}
