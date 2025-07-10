using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	class InboundMessageInterpreterBaseOnlyTest : TestCaseWithFactory
	{
		public void TestInboundMessageInterpreter()
		{
			var provider = new IM917Provider(new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM917.Im917()
			{
				XmlNegativeAcknowledgement = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.XmlNegativeAcknowledgement>()
			});
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_LinkedObject = entry;
			var testInterpreter = new InboundMessageInterpreterForBaseTesting(message, provider);
			AssertXMLEquals(
				"Interpration",
				@"Summary Text<br />
<br />AdvancedSummaries 1<br />AdvancedSummaries 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td colspan=""2"">Message Details Summary Text</td></tr><tr><td>MessageDetail1</td><td>MessageDetail1 Text</td></tr><tr><td>MessageDetail2</td><td>MessageDetail2 Text</td></tr><tr><td>SequencedMessageDetail1</td><td>SequencedMessageDetail1 Text</td></tr><tr><td>SequencedMessageDetail2</td><td>SequencedMessageDetail2 Text</td></tr></table><br />
<br />Summary:<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>AdditionalMessageDetail1</td><td>AdditionalMessageDetail1 Text</td></tr><tr><td>AdditionalMessageDetail2</td><td>AdditionalMessageDetail2 Text</td></tr></table><br />
<br />Summary Free Number of Columns:<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>FreeNumberOfColumns1 Title</td><td>FreeNumberOfColumns2 Title</td><td>FreeNumberOfColumns3 Title</td></tr><tr><td>FreeNumberOfColumns1 Text</td><td>FreeNumberOfColumns2 Text</td><td>FreeNumberOfColumns3 Text</td></tr></table><br />Description<br />
",
				testInterpreter.GetInterpretation()
			);
		}
	}

	class InboundMessageInterpreterForBaseTesting : InboundMessageInterpreter<IM917Provider>
	{
		public InboundMessageInterpreterForBaseTesting(EDIMessage message, IM917Provider provider) : base(message, provider) { }

		protected override string Summary => "Summary Text";

		protected override IEnumerable<string> GetAdvancedSummaries() => new[] { "AdvancedSummaries 1", "AdvancedSummaries 2" };

		protected override string MessageDetailsSummary => "Message Details Summary Text";

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return ("MessageDetail1", "MessageDetail1 Text");
			yield return ("MessageDetail2", "MessageDetail2 Text");
			yield return ("SequencedMessageDetail1", "SequencedMessageDetail1 Text");
			yield return ("SequencedMessageDetail2", "SequencedMessageDetail2 Text");
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			yield return ("Summary:",
				new (string, string)[] {
					("AdditionalMessageDetail1", "AdditionalMessageDetail1 Text"),
					("AdditionalMessageDetail2", "AdditionalMessageDetail2 Text"),
				});
		}

		protected override IEnumerable<(string summary, IEnumerable<string[]>)> GetAdditionalMessageWithFreeNumberOfColumns()
		{
			yield return ("Summary Free Number of Columns:",
				new List<string[]>
				{
					new string[] { "FreeNumberOfColumns1 Title", "FreeNumberOfColumns2 Title", "FreeNumberOfColumns3 Title" },
					new string[] { "FreeNumberOfColumns1 Text", "FreeNumberOfColumns2 Text", "FreeNumberOfColumns3 Text" },
				});
		}

		protected override IEnumerable<string> GetDescriptions() => new[] { "Description" };
	}
}
