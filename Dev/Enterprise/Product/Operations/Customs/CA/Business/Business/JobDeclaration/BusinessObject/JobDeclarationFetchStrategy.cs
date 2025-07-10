using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class JobDeclarationFetchStrategy : Customs.Business.FetchStrategies.BaseJobDeclarationFetchStrategy
	{
		public JobDeclarationFetchStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration declaration
		{
			get { return BusinessObject as JobDeclaration; }
		}

		protected override void FetchForViewDeclaration(TableColumn[] columns)
		{
			var entryLineFees = false;
			var importerOfRecordAddress = false;
			var requiredDocumentAddInfo = false;

			Factory.AddFetchHint(CusEntryHeaderSchema.CH_JE, declaration.PK);
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, declaration.PK);
			Factory.AddFetchHint(JobDocsAndCartageSchema.JP_ParentID, BusinessObject.PK);

			foreach (TableColumn tableColumn in columns)
			{
				switch (tableColumn.ColumnName)
				{
					case JobDeclaration.Schema.DeclarationNumber:
						Factory.AddFetchHint(OrgHeaderSchema.PK, declaration.JE_OH_Importer);
						Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, declaration.JE_OH_Importer);
						Factory.AddFetchHint(OrgMiscServSchema.OM_OH, declaration.JE_OH_Importer);
						break;
					case JobDeclaration.Schema.JE_MessageStatusDescription:
						foreach (CusEntryHeader header in declaration.CustomsEntryHeaders)
						{
							Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, header.PK);
						}
						break;
					case JobDeclaration.Schema.TotalNormalDuty:
					case JobDeclaration.Schema.TotalSimaDuty:
					case JobDeclaration.Schema.TotalGST:
					case JobDeclaration.Schema.TotalExciseTax:
					case JobDeclaration.Schema.TotalDutyAndTax:
					case JobDeclaration.Schema.TotalAmountPayable:
						if (!entryLineFees)
						{
							Factory.AddFetchHint(OrgHeaderSchema.PK, declaration.JE_OH_Importer);
							Factory.AddFetchHint(OrgMiscServSchema.OM_OH, declaration.JE_OH_Importer);
							Factory.AddFetchHint(OrgCountryDataSchema.OV_OH_OrgHeader, declaration.JE_OH_Importer);
							var b3Entry = declaration.B3EntryHeader;
							if (b3Entry != null)
							{
								Factory.AddFetchHint(CusEntryLineSchema.CL_CH, b3Entry.PK);
								foreach (CusEntryLine line in b3Entry.MergedLines)
								{
									Factory.AddFetchHint(CusEntryLineFeeSchema.CF_CL, line.PK);
								}
							}
							entryLineFees = true;
						}
						break;
					case JobDeclaration.Schema.JE_CCNsAsAString:
						Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, declaration.PK);
						Factory.AddFetchHint(typeof(CargoControlNumber), new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.CACCN), new ZQuery(CusAddInfoSchema.B7_ParentID, declaration.PK));
						break;
				}
				switch (tableColumn.ColumnName)
				{
					case "ImporterOfRecordAddress+OrganisationPK":
					case JobDeclaration.Schema.TotalAmountPayable:
						if (!importerOfRecordAddress)
						{
							Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, declaration.PK);
							Factory.AddFetchHint(JobDocAddressSchema.Instance, new ZQuery(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.ImporterOfRecord), new ZQuery(JobDocAddressSchema.E2_ParentID, declaration.PK));
							importerOfRecordAddress = true;
						}
						break;
					case "DIFURNs":
					case "DIFMessageStatus":
						if (!requiredDocumentAddInfo)
						{
							var rootPK = declaration.Shipment?.PK ?? declaration.PK;
							var docsAndCartage = declaration.DocsAndCartage;
							var requiredDocuments = docsAndCartage?.RequiredDocuments;

							if (requiredDocuments != null)
							{
								Factory.AddFetchHint(JobRequiredDocumentSchema.EQ_ParentID, docsAndCartage.PK);
								Factory.AddFetchHint(JobDocsAndCartageSchema.JP_ParentID, rootPK);
								foreach (JobRequiredDocument document in requiredDocuments)
								{
									Factory.AddFetchHint(JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument, document.PK);
								}
							}
							requiredDocumentAddInfo = true;
						}
						break;
				}
			}
			base.FetchForViewDeclaration(columns);
		}

		protected override bool IsJobComInvoiceHeaderBillRelatedColumn(string columnName)
		{
			return base.IsJobComInvoiceHeaderBillRelatedColumn(columnName) ||
				columnName.StartsWith("Total", System.StringComparison.Ordinal);
		}

		protected override void FetchForValidateCore()
		{
			Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);

			((JobDeclaration)BusinessObject).AddFetchsHintForPopulateDutiesAndTaxesIfNeeded();

			base.FetchForValidateCore();
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			((JobDeclaration)BusinessObject).AddFetchsHintForPopulateDutiesAndTaxesIfNeeded();
			base.FetchForLoadChildEditableObjectsCore();
		}

		protected override void FetchForDeleteCore()
		{
			Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
			base.FetchForDeleteCore();
		}

		protected override void AddMergeFetchHintsFor(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			base.AddMergeFetchHintsFor(invoiceLine);
			Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, invoiceLine.PK);
		}

		protected override void AddMergeFetchHintsAfterInvoiceLines()
		{
			base.AddMergeFetchHintsAfterInvoiceLines();

			((JobDeclaration)BusinessObject).AddFetchsHintForPopulateDutiesAndTaxesIfNeeded();
		}
	}
}
