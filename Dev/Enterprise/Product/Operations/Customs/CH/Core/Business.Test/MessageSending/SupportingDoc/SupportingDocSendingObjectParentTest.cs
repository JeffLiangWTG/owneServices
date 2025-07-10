using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(SupportingDocSendingObjectParent))]
public class SupportingDocSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestSupportingDocSendingObjectParent()
	{
		AssertNotNull(messageSendingObjectParent.SelectedSendingObjects);
	}

	public void TestCanSendMessage()
	{
		AssertEquals("CanSendMessage always empty", ZString.Empty, messageSendingObjectParent.CanSendMessage());
	}

	protected override BusinessObject GetNewBusinessObject() => messageSendingObjectParent;

	protected override void SetUp()
	{
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		messageSendingObjectParent = new SupportingDocSendingObjectParent(declaration);
	}
	JobDeclaration declaration;
	SupportingDocSendingObjectParent messageSendingObjectParent;
}
