using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageMessageSendingObject))]
	class TemporaryStorageMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageSendingDefaultValues()
		{
			Assert(sendingObject.ShouldSend);
		}

		public void TestTemporaryStorageMessageSendingObjectValidation()
		{
			AssertType<TemporaryStorageMessageSendingObjectValidation>(sendingObject.Validation);
		}

		public void TestDeclarationType_Readonly()
		{
			AssertEquals("Declaration Type should be readonly", true, sendingObject.DeclarationTypeInfo.ReadOnly);
		}

		public void TestDeclarationType_Caption()
		{
			AssertCaptions(sendingObject.DeclarationTypeInfo, "Declaration Type", ZString.Empty, ZString.Empty);
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

		public void TestAlternativeDateOfAcceptance()
		{
			AssertEquals("Alternative Date of Acceptance", ZDateTime.Empty, sendingObject.AlternativeDateOfAcceptance);
		}

		public void TestCustomsReference()
		{
			AssertEquals("Customs Reference", ZString.Empty, sendingObject.CustomsReference);
		}

		public void TestCustomsJustification()
		{
			AssertEquals("Customs Justification", ZString.Empty, sendingObject.CustomsJustification);
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
