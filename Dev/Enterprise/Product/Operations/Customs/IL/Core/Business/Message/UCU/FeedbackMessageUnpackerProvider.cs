namespace Enterprise.Customs.IL.Business
{
	public static class FeedbackMessageUnpackerProvider
	{
		public static IFeedbackMessageUnpacker GetFeedbackMessageUnpacker(string customsFeedbackMessageName)
			=> customsFeedbackMessageName switch
			{
				Constants.CustomsFeedBackMessageMainTagName.DeliveryOrder => new FeedbackMessageUnpacker<ILDLO122ResponseMessage>(),
				Constants.CustomsFeedBackMessageMainTagName.GatePassMovement => new FeedbackMessageUnpacker<ILGPM135ResponseMessage>(),
				Constants.CustomsFeedBackMessageMainTagName.ImportDeclarationResponse => new FeedbackMessageUnpacker<ILDEC274ResponseMessage>(),
				Constants.CustomsFeedBackMessageMainTagName.Manifest => new FeedbackMessageUnpacker<ILMAN171ResponseMessage>(),
				Constants.CustomsFeedBackMessageMainTagName.ManifestQueryResponse => new FeedbackMessageUnpacker<ILMAN821ResponseMessage>(),
				Constants.CustomsFeedBackMessageMainTagName.DocumentMessageResponse => new FeedbackMessageUnpacker<ILDOC276ResponseMessage>(),
				Constants.CustomsFeedBackMessageMainTagName.DocumentRqDecisionMessageResponse => new FeedbackMessageUnpacker<ILDOC828ResponseMessage>(),
				Constants.CustomsFeedBackMessageMainTagName.OutgoingMessageResponse => new OutgoingMessageResponseUnpacker(),
				Constants.CustomsFeedBackMessageMainTagName.InfMsgGenericResponse => new InfMsgGenericUnpacker(),
				_ => null,
			};
	}
}
