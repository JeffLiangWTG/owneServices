using CargoWise.Types;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.CN.Business
{
	public partial class JobMessageStatusList
	{
		public static bool IsAwaiting(ZString messageStatus)
		{
			return messageStatus == Codes.AwaitingResponseIntegratedDeclaration
				|| messageStatus == Codes.AwaitingResponseCompletedDeclaration
				|| messageStatus == Codes.AwaitingResponsePreliminaryDeclaration;
		}

		public static bool IsAcknowledged(ZString messageStatus)
		{
			return messageStatus == Codes.AcknowledgedCompletedDeclaration
				|| messageStatus == Codes.AcknowledgedIntegratedDeclaration
				|| messageStatus == Codes.AcknowledgedPreliminaryDeclaration;
		}

		public static ZString GetAcknowledgedStatus(ZString currentStatus)
		{
			ZString result;

			switch (currentStatus)
			{
				case Codes.AwaitingResponseCompletedDeclaration:
				case Codes.AcknowledgedCompletedDeclaration:
				case Codes.ErrorCompletedDeclaration:
				case Codes.ClearedCompletedDeclaration:
					result = Codes.AcknowledgedCompletedDeclaration;
					break;
				case Codes.AwaitingResponsePreliminaryDeclaration:
				case Codes.AcknowledgedPreliminaryDeclaration:
				case Codes.ErrorPreliminaryDeclaration:
				case Codes.ClearedPreliminaryDeclaration:
					result = Codes.AcknowledgedPreliminaryDeclaration;
					break;
				default:
					result = Codes.AcknowledgedIntegratedDeclaration;
					break;
			}
			return result;
		}

		public static ZString GetErrorStatus(ZString currentStatus)
		{
			ZString result;

			switch (currentStatus)
			{
				case Codes.AwaitingResponseCompletedDeclaration:
				case Codes.AcknowledgedCompletedDeclaration:
				case Codes.ErrorCompletedDeclaration:
				case Codes.ClearedCompletedDeclaration:
					result = Codes.ErrorCompletedDeclaration;
					break;
				case Codes.AwaitingResponsePreliminaryDeclaration:
				case Codes.AcknowledgedPreliminaryDeclaration:
				case Codes.ErrorPreliminaryDeclaration:
				case Codes.ClearedPreliminaryDeclaration:
					result = Codes.ErrorPreliminaryDeclaration;
					break;
				default:
					result = Codes.ErrorIntegratedDeclaration;
					break;
			}
			return result;
		}

		public static ZString GetClearedStatus(ZString curentStatus)
		{
			ZString result;

			switch (curentStatus)
			{
				case Codes.AwaitingResponseCompletedDeclaration:
				case Codes.AcknowledgedCompletedDeclaration:
				case Codes.ErrorCompletedDeclaration:
				case Codes.ClearedCompletedDeclaration:
					result = Codes.ClearedCompletedDeclaration;
					break;
				case Codes.AwaitingResponsePreliminaryDeclaration:
				case Codes.AcknowledgedPreliminaryDeclaration:
				case Codes.ErrorPreliminaryDeclaration:
				case Codes.ClearedPreliminaryDeclaration:
					result = Codes.ClearedPreliminaryDeclaration;
					break;
				default:
					result = Codes.ClearedIntegratedDeclaration;
					break;
			}
			return result;
		}

		public static ZString GetAwaitingStatusByDeclarationType(ZString currentStatus)
		{
			ZString result;

			switch (currentStatus)
			{
				case DeclarationTypeList.Codes.IntegratedDeclaration:
					result = Codes.AwaitingResponseIntegratedDeclaration;
					break;
				case DeclarationTypeList.Codes.PreliminaryDeclaration:
				case DeclarationTypeList.Codes.ManualDeclaration:
				case DeclarationTypeList.Codes.AutoDeclaration:
					result = Codes.AwaitingResponsePreliminaryDeclaration;
					break;
				case DeclarationTypeList.Codes.CompleteDeclaration:
					result = Codes.AwaitingResponseCompletedDeclaration;
					break;
				default:
					result = MessageStatusList.Codes.AwaitingOriginal;
					break;
			}

			return result;
		}

		public static bool AvailableForCompleteDeclaration(ZString entryStatus)
		{
			return entryStatus == Codes.ClearedPreliminaryDeclaration
						|| entryStatus == Codes.AwaitingResponseCompletedDeclaration
						|| entryStatus == Codes.AcknowledgedCompletedDeclaration
						|| entryStatus == Codes.ErrorCompletedDeclaration
						|| entryStatus == Codes.ClearedCompletedDeclaration;
		}
	}
}
