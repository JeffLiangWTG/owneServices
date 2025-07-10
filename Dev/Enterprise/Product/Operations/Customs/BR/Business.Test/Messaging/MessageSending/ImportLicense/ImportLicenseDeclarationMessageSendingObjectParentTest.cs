using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(ImportLicenseMessageSendingObjectParent))]
	sealed class ImportLicenseDeclarationMessageSendingObjectParentTest : DeclarationMessageSendingObjectParentTest
	{
		public void TestMessageSendingObjectProperties()
		{
			var parent = (ImportLicenseMessageSendingObjectParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;

			CombineAssertions(() =>
			{
				AssertEquals("Properties count", 6, properties.Count());

				AssertEquals("MessageType", properties.ElementAt(0).PropertyName);
				AssertEquals("SubmittedDate", properties.ElementAt(1).PropertyName);
				AssertEquals("CustomsStatus", properties.ElementAt(2).PropertyName);
				AssertEquals("MessageStatusDescription", properties.ElementAt(3).PropertyName);
				AssertEquals("EntryInstructionDescription", properties.ElementAt(4).PropertyName);
				AssertEquals("LocalReferenceNumber", properties.ElementAt(5).PropertyName);

				AssertEquals("Msg. Type", 100, properties.ElementAt(0).ColumnWidth);
				AssertEquals("Sub. Date", 140, properties.ElementAt(1).ColumnWidth);
				AssertEquals("Status", 100, properties.ElementAt(2).ColumnWidth);
				AssertEquals("Message Status Description", 180, properties.ElementAt(3).ColumnWidth);
				AssertEquals("Entry Instruction Description", 180, properties.ElementAt(4).ColumnWidth);
				AssertEquals("Local Reference Number", 180, properties.ElementAt(5).ColumnWidth);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			return new ImportLicenseMessageSendingObjectParent(declaration);
		}

		protected override bool ShowCreateInterchangeOnSendingMessage => true;

		protected override Type DeclarationMessageSendingObjectType => typeof(ImportLicenseMessageSendingObject);
	}
}
