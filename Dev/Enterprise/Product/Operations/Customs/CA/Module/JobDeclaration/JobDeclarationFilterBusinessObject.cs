using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Module;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using CusEntryHeader = Enterprise.Customs.CA.Business.CusEntryHeader;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;
using UniversalReferenceConstants = Enterprise.Customs.CA.Business.UniversalReferenceConstants;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public class JobDeclarationFilterBusinessObject : CommonJobDeclarationFilterBusinessObject
	{
		#region overrides

		protected override void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			base.AddOrganisationFilters(filters);

			var carrierCode = filters.AddNkFilter(DeclarationFilterConstants.OrgFilterTypes.CarrierCode, GetCarrierCodeQuery, ModuleIDs.Customs.Universal.ZZRefCarrier, Lookups.CarrierCodes).WithMaxLengthOf<ModuleNkFilter>(CAAddInfoSchema.CA_CarrierCode);
			carrierCode.Category = FilterCategories.Organisations;
			carrierCode.MaxLength = AddInfo.Schema.CA_CarrierCodeMaxLength;
			carrierCode.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|CarrierCode", DeclarationFilterConstants.OrgFilterTypes.CarrierCode);

			var importerOfRecordFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.ImporterOfRecord, ModuleIDs.Organisation, orgPK => GetJobDocAddressOfDeclarationQuery(DocAddressType.ImporterOfRecord, orgPK), Lookups.OrganisationList);
			importerOfRecordFilter.Category = FilterCategories.Organisations;
			importerOfRecordFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ImporterOfRecord", DeclarationFilterConstants.OrgFilterTypes.ImporterOfRecord);

			var bondedWarehouseFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.BondedWarehouse, ModuleIDs.Organisation, orgPK => GetJobDocAddressOfDeclarationQuery(DocAddressType.CustomsWarehouseAddress, orgPK), Lookups.OrganisationList);
			bondedWarehouseFilter.Category = FilterCategories.Organisations;
			bondedWarehouseFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|BondedWarehouse", DeclarationFilterConstants.OrgFilterTypes.BondedWarehouse);

			var originatorFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.Originator, ModuleIDs.Organisation, orgPK => GetJobDocAddressOfDeclarationQuery(DocAddressType.CommercialInvoiceOriginator, orgPK), Lookups.OrganisationList);
			originatorFilter.Category = FilterCategories.Organisations;
			originatorFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|Originator", DeclarationFilterConstants.OrgFilterTypes.Originator);

			var exportSellerFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.ExportSeller, ModuleIDs.Organisation, orgPK => GetJobDocAddressOfDeclarationQuery(DocAddressType.SellingParty, orgPK), Lookups.OrganisationList);
			exportSellerFilter.Category = FilterCategories.Organisations;
			exportSellerFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ExportSeller", DeclarationFilterConstants.OrgFilterTypes.ExportSeller);

			var addressFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.Address, ModuleIDs.Customs.CA.CAJobDocAddresses, GetJobDocAddressQuery, Lookups.JobDocAddressList);
			addressFilter.Category = FilterCategories.Organisations;
			addressFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|Address", DeclarationFilterConstants.OrgFilterTypes.Address);

			var invoiceVendorFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.InvoiceVendor, ModuleIDs.Organisation, orgPK => GetJobDocAddressOfInvoiceQuery(DocAddressType.SupplierDocumentaryAddress, orgPK), Lookups.OrganisationList);
			invoiceVendorFilter.Category = FilterCategories.Organisations;
			invoiceVendorFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|InvoiceVendor", DeclarationFilterConstants.OrgFilterTypes.InvoiceVendor);

			var invoicePurchaserFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.InvoicePurchaser, ModuleIDs.Organisation, orgPK => GetInvoiceOrganizationQuery(JobComInvoiceHeaderSchema.JZ_OH_Buyer, orgPK), Lookups.OrganisationList);
			invoicePurchaserFilter.Category = FilterCategories.Organisations;
			invoicePurchaserFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|InvoicePurchaser", DeclarationFilterConstants.OrgFilterTypes.InvoicePurchaser);

			var invoiceConsigneeFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.InvoiceConsignee, ModuleIDs.Organisation, orgPK => GetJobDocAddressOfInvoiceQuery(DocAddressType.FinalConsigneeAddress, orgPK), Lookups.OrganisationList);
			invoiceConsigneeFilter.Category = FilterCategories.Organisations;
			invoiceConsigneeFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|InvoiceConsignee", DeclarationFilterConstants.OrgFilterTypes.InvoiceConsignee);

			var invoiceShipperFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.InvoiceShipper, ModuleIDs.Organisation, orgPK => GetJobDocAddressOfInvoiceQuery(DocAddressType.SupplierPickupDeliveryAddress, orgPK), Lookups.OrganisationList);
			invoiceShipperFilter.Category = FilterCategories.Organisations;
			invoiceShipperFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|InvoiceShipper", DeclarationFilterConstants.OrgFilterTypes.InvoiceShipper);

			var invoiceExporterFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.InvoiceExporter, ModuleIDs.Organisation, orgPK => GetJobDocAddressOfInvoiceQuery(DocAddressType.Exporter, orgPK), Lookups.OrganisationList);
			invoiceExporterFilter.Category = FilterCategories.Organisations;
			invoiceExporterFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|InvoiceExporter", DeclarationFilterConstants.OrgFilterTypes.InvoiceExporter);

			var invoiceManufacturerFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.InvoiceManufacturer, ModuleIDs.Organisation, orgPK => GetInvoiceAddInfoOrganizationQueryForManufactuer(orgPK), Lookups.OrganisationList);
			invoiceManufacturerFilter.Category = FilterCategories.Organisations;
			invoiceManufacturerFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|InvoiceManufacturer", DeclarationFilterConstants.OrgFilterTypes.InvoiceManufacturer);

			var invoiceOriginatorFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.InvoiceOriginator, ModuleIDs.Organisation, orgPK => GetJobDocAddressOfInvoiceQuery(DocAddressType.CommercialInvoiceOriginator, orgPK), Lookups.OrganisationList);
			invoiceOriginatorFilter.Category = FilterCategories.Organisations;
			invoiceOriginatorFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|InvoiceOriginator", DeclarationFilterConstants.OrgFilterTypes.InvoiceOriginator);
		}

		ZQuery GetCarrierCodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(JobDeclarationSchema.JE_CarrierCode, comparisonOperator, value);
			return result;
		}

		ZQuery GetInvoiceOrganizationQuery(SchemaColumn schemaColumn, ZGuid value)
		{
			ZQuery result = new ZQuery();

			ZDBOnlyQuery jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZDBOnlySubQuery invoiceSubQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			if (!value.IsEmpty)
			{
				invoiceSubQuery.AddToFilter(schemaColumn, value);
			}
			jobDeclarationQuery.AddSubQuery(invoiceSubQuery, JoinCondition.And);
			result.AddToFilter(jobDeclarationQuery);

			return result;
		}

		ZQuery GetInvoiceAddInfoOrganizationQueryForManufactuer(ZGuid value)
		{
			var result = new ZQuery();
			if (value.IsValid)
			{
				var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

				var invoiceSubQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ);
				var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);
				invoiceSubQuery.AddSubQuery(JobComInvoiceLineSchema.JI_OA_ManufacturerAddress, orgAddressSubQuery, JoinCondition.And);
				var invoiceHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				invoiceHeaderSubQuery.AddSubQuery(invoiceSubQuery, JoinCondition.And);

				var invoiceSubQuery2 = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				invoiceSubQuery2.AddToFilter(OrgAddressSchema.OA_OH, value);
				var invoiceHeaderSubQuery2 = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				invoiceHeaderSubQuery2.AddSubQuery(JobComInvoiceHeaderSchema.JZ_OA_ManufacturerAddress, invoiceSubQuery2, JoinCondition.And);

				jobDeclarationQuery.AddSubQuery(invoiceHeaderSubQuery, JoinCondition.And);
				jobDeclarationQuery.AddSubQuery(invoiceHeaderSubQuery2, JoinCondition.Or);
				result.AddToFilter(jobDeclarationQuery);
			}
			return result;
		}

		#region GetJobDocAddressQuery

		ZQuery GetJobDocAddressQuery(ZGuid value)
		{
			ZQuery result = new ZQuery();
			if (value.IsValid)
			{
				ZQuery docAddressQuery = new ZQuery(JobDocAddressSchema.PK, value);
				ZDBOnlyQuery jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				docAddressSubQuery.AddToFilter(docAddressQuery);
				jobDeclarationQuery.AddSubQuery(docAddressSubQuery, JoinCondition.And);

				result.AddToFilter(jobDeclarationQuery);
			}
			return result;
		}

		ZQuery GetJobDocAddressOfDeclarationQuery(DocAddressType addressType, ZGuid value)
		{
			ZQuery result = new ZQuery();
			if (value.IsValid)
			{
				ZDBOnlyQuery docAddressQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);
				if (addressType != DocAddressType.None)
				{
					docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, addressType));
				}

				docAddressQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

				ZDBOnlyQuery jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				docAddressSubQuery.AddToFilter(docAddressQuery);
				jobDeclarationQuery.AddSubQuery(docAddressSubQuery, JoinCondition.And);

				result.AddToFilter(jobDeclarationQuery);
			}
			return result;
		}

		ZQuery GetJobDocAddressOfInvoiceQuery(DocAddressType addressType, ZGuid value)
		{
			ZQuery result = new ZQuery();
			if (value.IsValid)
			{
				ZDBOnlyQuery docAddressQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, value);
				if (addressType != DocAddressType.None)
				{
					docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, addressType));
				}

				docAddressQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

				ZDBOnlyQuery jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				ZDBOnlySubQuery invoiceSubQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
				ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				docAddressSubQuery.AddToFilter(docAddressQuery);
				invoiceSubQuery.AddSubQuery(docAddressSubQuery, JoinCondition.And);
				jobDeclarationQuery.AddSubQuery(invoiceSubQuery, JoinCondition.And);

				result.AddToFilter(jobDeclarationQuery);
			}
			return result;
		}

		#endregion

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			base.AddNumberFilters(filters);
			AddTransactionNumberFilter(filters);

			filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.CCN, GetCCNQuery).WithMaxLengthOf<ModuleNumberFilter>(CusEntryNumSchema.CE_EntryNum)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|CCN", DeclarationFilterConstants.NumberFilterTypes.CCN);

			filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.ProofOfReportNumber, GetProofOfReportQuery).WithMaxLengthOf<ModuleNumberFilter>(CusEntryNumSchema.CE_EntryNum)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ProofOfReportNumber", DeclarationFilterConstants.NumberFilterTypes.ProofOfReportNumber);

			var difUrnFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.DIFURN, (sqloperator, value) => GetDIFQuery(JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber, value, sqloperator));
			difUrnFilter.MaxLength = JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber.MaxLength;
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotEqual);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotStartsWith);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.NotContain);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.IsBlank);
			difUrnFilter.ComparisonOperator_List.RemoveCode(ModuleNumberFilter.ComparisonConstants.IsNotBlank);
			difUrnFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|DIFURN", DeclarationFilterConstants.NumberFilterTypes.DIFURN);

			if(UniversalReferenceConstants.IsCarmR2)
			{
				var bondSuretyFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.BondSurety, GetSuretyCodeQuery);
				bondSuretyFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|BondSurety", DeclarationFilterConstants.NumberFilterTypes.BondSurety);

				var bondNumberFilter = filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.BondNumber, GetBondNumberQuery);
				bondNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|BondNumber", DeclarationFilterConstants.NumberFilterTypes.BondNumber);
			}
		}

		protected override ZQuery GetCustomCommonModuleFilterQueryCore(SQLComparisonOperator @operator, ZString value)
		{
			var notOrBlank = GetNotForInSubqueryIfNegative(@operator);
			@operator = @operator.GetNegatingSQLOperatorIfNotInSubquery();
			var entryNumberQuery = GetEntryNumberQuery(@operator, value);

			var sqlParameters = new ZSqlParameterCollection();

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var entryNumberString = entryNumberQuery.GetAsWhereClause(true).TrimEnd(')');
			entryNumberString = Regex.Replace(entryNumberString, "WHERE.+?" + JobDeclarationSchema.PK.Name + ".+?IN", JobDeclarationSchema.PK.Name + " " + notOrBlank + " IN");

			var queryString = string.Format(@"
			{0}
UNION ALL

SELECT CU_JE FROM dbo.CusDecHouseBill
WHERE {1}
AND ( CU_BillType = 'SH'
OR CU_BillType = 'HB')

UNION ALL

SELECT JE_PK FROM dbo.JobDeclaration
WHERE JE_MessageType = 'EXP' AND 
(JE_PK IN ( 
	SELECT CH_JE FROM dbo.CusEntryHeader
	WHERE CH_MessageType = 'DLM' AND 
	{2}) ) 

UNION ALL

SELECT JE_PK FROM dbo.JobDeclaration
WHERE {3})",
entryNumberString,
SQLAndParametersForOneCondition(@operator, value, CusDecHouseBillSchema.CU_BillNum, sqlParameters),
SQLAndParametersForOneCondition(@operator, value, CusEntryHeaderSchema.CH_BGMReference, sqlParameters),
SQLAndParametersForOneCondition(@operator, value, JobDeclarationSchema.JE_DeclarationReference, sqlParameters)
);

			result.AddFilterAndZSQLParameterCollection(queryString, sqlParameters);

			return result;
		}

		protected ZQuery GetCCNQuery(SQLComparisonOperator @operator, ZString value)
		{
			var result = new ZQuery();
			var declQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var isNegativeSqlOperator = @operator.IsNegativeSQLOperator() || @operator == SpecialComparisonOperator.IsBlank;

			var joinCondition = isNegativeSqlOperator ? JoinCondition.And : JoinCondition.Or;
			declQuery.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusEntryNumberQuery(JobDeclarationSchema.Constants.TableName, @operator, value), joinCondition);
			declQuery.AddSubQuery(CusEntryNumHelperForCargoControlNumber.GetCargoControlNumberFromCusAddInfoQuery(@operator, value), joinCondition);
			result.AddToFilter(declQuery);

			return result;
		}

		ZQuery GetBondTypeQuery(SQLComparisonOperator @operator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, ModelViewBondType, @operator, value);
		}

		ZQuery GetSuretyCodeQuery(SQLComparisonOperator @operator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, ModelViewSuretyCode, @operator, value);
		}

		ZQuery GetBondNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, ModelViewBondNumber, @operator, value);
		}

		ZQuery GetDIFQuery(SchemaStringColumn column, ZString value, SQLComparisonOperator @operator)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var docsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobDocsAndCartageSchema.JP_ParentID);
			var documentQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocument), JobRequiredDocumentSchema.EQ_ParentID);
			var addinfoQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocumentAddInfo), JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument);
			addinfoQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, column, @operator, value);
			documentQuery.AddSubQuery(addinfoQuery, JoinCondition.And);
			docsAndCartageQuery.AddSubQuery(documentQuery, JoinCondition.And);
			result.AddSubQuery(docsAndCartageQuery, JoinCondition.And);
			result.AddSubQuery(JobDeclarationSchema.JE_JS, docsAndCartageQuery, JoinCondition.Or);
			return result;
		}

		ZQuery GetDIFQuery(SchemaStringColumn column, ZString value)
		{
			return GetDIFQuery(column, value, SQLComparisonOperator.Equal);
		}

		protected void AddTransactionNumberFilter(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.TransactionNumber, GetEntryNumberQuery).WithMaxLengthOf<ModuleNumberFilter>(CusEntryNumSchema.CE_EntryNum)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|TransactionNumber", DeclarationFilterConstants.NumberFilterTypes.TransactionNumber);

			filters.AddNumberFilter(DeclarationFilterConstants.NumberFilterTypes.FormKeyNumber, GetFormKeyNumberQuery).WithMaxLengthOf<ModuleNumberFilter>(CusEntryHeaderSchema.CH_BGMReference)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|FormKeyNumber", DeclarationFilterConstants.NumberFilterTypes.FormKeyNumber);
		}

		protected ZQuery GetFormKeyNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery result = new ZQuery();
			result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Export);
			ZDBOnlyQuery dbOnlyResult = new ZDBOnlyQuery(typeof(JobDeclaration));
			ZDBOnlySubQuery subQueryResult = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			subQueryResult.AddToFilter(CusEntryHeaderSchema.CH_MessageType, MessageTypeList.Codes.DataLoadingModule);
			subQueryResult.AddToFilter_PossiblyCommaSeparated(CusEntryHeaderSchema.CH_BGMReference, @operator, value);
			dbOnlyResult.AddSubQuery(subQueryResult, JoinCondition.And);
			result.AddToFilter(dbOnlyResult);
			return result;
		}

		protected ZQuery GetProofOfReportQuery(SQLComparisonOperator @operator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var cusEntryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumberQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, CusEntryNumSchema.CE_EntryNum, @operator, value);
			cusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Canada);
			cusEntryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CanadaAdditionalReferenceNumberTypes.Codes.CTN);
			result.AddSubQuery(cusEntryNumberQuery, JoinCondition.And);
			return result;
		}

		protected override void AddShipmentSubTypeFilter(ModuleFilterCollection filters)
		{
			ModuleFilter shipSubTypeFilter = filters.AddTextFilter(DeclarationFilterConstants.EntryType, GetMessageSubTypeQuery, Lookups.MessageSubTypeList);
			shipSubTypeFilter.Category = FilterCategories.ModesAndTypes;
			shipSubTypeFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|EntryType", DeclarationFilterConstants.EntryType);
		}

		ZQuery GetMessageSubTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_MessageSubType, comparisonOperator, CADEntryTypeList.ConvertToShortCode(value));
		}

		protected override void AddMessageStatusFilter(ModuleFilterCollection filters)
		{
			ModuleFilter messageStatusFilter = filters.AddTextFilter(DeclarationFilterConstants.MessageStatus, GetCustomsMessageStatusQuery, Lookups.MessageStatusList)
				.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_MessageStatus);
			messageStatusFilter.Category = FilterCategories.StatusAndFlags;
			messageStatusFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|MessageStatus", DeclarationFilterConstants.MessageStatus);
		}

		#region MessageStatusFilter

		protected ZQuery GetCustomsMessageStatusQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (!value.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageStatus, value == DeclarationFilterConstants.EntryStatus.NotSentForFilter ? ZString.Empty : value);
			}
			return result;
		}

		#endregion

		protected override void AddSubmittedDate(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.EntrySubmittedDate, JobDeclarationSchema.JE_EntrySubmittedDate)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|EntrySubmittedDate", DeclarationFilterConstants.DateFilterTypes.EntrySubmittedDate);
		}

		protected override void AddImporterSupplierFilter(ModuleFilterCollection filters)
		{
			ModuleGuidsFilter exporterConsigneeFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.ExporterConsignee, ModuleIDs.Organisation, GetExporterConsigneeQuery, Lookups.Consignors, Lookups.Consignees);
			exporterConsigneeFilter.SetItemDescriptions(new ResourceStringData("", DeclarationFilterConstants.OrgFilterTypes.Exporter), new ResourceStringData("", DeclarationFilterConstants.OrgFilterTypes.Consignee));
			exporterConsigneeFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ExporterConsignee", DeclarationFilterConstants.OrgFilterTypes.ExporterConsignee);
		}

		#region GetExporterConsigneeQuery

		protected ZQuery GetExporterConsigneeQuery(ZGuid exporter, ZGuid consignee)
		{
			ZQuery result = GetConsigneeQuery(consignee);

			if (!exporter.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_OH_Supplier, exporter);
			}

			return result;
		}

		#endregion

		protected override void AddServiceProviderFilter(ModuleFilterCollection filters)
		{
			ModuleGuidsFilter shipplineLineForwarderFilter = filters.AddGuidFilter(DeclarationFilterConstants.OrgFilterTypes.CarrierServiceProvider, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_ShippingLine, Lookups.ShippingLines, JobDeclarationSchema.JE_OH_Forwarder, Lookups.Forwarders);
			shipplineLineForwarderFilter.SetItemDescriptions(new ResourceStringData("", DeclarationFilterConstants.OrgFilterTypes.Carrier), new ResourceStringData("", DeclarationFilterConstants.OrgFilterTypes.ServiceProvider));
			shipplineLineForwarderFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|CarrierServiceProvider", DeclarationFilterConstants.OrgFilterTypes.CarrierServiceProvider);
		}

		protected override void AddAdditionalDateFilters(ModuleFilterCollection filters)
		{
			base.AddAdditionalDateFilters(filters);

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ReleaseDate, JobDeclarationSchema.JE_EntryAuthorisationDate)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ReleaseDate", DeclarationFilterConstants.DateFilterTypes.ReleaseDate);

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.SubLocationETD, JobDeclarationSchema.JE_WarehouseReleaseDate)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|SubLocationETD", DeclarationFilterConstants.DateFilterTypes.SubLocationETD);

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.K84AccountingDate, GetK84AccountingDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|K84AccountingDate", DeclarationFilterConstants.DateFilterTypes.K84AccountingDate);

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.EstimatedPaymentDueDate, GetEstimatedPaymentDueDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|EstimatedPaymentDueDate", DeclarationFilterConstants.DateFilterTypes.EstimatedPaymentDueDate);

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.EntrySubmissionDate, GetEntrySubmissionDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|EntrySubmissionDate", DeclarationFilterConstants.DateFilterTypes.EntrySubmissionDate);

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ReleaseSubmissionDate, GetReleaseSubmissionDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ReleaseSubmissionDate", DeclarationFilterConstants.DateFilterTypes.ReleaseSubmissionDate);

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.EntryAcceptedDate, GetEntryAcceptedDateQuery)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|EntryAcceptedDate", DeclarationFilterConstants.DateFilterTypes.EntryAcceptedDate);

			filters.AddDateFilter(DeclarationFilterConstants.DateFilterTypes.ExportDate, JobDeclarationSchema.JE_ExportDate)
				.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ExportDate", DeclarationFilterConstants.DateFilterTypes.ExportDate);
		}

		ZQuery GetEstimatedPaymentDueDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var result = GenAddOnColumnHelper.SimpleQueryHelper.GetQueryOnGenAddOnColumn(CAAddInfoSchema.Constants.CA_EstimatedPaymentDueDate, comparisonOperator, startDate, endDate);

			result.AddToFilter(JobDeclarationSchema.JE_MessageType, new[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.LowValueShipments });

			return result;
		}

		ZQuery GetEntrySubmissionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return GetImportHeaderDateQuery(comparisonOperator, startDate, endDate, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration }, CusEntryHeaderSchema.CH_EntrySubmittedDate);
		}

		ZQuery GetReleaseSubmissionDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return GetImportHeaderDateQuery(comparisonOperator, startDate, endDate, new[] { MessageTypeList.Codes.EDIRelease }, CusEntryHeaderSchema.CH_EntrySubmittedDate);
		}

		ZQuery GetEntryAcceptedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return GetImportHeaderDateQuery(comparisonOperator, startDate, endDate, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration }, CusEntryHeaderSchema.CH_EntryReleaseDate);
		}

		ZQuery GetImportHeaderDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate, string[] messageTypes, SchemaColumn schemaColumn)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(JobDeclarationSchema.JE_MessageType, new[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.LowValueShipments });

			ZDBOnlySubQuery subQueryResult = null;

			if (comparisonOperator == DateComparisonOperator.HasDateEntered || comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				subQueryResult = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, comparisonOperator == DateComparisonOperator.HasNoDateEntered);

				subQueryResult.AddToFilter(schemaColumn, SQLComparisonOperator.NotEqual, null);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				subQueryResult = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);

				if (startDate.IsValid)
				{
					subQueryResult.AddToFilter(schemaColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate);
				}

				if (endDate.IsValid)
				{
					subQueryResult.AddToFilter(schemaColumn, SQLComparisonOperator.LessThan, endDate.Date.ToZDateTime().AddDays(1));
				}
			}

			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (subQueryResult != null)
			{
				subQueryResult.AddToFilter(CusEntryHeaderSchema.CH_MessageType, messageTypes);
				dbOnlyQuery.AddSubQuery(subQueryResult, JoinCondition.And);
			}

			filter.AddToFilter(dbOnlyQuery);
			return filter;
		}

		protected override void AddAdditionalLocationFilters(ModuleFilterCollection filters)
		{
			base.AddAdditionalLocationFilters(filters);

			var placeOfReport = filters.AddNkFilter(DeclarationFilterConstants.PortFilterTypes.PlaceOfReport, GetPlaceOfReportQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CBSAOffices);
			placeOfReport.Category = FilterCategories.Locations;
			placeOfReport.MaxLength = AddInfo.Schema.CA_PlaceOfReportMaxLength;
			placeOfReport.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|PlaceOfReport", DeclarationFilterConstants.PortFilterTypes.PlaceOfReport);

			var portOfExit = filters.AddNkFilter(DeclarationFilterConstants.PortFilterTypes.PortOfExit, GetPortOfExitQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CBSAOffices);
			portOfExit.Category = FilterCategories.Locations;
			portOfExit.MaxLength = AddInfo.Schema.CA_PortOfExitMaxLength;
			portOfExit.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|PortOfExit", DeclarationFilterConstants.PortFilterTypes.PortOfExit);

			var portOfUnlading = filters.AddNkFilter(DeclarationFilterConstants.PortFilterTypes.PortOfUnlading, GetPortOfUnladingQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CBSAOffices);
			portOfUnlading.Category = FilterCategories.Locations;
			portOfUnlading.MaxLength = AddInfo.Schema.CA_UnladingOfficeMaxLength;
			portOfUnlading.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|PortOfUnlading", DeclarationFilterConstants.PortFilterTypes.PortOfUnlading);

			var portOfClearance = filters.AddNkFilter(DeclarationFilterConstants.PortFilterTypes.PortOfClearance, GetPortOfClearanceQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CBSAOffices);
			portOfClearance.Category = FilterCategories.Locations;
			portOfClearance.MaxLength = JobDeclaration.Schema.JE_CustomsOfficeMaxLength;
			portOfClearance.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|PortOfClearance", DeclarationFilterConstants.PortFilterTypes.PortOfClearance);

			var subLocation = filters.AddNkFilter(DeclarationFilterConstants.PortFilterTypes.SubLocation, JobDeclarationSchema.JE_LocationOfGoods, ModuleIDs.Customs.CA.SubLocation, Lookups.SubLocationCodes);
			subLocation.Category = FilterCategories.Locations;
			subLocation.ForeignCodeColumnOverride = ZZRefCusCodeListCombinedSchema.ZZD_Code;
			subLocation.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|SubLocation", DeclarationFilterConstants.PortFilterTypes.SubLocation);
		}

		ZQuery GetPlaceOfReportQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.JE_ClusterKey, ModelViewClusterKey, ModelView, ModelViewPlaceOfReport, comparisonOperator, value.IsEmpty ? value : value.PadLeft(4, '0'));
		}

		ZQuery GetPortOfExitQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.JE_ClusterKey, ModelViewClusterKey, ModelView, ModelViewPortOfExit, comparisonOperator, value.IsEmpty ? value : value.PadLeft(4, '0'));
		}

		ZQuery GetPortOfUnladingQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(CAAddInfoSchema.Constants.CA_UnladingOffice, comparisonOperator, value.IsEmpty ? value : value.PadLeft(4, '0'));
		}

		protected override void AddModeFilters(ModuleFilterCollection filters)
		{
			base.AddModeFilters(filters);

			var serviceOption = filters.AddTextFilter(DeclarationFilterConstants.ServiceOption, ServiceOptionQuery, Lookups.ServiceOptions)
				.WithMaxLengthOf<ModuleTextFilter>(CAAddInfoSchema.CA_ServiceOption);
			serviceOption.Category = FilterCategories.ModesAndTypes;
			serviceOption.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ServiceOption", DeclarationFilterConstants.ServiceOption);
		}

		ZQuery ServiceOptionQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(CAAddInfoSchema.Constants.CA_ServiceOption, comparisonOperator, value);
		}

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				if (!Globals.IsWeb && !result.Params.Any(param => param.SchemaColumn.Name == JobDeclarationSchema.JE_MessageType.Name))
				{
					result.AddToFilter(JobDeclarationSchema.JE_MessageType, SQLComparisonOperator.NotEqual, JobMessageTypeList.Codes.LVSForConsolidation);
				}
				return result;
			}
		}

		public override MultilingualString EntryStatusText => ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ReleaseStatus", DeclarationFilterConstants.ReleaseStatus);

		protected override bool SupportWHSStatus
		{
			get { return true; }
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var filter = filters.AddTextFilter(DeclarationFilterConstants.ReleaseMessage, GetReleaseMessageStatusQuery, Lookups.MessageStatusListForFilter(MessageTypeList.Codes.EDIRelease))
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_Status);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ReleaseMessage", DeclarationFilterConstants.ReleaseMessage);

			AddReleaseStatusFilter(filters);

			filter = filters.AddTextFilter(DeclarationFilterConstants.EntryMessageStatus, GetEntryMessageStatusQuery, Lookups.MessageStatusListForFilter(null))
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_Status);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|EntryMessageStatus", DeclarationFilterConstants.EntryMessageStatus);

			filter = filters.AddTextFilter(DeclarationFilterConstants.OGDStatus, GetAVSStatusQuery, Lookups.AVSStatusCodes)
				.WithMaxLengthOf<ModuleTextFilter>(CAAddInfoSchema.CA_OGDStatus);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|OGDStatus", DeclarationFilterConstants.OGDStatus);

			filter = filters.AddTextFilter(DeclarationFilterConstants.ExceptionCode, GetExceptionCodeQuery, Lookups.ExceptionCodes)
				.WithMaxLengthOf<ModuleTextFilter>(GenAddOnColumnSchema.XA_Data);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|ExceptionCode", DeclarationFilterConstants.ExceptionCode);

			filter = filters.AddTextFilter(DeclarationFilterConstants.DIFMessageStatus, value => GetDIFQuery(JobRequiredDocumentAddInfoSchema.EX_Status, value), Lookups.DIFMessageStatusList)
				.WithMaxLengthOf<ModuleTextFilter>(JobRequiredDocumentAddInfoSchema.EX_Status);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|DIFMessageStatus", DeclarationFilterConstants.DIFMessageStatus);

			filter = filters.AddTextFilter(DeclarationFilterConstants.EXPStatus, GetEXPStatusQuery, Lookups.MessageStatusListForFilter(MessageTypeList.Codes.G7Export))
				.WithMaxLengthOf<ModuleTextFilter>(CusEntryHeaderSchema.CH_Status);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|EXPStatus", DeclarationFilterConstants.EXPStatus);

			var lpcoDIFURNFilter = filters.AddTextFilter(DeclarationFilterConstants.LPCODIFURN, (sqloperator, value) => GetLPCOQuery(sqloperator, value, CusCALPCOSchema.CLP_DIFRefNumberOrLocation));
			lpcoDIFURNFilter.WithMaxLengthOf<ModuleTextFilter>(CusCALPCOSchema.CLP_DIFRefNumberOrLocation).Category = FilterCategories.NumbersAndReferences;
			lpcoDIFURNFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			lpcoDIFURNFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			lpcoDIFURNFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			lpcoDIFURNFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			lpcoDIFURNFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			lpcoDIFURNFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|LPCODIFURN", DeclarationFilterConstants.LPCODIFURN);

			var lpcoRefNoFilter = filters.AddTextFilter(DeclarationFilterConstants.LPCORefNo, (sqloperator, value) => GetLPCOQuery(sqloperator, value, CusCALPCOSchema.CLP_RefNo));
			lpcoRefNoFilter.WithMaxLengthOf<ModuleTextFilter>(CusCALPCOSchema.CLP_RefNo).Category = FilterCategories.NumbersAndReferences;
			lpcoRefNoFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotEqual);
			lpcoRefNoFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			lpcoRefNoFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			lpcoRefNoFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			lpcoRefNoFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			lpcoRefNoFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|LPCORefNo", DeclarationFilterConstants.LPCORefNo);

			var accountingAgeFilter = filters.AddNumberRangeFilter(DeclarationFilterConstants.AccountingAge, GetAccountingAgeQuery);
			accountingAgeFilter.PropertyType = ZCalcEditPropertyType.Int;
			accountingAgeFilter.Category = FilterCategories.Other;
			accountingAgeFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|AccountingAge", DeclarationFilterConstants.AccountingAge);

			var csaReleaseOnlyFilter = filters.AddFlagsFilter(DeclarationFilterConstants.CSAReleaseOnly, new string[] { DeclarationFilterConstants.CSAReleaseOnly }, new GetFlagsQuery[] { GetCSAReleaseOnlyQuery });
			csaReleaseOnlyFilter.Property0 = true;
			csaReleaseOnlyFilter.Category = FilterCategories.StatusAndFlags;
			csaReleaseOnlyFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|CSAReleaseOnly", DeclarationFilterConstants.CSAReleaseOnly);

			if (UniversalReferenceConstants.IsCarmR2)
			{
				var bondTypeFilter = filters.AddTextFilter(DeclarationFilterConstants.BondType, GetBondTypeQuery, Lookups.BondTypeList);
				bondTypeFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|BondType", DeclarationFilterConstants.BondType);
				bondTypeFilter.Category = FilterCategories.ModesAndTypes;
				bondTypeFilter.ComparisonOperator_List.Clear();
				bondTypeFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.Exact);
				bondTypeFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.NotEqual);
				bondTypeFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.IsBlank);
				bondTypeFilter.ComparisonOperator_List.AddPair(ModuleNumberFilter.ComparisonConstants.IsNotBlank);

				var legcayB3MergedEntriesFilter = filters.AddFlagsFilter(DeclarationFilterConstants.B3NeedRemergeToCAD, new string[] { DeclarationFilterConstants.B3NeedRemergeToCAD }, new GetFlagsQuery[] { GetB3NeedRemergeToCADQuery });
				legcayB3MergedEntriesFilter.Property0 = true;
				legcayB3MergedEntriesFilter.Category = FilterCategories.StatusAndFlags;
				legcayB3MergedEntriesFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|B3NeedRemergeToCAD", DeclarationFilterConstants.B3NeedRemergeToCAD);
			}

			return filters;
		}

		protected override void AddEntryStatusFilter(ModuleFilterCollection filters)
		{
			var entryStatusFilter = new EntryStatusFilter(DeclarationFilterConstants.EntryStatusText,
				(ZString status) => GetEntryStatusQuery(status), Lookups.EntryStatusList, false, false);
			entryStatusFilter.WithMaxLengthOf<EntryStatusFilter>(CusEntryHeaderSchema.CH_EntryStatus);
			entryStatusFilter.Category = FilterCategories.StatusAndFlags;
			entryStatusFilter.MultilingualDescription = ResString.GetMultilingualString("CA|JobDeclarationFilterBusinessObject|EntryStatusText", DeclarationFilterConstants.EntryStatusText);

			filters.AddFilter(entryStatusFilter);
		}

		ZQuery GetEntryStatusQuery(ZString value)
		{
			var result = new ZQuery();
			var dbOnlyResult = new ZDBOnlyQuery(typeof(JobDeclaration));
			dbOnlyResult.AddToFilter(JobDeclarationSchema.JE_MessageType, new[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.LowValueShipments });
			var subQueryResult = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, value == ExtraConstantCodes.Codes.NotEntryAccepted);
			subQueryResult.AddToFilter(CusEntryHeaderSchema.CH_MessageType, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration });
			if (value != ExtraConstantCodes.Codes.NotEntryAccepted)
			{
				subQueryResult.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, value);
			}
			else
			{
				subQueryResult.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, new string[] { B3EntryStatusList.Codes.Accepted, B3EntryStatusList.Codes.Confirmed, CADEntryStatusList.Codes.Approved });
			}
			dbOnlyResult.AddSubQuery(subQueryResult, JoinCondition.And);

			result.AddToFilter(dbOnlyResult);
			return result;
		}

		void AddReleaseStatusFilter(ModuleFilterCollection filters)
		{
			var entryStatusfilter = filters.AddTextFilter(DeclarationFilterConstants.ReleaseStatus, GetReleaseStatusQuery, Lookups.ReleaseStatusList())
				.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_EntryStatus);
			entryStatusfilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			entryStatusfilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			entryStatusfilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			entryStatusfilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			entryStatusfilter.Category = FilterCategories.StatusAndFlags;
			entryStatusfilter.MultilingualDescription = EntryStatusText;
			entryStatusfilter.ComparisonOperatorChanged += CustomsEntryStatusFilter_ComparisonOperatorChanged;
		}

		ZQuery GetReleaseStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			return new ZQuery().AddToFilter(JobDeclarationSchema.JE_EntryStatus, filterOperator, value);
		}

		ZQuery GetCSAReleaseOnlyQuery(ZBool value)
		{
			var query = new ZQuery();
			if (value)
			{
				query = GenAddOnColumnHelper.SimpleQueryHelper.GetQueryOnGenAddOnColumn(CAAddInfoSchema.Constants.CA_CSAEntry, "Y");
			}
			return query;
		}

		ZQuery GetB3NeedRemergeToCADQuery(ZBool value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			if (value)
			{
				result.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
				var entryQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				entryQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, MessageTypeList.Codes.CommercialAccountingDeclaration);
				entryQuery.AddFilterAndZSQLParameterCollection(GetMultipleCusUnderbondDecFilter(), new ZSqlParameterCollection());
				result.AddSubQuery(entryQuery, JoinCondition.And);
			}
			return result;
		}

		ZString GetMultipleCusUnderbondDecFilter()
		{
			return @"CH_PK IN (SELECT CH_PK from CusEntryHeader
	JOIN CusUnderbondDec ON CH_ClusterKey = BU_ClusterKey
	WHERE CH_MessageType = 'CAD'
	GROUP BY CH_PK, BU_CL
	HAVING COUNT(BU_CL) > 1)";
		}

		ZQuery GetLPCOQuery(SQLComparisonOperator @operator, ZString value, SchemaStringColumn column)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var miscLPCOQuery = new ZDBOnlySubQuery(typeof(CusCALPCO), CusCALPCOSchema.CLP_ParentID);
			miscLPCOQuery.AddToFilter_PossiblyCommaSeparated(column, @operator, value);

			var invoiceQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			var invoiceLineQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_JZ);
			var pgaQuery = new ZDBOnlySubQuery(typeof(CusAddInfo), CusAddInfoSchema.B7_ParentID);
			var pgaLPCOQuery = new ZDBOnlySubQuery(typeof(CusCALPCO), CusCALPCOSchema.CLP_ParentID);
			pgaLPCOQuery.AddToFilter_PossiblyCommaSeparated(column, @operator, value);
			pgaQuery.AddSubQuery(pgaLPCOQuery, JoinCondition.And);
			invoiceLineQuery.AddSubQuery(pgaQuery, JoinCondition.And);
			invoiceQuery.AddSubQuery(invoiceLineQuery, JoinCondition.And);

			result.AddSubQuery(miscLPCOQuery, JoinCondition.And);
			result.AddSubQuery(invoiceQuery, JoinCondition.Or);
			return result;
		}

		ZDBOnlyQuery GetExceptionCodeQuery(ZString value)
		{
			var decQueryWithDate = new ZDBOnlyQuery(typeof(JobDeclaration));
			decQueryWithDate.AddToFilter(JobDeclarationSchema.JE_MessageType, new[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.LowValueShipments });
			var genAddOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, CAAddInfoSchema.Constants.CA_DeclarationException);
			if (value == ExtraConstantCodes.Codes.ExceptionNotBlank)
			{
				genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, value);
			}
			decQueryWithDate.AddSubQuery(JobDeclarationSchema.PK, GenAddOnColumnSchema.XA_ParentID, genAddOnQuery, JoinCondition.And);
			return decQueryWithDate;
		}

		ZQuery GetEntryMessageStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZQuery();
			var dbOnlyResult = new ZDBOnlyQuery(typeof(JobDeclaration));
			dbOnlyResult.AddToFilter(JobDeclarationSchema.JE_MessageType, new[] { JobMessageTypeList.Codes.Import, JobMessageTypeList.Codes.LowValueShipments });
			var subQueryResult = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, NotIn(filterOperator, value));
			subQueryResult.AddToFilter(CusEntryHeaderSchema.CH_MessageType, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration });
			subQueryResult = GetSubQueryForFilterOperator(subQueryResult, filterOperator, value);
			dbOnlyResult.AddSubQuery(subQueryResult, JoinCondition.And);
			result.AddToFilter(dbOnlyResult);
			return result;
		}

		ZQuery GetReleaseMessageStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZQuery();
			var dbOnlyResult = new ZDBOnlyQuery(typeof(JobDeclaration));
			var subQueryResult = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, NotIn(filterOperator, value));
			subQueryResult.AddToFilter(CusEntryHeaderSchema.CH_MessageType, MessageTypeList.Codes.EDIRelease);
			subQueryResult = GetSubQueryForFilterOperator(subQueryResult, filterOperator, value);
			dbOnlyResult.AddSubQuery(subQueryResult, JoinCondition.And);
			result.AddToFilter(dbOnlyResult);
			return result;
		}

		ZQuery GetEXPStatusQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var result = new ZQuery();
			var dbOnlyResult = new ZDBOnlyQuery(typeof(JobDeclaration));
			var subQueryResult = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE, NotIn(filterOperator, value));
			subQueryResult.AddToFilter(CusEntryHeaderSchema.CH_MessageType, MessageTypeList.Codes.G7Export);
			subQueryResult = GetSubQueryForFilterOperator(subQueryResult, filterOperator, value);
			dbOnlyResult.AddSubQuery(subQueryResult, JoinCondition.And);
			result.AddToFilter(dbOnlyResult);
			return result;
		}

		ZQuery GetAVSStatusQuery(ZString value)
		{
			return GenAddOnColumnHelper.SimpleQueryHelper.GetQueryOnGenAddOnColumn(CAAddInfoSchema.Constants.CA_OGDStatus, value);
		}

		ZQuery GetAccountingAgeQuery(INumericZType value1, INumericZType value2)
		{
			var from = value1.ToZInt();
			var to = value2.ToZInt();

			var list = new List<(ZString, SQLComparisonOperator, object)>
			{
				(ModelViewAccountingAge, SQLComparisonOperator.GreaterThanOrEqualTo, from),
				(ModelViewAccountingAge, SQLComparisonOperator.LessThanOrEqualTo, to)
			};

			var query = ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.JE_ClusterKey, ModelViewClusterKey, ModelView, false, list.ToArray());

			if (from.IsEmpty || to.IsEmpty)
			{
				var nullQuery = ModelViewColumnHelper.GetModuleFilterQueryForNull(JobDeclaration.Schema.JE_ClusterKey, ModelViewClusterKey, ModelView, false, ModelViewAccountingAge);
				query.AddToFilter(nullQuery, JoinCondition.Or);
			}

			return query;
		}

		ZDBOnlySubQuery GetSubQueryForFilterOperator(ZDBOnlySubQuery subQueryResult, SQLComparisonOperator filterOperator, ZString value)
		{
			if (value == Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter
				|| value == Common.Shared.MessageStatusList.Codes.Sent
				|| filterOperator == SQLComparisonOperator.IsBlank)
			{
				subQueryResult.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else
			{
				SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref filterOperator);
				subQueryResult.AddToFilter(CusEntryHeaderSchema.CH_Status, filterOperator, value);
			}
			return subQueryResult;
		}

		bool NotIn(SQLComparisonOperator filterOperator, ZString value)
		{
			return value == Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter
				? filterOperator == SQLComparisonOperator.Equal || filterOperator == SQLComparisonOperator.Contains || filterOperator == SQLComparisonOperator.StartsWith
				: filterOperator == SQLComparisonOperator.NotEqual || filterOperator == SQLComparisonOperator.NotContains || filterOperator == SQLComparisonOperator.DoesNotStartWith || filterOperator == SQLComparisonOperator.IsBlank;
		}
	}
}
