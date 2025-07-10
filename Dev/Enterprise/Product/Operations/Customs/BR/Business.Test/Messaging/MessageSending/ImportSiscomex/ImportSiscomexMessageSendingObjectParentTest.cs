using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportSiscomexMessageSendingObjectParent))]
	sealed class ImportSiscomexMessageSendingObjectParentTest : DeclarationMessageSendingObjectParentTest
	{
		public void TestMessageSendingObjectProperties()
		{
			var parent = (ImportSiscomexMessageSendingObjectParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;

			CombineAssertions(() =>
			{
				AssertEquals("Properties count", 4, properties.Count());

				AssertEquals("MessageType", properties.ElementAt(0).PropertyName);
				AssertEquals("SubmittedDate", properties.ElementAt(1).PropertyName);
				AssertEquals("CustomsStatus", properties.ElementAt(2).PropertyName);
				AssertEquals("MessageStatusDescription", properties.ElementAt(3).PropertyName);

				AssertEquals("Msg. Type", 100, properties.ElementAt(0).ColumnWidth);
				AssertEquals("Sub. Date", 140, properties.ElementAt(1).ColumnWidth);
				AssertEquals("Status", 100, properties.ElementAt(2).ColumnWidth);
				AssertEquals("MessageStatusDescription", 180, properties.ElementAt(3).ColumnWidth);

				AssertEquals("Msg. Type", true, properties.ElementAt(0).IsMandatory);
				AssertEquals("Sub. Date", true, properties.ElementAt(1).IsMandatory);
				AssertEquals("Status", true, properties.ElementAt(2).IsMandatory);
				AssertEquals("MessageStatusDescription", false, properties.ElementAt(3).IsMandatory);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			return new ImportSiscomexMessageSendingObjectParent(declaration);
		}

		protected override bool ShowCreateInterchangeOnSendingMessage => true;

		protected override Type DeclarationMessageSendingObjectType => typeof(ImportSiscomexMessageSendingObject);
	}
}
