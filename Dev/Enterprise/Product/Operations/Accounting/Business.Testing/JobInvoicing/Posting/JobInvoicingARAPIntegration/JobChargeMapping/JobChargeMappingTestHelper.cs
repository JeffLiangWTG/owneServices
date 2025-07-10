using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Export;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public static class JobChargeMappingTestHelper
	{
		public static TransactionInfo CreateUniversalTransaction(string jobNumber, string chargePK)
		{
			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal>());
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			universalShipment.DataContext = DataContextFactory.New();
			universalShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, jobNumber);
			universalShipment.WayBillNumber = jobNumber;
			universalTransaction.ShipmentCollection.Add(universalShipment);

			var universalLine = CreateUniversalLine(chargePK, jobNumber: jobNumber);
			universalTransaction.PostingJournalCollection.Add(universalLine);

			return universalTransaction;
		}

		public static PostingJournal CreateUniversalLine(string primaryKey = null, string displaySequence = null, string jobNumber = null, string consolNumber = null)
		{
			var universalLine = new PostingJournal();

			if (!string.IsNullOrEmpty(jobNumber))
			{
				universalLine.Job = new EntityReference();
				universalLine.Job.Key = jobNumber;
				universalLine.Job.Type = AccountingDataTransferConstants.DataContextTypeString.Job;
			}

			if (!string.IsNullOrEmpty(consolNumber))
			{
				universalLine.CostSource = new EntityReference();
				universalLine.CostSource.Key = consolNumber;
				universalLine.CostSource.Type = nameof(DataContextType.ForwardingConsol);
			}

			var matchingCriteriaCollection = new List<MatchingCriteria>();

			if (!primaryKey.IsNullOrEmpty())
			{
				matchingCriteriaCollection.Add(CreateMatchingCriteria("PrimaryKey", primaryKey));
			}

			if (!displaySequence.IsNullOrEmpty())
			{
				matchingCriteriaCollection.Add(CreateMatchingCriteria("DisplaySequence", displaySequence));
			}

			universalLine.ImportMetaData = new ImportMetaData(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Instruction = InstructionType.UpdateAndInsertIfNotFound
			};

			universalLine.ImportMetaData.SetMatchingCriteriaCollection(() => matchingCriteriaCollection);

			return universalLine;
		}

		public static MatchingCriteria CreateMatchingCriteria(string filedName, string value = "")
		{
			return new MatchingCriteria()
			{
				FieldName = filedName,
				Value = value
			};
		}

		public static TransactionImportAdditionalInfoProvider RegisterAdditionalInfoProvider(BusinessObjectFactory factory)
		{
			var infoProvider = new TransactionImportAdditionalInfoProvider();
			factory.ServiceContainer.RemoveService<TransactionImportAdditionalInfoProvider>();
			factory.ServiceContainer.AddService<TransactionImportAdditionalInfoProvider>(infoProvider);
			return infoProvider;
		}
	}
}
