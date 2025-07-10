using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC014C_v515.CC014CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class CancelNCTSResponseMessageProcessor : NCTS5CommonResponseMessageProcessor<Cc014Cv1Sal, CancelNCTSMessagePrettyFormatter>
	{
		public CancelNCTSResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		const string CancellationOKCode = "I";

		protected override string MessageFriendlyNameCore => (NoResString)"NCTS Cancel Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC014CV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation };

		protected override ZBool SetPhaseStatusTo015 => true;

		protected override CancelNCTSMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc014Cv1Sal response, EDIMessage message, NctsHeader nctsHeader) => new CancelNCTSMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Cc014Cv1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			if (!nctsHeader.IsDepartureMovement)
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(nctsHeader, "Departure"));
			}

			var responseCode = response.ControlRespuesta.CodigoRespuesta;
			if (responseCode == CancellationOKCode)
			{
				nctsHeader.MovementHeader.BM_CustomsStatus = ESNCTS5DepartureCustomsStatusList.Codes.Cancelled;

				AddTransactionToGuarantees(nctsHeader, (ZDateTime)response.DatosRespuestaCorrecta.FechaInvalidacion);
			}

			return ZString.Empty;
		}

		void AddTransactionToGuarantees(NctsHeader nctsHeader, ZDateTime cancellationDate)
		{
			foreach (NctsGuarantee guarantee in nctsHeader.MovementHeader.Guarantees)
			{
				var guaranteeHeader = CusGuaranteeHeaderHelper.LoadCusGuaranteeHeaderFromReference(nctsHeader.Factory, guarantee.PW_BondNumber, nctsHeader.CountryCode, EUGuaranteeTypeList.Codes.TRA);
				if (guaranteeHeader != null)
				{
					AddTransactionsToGuarantee(nctsHeader, guaranteeHeader, cancellationDate);
				}
			}
		}

		void AddTransactionsToGuarantee(NctsHeader nctsHeader, CusGuaranteeHeader guaranteeHeader, ZDateTime cancellationDate)
		{
			var transactionsAmount = GetTransactionsAmount(guaranteeHeader, nctsHeader);

			if (transactionsAmount < 0)
			{
				var absTransactionsAmount = Math.Abs(transactionsAmount);
				AddGuaranteeTransaction(guaranteeHeader, nctsHeader.MovementReferenceNumber, (NoResString)"NCTS Departure " + nctsHeader.BH_JobReference + (NoResString)"(Canceled)", absTransactionsAmount, cancellationDate);
			}
		}

		decimal GetTransactionsAmount(CusGuaranteeHeader guaranteeHeader, NctsHeader nctsHeader) => guaranteeHeader.GetTransactions()?.Cast<SharedCusPermitLineTransaction>().Where(x => x.CPL_TransactionStatus == PermitTransactionStatusList.Codes.Confirmed && (x.CPL_Reference == nctsHeader.MovementReferenceNumber || x.CPL_Reference == nctsHeader.BH_JobReference)).Sum(x => x.CPL_TranValue) ?? ZDecimal.Zero;

		void AddGuaranteeTransaction(CusGuaranteeHeader guaranteeHeader, string reference, string comment, decimal tranValue, ZDateTime cancellationDate)
		{
			if (guaranteeHeader.HasOpeningBalanceTransaction)
			{
				var tranDate = cancellationDate.IsEmpty ? ZDateTime.Today : cancellationDate;

				guaranteeHeader.AddTransaction(reference, comment, "", "", tranValue, ZDecimal.Zero, status: PermitTransactionStatusList.Codes.Confirmed, transactionDate: tranDate, checkBursting: true);
			}
		}

		const string XsdSchemaNameCC014CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.CC014CV1Sal.xsd";
	}
}
