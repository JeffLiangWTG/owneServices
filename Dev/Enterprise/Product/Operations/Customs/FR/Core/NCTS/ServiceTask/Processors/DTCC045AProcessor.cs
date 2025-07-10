using System;
using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.CC045A;
using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NctsTransitStatusList = Enterprise.Customs.FR.Business.NctsTransitStatusList;

namespace Enterprise.Customs.FR.NCTS.ServiceTask
{
	public class DTCC045AProcessor : DTBaseProcessor<Cc045AType>
	{
		public DTCC045AProcessor(ILogger serviceLogger, LoggingInformation loggingInformation)
			: base(serviceLogger, loggingInformation)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"FR NCTS Delta T CC045A processor";

		protected override ZString GetNewMessageStatus(Cc045AType messageObject) => EU.NCTS.Business.NctsMessageStatusList.Codes.Ok;

		protected override ZString GetNewDepartureStatus(Cc045AType messageObject) => NctsTransitStatusList.Codes.GoodsWrittenOff;

		protected override ZString GetNewDetailedDepartureStatus(Cc045AType messageObject)
		{
			if (messageObject.Heahea != null)
			{
				switch (messageObject.Heahea.IrrHea1020)
				{
					case Flag.Item0:
						return NctsDetailedStatusList.Codes.NoIrregularities;
					case Flag.Item1:
						return NctsDetailedStatusList.Codes.Irregularities;
				}
			}
			return ZString.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key value strings")]
		protected override ZString GetMessageInterpretation(Cc045AType messageObject)
		{
			var sb = new ZStringBuilder();
			sb.Append(GetDepartureStatusInterpretation(messageObject));
			sb.Append(GetDetailedDepartureStatusInterpretation(messageObject));
			sb.Append(GetGrantedTimeInterpretation(messageObject));
			if (messageObject.Heahea != null)
			{
				sb.Append(GetKeyValuePairInterpretation("Written off date", GetReadableDate(messageObject.Heahea.WriOffDatHea619)));
				sb.Append(GetReadableDate(GetKeyValuePairInterpretation("Irregularities", messageObject.Heahea.IrrHea1020 == Flag.Item1 ? "Yes" : "No")));
			}
			return sb.ToString();
		}

		internal override void UpdateGuaranteeTransactionsIfNeeded(Cc045AType messageObject, EDIMessage incomingMessage)
		{
			var header = incomingMessage.EM_LinkedObject as EU.NCTS.Business.NctsHeader;
			UpdateGuaranteeTransactionsIfNeeded(header, incomingMessage, (errorMessage) => { ServiceLogger.Log(LogType.Warning, errorMessage); });
		}

		public static void UpdateGuaranteeTransactionsIfNeeded(EU.NCTS.Business.NctsHeader header, List<ZString> errorMessageList)
		{
			UpdateGuaranteeTransactionsIfNeeded(header, null, (errorMessage) => { errorMessageList.Add(errorMessage); });
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Key value strings")]
		static void UpdateGuaranteeTransactionsIfNeeded(EU.NCTS.Business.NctsHeader header, EDIMessage incomingMessage, Action<ZString> logErrorMessage)
		{
			if (header != null)
			{
				var guarantees = header.GetEffectiveGuarantees();
				if (guarantees.Count > 0)
				{
					if(incomingMessage == null)
					{
						PermitHelper.UpdatePendingTransactionsWithAdditionalCriteria(header.Factory, Core.Constants.CountryCodes.France, header.GetPermitReference(), (SharedCusPermitLineTransaction x) => true, status: PermitTransactionStatusList.Codes.Confirmed, referenceNumberLine: header.GetPermitReferenceNumberLine());
					}
					else
					{
						PermitHelper.UpdatePendingTransactions(incomingMessage, header.Messages.LastOutgoingMessage, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.France, false, PermitTransactionStatusList.Codes.Confirmed);
					}

					foreach (EU.NCTS.Business.NctsGuarantee nctsGuarantee in guarantees)
					{
						if (!nctsGuarantee.PW_BondNumber.IsEmpty && nctsGuarantee.CusGuarantee == null)
						{
							var errorMessage = $"Guarantee was not written off for {header.BH_JobReference} because {nctsGuarantee.PW_BondNumber} does not refer to a guarantee managed by CW1.";
							header.Logs.AddNew(Events.DeclarationHasErrors, errorMessage);
							logErrorMessage.Invoke(errorMessage);
						}
						var appId = incomingMessage == null ? "Manual Closure" : incomingMessage.EM_MessageNum.ToString();
						nctsGuarantee.CusGuarantee?.AddTransaction(header.GetPermitReference(), $"NCTS write-off {header.LocalReferenceNumber} [{header.MovementReferenceNumber}]", appId, "", nctsGuarantee.PW_BondAmount, 0, status: PermitTransactionStatusList.Codes.Confirmed, notifier: NotifierWhenBusting);
					}

					void NotifierWhenBusting(ZString errMsg, ZDecimal balance)
					{
						header.Logs.AddNew(Events.DeclarationHasErrors, errMsg);
						logErrorMessage.Invoke(errMsg);
					}
				}
			}
		}
	}
}
