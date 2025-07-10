using System;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using UniversalTransactionBatch = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionBatch;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	public class GlobalEInvoicingBuilder : IGlobalElectronicInvoiceBuilder
	{
		UniversalTransactionBatch Batch { get; }
		string BatchNumber { get; }
		string MessagingSystem { get; }
		string MessageType { get; }
		ICountryEInvoicingObjectFactory CountryFactory { get; }
		ITransactionBatchDataLoader DataLoader { get; }
		Func<INotifications> LoggerCreator { get; }

		public GlobalEInvoicingBuilder(
			ICountryEInvoicingObjectFactory countryFactory,
			string batchNumber,
			UniversalTransactionBatch universalTransactionBatch,
			Func<INotifications> loggerCreator,
			ITransactionBatchDataLoader dataLoader = null)
		{
			Argument.NotNull(universalTransactionBatch, "universalTransactionBatch");
			Argument.NotNull(loggerCreator, "loggerCreater");
			Argument.NotNullOrEmpty(batchNumber, "batchNumber");

			BatchNumber = batchNumber;
			Batch = universalTransactionBatch;
			CountryFactory = countryFactory;
			MessagingSystem = CountryFactory.CountryCode + (NoResString)" Electronic invoicing system"; // Constant string used in XML file. Not a user visible text.
			MessageType = "REQ"; // Constant string used in XML file. Not a user visible text.
			LoggerCreator = loggerCreator;

			DataLoader = dataLoader ?? new TransactionBatchDataLoader();
		}

		(GlobalElectronicInvoicing EInvoice, INotifications ValidationErrors, INotifications ValidationWarnings) IGlobalElectronicInvoiceBuilder.Create()
		{
			var notifications = LoggerCreator.Invoke();
			var warnings = LoggerCreator.Invoke();

			var (branch, loaderError) = DataLoader.LoadBranchAndCompany(Batch);
			if (!string.IsNullOrEmpty(loaderError))
			{
				notifications.AddError(loaderError);
				return (null, notifications, warnings);
			}

			AccEInvoicingBatch eInvoicingBatch = null;
			if (branch?.Company != null)
			{
				var loaderResult = DataLoader.LoadAccBatch(branch?.Company, BatchNumber);
				eInvoicingBatch = loaderResult.accBatch;

				if (!string.IsNullOrEmpty(loaderResult.error))
				{
					notifications.AddError(loaderResult.error);
				}
			}

			var eInvoice = CreateGEIObject(branch, eInvoicingBatch, warnings);
			WritePayload(eInvoice, eInvoicingBatch, notifications, warnings);
			SetUniversalTransaction(eInvoice);

			return (eInvoice, notifications, warnings);
		}

		GlobalElectronicInvoicing CreateGEIObject(GlbBranch branch, AccEInvoicingBatch eInvoicingBatch, INotifications warnings)
		{
			var messageType = CountryFactory.GetMessageType(Batch, eInvoicingBatch);
			if (messageType.IsEmpty)
			{
				messageType = MessageType;
			}

			var geiBatchRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest
			{
				MessagingSystem = MessagingSystem,
				BatchNumber = BatchNumber,
				MessageType = messageType,
				BranchCode = branch?.GB_Code ?? ZString.Empty,
				CompanyCode = branch?.Company?.GC_Code ?? ZString.Empty,
				IsProductionSystem = GEIMessageHelper.GetIsProductionSystem(branch.Company),
				IsProductionSystemSpecified = true,
				Credentials = LoadCredentials(branch),
				AdditionalDataItems = LoadAdditionalHeaderDataItems(eInvoicingBatch, branch, Batch, CountryFactory, warnings),
			};
			CountryFactory.UpdateGEIBatchRequest(eInvoicingBatch, geiBatchRequest);

			return new GlobalElectronicInvoicing
			{
				Header = new GlobalElectronicInvoicingHeader { ElectronicInvoiceBatchRequest = geiBatchRequest },
				TransactionBatchSpecified = false,
			};
		}

		GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestAdditionalDataItemCollection LoadAdditionalHeaderDataItems(AccEInvoicingBatch eInvoicingBatch, GlbBranch branch, UniversalTransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory, INotifications warnings)
		{
			var additionalDataItemsProvider = CountryFactory.GetIAdditionalDataItemsProvider();
			if (additionalDataItemsProvider != null)
			{
				return additionalDataItemsProvider.GetAdditionalHeaderDataItems(eInvoicingBatch, branch, batch, countryFactory, warnings);
			}

			return null;
		}

		GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialCollection LoadCredentials(GlbBranch branch)
		{
			var result = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredentialCollection();

			var credentialsLoader = CountryFactory.GetCredentialsLoader();
			if (credentialsLoader != null)
			{
				var credentials = credentialsLoader.LoadForGEIRequest(branch, Batch, CountryFactory) ?? Enumerable.Empty<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>();
				foreach (var c in credentials)
				{
					result.Add(c);
				}
			}

			return result;
		}

		void WritePayload(GlobalElectronicInvoicing eInvoice, AccEInvoicingBatch accBatch, INotifications notifications, INotifications warnings)
		{
			var payloadWriter = CountryFactory.GetTransactionBatchToPayloadWriter();
			if (payloadWriter != null)
			{
				var messageType = eInvoice.Header.ElectronicInvoiceBatchRequest.MessageType;
				using (var payloadStream = new MemoryStream())
				{
					payloadStream.Position = 0;
					payloadWriter.WritePayloadToStream(Batch, payloadStream, messageType, accBatch, notifications, warnings);
					payloadWriter.GetPayloadValidation(messageType)?.Validate(payloadStream, notifications, warnings);
					eInvoice.Payload = Convert.ToBase64String(payloadStream.ToArray());
				}
			}
		}

		void SetUniversalTransaction(GlobalElectronicInvoicing eInvoice)
		{
			var strategy = CountryFactory.GEIMessageShouldIncludeUniversalTransaction(Batch, eInvoice.Header.ElectronicInvoiceBatchRequest);
			switch (strategy)
			{
				case IncludeUniversalTransactionStrategy.NoTransaction:
					break;
				case IncludeUniversalTransactionStrategy.SingleTransaction:
					CountryFactory.ModifyUniversalTransactionBeforeGEI(Batch, eInvoice.Header.ElectronicInvoiceBatchRequest);
					SetSingleUniveralTransactionPerBatch(eInvoice, Batch);
					break;
				case IncludeUniversalTransactionStrategy.BatchedTransactions:
					CountryFactory.ModifyUniversalTransactionBeforeGEI(Batch, eInvoice.Header.ElectronicInvoiceBatchRequest);
					SetBatchedUniversalTransactions(eInvoice, Batch);
					break;
			}
		}

		static void SetBatchedUniversalTransactions(GlobalElectronicInvoicing eInvoice, UniversalTransactionBatch batch)
		{
			var encodedTransactions = batch.TransactionCollection
				.Select(t => t.ToBase64EncodedXmlFragment())
				.ToArray();
			eInvoice.TransactionBatch = new GlobalElectronicInvoicingTransactionBatch
			{
				Transactions = encodedTransactions,
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception message")]
		internal const string ExceptionMessageForSingleTransactionOnly = "Each transaction batch must have exactly one transaction when setting Universal Transaction. Batches with many transactions are not supported.";
		static void SetSingleUniveralTransactionPerBatch(GlobalElectronicInvoicing eInvoice, UniversalTransactionBatch batch)
		{
			if (batch.TransactionCollection?.Count != 1)
			{
				throw new ArgumentException(ExceptionMessageForSingleTransactionOnly);
			}

			eInvoice.Transaction = batch.TransactionCollection[0].ToBase64EncodedXmlFragment();
		}

		public static void RemoveShipmentCollection(UniversalTransactionBatch batch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
		{
			if (batch == null)
			{
				return;
			}

			foreach (var transaction in batch.TransactionCollection.Where(t => t.ShipmentCollection != null))
			{
				// Depending on how the Universal Batch / Transactions are configured, SetShipmentCollection() may have no effect, but Clear() is close enough.
				transaction.ShipmentCollection.Clear();
				transaction.SetShipmentCollection(() => null);
			}
		}
	}
}
