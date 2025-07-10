using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Netting;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Matching;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting
{
	public class NettingTransactionImporter : INettingTransactionImporter
	{
		public NettingTransactionImporter(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			this.Factory = factory;
			this.Logger = logger;
		}

		readonly BusinessObjectFactory Factory;
		readonly IXmlImportLogger Logger;

		IEDIMessage Message;

		public IKeysResult GetKeysForBlockingParallelImport(IEDIMessage message, ITopLevelDataObject dataObject)
		{
			/*
			 * This code is too hard to understand.
			 * I do not feel safe running ImportNettingTransactions in parallel.
			 */
			return new UniversalKeysResult(new[] { (nameof(NettingTransactionImporter), nameof(NettingTransactionImporter)) });
		}

		public bool ImportNettingTransaction(IEDIMessage message, ITopLevelDataObject dataObject)
		{
			NettingReceivableTransaction nettingReceivableTransaction = null;
			var result = ImportNettingTransaction(message, dataObject, ref nettingReceivableTransaction);
			var universalTransaction = dataObject as UniversalTransaction;
			if (result && universalTransaction.Ledger.Value == LedgerTypes.AccountsReceivable)
			{
				return new UniversalTransactionReExporter(Factory, Logger).ReExport(message, universalTransaction, nettingReceivableTransaction);
			}
			else
			{
				return result;
			}
		}

		public bool ImportNettingTransaction(IEDIMessage message, ITopLevelDataObject dataObject, ref NettingReceivableTransaction nettingReceivableTransaction)
		{
			var result = false;
			Message = message;

			var universalTransaction = dataObject as UniversalTransaction;
			try
			{
				using (var transactionManager = Connection.BeginTransactionWithManager())
				{
					if (universalTransaction.Ledger.HasValue)
					{
						if (universalTransaction.Ledger.Value == LedgerTypes.AccountsReceivable)
						{
							result = ImportAccountsReceivableTransaction(message, universalTransaction, ref nettingReceivableTransaction);
						}
						else if (universalTransaction.Ledger.Value == LedgerTypes.AccountsPayable)
						{
							result = ImportAccountsPayableTransaction(message, universalTransaction);
						}
						else
						{
							throw new ArgumentException("Unsupported transaction");
						}
					}

					if (result)
					{
						transactionManager.CommitTransaction();
					}
				}
			}
			catch (IncorrectDataSetupException ex)
			{
				LogError(ex);
				result = false;
			}
			catch (Exception ex) when (ex is MalformedUniversalXmlException || ex is ArgumentException)
			{
				LogError(ex);
				throw;
			}

			return result;

			void LogError(Exception ex) => Logger.Log(Enterprise.Integration.LogType.Error, ex.Message);
		}

		bool ImportAccountsReceivableTransaction(IEDIMessage message, UniversalTransaction universalTransaction, ref NettingReceivableTransaction nettingReceivableTransaction)
		{
			NettingOrganisation issuer = null;
			NettingOrganisation recipient = null;
			NettingReceivableTransaction arNettingTransaction = null;
			NettingPayableTransaction apNettingTransaction = null;

			try
			{
				GetIssuerAndRecipient(universalTransaction, ref issuer, ref recipient, isReceivable: true);

				if (universalTransaction.OSCurrency == null)
				{
					throw new MalformedUniversalXmlException(Res.GetString("17D53DA7-0F19-4441-A199-FED8EDE38934", "Currency is not set in Universal Transaction."));
				}

				if (NettingTransactionValidation(universalTransaction))
				{
					arNettingTransaction = NettingHelper.GetNettingReceivableTransaction(issuer.PK, recipient.PK, GetPrimaryInvoiceReference(universalTransaction), universalTransaction.OSCurrency.Code.GetValueOrDefault(), Factory);

					if (arNettingTransaction == null)
					{
						arNettingTransaction = Factory.New<NettingReceivableTransaction>();
					}
					else
					{
						if (IsTansactionAlreadyMatchedOrSettled(universalTransaction, arNettingTransaction))
						{
							return false;
						}
					}
					PopulateNettingTransaction(arNettingTransaction, message, universalTransaction, issuer, recipient);
					PurgeAndAddNewTransactionReferences(arNettingTransaction, universalTransaction);

					PurgeAndAddNewLinesAndLineReferences(arNettingTransaction, universalTransaction);

					apNettingTransaction = NettingHelper.GetNettingPayableTransaction(issuer.PK, recipient.PK, GetPrimaryInvoiceReference(universalTransaction), universalTransaction.OSCurrency.Code.GetValueOrDefault(), Factory);

					if (apNettingTransaction == null)
					{
						apNettingTransaction = CreatePayableNettingTransactionFromReceivable(arNettingTransaction);

						if (!arNettingTransaction.OriginalTransaction.IsEmpty && universalTransaction.OriginalReference != null)
						{
							var originalTransactionNumber = universalTransaction.OriginalReference.OriginalTransactionNumber.GetValueOrDefault();
							var originalAPTransaction = NettingHelper.GetNettingPayableTransaction(apNettingTransaction.IssuerPK, apNettingTransaction.RecipientPK, originalTransactionNumber, apNettingTransaction.Currency, Factory);
							apNettingTransaction.OriginalTransaction = originalAPTransaction != null ? originalAPTransaction.PK : ZGuid.Empty;
						}
					}

#if DEBUG
					ARNettingTransaction_ForTestOnly = arNettingTransaction;
					APNettingTransaction_ForTestOnly = apNettingTransaction;
#endif

					nettingReceivableTransaction = arNettingTransaction;
					return true;
				}
			}
			catch (IncorrectDataSetupException)
			{
				try
				{
					arNettingTransaction?.Delete();
					apNettingTransaction?.Delete();
				}
				catch { }

				throw;
			}

			return false;
		}

		bool NettingTransactionValidation(UniversalTransaction universalTransaction)
		{
			ZDecimal totalAmount = 0M;
			foreach (var postingJournal in universalTransaction.PostingJournalCollection)
			{
				var lineAmount = postingJournal.OSTotalAmount.GetValueOrDefault();
				totalAmount += lineAmount;
				continue;
			}

			return totalAmount != 0;
		}

		bool ImportAccountsPayableTransaction(IEDIMessage message, UniversalTransaction universalTransaction)
		{
			NettingOrganisation issuer = null;
			NettingOrganisation recipient = null;
			NettingPayableTransaction apNettingTransaction = null;

			try
			{
				GetIssuerAndRecipient(universalTransaction, ref issuer, ref recipient, isReceivable: false);

				if (NettingTransactionValidation(universalTransaction))
				{
					apNettingTransaction = NettingHelper.GetNettingPayableTransaction(issuer.PK, recipient.PK, GetPrimaryInvoiceReference(universalTransaction), universalTransaction.OSCurrency.Code.GetValueOrDefault(), Factory);

					if (apNettingTransaction == null)
					{
						apNettingTransaction = Factory.New<NettingPayableTransaction>();
					}
					else
					{
						if (IsTansactionAlreadyMatchedOrSettled(universalTransaction, apNettingTransaction))
						{
							return false;
						}
					}

#if DEBUG
					APNettingTransaction_ForTestOnly = apNettingTransaction;
#endif
					PopulateNettingTransaction(apNettingTransaction, message, universalTransaction, issuer, recipient);

					PurgeAndAddNewTransactionReferences(apNettingTransaction, universalTransaction);
					PurgeAndAddNewLinesAndLineReferences(apNettingTransaction, universalTransaction);

					return true;
				}
			}
			catch (IncorrectDataSetupException)
			{
				try
				{
					apNettingTransaction?.Delete();
				}
				catch { }

				throw;
			}

			return false;
		}

		bool IsTansactionAlreadyMatchedOrSettled(UniversalTransaction universalTransaction, INettingTransaction transaction)
		{
			return (transaction.ApprovalStatus == NettingTransactionApprovalStatus.Matched || transaction.ApprovalStatus == NettingTransactionApprovalStatus.Setteled)
											&& universalTransaction.OriginalReference == null;
		}

		DbConnection Connection
		{
			get { return connection ?? (connection = Db.Connection); }
		}
		DbConnection connection;

		NettingPayableTransaction CreatePayableNettingTransactionFromReceivable(NettingReceivableTransaction arNettingTransaction)
		{
			var apNettingTransaction = Factory.New<NettingPayableTransaction>();
			apNettingTransaction.NettingSystemPK = arNettingTransaction.NettingSystemPK;
			apNettingTransaction.NettingPeriodPK = NettingHelper.GetNettingPeriod(NettingSystem, arNettingTransaction.DueDate, isReceivable: false);
			apNettingTransaction.IssuerPK = arNettingTransaction.IssuerPK;
			apNettingTransaction.RecipientPK = arNettingTransaction.RecipientPK;

			apNettingTransaction.Date = arNettingTransaction.Date;
			apNettingTransaction.DueDate = arNettingTransaction.DueDate;
			apNettingTransaction.Amount = arNettingTransaction.Amount;
			apNettingTransaction.Currency = arNettingTransaction.Currency;

			apNettingTransaction.TransactionType = arNettingTransaction.TransactionType;
			apNettingTransaction.Reference = arNettingTransaction.Reference;
			apNettingTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Added;

			foreach (NettingReceivableTransactionRef arTransactionRef in arNettingTransaction.References)
			{
				var apTransactionRef = apNettingTransaction.AddNewTransactionReference();

				apTransactionRef.Type = arTransactionRef.Type;
				apTransactionRef.Reference = arTransactionRef.Reference;
			}

			foreach (NettingReceivableTransactionLine arLine in arNettingTransaction.Lines)
			{
				var apLine = apNettingTransaction.AddNewLine();
				apLine.JobReference = arLine.JobReference;
				apLine.Amount = arLine.Amount;
				apLine.TransactionCurrency = arLine.TransactionCurrency;
				apLine.IsApproved = false;

				foreach (NettingReceivableLineReference arLineRef in arLine.References)
				{
					var apLineRef = apLine.AddNewLineReference();

					apLineRef.Type = arLineRef.Type;
					apLineRef.Reference = arLineRef.Reference;
				}
			}

			return apNettingTransaction;
		}

#if DEBUG
		public BusinessObject ARNettingTransaction_ForTestOnly;
		public BusinessObject APNettingTransaction_ForTestOnly;

#endif
		NettingSystem NettingSystem => nettingSystem ?? (nettingSystem = NettingHelper.GetNettingSystem(Factory, Message.Interchange.EI_To));
		NettingSystem nettingSystem;

		void PopulateNettingTransaction(INettingTransaction nettingTransaction, IEDIMessage message, UniversalTransaction universalTransaction, NettingOrganisation issuer, NettingOrganisation recipient)
		{
			if (NettingSystem == null)
			{
				throw new IncorrectDataSetupException(Res.GetString("bfa1ad2c-2fd7-4cf0-8774-9dc02269baad", "No Netting System is setup for eHub ID: '{0}'", message.Interchange.EI_To));
			}
			nettingTransaction.NettingSystemPK = NettingSystem.PK;

			bool isReceivableTransaction = nettingTransaction is NettingReceivableTransaction;

			int multiplier = isReceivableTransaction ? 1 : -1;
			var nettingPeriod = NettingHelper.GetNettingPeriod(NettingSystem, universalTransaction.DueDate.GetValueOrDefault(), isReceivableTransaction);

			nettingTransaction.IssuerPK = issuer != null ? issuer.PK : ZGuid.Empty;
			nettingTransaction.RecipientPK = recipient != null ? recipient.PK : ZGuid.Empty;

			nettingTransaction.NettingPeriodPK = nettingPeriod;

			nettingTransaction.Date = universalTransaction.TransactionDate.GetValueOrDefault();
			nettingTransaction.DueDate = universalTransaction.DueDate.GetValueOrDefault();
			nettingTransaction.Amount = multiplier * universalTransaction.OSTotal.GetValueOrDefault();
			nettingTransaction.Currency = universalTransaction.OSCurrency.Code.GetValueOrDefault();
			nettingTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Approved;

			nettingTransaction.Reference = GetPrimaryInvoiceReference(universalTransaction);
			nettingTransaction.TransactionType = universalTransaction.TransactionType.GetValueOrDefault().ToString();

			nettingTransaction.OriginalTransaction = ZGuid.Empty;
			if (universalTransaction.OriginalReference != null)
			{
				INettingTransactionReference nettingTransactionRef = null;
				var originalTransactionNumber = universalTransaction.OriginalReference.OriginalTransactionNumber.GetValueOrDefault();

				if (!originalTransactionNumber.IsEmpty)
				{
					INettingTransaction originalTransaction = null;
					if (isReceivableTransaction)
					{
						originalTransaction = NettingHelper.GetNettingReceivableTransaction(nettingTransaction.IssuerPK, nettingTransaction.RecipientPK, originalTransactionNumber, nettingTransaction.Currency, Factory);
					}
					else
					{
						originalTransaction = NettingHelper.GetNettingPayableTransaction(nettingTransaction.IssuerPK, nettingTransaction.RecipientPK, originalTransactionNumber, nettingTransaction.Currency, Factory);
					}

					if (originalTransaction != null)
					{
						if (universalTransaction.IsCancelled.GetValueOrDefault())
						{
							if (!IsTransactionSettled(originalTransaction))
							{
								if (originalTransaction.ApprovalStatus == NettingTransactionApprovalStatus.Matched)
								{
									NettingHelper.UnmatchTransaction(Connection, nettingPeriod, originalTransaction, NettingTransactionApprovalStatus.Reversed);
								}
								else if (originalTransaction.ApprovalStatus == NettingTransactionApprovalStatus.Approved)
								{
									originalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Reversed;
								}

								nettingTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Reversed;
							}
						}
						else //transaction is amended
						{
							nettingTransactionRef = nettingTransaction.AddNewTransactionReference();
							nettingTransactionRef.Type = AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber;
							nettingTransactionRef.Reference = originalTransactionNumber;
						}

						nettingTransaction.OriginalTransaction = originalTransaction.PK;
					}
				}
			}
		}

		bool IsTransactionSettled(INettingTransaction nettingTransaction) => new[] { NettingTransactionApprovalStatus.Setteled, NettingTransactionApprovalStatus.SettledOutOfNetting }.Contains(nettingTransaction.ApprovalStatus.ToString());

		void GetIssuerAndRecipient(UniversalTransaction universalTransaction, ref NettingOrganisation issuer, ref NettingOrganisation recipient, bool isReceivable)
		{
			var initiatingParticipant = GetNettingParticipant(universalTransaction.BranchAddress, Res.GetString("b30d16a3-7704-4341-9025-3c0ef43a8a5c", "branch"));
			var receivingParticipant = GetNettingParticipant(universalTransaction.OrganizationAddress, Res.GetString("bb33bd35-1d74-42cd-b685-8f08f0ca21c5", "organization"));

			if (isReceivable)
			{
				issuer = initiatingParticipant;
				recipient = receivingParticipant;
			}
			else
			{
				issuer = receivingParticipant;
				recipient = initiatingParticipant;
			}

			#region SuppressResourceStringsCheckRegion

			var logMessage = FormattableString.Invariant($"{universalTransaction.Ledger} Netting Transaction {universalTransaction.Number}: Issuer {issuer?.Organisation?.OH_Code} and Recipient {recipient?.Organisation?.OH_Code}");
			Logger.Log(LogType.Information, logMessage);

			#endregion
		}

		NettingOrganisation GetNettingParticipant(OrganizationAddress orgAddress, ZString addressType)
		{
			if (orgAddress == null)
			{
				var exceptionMessage = Res.GetString("8dd0467a-1e91-4945-a8fd-d581bbfd0cca", "No address was provided for the {0} address in this Universal Transaction. Netting cannot continue.", addressType);
				throw new IncorrectDataSetupException(exceptionMessage);
			}

			var orgHeader = new OrganizationAddressMatcher().GetMatchingOrgHeader(orgAddress, Factory);
			if (orgHeader == null)
			{
				var exceptionMessage = Res.GetString("53b6056b-19c3-49fe-bafe-c7fbbc2a90c2", @"No active organization can be found for matching {0} address ({1}).
{2}, {3}, {4}, {5}, {6} ({7}).",
addressType,
orgAddress.OrganizationCode,
orgAddress.Address1,
orgAddress.City,
orgAddress.State?.Code,
orgAddress.Postcode,
orgAddress.Country?.Code,
orgAddress.Port?.Code);
				throw new IncorrectDataSetupException(exceptionMessage);
			}

			var nettingParticipant = NettingHelper.GetNettingOrganization(orgHeader.PK, Factory);
			if (nettingParticipant == null)
			{
				var exceptionMessage = Res.GetString("d0c08191-00b9-4166-9fc2-c291d78ec069", "No Netting Participant found for {0} ({1}).", addressType, orgHeader.OH_Code);
				throw new IncorrectDataSetupException(exceptionMessage);
			}

			#region SuppressResourceStringsCheckRegion

			var logMessage = FormattableString.Invariant($"Netting Participant {nettingParticipant?.Organisation?.OH_Code} matched for {addressType} address.");
			Logger.Log(LogType.Information, logMessage);

			#endregion

			return nettingParticipant;
		}

		void PurgeAndAddNewTransactionReferences(INettingTransaction nettingTransaction, UniversalTransaction universalTransaction)
		{
			nettingTransaction.DeleteTransactionReferences();

			var references = GetInvoiceAndConsolReferences(universalTransaction);

			foreach (var reference in references)
			{
				INettingTransactionReference nettingTransactionRef = nettingTransaction.AddNewTransactionReference();
				nettingTransactionRef.Type = reference.Item1;
				nettingTransactionRef.Reference = reference.Item2;
			}
		}

		List<Tuple<ZString, ZString>> GetInvoiceAndConsolReferences(UniversalTransaction universalTransaction)
		{
			ZString referenceType = ZString.Empty;
			ZString referenceValue = ZString.Empty;

			var headerReferences = new List<Tuple<ZString, ZString>>();

			referenceType = AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber;
			referenceValue = GetPrimaryInvoiceReference(universalTransaction);

			AddReferenceIfAlreadyNotExists(headerReferences, referenceType, referenceValue);

			if (!universalTransaction.JobInvoiceNumber.GetValueOrDefault().IsEmpty) //consol or shipment level invoices have JobInvoiceNumber
			{
				referenceType = AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber;
				referenceValue = universalTransaction.JobInvoiceNumber.GetValueOrDefault();

				AddReferenceIfAlreadyNotExists(headerReferences, referenceType, referenceValue);
			}

			if (universalTransaction.Job != null && universalTransaction.Job.Key.HasValue) //shipment level invoices does set Job.Key
			{
				referenceType = AccountingConstants.TransactionReferenceTypes.ShipmentNumber;
				referenceValue = universalTransaction.Job.Key.GetValueOrDefault();

				AddReferenceIfAlreadyNotExists(headerReferences, referenceType, referenceValue);
			}

			if (universalTransaction.ShipmentCollection != null)
			{
				//check if invoice is linked to multiple consols, if that is the case then job references will be more appropriate than header references
				var dataSourceKeys = (from s in universalTransaction.ShipmentCollection
									  where s.DataContext != null && s.DataContext.DataSourceCollection != null
									  from d in s.DataContext.DataSourceCollection
									  where d.Type.GetValueOrDefault() == Enum.GetName(typeof(DataContextType), DataContextType.ForwardingConsol)
									  select d.Key).Distinct();

				if (dataSourceKeys.Count() != 1)
				{
					return headerReferences;
				}

				AddConsolReferences(universalTransaction, headerReferences, dataSourceKeys.First().GetValueOrDefault());
			}

			return headerReferences;
		}

		void AddConsolReferences(UniversalTransaction universalTransaction, List<Tuple<ZString, ZString>> headerReferences, ZString consolNumber, bool addToJobReference = false)
		{
			ZString referenceType = string.Empty;
			ZString referenceValue = string.Empty;

			//consol number
			if (!consolNumber.IsEmpty)
			{
				referenceType = AccountingConstants.TransactionReferenceTypes.ConsolNumber;
				referenceValue = consolNumber;

				AddReferenceIfAlreadyNotExists(headerReferences, referenceType, referenceValue);
			}

			var consol = ExtractConsol(universalTransaction, consolNumber);

			if (consol != null)
			{
				//Vessel/Voyage number
				if (consol != null && consol.VesselName.HasValue && consol.VoyageFlightNo.HasValue)
				{
					referenceType = AccountingConstants.TransactionReferenceTypes.VesselVoyage;
					referenceValue = consol.VoyageFlightNo.GetValueOrDefault();

					AddReferenceIfAlreadyNotExists(headerReferences, referenceType, referenceValue);
				}

				//consol container number
				if (consol.ContainerCollection != null && consol.ContainerCollection.Any())
				{
					foreach (var container in consol.ContainerCollection)
					{
						if (container.ContainerNumber.HasValue)
						{
							referenceType = AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber;
							referenceValue = container.ContainerNumber.GetValueOrDefault();

							AddReferenceIfAlreadyNotExists(headerReferences, referenceType, referenceValue);
						}
					}
				}

				//master bill
				if (consol.WayBillType != null && consol.WayBillType.Code.HasValue && consol.WayBillNumber.HasValue)
				{
					if (consol.WayBillType.Code.GetValueOrDefault() == "MWB")
					{
						referenceType = AccountingConstants.TransactionReferenceTypes.MasterBill;
					}
					referenceValue = consol.WayBillNumber.GetValueOrDefault();

					AddReferenceIfAlreadyNotExists(headerReferences, referenceType, referenceValue);
				}

				if (!addToJobReference)
				{
					if (consol.BookingConfirmationReference.HasValue)
					{
						referenceType = AccountingConstants.TransactionReferenceTypes.CarrierBookingReference;
						referenceValue = consol.BookingConfirmationReference.GetValueOrDefault();

						AddReferenceIfAlreadyNotExists(headerReferences, referenceType, referenceValue);
					}

					if (consol.AgentsReference.HasValue)
					{
						referenceType = AccountingConstants.TransactionReferenceTypes.AgentReference;
						referenceValue = consol.AgentsReference.GetValueOrDefault();

						AddReferenceIfAlreadyNotExists(headerReferences, referenceType, referenceValue);
					}
				}
			}
		}

		Shipment ExtractConsol(UniversalTransaction universalTransaction, ZString consolNumber)
		{
			var consol = (from n in universalTransaction.ShipmentCollection
						  let dataSources = n.DataContext.DataSourceCollection
						  from d in dataSources
						  where (d.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingConsol) && d.Key.GetValueOrDefault() == consolNumber)
							  && !n.DataContext.DataSourceCollection.Any(x => x.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingShipment))
						  select n).FirstOrDefault();
			return consol;
		}

		List<Tuple<ZString, ZString>> GetJobReferences(UniversalTransaction universalTransaction, ZString jobNumber)
		{
			var jobReferences = new List<Tuple<ZString, ZString>>();
			if (universalTransaction.ShipmentCollection != null)
			{
				#region Consol references

				var dataContexts =
						(from s in universalTransaction.ShipmentCollection
						 where s.DataContext != null && s.DataContext.DataSourceCollection != null
						 from d in s.DataContext.DataSourceCollection
						 where d.Type.GetValueOrDefault() == Enum.GetName(typeof(DataContextType), DataContextType.ForwardingShipment)
								 && d.Key.GetValueOrDefault() == jobNumber
						 select s.DataContext);

				ZString consolNumber = ZString.Empty;
				foreach (var context in dataContexts)
				{
					foreach (var dataSource in context.DataSourceCollection)
					{
						if (dataSource.Type.GetValueOrDefault() == Enum.GetName(typeof(DataContextType), DataContextType.ForwardingConsol))
						{
							consolNumber = dataSource.Key.GetValueOrDefault();
							break;
						}
					}
				}

				AddConsolReferences(universalTransaction, jobReferences, consolNumber, true);

				#endregion

				#region Shipment references

				var shipments = from s in universalTransaction.ShipmentCollection
								where s.DataContext != null && s.DataContext.DataSourceCollection != null
								from d in s.DataContext.DataSourceCollection
								where d.Type.GetValueOrDefault() == Enum.GetName(typeof(DataContextType), DataContextType.ForwardingShipment)
									&& d.Key.GetValueOrDefault() == jobNumber
								select s;

				foreach (var shipment in shipments)
				{
					AddShipmentReferences(shipment, jobReferences, jobNumber);
				}
				#endregion

				#region Shipment references in case invoice was a consol level invoice

				var subShipment = from s in universalTransaction.ShipmentCollection
								  where s.SubShipmentCollection != null
								  from sub in s.SubShipmentCollection
								  where sub.DataContext != null && sub.DataContext.DataSourceCollection != null
								  from d in sub.DataContext.DataSourceCollection
								  where d.Type.GetValueOrDefault() == Enum.GetName(typeof(DataContextType), DataContextType.ForwardingShipment)
								   && d.Key.GetValueOrDefault() == jobNumber
								  select sub;

				foreach (var shipment in subShipment)
				{
					AddShipmentReferences(shipment, jobReferences, jobNumber);
				}

				#endregion
			}

			return jobReferences;
		}

		void AddShipmentReferences(Shipment shipment, List<Tuple<ZString, ZString>> jobReferences, ZString shipmentNumber)
		{
			ZString referenceType = ZString.Empty;
			ZString referenceValue = ZString.Empty;

			if (!shipmentNumber.IsEmpty)
			{
				referenceType = AccountingConstants.TransactionReferenceTypes.ShipmentNumber;
				referenceValue = shipmentNumber;

				AddReferenceIfAlreadyNotExists(jobReferences, referenceType, referenceValue);
			}

			AddShipmentReferences(shipment, jobReferences, ref referenceType, ref referenceValue);

			if (shipment.SubShipmentCollection != null)
			{
				foreach (var subShipment in shipment.SubShipmentCollection)
				{
					AddShipmentReferences(subShipment, jobReferences, ref referenceType, ref referenceValue);
				}
			}
		}

		void AddShipmentReferences(Shipment shipment, List<Tuple<ZString, ZString>> jobReferences, ref ZString referenceType, ref ZString referenceValue)
		{
			if (shipment.BookingConfirmationReference.HasValue)
			{
				referenceType = AccountingConstants.TransactionReferenceTypes.CarrierBookingReference;
				referenceValue = shipment.BookingConfirmationReference.GetValueOrDefault();
				AddReferenceIfAlreadyNotExists(jobReferences, referenceType, referenceValue);
			}

			if (shipment.AdditionalReferenceCollection != null && shipment.AdditionalReferenceCollection.Any())
			{
				AddAdditionalReferences(shipment, jobReferences, ref referenceType, ref referenceValue);
			}

			if (shipment.PackingLineCollection != null && shipment.PackingLineCollection.Any())
			{
				AddPackingContainerReference(shipment, jobReferences, ref referenceType, ref referenceValue);
			}

			if (shipment.WayBillType != null && shipment.WayBillType.Code.HasValue && shipment.WayBillNumber.HasValue)
			{
				AddHouseBillReference(shipment, jobReferences, ref referenceType, ref referenceValue);
			}

			if (shipment.LocalProcessing != null && shipment.LocalProcessing.OrderNumberCollection != null && shipment.LocalProcessing.OrderNumberCollection.Any())
			{
				AddOrderReference(shipment, jobReferences, ref referenceType, ref referenceValue);
			}
		}

		void AddOrderReference(Shipment shipment, List<Tuple<ZString, ZString>> jobReferences, ref ZString referenceType, ref ZString referenceValue)
		{
			foreach (var orderNumber in shipment.LocalProcessing.OrderNumberCollection)
			{
				if (orderNumber.OrderReference.HasValue)
				{
					referenceType = AccountingConstants.TransactionReferenceTypes.OrderReferences;
					referenceValue = orderNumber.OrderReference.GetValueOrDefault();

					AddReferenceIfAlreadyNotExists(jobReferences, referenceType, referenceValue);
				}
			}
		}

		void AddHouseBillReference(Shipment shipment, List<Tuple<ZString, ZString>> jobReferences, ref ZString referenceType, ref ZString referenceValue)
		{
			if (shipment.WayBillType.Code.GetValueOrDefault() == "HWB")
			{
				referenceType = AccountingConstants.TransactionReferenceTypes.HouseBill;
				referenceValue = shipment.WayBillNumber.GetValueOrDefault();

				AddReferenceIfAlreadyNotExists(jobReferences, referenceType, referenceValue);
			}
		}

		void AddPackingContainerReference(Shipment shipment, List<Tuple<ZString, ZString>> jobReferences, ref ZString referenceType, ref ZString referenceValue)
		{
			foreach (var packingLine in shipment.PackingLineCollection)
			{
				referenceType = AccountingConstants.TransactionReferenceTypes.PackedContainer;
				referenceValue = packingLine.ContainerNumber.GetValueOrDefault();

				AddReferenceIfAlreadyNotExists(jobReferences, referenceType, referenceValue);
			}
		}

		void AddAdditionalReferences(Shipment shipment, List<Tuple<ZString, ZString>> jobReferences, ref ZString referenceType, ref ZString referenceValue)
		{
			foreach (var additionalReference in shipment.AdditionalReferenceCollection)
			{
				if (additionalReference != null && additionalReference.Type.Code.HasValue && additionalReference.ReferenceNumber.HasValue)
				{
					if (additionalReference.Type.Code.GetValueOrDefault() == "OAG")
					{
						referenceType = AccountingConstants.TransactionReferenceTypes.AgentReference;
					}
					referenceValue = additionalReference.ReferenceNumber.GetValueOrDefault();

					AddReferenceIfAlreadyNotExists(jobReferences, referenceType, referenceValue);
				}
			}
		}

		void AddReferenceIfAlreadyNotExists(List<Tuple<ZString, ZString>> referenceList, ZString referenceType, ZString referenceValue)
		{
			if (!referenceType.IsEmpty && !referenceValue.IsEmpty)
			{
				var keyValuePair = new Tuple<ZString, ZString>(referenceType, referenceValue);
				if (!referenceList.Contains(keyValuePair))
				{
					referenceList.Add(new Tuple<ZString, ZString>(referenceType, referenceValue));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "get job number from description, for collect invoices job number is not populated in Job.Key rather can be found in the description!")]
		void PurgeAndAddNewLinesAndLineReferences(INettingTransaction nettingTransaction, UniversalTransaction universalTransaction)
		{
			nettingTransaction.DeleteLines();

			int multiplier = 1;
			bool isApproved = false;
			foreach (var postingJournal in universalTransaction.PostingJournalCollection)
			{
				var lineAmount = postingJournal.OSTotalAmount.GetValueOrDefault();
				if (lineAmount == 0M)
				{
					continue;
				}
				var jobReference = postingJournal.Job != null ? postingJournal.Job.Key.GetValueOrDefault() : ZString.Empty;
				if (jobReference.IsEmpty)
				{
					if (postingJournal.Description.HasValue)
					{
						var token = postingJournal.Description.Value.ToUpper().ToString().Split(new[] { "JOB NUMBER:" }, StringSplitOptions.None);
						if (token.Length > 1)
						{
							jobReference = token[1].Trim();
						}
					}

					if (jobReference.IsEmpty)
					{
						continue;
					}
				}

				INettingTransactionLine nettingLine = nettingTransaction.AddNewLine();
				if (nettingTransaction is NettingReceivableTransaction)
				{
					isApproved = false;
				}
				else if (nettingTransaction is NettingPayableTransaction)
				{
					multiplier = -1;
					isApproved = true;
				}
				else
				{
					throw new ArgumentException("Netting Transaction can only be either AR or AP.");
				}

				nettingLine.JobReference = jobReference;
				nettingLine.Amount = multiplier * lineAmount;
				nettingLine.TransactionCurrency = postingJournal.OSCurrency != null ? postingJournal.OSCurrency.Code.GetValueOrDefault() : ZString.Empty;
				nettingLine.IsApproved = isApproved;

				AddNettingLineReference(universalTransaction, nettingLine, jobReference);
			}
		}

		void AddNettingLineReference(UniversalTransaction universalTransaction, INettingTransactionLine nettingLine, ZString jobNumber)
		{
			var jobReferences = GetJobReferences(universalTransaction, jobNumber);
			foreach (var jobRef in jobReferences)
			{
				INettingTransactionLineReference nettingLineRef = nettingLine.AddNewLineReference();
				nettingLineRef.Type = jobRef.Item1;
				nettingLineRef.Reference = jobRef.Item2;
			}
		}

		ZString GetPrimaryInvoiceReference(UniversalTransaction universalTransaction)
		{
			var result = ZString.Empty;
			if (universalTransaction.Number.HasValue)
			{
				result = universalTransaction.Number.GetValueOrDefault();
			}

			return result;
		}
	}
}
