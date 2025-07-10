using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Testing
{
	sealed class NonSupportedDeliveryMethodMessageBuilderTest : TestCase
	{
		public void TestGetMessage()
		{
			AssertEquals(string.Empty, NonSupportedDeliveryMethodMessageBuilder.GetMessage(null, null));
			AssertEquals(string.Empty, NonSupportedDeliveryMethodMessageBuilder.GetMessage(System.Array.Empty<IDocument>(), null));
			AssertEquals(string.Empty, NonSupportedDeliveryMethodMessageBuilder.GetMessage(new IDocument[] { null, null }, null));

			var document1 = new MockDocument("1", nameof(PrintCopyType.EML));
			var document2 = new MockDocument("2", nameof(PrintCopyType.EML));
			var document3 = new MockDocument("3", nameof(PrintCopyType.PRN));
			var document4 = new MockDocument("4", nameof(PrintCopyType.FAX));
			var document5 = new MockDocument("5", nameof(PrintCopyType.ALL));

			var documents = new[] { document1, document2, document3, document4, document5 };

			AssertEquals("GetMessage() - Email", @"'3' cannot be delivered because it has been restricted for 'Print' and 'ePrint' delivery only.
'4' cannot be delivered because it has been restricted for 'Fax' delivery only.", NonSupportedDeliveryMethodMessageBuilder.GetMessage(documents, Core.Constants.ContactNotifyModes.Email));

			AssertEquals("GetMessage() - Print", @"'1' cannot be delivered because it has been restricted for 'E-Mail' and 'ePrint' delivery only.
'2' cannot be delivered because it has been restricted for 'E-Mail' and 'ePrint' delivery only.
'4' cannot be delivered because it has been restricted for 'Fax' delivery only.", NonSupportedDeliveryMethodMessageBuilder.GetMessage(documents, Core.Constants.ContactNotifyModes.Print));

			AssertEquals("GetMessage() - EPrint", @"'4' cannot be delivered because it has been restricted for 'Fax' delivery only.", NonSupportedDeliveryMethodMessageBuilder.GetMessage(documents, Core.Constants.ContactNotifyModes.EPrint));

			AssertEquals("GetMessage() - Fax", @"'1' cannot be delivered because it has been restricted for 'E-Mail' and 'ePrint' delivery only.
'2' cannot be delivered because it has been restricted for 'E-Mail' and 'ePrint' delivery only.
'3' cannot be delivered because it has been restricted for 'Print' and 'ePrint' delivery only.", NonSupportedDeliveryMethodMessageBuilder.GetMessage(documents, Core.Constants.ContactNotifyModes.Fax));

			AssertEquals("GetMessage() - null", string.Empty, NonSupportedDeliveryMethodMessageBuilder.GetMessage(documents, null));
		}
	}
}
