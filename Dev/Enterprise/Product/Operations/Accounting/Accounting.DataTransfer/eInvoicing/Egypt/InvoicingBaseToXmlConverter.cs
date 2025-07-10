using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.DataTransfer.Universal;
using Enterprise.Accounting.Export.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalTransactionInfo = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.DataTransfer.EInvoicing.Egypt
{
	public class InvoicingBaseToXmlConverter : IInvoicingBaseToXmlConverter
	{
		public (bool Success, string Xml) ConvertToXml(InvoicingBase invoicingBase, INotifications notifications)
		{
			Argument.NotNull(invoicingBase, nameof(invoicingBase));
			Argument.NotNull(invoicingBase.Company, nameof(invoicingBase.Company));

			var writerStrategy = new AccountingTransactionDataObjectWriterStrategy(context: "eInvoicing");
			var transactionInfo = new UniversalTransactionInfo(writerStrategy);

			var dataAcess = new BatchExportDataAccess(((IDbConnectionInternals)CargoWise.Data.Db.Connection).ADOConnection, ((IDbConnectionInternals)CargoWise.Data.Db.Connection).ADOTransaction);
			var transactionExporter = new TransactionExporter(dataAcess);
			transactionExporter.PopulateUniversalTransaction(writerStrategy, invoicingBase.Company.GC_Code.ToString(), invoicingBase.PK.ToGuid(), transactionInfo);

			var documentType = invoicingBase.AH_TransactionType == TransactionTypes.CreditNote
				? DocumentType.c
				: invoicingBase.AH_TransactionBelongsToGroup.IsValid
				? DocumentType.d
				: DocumentType.i;

			var idLookupResult = GetOriginalTransactionId(invoicingBase);
			if (idLookupResult.NotFound)
			{
				notifications.AddError(Res.GetString("353e4a2b-d9cd-4882-9a73-dccefe1d361c", "Missing government UUID for the original transaction of the AR {0} {1}", invoicingBase.AH_TransactionType, invoicingBase.AH_TransactionNum));
				return (Success: false, Xml: null);
			}
			else
			{
				var shipment = invoicingBase.Job?.Parent as ForwardingShipment;
				var mapper = new XUTtoDocumentMapper(transactionInfo, documentType, invoicingBase.Header.OH_Category, invoicingBase.Branch?.HomePort?.TimeZoneSet, idLookupResult.Id, shipment: shipment);
				return mapper.MapToXml(notifications);
			}
		}

		(string Id, bool NotFound) GetOriginalTransactionId(InvoicingBase invoicingBase)
		{
			if (invoicingBase.AH_TransactionBelongsToGroup.IsValid)
			{
				var authorisationRecordQuery = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
				authorisationRecordQuery.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, invoicingBase.AH_TransactionBelongsToGroup);
				authorisationRecordQuery.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, AccTransactionHeaderAuthorisationRecordTypes.Egypt);

				var authorisationRecord = invoicingBase.Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(authorisationRecordQuery);
				return (Id: authorisationRecord?.AHF_Number, NotFound: authorisationRecord == null || authorisationRecord.AHF_Number.IsEmpty);
			}
			return (null, false);
		}
	}
}
