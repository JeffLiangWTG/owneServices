using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageMessageSendingObject))]
	public class TemporaryStorageMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageSendingDefaultValues()
		{
			Assert(sendingObject.ShouldSend);
		}

		public void TestEntryStatus_ReadOnly()
		{
			AssertEquals("Entry status should be readonly", true, sendingObject.EntryStatusInfo.ReadOnly);
		}

		public void TestMessageStatus_ReadOnly()
		{
			AssertEquals("Message status should be readonly", true, sendingObject.MessageStatusInfo.ReadOnly);
		}

		public void TestEntryStatus_Caption()
		{
			AssertCaptions(sendingObject.EntryStatusInfo, "Entry Status", ZString.Empty, ZString.Empty);
		}

		public void TestMessageStatus_Caption()
		{
			AssertCaptions(sendingObject.MessageStatusInfo, "Message Status", ZString.Empty, ZString.Empty);
		}

		void AssertCaptions(ZPropertyInfo propertyInfo, string expectedCaption, string expectedMediumCaption, string expectedShortCaption)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", expectedCaption, resourceStringData.Caption);
				AssertEquals("MediumCaption", expectedMediumCaption, resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", expectedShortCaption, resourceStringData.ShortCaption);
			});
		}

		public void TestAlternativeDateOfAcceptance_ReadOnly()
		{
			AssertEquals("Alternative date of acceptance should not be readonly", false, sendingObject.AlternativeDateOfAcceptanceInfo.ReadOnly);
		}

		public void TestAlternativeDateOfAcceptance_Caption()
		{
			AssertEquals("Alternative date of acceptance caption", "Alternative Date of Acceptance", sendingObject.AlternativeDateOfAcceptanceInfo.HumanReadableName);
		}

		public void TestCustomsReference_ReadOnly()
		{
			AssertEquals("Customs Reference should not be readonly", false, sendingObject.CustomsReferenceInfo.ReadOnly);
		}

		public void TestCustomsReference_MaxLength()
		{
			AssertEquals("Customs Reference max length", 512, sendingObject.CustomsReferenceInfo.MaxLength);
		}

		public void TestCustomsReference_Caption()
		{
			AssertEquals("Customs Reference caption", "Customs Reference", sendingObject.CustomsReferenceInfo.HumanReadableName);
		}

		public void TestCustomsJustification_ReadOnly()
		{
			AssertEquals("Customs Justification should not be readonly", false, sendingObject.CustomsJustificationInfo.ReadOnly);
		}

		public void TestCustomsJustification_MaxLength()
		{
			AssertEquals("Customs Justification max length", 2048, sendingObject.CustomsJustificationInfo.MaxLength);
		}

		public void TestCustomsJustification_Caption()
		{
			AssertEquals("Customs Justification caption", "Customs Justification", sendingObject.CustomsJustificationInfo.HumanReadableName);
		}

		public void TestFactory()
		{
			AssertNotNull("Sending Object Factory", sendingObject.Factory);
			AssertSame("Sending Object Factory should be the Header factory", header.Factory, sendingObject.Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TemporaryStorageMessageSendingObject(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<TemporaryStorageHeader>();
			sendingObject = new TemporaryStorageMessageSendingObject(header);
		}

		TemporaryStorageHeader header;
		TemporaryStorageMessageSendingObject sendingObject;
	}
}
