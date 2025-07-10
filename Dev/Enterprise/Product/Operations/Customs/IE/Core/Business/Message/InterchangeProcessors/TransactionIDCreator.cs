using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.TransactionIDResponse;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	class TransactionIDCreator : InboundMessageCreator<TransactionIdResponse>
	{
		internal TransactionIDCreator(LoggingInformation logger, TransactionIDManager.SendErrorNotificationDelegate sendErrorNotification, int noOfDaysOld, string transactionType, IList<ZGuid> transactionNumberPKs = null)
			: base(logger)
		{
			this.sendErrorNotification = sendErrorNotification;
			this.noOfDaysOld = noOfDaysOld;
			this.transactionType = transactionType;
			this.transactionNumberPKs = transactionNumberPKs;
		}

		public static ZQuery GetNonExpiredTransactionNumberQuery(int noOfDays, string type)
		{
			var query = new ZQuery(CusTransactionNumberSchema.TN_Type, type);
			query.AddToFilter(CusTransactionNumberSchema.TN_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddDays(-noOfDays));
			return query;
		}

		readonly TransactionIDManager.SendErrorNotificationDelegate sendErrorNotification;
		readonly int noOfDaysOld;
		readonly string transactionType;
		readonly IList<ZGuid> transactionNumberPKs;

		protected override void CreateMessagesForInterchange(EDIInterchange interchange, string elementName, string xmlBody)
		{
			if (elementName == nameof(CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement.MessageAcknowledgement)
				&& IEXmlObjectSerializer.Deserialize<CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement.MessageAcknowledgement>(new StringReader(xmlBody)) is CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement.MessageAcknowledgement messageAcknowledgement)
			{
				var errorCode = messageAcknowledgement.ErrorCode;
				var revenueErrorsList = Universal.RefCusCodeListTypes.GetCachedList(
					interchange.Factory,
					Core.Constants.CountryCodes.Ireland,
					Messaging.UniversalReferenceConstants.RefCusCodeListTypes.RevenueErrorType,
					interchange.EI_SystemCreateTimeUtc,
					includeParentDataGrouping: false
				);
				SetToErrorAndLog(interchange, string.Format((NoResString)"Error submitting transaction request. Error Code: {0} - {1}", errorCode, revenueErrorsList.GetDescriptionFromCode(errorCode)));
			}
			else
			{
				base.CreateMessagesForInterchange(interchange, elementName, xmlBody);
			}
		}

		protected override void CreateMessagesForInterchange(EDIInterchange interchange, TransactionIdResponse response)
		{
			if (response.TransactionsSpecified)
			{
				var existingBlanksRecords = GetBlankRecords(interchange);
				if (existingBlanksRecords.Length > 0)
				{
					var queue = new Queue<CusTransactionNumber>(existingBlanksRecords);
					foreach (var transactionID in response.Transactions)
					{
						var transactionNumber = queue.Dequeue();
						if (transactionNumber == null)
						{
							break;
						}
						transactionNumber.TN_TransactionReference = transactionID;
						transactionNumberPKs?.Add(transactionNumber.PK);
						if (queue.Count == 0)
						{
							break;
						}
					}
				}
			}
		}

		CusTransactionNumber[] GetBlankRecords(EDIInterchange interchange)
		{
			CusTransactionNumber[] result = null;
			if (interchange.Company is GlbCompany company)
			{
				var trackingReference = interchange.EI_SessionGUID.ToString();
				var query = GetNonExpiredTransactionNumberQuery(noOfDaysOld, transactionType);
				query.AddToFilter(CusTransactionNumberSchema.TN_GC_Company, company.PK);
				query.AddToFilter(CusTransactionNumberSchema.TN_TrackingReference, trackingReference);
				query.AddToFilter(CusTransactionNumberSchema.TN_TransactionReference, ZString.Empty);
				result = interchange.Factory.Load<CusTransactionNumber>(query);
			}
			return result ?? Array.Empty<CusTransactionNumber>();
		}

		protected override string GetErrorLog(Exception e) => (NoResString)"Error Processing Transaction ID Request Response.\r\n" + e.Message;

		protected override void SetToErrorAndLog(EDIInterchange interchange, string errorReportLog)
		{
			base.SetToErrorAndLog(interchange, errorReportLog);
			GetBlankRecords(interchange).DeleteAll();
			if (interchange.Branch is GlbBranch branch)
			{
				sendErrorNotification?.Invoke(branch.GB_GC, branch.PK,
					Res.GetString("{8CF2EC9F-B52A-4CCC-98BB-EEC867875A51}", "TID Interchange #{0} Set To '{1}'", interchange.EI_InterchangeNum, EDIInterchange.Status.Error),
					Res.GetString("{765D5AA7-5B1A-40AB-AACE-7FBA927970C4}", "The following error occurred: {0}", errorReportLog));
			}
		}
	}
}
