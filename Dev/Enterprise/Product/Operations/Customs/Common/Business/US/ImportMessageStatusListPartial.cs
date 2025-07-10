using System;
using System.Collections.Generic;

namespace Enterprise.Customs.Common.US
{
	partial class ImportMessageStatusList : IStatusList
	{
		public enum MessageType { Undefined, Export, EntrySummary, InBondDeparture, InBondUpdate, CargoRelease, ElectronicInvoice, BorderCargoRelease, ReconEntry, Protest, ACECargoRelease, TemporaryImportationBond }

		public static string[] GetErrorStatusForEntrySummary()
		{
			return new string[]
			{
				Codes.EntrySummaryOriginalAcceptedWithWarnings,
				Codes.EntrySummaryReplaceAcceptedWithWarnings,
				Codes.ErrorEntrySummaryDelete,
				Codes.ErrorEntrySummaryOriginal,
				Codes.ErrorEntrySummaryReplace
			};
		}

		#region IStatusList Members

		public IReadOnlyList<string> RejectStatusInterested
		{
			get
			{
				return new string[]
				{
					Codes.ErrorEntrySummaryDelete,
					Codes.ErrorEntrySummaryOriginal,
					Codes.ErrorEntrySummaryReplace,

					Codes.ErrorElectronicInvoiceDelete,
					Codes.ErrorElectronicInvoiceOriginal,
					Codes.ErrorElectronicInvoiceReplace,

					Codes.ErrorACECargoReleaseDelete,
					Codes.ErrorACECargoReleaseAdd,
					Codes.ErrorACECargoReleaseReplace,
					Codes.ErrorACECargoReleaseUpdate
				};
			}
		}

		public IReadOnlyList<string> AcceptedStatusToCancelRejectStatusInterested
		{
			get
			{
				return new string[]
				{
					Codes.ClearEntrySummaryDelete,
					Codes.ClearEntrySummaryOriginal,
					Codes.ClearEntrySummaryReplace,

					Codes.ClearElectronicInvoiceDelete,
					Codes.ClearElectronicInvoiceOriginal,
					Codes.ClearElectronicInvoiceReplace,

					Codes.EntrySummaryOriginalAcceptedWithWarnings,
					Codes.EntrySummaryReplaceAcceptedWithWarnings,

					Codes.ClearACECargoReleaseDelete,
					Codes.ClearACECargoReleaseReplace,
					Codes.ClearACECargoReleaseUpdate
				};
			}
		}

		public bool IsStatusClear(string status)
		{
			switch (status)
			{
				case Codes.ClearEntrySummaryOriginal:
				case Codes.ClearEntrySummaryReplace:
				case Codes.ClearEntrySummaryDelete:
				case Codes.EntrySummaryOriginalAcceptedWithWarnings:
				case Codes.EntrySummaryReplaceAcceptedWithWarnings:

				case Codes.EntrySummaryOriginalAcceptedWithCensusWarnings:
				case Codes.EntrySummaryReplaceAcceptedWithCensusWarnings:

				case Codes.ClearElectronicInvoiceOriginal:
				case Codes.ClearElectronicInvoiceReplace:
				case Codes.ClearElectronicInvoiceDelete:

				case Codes.ClearDepartureAmendment:
				case Codes.ClearDepartureOriginal:
				case Codes.ClearArrival:
				case Codes.ClearExportation:
				case Codes.ClearDepartureWithdraw:

				case Codes.ClearFDATransmission:
				case Codes.ClearTransferOfLiability:
				case Codes.ClearDeparturePartialOriginal:
				case Codes.ClearDeparturePartialWithdraw:
				case Codes.ClearCargoReleaseOriginal:
				case Codes.ClearCargoReleaseReplace:
				case Codes.ClearCargoReleaseDelete:

				case Codes.ClearBorderCargoReleaseOriginal:
				case Codes.ClearBorderCargoReleaseReplace:
				case Codes.ClearBorderCargoReleaseDelete:

				case Codes.ClearConsigneeNameAddressAdd:

				case Codes.ClearACECargoReleaseAdd:
				case Codes.ClearACECargoReleaseDelete:
				case Codes.ClearACECargoReleaseReplace:
				case Codes.ClearACECargoReleaseUpdate:
					return true;
				default:
					return false;
			}
		}

