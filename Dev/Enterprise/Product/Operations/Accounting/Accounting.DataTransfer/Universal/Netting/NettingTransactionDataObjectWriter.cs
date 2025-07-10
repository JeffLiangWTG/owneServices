using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Export;
using Enterprise.Accounting.Netting;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting
{
	public class NettingTransactionDataObjectWriter : TopLevelDataObjectWriter<NettingReceivableTransaction, UniversalTransaction>
	{
		public NettingTransactionDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalTransaction;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.NettingTransaction;
		}

		protected override void PopulateDataObject(NettingReceivableTransaction sourceBO, UniversalTransaction invoice)
		{
			invoice.Ledger = "AP";
			invoice.TransactionType = new TransactionTypeConverter().ToEnumValue(sourceBO.NRT_Amount > 0 ? TransactionTypes.Invoice : TransactionTypes.CreditNote);
			invoice.DueDate = sourceBO.NRT_DueDate;
			invoice.TransactionDate = invoice.PostDate = sourceBO.NRT_Date;
			invoice.OSExGSTVATAmount = -1 * sourceBO.NRT_Amount;
			invoice.OSGSTVATAmount = 0;
			invoice.OSTotal = -1 * sourceBO.NRT_Amount;

			invoice.OSCurrency = new Currency();
			invoice.OSCurrency.Code = sourceBO.NRT_RX_NKInvoiceCurrency;

			invoice.Number = sourceBO.NRT_Reference;

			invoice.CreateTime = sourceBO.NRT_SystemCreateTimeUtc;
			invoice.CreateUser = new StaffUsingAttributes() { Code = sourceBO.NRT_SystemCreateUser };

			PopulateInvoiceJob(sourceBO, invoice);
			PopulateJobInvoiceReference(sourceBO, invoice);
			PopulateOrganizationAddress(sourceBO, invoice);
			PopulatePostingJournals(sourceBO, invoice);
			PopulateShipments(sourceBO, invoice);
		}

		void PopulateShipments(NettingReceivableTransaction sourceBO, UniversalTransaction invoice)
		{
			var tempFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			Dictionary<ZString, ForwardingConsol> consols = new Dictionary<ZString, ForwardingConsol>();

			ForwardingConsol consol = null;
			if (sourceBO.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolNumber))
			{
				consol = tempFactory.New<ForwardingConsol>();

				consol.JK_UniqueConsignRef = sourceBO.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolNumber);
				consol.JK_MasterBillNum = sourceBO.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.MasterBill) ? sourceBO.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.MasterBill) : ZString.Empty;
				consol.JK_BookingReference = sourceBO.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference) ? sourceBO.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference) : ZString.Empty;
				consol.JK_AgentsReference = sourceBO.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.AgentReference) ? sourceBO.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.AgentReference) : ZString.Empty;

				consols[consol.JK_UniqueConsignRef] = consol;
			}

			var uniqueJobs = from n in sourceBO.Lines.Cast<NettingReceivableTransactionLine>()
							 group n by n.NRL_PrimaryJobReference into g
							 select g;

			foreach (var uniqueJob in uniqueJobs)
			{
				consol = null;
				NettingReceivableTransactionLine job = uniqueJob.First();

				ForwardingShipment shipment = null;
				var consolNumber = job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolNumber) ? job.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolNumber) : ZString.Empty;

				if (!consolNumber.IsEmpty)
				{
					if (!consols.ContainsKey(consolNumber))
					{
						consol = tempFactory.New<ForwardingConsol>();
						consol.JK_UniqueConsignRef = job.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolNumber);
						consol.JK_MasterBillNum = job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.MasterBill) ? job.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.MasterBill) : ZString.Empty;

						consols[consol.JK_UniqueConsignRef] = consol;
					}
					else
					{
						consol = consols[consolNumber];
					}

					shipment = consol.Shipments.AddNew();
				}
				else
				{
					shipment = tempFactory.New<ForwardingShipment>();
				}

				shipment.JS_UniqueConsignRef = job.NRL_PrimaryJobReference.Substring(0, 20);
				shipment.JS_HouseBill = job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.HouseBill)
					? job.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.HouseBill) : ZString.Empty;

				PopulateAdditionalReferencesAndOrderReference(job, shipment);

				if (consol != null && consol.Shipments.Contains(shipment.PK))
				{
					PopulateContainerPack(consol, job, shipment);
				}

				AddUniversalShipment(invoice, shipment);
			}

			if (consol != null)
			{
				if (sourceBO.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber))
				{
					foreach (var reference in sourceBO.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber))
					{
						if (!consol.Containers.Cast<ForwardingContainer>().Any(x => x.JC_ContainerNum == reference))
						{
							var container = consol.Containers.AddNew();
							container.JC_ContainerNum = reference;
						}
					}
				}

				AddUniversalShipment(invoice, consol);
			}

			GCWrapper.ReclaimMemory(ref tempFactory);
		}

		void PopulateContainerPack(ForwardingConsol consol, NettingReceivableTransactionLine job, ForwardingShipment shipment)
		{
			if (job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber))
			{
				foreach (var containerNumber in job.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber))
				{
					ForwardingContainer container = null;
					if (!consol.Containers.Cast<ForwardingContainer>().Any(x => x.JC_ContainerNum == containerNumber))
					{
						container = consol.Containers.AddNew();
						container.JC_ContainerNum = containerNumber;
					}
					else
					{
						container = consol.Containers.Cast<ForwardingContainer>().First(x => x.JC_ContainerNum == containerNumber);
					}

					if (job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.PackedContainer))
					{
						var packedContainer = shipment.OuterPackLines.AddNew();
						packedContainer.JL_RefNumber = job.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.PackedContainer);
					}
				}
			}
		}

		void PopulateAdditionalReferencesAndOrderReference(NettingReceivableTransactionLine job, ForwardingShipment shipment)
		{
			if (job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference))
			{
				var carrierBookingReference = job.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference).First();
				shipment.JS_BookingReference = carrierBookingReference;
			}

			if (job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.AgentReference))
			{
				foreach (var reference in job.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.AgentReference))
				{
					var agentReference = shipment.Numbers.AddNew();
					agentReference.CE_EntryType = "OAG";
					agentReference.CE_EntryNum = reference;
					agentReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
				}
			}

			if (job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.OrderReferences))
			{
				foreach (var reference in job.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.OrderReferences))
				{
					var order = shipment.AttachedOrders.AddNew();
					order.JD_OrderNumber = reference;
				}
			}
		}

		void AddUniversalShipment(UniversalTransaction dataObject, BusinessObject consumer)
		{
			UniversalShipment shipment = GetUniversalShipmentIfAvailable(consumer);

			if (shipment != null)
			{
				if (dataObject.ShipmentCollection != null || dataObject.SetShipmentCollection(() => new List<UniversalShipment>()))
				{
					dataObject.ShipmentCollection.Add(shipment);
				}
			}
		}

		UniversalShipment GetUniversalShipmentIfAvailable(BusinessObject jobHeaderParentBO)
		{
			var dataContextManager = jobHeaderParentBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			if (dataContextManager != null && dataContextManager.ManagesShipments)
			{
				using (writeManager.UseNewListForDuplicatePKCheck())
				{
					var shipmentDataObjectWriter = dataContextManager.GetShipmentDataObjectWriter(writeManager);
					var universalShipment = shipmentDataObjectWriter.GetDataObject(jobHeaderParentBO);
					return (UniversalShipment)universalShipment;
				}
			}

			return null;
		}

		void PopulatePostingJournals(NettingReceivableTransaction sourceBO, UniversalTransaction invoice)
		{
			invoice.SetPostingJournalCollection(() =>
			{
				var result = new List<PostingJournal>();

				PostingJournal postingJournal = null;

				if (sourceBO.Lines.Count == 0)
				{
					postingJournal = new PostingJournal(writeManager.WriterStrategy);
					result.Add(postingJournal);
				}

				foreach (NettingReceivableTransactionLine line in sourceBO.Lines)
				{
					postingJournal = new PostingJournal(writeManager.WriterStrategy);

					postingJournal.Job = new EntityReference();
					postingJournal.Job.Key = line.NRL_PrimaryJobReference;
					postingJournal.Job.Type = AccountingDataTransferConstants.DataContextTypeString.Job; // context type constant

					postingJournal.OSTotalAmount = -1 * line.NRL_Amount;
					postingJournal.OSGSTVATAmount = 0;
					postingJournal.OSAmount = -1 * line.NRL_Amount;

					postingJournal.OSCurrency = new Currency();
					postingJournal.OSCurrency.Code = line.NRL_RX_NKCurrency;

					if (line.References.Cast<NettingReceivableLineReference>().Any(x => x.NR1_Type == AccountingConstants.TransactionReferenceTypes.ConsolNumber))
					{
						postingJournal.CostSource = new EntityReference();
						postingJournal.CostSource.Key = line.References.Cast<NettingReceivableLineReference>().First(x => x.NR1_Type == AccountingConstants.TransactionReferenceTypes.ConsolNumber).NR1_Reference;
						postingJournal.CostSource.Type = nameof(DataContextType.ForwardingConsol); //consol type is required, the actual type should not affect finding and linking the consol
					}

					result.Add(postingJournal);
				}
				return result;
			});
		}

		void PopulateOrganizationAddress(NettingReceivableTransaction sourceBO, UniversalTransaction invoice)
		{
			var orgHeader = sourceBO.Issuer.Organisation;
			var orgAddress = new OrganizationAddress(writeManager.WriterStrategy);
			orgAddress.AddressType = nameof(DocAddressType.None);
			orgAddress.OrganizationCode = orgHeader.OH_Code;

			if (orgHeader.CustomsCodes.Any())
			{
				orgAddress.SetRegistrationNumberCollection(() =>
				{
					var result = new List<RegistrationNumber>();
					foreach (OrgCusCode regNum in orgHeader.CustomsCodes)
					{
						result.Add(OrganizationAddressHelper.CreateRegistrationNumber(regNum));
					}
					return result;
				});
			}

			invoice.OrganizationAddress = orgAddress;
		}

		void PopulateJobInvoiceReference(NettingReceivableTransaction sourceBO, UniversalTransaction invoice)
		{
			if (sourceBO.References.Count > 0)
			{
				ZString jobInvoiceNumber = sourceBO.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber)
					? sourceBO.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber) : ZString.Empty;

				if (!jobInvoiceNumber.IsEmpty)
				{
					invoice.JobInvoiceNumber = jobInvoiceNumber;
				}
			}
		}

		void PopulateInvoiceJob(NettingReceivableTransaction sourceBO, UniversalTransaction invoice)
		{
			invoice.Job = new EntityReference();
			if (sourceBO.Lines.Count > 0)
			{
				var jobNumbers = sourceBO.Lines.Cast<NettingReceivableTransactionLine>().Select(x => x.NRL_PrimaryJobReference).ToList();
				if (jobNumbers.Distinct().Count() == 1)
				{
					var jobNumber = jobNumbers.First().ToString();
					invoice.Job.Key = jobNumber;
					invoice.Job.Type = AccountingDataTransferConstants.DataContextTypeString.Job; // context type constant
				}
			}
		}
	}
}
