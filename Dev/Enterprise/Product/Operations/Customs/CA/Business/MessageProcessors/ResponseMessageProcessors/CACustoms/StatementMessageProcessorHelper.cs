using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.CA.Registry;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public static class StatementMessageProcessorHelper
	{
		public static CusStatementHeader GetOrCreateCusStatementHeader(BusinessObjectFactory factory, ZString statementNumber, ZString statementType, ZString importerBusinessNumber, ZGuid importerPK, bool isMonthlyStatement)
		{
			var query = new ZQuery();
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, statementNumber);
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, statementType);
			query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, isMonthlyStatement);
			var result = factory.LoadTop1<CusStatementHeader>(query);

			if (result == null)
			{
				result = factory.New<CusStatementHeader>();
				result.B2_IsMonthlyStatement = isMonthlyStatement;

				SetValue(result, CusStatementHeaderSchema.B2_StatementType, statementType);
				SetValue(result, CusStatementHeaderSchema.B2_StatementNumber, statementNumber);

				if (!importerPK.IsEmpty)
				{
					result.B2_OH_Importer = importerPK;
				}

				if (CusStatementHeader.IsCARMDailyNoticeRegex.IsMatch(statementNumber))
				{
					var bn9 = importerBusinessNumber.Substring(0, 9);
					SetValue(result, CusStatementHeaderSchema.B2_ImporterCustomsID, bn9);

					if (importerBusinessNumber.Length == 15)
					{
						var rnNumber = importerBusinessNumber.Substring(9, 6);
						if (rnNumber.StartsWith("RM", StringComparison.InvariantCultureIgnoreCase))
						{
							SetValue(result, CusStatementHeaderSchema.B2_RMNumber, rnNumber.Substring(2, 4));
						}
					}
				}
				else
				{
					SetValue(result, CusStatementHeaderSchema.B2_ImporterCustomsID, importerBusinessNumber);
				}
			}

			return result;
		}

		public static void SetValue(BusinessObject obj, SchemaStringColumn schemaColumn, ZString value)
		{
			if (obj != null && schemaColumn != null)
			{
				obj[schemaColumn] = value.SubstringSafe(0, schemaColumn.MaxLength);
			}
		}

		public static void LocateDailyStatementHeadersIfNeed(CusStatementHeader header, string statementType, string importerBusinessNumber, ZDateTime startDate, ZDateTime endDate)
		{
			var query = new ZQuery();
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, statementType);
			query.AddToFilter(CusStatementHeaderSchema.B2_ImporterCustomsID, importerBusinessNumber);
			query.AddToFilter(CusStatementHeaderSchema.B2_IsMonthlyStatement, false);
			query.AddToFilter(CusStatementHeaderSchema.B2_B2_PeriodicStatement, null);
			query.AddToFilter(CusStatementHeaderSchema.B2_ProcessDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);
			query.AddToFilter(CusStatementHeaderSchema.B2_ProcessDate, SQLComparisonOperator.LessThanOrEqualTo, endDate);

			var dailyStatementHeaders = header.Factory.Load<CusStatementHeader>(query);
			dailyStatementHeaders.ForEach(c => c.B2_B2_PeriodicStatement = header.PK);
		}

		public static ZGuid FindImporter(BusinessObjectFactory factory, ZString importerBusinessNumber, ZString statementType)
		{
			bool isBroker = false;
			if (statementType == CusStatementHeaderTypes.Codes.Broker)
			{
				isBroker = true;
			}
			return TransactionBatchExtension.GetOrgsFromBN(importerBusinessNumber, ZString.Empty, factory, isBroker).FirstOrDefault()?.PK ?? ZGuid.Empty;
		}

		public static void UpdateDates(ZDateTime statementDate, ZDateTime accountingDate, bool isNotUnderReviewTransaction, IK84ReportAttachee k84Attachee)
		{
			if (k84Attachee != null)
			{
				if (isNotUnderReviewTransaction)
				{
					if (k84Attachee.StatementDate.IsEmpty)
					{
						k84Attachee.StatementDate = statementDate;
					}

					if (k84Attachee.AccountingDate.IsEmpty)
					{
						k84Attachee.AccountingDate = accountingDate;
					}

					if (k84Attachee.MessageType == JobMessageTypeList.Codes.ImportCopyforB2 || k84Attachee.MessageType == JobMessageTypeList.Codes.B2Adjustments)
					{
						if (k84Attachee.B2AcceptedDate.IsEmpty)
						{
							k84Attachee.B2AcceptedDate = accountingDate;
						}
					}
				}
				else
				{
					if (k84Attachee.MessageType == JobMessageTypeList.Codes.ImportCopyforB2 || k84Attachee.MessageType == JobMessageTypeList.Codes.B2Adjustments)
					{
						if (k84Attachee.ConfirmedDate.IsEmpty)
						{
							k84Attachee.ConfirmedDate = accountingDate;
						}
					}
				}
			}
		}

		public static void AddMessageInfosIfRequired(HtmlTableCreator parentTable, ZString messageEN, ZString messageFR)
		{
			if (!messageEN.IsEmpty || !messageFR.IsEmpty)
			{
				var table = new FieldValueTableInterpretation(false);
				table.Add(EnglishMessageToRecipient, messageEN);
				table.Add(FrenchMessageToRecipient, messageFR);

				parentTable.WriteRow(table.ToHtml());
			}
		}

		public static CusStatementLineGroup CreateLineGroupIfNeed(CusStatementHeader header, ZString importerBusinessNumber, ZDecimal refundAmount, ZDecimal paymentReceivedAmount)
		{
			CusStatementLineGroup lineGroup = null;

			if (!importerBusinessNumber.IsEmpty)
			{
				lineGroup = header.LineGroupCollection.FindOrCreate(importerBusinessNumber);

				lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.Refund, refundAmount);
				lineGroup.FinancialDetailCollection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalPaymentReceived, paymentReceivedAmount);
			}

			return lineGroup;
		}

		public static CusStatementLineGroup CreateLineGroupIfNeed(CusStatementHeader header, ZString importerBusinessNumber)
		{
			CusStatementLineGroup lineGroup = null;

			if (!importerBusinessNumber.IsEmpty)
			{
				lineGroup = header.LineGroupCollection.FindOrCreate(importerBusinessNumber);
			}

			return lineGroup;
		}

		public static void CreateStatementCharges(CusStatementLine statementLine, ZDecimal chargeAmount, ZString chargeType, ZString paymentParty)
		{
			if (!chargeAmount.IsEmpty)
			{
				var charge = statementLine.Charges.AddNew();
				charge.B4_ChargeType = chargeType;
				charge.B4_ChargeAmount = chargeAmount;
				charge.B4_PaymentParty = paymentParty;
			}
		}

		public static string EnglishMessageToRecipient => Res.GetString("9e4f21ab-489f-4731-a9c8-2d0aef1b8bc9", "Message to Recipient (English)");

		public static string FrenchMessageToRecipient => Res.GetString("45ddcd31-23d2-49ab-8e29-98816bf837af", "Message to Recipient (French)");

		public static ZGuid AcknowledgementEmailGroup => CACustomsDataRegistry.Instance.SendK84ReportNotificationsToGroup.Value;

		public static string MessageSender => Res.GetString("f59057e8-2370-403c-8b76-0c24afeb0d3d", "from the CBSA") + " ";
	}
}
