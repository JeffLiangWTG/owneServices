using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC529CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC529CProvider>
	{
		public CC529CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("D73CC839-A520-4A5A-8351-0B4244740DEF", "CC529C: RELEASE FOR EXPORT");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC529CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC529CProvider provider) => AESEntryStatusList.Codes.ReleasedForExport;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC529CProvider provider)
		{
			if (messageAttachee is CusEntryHeader entryHeader)
			{
				entryHeader.CH_EntryReleaseDate = provider.ReleaseDate;
			}
		}

		protected override Type MessageInterpreterType => typeof(CC529MessageInterpreter);
	}
}
