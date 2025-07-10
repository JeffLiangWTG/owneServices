using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADPreviousDocumentWrapperTest : TestCaseWithFactory
{
	public void TestPreviousDocument()
	{
		previousDocumentMock.Setup(m => m.Register).Returns("A3T");
		previousDocumentMock.Setup(m => m.Type).Returns("Z");
		previousDocumentMock.Setup(m => m.Category).Returns("ZZZ");
		previousDocumentMock.Setup(m => m.ReferenceNumber).Returns("123456");
		previousDocumentMock.Setup(m => m.ReferenceNumberCin).Returns("D");
		previousDocumentMock.Setup(m => m.Date).Returns(new ZDate(2019, 01, 01));
		previousDocumentMock.Setup(m => m.Series).Returns("A");
		previousDocumentMock.Setup(m => m.CustomsOffice).Returns("IT654321");
		previousDocumentMock.Setup(m => m.ItemNumber).Returns(1);
		previousDocumentMock.Setup(m => m.Mrn).Returns("REF2");
		CombineAssertions(() =>
		{
			AssertEquals(nameof(wrapper.DocType), "Z", wrapper.DocType);
			AssertEquals(nameof(wrapper.Category), "ZZZ", wrapper.Category);
			AssertEquals(nameof(wrapper.Mrn), "", wrapper.Mrn);
			AssertEquals(nameof(wrapper.ComplementOfInformation), "", wrapper.ComplementOfInformation);
			AssertEquals(nameof(wrapper.Register), "A3T", wrapper.Register);
			AssertEquals(nameof(wrapper.ReferenceNumber), "123456", wrapper.ReferenceNumber);
			AssertEquals(nameof(wrapper.ReferenceCIN), "D", wrapper.ReferenceCIN);
			AssertEquals(nameof(wrapper.Date), new ZDate(2019, 01, 01), wrapper.Date);
			AssertEquals(nameof(wrapper.Series), "A", wrapper.Series);
			AssertEquals(nameof(wrapper.CustomsOffice), "654321  ", wrapper.CustomsOffice);
			AssertEquals(nameof(wrapper.ItemNumber), 1, wrapper.ItemNumber);
		});
	}

	public void TestRegister()
	{
		previousDocumentMock.Setup(m => m.Register).Returns(ZString.Empty);
		AssertEquals(nameof(wrapper.Register), ZString.Empty, wrapper.Register);

		previousDocumentMock.Setup(m => m.Register).Returns("A3");
		AssertEquals(nameof(wrapper.Register), "A3", wrapper.Register);
	}

	public void TestReferenceNumber()
	{
		previousDocumentMock.Setup(m => m.Register).Returns("MRN");
		previousDocumentMock.Setup(m => m.ReferenceNumber).Returns("REF1");
		AssertEquals($"{nameof(wrapper.ReferenceNumber)} when register is MRN", ZString.Empty, wrapper.ReferenceNumber);

		previousDocumentMock.Setup(m => m.Register).Returns("XXXX");
		AssertEquals($"{nameof(wrapper.ReferenceNumber)} when register is not MRN", "REF1", wrapper.ReferenceNumber);
	}

	public void TestReferenceNumberCin()
	{
		previousDocumentMock.Setup(m => m.Register).Returns("MRN");
		previousDocumentMock.Setup(m => m.ReferenceNumberCin).Returns("A");
		AssertEquals($"{nameof(wrapper.ReferenceCIN)} when register is MRN", ZString.Empty, wrapper.ReferenceCIN);

		previousDocumentMock.Setup(m => m.Register).Returns("XXXX");
		previousDocumentMock.Setup(m => m.ReferenceNumberCin).Returns("A");
		AssertEquals($"{nameof(wrapper.ReferenceCIN)} when register is not MRN", "A", wrapper.ReferenceCIN);
	}

	public void TestSeries()
	{
		previousDocumentMock.Setup(m => m.Series).Returns(ZString.Empty);
		AssertEquals(nameof(wrapper.Series), ZString.Empty, wrapper.Series);

		previousDocumentMock.Setup(m => m.Series).Returns("12");
		AssertEquals(nameof(wrapper.Series), "12", wrapper.Series);
	}

	public void TestCustomsOffice()
	{
		previousDocumentMock.Setup(m => m.CustomsOffice).Returns(ZString.Empty);
		AssertEquals(nameof(wrapper.CustomsOffice), ZString.Empty, wrapper.CustomsOffice);

		previousDocumentMock.Setup(m => m.CustomsOffice).Returns("IT123456");
		AssertEquals(nameof(wrapper.CustomsOffice), "123456  ", wrapper.CustomsOffice);
	}

	public void TestItemNumber()
	{
		previousDocumentMock.Setup(m => m.ItemNumber).Returns(1);
		AssertEquals($"{nameof(wrapper.ItemNumber)} when Item number is not 0", 1, wrapper.ItemNumber);

		previousDocumentMock.Setup(m => m.ItemNumber).Returns(0);
		AssertNull($"{nameof(wrapper.ItemNumber)} when Item number is 0", wrapper.ItemNumber);
	}

	public void TestMrn()
	{
		previousDocumentMock.Setup(m => m.Register).Returns("MRN");
		previousDocumentMock.Setup(m => m.Mrn).Returns("THISISMRN");
		AssertEquals($"{nameof(wrapper.Mrn)} when register is MRN", "THISISMRN", wrapper.Mrn);

		previousDocumentMock.Setup(m => m.Register).Returns("ZXX");
		previousDocumentMock.Setup(m => m.Mrn).Returns("THISISMRN");
		AssertEquals($"{nameof(wrapper.Mrn)} when register is not MRN", ZString.Empty, wrapper.Mrn);
	}

	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => { new SADPreviousDocumentWrapper(null); });
		AssertNoExceptionThrown(() => { new SADPreviousDocumentWrapper(previousDocumentMock.Object); });
	}

	protected override void SetUp()
	{
		base.SetUp();
		previousDocumentMock = new Mock<IMergedPreviousDocument>();
		wrapper = new SADPreviousDocumentWrapper(previousDocumentMock.Object);
	}

	Mock<IMergedPreviousDocument> previousDocumentMock;
	SADPreviousDocumentWrapper wrapper;
}
