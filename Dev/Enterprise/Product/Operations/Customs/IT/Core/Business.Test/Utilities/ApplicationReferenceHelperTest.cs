using System;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ApplicationReferenceHelperTest : TestCase
{
	public void TestGetNewSeparatedArguments()
	{
		AssertEquals("When all params are provided, application reference", "845A:CRR:IT279100", ApplicationReferenceHelper.GetNew("845A", "CRR", "IT279100"));
		AssertEquals("When all params are NOT provided, application reference", "::", ApplicationReferenceHelper.GetNew(ZString.Empty, ZString.Empty, ZString.Empty));
	}

	public void TestGetNewICustomsEntryApplicationReference()
	{
		AssertExceptionThrown<ArgumentNullException>("customsEntryApplicationReference is required", () => ApplicationReferenceHelper.GetNew(null));

		var customsEntryApplicationReferenceMock = new Mock<ICustomsEntryApplicationReference>();
		customsEntryApplicationReferenceMock.Setup(m => m.CustomsOffice).Returns("IT279100");
		customsEntryApplicationReferenceMock.Setup(m => m.Node).Returns("845A");
		customsEntryApplicationReferenceMock.Setup(m => m.Subscriber).Returns("CRR");
		AssertEquals("Formatted ApplicationReference", "845A:CRR:IT279100", ApplicationReferenceHelper.GetNew("845A", "CRR", "IT279100"));
	}
}
