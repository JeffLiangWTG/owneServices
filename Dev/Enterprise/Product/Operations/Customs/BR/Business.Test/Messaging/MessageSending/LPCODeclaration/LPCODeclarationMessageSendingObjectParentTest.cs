using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LPCODeclarationMessageSendingObjectParent))]
	class LPCODeclarationMessageSendingObjectParentTest : DeclarationMessageSendingObjectParentTest
	{
		public void TestMessageSendingObjectProperties()
		{
			var parent = (LPCODeclarationMessageSendingObjectParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;

			CombineAssertions(() =>
			{
				AssertEquals("Properties count", 11, properties.Count());

				AssertEquals("MessageType", properties.ElementAt(0).PropertyName);
				AssertEquals("SubmittedDate", properties.ElementAt(1).PropertyName);
				AssertEquals("CustomsStatus", properties.ElementAt(2).PropertyName);
				AssertEquals("Reason", properties.ElementAt(3).PropertyName);
				AssertEquals("Requirement", properties.ElementAt(4).PropertyName);
				AssertEquals("EntryNumber", properties.ElementAt(5).PropertyName);
				AssertEquals("EntryLineNumber", properties.ElementAt(6).PropertyName);
				AssertEquals("Version", properties.ElementAt(7).PropertyName);
				AssertEquals("MessageStatusDescription", properties.ElementAt(8).PropertyName);
				AssertEquals("NewEffectiveDate", properties.ElementAt(9).PropertyName);
				AssertEquals("Message", properties.ElementAt(10).PropertyName);

				AssertEquals("Msg. Type", 100, properties.ElementAt(0).ColumnWidth);
				AssertEquals("Sub. Date", 140, properties.ElementAt(1).ColumnWidth);
				AssertEquals("Status", 100, properties.ElementAt(2).ColumnWidth);
				AssertEquals("Reason", 140, properties.ElementAt(3).ColumnWidth);
				AssertEquals("Requirement", 140, properties.ElementAt(4).ColumnWidth);
				AssertEquals("Entry Number", 100, properties.ElementAt(5).ColumnWidth);
				AssertEquals("Entry Line Number", 100, properties.ElementAt(6).ColumnWidth);
				AssertEquals("Version", 80, properties.ElementAt(7).ColumnWidth);
				AssertEquals("MessageStatusDescription", 180, properties.ElementAt(8).ColumnWidth);
				AssertEquals("NewEffectiveDate", 120, properties.ElementAt(9).ColumnWidth);
				AssertEquals("Message", properties.ElementAt(10).PropertyName);

				AssertEquals("Msg. Type", true, properties.ElementAt(0).IsMandatory);
				AssertEquals("Sub. Date", true, properties.ElementAt(1).IsMandatory);
				AssertEquals("Status", true, properties.ElementAt(2).IsMandatory);
				AssertEquals("Reason", true, properties.ElementAt(3).IsMandatory);
				AssertEquals("Requirement", true, properties.ElementAt(4).IsMandatory);
				AssertEquals("Entry Number", true, properties.ElementAt(5).IsMandatory);
				AssertEquals("Entry Line Number", true, properties.ElementAt(6).IsMandatory);
				AssertEquals("Version", true, properties.ElementAt(7).IsMandatory);
				AssertEquals("MessageStatusDescription", false, properties.ElementAt(8).IsMandatory);
				AssertEquals("NewEffectiveDate", true, properties.ElementAt(9).IsMandatory);
				AssertEquals("Message", properties.ElementAt(10).PropertyName);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
			return new LPCODeclarationMessageSendingObjectParent(declaration);
		}

		protected override bool ShowCreateInterchangeOnSendingMessage => false;

		protected override Type DeclarationMessageSendingObjectType => typeof(LPCODeclarationMessageSendingObject);
	}
}
