using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.PBN.Messaging;

namespace Enterprise.Customs.IE.PBN.Business;

public class ROSErrorProcessor : PBNMessageProcessor<ROSErrorProvider>
{
	public ROSErrorProcessor(LoggingInformation logger, Type messageObjectType) : base(logger, messageObjectType) { }

	protected override string MessageFriendlyNameCore => Res.GetString("DF728714-072B-4BE5-9896-AD4F79C656FE", "ROS Error Processor");

	protected override Type MessageInterpreterType => typeof(ROSErrorInterpreter);

	public override string GetEntryStatus(PBNInboundEDIMessage message, IMessageAttachee messageAttachee, ROSErrorProvider provider) => PBNCustomsStatusList.Codes.Rejected;

	public override string GetLogicalStatus(PBNInboundEDIMessage message, IMessageAttachee messageAttachee, ROSErrorProvider provider) => LogicalStatusList.Codes.Failed;
}
