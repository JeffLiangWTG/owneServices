using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class EmptyCustomsEndorsementWrapperTest : TestCase
{
	public void TestForm()
	{
		var xmlCustomsEndorsementWrapper = (ICustomsEndorsement)new EmptyCustomsEndorsementWrapper();
		AssertEquals("Form", "", xmlCustomsEndorsementWrapper.Form);
	}

	public void TestFormNo()
	{
		var xmlCustomsEndorsementWrapper = (ICustomsEndorsement)new EmptyCustomsEndorsementWrapper();
		AssertEquals("FormNo", "", xmlCustomsEndorsementWrapper.FormNo);
	}

	public void TestDate()
	{
		var xmlCustomsEndorsementWrapper = (ICustomsEndorsement)new EmptyCustomsEndorsementWrapper();
		AssertEquals("Date", ZDate.Empty, xmlCustomsEndorsementWrapper.Date);
	}

	public void TestReferenceDate()
	{
		var xmlCustomsEndorsementWrapper = (ICustomsEndorsement)new EmptyCustomsEndorsementWrapper();
		AssertEquals("RefereneDate", ZDate.Empty, xmlCustomsEndorsementWrapper.ReferenceDate);
	}

	public void TestCustomsOffice()
	{
		var xmlCustomsEndorsementWrapper = (ICustomsEndorsement)new EmptyCustomsEndorsementWrapper();
		AssertEquals("CustomsOffice", "", xmlCustomsEndorsementWrapper.CustomsOffice);
	}

	public void TestIssuingCountry()
	{
		var xmlCustomsEndorsementWrapper = (ICustomsEndorsement)new EmptyCustomsEndorsementWrapper();
		AssertEquals("IssuingCountry", "", xmlCustomsEndorsementWrapper.IssuingCountry);
	}

	public void TestPlace()
	{
		var xmlCustomsEndorsementWrapper = (ICustomsEndorsement)new EmptyCustomsEndorsementWrapper();
		AssertEquals("Place", "", xmlCustomsEndorsementWrapper.Place);
	}

	public void TestEntryNumber()
	{
		var xmlCustomsEndorsementWrapper = (ICustomsEndorsement)new EmptyCustomsEndorsementWrapper();
		AssertEquals("EntryNumber", "", xmlCustomsEndorsementWrapper.EntryNumber);
	}

	public void TestShowEntryNumber()
	{
		var xmlCustomsEndorsementWrapper = (ICustomsEndorsement)new EmptyCustomsEndorsementWrapper();
		AssertEquals("ShowEntryNumber", false, xmlCustomsEndorsementWrapper.ShowEntryNumber);
	}

	public void TestEUR1Pg1Box11TextEntryNumber()
	{
		var xmlCustomsEndorsementWrapper = (ICustomsEndorsement)new EmptyCustomsEndorsementWrapper();
		AssertEquals("EUR1Pg1Box11TextEntryNumber", "", xmlCustomsEndorsementWrapper.EUR1Pg1Box11TextEntryNumber);
	}
}
