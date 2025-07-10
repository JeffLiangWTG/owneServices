using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SoapHeaderProvider : ICS2BaseMessageProvider, ISoapHeader
	{
		public SoapHeaderProvider(AsycudaManifestHeader header, EDIInterchange interchange)
			: this(header, interchange, Array.Empty<IBinaryFile>())
		{
		}

		public SoapHeaderProvider(AsycudaManifestHeader header, EDIInterchange interchange, IReadOnlyCollection<IBinaryFile> binaryFiles)
			: base(header)
		{
			this.interchange = Argument.NotNull(interchange, nameof(interchange));
			BinaryFiles = Argument.NotNull(binaryFiles, nameof(binaryFiles));
		}

		readonly EDIInterchange interchange;

		public IReadOnlyCollection<IBinaryFile> BinaryFiles { get; }

		public string SpecificCircumstance
		{
			get
			{
				return interchange.EI_InterchangeType == EDIInterchangeTypeList.Codes.TST
				? ZString.Empty
				: interchange.ContainedMessages.Cast<EDIMessage>().FirstOrDefault()?.EM_MessageType ?? ZString.Empty;
			}
		}

		public string FromPartyValue => GetFromPartyValueCore();

		protected virtual string GetFromPartyValueCore()
		{
			return MessageProviderHelper.GetFromParty(interchange.Factory, manifestHeader.ProfileCompany);
		}

		public string MessageId => GetMessageIdCore();

		protected virtual string GetMessageIdCore()
		{
			if (interchange.EI_InterchangeType != EDIInterchangeTypeList.Codes.TST)
			{
				return $"{interchange.PK}@{MessageProviderHelper.DomainName}";
			}
			else
			{
				return TestMessageId;
			}
		}

		public string PartInfoHref => $"cid:attachment1@{MessageProviderHelper.DomainName}";

		public const string TestMessageId = "985f1178-1447-4660-a365-1a843fb3906c";

		string IICS2MessageHeader.LRN => interchange.EI_InterchangeType != EDIInterchangeTypeList.Codes.TST ? LRN : null;
	}
}
