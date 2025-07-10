using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.ClientSharedComponents.Testing;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.ELG.Testing
{
	public abstract class SagAccountsExporterTest : AccountsExporterARAPTest
	{
		public void TestFilterProvider()
		{
			Assert(!Exporter.FilterProvider.IncludeAccrualsPosting);
			Assert(!Exporter.FilterProvider.IncludeAccrualsReversing);
			Assert(!Exporter.FilterProvider.IncludeWIPsPosting);
			Assert(!Exporter.FilterProvider.IncludeWIPsReversing);
			Assert(Exporter.FilterProvider.IncludeAPCreditNotes);
			Assert(Exporter.FilterProvider.IncludeAPInvoices);
			Assert(!Exporter.FilterProvider.IncludeAPAdjustmentNotes);
			Assert(Exporter.FilterProvider.IncludeARCreditNotes);
			Assert(Exporter.FilterProvider.IncludeARInvoices);
			Assert(!Exporter.FilterProvider.IncludeARAdjustmentNotes);
			Assert(Exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAP);
			Assert(Exporter.FilterProvider.ExcludeNonJobRelatedTransactionsForAR);
			AssertEquals("Current Batch Number", 0, Exporter.FilterProvider.CurrentBatchNo);
		}

		[ExpectException(typeof(Exception))]
		public void TestConverterUnknownException()
		{
			SagAccountsExporterForTest exporter = new SagAccountsExporterForTest(0, Factory, Notifications);
			using (TempFile document = TempFile.New())
			{
				using (TextWriter documentWriter = new StreamWriter(document.Filename))
				{
					exporter.Convert(documentWriter);
				}
			}
		}

		class SagAccountsExporterForTest : SagAccountsExporter
		{
			public SagAccountsExporterForTest(int batchNumber, BusinessObjectFactory factory, NotificationBuffer notifications) : base(batchNumber, factory, notifications)
			{
			}

			protected override AccountingFlatFileConverter Converter
			{
				get
				{
					return new SagAccountsConverterForTest(Factory, notifications);
				}
			}

			public void Convert(TextWriter documentWriter)
			{
				base.Convert(Converter, new Xsd.TxnHeader(), new SagAccountsFlatFileFormat(), documentWriter);
			}

			protected override string CurrentLedgerType
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public override ZString FileNamePrefix
			{
				get
				{
					throw new NotImplementedException();
				}
			}
		}

		class SagAccountsConverterForTest : SagAccountsConverter
		{
			public SagAccountsConverterForTest(BusinessObjectFactory factory, NotificationBuffer notifications) : base(factory, notifications)
			{
			}

			protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
			{
				throw new ELGException(ELGExceptionType.Unknown);
			}

			protected override ZString MapToNominalCode(ZString transportMode, ZString chargeCode)
			{
				throw new NotImplementedException();
			}

			protected override string NominalCodeType
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			protected override SagFlatFileDataRow NewExternalInvoice
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			protected override void AddSpecificInvoiceHeader(SagFlatFileDataRow invoiceHeader, ref SagFlatFileDataRow result)
			{
				throw new NotImplementedException();
			}

			protected override void AddSpecificInvoiceLineAggregateCollection(Dictionary<ZString, SagInvoiceLineAggregateDataRow> invoiceLineAggregateCollection, ref SagFlatFileDataRow result)
			{
				throw new NotImplementedException();
			}

			protected override void AddSpecificInvoiceTaxAggregateCollection(Dictionary<ZString, SagInvoiceLineTaxAggregateDataRow> invoiceLineTaxAggregateCollection, ref SagFlatFileDataRow result)
			{
				throw new NotImplementedException();
			}

			protected override int LedgerSource
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			protected override bool IsOkToProcess(Enterprise.DataTransfer.Xml.XsdVersion1.TxnHeader xmlHeader)
			{
				throw new NotImplementedException();
			}

			protected override void BuildSpecificInvoiceHeaderRow(Enterprise.DataTransfer.Xml.XsdVersion1.TxnHeader xmlHeader, SagInvoiceHeaderDataRow headerRow)
			{
				throw new NotImplementedException();
			}

			protected override SagInvoiceHeaderDataRow NewSagInvoiceHeaderDataRow
			{
				get
				{
					throw new NotImplementedException();
				}
			}
		}
	}
}
