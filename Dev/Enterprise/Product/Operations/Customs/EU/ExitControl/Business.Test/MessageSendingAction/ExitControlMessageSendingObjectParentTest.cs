using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(ExitControlMessageSendingObjectParent))]
	public class ExitControlMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageSendingObjectProperties()
		{
			var exitHeader = GetNewCusExitHeader();
			var parent = GetNewExitControlMessageSendingObjectParent(exitHeader);
			var properties = parent.MessageSendingObjectProperties.ToArray();

			CombineAssertions(() =>
			{
				AssertEquals("Expecting 6 columns for ExitControlMessageSendingObject", 6, properties.Length);
				AssertMessageSendingObjectProperty(properties[0], expectedColumnName: nameof(ExitControlMessageSendingObject.Type), expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(properties[1], expectedColumnName: nameof(ExitControlMessageSendingObject.TransportID), expectedIsMandatory: true, expectedWidth: 80);
				AssertMessageSendingObjectProperty(properties[2], expectedColumnName: nameof(ExitControlMessageSendingObject.Location), expectedIsMandatory: true, expectedWidth: 200);
				AssertMessageSendingObjectProperty(properties[3], expectedColumnName: nameof(ExitControlMessageSendingObject.DateTime), expectedIsMandatory: true, expectedWidth: 200);
				AssertMessageSendingObjectProperty(properties[4], expectedColumnName: nameof(ExitControlMessageSendingObject.MRN), expectedIsMandatory: true, expectedWidth: 200);
				AssertMessageSendingObjectProperty(properties[5], expectedColumnName: nameof(ExitControlMessageSendingObject.CustomsStatus), expectedIsMandatory: true, expectedWidth: 80);
			});
		}

		void AssertMessageSendingObjectProperty(MessageSendingObjectProperty property, string expectedColumnName, bool expectedIsMandatory, int expectedWidth)
		{
			AssertEquals("PropertyName", expectedColumnName, property.PropertyName);
			AssertEquals("IsMandatory", expectedIsMandatory, property.IsMandatory);
			AssertEquals("ColumnWidth", expectedWidth, property.ColumnWidth);
		}

		public void TestSendingObjectsCollectionType()
		{
			var exitHeader = GetNewCusExitHeader();
			var parent = GetNewExitControlMessageSendingObjectParent(exitHeader);
			AssertType<ExitControlMessageSendingObjectCollection>(parent.SendingObjectsCollection);
		}

		public void TestSendingObjectsCollection()
		{
			var exitHeader = GetNewCusExitHeader();
			var report1 = exitHeader.CusExitReports.AddNew();
			report1.CER_Type = ExitReportTypeList.Codes.Presentation;
			var report2 = exitHeader.CusExitReports.AddNew();
			report2.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;

			var parent = GetNewExitControlMessageSendingObjectParent(exitHeader);
			AssertContainsExactElementsInAnyOrder("All types", ExpectedSendingObjectsCollection, parent.SendingObjectsCollection.Cast<ExitControlMessageSendingObject>().Select(x => x.Type));
		}

		public void TestTopLevelBusinessObject()
		{
			var exitHeader = GetNewCusExitHeader();
			var parent = GetNewExitControlMessageSendingObjectParent(exitHeader);
			AssertEquals("TopLevelBusinessObject", exitHeader.PK, parent.TopLevelBusinessObject.PK);
		}

		public void TestMessageErrorCollector()
		{
			var exitHeader = GetNewCusExitHeader();
			exitHeader.CXH_OA_Carrier = ZGuid.Empty;
			var report1 = exitHeader.CusExitReports.AddNew();
			var report2 = exitHeader.CusExitReports.AddNew();
			report1.CER_Type = ExitReportTypeList.Codes.Presentation;
			report1.CER_OfficeOfExit = "EXT001";
			report2.CER_Type = ExitReportTypeList.Codes.InformationOnNonExitedExport;
			var parent = GetNewExitControlMessageSendingObjectParent(exitHeader);
			var sendingObject1 = parent.SendingObjectsCollection[0];
			var sendingObject2 = parent.SendingObjectsCollection[1];

			sendingObject1.ShouldSend = true;
			sendingObject2.ShouldSend = false;

			var t = parent.MessageSendingValidation.MessageErrors.GetUniqueMessageList();

			CombineAssertions(() =>
			{
				sendingObject1.ShouldSend = true;
				sendingObject2.ShouldSend = false;
				AssertCollectionContains("CusExitHeader Included (True/False)", "Carrier: You have not entered a Carrier.", parent.MessageSendingValidation.MessageErrors.GetUniqueMessageList());
				AssertCollectionContains("Error report selected", "Office of Exit: The code you have selected is not in the list.", parent.MessageSendingValidation.MessageErrors.GetUniqueMessageList());
				sendingObject1.ShouldSend = false;
				sendingObject2.ShouldSend = true;
				AssertCollectionContains("CusExitHeader Included (False/True)", "Carrier: You have not entered a Carrier.", parent.MessageSendingValidation.MessageErrors.GetUniqueMessageList());
				AssertCollectionNotContains("Error report not selected", "Office of Exit: The code you have selected is not in the list.", parent.MessageSendingValidation.MessageErrors.GetUniqueMessageList());
				sendingObject1.ShouldSend = true;
				sendingObject2.ShouldSend = true;
				AssertCollectionContains("CusExitHeader Included (True/True)", "Carrier: You have not entered a Carrier.", parent.MessageSendingValidation.MessageErrors.GetUniqueMessageList());
				AssertCollectionContains("Both reports selected", "Office of Exit: The code you have selected is not in the list.", parent.MessageSendingValidation.MessageErrors.GetUniqueMessageList());
				sendingObject1.ShouldSend = false;
				sendingObject2.ShouldSend = false;
				AssertCollectionNotContains("CusExitHeader Included (False/False)", "Carrier: You have not entered a Carrier.", parent.MessageSendingValidation.MessageErrors.GetUniqueMessageList());
				AssertCollectionNotContains("Both reports not selected", "Office of Exit: The code you have selected is not in the list.", parent.MessageSendingValidation.MessageErrors.GetUniqueMessageList());
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewExitControlMessageSendingObjectParent(GetNewCusExitHeader());

		protected virtual IReadOnlyList<ZString> ExpectedSendingObjectsCollection => new ZString[]
		{
			ExitReportTypeList.Codes.Presentation, ExitReportTypeList.Codes.InformationOnNonExitedExport
		};

		protected virtual ExitControlMessageSendingObjectParent GetNewExitControlMessageSendingObjectParent(CusExitHeader exitHeader) => new ExitControlMessageSendingObjectParent(exitHeader);

		protected virtual CusExitHeader GetNewCusExitHeader() => Factory.New<CusExitHeader>();
	}
}
