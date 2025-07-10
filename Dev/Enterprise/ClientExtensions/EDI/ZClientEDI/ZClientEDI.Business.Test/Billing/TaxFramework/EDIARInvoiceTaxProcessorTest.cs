using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.JobInvoicing.Posting.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class EDIARInvoiceTaxProcessorTest : TestCaseWithFactory
	{
		public void TestCalculateOtherTaxes()
		{
			var mock = new Mock<ITaxProcessor>();
			mock.Setup(x => x.ProcessTaxesOnPosting(It.IsAny<ITaxRecordParent>())).Returns("ProcessTaxesOnPosting");
			var processor = new EDIARInvoiceTaxProcessor();

			using (ObjectFactory.Substitute(mock.Object))
			{
				var invoice = GetValidInvoice("S0001", 0);
				AssertNoExceptionThrown(() => processor.Process(invoice));

				AssertEquals(false, invoice.Company.IsEnabledForTaxFrameworkConfiguration(Factory));
				var config = Factory.New<AccTaxConfiguration>();
				config.ETC_ParentId = invoice.Company.PK;
				AssertEquals(true, invoice.Company.IsEnabledForTaxFrameworkConfiguration(Factory));
				AssertExceptionThrown<InvalidOperationException>("AssertExceptionThrown", "ProcessTaxesOnPosting", () => processor.Process(invoice));

				EDIDataRegistry.Instance.EnableTaxProcessorForBilling.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				AssertNoExceptionThrown(() => processor.Process(invoice));
			}
		}

		public void TestSplitInvoice()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var org = testObjectCreator.Debtor;
			var currency = testObjectCreator.USD;
			var exchangeRate = 0.25m;
			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("AR001", currency, exchangeRate, org);
			var config = Factory.NewWithValidTestData<AccTaxConfiguration>();
			config.ETC_ParentId = invoice.Company.PK;
			var amount = 15;

			for (var idx = 0; idx < 5; idx++)
			{
				testObjectCreator.CreateInvoiceLine(invoice, currency, exchangeRate, amount);
				amount += 5;
			}

			var splitLog = new ZStringBuilder();
			var processor = new EDIARInvoiceTaxProcessorForTest();
			processor.OnInvoiceSplit += (_, e) =>
			{
				var line = e.InvoiceLines.Single();
				splitLog.AppendLine($"{line.AL_RX_NKTransactionCurrency} - {line.AL_OverseasTotal}");
			};
			var invoices = processor.Process(invoice);
			Factory.Save();
			AssertEquals(5, invoices.Count());
			var invoiceAsString = string.Join("\r\n", invoices.Select(x => $"[{x.AH_Desc}] - {x.AH_RX_NKTransactionCurrency} - {x.AH_OSTotal}"));
			AssertEquals(@"[Test Invoice - 1 of 5] - USD - 16.50
[Test Invoice - 2 of 5] - USD - 22.0
[Test Invoice - 3 of 5] - USD - 27.50
[Test Invoice - 4 of 5] - USD - 33.0
[Test Invoice - 5 of 5] - USD - 38.50", invoiceAsString);
				AssertEquals(@"USD - 22.0
USD - 27.50
USD - 33.0
USD - 38.50
", splitLog.ToString());
		}

		ARInvoice GetValidInvoice(ZString jobNum, ZDecimal amount)
		{
			var invoice = Factory.New<ARInvoice>();

			ARInvoiceLine line = Factory.New<ARInvoiceLine>();
			invoice.Lines.Add(line);
			line.AL_AG = Factory.LoadTop1<AccGLHeader>(new ZQuery()).PK;
			line.AL_LineType = Enterprise.ZArchitecture.Core.TransactionLineTypes.Revenue;
			line.AL_OSExTaxAmount = amount;

			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			invoice.AH_OutstandingAmount = invoice.AH_InvoiceAmount + invoice.AH_GSTAmount;

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_JobNum = jobNum;
			invoice.AH_JH = job.PK;

			Factory.Save();

			return invoice;
		}

		public class EDIARInvoiceTaxProcessorForTest : EDIARInvoiceTaxProcessor
		{
			protected override PostingChargeDistributor GetPostingChargeDistributor()
			{
				var result = base.GetPostingChargeDistributor();
				result.SubstituteChargeSplitterDueToSingleTaxPerTransaction_ForTestOnly(new Splitter());
				return result;
			}
		}

		class Splitter : IChargeSplitterDueToSingleTaxPerTransaction
		{
			public PostingChargeCollection GetSplitCharges(PostingChargeCollection postingCharges)
			{
				var result = new PostingChargeCollection();
				var charges = postingCharges.OfType<IReceivablesPostingChargeCollection>().SelectMany(x => x);
				var comments = charges.Where(x => x.IsCommentChargeCode);
				var idx = 0;

				foreach (var charge in charges.Where(x => !x.IsCommentChargeCode))
				{
					var collection = new IReceivablesPostingChargeCollection() { charge };
					if (idx++ == 0)
					{
						collection.AddRange(comments);
					}
					result.SetCharges(new PostingChargeKey(ZGuid.NewZGuid(), "FIN", ZGuid.Empty, ZGuid.Empty, 0), collection);
				}

				return result;
			}
		}
	}
}
