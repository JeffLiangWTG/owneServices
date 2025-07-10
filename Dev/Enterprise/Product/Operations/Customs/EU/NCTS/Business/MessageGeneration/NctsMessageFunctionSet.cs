using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("As of Sep 2014 the development of this module is incomplete and not all message types have been implemented; implementations will be added in the future. No point in deleting code only to re-add it.")]
	public abstract class NctsMessageFunctionSet
	{
		public string Code { get { return CodeCore; } }
		protected abstract string CodeCore { get; }

		public string SentMessageStatusCode;
		internal virtual string GetSentMessageStatusCode { get { return NctsMessageStatusList.Codes.Ok; } }
		internal virtual ZString AdditionalInformation { get { return ""; } }

		public class ArrivalNotificationMessage : NctsMessageFunctionSet
		{
			public ArrivalNotificationMessage()
			{
				SentMessageStatusCode = NctsMessageStatusList.Codes.ArrivalNotificationSent;
			}

			protected override string CodeCore
			{
				get { return "IE007"; }
			}

			internal override string GetSentMessageStatusCode
			{
				get { return SentMessageStatusCode; }
			}
		}

		public class ArrivalNotificationRejectionMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE008"; }
			}
		}

		public class CancellationDecisionMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE009"; }
			}
		}

		public class DeclarationAmendmentMessage : NctsMessageFunctionSet
		{
			public DeclarationAmendmentMessage(ZString explanationToCustomsForWhyCancelling, ZString commentOnTheCancelling) : base()
			{
				UserReasonForCancellation = explanationToCustomsForWhyCancelling;
				CommentOnCancellation = commentOnTheCancelling;
			}
			public readonly ZString CommentOnCancellation;
			public readonly ZString UserReasonForCancellation;

			protected override string CodeCore
			{
				get { return "IE013"; }
			}
		}

		public class DeclarationCancellationRequestMessage : NctsMessageFunctionSet
		{
			public DeclarationCancellationRequestMessage(ZString explanationToCustomsForWhyCancelling, ZString commentOnTheCancelling) : base()
			{
				UserReasonForCancellation = explanationToCustomsForWhyCancelling;
				CommentOnCancellation = commentOnTheCancelling;
				SentMessageStatusCode = NctsMessageStatusList.Codes.CancellationRequestSent;
			}
			public readonly ZString CommentOnCancellation;

			protected override string CodeCore
			{
				get { return "IE014"; }
			}

			internal override string GetSentMessageStatusCode
			{
				get { return SentMessageStatusCode; }
			}

			public readonly ZString UserReasonForCancellation;
			internal override ZString AdditionalInformation
			{
				get { return UserReasonForCancellation; }
			}
		}

		public class DeclarationDataMessage : NctsMessageFunctionSet
		{
			public DeclarationDataMessage()
			{
				SentMessageStatusCode = NctsMessageStatusList.Codes.DepartureDeclarationSent;
			}

			protected override string CodeCore
			{
				get { return "IE015"; }
			}

			internal override string GetSentMessageStatusCode
			{
				get { return SentMessageStatusCode; }
			}
		}

		public class DeclarationRejectedMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE016"; }
			}
		}

		public class GoodsReleaseNotificationMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE025"; }
			}
		}

		public class AcceptanceNotificationMrnAllocatedMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE028"; }
			}
		}

		public class ReleaseOfTransitMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE029"; }
			}
		}

		public class UnloadingPermissionMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE043"; }
			}
		}

		public class UnloadingRemarksMessage : NctsMessageFunctionSet
		{
			public UnloadingRemarksMessage()
			{
				SentMessageStatusCode = NctsMessageStatusList.Codes.UnloadingRemarksSent;
			}

			protected override string CodeCore
			{
				get { return "IE044"; }
			}

			internal override string GetSentMessageStatusCode
			{
				get { return SentMessageStatusCode; }
			}
		}

		public class WriteOffNotificationMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE045"; }
			}
		}

		public class NoReleaseForTransitMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE051"; }
			}
		}

		public class GuaranteeNotValidMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE055"; }
			}
		}

		public class UnloadingRemarksRejectionMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE058"; }
			}
		}

		public class ControlDecisionNotificationMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE060"; }
			}
		}

		public class InformationAboutNonArrivedMovementMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE141"; }
			}
		}

		public class EdifactNackMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE907"; }
			}
		}

		public class XmlNckMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE917"; }
			}
		}

		public class PositiveAcknowledgementMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "IE928"; }
			}
		}

		public class CombinedArrivalAndDepartureMessage : NctsMessageFunctionSet
		{
			protected override string CodeCore
			{
				get { return "TNN"; }
			}
		}
	}
}
