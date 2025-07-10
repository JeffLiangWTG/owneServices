using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiBillingTransactionFlattenedDataTransferProcessor : DataTransferProcessor
	{
		public EdiBillingTransactionFlattenedDataTransferProcessor(EdiBillingTransactionImportInfo flattenedCollectionInfo)
		{
			FlattenedCollection = (EdiBillingTransactionFlattenedCollection)((IImportCollectionInfo)flattenedCollectionInfo).Collection;
			ServerAddress = eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.Value;
			LicenceCode = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			Password = ObjectFactory.Get<IProductRegistration>()?.Key.Password ?? "";
		}

		public override void Import()
		{
			OnProgressChanged(1, $"Validating Billing Transactions... {FlattenedCollection.Count}");
			var lineNumber = 0;
			FlattenedCollection.OfType<EdiBillingTransactionFlattened>().ForEach(x => x.LineNumber = ++lineNumber);
			FlattenedCollection.RunPreSaveValidation();
			if (FlattenedCollection.HasErrors())
			{
				return;
			}

			var transactions = FlattenedCollection.OfType<EdiBillingTransactionFlattened>().ToArray();
			SendTransactions(transactions);
		}

		void SendTransactions(IEnumerable<EdiBillingTransactionFlattened> transactions)
		{
			var transactionsProcessedCount = 0;
			var transactionsCount = transactions.Count();
			foreach (var currentBatch in transactions.GroupBy(x => x.LineNumber / BatchSize))
			{
				transactionsProcessedCount += currentBatch.Count();
				OnProgressChanged(transactionsProcessedCount * 100 / transactionsCount, $"Sending Billing Transactions... {transactionsProcessedCount} / {transactionsCount}");
				SendTransactionsCore(currentBatch);
				Sleep(100);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		protected virtual void Sleep(int millisecondsTimeout) => System.Threading.Thread.Sleep(millisecondsTimeout);

		public override void Rollback()
		{
		}

		public readonly EdiBillingTransactionFlattenedCollection FlattenedCollection;
		const int BatchSize = 100;

		#region eHub

		void SendTransactionsCore(IEnumerable<EdiBillingTransactionFlattened> transactions)
		{
			using (var adapter = GetNewAdapter())
			{
				var messages = transactions.Select(x =>
				{
					var transaction = new BillingTransaction()
					{
						Category = x.Category,
						PriceItemCode = x.PriceItemCode,
						BillableCount = x.BillableCount,
						ReportingSource = x.ReportingSource,
						ServiceOccuredUTC = x.ServiceOccuredUTC.ToDateTime(),
						ClientID = DefaultEmptyClientID,
						Reference1 = x.Reference1,
						Branch = x.Branch,
						ClientNumber = x.ClientNumber,
						ClientStaffCode = x.ClientStaffCode,
						MessageTrackingID = x.MessageTrackingID.IsValid ? x.MessageTrackingID.ToString() : "",
						Reference2 = x.Reference2,
						Reference3 = x.Reference3,
						Reference4 = x.Reference4,
						Reference5 = x.Reference5,
						Version = x.Version,
					};

					var transactionStream = new MemoryStream(Encoding.UTF8.GetBytes(BillingManager.GenerateTransactionXml(transaction)));
					return new eHubMessage(x.PK.ToGuid(), LicenceCode, RecipientID, CargoWise.eHub.Common.MessageSchemaType.Xml, ApplicationCode, SchemaName, transactionStream);
				}
				).ToArray();

				messages.ForEach(x => adapter.Outbox.AddMessage(x));
				adapter.SendMessages();
			}
		}

		protected virtual IeHubAdapter GetNewAdapter() => new eHubAdapter(ServerAddress, LicenceCode, Password);

		readonly string ServerAddress;
		readonly string LicenceCode;
		readonly string Password;
		const string ApplicationCode = "SCV";
		const string RecipientID = "eHubASService";
		const string SchemaName = "http://www.edi.com.au/EnterpriseService/#Billing_1.3";
		const string DefaultEmptyClientID = "?????????";

		#endregion
	}
}