		public bool IsWaitingForResponse(string status)
		{
			switch (status)
			{
				case Codes.AwaitingEntrySummaryOriginal:
				case Codes.AwaitingEntrySummaryDelete:
				case Codes.AwaitingEntrySummaryReplace:
				case Codes.AwaitingElectronicInvoiceDelete:
				case Codes.AwaitingElectronicInvoiceOriginal:
				case Codes.AwaitingElectronicInvoiceReplace:
				case Codes.AwaitingDepartureAmendment:
				case Codes.AwaitingArrival:
				case Codes.AwaitingExportation:
				case Codes.AwaitingFDATransmission:
				case Codes.AwaitingTransferOfLiability:
				case Codes.AwaitingDepartureOriginal:
				case Codes.AwaitingDepartureWithdraw:
				case Codes.AwaitingCargoReleaseDelete:
				case Codes.AwaitingCargoReleaseOriginal:
				case Codes.AwaitingCargoReleaseReplace:
				case Codes.AwaitingBorderCargoReleaseDelete:
				case Codes.AwaitingBorderCargoReleaseOriginal:
				case Codes.AwaitingBorderCargoReleaseReplace:
				case Codes.AwaitingConsigneeNameAddressAdd:

				case Codes.AwaitingACECargoReleaseAdd:
				case Codes.AwaitingACECargoReleaseDelete:
				case Codes.AwaitingACECargoReleaseReplace:
				case Codes.AwaitingACECargoReleaseUpdate:
					return true;
				default:
					return false;
			}
		}

		public bool IsWithdrawnStatus(string status)
		{
			switch (status)
			{
				case Codes.ClearDepartureWithdraw:
				case Codes.ClearDeparturePartialWithdraw:
				case Codes.ClearEntrySummaryDelete:
				case Codes.ClearElectronicInvoiceDelete:
				case Codes.ClearCargoReleaseDelete:
				case Codes.ClearBorderCargoReleaseDelete:
				case Codes.ClearACECargoReleaseDelete:
					return true;
				default:
					return false;
			}
		}

		public bool IsPartialStatus(string status)
		{
			switch (status)
			{
				case Codes.ClearDeparturePartialAmendment:
				case Codes.ClearDeparturePartialOriginal:
				case Codes.ClearDeparturePartialWithdraw:
					return true;
				default:
					return false;
			}
		}

		public bool IsArrivalExportBTATransmissionStatus(string status)
		{
			switch (status)
			{
				case Codes.AwaitingArrival:
				case Codes.AwaitingExportation:
				case Codes.AwaitingFDATransmission:
				case Codes.AwaitingTransferOfLiability:

				case Codes.ClearArrival:
				case Codes.ClearExportation:
				case Codes.ClearFDATransmission:
				case Codes.ClearTransferOfLiability:

				case Codes.ErrorArrival:
				case Codes.ErrorExportation:
				case Codes.ErrorFDATransmission:
				case Codes.ErrorTransferOfLiability:
					return true;
				default:
					return false;
			}
		}

		public IReadOnlyList<string> GetFirstClearStatusFor(MessageType messageType)
		{
			switch (messageType)
			{
				case MessageType.BorderCargoRelease:
					return new string[] { Codes.ClearBorderCargoReleaseOriginal };
				case MessageType.EntrySummary:
					return new string[]
					{
						Codes.ClearEntrySummaryOriginal, Codes.EntrySummaryOriginalAcceptedWithWarnings, Codes.EntrySummaryOriginalAcceptedWithCensusWarnings,
						Codes.ClearEntrySummaryReplace, Codes.EntrySummaryReplaceAcceptedWithWarnings, Codes.EntrySummaryReplaceAcceptedWithCensusWarnings
					};
				case MessageType.ElectronicInvoice:
					return new string[] { Codes.ClearElectronicInvoiceOriginal };
				case MessageType.CargoRelease:
					return new string[] { Codes.ClearCargoReleaseOriginal };
				case MessageType.InBondDeparture:
					return new string[] { Codes.ClearDepartureOriginal, Codes.ClearDeparturePartialOriginal };
				case MessageType.InBondUpdate:
					return new string[] { Codes.ClearArrival, Codes.ClearExportation, Codes.ClearTransferOfLiability };
				case MessageType.ACECargoRelease:
					return new string[] { Codes.ClearACECargoReleaseAdd };
				case MessageType.Undefined:
					return Array.Empty<string>();
				default:
					throw new NotSupportedException(string.Format("Message type '{0}' is not supported in {1}", messageType, GetType().FullName));
			}
		}

		#endregion
	}
}
