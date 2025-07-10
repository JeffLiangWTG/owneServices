using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class EMCSMessageSendingActionParentBaseOnlyTest : TestCaseWithFactory
	{
		public void TestTopLevelBusinessObject()
		{
			AssertSame("TopLevelBusinessObject should return an EMCSJobDeclaration.", declaration, sendingActionParent.TopLevelBusinessObject);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			AssertSame("SecurityCheckpointToSendWithMessageError", Env.Security.CustomsDeclarationSendWithMessageErrors, sendingActionParent.SecurityCheckpointToSendWithMessageError);
		}

		public void TestGetSendingObjectsCollectionCore()
		{
			AssertEquals("Should only ever create 1 SendingAction for EMCSJobDeclaration.", 1, sendingActionParent.SendingObjectsCollection.Count);
		}

		public void TestGetNewMessageErrorCollector()
		{
			declaration.EMCSPackages.AddNew();
			declaration.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Hours;
			declaration.JourneyTimeNumericPart = 25;

			CombineAssertions(() =>
			{
				var messageError = "Journey Time Cannot exceed 24 Hours";
				AssertHasMessageError("Precondition, has message error", declaration.JourneyTimeFormatPartInfo, messageError);
				var errors = sendingActionParent.BizObjValidationMessageErrors;
				AssertContains("Should contain message error", messageError, errors);
			});
		}

		public void TestColumns()
		{
			var columns = sendingActionParent.MessageSendingObjectProperties;
			AssertContainsExactElementsInExactOrder(new[] { BaseMessageSendingObject.SchemaShouldSend, nameof(EMCSMessageSendingAction.DeclarantType), nameof(EMCSMessageSendingAction.EADNumber), nameof(EMCSMessageSendingAction.RegistrationStatus) }, sendingActionParent.MessageSendingObjectProperties.Select(x => x.PropertyName));
		}

		public void TestShouldSend()
		{
			AssertMessageSendingObjectProperty(BaseMessageSendingObject.SchemaShouldSend, 80);
		}

		public void TestDeclarantType()
		{
			AssertMessageSendingObjectProperty(nameof(EMCSMessageSendingAction.DeclarantType), 100);
		}

		public void TestEADNumber()
		{
			AssertMessageSendingObjectProperty(nameof(EMCSMessageSendingAction.EADNumber), 80);
		}

		public void TestRegistrationStatus()
		{
			AssertMessageSendingObjectProperty(nameof(EMCSMessageSendingAction.RegistrationStatus), 100);
		}

		EMCSJobDeclaration declaration;
		EMCSMessageSendingActionParentForTesting sendingActionParent;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			sendingActionParent = new EMCSMessageSendingActionParentForTesting(declaration);
		}

		void AssertMessageSendingObjectProperty(string propertyName, int expectedWidth)
		{
			var columns = sendingActionParent.MessageSendingObjectProperties;
			var property = columns.Single(x => x.PropertyName == propertyName);
			CombineAssertions(() =>
			{
				AssertEquals("IsMandatory", true, property.IsMandatory);
				AssertEquals("ColumnWidth", expectedWidth, property.ColumnWidth);
			});
		}
	}
}
