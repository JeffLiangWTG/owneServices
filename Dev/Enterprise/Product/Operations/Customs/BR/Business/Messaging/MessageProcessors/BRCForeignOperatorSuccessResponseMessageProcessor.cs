using System.Collections.Generic;
using CargoWise.Customs.BR.MessageDefinitions.ProductCatalog;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRCForeignOperatorSuccessResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCForeignOperatorSuccessResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("D19AD911-B0A9-4379-BB43-1674639C63C1", "Foreign Operator Success Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.OPE };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.Success };

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			BusinessObject foreignOperator = null;
			var batchVersionValidation = BRMessageHelper.DeserializeObject<LoteValidacaoVersaoDTO>(message.EM_MessageText);
			if (batchVersionValidation != null && batchVersionValidation.seq != 0)
			{
				foreignOperator = GetLinkedObjectFromOutgoingMessage(message, batchVersionValidation.seq);
			}
			else
			{
				Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed or tag '{nameof(batchVersionValidation.seq)}' not found or is empty.");
			}
			return foreignOperator;
		}

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is CusBRForeignOperator foreignOperator)
			{
				foreignOperator.SuspendUpdateMessageStatusOnSavingUntilSaved();
				var loteValidacaoVersao = BRMessageHelper.DeserializeObject<LoteValidacaoVersaoDTO>(message.EM_MessageText);
				if (loteValidacaoVersao.sucesso)
				{
					if (!loteValidacaoVersao.codigo.IsEmpty())
					{
						foreignOperator.BFR_AuthorityIdentifier = loteValidacaoVersao.codigo;
					}

					if (!loteValidacaoVersao.versao.IsEmpty())
					{
						foreignOperator.BFR_AuthorityVersion = loteValidacaoVersao.versao;
					}

					if (GetCustomsStatus(foreignOperator) is string customStatus)
					{
						foreignOperator.BFR_CustomsStatus = customStatus;
					}

					foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Accepted;
				}
				else
				{
					foreignOperator.Logs.AddNew(AutoEvents.MessageRejected);
					foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.Rejected;
				}
			}
		}

		string GetCustomsStatus(CusBRForeignOperator foreignOperator)
		{
			string action = foreignOperator.Logs.MostRecentLogByEventTime(Events.MessageSent)?.SL_Reference;
			return action switch
			{
				ActionList.Codes.CreateNewVersion => ForeignOperatorCustomsStatusTypeList.Codes.Active,
				ActionList.Codes.Activate => ForeignOperatorCustomsStatusTypeList.Codes.Active,
				ActionList.Codes.Deactivate => ForeignOperatorCustomsStatusTypeList.Codes.Inactive,
				_ => null,
			};
		}
	}
}
