using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	[TestedType(typeof(ExitControlMessageSendingObjectParent))]
	class ExitControlMessageSendingObjectParentTest : EU.ExitControl.Business.Testing.ExitControlMessageSendingObjectParentTest
	{
		public new void TestMessageSendingObjectProperties()
		{
			var properties = exitControlMessageSendingObjectParent.MessageSendingObjectProperties.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Expecting 4 columns for ExitControlMessageSendingObject", 6, properties.Length);
				AssertMessageSendingObjectProperty(properties[0], expectedColumnName: nameof(ExitControlMessageSendingObject.Type), expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(properties[1], expectedColumnName: nameof(ExitControlMessageSendingObject.DateTime), expectedIsMandatory: true, expectedWidth: 200);
				AssertMessageSendingObjectProperty(properties[2], expectedColumnName: nameof(ExitControlMessageSendingObject.ExitOffice), expectedIsMandatory: true, expectedWidth: 200);
				AssertMessageSendingObjectProperty(properties[3], expectedColumnName: nameof(ExitControlMessageSendingObject.MRN), expectedIsMandatory: true, expectedWidth: 200);
				AssertMessageSendingObjectProperty(properties[4], expectedColumnName: nameof(ExitControlMessageSendingObject.CustomsStatus), expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(properties[5], expectedColumnName: nameof(ExitControlMessageSendingObject.MessageStatus), expectedIsMandatory: true, expectedWidth: 80);
			});

			void AssertMessageSendingObjectProperty(MessageSendingObjectProperty property, string expectedColumnName, bool expectedIsMandatory, int expectedWidth)
			{
				AssertEquals("PropertyName", expectedColumnName, property.PropertyName);
				AssertEquals("IsMandatory", expectedIsMandatory, property.IsMandatory);
				AssertEquals("ColumnWidth", expectedWidth, property.ColumnWidth);
			}
		}

		public new void TestSendingObjectsCollectionType()
		{
			AssertType<ExitControlMessageSendingObjectCollection>(exitControlMessageSendingObjectParent.SendingObjectsCollection);
		}

		protected override EU.ExitControl.Business.ExitControlMessageSendingObjectParent GetNewExitControlMessageSendingObjectParent(EU.ExitControl.Business.CusExitHeader exitHeader)
			=> new ExitControlMessageSendingObjectParent((CusExitHeader)exitHeader);

		protected override EU.ExitControl.Business.CusExitHeader GetNewCusExitHeader() => Factory.New<CusExitHeader>();

		protected override BusinessObject GetNewBusinessObject() => exitControlMessageSendingObjectParent;

		protected override void SetUp()
		{
			base.SetUp();
			var cusExitHeader = Factory.New<CusExitHeader>();
			exitControlMessageSendingObjectParent = new ExitControlMessageSendingObjectParent(cusExitHeader);
		}
		ExitControlMessageSendingObjectParent exitControlMessageSendingObjectParent;
	}
}
