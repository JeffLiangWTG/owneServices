using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.Universal.Netting;
using Enterprise.Accounting.Netting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.Testing.Universal.Netting
{
	public class NettingTransactionDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestFullConsolTransaction()
		{
			var transaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.ConsolNumber, "C1");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.MasterBill, "123-23423423");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, "CCN123_1");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, "CCN123_2");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, "90881");

			var line1 = testObjectCreator.CreateNettingTransactionLine(transaction, "SH1", 300M, "USD");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, "SH1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ConsolNumber, "C1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.HouseBill, "23423423");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, "CCN123_1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, "CBR1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.AgentReference, "OAG1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.PackedContainer, "PGG1");

			var line2 = testObjectCreator.CreateNettingTransactionLine(transaction, "SH2", 200M, "USD");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, "SH2");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.ConsolNumber, "C1");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.HouseBill, "23423423");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, "CCN123_2");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, "CBR2");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.PackedContainer, "PGG2");

			var writer = new NettingTransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, transaction)));
			var transactionDataObject = writer.GetDataObject(transaction);

			AssertTransactionDataObject(transaction, transactionDataObject);
		}

		public void TestConsolTransaction_TransactionDoesNotHaveContainerDetails_ButJobHas()
		{
			var transaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.ConsolNumber, "C1");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.MasterBill, "123-23423423");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, "90881");

			var line1 = testObjectCreator.CreateNettingTransactionLine(transaction, "SH1", 300M, "USD");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, "SH1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ConsolNumber, "C1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.HouseBill, "23423423");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, "CCN123_1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, "CBR1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.AgentReference, "OAG1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.PackedContainer, "PGG1");

			var line2 = testObjectCreator.CreateNettingTransactionLine(transaction, "SH2", 200M, "USD");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, "SH2");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.ConsolNumber, "C1");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.HouseBill, "23423423");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber, "CCN123_2");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, "CBR2");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.PackedContainer, "PGG2");

			var writer = new NettingTransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, transaction)));
			var transactionDataObject = writer.GetDataObject(transaction);

			AssertTransactionDataObject(transaction, transactionDataObject);
		}

		public void TestConsolTransaction_NoContainerInformation()
		{
			var transaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.ConsolNumber, "C1");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.MasterBill, "123-23423423");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, "90881");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, "CBR1");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.AgentReference, "OAG1");

			var line1 = testObjectCreator.CreateNettingTransactionLine(transaction, "SH1", 300M, "USD");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, "SH1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ConsolNumber, "C1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.HouseBill, "23423423");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, "CBR2");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.AgentReference, "OAG2");

			var line2 = testObjectCreator.CreateNettingTransactionLine(transaction, "SH2", 200M, "USD");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, "SH2");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.ConsolNumber, "C1");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.HouseBill, "23423423");
			testObjectCreator.AddNettingLineReference(line2, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, "CBR2");

			var writer = new NettingTransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, transaction)));
			var transactionDataObject = writer.GetDataObject(transaction);

			AssertTransactionDataObject(transaction, transactionDataObject);
		}

		public void TestSingleShipmentInvoice_ShipmentNotPartOfConsol()
		{
			var transaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, "90881");

			var line1 = testObjectCreator.CreateNettingTransactionLine(transaction, "SH1", 500M, "USD");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, "SH1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.HouseBill, "23423423");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, "CBR1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.AgentReference, "OAG1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.OrderReferences, "ORD1");

			var writer = new NettingTransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, transaction)));
			var transactionDataObject = writer.GetDataObject(transaction);

			AssertTransactionDataObject(transaction, transactionDataObject);
		}

		public void TestSingleShipmentInvoice_ShipmentPartOfConsol()
		{
			var transaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, "90881");

			var line1 = testObjectCreator.CreateNettingTransactionLine(transaction, "SH1", 500M, "USD");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, "SH1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.ConsolNumber, "C1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.HouseBill, "23423423");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, "CBR1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.AgentReference, "OAG1");
			testObjectCreator.AddNettingLineReference(line1, AccountingConstants.TransactionReferenceTypes.OrderReferences, "ORD1");

			var writer = new NettingTransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, transaction)));
			var transactionDataObject = writer.GetDataObject(transaction);

			AssertTransactionDataObject(transaction, transactionDataObject);
		}

		[ExpectNoExceptions]
		public void TestPopulateOrganizationAddressWithoutException()
		{
			var cusCode = issuer.Organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = "ABC";
			cusCode.OK_CustomsRegNo = "ABCDEFG";

			var transaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			testObjectCreator.AddNettingTransactionReference(transaction, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, "90881");

			var writer = new NettingTransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, transaction)));
			var transactionDataObject = writer.GetDataObject(transaction);

			Assert(transactionDataObject.OrganizationAddress.RegistrationNumberCollection.Any(x => x.Type.Code.Value == cusCode.OK_CodeType && x.Value.Value == cusCode.OK_CustomsRegNo));
		}

		void AssertTransactionDataObject(NettingReceivableTransaction source, UniversalTransaction target)
		{
			AssertNotNull(target);
			AssertNotNull(target.DataContext);
			AssertEquals(1, target.DataContext.DataSourceCollection.Count());
			AssertEquals(nameof(DataContextType.NettingTransaction), target.DataContext.DataSourceCollection.First().Type.GetValueOrDefault());
			AssertEquals(string.Format("{0} {1} {2}", source.Period.NSP_Period, source.Issuer.Organisation.OH_Code, source.NRT_Reference), target.DataContext.DataSourceCollection.First().Key.GetValueOrDefault());

			AssertNotNull(target.Job);
			if (source.Lines.Count > 0 && source.Lines.Cast<NettingReceivableTransactionLine>().Select(x => x.NRL_PrimaryJobReference).Distinct().Count() == 1)
			{
				AssertEquals(source.Lines.Cast<NettingReceivableTransactionLine>().First().NRL_PrimaryJobReference, target.Job.Key.GetValueOrDefault());
			}

			AssertEquals(source.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber), target.JobInvoiceNumber);

			AssertEquals("AP", target.Ledger);
			AssertEquals(source.NRT_Reference, target.Number);
			AssertEquals(source.Issuer.Organisation.OH_Code, target.OrganizationAddress.OrganizationCode);
			AssertEquals(source.Currency, target.OSCurrency.Code);
			AssertEquals(-1 * source.Amount, target.OSExGSTVATAmount);
			AssertEquals(-1 * source.Amount, target.OSTotal);
			AssertEquals(Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionType.INV, target.TransactionType);

			AssertEquals(source.Lines.Count, target.PostingJournalCollection.Count);

			foreach (var line in source.Lines.Cast<NettingReceivableTransactionLine>())
			{
				var job = target.PostingJournalCollection.First(x => x.OSAmount == -1 * line.NRL_Amount);
				AssertNotNull(job);
				AssertEquals(line.JobReference, job.Job.Key);
				AssertEquals(-1 * line.Amount, job.OSTotalAmount);
				AssertEquals(line.Currency.Code, job.OSCurrency.Code);
				if (line.References.Cast<NettingReceivableLineReference>().Any(x => x.NR1_Type == AccountingConstants.TransactionReferenceTypes.ConsolNumber))
				{
					AssertNotNull(job.CostSource);
					AssertEquals(job.CostSource.Key, line.References.Cast<NettingReceivableLineReference>().First(x => x.NR1_Type == AccountingConstants.TransactionReferenceTypes.ConsolNumber).NR1_Reference);
				}
			}

			AssertShipments(source, target);
		}

		void AssertShipments(NettingReceivableTransaction source, UniversalTransaction target)
		{
			var jobNumbers = source.Lines.Cast<NettingReceivableTransactionLine>().Select(x => x.NRL_PrimaryJobReference).Distinct();
			int shipmentCount = jobNumbers.Count();
			if (source.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolNumber)
				|| source.Lines.Cast<NettingReceivableTransactionLine>().Any(x => x.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolNumber)))
			{
				shipmentCount++;
			}
			AssertEquals(shipmentCount, target.ShipmentCollection.Count);

			foreach (var jobNumber in jobNumbers)
			{
				var job = source.Lines.Cast<NettingReceivableTransactionLine>().First(x => x.NRL_PrimaryJobReference == jobNumber);
				AssertNotNull(job);

				var shipment = (from n in target.ShipmentCollection
								let dataSources = n.DataContext.DataSourceCollection
								from d in dataSources
								where (d.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingShipment) && d.Key.GetValueOrDefault() == jobNumber)
								select n).First();

				AssertNotNull(shipment);

				if (source.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.MasterBill))
				{
					var masterBill = source.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.MasterBill);
					AssertEquals(masterBill, shipment.WayBillNumber.GetValueOrDefault());
				}

				if (job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolNumber))
				{
					AssertNotNull(shipment.SubShipmentCollection);
					AssertEquals(1, shipment.SubShipmentCollection.Count);

					var subShipment = (from n in shipment.SubShipmentCollection
									   let dataSources = n.DataContext.DataSourceCollection
									   from d in dataSources
									   where (d.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingShipment) && d.Key.GetValueOrDefault() == jobNumber)
									   select n).First();

					AssertNotNull(subShipment);

					AssertAdditionalReferencesAndOrders(job, subShipment);

					if (job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber)
						&& job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.PackedContainer))
					{
						AssertNotNull(subShipment.ContainerCollection);
						foreach (var containerNumber in job.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber))
						{
							AssertNotNull(subShipment.ContainerCollection.First(x => x.ContainerNumber.GetValueOrDefault() == containerNumber));
						}

						AssertNotNull(subShipment.PackingLineCollection);
						foreach (var packingLineReference in job.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.PackedContainer))
						{
							AssertNotNull(subShipment.PackingLineCollection.First(x => x.ReferenceNumber.GetValueOrDefault() == packingLineReference));
						}
					}
				}
				else
				{
					AssertAdditionalReferencesAndOrders(job, shipment);
				}

				AssertConsols(source, target);
			}
		}

		void AssertAdditionalReferencesAndOrders(NettingReceivableTransactionLine job, UniversalShipment shipment)
		{
			if (job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.OrderReferences))
			{
				AssertNotNull(shipment.LocalProcessing.OrderNumberCollection);
				foreach (var orderNumber in job.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.OrderReferences))
				{
					AssertNotNull(shipment.LocalProcessing.OrderNumberCollection.First(x => x.OrderReference.GetValueOrDefault() == orderNumber));
				}
			}

			if (job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.AgentReference))
			{
				AssertNotNull(shipment.AdditionalReferenceCollection);
				foreach (var agentReferenceNumber in job.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.AgentReference))
				{
					var additionalReference = shipment.AdditionalReferenceCollection.Cast<AdditionalReference>().First(x => x.Type.Code.GetValueOrDefault() == "OAG");
					AssertNotNull(additionalReference);
					AssertEquals(agentReferenceNumber, additionalReference.ReferenceNumber);
				}
			}

			if (job.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference))
			{
				AssertEquals(shipment.BookingConfirmationReference, job.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference));
			}
		}

		void AssertConsols(NettingReceivableTransaction source, UniversalTransaction target)
		{
			UniversalShipment consol = null;
			if (source.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolNumber))
			{
				var consolNumber = source.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolNumber);

				consol = (from n in target.ShipmentCollection
						  let dataSources = n.DataContext.DataSourceCollection
						  from d in dataSources
						  where (d.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingConsol) && d.Key.GetValueOrDefault() == consolNumber)
								&& !n.DataContext.DataSourceCollection.Any(x => x.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingShipment))
						  select n).First();

				AssertNotNull(consol);
			}

			if (consol == null)
			{
				return;
			}

			if (source.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber))
			{
				AssertNotNull(consol.ContainerCollection);
				foreach (var containerNumber in source.References.GetReferenceValues(AccountingConstants.TransactionReferenceTypes.ConsolContainerNumber))
				{
					AssertNotNull(consol.ContainerCollection.First(x => x.ContainerNumber.GetValueOrDefault() == containerNumber));
				}
			}

			if (source.References.HasReferenceValue(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference))
			{
				AssertEquals(consol.BookingConfirmationReference, source.References.GetReferenceValue(AccountingConstants.TransactionReferenceTypes.CarrierBookingReference));
			}

			AssertNotNull(consol.SubShipmentCollection);

			int shipmentCount = source.Lines.Cast<NettingReceivableTransactionLine>().Select(x => x.NRL_PrimaryJobReference).Distinct().Count();
			AssertEquals(shipmentCount, consol.SubShipmentCollection.Count);
		}

		NettingObjectCreator testObjectCreator;
		NettingSystem nettingSystem;
		NettingSystemPeriod period;
		NettingOrganisation issuer;
		OrgHeader issuerOrgHeader;
		NettingOrganisation recipient;
		OrgHeader recipientOrgHeader;

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new NettingObjectCreator(Factory);
			nettingSystem = testObjectCreator.CreateNettingSystem("NS1", "Test Netting System", GlbCompany.CurrentCompany);
			period = testObjectCreator.CreateNettingPeriod(nettingSystem, "201504", ZDateTime.Today, ZDateTime.Today.AddDays(30), ZDateTime.Today.AddDays(25)
				, ZDateTime.Today.AddDays(37), ZDateTime.Today.AddDays(40), ZDateTime.Today.AddDays(45), ZDate.Today.AddDays(30));
			issuerOrgHeader = testObjectCreator.ABIGAS;
			issuer = testObjectCreator.CreateNettingOrganisation(nettingSystem, issuerOrgHeader, "FUL");
			recipientOrgHeader = testObjectCreator.AALSHI;
			recipient = testObjectCreator.CreateNettingOrganisation(nettingSystem, recipientOrgHeader, "FUL");
		}
	}
}
