using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Business
{
	public abstract class GbTransmissionMessageGenerator : EU.Business.MessageBuilders.IMessageGenerator<CusEntryHeader>
	{
		public virtual void AfterFullSuccess(IBuilderResult builderResult)
		{
			if (builderResult.Owner is Declaration.CusEntryHeader entryHeader)
			{
				builderResult.Message.EM_MessageText = PutBizoPkIntoSysCarPlaceholder(builderResult.Message.EM_MessageText, entryHeader);
				entryHeader.Messages.Add(builderResult.Message);

				UpdateStatus(entryHeader, builderResult.Message);

				if (declarationMessageFunction is Customs.Business.CusdecMessageFunction.Deleted _)
				{
					// Do not remove the entry number yet, as our cancellation request might fail.  Do so only when we get a cancellationacknowledgement.
					var lastOutgoingMessage = entryHeader.Messages.LastOutgoingMessage;
					if (lastOutgoingMessage != null && lastOutgoingMessage != builderResult.Message)
					{
						lastOutgoingMessage.EM_Status = EDIMessage.Status.Cancelled;
					}
				}
				else if (declarationMessageFunction is GbDes242MessageFunction messageFunction)
				{
					// add a log but DO NOT change the CH_ statuses. These messages are auxiliary. 
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					entryHeader.Logs.AddNew(Events.EditedARecord, messageFunction.FunctionHuman);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
				else if (IsNewMessage(entryHeader) || IsAmendMessage(entryHeader))
				{
					// TODO - do permit processing for delete
					// save permit consumptions....
					PermitProcessor.AddPermitTransactions(builderResult.Message, (msg) => GBPermitHelper.GetPermitAppIdForMessage(builderResult.Message));
				}

				entryHeader.ClearAllHadLastErrorYellowFlagsOnEntrysInvoiceLines();
			}
		}

		ZString GetMessageSubType(CusEntryHeader entryHeader) => IsNewMessage(entryHeader) ? MessageSubTypeCodes.Codes.Original
																					: IsAmendMessage(entryHeader) ? MessageSubTypeCodes.Codes.Change
																					: MessageSubTypeCodes.Codes.Undefined;

		protected virtual bool IsNewMessage(CusEntryHeader entryHeader) => declarationMessageFunction is Customs.Business.CusdecMessageFunction.New;

		protected virtual bool IsAmendMessage(CusEntryHeader entryHeader) => declarationMessageFunction is Customs.Business.CusdecMessageFunction.Amended;

		void UpdateStatus(Declaration.CusEntryHeader entryHeader, EDIMessage outgoingMessage)
		{
			var status = GetStatus(outgoingMessage);
			entryHeader.UpdateStatusIfNotEmpty(status);
		}

		protected virtual ZString GetStatus(EDIMessage outgoingMessage)
		{
			return GbMessageStatusCalculator.GetMessageAwaitingStatus(declarationMessageFunction);
		}

		public static string GetBizoPkHexadecimalOnly(BusinessObject bizO)
		{
			return bizO.PK.ToString().Replace("-", "").ToUpper();
		}

		public abstract ZString MakePrettyForInterpretation(EDIMessage message);
		public abstract IBuilderResult Generate(CusEntryHeader entryHeader);

		protected Customs.Business.CusdecMessageFunction declarationMessageFunction;

		public static string PutBizoPkIntoSysCarPlaceholder(string input, BusinessObject bizO)
		{
			return input.Replace(SysCarPlaceHolder, GetBizoPkHexadecimalOnly(bizO));
		}

		public void PutReferenceNumberIntoMessageFromPlaceholder(EDIMessage message, ZString messageText, CusEntryHeader bizO)
		{
			PutDeclarationNumberIntoMessageFromPlaceholderCore(message, messageText, bizO);
		}

		protected virtual void PutDeclarationNumberIntoMessageFromPlaceholderCore(EDIMessage message, ZString messageText, CusEntryHeader entryHeader)
		{
			message.EM_MessageText = messageText.Replace(JobDeclaration.DeclarationReferencePlaceHolder, entryHeader.Declaration.JE_DeclarationReference);
			message.EM_MessageText = message.EM_MessageText.Replace(DeclarationReferenceWithBox7OwnerReferencePlaceHolder, entryHeader.Declaration.TradersOwnReferenceFullForBox7);
		}

		public GBCusPermitCusDecProcessor PermitProcessor => permitProcessor ?? (permitProcessor = new GBCusPermitCusDecProcessor(Entry, GetMessageSubType(Entry)));

		GBCusPermitCusDecProcessor permitProcessor;

		protected CusEntryHeader Entry { get; set; }

		public const string SysCarPlaceHolder = "<<SYSCAR>>";
		public static ZString DeclarationReferenceWithBox7OwnerReferencePlaceHolder = "<<BOX7>>";
		public Customs.Business.ISendsMessagesToCustoms SendMessagesToCustoms { get; set; }
	}
}
