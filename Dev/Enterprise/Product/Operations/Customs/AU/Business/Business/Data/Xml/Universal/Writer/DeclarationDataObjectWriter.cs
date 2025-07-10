using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DeclarationDataObjectWriter : DataTransfer.Universal.DeclarationDataObjectWriter
	{
		internal DeclarationDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.Business.CusEntryHeader relatedEntry)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		protected override void PopulateUNDG(BasePackage packageBO, PackingLine packingLineData, IDataWritingManager manager)
		{
			packingLineData.SetUNDGCollection(() =>
			{
				if (packageBO.UNDGs.Count > 0)
				{
					var packageUNDG = packageBO.UNDGs[0];
					var writer = new UNDGDataObjectWriter(manager);
					var undgData = writer.GetDataObject(packageUNDG);
					return new List<UNDG> { undgData };
				}
				return packingLineData.UNDGCollection;
			});
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceHeaderRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetJobComInvoiceHeaderRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var invoicePK = row.GetValue(JobComInvoiceHeaderSchema.PK);
			yield return new FetchHint(QuarantineExDocHeaderSchema.QH_JZ, invoicePK);
			yield return new FetchHint(CusStorageDocPivotSchema.CSD_ParentID, invoicePK);
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, invoiceLinePK);
			yield return new FetchHint(QuarantineExDocLineSchema.QL_JI, invoiceLinePK);
			yield return new FetchHint(CusStorageDocPivotSchema.CSD_ParentID, invoiceLinePK);
		}

		protected override void PopulateExtraOrganisation(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			base.PopulateExtraOrganisation(declarationBO, declarationData, keepExistingData);
			PopulateInvoiceARPATDAddress(declarationBO, declarationData);
		}

		protected void PopulateInvoiceARPATDAddress(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			var quarantineInvoice = (declarationBO as JobDeclaration)?.QuarantineInvoice;
			if (quarantineInvoice != null)
			{
				declarationData.AddOrgAddress(writeManager, quarantineInvoice.AQISResponsiblePerson);
				declarationData.AddOrgAddress(writeManager, quarantineInvoice.AQISTransitDestination);
				declarationData.AddOrgAddress(writeManager, quarantineInvoice.AQISEUContactPerson);
				declarationData.AddOrgAddress(writeManager, quarantineInvoice.AQISEUPlaceOfDestination);
			}
		}

		protected override void PopulateAttachedDocumentCollection(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			if (declarationBO is JobDeclaration dec)
			{
				base.PopulateAttachedDocumentCollection(declarationData, dec);
			}
		}

		protected override IEnumerable<IeDoc> GeteDocsToPopulate(BaseJobDeclaration declarationBO)
		{
			var rfpInvoices = declarationBO.Invoices.Cast<JobComInvoiceHeader>()
					.Where(x => x.QuarantineExDocHeader != null && x.IsNEXDOCSActive)
					.ToArray();

			var eDocsInvoice = rfpInvoices
				.SelectMany(x => x.EDocPivotCollection.Cast<CusStorageDocPivot>())
				.Select(x => x.Document)
				.Where(x => x != null);

			var eDocsInvoiceLine = rfpInvoices
				.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>())
				.Where(x => x.QuarantineExDocLine != null)
				.SelectMany(x => x.EDocPivotCollection.Cast<CusStorageDocPivot>())
				.Select(x => x.Document)
				.Where(x => x != null);

			return eDocsInvoice.Union(eDocsInvoiceLine).Distinct()
				.OrderBy(x => x.FileName);
		}
	}
}
