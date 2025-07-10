using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.CLE.MattelARInvoiceExport
{
	public class MattelARInvoiceExporter
	{
		public MattelARInvoiceExporter(INotifications buffer, BusinessObjectFactory factory)
		{
			this.Buffer = buffer;
			this.Factory = factory;
		}

		public void ExportARInvoice()
		{
			ExportARInvoiceCore();
		}

		#region ExportARInvoice

		protected INotifications Buffer;

		protected virtual
 void ExportARInvoiceCore()
		{
			InvoicingBase[] result = GetInvoices(NonExportedInvoice, ZGuid.Empty);
			JobInvoiceRecordCollection records = CreateJobInvoiceRecord(result);

			if (records.Count > 0)
			{
				PopulateRecordToMessageFile(records);
			}
		}

		#endregion

		readonly string DEXReference = "AR Invoice Export (Interchange: ";

		#region AddDataExportEvent

		void AddDataExportEvent(IReadOnlyList<InvoicingBase> invoices, ZString interchangeNumber)
		{
			foreach (InvoicingBase invoice in invoices)
			{
				invoice.Logs.AddNew(Events.DataExport, DEXReference + interchangeNumber + ")");
			}
		}

		#endregion

		#region PopulateRecordToMessageFile

		void PopulateRecordToMessageFile(JobInvoiceRecordCollection jobInvoiceRecords)
		{
			foreach (JobInvoiceRecord record in jobInvoiceRecords)
			{
				try
				{
					GenerateMessageFile(record);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Buffer.Notify(new ErrorNotification(ErrorType.Error, "Error occurs while exporting AR Invoices of Shipment(" + record.Shipment.JobNumber + ") -" + ex.Message));
				}
			}
		}

		#endregion

		#region GenerateMessageFile

		protected virtual

		void GenerateMessageFile(JobInvoiceRecord record)
		{
			ZString outputFile = "";
			ZString interchangeNumber = "";

			BusinessObjectFactory messageBuilderFactory = new BusinessObjectFactory();

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				AUINVOICMessageBuilder aUMesgBuilder = new AUINVOICMessageBuilder(record, messageBuilderFactory);
				outputFile = aUMesgBuilder.GenerateMessageText();
				interchangeNumber = aUMesgBuilder.InterchangeNumber;
			}
			else if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.NewZealand)
			{
				NZINVOICMessageBuilder nZMesgBuilder = new NZINVOICMessageBuilder(record, messageBuilderFactory);
				outputFile = nZMesgBuilder.GenerateMessageText();
				interchangeNumber = nZMesgBuilder.InterchangeNumber;
			}

			SendEmail(outputFile);
			int startIndex = outputFile.IndexOf('_', 2) + 1;
			int endIndex = outputFile.IndexOf(".EDI");
			string jobNumber = outputFile.Substring(startIndex, endIndex - startIndex);

			Buffer.Notify(new InfoNotification("Invoices of Job Number " + jobNumber + " are exported."));

			AddDataExportEvent(record.Invoices, interchangeNumber);

			Factory.Save();
		}

		#endregion

		#region SendEmail

		void SendEmail(ZString outputFile)
		{
			if (File.Exists(outputFile))
			{
				EmailDef email = CreateEmail(outputFile);
				Env.OutgoingMailManager.Create(Factory, email);
				File.Delete(outputFile);
			}
		}

		#endregion

		#region CreateEmail

		EmailDef CreateEmail(ZString attachmentFile)
		{
			ZString recipient = CLEDataRegistry.Instance.MattelEmailAddress;
			ZString subject = CLEDataRegistry.Instance.MattelEmailSubject;

			EmailDef email = new EmailDef();
			email.Subject = subject;
			email.AddRecipientForSystemCommunication(recipient);
			email.FromAddress = CLEDataRegistry.Instance.ClemengerEmailAddress;
			AttachmentDef attachment = new AttachmentDef(attachmentFile);
			email.Attachments.Add(attachment);
			return email;
		}

		#endregion

		#region CreateJobInvoiceRecord

		JobInvoiceRecordCollection CreateJobInvoiceRecord(InvoicingBase[] invoices)
		{
			ZGuid[] jobHeaderPKs = GetJobHeaderList(invoices);
			JobInvoiceRecordCollection jobInvoiceRecords = new JobInvoiceRecordCollection();

			foreach (ZGuid headerPK in jobHeaderPKs)
			{
				ForwardingShipment shipment = GetShipment(headerPK);

				if (shipment != null && shipment.Consols.Count > 0 && shipment.IsImport())
				{
					BaseJobDeclaration jobDec = GetShipmentDeclaration(shipment);

					if (jobDec != null)
					{
						InvoicingBase[] jobHeaderInvoices = GetInvoicesOfThisJob(invoices, headerPK);

						if (InvoicesQualifyForDataExport(jobHeaderInvoices) && InvoiceDataHaveNotBeenExported(headerPK))
						{
							JobInvoiceRecord record = new JobInvoiceRecord(shipment, jobDec, jobHeaderInvoices);
							jobInvoiceRecords.Add(record);
						}
					}
				}
			}

			return jobInvoiceRecords;
		}

		#endregion

		#region GetInvoicesOfThisJob

		InvoicingBase[] GetInvoicesOfThisJob(InvoicingBase[] invoices, ZGuid jobPK)
		{
			ArrayList jobInvoices = new ArrayList();

			foreach (InvoicingBase invoice in invoices)
			{
				if (invoice.AH_JH == jobPK)
				{
					jobInvoices.Add(invoice);
				}
			}
			return (InvoicingBase[])jobInvoices.ToArray(typeof(InvoicingBase));
		}

		#endregion

		#region GetShipmentDeclaration

		BaseJobDeclaration GetShipmentDeclaration(ForwardingShipment shipment)
		{
			BaseJobDeclaration jobDec = null;

			if (shipment.Declarations.Length > 0)
			{
				jobDec = (BaseJobDeclaration)shipment.Declarations[0];
			}
			else if (!shipment.JS_JS_ColoadMasterShipment.IsEmpty)
			{
				ForwardingShipment coloadMaster = Factory.Load<ForwardingShipment>(shipment.JS_JS_ColoadMasterShipment);
				jobDec = (coloadMaster != null && coloadMaster.Declarations.Length > 0) ? (BaseJobDeclaration)coloadMaster.Declarations[0] : null;
			}

			return jobDec;
		}

		#endregion

		#region GetShipment
		ForwardingShipment GetShipment(ZGuid headerPK)
		{
			ForwardingShipment result = null;

			JobHeader header = Factory.Load<JobHeader>(headerPK);

			if (header.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix)
			{
				result = Factory.Load<ForwardingShipment>(header.JH_ParentID);
			}

			return result;
		}

		#endregion

		#region InvoiceDataHaveNotBeenExported

		bool InvoiceDataHaveNotBeenExported(ZGuid jobHeader)
		{
			return GetInvoices(!NonExportedInvoice, jobHeader).Length == 0;
		}

		#endregion

		#region InvoicesQualifyForDataExport

		bool InvoicesQualifyForDataExport(InvoicingBase[] invoices)
		{
			bool hasDisbursementInvoice = false;
			bool hasFinalInvoice = false;

			foreach (InvoicingBase invoice in invoices)
			{
				if (invoice.AH_IsDisbursementCalc)
				{
					hasDisbursementInvoice = true;
				}
				else
				{
					hasFinalInvoice = true;
				}
			}

			return hasFinalInvoice && hasDisbursementInvoice;
		}

		#endregion

		#region GetJobHeaderList

		ZGuid[] GetJobHeaderList(InvoicingBase[] invoices)
		{
			ArrayList jobHeaderPKs = new ArrayList();

			foreach (InvoicingBase invoice in invoices)
			{
				if (!jobHeaderPKs.Contains(invoice.AH_JH))
				{
					jobHeaderPKs.Add(invoice.AH_JH);
				}
			}
			return (ZGuid[])jobHeaderPKs.ToArray(typeof(ZGuid));
		}

		#endregion

		#region GetInvoices

		public InvoicingBase[] GetInvoices(bool nonExportedInvoice, ZGuid jobHeaderPK)
		{
			var sqlScript = new StringBuilder();
			sqlScript.Append($@"
SELECT {AccTransactionHeaderSchema.Constants.PK}
FROM {AccTransactionHeaderSchema.Constants.SqlSchemaName}.{AccTransactionHeaderSchema.Constants.TableName} 
WHERE {AccTransactionHeaderSchema.Constants.AH_OH} = @DebtorPK 
  AND {AccTransactionHeaderSchema.Constants.AH_Ledger} = @LedgerType 
  AND {AccTransactionHeaderSchema.Constants.AH_TransactionType} = @TransactionType 
  AND {AccTransactionHeaderSchema.Constants.AH_IsCancelled} = @AHFalse
  AND {AccTransactionHeaderSchema.Constants.AH_GC} = @CompanyPK 
  AND {AccTransactionHeaderSchema.Constants.AH_JH} IS NOT NULL
");

			var stmSql = $@"
SELECT TOP(1) 1
FROM {StmALogSchema.Constants.SqlSchemaName}.{StmALogSchema.Constants.TableName} 
WHERE   {StmALogSchema.Constants.SL_Reference} LIKE @DEXReference
	AND {StmALogSchema.Constants.SL_Table} = @AccTransactionTableName
	AND {StmALogSchema.Constants.SL_SE_NKEvent} = @DEXEvent
	AND {StmALogSchema.Constants.SL_IsCancelled} <> @SLTrue
	AND {StmALogSchema.Constants.SL_Parent} = {AccTransactionHeaderSchema.Constants.PK}";
			if (nonExportedInvoice)
			{
				sqlScript.Append($"AND {AccTransactionHeaderSchema.Constants.AH_PostDate} >= @PostDate ");
				sqlScript.Append($"AND NOT EXISTS ({stmSql}) ");
			}
			else
			{
				sqlScript.Append($"AND {AccTransactionHeaderSchema.Constants.AH_JH} = @JobHeaderPK ");
				sqlScript.Append($"AND EXISTS ({stmSql}) ");
			}

			var @params = new ZSqlParameterCollection();
			@params.Add("@DEXReference", DEXReference + "%", StmALogSchema.SL_Reference);
			@params.Add("@AccTransactionTableName", AccTransactionHeaderSchema.Constants.TableName, StmALogSchema.SL_Table);
			@params.Add("@DEXEvent", Events.DataExport.Code, StmALogSchema.SL_SE_NKEvent);
			@params.Add("@DebtorPK", CLEDataRegistry.Instance.MattelDebtor, AccTransactionHeaderSchema.AH_OH);
			@params.Add("@LedgerType", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger);
			@params.Add("@TransactionType", ZArchitecture.Core.TransactionTypes.Invoice, AccTransactionHeaderSchema.AH_TransactionType);
			@params.Add("@CompanyPK", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);
			@params.Add("@SLTrue", "Y", StmALogSchema.SL_IsCancelled);
			@params.Add("@AHFalse", false, AccTransactionHeaderSchema.AH_IsCancelled);
			@params.Add("@PostDate", ZDateTime.Now.Date.AddMonths(-3), AccTransactionHeaderSchema.AH_PostDate);
			@params.Add("@JobHeaderPK", jobHeaderPK, AccTransactionHeaderSchema.AH_JH);

			var invoices = new ArrayList();
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sqlScript.ToString(), @params);
			foreach (DynamicBusinessObject bizObj in collection)
			{
				var invoicePK = (ZGuid)bizObj[AccTransactionHeaderSchema.Constants.PK];

				if (invoicePK.IsValid)
				{
					var invoice = Factory.Load<InvoicingBase>(invoicePK);
					if (invoice != null)
					{
						invoices.Add(invoice);
					}
				}
			}

			return (InvoicingBase[])invoices.ToArray(typeof(InvoicingBase));
		}

		#endregion

		readonly BusinessObjectFactory Factory;

		const bool NonExportedInvoice = true;
	}
}

#region Test
#region TestExportWithException
#endregion
#region TestExportARInvoiceForShipment
#endregion
#endregion
#region AssertExportResultEmailSent
#endregion
#region Implementation
#region SetupOrganisation
#endregion
#region SetupShipmentWithARInvoices
#endregion
#region CreateInvoiceLine
#endregion
#endregion
