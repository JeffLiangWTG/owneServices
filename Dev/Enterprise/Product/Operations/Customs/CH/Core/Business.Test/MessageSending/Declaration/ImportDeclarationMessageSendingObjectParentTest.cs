using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ImportDeclarationMessageSendingObjectParent))]
sealed class ImportDeclarationMessageSendingObjectParentTest : DeclarationMessageSendingObjectParentTest
{
	public override void TestCanSendMessage_CheckMessageSendingEnvironment() => CombineAssertions(() =>
	{
		const string noRegistrationNoMessage = "Customs Registration Number is not configured for the current company. Please contact your system administrator.";

		var messageSendingObjectParent = (ImportDeclarationMessageSendingObjectParent)GetNewBusinessObject();
		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = ZString.Empty;
		AssertEquals("No customs registration number", noRegistrationNoMessage, messageSendingObjectParent.CanSendMessage());

		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456";
		AssertEquals("Has customs registration number", ZString.Empty, messageSendingObjectParent.CanSendMessage());
	});

	protected override void SetMessageSendingEnvironment()
	{
		GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "123456";
	}

	public void TestGetSendingObjectsCollectionCore()
	{
		var parent = (ImportDeclarationMessageSendingObjectParent)GetNewBusinessObject();

		CombineAssertions(() =>
		{
			AssertEquals(0, parent.SendingObjectsCollection.Count);
			Declaration.CustomsEntryHeaders.AddNew();
			Declaration.CustomsEntryHeaders.AddNew();
			parent = (ImportDeclarationMessageSendingObjectParent)GetNewBusinessObject();
			AssertEquals(2, parent.SendingObjectsCollection.Count);
		});
	}

	public void TestMessageSendingObjectProperties()
	{
		var parent = (ImportDeclarationMessageSendingObjectParent)GetNewBusinessObject();
		var properties = parent.MessageSendingObjectProperties;

		CombineAssertions(() =>
		{
			AssertEquals("Properties count", 7, properties.Count());

			AssertMessageObjectProperty(0, nameof(DeclarationMessageSendingObject.MessageType), 160, true);
			AssertMessageObjectProperty(1, nameof(DeclarationMessageSendingObject.VOCReason), 160, true);
			AssertMessageObjectProperty(2, nameof(DeclarationMessageSendingObject.DeclarationType), 100, true);
			AssertMessageObjectProperty(3, nameof(DeclarationMessageSendingObject.SubStyle), 100, true);
			AssertMessageObjectProperty(4, nameof(DeclarationMessageSendingObject.Description), 200, false);
			AssertMessageObjectProperty(5, nameof(DeclarationMessageSendingObject.LocalReferenceNumber), 180, true);
			AssertMessageObjectProperty(6, nameof(DeclarationMessageSendingObject.EntryStatus), 100, true);

			void AssertMessageObjectProperty(int index, string propertyName, int width, bool isMandatory)
			{
				AssertEquals($"{propertyName}.PropertyName", propertyName, properties.ElementAt(index).PropertyName);
				AssertEquals($"{propertyName}.ColumnWidth", width, properties.ElementAt(index).ColumnWidth);
				AssertEquals($"{propertyName}.IsMandatory", isMandatory, properties.ElementAt(index).IsMandatory);
			}
		});
	}

	public void TestParentDeclaration()
	{
		var objectParent = (ImportDeclarationMessageSendingObjectParent)GetNewBusinessObject();
		AssertType<JobDeclaration>(objectParent.ParentDeclaration);
	}

	protected override string MessageType => JobMessageTypeList.Codes.Import;

	protected override BusinessObject GetNewBusinessObject() => new ImportDeclarationMessageSendingObjectParent(Declaration);
}
