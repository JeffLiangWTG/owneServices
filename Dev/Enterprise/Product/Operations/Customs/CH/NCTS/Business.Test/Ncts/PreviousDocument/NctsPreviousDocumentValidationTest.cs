using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsPreviousDocument))]
sealed class NctsPreviousDocumentValidationTest : TestCaseWithFactory
{
	public void TestSubTypeNotMandatory()
	{
		const string messageError = "Please enter a Class.";
		PreviousDocument.Validation.ValidateCSI_SubType();
		AssertEquals("No Mandatory Notification at CSI_SubTypeInfo", false, PreviousDocument.CSI_SubTypeInfo.Notifications.Any(n => n.Message.Contains(messageError)));
	}

	NctsPreviousDocument PreviousDocument => previousDocument ?? (previousDocument = Factory.New<NctsPreviousDocument>());
	NctsPreviousDocument previousDocument;
}
