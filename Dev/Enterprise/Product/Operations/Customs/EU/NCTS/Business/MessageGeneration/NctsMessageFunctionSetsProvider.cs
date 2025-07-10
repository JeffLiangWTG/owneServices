using System.Diagnostics.CodeAnalysis;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public class NctsMessageFunctionSetsProvider
	{
		public NctsMessageFunctionSet GetMessageFunction(ZString messageCode)
		{
			return GetMessageFunctionCore(messageCode);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual NctsMessageFunctionSet GetMessageFunctionCore(ZString messageCode)
		{
			switch (messageCode)
			{
				case "007":
					return new NctsMessageFunctionSet.ArrivalNotificationMessage();
				case "008":
					return new NctsMessageFunctionSet.ArrivalNotificationRejectionMessage();
				case "009":
					return new NctsMessageFunctionSet.CancellationDecisionMessage();
				case "013":
					return new NctsMessageFunctionSet.DeclarationAmendmentMessage("", "");
				case "014":
					return new NctsMessageFunctionSet.DeclarationCancellationRequestMessage("", "");
				case "015":
					return new NctsMessageFunctionSet.DeclarationDataMessage();
				case "016":
					return new NctsMessageFunctionSet.DeclarationRejectedMessage();
				case "025":
					return new NctsMessageFunctionSet.GoodsReleaseNotificationMessage();
				case "028":
					return new NctsMessageFunctionSet.AcceptanceNotificationMrnAllocatedMessage();
				case "029":
					return new NctsMessageFunctionSet.ReleaseOfTransitMessage();
				case "043":
					return new NctsMessageFunctionSet.UnloadingPermissionMessage();
				case "044":
					return new NctsMessageFunctionSet.UnloadingRemarksMessage();
				case "045":
					return new NctsMessageFunctionSet.WriteOffNotificationMessage();
				case "051":
					return new NctsMessageFunctionSet.NoReleaseForTransitMessage();
				case "055":
					return new NctsMessageFunctionSet.GuaranteeNotValidMessage();
				case "058":
					return new NctsMessageFunctionSet.UnloadingRemarksRejectionMessage();
				case "060":
					return new NctsMessageFunctionSet.ControlDecisionNotificationMessage();
				case "141":
					return new NctsMessageFunctionSet.InformationAboutNonArrivedMovementMessage();
				case "907":
					return new NctsMessageFunctionSet.EdifactNackMessage();
				case "917":
					return new NctsMessageFunctionSet.XmlNckMessage();
				case "928":
					return new NctsMessageFunctionSet.PositiveAcknowledgementMessage();
				default:
					return null;
			}
		}
	}
}
