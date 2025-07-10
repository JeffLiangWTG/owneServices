using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.GB.Chief.Declaration
{
	public class ChiefEntryDeclarationMessageProcessor : GBAutoSendCustomsMessageProcessor
	{
		public ChiefEntryDeclarationMessageProcessor(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ZBool SendCustomsMessageCore(INotifications notifications, CusEntryHeader entryHeader)
		{
			var minutesAfterRoute6AcceptanceToGetClearance = GBCustomsDataRegistry.Instance.MinutesToWaitAfterRoute6AcceptanceToGetClearance.Value;
			return SendCustomsMessage(notifications, entryHeader, minutesAfterRoute6AcceptanceToGetClearance + 1, (n, m) => LogSystemError(n, m));
		}

		protected override ZString MessageDescription => Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public static bool SendCustomsMessage(INotifications notifications, CusEntryHeader entryHeader, int delayInMinutes, Action<INotifications, string> logError)
		{
			try
			{
				var entry = (Business.Declaration.CusEntryHeader)entryHeader;
				var sender = new GenericImmediateEntryMessageSender(entry, string.Empty, delayInMinutes);
				var shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
				sender.Send(entry.Declaration, shutterUpperer, new CusdecMessageFunction.New());
				entry.CH_Status = GbMessageStatusCalculator.GetMessageAwaitingStatus(new CusdecMessageFunction.New());

				if (!shutterUpperer.InvalidOperationText.IsNullOrEmpty())
				{
					logError?.Invoke(notifications, shutterUpperer.InvalidOperationText);
					return false;
				}

				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logError?.Invoke(notifications, ex.Message);
				return false;
			}
		}
	}
}
