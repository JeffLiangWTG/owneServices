using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Customs.BR.MessageContracts.ImportLicense.Outgoing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRImportLicenseInterchangeProvider : BRInterchangeProvider
	{
		public BRImportLicenseInterchangeProvider(NonDependentEDIMessageCollection messageCollection)
			: base(messageCollection)
		{
		}

		protected override ZString ProcessedMessageStatusCode(EDIInterchange interchange) => Constants.EDIMessageStatusCodes.Manual;

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => Constants.EDIMessageStatusCodes.Manual;

		protected override ZString GetTransportTypeCode() => EDIInterchange.TransportType.tXT;

		protected override string GetCollationKey(EDIMessage message) => message.EM_MessageType;

		protected override bool ShouldAddToEDocs(EDIMessage message) => true;

		protected override string GetEDocFileName(EDIMessage message, JobDeclaration declaration) => $"{declaration.JE_DeclarationReference}_{message.EM_ApplicationReference}.{Core.Constants.FileFormats.XML}";

		protected override void AppendMessageTextToMessageBody(StringBuilder stringBuilder, IEnumerable<ZString> messageTextList, EDIInterchange interchange)
		{
			stringBuilder.Append(ImportLicenseMessageBuilder.JoinImportLicenseMessages(messageTextList.Select(x => x.ToString())));
		}
	}
}

