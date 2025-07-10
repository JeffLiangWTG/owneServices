using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using Moq;
using NUnit.Framework;
using IXmlCustomsEndorsement = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.ICustomsEndorsement;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class XmlCustomsEndorsementWrapperTest : TestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When customsEndorsement is null", () => new XmlCustomsEndorsementWrapper(customsEndorsement: null));
	}

	public void TestForm()
	{
		xmlCustomsEndorsementMock.Setup(x => x.Form).Returns("FRM");
		var xmlCustomsEndorsementWrapper = GetNewXmlCustomsEndorsementWrapper();
		AssertEquals("Form", "FRM", xmlCustomsEndorsementWrapper.Form);
	}

	public void TestFormNo()
	{
		xmlCustomsEndorsementMock.Setup(x => x.FormNo).Returns("FRMNO");
		var xmlCustomsEndorsementWrapper = GetNewXmlCustomsEndorsementWrapper();
		AssertEquals("FormNo", "FRMNO", xmlCustomsEndorsementWrapper.FormNo);
	}

	public void TestDate()
	{
		xmlCustomsEndorsementMock.Setup(x => x.IssuingDate).Returns(new DateTime(2022, 01, 01));
		var xmlCustomsEndorsementWrapper = GetNewXmlCustomsEndorsementWrapper();
		AssertEquals("Date", new ZDate(2022, 01, 01), xmlCustomsEndorsementWrapper.Date);
	}

	public void TestReferenceDate()
	{
		xmlCustomsEndorsementMock.Setup(x => x.ReferenceDate).Returns(new DateTime(2022, 01, 01));
		var xmlCustomsEndorsementWrapper = GetNewXmlCustomsEndorsementWrapper();
		AssertEquals("ReferenceDate", new ZDate(2022, 01, 01), xmlCustomsEndorsementWrapper.ReferenceDate);
	}

	public void TestCustomsOffice()
	{
		xmlCustomsEndorsementMock.Setup(x => x.CustomsOffice).Returns("CUSOFF");
		var xmlCustomsEndorsementWrapper = GetNewXmlCustomsEndorsementWrapper();
		AssertEquals("CustomsOffice", "CUSOFF", xmlCustomsEndorsementWrapper.CustomsOffice);
	}

	public void TestIssuingCountry()
	{
		xmlCustomsEndorsementMock.Setup(x => x.IssuingCountry).Returns("ISSCOU");
		var xmlCustomsEndorsementWrapper = GetNewXmlCustomsEndorsementWrapper();
		AssertEquals("IssuingCountry", "ISSCOU", xmlCustomsEndorsementWrapper.IssuingCountry);
	}

	public void TestPlace()
	{
		xmlCustomsEndorsementMock.Setup(x => x.ReferencePlace).Returns("PLC");
		var xmlCustomsEndorsementWrapper = GetNewXmlCustomsEndorsementWrapper();
		AssertEquals("Place", "PLC", xmlCustomsEndorsementWrapper.Place);
	}

	public void TestEntryNumber()
	{
		var xmlCustomsEndorsementWrapper = GetNewXmlCustomsEndorsementWrapper();
		AssertEquals("EntryNumber", "", xmlCustomsEndorsementWrapper.EntryNumber);
	}

	public void TestShowEntryNumber()
	{
		var xmlCustomsEndorsementWrapper = GetNewXmlCustomsEndorsementWrapper();
		AssertEquals("ShowEntryNumber", false, xmlCustomsEndorsementWrapper.ShowEntryNumber);
	}

	public void TestEUR1Pg1Box11TextEntryNumber()
	{
		var xmlCustomsEndorsementWrapper = GetNewXmlCustomsEndorsementWrapper();
		AssertEquals("EUR1Pg1Box11TextEntryNumber", "", xmlCustomsEndorsementWrapper.EUR1Pg1Box11TextEntryNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		xmlCustomsEndorsementMock = new Mock<IXmlCustomsEndorsement>();
	}

	Mock<IXmlCustomsEndorsement> xmlCustomsEndorsementMock;

	ICustomsEndorsement GetNewXmlCustomsEndorsementWrapper() => new XmlCustomsEndorsementWrapper(xmlCustomsEndorsementMock.Object);
}
