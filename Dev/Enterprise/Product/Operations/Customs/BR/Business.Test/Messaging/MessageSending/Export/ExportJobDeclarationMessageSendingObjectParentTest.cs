using System;
using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ExportJobDeclarationMessageSendingObjectParent))]
	sealed class ExportJobDeclarationMessageSendingObjectParentTest : DeclarationMessageSendingObjectParentTest
	{
		public void TestMessageSendingObjectProperties()
		{
			var parent = (ExportJobDeclarationMessageSendingObjectParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;

			CombineAssertions(() =>
			{
				AssertEquals("Properties count", 5, properties.Count());

				AssertEquals("MessageType", properties.ElementAt(0).PropertyName);
				AssertEquals("SubmittedDate", properties.ElementAt(1).PropertyName);
				AssertEquals("CustomsStatus", properties.ElementAt(2).PropertyName);
				AssertEquals("VOCReason", properties.ElementAt(3).PropertyName);
				AssertEquals("MessageStatusDescription", properties.ElementAt(4).PropertyName);

				AssertEquals("Msg. Type", 100, properties.ElementAt(0).ColumnWidth);
				AssertEquals("Sub. Date", 140, properties.ElementAt(1).ColumnWidth);
				AssertEquals("Status", 100, properties.ElementAt(2).ColumnWidth);
				AssertEquals("Reason", 140, properties.ElementAt(3).ColumnWidth);
				AssertEquals("MessageStatusDescription", 180, properties.ElementAt(4).ColumnWidth);

				AssertEquals("Msg. Type", true, properties.ElementAt(0).IsMandatory);
				AssertEquals("Sub. Date", true, properties.ElementAt(1).IsMandatory);
				AssertEquals("Status", true, properties.ElementAt(2).IsMandatory);
				AssertEquals("Reason", true, properties.ElementAt(3).IsMandatory);
				AssertEquals("MessageStatusDescription", false, properties.ElementAt(4).IsMandatory);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new ExportJobDeclarationMessageSendingObjectParent(declaration);
		}

		protected override Type DeclarationMessageSendingObjectType => typeof(ExportDeclarationMessageSendingObject);
	}
}
