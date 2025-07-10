using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class MotivationWrapper : IMotivation
	{
		public int vocReasonMotivationLengthPart = 260;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public MotivationWrapper(DeltaGJobDeclarationMessageSendingObject sendingObject)
		{
			this.sendingObjectItem = Argument.NotNull(sendingObject, "DeltaGJobDeclarationMessageSendingObject cannot be null");

			this.itemVOCReason = sendingObject?.VOCReason ?? ZString.Empty;
			this.itemAmendmentReasonCode = sendingObject?.ChangeAcknowledgementIndicator ?? ZString.Empty;
		}

		public ZString Motivation => itemVOCReason.Left(vocReasonMotivationLengthPart);

		public ZString RegularJustification => GetReasonCodeShortDescription();

		public ZString Comment => (itemVOCReason.Length > vocReasonMotivationLengthPart) ? itemVOCReason.SubstringSafe(vocReasonMotivationLengthPart, itemVOCReason.Length) : ZString.Empty;

		public ZString NewDestination => (this.sendingObjectItem?.ReplacementDeclarationList.GetDescriptionFromCode(sendingObjectItem?.ReplacementDeclarationType)) ?? ZString.Empty;

		#region Methods
		ZString GetReasonCodeShortDescription()
		{
			ZString result = "";

			var reasonShortCode = sendingObjectItem.ReasonCodeShortDescriptionList.ToArray().Where(c => c.Code == itemAmendmentReasonCode).FirstOrDefault();

			if (reasonShortCode != null)
			{
				result = reasonShortCode.Description;
			}

			return result;
		}
		#endregion

		protected readonly ZString itemVOCReason;
		protected readonly ZString itemAmendmentReasonCode;
		readonly DeltaGJobDeclarationMessageSendingObject sendingObjectItem;
	}
}
