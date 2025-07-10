using System;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business.Testing;
using Moq;
using CoreConstants = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class NT035ResponsePrettyFormatterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var responseDetailMock = new Mock<INT035ResponseDetail>();
		AssertExceptionThrown<ArgumentNullException>("Factory null", () => new NT035ResponsePrettyFormatter(null, responseDetailMock.Object));
		AssertExceptionThrown<ArgumentNullException>("ResponseDetail null", () => new NT035ResponsePrettyFormatter(Factory, null));
	}

	public void TestGetFormattedText()
	{
		RefCusCodeTestHelper.CreateCustomsOfficesList(Factory, CoreConstants.RefCusCodeListTypes.Codes.CustomsOffice);
		Factory.Save();

		var recoveryNotificationMock = new Mock<INT035ResponseDetail>();
		recoveryNotificationMock.Setup(e => e.MRN).Returns("21CH16360164625756");
		recoveryNotificationMock.Setup(e => e.MRNVersion).Returns("1");
		recoveryNotificationMock.Setup(e => e.DeclarationAcceptanceDate).Returns(new DateTime(2021, 11, 18));
		recoveryNotificationMock.Setup(e => e.RecoveryNotificationDate).Returns(new DateTime(2021, 11, 19));
		recoveryNotificationMock.Setup(e => e.CustomsOfficeOfDepartureReferenceNumber).Returns("CH001251");
		recoveryNotificationMock.Setup(e => e.CustomsOfficeOfRecoveryReferenceNumber).Returns("CH001252");
		recoveryNotificationMock.Setup(e => e.RecoveryNotificationText).Returns("recovery notification text");

		var formatter = new NT035ResponsePrettyFormatter(Factory, recoveryNotificationMock.Object);
		var html = formatter.GetFormattedText();
		AssertMultilineASCIIEquals("formatted text", GetExpectedFormattedText("21CH16360164625756", "1", "18.11.2021", "19.11.2021", "CH001251", "Allschwil 1", "CH001252", "Allschwil 2", "recovery notification text"), html);
	}

	string GetExpectedFormattedText(string mrn, string mrnVersion, string accDate, string recDate, string cusOfDepNo, string cusOfDepDesc, string cusOfRecNo, string cusOfRecDesc, string text)
	{
		return $"<h2>Recovery Notification</h2>" +
			$"<p>MRN: {mrn}.{mrnVersion}" +
			$"<p>Declaration Acceptance Date: {accDate}" +
			$"<p>Recovery Date: {recDate}" +
			$"<p>Customs Office of Departure: {cusOfDepNo} - {cusOfDepDesc}" +
			$"<p>Customs Office of Recovery: {cusOfRecNo} - {cusOfRecDesc}" +
			$"<p>{text}";
	}
}
