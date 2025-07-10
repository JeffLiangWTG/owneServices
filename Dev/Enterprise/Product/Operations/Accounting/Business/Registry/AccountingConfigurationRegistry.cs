using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing.KoreaSouth;
using Enterprise.Accounting.Business.Registry;
using Enterprise.Accounting.Business.Registry.PaymentCriticality;
using Enterprise.Accounting.Registry.Business.Accounting;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Compliance;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Registry;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Accounting.Integration.CommonUtils;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using Res = Enterprise.Accounting.Business.Res;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public sealed class AccountingConfigurationRegistry : RegistryItemSet
	{
		#region Construction

		public static AccountingConfigurationRegistry Instance
		{
			get { return instance ?? (instance = new AccountingConfigurationRegistry()); }
		}
		[ThreadStatic]
		static AccountingConfigurationRegistry instance;

		AccountingConfigurationRegistry()
		{
		}

		BusinessObjectFactory FactoryForCountryDefaultValues
			=> factoryForCountryDefaultValues ?? (factoryForCountryDefaultValues = new BusinessObjectFactory() { NameForDebugging = "AccountingConfigurationRegistry Factory for Country Defaults" });
		[ThreadStatic]
		static BusinessObjectFactory factoryForCountryDefaultValues;

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : AccountingMasterFilesRegistry.Categories
		{
			public static MultilingualString Accounting_JobInvoicing_InvoiceSplittingRules { get { return CombineCategories(Accounting_JobInvoicing, ResString.GetMultilingualString("C12C5AAD-C6EA-4BE9-B7BD-73D2B24A23CC", "Invoice Splitting Rules")); } }
			public static MultilingualString Accounting_JobInvoicing_ElectronicProcessingChargeManagement { get { return CombineCategories(Accounting_JobInvoicing, ResString.GetMultilingualString("0e81ccff-44e0-42cb-a517-10905c2ac1c7", "Electronic Processing Charge Management")); } }
			public static MultilingualString Accounting_JobInvoicing_BackDateARInvoices { get { return CombineCategories(Accounting_JobInvoicing, ResString.GetMultilingualString("C946B553-F007-4891-823E-9AD941C2B058", "Back Date AR Invoices")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments { get { return CombineCategories(Accounting_JobInvoicing, ResString.GetMultilingualString("d924a67b-0911-4835-9e01-1d1e41d9a6a9", "Default Departments")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_Gateway { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("F9E27A68-DDF3-4FDB-B5BA-1350C82D788E", "Gateway Billing")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_Forwarding { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("e25f1612-c4ff-4441-b2cd-b2f274aa8203", "Forwarding")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_DepartmentMapping { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("c2f30bc5-b10c-4eb9-aafe-544b29cee423", "Department Mapping")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_ShippingManager { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("423608a6-a6d0-4cde-8315-77687b98afaa", "Liner & Agency")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_Customs { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("abd230c9-b1bc-422f-a87f-47f267457874", "Customs")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_NCTS { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("1A343C74-C81A-45BD-9B4A-F89A99AF0D7B", "NCTS")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_CFS { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("c3a7f367-b99c-4314-a495-292d40a849f4", "CFS")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_Warehouse { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("9bff49bf-5c0a-4938-819d-e5bdb4e0e134", "Warehouse")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_Project { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("c5806049-291d-4138-98e3-3c07dbb31a74", "Project")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_CustomerServiceTicket { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("C29FEE08-D91F-4B63-818F-E81B409BF93D", "Customer Service Ticket")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_WorkItem { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("b39eedcf-9052-4cb8-9ad3-9715ec18df67", "Work Item")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_TransportBooking { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("115872AA-DAA3-44EE-A3D9-96FEB8EF7532", "Transport Booking")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_LandTransport { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("4cce4704-760b-4764-a18e-ea236edab582", "Land Transport")); } }
			public static MultilingualString Accounting_JobInvoicing_DisbursementChargeManagement { get { return CombineCategories(Accounting_JobInvoicing, ResString.GetMultilingualString("d27ed87e-4d23-4c83-a54e-f5bf91703f85", "Disbursement Charge Management")); } }
			public static MultilingualString Accounting_JobInvoicing_Rounding { get { return CombineCategories(Accounting_JobInvoicing, ResString.GetMultilingualString("7AC1F218-3376-48F5-BD14-D198B9770AE8", "Rounding")); } }
			public static MultilingualString Accounting_JobInvoicing_ProfitShare { get { return OrganisationRegistry.Categories.Accounting_JobInvoicing_ProfitShare; } }
			public static MultilingualString Accounting_JobInvoicing_JobProfitReason { get { return CombineCategories(Accounting_JobInvoicing, ResString.GetMultilingualString("8a6dcb56-28bc-4378-9327-7aac787e6b6b", "Job Profit Reason")); } }
			public static MultilingualString Accounting_VoucherDefaults { get { return CombineCategories(Accounting, ResString.GetMultilingualString("B00D576F-A460-4955-90D5-18B636EFE88C", "Voucher Defaults")); } }
			public static MultilingualString Accounting_VoucherDefaults_VoucherTransactionAppointedParties { get { return CombineCategories(Accounting_VoucherDefaults, ResString.GetMultilingualString("bbf6651b-cd21-4059-a312-47aa6e3e2111", "Voucher Transaction Appointed Parties")); } }
			public static MultilingualString Accounting_DataImport { get { return CombineCategories(Accounting, ResString.GetMultilingualString("CB5995D6-3E91-43c3-BAF5-B82F570701F2", "Data Import")); } }
			public static MultilingualString Accounting_Reversal { get { return CombineCategories(Accounting, ResString.GetMultilingualString("da99d2ee-de1c-4f29-a321-6169cf1387f2", "Reversal")); } }
			public static MultilingualString Accounting_ComPay { get { return CombineCategories(Accounting, ResString.GetMultilingualString("1D0BA6E6-0D7A-4265-BA64-B08084D1019D", "ComPay")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings_ImportAccrualsConfigurations { get { return CombineCategories(Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("D54FA65C-2308-4083-BB85-B65E125E116A", "Import Accruals Configurations")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings_PayInvoices { get { return CombineCategories(Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("5a551291-eba8-4f83-8d63-b7c1202261c6", "Pay Invoices")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings_HotCheck { get { return CombineCategories(Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("131fd6f4-a657-43a3-bc1a-24658cf058c1", "Hot Check")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings_HotCheck_HotCheckRequiredFields { get { return CombineCategories(Accounting_PayableDefaults_DefaultSettings_HotCheck, ResString.GetMultilingualString("5e7b3ab9-8a88-4370-b834-35014a0fca04", "Hot Check Required Fields")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings_UnapprovedInvoices { get { return CombineCategories(Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("1cda996e-8fda-4076-8b2e-671814084b4c", "Unapproved Invoices")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings_PaymentPrintDefaults { get { return CombineCategories(Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("98f5fac3-4940-45fe-8dcb-9f8acf92d027", "Payment Print Defaults")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings_PaymentPrintDefaults_DefaultPaymentPrintOption { get { return CombineCategories(Accounting_PayableDefaults_DefaultSettings_PaymentPrintDefaults, ResString.GetMultilingualString("9b77c444-0387-438d-85c5-5fcef252d79b", "Default Payment Print Option")); } }
			public static MultilingualString Accounting_PayableDefaults_DefaultSettings_CASS { get { return CombineCategories(Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("c49f3fed-e016-4682-b279-77b50f4102ca", "CASS")); } }
			public static MultilingualString Accounting_PayableDefaults_SelfBillingInvoice { get { return CombineCategories(Accounting_PayableDefaults, ResString.GetMultilingualString("25ee4f6b-4439-4a31-bda6-d15f36f9159d", "Self Billing Invoice")); } }
			public static MultilingualString Accounting_PayableDefaults_CostConfirmationDocument { get { return CombineCategories(Accounting_PayableDefaults, ResString.GetMultilingualString("007a599f-7356-46f0-9a83-5e26c6e7cd33", "Cost Confirmation Document")); } }
			public static MultilingualString Accounting_ReceivableDefaults_DefaultSettings_ReceiptDetail { get { return CombineCategories(Accounting_ReceivableDefaults_DefaultSettings, ResString.GetMultilingualString("2BBC0D6A-0AE9-4305-8471-B9C71CFA29B5", "Receipt Detail")); } }
			public static MultilingualString Accounting_ReceivableDefaults_DefaultSettings_ElectronicPaymentsDefaults { get { return CombineCategories(Accounting_ReceivableDefaults_DefaultSettings, ResString.GetMultilingualString("509aac30-9bdc-4ba1-958c-f767781c2af4", "Electronic Payments Defaults")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations, ResString.GetMultilingualString("930D4D48-9952-4f8e-B3A6-9E70C7E9208E", "Invoice")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("02741bd7-dbe7-4589-8920-ebd6fdaa732f", "Invoice Document Titles")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_AdjustmentNote { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles, ResString.GetMultilingualString("4ffc69e1-3dca-4ed5-8e49-f300a4391847", "Adjustment Note")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_CreditNote { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles, ResString.GetMultilingualString("45ba4234-146a-49b6-9c92-618bf2592ea7", "Credit Note")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_CreditNoteDisbursement { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles, ResString.GetMultilingualString("e0cd492b-369d-4320-88a5-1e048155e770", "Credit Note Disbursement")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_Invoice { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles, ResString.GetMultilingualString("b1e49733-4e5e-47ed-89ad-4bddcca8343d", "Invoice")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_InvoiceDisbursement { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles, ResString.GetMultilingualString("cafe1c1d-e79f-4389-bd25-1a981ca01983", "Invoice Disbursement")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("ff1cf7dc-0b11-46ba-b444-dbcfd34125b7", "Reference Display Items")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems, ResString.GetMultilingualString("798cd265-8860-4f37-91bb-94a7971135e2", "Forwarding")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_Consols { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding, ResString.GetMultilingualString("67b7c5f6-751c-4aef-9f29-595d2b9743fc", "Consols")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_CustomsDeclarations { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding, ResString.GetMultilingualString("ff306b65-8c23-4202-8587-47203b176462", "Customs Declarations")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems, ResString.GetMultilingualString("0d22bbc4-a82d-40ad-8fa6-3ec729ec6a10", "CFS&CTO")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO_LoadList { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO, ResString.GetMultilingualString("67f1488d-30fe-4437-bdcd-8e9eb488d6df", "Load List")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO_ShipmentsGatepass { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO, ResString.GetMultilingualString("321fb3ee-346b-49c7-a4d7-afcc294d9e2c", "Shipments & Gatepass")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_Invoice_TaxTransaction { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("45d81f79-8aa7-46a8-820a-b404d5b1d3b2", "Tax Transaction")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_CreditNote { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations, ResString.GetMultilingualString("834638d7-91ea-4fa6-9228-f52d054e9307", "Credit Note")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_StatementDefault { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations, ResString.GetMultilingualString("4816c762-0d43-470f-a07a-5b166a1396b3", "Statement Default")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations, ResString.GetMultilingualString("1dc873e5-2600-4a54-a1b8-a25b9f43d751", "Pro Forma Invoice")); } }
			public static MultilingualString Accounting_ReceivableDefaults_FormConfigurations_BatchInvoice { get { return CombineCategories(Accounting_ReceivableDefaults_FormConfigurations, ResString.GetMultilingualString("96412815-048d-4302-984b-17a2c646f911", "Batch Invoice")); } }
			public static MultilingualString Accounting_GeneralLedgerDefaults_GLJournalsApproval { get { return CombineCategories(Accounting_GeneralLedgerDefaults, ResString.GetMultilingualString("7aa8dc2c-eb69-4518-b881-ac190683ee6e", "GL Journals Approval")); } }
			public static MultilingualString Accounting_ClaimsandQueries { get { return CombineCategories(Accounting, ResString.GetMultilingualString("94fe69ee-bc7e-4d82-9640-2df92529bf7f", "Claims and Queries")); } }
			public static MultilingualString Accounting_CollectionCalls { get { return CombineCategories(Accounting, ResString.GetMultilingualString("87570865-F079-401a-AB32-ED7259E4ADD9", "Collection Calls")); } }
			public static MultilingualString Accounting_Framework { get { return CombineCategories(Accounting, ResString.GetMultilingualString("3dc84c9f-700b-43af-8a83-d71492fb797e", "Framework")); } }
			public static MultilingualString Accounting_Framework_ReportOrder { get { return CombineCategories(Accounting_Framework, ResString.GetMultilingualString("6d63592e-844b-417a-b6e9-c480ca23e1ef", "Report Order")); } }
			public static MultilingualString Accounting_Framework_ReportOrder_Global { get { return CombineCategories(Accounting_Framework_ReportOrder, ResString.GetMultilingualString("13588891-058a-45ae-8e0b-93a3ab9b2647", "Global")); } }
			public static MultilingualString Accounting_JobCostingDefaults { get { return CombineCategories(Accounting, ResString.GetMultilingualString("c3003463-3f0e-4a17-85be-e831183fe63e", "Job Costing Defaults")); } }
			public static MultilingualString Accounting_CashBookDefaults { get { return CombineCategories(Accounting, ResString.GetMultilingualString("444c72e7-552e-4b35-b642-ca3f9ffac697", "Cash Book Defaults")); } }
			public static MultilingualString Accounting_AccountingNextNumbers { get { return CombineCategories(Accounting, ResString.GetMultilingualString("d3e4779b-23e9-429e-8fae-861abf648026", "Accounting Next Numbers")); } }
			public static MultilingualString Accounting_AccountingNextNumbers_ReceivableNumbers { get { return CombineCategories(Accounting_AccountingNextNumbers, ResString.GetMultilingualString("Receivable Numbers", "Receivable Numbers")); } }
			public static MultilingualString Accounting_AccountingNextNumbers_ReceivableandPayable { get { return CombineCategories(Accounting_AccountingNextNumbers, ResString.GetMultilingualString("Receivable and Payable", "Receivable and Payable")); } }
			public static MultilingualString Accounting_AccountingNextNumbers_CashBookNumbers { get { return CombineCategories(Accounting_AccountingNextNumbers, ResString.GetMultilingualString("Cash Book Numbers", "Cash Book Numbers")); } }
			public static MultilingualString Accounting_AccountingNextNumbers_GovtTaxInvoiceReference { get { return CombineCategories(Accounting_AccountingNextNumbers, ResString.GetMultilingualString("2b4c12a9-dbc5-4c95-826a-1a3bec443676", "Govt Tax Invoice Reference")); } }
			public static MultilingualString Accounting_TransactionPaymentStatus { get { return CombineCategories(Accounting, ResString.GetMultilingualString("889eb6bb-a295-4a11-99f3-09b44fa2a40a", "Transaction Payment Status")); } }
			public static MultilingualString Accounting_Matching { get { return CombineCategories(Accounting, ResString.GetMultilingualString("FE8E5135-25A1-4083-BF6E-3C2BBF91F6D9", "Matching")); } }
			public static MultilingualString Accounting_CashBasisTaxConfiguration { get { return CombineCategories(Accounting, ResString.GetMultilingualString("E3759A17-29F8-48E8-AEB9-F28BE6B512E9", "Cash Basis Tax Configuration")); } }
			public static MultilingualString Accounting_CashAdvance { get { return CombineCategories(Accounting, ResString.GetMultilingualString("6CB953BC-AAD8-45FD-AAD3-B4955040A99D", "Advance Payments")); } }
			public static MultilingualString Accounting_CashAdvance_Receivables { get { return CombineCategories(Accounting_CashAdvance, ResString.GetMultilingualString("FB7C907A-7889-4EF2-A504-266EE6AF99B4", "Receivables")); } }
			public static MultilingualString Accounting_CashAdvance_Payables { get { return CombineCategories(Accounting_CashAdvance, ResString.GetMultilingualString("B89419EA-1E23-4340-81F5-7252D18EE66C", "Payables")); } }
			public static MultilingualString Accounting_PurchaseOrders { get { return CombineCategories(Accounting, ResString.GetMultilingualString("A2E52488-D46A-4F02-8084-4AC73382C9AF", "Purchase Orders")); } }
			public static MultilingualString Accounting_JobCostingReports { get { return CombineCategories(Accounting_JobCostingDefaults, ResString.GetMultilingualString("ab970035-a025-4495-bef0-051993a23806", "Job Costing Reports")); } }
			public static MultilingualString Accounting_AutoJobClosure { get { return CombineCategories(Accounting, ResString.GetMultilingualString("67092cf6-1ef0-4cf7-bba0-0d88fa50b020", "Automatic Job Closure Defaults")); } }
			public static MultilingualString Accounting_AutoCompactAccGLAggregate { get { return CombineCategories(Accounting, ResString.GetMultilingualString("9769482C-90CC-4F61-A027-5FB61FCAE371", "Compact General Ledger Aggregate Service Task")); } }
			public static MultilingualString Accounting_JobInvoicing_DefaultDepartments_ContainerYardJobs { get { return CombineCategories(Accounting_JobInvoicing_DefaultDepartments, ResString.GetMultilingualString("d8a9691a-351d-46fd-8fac-1227712bc1a6", "Container Yard Jobs")); } }
			public static MultilingualString Accounting_UKMTDPreviousPeriodInclusionRules { get { return CombineCategories(AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, ResString.GetMultilingualString("755db83b-e407-4c63-a48b-9548a12e2b5b", "UK MTD Previous Period Inclusion Rules")); } }
			public static MultilingualString Accounting_GovernmentComplianceInvoiceDocument_Argentina => CombineCategories(Accounting_GovernmentComplianceInvoiceDocument, ResString.GetMultilingualString("25AB1AC9-62EE-4B5B-8F15-3AD4D1F56A8D", "Argentina (AR)"));
			public static MultilingualString Accounting_GovernmentComplianceInvoiceDocument_Norway => CombineCategories(Accounting_GovernmentComplianceInvoiceDocument, ResString.GetMultilingualString("967C8CA2-5407-4200-863A-1D273A342DCA", "Norway (NO)"));
			public static MultilingualString Accounting_TaxConfigurations_Germany { get { return CombineCategories(AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations, ResString.GetMultilingualString("E7C07CF9-B3ED-4A30-9A90-8798D754E3BD", "Germany")); } }
			public static MultilingualString Accounting_PayablesInvoiceProcessingPortal { get { return CombineCategories(Accounting, ResString.GetMultilingualString("D5FCB340-F08A-45EE-9042-3DF2BFAFF7D7", "Payables Invoice Processing Portal")); } }
		}

		#endregion

		public BooleanRegistryItem CustomBranchDefaultingRulesEngineConfiguration
		{
			get
			{
				return GetItem("CustomBranchDefaultingRulesEngineConfiguration", delegate
				{
					return new BooleanRegistryItem(
						"CustomBranchDefaultingRulesEngineConfiguration",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("db60893b-b0c5-4550-bcfa-6e20c27b709d", "Custom Branch Defaulting Rules Engine Configuration"),
						ResString.GetMultilingualString("feca7c29-2557-4fd8-a718-99a4af550806", @"This registry will enable setting up Rules for Branch Defaulting in Rules Engine."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem CustomDepartmentDefaultingRuleEngineConfiguration
		{
			get
			{
				return GetItem("CustomDepartmentDefaultingRuleEngineConfiguration", delegate
				{
					return new BooleanRegistryItem(
						"CustomDepartmentDefaultingRuleEngineConfiguration",
						Categories.Accounting_JobInvoicing_DefaultDepartments,
						ResString.GetMultilingualString("319555b1-5b2c-47a4-8949-dd9b83ce01c0", "Custom Department Defaulting Rules Engine Configuration"),
						ResString.GetMultilingualString("7d40c765-ba07-476c-9b5d-0103646b543b", @"This registry will enable setting up Rules for Department Defaulting in Rules Engine."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem UseCurrentDateAsTransactionDateWhenAutoPosting
		{
			get
			{
				return GetItem("UseCurrentDateAsTransactionDateWhenAutoPosting", delegate
				{
					return new BooleanRegistryItem(
						"UseCurrentDateAsTransactionDateWhenAutoPosting",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_CreditNoteInvoiceReversalAuthorisationSetting,
						ResString.GetMultilingualString("ec4b9ed4-a98e-409d-9776-ffc425b1a968", "Use Current Date as Transaction Date When Auto Posting"),
						ResString.GetMultilingualString("83663bd4-40a7-45c0-ad1f-cddfabc5b51b", @"This registry applies when the Post Credit Note On Approval registry is set to Yes.

When this registry is set to Yes, when credit notes post on final approval, the transaction date of the credit note will be current date (the date of final approval).

When this registry is set to No, when the credit note posts (on final approval), the transaction date of the credit note will be the date that the approval request was created.

Note: This registry will be ignored when the 'Invoice and Post Dates Defaulting Behavior' registry is set to 'MTH'"),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem GenerateARInvoiceAttachmentForEReporting
		{
			get
			{
				return GetItem("GenerateARInvoiceAttachmentForEReporting", delegate
				{
					return new BooleanRegistryItem(
						"GenerateARInvoiceAttachmentForEReporting",
						Categories.Accounting_EReportingAndEInvoicingConfigurations,
						ResString.GetMultilingualString("A3276F3A-1450-4691-B2B3-D9DFE67C06FB", "Generate AR Invoice Attachment for E-Reporting"),
						ResString.GetMultilingualString("D185FD60-2B63-4B81-8422-B045D6218B15", @"This registry allows you to configure the system to automatically generate AR Invoice document during the E-Reporting batching process.
When enabled, an AR Invoice document PDF will be generated and attached to eDocs of the transaction, while the invoice is being batched for E-Reporting. 

Please note that only eDocs marked as ‘Published’ are included in E-Reporting files. To ensure that the AR Invoice is included in your E-Reporting mapped files, please review the setup of the INV – Invoice document. 
To do this, navigate to Maintain > Reference Files > Document Types module, locate the invoice document (filter by Document Type = INV) and ensure the ‘Published’ check box is selected.

If you enable this registry for a login company, please ensure that this company does not have other workflow triggers that would generate additional Invoice document attachments."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableLineLevelApprovalRequestForARCreditNote
		{
			get
			{
				return GetItem("EnableLineLevelApprovalRequestForARCreditNote", delegate
				{
					return new BooleanRegistryItem(
						"EnableLineLevelApprovalRequestForARCreditNote",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_CreditNoteInvoiceReversalAuthorisationSetting,
						ResString.GetMultilingualString("501c7144-4f2f-4a43-8e10-1394128e2bda", "Enable line-level credit note evaluation and approval"),
						ResString.GetMultilingualString("14dac907-0cbd-4582-9d6a-da306d6a7c1f", @"This registry allows you to enable evaluation and approval of credit notes at line level branch and department combination.
By default, this registry is set to NO. When posting credit notes, the system evaluates the required approval level, based on total amount of the credit note and the header branch and department.
Set this registry to YES if you want the system to additionally evaluate the required approval level, based on the total amounts for each branch and department combination at line level.
When enabled, the user will be able to post the credit note only if:
*They have sufficient approval level to header branch and department, based on the total amount of the credit.
*They have sufficient approval level to all branches and departments on the credit note, based on the total amount of each branch and department combination at line level.

For example, your system is configured to require Level 1 approval for all credit note amounts in branch B1, department D1; and Level 2 approval for credit notes in branch B2, department D2.
The user has Level 1 approval rights across all branches and departments.
This user is raising a credit note from the billing job with header branch B1 and department D1. The credit note contains a credit charge associated with branch B2 and department D2.
When this registry is set to NO, user is able to post the credit note. When this registry is set to YES, user is prevented from posting the credit note, since they don't have sufficient approval rights to post one of the line level charges.

If the posting user does not have sufficient approval level, the Security Override Login pop up screen is displayed and another user can provide on the spot authorization to post the credit note.
Alternatively, an approval request can be queued to be approved in the Credit Note Approval module. A separate approval request is created for the total amount of the credit note and the header level branch and department (parent request).
Additionally, a separate request is created for the total amount of each branch and department combination at line level (child requests).
Parent request can be approved only when all child requests are in the Approved status. Once parent request is Approved, then the credit note can be posted.
If any of the child requests gets Rejected, then the parent request is Rejected and the credit note cannot be posted."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public CodePairRegistryItem PrintTaxDateInARInvoiceDocument
		{
			get
			{
				return GetItem("PrintTaxDateInARInvoiceDocument", delegate
				{
					return new CodePairRegistryItem(
						"PrintTaxDateInARInvoiceDocument",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						ResString.GetMultilingualString("c1bde4d2-f334-472c-aac1-e0545191113f", "Print Tax Date in AR Invoice Document"),
						ResString.GetMultilingualString("be6bd357-3e0a-47db-99b6-67808a1591cc", @"This registry controls the ability to print the Tax Date (Date of Supply) in your AR Invoice. You can decide to print a single Tax Date in the Header or each charge line Tax Date in the body of the AR Invoice.
This is relevant when printing Receivables Tax Invoices, Tax Credit Notes and Tax Adjustment Notes.

By default, this Registry is set to 'NOT - Do Not Print Tax Date' and the receivables invoice document will not print the Date of Supply (Tax Date captured during transaction data entry) in the AR Invoice document. 

When overriding the value to 'HDR', the earliest Tax Date (Date of Supply) recorded against the charge lines of the transaction prints in the header of the AR Invoice document.

When overriding the value to 'HDT', the latest Tax Date (Date of Supply) recorded against the charge lines of the transaction prints in the header of the AR Invoice Document.

When overriding the value to 'BOD',  the Tax Date (Date of Supply) of each charge line prints at the beginning of the charge line description in the body of the AR Invoice Document."),
						new CodeDescriptionPairListProvider(() => PrintTaxDateInARInvoiceDocumentOption.CodeList),
						RegistryStorageFlags.Company,
						PrintTaxDateInARInvoiceDocumentOption.DoNotPrintTaxDate.Code);
				});
			}
		}

		public BooleanRegistryItem DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed
		{
			get
			{
				return GetItem("DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed", delegate
				{
					return new BooleanRegistryItem(
						new CountryEnabledBooleanRegistryItemImpl("DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed",
						Categories.Accounting,
						ResString.GetMultilingualString("142d31ab-2dee-455f-9e04-cc67e8b1319d", "Disallow reversing original transactions when amendments are not reversed"),
						ResString.GetMultilingualString("d366504b-b81a-4bca-8bb3-474a911b4c0c", @"This registry is used to control the behavior of the system when reversing original transactions that have been amended through the 'Amend with Credit Note' or 'Amend with Invoice' action in the Billing Tab of a Job

By default, this registry is enabled for Portugal Login Companies.

When enabled, the system will validate that all the amending documents of an original invoice are canceled before allowing users to cancel the original invoice itself."),
						RegistryStorageFlags.Company,
						Core.Constants.CountryCodes.Portugal)
						);
				});
			}
		}

		public BooleanRegistryItem PostCreditNoteOnApproval
		{
			get
			{
				return GetItem("PostCreditNoteOnApproval",
						delegate
						{
							return new BooleanRegistryItem("PostCreditNoteOnApproval",
								Categories.Accounting_ReceivableDefaults_DefaultSettings_CreditNoteInvoiceReversalAuthorisationSetting,
								ResString.GetMultilingualString("4ACD36F6-992B-41CD-8A92-F37A3794229C", "Post Credit Note On Approval"),
								ResString.GetMultilingualString("AD031FFA-0A96-4CDA-9144-AEBDDC8C86B6", @"When this registry is set to Yes, credit note approval requests with the following posting options will automatically post on final approval:
Post All Charges and Costs.
Post Overseas Agent Charges.
Post All Revenue Charges.
Post Charges for All Group Companies.
Post Charges for Group Companies in My Login Country.

On final approval of the credit note request, approval requests with one of the above options recorded as the Posting Option, will post all revenue charges from the job.
The creator of the credit note approval request will be recorded as the posting user.
If charges are added after the approval has been submitted, automatic posting will be canceled and posting will need to be done from the job. 

Approval requests with the following posting options need to be manually posted from the job:
Post Local Client Charges.
Post Disbursement Charges Only.

When this registry is set to No, on final approval of the credit note request, the user is notified, and the credit note must be posted from the job.

Note: When Enforce Two Credit Note Approvers registry is set to Yes, final approval is second approval."),
								RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, false);
						});
			}
		}

		public GuidRegistryItem EInvoicingErrorNotificationGroup
		{
			get
			{
				return GetItem("E-ReportingErrorNotificationGroup", delegate
				{
					var result = new GuidRegistryItem(
						(NoResString)"E-ReportingErrorNotificationGroup",
						Categories.Accounting_EReportingAndEInvoicingConfigurations,
						ResString.GetMultilingualString("cee09d08-bba7-49d4-9d53-911eae621c02", "E-Reporting Error Notification Group"),
						ResString.GetMultilingualString("2433bfaf-901d-456e-a1b9-8571d190f47f", "Notify Party when any accounting transaction failed to get successfully submitted for E-Reporting."),
						RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public EInvoicingCertificateExpiryNotificationGroupRegistryItem EInvoicingCertificateExpiryNotificationGroup
		{
			get
			{
				return GetItem("E-ReportingCertificateExpiryNotificationGroup", delegate
				{
					var result = new EInvoicingCertificateExpiryNotificationGroupRegistryItem(
						(NoResString)"E-ReportingCertificateExpiryNotificationGroup",
						Categories.Accounting_EReportingAndEInvoicingConfigurations,
						ResString.GetMultilingualString("4A873729-432F-45FC-9BAE-4A544A646027", "E-Reporting Certificate/Token Expiry Notification Group"),
						ResString.GetMultilingualString("1F1C678F-DD23-482A-B720-F395077D9733", @"Notify the party when the E-Invoicing Certificate/Token is approaching the expiry date.
By default, no notification will be sent. If required, you can nominate a group and configure the days to be alerted before the expiry date.
The system will send the notification as per configuration."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						new EInvoicingCertificateExpiryNotificationGroup());

					result.OnBuildLogReference += (args) =>
					{
						var newValue = args.NewValue as EInvoicingCertificateExpiryNotificationGroup;
						var newNotificationGroup = new BusinessObjectFactory().Load<GlbGroup>(newValue.NotificationGroup);
						return Res.GetString("8C75C179-8EE8-40E9-9F5B-36093182EDA3", "Set notification group [{0}] and [{1}] days.", newNotificationGroup?.GG_Code ?? "NULL", newValue.AlertDays);
					};
					return result;
				});
			}
		}

		public IntRegistryItem PeriodicBillingChargePostingPerformanceImprovementConfiguration
		{
			get
			{
				return GetItem("PeriodicBillingChargePostingPerformanceImprovementConfiguration", delegate
				{
					return new IntRegistryItem(
						"PeriodicBillingChargePostingPerformanceImprovementConfiguration",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("92dc51c4-9d3a-496c-a5f3-12b3a5a374ff", "Periodic billing charge posting performance improvement configuration"),
						ResString.GetMultilingualString("2a985dfc-ad5b-436e-bb2f-ad579b3cbfeb", @"This registry is used to configure the minimum number of charges which will trigger the special performance improvement functionality of periodic billing charge posting.

By default this registry is set to 500. When the total number of charges is greater than 500 the performance improvement will be applied."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport,
						500, 0, int.MaxValue);
				});
			}
		}

		public BooleanRegistryItem AllowSendingEInvoicingBatchWithError
		{
			get
			{
				return GetItem("AllowSendingEInvoicingBatchWithError",
						delegate
						{
							return new BooleanRegistryItem("AllowSendingEInvoicingBatchWithError",
								Categories.Accounting_EReportingAndEInvoicingConfigurations,
								(NoResString)"Allow sending EInvoicing batch with error (CargoWiseOne Support Only)",
								(NoResString)@"This registry is used to control the behaviour when sending EInvoicing batches.

By default this registry is not enabled, only transactions with 'BCH' status would be send to EHub.
When enabled, transactions with 'BER' status would also be sent.
NOTE: Please consult with the Accounting Product Team before turning on this registry.",
								RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
						});
			}
		}

		public StringRegistryItem VietnamEInvoicingUserName
		{
			get
			{
				return GetItem("VietnamEInvoicingUserName", delegate
				{
					return new StringRegistryItem("VietnamEInvoicingUserName",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("f78070aa-4396-42a8-bb1f-eb8dfefb57e1", "E-Invoicing Service Partner Connection User Name"),
						ResString.GetMultilingualString("dc26d334-f650-4a10-a057-66ac0d8da849", "This registry defines the User Name and Password to allow access to the Vietnam e-Invoicing service partner system.\r\nBoth User Name and Password must be specified."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						string.Empty)
					{
						DataType = new StringRegistryDataType(true)
					};
				});
			}
		}

		public StringRegistryItem VietnamEInvoicingPassword
		{
			get
			{
				return GetItem("VietnamEInvoicingPassword", delegate
				{
					return new StringRegistryItem("VietnamEInvoicingPassword",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("de210ab4-b1cf-4f9f-a54a-0023e76009f3", "E-Invoicing Service Partner Connection Password"),
						ResString.GetMultilingualString("8935e3a1-4bdb-4792-ae9b-5a0ac00a9c46", "This registry defines the Password to allow access to the Vietnam e-Invoicing service partner system.\r\nBoth User Name and Password must be specified."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						string.Empty)
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
						DataType = new StringRegistryDataType(true)
					};
				});
			}
		}

		public StringRegistryItem VietnamEInvoicingFormNumber
		{
			get
			{
				return GetItem("VietnamEInvoicingFormNumber", delegate
				{
					var item = new StringRegistryItem("VietnamEInvoicingFormNumber",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("d6f2c710-b09b-410e-a454-f95d8ee6c442", "E-Invoicing Form Number"),
						ResString.GetMultilingualString("388c1eed-8948-468e-8536-d123a6e141e6", "This registry defines the Form Number for Vietnam E-Invoicing."),
						 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						 string.Empty);

					item.DataType = new StringRegistryDataType(0, 11);

					return item;
				});
			}
		}

		public CodePairRegistryItem VietnamEInvoicingReceivingFileType
		{
			get
			{
				return GetItem("VietnamEInvoicingReceivingFileType", delegate
				{
					var item = new CodePairRegistryItem("VietnamEInvoicingReceivingFileType",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("cb9f15f2-805d-45e0-873e-e982f39e8385", "E-Invoicing Receiving File Type"),
						ResString.GetMultilingualString("a6d98f5d-62a1-4df8-b508-0d603cd2dacb", @"This registry is relevant to Vietnam Login Company only.
On successfully submission of electronic invoice (status = SUC), a request will be sent to intermediate service provider to request for a copy of the electronic invoice.
The file received will be attached to the respective invoice record's eDoc tab.
By default, the system is configured to receive a copy of the electronic invoice in PDF format.
If required, you can override this registry value and select a different file type from the available option."),
						new CodeDescriptionPairListProvider(() => AccountingConstants.VietnamEInvoicingReceivingFileTypeList),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						AccountingConstants.VietnamEInvoicingReceivingFileTypeCodes.PDF);

					item.OnBuildLogReference += (args) => Res.GetString("89977e58-3d8e-42af-943c-e30243b3b83d", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

					return item;
				});
			}
		}

		public BooleanRegistryItem VietnamEInvoicingAdjustment
		{
			get
			{
				var item = GetItem("EnableEInvoicingAdjustment", delegate
				{
					return new BooleanRegistryItem(
						"EnableEInvoicingAdjustment",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("47EBB947-2C0F-49CB-96D0-6323AF6504D9", "Issue Negative Adjustment via Amend with Credit Note"),
						ResString.GetMultilingualString("D7BE0490-1B56-4E8A-8A6F-F0B52A7B4F95", @"This registry is relevant to Vietnam Login Companies only and controls Negative Adjustment e-Invoicing functionality.
When the registry is enabled, Amendment Credit Notes created via 'Job Billing > AR Invoice > Amend with Credit Note' function can be used to Negatively Adjust Original Invoice amount once compliance number is allocated."),
						RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						false);
				});
				item.OnBuildLogReference += (args) => Res.GetString("992A8904-206A-40D2-B939-0AB46595148B", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
				return item;
			}
		}

		public BooleanRegistryItem VietnamExportAmountInWordsBasedOnInvoicedCurrency
		{
			get
			{
				var item = GetItem("ExportAmountInWordsBasedOnInvoicedCurrency", delegate
				{
					var totalvString = (NoResString)"totalv";
					var totalString = (NoResString)"total";

					return new BooleanRegistryItem(
						"ExportAmountInWordsBasedOnInvoicedCurrency",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("CA80B816-7276-42F7-A013-2FCA35C4C22E", "Export 'Amount in Words' based on Invoiced Currency"),
						ResString.GetMultilingualString("CCC87E23-5442-450B-9C01-8CCD676344FE", @"This registry defines whether the Amount in Words ('word') exported in the E-Invoicing Message is based on Local or Invoiced Currency.

By default, this registry is set to 'No' and the Total Amount Including Tax in Local Currency ('{0}') describes in words will be exported.
If required, you can override this value to 'Yes' and the Total Amount Including Tax in Invoiced Currency ('{1}') describes in words will be exported.", totalvString, totalString),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
				item.OnBuildLogReference += (args) => Res.GetString("1331BCC4-EEFB-4BF7-A12C-526E1D48DAFE", "Registry has been overridden from {0} to {1}.", args.OriginalValue, args.NewValue);
				return item;
			}
		}

		public BooleanRegistryItem VietnamAlwaysIssueElectronicInvoicesInLocalCurrency
		{
			get
			{
				var item = GetItem("AlwaysIssueElectronicInvoicesInLocalCurrency", delegate
				{
					return new BooleanRegistryItem(
						"AlwaysIssueElectronicInvoicesInLocalCurrency",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("E043DEEC-A3B1-4243-8C2D-326045D9E109", "Always Issue Electronic Invoices in Local Currency"),
						ResString.GetMultilingualString("DA4DDE43-6CBC-48E8-B511-4EC3F4EC2A1F", @"This registry is relevant to Vietnam Login Companies only.
By default, this is set to 'No' and the Electronic Invoices will be issued as in Invoiced Currency.
If required, you can override this registry value to 'Yes', and the Electronic Invoices will always be issued in Local Currency."),
						RegistryStorageFlags.Company,
						false);
				});
				item.OnBuildLogReference += (args) => Res.GetString("402AAEF1-9A17-4E91-AF2F-6723076F012D", "Registry has been overridden from {0} to {1}.", args.OriginalValue, args.NewValue);
				return item;
			}
		}

		public BooleanRegistryItem VietnamIssuePositiveAdjustmentViaAmendWithInvoice
		{
			get
			{
				var item = GetItem("VietnamIssuePositiveAdjustmentViaAmendWithInvoice", delegate
				{
					return new BooleanRegistryItem(
						"VietnamIssuePositiveAdjustmentViaAmendWithInvoice",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("8582E94A-88CC-427C-A4A5-830734E777B3", "Issue Positive Adjustment via Amend with Invoice"),
						ResString.GetMultilingualString("AC5344C4-4718-4ABE-B924-75D325D6E8C3", @"This registry is relevant to Vietnam Login Companies only and controls Positive Adjustment e-Invoicing functionality.
When the registry is enabled, Amendment Invoices created via 'Job Billing > AR Invoice > Amend with Invoice' function can be used to Positively Adjust Original Invoice amount once compliance number is allocated."),
						RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						false);
				});
				item.OnBuildLogReference += (args) => Res.GetString("DA6FBD0E-1C67-4DD2-A60E-AC556050FCBF", "Registry has been overridden from {0} to {1}.", args.OriginalValue, args.NewValue);
				return item;
			}
		}

		public BooleanRegistryItem EInvoicingSendType
		{
			get
			{
				var item = GetItem("EInvoicingSendType", delegate
				{
					return new BooleanRegistryItem(
						"EInvoicingSendType",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("AB2DC2E4-26F7-46B2-A9F0-79AD5B7ED5D6", "E-Invoicing Send Type"),
						ResString.GetMultilingualString("7A6E38C1-5CEA-474E-B9AE-FF54FB99AFB3", @"This registry defines whether e-invoices will be sent to Vietnam General Department of Taxation (""GDT"") individually or in batches.

By default, this registry is set to 'No' and e-Invoices will be automatically sent to GDT individually.
When this registry is overridden to 'Yes', you will need to manually batch the e-Invoices on {0} portal and send them in batches to GDT.", "FPT.eInvoice"),
						RegistryStorageFlags.Company,
						false);
				});
				item.OnBuildLogReference += (args) => Res.GetString("FE1F8028-AAC8-44A0-A0C4-2D1C65031C71", "Registry has been overridden from {0} to {1}.", args.OriginalValue, args.NewValue);
				return item;
			}
		}

		#region SuppressResourceStringsCheckRegion

		public CodePairRegistryItem VietnamEInvoicingErrorMessageLanguage
		{
			get
			{
				var languageListProvider = new CodeDescriptionPairListProvider(() =>
				{
					var languageList = new CodeDescriptionPairList();
					languageList.AddPair("vi", ResString.GetMultilingualString("5DD30DF5-5BA4-45B4-9FC8-75F0C983C25F", "Vietnamese"));
					languageList.AddPair("en", ResString.GetMultilingualString("BF38E3F1-1E7B-40A6-8417-AFBA3D13D22E", "English"));
					return languageList;
				});

				return GetItem("VietnamEInvoicingErrorMessageLanguage", delegate
				{
					return new CodePairRegistryItem(
						"VietnamEInvoicingErrorMessageLanguage",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("E1FF0D17-72AB-4A2D-836E-77997F5FB771", "Language of E-Invoicing Error Message Returned"),
						ResString.GetMultilingualString("A84E29AB-94B9-4D43-8988-D5BA903E7BBD", @"This registry defines the language of E-Invoicing error message returned from the service provider.
By default, the language is set to Vietnamese.
If required, user can change the language to English."),
						languageListProvider,
						RegistryStorageFlags.Company,
						"vi");
				});
			}
		}

		#endregion

		public UnitMeasurementTextOverrideRegistryItem UnitMeasurementTextOverride
		{
			get
			{
				return GetItem("UnitMeasurementTextOverride", delegate
				{
					var item = new UnitMeasurementTextOverrideRegistryItem(
						"UnitMeasurementTextOverride",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						ResString.GetMultilingualString("C2B1C048-12FF-46B0-9330-B7ABB388E06E", "Unit Measurement Text Override"),
						ResString.GetMultilingualString("0A8E72FF-795F-4C4E-ACD2-59394D100F71", @"This registry is relevant to Vietnam Login Companies where the E-Reporting functionality is enabled only.
By default, the system will export the unit measurement as documented in the Vietnam E-Invoicing mapping guide.
If required, you can configure the list of overrides and the system will use the overrides specified during the E-Invoicing transmission.

Note: Only rate units derived from Freight Consol, Shipment, and Customs Declarations that listed in the mapping guide are supported currently."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch)
					{
						CountryFilterPKs = new[] { Constants.CountryGuids.Vietnam }
					};

					item.OnBuildLogReference += AddUnitMeasurementTextOverride;
					return item;
				});
			}
		}

		public DateTimeRegistryItem PreventTheVietnamElectronicInvoiceFromBeingReversed
		{
			get
			{
				return GetItem("PreventTheVietnamElectronicInvoiceFromBeingReversed", delegate
				{
					var item = new DateTimeRegistryItem(
						"PreventTheVietnamElectronicInvoiceFromBeingReversed",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Vietnam,
						(NoResString)"Prevent the Vietnam Electronic Invoice from being reversed",
						(NoResString)@"This registry defines the effective date for restricting the reversal of Vietnamese e-Invoices.
By default, the effective date is set to June 1, 2025, aligning with Decree 70/2025/ND-CP (“Decree 70”), and the system will block invoice reversals from that date onward.
You may override the default date as needed to suit your specific requirements.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						new DateTime(2025, 6, 1)
					)
					{
						CountryFilterPKs = new[] { Constants.CountryGuids.Vietnam }
					};

					item.OnBuildLogReference += (args) => Res.GetString("B8D8B1C6-22AA-4AB9-A863-515DA2F2C675", "Registry has been changed from {0} to {1}.", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		string AddUnitMeasurementTextOverride(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			string result = "";
			var newCollection = ((UnitMeasurementTextOverrideCollection)args.NewValue).Cast<UnitMeasurementTextOverride>();
			var originalCollection = ((UnitMeasurementTextOverrideCollection)args.OriginalValue).Cast<UnitMeasurementTextOverride>();

			foreach (var configuration in originalCollection)
			{
				if (!newCollection.Any(x => x.UnitMeasurement == configuration.UnitMeasurement && x.TextOverride == configuration.TextOverride))
				{
					result += GetUnitMeasurementTextOverrideChangeLog((NoResString)"Deleted", configuration);
				}
			}

			foreach (var newConfiguration in newCollection)
			{
				if (!originalCollection.Any(x => x.UnitMeasurement == newConfiguration.UnitMeasurement && x.TextOverride == newConfiguration.TextOverride))
				{
					result += GetUnitMeasurementTextOverrideChangeLog((NoResString)"Added", newConfiguration);
				}
			}

			return result;
		}

		string GetUnitMeasurementTextOverrideChangeLog(string changeType, UnitMeasurementTextOverride unitMeasurementTextOverride)
		{
			return Res.GetString("08E43FE5-AF84-4156-BA13-8F9BCC6AC318", "Override {0}: {1}, {2}", changeType, unitMeasurementTextOverride.UnitMeasurement, unitMeasurementTextOverride.TextOverride) + "\r\n";
		}

		#region E-Invoicing Registries (India specific)

		public DateTimeRegistryItem IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom
			=> GetItem(nameof(IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom), () =>
				new DateTimeRegistryItem(nameof(IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_India,
					ResString.GetMultilingualString("f604e2b9-6c0a-427d-a1b6-bc97c4cdacc2", "Use Compliance Numbers instead of Transaction Numbers from"),
					ResString.GetMultilingualString("8dbf74d7-c835-4914-a4c5-b378505ea52e", "Set the date from which you want to send Compliance Numbers instead of transaction numbers for generating the E-Invoice JSON file."),
					RegistryStorageFlags.Company));

		#endregion

		#region E-Invoicing Registries (Korea specific)

		public CodeDescriptionPairListRegistryItem KoreaEInvoicingAmendmentStatusCode
		{
			get
			{
				return GetItem(nameof(KoreaEInvoicingAmendmentStatusCode), () =>
				new CodeDescriptionPairListRegistryItem(nameof(KoreaEInvoicingAmendmentStatusCode),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea,
					(NoResString)"Korea e-Invoicing Amendment Status Code(CargoWiseOne Support Only)",
					(NoResString)"This registry is used for Korea e-Invoicing amendment status code.",
					2,
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					KoreaEInvoicingAmendmentStatusCode_DefaultValue));
			}
		}

		CodeDescriptionPairList KoreaEInvoicingAmendmentStatusCode_DefaultValue
		{
			get
			{
				if (koreaEInvoicingAmendmentStatusCode_DefaultValue == null)
				{
					koreaEInvoicingAmendmentStatusCode_DefaultValue = new CodeDescriptionPairList();
					koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("01", ResString.GetMultilingualString("89CA6408-71A7-4B78-A011-75835C34C48E", "Mistakes or correction of entries or the tax rate is incorrectly applied"));
					koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("02", ResString.GetMultilingualString("49E6C7DE-1C6A-4B95-BCAB-1492AEF34D82", "Supply amount change"));
					koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("03", ResString.GetMultilingualString("D7D20B53-664E-412F-A63C-817D4DB2BE5C", "The goods supplied have been returned"));
					koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("04", ResString.GetMultilingualString("336119A8-6066-4B91-B728-CEF40C3BD65A", "Disengagement of a contract"));
					koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("05", ResString.GetMultilingualString("C541E09F-CF83-4486-9675-E743C61721C7", "Post-opening of domestic L/C"));
					koreaEInvoicingAmendmentStatusCode_DefaultValue.AddPair("06", ResString.GetMultilingualString("1A035A0C-499D-4E85-8D24-A358A35ED4D7", "Double issuance by mistake"));
				}
				return koreaEInvoicingAmendmentStatusCode_DefaultValue;
			}
		}
		CodeDescriptionPairList koreaEInvoicingAmendmentStatusCode_DefaultValue;

		public StringRegistryItem ElectronicInvoiceDocumentFallbackPassword
		{
			get
			{
				return GetItem("ElectronicInvoiceDocumentFallbackPassword", delegate
				{
					return new StringRegistryItem(
						"ElectronicInvoiceDocumentFallbackPassword",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea,
						ResString.GetMultilingualString("8B8FE14C-0424-4152-BB67-8479F1709BD4", "Electronic Invoice Document Fallback Password"),
						ResString.GetMultilingualString("F55EF451-7DAB-46CE-88BD-1B6C2DCC5089", "This registry defines the fallback password to be used to secured electronic invoice document in the event the invoice recipient's organization's business tax registration number cannot be identified."),
						new StringRegistryDataType(0, 32),
						RegistryStorageFlags.Company,
						string.Empty)
					{
						CountryFilterPKs = CountryFilterPKs.KoreaRepublicOf,
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password)
					};
				});
			}
		}

		public IntRegistryItem BackDateInvoiceDateDeadline
		{
			get
			{
				return GetItem("BackDateInvoiceDateDeadline", delegate
				{
					return new IntRegistryItem(
						"BackDateInvoiceDateDeadline",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea,
						ResString.GetMultilingualString("9EF5CD0C-7016-4583-9D58-E869DB222711", "Back Date Invoice Date Deadline (CargoWiseOne Support Only)"),
						ResString.GetMultilingualString("BC1DDF0A-EB0B-4C96-8A4B-76EA7ABC3DEB", @"Please specify the deadline (number of days from the beginning of the current month) where the system should allow invoice date to be back date to previous month.

For instance, if the value specified is 10, then if today is 10 April, then user will be able to back date the invoice date to 01 March to 31 March. If today is 11 April, then a validation error should be shown when invoice date is back dated to 01 March to 31 March.

If this registry is set to 0 then no validation will be enforced."),
						RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						0, 0, 26)
					{
						CountryFilterPKs = CountryFilterPKs.KoreaRepublicOf,
					};
				});
			}
		}

		public KoreaSouthEInvoicingDataElementConfigurationRegistryItem ElectronicInvoiceDataElementsConfiguration
		{
			get
			{
				return GetItem("ElectronicInvoiceDataElementsConfiguration", delegate
				{
					var defaultValue = new KoreaSouthEInvoicingDataElementConfigurationCollection
					{
						new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1, $"[외국인등록번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.ForeignerRegistrationNumber)}>][/][여권번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.PassportNumber)}>]"),
						new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2),
						new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.OriginalInvoice, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3),

						new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine1,$"[당초승인번호: <{nameof(KoreaSouthEInvoicingDataElementProvider.OriginalApprovalNumber)}>][/][당초작성일자: <{nameof(KoreaSouthEInvoicingDataElementProvider.OriginalApprovalDateForCode020304)}>]"),
						new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine2),
						new KoreaSouthEInvoicingDataElementConfiguration(EInvoicingKoreaSouthConstants.InvoiceTypeList.Amendment, EInvoicingKoreaSouthConstants.DataElementList.InvoiceDocumentHeaderDescriptionLine3)
					};

					var item = new KoreaSouthEInvoicingDataElementConfigurationRegistryItem(
						"ElectronicInvoiceDataElementsConfiguration",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea,
						ResString.GetMultilingualString("D3BC2447-020A-49D8-BFEC-95A2662FF514", "Electronic Invoice Data Elements Configuration"),
						ResString.GetMultilingualString("3756702B-EBDB-4EE9-9E00-4A7C46455C21", @"This registry is only relevant if '{0}' has been set to 'Yes'.
This registry enables you to configure the contents to be included in specific electronic invoice data elements.", AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Location()),
						RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						defaultValue)
					{
						CountryFilterPKs = CountryFilterPKs.KoreaRepublicOf,
					};

					item.OnBuildLogReference += BuildElectronicInvoiceDataElementsConfigurationLogReference;

					return item;
				});
			}
		}

		public BooleanRegistryItem EnableKoreaSouthEDIInterchangeCreator
		{
			get
			{
				return GetItem("EnableKoreaSouthEDIInterchangeCreator", delegate
				{
					return new BooleanRegistryItem(
						"EnableKoreaSouthEDIInterchangeCreator",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea,
						(NoResString)"Enable KoreaSouth EDI Interchange Creator (CargoWiseOne Support Only)",
						(NoResString)@"This registry defines whether the EDI Interchange can be created when running EKR service task.
This is a temporary registry for testing purposes.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItem DSBSummaryAppendingRule
		{
			get
			{
				return GetItem("DSBSummaryAppendingRule", delegate
				{
					var item = new KoreaSouthEInvoicingDSBSummaryAppendingRuleRegistryItem(
						"DSBSummaryAppendingRule",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Korea,
						ResString.GetMultilingualString("FD3F8C11-BEDE-48B5-9D2D-72633223F618", "Disbursement Fees Summary Appending Rule"),
						ResString.GetMultilingualString("34381560-5FF5-47CD-BF95-8B4A15145BED", @"This registry is only relevant to South Korean companies with the Receivables E-Reporting functionality enabled. 
It defines the rule for appending the disbursement fees ('대납금') summary to the remark ('비고') section of the e-Invoice. 
By default, the disbursement fees summary will not be appended to any tax invoice. 
If necessary, you can define the rule for appending the disbursement fees summary to a tax invoice based on business needs.

Note: 
1. Disbursement fees should be recorded with EXCLUDE tax ID.
2. A value of zero(0) means that the disbursement summary will not be appended to tax invoice(s) created for the said Tax ID. 
3. The system will append the disbursement summary to one of the tax invoice posted within the posting session based on the order specified. E.g. Order 1 will take priority over order 2."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default
						)
					{
						CountryFilterPKs = CountryFilterPKs.KoreaRepublicOf,
					};

					return item;
				});
			}
		}

		string BuildElectronicInvoiceDataElementsConfigurationLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var newValues = ((KoreaSouthEInvoicingDataElementConfigurationCollection)args.NewValue).Cast<KoreaSouthEInvoicingDataElementConfiguration>();
			var originalValues = ((KoreaSouthEInvoicingDataElementConfigurationCollection)args.OriginalValue).Cast<KoreaSouthEInvoicingDataElementConfiguration>();
			var changedValues = newValues.Where(newValue =>
			{
				return originalValues.Any(x => x.InvoiceType == newValue.InvoiceType && x.DataElement == newValue.DataElement && x.Configuration != newValue.Configuration);
			});

			var result = new ZStringBuilder();
			foreach (var changedValue in changedValues)
			{
				result.AppendLine($"{changedValue.InvoiceType}: {changedValue.DataElement} set to '{changedValue.Configuration}'");
			}
			return result.ToString();
		}

		#endregion

		public BooleanRegistryItem EnableAutomaticChargeCodeMappingForUnallocatedInvoices
		{
			get
			{
				return GetItem("EnableAutomaticChargeCodeMappingForUnallocatedInvoices", delegate
				{
					return new BooleanRegistryItem(
						"EnableAutomaticChargeCodeMappingForUnallocatedInvoices",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("09c33f6f-c9b1-41aa-a4ee-a89ec14bc2d2", "Enable Automatic Charge Code Mapping For Unallocated Invoices"),
						ResString.GetMultilingualString("b07b5f92-1124-49c4-9043-47789d1df46f", @"This registry is used when importing AP Invoices using the Universal Transaction schema. 
These invoices are imported as Transactions Pending Allocation. By default, the system will automatically map the ‘Charge Code’ value in the XML to the charge code an operator selects. 
This mapping will be updated every time an invoice is posted. 
Set this registry to ‘No’ if you want to disable this automatic mapping."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem EnableAutomaticOrganizationCodeMappingForUnallocatedInvoices
		{
			get
			{
				return GetItem("EnableAutomaticOrganizationCodeMappingForUnallocatedInvoices", delegate
				{
					return new BooleanRegistryItem(
						"EnableAutomaticOrganizationCodeMappingForUnallocatedInvoices",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("58329EAC-99D5-4F31-8B27-2F0FBE7D43B7", "Enable Automatic Organization Code Mapping For Unallocated Invoices"),
						ResString.GetMultilingualString("E2BCD60A-364F-4AE3-A89B-99496CA1437A", @"This registry is used when importing AP invoices using the Universal Transaction schema (XUT).
These invoices are imported as Transactions Pending Allocation (TPA). By default, the system will automatically map the 'Organization Code' value in the XML to the corresponding Organization Proxy found in Details > Config > EDI Code Mapping.
This mapping will be updated every time a change is made to the organization code on an invoice that is posted."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#region Payment and Receipt Type Reference Number Registry Defaults

		public PaymentReceiptTypeReferenceNumberRegistryItem PaymentReceiptTypeReferenceNumberRegistryDefaults
		{
			get
			{
				return GetItem("PaymentReceiptTypeReferenceNumber", delegate
				{
					return new PaymentReceiptTypeReferenceNumberRegistryItem("PaymentReceiptTypeReferenceNumber",
							Categories.Accounting,
							ResString.GetMultilingualString("0f6c85bb-02f6-4efc-99c4-923cc18cf05b", "Default Payment / Receipt Reference Number"),
							ResString.GetMultilingualString("d6fd87f0-a095-4566-b7a7-1e59178e5b35", @"The Registry setting allows you to configure a default reference number for all receipt and payment types. You can nominate the reference number for all new transactions in the below grid.

These values will default into the Reference Number field when creating a new transaction.
As the reference number for ‘CHQ’ type will always be unique, the system will not support a default reference for this receipt and payment type.

Note: Where the same transactions type can be used in Receipts and Payment the same reference number value will be used."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							RegistryOptions.Default,
							PaymentReceiptTypeReferenceNumberDefaultValueGetter
						);
				});
			}
		}

		public static object PaymentReceiptTypeReferenceNumberDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var defaultValuesShouldContainEPAPaymentType = companyPK != Guid.Empty && ObjectFactory.Get<IAccounting>().IsEPaymentFunctionalityEnabledForAnyProvider(companyPK);

			var defaultValues = new PaymentReceiptTypeReferenceNumberCollection();
			var paymentList = new CodeDescriptionPairList(OLookUpEditType.PaymentMethod);
			if (paymentList.ContainsCode(ReceiptTypes.EPayment) && !defaultValuesShouldContainEPAPaymentType)
			{
				paymentList.RemoveCode(ReceiptTypes.EPayment);
			}
			else if (!paymentList.ContainsCode(ReceiptTypes.EPayment) && defaultValuesShouldContainEPAPaymentType)
			{
				paymentList.AddPair(ReceiptTypes.EPayment, ResString.GetMultilingualString("e013cebe-ee88-49c0-93d1-da5e06a1d51a", "E-Payment"));
			}

			var receiptList = new CodeDescriptionPairList(OLookUpEditType.ReceiptMethod);
			paymentList.AddRange(receiptList);
			string[] codes = paymentList.ToArray().Where(x => x.Code != ReceiptTypes.Cheque).Select(x => x.Code).Distinct().ToArray();

			defaultValues.AddDefaultValues(codes);

			return defaultValues;
		}

		public ZString GetReferenceNumberFromType(string type)
		{
			return PaymentReceiptTypeReferenceNumberRegistryDefaults.Value.GetValueFromType(type);
		}

		#endregion

		#region Transaction Type Prefix

		public TransactionTypePrefixRegistryItem TransactionTypePrefix
		{
			get
			{
				var item = GetItem("TransactionTypePrefix", delegate
				{
					return new TransactionTypePrefixRegistryItem("TransactionTypePrefix",
							Categories.Accounting,
							ResString.GetMultilingualString("db5265f9-866e-4ff3-9129-c4aad6fbf668", "Transaction Type Prefix"),
							ResString.GetMultilingualString("47c3e463-15c8-4419-b39b-02939543d02e", @"A new element 'Transaction Type Prefix' has been added to the Accounting > Number Sequence Customization.

This registry enables you to specify the prefix by ledger and transaction type for inclusion to transaction number and internal reference.
When specified and the 'Transaction Type Prefix' element is included in the transaction number, then this prefix will be added to the transaction number."),
							RegistryStorageFlags.Company
						);
				});

				item.OnBuildLogReference += BuildTransactionTypePrefixChangeLogReference;
				return item;
			}
		}

		string BuildTransactionTypePrefixChangeLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			string result = "";
			var newConfigurationCollection = ((TransactionTypePrefixCollection)args.NewValue).Cast<TransactionTypePrefix>();
			var originalConfigurationCollection = ((TransactionTypePrefixCollection)args.OriginalValue).Cast<TransactionTypePrefix>();

			foreach (var configuration in originalConfigurationCollection)
			{
				if (!newConfigurationCollection.Any(x => x.Ledger == configuration.Ledger && x.TransactionType == configuration.TransactionType))
				{
					result += GetPrefixChangeLog((NoResString)"Deleted", configuration);
				}
			}

			foreach (var newConfiguration in newConfigurationCollection)
			{
				if (!originalConfigurationCollection.Any(x => x.Ledger == newConfiguration.Ledger && x.TransactionType == newConfiguration.TransactionType))
				{
					result += GetPrefixChangeLog((NoResString)"Added", newConfiguration);
				}

				if (originalConfigurationCollection.Any(x => x.Ledger == newConfiguration.Ledger && x.TransactionType == newConfiguration.TransactionType && x.Prefix != newConfiguration.Prefix))
				{
					result += GetPrefixChangeLog((NoResString)"Edited", newConfiguration);
				}
			}

			return result;
		}

		string GetPrefixChangeLog(string changeType, TransactionTypePrefix trasanctionTypePrefix)
		{
			return Res.GetString("87e23d6c-38a1-442b-be78-28e7e03fe884", "{0}: Ledger '{1}', Transaction Type '{2}', Prefix '{3}'", changeType, trasanctionTypePrefix.Ledger, trasanctionTypePrefix.TransactionType, trasanctionTypePrefix.Prefix) + "\r\n";
		}

		#endregion

		public BooleanRegistryItem AllowBackPostingSubLedgerTransaction
		{
			get
			{
				return GetItem("AllowBackPostingSubLedgerTransaction", delegate
				{
					return new AllowBackPostingSubLedgerTransactionRegistryItem(
						"AllowBackPostingSubLedgerTransaction",
						Categories.Accounting,
						ResString.GetMultilingualString("AFD2A07A-35A0-4d09-A768-598C90CA406F", "Allow Back Posting Sub Ledger Transaction"),
						ResString.GetMultilingualString("70d1a377-a998-4a0d-9c1d-6faa51098913", @"Setting this registry to ‘Yes’ allows users to back post Sub Ledger Transactions to previous open accounting periods in the Accounting modules.

Note: This registry will be ignored when the ‘Invoice and Post Dates Defaulting Behavior’ registry is set to ‘MTH’."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem AllowForwardDatingofAPInvoiceDate
		{
			get
			{
				return GetItem("AllowForwardDatingofAPInvoiceDate", delegate
				{
					return new BooleanRegistryItem(
						"AllowForwardDatingofAPInvoiceDate",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("4FE71EA5-1223-434C-A643-434EF9B726E9", "Allow Forward Dating of AP Invoice Date"),
						ResString.GetMultilingualString("DAD09A73-A76E-4C65-877F-424E5F12B6B0", "Set this registry to 'Yes' to allow forward dating of AP Invoices"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem AllowZeroValueARInvoices
		{
			get
			{
				return GetItem("AllowZeroValueARInvoices", delegate
				{
					return new BooleanRegistryItem("AllowZeroValueARInvoices",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("70c38f7f-37bb-46fc-bfa2-b01a163c30d5", "Allow Posting of Zero Value AR Invoices"),
						ResString.GetMultilingualString("5e68ef0a-abef-4214-a413-8afd7c4e89c2", "Set this registry to 'No' to prevent users from posting AR Invoices, Credit Notes and Adjustment Notes that have a total value of zero."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem EditPaymentAddress
		{
			get
			{
				return GetItem("EditPaymentAddress", delegate
				{
					return new BooleanRegistryItem("EditPaymentAddress",
						Categories.Accounting,
						ResString.GetMultilingualString("8aa3d951-048b-49ca-90be-b9ed478649b9", "Edit AR/AP Payment Address"),
						ResString.GetMultilingualString("87b5e59e-b45c-4f30-86f2-17476e233257", "Setting this registry to 'Yes' will allow users to edit the address and contact on AR/AP Payments."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public DecimalRegistryItem ExporterExemptionCellingLimitThreshold
		{
			get
			{
				return GetItem("ExporterExemptionCellingLimitThreshold", delegate
				{
					return new DecimalRegistryItem(
						"ExporterExemptionCellingLimitThreshold",
						Categories.Accounting,
						ResString.GetMultilingualString("79EAAC87-5D51-44D8-B95C-830E7F93B12E", "Exporter Exemption Celling Limit Threshold"),
						ResString.GetMultilingualString("2FAD577C-507B-4336-9D70-B0F777963A82", @"Threshold expressed as a percentage of Exporter Exemption Ceiling Limit.
If the threshold is exceeded, a warning message will be displayed against the debtor or creditor field depending on the document type in the Billing tab, Consol Costing, AR Invoice and AP Invoice. 

This threshold is only relevant to Italy login companies. 

By default, the threshold is set to 0% in which case no warning message will be displayed.
If required, please specify a threshold of up to 100%."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						0, 0, 100);
				});
			}
		}

		public DecimalRegistryItem ThresholdValidationFCEElectronicCreditInvoice
		{
			get
			{
				return GetItem("ThresholdValidationFCEElectronicCreditInvoice", delegate
				{
					DecimalRegistryItem item = new DecimalRegistryItem(
						"ThresholdValidationFCEElectronicCreditInvoice",
						Categories.Accounting_GovernmentComplianceInvoiceDocument_Argentina,
						ResString.GetMultilingualString("4C1A62B2-F7A3-4F3D-A21E-7CAC7EA115F3", "Threshold validation for FCE-Electronic Credit Invoice"),
						ResString.GetMultilingualString("614860F7-61C3-4E6F-A7EB-F8DE104405BF", "This registry is relevant to Argentina Login companies. It allows you to specify the minimum amount, expressed in ARS, that is required to issue FCE-Electronic Credit Invoices ({0} Regime). When a Compliance Sub Type Attribution Rule Set that supports FCE-Electronic Credit Invoices is selected, if an invoice is issued to an Organization that adhered to the {0} Regime and this threshold is met, a FCE related compliance sub type will be defaulted.\r\nThe threshold only applies for FCE Invoices, but not for FCE Debit and Credit Notes.", "MiPyme"),
						new NumericRegistryEditorInfo(2),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						299555.00M, 0, double.MaxValue);
					item.CountryFilterPKs = CountryFilterPKs.Argentina;
					return item;
				});
			}
		}

		public GuidRegistryItem AlternateChartOfAccountsForSAFT
		{
			get
			{
				var isSAFTv130FeatureEnabled = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingSAFT13Report) != null;

				var item = GetItem("AlternateChartOfAccountsForSAFT", () => new GuidRegistryItem(
					"AlternateChartOfAccountsForSAFT",
					Categories.Accounting_GovernmentComplianceInvoiceDocument_Norway,
					ResString.GetMultilingualString("B0D075EA-CCD4-4822-9D99-DC6220980BB0", "Alternate Chart of Accounts for SAF-T"),
					ResString.GetMultilingualString("F56E5B81-657C-44C3-8504-20200BCB0ABC", "Please select the Alternate Chart of Accounts for the Norwegian SAF-T report."),
					new GLAccountSelectionAndEntryGuidRegistryDataType(),
					RegistryStorageFlags.Company,
					Guid.Empty)
				{
					EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccAlternateChart),
					CountryFilterPKs = CountryFilterPKs.Norway,
					Options = isSAFTv130FeatureEnabled ? RegistryOptions.IsValueOptional : RegistryOptions.IsHidden
				}
				);

				item.OnBuildLogReference += (args) =>
				{
					var factory = new BusinessObjectFactory();
					var originalValue = factory.Load<AccAlternateChart>(new ZGuid(args.OriginalValue));
					var newValue = factory.Load<AccAlternateChart>(new ZGuid(args.NewValue));
					return Res.GetString("5BF34659-6557-488E-AD2D-1A89CB117A64", "Alternate Chart of Account Code For SAF-T changed from [{0}] to [{1}].", originalValue?.AAC_Code ?? ZString.Empty, newValue?.AAC_Code ?? ZString.Empty);
				};

				return item;
			}
		}

		public BooleanRegistryItem DisplayARStatementAgeingFields
		{
			get
			{
				return GetItem("DisplayARStatementAgeingFields", delegate
				{
					return new BooleanRegistryItem(
						"DisplayARStatementAgeingFields",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_StatementDefault,
						(NoResString)"Display Aging Fields (CargoWiseOne Support Only)",
						(NoResString)@"When this registry is set to 'YES', authorized users will be able to customize the Receivables Statement of Account to include the aging information in the statement.


NOTE: Please consult with the Accounting Product Team before turning on this registry.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem ResetAddressInChargeWhenARInvoiceReversed
		{
			get
			{
				return GetItem("ResetAddressInChargeWhenARInvoiceReversed", delegate
				{
					return new BooleanRegistryItem(
						"ResetAddressInChargeWhenARInvoiceReversed",
						Categories.Accounting,
						(NoResString)"Clear Sell Address in Charge lines when AR Invoice is reversed (CargoWiseOne Support Only)",
						(NoResString)@"When this registry is set to 'YES', Sell Address will be cleared in Charge line(s) after reversing the AR Invoice.


NOTE: Please consult with the Accounting Product Team before turning on this registry.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public AutomaticProcessRegistryItem AllowAutomaticSubLedgerTakeup
		{
			get
			{
				return GetItem("AllowAutomaticSubLedgerTakeup", delegate
				{
					var item = new AutomaticProcessRegistryItem("AllowAutomaticSubLedgerTakeup"
						, Categories.Accounting
						, ResString.GetMultilingualString("6dff17cd-b6a2-4f3e-94f5-050b0c3f105d", "Allow Scheduling Automatic Sub-Ledger (A/R & A/P) Take up")
						, ResString.GetMultilingualString("75813512-0c26-4b0d-948b-84f1568a6ec1", @"This registry enables to configure automatic take up of Sub-Ledger (A/R & A/P). 
If this registry is configured then the system will automatically run GL Take up at the specified time through 'ATU' service task (The next time to run is shown as local time for a Branch with the earliest Time Zone)")
						, RegistryStorageFlags.Company, true);

					item.OnBuildLogReference += BuildAllowAutomaticSubLedgerTakeupSetupLogReference;

					return item;
				});
			}
		}

		string BuildAllowAutomaticSubLedgerTakeupSetupLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var result = string.Empty;

			var originalValue = (AutomaticProcessRegistryBusinessObject)args.OriginalValue;
			var newValue = (AutomaticProcessRegistryBusinessObject)args.NewValue;
			if (originalValue.NextRunDateTime != newValue.NextRunDateTime)
			{
				result += Res.GetString("c4cb0e57-d67a-4635-9d87-0cbfb6d2def7", "Next Run Time has been set to {0}.", newValue.NextRunDateTime) + "\r\n";
			}

			if (originalValue.Interval != newValue.Interval || originalValue.IntervalType != newValue.IntervalType)
			{
				result += Res.GetString("5ee4b6f2-c999-4fb0-a636-59ae9250af44", "Interval has been set to {0} {1}.", newValue.Interval, newValue.IntervalType);
			}

			return result;
		}

		public BooleanRegistryItem AutoPostPaymentsOnceFullyApprovedWhenPostingCosts
		{
			get
			{
				return GetItem("AutomaticallyPostPaymentApprovalsOnceFullyApproved",
					() => new BooleanRegistryItem(
						"AutomaticallyPostPaymentApprovalsOnceFullyApproved",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("056ddf32-93fb-47d0-aa1e-0efdf9a99112", "Auto Post Payments once Fully Approved When Posting Costs"),
						ResString.GetMultilingualString("369003a4-9f24-4333-8353-2747b14803e4", @"By default this registry is set to ‘Yes’.  This means that {0} will automatically POST each Payment (PAY) transaction as soon as it is fully approved.  Approving a Payment transaction automatically posts that transaction.
When overridden and set to ‘No’, the ‘Approve Payment’ step is specifically separated from the ‘Post Payment’ step.   Payment transactions must be specifically selected and posted through the Payment Processing module.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true));
			}
		}

		public PeriodReopenLevelsRegistryItem PeriodReopenLevels
		{
			get
			{
				return GetItem("PeriodReopenLevels", delegate
				{
					return new PeriodReopenLevelsRegistryItem(
											"PeriodReopenLevels",
											Categories.Accounting,
											ResString.GetMultilingualString("3E7B2128-5545-4420-886D-A05C34580A1D", "Period Reopen Levels"),
											ResString.GetMultilingualString("c5785739-59a4-44d5-9f55-5addf1295681", @"By default, closed accounting periods cannot be reopened. 
When overridden, this registry can be used to enable the re-opening of accounting periods.
This registry can be used to nominate the number of days past an accounting period's end date that a user with the relevant security right can reopen a closed accounting period."),
											RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
				});
			}
		}

		public BooleanRegistryItem EnableLightValidationForChargeAndConsolCost
		{
			get
			{
				return GetItem("EnableLightValidationForChargeAndConsolCost", delegate
				{
					return new BooleanRegistryItem("EnableLightValidationForChargeAndConsolCost",
							Categories.Accounting,
							(NoResString)"Enable Light Validation For Charge And Consol Cost (CargoWiseOne Support Only)",
							(NoResString)@"By default light validation is disabled for consol cost and charge.
In case of emergency where it is impossible for a client to continue usual operations with light validation disabled, turn on this registry to enable light validation and make the system behave as before.",
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							false);
				});
			}
		}

		public BooleanRegistryItem EnablePostTransactionCalculatedPropertyCache
		{
			get
			{
				return GetItem("EnablePostTransactionCalculatedPropertyCache", delegate
				{
					return new BooleanRegistryItem("EnablePostTransactionCalculatedPropertyCache",
							Categories.Accounting,
							(NoResString)"Enable Post Transaction Calculated Property Cache",
							(NoResString)"Enable this registry to cache those heavy calculated properties of Accounting transactions. It will significantly improve the performance when posting Accounting transactions.",
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							false);
				});
			}
		}

		public BooleanRegistryItem EnableValidationForChargeWhenPostTransactions
		{
			get
			{
				return GetItem("EnableValidationForChargeWhenPostTransactions", delegate
				{
					return new BooleanRegistryItem("EnableValidationForChargeWhenPostTransactions",
							Categories.Accounting,
							(NoResString)"Enable Validation For Charge When Post Transactions (CargoWiseOne Support Only)",
							(NoResString)@"By default charge validation is enabled when client post revenue or cost.
In case of emergency where it is impossible for a client to continue usual operations with validation enabled, turn off this registry to disable charge validation and make the system behave as before.",
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							true);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion
		public BooleanRegistryItem EnableUsersAuthorisedToReopenClosedPeriodsRegistry
		{
			get
			{
				return GetItem("EnableUsersAuthorisedToReopenClosedPeriodsRegistry", delegate
				{
					return new BooleanRegistryItem(
						"EnableUsersAuthorisedToReopenClosedPeriodsRegistry",
						Categories.Accounting,
						(NoResString)"Enable Users Authorized to Reopen Closed Periods (CargoWiseOne Support Only)",
						(NoResString)@"When this registry is set to 'YES', 'Users Authorized to Reopen Closed Periods' Regitry will be visible to the user. Otherwise it will be visible only to CWSupport. 


NOTE: Please consult with the Accounting Product Team before turning on this registry.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableSelfAdministrationToReopenClosedPeriods
		{
			get
			{
				return GetItem("EnableSelfAdministrationToReopenClosedPeriods", delegate
				{
					return new BooleanRegistryItem(
						"EnableSelfAdministrationToReopenClosedPeriods",
						Categories.Accounting,
						(NoResString)"Enable self-administration to reopen closed periods",
						(NoResString)"When this registry is set to 'YES', Self administration, the new approval form will be displayed. Otherwise as existing behavior, request key form will be displayed.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public UsersAuthorizedToReopenClosedPeriodsRegistryItem UsersAuthorizedToReopenClosedPeriods
		{
			get
			{
				return GetItem("UsersAuthorizedToReopenClosedPeriods", delegate
				{
					return new UsersAuthorizedToReopenClosedPeriodsRegistryItem(
											"UsersAuthorizedToReopenClosedPeriods",
											Categories.Accounting,
											"Users Authorized to Reopen Closed Periods",
											@"CargoWise support staff can enable the Reopening of Closed Accounting Periods for a login company. 
Only the users nominated in this list will be allowed to re-open accounting periods.
After enabling this registry, Period Reopen Levels must be defined and appropriate staff security levels assigned.",
											RegistryStorageFlags.System | RegistryStorageFlags.Company,
											!EnableUsersAuthorisedToReopenClosedPeriodsRegistry.Value);
				});
			}
		}

		#endregion

		#region Local Financial Year End Date

		public IntRegistryItem IndiaGSTReversalAllowedPeriod
		{
			get
			{
				return GetItem("IndiaGSTReversalAllowedPeriod", () =>
					new IntRegistryItem("IndiaGSTReversalAllowedPeriod",
						Categories.Accounting,
						(NoResString)"India GST Reversal Allowed Period (CargoWiseOne Support Only)",
						(NoResString)@"By default the government allows crediting GST 8 months after Financial year end date in India (31-March). However in special circumstances, the government might change this value. Change this value after consulting with the government notification as a temporary measure till we can make permanent changes to CW.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						8, 0, int.MaxValue));
			}
		}

		#endregion

		#region Voucher of China Defaults.
		#region Voucher Transaction Appointed Parties.

		public ZString GetVoucherAppointedPartiesDefault(Guid comPK, Guid bhPK, Guid depPK, ZString partiesCode)
		{
			ZGuid staffPK;
			switch (partiesCode)
			{
				case AccountingConstants.VoucherAppointedPartiesCode.VoucherCasher:
					staffPK = VoucherAppointedPartiesCashierDefault.GetFallBackValueAtAllLevels(comPK, bhPK, depPK);
					break;
				case AccountingConstants.VoucherAppointedPartiesCode.VoucherReviewer:
					staffPK = VoucherAppointedPartiesReviewerDefault.GetFallBackValueAtAllLevels(comPK, bhPK, depPK);
					break;
				default:
					throw new ApplicationException(string.Format("Get default value for VoucherAppointedPartiesCode {0} is not supported", partiesCode));
			}

			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbStaff staff = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, staffPK));
			if (staff != null)
			{
				return (ZString)staff[GlbStaffSchema.GS_FullName];
			}
			return ZString.Empty;
		}

		public GuidRegistryItem VoucherAppointedPartiesReviewerDefault
		{
			get
			{
				return GetItem("VoucherAppointedPartiesReviewerDefault", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("VoucherAppointedPartiesReviewerDefault",
						Categories.Accounting_VoucherDefaults_VoucherTransactionAppointedParties,
						ResString.GetMultilingualString("bcf6651b-cd21-4059-a312-47aa6e3e21ad", "Default Voucher Reviewer"),
						ResString.GetMultilingualString("ea347b49-9b71-424d-b7b3-44d6195a14ad", @"Voucher Reviewer of system default."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsValueMandatory | (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan ? RegistryOptions.Default : RegistryOptions.IsHidden));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff);
					return result;
				});
			}
		}

		public GuidRegistryItem VoucherAppointedPartiesCashierDefault
		{
			get
			{
				return GetItem("VoucherAppointedPartiesCashierDefault", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("VoucherAppointedPartiesCashierDefault",
						Categories.Accounting_VoucherDefaults_VoucherTransactionAppointedParties,
						ResString.GetMultilingualString("bbf6651b-cd21-4659-a312-47aa6e3e21ae", "Default Voucher Cashier"),
						ResString.GetMultilingualString("8a347b49-9b71-434d-b7b3-44d6195a14ae", @"Voucher Cashier of system default."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsValueMandatory | (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China || GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan ? RegistryOptions.Default : RegistryOptions.IsHidden));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff);
					return result;
				});
			}
		}

		#endregion

		#region Voucher Transaction Description and Number of Supporting Document Defaults

		public DefaultNumberOfSupportingDocumentsRegistryItem VoucherNumberOfSupportingDocumentDefaults
		{
			get
			{
				return GetItem("VoucherDescriptionAndNoOfAttachments", delegate
				{
					DefaultNumberOfSupportingDocumentsCollection defaultValues = LookUpCodeDescriptionNumberPairList();
					return new DefaultNumberOfSupportingDocumentsRegistryItem("VoucherDescriptionAndNoOfAttachments",
							Categories.Accounting,
							ResString.GetMultilingualString("80A25F3C-905F-4AE2-BE47-CE623E86D728", "Transaction Description Defaults"),
							ResString.GetMultilingualString("50098621-2BB3-4097-9881-D2DECA634B1E", @"This registry setting allow you to configure the default values for all new transactions.
You can nominate the default value for the transaction description in the grid below.

These values will be defaulted into the 'Description' fields when creating a new transaction.
Note: Users can override these default values when posting a transaction."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							RegistryOptions.Default,
							defaultValues
						);
				});
			}
		}

		internal DefaultNumberOfSupportingDocumentsCollection LookUpCodeDescriptionNumberPairList()
		{
			DefaultNumberOfSupportingDocumentsCollection defaultValues = new DefaultNumberOfSupportingDocumentsCollection();
			defaultValues.AddDefaultValues(LedgerTypes.General + TransactionTypes.GLStandardJournal, ResString.GetMultilingualString("cb201a74-7376-41ae-9cf3-b6a1e88f5a04", "General Ledger Journal"), ResString.GetMultilingualString("cb201a74-7376-41ae-9cf3-b6a1e88f5a04", "General Ledger Journal"), 1);

			defaultValues.AddDefaultValues(LedgerTypes.CashBook + TransactionTypes.DirectReceipt, ResString.GetMultilingualString("04f3ee08-7af8-4728-b6ce-5c822372539f", "Cash Book Direct Receipt"), ResString.GetMultilingualString("33f3ee08-7af8-4728-b6ce-5c822372539f", "Cash Book Direct Receipt"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.CashBook + TransactionTypes.DirectPayment, ResString.GetMultilingualString("a4f3ee08-7af8-4728-b6ce-5c822372539f", "Cash Book Direct Payment"), ResString.GetMultilingualString("36f3ee08-7af8-4728-b6ce-5c822372539f", "Cash Book Direct Payment"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.CashBook + TransactionTypes.Transfer, ResString.GetMultilingualString("b4f3ee08-7af8-4728-b6ce-5c822372539f", "Cash Book Transfer"), ResString.GetMultilingualString("39f3ee08-7af8-4728-b6ce-5c822372539f", "Cash Book Transfer"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.CashBook + TransactionTypes.ExchangeDifference, ResString.GetMultilingualString("c4f3ee08-7af8-4728-b6ce-5c822372539f", "Cash Book Exchange Difference"), ResString.GetMultilingualString("38f3ee08-7af8-4728-b6ce-5c822372539f", "Cash Book Exchange Difference"), 1);

			defaultValues.AddDefaultValues(LedgerTypes.AccountsReceivable + TransactionTypes.Contra, ResString.GetMultilingualString("068d9647-148a-4021-be5e-0c3da7d65150", "Receivable and Payable Contra"), ResString.GetMultilingualString("188d9647-148a-4021-be5e-0c3da7d65150", "Receivable and Payable Contra"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsReceivable + TransactionTypes.Invoice, ResString.GetMultilingualString("168d9647-148a-4021-be5e-1c3da7d65150", "Receivable Invoice (Miscellaneous)"), ResString.GetMultilingualString("178d9647-148a-4021-be5e-ec3da7d65150", "AR Invoice"), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.JobARInvoice, ResString.GetMultilingualString("BC0D4217-3770-4F22-924F-B17EAB981357", "Receivable Invoice (Job)"), (NoResString)"", 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.ConsolARInvoice, ResString.GetMultilingualString("340D4217-2370-ea22-564F-B17EAB981366", "Receivable Invoice (Consol)"), ResString.GetMultilingualString("34da305f-f49f-45a4-8959-e4f32b4179a1", "Freight Consol Invoice"), 1);

			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.ARPeriodicInvoice, ResString.GetMultilingualString("A114CCAE-4BCE-434E-A440-490B11CFE1DF", "Receivable Invoice (Periodic)"), ResString.GetMultilingualString("43184083-B4C3-4F81-8B67-13789CAC268B", "AR Periodic Invoice"), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.BadDebtWriteOff, ResString.GetMultilingualString("2487a915-b3a4-4cc7-bb0b-def77a103d10", @"Receivable Write Off As Bad Debt"), ResString.GetMultilingualString("8487a915-b3a4-4cc7-bb0b-def77a103d10", @"Bad Debt Write-Off"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsReceivable + TransactionTypes.CreditNote, ResString.GetMultilingualString("368d9647-148a-4021-be5e-2c3da7d65150", "Receivable Credit Note (Miscellaneous)"), ResString.GetMultilingualString("168d9647-148a-4021-be5e-0c3da7d65152", "AR Credit Note"), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.JobARCreditNote, ResString.GetMultilingualString("AB0D4217-3770-4F22-924F-B17EAB98F127", "Receivable Credit Note (Job)"), (NoResString)"", 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.ConsolARCreditNote, ResString.GetMultilingualString("ef0D4217-ed70-4782-764F-ef7EAB98Fac7", "Receivable Credit Note (Consol)"), ResString.GetMultilingualString("cc0D4217-cc70-b782-b64F-bf7EAB98Facf", "Freight Consol Invoice"), 1);

			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.ARPeriodicCreditNote, ResString.GetMultilingualString("923AEBFE-F835-4CB2-BD53-3972F954FCCB", "Receivable Credit Note (Periodic)"), ResString.GetMultilingualString("1402AB40-B920-4F1A-888E-9524FC1FFDD2", "AR Periodic Credit Note"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsReceivable + TransactionTypes.AdjustmentNote, ResString.GetMultilingualString("468d9647-148a-4021-be5e-3c3da7d65150", "Receivables Adjustment Note"), ResString.GetMultilingualString("158d9647-148a-4021-be5e-0c3da7d65153", "AR Adjustment Note"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsReceivable + TransactionTypes.Journal, ResString.GetMultilingualString("E98FD264-C59B-447B-9651-B7F516B43EFD", "Receivables Journal"), ResString.GetMultilingualString("D73725B2-2160-40B0-9E61-1556C7BAE84F", "AR Journal"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsReceivable + TransactionTypes.Payment, ResString.GetMultilingualString("568d9647-148a-4021-be5e-4c3da7d65150", "Receivables Payment"), ResString.GetMultilingualString("148d9647-148a-4021-be5e-0c3da7d65154", "AR Payment"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsReceivable + TransactionTypes.Receipt, ResString.GetMultilingualString("668d9647-148a-4021-be5e-5c3da7d65150", "Receivables Receipt"), ResString.GetMultilingualString("138d9647-148a-4021-be5e-0c3da7d65155", "AR Receipt"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsReceivable + TransactionTypes.Transfer, ResString.GetMultilingualString("768d9647-148a-4021-be5e-6c3da7d65150", "Receivables Transfer"), ResString.GetMultilingualString("128d9647-148a-4021-be5e-0c3da7d65156", "AR Transfer"), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.ARCASH, ResString.GetMultilingualString("868d9647-148a-4021-be5e-7c3da7d65150", "Receivables Cash Receipt"), ResString.GetMultilingualString("C2B97AF5-F931-48DF-8EF3-9B923E89C2D8", "AR Cash Receipt"), 1);

			defaultValues.AddDefaultValues(LedgerTypes.AccountsPayable + TransactionTypes.Invoice, ResString.GetMultilingualString("170fb2c8-40a6-415f-b6c8-cde272c65bcf", "Payables Invoice"), ResString.GetMultilingualString("868d9647-148a-4021-be5e-0c3da7d6515a", "AP Invoice"), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.JobAPInvoice, ResString.GetMultilingualString("645E1612-E3D3-44CC-A7FC-D5FC00C03D29", "Payables Invoice (Job)"), (NoResString)"", 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsPayable + TransactionTypes.CreditNote, ResString.GetMultilingualString("270fb2c8-40a6-415f-b6c8-cde272c65bcf", "Payables Credit Note"), ResString.GetMultilingualString("768d9647-148a-4021-be5e-0c3da7d6515b", "AP Credit Note"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsPayable + TransactionTypes.AdjustmentNote, ResString.GetMultilingualString("370fb2c8-40a6-415f-b6c8-cde272c65bcf", "Payables Adjustment Note"), ResString.GetMultilingualString("668d9647-148a-4021-be5e-0c3da7d6515c", "AP Adjustment Note"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsPayable + TransactionTypes.Journal, ResString.GetMultilingualString("4B95070C-1CAB-4D31-B0D6-DDCAE29BB125", "Payables Journal"), ResString.GetMultilingualString("518004F6-7001-4FD6-824E-E40E04B99242", "AP Journal"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsPayable + TransactionTypes.Payment, ResString.GetMultilingualString("470fb2c8-40a6-415f-b6c8-cde272c65bcf", "Payables Payment"), ResString.GetMultilingualString("568d9647-148a-4021-be5e-0c3da7d6515d", "AP Payment"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsPayable + TransactionTypes.Receipt, ResString.GetMultilingualString("570fb2c8-40a6-415f-b6c8-cde272c65bcf", "Payables Receipt"), ResString.GetMultilingualString("468d9647-148a-4021-be5e-0c3da7d6515e", "AP Receipt"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.AccountsPayable + TransactionTypes.Transfer, ResString.GetMultilingualString("670fb2c8-40a6-415f-b6c8-cde272c65bcf", "Payables Transfer"), ResString.GetMultilingualString("368d9647-148a-4021-be5e-0c3da7d6515f", "AP Transfer"), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.APCASH, ResString.GetMultilingualString("770fb2c8-40a6-415f-b6c8-cde272c65bcf", "Payables Cash Payment"), ResString.GetMultilingualString("D26087D2-556B-4324-BE49-4E029CD1A87D", "AP Cash Payment"), 1);

			defaultValues.AddDefaultValues(LedgerTypes.UnapprovedPayableTransactions + TransactionTypes.UACreditNote, ResString.GetMultilingualString("4FCFFFCD-982F-42B7-97EA-AFA89E484B59", "Payable Transaction Approval / Unapproved Credit Note"), ResString.GetMultilingualString("5F4E7C0C-B7E0-4B0E-99A5-DCDCF3C61473", "Unapproved Credit Note"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.UnapprovedPayableTransactions + TransactionTypes.UAInvoice, ResString.GetMultilingualString("21E45F24-5D92-488F-8919-D4304993C934", "Payable Transaction Approval / Unapproved Invoice"), ResString.GetMultilingualString("C1F0BCC5-8FC0-4A0A-8F44-AC52E5E5D80E", "Unapproved Invoice"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.TransactionsPendingAllocation + TransactionTypes.InvoicePendingAllocation, ResString.GetMultilingualString("23A0505A-AD24-4A03-980D-9D1D871F9EFE", "Payable Transactions Pending Allocation / Unapproved Invoice"), ResString.GetMultilingualString("BBD9F0B3-2AFF-4E7C-8B24-35EF52379075", "Unapproved Invoice"), 1);

			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.ReversalRelated, ResString.GetMultilingualString("d5dc6ec7-a8db-4b43-92c1-d0b19109eab2", @"Reversal related to"), ResString.GetMultilingualString("d5dc6ec7-a8db-4b43-92c1-d0b19109eab2", @"Reversal related to"), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.OverpaymentRelated, MiscTransactionDefaultDesc, MiscTransactionDefaultDesc, 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.BankFeeJournal, ResString.GetMultilingualString("88139153-b249-4710-b849-a8ae1b14deea", "Bank Fee Journal Relating To Match No."), ResString.GetMultilingualString("88139153-b249-4710-b849-a8ae1b14deea", "Bank Fee Journal Relating To Match No."), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.ExchangeDiff, MiscTransactionDefaultDesc, MiscTransactionDefaultDesc, 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.DiscountRelated, MiscTransactionDefaultDesc, MiscTransactionDefaultDesc, 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.UnOverpaymentRelated, ResString.GetMultilingualString("84254687-9705-40a6-84ca-e0a202df339a", "Overpayment Relating To Un-Match No."), ResString.GetMultilingualString("84254687-9705-40a6-84ca-e0a202df339a", "Overpayment Relating To Un-Match No."), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.UnBankFeeJournal, ResString.GetMultilingualString("88139153-b249-4710-b849-a8ae1b14d77a", "Bank Fee Journal Relating To Un-Match No."), ResString.GetMultilingualString("88139153-b249-4710-b849-a8ae1b14d77a", "Bank Fee Journal Relating To Un-Match No."), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.UnExchangeDiff, ResString.GetMultilingualString("1c360750-1883-4f5d-a318-36389e840617", "Exchange Difference Relating To Un-Match No."), ResString.GetMultilingualString("1c360750-1883-4f5d-a318-36389e840617", "Exchange Difference Relating To Un-Match No."), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.UnDiscountRelated, ResString.GetMultilingualString("1574f86c-9c19-4908-8412-12819ae50137", "Discount Relating To Un-Match No."), ResString.GetMultilingualString("1574f86c-9c19-4908-8412-12819ae50137", "Discount Relating To Un-Match No."), 1);

			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.CASS, ResString.GetMultilingualString("4cf99b4d-7076-4d4b-bcc0-18aa90e0c8da", "Payable CASS Cost File Import / CASS Discrepancy Clearing Transaction"), ResString.GetMultilingualString("1574f86c-9c19-4908-8412-12819ae50190", "CASS discrepancy clearing"), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.FinanceCharge, ResString.GetMultilingualString("863050e4-eb28-4074-ac8c-ac9ccbb2f411", "Cash Book Bank Transfer Finance Charge"), ResString.GetMultilingualString("863050e4-eb28-4074-ac8c-ac9ccbb2f411", "Cash Book Bank Transfer Finance Charge"), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.JCCFX, ResString.GetMultilingualString("34d46029-e2ae-47f8-84f2-fcd1eb031c27", "Job Costing Journal (CFX)"), ResString.GetMultilingualString("34d46029-e2ae-47f8-84f2-fcd1eb031c27", "Job Costing Journal (CFX)"), 1);
			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.BJGDM, ResString.GetMultilingualString("88b8e4f3-b331-4f51-bf74-f497fbc6da3b", "Receivable/Payable Balancing Journal generated during Matching"), ResString.GetMultilingualString("34d46029-e2ae-47f8-84f2-fcd1eb031c67", "Transaction Already Paid: Trans. Num."), 1);
			defaultValues.AddDefaultValues(LedgerTypes.JobCosting + TransactionTypes.JobRevenueJournal, ResString.GetMultilingualString("B20D4217-3770-4F22-924F-B17EAB98FC57", "Job Revenue Journal"), ResString.GetMultilingualString("ECFF0EE4-8965-490C-8852-0C3389590A47", "Job Revenue Journal"), 1);
			defaultValues.AddDefaultValues(Constants.TransactionCategory.Codes.AutoJobRevenueJournal, ResString.GetMultilingualString("3621d53a-6494-4300-8b2d-2714f1bd4ebf", "Job Revenue Journal (Gateway Sell Apportionment)"), ResString.GetMultilingualString("9b0da8b2-093e-4869-968d-524fedf77aff", "Gateway Sell Apportionment"), 1);
			defaultValues.AddDefaultValues(LedgerTypes.General + ReceiptTypes.ForeignCurrencyBalance, ResString.GetMultilingualString("89919e78-5df0-4854-bac8-824311c7081a", "General Ledger Foreign Currency Balance Adjustment"), ResString.GetMultilingualString("B9BA56F6-7214-4670-B9F9-4D7B11F07D9D", "Foreign Currency Balance Adjustment"), 1);

			defaultValues.AddDefaultValues(AccountingConstants.VoucherItemRegistryCode.OutstandingBalanceCurrencyAdjustmentJournal, ResString.GetMultilingualString("d7576790-2433-46d5-8bf8-93cae3d6de8c", "A/R and A/P Outstanding Balance Currency Adjustment Journal."), ResString.GetMultilingualString("2bd1f7c8-a676-4103-9bb0-450016ca4935", "A/R and A/P Outstanding Balance Currency Adjustment Journal."), 1);
			defaultValues.AddDefaultValues(FormattableString.Invariant($"{LedgerTypes.General}{TransactionTypes.GLNoteJournal}"), ResString.GetMultilingualString("0aa99b87-e7bd-48ad-b693-1a7638aedbef", "Note Journal"), ResString.GetMultilingualString("9fd78140-ebf4-4239-a0c1-d57f47569e0e", "Note Journal"), 1);
			if (Instance.EnableBulkDisbursementJobsClosure.Value)
			{
				defaultValues.AddDefaultValues(AccountingConstants.DisbursementShortfallSurplusCode.DisbursementShortfallSurplus, GetDefaultDisbursementShortfallSurplusDesc(), ResString.GetMultilingualString("640D53BA-BEF0-4629-A34E-9F6DE508CE05", "Disbursement Surplus/Shortfall Take Up On Job Closure"), 1);
			}

			return defaultValues;
		}

		static MultilingualString MiscTransactionDefaultDesc => ResString.GetMultilingualString("84254687-9705-40a6-84ca-e0a202df3eea", "Match No.");

		public string GetTransactionDescriptionFromCode(string code, string defaultDescription)
		{
			string description = VoucherNumberOfSupportingDocumentDefaults.
											 GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).
											 GetDescriptionFromCode(code)
								?? defaultDescription;
			return description.ToUpper();
		}
		public ZByte GetVoucherNoOfAttchmentsFromCode(string code, ZByte defaultNo)
		{
			ZByte number = VoucherNumberOfSupportingDocumentDefaults.
											 GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
											 .GetValueFromCode(code);
			if (number.IsEmpty)
			{
				number = defaultNo;
			}
			return number;
		}

		public ResourceString GetDefaultDisbursementShortfallSurplusDesc()
		{
			return ResString.GetMultilingualString("F7DABC86-1C44-4B58-8836-EEF55AE417AD", "Disbursement Surplus/Shortfall Take Up On Job Closure");
		}

		#endregion
		#endregion

		#region Data Import

		public BooleanRegistryItem AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile
		{
			get
			{
				return GetItem("AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile", delegate
				{
					return new BooleanRegistryItem("AutomaticallyCreateBalancingJournalsWhenImportingRemittanceFile",
						Categories.Accounting_DataImport,
						ResString.GetMultilingualString("1D867AEE-5A3A-44e4-8109-37E63CC5298A", "Automatically Create Balancing Journals When Importing Remittance File"),
						ResString.GetMultilingualString("F13E6A65-07E3-4576-BD49-B3AE9939938D", @"When importing a remittance file, it’s possible that some of the invoices identified in the ‘PTR – Paid Transaction’ lines can’t be found in {0} OR are already fully paid.
When this registry is set to ‘Yes’, {0} will automatically create ‘balancing journals’ that will create two opposite AR or AP journals (that will balance to zero).
One journal will be included in the matching session and the other will be created as a ‘carry forward’ journal, for the amount indicated in the remittance file.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem DataImportShouldSaveOnlyWhenThereAreNoErrors
		{
			get
			{
				return GetItem("DataImportShouldSaveOnlyWhenThereAreNoErrors", delegate
				{
					return new BooleanRegistryItem(
						"DataImportShouldSaveOnlyWhenThereAreNoErrors",
						Categories.Accounting_DataImport,
						ResString.GetMultilingualString("7F6A497A-8A7F-4b9f-A6C8-A80F7AA5D6BD", "Default for 'Only Save data when no imported records have errors'"),
						ResString.GetMultilingualString("20ED8FD6-3008-4151-BEF5-8BB918218D3C", @"This registry setting controls the default behavior for the accounting data import processes.

Select 'Yes' - The system will prevent saving of transactions if there are errors on any transactions within the file.
If you experience errors during a data import, you can modify the file to fix the errors and re-import the entire file again.

Select 'No' - The system will attempt to save any transactions that do not have errors - even if other transactions in the same file have errors.
Transactions that have errors will not be saved. If you experience errors during a data import, you can manually enter the erroneous transaction."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem CombineInvoiceLinesByChargeCode
		{
			get
			{
				return GetItem("CombineInvoiceLinesByChargeCode", delegate
				{
					return new BooleanRegistryItem(
						"CombineInvoiceLinesByChargeCode",
						Categories.Accounting_DataImport,
						ResString.GetMultilingualString("3850B56B-69D7-4154-AF03-FB120567A618", "Combine Invoice Lines by Charge Code"),
						ResString.GetMultilingualString("6d449177-be8d-4562-a0f9-25ea1b959e2a", @"This registry setting controls the import behavior when importing transactions using the Intercompany Charge Codes or Global Charge Codes.

Select 'Yes' - The system will combine invoice lines with matching details into a single line on an imported invoice.
When importing a number of lines with the same charge code, job number, tax rate, target job and related job - these lines will be combined into a single invoice line for the total value of these lines.

Select 'No' - The system will not combine lines with the same details."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Job Invoicing

		public BooleanRegistryItem DisableDisplaySequenceCalculationForWarehousePeriodicInvoicingJobTypes
		{
			get
			{
				return GetItem(nameof(DisableDisplaySequenceCalculationForWarehousePeriodicInvoicingJobTypes), () =>
				{
					return new BooleanRegistryItem(
						nameof(DisableDisplaySequenceCalculationForWarehousePeriodicInvoicingJobTypes),
						Categories.Accounting_JobInvoicing,
						(NoResString)@"Disable Display Sequence Calculation for Warehouse Periodic Invoicing Job Types (CargoWiseOne Support Only)",
						(NoResString)@"By default, the system will calculate the display sequence as per current behavior. 

When this registry is set to ‘Yes’,  the display sequence of warehouse periodic invoicing jobs will not be calculated. The display sequence will always be set to 0 and all validation in relation to display sequence will be suspended.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#region ElectronicProcessingChargeManagement

		public BooleanRegistryItem EnableElectronicProcessingChargeFunctionality
		{
			get
			{
				var item = GetItem(nameof(EnableElectronicProcessingChargeFunctionality), () =>
				{
					return new BooleanRegistryItem(
						nameof(EnableElectronicProcessingChargeFunctionality),
						Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement,
						(NoResString)@"Enable Electronic Processing Charge Functionality (CargoWiseOne Support Only)",
						(NoResString)@"When this registry is set to 'Yes', the system will auto insert electronic processing charge when invoicing job is created for 'SHP - Shipment' job type. 
When this registry is set to 'No', the system will stop the insertion of electronic processing charge for new invoicing job created thereafter.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
				item.OnBuildLogReference += (args) => Res.GetString("38e36faa-1816-43f0-9830-5a60eec3e660", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

				return item;
			}
		}

		public BooleanRegistryItem CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting
		{
			get
			{
				var item = GetItem(nameof(CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting), () =>
				{
					return new BooleanRegistryItem(
						new CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePostingRegistryItemImpl(
							nameof(CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting),
							Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement,
							ResString.GetMultilingualString("10E3F9B7-447E-4AD1-BD23-EC3D70663C6B", @"Carry Over Invoice Line Description During Intercompany Invoice Posting"),
							ResString.GetMultilingualString("CBABB1A1-DC27-405C-920D-0776A5537F20", @"When this registry is set to 'Yes', the system will carry over the invoice line description to the respective job charge line during Intercompany Invoice Posting.
When this registry is set to 'No', the system will work as before."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							DisplayElectronicProcessingCharge ? RegistryOptions.Default : RegistryOptions.IsHidden,
							true)
						);
				});

				return item;
			}
		}

		public class CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePostingRegistryItemImpl : RegistryItemImpl
		{
			public CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePostingRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool defaultValue)
				: base(name, category, caption, hint, RegistryDataTypes.BoolType, storage, options, defaultValue)
			{
			}

			public override bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				return Instance.EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
			}
		}

		public GuidRegistryItem ElectronicProcessingChargeDisbursementClearingAccount
		{
			get
			{
				return GetItem("ElectronicProcessingChargeDisbursementClearingAccount", delegate
				{
					var result = new GuidRegistryItem(
						"ElectronicProcessingChargeDisbursementClearingAccount",
						Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement,
						ResString.GetMultilingualString("10e89351-bee6-464f-ad51-b4096ee58dce", "Electronic Processing Charge Disbursement Clearing Account"),
						ResString.GetMultilingualString("fe821ea5-eac3-4488-9d42-b754cedb5054", "This GL Account will be used for the recording and clearing of the Electronic Processing Disbursement Charge."),
						RegistryStorageFlags.System,
						DisplayElectronicProcessingCharge ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHAndNonControl_DisallowDirectPost);
					result.DataType = new WithoutCheckExistAccountDataType();
					result.OnBuildLogReference += BuildElectronicProcessingChargeClearingAccountLogReference;

					return result;
				});
			}
		}

		public GuidRegistryItem ElectronicProcessingChargePayableClearingAccount
		{
			get
			{
				return GetItem("ElectronicProcessingChargePayableClearingAccount", delegate
				{
					var result = new GuidRegistryItem(
						"ElectronicProcessingChargePayableClearingAccount",
						Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement,
						ResString.GetMultilingualString("c662b8f2-4bc6-4c64-a964-1039a34646dd", "Electronic Processing Charge Payable Clearing Account"),
						ResString.GetMultilingualString("5b3feda6-d49a-4060-a830-4a3418986b32", "This GL Account will be used for the recording and clearing of the Electronic Processing Charge Payables."),
						RegistryStorageFlags.System,
						DisplayElectronicProcessingCharge ? (GlbStaff.CurrentUser.IsSupportUser ? RegistryOptions.Default : RegistryOptions.IsReadOnly) : RegistryOptions.IsHidden);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHAndNonControl_AllowDirectPost);
					result.DataType = new WithoutCheckExistAccountDataType();
					result.OnBuildLogReference += BuildElectronicProcessingChargeClearingAccountLogReference;

					return result;
				});
			}
		}

		string BuildElectronicProcessingChargeClearingAccountLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var factory = new BusinessObjectFactory();
			var originalGLHeader = factory.Load<AccGLHeader>(new ZGuid(args.OriginalValue));
			var newGLHeader = factory.Load<AccGLHeader>(new ZGuid(args.NewValue));
			return Res.GetString("04bac8e4-062b-4325-b3a3-963979a43ff9", "GL Account changed from [{0}] to [{1}].", originalGLHeader?.AccountNum ?? ZString.Empty, newGLHeader?.AccountNum ?? ZString.Empty);
		}

		bool DisplayElectronicProcessingCharge
		{
			get
			{
				var allCompanies = new BusinessObjectFactory().Load<GlbCompany>(new ZQuery());
				return allCompanies.Any(x => EnableElectronicProcessingChargeFunctionality.GetFallBackValueAtAllLevels(x.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		public GuidRegistryItem ElectronicProcessingChargeCode
		{
			get
			{
				return GetItem("ElectronicProcessingChargeCode", delegate
				{
					var result = new GuidRegistryItem(
						"ElectronicProcessingChargeCode",
						Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement,
						ResString.GetMultilingualString("2D8DE4EF-91E5-4D0C-AF87-8AD02D6749C3", "Electronic Processing Charge Code"),
						ResString.GetMultilingualString("01D005E7-5C32-48DF-9F54-3A3DD9284304", "This charge code will be used for the posting of accounting transactions relating to the Electronic Processing Charge."),
						RegistryStorageFlags.System,
						DisplayElectronicProcessingCharge ? (GlbStaff.CurrentUser.IsSupportUser ? RegistryOptions.Default : RegistryOptions.IsReadOnly) : RegistryOptions.IsHidden);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.GlobalDSBOrMRGChargeCode);

					result.OnBuildLogReference += (args) =>
					{
						var factory = new BusinessObjectFactory();
						var originalChargeCode = factory.Load<AccChargeCode>(new ZGuid(args.OriginalValue));
						var newChargeCode = factory.Load<AccChargeCode>(new ZGuid(args.NewValue));
						return Res.GetString("65B47904-B66A-4471-9DD9-0A0BA06D53B0", "Charge Code changed from [{0}] to [{1}].", originalChargeCode?.AC_Code ?? ZString.Empty, newChargeCode?.AC_Code ?? ZString.Empty);
					};

					return result;
				});
			}
		}

		public ElectronicProcessingChargeConfigurationRegistryItem ElectronicProcessingChargeConfiguration
		{
			get
			{
				return GetItem("ElectronicProcessingChargeConfiguration", delegate
				{
					var result = new ElectronicProcessingChargeConfigurationRegistryItem(
						"ElectronicProcessingChargeConfiguration",
						Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement,
						(NoResString)"Electronic Processing Charge Configuration (CargoWise Support Only)",
						(NoResString)@"This registry defines the Job Types and relative Start Date and End Date for the posting of accounting transactions relating to the Electronic Processing Charge when the functionality has been enabled.
Note:
The job header will be always created for the corresponding Job Type during the date range specified and the Registry 'Add Job Invoicing Record at Saving/Editing of Operations Job' will be bypassed.",
						RegistryStorageFlags.System,
						DisplayElectronicProcessingCharge ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						GetElectronicProcessingChargeConfigurationDefaultValue());

					result.OnBuildLogReference += BuildElectronicProcessingChargeConfigurationLogReference;

					return result;
				});
			}
		}

		string BuildElectronicProcessingChargeConfigurationLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var result = new ZStringBuilder();
			var newCollection = ((ElectronicProcessingChargeConfigurationCollection)args.NewValue).Cast<ElectronicProcessingChargeConfiguration>();
			var originalCollection = ((ElectronicProcessingChargeConfigurationCollection)args.OriginalValue).Cast<ElectronicProcessingChargeConfiguration>();

			foreach (var configuration in originalCollection)
			{
				var newConfiguration = newCollection.FirstOrDefault(x => x.JobType == configuration.JobType);
				if (newConfiguration == null)
				{
					result.AppendLine(GetElectronicProcessingChargeConfigurationChangeLog((NoResString)"Deleted", configuration));
				}
				else if (newConfiguration.StartDate != configuration.StartDate || newConfiguration.EndDate != configuration.EndDate)
				{
					result.AppendLine(GetElectronicProcessingChargeConfigurationChangeLog((NoResString)"Deleted", configuration));
					result.AppendLine(GetElectronicProcessingChargeConfigurationChangeLog((NoResString)"Added", newConfiguration));
				}
			}

			foreach (var newConfiguration in newCollection)
			{
				if (!originalCollection.Any(x => x.JobType == newConfiguration.JobType))
				{
					result.AppendLine(GetElectronicProcessingChargeConfigurationChangeLog((NoResString)"Added", newConfiguration));
				}
			}

			return result.ToString();
		}

		string GetElectronicProcessingChargeConfigurationChangeLog(string changeType, ElectronicProcessingChargeConfiguration electronicProcessingChargeConfiguration)
		{
			return Res.GetString("E196DC49-63B7-4B19-8C74-61A6EF366897", "{0}: Job Type={1}, Start Date={2}, End Date={3}",
				changeType,
				electronicProcessingChargeConfiguration.JobType,
				electronicProcessingChargeConfiguration.StartDate.ToShortDateString(),
				electronicProcessingChargeConfiguration.EndDate.ToShortDateString());
		}

		ElectronicProcessingChargeConfigurationCollection GetElectronicProcessingChargeConfigurationDefaultValue()
		{
			var result = new ElectronicProcessingChargeConfigurationCollection();
			var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingElectronicProcessingChargeFeature);
			if (featureData != null
				&& featureData.TryDeserializeParameterAsJson<ElectronicProcessingChargeFeatureControlModel>(out var electronicProcessingChargeFeatureControl)
				&& electronicProcessingChargeFeatureControl.ElectronicProcessingChargeConfiguration != null)
			{
				foreach (var model in electronicProcessingChargeFeatureControl.ElectronicProcessingChargeConfiguration)
				{
					if (model.StartDate != null)
					{
						var config = result.AddNew();
						config.JobType = model.JobType;
						config.StartDate = new ZDate(model.StartDate);
						if (model.EndDate != null && model.EndDate != DateTime.MinValue)
						{
							config.EndDate = new ZDate(model.EndDate);
						}
					}
				}
			}

			return result;
		}

		public ElectronicProcessingChargeDescriptionOverrideRegistryItem ElectronicProcessingChargeDescriptionOverride
		{
			get
			{
				return GetItem("ElectronicProcessingChargeDescriptionOverride", delegate
				{
					var result = new ElectronicProcessingChargeDescriptionOverrideRegistryItem(
						"ElectronicProcessingChargeDescriptionOverride",
						Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement,
						ResString.GetMultilingualString("45E08499-B4F3-4779-8ECC-AEFED4270F29", "Electronic Processing Charge Description Override"),
						ResString.GetMultilingualString("34C39B6E-B557-435E-BDE1-CA4A482A9930", @"This registry enables you to configure default Electronic Processing Charge line description to help you differentiate Electronic Processing Charge generated from different jobs.

By default, the Electronic Processing Charge Code description is used as charge line description for sale invoices.

If you override this registry, the system will add these elements as part of the charge line description.

Note:
1. This Charge Description Override configuration only applies to SHP-Shipment Job Type.
2. For Domestic jobs, the system prioritizes applying job header branch rule SUJ/NSJ. If no configuration matches, then the company rule SCC/NSC will be used.
3. Shipment number will always be placed at the end of the charge line description."),
						RegistryStorageFlags.Company,
						DisplayElectronicProcessingCharge ? RegistryOptions.Default : RegistryOptions.IsHidden);

					return result;
				});
			}
		}

		public ElectronicProcessingChargeCurrencyRegistryItem ElectronicProcessingChargeCurrency
		{
			get
			{
				var item = GetItem("ElectronicProcessingChargeCurrency", delegate
				{
					var result = new ElectronicProcessingChargeCurrencyRegistryItem(
						"ElectronicProcessingChargeCurrency",
						Categories.Accounting_JobInvoicing_ElectronicProcessingChargeManagement,
						(NoResString)"Electronic Processing Charge Currency (CargoWiseOne Support Only)",
						(NoResString)@"The specified currency will be used to locate the applicable license fee from the reference database for the creation of the accrued transactions relating to Electronic Processing Charge.
This registry will be in a table layout with two columns. Both values must be provided
1. Currency: This refers to the charge currency.
2. Valid From Date: This refers to the start date when the charge currency should be used. Note: It can be superseded by a later start date.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						GetElectronicProcessingChargeCurrencyDefaultValue());

					return result;
				});

				item.OnBuildLogReference += BuildElectronicProcessingChargeCurrencyLogReference;

				return item;
			}
		}

		string BuildElectronicProcessingChargeCurrencyLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var result = "";
			var newCollection = ((ElectronicProcessingChargeCurrencyCollection)args.NewValue).Cast<ElectronicProcessingChargeCurrency>();
			var originalCollection = ((ElectronicProcessingChargeCurrencyCollection)args.OriginalValue).Cast<ElectronicProcessingChargeCurrency>();

			foreach (var configuration in originalCollection)
			{
				if (!newCollection.Any(x => x.CurrencyPK == configuration.CurrencyPK && x.ValidFromDate == configuration.ValidFromDate))
				{
					result += GetElectronicProcessingChargeCurrencyChangeLog((NoResString)"Deleted", configuration);
				}
			}

			foreach (var newConfiguration in newCollection)
			{
				if (!originalCollection.Any(x => x.CurrencyPK == newConfiguration.CurrencyPK && x.ValidFromDate == newConfiguration.ValidFromDate))
				{
					result += GetElectronicProcessingChargeCurrencyChangeLog((NoResString)"Added", newConfiguration);
				}
			}

			return result;
		}

		string GetElectronicProcessingChargeCurrencyChangeLog(string changeType, ElectronicProcessingChargeCurrency electronicProcessingChargeCurrency)
		{
			return Res.GetString("dcb729a5-d160-48ef-8202-77e1657d21b4", "Override {0}: {1}, {2}", changeType, electronicProcessingChargeCurrency.Currency.RX_Code, electronicProcessingChargeCurrency.ValidFromDate) + "\r\n";
		}

		ElectronicProcessingChargeCurrencyCollection GetElectronicProcessingChargeCurrencyDefaultValue()
		{
			var result = new ElectronicProcessingChargeCurrencyCollection();

			var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingElectronicProcessingChargeFeature);
			if (featureData != null
				&& featureData.TryDeserializeParameterAsJson<ElectronicProcessingChargeFeatureControlModel>(out var electronicProcessingChargeFeatureControl)
				&& electronicProcessingChargeFeatureControl.ElectronicProcessingChargeCurrency != null)
			{
				var readonlyFactory = new ReadOnlyBusinessObjectFactory();
				var currencies = readonlyFactory.Load<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, electronicProcessingChargeFeatureControl.ElectronicProcessingChargeCurrency.Select(x => x.CurrencyCode)));
				foreach (var chargeCurrency in electronicProcessingChargeFeatureControl.ElectronicProcessingChargeCurrency)
				{
					var currency = currencies.FirstOrDefault(x => x.RX_Code == chargeCurrency.CurrencyCode);
					if (currency is not null && chargeCurrency.ValidFromDate != null)
					{
						var electronicProcessingChargeCurrency = result.AddNew();
						electronicProcessingChargeCurrency.CurrencyPK = currency.PK;
						electronicProcessingChargeCurrency.ValidFromDate = new ZDateTime(chargeCurrency.ValidFromDate);
					}
				}
			}

			return result;
		}

		#endregion

		public BooleanRegistryItem IncludeDisbursementsPercentageMarginCalculations
		{
			get
			{
				return GetItem("IncludeDisbursementsPercentageMarginCalculations", delegate
				{
					return new BooleanRegistryItem(
						"IncludeDisbursementsPercentageMarginCalculations",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("bceefc4a-9088-4d07-a8f9-de95afd4e55f", "Include Disbursements Percentage Margin Calculations"),
						ResString.GetMultilingualString("b802d3b3-9781-44af-a972-5a8aaeec0d31", @"When this registry is set to 'NO', any charge line to a charge code with a Charge Type of DSB – Disbursement will not be included in the percentage margin calculations.Charge code Type Overrides will be respected. i.e. if a charge code has a Charge Type of MRG, with a Type Override of DSB for Export Air jobs, on all jobs except Export Air, that charge code will be included in the percentage margin calculations. On Export Air jobs, charges to that charge code will be excluded."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public DecimalRegistryItem JobInvoiceMaximumValueOfSplittingRules
		{
			get
			{
				return GetItem("JobInvoiceMaximumValueOfSplittingRules", delegate
				{
					return new DecimalRegistryItem(
						"JobInvoiceMaximumValueOfSplittingRules",
						Categories.Accounting_JobInvoicing_InvoiceSplittingRules,
						ResString.GetMultilingualString("E10FA2A6-4C54-460A-A3DD-86F958316E8F", "Maximum value"),
						ResString.GetMultilingualString("9E5A54E4-88C9-4714-9F0C-8EBB66003CFC", @"If required, use this registry to limit the maximum local currency value (excluding tax) posted per invoice."), new NumericRegistryEditorInfo(2),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default,
						0m, 0, double.MaxValue);
				});
			}
		}

		public IntRegistryItem JobInvoiceMaximumNumberOfChargesOfSplittingRules
		{
			get
			{
				return GetItem("JobInvoiceMaximumNumberOfChargesOfSplittingRules", delegate
				{
					return new JobInvoiceMaximumNumberOfChargesRegistryItem(
						"JobInvoiceMaximumNumberOfChargesOfSplittingRules",
						Categories.Accounting_JobInvoicing_InvoiceSplittingRules,
						ResString.GetMultilingualString("A10BC2C6-2D34-660A-63DD-66F958316E81", "Maximum number of charges"),
						ResString.GetMultilingualString("2CF08701-6801-4E73-8C1C-005FF0A29E73", @"If required, use this registry to limit the maximum transaction lines (charges) posted per invoice."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default,
						0, 0, int.MaxValue);
				});
			}
		}

		public JobInvoiceAddressCountryRegistryItem JobInvoiceAddressCountry
		{
			get
			{
				return GetItem("JobInvoiceAddressCountry", delegate
				{
					return new JobInvoiceAddressCountryRegistryItem(
						"JobInvoiceAddressCountry",
						Categories.Accounting_JobInvoicing_InvoiceSplittingRules,
						ResString.GetMultilingualString("F458D613-16BC-45A8-A288-3575316FF537", "Invoice Address Country/Region"),
						ResString.GetMultilingualString("C1671F77-0682-4490-AFA6-E6898108ACED", @"This registry determines if the invoice splitting rule should be applied (when specified) with reference to the country/region of the invoice address at the point of posting."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default);
				});
			}
		}

		#region Revenue Recognition Setup

		public RevenueRecognitionRegistryItem RevenueRecognitionSetup
		{
			get
			{
				return GetItem("RevenueRecognitionSetup", delegate
				{
					RevenueRecognitionCollection defaultCollection = new RevenueRecognitionCollection();
					RevenueRecognition defaultValue = defaultCollection.AddNew();
					defaultValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
					defaultValue.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
					return new RevenueRecognitionRegistryItem(
											"RevenueRecognitionSetup",
											Categories.Accounting_JobInvoicing,
											ResString.GetMultilingualString("A80AFDA8-DE81-4aa4-826F-1545DA410D22", "Revenue Recognition Setup"),
											ResString.GetMultilingualString("8c0a28d7-9404-4541-a18e-2502fcc06ecc", @"This registry is used to set the Revenue (Profit) Recognition date of each job.

By default, movements in profit are recognized “Immediately”.

By default, the incremental profit movements resulting from changes in a job’s WIP, Accrual, Cost and Revenue charge lines are recognized as profit at the time those changes occur. Any profit movement is recognized “immediately”.

Alternatively, a specific Revenue Recognition date can be set on each job. This date controls the timing of job profit recognition. When a specific Revenue Recognition date has been set, all the incremental movements over time in a job’s profit will be attributed to the Revenue Recognition date until the General Ledger Accounting Period for that date is ‘closed’. Once the relevant Accounting Period is closed, any subsequent movements in profit will be recognized “immediately”."),
											RegistryStorageFlags.System | RegistryStorageFlags.Company,
											defaultCollection);
				});
			}
		}

		public RevenueRecognitionByChargeGroupRegistryItem RevenueRecognitionByChargeGroupSetup
		{
			get
			{
				return GetItem("RevenueRecognitionByChargeGroupSetup", delegate
				{
					RevenueRecognitionByChargeGroupCollection defaultCollection = new RevenueRecognitionByChargeGroupCollection();
					ChargeCodeGroupList chargeCodeGroups = new ChargeCodeGroupList();
					foreach (CodeDescriptionPair group in chargeCodeGroups)
					{
						RevenueRecognitionByChargeGroup defaultValue = defaultCollection.AddNew();
						defaultValue.ChargeGroup = group.Code;
						defaultValue.ChargeGroupDescription = group.MultilingualDescription;
					}
					return new RevenueRecognitionByChargeGroupRegistryItem(
											"RevenueRecognitionByChargeGroupSetup",
											Categories.Accounting_JobInvoicing,
											ResString.GetMultilingualString("2334578B-E9A9-4880-A456-B3C27534A353", "Revenue Recognition By Charge Group Setup"),
											ResString.GetMultilingualString("f08c5f17-f69b-4176-9842-42f22a382e0d", @"This registry is used to configure Revenue (Profit) Recognition behaviors by Charge Code Ratings Group. 
By default, all charges on a job have the same revenue recognition behavior.  By default each job has a single Revenue Recognition Configuration.  That behavior is defined through the Revenue Recognition Setup registry item. 
That default behavior can be overridden through this registry.  This registry enables the assignment of revenue recognition behaviors against each Charge Code Ratings Group.  This means that charges within the one job can be recognized in the General Ledger accounting system on different dates, as a result of the recognition styles assigned through this registry against the related Charge Code Ratings Group."),
											RegistryStorageFlags.System | RegistryStorageFlags.Company,
											defaultCollection);
				});
			}
		}

		public BooleanRegistryItem RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture
		{
			get
			{
				return GetItem(nameof(RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture), () =>
				{
					var item = new BooleanRegistryItem(
						nameof(RecognizeRevenueUsingCurrentDateWhenRevenueRecognitionDateInFuture),
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("2F32B1CA-93D3-429F-A162-A02105BE9E8D", "Recognize Revenue using Current Date when Revenue Recognition Date is in the future"),
						ResString.GetMultilingualString("155F4EFE-E5A3-45F7-A235-1DF14B0F6108", @"This registry affects the recognition of transactions posted against jobs with a Revenue Recognition Date that falls in the future. 

By default, the registry is set to ‘No’ and the system will recognize the transactions based on the respective revenue recognition setting as per current system behavior.

When the registry is overridden to ‘Yes’, the system will recognize the transactions using current date if the revenue recognition date falls in the future. 

Note: Any change to this registry value will not affect job revenue recognition dates that have already been recorded against the job. All subsequent job charges posted against the same job with the same recognition type will be recognized using the same date that has been recorded against the job except for IMM revenue recognition type."),
						RegistryStorageFlags.Company,
						false);
					item.OnBuildLogReference += (args) => Res.GetString("CDBD1AE4-111F-4A0B-AAFD-A4DBDDCEBEC0", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		#region AutoJobClosure

		public JobClosureConfigurationRegistryItem JobClosureConfigurationSetup
		{
			get
			{
				return GetItem("JobClosureConfigurationSetup", delegate
				{
					var defaultValue = new JobClosureConfigurationHeader();

					var item = new JobClosureConfigurationRegistryItem(
						"JobClosureConfigurationSetup",
						Categories.Accounting_AutoJobClosure,
						ResString.GetMultilingualString("2217c67e-08cb-455a-9f5e-1439c125d5dc", "Auto Job Closure Configuration"),
						ResString.GetMultilingualString("b520ed92-f747-434d-b8c1-0e93135af0ee", @"This registry enables you to configure the auto closing of jobs based on Job Type, Mode, Direction, Department, Relevant Date and Number of Offset days from the Relevant Date.
You can also specify a Re-Open Restriction Offset value to restrict re-opening of closed jobs past a certain number of offset days from the relevant date. 

If required, you can optionally configure the system to update job status to the intermediate job status 'JFC - Job Ready For Financial Closure' prior to auto job closure.
For instance, to auto update status of open jobs to intermediate job status 'JFC - Job Ready for Financial Closure' after 2 months from Job Open Date to prevent unauthorized transaction posting. 
Then auto close these jobs after 3 months from Job Open Date to finalized the job profit and loss. 

Note: The above configuration will be processed by the JCS - Job Closure Service Task."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, defaultValue);

					item.OnBuildLogReference += BuildJobClosureConfigurationSetupLogReference;

					return item;
				});
			}
		}

		string BuildJobClosureConfigurationSetupLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var originalElements = ((JobClosureConfigurationHeader)args.OriginalValue).ConfigurationCollection.Cast<JobClosureConfiguration>();
			var newElements = ((JobClosureConfigurationHeader)args.NewValue).ConfigurationCollection.Cast<JobClosureConfiguration>();

			return RegistryChangeLogger.GetLogReference(originalElements, newElements);
		}

		public BooleanRegistryItem AutoJobClosureProcessUnboundedBatchSize
		{
			get
			{
				return GetItem("AutoJobClosureProcessUnboundedBatchSize", delegate
				{
					return new BooleanRegistryItem(
						"AutoJobClosureProcessUnboundedBatchSize",
						Categories.Accounting_AutoJobClosure,
						(NoResString)"Unbounded Auto Job Closure process batch size",
						(NoResString)@"This registry is used to bound/unbound the number of jobs processed by the Job Closure Service Task every time it runs.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true
					);
				});
			}
		}

		public IntRegistryItem AutoJobClosureProcessBatchSize
		{
			get
			{
				return GetItem("AutoJobClosureProcessBatchSize", delegate
				{
					return new IntRegistryItem(
						"AutoJobClosureProcessBatchSize",
						Categories.Accounting_AutoJobClosure,
						ResString.GetMultilingualString("74d84e60-b329-4815-bc38-a62d4ca8d5d7", "Auto Job Closure process batch size"),
						ResString.GetMultilingualString("287a9500-8b24-4dcf-b212-eb240b0d434b", @"This registry is used to configure the number of jobs processed by the Job Closure Service Task every time it runs. 

By default this registry is set to 250. Maximum batch size is 100,000

Note: This batch size may impact the amount of memory taken up by Job Closure Service Task. If you need to automatically close a large number of jobs, you can set a shorter recurrence pattern for Job Closure Service Task."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						250, 0, 100000);
				});
			}
		}

		public DateTimeRegistryItem LastUTCDateTimeOfReachingTheHighestJCSWatermark
		{
			get
			{
				return GetItem("LastUTCDateTimeOfReachingTheHighestJCSWatermark", delegate
				{
					return new DateTimeRegistryItem(
						"LastUTCDateTimeOfReachingTheHighestJCSWatermark",
						Categories.Accounting_AutoJobClosure,
						(NoResString)"Last time when JCS completed scanning all jobs (CargoWiseOne Support Only)",
						(NoResString)"This registry is used to check when last time Auto Job Closure service task (JCS) finished scanning all jobs for auto closure. This registry is updated by the service task once it finishes scanning all jobs available in JobHeader table.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.LongIncludingSeconds),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						true);
				});
			}
		}

		public DateTimeRegistryItem AutoJobClosureProcessingWatermark
		{
			get
			{
				return GetItem("AutoJobClosureProcessingWatermark", delegate
				{
					return new DateTimeRegistryItem(
						"AutoJobClosureProcessingWatermark",
						Categories.Accounting_AutoJobClosure,
						(NoResString)"Auto Job Closure processing watermark (CargoWiseOne Support Only)",
						(NoResString)@"This registry is used to track up-to which date Auto Job Closure service task finished scanning jobs for auto closure.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						DateTime.MinValue,
						true);
				});
			}
		}

		public IntRegistryItem AutoJobClosureQueueMaximumLength
		{
			get
			{
				return GetItem("AutoJobClosureQueueMaximumLength", delegate
				{
					return new IntRegistryItem(
						"AutoJobClosureQueueMaximumLength",
						Categories.Accounting_AutoJobClosure,
						ResString.GetMultilingualString("61120b84-4511-4e36-bf0d-0f94dca9f6f7", "Maximum number of jobs allowed in the queue"),
						ResString.GetMultilingualString("ee64761b-35b3-4311-aa0d-569cb6f93afe", "This registry determines maximum number of jobs that can be queued for auto closure at a time. Once Auto Job Closure service task completes processing queued jobs, a new batch of jobs will be queued."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						100000, 0, 1000000);
				});
			}
		}

		public IntRegistryItem NumberOfJobsProcessedForAutoClosingInTheCurrentBatch
		{
			get
			{
				var maxQueueLengthInfo = (AutoJobClosureQueueMaximumLength.DataType as IntRegistryDataType);
				return GetItem("NumberOfJobsProcessedForAutoClosingInTheCurrentBatch", delegate
				{
					return new IntRegistryItem(
						"NumberOfJobsProcessedForAutoClosingInTheCurrentBatch",
						Categories.Accounting_AutoJobClosure,
						(NoResString)"Sequence number of the row that was last processed by Job Closure Service (JCS) task",
						(NoResString)"This registry sets the sequence number of the row that was last processed by Job Closure Service (JCS) task.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.NotCached,
						0,
						0,
						Convert.ToInt32(maxQueueLengthInfo.UpperBound));
				});
			}
		}
		#endregion

		#region AutoCompactAccGLAggregate

		public StringRegistryItem AutoCompactAccGLAggregateLastCompanyPeriod
		{
			get
			{
				return GetItem("AutoCompactAccGLAggregateLastCompanyPeriod", delegate
				{
					return new StringRegistryItem(
						"AutoCompactAccGLAggregateLastCompanyPeriod",
						Categories.Accounting_AutoCompactAccGLAggregate,
						(NoResString)"Last company/period compacted",
						(NoResString)@"This registry stores the last company PK and period that was processed by the Compact General Ledger Aggregate Service Task.

The service task processes each combination of login company and period in turn. This data records the last company/period which was processed and determines where the service task will run from next.

Clear this registry to begin processing from the first company/period. 
The data format is: '<company PK>|<period>'.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport
					);
				});
			}
		}

		public DecimalRegistryItem AutoCompactAccGLAggregateRunDuration
		{
			get
			{
				return GetItem("AutoCompactAccGLAggregateRunDuration", delegate
				{
					return new DecimalRegistryItem(
						"AutoCompactAccGLAggregateRunDuration",
						Categories.Accounting_AutoCompactAccGLAggregate,
						ResString.GetMultilingualString("013C0DA7-80E0-491D-A4B6-B08FA58CA96B", "Maximum run time (in hours) of Compact General Ledger Aggregate Service Task"),
						ResString.GetMultilingualString("1416056A-CCC1-4845-A77D-C1D5B474D540", @"This registry determines the maximum time, in hours, that the Compact General Ledger Aggregate Service Task will run for.

Use 1 for 1 hour, and therefore 0.5 for 30 minutes, 0.25 for 15 minutes, etc.
If 0 is entered, the service task will compact only 1 company/period per run.

You should set this registry value in conjunction with the Compact General Ledger Aggregate Service Task scheduled frequency."),
						new NumericRegistryEditorInfo(4),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 4, lowerBound: 0, upperBound: 24
					);
				});
			}
		}

		public DecimalRegistryItem AutoCompactAccGLAggregateThreshold
		{
			get
			{
				return GetItem("AutoCompactAccGLAggregateThreshold", delegate
				{
					return new DecimalRegistryItem(
						"AutoCompactAccGLAggregateThreshold",
						Categories.Accounting_AutoCompactAccGLAggregate,
						ResString.GetMultilingualString("346FFE70-51F7-4250-8A10-8FC77264478C", "Minimum threshold to reach for rows to be compacted"),
						ResString.GetMultilingualString("8FC4B5E1-501B-498F-9AF1-4FD31C15C96A", @"This registry determines the minimum threshold for rows in a certain period/company that will be compacted.

E.g. If we set a value of 0.5, the service task will only compact period/companies where 50% or more rows will be eliminated from compaction.

A value of 0 will always compact even if there is no benefit.
A value of 1.0 will never compact."),
						new NumericRegistryEditorInfo(2),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 0.1m, lowerBound: 0, upperBound: 1
					);
				});
			}
		}

		#endregion

		public BooleanRegistryItem PostIntoNextOpenPeriodWhenRecognitionPeriodClosed
		{
			get
			{
				return GetItem("PostIntoNextOpenPeriodWhenRecognitionPeriodClosed", delegate
				{
					return new BooleanRegistryItem(
						 "PostIntoNextOpenPeriodWhenRecognitionPeriodClosed",
						 Categories.Accounting_JobInvoicing,
						 ResString.GetMultilingualString("8C33ACCF-DE19-4b57-BD2B-14D11A286ECC", "Recognize Revenue in First Open Sub Ledger Period"),
						 ResString.GetMultilingualString("798e27f5-674b-4e16-a4eb-2727d3be7b8b", "This registry affects the recognition of transactions posted against jobs with a Revenue Recognition date that falls into a closed sub-ledger accounting period.By default (Registry set to ‘No’), when a job’s Revenue Recognition Date belongs to closed accounting period, any further transactions posted against the job will be recognized ‘today’ in the current accounting period.When the registry is overridden and set to ‘Yes’, any subsequent transactions posted against the job will be recognized in the first open sub-ledger period.  Ticking ‘Yes’ on this registry item will recognize subsequent transactions in the profit and loss in the earliest open accounting period."),
						 RegistryStorageFlags.Company,
						 false);
				});
			}
		}

		#endregion

		#region BackDateARInvoicesSubCategory

		public BackDateInvoicesConfigurationRegistryItem BackDateInvoicesConfiguration
		{
			get
			{
				return GetItem("BackDateInvoicesConfiguration", delegate
				{
					return new BackDateInvoicesConfigurationRegistryItem(
						"BackDateInvoicesConfiguration",
						Categories.Accounting_JobInvoicing_BackDateARInvoices,
						ResString.GetMultilingualString("211F27E8-56CB-47f4-8171-F69A74D4BC83", "Back Date Invoices Configuration"),
						ResString.GetMultilingualString("5673AFF4-D560-44df-87D3-ECB35458C815", @"By default, the Post Date and Invoice Date is set to 'Today' when posting new job related AR Invoice and AR Credit Note transactions. By default users are not able to modify either the Invoice Date or Post Date on a new job related AR transaction. Use this registry to override the default behavior and enable the back dating of both Invoice Date and / or Post Date.
Depending on the combination of options ticked, this registry can enable the following features:
- Allow Invoice Date on a new job related AR transaction to be back dated to the last day of the previous month. 
- Allow users to manually override the Invoice Date on a new job related AR transaction before posting.
- Allow users to manually override the Post Date on a new job related AR transaction before posting.
- Default the Post Date on a new job related AR transaction from the overridden Invoice Date on a new job related AR transaction when the user has rights to modify the invoice date.

Note:  This registry will be ignored when the 'Invoice and Post Dates Defaulting Behavior' registry is set to 'MTH'."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		public BooleanRegistryItem DefaultAllowUsersToBackDateInvoicesSetting
		{
			get
			{
				return GetItem("DefaultAllowUsersToBackDateInvoicesSetting", delegate
				{
					return new BooleanRegistryItem(
						"DefaultAllowUsersToBackDateInvoicesSetting",
						Categories.Accounting_JobInvoicing_BackDateARInvoices,
						ResString.GetMultilingualString("B8282D44-E1EC-4eb3-B84C-EE887A1A2A94", "Default 'Allow Users to Back Date Invoices' Setting"),
						ResString.GetMultilingualString("EC5C3B1A-675E-4fe3-942B-3BF717D659D3", @"This registry defaults 'Yes' or 'No' into the 'New Back Date and/or Back Post AR Transactions' posting screen. That screen opens when AR Invoices are posted from Jobs and the 'Back Date Invoices Configuration' registry has been enabled to permit Invoice Date and/or Post Date back dating.

Note:  This registry will be ignored when the 'Invoice and Post Dates Defaulting Behavior' registry is set to 'MTH'."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public CodePairRegistryItem AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate
		{
			get
			{
				return GetItem("AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate", delegate
				{
					return new CodePairRegistryItem(
						"AllowARInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate",
						Categories.Accounting_Reversal,
						ResString.GetMultilingualString("3a24790f-31a4-4be9-ad47-397ff5121243", "Allow AR Reversal Invoice Date to Default to the Original Transaction Invoice Date"),
						ResString.GetMultilingualString("e7ae0fe8-b6fc-4966-a385-43f5a5fa4324", @"This registry item allows the accounts receivables invoice date for miscellaneous and periodic invoice reversals to default to the original transaction's invoice date.

Note: This registry will be ignored when the ‘Invoice and Post Dates Defaulting Behavior’ registry is set to ‘MTH’."),
						new CodeDescriptionPairListProvider(() => new ReversalDefaultFromOriginalInvoiceDateTypes()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingConstants.ReversalDefaultFromOriginalTransactionDate.TodaysDate);
				});
			}
		}

		public CodePairRegistryItem AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate
		{
			get
			{
				return GetItem("AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate", delegate
				{
					return new CodePairRegistryItem(
						"AllowARPostDateToDefaultToTheOriginalTransactionInvoiceDate",
						Categories.Accounting_Reversal,
						ResString.GetMultilingualString("56c4b118-9450-454e-95b9-5dbc91850314", "Allow AR Reversal Post Date to Default to the Original Transaction Invoice Date"),
						ResString.GetMultilingualString("7fb428f1-09f6-4813-8b99-4ef7c0431998", @"This registry item allows the accounts receivables post date for miscellaneous and periodic invoice reversals to default to the original transaction's invoice date.

Note: This registry will be ignored when the ‘Invoice and Post Dates Defaulting Behavior’ registry is set to ‘MTH’."),
						new CodeDescriptionPairListProvider(() => new ReversalDefaultFromOriginalInvoiceDateTypes()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingConstants.ReversalDefaultFromOriginalTransactionDate.TodaysDate);
				});
			}
		}

		public CodePairRegistryItem AllowARReversalDueDateCalculation
		{
			get
			{
				return GetItem("AllowARReversalDueDateCalculation", delegate
				{
					return new CodePairRegistryItem(
						"AllowARReversalDueDateCalculation",
						Categories.Accounting_Reversal,
						ResString.GetMultilingualString("b3c8e651-28a8-4c0c-a10e-a8079f4cddb1", "Allow Calculation of Due Date for AR Reversals"),
						ResString.GetMultilingualString("3c424b3f-61f4-4c25-b1eb-8be894bf85ae", "This registry item allows the calculation of the due date for accounts receivables invoice and credit note reversals."),
						new CodeDescriptionPairListProvider(() => new ReversalDueDateCalculationTypes(AccountingConstants.DebtorCreditorTerms.DebtorTerms)),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingConstants.ReversalDueDateCalculation.DebtorsTerms);
				});
			}
		}

		#endregion

		#region BackDateAPInvoicesSubCategory

		public BackDateAPInvoicesConfigurationRegistryItem BackDateAPInvoicesConfiguration
		{
			get
			{
				return GetItem("BackDateAPInvoicesConfiguration", delegate
				{
					return new BackDateAPInvoicesConfigurationRegistryItem(
						"BackDateAPInvoicesConfiguration",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("0d613f22-8fd8-4d0e-b474-a530f93ac3ff", "Back Date AP Invoices Configuration"),
						ResString.GetMultilingualString("2b4e3946-376a-49e7-92a9-a7a2456f3bd8", @"Allows the Post Date of AP Invoices to be back dated according to a set of rules.
It's possible to configure a Post Date defaulting rule depending on job type, direction, transport mode, custom broker and significant date.
It's possible to configure different Post Date defaulting rules depending on whether the ‘significant date’ falls into a prior closed, prior open, current and future accounting period."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		public CodePairRegistryItem AllowAPReversalDueDateCalculation
		{
			get
			{
				return GetItem("AllowAPReversalDueDateCalculation", delegate
				{
					return new CodePairRegistryItem(
						"AllowAPReversalDueDateCalculation",
						Categories.Accounting_Reversal,
						ResString.GetMultilingualString("45ab1bf0-5608-4518-871b-51ec7e790f27", "Allow Calculation of Due Date for AP Reversals"),
						ResString.GetMultilingualString("5a7a1d94-3378-4276-a1ef-1644d0104220", "This registry item allows the calculation of the due date for accounts payables invoice and credit note reversals."),
						new CodeDescriptionPairListProvider(() => new ReversalDueDateCalculationTypes(AccountingConstants.DebtorCreditorTerms.CreditorTerms)),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingConstants.ReversalDueDateCalculation.CreditorsTerms);
				});
			}
		}

		public BooleanRegistryItem AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate
		{
			get
			{
				return GetItem("AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate", delegate
				{
					return new BooleanRegistryItem(
						"AllowAPInvoiceDateToDefaultToTheOriginalTransactionInvoiceDate",
						Categories.Accounting_Reversal,
						ResString.GetMultilingualString("7d24bb99-8176-42a0-9154-45f805cfa812", "Allow AP Reversal Invoice Date to Default to the Original Transaction Invoice Date"),
						ResString.GetMultilingualString("6b0a59d3-9a06-4f27-87c5-02668b892170", "This registry item allows the accounts payables invoice and credit note reversals to have their invoice date default to the original transaction's invoice date."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region JobInvoiceDescription

		public JobInvoiceDescriptionRegistryItem JobInvoiceDescriptionConfiguration
		{
			get
			{
				return GetItem("JobInvoiceDescriptionConfiguration", delegate
				{
					return new JobInvoiceDescriptionRegistryItem(
						"JobInvoiceDescriptionConfiguration",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("c3a3a729-7f19-4362-a9a0-0207c14d264a", "Job Invoice Description Configuration"),
						ResString.GetMultilingualString("2842b69f-950a-43a7-b563-5611e60254f4", @"This registry enables you to define default job invoice description by Job Type, Direction and Mode. 
Additionally, you can include macro in the description and the system will pick up the relevant values. 
For instance, <JobHeader.JobNumber> will return the job reference number.

Except for ‘FCN’ Job Type, job invoice description will be defaulted into Operation > Billing module. 
This defaulted job invoice description can be overridden by authorized users where applicable. 

On posting of job related accounting transaction from operational modules, the job invoice description will be saved as transaction description.
This logic applies to the posting of:
•	Receivable Invoice and Credit Note
•	Payable Invoice and Credit Note including unapproved transactions
•	Payments 
•	Job Revenue Journals
•	CFX Journals

In the absence of ‘Job Invoice Description Configuration’ setting, the system will fall back to the ‘Transaction Description Defaults’ registry values."),
						RegistryStorageFlags.Company,
						new JobInvoiceDescriptionCollection());
				});
			}
		}

		#endregion

		Guid DefaultMenuItemGuid
		{
			get
			{
				Guid result;
				if (DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.Value)
				{
					result = DocBuilderMenuItemGuid;
				}
				else
				{
					result = OldStyleInvoiceMenuItemGuid;
				}
				return result;
			}
		}

		Guid DefaultLocalInvoiceMenuItemGuid
		{
			get
			{
				if (fDefaultLocalInvoiceMenuItemGuid == Guid.Empty)
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					StmMenuItem item = factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.ClassAInvoiceName));
					if (item != null)
					{
						fDefaultLocalInvoiceMenuItemGuid = item.PK.ToGuid();
					}
				}
				return fDefaultLocalInvoiceMenuItemGuid;
			}
		}
		Guid fDefaultLocalInvoiceMenuItemGuid;

		Guid fDocBuilderMenuItemGuid;
		Guid DocBuilderMenuItemGuid
		{
			get
			{
				if (fDocBuilderMenuItemGuid == Guid.Empty)
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					StmMenuItem item = factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.DocBuilderInvoiceName));
					if (item != null)
					{
						fDocBuilderMenuItemGuid = item.PK.ToGuid();
					}
				}
				return fDocBuilderMenuItemGuid;
			}
		}

		Guid fOldStyleInvoiceMenuItemGuid;
		Guid OldStyleInvoiceMenuItemGuid
		{
			get
			{
				if (fOldStyleInvoiceMenuItemGuid == Guid.Empty)
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();
					StmMenuItem item = factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.OldStyleInvoiceName));
					if (item != null)
					{
						fOldStyleInvoiceMenuItemGuid = item.PK.ToGuid();
					}
				}
				return fOldStyleInvoiceMenuItemGuid;
			}
		}

		#region Job Invoicing Default Departments

		public BooleanRegistryItem GatewayDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem(nameof(GatewayDefaultToCurrentLoginDept), delegate
				{
					return new BooleanRegistryItem(nameof(GatewayDefaultToCurrentLoginDept),
													Categories.Accounting_JobInvoicing_DefaultDepartments_Gateway,
													ResString.GetMultilingualString("8e3d7074-ff04-4f80-9871-cc6c94dabe77", "Default to Current Login Dept."),
													ResString.GetMultilingualString("04FC2EAF-34EC-4137-8B41-E2A849C8CB44", "When the value is set to Yes, the Job department will be defaulted to the department selected during login when creating new Gateway Billing job. Otherwise, the system will default department as setup for each operation Activity, Direction and Consol Type."),
													RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
													false);
				});
			}
		}

		public JobInvoicingDefaultGatewayDepartmentsRegistryItem JobInvoicingDefaultDepartmentGateway
		{
			get
			{
				var defaultDeptsCollection = new JobInvoicingDefaultGatewayDepartmentsCollection()
				{
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Domestic, TransportMode = Constants.TransportModes.Air, Department = RegistryConstants.DepartmentPKs.GatewayDomesticAir },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Domestic, TransportMode = Constants.TransportModes.Rail, Department = RegistryConstants.DepartmentPKs.GatewayDomesticRail },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Domestic, TransportMode = Constants.TransportModes.Sea, Department = RegistryConstants.DepartmentPKs.GatewayDomesticSea },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Domestic, TransportMode = Constants.TransportModes.Road, Department = RegistryConstants.DepartmentPKs.GatewayDomesticRoad },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Export, TransportMode = Constants.TransportModes.Air, Department = RegistryConstants.DepartmentPKs.GatewayExportAir },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Export, TransportMode = Constants.TransportModes.Rail, Department = RegistryConstants.DepartmentPKs.GatewayExportRail },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Export, TransportMode = Constants.TransportModes.Sea, Department = RegistryConstants.DepartmentPKs.GatewayExportSea },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Export, TransportMode = Constants.TransportModes.Road, Department = RegistryConstants.DepartmentPKs.GatewayExportRoad },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Other, TransportMode = Constants.TransportModes.Air, Department = RegistryConstants.DepartmentPKs.GatewayImportAir },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Other, TransportMode = Constants.TransportModes.Rail, Department = RegistryConstants.DepartmentPKs.GatewayImportRail },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Other, TransportMode = Constants.TransportModes.Sea, Department = RegistryConstants.DepartmentPKs.GatewayImportSea },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Other, TransportMode = Constants.TransportModes.Road, Department = RegistryConstants.DepartmentPKs.GatewayImportRoad },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Import, TransportMode = Constants.TransportModes.Air, Department = RegistryConstants.DepartmentPKs.GatewayImportAir },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Import, TransportMode = Constants.TransportModes.Rail, Department = RegistryConstants.DepartmentPKs.GatewayImportRail },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Import, TransportMode = Constants.TransportModes.Sea, Department = RegistryConstants.DepartmentPKs.GatewayImportSea },
					new JobInvoicingDefaultGatewayDepartments(true) { ConsolType = Constants.JobInvoicingDefaultDepartmentConsolType.All, Direction = Constants.FreightShipmentDirection.Code.Import, TransportMode = Constants.TransportModes.Road, Department = RegistryConstants.DepartmentPKs.GatewayImportRoad },
				};

				return GetItem(nameof(JobInvoicingDefaultDepartmentGateway),
					() => new JobInvoicingDefaultGatewayDepartmentsRegistryItem(
						nameof(JobInvoicingDefaultDepartmentGateway),
						Categories.Accounting_JobInvoicing_DefaultDepartments_Gateway,
						ResString.GetMultilingualString("0EE4E26C-DD64-4F9B-BD40-C2C1972D4CB3", "Gateway Billing"),
						JobInvoicingDefaultGatewayDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						defaultDeptsCollection
					)
				);
			}
		}

		MultilingualString JobInvoicingDefaultGatewayDepartmentHint
		{
			get
			{
				return ResString.GetMultilingualString("157C3EA8-3D09-49C7-92CC-DBE2F1E7C27D", @"When creating an Accounting Job Header on a Gateway Consolidation {0} uses this registry to determine the Job Header’s Department.
Note:  This registry is NOT used when the ‘Default to Current Login Department’ registry option has been enabled.", BrandingFactory.Instance.ProductName);
			}
		}

		MultilingualString JobInvoicingDefaultDepartmentHint
		{
			get
			{
				return ResString.GetMultilingualString("c5387f78-5819-4858-b908-22db743d7f11", @"When creating an Accounting Job Header on a Forwarding Shipment {0} uses this registry to determine the Job Header’s Department.
Note:  This registry is NOT used when the ‘Default to Current Login Department’ registry option has been enabled.", BrandingFactory.Instance.ProductName);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingExportSeaFcl
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingExportSeaFcl",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingExportSeaFcl",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("5ca908d8-7510-4366-bd94-b3228c14ba29", "Forwarding Export Sea FCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingExportSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingExportSeaLcl
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingExportSeaLcl",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingExportSeaLcl",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("7d979953-9366-47f3-8487-7e656dd15174", "Forwarding Export Sea LCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingExportSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingExportSeaOther
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingExportSeaOther",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingExportSeaOther",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("1d19f5ca-e010-47d5-a98a-48f9854e519f", "Forwarding Export Sea Other"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingExportSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingExportAirULD
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingExportAirULD",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingExportAirULD",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("95225c18-13c7-46e2-8be7-18942828f813", "Forwarding Export Air ULD"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingExportAir
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingExportAirOther
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingExportAirOther",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingExportAirOther",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("d1240dae-1520-44bc-8517-332bf58a74b1", "Forwarding Export Air Other"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingExportAir
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingExportRailFCL
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingExportRailFCL",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingExportRailFCL",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("f4240cb7-5d02-4dae-881b-042faa4a0d8f", "Forwarding Export Rail FCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingExportRail
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingExportRailLCL
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingExportRailLCL",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingExportRailLCL",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("43971fe8-aecf-4554-9677-631d5bc283d0", "Forwarding Export Rail LCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingExportRail
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingExportRail
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingExportRail",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingExportRail",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("635a5654-e0f3-4777-848f-035eb2dd6270", "Forwarding Export Rail"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingExportRail
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingExportRoad
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingExportRoad",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingExportRoad",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("6e9ef621-c78e-4f5a-af48-901812f6f162", "Forwarding Export Road"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingExportRoad
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingImportSeaFcl
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingImportSeaFcl",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingImportSeaFcl",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("3854db2b-6f7b-4958-b8b1-e38ae52136c5", "Forwarding Import Sea FCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingImportSeaLcl
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingImportSeaLcl",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingImportSeaLcl",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("2513de25-042d-4e45-9f55-a2119123d0a3", "Forwarding Import Sea LCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingImportSeaOther
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingImportSeaOther",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingImportSeaOther",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("40d55d82-92b3-482a-84cf-12b12dc1876a", "Forwarding Import Sea Other"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingImportAirULD
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingImportAirULD",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingImportAirULD",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("b4bdd025-bab1-4475-b3ab-232013e6a17c", "Forwarding Import Air ULD"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportAir
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingImportAirOther
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingImportAirOther",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingImportAirOther",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("eaca6405-538d-42a9-b13b-fa79ee70358e", "Forwarding Import Air Other"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportAir
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingImportRailFCL
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingImportRailFCL",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingImportRailFCL",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("66854a7d-3510-47db-9e9d-99d05c68e627", "Forwarding Import Rail FCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportRail
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingImportRailLCL
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingImportRailLCL",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingImportRailLCL",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("25769f08-a516-447e-bcf6-a0d215759d5e", "Forwarding Import Rail LCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportRail
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingImportRail
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingImportRail",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingImportRail",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("9c62728b-f471-499d-981f-17e5d58b0a69", "Forwarding Import Rail"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportRail
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingImportRoad
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingImportRoad",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingImportRoad",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("1c3dfd5e-ff33-42ff-a8c8-411e69967a2b", "Forwarding Import Road"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportRoad
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingForeignSeaFcl
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingForeignSeaFcl",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingForeignSeaFcl",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("32a31764-dc5e-4187-9c2f-51023a23af7b", "Forwarding Foreign Sea FCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingForeignSeaLcl
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingForeignSeaLcl",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingForeignSeaLcl",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("f2d1b4c7-587a-4b42-87f2-96c94c99a7ec", "Forwarding Foreign Sea LCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingForeignSeaOther
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingForeignSeaOther",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingForeignSeaOther",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("06c14762-d146-4951-85aa-c2ce8548f86b", "Forwarding Foreign Sea Other"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingForeignAirULD
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingForeignAirULD",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingForeignAirULD",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("10889ee3-b447-480a-bead-7f7c0729a506", "Forwarding Foreign Air ULD"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportAir
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingForeignAirOther
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingForeignAirOther",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingForeignAirOther",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("922dd169-6dc4-4df6-b671-7de3c7d26406", "Forwarding Foreign Air Other"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportAir
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingForeignRail
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingForeignRail",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingForeignRail",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("011a726b-9f2e-4e4a-b7b2-089eb45e6778", "Forwarding Foreign Rail"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportRail
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingForeignRoad
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingForeignRoad",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingForeignRoad",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("91fabebb-c858-4d5c-9e72-38bb4660258d", "Forwarding Foreign Road"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingImportRoad
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingDomesticSeaFcl
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingDomesticSeaFcl",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingDomesticSeaFcl",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("088dfc88-6ea9-4ac0-ac31-0f6b937a424d", "Forwarding Domestic Sea FCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingDomesticSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingDomesticSeaLcl
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingDomesticSeaLcl",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingDomesticSeaLcl",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("18965807-bbf2-4390-bb56-8bfb837fe1d7", "Forwarding Domestic Sea LCL"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingDomesticSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingDomesticSeaOther
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingDomesticSeaOther",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingDomesticSeaOther",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("8091b26c-41ba-491c-83c5-5c34cc21a807", "Forwarding Domestic Sea Other"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingDomesticSea
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingDomesticAirULD
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingDomesticAirULD",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingDomesticAirULD",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("69d2efb6-3afc-44fd-8abd-ef97d92377cb", "Forwarding Domestic Air ULD"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingDomesticAir
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingDomesticAirOther
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingDomesticAirOther",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingDomesticAirOther",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("ec1e7467-1c97-450b-a453-7ebba813760e", "Forwarding Domestic Air Other"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingDomesticAir
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingDomesticRail
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingDomesticRail",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingDomesticRail",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("5027b742-f703-4c85-a2e1-e29ce67328cc", "Forwarding Domestic Rail"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingDomesticRail
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentForwardingDomesticRoad
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentForwardingDomesticRoad",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentForwardingDomesticRoad",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("9e14679b-cc7a-4d52-8551-087534f75183", "Forwarding Domestic Road"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ForwardingDomesticRoad
					)
				);
			}
		}

		public JobInvoicingDefaultDepartmentsRegistryItem JobInvoicingDefaultDepartmentMasterAWBDefaultDept
		{
			get
			{
				return GetItem("JobInvoicingDefaultDepartmentMasterAWBDefaultDept",
					() => new JobInvoicingDefaultDepartmentsRegistryItem(
						"JobInvoicingDefaultDepartmentMasterAWBDefaultDept",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
						ResString.GetMultilingualString("07704cd6-0893-40e5-9999-3e05b1f71f82", "Master AWB"),
						JobInvoicingDefaultDepartmentHint,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.MasterAwb
					)
				);
			}
		}

		#endregion

		#endregion

		#region ENett

		public BooleanRegistryItem ENettUseInvoiceExchangeRateForForeignCurrencyReceipts
		{
			get
			{
				return GetItem("ENettUseInvoiceExchangeRateForForeignCurrencyReceipts", () => new BooleanRegistryItem(
					"ENettUseInvoiceExchangeRateForForeignCurrencyReceipts",
					Categories.Accounting_ComPay,
					ResString.GetMultilingualString("5db5c35e-6361-4fa6-be25-47ec63470080", "Use Invoice Exchange Rate For Foreign Currency Receipts"),
					ResString.GetMultilingualString("96b33a28-fd42-416c-a112-05ea82c8de8f", @"When receipting foreign currency receipts through ComPay, the current exchange rate is used by default.
			When this registry is overridden to 'Yes', {0} will calculate an exchange rate based on the foreign currency invoices that are being paid.
			The effect of setting this registry to 'Yes' is that any exchange differences aren't taken up until you revalue your foreign currency bank account.", BrandingFactory.Instance.ProductName),
					RegistryStorageFlags.Company,
					false));
			}
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem EnableExtraLoggingForENettWebExceptions
		{
			get
			{
				return GetItem("EnableExtraLoggingForENettWebExceptions", delegate
				{
					MultilingualString captionAndHint = (NoResString)"Enable Extra Logging for Web Exceptions";
					return new BooleanRegistryItem(
						 "EnableExtraLoggingForENettWebExceptions",
						 Categories.Accounting_ComPay,
						 captionAndHint,
						 captionAndHint,
						 RegistryStorageFlags.System,
						 RegistryOptions.IsOnlyForDevelopers,
						 false);
				});
			}
		}

		public BooleanRegistryItem IncludeTimeZoneInformationForLastUpdateDate
		{
			get
			{
				return GetItem("IncludeTimeZoneInformationForLastUpdateDate", delegate
				{
					MultilingualString caption = (NoResString)"Include Time Zone Information For Last Update Date";
					MultilingualString hint = (NoResString)@"The lastUpdate parameter is passed to the Compay web service.
This registry setting controls the date format that is sent.
No - no time zone will be passed to the web service and it will look like this: 2014-02-04-T16:01:00.
Yes - the local time zone will be passed to the web service and it will look like this: 2014-02-04-T16:01:00+10:00.

This will affect how the compay web service handles the date internally and is relevant for clients who are not in Sydney or Melboure time zones.";

					return new BooleanRegistryItem(
						 "IncludeTimeZoneInformationForLastUpdateDate",
						 Categories.Accounting_ComPay,
						 caption,
						 hint,
						 RegistryStorageFlags.System,
						 RegistryOptions.IsOnlyForDevelopers,
						 false);
				});
			}
		}

		#endregion

		#region eNett Container Storage Payment

		public GuidRegistryItem ENettStoragePaymentChargeCode
		{
			get
			{
				return GetItem("ENettStoragePaymentChargeCode", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ENettStoragePaymentChargeCode",
						Categories.Accounting_ComPay,
						ResString.GetMultilingualString("CDF96141-067D-43DB-AA7C-81AD4B0489B1", "Storage Payments Charge Code"),
						ResString.GetMultilingualString("3E55CAF5-6E89-428C-A63B-E85CF78D4270", "This defines the default charge code to use when making Container Storage payments."),
						RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode);
					return result;
				});
			}
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem EnableCreditCardPaymentsViaComPay
		{
			get
			{
				return GetItem("EnableCreditCardPaymentsViaComPay", delegate
				{
					return new BooleanRegistryItem(
						"EnableCreditCardPaymentsViaComPay",
						Categories.Accounting_ComPay,
						(NoResString)"Enable Credit Card Payments Via ComPay",
						(NoResString)"This registry item will enable credit card payments to be made via ComPay.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion

		public BooleanRegistryItem CreateAPInvoiceOnENettImport
		{
			get
			{
				return GetItem("CreateAPInvoiceOnENettImport", delegate
				{
					MultilingualString caption = ResString.GetMultilingualString("5c67f8d0-f88a-4a41-ad23-969b441e4a67", "Create Accounts Payable Invoice On ComPay Import");
					MultilingualString hint = ResString.GetMultilingualString("ca2bdd9e-f362-47a7-a9a5-d082fe2f6fbe", "This registry item allows you to override the default behavior of creating an Accounts Payable invoice on a ComPay import.  If this is set to 'No', an Unapproved invoice is created instead.");
					return new BooleanRegistryItem(
						 "CreateAPInvoiceOnENettImport",
						 Categories.Accounting_ComPay,
						 caption,
						 hint,
						 RegistryStorageFlags.Company,
						 true);
				});
			}
		}

		#endregion

		public BooleanRegistryItem AutomaticallyReDefaultDebtorsWhenIncoTermChanges
		{
			get
			{
				return GetItem("AutomaticallyReDefaultDebtorsWhenIncoTermChanges",
					() => new BooleanRegistryItem(
						"AutomaticallyReDefaultDebtorsWhenIncoTermChanges",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("bdc198b5-662a-0d8c-40f6-e2352d0e8573", "Automatically re-default debtors on the billing tab when Incoterm changed"),
						ResString.GetMultilingualString("07567383-caba-1787-4373-5569c0f5e252", @"This registry controls the re-defaulting behavior of a charge line's Debtor each time a Forwarding Shipment's Incoterm is changed.  
By default this registry is set to No.
By default, users are asked if they want to re-default a job's Debtor's each time they save changes to a shipment's Incoterm.
When this registry is overridden and set to Yes, this re-defaulting behavior will automatically occur."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		#region Payable Defaults

		#region Default Settings Category

		public CodePairRegistryItem DefaultPaymentType
		{
			get
			{
				return GetItem("DefaultPaymentType", delegate
				{
					return new DefaultPaymentTypeRegistryItem(
						"DefaultPaymentType",
						Categories.Accounting_PayableDefaults,
						ResString.GetMultilingualString("b9efdad2-d217-48d7-be03-9217562762a2", "Default Payment Type"),
						ResString.GetMultilingualString("266b2c03-b3a6-4164-ad09-a3170fa15219", @"This registry item provides you with the ability to nominate the default Payment Type for any new payment created in the Payables and Receivables. 

By default the Payment Type is CHQ – cheque"),
						new DefaultPaymentTypeListProvider(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ReceiptTypes.Cheque);
				});
			}
		}

		public BooleanRegistryItem DefaultExpectedTotalValue
		{
			get
			{
				return GetItem("DefaultExpectedTotalValue", delegate
				{
					return new BooleanRegistryItem(
						"DefaultExpectedTotalValue",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("b7856bd4-d4ca-448e-9f0d-56c77912bf74", "Default Expected Total Value"),
						ResString.GetMultilingualString("5b9e88b6-9c7f-4650-be34-563d8f83848f", "This registry will set the default value for the 'Expected Total' checkbox field on the AP Invoice, AP Credit Note and AP Adjustment Note screen."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem UseJobExchangeRateDefault
		{
			get
			{
				return GetItem("UseJobExchangeRateDefault", delegate
				{
					return new BooleanRegistryItem(
						"UseJobExchangeRateDefault",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("61380e58-aa37-4d46-9932-8e05fea69e96", "Default Use Job Exchange Rate Setting"),
						ResString.GetMultilingualString("91ede0f4-5fc6-4194-bc77-d1cd92b5b9b8", "This registry will set the default value for the 'Use Job Exchange Rate' field on the AP Invoice and AP Credit Note screen, when the AP Invoice/AP Credit Note currency is not the local currency of the current login company."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem EnforceExpectedTotalTaxAndExcludingTaxAmountValidation
		{
			get
			{
				return GetItem("EnforceExpectedTotalTaxAndExcludingTaxAmountValidation", delegate
				{
					var item = new BooleanRegistryItem(
						"EnforceExpectedTotalTaxAndExcludingTaxAmountValidation",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("157a0847-0c3e-45af-ade7-f3b785c291cc", "Enforce Expected Total Tax and Excluding Tax Amount Validation"),
						ResString.GetMultilingualString("d79ba41c-ce32-4005-9f4c-880639427571", @"This registry affects the posting behavior of Payable Invoice and Credit Note only.

By default, this registry is set to 'No' and the system will allow posting as long as the Invoice Total (inclusive of tax) agrees with the Expected Total (inclusive of tax).

If you want the system to prevent posting when the Invoice Total Ex-Tax and/or Tax Amount does not agree with the respective Expected Totals, set this registry to 'Yes'.

When this registry is set to 'Yes', a validation error will be shown instead of a warning message when there are any discrepancies."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);

					item.OnBuildLogReference += (args) => Res.GetString("5fec9bf6-a2f2-4679-a2e5-9f83be1aed89", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		public TransactionsNumberSequenceCustomisationRegistryItem TransactionsNumberSequenceCustomisation
		{
			get
			{
				return GetItem("TransactionNumberSequenceCustomisation", () => new TransactionsNumberSequenceCustomisationRegistryItem
					(
						"TransactionNumberSequenceCustomisation",
						Categories.Accounting,
						ResString.GetMultilingualString("61a28b51-cc95-441c-88e3-0a3fab442f70", "Number Sequence Customization"),
						ResString.GetMultilingualString("245B60EE-1E8F-41ae-987A-146C26B9FE2F", @"By default, an eight characters sequential transaction number is assigned to each transaction for all non-China login companies (E.g. 00001000). For all China login companies, a ten characters sequential transaction number comprises of two digits Accounting Year, two digits Accounting Period and six digits sequential number is assigned to each transaction (E.g. 1801001000).

Typically, all transactions of a particular ledger and type (E.g. AR Receipt, AR Invoice, AP Journal) are assigned sequential numbers from a number fountain specific to the ledger and transaction type. For example, AR Receipts are numbered sequentially from one number fountain, while AP Journals will draw their sequential transaction numbers from a separate number fountain.

When this registry is overridden, the transaction number assigned to each transaction can be a mix of Alpha Numeric characters derived from a combination of the listed elements. The ‘order’ assigned against each included element determines the order in which the elements will be combined when creating and assigning new transaction numbers.

Additionally, if the ‘Fountain’ check box of an element is ticked, this will extend the basic number fountain behavior for each transaction type and ledger combination. Each unique combination of the additionally included elements with ‘Fountain’ check box ticked will be numbered sequentially from a separate number fountain."),
						RegistryStorageFlags.Company
					)
				);
			}
		}

		public JobsNumberSequenceCustomisationRegistryItem JobsNumberSequenceCustomisation
		{
			get
			{
				return GetItem("JobNumberSequenceCustomisation", () => new JobsNumberSequenceCustomisationRegistryItem
					(
						"JobNumberSequenceCustomisation",
						Categories.Accounting,
						ResString.GetMultilingualString("bb212940-18a1-4a31-b3fb-a370e37c972c", "Local Job Reference Sequence Customization"),
						ResString.GetMultilingualString("0c7ed376-94e5-4180-8ef5-71e78abbc75d", @"By default, an eight character, sequential local job number is assigned to each job header created by {0}.
When overridden, this registry can change this behavior. When overridden the local job reference can be a mix of Alpha Numeric characters derived from a combination of up to five included elements with a maximum length of 20. 
The “order” assigned against each included element determines the order in which the elements will be combined when creating and assigning new local job reference.
Additionally, including Custom 1, Custom 2, Job Branch and Department elements in the Number Sequence Customization extends the basic number fountain behavior. Each unique combination of the additionally included elements will be numbered sequentially from a separate number fountain.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company
					)
				);
			}
		}

		public IntercompanyClearingConfigurationRegistryItem IntercompanyClearingConfiguration
		{
			get
			{
				return GetItem("IntercompanyClearingConfiguration", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("23a521ce-45f5-4734-8aba-6be4c2cb4648", "The General Ledger Intercompany Clearing Accounts defined against this registry are used by {0} when automatically allocating AP transactions across multiple login companies.  The General Ledger Clearing accounts defined here will be used by {0} when posting AP transactions in one company and automatically creating corresponding General ledger journals in a sister company.", BrandingFactory.Instance.ProductName);
					return new IntercompanyClearingConfigurationRegistryItem(
						"IntercompanyClearingConfiguration",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("e6a81a36-d5e6-439e-b4de-afbad55714cf", "Intercompany Clearing Configuration"),
						hint,
						RegistryStorageFlags.Company);
				});
			}
		}

		public CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem PaymentRequisitionStatuses
		{
			get
			{
				var defaultValue = new PaymentCriticalitySettingsList { DefaultCode = PaymentCriticalitySettingsList.Codes.Normal };
				return GetItem("PaymentRequisitionStatuses",
					() => new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem(
						"PaymentRequisitionStatuses", Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("e9967e6e-5dd2-4a38-89e5-d895b4dd60eb", "Payment Requisition Statuses"),
						ResString.GetMultilingualString("20be4f5a-e048-4de6-931b-bce6a09523f9", @"This registry item defines a set of Payment Requisition codes that are used to identify how urgently each Account Payable Invoice and Unapproved Invoice needs to be paid.
	When posting a new AP Invoice, one of these status codes can be recorded against the transaction. Then, when creating Payments in the Accounts Payables modules, the Payment Requisition Status filters can be used to identify, group and prioritize outstanding invoices for payment.  
	Note: When paying invoices that have a payment requisition status with the ‘Create Separate Payments’ flag ticked in the grid below, the ‘Pay Invoices’ function will create separate payments for those invoices when grouping by payment requisition status.
	Note: The code flagged in this grid as the ‘default’ will be the default payment status for all Accounts Payable and unapproved invoices. 
	Note: When creating payments using the Payables Module’s ‘Pay Invoices’ function and ‘Grouping Payments by Requisition Status’, a separate payments will be created for individual invoices when a Payment Requisition Status code is also flagged to ‘Create Separate Payments’."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new PaymentRequisitionStatusesDataType(defaultValue)));
			}
		}

		public BooleanRegistryItem AllowIncludingRelatedTransasctionDebtorColumnOfAP
		{
			get
			{
				return GetItem("AllowIncludingRelatedTransasctionDebtorColumnOfAP",
					() => new BooleanRegistryItem(
						"AllowIncludingRelatedTransasctionDebtorColumnOfAP",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("fdb0adf6-cd15-4e98-8a61-08fcbed10609", "Show Related Debtor Column"),
						ResString.GetMultilingualString("3e4e1304-f4f0-40c4-9559-0bb4aae260d4", "Setting this registry to 'Yes' will allow users to include Related Transaction Debtor Column in Account Payable session"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		public BooleanRegistryItem AllowIncludingRelatedTransasctionDebtorColumnOfAR
		{
			get
			{
				return GetItem("AllowIncludingRelatedTransasctionDebtorColumnOfAR",
					() => new BooleanRegistryItem(
						"AllowIncludingRelatedTransasctionDebtorColumnOfAR",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("d224ead0-990b-4574-95e7-f30f36ed6396", "Show Related Creditor Column"),
						ResString.GetMultilingualString("851f4bfb-fc11-4781-a2c4-100e6c141494", "Setting this registry to 'Yes' will allow users to include Related Transaction Debtor Column in Account Receivables session"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		public BooleanRegistryItem AllowDescriptionChargeLineFilterInPeriodicInvoice
		{
			get
			{
				return GetItem("AllowDescriptionChargeLineFilterInPeriodicInvoice",
					() => new BooleanRegistryItem(
						"AllowDescriptionChargeLineFilterInPeriodicInvoice",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("abd6b942-4d36-4826-aa52-c4e3e6f1836a", "Enable Charge Line Description Filter in Periodic Invoice"),
						ResString.GetMultilingualString("68d4c6e3-bbf3-4700-9058-bf6646786ab3", "Please do not enable this registry - it may have serious performance implications for any system that uses it and it will be removed very soon"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public BooleanRegistryItem DisallowSelectionOfSBRandSBDInvoiceType
		{
			get
			{
				return GetItem("DisallowSelectionOfSBRandSBDInvoiceType",
					() => new BooleanRegistryItem(
						"DisallowSelectionOfSBRandSBDInvoiceType",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("A862EE16-C372-443B-AD2E-7ECB293B86D6", "Disallow selection of 'SBR' and 'SBD' invoice type if Charge Debtor is not setup as Self Bills Customer"),
						ResString.GetMultilingualString("FEA1B4B6-D173-4354-89C5-0E9CCCECD00D", @"By default, this registry is set to 'No' and the system will always allow users to select invoice type 'SBR' and 'SBD' when entering job billing charges, as per current system behavior.

When this registry is set to 'Yes', the system will disallow users from selecting invoice type 'SBR' and 'SBD' if the Charge Debtor's Organization > A/R > Invoicing > Invoicing > Customer Self Bills check box is NOT ticked."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		public ChargeCodeListRegistryItem AllowChargeDescriptionOverrideOnPostedARInvoice
		{
			get
			{
				return GetItem("AllowChargeDescriptionOverrideOnPostedARInvoice",
					() => new ChargeCodeListRegistryItem(
						"AllowChargeDescriptionOverrideOnPostedARInvoice",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("c40cb016-dabc-48f9-ac8c-c9ba49cb27c4", "Allow Description Override on AR Invoice Lines"),
						ResString.GetMultilingualString("7cbefd74-a898-4d86-903d-faab106dae4f", "Charge codes listed in this registry will allow the description on invoice lines to be edited after posting. Edits will also affect the description on the charges seen in the billing tab."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ZString.Empty,
						RegistryFindBoxFilter.None)
				);
			}
		}

		public BooleanRegistryItem AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing
		{
			get
			{
				return GetItem("AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing",
					() => new BooleanRegistryItem(
						"AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						(NoResString)"Allow Charge To Retrieve Sell Invoice Exchange Rate From Job During Periodic Invoicing (CargoWiseOne Support Only)",
						(NoResString)"This aims to fix a bug where the sell invoice currency shows as 0 when periodic invoicing.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true)
				);
			}
		}

		public BooleanRegistryItem AllowPaymentRequisitionStatusOverride
		{
			get
			{
				return GetItem("AllowPaymentRequisitionStatusOverride",
					() => new BooleanRegistryItem(
						"AllowPaymentRequisitionStatusOverride",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("1d981d0c-05b5-41c2-985f-6ca11299bc9d", "Allow Payment Requisition Status Override"),
						ResString.GetMultilingualString("268c0d2b-a6ca-46f6-b027-8992a982b66d", "When this registry is set to ‘Yes’, {0} will popup a form to allow users to override the default payment requisition status on Accounts Payable or Unapproved Invoices. The payment requisition statuses are configured in the registry item Payable Defaults > Payment Requisition Statuses.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		public IntRegistryItem SizeOfRequestedAPTransactionList
		{
			get
			{
				return GetItem("SizeOfRequestedAPTransactionList", delegate
				{
					var result = new IntRegistryItem(
						"SizeOfRequestedAPTransactionList",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_Turkey,
						ResString.GetMultilingualString("8B4D9AD2-A51A-4A43-9555-1867F5E4357B", "Size Of Requested AP Transaction List"),
						ResString.GetMultilingualString("084D9C6D-C03C-42AA-85D8-505231E14432", @"This registry is currently only referenced by Turkey login companies.
Use this registry to adjust the count of invoices included per AP list response. It can be more efficient to nominate a smaller invoice count per list request and request more regularly as opposed to very large invoice list requests.
This registry item should be set with consideration of the setting of registry item '{0}'.", AccountingMasterFilesRegistry.Instance.APListAutomatedRequestSchedule.Caption),
						RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.EnableNewTurkeyAPComplianceFeatures ? RegistryOptions.Default : RegistryOptions.IsHidden,
						20,
						1,
						1000);
					result.CountryFilterPKs = CountryFilterPKs.Turkey;
					return result;
				}
				);
			}
		}

		#region Import Accruals Configurations

		public BooleanRegistryItem PopupImportAccrualsScreenOnAPInvoice
		{
			get
			{
				return GetItem("PopupImportAccrualsScreenOnAPInvoice", delegate
				{
					return new BooleanRegistryItem(
					"PopupImportAccrualsScreenOnAPInvoice",
					Categories.Accounting_PayableDefaults_DefaultSettings_ImportAccrualsConfigurations,
					ResString.GetMultilingualString("a65ab9f5-695d-4116-9f10-801c81879985", "Popup Import Accruals Screen on AP Invoice"),
					ResString.GetMultilingualString("7e8fcf20-c4f4-4714-8501-8d4408c16ecc", "When entering the job number and/or charge code in AP Invoices, a screen containing all current accruals will be shown depending on the value of this registry item."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					true);
				});
			}
		}

		public BooleanRegistryItem IncludeChargesForAllOtherCreditors
		{
			get
			{
				return GetItem("IncludeChargesForAllOtherCreditors", delegate
				{
					return new BooleanRegistryItem(
					"IncludeChargesForAllOtherCreditors",
					Categories.Accounting_PayableDefaults_DefaultSettings_ImportAccrualsConfigurations,
					ResString.GetMultilingualString("55B91370-B9A2-4a9f-A3FD-6BB0777E1145", "Include Charges for All Other Creditors"),
					ResString.GetMultilingualString("61196E4F-32D8-400d-A9C9-4C60CB5C76E1", "Default state of 'Include Charges for All Other Creditors' check box."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					false);
				});
			}
		}

		public BooleanRegistryItem IncludeChargesForCreditorsWithTheSameAPSettlementGroup
		{
			get
			{
				return GetItem("IncludeChargesForCreditorsWithTheSameAPSettlementGroup", delegate
				{
					return new BooleanRegistryItem(
					"IncludeChargesForCreditorsWithTheSameAPSettlementGroup",
					Categories.Accounting_PayableDefaults_DefaultSettings_ImportAccrualsConfigurations,
					ResString.GetMultilingualString("EBC65264-E1EF-437B-B09E-36B2753FEAB8", "Include Charges for Creditors with the same AP Settlement Group"),
					ResString.GetMultilingualString("554DCE8C-01C4-41c8-943E-3DA2F648D65D", "Default state of 'Include Charges for Creditors with the same AP Settlement Group' check box."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					false);
				});
			}
		}

		#endregion

		#region Pay Invoices

		public BooleanRegistryItem PayInvoicesAllowFurtherGrouping
		{
			get
			{
				return GetItem("PayInvoicesAllowFurtherGrouping",
					() => new BooleanRegistryItem(
						"PayInvoicesAllowFurtherGrouping",
						Categories.Accounting_PayableDefaults_DefaultSettings_PayInvoices,
						ResString.GetMultilingualString("bb60ac04-4de8-4489-ba9b-9e72390bac30", "Allow ‘Pay Invoices’ Payment Grouping Override"),
						ResString.GetMultilingualString("546e60ac-1336-459f-ad10-046940ec703c", @"When overridden and set to ‘Yes’ users with appropriate security rights will be able to override the default payment grouping behaviors of the ‘Pay Invoices’ function in the Accounts Payables module.   
A ‘Payment Grouping’ form will open when using the ‘Pay Invoices’ function and when this registry is set to ‘Yes’. From within that form users can OVERRIDE the defaulted payment grouping behaviors. Those defaulted behaviors are defined in three related registry items (Default ‘Group by Invoice Payment Requisition Status’; Default ‘Group by Invoice Related Debtor’; and Default ‘Group by Invoice Creating User’.)
Note:  Users will be unable to override the default ‘Pay Invoices’ grouping behaviors when this Payment Grouping Override registry is set to ‘No’."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		public BooleanRegistryItem PayInvoicesDefaultGroupByInvoicePaymentPaymentRequisitionStatus
		{
			get
			{
				return GetItem("PayInvoicesDefaultGroupByInvoicePaymentPaymentRequisitionStatus",
					() => new BooleanRegistryItem(
						"PayInvoicesDefaultGroupByInvoicePaymentPaymentRequisitionStatus",
						Categories.Accounting_PayableDefaults_DefaultSettings_PayInvoices,
						ResString.GetMultilingualString("d19f5def-6c3a-4016-9bc3-796b75eeb7ad", "Default ‘Group by Invoice Payment Requisition Status’"),
						ResString.GetMultilingualString("b9aeb664-b1da-42e9-a6fb-fde5f95d7553", @"When paying invoices through the ‘Pay Invoices’ function, if multiple invoices for the same creditor have different payment requisition statuses, {0} will create separate payments for each of the unique payment requisition statuses on the invoices being paid.
Further, {0} will create separate payments for each invoice where the invoice’s payment requisition status is flagged to ‘Create Separate Payments’ in the registry ‘Payable Defaults > Payment Requisition Statuses’.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		public BooleanRegistryItem PayInvoicesDefaultGroupByInvoiceRelatedDebtorOrganisation
		{
			get
			{
				return GetItem("PayInvoicesDefaultGroupByInvoiceRelatedDebtorOrganisation",
					() => new BooleanRegistryItem(
						"PayInvoicesDefaultGroupByInvoiceRelatedDebtorOrganisation",
						Categories.Accounting_PayableDefaults_DefaultSettings_PayInvoices,
						ResString.GetMultilingualString("60b0ff05-ba46-4ab0-bd9b-cfcad90b7d52", "Default ‘Group by Invoice Related Debtor Organization’"),
						ResString.GetMultilingualString("6bca1403-ff61-4f78-8862-1943d9027a6f", @"This registry is used by the Payables ‘Pay Invoices’ function.
	When enabled and set to ‘Yes’, the {0} ‘Pay Invoices’ function will create separate payments for a creditor by evaluating the Related Invoice Debtor codes associated with each outstanding invoice. Invoices with the same Related Invoice Debtor will be grouped together and paid in a single payment. Individual payments will be issued for any invoices with more than one ‘Related Invoice Debtor’.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		public BooleanRegistryItem PayInvoicesDefaultGroupByInvoiceCreatingUser
		{
			get
			{
				return GetItem("PayInvoicesDefaultGroupByInvoiceCreatingUser",
					() => new BooleanRegistryItem(
						"PayInvoicesDefaultGroupByInvoiceCreatingUser",
						Categories.Accounting_PayableDefaults_DefaultSettings_PayInvoices,
						ResString.GetMultilingualString("6c9a5403-18d4-4618-b228-21e4e045e234", "Default ‘Group by Invoice Creating User’"),
						ResString.GetMultilingualString("86ad4175-0bcc-4803-8be1-26d7a9072d36", @"If this registry is set to ‘Yes’, when paying invoices through the ‘Pay Invoices’ function, {0} will create separate payments for each user that has created the invoices being paid.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		public BooleanRegistryItem PayInvoicesDefaultPostPaymentsAsPaymentApprovals
		{
			get
			{
				return GetItem("PayInvoicesDefaultPostPaymentsAsPaymentApprovals",
					() => new BooleanRegistryItem(
							"PayInvoicesDefaultPostPaymentsAsPaymentApprovals",
							Categories.Accounting_PayableDefaults_DefaultSettings_PayInvoices,
							ResString.GetMultilingualString("6857BF5E-D157-49CE-9561-1BC51AB7E9DA", "Default ‘Post Payments as Payment Approvals’"),
							ResString.GetMultilingualString("2ca5d2f2-63ab-425b-901e-13635c575540", @"This registry is used by the ‘Pay Invoices’ action available in the Payables Transactions module.
By default, this registry is not used and is set to ‘No’. 
When overridden and set to ‘Yes’ payments created using the Pay Invoices action will be directed through the Payment Processing module’s Approval and Posting processes."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							false));
			}
		}

		#endregion

		#endregion

		#endregion

		#region Receivable Defaults

		#region Form Configurations

		#region Invoice

		#region ARInvoiceMenuItem

		public string GetARInvoiceMenuItemName(BusinessObjectFactory factory)
		{
			Guid pK = ARInvoiceMenuItem.Value;
			StmMenuItem item = factory.Load<StmMenuItem>(pK);
			return item != null ? (string)item.SU_MenuName : string.Empty;
		}

		public GuidRegistryItem ARInvoiceMenuItem
		{
			get
			{
				return GetItem("ARInvoiceMenuItem",
				delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("ARInvoiceMenuItem",
					Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
					ResString.GetMultilingualString("3183aedb-6255-4fe9-86e9-6acdcac50bfd", "AR Invoice Menu Item Name"),
					ResString.GetMultilingualString("c450527f-689b-4c38-a12e-08c4bd3b429b", "This registry item allows you to nominate a menu item that should be used when previewing and delivering AR Invoices, Credit Notes and Adjustment Notes throughout {0}", BrandingFactory.Instance.ProductName),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					IsARInvoiceMenuOverrideAllowed.Value ? RegistryOptions.Default : RegistryOptions.IsOnlyForDevelopers,
					DefaultMenuItemGuid);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.ARInvoiceMenuItem);
					return result;
				});
			}
		}

		#endregion

		#region IsARInvoiceMenuOverrideAllowed
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem IsARInvoiceMenuOverrideAllowed
		{
			get
			{
				return GetItem("IsARInvoiceMenuOverrideAllowed",
					delegate
					{
						return new BooleanRegistryItem("IsARInvoiceMenuOverrideAllowed",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
							(NoResString)"Allow AR Invoice Menu Override (CargoWiseOne Support Only)",
							(NoResString)"Setting this registry to 'Yes' will allow users to specify their own menu name to use when printing invoices through CargoWiseOne",
							RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
					});
			}
		}

		#endregion
		#endregion

		#region Local Invoice

		#region ARLocalInvoiceMenuItem

		public string GetARLocalInvoiceMenuItemName(BusinessObjectFactory factory)
		{
			Guid pK = ARLocalInvoiceMenuItem.Value;
			StmMenuItem item = factory.Load<StmMenuItem>(pK);
			return item != null ? (string)item.SU_MenuName : string.Empty;
		}

		public GuidRegistryItem ARLocalInvoiceMenuItem
		{
			get
			{
				return GetItem("ARLocalInvoiceMenuItem",
				delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("ARLocalInvoiceMenuItem",
					Categories.Accounting_ReceivableDefaults_FormConfigurations_LocalInvoice,
					ResString.GetMultilingualString("cbb5f7ed-345e-4366-83bd-6835f8acbc0d", "AR Local Invoice Menu Item Name"),
					ResString.GetMultilingualString("ba5e9fdc-77a4-4ac9-ba48-65d078bce2cf", "This registry item allows you to nominate a menu item that should be used when previewing and delivering AR Local Invoices throughout {0}", BrandingFactory.Instance.ProductName),
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					IsARLocalInvoiceMenuOverrideAllowed.Value ? RegistryOptions.Default : RegistryOptions.IsOnlyForDevelopers,
					DefaultLocalInvoiceMenuItemGuid);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.ARInvoiceMenuItem);
					return result;
				});
			}
		}

		#endregion

		#region IsARLocalInvoiceMenuOverrideAllowed
		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem IsARLocalInvoiceMenuOverrideAllowed
		{
			get
			{
				return GetItem("IsARLocalInvoiceMenuOverrideAllowed",
					delegate
					{
						return new BooleanRegistryItem("IsARLocalInvoiceMenuOverrideAllowed",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_LocalInvoice,
							(NoResString)"Allow AR Local Invoice Menu Override (CargoWiseOne Support Only)",
							(NoResString)"Setting this registry to 'Yes' will allow users to specify their own menu name to use when printing local invoices through CargoWiseOne",
							RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
					});
			}
		}

		#endregion
		#endregion

		#endregion

		#endregion

		#region Credit Note

		#region ShowARCreditNoteAmountsWithOppositeSign

		public BooleanRegistryItem ShowARCreditNoteAmountsWithOppositeSign
		{
			get
			{
				return GetItem("ShowARCreditNoteAmountsWithOppositeSign",
					delegate
					{
						return new BooleanRegistryItem("ShowARCreditNoteAmountsWithOppositeSign",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_CreditNote,
							ResString.GetMultilingualString("9a419ad1-5618-416b-a375-b1f15995c491", "Show AR Credit Note Amounts With Opposite Sign"),
							ResString.GetMultilingualString("1882d92e-0ca2-4890-971a-9d51f275e654", "By default , when printing an AR Credit Note, all values are printed as a “positive” figure.  The document type, it’s title and content all clearly identify the transaction as a “Credit”.  When overridden and set to “Yes” the AR Credit Note will print with amounts negated.  Note:  This registry only applies to the way amounts are printed in the AR Credit Note DocBuilder document."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, false);
					});
			}
		}

		#endregion

		#region DisplayTaxRateInAllLinesOfTaxSummary
		public BooleanRegistryItem DisplayTaxRateInAllLinesOfTaxSummary
		{
			get
			{
				return GetItem("DisplayTaxRateInAllLinesOfTaxSummary",
					delegate
					{
						return new BooleanRegistryItem("DisplayTaxRateInAllLinesOfTaxSummary",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
							ResString.GetMultilingualString("9661c513-9680-4f42-8b02-14287aace997", "Display Tax Rate in All Lines of Tax Summary"),
							ResString.GetMultilingualString("41a29349-b013-448e-bba8-f1aca6178366", @"This registry is relevant to Companies that use the “Tax Total Summary by Rate and Tax Message” doc strip.

By default this registry is not enabled.
This means that the “Tax Total Summary by Rate and Tax Message” doc strip prints a tax rate only on lines that contain a Tax Amount.

When set to Yes, the tax rate relevant on the tax date of the charge line will print on every line of the “Tax Total Summary by Rate and Tax” doc strip.

Charges with no tax amount will print the value “0%” in the rate column. "),
							RegistryStorageFlags.Company, RegistryOptions.Default, false);
					});
			}
		}
		#endregion

		#endregion

		#endregion

		#region Default Settings Category

		public CodePairRegistryItem DefaultReceiptType
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.ReceiptMethod));

				return GetItem("DefaultReceiptType", delegate
				{
					return new CodePairRegistryItem(
						"DefaultReceiptType",
						Categories.Accounting_ReceivableDefaults,
						ResString.GetMultilingualString("c456e7b8-3073-4c37-a791-768d9d803143", "Default Receipt Type"),
						ResString.GetMultilingualString("ce6057f9-3509-4092-9e26-befbafb39770", @"This registry item provides you with the ability to nominate the default Receipt Type for any new receipt created in the Receivable and Payables. 

By default the receipt Type is CHQ – cheque "),
						listProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ReceiptTypes.Cheque);
				});
			}
		}

		public GuidRegistryItem CreditNoteApprovalsNotifyGroup
		{
			get
			{
				return GetItem("CreditNoteApprovalsNotifyGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CreditNoteApprovalsNotifyGroup",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_CreditNoteInvoiceReversalAuthorisationSetting,
						ResString.GetMultilingualString("dd189d0d-cebc-4d16-a23d-b8df79efe8b9", "Credit Note Approvals Notify Group"),
						ResString.GetMultilingualString("97c42fc1-fbd1-40c5-9084-a729a0749c74", @"This registry allows you to nominate a user group who will receive notifications about outstanding credit note approval requests.

By default, a daily email will summarize the credit note approval requests that are still in REQ - Requested status. To change the frequency of the email, please navigate to Maintain > System > Service Tasks and locate the following task: 
UCN - Credit Note Approvals Notification Email. You can then edit the Recurrence Pattern for the scheduled email.

Credit Note Approval requests can be approved or rejected in the Credit Note Approval module."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#region ARInvoiceNumberLengthConfiguration

		public ARInvoiceNumberLengthConfigurationRegistryItem ARInvoiceNumberLengthConfiguration
		{
			get
			{
				return GetItem("ARInvoiceNumberLengthConfiguration",
					() => new ARInvoiceNumberLengthConfigurationRegistryItem(
						"ARInvoiceNumberLengthConfiguration",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("e50c09ed-6545-4f0c-a332-ceb8aab47d01", "AR Invoice Number Length Configuration"),
						ResString.GetMultilingualString("c6c7750f-abf6-45d0-b098-1503fb37c07a", @"This registry can be used to configure the length of transaction numbers for the AR Invoice, AR Credit Note and AR Adjustment Note. The minimum length is 5 digits. The maximum length is 8 digits.
Note: This registry will be ignored when the Accounting > Number Sequence Customization registry has also been overridden and enabled. Any overrides configured in the Number Sequence Customization registry will be used instead."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						8,
						5,
						8
					)
				);
			}
		}

		#endregion

		#region Invoice Date Defaulting Behaviour

		public CodePairRegistryItem InvAndPstDateDefaultingBehaviour
		{
			get
			{
				return GetItem("InvAndPstDateDefaultingBehaviour", delegate
				{
					return new CodePairRegistryItem(
						"InvAndPstDateDefaultingBehaviour",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("501f4369-07b1-4003-a27f-872b35a356f0", "Invoice and Post Dates Defaulting Behavior"),
						ResString.GetMultilingualString("92cf1967-e37d-46f7-9fe4-c36d91a8a401", @"This registry defines the Invoice and Post Dates defaulting behaviors of Receivables Invoice (INV), Credit Note (CRD) and Adjustment Note (ADJ) transactions.

DEF - Default Behavior.
Both Invoice and Post Dates observe the respective standard registries’ configurations, such as Back Date Invoices Configuration, Allow Back Posting Sub Ledger Transactions, etc. 
In various scenarios, users with appropriate rights can modify either or both the Invoice and Post Dates before posting. These dates recorded against transactions posted in a single day can vary.

MTH - Month End Suspension Behavior.
Single Invoice Date across the Company, incrementing each day, with Automatic Calendar Month End Suspension.
A single Invoice Date will be used for all AR INV, CRD and ADJ transactions (both Job and non-Job related) created and posted in a Login Company. The Post Date will always be the same as the Invoice Date."),
						new CodeDescriptionPairListProvider(() => new InvAndPstDateDefaultingRuleTypes()),
						RegistryStorageFlags.Company,
						Enterprise.Accounting.Business.AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
				});
			}
		}

		public DateTimeRegistryItem InvAndPstDateDefaultingBehaviourInstatedDate
		{
			get
			{
				return GetItem("InvAndPstDateDefaultingBehaviourInstatedDate", delegate
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
					"InvAndPstDateDefaultingBehaviourInstatedDate",
					null, null, null,
					RegistryStorageFlags.Company,
					RegistryOptions.IsHidden, DateTime.MinValue);
					return result;
				});
			}
		}

		#region Invoice Rollup / Group Description

		public InvoiceRollupAndGroupDescriptionRegistryItem InvoiceRollupAndGroupDescriptionRegistryItem
		{
			get
			{
				return GetItem("InvoiceRollupAndGroupDescriptionRegistryItem", delegate
				{
					InvoiceRollupAndGroupDescriptionCollection defaultValues = LookupInvoiceRollupAndGroupDescriptionDefaultValues();
					return new InvoiceRollupAndGroupDescriptionRegistryItem(
						"InvoiceRollupAndGroupDescriptionRegistryItem",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("3b378c45-6370-4087-82d1-aa9721b105ac", "Invoice Roll-up / Group Description"),
						ResString.GetMultilingualString("29920ee6-2488-4f57-9d3d-f18fbe246918", "This registry setting allow you to configure the default Invoice Roll-up/Group Description."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValues);
				});
			}
		}

		public InvoiceRollupAndGroupDescriptionCollection LookupInvoiceRollupAndGroupDescriptionDefaultValues()
		{
			InvoiceRollupAndGroupDescriptionCollection defaultValues = new InvoiceRollupAndGroupDescriptionCollection();

			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.All, DocRollUpConstants.RollupAndSubTotalGroups.AllChargesExceptCustomsDutyAndTax, DocRollUpConstants.RollupAndSubTotalDescriptions.AllChargesExceptCustomsDutyAndTax);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.AEC, DocRollUpConstants.RollupAndSubTotalGroups.OriginFreightInsuranceAndDestination, DocRollUpConstants.RollupAndSubTotalDescriptions.OriginFreightInsuranceAndDestination);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OandF, DocRollUpConstants.RollupAndSubTotalGroups.OriginFreightAndInsurance, DocRollUpConstants.RollupAndSubTotalDescriptions.OriginFreightAndInsurance);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.ORF, DocRollUpConstants.RollupAndSubTotalGroups.OriginAndFreight, DocRollUpConstants.RollupAndSubTotalDescriptions.OriginAndFreight);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.FRT, DocRollUpConstants.RollupAndSubTotalGroups.Freight, DocRollUpConstants.RollupAndSubTotalDescriptions.Freight);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.FandD, DocRollUpConstants.RollupAndSubTotalGroups.FreightInsuranceAndDestination, DocRollUpConstants.RollupAndSubTotalDescriptions.FreightInsuranceAndDestination);

			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFD, DocRollUpConstants.RollupAndSubTotalGroups.Origin, DocRollUpConstants.RollupAndSubTotalDescriptions.Origin);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFD, DocRollUpConstants.RollupAndSubTotalGroups.FreightAndInsurance, DocRollUpConstants.RollupAndSubTotalDescriptions.FreightAndInsurance);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFD, DocRollUpConstants.RollupAndSubTotalGroups.Destination, DocRollUpConstants.RollupAndSubTotalDescriptions.Destination);

			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFO, DocRollUpConstants.RollupAndSubTotalGroups.Origin, DocRollUpConstants.RollupAndSubTotalDescriptions.Origin);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFO, DocRollUpConstants.RollupAndSubTotalGroups.FreightAndInsurance, DocRollUpConstants.RollupAndSubTotalDescriptions.FreightAndInsurance);

			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalGroups.Origin, DocRollUpConstants.RollupAndSubTotalDescriptions.Origin);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalGroups.Freight, DocRollUpConstants.RollupAndSubTotalDescriptions.Freight);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalGroups.Insurance, DocRollUpConstants.RollupAndSubTotalDescriptions.Insurance);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFF, DocRollUpConstants.RollupAndSubTotalGroups.Destination, DocRollUpConstants.RollupAndSubTotalDescriptions.Destination);

			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFI, DocRollUpConstants.RollupAndSubTotalGroups.Origin, DocRollUpConstants.RollupAndSubTotalDescriptions.Origin);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFI, DocRollUpConstants.RollupAndSubTotalGroups.Freight, DocRollUpConstants.RollupAndSubTotalDescriptions.Freight);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.OFI, DocRollUpConstants.RollupAndSubTotalGroups.Insurance, DocRollUpConstants.RollupAndSubTotalDescriptions.Insurance);

			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Brokerage, ChargeCodeGroupList.Descriptions.Brokerage);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.BrokerageOnly, ChargeCodeGroupList.Descriptions.BrokerageOnly);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.CFSLoadList, ChargeCodeGroupList.Descriptions.CFSLoadList);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.CFSShipment, ChargeCodeGroupList.Descriptions.CFSShipment);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.ContainerStorage, ChargeCodeGroupList.Descriptions.ContainerStorage);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.CustomsDuty, ChargeCodeGroupList.Descriptions.CustomsDuty);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Destination, DocRollUpConstants.RollupAndSubTotalDescriptions.Destination);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Freight, DocRollUpConstants.RollupAndSubTotalDescriptions.Freight);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Insurance, DocRollUpConstants.RollupAndSubTotalDescriptions.Insurance);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Loading, ChargeCodeGroupList.Descriptions.Loading);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.MNRWorkOrderHeader, ChargeCodeGroupList.Descriptions.MNRWorkOrderHeader);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.NonJobRelated, ChargeCodeGroupList.Descriptions.NonJobRelated);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Origin, DocRollUpConstants.RollupAndSubTotalDescriptions.Origin);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.OriginBrokerage, ChargeCodeGroupList.Descriptions.OriginBrokerage);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.OriginBrokerageOnly, ChargeCodeGroupList.Descriptions.OriginBrokerageOnly);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.ShippingDisbursements, ChargeCodeGroupList.Descriptions.ShippingDisbursements);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Transport, ChargeCodeGroupList.Descriptions.Transport);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.TransportBooking, ChargeCodeGroupList.Descriptions.TransportBooking);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.Unloading, ChargeCodeGroupList.Descriptions.Unloading);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.WHSAdHocServiceJob, ChargeCodeGroupList.Descriptions.WHSAdHocServiceJob);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.WHSInwards, ChargeCodeGroupList.Descriptions.WHSInwards);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.WHSOutwards, ChargeCodeGroupList.Descriptions.WHSOutwards);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.WHSStorage, ChargeCodeGroupList.Descriptions.WHSStorage);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.YardGateIn, ChargeCodeGroupList.Descriptions.YardGateIn);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.YardGateOut, ChargeCodeGroupList.Descriptions.YardGateOut);
			defaultValues.AddDefaultValue(OrgConstants.InvoiceLineGroupings.Code.CCG, ChargeCodeGroupList.Codes.YardStorage, ChargeCodeGroupList.Descriptions.YardStorage);

			return defaultValues;
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region General Ledger Defaults

		#region Control Accounts

		public CFXAccountRegistryItem CFXAccount
		{
			get
			{
				return GetItem("GL_CFX_ACCOUNT",
					() => new CFXAccountRegistryItem(
						"GL_CFX_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						ResString.GetMultilingualString("aecf1509-7c90-461e-971d-b35d02b609ab", "CFX Account"),
						ResString.GetMultilingualString("60c102ac-d087-46c4-94fe-6e4062fea049", "This registry identifies the General Ledger Accounts that will be used when Job Invoicing CFX Journals are posted.\r\nThe registry can be defined at the System, Company and Transaction Line Department level."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty)
				);
			}
		}

		public GuidRegistryItem JobRevenueJournalControlAccount
		{
			get
			{
				return GetItem("GL_JOB_REVENUE_JOURNAL_CONTROL_ACCOUNT", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_JOB_REVENUE_JOURNAL_CONTROL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						ResString.GetMultilingualString("CB87D1EB-974D-428F-937C-B0B4C82CCFB4", "Job Revenue Journal Control Account"),
						ResString.GetMultilingualString("1362CB74-985B-42EA-A40E-BB9EA906AC35", "This is the job revenue journal control account.\r\nYou cannot post job revenue journal transactions without populating this registry item."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);
					result.DataType = new JobRevenueJournalControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem ARControlAccount
		{
			get
			{
				return GetItem("GL_AR_CONTROL_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("aac62ada-e0d8-4fb3-a29a-ef7a389f16dc", "AR Control Account");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_AR_CONTROL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						captionAndHint,
						captionAndHint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);
					result.DataType = new ARControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem APControlAccount
		{
			get
			{
				return GetItem("GL_AP_CONTROL_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("f5e44bfd-dfed-42f9-be55-ceb46e433b1d", "AP Control Account");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_AP_CONTROL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						captionAndHint,
						captionAndHint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);
					result.DataType = new APControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem ARSuspenseControlAccount
		{
			get
			{
				return GetItem("GL_AR_SUSPENSE_CONTROL_ACCOUNT", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("01cf5eb9-b9ef-4af8-9acb-72213c2e3731", @"The account defined here holds job related REVENUE charges in 'suspense'.

At any point in time the balance reported against this account identifies posted revenue that have not yet been recognized within the financial accounting system's 'operating profit'.

Revenue is held in suspense as a way of controlling the time at which revenue and any associated profit is recognized in the financial accounting system.

Revenue Suspense items are charges that have been posted as AR transactions and recognized in the Receivables Subsidiary Ledger HOWEVER they have not been recognized in the General Ledger Profit and Loss because the relevant Job Revenue Recognition date has not been reached.

The account is used to

a) hold revenue in suspense until a job's Revenue Recognition Date has passed

b) as a way of matching new revenue charges back against past job Revenue Recognition dates");

					GuidRegistryItem result = new GuidRegistryItem(
						"GL_AR_SUSPENSE_CONTROL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						ResString.GetMultilingualString("f3e58243-896a-4631-8d72-82cf045bf95c", "Revenue Suspense Control Account"),
						hint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);
					result.DataType = new ARSuspenseControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem APSuspenseControlAccount
		{
			get
			{
				return GetItem("GL_AP_SUSPENSE_CONTROL_ACCOUNT", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("457552cc-dc61-44cd-ad62-040e9a9e4037", @"The account defined here holds job related COST charges in 'suspense'.

At any point in time the balance reported against this account identifies posted cost that have not yet been recognized within the financial accounting system's 'operating profit'.

Costs are held in suspense as a way of controlling the time at which costs and profits are recognized in the financial accounting system. 

Cost Suspense items are charges that have been posted as AP transactions and recognized in the Payables Subsidiary Ledger HOWEVER they have not been recognized in the General Ledger Profit and Loss because the relevant Job Revenue Recognition date has not been reached.

The account is used to hold costs in suspense until a job's Revenue Recognition Date has passed, and to match new costs against past job Revenue Recognition dates.");

					GuidRegistryItem result = new GuidRegistryItem(
						"GL_AP_SUSPENSE_CONTROL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						ResString.GetMultilingualString("93c6d92d-0245-4275-92b3-450d99c93db4", "Cost Suspense Control Account"),
						hint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);
					result.DataType = new APSuspenseControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem AccruedRevenueControlAccount
		{
			get
			{
				return GetItem("GL_ACCRUED_REVENUE_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("6416e40f-8350-4e27-9357-eecc1f5c1282", "Accrued Revenue Control Account");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_ACCRUED_REVENUE_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						captionAndHint,
						captionAndHint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);
					result.DataType = new AccruedRevenueControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem AccruedCostControlAccount
		{
			get
			{
				return GetItem("GL_ACCRUED_COST_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("79e82130-ac02-4130-9651-876a4f7763ce", "Accrued Cost Control Account");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_ACCRUED_COST_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						captionAndHint,
						captionAndHint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);
					result.DataType = new AccruedCostControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem GSTInputControlAccount
		{
			get
			{
				return GetItem("GL_GST_INPUT_ACCOUNT", delegate
				{
					var gstCompany = new BusinessObjectFactory().LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsGSTRegistered, true));
					MultilingualString hint = ResString.GetMultilingualString("F6641EE2-E3D1-4A3F-87D5-F33A12DEDF03", @"This registry identifies one of two General Ledger Control Accounts used for all VAT/GST Input Tax amounts.
All VAT/GST tax amounts posted on Payables Invoice, Credit Note and Adjustment Note transactions post through the relevant Input Tax Control Account/s.
All VAT/GST Tax reported on an Accruals Basis posts to the Reportable Input Tax Account on the transaction Post Date.
All VAT/GST Tax amounts posted on Cash Book Direct Payments post to the Reportable Input Tax account on the transaction Post Date.
All VAT/GST Tax reported on a Cash Basis posts to the Pending Tax Input Control Account on the transaction Post Date, and subsequently posts through to the Reportable Input Tax Account on the relevant match Date/s (Paid date/s).
Note: The Pending Tax Input Control Account is only relevant to Login Companies enabled for VAT/GST Cash Basis Tax reporting features.");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_GST_INPUT_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						ResString.GetMultilingualString("C3714B13-049D-411C-A660-170F8E8159C4", "Reportable Tax Input Control Account"),
						hint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						gstCompany == null ? RegistryOptions.Default : (RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue),
						Guid.Empty);
					result.DataType = new GSTInputControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem GSTOutputControlAccount
		{
			get
			{
				return GetItem("GL_GST_OUTPUT_ACCOUNT", delegate
				{
					var gstCompany = new BusinessObjectFactory().LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsGSTRegistered, true));
					MultilingualString hint = ResString.GetMultilingualString("EB5054F3-F245-4E36-9A02-B06478EA47E4", @"This registry identifies one of two General Ledger Control Accounts used for all VAT/GST Output Tax amounts.
All VAT/GST tax amounts posted on Receivables Invoice, Credit Note and Adjustment Note transactions post through the relevant Output Tax Control Account/s.
All VAT/GST Tax reported on an Accruals Basis posts to the Reportable Output Tax Account on the transaction Post Date.
All VAT/GST Tax amounts posted on Cash Book Direct Receipt post to the Reportable Output Tax account on the transaction Post Date.
All VAT/GST Tax reported on a Cash Basis posts to the Pending Tax Output Control Account on the transaction Post Date, and subsequently posts through to the Reportable Output Tax Account on the relevant match Date/s (Paid date/s).
Note: The Pending Tax Output Control Account is only relevant to Login Companies enabled for VAT/GST Cash Basis Tax reporting features.");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_GST_OUTPUT_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						ResString.GetMultilingualString("38E30923-FDC3-45A7-B0EB-3DC4F36BCC13", "Reportable Tax Output Control Account"),
						hint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						gstCompany == null ? RegistryOptions.Default : (RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue),
						Guid.Empty);
					result.DataType = new GSTOutputControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem PendingGSTInputControlAccount
		{
			get
			{
				return GetItem("GL_PENDING_GST_INPUT_ACCOUNT", delegate
				{
					var gstCompany = new BusinessObjectFactory().LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsGSTCashBasis, true));
					MultilingualString hint = ResString.GetMultilingualString("48D3FD69-76EC-4E93-9932-51D17B2A5C32", @"This registry identifies one of two General Ledger Control Accounts used for all VAT/GST Input Tax amounts.
All VAT/GST tax amounts posted on Payables Invoice, Credit Note and Adjustment Note transactions post through the relevant Input Tax Control Account/s.
All VAT/GST Tax reported on an Accruals Basis posts to the Reportable Input Tax Account on the transaction Post Date.
All VAT/GST Tax amounts posted on Cash Book Direct Payments post to the Reportable Input Tax account on the transaction Post Date.
All VAT/GST Tax reported on a Cash Basis posts to the Pending Tax Input Control Account on the transaction Post Date, and subsequently posts through to the Reportable Input Tax Account on the relevant match Date/s (Paid date/s).
Note: The Pending Tax Input Control Account is only relevant to Login Companies enabled for VAT/GST Cash Basis Tax reporting features.");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_PENDING_GST_INPUT_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						ResString.GetMultilingualString("79DE7908-6FBB-4E4B-B076-CE0504CAE61B", "Pending Tax Input Control Account"),
						hint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						gstCompany == null ? RegistryOptions.Default : (RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue),
						Guid.Empty);
					result.DataType = new PendingGSTInputControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem PendingGSTOutputControlAccount
		{
			get
			{
				return GetItem("GL_PENDING_GST_OUTPUT_ACCOUNT", delegate
				{
					var gstCompany = new BusinessObjectFactory().LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsGSTCashBasis, true));
					MultilingualString hint = ResString.GetMultilingualString("12CC2C5F-DF47-4563-9713-F5DB4BF89BEF", @"This registry identifies one of two General Ledger Control Accounts used for all VAT/GST Output Tax amounts.
All VAT/GST tax amounts posted on Receivables Invoice, Credit Note and Adjustment Note transactions post through the relevant Output Tax Control Account/s.
All VAT/GST Tax reported on an Accruals Basis posts to the Reportable Output Tax Account on the transaction Post Date.
All VAT/GST Tax amounts posted on Cash Book Direct Receipt post to the Reportable Output Tax account on the transaction Post Date.
All VAT/GST Tax reported on a Cash Basis posts to the Pending Tax Output Control Account on the transaction Post Date, and subsequently posts through to the Reportable Output Tax Account on the relevant match Date/s (Paid date/s).
Note: The Pending Tax Output Control Account is only relevant to Login Companies enabled for VAT/GST Cash Basis Tax reporting features.");
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_PENDING_GST_OUTPUT_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						ResString.GetMultilingualString("CB0F9C3A-DF26-4DE3-9C1D-2F790BFE756F", "Pending Tax Output Control Account"),
						hint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						gstCompany == null ? RegistryOptions.Default : (RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue),
						Guid.Empty);
					result.DataType = new PendingGSTOutputControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem WHTInputControlAccount
		{
			get
			{
				return GetItem("GL_WHT_INPUT_ACCOUNT", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_WHT_INPUT_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						ResString.GetMultilingualString("83AC1D3A-2D64-4CEA-9794-AA31D7AD748A", "WHT Input Control Account"),
						ResString.GetMultilingualString("E0E4A91D-0659-4D1A-86A6-1EED51E6F9FF",
@"THIS REGISTRY IS OBSOLETE.
Please go to the 'Link Account > Tax Transaction' registry tree to define default general ledger accounts used when adding Tax Configurations against Login Company and Branch Records in Tax Configuration supported countries.
The WHT Input Control Account registry is not used by Tax Configuration and Tax Transaction features.

Please note: The WHT Input Control Account registry is only relevant in a small number of databases where Legacy WHT features were previously deployed."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);
					result.DataType = new WHTInputControlAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem WHTOutputControlAccount
		{
			get
			{
				return GetItem("GL_WHT_OUTPUT_ACCOUNT", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"GL_WHT_OUTPUT_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_ControlAccount,
						ResString.GetMultilingualString("485097E6-83B5-48A6-B536-F5159F456623", "WHT Output Control Account"),
						ResString.GetMultilingualString("AA8A7768-6F1D-4238-90F9-8DC0B98007F2",
@"THIS REGISTRY IS OBSOLETE.
Please go to the 'Link Account > Tax Transaction' registry tree to define default general ledger accounts used when adding Tax Configurations against Login Company and Branch Records in Tax Configuration supported countries.
The WHT Output Control Account registry is not used by Tax Configuration and Tax Transaction features.

Please note: The WHT Output Control Account registry is only relevant in a small number of databases where Legacy WHT features were previously deployed."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);
					result.DataType = new WHTOutputControlAccountDataType();
					return result;
				});
			}
		}

		#region Functions

		public void ClearAllControlAccountRegistryItems()
		{
			JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			RealizedExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			RealizedExchangeLossAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			ARDiscountAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			APDiscountAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			OverpaymentsAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccruedRevenueControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccruedCostControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			GSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			GSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			WHTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			WHTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ForeignCurrencyGLBalanceAdjustmentAccount.DefaultValue);
			FinanceChargesAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			CurrencyAdjustmentExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ForeignCurrencyGLBalanceAdjustmentAccount.DefaultValue);
			CurrencyAdjustmentExchangeLossAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ForeignCurrencyGLBalanceAdjustmentAccount.DefaultValue);
		}

		public bool AreControlAccountRegistryItemsSet()
		{
			return
			JobRevenueJournalControlAccount.Value != Guid.Empty ||
			ARControlAccount.Value != Guid.Empty ||
			APControlAccount.Value != Guid.Empty ||
			ARSuspenseControlAccount.Value != Guid.Empty ||
			APSuspenseControlAccount.Value != Guid.Empty ||
			RealizedExchangeGainAccount.Value != Guid.Empty ||
			RealizedExchangeLossAccount.Value != Guid.Empty ||
			ARDiscountAccount.Value != Guid.Empty ||
			APDiscountAccount.Value != Guid.Empty ||
			OverpaymentsAccount.Value != Guid.Empty ||
			AccruedRevenueControlAccount.Value != Guid.Empty ||
			AccruedCostControlAccount.Value != Guid.Empty ||
			GSTInputControlAccount.Value != Guid.Empty ||
			GSTOutputControlAccount.Value != Guid.Empty ||
			PendingGSTInputControlAccount.Value != Guid.Empty ||
			PendingGSTOutputControlAccount.Value != Guid.Empty ||
			WHTInputControlAccount.Value != Guid.Empty ||
			WHTOutputControlAccount.Value != Guid.Empty ||
			ForeignCurrencyGLBalanceAdjustmentAccount.Value != ForeignCurrencyGLBalanceAdjustmentAccount.DefaultValue ||
			CurrencyAdjustmentExchangeGainAccount.Value != ForeignCurrencyGLBalanceAdjustmentAccount.DefaultValue ||
			CurrencyAdjustmentExchangeLossAccount.Value != ForeignCurrencyGLBalanceAdjustmentAccount.DefaultValue;
		}

		#endregion

		#endregion

		#region Link Accounts

		public GuidRegistryItem RealizedExchangeGainAccount
		{
			get
			{
				return GetItem("GL_EXCHANGE_GAIN_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("b9e3d176-05ea-4fe5-92ac-2cf5bb928d9e", "Realized Exchange Gain Account");
					var result = new GuidRegistryItem(
						"GL_EXCHANGE_GAIN_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						ResString.GetMultilingualString("691c2943-0868-4d93-ab04-7d146f757ead", "This link account is required for the posting of exchange gain when matching outstanding receivables and payables outstanding transactions."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSH),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);

					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem RealizedExchangeLossAccount
		{
			get
			{
				return GetItem("GL_EXCHANGE_LOSS_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("32936F13-F9E6-4B58-870F-AD40C1ED3806", "Realized Exchange Loss Account");
					var result = new GuidRegistryItem(
						"GL_EXCHANGE_LOSS_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						ResString.GetMultilingualString("599015F1-39EE-4C88-9703-3D5D40C5B5E5", "This link account is required for the posting of exchange loss when matching outstanding receivables and payables outstanding transactions."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSH),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);

					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem ForeignCurrencyGLBalanceAdjustmentAccount
		{
			get
			{
				return GetItem("GL_FOREIGN_CURRENCY_BALANCE_ADJUSTMENT_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("DAAD7DE4-D7F7-4127-BA6B-89454267F823", "Foreign Currency GL Balance Adjustment Account");
					var result = new GuidRegistryItem(
						"GL_FOREIGN_CURRENCY_BALANCE_ADJUSTMENT_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						ResString.GetMultilingualString("E6AB0358-B3D0-4F31-9D4E-5FE2649F3684", @"This link account will be defaulted during the creation of Foreign Currency GL Balance Adjustment via the General Ledger > Journals > New Foreign Currency Balances Adjustment menu.
You can override the GL Account as required."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSH),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						ForeignCurrencyGLBalanceAdjustmentAccountAccountDefaultValue);

					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		Guid ForeignCurrencyGLBalanceAdjustmentAccountAccountDefaultValue
		{
			get
			{
				var gl = RegistryFactory.Instance.LoadFromNaturalKey<AccGLHeader>(
					AccGLHeaderSchema.AG_AccountNum, AccountingConstants.RegistryDefaultValues.ForeignCurrencyGLBalanceAdjustmentAccount);
				return gl == null ? Guid.Empty : gl.PK.ToGuid();
			}
		}

		public GuidRegistryItem CurrencyAdjustmentExchangeGainAccount
		{
			get
			{
				return GetItem("GL_CURRENCY_ADJUSTMENT_EXCHANGE_GAIN_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("17B4CFA6-3BA7-4219-B26E-B5A0C833E1CA", "Currency Adjustment Exchange Gain Account");
					var result = new GuidRegistryItem(
						"GL_CURRENCY_ADJUSTMENT_EXCHANGE_GAIN_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						ResString.GetMultilingualString("6C4BCFF5-126B-4410-AA9B-0E145D135325", @"This link account is required for the posting of exchange gain related to the bank, receivables and payables foreign balances currency adjustments.

For bank currency adjustment, this can be done via the Cash Book > Cash Book Transaction > New Bank Currency Adjustment.
For receivables and payables foreign currency balances, this will be done by the automated process that can be enabled via the 'Auto Create A/R and A/P Outstanding Balance Currency Adjustments' system registry."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSH),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						ForeignCurrencyGLBalanceAdjustmentAccountAccountDefaultValue);

					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem CurrencyAdjustmentExchangeLossAccount
		{
			get
			{
				return GetItem("GL_CURRENCY_ADJUSTMENT_EXCHANGE_LOSS_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("9E7225E3-2CD0-4120-BA29-79AAF4F7122A", "Currency Adjustment Exchange Loss Account");
					var result = new GuidRegistryItem(
						"GL_CURRENCY_ADJUSTMENT_EXCHANGE_LOSS_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						ResString.GetMultilingualString("6DA6DEE9-0687-4156-8D3A-D926F2589F99", @"This link account is required for the posting of exchange loss related to the bank, receivables and payables foreign balances currency adjustments.

For bank currency adjustment, this can be done via the Cash Book > Cash Book Transaction > New Bank Currency Adjustment.
For receivables and payables foreign currency balances, this will be done by the automated process that can be enabled via the 'Auto Create A/R and A/P Outstanding Balance Currency Adjustments' system registry."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSH),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory,
						ForeignCurrencyGLBalanceAdjustmentAccountAccountDefaultValue);

					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem ARDiscountAccount
		{
			get
			{
				return GetItem("GL_AR_DISCOUNT_ACCOUNT", delegate
				{
					var result = new GuidRegistryItem(
						"GL_AR_DISCOUNT_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("c7610293-113c-4973-ba12-087207db96d2", "AR Discount Account"),
						ResString.GetMultilingualString("4a428063-0114-43e0-8cae-55e1dfab99cb", "When DSC transaction is created in Receivables Ledger, this GL Account will be used."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSH),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);

					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem APDiscountAccount
		{
			get
			{
				return GetItem("GL_AP_DISCOUNT_ACCOUNT", delegate
				{
					var result = new GuidRegistryItem(
						"GL_AP_DISCOUNT_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("ce4a797d-0429-476b-acec-2f72ba9890f5", "AP Discount Account"),
						ResString.GetMultilingualString("f1b4360a-0b70-4749-ba7f-56008cfe8918", "When DSC transaction is created in Payables Ledger, this GL Account will be used."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSH),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);

					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem OverpaymentsAccount
		{
			get
			{
				return GetItem("GL_OVERPAYMENTS_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("64c29137-ee44-4b31-8035-99e9c0ba30a4", "Over-payments Account");
					var result = new GuidRegistryItem(
						"GL_OVERPAYMENTS_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						captionAndHint,
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSH),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue,
						Guid.Empty);

					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem AdvancedTurnoverTaxReturnAccount
		{
			get
			{
				return GetItem("GL_ADVANCED_TURNOVER_TAX_RETURN_ACCOUNT", delegate
				{
					var result = new GuidRegistryItem(
						"GL_ADVANCED_TURNOVER_TAX_RETURN_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("7E47E8F9-5670-45B5-BD7D-985E88E2EC8D", "Advanced Turnover Tax Return Account"),
						ResString.GetMultilingualString("98DFDAE7-0C41-4816-A976-88280DF959C9", @"This link account indicates the account number for advance turnover tax returns. Advance turnover tax returns are reported (monthly) to fiscal authorities.
Book your VAT payment as Cashbook / Direct Payment on this account."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						Guid.Empty);

					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem SpecialVATPrepaymentAccount
		{
			get
			{
				return GetItem("GL_SPECIAL_VAT_PREPAYMENT_ACCOUNT", delegate
				{
					var result = new GuidRegistryItem(
						"GL_SPECIAL_VAT_PREPAYMENT_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("D15328AE-801B-4F0F-8A8E-6C60179AE332", "Special VAT Pre-payment Account"),
						ResString.GetMultilingualString("D38B75A7-B70D-4418-8FDE-6D8DF4857640", @"This link account indicates the account number for Special VAT Prepayments (e.g. UST 1/11 in Germany).
Usually once a year, these VAT prepayments are made when a Permanent Extension of Time has been authorized by Fiscal Authorities.
Book your special VAT prepayment as Cashbook / Direct Payment on this account."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						Guid.Empty);

					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		#endregion

		#region GL Journals Approval

		public BooleanRegistryItem AllowUsersToApproveOwnGLJournals
		{
			get
			{
				return GetItem("AllowUsersToApproveOwnGLJournals", delegate
				{
					return new BooleanRegistryItem(
						"AllowUsersToApproveOwnGLJournals",
						Categories.Accounting_GeneralLedgerDefaults_GLJournalsApproval,
						ResString.GetMultilingualString("AEEA550B-BBBE-43B6-99EC-5504C4C31C29", "Allow users to approve own GL Journals"),
						ResString.GetMultilingualString("B7429BD9-4007-43BD-83EC-D019D3251E32",
@"By default, this registry is set to 'Yes'.

When set to 'No', users will not be able to approve GL Journals where they are the creating/editing users even  if
- they have the security rights to approve GL Journals AND
- the Journal values are within their approval thresholds.
GL Journals will need to be approved by entering another login username/password OR queue for approval by another login user via the Journal Awaiting Approval module.

In addition, when set to 'No', only current login user can approve requests (other than his own) in the Journal Awaiting Approval module if he has the rights to do that.
The Security Override Login window will not be prompted that allows another authorized username and password to be entered."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem AllowOnTheSpotApprovalsOfGLJournals
		{
			get
			{
				return GetItem("AllowOnTheSpotApprovalsOfGLJournals", delegate
				{
					return new BooleanRegistryItem(
						"AllowOnTheSpotApprovalsOfGLJournals",
						Categories.Accounting_GeneralLedgerDefaults_GLJournalsApproval,
						ResString.GetMultilingualString("2F22BB65-1157-4D48-AB5C-0682DE2033A2", "Allow On-the-Spot approvals of GL Journals"),
						ResString.GetMultilingualString("637B1F80-81EA-40D2-BAD0-40D294044D4E",
@"By default, this registry is set to 'Yes'.

When set to 'No', users will not be able to approve GL Journals via the GL Journal screen by entering an authorized username and password. GL Journals will need to be queued for approval via the Journal Awaiting Approval module."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public GLJournalApprovalThresholdRegistryItem NewGLJournalApprovalThresholdSetup
		{
			get
			{
				return GetItem("NewGLJournalApprovalThresholdSetup", delegate
				{
					var defaultCollection = new GLJournalApprovalThresholdCollection();

					return new GLJournalApprovalThresholdRegistryItem(
						"NewGLJournalApprovalThresholdSetup",
						Categories.Accounting_GeneralLedgerDefaults_GLJournalsApproval,
						ResString.GetMultilingualString("f1b1506d-e1b1-4f29-ad55-7765bc83ea1d", "New GL Journal Approval Threshold"),
						ResString.GetMultilingualString("62E99B66-40EA-47FE-BD8E-435251D9F33E",
@"Use this registry to set the amount threshold at which an authorized user can approve new GL Journals.

The authorization levels (up to 3) restrict the amounts that can be approved by users with specific authorization levels that is set up in the security settings.
'The authorization requirement 'None' allows user with access to save and approve journals with amount up to the specific threshold level.

NOTE: When ANY type is configured, all new GL Journals (except for NJL – Note Journal) will need to be approved before they can be posted."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultCollection);
				});
			}
		}

		public GLJournalApprovalThresholdRegistryItem ExistingGLJournalApprovalThresholdSetup
		{
			get
			{
				return GetItem("ExistingGLJournalApprovalThresholdSetup", delegate
				{
					var defaultCollection = new GLJournalApprovalThresholdCollection();

					return new GLJournalApprovalThresholdRegistryItem(
						"ExistingGLJournalApprovalThresholdSetup",
						Categories.Accounting_GeneralLedgerDefaults_GLJournalsApproval,
						ResString.GetMultilingualString("79AC120B-9D9C-4602-91C9-BF271D2AFBFB", "Existing GL Journal Approval Threshold"),
						ResString.GetMultilingualString("9BFB864F-5F4A-458D-A05C-2E09C6E76251",
@"Use this registry to set the amount threshold at which an authorized user can approve existing approved GL Journals.

The authorization levels (up to 3) restrict the amounts that can be approved by users with specific authorization levels that is set up in the security settings.
'The authorization requirement 'None' allows user with access to save and approve journals with amount up to the specific threshold level.

NOTE: When ANY type is configured, all changes to existing GL Journals (except for NJL – Note Journal) will need to be approved."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultCollection);
				});
			}
		}

		#endregion

		#endregion

		#region Tax Configurations

		#region SuppressResourceStringsCheckRegion

		public ZeroAmountTaxTypesDescriptionsRegistryItem ZeroAmountTaxTypesDescription
		{
			get
			{
				return GetItem("ZeroAmountTaxTypesDescription", delegate
				{
					return new ZeroAmountTaxTypesDescriptionsRegistryItem(
						"ZeroAmountTaxTypesDescription",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						ResString.GetMultilingualString("549257C0-D846-411D-AC39-C3DC2EE99031", "Description Printed in Documents for Zero Amount Tax Types"),
						ResString.GetMultilingualString("FD84F36F-3222-491C-8448-45A2570E3541", @"This registry defines the tax description printed in the invoice document when the VAT/GST Tax ID recorded against individual lines does not record an amount of VAT/GST tax.
If required, you can override the default values and configure a login company specific description."),
						RegistryStorageFlags.Company,
						ZeroAmountTaxTypesDescriptionsCollection.GetDefault());
				});
			}
		}

		public CodePairRegistryItem DescriptionInDocumentsForTaxAmountsRule
		{
			get
			{
				return GetItem("DescriptionInDocumentsForTaxAmountsRule",
					() => new CodePairRegistryItem(
						"DescriptionInDocumentsForTaxAmountsRule",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						ResString.GetMultilingualString("FED463E4-2D93-4EDB-8BFE-1322CDFD95A9", "Description Printed in Documents for Tax Amounts"),
						ResString.GetMultilingualString("5E66B2AC-D56A-4AE6-A4CA-E01BBDF5AC47", @"This registry decides how charges recorded with a VAT/GST tax amount will print in the Invoice document.
Depending on your configuration of this registry, the Invoice document will print the tax rate used to calculate VAT/GST tax and/or the resulting VAT/GST tax amount.
This registry is configurable at the Login Company level only."),
						new CodeDescriptionPairListProvider(() => new DescriptionInDocumentsForTaxAmountsRuleTypes()),
						RegistryStorageFlags.Company,
						AccountingConstants.DescriptionInDocumentsForTaxAmountsRuleTypes.Both.Code));
			}
		}

		public GuidRegistryItem MainGSTTaxID
		{
			get
			{
				return GetItem(AccTaxRate.Helper.MainGSTTaxRegistryID, delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						AccTaxRate.Helper.MainGSTTaxRegistryID,
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"Default GST Tax ID (CargoWiseOne Support Only)",
						(NoResString)"This defines the default GST Tax ID. It is used as a default value for the EU Tax ID Defaulting and CASS File Import Default Tax ID registry settings.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.IsOnlyForSupport,
						Guid.Empty);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.AccTaxRateTypeRated);
					return result;
				});
			}
		}

		public GuidRegistryItem MainGSTReverseTaxID
		{
			get
			{
				return GetItem(AccTaxRate.Helper.MainGSTReverseTaxRegistryID, delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						AccTaxRate.Helper.MainGSTReverseTaxRegistryID,
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"Default GST Reverse Tax ID (CargoWiseOne Support Only)",
						(NoResString)"This defines the default GST Reverse Tax ID. It is used as a default value for the EU Tax ID Defaulting registry setting.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.IsOnlyForSupport,
						Guid.Empty);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.AccTaxRateTypeReverseRated);
					return result;
				});
			}
		}

		public GuidRegistryItem MainNotReportableTaxID
		{
			get
			{
				return GetItem(AccTaxRate.Helper.MainNotReportableTaxRegistryID, delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						AccTaxRate.Helper.MainNotReportableTaxRegistryID,
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"Default Not Reportable Tax ID (CargoWiseOne Support Only)",
						(NoResString)"This defines the default Not Reportable Tax ID. It is used as a default value for the EU Tax ID Defaulting registry setting.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.IsOnlyForSupport,
						Guid.Empty);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.AccTaxRateTypeNotReportable);
					return result;
				});
			}
		}

		public GuidRegistryItem MainFreeGSTTaxID
		{
			get
			{
				return GetItem(AccTaxRate.Helper.MainFreeGSTTaxRegistryID, delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						AccTaxRate.Helper.MainFreeGSTTaxRegistryID,
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"Default Free GST Tax ID (CargoWiseOne Support Only)",
						(NoResString)"This defines the default Free GST Tax ID. It is used as a default value for the EU Tax ID Defaulting and CASS File Import Default Tax ID registry settings.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.IsOnlyForSupport,
						Guid.Empty);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.AccTaxRateTypeRated);
					return result;
				});
			}
		}

		public GuidRegistryItem MainFreeGSTReverseTaxID
		{
			get
			{
				return GetItem(AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID, delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						AccTaxRate.Helper.MainFreeGSTReverseTaxRegistryID,
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"Default Free GST Reverse Tax ID (CargoWiseOne Support Only)",
						(NoResString)"This defines the default Free GST Reverse Tax ID. It is used as a default value for the EU Tax ID Defaulting registry setting.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsValueMandatory | RegistryOptions.MustOverrideDefaultValue | RegistryOptions.IsOnlyForSupport,
						Guid.Empty);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.AccTaxRateTypeReverseRated);
					return result;
				});
			}
		}

		public CASSFileImportDefaultTaxIDRegistryItem CASSFileImportDefaultTaxID
		{
			get
			{
				return GetItem("CASSFileImportDefaultTaxID",
								() => new CASSFileImportDefaultTaxIDRegistryItem(
										"CASSFileImportDefaultTaxID",
										AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
										(NoResString)"CASS File Import Default Tax ID (CargoWiseOne Support Only)",
										(NoResString)@"This registry is referenced when importing CASS costs via the CASS Cost File Import module.
It is used to default an appropriate Tax ID against each charge imported via the CASS Cost File Import.",
										RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport));
			}
		}

		public GuidRegistryItem AccountFeeDefaultTaxID
		{
			get
			{
				return GetItem("AccountFeeDefaultTaxID", delegate
				{
					GuidRegistryItem result = new AccountFeeDefaultTaxIDRegistryItem(
						"AccountFeeDefaultTaxID",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						ResString.GetMultilingualString("beb2ef5d-1112-4c7d-9fad-f062729596dd", "Default Tax ID for Account Fee Invoice"),
						ResString.GetMultilingualString("B161D319-E6B2-43BA-BAFB-1FF577AEC594", @"This registry is referenced when creating Receivables (AR) Account Fee invoices.
This registry defines the VAT/GST Tax ID that is used when creating Receivables Account Fee Invoices."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccTaxRate, RegistryFindBoxFilter.VATTaxSystem);
					return result;
				});
			}
		}

		#region ConsumptionTaxGroupReportingCompany

		public GuidRegistryItem ConsumptionTaxGroupReportingCompany => GetItem(nameof(ConsumptionTaxGroupReportingCompany),
			() => new GuidRegistryItem(
						nameof(ConsumptionTaxGroupReportingCompany),
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						ResString.GetMultilingualString("097119a2-5c98-4eb1-85d1-e83c688b3131", "Consumption Tax Group Reporting Company"),
						ResString.GetMultilingualString("efa01086-fc5d-45c4-a471-ffaedcf8e0af", @"Set the value of this registry to the Company that is responsible for reporting the VAT consumption tax returns for the entire Tax Group. For the reporting Company itself, leave the registry empty.

Note: This registry is only applicable for MTD for VAT in UK."),
						new ConsumptionTaxGroupReportingCompanyDataType(),
						RegistryStorageFlags.Company,
						Guid.Empty)
			{
				Options = RegistryOptions.Default,
				EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbCompany)
			});

		internal class ConsumptionTaxGroupReportingCompanyDataType : GuidRegistryDataType
		{
			public ConsumptionTaxGroupReportingCompanyDataType() : base()
			{
			}

			protected override void ValidateCore(IRegistryItem registryItem, Guid proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

				if (proposedValue == companyPK)
				{
					throw new RegistryValidationException(ResString.GetMultilingualString("b4454553-8c2a-4c51-a71e-2b77b96ca32e", "The value cannot be equal to the fallback"));
				}
			}
		}

		#endregion

		public BooleanRegistryItem UseLocalExTaxAmountWhileCalculatingLocalTaxAmount
		{
			get
			{
				return GetItem("UseLocalExTaxAmountWhileCalculatingLocalTaxAmount",
					delegate
					{
						return new BooleanRegistryItem(
							new CountryEnabledBooleanRegistryItemImpl("UseLocalExTaxAmountWhileCalculatingLocalTaxAmount",
							AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
							(NoResString)"Use Local Ex Tax Amount While Calculating Local Tax Amount (CargoWiseOne Support Only)",
							(NoResString)@"If the value of this registry is set to 'Yes', the Local Tax Amount is calculated by applying the Tax Rate to the Local Ex Tax Amount. Otherwise, it is calculated by converting the OS Tax Amount to the Local Tax Amount using the exchange rate.",
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForSupport,
							Constants.CountryCodes.Italy));
					});
			}
		}

		public CodePairRegistryItem ItalyTaxRegimeID => GetItem("ItalyTaxRegimeID",
			() => new CodePairRegistryItem(
				"ItalyTaxRegimeID",
				Categories.Accounting_EReportingAndEInvoicingConfigurations_Italy,
				(NoResString)"Tax Regime ID (CargoWiseOne Support Only)",
				(NoResString)"This registry provides Tax Regime ID for <RegimeFiscale> element in Italy e-Invoicing XML mapping.",
				new CodeDescriptionPairListProvider(() => new ItalyTaxRegimeIdTypes()),
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				AccountingConstants.ItalyTaxRegimeIdTypes.RF01.Code));

		#region MTD Previous Period Inclusion Rules

		public IntRegistryItem NetValueOfVATErrorsThresholdLowerLimit
		{
			get
			{
				return GetItem("NetValueOfVATErrorsThresholdLowerLimit", () =>
					new IntRegistryItem(
					"NetValueOfVATErrorsThresholdLowerLimit",
					Categories.Accounting_UKMTDPreviousPeriodInclusionRules,
					(NoResString)"Net value of VAT errors threshold (lower limit)",
					(NoResString)"",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					10000));
			}
		}

		public IntRegistryItem NetValueOfVATErrorsThresholdUpperLimit
		{
			get
			{
				return GetItem("NetValueOfVATErrorsThresholdUpperLimit", () =>
					new IntRegistryItem(
					"NetValueOfVATErrorsThresholdUpperLimit",
					Categories.Accounting_UKMTDPreviousPeriodInclusionRules,
					(NoResString)"Net value of VAT errors threshold (upper limit)",
					(NoResString)"",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					50000));
			}
		}

		public DecimalRegistryItem PercentageOfNetOutputs
		{
			get
			{
				return GetItem("PercentageOfNetOutputs", () =>
					new DecimalRegistryItem(
					"PercentageOfNetOutputs",
					Categories.Accounting_UKMTDPreviousPeriodInclusionRules,
					(NoResString)"Percentage of Net outputs (box 6)",
					(NoResString)"",
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					1m));
			}
		}

		#endregion

		public CodeDescriptionPairListRegistryItem TaxReturnAdjustmentReasonsList
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair("CLS", ResString.GetMultilingualString("d3d1cd8b-f4f9-47fe-9883-927102b032e1", "Incorrect Classification"));
				lookUpList.AddPair("DEL", ResString.GetMultilingualString("41254d92-8398-443d-88a3-46cf1cfe9680", "Values not known at the time of input into software"));
				lookUpList.AddPair("IDE", ResString.GetMultilingualString("02c4b6e5-aed5-4ccf-82b7-7d552c6ea2c9", "Incorrect Data Entry"));

				return GetItem("TaxReturnAdjustmentReasonsList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"TaxReturnAdjustmentReasonsList",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						ResString.GetMultilingualString("4475b025-5b88-44d3-b3fe-8c3d89ef28b5", "Tax Return Adjustment Reason Codes"),
						ResString.GetMultilingualString("3d7ce6ee-a7ff-4919-9132-b2d1ee767dbe", "You can override the system defined Adjustment reason codes and descriptions for VAT return values using this Registry."),
						3,
						RegistryStorageFlags.Company,
						lookUpList
						);
				});
			}
		}

		#region Fiscal Tax Codes

		public CodeDescriptionPairListRegistryItem FiscalTaxCodeForOutputTaxAmountList
		{
			get
			{
				RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetter = (companyPK, branchPK, departmentPK) =>
																						 FiscalTaxCodeListHelper.GetDefaultValueOTX();
				return GetItem("FiscalTaxCodeForOutputTaxAmountList", delegate
				{
					var registryEntry = new CodeDescriptionPairListRegistryItem(
						name: "FiscalTaxCodeForOutputTaxAmountList",
						category: Categories.Accounting_TaxConfigurations_Germany,
						caption: (NoResString)"Fiscal Output Tax Codes",
						hint: (NoResString)@"This is a list of Fiscal Tax Codes used in the Advanced Turnover Tax Return reporting for Germany. 
The Codes are equivalent to the Number Codes used for the amounts for output tax on the paper form.",
						maxCodeLength: 2,
						editorInfo: new CodeDescriptionPairListEditorInfo(),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValueGetter: valueGetter);
					return registryEntry;
				});
			}
		}

		public CodeDescriptionPairListRegistryItem FiscalTaxCodeForInputTaxAmountList
		{
			get
			{
				RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetter = (companyPK, branchPK, departmentPK) =>
																						 FiscalTaxCodeListHelper.GetDefaultValueITX();
				return GetItem("FiscalTaxCodeForInputTaxAmountList", delegate
				{
					var registryEntry = new CodeDescriptionPairListRegistryItem(name: "FiscalTaxCodeForInputTaxAmountList",
						category: Categories.Accounting_TaxConfigurations_Germany,
						caption: (NoResString)"Fiscal Input Tax Codes",
						hint: (NoResString)@"This is a list of Fiscal Tax Codes used in the Advanced Turnover Tax Return reporting for Germany. 
The Codes are equivalent to the Number Codes used for the amounts for input tax on the paper form.",
						maxCodeLength: 2,
						editorInfo: new CodeDescriptionPairListEditorInfo(),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValueGetter: valueGetter);
					return registryEntry;
				});
			}
		}

		public CodeDescriptionPairListRegistryItem FiscalTaxCodeForOutputNetAmountList
		{
			get
			{
				RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetter = (companyPK, branchPK, departmentPK) =>
																						 FiscalTaxCodeListHelper.GetDefaultValueONT();
				return GetItem("FiscalTaxCodeForOutputNetAmountList", delegate
				{
					var registryEntry = new CodeDescriptionPairListRegistryItem(
						name: "FiscalTaxCodeForOutputNetAmountList",
						category: Categories.Accounting_TaxConfigurations_Germany,
						caption: (NoResString)"Fiscal Output Net Codes",
						hint: (NoResString)@"This is a list of Fiscal Tax Codes used in the Advanced Turnover Tax Return reporting for Germany. 
The Codes are equivalent to the Number Codes used for the net resp. base amounts for output tax on the paper form.",
						maxCodeLength: 2,
						editorInfo: new CodeDescriptionPairListEditorInfo(),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValueGetter: valueGetter);
					return registryEntry;
				});
			}
		}

		static class FiscalTaxCodeListHelper
		{
			static internal object GetDefaultValueONT() => defaultValuesONT ?? (defaultValuesONT = LoadDefaultValuesONT());

			static internal object GetDefaultValueOTX() => defaultValuesOTX ?? (defaultValuesOTX = LoadDefaultValuesOTX());

			static internal object GetDefaultValueITX() => defaultValuesITX ?? (defaultValuesITX = LoadDefaultValuesITX());

			static CodeDescriptionPairList LoadDefaultValuesONT() => ObjectFactory.Get<ICountryComplianceFactory>().GetIFiscalTaxCodeProvider(Constants.CountryCodes.Germany)?.GetFiscalTaxCodeForOutputNetAmount();

			static CodeDescriptionPairList LoadDefaultValuesOTX() => ObjectFactory.Get<ICountryComplianceFactory>().GetIFiscalTaxCodeProvider(Constants.CountryCodes.Germany)?.GetFiscalTaxCodeForOutputTaxAmount();

			static CodeDescriptionPairList LoadDefaultValuesITX() => ObjectFactory.Get<ICountryComplianceFactory>().GetIFiscalTaxCodeProvider(Constants.CountryCodes.Germany)?.GetFiscalTaxCodeForInputTaxAmount();

			[ThreadStatic]
			static CodeDescriptionPairList defaultValuesONT;
			[ThreadStatic]
			static CodeDescriptionPairList defaultValuesOTX;
			[ThreadStatic]
			static CodeDescriptionPairList defaultValuesITX;
		}

		#endregion

		#region Fiscal Tax Code Combinations

		public CodeDescriptionWithThreeGroupsRegistryItem ValidFiscalTaxCodeCombinationsList
		{
			get
			{
				RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetter = (companyPK, branchPK, departmentPK) =>
																						 FiscalTaxCodeCombinationsListHelper.GetDefaultValue();
				var registryEntry = GetItem("ValidFiscalTaxCodeCombinationsList", delegate
				{
					var regEnt = new CodeDescriptionWithThreeGroupsRegistryItem(
						name: "ValidFiscalTaxCodeCombinationsList",
						category: Categories.Accounting_TaxConfigurations_Germany,
						caption: (NoResString)"Valid Fiscal Tax Code Combinations",
						hint: (NoResString)@"This is a list of Valid Fiscal Tax Code Combinations used to set-up the Advanced Turnover Tax Return reporting for Germany. 
The Combinations are required to identify under which Fiscal Tax Code (Number Code) a base amount for output tax, the output tax itself or the input tax has to be specified.
Sometimes both amounts, means the base amount for output tax as well as the output tax amount have to be reported.
For reverse charge taxation all three amounts are required, means the base amount for output tax, the output tax amount as well as the input tax amount which is equal to the output tax.

The Code must have a certain structure: FiscalTaxCodeOutputNet:SortOrder_FiscalTaxCodeOutputTax:SortOrder_FiscalTaxCodeInputTax:SortOrder
Each Fiscal Tax Code and the Sort Order both have 2 digits. In case the Fiscal Tax Code = NA then colon and sort order are omitted.
Example: 46:40_47:41_67:68 or 35:03_36:04_NA or NA_NA_66:65",
						codeMaxLength: 17,
						editorInfo: new CodeDescriptionWithThreeGroupsRegistryEditorInfo(
							groupColumnCaption: ResString.GetMultilingualString("17C44AB1-CD94-4039-85AD-80DF7D787B29", "Fiscal Tax Code Output Net"),
							group2ColumnCaption: ResString.GetMultilingualString("60E20D66-B4F9-4B0A-95B8-E3613C5CE6D6", "Fiscal Tax Code Output Tax"),
							group3ColumnCaption: ResString.GetMultilingualString("58935D3E-F26D-4D9C-A1AE-E180D51DF79E", "Fiscal Tax Code Input Tax"),
							extraDescriptionColumnCaption: ResString.GetMultilingualString("3B4877FB-57EB-4FE8-802D-65B7C414F1CC", "Description Input Tax"),
							mainDescriptionColumnCaption: ResString.GetMultilingualString("2880DB2E-A057-44A9-AE8E-FDDA1E2F6B9B", "Description Output Tax"),
							areGroupColumnsVisible: true,
							areOnlyCodeAndGroupColumnsEditable: true),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.IsOnlyForSupport,
						defaultValueGetter: valueGetter,
						fiscalOutputNetCodeList: Instance.FiscalTaxCodeForOutputNetAmountList.Value,
						fiscalOutputTaxCodeList: Instance.FiscalTaxCodeForOutputTaxAmountList.Value,
						fiscalInputTaxCodeList: Instance.FiscalTaxCodeForInputTaxAmountList.Value);
					return regEnt;
				});
				return registryEntry;
			}
		}

		static class FiscalTaxCodeCombinationsListHelper
		{
			static internal object GetDefaultValue() => defaultValuesVFTCC ?? (defaultValuesVFTCC = LoadDefaultValuesVFTCC(Instance.FiscalTaxCodeForOutputNetAmountList.Value,
																															Instance.FiscalTaxCodeForOutputTaxAmountList.Value,
																															Instance.FiscalTaxCodeForInputTaxAmountList.Value));

			static CodeDescriptionWithThreeGroupsCollection LoadDefaultValuesVFTCC(ReadOnlyCodeDescriptionPairList ont, ReadOnlyCodeDescriptionPairList otx, ReadOnlyCodeDescriptionPairList itx) =>
				 ObjectFactory.Get<ICountryComplianceFactory>().GetIFiscalTaxCodeProvider(Constants.CountryCodes.Germany)?.GetValidFiscalTaxCodeCombinations(ont, otx, itx);

			[ThreadStatic]
			static CodeDescriptionWithThreeGroupsCollection defaultValuesVFTCC;
		}

		#endregion

		#region Tax Message ID – Fiscal Tax Code Combinations Mapping

		public CodeDescriptionWithGroupRegistryItem TaxMessageIdMappingList
		{
			get
			{
				return GetItem("TaxMessageIdMappingList", delegate
				{
					var defaultValue = GetTaxMessageIdMappingListDefaultValue();
					return new CodeDescriptionWithGroupRegistryItem(
						name: "TaxMessageIdMappingList",
						category: Categories.Accounting_TaxConfigurations_Germany,
						caption: ResString.GetMultilingualString("F0CD4BEA-8B39-4AA6-B877-B362E4AB839F", "Tax Message ID – Fiscal Tax Code Combinations Mapping"),
						hint: ResString.GetMultilingualString("79A335CA-1749-4E44-8D2A-3C22C2AD4D84", @"This is a mapping between Tax Message IDs and Valid Fiscal Tax Code Combinations.
A Tax Message ID is set for all relevant transaction lines.
This mapping is used to identify under which Fiscal Tax Code (Number Code) a base amount for output tax, the output tax itself or the input tax has to be specified in Advanced Turnover Tax Return reporting in Germany."),
						storage: RegistryStorageFlags.System,
						editorInfo: new CodeDescriptionWithGroupRegistryEditorInfo(ResString.GetMultilingualString("Accounting|TaxMessageIdMapping|Valid Fiscal Tax Code Combination", "Valid Fiscal Tax Code Combination"), true, true),
						defaultValue: defaultValue,
						includeMissingDefaults: true,
						removeNonDefaults: true);
				});
			}
		}

		CodeDescriptionWithGroupCollection GetTaxMessageIdMappingListDefaultValue()
		{
			var groupLookup = new CodeDescriptionPairList();
			var validFiscalTaxCodeCombinations = Instance.ValidFiscalTaxCodeCombinationsList.Value ?? new CodeDescriptionWithThreeGroupsCollection(10);
			foreach (CodeDescriptionWithThreeGroups v in validFiscalTaxCodeCombinations)
			{
				var description = v.Group != "NA"
					? v.Group + (v.Group2 != "NA" ? "/" + v.Group2 : "") + (v.Group3 != "NA" ? "/" + v.Group3 : "") + " " + v.MainDescription
					: v.Group3 + " " + v.ExtraDescription;
				groupLookup.Add(new CodeDescriptionPair((string)v.Code, description));
			}

			var result = new CodeDescriptionWithGroupCollection(groupLookup, "NA_NA_NA", 10);

			var factory = new BusinessObjectFactory();
			var taxMessageQuery = new ZQuery(AccInvMsgSchema.A9_RN_NKCountryCode, Constants.CountryCodes.Germany);
			taxMessageQuery.AddToFilter(AccInvMsgSchema.A9_IsActive, true);
			var taxMessages = factory.Load<AccInvMsg>(taxMessageQuery);

			taxMessages.ForEach(t => result.Add(t.A9_Code, (NoResString)t.A9_Description, "NA_NA_NA"));

			return result;
		}

		#endregion

		#region Tax Date Defaulting Option

		public TaxDateDefaultingOptionRegistryItem TaxDateDefaultingOption
		{
			get
			{
				return GetItem("TaxDateDefaultingOption", delegate
				{
					var defaultValues = GetTaxDateDefaultingOptionDafaultValue();
					return new TaxDateDefaultingOptionRegistryItem(
						"TaxDateDefaultingOption",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						ResString.GetMultilingualString("516FED2A-9D5F-4BDB-B45B-4B836CE9BDC0", "Tax Date Defaulting Option"),
						ResString.GetMultilingualString("5DA2E655-0B60-4A96-BE05-CCB92222523C", @"This registry allows you to configure the preferences for setting a Tax Date on Invoice Charges.

As you prepare charges in job billing, tax date defaults empty. If you enter a specific Tax Date on a charge, then this tax date is retained and is used to determine the applicable tax rate when posting the invoice. However, if you leave Tax Date as empty, then when posting the transaction, Tax Date is set to the preferred option as per the settings defined in this registry.

By default, Tax Date is set to Today's Date on all AR and AP Invoices. Override this registry to set the tax date to Invoice Date, Today's Date or various operational dates including Arrival Date and Departure Date."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						defaultValues);
				});
			}
		}

		TaxDateDefaultingOptionCollection GetTaxDateDefaultingOptionDafaultValue()
		{
			var result = new TaxDateDefaultingOptionCollection();

			var defaultValue = new TaxDateDefaultingOption();
			using (defaultValue.GetValidationSuspender()) // To avoid access to Resource String when validating on JobType property set
			{
				defaultValue.JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
				defaultValue.Ledger = TaxDateDefaultingOptionLookups.LedgerTypeAdditionalCodes.All;
				defaultValue.TaxDateOption = MasterFiles.Business.TaxDateDefaultingOption.Code.Today;
				result.Add(defaultValue);
			}

			return result;
		}

		#endregion

		#region Stamp Duty

		public GuidRegistryItem StampDutyInvoiceTaxMessage
		{
			get
			{
				return GetItem("StampDutyInvoiceTaxMessage", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"StampDutyInvoiceTaxMessage",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("e72892e7-4945-4d32-a9c5-681ed59b721c", "Stamp Duty Invoice Tax Message"),
						ResString.GetMultilingualString("68374094-aaff-43bc-9e82-4d6c5be315cf", @"{0} will record the Invoice Tax Message Code nominated in this registry against each Stamp Duty Charge Line automatically defaulted by {0} into an AR Invoice at the point of posting.
Note:  Currently this behavior is only enabled for Italy Login Companies.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccInvMsg);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem StampDutyARDocumentMessage
		{
			get
			{
				return GetItem("StampDutyARDocumentMessage", delegate
				{
					return new MultilingualStringRegistryItem("StampDutyARDocumentMessage",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("98c2cef5-d23a-45e2-8a9f-534ac5738009", "Stamp Duty AR Document Message"),
						ResString.GetMultilingualString("c7cf14ad-0050-4d29-a742-fe5e7cd6c50c", @"{0} will print this message in all Receivables Invoice or Receivables Credit Note documents WHEN the details of the transaction itself incurs a liability of the Login Company to pay a document stamp duty.
Note: Please remember to configure your system to clearly identify AR Document copies.
Note: Currently this behavior is only enabled for Italy Login Companies.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company);
				});
			}
		}

		public AccTaxRateListRegistryItem TaxIDsAttractingStampDuty
		{
			get
			{
				return GetItem("TaxIDsAttractingStampDuty", delegate
				{
					AccTaxRateListRegistryItem item = new AccTaxRateListRegistryItem(
						"TaxIDsAttractingStampDuty",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"Tax IDs Attracting Stamp Duty (CargoWiseOne Support Only)",
						(NoResString)"When Tax IDs Attracting Stamp Duty are present on an AR invoice and the local ex tax amount of these lines exceeds the Stamp Duty Threshold, the Stamp Duty Fixed Amount will be applied to the invoice.",
						string.Empty,
						RegistryFindBoxFilter.None);
					item.Options = RegistryOptions.IsOnlyForSupport;
					return item;
				});
			}
		}

		public DecimalRegistryItem StampDutyFixedAmount
		{
			get
			{
				return GetItem("StampDutyFixedAmount", delegate
				{
					DecimalRegistryItem item = new DecimalRegistryItem(
						"StampDutyFixedAmount",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"Stamp Duty Fixed Amount (CargoWiseOne Support Only)",
						(NoResString)"The Stamp Duty Fixed Amount will be applied to AR Invoices, when Tax IDs Attracting Stamp Duty are present on the invoice and the local ex tax amount of these lines exceeds the Stamp Duty Threshold.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						0m
						);
					item.EditorInfo = new NumericRegistryEditorInfo(2);
					return item;
				});
			}
		}

		public DecimalRegistryItem StampDutyThreshold
		{
			get
			{
				return GetItem("StampDutyThreshold", delegate
				{
					DecimalRegistryItem item = new DecimalRegistryItem(
						"StampDutyThreshold",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"Stamp Duty Threshold (CargoWiseOne Support Only)",
						(NoResString)"If the local ex tax amount on an invoice exceeds the Stamp Duty Threshold and Tax IDs Attracting Stamp Duty are present on these lines, the Stamp Duty Fixed Amount will be applied to the AR invoice.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						0m
						);
					item.EditorInfo = new NumericRegistryEditorInfo(2);
					return item;
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region General Ledger Defaults

		#region GL Account Format

		public static string GetGLAccountFormat(string accountNum)
		{
			var result = new ZStringBuilder();
			foreach (char c in accountNum)
			{
				result.Append(c == '.' ? "." : "X");
			}
			return result.IsEmpty ? "XXXX.XXX" : result.ToString();
		}

		public StringRegistryItem GLAccountFormat
		{
			get
			{
				return GetItem("GL_ACCOUNT_FORMAT", delegate
				{
					AccGLHeader header = new BusinessObjectFactory().LoadTop1<AccGLHeader>(new ZQuery() { OrderBy = AccGLHeaderSchema.PK.Name });
					string defaultValue = (header != null) ? GetGLAccountFormat(header.AG_AccountNum) : "";
					return new StringRegistryItem("GL_ACCOUNT_FORMAT",
						null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden, defaultValue);
				});
			}
		}

		public LinkRegistryItem GLAccountFormatLink
		{
			get
			{
				return GetNonCachedItem(delegate
				{
					return new LinkRegistryItem(
						Categories.Accounting_GeneralLedgerDefaults,
						ResString.GetMultilingualString("3b3b6c85-3810-4ff0-b4f1-bdfe7c928a18", "GL Account Format"),
						ResString.GetMultilingualString("e41eb03a-761a-49b2-9ffd-b79fcbc640c0", "The format of numbers in the General Ledger Chart of Accounts (go to Maintain -> Account -> GL Accounts) must be in the format set in this registry. Click the button below to migrate the current format to a different format. This will also update all existing General Ledger accounts."),
						ModuleIDs.GLAccountFormat);
				});
			}
		}

		#endregion

		#region Generate Journal Entries - Start Date

		public DateTimeRegistryItem GenerateJournalEntriesStartDate
		{
			get
			{
				var item = GetItem(nameof(GenerateJournalEntriesStartDate),
					() => new GenerateJournalEntriesStartDateItemImpl(
						nameof(GenerateJournalEntriesStartDate),
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						ResString.GetMultilingualString("B8AD8E7C-6510-4A64-8593-6B33D3C92C3A", "Generate Journal Entries - Start Date"),
						ResString.GetMultilingualString("2024098B-C6DD-4F7E-8254-45B56B0C76D2", @"By default, journal entries are not stored in the database. They are calculated as needed when generating certain types of reports, such as the GL Transactions report.
When a start date is specified, the system will generate journal entries for all accounting transactions posted from that date onwards and store them in the database.

Note:
1. The date specified must be the start date of the first accounting period.
2. This date must not be later than today's date.
3. This date must be earlier or the same as previous date, if already specified.
4. If 'Journal Entries Last Processed Date' registry does not have a value, then the 'Start Date' must fall in the current accounting year.
5. If 'Journal Entries Last Processed Date' registry has a value, then the 'Start Date' must fall in the prior accounting year based on the last processed date."),
						RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly : RegistryOptions.IsHidden));
				item.OnBuildLogReference += (args) => Res.GetString("51AEF333-AF11-4776-9735-3F913DD5C969", "Start Date set to '{0}'.", args.NewValue);
				return item;
			}
		}

		#endregion

		#region Generate and Store Journal Entries for Posted Accounting Transactions

		public BooleanRegistryItem GenerateAndStoreJournalEntriesForPostedAccountingTransactions
		{
			get
			{
				var item = GetItem(nameof(GenerateAndStoreJournalEntriesForPostedAccountingTransactions), delegate
				{
					return new GenerateAndStoreJournalEntriesForPostedAccountingTransactionsItemImpl(
						nameof(GenerateAndStoreJournalEntriesForPostedAccountingTransactions),
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						ResString.GetMultilingualString("DF03E728-C1AB-441B-B4CC-C23C0D87268D", "Generate and Store Journal Entries for Posted Accounting Transactions"),
						ResString.GetMultilingualString("EFC665F0-6260-45C0-95F0-5E278DED258C", @"By default, this feature is disabled. 

When enabled, the system will generate and store the journal entries for all newly posted accounting transactions. Additionally, the journal entries of all previously posted accounting transactions will be generated in batches from the most recent accounting year to the first accounting year. 

You can track the status of the backlog transactions processing via the ""Journal Entries Last Processed Date"" registry.  

Important Note: You cannot disable this registry once it has been enabled."
),
						RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value ? RegistryOptions.MustOverrideDefaultValue : RegistryOptions.IsHidden,
					false,
					new GenerateAndStoreJournalEntriesForPostedAccountingTransactionsType());
				});

				item.OnBuildLogReference += (args) => Res.GetString("E8DDF6C0-D435-490D-B53E-455357642679", "Registry value changed from [{0}] to [{1}].", (bool)args.OriginalValue ? (NoResString)"Yes" : (NoResString)"No", (bool)args.NewValue ? (NoResString)"Yes" : (NoResString)"No");
				return item;
			}
		}

		#endregion

		#region Bank Currency Adjustment Exchange Rate Type

		public CodePairRegistryItem BankCurrencyAdjustmentExchangeRateType
		{
			get
			{
				return GetItem("BankCurrencyAdjustmentExchangeRateType", delegate
				{
					var item = new CodePairRegistryItem(
					"BankCurrencyAdjustmentExchangeRateType",
					Categories.Accounting_GeneralLedgerDefaults,
					ResString.GetMultilingualString("a3390c05-bc14-4542-ab3d-6ccee893f0a3", "Bank Currency Adjustment Exchange Rate Type"),
					ResString.GetMultilingualString("11c38162-0dc6-4a5f-afb5-0b72ad8aeee0", @"This registry defines the exchange rate type that will be used when performing Bank Currency Adjustment. 

By default, this registry will be set to ‘PER’ exchange rate type. 

Note: 
For ‘PER’ exchange rate type, the system will fallback to ‘BUY’ exchange rate type if a ‘PER’ exchange rate cannot be found.
For all other exchange rate types, there will be no fallback logic."),
											new ExchangeRateTypeListProvider(),
											RegistryStorageFlags.System | RegistryStorageFlags.Company,
											Constants.ExchangeRateTypes.Code.PeriodEndRate);

					item.OnBuildLogReference += (args) => Res.GetString("b25bccee-c512-418f-a01e-90bb0756e24c", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		#endregion

		#region AR/AP Outstanding Balances Currency Adjustment Exchange Rate Type
		public CodePairRegistryItem ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType
		{
			get
			{
				return GetItem("ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType", delegate
				{
					return new CodePairRegistryItem(
						"ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType",
						Categories.Accounting_GeneralLedgerDefaults,
						ResString.GetMultilingualString("5caa54ca-4bd1-490d-9c19-f6ad24192a6c", "AR/AP Outstanding Balances Currency Adjustment Exchange Rate Type"),
						ResString.GetMultilingualString("42886e6e-3a57-42d1-9ae3-d4763f942753", @"This registry defines the exchange rate type that will be used when performing Receivables/Payables Outstanding Balances Currency Adjustments.
By default, this registry will be set to ‘PER’ exchange rate type. 

Note: 
For 'PER' exchange rate type, there will only be one exchange rate recorded for each accounting period and this will be used for the currency adjustment calculation.
For other exchange rate types (non-PER), the currency adjustment calculation will use the exchange rate valid on the End Date of the accounting period."),
						new CodeDescriptionPairListProvider(() => AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value.GetCodeDescriptionPairList()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Constants.ExchangeRateTypes.Code.PeriodEndRate);
				});
			}
		}
		#endregion

		#region AR Control Account Adjustment
		public GuidRegistryItem ARControlAccountAdjustment
		{
			get
			{
				return GetItem("ARControlAccountAdjustment", delegate
				{
					var result = new GuidRegistryItem(
						"ARControlAccountAdjustment",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("029D7A17-0FA8-4091-A244-7693333FE55C", "AR Control Account Adjustment"),
						ResString.GetMultilingualString("7AC0079D-B350-4CB0-8AC1-86D3226F879B", @"When the registry 'Auto Create A/R and A/P Outstanding Balances Currency Adjustments' is set to 'Yes', {0} will use this general ledger account when creating A/R Outstanding Balance Currency Adjustment GL Journal.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Guid.Empty);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSH);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}
		#endregion

		#region AP Control Account Adjustment
		public GuidRegistryItem APControlAccountAdjustment
		{
			get
			{
				return GetItem("APControlAccountAdjustment", delegate
				{
					var result = new GuidRegistryItem(
						"APControlAccountAdjustment",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("d2d37c0e-1706-40d5-9738-895df833b7b1", "AP Control Account Adjustment"),
						ResString.GetMultilingualString("02623206-c39c-42b2-8954-9378402a9153", @"When the registry 'Auto Create A/R and A/P Outstanding Balances Currency Adjustments' is set to 'Yes', {0} will use this general ledger account when creating A/P Outstanding Balance Currency Adjustment GL Journal.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Guid.Empty);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSH);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}
		#endregion

		#region Auto Create A/R and A/P Outstanding Balances Currency Adjustments
		public class AutoCreateARAndAPOutstandingBalancesCurrencyAdjustmentsRegistryItem : BooleanRegistryItem
		{
			public AutoCreateARAndAPOutstandingBalancesCurrencyAdjustmentsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool defaultValue)
				: base(name, category, caption, hint, storage, options, defaultValue)
			{
				OnBuildLogReference = BuildLogReference;
				OnUpdateAction = DeleteCurrencyAdjustmentQueue_OnUpdateAction;
			}

			ZString OnUpdateActionMessage { get; set; }

			string BuildLogReference(BuildLogReferenceArgs args)
			{
				var log = Res.GetString("c183d2b1-99b8-44ee-b138-d4c763d67f76", "Registry value changed from [{0}] to [{1}]. {2}", args.OriginalValue, args.NewValue, OnUpdateActionMessage);
				OnUpdateActionMessage = ZString.Empty;
				return log;
			}

			[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
			internal void DeleteCurrencyAdjustmentQueue_OnUpdateAction(Guid companyPk, Guid branchPk, Guid departmentPk, object newValue)
			{
				if (!(bool)newValue)
				{
					var cmdText = FormattableString.Invariant($@"DELETE dbo.AccCurrencyAdjustmentQueue
					OUTPUT AccPeriodManagement.AM_Period
					FROM dbo.AccCurrencyAdjustmentQueue LEFT JOIN dbo.AccPeriodManagement ON ACA_ParentID = AM_PK
					WHERE ACA_GC = @CompanyPK");

					using (var cmd = Db.Connection.Command(cmdText))
					{
						cmd.CommandType = CommandType.Text;
						cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPk);
						var result = new List<ZString>();
						using (var reader = cmd.ExecuteReader())
						{
							while (reader.Read())
							{
								if (!reader.IsDBNull(0))
								{
									result.Add(reader.GetInt32(0).ToString(CultureInfo.InvariantCulture));
								}
							}
						}
						if (result.Count > 0)
						{
							OnUpdateActionMessage = Res.GetString("cd7e613d-aaec-4f82-abaa-d211de652c5a", "All queue records deleted. Period:");
							OnUpdateActionMessage += ZString.Join(", ", result.ToArray());
						}
					}
				}
			}
		}

		public AutoCreateARAndAPOutstandingBalancesCurrencyAdjustmentsRegistryItem AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments
		{
			get
			{
				return GetItem("AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments", delegate
				{
					return new AutoCreateARAndAPOutstandingBalancesCurrencyAdjustmentsRegistryItem(
						"AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments",
						Categories.Accounting,
						ResString.GetMultilingualString("d2abcb4a-1e5a-4269-8fca-7adb715f1129", "Auto Create A/R and A/P Outstanding Balances Currency Adjustments"),
						ResString.GetMultilingualString("7ac8aec5-2621-4f13-b00f-70faeeb3e1a2", @"By default this registry is set to 'No'.
When this registry is set to 'Yes' and a sub ledger is closed, the system will queue the accounting period for A/R and A/P ledger outstanding balances currency adjustment according to exchange rate type specified in the 'AR/AP Outstanding Balances Currency Adjustment Exchange Rate Type' registry.

The system will auto-calculate the currency adjustment for outstanding balance in each transacted foreign currency as at the end of the accounting period.
At the end of the process, the system will create a 'RJL' general ledger journal. (Note: The general ledger must not be closed else the automation process will fail.)

The value of currency adjustment will be determined based on the difference between: 
1. Outstanding Balance in Invoiced Currency converted to Local Currency Equivalent using the Transaction's Historical Exchange Rate. 
2. Outstanding Balance in Invoiced Currency converted to Local Currency Equivalent using the AR/AP Outstanding Balances Currency Adjustment Exchange Rate.

The 'RJL' general ledger journal will be created as follows:
1. The header description will be set with reference to the 'Accounting > Transaction Description Defaults > A/R and A/P Outstanding Balance Currency Adjustment Journal'.
2. The 'Post In' period will be set with reference to the period of which sub ledger has been closed.
3. The 'Reverse Period' will be set to 'Post In' period + 1 (Note: If the Post In period is the last period of the accounting year, it will be set to the first period of the subsequent accounting year.)
4. The general ledger journal will be automatically approved and posted as this is system generated. 
5. The accounting entries will be posted as follows:

   For A/R Outstanding Balances Currency Adjustment
   Debit/Credit  
   Credit/Debit

   For A/P Outstanding Balances Currency Adjustment
   Debit/Credit  
   Credit/Debit

A pair of journal lines will be added for each currency + ledger + unrealized exchange gain.
A pair of journal lines will be added for each currency + ledger + unrealized exchange loss.

The line description will contain information on the foreign currency balance, original local and adjusted local. 
E.g. unrealized Gain based on USD 57,550.00, Original Local AUD 70,953.03 (Ex.Rate 0.8111), Adjusted Local AUD 70,544.25 (Ex.Rate 0.8158)

Note: 
On successful creation, you will be able to view the GL Journal in Manage > General Ledger > Journals module. 
A flag will also be updated in Manage > General Ledger > Period Management module to indicate if the A/R and A/P Outstanding Balance Adjustment Journal has been created.
In the event where there is an error, the general ledger journal will not be posted with the error details provided in the service task's log file. 
You will need to correct the error and the service task will auto create the journal once all errors are cleared."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}
		#endregion

		#region MatchStatus

		public CodeDescriptionPairListWithDefaultCodeRegistryItem MatchStatus
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(AccountingConstants.MatchStatusTypes.Unallocated.Code, AccountingConstants.MatchStatusTypes.Unallocated.Description);

				return GetItem("MatchStatus", delegate
				{
					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"MatchStatus",
						Categories.Accounting,
						ResString.GetMultilingualString("0280202e-343c-45f6-82a6-eff35ec5b971", "Match Status"),
						ResString.GetMultilingualString("9b1b6545-4804-4110-9b90-ecd9dfafd626", @"The Match Statuses listed here are used on Accounts Receivables and Account Payables Transaction.
They can be used optionally to classify the match status of each transaction.
Further, a reason code can be optionally recorded to provide the background of this classification.
The Match Status Reason Codes are configurable via the Accounting > Match Status Reason Codes system registry."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						lookUpList,
						false,
						false,
						3,
						false
						);
				});
			}
		}

		#endregion

		#region MatchStatus

		public CodeDescriptionPairListWithDefaultCodeRegistryItem MatchStatusReason
		{
			get
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(AccountingConstants.MatchStatusReasonCodeTypes.InAdvance.Code, AccountingConstants.MatchStatusReasonCodeTypes.InAdvance.Description);

				return GetItem("MatchStatusReason", delegate
				{
					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"MatchStatusReason",
						Categories.Accounting,
						ResString.GetMultilingualString("a96f22c9-3c54-4495-a16e-b61a1d544dfb", "Match Status Reason"),
						ResString.GetMultilingualString("e9240ad5-615e-4ba6-b8b5-dbff610d6bc0", @"The Match Status Reason Codes listed here are used on Accounts Receivables and Account Payables Transaction to support a matching status.
They can be used to provide the background of the match status classification."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						lookUpList,
						false,
						false,
						3,
						false
						);
				});
			}
		}

		#endregion

		#region Netting

		public GuidRegistryItem NettingSystemOrg
		{
			get
			{
				return GetItem("NettingSystemOrg",
					delegate
					{
						var result = new GuidRegistryItem("NettingSystemOrg",
							Categories.Accounting_Netting,
							ResString.GetMultilingualString("9821b233-4b8e-4944-a3d2-18c7e468c5a0", "Netting System Organization"),
							ResString.GetMultilingualString("2a194820-2b26-4da9-aeb6-7286eb31c9be", "This registry allows to set the Netting System organization."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							AccountingMasterFilesRegistry.Instance.EnableNetting.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
							Guid.Empty);

						result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgHeader);
						return result;
					});
			}
		}

		public BooleanRegistryItem IsNettingSystem
		{
			get
			{
				return GetItem("IsNettingSystem",
						delegate
						{
							return new BooleanRegistryItem("IsNettingSystem",
								Categories.Accounting_Netting,
								(NoResString)"Is Netting System? (CargoWiseOne Support Only)",
								(NoResString)"Marks a company as a Netting System. It can be set only through CargoWiseOne Support.",
								RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, false);
						});
			}
		}

		public DecimalRegistryItem NettingThresholdValue
		{
			get
			{
				return GetItem("NettingThresholdValue",
					delegate
					{
						return new DecimalRegistryItem("NettingThresholdValue",
							Categories.Accounting_Netting,
							ResString.GetMultilingualString("550fe6b4-c2a9-4315-824e-f960dc371b4c", "Threshold for Netting matching"),
							ResString.GetMultilingualString("b870e691-2119-4795-a789-c9676fec7af1", @"This threshold value is used as percentage value to match Receivables Netting Transactions with Payables Netting Transactions in the netting system."),
							RegistryStorageFlags.Company,
							AccountingMasterFilesRegistry.Instance.EnableNetting.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
							1M);
					});
			}
		}

		public CodePairRegistryItem NettingModeOption
		{
			get
			{
				return GetItem("NettingModeOption", delegate
				{
					return new CodePairRegistryItem(
						"NettingModeOption",
						Categories.Accounting_Netting,
						(NoResString)"Netting Mode (CargoWiseOne Support Only)",
						(NoResString)@"Use this registry to configure the netting mode.
Company Level: Netting statements will be generated per company.
Organization Level: Netting statements will be generated per participant.",
						new CodeDescriptionPairListProvider(() => new NettingModeOption()),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						AccountingConstants.NettingModeOption.CompanyLevel.Code);
				});
			}
		}

		public DateTimeRegistryItem NettingStartDate
		{
			get
			{
				return GetItem("NettingStartDate", delegate
				{
					return new DateTimeRegistryItem(
						"NettingStartDate",
						Categories.Accounting_Netting,
						(NoResString)"Netting participation start date (CargoWiseOne Support Only)",
						(NoResString)@"When this registry value is set in conjunction with 'Enable Netting', enables invoices to be sent to the netting system for processing. Those invoices that are created after this date will be sent to the netting system.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public AccountingRegistryItem NettingMatchingControlAccount
		{
			get
			{
				return GetItem("NettingMatchingControlAccount", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"NettingMatchingControlAccount",
						Categories.Accounting_Netting,
						ResString.GetMultilingualString("2ef5d87c-6621-41ff-a0cf-1493d951e720", "Netting Clearing Account"),
						ResString.GetMultilingualString("715b3cdd-4200-4841-b033-7f8ac43ec01c", "The system will use this general ledger account when creating balancing journals as part of the Netting Process."));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl);
					return result;
				});
			}
		}

		public DateTimeRegistryItem NettingLastMoveUnmatchedTransactionTimeStamp
		{
			get
			{
				return GetItem("NettingLastMoveUnmatchedTransactionTimeStamp", delegate
				{
					return new DateTimeRegistryItem(
						"NettingLastMoveUnmatchedTransactionTimeStamp",
						null, null, null,
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden);
				});
			}
		}

		#endregion

		#region Max Accrual vs Actual discrepancies
		public IntRegistryItem MaxAccrualVsActualDiscrepancies
		{
			get
			{
				return GetItem("MaxAccrualVsActualDiscrepancies", delegate
				{
					return new IntRegistryItem(
						"MaxAccrualVsActualDiscrepancies",
						Categories.Accounting_PayableDefaults_DefaultSettings_BulkAPInvoicePosting,
						ResString.GetMultilingualString("14a120ef-75e1-46ce-b288-b87d28adb7ec", "Max Accrual vs Actual discrepancies"),
						ResString.GetMultilingualString("661e02f0-c1bb-4b38-8080-529a9cbcb6e2", @"Enter the value that is the maximum acceptable difference between accruals and the AP invoice total being posted by the Bulk AP Invoice Posting option.
A discrepancy greater than this amount will prevent a Bulk AP Invoice from posting.
The Discrepancy value will be posted to the account defined in the registry 'Discrepancy GL Account'."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						0);
				});
			}
		}
		#endregion

		#region GL Presentation Journal Categories

		public GLPresentationJournalCategoryRegistryItem GLPresentationJournalCategoriesList
		{
			get
			{
				return GetItem("GLJournalAdjustmentCategoriesList", delegate
				{
					return new GLPresentationJournalCategoryRegistryItem(
						"GLJournalAdjustmentCategoriesList",
						Categories.Accounting_GeneralLedgerDefaults,
						ResString.GetMultilingualString("b4cd97b0-4ffc-4b64-adfc-088cf478c272", "GL Presentation Journal Categories"),
						ResString.GetMultilingualString("0002af1e-93b2-4750-9333-9c670c98c9a8", @"When overridden, the category codes added / listed here are available for use on General Ledger Journals.

By default, journals assigned a category are ignored by all General Ledger Reports. They are only included in the output of a report when expressly included.  These categories are used for the preparation and presentation of special purpose general ledger reports without affecting the standard financial reports.

Note:
1.	‘Elimination’ category is used in the GL Consolidations module to generate elimination journals. 
2.	‘Parent Code’ is used for grouping presentation categories. Each presentation category can only be assigned to a single parent code. During the generation of the reports, you will have the option to include presentation journals for a parent category and it’s child OR a specific category only. 
3.	‘Closing’ category is used to identify period end closing journals. At this stage, this is used in the generation of the GB-T 24589.1 Data Interface in China environment. All GL presentation journals posted against this category will be included in the accounting voucher and GL account balance sections of the XML file."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Collection Order Rejection Reason Codes List

		public CodeDescriptionPairListRegistryItem OrderRejectReasonCodesList
		{
			get
			{
				CodeDescriptionPairList lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair("INS", ResString.GetMultilingualString("d8f966d6-a44b-49e5-a5de-37779a90ae39", "Insufficient Funds in Bank Account"));
				lookUpList.AddPair("DIS", ResString.GetMultilingualString("f9da0666-bfd6-44a7-9dc3-c0e613b72e4c", "Invoice charges disputed"));
				lookUpList.AddPair("MII", ResString.GetMultilingualString("1750b1c7-4490-4cd4-afd3-445dbc23f173", "Missing Invoice"));
				lookUpList.AddPair("MIS", ResString.GetMultilingualString("95fbcd21-e2fd-42f3-8c83-1cb4cf63c20c", "Missing Statement"));
				lookUpList.AddPair("TXT", ResString.GetMultilingualString("e7a3fda6-16f7-4da2-a258-07e405535dfe", "Free Text"));

				return GetItem("ReasonCodesListForRejectOrders", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"ReasonCodesListForRejectOrders",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_CollectionBatchesAndOrders,
						ResString.GetMultilingualString("17eae972-0894-453b-9197-9e3810c03067", "Collection Order Rejection Reason Codes"),
						ResString.GetMultilingualString("54c13d30-ca7d-46f1-a1ce-8cef048eb393", @"Collection Order Rejection Reason Codes."),
						3,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						lookUpList
						);
				});
			}
		}

		#endregion

		#endregion

		#region Claims and Queries

		public CodeDescriptionPairListWithDefaultCodeRegistryItem QueryClaimType
		{
			get
			{
				return GetItem("AccountingQueryClaimType", delegate
				{
					QueryClaimTypeCodeList defaultValue = new QueryClaimTypeCodeList();
					defaultValue.DefaultCode = QueryClaimTypeCodeList.Codes.QCType3;
					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"AccountingQueryClaimType", Categories.Accounting_ClaimsandQueries, ResString.GetMultilingualString("f464a735-ae5b-4de1-97f3-0430d3ca7fd7", "Claim Type"), ResString.GetMultilingualString("9dddef41-bf74-4964-be81-3bb79caf6c18", @"The Type Codes listed here are used on Accounts Payable and Accounts Receivable Claims and Queries.
They are used operationally to assist in the classification, evaluation and review of each claim recorded in a company."), RegistryStorageFlags.System | RegistryStorageFlags.Company, defaultValue, true);
				});
			}
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem ClaimStatus
		{
			get
			{
				return GetItem("AccountingClaimStatus", delegate
				{
					QueryClaimStatusCodeList defaultValue = new QueryClaimStatusCodeList();
					defaultValue.DefaultCode = QueryClaimStatusCodeList.Codes.QCStatus1Open;
					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"AccountingClaimStatus", Categories.Accounting_ClaimsandQueries, ResString.GetMultilingualString("619231c5-083c-4ca9-b6a3-47db01845331", "Claim Status"), ResString.GetMultilingualString("f2cf3635-80f0-4c3a-9672-e0919256899f", @"The Status Codes listed here are used on Accounts Payable and Accounts Receivable Claims and Queries.
They are used operationally to identify the current status of each claim recorded in a company."), RegistryStorageFlags.System | RegistryStorageFlags.Company, defaultValue, true);
				});
			}
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem ClaimReason
		{
			get
			{
				return GetItem("AccountingClaimReason", delegate
				{
					QueryClaimReasonCodeList defaultValue = new QueryClaimReasonCodeList();
					defaultValue.DefaultCode = QueryClaimReasonCodeList.Codes.QCReason1;
					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"AccountingClaimReason", Categories.Accounting_ClaimsandQueries, ResString.GetMultilingualString("faf2de11-4eb2-4aa5-a19d-f1847058871b", "Claim Reason"), ResString.GetMultilingualString("53f868f6-1082-44ad-9b8f-f589cff8beac", @"The Reason Codes listed here are used on Accounts Payable and Accounts Receivable Claims and Queries.
They are used operationally to assist in the classification, evaluation and review of each claim recorded in a company."), RegistryStorageFlags.System | RegistryStorageFlags.Company, defaultValue, true);
				});
			}
		}

		#endregion

		#region Collection Calls

		public IntRegistryItem CollectionCallFollowUpDays
		{
			get
			{
				return GetItem("CollectionCallFollowUpDays", delegate
				{
					return new IntRegistryItem(
						"CollectionCallFollowUpDays",
						Categories.Accounting_CollectionCalls,
						ResString.GetMultilingualString("24ab70bb-b381-421b-8be3-d469ab5ab47d", "Collection Call Follow Up Days"),
						ResString.GetMultilingualString("696f8e0b-77e4-4919-91d2-fa3f4370f27b", "The number of days between when a Collection Call is created and its 'Follow Up' Date."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						7);
				});
			}
		}

		public BooleanRegistryItem CollectionCallCreateFollowUpAppointments
		{
			get
			{
				return GetItem("CollectionCallCreateFollowUpAppointments", delegate
				{
					return new BooleanRegistryItem(
						"CollectionCallCreateFollowUpAppointments",
						Categories.Accounting_CollectionCalls,
						ResString.GetMultilingualString("259055e9-0197-493c-8bff-b335e736a0ad", "Create follow up appointments"),
						ResString.GetMultilingualString("87d6f1c4-f7a9-4452-8c87-a249eedddddb", "This controls whether a follow up appointment will be created when you nominate a 'Follow up' date on a Collection Call"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Intercompany Posting Configuration

		public IntercompanyPostingConfigurationRegistryItem IntercompanyPostingConfiguration
		{
			get
			{
				return GetItem("IntercompanyPostingConfiguration", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("0728bd6d-60a3-442f-8fa8-d5f82a82d3c6", "This registry defines the Cost Variance Approval Levels used by {0} to decide when to automatically accept charges (import and post) from a sister company; and when not to import the charges because the variance between costs and accrued charges is too great.  Note: This registry is only relevant when the 'Auto Import Sister Company AR Invoices as AP Invoices' registry is also enabled.", BrandingFactory.Instance.ProductName);
					return new IntercompanyPostingConfigurationRegistryItem(
						"IntercompanyPostingConfiguration",
						Categories.Accounting,
						ResString.GetMultilingualString("93da338e-1c6f-45ee-8e00-0defc6710c9e", "Intercompany Posting Configuration"),
						hint,
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Stamp Duty Recharge

		public StampDutyRechargeRegistryItem StampDutyRecharge
		{
			get
			{
				return GetItem("StampDutyRecharge", delegate
				{
					MultilingualString hint = ResString.GetMultilingualString("846D0A8C-1729-4CD9-BF10-AA6D4D22AACF", @"By default, {0} will automatically add a Stamp Duty charge line to a Receivables Invoice or Credit Note when the posting of that transaction triggers a Stamp Duty Liability for the Login Company. 
The addition of a Stamp Duty charge line effectively recovers the Login Company’s Stamp Duty cost from the Receivables organization. 
If required, this registry can be overridden and used to limit this automatic stamp duty recharge behavior. 
Use this registry to limit the Stamp Duty recharge behavior based on the Receivable Organization’s location and/or transaction type. 
Note: Currently this behavior is only enabled for Italy Login Companies.", BrandingFactory.Instance.ProductName);
					return new StampDutyRechargeRegistryItem(
						"StampDutyRecharge",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("71dc6433-484c-47e4-bb92-1af413ad6fa7", "Stamp Duty Recharge"),
						hint,
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Receivable Defaults

		#region Default Settings

		#region Tax Message Is Mandatory

		public CodePairRegistryItem TaxMessageIsMandatoryReceivables => GetItem("TaxMessageIsMandatoryReceivables",
			() => new CodePairRegistryItem(
					new CountrySpecificDefaultValueRegistryItemImpl<string>(
						"TaxMessageIsMandatoryReceivables",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("81D2E954-D669-415A-8B8E-CF94BB2969A5", @"This registry allows you to enforce that a tax message must be recorded on receivable transactions and cash book direct receipt transactions. 

When set to NOT - Not Required, then Invoice Tax Message is not mandatory.
When set to RTZ - Required When Tax is Zero, users are prevented from posting transactions without recording a tax message when tax rate is zero.
When set to REQ - Required Always, users are prevented from posting transactions without recording a tax message, regardless of the tax rate.
When set to RET - Required When an Extra Tax element is Configured as part of the Tax ID, users are prevented from posting transactions without recording a tax message when the Tax ID includes an extra tax behavior.
When set to REZ - Required when Tax is Zero, or when Extra Tax element is Configured as part of  the Tax ID, users are prevented from posting transactions without recording a tax message when the tax rate is zero or Tax ID includes an extra tax behavior."),
			new CodePairRegistryDataType(new CodeDescriptionPairListProvider(() => TaxMessageMandatoryOptions.CodeList), false, false),
			RegistryOptions.CacheExpensiveDefaultValue,
			FactoryForCountryDefaultValues,
			new TaxMessageIsMandatoryReceivables_RegistryDescriptor())));

		#endregion

		#region Receipt Detail

		public BooleanRegistryItem UseInvoiceExchangeRateWhenChangingReceiptAmount
		{
			get
			{
				return GetItem("UseInvoiceExchangeRateWhenChangingReceiptAmount", delegate
				{
					return new BooleanRegistryItem(
						"UseInvoiceExchangeRateWhenChangingReceiptAmount",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_ReceiptDetail,
						ResString.GetMultilingualString("6E82180A-426B-414b-AA92-AAFF29062A9A", "Use Invoice Exchange Rate when Changing Receipt Amount"),
						ResString.GetMultilingualString("221A67C0-5FD2-4bf3-A3FE-34B86D91F0C2", @"This registry setting is referenced when users click on the 'Change Receipt Amount' button on the matching screen, when using the 'Receipt Detail' button on the AR and AP Receipt screen.
When this registry is set to 'Yes', when users click on the 'Change Receipt Amount' button, {0} will default the 'local amount' and 'foreign amount' according to the invoices and invoice lines that the receipt is being matched with.
{0} will only behave in this way when all invoices and/or invoice lines being matched have the same currency as the receipt transaction.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Credit Card Fee
		#region SuppressResourceStringsCheckRegion

		public CreditCardFeeRegistryItem CreditCardFee
		{
			get
			{
				return GetItem("CreditCardFee", delegate
				{
					return new CreditCardFeeRegistryItem(
						"CreditCardFee",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						(NoResString)"Credit Card Fee",
						(NoResString)"Credit Card Fee.",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
				});
			}
		}

		#endregion
		#endregion

		#region Receivable Authorization Settings

		public AuthorizationModeAndSettingsRegistryItem ReceivableAuthorizationModeAndSettings
		{
			get
			{
				return GetItem(nameof(ReceivableAuthorizationModeAndSettings), () =>
					new AuthorizationModeAndSettingsRegistryItem(
						nameof(ReceivableAuthorizationModeAndSettings),
						Categories.Accounting_ReceivableDefaults_DefaultSettings_CreditNoteInvoiceReversalAuthorisationSetting,
						ResString.GetMultilingualString("37B1C838-DE01-48EA-9DAC-F822D03B136F", "Credit / Adjustment Note Authorization Settings"),
						ResString.GetMultilingualString("398F8E68-EB34-4DAB-931D-BE1BC2725B64", @"Use this registry to control the posting of Receivables Credit Notes and Negative Adjustment Notes by setting 'Authorization' thresholds.
NOTE: When no threshold values are set, authorization of transactions at posting is not required.
Use this registry to define the amount thresholds at which AR Credit Notes and AR Negative Adjustment Notes require authorization before they can be posted. There are six levels of Authorization.
The authorization requirement 'None' allows any user with Credit Note and Adjustment Note posting rights to post transactions up to the threshold amount without need of an authorizing user.
The authorization requirements '1st Level' through to '6th Level' restricts posting or authorization of posting to users flagged as having the corresponding Approval security rights.

By default, a single authorized user needs to approve the posting of a credit note.
Set 'Authorization Mode' to TWO to enforce that two authorized users must review each credit note posting. Both users must have the specified approval level in order to authorize the posting.
Set 'Authorization Mode' to SEQ to enforce that multiple authorized users must review each credit note posting. The credit note must be reviewed by all approval levels, starting from Level 1 approver, then Level 2 and so on. Finally, the user with the specified 'Authorization Requirement' can fully approve the posting. Note that lower level authorizing users are able to reject the credit note posting. However, in all users up to the specified Approval Level must approve the credit note, before it can be posted.

NOTE: When a single approval is sufficient in order to post the credit note, if the creating user doesn't have sufficient approval level, then the 'Security Override Login' authorization is displayed. Another user with the required approval level can approve the credit note posting by supplying their username and password. Otherwise, and in cases where multiple approvers must review the credit note, the creating user can queue an Approval Request. Authorized users can review and approve the requests in the Credit Note Approval module."),
						RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default));
			}
		}

		public AuthorizationModeAndSettingsRegistryItem ReceivableReversalAuthorizationModeAndSettings
		{
			get
			{
				return GetItem(nameof(ReceivableReversalAuthorizationModeAndSettings), () =>
					new AuthorizationModeAndSettingsRegistryItem(
						nameof(ReceivableReversalAuthorizationModeAndSettings),
						Categories.Accounting_ReceivableDefaults_DefaultSettings_CreditNoteInvoiceReversalAuthorisationSetting,
						ResString.GetMultilingualString("48A438E1-E6EE-4B63-8275-3F7B905A118F", "Invoice / Adjustment Note Reversal Authorization Settings"),
						ResString.GetMultilingualString("6434D66F-2982-43F3-BA8F-95F982F000B4", @"Use this registry to control the reversal of Receivables Invoices and Positive Adjustment Notes by setting 'Authorization' thresholds.
NOTE: When no threshold values are set, authorization of transactions at posting is not required.
Use this registry to define the amount thresholds at which AR Credit Notes and AR Negative Adjustment Notes require authorization before they can be posted. There are six levels of Authorization.
The authorization requirement 'None' allows any user with Credit Note and Adjustment Note posting rights to post transactions up to the threshold amount without need of an authorizing user.
The authorization requirements '1st Level' through to '6th Level' restricts posting or authorization of posting to users flagged as having the corresponding Approval security rights.

By default, a single authorized user needs to approve the posting of a credit note.
Set 'Authorization Mode' to TWO to enforce that two authorized users must review each credit note posting. Both users must have the specified approval level in order to authorize the posting.
Set 'Authorization Mode' to SEQ to enforce that multiple authorized users must review each credit note posting. The credit note must be reviewed by all approval levels, starting from Level 1 approver, then Level 2 and so on. Finally, the user with the specified 'Authorization Requirement' can fully approve the posting. Note that lower level authorizing users are able to reject the credit note posting. However, in all users up to the specified Approval Level must approve the credit note, before it can be posted.

NOTE: When a single approval is sufficient in order to post the credit note, if the creating user doesn't have sufficient approval level, then the 'Security Override Login' authorization is displayed. Another user with the required approval level can approve the credit note posting by supplying their username and password. Otherwise, and in cases where multiple approvers must review the credit note, the creating user can queue an Approval Request. Authorized users can review and approve the requests in the Credit Note Approval module."),
						RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default));
			}
		}

		#endregion

		#region Share Sequential Invoice Transaction Numbers

		public ShareSequentialTransactionNumbersRegistryItem ShareSequentialInvoiceTransactionNumbers
		{
			get
			{
				return GetItem("ShareSequentialInvoiceTransactionNumbers", delegate
				{
					return new ShareSequentialTransactionNumbersRegistryItem(
					"ShareSequentialInvoiceTransactionNumbers",
					Categories.Accounting_ReceivableDefaults_DefaultSettings,
					ResString.GetMultilingualString("919be070-5c8a-4c2e-8719-b7a6052b3b40", "Share Sequential Invoice Transaction Numbers"),
					ResString.GetMultilingualString("7c9b2446-c8d2-4028-ab05-528adb3a384d", @"Use this registry to control how transaction numbers are allocated to Receivable Invoice, Credit Note and Adjustment Note transactions.
The default behavior of 'No' assigns transaction numbers from individual 'AR Invoice', 'AR Credit Note' and 'AR Adjustment Note' number sequences.
By default each transaction type (INV, CRD, ADJ) has its own, separate number sequence.  This behavior can be overridden by changing this registry to 'Yes'.
When this registry is set to 'Yes' AR Invoices, AR Credit Notes and AR Adjustment Note transactions will all be assigned a sequential reference number from a single number sequence."),
					RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Invoice Transaction Number Prefix

		public InvoiceTransactionNumberPrefixRegistryItem InvoiceTransactionNumberPrefix
		{
			get
			{
				return GetItem("InvoiceTransactionNumberPrefix", delegate
				{
					InvoiceTransactionNumberPrefixRegistryItem result = new InvoiceTransactionNumberPrefixRegistryItem(
						"InvoiceTransactionNumberPrefix",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("926c9dde-7a85-4d1a-9d3e-23fcdfc4e720", "Invoice Transaction Number Prefix"),
						ResString.GetMultilingualString("f7b6f8e4-9ddd-4a1b-82d5-6f52586a8ab2", "This registry changes the way Accounts Receivable Invoice, Credit Note and Adjustment Note transaction numbers print in receivable documents.  When this registry is overridden the selected prefix will be included as part of the Receivables Invoice, Credit Note or Adjustment Note transaction number. When overridden, the nominated prefix will also be included in all XML exports of Receivables Invoices, Credit Notes and Adjustment Notes."),
						RegistryStorageFlags.Company
						);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					return result;
				});
			}
		}

		#endregion

		#region Accounting Receipt Print Prompting

		public BooleanRegistryItem AccountingReceiptPrintPrompting
		{
			get
			{
				return GetItem("AccountingReceiptPrintPrompting", delegate
				{
					return new BooleanRegistryItem(
					"AccountingReceiptPrintPrompting",
					Categories.Accounting_ReceivableDefaults_DefaultSettings,
					ResString.GetMultilingualString("c6f15b53-2db5-4b47-9380-51baaf922591", "Receipt Print Prompting"),
					ResString.GetMultilingualString("943db2f9-4df4-4aa7-915f-6a2c4da311d6", "When enabled, system will prompt user to print a Receipt document upon AR Receipting."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					true);
				});
			}
		}

		#endregion

		#region Invoice Printing Group by Organization

		public BooleanRegistryItem InvoicePrintingGroupByOrganization
		{
			get
			{
				return GetItem("InvoicePrintingGroupByOrganization", delegate
				{
					return new BooleanRegistryItem(
						"InvoicePrintingGroupByOrganization",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("db03d267-540a-4a9a-ac35-1b7b007d9113", "Invoice Printing Group by Organization"),
						ResString.GetMultilingualString("4b0060d7-3dcd-4cca-9306-ab27c7bec993", @"This registry setting controls the print order for invoices delivered from the 'Print Invoice' module.
When set to 'Yes', invoices will be grouped by organization so that invoices for the same organization will be printed in sequence.
When set to 'No', invoices will not be grouped by organization and will print in the order that they were selected in the 'Invoice Printing' module."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Always Group Invoices Printing For Single Debtor

		public BooleanRegistryItem AlwaysGroupInvoicesPrintingForSingleDebtor
		{
			get
			{
				return GetItem("AlwaysGroupInvoicesPrintingForSingleDebtor", delegate
				{
					return new BooleanRegistryItem(
						"AlwaysGroupInvoicesPrintingForSingleDebtor",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("7a48c262-cc08-47d8-81a0-56306c5639f2", "Always Group Invoices Printing For Single Debtor"),
						ResString.GetMultilingualString("8391deac-327c-492f-980f-271969cf682f", @"When this registry is set to 'Yes' and all invoices selected belongs to a single debtor, the system will deliver all selected invoices in a single file (regardless of the 'Invoice Printing Group by Organization' registry value)."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Stamp Duty Charge Code

		public GuidRegistryItem StampDutyChargeCode
		{
			get
			{
				return GetItem("StampDutyChargeCode", delegate
				{
					GuidRegistryItem item = new GuidRegistryItem(
						"StampDutyChargeCode",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("14481f57-12ff-4e5e-9c49-6c05589141fe", "Stamp Duty Charge Code"),
						ResString.GetMultilingualString("a1481c37-56aa-4be8-87a5-12b0d1b66064", "{0} will use this charge code when posting invoices that attract stamp duty.  The remainder of the Stamp Duty configuration must be setup by {1}.", BrandingFactory.Instance.ProductName, BrandingFactory.Instance.ProductSupportName),
						RegistryStorageFlags.Company,
						Guid.Empty);
					item.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode);
					return item;
				});
			}
		}

		#endregion

		#region Aging Option

		public CodePairRegistryItem AgingOptionReceivables
		{
			get
			{
				return GetItem("AgingOptionReceivables", delegate
				{
					return new CodePairRegistryItem(
						"AgingOptionReceivables",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("4E1984A4-9DDD-4EF7-9A70-6D0EF326DFA4", "Aging Option in Receivables Enquiries"),
						ResString.GetMultilingualString("951B2802-51B6-4462-8CDF-84B9B101FF32", @"Use this registry to set the default Aging Option in Receivables Enquiries. 

a. INV – Transactions will be aged by Invoice Date.
b. DUE – Transactions will be aged by Due Date.
c. PST - Transactions will be aged by Post Date. 

By default, this registry will be set to 'INV'."),
						new CodeDescriptionPairListProvider(() => AccountingConstants.AgingOptions.CodeList),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingConstants.AgingOptions.InvoiceDate);
				});
			}
		}

		#endregion

		#region Transaction Number Option for Collection Batch Export File

		public CodePairRegistryItem TransactionNumberOptionForCollectionBatchExportFile
		{
			get
			{
				return GetItem("TransactionNumberOptionForCollectionBatchExportFile", delegate
				{
					return new CodePairRegistryItem(
						"TransactionNumberOptionForCollectionBatchExportFile",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_CollectionBatchesAndOrders,
						ResString.GetMultilingualString("36FDAAC3-8BC9-4049-8526-B6537E77862D", "Transaction Number Option for Collection Batch Export File"),
						ResString.GetMultilingualString("4A868D64-E27D-42C0-B656-AB67D0A723A8", @"This registry is used to define which Reference Number should be mapped as Invoice Number when RIBA and SEPA files are generated for Collection Order Batches.
When setting the value to ""INV"", the system uses the Transaction Number;
when setting the value to ""CIN"", the system uses the Compliance Number when it is filled, otherwise it uses the Transaction Number."),
						new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList()
							{
								EReportingTransactionNumberOptions.InvoiceNr,
								EReportingTransactionNumberOptions.ComplianceOrInvoiceNr
							}
						),
						RegistryStorageFlags.Company,
						AccountingMasterFilesConstants.TransactionNumberCodes.ComplianceOrInvoiceNr);
				});
			}
		}
		#endregion

		#endregion

		#region Form Configurations
#if DEBUG
		public
#endif
		class ReferenceDisplayRegistryItem : BooleanRegistryItem
		{
			public ReferenceDisplayRegistryItem(string name, MultilingualString category, MultilingualString caption)
				: base(name, category, caption, ResString.GetMultilingualString("61a681ae-5fb3-42c3-83a0-daa7296c33b3", "Shipment/Job Details to display in the AR Invoice."), RegistryStorageFlags.System | RegistryStorageFlags.Company, true)
			{
			}
		}

		#region Forwarding

		#region Consols

		#region Send & Receiving Agents

		public BooleanRegistryItem ConsolsSendAndReceivingAgents
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ConsolsSendAndReceivingAgents", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"ConsolsSendAndReceivingAgents",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_Consols,
						ResString.GetMultilingualString("c7e66fc4-ad11-4489-9d59-a196a814b9c6", "Send & Receiving Agents"));
				});
			}
		}

		#endregion

		#region Carrier & Transport Details

		public BooleanRegistryItem ConsolsCarrierAndTransportDetails
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ConsolsCarrierAndTransportDetails", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"ConsolsCarrierAndTransportDetails",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_Consols,
						ResString.GetMultilingualString("43af0acd-9965-474b-a680-fc6ea0dfc37a", "Carrier & Transport Details"));
				});
			}
		}

		#endregion

		#region Bill, Weight, Volume & Packages

		public BooleanRegistryItem ConsolsBillWeightVolumeAndPackages
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ConsolsBillWeightVolumeAndPackages", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"ConsolsBillWeightVolumeAndPackages",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_Consols,
						ResString.GetMultilingualString("d11706e5-ae18-45cb-b8a1-562450ea3df8", "Bill, Weight, Volume & Packages"));
				});
			}
		}

		#endregion

		#region Load & Discharge Ports

		public BooleanRegistryItem ConsolsLoadAndDischargePorts
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ConsolsLoadAndDischargePorts", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"ConsolsLoadAndDischargePorts",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_Consols,
						ResString.GetMultilingualString("5c55df83-8ca2-4ec7-a4fe-dec2f3d390cf", "Load & Discharge Ports"));
				});
			}
		}

		#endregion

		#region Container Details

		public BooleanRegistryItem ConsolsContainerDetails
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ConsolsContainerDetails", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"ConsolsContainerDetails",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_Consols,
						ResString.GetMultilingualString("ac3e9b04-3ed5-4f4f-ab64-221bd1992593", "Container Details"));
				});
			}
		}

		#endregion

		#endregion

		#region Customs Declarations

		#region Supplier & Importer

		public BooleanRegistryItem CustomsDeclarationsSupplierAndImporter
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CustomsDeclarationsSupplierAndImporter", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"CustomsDeclarationsSupplierAndImporter",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_CustomsDeclarations,
						ResString.GetMultilingualString("1a808219-a7dd-42eb-863a-9e46f8b5516f", "Supplier & Importer"));
				});
			}
		}

		#endregion

		#region Goods Description & Invoice References

		public BooleanRegistryItem CustomsDeclarationsGoodsDescriptionAndInvoiceReferences
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CustomsDeclarationsGoodsDescriptionAndInvoiceReferences", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"CustomsDeclarationsGoodsDescriptionAndInvoiceReferences",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_CustomsDeclarations,
						ResString.GetMultilingualString("18ac80e1-fe68-402b-a35b-8e101d484063", "Goods Description & Invoice References"));
				});
			}
		}

		#endregion

		#region Order Reference, Weight, Volume, Packages

		public BooleanRegistryItem CustomsDeclarationsOrderReferenceWeightVolumePackages
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CustomsDeclarationsOrderReferenceWeightVolumePackages", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"CustomsDeclarationsOrderReferenceWeightVolumePackages",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_CustomsDeclarations,
						ResString.GetMultilingualString("67fbb38f-33e3-4fde-ae21-e609ce302b70", "Order Reference, Weight, Volume, Packages"));
				});
			}
		}

		#endregion

		#region Transport Details & Bill References

		public BooleanRegistryItem CustomsDeclarationsTransportDetailsAndBillReferences
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CustomsDeclarationsTransportDetailsAndBillReferences", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"CustomsDeclarationsTransportDetailsAndBillReferences",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_CustomsDeclarations,
						ResString.GetMultilingualString("040c6e07-f866-47ec-9452-8c1616d1c213", "Transport Details & Bill References"));
				});
			}
		}

		#endregion

		#region Origin & Destination Ports

		public BooleanRegistryItem CustomsDeclarationsOriginAndDestinationPorts
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CustomsDeclarationsOriginAndDestinationPorts", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"CustomsDeclarationsOriginAndDestinationPorts",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_CustomsDeclarations,
						ResString.GetMultilingualString("273d5ece-caff-4893-867a-32933adc6525", "Origin & Destination Ports"));
				});
			}
		}

		#endregion

		#region Container Details

		public BooleanRegistryItem CustomsDeclarationsContainerDetails
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CustomsDeclarationsContainerDetails", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"CustomsDeclarationsContainerDetails",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_Forwarding_CustomsDeclarations,
						ResString.GetMultilingualString("ac3e9b04-3ed5-4f4f-ab64-221bd1992593", "Container Details"));
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region CFS/CTO

		#region Load List

		#region Client Reference & Carrier

		public BooleanRegistryItem LoadListClientReferenceAndCarrier
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CFSCTOClientReferenceAndCarrier", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"CFSCTOClientReferenceAndCarrier",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO_LoadList,
						ResString.GetMultilingualString("306be887-d6b1-43c4-adb2-21f517d895b1", "Client Reference & Carrier"));
				});
			}
		}

		#endregion

		#region Transport Details

		public BooleanRegistryItem LoadListTransportDetails
		{
			get
			{
				return GetItem<BooleanRegistryItem>("LoadListTransportDetails", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"LoadListTransportDetails",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO_LoadList,
						ResString.GetMultilingualString("4b85863b-9454-414e-9811-eb41696ccfa7", "Transport Details"));
				});
			}
		}

		#endregion

		#region Container Details

		public BooleanRegistryItem LoadListContainerDetails
		{
			get
			{
				return GetItem<BooleanRegistryItem>("LoadListContainerDetails", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"LoadListContainerDetails",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO_LoadList,
						ResString.GetMultilingualString("ac3e9b04-3ed5-4f4f-ab64-221bd1992593", "Container Details"));
				});
			}
		}

		#endregion

		#endregion

		#region Shipments & Gatepass

		#region Consignee & Consignor

		public BooleanRegistryItem ShipmentsAndGatepassConsigneeAndConsignor
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShipmentsAndGatepassConsigneeAndConsignor", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"ShipmentsAndGatepassConsigneeAndConsignor",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO_ShipmentsGatepass,
						ResString.GetMultilingualString("609d476d-93eb-4749-b32b-1fc64644581a", "Consignee & Consignor"));
				});
			}
		}

		#endregion

		#region Goods Description & Interim Receipt

		public BooleanRegistryItem ShipmentsAndGatepassGoodsDescriptionAndInterimReceipt
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShipmentsAndGatepassGoodsDescriptionAndInterimReceipt", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"ShipmentsAndGatepassGoodsDescriptionAndInterimReceipt",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO_ShipmentsGatepass,
						ResString.GetMultilingualString("d4fbd518-00de-4110-b2dd-1cad1691f159", "Goods Description & Interim Receipt"));
				});
			}
		}

		#endregion

		#region Transport Details, Weight, Volume, Packages

		public BooleanRegistryItem ShipmentsAndGatepassTransportDetailsWeightVolumePackages
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShipmentsAndGatepassTransportDetailsWeightVolumePackages", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"ShipmentsAndGatepassTransportDetailsWeightVolumePackages",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO_ShipmentsGatepass,
						ResString.GetMultilingualString("48494128-3865-4c25-aa66-6aee7824f9f0", "Transport Details, Weight, Volume, Packages"));
				});
			}
		}

		#endregion

		#region Origin & Destination Ports

		public BooleanRegistryItem ShipmentsAndGatepassOriginAndDestinationPorts
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShipmentsAndGatepassOriginAndDestinationPorts", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"ShipmentsAndGatepassOriginAndDestinationPorts",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO_ShipmentsGatepass,
						ResString.GetMultilingualString("273d5ece-caff-4893-867a-32933adc6525", "Origin & Destination Ports"));
				});
			}
		}

		#endregion

		#region Bill & Client References

		public BooleanRegistryItem ShipmentsAndGatepassBillAndClientReferences
		{
			get
			{
				return GetItem<BooleanRegistryItem>("ShipmentsAndGatepassBillAndClientReferences", delegate
				{
					return new ReferenceDisplayRegistryItem(
						"ShipmentsAndGatepassBillAndClientReferences",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_ReferenceDisplayItems_CFSCTO_ShipmentsGatepass,
						ResString.GetMultilingualString("b62f5d64-c5bb-4d79-bca3-f5ca3491dbbf", "Bill & Client References"));
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Tax Transaction

		#region Print Tax Detail - PER RII SLX VAT

		public BooleanRegistryItem PrintTaxDetailPERRIISLX
		{
			get
			{
				return GetItem("PrintTaxDetailPERRIISLX", delegate
				{
					return new BooleanRegistryItem(
						"PrintTaxDetailPERRIISLX",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_TaxTransaction,
						ResString.GetMultilingualString("c2bf2bC8-4a0f-41c6-9171-0cdce734f587", "Print Tax Detail - PER RII SLX VAT"),
						ResString.GetMultilingualString("9334494e-33ec-4668-a027-18ca58746c60", @"This registry is relevant when Perceptions (PER), Value Added Tax (VAT), Retention In Invoice (RII) or Sales Tax (SLX) Tax Transactions are recorded against Invoice or Credit Note transactions.
By default this registry is set to Yes.
When set to Yes, the details of each PER, VAT, RII or SLX tax transaction recorded will be included in the document.
When overridden and set to No, details will not be included."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Print Tax Detail - TRX

		public BooleanRegistryItem PrintTaxDetailTRX
		{
			get
			{
				return GetItem("PrintTaxDetailTRX", delegate
				{
					return new BooleanRegistryItem(
						"PrintTaxDetailTRX",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_TaxTransaction,
						ResString.GetMultilingualString("3d8c1b8f-ec74-4ff0-9699-e75d7fdd5cdc", "Print Tax Detail - TRX"),
						ResString.GetMultilingualString("91b3643d-2eb5-4b2d-9d1e-e12c1ea6b0fe", @"This registry is relevant when Turnover Tax (TRX) Tax Transactions are recorded against Invoice or Credit Note transactions.
By default this registry is set to Yes.
When set to Yes, the details of each TRX tax transaction recorded will be included in the document.
When overridden and set to No, details will not be included."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Print Tax Detail - SPR

		public BooleanRegistryItem PrintTaxDetailSPR
		{
			get
			{
				return GetItem("PrintTaxDetailSPR", delegate
				{
					return new BooleanRegistryItem(
						"PrintTaxDetailSPR",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_TaxTransaction,
						ResString.GetMultilingualString("c229c013-95f7-48f8-ab5e-565ee751def7", "Print Tax Detail - SPR"),
						ResString.GetMultilingualString("ebbdb282-87aa-4c4d-90b1-d937cfe71440", @"This registry is relevant when Standard Payments Basis Withholding (SPR) Tax Transactions are recorded against Invoice or Credit Note transactions.
By default this registry is set to No and details of each Notional (or Realized) Withholding tax transaction recorded against a transaction are omitted from the document. 
When overridden and set to Yes, details of each SPR tax transaction will be included."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region Invoice Trading Terms

		public ImageRegistryItem InvoiceTradingTerms
		{
			get
			{
				return GetItem("InvoiceTradingTerms", delegate
				{
					return new ImageRegistryItem(
						"InvoiceTradingTerms",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						ResString.GetMultilingualString("f4f28f92-9321-450a-9777-30cdb9f89116", "Invoice Trading Terms"),
						ResString.GetMultilingualString("702c959d-4e72-4069-8107-4f99fb065b31", "These are your Business Trading Terms and Conditions. When an image of your Trading Terms is loaded here, it will print on the reverse of your AR Invoice. Trading Terms can also be included on copies by editing the 'Invoice Copies' Registry item."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Invoice Copies

		public InvoiceCopiesRegistryItem InvoiceCopies
		{
			get
			{
				return GetItem("InvoiceCopies", delegate
				{
					var defaultValue = new InvoiceCopyCollection();
					var copy = defaultValue.AddNew();
					copy.Order = 1;
					copy.IsOriginal = true;
					copy.DeliveryMethod = nameof(PrintCopyType.ALL);
					copy.IncludeTradingTerms = true;
					return new InvoiceCopiesRegistryItem(
						"InvoiceCopies",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						ResString.GetMultilingualString("b1f0a0c3-fbc2-41ee-9bf1-68f28bb44cdc", "Invoice Copies"),
						ResString.GetMultilingualString("49642d61-519d-443f-814f-4e4135bb29e1", "Add a line for each copy of the AR Invoice you want to generate. The name of the copy will appear on the document, and the copy will only be generated for the chosen delivery method (either print, fax, or email). Tick the 'Include Trading Terms' checkbox if you would like the trading terms printed on the back of the copy."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValue);
				});
			}
		}

		#endregion

		#region Invoice Copy Printing

		public BooleanRegistryItem ShouldInvoiceShowCopyWhenPrintedSubsequentTimes
		{
			get
			{
				return GetItem("ShouldInvoiceShowCopyWhenPrintedSubsequentTimes", delegate
				{
					return new BooleanRegistryItem(
						"ShouldInvoiceShowCopyWhenPrintedSubsequentTimes",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						ResString.GetMultilingualString("3d91d3a3-7f32-44c6-89e5-46af5664e65c", "Show 'Copy' on subsequently printed invoices"),
						ResString.GetMultilingualString("1eb2dcd0-b920-4182-a8de-2047ea52283f", "Select 'Yes' to make the invoice show the word 'Copy' when the invoice is delivered subsequent times (i.e. after the first time)"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Invoice Printing Option
		RegistryOptions HidePrintGovtTaxInvoiceMenuForChina
		{
			get
			{
				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China ? RegistryOptions.IsHidden : RegistryOptions.Default;
			}
		}

		public CodePairRegistryItem InvoicePrintingOption
		{
			get
			{
				return GetItem("InvoicePrintingOption", delegate
				{
					var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var lookUpList = new CodeDescriptionPairList();

						lookUpList.AddPair("TAX", ResString.GetMultilingualString("80729203-beb0-42e5-beba-f33677430f37", "Government Tax Invoice"));
						lookUpList.AddPair("ENT", ResString.GetMultilingualString("8aa81db4-f951-44c9-affe-365fa8dfc271", "{0} Invoice", BrandingFactory.Instance.ProductName));
						lookUpList.AddPair("ALL", ResString.GetMultilingualString("863b69c9-a353-422c-b6b7-66370bcc6feb", "Both Government Tax Invoice and {0} Invoice", BrandingFactory.Instance.ProductName));
						return lookUpList;
					});
					return new CodePairRegistryItem(
						"InvoicePrintingOption",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_LocalInvoice,
						ResString.GetMultilingualString("98d6b5d6-7a8c-42fb-ad52-1277ea66924d", "Invoice Printing Option"),
						ResString.GetMultilingualString("10f85ee3-0c1c-4935-ac89-6d84131c661f", "This specifies the invoice document to be printed on posting of Local Billing charges in Job Invoicing screen."),
						lookUpListProvider,
						RegistryStorageFlags.Company,
						HidePrintGovtTaxInvoiceMenuForChina,
						GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China ? "ENT" : "TAX");
				});
			}
		}
		#endregion

		#region Electronic Payments

		#region Enable Electronic Payments

		public BooleanRegistryItem EnableElectronicPayments
		{
			get
			{
				return GetItem("EnableElectronicPayments", delegate
				{
					return new BooleanRegistryItem(
						 "EnableElectronicPayments",
						 Categories.Accounting_ReceivableDefaults_DefaultSettings_ElectronicPaymentsDefaults,
						 ResString.GetMultilingualString("b147b7f8-eea0-40e8-8ffe-9e7108712d82", "Enable Electronic Payment option"),
						 ResString.GetMultilingualString("74bd3e05-c67f-452c-bacf-c2467cf8b29d", "Tick this to 'Yes' to enable Electronic Payments option."),
						 RegistryStorageFlags.Company,
						 false);
				});
			}
		}

		#endregion

		#region ElectronicPaymentBillerCode
		public StringRegistryItem ElectronicPaymentBillerCode
		{
			get
			{
				return GetItem("ElectronicPaymentBillerCode", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"ElectronicPaymentBillerCode",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_ElectronicPaymentsDefaults,
						ResString.GetMultilingualString("5e970ffd-8380-47f7-8a25-70ab00cd5347", "Electronic Payment Biller Code"),
						ResString.GetMultilingualString("f0d44d16-14d7-4ed7-b6d7-8bd26aff0534", "Biller Code used on Electronic Payments."),
						RegistryStorageFlags.Company);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					return result;
				});
			}
		}
		#endregion

		#region ElectronicPaymentTerms
		public MultilingualStringRegistryItem ElectronicPaymentTerms
		{
			get
			{
				return GetItem("ElectronicPaymentTerms", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"ElectronicPaymentTerms",
						Categories.Accounting_ReceivableDefaults_DefaultSettings_ElectronicPaymentsDefaults,
						ResString.GetMultilingualString("530fe94e-279a-4c83-b0fc-8c03387a7394", "Electronic Payment Terms"),
						ResString.GetMultilingualString("20ebf7a7-dd17-460e-8f97-2251d43bd75d", "Terms used on Electronic Payments."),
						RegistryStorageFlags.Company,
						ResString.GetMultilingualString("df12fa67-df1d-4673-8333-5b144bc58f81", "Contact your bank or financial institution to make this payment from your cheque, savings, debit, credit card or transaction account."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}
		#endregion

		#region Portugal Certification RSA private key
		public StringRegistryItem PortugalCertificationKey
		{
			get
			{
				return GetItem("PortugalCertificationKey", delegate
				{
					StringRegistryItem item = new StringRegistryItem(
						"PortugalCertificationKey",
						null, null, null,
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.IsReadOnly,
						"");

					item.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return item;
				});
			}
		}

		#endregion

		#endregion

		#region Show Printing Operator's details on Invoice

		public BooleanRegistryItem ShowOperatorsNameOnInvoice
		{
			get
			{
				return GetItem("ShowOperatorsNameOnInvoice", delegate
				{
					return new BooleanRegistryItem(
						 "ShowOperatorsNameOnInvoice",
						 Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						 ResString.GetMultilingualString("36fb938f-a683-412c-bea5-ef882b79db83", "Show Printing Operator's Name on Invoice"),
						 ResString.GetMultilingualString("56aca020-6260-4277-8d05-4d57fb84fa41", "Tick this item as 'yes' when the name of the printing operator is required to be displayed in the footer of the invoice."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 false);
				});
			}
		}

		public BooleanRegistryItem ShowOperatorsSignature
		{
			get
			{
				return GetItem("ShowOperatorsSignature", delegate
				{
					return new BooleanRegistryItem(
						"ShowOperatorsSignature",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						ResString.GetMultilingualString("996dce59-a0cb-42b8-b274-e1c9d1508f11", "Show Printing Operator's Signature on Invoice"),
						ResString.GetMultilingualString("fd5c5324-028c-4919-9e14-deb42e44eb7e", @"Tick this item as 'yes' when the signature graphic recorded against the printing operator's (creating user) staff record is to be displayed in the footer of the invoice.
This registry item is only applicable when the 'Show Printing Operator's Name on Invoice' registry item is also flagged 'Yes'"),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Show Full Listing Of Container Numbers On Separate Page

		public BooleanRegistryItem ShowFullListingOfContainerNumbersOnSeparatePage
		{
			get
			{
				return GetItem("ShowFullListingOfContainerNumbersOnSeparatePage", delegate
				{
					return new BooleanRegistryItem(
						"ShowFullListingOfContainerNumbersOnSeparatePage",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						ResString.GetMultilingualString("7c71c416-9b29-4228-bf53-5d869f1f0b87", "Show Full Listing Of Container Numbers On Separate Page"),
						ResString.GetMultilingualString("c18dab39-76fd-44e4-946a-f2d09d9a298d", "Tick this item as 'yes' when the full listing of container numbers and types is required to be displayed in the separate page."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						false);
				});
			}
		}
		#endregion

		#region Third Party E-Invoice Doc Type

		public CodePairRegistryItem ThirdPartyEInvoiceDocType =>
			 GetItem(
				 "ThirdPartyEInvoiceDocType",
				 () => new CodePairRegistryItem(
					 new CountrySpecificDefaultValueRegistryItemImpl<string>(
						 "ThirdPartyEInvoiceDocType",
						 Categories.Documents,
						 ResString.GetMultilingualString("40F63570-854B-4C8F-A6D5-92ADEBEF238F", @"This registry is used to specify the default document type of e-invoice documents obtained from third party provider.
Changing the value of this registry will only affect e-invoice documents from a third party provider that is automatically saved in eDocs through a PDF request API.
The behavior of adding eDocs using other method will not be affected by this registry."),
						 new CodePairRegistryDataType(new CodeDescriptionPairListProvider(() => DocTypeListProvider()), false, true),
						 RegistryOptions.CacheExpensiveDefaultValue | RegistryOptions.IsOnlyForSupport,
						 FactoryForCountryDefaultValues,
						 new ThirdPartyEInvoiceDocType_RegistryDescriptor())));

		CodeDescriptionPairList DocTypeListProvider()
		{
			var masterFactory = new DocumentScanning.Business.DocumentFactoryProvider().GetFactory(FactoryForCountryDefaultValues);
			var docTypeCategoryQuery = new DocTypeCategoryQuery(masterFactory, new ZString(Constants.ReferenceTypes.Accounting));

			return DocumentScanning.Business.DocScanningHelper.GetCategoryDocTypesFromJobType(docTypeCategoryQuery, masterFactory, true);
		}

		#endregion

		#region Show Local Currency Equivalent Totals on Account Receivable Invoice in Foreign Currency

		public BooleanRegistryItem ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency
		{
			get
			{
				return GetItem("ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency", () =>
				new BooleanRegistryItem(
					new CountrySpecificDefaultValueRegistryItemImpl<bool>(
					"ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency",
					Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
					ResString.GetMultilingualString("8cb520d8-f911-4b5d-9a98-c611b54da886", "Tick this item as 'yes' when the Subtotal, Tax Total and Grand Total in local currency are required by law to be displayed in the Account Receivable Invoice."),
					RegistryDataTypes.BoolType,
					RegistryOptions.CacheExpensiveDefaultValue,
					FactoryForCountryDefaultValues,
					new ShowLocalCurrencyEquivalentTotalsOnARInvoiceInOSCurrency_RegistryDescriptor())));
			}
		}

		#endregion

		public BooleanRegistryItem DisallowPostingTransactionWithEmptyComplianceSubtype
		{
			get
			{
				return GetItem("DisallowPostingTransactionWithEmptyComplianceSubtype", delegate
				{
					return new BooleanRegistryItem(
						new CountryEnabledBooleanRegistryItemImpl(
							"DisallowPostingTransactionWithEmptyComplianceSubtype",
							Categories.Accounting_GovernmentComplianceInvoiceDocument,
							ResString.GetMultilingualString("736cd481-ba05-43f1-ae24-15ded6efbecd", "Disallow posting transactions with empty compliance subtype"),
							ResString.GetMultilingualString("cdbd0279-6f6d-4bef-9c30-8700ce61f19e", "Tick this item as 'yes' when you want to disallow posting transactions with empty compliance subtype."),
							RegistryStorageFlags.Company));
				});
			}
		}

		#region Show Printing Operator's Work Phone Number on Invoice

		public BooleanRegistryItem ShowOperatorsWorkPhoneNumberOnInvoice
		{
			get
			{
				return GetItem("ShowOperatorsWorkPhoneNumberOnInvoice", delegate
				{
					return new BooleanRegistryItem(
						"ShowOperatorsWorkPhoneNumberOnInvoice",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						ResString.GetMultilingualString("d46c1dd8-57f1-4b60-9f60-a6d7c0a81341", "Show Printing Operator's Work Phone Number on Invoice"),
						ResString.GetMultilingualString("368688e0-a0f1-460d-a205-ba0547bcb434", "Tick this item as 'yes' when the name of the printing operator is required to be displayed in the footer of the invoice. This registry item is only applicable when the 'Show Printing Operator's Name on Invoice' registry item is flagged as 'Yes'"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Display Invoice Totals by Tax Rate

		public BooleanRegistryItem DisplayInvoiceTotalsbyTaxRate
		{
			get
			{
				return GetItem("DisplayInvoiceTotalsbyTaxRate", delegate
				{
					return new BooleanRegistryItem(
						"DisplayInvoiceTotalsbyTaxRate",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						ResString.GetMultilingualString("dae7d58e-4344-4401-96ed-c5f3319975c1", "Display Invoice Totals by Tax Rate"),
						ResString.GetMultilingualString("3ee271b8-64b7-424c-9276-e55df75d99ec", @"This registry is only relevant to Companies that issue Tax Invoices and also require tax invoices to include Totals by Tax Rate in the invoice document.  By default this registry is not enabled.
When this registry is overridden and set to Yes, an 'Invoice Totals by Tax Rate' section is enabled for the Invoice.  
When overridden and set to yes, an additional sub total section can be displayed in the invoice."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Display Not Yet Outstanding Amount On AR Statement Documents

		public BooleanRegistryItem DisplayNotYetOutstandingAmountOnARStatementDocuments
		{
			get
			{
				return GetItem("DisplayNotYetOutstandingAmountOnARStatementDocuments", delegate
				{
					return new BooleanRegistryItem(
						"DisplayNotYetOutstandingAmountOnARStatementDocuments",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_StatementDefault,
						ResString.GetMultilingualString("9570264b-14f5-4888-ac9d-c155e7edacd3", "Display Sub total of Current Items Not Overdue for Payment"),
						ResString.GetMultilingualString("3e5c4c06-6bdb-482c-be9c-76c6a96fa376", @"When this registry is set to 'NO'  the Receivables Statement of Account will only include a subtotal identifying the value of Overdue items listed in the statement. 

When this registry is set to 'YES' the Receivables Statement of Account will include both a subtotal identifying the value of Overdue items listed in the statement, and a sub total of Current items not yet past due."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region Payable Defaults

		#region Default Settings

		#region Tax Message Is Mandatory

		public CodePairRegistryItem TaxMessageIsMandatoryPayables => GetItem("TaxMessageIsMandatoryPayables",
			() => new CodePairRegistryItem(new CountrySpecificDefaultValueRegistryItemImpl<string>(
			"TaxMessageIsMandatoryPayables",
			Categories.Accounting_PayableDefaults_DefaultSettings,
			ResString.GetMultilingualString("E5D37F8C-032D-4F93-B95E-5621B3AF2BDF", @"This registry allows you to enforce that a tax message must be recorded on payable transactions and cash book direct payment transactions. 

When set to NOT - Not Required, then Invoice Tax Message is not mandatory.
When set to RTZ - Required When Tax is Zero, users are prevented from posting transactions without recording a tax message when tax rate is zero.
When set to REQ - Required Always, users are prevented from posting transactions without recording a tax message, regardless of the tax rate.
When set to RET - Required When an Extra Tax element is Configured as part of the Tax ID, users are prevented from posting transactions without recording a tax message when the Tax ID includes an extra tax behavior.
When set to REZ - Required when Tax is Zero, or when Extra Tax element is Configured as part of  the Tax ID, users are prevented from posting transactions without recording a tax message when the tax rate is zero or Tax ID includes an extra tax behavior."),
			new CodePairRegistryDataType(new CodeDescriptionPairListProvider(() => TaxMessageMandatoryOptions.CodeList), false, false),
			RegistryOptions.CacheExpensiveDefaultValue,
			FactoryForCountryDefaultValues,
			new TaxMessageIsMandatoryPayables_RegistryDescriptor())));

		#endregion

		#region Hot Cheque

		#region Required Cheque Date

		public BooleanRegistryItem AccountingRequiredChequeDate
		{
			get
			{
				return GetItem("AccountingRequiredChequeDate", delegate
				{
					return new BooleanRegistryItem(
					"AccountingRequiredChequeDate",
					Categories.Accounting_PayableDefaults_DefaultSettings_HotCheck_HotCheckRequiredFields,
					ResString.GetMultilingualString("decc4fe8-f2e3-4742-90d7-022a369a3aea", "Required Check Date"),
					ResString.GetMultilingualString("ebe4d6a3-16b7-4a60-9d3e-3854c8e4fe6d", "This registry setting controls whether the Check Date is required on creating a Hot Check.\r\nChoose 'Yes' (default) to make it mandatory to enter the Check Date.\r\nChoose 'No' to make it optional."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					true);
				});
			}
		}

		#endregion

		#region Required Job Number

		public BooleanRegistryItem AccountingRequiredJobNumber
		{
			get
			{
				return GetItem("AccountingRequiredJobNumber", delegate
				{
					return new BooleanRegistryItem(
					"AccountingRequiredJobNumber",
					Categories.Accounting_PayableDefaults_DefaultSettings_HotCheck_HotCheckRequiredFields,
					ResString.GetMultilingualString("9420eda6-4d24-4509-826b-982a353db85c", "Required Job Number"),
					ResString.GetMultilingualString("40427aa1-8c7e-4151-8a96-d82fc92707f6", "This registry setting controls whether Job Number is required on creating a Hot Check.\r\nChoose 'Yes' to make it mandatory to enter the Job Number.\r\nChoose 'No' (default) to make it optional."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					false);
				});
			}
		}

		#endregion

		#region Required Master Bill No

		public BooleanRegistryItem AccountingRequiredMasterBillNo
		{
			get
			{
				return GetItem("AccountingRequiredMasterBillNo", delegate
				{
					return new BooleanRegistryItem(
					"AccountingRequiredMasterBillNo",
					Categories.Accounting_PayableDefaults_DefaultSettings_HotCheck_HotCheckRequiredFields,
					ResString.GetMultilingualString("16311084-4670-4d56-84ce-2e881a9bc061", "Required Master Bill No"),
					ResString.GetMultilingualString("fd84f211-f1c0-4b11-8a45-429f36b5a776", "This registry setting controls whether the Master Bill Number is required on creating a Hot Check.\r\nChoose 'Yes' to make it mandatory to enter the Master Bill Number.\r\nChoose 'No' (default) to make it optional."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					false);
				});
			}
		}

		#endregion

		#region Required House Bill No

		public BooleanRegistryItem AccountingRequiredHouseBillNo
		{
			get
			{
				return GetItem("AccountingRequiredHouseBillNo", delegate
				{
					return new BooleanRegistryItem(
					"AccountingRequiredHouseBillNo",
					Categories.Accounting_PayableDefaults_DefaultSettings_HotCheck_HotCheckRequiredFields,
					ResString.GetMultilingualString("8cb45a06-c1db-43d5-b179-07055bde9610", "Required House Bill No"),
					ResString.GetMultilingualString("8ea36466-60e8-4fc7-b90b-f25acca31253", "This registry setting controls whether the House Bill Number is required on creating a Hot Check.\r\nChoose 'Yes' to make it mandatory to enter the House Bill Number.\r\nChoose 'No' (default) to make it optional."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					false);
				});
			}
		}

		#endregion

		#region Required Description

		public BooleanRegistryItem AccountingRequiredDescription
		{
			get
			{
				return GetItem("AccountingRequiredDescription", delegate
				{
					return new BooleanRegistryItem(
					"AccountingRequiredDescription",
					Categories.Accounting_PayableDefaults_DefaultSettings_HotCheck_HotCheckRequiredFields,
					ResString.GetMultilingualString("7328c03b-17a7-4a36-9f14-454e90963a32", "Required Description"),
					ResString.GetMultilingualString("36e6db96-cad2-46c7-972f-a6cd8ae66860", "This registry setting controls whether the Description is required on creating a Hot Check.\r\nChoose 'Yes' (default) to make it mandatory to enter the Description.\r\nChoose 'No' to make it optional."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					true);
				});
			}
		}

		#endregion

		#region Required Staff

		public BooleanRegistryItem AccountingRequiredStaff
		{
			get
			{
				return GetItem("AccountingRequiredStaff", delegate
				{
					return new BooleanRegistryItem(
					"AccountingRequiredStaff",
					Categories.Accounting_PayableDefaults_DefaultSettings_HotCheck_HotCheckRequiredFields,
					ResString.GetMultilingualString("7d5224dc-7045-4d0a-9f28-ef775b1d38f4", "Required Staff"),
					ResString.GetMultilingualString("43dd2deb-da6b-4186-b19c-47628f26b3e1", "This registry setting controls whether the Staff is required on creating a Hot Check.\r\nChoose 'Yes' (default) to make it mandatory to enter the Staff code.\r\nChoose 'No' to make it optional."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					true);
				});
			}
		}

		#endregion

		#region Allow User To Enter Maximum Amount

		public BooleanRegistryItem AccountingAllowUserToEnterMaximumAmount
		{
			get
			{
				return GetItem("AccountingAllowUserToEnterMaximumAmount", delegate
				{
					return new BooleanRegistryItem(
					"AccountingAllowUserToEnterMaximumAmount",
					Categories.Accounting_PayableDefaults_DefaultSettings_HotCheck,
					ResString.GetMultilingualString("9f9b56db-6b29-458d-8694-cdc3797499cf", "Allow User To Enter Maximum Amount"),
					ResString.GetMultilingualString("dafbebc6-f3ce-4fea-bb71-8cc92ab35aad", "This registry item controls the entering of Hot Check Amount.\r\nSelect 'Yes' to allow user to enter 'Actual Amount' or 'Maximum Amount'.\r\nSelect 'No' to allow user to enter 'Actual Amount' only."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					true);
				});
			}
		}

		#endregion

		#endregion

		#region Payment Processing

		public BooleanRegistryItem AllowEditCheckNumberBeforePosting
		{
			get
			{
				return GetItem("AllowEditCheckNumberBeforePosting",
					() => new BooleanRegistryItem(
						"AllowEditCheckNumberBeforePosting",
						Categories.Accounting_PayableDefaults_DefaultSettings_PaymentProcessing,
						ResString.GetMultilingualString("9488fe0c-4e03-4e80-a671-f4ac592b10d6", "Allow users to modify cheque number before posting a payment"),
													ResString.GetMultilingualString("a10a5cd7-52de-46e5-9f10-2c8e3d0e0cc9", @"This registry allows override of defaulting cheque number/s. When set to Yes and either 'Allocate Cheque No.' or 'Allocate Cheque No. and Post' is selected, a popup displays allowing override of first cheque number. When set to No, the next available cheque number/s are automatically allocated."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false));
			}
		}

		public PaymentAuthorisationSettingsRegistryItem PayableAuthorizationSettings
		{
			get
			{
				return GetItem("PayableAuthorizationSettings", delegate
				{
					return new PaymentAuthorisationSettingsRegistryItem(
						"PayableAuthorizationSettings",
						Categories.Accounting_PayableDefaults_DefaultSettings_PaymentProcessing,
						ResString.GetMultilingualString("ce33f441-0664-4c6a-9128-abeb3ca2d99f", "Authorization Settings"),
						ResString.GetMultilingualString("ed4f7bd9-f330-42ac-a778-b330c2d821b8", "This registry item allows you to specify the authorization required to fully approve an unapproved payment.\r\nYou can set up the authorization required based on the local value of the payment. When no options are set, no approval is required regardless of payment value.\r\nThe system will allow you to specify required authorization for different ranges. You must specify at least one \"Up to\" line and only one \"Above\" line."),
						RegistryStorageFlags.Company);
				});
			}
		}

		public GuidRegistryItem PaymentApprovalsNotifyGroup
		{
			get
			{
				return GetItem("PaymentApprovalsNotifyGroup", delegate
				{
					var result = new GuidRegistryItem("PaymentApprovalsNotifyGroup",
						Categories.Accounting_PayableDefaults_DefaultSettings_PaymentProcessing,
						ResString.GetMultilingualString("c34accc6-b204-407b-8451-14316a4a8b4b", "Payment Approvals Notify Group"),
						ResString.GetMultilingualString("e9fe745d-2a9d-41b2-81db-73636ea718eb", @"This registry allows you to nominate a user group who will receive notifications about outstanding AR and AP payment approval requests.

A regular email will summarize the payment approval requests that are still in AWA - Awaiting Approval status. By default, the email will be sent out daily. To change the frequency of the email, please navigate to Maintain > System > Service Tasks and locate the following task:
UPA - Payment Approvals Notification Email. You can then edit the Recurrence for the scheduled email.

Payment Approval requests can be approved or rejected in the AR and AP Payment Processing modules."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public ExchangeRateToleranceRegistryItem ExchangeRateTolerance
		{
			get
			{
				return GetItem("ExchangeRateTolerance", () =>
				{
					var defaultConfiguration = new ExchangeRateToleranceConfiguration();
					defaultConfiguration.ExchangeRateToleranceCollection.Add(Business.ExchangeRateTolerance.GetDefaultExchangeRateTolerance());
					return new ExchangeRateToleranceRegistryItem("ExchangeRateTolerance",
						Categories.Accounting_PayableDefaults_DefaultSettings_PaymentProcessing,
						ResString.GetMultilingualString("6A9600D4-E6D4-486F-9948-0EFD62E32456", "Exchange Rate Tolerance"),
						ResString.GetMultilingualString("3696CA65-40C9-4167-BCB9-B3A690F503DC", @"This registry controls the ""Exchange Rate Tolerance"" setup of foreign currencies in Payment Approvals.

In a payment approval, when a new exchange rate is entered - it is considered to be;
i) ""Un-favorable"" when for the same overseas ""Payment Amount"" - the ""Local Amount"" to be paid increases.
ii) ""Favorable"" when for the same overseas ""Payment Amount"" - the ""Local Amount"" to be paid decreases.

When a new un-favorable exchange rate is entered, ""Exchange Rate Tolerance"" represents the maximum allowed percentage increase to the ""Local Amount"" paid (for the same foreign ""Payment Amount"") 
on the payment approval before its approval status is reset for re-approval.

Exchange Rate Tolerances can only be set as a positive value between 0-100

By default, the registry applies a ""0%"" Exchange Rate Tolerance to ALL currencies – but currency specific tolerance setups will take precedence"),
						(RegistryStorageFlags.System | RegistryStorageFlags.Company),
						defaultConfiguration);
				});
			}
		}

		#endregion

		#region Share Sequential Invoice Reference Numbers

		public ShareSequentialReferenceNumbersRegistryItem ShareSequentialInvoiceReferenceNumbers
		{
			get
			{
				return GetItem("ShareSequentialInvoiceReferenceNumbers", delegate
				{
					return new ShareSequentialReferenceNumbersRegistryItem(
					"ShareSequentialInvoiceReferenceNumbers",
					Categories.Accounting_PayableDefaults_DefaultSettings,
					ResString.GetMultilingualString("69f31a94-f2ab-407e-9cf7-a02d011beaae", "Share Sequential Invoice Reference Numbers"),
					ResString.GetMultilingualString("27471aca-3016-41ae-bc6d-4e4cd3e56dce", @"Use this registry to control how sequential transaction reference numbers are allocated to Payable Invoice, Credit Note and Adjustment Note transactions.
The default behavior of 'No' assigns reference numbers from individual 'AP Invoice', 'AP Credit Note' and 'AP Adjustment Note' number sequences.  By default each transaction type (INV, CRD, ADJ) has its own, separate number sequence.  This behavior can be overridden by changing this registry to 'Yes'.
When this registry is set to 'Yes' AP Invoices, AP Credit Notes and AP Adjustment Note transactions will all be assigned a sequential reference number from a single number sequence."),
					RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Unapproved Invoices

		public PaymentTwelveLevelAuthorisationSettingsRegistryItem UnapprovedInvoicesAuthorizationSettings
		{
			get
			{
				return GetItem("UnapprovedInvoicesAuthorizationSettings", delegate
				{
					return new PaymentTwelveLevelAuthorisationSettingsRegistryItem(
						"UnapprovedInvoicesAuthorizationSettings",
						Categories.Accounting_PayableDefaults_DefaultSettings_UnapprovedInvoices,
						ResString.GetMultilingualString("b65970ee-c53a-47cf-8c5e-6736f0a36770", "Unapproved Invoices Authorization Settings"),
						ResString.GetMultilingualString("d2e02ceb-6a7f-4cab-a3dd-fa2e1f1bc9ce", @"This registry allows you to specify the authorization required to fully approve an Unapproved Invoice.
You can set up the authorization required based on the local value of the payment. When no options are set, users with security rights to posting AP Invoices, can post invoices of any value.

When enabled, users without sufficient approval level are prevented from posting AP Invoice.
Another user can approve the invoice on the Security Override prompt during posting. Alternatively, user can queue an approval request. Authorized users can review the Unapproved Invoices in the Invoice Approval module."),
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Cost Variance Approval

		public CostVarianceApprovalRegistryItem CostVarianceApproval
		{
			get
			{
				return GetItem("CostVarianceApproval", delegate
				{
					return new CostVarianceApprovalRegistryItem(
						"CostVarianceApproval",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("1e6e6721-dede-46fb-b086-4b737da2e53d", "Cost Variance Style and Approval Thresholds"),
						ResString.GetMultilingualString("DBBE93D3-52A6-484A-94FE-069DFB2CE29F", @"This registry configuration affects the posting of AP Invoice transactions containing job related charges.
When enabled, the posting of AP Invoices will be restricted / require authorization when the job related costs within an invoice are different to the accruals actually provided for on a job.

In configuring this registry levels of 'acceptable variance' can be defined.  
These variance levels identify the amount and/or percentage by which a cost can exceed the original accrual before approval is required on posting.  When the variance of a job/charge exceeds the defined variance thresholds approval of the costs exceeding accruals will be required before an invoice can be posted.

When posting a job related cost, variance can be calculated against the total value of accruals on the job being used when posting the cost; or the total value of accruals for a specific creditor on the job being used when posting the cost; or the total value of accruals by charge code being reversed on the job; or the value of specific accruals being imported to the invoice.

Optionally, the variance of the invoice total to the sum of all accruals being reversed by the invoice can be calculated and an appropriate Approval Level applied if the variance is above the threshold amount specified."),
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Cost Variance No Approval Required

		public BooleanRegistryItem CostVarianceNoApprovalRequired
		{
			get
			{
				return GetItem("CostVarianceNoApprovalRequired", delegate
				{
					var item = new BooleanRegistryItem(
						"CostVarianceNoApprovalRequired",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("C4A1C94E-2FB4-4D65-80B0-BB4AB70DCD39", "Cost Variance - No approval required when charge is imported and both amount and currency matches"),
						ResString.GetMultilingualString("810C97F2-71BC-494E-8A76-421C1C1505EA", @"When the ""Cost Variance Style And Approval Thresholds"" registry is set to 'CJB', 'CCH' or 'CJR' AND this registry is set to 'Yes', no approval will be required for charges that fulfilled the below conditions.
Further, if the ""Cost Variance Style And Approval Thresholds registry > Automatically tick cost as Final when they fall within the 'None' approval threshold."" setting is enabled, the Final flag will be ticked.

1. The charge is imported.
2. The ACR and CST amount and currency is the same.

Example 1: No approval will be required regardless of variance in local amount.
ACR is USD 800 (Local Amount 1000).
CST is USD 800 (Local Amount 1100 due to exchange rate movement).

Example 2: As the OS amount does not match, cost variance check will be enforced and may subject to approval depending on ""Cost Variance Style and Approval Threshold"" settings.
ACR is USD 800(Local Amount 1000).
CST is USD 801(Local Amount 1100).

By default, this registry will be set to 'Yes'.
If this registry is set to 'No', all charges will be subjected to cost variance analysis as per 'Cost Variance Style and Approval Threshold' registry settings."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);

					item.OnBuildLogReference += (args) => Res.GetString("319A3491-4B99-4A46-9738-1689B590B063", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		#endregion

		#region Self Billing Invoice

		#region Self Billing Invoice Transaction Number Prefix

		public InvoiceTransactionNumberPrefixRegistryItem SelfBillingInvoiceTransactionNumberPrefix
		{
			get
			{
				return GetItem("SelfBillingInvoiceTransactionNumberPrefix", delegate
				{
					InvoiceTransactionNumberPrefixRegistryItem result = new InvoiceTransactionNumberPrefixRegistryItem(
						"SelfBillingInvoiceTransactionNumberPrefix",
						Categories.Accounting_PayableDefaults_SelfBillingInvoice,
						ResString.GetMultilingualString("22d10fc8-8747-4c8f-9a7c-02244d7ad291", "Self Billing Invoice Transaction Number Prefix"),
						ResString.GetMultilingualString("8571c615-498e-4844-8463-381ac6222c6d", "This registry changes the way the transaction number prints in the Accounts Payable Self Billing Invoice Document.  When this registry is overridden the selected prefix will be included as part of the Payables Self Billing Invoice transaction number. When overridden, the nominated prefix will also be included in all XML exports of all Payables Self Billing Invoices and Credit Note transactions."),
						RegistryStorageFlags.Company
						);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region CASSBilling

		public CodePairRegistryItem CASSImportAutoCreateClaims
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.CASSAutoCreateClaim));

				return GetItem("CASSImportAutoCreateClaims", delegate
				{
					return new CodePairRegistryItem(
						"CASSImportAutoCreateClaims",
						Categories.Accounting_PayableDefaults_DefaultSettings_CASS,
						ResString.GetMultilingualString("031d564b-bda9-46f7-9387-d2b0d7a5d47a", "CASS Import Auto Create Claims"),
						ResString.GetMultilingualString("5afc7957-637d-4c4f-85e5-0ad36cfc74d7", @"This registry controls the automatic creation of claims during CASS file import. Following are the options available:
1. Create Claims for Over-billing, this option will create claims only for CASS over billing cases. (Default)
2. Create Claims for Over-billing and Under-billing, this option will create claims for CASS over billing as well as under billing cases.
3. Do NOT create claims, this option will not automatically create claims during CASS file import."),
						listProvider,
						RegistryStorageFlags.Company,
						CASSAutoCreateClaim.OverBilled);
				});
			}
		}

		public CASSChargeCodesRegistryItem CASSChargeCodes
		{
			get
			{
				return GetItem("CASSChargeCodes", delegate
				{
					return new CASSChargeCodesRegistryItem(
						"CASSChargeCodes",
						Categories.Accounting_PayableDefaults_DefaultSettings_CASS,
						ResString.GetMultilingualString("da8484e8-14b2-4f39-8b28-802452163e11", "CASS Charge Codes"),
						ResString.GetMultilingualString("0ce062f0-85fe-46e0-8e81-923afbd00d88", @"You have the following options.
1. Allocate all CASS Costs to a single charge code. (Default setting)
2. Allocate all CASS Costs to multiple charge codes. With this setup, if there are no existing Accruals, the system will apportion all CASS Costs equally to the specify charge codes in the registry.However, if there are existing Accruals, the system will apportion all CASS Costs proportionately against existing Accruals only. 
3. Allocate each CASS Costs wholly to a charge code or proportionately to multiple charge codes. 

(Note: You can allocate multiple CASS Costs to a single charge code or single CASS Costs to multiple charge codes.In the event when a single CASS Costs are allocated to multiple charge codes, the allocation logic will be the same as the allocation logic applied to option 2 above.)"));
				});
			}
		}

		public GuidRegistryItem CASSGLAccount
		{
			get
			{
				return GetItem("CASSGLAccount", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"CASSGLAccount",
						Categories.Accounting_PayableDefaults_DefaultSettings_CASS,
						ResString.GetMultilingualString("79bc6e7e-310f-4084-afbe-cfbd2f663b0a", "CASS GL Account"),
						ResString.GetMultilingualString("72372142-34c8-463c-9887-dd94e6b85c28", "Allow to define a GL account to post costs billed by CASS for MAWBs not in the {0} database", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSHAndNonControl_AllowDirectPost);
					return result;
				});
			}
		}

		public DecimalRegistryItem CASSCostImportAllowedDiscrepancy
		{
			get
			{
				return GetItem("CASSCostImportAllowedDiscrepancy", delegate
				{
					return new DecimalRegistryItem(
						"CASSCostImportAllowedDiscrepancy",
						Categories.Accounting_PayableDefaults_DefaultSettings_CASS,
						ResString.GetMultilingualString("1b9ea083-410e-4646-9bee-7cb5339c8641", "CASS Cost Import Allowed Discrepancy"),
						ResString.GetMultilingualString("47e89c18-0d56-4cde-8a26-6b8d28d03754", @"This registry item provides you with the ability to set a default 'allowed' margin when importing the CASS Cost File.
This amount is defined in the local currency of the login company."),
						new NumericRegistryEditorInfo(2),
						RegistryStorageFlags.Company, RegistryOptions.Default, -1, 0, double.MaxValue);
				});
			}
		}

		#endregion

		#region Charge Code Message Override

		public ChargeCodeInvoiceTaxMessageOverrideRegistryItem ChargeCodeInvoiceTaxMessageOverride
		{
			get
			{
				return GetItem("ChargeCodeInvoiceTaxMessageOverride", delegate
				{
					return new ChargeCodeInvoiceTaxMessageOverrideRegistryItem(
						 "ChargeCodeInvoiceTaxMessageOverride",
						 Categories.Accounting_ReceivableDefaults_DefaultSettings,
						 ResString.GetMultilingualString("66ae0af0-1291-4438-9428-555e08f327af", "Invoice Tax Message Override"),
						 ResString.GetMultilingualString("2fe8db4b-5ec0-4c06-9fe5-d6d4475126a8", @"This Registry is Obsolete and will be removed in future versions. 
This Registry is now read only and has been retained for reference purposes only. 
The functionality previously supported by this registry are now supported in the Invoice Tax Messages Module."), RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Invoice Total Rounding

		public InvoiceTotalRoundingRegistryItem InvoiceTotalRounding
		{
			get
			{
				return GetItem("InvoiceTotalRounding", delegate
				{
					var item = new InvoiceTotalRoundingRegistryItem(
						"InvoiceTotalRounding",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("cb99c30d-37a7-4df7-ba4a-0c8f7e32ca3b", "Invoice Total Rounding"),
						ResString.GetMultilingualString("6b28dff4-04d4-4715-99da-6cf6ddee24d1", @"This feature is useful for companies that requires invoice total to be rounded to a specific currency units for specific currencies.
This feature is only relevant invoice currencies with 100 minor units. (e.g. 100 cents, 100 pence, 100 franc, etc.)

By default, this feature is not enabled.
When enabled, an additional non-job 'rounding' transaction line will be added to each invoice using the 'Invoice Total Rounding Charge Code', where applicable.
The Tax ID of additional line will always be set to NOTREPORT regardless of the charge code setup.

The rounding will be done on the gross up total of the invoice (i.e. VAT Inclusive charges).
You can configure the system to always round up, always round down or round based on midpoint.

Note:
The 'round based on midpoint' option can only be used if the round to currency unit is set to 1.00.
The 'round based on midpoint' option will round down if the invoice total's minor unit less than 50. E.g. 1,081.23 will be rounded to 1,081.00.
The 'round based on midpoint' option will round up if the invoice total's minor unit is 50 or more. E.g. 1,081.67 will be rounded to 1,082.00."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default);
					item.OnBuildLogReference += BuildInvoiceTotalRoundingLogReference;
					return item;
				});
			}
		}

		string BuildInvoiceTotalRoundingLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var result = string.Empty;

			var originalElements = ((InvoiceTotalRoundingCollection)args.OriginalValue).Cast<InvoiceTotalRounding>();
			var newElements = ((InvoiceTotalRoundingCollection)args.NewValue).Cast<InvoiceTotalRounding>();

			foreach (var setting in newElements.Where(added => !originalElements.Any(original => original.Currency == added.Currency)))
			{
				result += Res.GetString("6F324322-F40A-464A-83E2-20DAF46CF482", "New Currency Added '{0}' with Rounding Option '{1}' and Round To Currency Unit '{2}'.", setting.Currency, setting.RoundingOption, setting.RoundToCurrencyUnit) + "\r\n";
			}

			foreach (var setting in originalElements)
			{
				var updatedElement = newElements.FirstOrDefault(x => x.Currency == setting.Currency && (x.RoundingOption != setting.RoundingOption || x.RoundToCurrencyUnit != setting.RoundToCurrencyUnit));
				if (updatedElement != null)
				{
					result += Res.GetString("4d97907b-6807-4abd-a8b4-396671e34b27", "Currency '{0}', Rounding Option change from '{1}' to '{2}', Round To Currency Unit change from '{3}' to '{4}'.", setting.Currency, setting.RoundingOption, updatedElement.RoundingOption, setting.RoundToCurrencyUnit, updatedElement.RoundToCurrencyUnit) + "\r\n";
				}

				if (!newElements.Any(x => x.Currency == setting.Currency))
				{
					result += Res.GetString("64F4B8D4-3295-4FE5-B771-4E0B56FF1A8A", "Existing Currency '{0}', Rounding Option '{1}', Round To Currency Unit '{2}' deleted.", setting.Currency, setting.RoundingOption, setting.RoundToCurrencyUnit) + "\r\n";
				}
			}

			return result;
		}

		#endregion

		#region Invoice Total Rounding Charge Code

		public GuidRegistryItem InvoiceTotalRoundingChargeCode
		{
			get
			{
				return GetItem("InvoiceTotalRoundingChargeCode", delegate
				{
					var item = new GuidRegistryItem(
						"InvoiceTotalRoundingChargeCode",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("2c8d042f-4d2a-4dd2-879f-454a7d4db42a", "Invoice Total Rounding Charge Code"),
						ResString.GetMultilingualString("67203283-129d-4173-8117-5f89c4d10804", @"This charge code will be used for the creation of the rounding transaction line with reference to the 'Invoice Total Rounding' configuration.
This charge code must have a charge type of 'REV' or 'NON' only.
The Tax ID of this rounding transaction line will always be set to NOTREPORT regardless of the charge code setup."),
						RegistryStorageFlags.Company);
					item.Options = RegistryOptions.Default;
					item.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.RevenueOrNonJobRelatedChargeCode);
					item.OnBuildLogReference += (args) =>
					{
						var factory = new BusinessObjectFactory();
						var originalChargeCode = factory.Load<AccChargeCode>(new ZGuid(args.OriginalValue));
						var newChargeCode = factory.Load<AccChargeCode>(new ZGuid(args.NewValue));
						return Res.GetString("005E94CD-213D-49A7-B9A0-AFE4D17CCD63", "Charge Code changed from [{0}] to [{1}].", originalChargeCode?.AC_Code ?? ZString.Empty, newChargeCode?.AC_Code ?? ZString.Empty);
					};

					return item;
				});
			}
		}

		#endregion

		#region Aging Option

		public CodePairRegistryItem AgingOptionPayables
		{
			get
			{
				return GetItem("AgingOptionPayables", delegate
				{
					return new CodePairRegistryItem(
						"AgingOptionPayables",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("B8322883-2976-45CB-8EFB-85E542659292", "Aging Option in Payables Enquiries"),
						ResString.GetMultilingualString("73ABDB40-3418-427F-832D-68E13867F5A5", @"Use this registry to set the default Aging Option in Payables Enquiries. 

a. INV – Transactions will be aged by Invoice Date.
b. DUE – Transactions will be aged by Due Date.
c. PST - Transactions will be aged by Post Date. 

By default, this registry will be set to 'INV'."),
						new CodeDescriptionPairListProvider(() => AccountingConstants.AgingOptions.CodeList),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingConstants.AgingOptions.InvoiceDate);
				});
			}
		}

		#endregion

		#endregion

		#region Cost Confirmation Document

		#region Cost Confirmation Heading Text

		public MultilingualStringRegistryItem CostConfirmationHeadingText
		{
			get
			{
				return GetItem("CostConfirmationHeadingText", delegate
				{
					return new MultilingualStringRegistryItem(
						"CostConfirmationHeadingText",
						Categories.Accounting_PayableDefaults_CostConfirmationDocument,
						ResString.GetMultilingualString("4e5aec7e-47c1-4561-9214-43dcc4c0b17c", "Cost Confirmation Heading Text"),
						ResString.GetMultilingualString("7b16a52e-33d9-4390-a239-a9f15533d08b", @"The text recorded against this registry will print near the beginning of the Cost Confirmation Document.
The Cost Confirmation Document can be printed from AP Invoice, Credit Note and Adjustment Note transactions."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("7de98467-762e-4fef-9857-47d11848790e", "This document confirms that an Accounts Payable transaction has been posted with the following details:"));
				});
			}
		}

		#endregion

		#region Cost Confirmation Document Title

		public MultilingualStringRegistryItem CostConfirmationDocumentTitle
		{
			get
			{
				return GetItem("CostConfirmationDocumentTitle", delegate
				{
					return new MultilingualStringRegistryItem(
						"CostConfirmationDocumentTitle",
						Categories.Accounting_PayableDefaults_CostConfirmationDocument,
						ResString.GetMultilingualString("f96f0b48-b8f1-45fb-9ffa-fea0b9a49e07", "Cost Confirmation Document Title"),
						ResString.GetMultilingualString("9ad1d02f-9ea7-4756-88dc-dbd51fc95e66", "The text recorded against this registry will be printed as the title of the Cost Confirmation Document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("9503e08d-707f-4f05-a8ec-71180c59c138", "Confirmation of Costs Posted"));
				});
			}
		}

		#endregion

		#region Print Option When AP Invoice Posted

		public BooleanRegistryItem PrintOptionWhenAPInvoicePosted
		{
			get
			{
				return GetItem("PrintOptionWhenAPInvoicePosted", delegate
				{
					return new BooleanRegistryItem(
						"PrintOptionWhenAPInvoicePosted",
						Categories.Accounting_PayableDefaults_CostConfirmationDocument,
						ResString.GetMultilingualString("5d49a478-2075-4c0b-8928-0d8df786880c", "Print Option When AP Invoice Posted"),
						ResString.GetMultilingualString("493f996c-c000-4d91-b811-ec63e088287f", "When the registry is overridden & set to Yes, {0} will ask users 'Do you want to print the Cost Confirmation Document' each time an AP Invoice, Credit Note or Adjustment Note is posted.  By default this prompting behavior is turned off.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		public CodePairRegistryItem CostConfirmationDocumentSettings
		{
			get
			{
				return GetItem("CostConfirmationDocumentSettings", delegate
				{
					return new CodePairRegistryItem(
						"CostConfirmationDocumentSettings",
						Categories.Accounting_PayableDefaults_CostConfirmationDocument,
						ResString.GetMultilingualString("7827B4E6-EFA5-4b75-8992-88265E5ED1A8", "Document Settings"),
						ResString.GetMultilingualString("AA516EC3-8C8E-4e0a-AE93-E135BE9CAF85", "Cost confirmation document settings."),
						new CodeDescriptionPairListProvider(() => AccountingConstants.CostConfirmationDocumentSettingList),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingConstants.CostConfirmationDocumentSettingsCodes.Detail);
				});
			}
		}

		public CodePairRegistryItem CostConfirmationDocumentRollupSettings
		{
			get
			{
				return GetItem("CostConfirmationDocumentRollupSettings", delegate
				{
					return new CodePairRegistryItem(
						"CostConfirmationDocumentRollupSettings",
						Categories.Accounting_PayableDefaults_CostConfirmationDocument,
						ResString.GetMultilingualString("8B78347B-FAE1-473d-BB7D-FE9C76C2B768", "Document Roll Up Settings"),
						ResString.GetMultilingualString("CE5EF650-DCCA-45ab-B9CB-E01F689C1802", "Cost confirmation document roll up settings."),
						new CodeDescriptionPairListProvider(() => DocRollUpConstants.CostConfirmationDocumentRollupSettingList),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DocRollUpConstants.CostConfirmationDocumentRollupSettingsCodes.ChargeCode);
				});
			}
		}

		public MultilingualStringRegistryItem UnapprovedInvoiceCostConfirmationHeadingText
		{
			get
			{
				return GetItem("UnapprovedInvoiceCostConfirmationHeadingText", delegate
				{
					return new MultilingualStringRegistryItem(
						"UnapprovedInvoiceCostConfirmationHeadingText",
						Categories.Accounting_PayableDefaults_CostConfirmationDocument,
						ResString.GetMultilingualString("730e0221-0282-4f56-a3b6-3514a9d8a4cc", "Unapproved Invoice Cost Confirmation Heading Text"),
						ResString.GetMultilingualString("730713ec-9592-4de7-b952-93c91f056986", @"The text recorded against this registry will print near the beginning of the Cost Confirmation Document of an Unapproved AP invoice.
The Cost Confirmation Document can be printed from Unapproved AP Invoice and Unapproved Credit Note."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("4dda63bc-c1e1-42a3-888e-624338f38eee", "This document confirms that an Unapproved Accounts Payable transaction has been posted with the following details:"));
				});
			}
		}

		public MultilingualStringRegistryItem UnapprovedInvoiceCostConfirmationDocumentTitle
		{
			get
			{
				return GetItem("UnapprovedInvoiceCostConfirmationDocumentTitle", delegate
				{
					return new MultilingualStringRegistryItem(
						"UnapprovedInvoiceCostConfirmationDocumentTitle",
						Categories.Accounting_PayableDefaults_CostConfirmationDocument,
						ResString.GetMultilingualString("e318e9d0-b872-4191-b583-f0f408f63370", "Unapproved Invoice Cost Confirmation Document Title"),
						ResString.GetMultilingualString("a9962da2-dca7-4e77-bb62-8360fa765b24", "The text recorded against this registry will be printed as the title of the Cost Confirmation Document of an Unapproved AP invoice."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ResString.GetMultilingualString("9503e08d-707f-4f05-a8ec-71180c59c138", "Confirmation of Costs Posted"));
				});
			}
		}

		public BooleanRegistryItem PrintOptionWhenUnapprovedAPInvoicePosted
		{
			get
			{
				return GetItem("PrintOptionWhenUnapprovedAPInvoicePosted", delegate
				{
					return new BooleanRegistryItem(
						"PrintOptionWhenUnapprovedAPInvoicePosted",
						Categories.Accounting_PayableDefaults_CostConfirmationDocument,
						ResString.GetMultilingualString("72b74efd-0ef8-40c2-bafc-492865a97c79", "Print Option When Unapproved AP Invoice Posted"),
						ResString.GetMultilingualString("0d479bb8-ecea-4642-9c5d-45591ca6cf3b", "When the registry is overridden & set to Yes, {0} will ask users 'Do you want to print the Cost Confirmation Document' each time an Unapproved AP Invoice or Unapproved Credit Note is posted.  By default this prompting behavior is turned off.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem PrintAutofatturaItalyDocument
		{
			get
			{
				return GetItem("PrintAutofatturaItalyDocument", delegate
				{
					return new BooleanRegistryItem(
						"PrintAutofatturaItalyDocument",
						Categories.Accounting_PayableDefaults_CostConfirmationDocument,
						ResString.GetMultilingualString("5FF02C93-6CAC-4674-A496-85DAE1E921F8", "Print Autofattura (Italy) Document"),
						ResString.GetMultilingualString("828EA262-DE40-4CAA-978B-97F6D98FBF25", @"When the registry is overridden & set to Yes, Cargo Wise Will ask user 'Do you want to print the Autofattura (IT) Document' each time an AP Invoice, Credit Note or Adjustment is posted in Payables Transactions module, or in  Unapproved Invoices module,  with Compliance subtype 'APS' and Compliance Number filled.

By default this prompting behavior is turned off."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region Report Order

		public ReportOrderRegistryItem ReportOrder
		{
			get
			{
				return GetItem("AccountingReportOrder", delegate
				{
					return new ReportOrderRegistryItem(
						"AccountingReportOrder",
						Categories.Accounting_Framework_ReportOrder,
						ResString.GetMultilingualString("815fb7b4-a954-4c72-94b8-c42d0cb533ab", "Report Order"),
						ResString.GetMultilingualString("0c899efb-ca2d-4d1a-8fe4-23a86f8211bb",
@"This registry is used when running language specific (multi-language) Balance Sheet and Profit and Loss reports.
The registry identifies the local language starting points for the Balance Sheet and Profit and Loss reports for languages mapped through the Maintain > Account > GL Multi-Language Mapping module.
The ‘First Report’ column defines the report that starts a local language account mapping, when these mappings are sorted in ascending order.
So, if your multi language mapping ‘chart of accounts’ starts with ‘Balance Sheet’ accounts at the top,  you should select ‘Balance Sheet’ as the ‘First Report’.
Conversely, if your multi language mapping ‘chart of accounts’ commences with ‘Profit and Loss’ accounts at the top,  you should select ‘Profit and Loss’ as the ‘First Report’.

When the ‘First Report’ is defined as ‘Balance Sheet’, the ‘Second Report Start Account’ column defines the starting point for the ‘Profit and Loss’ report.
When the ‘First Report’ is defined as ‘Profit and Loss’, the ‘Second Report Start Account’ column defines the starting point for the ‘Balance Sheet’ report.

Note: This is an system level registry item.  The configurations here are used across all login companies."),
						RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region Last Aggregation Date and User

		public DateTimeRegistryItem LastAggregationDate
		{
			get
			{
				return GetItem("LastGLAggregateDate", delegate
				{
					DateTimeRegistryItem result = new DateTimeRegistryItem(
					"LastGLAggregateDate",
					null, null, null,
					RegistryStorageFlags.Company,
					RegistryOptions.IsHidden | RegistryOptions.NotCached, DateTime.MinValue);
					return result;
				});
			}
		}

		public GuidRegistryItem LastAggregationStaff
		{
			get
			{
				return GetItem("LastGLAggregateUser", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("LastGLAggregateUser",
					null, null, null,
					RegistryStorageFlags.Company,
					RegistryOptions.IsHidden, Guid.Empty);
					return result;
				});
			}
		}

		#endregion

		#region Should Display Recipient Tax ID

		public BooleanRegistryItem DisplayRecipientTaxID
		{
			get
			{
				return GetItem("DisplayRecipientTaxID", delegate
				{
					return new BooleanRegistryItem(
						new DisplayRecipientTaxIDRegistryItemImpl(
						"DisplayRecipientTaxID",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						(NoResString)"Display Recipient Tax ID on AR Invoice (CargoWiseOne Support Only)",
						(NoResString)@"This is a CargoWiseOne Support Only Registry.
It is used when the Login Company needs to print the Main Tax Registration Number of the Receivables Organization in the AR Invoice AND the CargoWiseOne Invoice is NOT yet configured to support its display.
When the Registry is set to YES, the Recipient Main Tax Registration Number will print in the AR Invoice.
NOTE: When you override this registry from NO to YES, the Recipient Tax ID Heading will use the Login Country’s name of tax. I.e. ‘CLIENT VAT’, or ‘CLIENT GST’ etc.
Please see the ‘Recipient Tax ID Heading on AR Invoices’ registry if you wish to modify the default heading displayed.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport));
				});
			}
		}

		class DisplayRecipientTaxIDRegistryItemImpl : RegistryItemImpl
		{
			public DisplayRecipientTaxIDRegistryItemImpl(string name, MultilingualString category, NoResString caption, NoResString hint, RegistryStorageFlags storage, RegistryOptions option)
				: base(name, category, caption, hint, new BooleanRegistryDataType(), storage, option)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				bool result;

#if DEBUG
				if (Cache != null && Globals.IsTest)
				{
					Cache = null;
				}
#endif

				if (Cache == null)
				{
					Cache = new Dictionary<Guid, bool>();
					UpdateCache();
				}

				if (!Cache.TryGetValue(companyPK, out result))
				{
					result = false;
				}

				return result;
			}

			Dictionary<Guid, bool> Cache;

			void UpdateCache()
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				ZQuery companyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Cache.Keys);
				companyQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.NotEqual, string.Empty);
				GlbCompany[] companies = factory.Load<GlbCompany>(companyQuery);

				foreach (GlbCompany company in companies)
				{
					bool defaultValue = false;
					var countryCode = company.GC_RN_NKCountryCode;

					var defaultValueForDisplayRecipientTaxIDRegistry = ObjectFactory.Get<ICountryComplianceFactoryIntegration>()?.GetICountryComplianceInfo(countryCode)?.GetDefaultValueForDisplayRecipientTaxIDRegistry();
					if (defaultValueForDisplayRecipientTaxIDRegistry != null)
					{
						defaultValue = defaultValueForDisplayRecipientTaxIDRegistry.Value;
					}
					#region This code design is obsolete please add new codes to through classes created by CountryComplianceFactory
					else
					{
						switch (countryCode)
						{
							case Core.Constants.CountryCodes.Netherlands:
							case Core.Constants.CountryCodes.Norway:
							case Core.Constants.CountryCodes.SouthAfrica:
							case Core.Constants.CountryCodes.Philippines:
							case Core.Constants.CountryCodes.Peru:
							case Core.Constants.CountryCodes.SriLanka:
							case Core.Constants.CountryCodes.Ethiopia:
							case Core.Constants.CountryCodes.Maldives:
							case Core.Constants.CountryCodes.Tonga:
							case Core.Constants.CountryCodes.Uganda:
							case Core.Constants.CountryCodes.Venezuela:
							case Core.Constants.CountryCodes.PuertoRico:
							case Core.Constants.CountryCodes.Paraguay:
							case Core.Constants.CountryCodes.Uruguay:
							case Core.Constants.CountryCodes.Azerbaijan:
							case Core.Constants.CountryCodes.Bolivia:
							case Core.Constants.CountryCodes.FrenchPolynesia:
							case Core.Constants.CountryCodes.Kenya:
							case Core.Constants.CountryCodes.Nicaragua:
							case Core.Constants.CountryCodes.Mongolia:
							case Core.Constants.CountryCodes.Botswana:
							case Core.Constants.CountryCodes.Kazakhstan:
							case Core.Constants.CountryCodes.Tanzania:
							case Core.Constants.CountryCodes.Mali:
							case Core.Constants.CountryCodes.Jordan:
							case Core.Constants.CountryCodes.Zimbabwe:
							case Core.Constants.CountryCodes.Thailand:
							case Core.Constants.CountryCodes.Senegal:
							case Core.Constants.CountryCodes.CoteDivoire:
							case Core.Constants.CountryCodes.Cameroon:
							case Core.Constants.CountryCodes.Mozambique:
							case Core.Constants.CountryCodes.EquatorialGuinea:
							case Core.Constants.CountryCodes.Yemen:
							case Core.Constants.CountryCodes.Malawi:
							case Core.Constants.CountryCodes.Russia:
							case Core.Constants.CountryCodes.Lebanon:
							case Core.Constants.CountryCodes.Niger:
							case Core.Constants.CountryCodes.Ghana:
							case Core.Constants.CountryCodes.Belarus:
							case Core.Constants.CountryCodes.SierraLeone:
							case Core.Constants.CountryCodes.Spain:
							case Core.Constants.CountryCodes.Cambodia:
							case Core.Constants.CountryCodes.Madagascar:
							case Core.Constants.CountryCodes.Kiribati:
							case Core.Constants.CountryCodes.Nepal:
							case Core.Constants.CountryCodes.Curacao:
							case Core.Constants.CountryCodes.Jamaica:
							case Core.Constants.CountryCodes.TrinidadAndTobago:
							case Core.Constants.CountryCodes.Togo:
							case Core.Constants.CountryCodes.Macedonia:
							case Core.Constants.CountryCodes.Croatia:
							case Core.Constants.CountryCodes.Barbados:
							case Core.Constants.CountryCodes.Rwanda:
							case Core.Constants.CountryCodes.Kosovo:
							case Core.Constants.CountryCodes.Iran:
							case Core.Constants.CountryCodes.UnitedArabEmirates:
							case Core.Constants.CountryCodes.Bahrain:
							case Core.Constants.CountryCodes.Kuwait:
							case Core.Constants.CountryCodes.Oman:
							case Core.Constants.CountryCodes.Qatar:
							case Core.Constants.CountryCodes.SaudiArabia:
							case Core.Constants.CountryCodes.Georgia:
							case Core.Constants.CountryCodes.NewCaledonia:
							case Core.Constants.CountryCodes.NewZealand:
							case Core.Constants.CountryCodes.Chad:
							case Core.Constants.CountryCodes.LaoPeoplesDemocraticRepublic:
							case Core.Constants.CountryCodes.Morocco:
							case Core.Constants.CountryCodes.BosniaAndHerzegovina:
							case Core.Constants.CountryCodes.Angola:
								defaultValue = true;
								break;
						}

						if (company.Country != null && company.Country.IsPartOfEuropeanUnion)
						{
							defaultValue = true;
						}
					}
					#endregion

					Cache.Add(company.PK.ToGuid(), defaultValue);
				}
			}
		}

		#endregion

		#region Default Display Recipient Tax ID Heading

		public StringRegistryItem DisplayRecipientTaxIDHeading
		{
			get
			{
				return GetItem("DisplayRecipientTaxIDHeading", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"DisplayRecipientTaxIDHeading",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						(NoResString)"Display Recipient Tax ID Heading on AR Invoice (CargoWiseOne Support Only)",
						(NoResString)@"This is a CargoWiseOne Support Only Registry.
Use this registry to modify the display of the Recipient Main Tax Registration heading in the AR Invoice document.
NOTE: The Recipient Tax ID Heading will only display in the AR Invoice document IF the ‘Display Recipient Tax ID on AR Invoice’ registry is set to YES.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.TextBox);
					return result;
				});
			}
		}

		#endregion

		#region Calculate Tax at Header Level

		public BooleanRegistryItem CalculateTaxAtHeaderLevel
		{
			get
			{
				return GetItem("CalculateTaxAtHeaderLevel", delegate
				{
					return new BooleanRegistryItem(
						new CalculateTaxAtHeaderLevelRegistryItemImpl(
						"CalculateTaxAtHeaderLevel",
						Categories.Accounting,
						ResString.GetMultilingualString("384b253f-106f-459a-80c3-ac2d31ebf4ee", "Calculate Tax at Header Level"),
						ResString.GetMultilingualString("030E47BB-C3E0-45DF-A0CD-64973B51B1D5", @"When this Registry is set to 'No', the VAT/GST amount of each charge line is calculated and rounded line by line. The VAT/GST amount of each charge line is calculated individually without reference to any other charge line in the same transaction.

Alternatively, when this registry is set to YES, charge lines with same VAT / GST rates in the one transaction will be ""grouped"" and if necessary, the tax amount of the largest line will be adjusted to ensure the amount recorded is correct 'at header level'.The system will make this adjustment so that the total VAT/ GST for each rate of VAT within an Invoice will be calculated based on a rounded subtotal of all taxable lines attracting the same rate of VAT / GST within an invoice."),
						RegistryStorageFlags.Company));
				});
			}
		}

		class CalculateTaxAtHeaderLevelRegistryItemImpl : RegistryItemImpl
		{
			public CalculateTaxAtHeaderLevelRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new BooleanRegistryDataType(), storage)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				if (cache == null)
				{
					cache = new Dictionary<Guid, bool>();
					UpdateCache();
				}

				bool calculatedAtHeaderLevel;

				if (!cache.TryGetValue(companyPK, out calculatedAtHeaderLevel))
				{
					UpdateCache();

					if (!cache.TryGetValue(companyPK, out calculatedAtHeaderLevel))
					{
						cache.Add(companyPK, false);
						calculatedAtHeaderLevel = false;
					}
				}

				return calculatedAtHeaderLevel;
			}

			void UpdateCache()
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				ZQuery companyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, cache.Keys);
				BusinessObject[] companies = factory.Load<GlbCompany>(companyQuery);

				foreach (BusinessObject company in companies)
				{
					ZString countryCode = new ZString(company[GlbCompanySchema.GC_RN_NKCountryCode]);
					bool defaultValue = CountryCodes.Contains(countryCode);
					cache.Add(company.PK.ToGuid(), defaultValue);
				}
			}

			Dictionary<Guid, bool> cache;

			List<ZString> CountryCodes
			{
				get
				{
					if (fCountryCodes == null)
					{
						fCountryCodes = new List<ZString>();
						fCountryCodes.Add(Core.Constants.CountryCodes.Taiwan);
						fCountryCodes.Add(Core.Constants.CountryCodes.Italy);
					}

					return fCountryCodes;
				}
			}
			List<ZString> fCountryCodes;
		}

		#endregion

		#region Fall Back to Previous Exchange Rate

		public BooleanRegistryItem FallBackToPreviousExchangeRate
		{
			get
			{
				return GetItem("FallBackToPreviousExchangeRate", delegate
				{
					return new BooleanRegistryItem(
						"FallBackToPreviousExchangeRate",
						Categories.Accounting,
						ResString.GetMultilingualString("8bbb0fe7-51f7-41b9-bf40-d2ab2794f9af", "Fall Back to Previous Exchange Rate"),
						ResString.GetMultilingualString("8c1f2195-f19b-41ab-b221-4d592506f0fc", "Fall back to the previous exchange rate if today's exchange rate not found."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Negative Cost Validation Enforcement

		public BooleanRegistryItem NegativeCostValidationEnforced
		{
			get
			{
				return GetItem("NegativeCostValidationEnforced", delegate
				{
					return new BooleanRegistryItem(
						"NegativeCostValidationEnforced",
						Categories.Accounting_JobCostingDefaults,
						ResString.GetMultilingualString("6c490dc9-cf2e-40b8-8a4c-12c1ed812ffd", "Negative Cost Validation Enforced"),
						ResString.GetMultilingualString("cc7dea1e-c489-467b-aef4-cc379e1c47f9", "When this Registry item is set to 'Yes', negative costs will not be permitted on the billing screen unless AP Invoice Details are provided."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Job Invoicing

		#region Rounding

		public CodePairRegistryItem JapanIATAImportAirLocalClientFRTChargeGroupRounding
		{
			get
			{
				return GetItem("JapanIATAImportAirLocalClientFRTChargeGroupRounding", delegate
				{
					return new CodePairRegistryItem(
						"JapanIATAImportAirLocalClientFRTChargeGroupRounding",
						Categories.Accounting_JobInvoicing_Rounding,
						ResString.GetMultilingualString("9384408D-7A37-4F3F-8B37-11624E08EA8F", "Japan IATA Import Air Local Client FRT Charge Group Rounding"),
						ResString.GetMultilingualString("834E4F42-5020-46BC-8249-885727A47A35", @"This registry is relevant to Japan Login Companies only.  By default this registry is not enabled.
When enabled, the rounding behaviors described below will only apply to sell charges prepared on a Forwarding Import Air Shipment.
When enabled, the sum of local currency equivalent FRT group charge codes prepared for invoicing to the job’s Local Client in a local currency invoice will be adjusted and rounded as per the nominated rule:
JPY - Adjustment will be made to the Local Sell amount of the largest, unposted local currency FRT group charge line prepared for the Local Client.
JPX - A new charge line will be added to the billing tab for the value of the rounding amount using the charge code specified in 'Rounding Charge Code' registry.
The OS Sell amount will also be amended when the adjusted line’s OS Sell Currency is local."),
						OLookUpEditType.RoundingRules,
						RegistryStorageFlags.Company,
						Constants.RoundingRules.Codes.None);
				});
			}
		}

		public ChargeCodeRegistryItem RoundingChargeCode
		{
			get
			{
				return GetItem("RoundingChargeCode", delegate
				{
					ChargeCodeRegistryItem result = new ChargeCodeRegistryItem(
						"RoundingChargeCode",
						Categories.Accounting_JobInvoicing_Rounding,
						ResString.GetMultilingualString("9FAB3EAA-D3D5-4EC9-A885-06590D8DFD4E", "Rounding Charge Code"),
						ResString.GetMultilingualString("1806028C-8D0E-44AE-83FB-3605E1569C36", "When using the 'JPX' rounding rule in the registry 'Japan IATA Import Air Local Client FRT Charge Group Rounding', {0} will add a charge line to the billing tab for the value of the rounding amount using this charge code.", BrandingFactory.Instance.ProductName),
						"");

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.FreightChargeCode);
					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeRegistryItem IncludeInRoundingChargeCode
		{
			get
			{
				return GetItem("IncludeInRoundingChargeCode", delegate
				{
					ChargeCodeRegistryItem result = new ChargeCodeRegistryItem(
						"IncludeInRoundingChargeCode",
						Categories.Accounting_JobInvoicing_Rounding,
						ResString.GetMultilingualString("B4C553D1-9FD9-427F-AF9C-5F4EA803326E", "Include In Rounding Charge Code"),
						ResString.GetMultilingualString("80A96089-AD15-4073-BABD-A742CD0E80AA", "When using the 'JPX' rounding rule in the registry 'Japan IATA Import Air Local Client FRT Charge Group Rounding', {0} will include a charge line with this charge code on the billing tab into rounding calculation.", BrandingFactory.Instance.ProductName),
						"");

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.FreightChargeCode);
					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		#endregion

		#region DefaultDepartmentsOfLocalTransport

		public LinkRegistryItem DefaultDepartmentsOfLocalTransport
		{
			get
			{
				return GetNonCachedItem(delegate
				{
					return new LinkRegistryItem(
						Categories.Accounting_JobInvoicing_DefaultDepartments,
						ResString.GetMultilingualString("7696d7b6-e0e0-42b1-b713-f0701ec3fc11", "Port Transport"),
						ResString.GetMultilingualString("a815a12a-bd1c-4384-b9b4-a4ee896f94da", "Edit Job Types"),
						ResString.GetMultilingualString("b7b40df8-c13e-410c-8f37-c251eb92560a", "Default Departments for Port Transport Jobs can be set per Job Type. You can select a Job Type and edit its Default Department."),
						ModuleIDs.CartageType);
				});
			}
		}

		#endregion

		#region Post Job Invoicing Transactions To Login Branch

		public BooleanRegistryItem PostJobInvoicingTransactionsToLoginBranch
		{
			get
			{
				return GetItem("PostJobInvoicingTransactionsToLoginBranch", delegate
				{
					return new BooleanRegistryItem(
						 "PostJobInvoicingTransactionsToLoginBranch",
						 Categories.Accounting_JobInvoicing,
						 ResString.GetMultilingualString("760b09de-a02e-45ff-8c43-cfb8ff51607f", "Post Job Invoicing Transactions To Login Branch"),
						 ResString.GetMultilingualString("9b4db40e-9f39-4cd5-8f2b-ec76fa5f2432", @"When this Registry item is set to 'Yes', the Invoice/Credit Note and Payment transactions will post to the current login Branch.

Otherwise, 
o When posting from Job level, system will use the Branch and Department from the Job Header
o When posting from Consol level, system will use 
  - Branch of Sending Agent Proxy for Export Consol
  - Branch of Receiving Agent Proxy for Import Consol
  - Fall back to current login Branch."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 false);
				});
			}
		}

		#endregion

		#region Profit Share

		public ChargeCodeRegistryItem ProfitShareChargeCode
		{
			get
			{
				return GetItem("ProfitShareChargeCode", delegate
				{
					ChargeCodeRegistryItem result = new ChargeCodeRegistryItem(
						"ProfitShareChargeCode",
						Categories.Accounting_JobInvoicing_ProfitShare,
						ResString.GetMultilingualString("0c936a8f-f185-40a9-be0f-ba1b37a0dc41", "Profit Share Charge Code"),
						ResString.GetMultilingualString("599bc69e-977f-41e7-9cb0-1c5bfc50485e", "Specify the charge code to use when calculating and posting Profit Share to jobs."),
						"PS");

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.MrgDsbOrMjaChargeCode);
					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public ChargeCodeWithTypeRegistryItem ProfitShareChargeCodesPerParty
		{
			get
			{
				return GetItem("ProfitShareChargeCodesPerParty", delegate
				{
					ChargeCodeWithTypeRegistryItem result = new ChargeCodeWithTypeRegistryItem(
						"ProfitShareChargeCodesPerParty",
						Categories.Accounting_JobInvoicing_ProfitShare,
						ResString.GetMultilingualString("7233930e-0f15-4d99-870c-89f69f208766", "Profit Share Charge Codes Per Party"),
						ResString.GetMultilingualString("f2c4f425-b078-44ca-9581-9be4ce9b3844", "Specify the charge code per party to use when calculating and posting Profit Share to jobs."),
						RegistryStorageFlags.Company,
						ChargeCodeWithTypeCollection.GetDefaultCollection());

					return result;
				});
			}
		}

		public ChargeCodeRegistryItem ProfitShareAdjustmentChargeCode
		{
			get
			{
				return GetItem("ProfitShareAdjustmentChargeCode", delegate
				{
					ChargeCodeRegistryItem result = new ChargeCodeRegistryItem(
						"ProfitShareAdjustmentChargeCode",
						Categories.Accounting_JobInvoicing_ProfitShare,
						ResString.GetMultilingualString("bbb499bc-8011-4182-9b8f-d9b3ccd35633", "Profit Share Adjustment Charge Code"),
						ResString.GetMultilingualString("b58ff00f-0dd3-4357-a748-6276f8ef06a5", "Specify the charge code to use when calculating and posting Profit Share Adjustments to jobs."),
						"PS");

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.MrgDsbOrMjaChargeCode);
					((GuidFindBoxRegistryEditorInfo)result.EditorInfo).IsPrimaryKeyFromCodeRequired = true;
					return result;
				});
			}
		}

		public BooleanRegistryItem AutoCompleteJobOnProfitShareCalculation
		{
			get
			{
				return GetItem("AutoCompleteJobOnProfitShareCalculation", delegate
				{
					return new BooleanRegistryItem(
						"AutoCompleteJobOnProfitShareCalculation",
						Categories.Accounting_JobInvoicing_ProfitShare,
						ResString.GetMultilingualString("c197db14-e881-48d9-9930-dcfdc5fcb698", "Complete Job on Profit Share Calculation"),
						ResString.GetMultilingualString("102e744e-8638-4940-b0c6-a1a2e41d7ccf", "When this Registry item is set to 'Yes', after profit share is calculated, the job's status will be set to Complete."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem ProfitSharePostProfitShareOnCreation
		{
			get
			{
				return GetItem("ProfitSharePostProfitShareOnCreation", delegate
				{
					return new BooleanRegistryItem(
						"ProfitSharePostProfitShareOnCreation",
						Categories.Accounting_JobInvoicing_ProfitShare,
						ResString.GetMultilingualString("c17025b1-c3fb-42bf-a4d7-6eaa0598c976", "Post Profit Share Shipment Charges When Calculated"),
						ResString.GetMultilingualString("8777db68-b8f5-4d87-b61e-8cf70a7003a6", "When this Registry item is set to 'Yes', after profit share is calculated at the shipment level, the profit share charges will be automatically posted."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem ProfitShareCreateChargesPerCurrency
		{
			get
			{
				return GetItem("ProfitShareCreateChargesPerCurrency", delegate
				{
					return new BooleanRegistryItem(
						"ProfitShareCreateChargesPerCurrency",
						Categories.Accounting_JobInvoicing_ProfitShare,
						ResString.GetMultilingualString("eaef7f3a-a3b7-4c32-857b-606d91b407c0", "Create Profit Share Charges per Currency"),
						ResString.GetMultilingualString("bc1c44a2-5b49-4afa-997f-1d597553add7", "When this Registry item is set to 'Yes', profit share costs will be created in each applicable currency, rather than 1 charge in Local Currency."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem ProfitShareUpdateOnSave
		{
			get
			{
				return GetItem("ProfitShareUpdateOnSave", delegate
				{
					return new BooleanRegistryItem(
						"ProfitShareUpdateOnSave",
						Categories.Accounting_JobInvoicing_ProfitShare,
						ResString.GetMultilingualString("ce7ce5bf-3fc4-4edf-875c-73ec025b0992", "Update Shipment Profit Share when Saving"),
						ResString.GetMultilingualString("25addfff-ceef-4e98-b966-e2c323c07abf", "When this Registry item is set to 'Yes', if any changes occur on the billing tab, profit share will automatically be updated."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem CreateProfitShareAsAR
		{
			get
			{
				return GetItem("CreateProfitShareAsAR", delegate
				{
					return new BooleanRegistryItem(
						"CreateProfitShareAsAR",
						Categories.Accounting_JobInvoicing_ProfitShare,
						ResString.GetMultilingualString("0c9cff6a-c064-4e95-be6e-200cacfd842c", "Profit Share as AR"),
						ResString.GetMultilingualString("df7e14c0-f2c8-40bc-a650-c7e03c8ccaea", "When this Registry item is set to 'Yes', profit share charges will be created as Receivables transactions, rather than the default of Payables transactions."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem ProfitShareRedirectHeadOfficeIfExternal
		{
			get
			{
				return GetItem("ProfitShareRedirectHeadOfficeIfExternal", delegate
				{
					return new BooleanRegistryItem(
						"ProfitShareRedirectHeadOfficeIfExternal",
						Categories.Accounting_JobInvoicing_ProfitShare,
						ResString.GetMultilingualString("647aa957-b66d-40d6-9046-b934d42fcb3d", "Redirect Head Office Profit Share for External Controlling Agent"),
						ResString.GetMultilingualString("3641006c-df5c-4488-a236-afa55aeb2d9e", "When this Registry item is set to 'Yes', if the controlling agent on the job is not an organization proxy of the current company, any profit share due the head office will instead be assigned to that external controlling agent."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Job Branch Default Order Rule

		public JobBranchDefaultOrderRuleRegistryItem JobBranchDefaultOrderRule
		{
			get
			{
				return GetItem("JobBranchDefaultOrderRule", delegate
				{
					return new JobBranchDefaultOrderRuleRegistryItem(
						"JobBranchDefaultOrderRule",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("15cc909e-92cb-4867-85a3-ddc493ce7960", "Job Creation - Default Branch Rule"),
						ResString.GetMultilingualString("a7421b91-025c-4d27-b05f-26fa9cbc18d8", "Override these values to set the order of the default branch on creation of a Job.\r\nThe values can be between 0 and 4.\r\nA value of 0 means the rule will not be used.\r\nThere can be multiple rules with a value of 0. At least one rule must have a value greater than 0.\r\nAny value greater than 1 must not be duplicated, i.e. 0, 1, 1, 2 is not valid.\r\nThere must not be gaps between the sequence of numbers for the values, i.e. 0, 1, 2, 3 is valid but 0, 1, 2, 4 is not valid."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
				});
			}
		}

		#endregion

		#region Job Costing Category

		#region Consol Cost Default Apportionment Method

		public ConsolCostDefaultApportionmentMethodRegistryItem ConsolCostDefaultApportionmentMethod
		{
			get
			{
				return GetItem("ConsolCostDefaultApportionmentMethod", delegate
				{
					var item = new ConsolCostDefaultApportionmentMethodRegistryItem(
						"ConsolCostDefaultApportionmentMethod",
						Categories.Accounting_JobCostingDefaults,
						ResString.GetMultilingualString("9ec89562-2312-4d42-97c4-fc4f3fafa416", "Consol Cost Default Apportionment Method"),
						ResString.GetMultilingualString("69e52454-c019-44d4-aff6-8f698454fdfd", "Please select the default apportionment method for a Consol Cost."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company
						);

					item.OnBuildLogReference += BuildConsolCostDefaultApportionmentMethodRegistryItemLogReference;

					return item;
				});
			}
		}

		string BuildConsolCostDefaultApportionmentMethodRegistryItemLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var msg = new StringBuilder();

			var oldValue = args.OriginalValue as ConsolCostDefaultApportionmentMethodConfiguration;
			var oldValueLogInfos = oldValue.ConsolCostDefaultApportionmentMethodCollection.Cast<ConsolCostDefaultApportionmentMethod>()
				.Select(x => GetLogInfo(x))
				.ToArray();

			var newValue = args.NewValue as ConsolCostDefaultApportionmentMethodConfiguration;
			var newValueLogInfos = newValue.ConsolCostDefaultApportionmentMethodCollection.Cast<ConsolCostDefaultApportionmentMethod>()
				.Select(x => GetLogInfo(x))
				.ToArray();

			oldValueLogInfos.Except(newValueLogInfos)
				.ForEach(logInfo => msg.AppendLine(Res.GetString("21A9CA09-E17A-4972-95FD-BF79AF6F6EF2", "Deleted: {0}", logInfo)));

			newValueLogInfos.Except(oldValueLogInfos)
				.ForEach(logInfo => msg.AppendLine(Res.GetString("86AA591A-CDA8-4C8F-874F-02BD4E04096E", "Added: {0}", logInfo)));

			return msg.ToString();

			string GetLogInfo(ConsolCostDefaultApportionmentMethod item)
			{
				return Res.GetString("F6ABB371-75AE-4FE0-B3C9-41F08E5ABC79", "Module={0}, Consol Type={1}, Direction={2}, Transport Mode={3}, Container Mode={4}, Apportionment={5}"
					, item.Module
					, item.ConsolType
					, item.Direction
					, item.TransportMode
					, item.ContainerMode
					, item.Apportionment);
			}
		}

		#endregion

		#region Consol Cost Child Shipments Apportionment

		public BooleanRegistryItem ConsolCostDefaultRelatedShipmentsApportionment
		{
			get
			{
				return GetItem("ConsolCostDefaultRelatedShipmentsApportionment", delegate
				{
					return new BooleanRegistryItem(
						"ConsolCostDefaultRelatedShipmentsApportionment",
						Categories.Accounting_JobCostingDefaults,
						ResString.GetMultilingualString("e5387f9d-2f96-4e42-b04c-1a8a206f705a", "Consol Cost Default Display of Related Shipments"),
						ResString.GetMultilingualString("74f06d56-d5e4-4738-a840-529e511f0638", @"This registry controls the default shipment display behavior when creating a Consol Cost Apportionments through either the Consol or AP Invoice form.  
When set to 'No' (the default configuration) only Master shipments will be returned in the apportionment grid.  
By default, related shipments (e.g. Sub House Bills, Buyers Consol Related Shipments) are excluded from the display.
When this registry is overridden and set to 'Yes' all shipments attached to each consol will default into the apportionment grid when creating consol costs.  Related shipments will not be excluded from the display."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Accrual Must Have Creditor Code

		public BooleanRegistryItem AccrualMustHaveCreditorCode
		{
			get
			{
				return GetItem("AccrualMustHaveCreditorCode", delegate
				{
					return new BooleanRegistryItem(
						"AccrualMustHaveCreditorCode",
						Categories.Accounting_JobCostingDefaults,
						ResString.GetMultilingualString("21369963-f50e-4395-89b0-8672d4f0e10f", "Accrual Must Have Creditor Code"),
						ResString.GetMultilingualString("e3b8eb96-d20f-4a6b-84d8-4a2df8c7c214", "When overridden and set to 'Yes', users will be required to record a Creditor against each accrual added to a job.  By default, a Creditor is not required when adding accruals into the Job Billing screen. It is recommended that you set the registry item 'Accrual Reversal Behavior When Allocated To Creditor' to 'Yes' when setting this registry item to 'Yes'.\r\n\r\nNote: This setting does not affect spot quotes created via WebTracker."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region WIP Must Have Debtor Code

		public BooleanRegistryItem WIPMustHaveDebtorCode
		{
			get
			{
				return GetItem("WIPMustHaveDebtorCode", delegate
				{
					return new BooleanRegistryItem(
						"WIPMustHaveDebtorCode",
						Categories.Accounting_JobCostingDefaults,
						ResString.GetMultilingualString("a96d773e-8f45-47eb-968e-5526d03d1db6", "WIP Must Have Debtor Code"),
						ResString.GetMultilingualString("d2bf8afb-9a2b-4e8c-9906-0d86c6d9ebac", "When overridden and set to 'Yes', users will be required to record a Debtor against each WIP added to a job.  By default, a Debtor is not required when adding WIPs into the Job Billing screen."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Enable Negative Accrual Behaviours

		public BooleanRegistryItem EnableNegativeAccrualBehaviors
		{
			get
			{
				return GetItem("EnableNegativeAccrualBehaviors", delegate
				{
					return new BooleanRegistryItem(
						"EnableNegativeAccrualBehaviors",
						Categories.Accounting_JobCostingDefaults,
						ResString.GetMultilingualString("8D1ECAB3-2C80-4ae6-8A43-99BD53CD2DC8", "Enable Negative Accrual Behaviors"),
						ResString.GetMultilingualString("D003B603-0739-41a2-86B1-7255B5A17DBE",
@"When this registry is set to 'Yes' accrual accounting transactions (WIP and / or ACR) will be created for un-posted negative charges in the same way WIP and ACR transactions are created for positive un-posted charges.
When posting costs and revenues these negative WIP and ACR transactions will be appropriately reversed.
The creation and reversal behavior of negative ACR transactions respects all other Accrual controls: re-accrual of unused amounts, timing of revenue recognition, creation of corresponding WIP amounts, etc."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Enable Auto Job Revenue Journals

		public CodePairRegistryItem EnableAutoJobRevenueJournals
		{
			get
			{
				return GetItem("EnableAutoJobRevenueJournals", delegate
				{
					return new CodePairRegistryItem(
						"EnableAutoJobRevenueJournals",
						Categories.Accounting_JobCostingDefaults,
						ResString.GetMultilingualString("ABFF7C25-36C6-4006-A3FF-4C7C070C14C4", "Enable Auto Job Revenue Journals"),
						ResString.GetMultilingualString("9855E04F-437A-4CCB-AE65-392F28A15F12", @"When this registry is set to YES, Job Revenue Journals (JRJ) will be created as soon as charges that are linked to organization proxies that are branches of the same log in company.

When this registry is set to TAX, the Debtor or Creditor must also have the same GST/VAT registration as the branch's organization proxy."),
						new CodeDescriptionPairListProvider(() => new AutoJobRevenueJournalOptions()),
						RegistryStorageFlags.Company,
						AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non);
				});
			}
		}

		#endregion

		#region Check Different Signs When Apportion Consol Cost (CargoWise Support Only)

		public BooleanRegistryItem CheckDifferentSignsWhenApportionConsolCost
		{
			get
			{
				return GetItem("CheckDifferentSignsWhenApportionConsolCost", delegate
				{
					return new BooleanRegistryItem(
						"CheckDifferentSignsWhenApportionConsolCost",
						Categories.Accounting_JobCostingDefaults,
						(NoResString)@"Check Different Signs When Apportion Consol Cost (CargoWiseOne Support Only)",
						(NoResString)@"When this registry is set to 'YES', it will only allow to use different signs on Amounts for MAN apportionment method.


NOTE: Please consult with the Accounting Product Team before turning on this registry.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		#endregion

		#endregion

		#region Auto Post Master Collect Charge

		public BooleanRegistryItem AutoPostMasterCollectCharge
		{
			get
			{
				return GetItem("AutoPostMasterCollectCharge", delegate
				{
					return new BooleanRegistryItem(
						"AutoPostMasterCollectCharge",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("84818b88-94ea-4265-94cb-445ff657c91c", "Credit Agent Consol Costs on Collect Invoice"),
						ResString.GetMultilingualString("f587cb13-5fd3-4684-a8cc-cdb9124adcc0", "Select 'Yes' to default all consol costs where the creditor is the Sending (Import) / Receiving (Export) agent to automatically journal those costs onto the agent's collect invoice. This can be overridden on a cost by cost basis by the operator."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Add Job Invoicing Record at Saving/Editing of Operations Job

		public bool ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(IJobInvoicingPlugIn plugin, ZGuid companyPK = default)
		{
			return (companyPK.IsDefault ? AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.Value : AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty))
				|| AccountingUtils.EnableElectronicProcessingCharge(plugin);
		}

		public string AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation
		{
			get { return AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.Location(); }
		}

#if DEBUG
		public
#endif
		BooleanRegistryItem AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob
		{
			get
			{
				return GetItem("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob", delegate
				{
					MultilingualString hint =
						ResString.GetMultilingualString("d74b8af7-fecd-4462-a5ea-68d5f86ab02c", "When set to YES, this registry will automatically create BOTH the operations job (e.g. Shipment; Load List) AND the 'Invoicing Job' when the user first saves (or edits) the job.\r\nThe 'Invoicing Job' is used for invoicing and processing costs. It is the 'Job Invoicing' tab seen on many operations jobs.\r\n\r\nWhen set to NO, the Invoicing Job will NOT be automatically added by {0}.\r\nUsers will manually create the Invoicing Job by selecting the Job Invoicing tab themselves OR, by allocating costs through the Accounts Payables module.", Core.Constants.ProductName);

					return new BooleanRegistryItem(
						"AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("5ce22237-0fb8-46b5-9755-a33439af0db9", "Add Job Invoicing Record at Saving/Editing of Operations Job"),
						hint,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Enable Local Charge Code Description Default

		public BooleanRegistryItem EnableLocalChargeCodeDescriptionDefault
		{
			get
			{
				return GetItem("ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT", delegate
				{
					return new BooleanRegistryItem(
						new CountryEnabledBooleanRegistryItemImpl(
						"ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("67a81d89-3d62-44f4-9387-8d6bfcaa5e42", "Enable Local Charge Code Description Default"),
						ResString.GetMultilingualString("31e545a5-a532-4d99-915c-432458715cb1", "In Charge Code, you can maintain translations of the Charge Code description into foreign languages.\r\n\r\nWhen this registry is set to ‘No’, Charge Code descriptions are translated into the foreign language that you used for printing a sales invoice or Rating document.\r\n\r\nWhen this registry is set to ‘Yes’, you can setup Local Language Description in Charge Code which is used as the default description for printing a sales invoice or Rating document to a local client (meaning client domicile in the same country/region as the current local in company domicile country/region).\r\n\r\nYou can always override the description of the charges before posting."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, Core.Constants.CountryCodes.China));
				});
			}
		}

		#region Apply Local Charge Code Description Default to Foreign Debtors

		public class ApplyLocalChargeCodeDescriptionDefaultToForeignDebtorsRegistryItem : RegistryItemImpl
		{
			public ApplyLocalChargeCodeDescriptionDefaultToForeignDebtorsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool defaultValue)
				: base(name, category, caption, hint, RegistryDataTypes.BoolType, storage, options, defaultValue)
			{
			}

			public override bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				return Instance.EnableLocalChargeCodeDescriptionDefault.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK);
			}
		}

		public BooleanRegistryItem ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors
		{
			get
			{
				var item = GetItem("ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors", delegate
				{
					return new BooleanRegistryItem(
					new ApplyLocalChargeCodeDescriptionDefaultToForeignDebtorsRegistryItem(
						"ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("11DC2046-8328-4FD9-8C34-70224A84A732", "Apply Local Charge Code Description Default to Foreign Debtors"),
						ResString.GetMultilingualString("A340C5CE-93C3-4AC1-852E-F9FC7F47B3B9", @"This registry can only be enabled when the  ‘Enable Local Charge Code Description Default’ registry is set to ‘Yes’.

By default, this registry is set to 'No'.
When this registry is set to ‘Yes’, local language description setup in Charge Code will be defaulted when job charges are entered against foreign debtors.
Likewise, local language description will be used when printing rating document to foreign clients."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false));
				});

				item.OnBuildLogReference += (args) => Res.GetString("39E46262-6E12-4098-9615-2CD06A6ACE4A", "Registry value changed from {0} to {1}.", args.OriginalValue, args.NewValue);

				return item;
			}
		}

		#endregion

#if DEBUG
		public
#endif
		class CountryEnabledBooleanRegistryItemImpl : RegistryItemImpl
		{
			public CountryEnabledBooleanRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, params string[] countryCodes)
				: this(name, category, caption, hint, storage, RegistryOptions.Default, countryCodes)
			{
			}
			public CountryEnabledBooleanRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOptions, params string[] countryCodes)
				: base(name, category, caption, hint, new BooleanRegistryDataType(), storage, registryOptions)
			{
				this.countryCodes = countryCodes;
			}

			readonly string[] countryCodes;

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				bool result;

				if (!Cache.TryGetValue(companyPK, out result))
				{
					UpdateCache();
					if (!Cache.TryGetValue(companyPK, out result))
					{
						result = false;
						Cache.Add(companyPK, result);
					}
				}
				return result;
			}

			bool IsEnabledForCountry(string country)
			{
				return countryCodes != null && countryCodes.Any() &&
					(from string enabledCountry in countryCodes where enabledCountry == country select enabledCountry).Any();
			}

			void UpdateCache()
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				ZQuery query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Cache.Keys);
				BusinessObject[] companies = factory.Load<GlbCompany>(query);

				foreach (BusinessObject company in companies)
				{
					Cache.Add(company.PK.ToGuid(), IsEnabledForCountry((ZString)company[GlbCompanySchema.GC_RN_NKCountryCode]));
				}
			}

			Dictionary<Guid, bool> Cache
			{
				get { return cache ?? (cache = new Dictionary<Guid, bool>()); }
			}
			Dictionary<Guid, bool> cache;
#if DEBUG
			public void ClearCacheForTestOnly()
			{
				Cache.Clear();
			}
#endif

		}

		#endregion

		#region Default Job Status

		public BooleanRegistryItem CreateWIPOrAccrualWhenNoInvoicesPosted
		{
			get
			{
				return GetItem("CreateWIPOrAccrualWhenNoInvoicesPosted", delegate
				{
					return new BooleanRegistryItem(
											"CreateWIPOrAccrualWhenNoInvoicesPosted",
											Categories.Accounting_JobInvoicing,
											ResString.GetMultilingualString("b6df5456-9cd3-4c99-abc9-836d76e489e8", "Create WIPs or Accruals When No Invoices Posted"),
											ResString.GetMultilingualString("d632e9b4-741c-4c4b-86b4-fe284a1fd55d", "This registry item controls whether WIPs or Accruals are created before an actual AR or AP transaction is posted on a job.\r\nSet this registry item to false to suspend creation of WIPs and Accruals until an actual AP or AR transactions is posted on the job."),
											RegistryStorageFlags.System | RegistryStorageFlags.Company,
											true);
				});
			}
		}

		#endregion

		#region Recognize Profit on WIPs and Accruals before posting invoice or credit note

		public BooleanRegistryItem RecognizeProfitOnWIPsAccrualsBeforePosting
		{
			get
			{
				return GetItem("RecognizeProfitOnWIPsAccrualsBeforePosting", delegate
				{
					return new BooleanRegistryItem(
						"RecognizeProfitOnWIPsAccrualsBeforePosting",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("5dc0d63c-c959-4997-a4bc-b56fe079f305", "Recognize Profit on WIPs and Accruals before posting invoice or credit note"),
						ResString.GetMultilingualString("c2480c3d-713e-4737-a790-d0f104d4dfb2", "When set to 'Yes' this registry will trigger both the setting of a job's revenue recognition date and the creation of WIP and Accrual transactions when charges are first added into a job's billing screen.  When this registry is overridden to 'Yes' no WIP or Accrual charges can be saved on a job until the appropriate recognition date (e.g. arrival date, pickup date) has also been recorded against the job.\r\nBy default, some revenue recognition setup options (e.g. Actual Pickup; Actual Arrival) do not create WIP or Accrual transactions on a job until the first Accounts Receivable or Payable transaction is actually posted on the job.  For those recognition setups, when this  registry is set to 'No',  charges can be prepared on the job billing screen without an actual WIP or ACR transaction being created."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Set Job Status to Invoiced when first AR invoice posted

		public CodeDescriptionBoolDisallowNewRegistryItem SetJobStatusToInvoicedWhenFirstARInvoicePosted
		{
			get
			{
				return GetItem("SetJobStatusToInvoicedWhenFirstARInvoicePosted", delegate
				{
					return new CodeDescriptionBoolDisallowNewRegistryItem(
						"SetJobStatusToInvoicedWhenFirstARInvoicePosted",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("a729e13b-fe75-475a-8b3c-c16fe1ae83b1", "Set Job Status to Invoiced when AR invoice posted"),
						ResString.GetMultilingualString("d0cdf8f0-6a69-495e-8d65-a3b551b2dbb7", "If a Job's status at the time of invoicing is not ticked as an exception on the list below, the status will be changed to INV when an AR Invoice is posted."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("5287d2fe-2940-461b-88aa-3d8b5bb72350", "Exceptions"), true, true),
						GetJobStausList());
				});
			}
		}

		public bool DoesJobStatusGetChangedToInvoicedWhenARInvoicePosted(ZString currentJobStatus)
		{
			return AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.Value.ContainsCode(currentJobStatus)
						&& !AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.Value.GetBoolFromCode(currentJobStatus);
		}

		CodeDescriptionBoolDisallowNewCollection GetJobStausList()
		{
			var statusList = new JobHeaderStatusList();
			statusList.RemoveCode(JobHeaderStatus.JobInvoiced);

			var codeDescriptionBoolDisallowNewCollection = new CodeDescriptionBoolDisallowNewCollection();
			foreach (CodeDescriptionPair item in statusList)
			{
				bool isTickedOn = (item.Code == JobHeaderStatus.WorkOnHold.Code
									|| item.Code == JobHeaderStatus.InvoiceOnHold.Code
									|| item.Code == JobHeaderStatus.Closed.Code);
				codeDescriptionBoolDisallowNewCollection.Add(item.Code, ResString.GetMultilingualString("2907487A-7706-4516-A80E-651A2180CD93", "{0} - {1}", item.MultilingualCode, item.MultilingualDescription), isTickedOn);
			}
			return codeDescriptionBoolDisallowNewCollection;
		}

		#endregion

		#region Job Profit Reason

		public JobProfitLossReasonCodeRegistryItem JobProfitLossReasonCode
		{
			get
			{
				return GetItem("JobProfitLossReasonCode", delegate
				{
					return new JobProfitLossReasonCodeRegistryItem(
											"JobProfitLossReasonCode",
											Categories.Accounting_JobInvoicing_JobProfitReason,
											ResString.GetMultilingualString("07cc20dc-6a51-49ab-9c05-938241781ee1", "Job Profit/Loss Reason Codes"),
											ResString.GetMultilingualString("07cc20dc-6a51-49ab-9c05-938241781ee1", "Job Profit/Loss Reason Codes"),
											RegistryStorageFlags.System | RegistryStorageFlags.Company,
											new JobProfitLossReasonCodeCollection());
				});
			}
		}

		public JobProfitLossRequiringReasonParametersRegistryItem JobProfitLossRequiringReasonParameters
		{
			get
			{
				return GetItem("JobProfitLossRequiringReasonParameters", delegate
				{
					return new JobProfitLossRequiringReasonParametersRegistryItem(
											"JobProfitLossRequiringReasonParameters",
											Categories.Accounting_JobInvoicing_JobProfitReason,
											ResString.GetMultilingualString("564e6293-087c-4367-81e2-bb7d6abdddad", "Profit Margin Threshold Requiring Reason Codes"),
											ResString.GetMultilingualString("934838d1-aca5-4f45-82f6-882d75b52dcf", "When enabled, the registry will force users to set a Job Profit/Loss Reason code against a job when a job's Status is nominated in the grid below as requiring a Reason Code, AND either:\r\n\ta) The Job's PROFIT / REVENUE margin percentage exceeds the nominated Profit Margin Threshold\r\n\tOR\r\n\tb) The Job's PROFIT / REVENUE margin falls below the nominated Loss Margin Threshold"),
											RegistryStorageFlags.All,
											new JobProfitLossRequiringReasonParameters());
				});
			}
		}

		#endregion

		public CodePairRegistryItem JobDeactivationConfiguration
		{
			get
			{
				return GetItem("JobDeactivationConfiguration", delegate
				{
					var item = new CodePairRegistryItem(
						"JobDeactivationConfiguration",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("2ff8b702-486d-45da-b3d8-3bf65c655015", "Job Deactivation Configuration"),
						ResString.GetMultilingualString("f03faca9-44ed-432c-9003-61f256d290b6",
							@"By default, this registry value will be set to ""DEF - No accounting transaction has been posted"" and  billing job can only be deactivated if no accounting transaction has been posted.

If this registry value is overridden and set to ""NPL - No active WIPs and ACRs"", billing jobs with reversed WIPs and ACRs can be deactivated."),
						new CodeDescriptionPairListProvider(() => new JobDeactivationConfigurations()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingMasterFilesConstants.JobDeactivationConfigurations.Default.Code);

					item.OnBuildLogReference += (args) => Res.GetString("4130bc60-d9bb-4c69-861f-a3e1610dce9a", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

					return item;
				});
			}
		}

		public BooleanRegistryItem JobRevenueJournalPrintPrompting
		{
			get
			{
				return GetItem("JobRevenueJournalPrintPrompting", delegate
				{
					return new BooleanRegistryItem(
						"JobRevenueJournalPrintPrompting",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("B5111DEF-C84C-4757-B290-B82A32F91479", "Job Revenue Journal Prompting"),
						ResString.GetMultilingualString("7FDA761E-9CD3-4BEC-A1EF-033A1BA7E727", @"When enabled system will prompt user to print Job Revenue Journal Document upon saving the Job Revenue Journal."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public CodePairRegistryItem JobRevenueJournalGLAccountDefaultingRules
		{
			get
			{
				return GetItem("JobRevenueJournalGLAccountDefaultingRules", delegate
				{
					var item = new CodePairRegistryItem(
						"JobRevenueJournalGLAccountDefaultingRules",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("5d957035-8a8a-4115-be96-5a508aada87e", "Job Revenue Journal GL Account Defaulting Rules"),
						ResString.GetMultilingualString("be51da19-8cd8-448f-bd77-284bcfcc5b17",
		@"By default, this registry value will be set to 'REV' and the system will use Charge Code's Revenue GL Account.

To use the Charge Code's Cost GL Account, set to 'CST' option.
To use the Charge Code's Cost GL Account for Debit entry and Revenue GL Account for Credit entry, set to 'BTH' option."),
							new CodeDescriptionPairListProvider(() => new JobRevenueJournalGLAccountDefaultingRuleTypes()),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code);

					item.OnBuildLogReference += (args) => Res.GetString("40dbed76-f2ed-4581-8cf2-414e87591358", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

					return item;
				});
			}
		}

		public BooleanRegistryItem SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused
		{
			get
			{
				return GetItem("SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused", delegate
				{
					var item = new BooleanRegistryItem(
						"SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("982735F5-F4F6-4794-B552-BA2C442E20C8", "Set Job Revenue Journal's Line Type based on Cost/Revenue GL Account used"),
						ResString.GetMultilingualString("03866127-28A5-466F-8180-5C57D0A672FF", @"By default, this registry will be set to 'No'

When this registry is set to 'Yes', the system will set the Job Revenue Journal's line type according to the GL Account used.
For instance, if the Cost GL Account is used, then the line type will be set to 'CST'. If the Revenue GL Account is used, then the line type will be set to 'REV'.

Note: You can specify the defaulting rule via the 'Job Revenue Journal GL Account Defaulting Rules' registry. You can override this default during the manual creation of Job Revenue Journal."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							false);

					item.OnBuildLogReference += (args) => Res.GetString("40dbed76-f2ed-4581-8cf2-414e87591358", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

					return item;
				});
			}
		}

		public BooleanRegistryItem PopupARInvoiceDescriptionOverrideOnPosting
		{
			get
			{
				return GetItem("PopupARInvoiceDescriptionOverrideOnPosting", delegate
				{
					return new BooleanRegistryItem(
						"PopupARInvoiceDescriptionOverrideOnPosting",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("48703B4C-7C51-4C29-8F8A-4E598C38B0AD", "Popup AR Invoice Description Override on posting"),
						ResString.GetMultilingualString("8B9BB123-CBBA-4CEE-90BE-D54C0D4A4917", @"When this registry is set to 'Yes', {0} will pop-up a form allowing users to override the description of the transaction header.
The popup will only appear where the posting user has the following security right:
{1}", BrandingFactory.Instance.ProductName, Env.Security.AROverrideTransactionDescription.DisplayTextPathToSecurityRight),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableDeferredRevenueRecognition
		{
			get
			{
				return GetItem("EnableDeferredRevenueRecognition", delegate
				{
					return new BooleanRegistryItem(
						"EnableDeferredRevenueRecognition",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("4ABE5F46-28A6-4AAD-8B06-4ED78A68D1F2", "Enable Deferred Revenue Recognition"),
						ResString.GetMultilingualString("8E1708E8-A8AA-4038-8588-81B33217DB0E", @"When this registry is set to 'Yes', {0} will not recognize transactions until either a specific user action OR a specific workflow trigger action is executed.
You can configure the 'RRV – Recognize Revenue' workflow trigger action on your workflow templates so that revenue recognition behaviors are triggered on a consistent basis. This approach will also ensure that users don't 'forget' to recognize revenue on their jobs. 
When a job's status is set to 'CMP – Complete' or 'CLS – Closed', {0} will automatically recognize any transactions that aren't recognized using the configured 'revenue recognition' date.
Users can also manually trigger this functionality using the 'Recognize Revenue' menu item that's available on all screens with the billing tab.
When this registry is set to 'No', {0} will not allow an AR or AP Invoice to be posted until a recognition date is entered. This is the default behavior of the system.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region eNett

		#region eNett Web Service Location

		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem ENettWebServiceLocation
		{
			get
			{
				return GetItem("ENettWebServiceLocation", delegate
				{
					return new StringRegistryItem(
						"ENettWebServiceLocation",
						Categories.Accounting_ComPay,
						(NoResString)"ComPay Web Service Location",
						(NoResString)"This registry item controls the location of the ComPay web service.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						DefaulteNettWebServiceURL);
				});
			}
		}

		string DefaulteNettWebServiceURL
		{
			get
			{
				string result;
				if (IsProductionDatabase)
				{
					result = "https://compay.1-stop.biz/integrationservice/integrationservice.asmx";
				}
				else
				{
					result = "http://compay20-dev.1-stop.biz/IntegrationService/IntegrationService.asmx";
				}
				return result;
			}
		}

		bool IsProductionDatabase
		{
			get
			{
				return ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production && !Globals.IsTest && !Globals.IsDebugMode;
			}
		}

		#endregion

		#region eNett Notifications

		public GuidRegistryItem ENettNotificationsGroup
		{
			get
			{
				return GetItem("ENettNotificationsGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ENettNotificationsGroup",
						Categories.Accounting_ComPay,
						ResString.GetMultilingualString("61ad280c-a5d6-447b-bd1a-f55b5cf2a5ae", "ComPay Notifications"),
						ResString.GetMultilingualString("07968e97-25f0-4321-8823-82a563d1e6db", "This registry item allows users to define the notification group to receive {0} generated notification emails relating to automated ComPay transaction processing.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company,
						RegistryFactory.Instance.GetGroupPK("ALL"));

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region ComPay Registered Bank Account

		public ENettRegisteredBankAccountRegistryItem ENettRegisteredBankAccount
		{
			get
			{
				return GetItem("ENettRegisteredBankAccount", delegate
				{
					return new ENettRegisteredBankAccountRegistryItem(
						"ENettRegisteredBankAccount",
						Categories.Accounting_ComPay,
						ResString.GetMultilingualString("a9a1d05c-8cbd-4dd5-9f36-ed5085ebf279", "ComPay Registered Bank Accounts"),
						ResString.GetMultilingualString("c709924c-bc73-41cd-a6cf-8fa493760c51", @"This registry identifies the bank accounts used by each login company for creating and remitting payments via ComPay. Only accounts flagged here can have 'ComPay' payments created against them. 
This registry is also used to identify the bank account used by each login company for ComPay receipts. Only one 'Default Receipt' bank account can be recorded for each login company.
Note: The default receipt account defined here must be the same account registered with ComPay."),
						RegistryStorageFlags.Company);
				});
			}
		}

		public ENettRegistrationRegistryItem ENettRegistration
		{
			get
			{
				return GetItem("ENettRegistration", delegate
				{
					return new ENettRegistrationRegistryItem(
						 "ENettRegistration",
						 Categories.Accounting_ComPay,
						 ResString.GetMultilingualString("56eddfed-4c49-4603-8bc3-9aa7398b58d2", "ComPay Registration"),
						 ResString.GetMultilingualString("2a30b457-b734-42c3-bb14-d0499bf3eef7", @"This registry is used to record the ComPay registration code and ComPay authentication code for use of the ComPay system."),
						 RegistryStorageFlags.Company);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem ENettIntegratorKey
		{
			get
			{
				return GetItem("ENettIntegratorKey", delegate
				{
					return new StringRegistryItem(
						 "ENettIntegratorKey",
						 Categories.Accounting_ComPay,
						 (NoResString)"ComPay Integrator Key",
						 (NoResString)"This is a developer only item that shows the Integrator Key depending on whether the system is live or otherwise",
						 RegistryStorageFlags.All,
						 RegistryOptions.IsOnlyForDevelopers,
						 DefaultIntegratorKey);
				});
			}
		}

		string DefaultIntegratorKey
		{
			get
			{
				string result = "key";
				if (IsProductionDatabase)
				{
					result = "hd72knx854vd6HQUBE62njg5dH892vFWhuK81vwBjfy6G8qvst6kmhd6b3r1bf7K";
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region SuppressResourceStringsCheckRegion

		#region eNett Get Last Payment Date

		public DateTimeRegistryItem ENettGetLastPaymentDate
		{
			get
			{
				return GetItem("ENettGetLastPaymentDate", delegate
				{
					return new DateTimeRegistryItem(
						"ENettGetLastPaymentDate",
						Categories.Accounting_ComPay,
						(NoResString)"ComPay Get Last Payment Date",
						(NoResString)@"This identifies the last date and time an AR Receipt was received into CargoWiseOne from the ComPay service provider.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers, ZDateTime.Now.ToDateTime());
				});
			}
		}

		#endregion

		#region eNett Get Last Invoice Date

		public DateTimeRegistryItem ENettGetLastInvoiceDate
		{
			get
			{
				return GetItem("ENettGetLastInvoiceDate", delegate
				{
					return new DateTimeRegistryItem(
						"ENettGetLastInvoiceDate",
						Categories.Accounting_ComPay,
						(NoResString)"ComPay Get Last Invoice Date",
						(NoResString)"This identifies the last date and time an AP invoice was received into CargoWiseOne from the ComPay service provider.",
							RegistryStorageFlags.Company,
							RegistryOptions.IsOnlyForDevelopers, ZDateTime.Now.ToDateTime());
				});
			}
		}

		#endregion

		#region eNett Get Last Cancelled Invoices Date

		public DateTimeRegistryItem ENettGetLastCancelledInvoicesDate
		{
			get
			{
				return GetItem("ENettGetLastCancelledInvoicesDate", delegate
				{
					return new DateTimeRegistryItem(
						"ENettGetLastCancelledInvoicesDate",
						Categories.Accounting_ComPay,
						(NoResString)"ComPay Get Last Canceled Invoices Date",
						(NoResString)"This identifies the last date and time invoice cancellations were received into CargoWiseOne from the ComPay service provider.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForDevelopers, ZDateTime.Now.ToDateTime());
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#endregion

		public BooleanRegistryItem ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting
		{
			get
			{
				return GetItem("ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting", () =>
					new BooleanRegistryItem("ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						(NoResString)"AR invoice store and use invoice issuer and recepient information during post when printing",
						(NoResString)"If the registry is set to 'Yes' store issuer and recepient information during invoice post and then use that information when printing the invoice.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public GuidRegistryItem ARAccountGroup
		{
			get
			{
				return GetItem("ARAccountGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("ARAccountGroup",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("668c708b-020a-4210-a29e-32caf08c7244", "Account Group"),
						ResString.GetMultilingualString("fda347c4-96ec-4ead-b8b4-eb3f750bc971", "Use this registry to define the A/R Account Group defaulted against each new Receivable Organization as it is created. When overridden, the Debtor Group defined against this registry is used to set an organization’s A/R Account Group when creating new Receivable Organizations."),
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgDebtorGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem APAccountGroup
		{
			get
			{
				return GetItem("APAccountGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("APAccountGroup",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("668c708b-020a-4210-a29e-32caf08c7244", "Account Group"),
						ResString.GetMultilingualString("d36734fa-30cd-40de-b83c-e3e6e0b13a5c", "Use this registry to define the A/P Account Group defaulted against each new Payable Organization as it is created. When overridden, the Creditor Group defined against this registry is used to set an organization’s A/P Account Group when creating new Payable Organizations."),
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.OrgCreditorGroup);
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableAutoAccrualMatching
		{
			get
			{
				return GetItem("EnableAutoAccrualMatching", () =>
					new BooleanRegistryItem("EnableAutoAccrualMatching",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						(NoResString)"Enable Auto Accrual Matching",
						(NoResString)"Enable Auto Accrual Matching when allocating Tranaction Pending Allocations (For Developers Only)",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false));
			}
		}

		public BooleanRegistryItem OverrideInterOfficeBillingTaxIDToNOTREPORT
		{
			get
			{
				return GetItem("OverrideInterOfficeBillingTaxIDToNOTREPORT", delegate
				{
					return new BooleanRegistryItem("OverrideInterOfficeBillingTaxIDToNOTREPORT",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						ResString.GetMultilingualString("c7a36c37-6693-40ff-ab11-29906b7ac722", "{0} Group Member Billing Default Tax ID", TheTaxCode),
						ResString.GetMultilingualString("86d8403b-274d-4b30-bfc2-4962fac942c0", @"This registry is relevant to Companies flagged for VAT, GST, Consumption Tax etc.
By default, {0} will record the ‘Not Reportable’ Tax ID against each AR and AP Charge Line WHEN the Organization is considered part of the same VAT/GST Group as the current Login Company/Branch.
An Organization is considered part of the same VAT/GST Group as the current Login Company/Branch WHEN the organization:
1. Has the same Tax Registration details as the current Login Company, OR
2. Is an Organization proxy of the Login Company/Branch, OR
3. Has the Organization Proxy of the current Login Company/Branch listed as an ‘ACG - Accounting VAT/GST Group’ related party.
When overridden to NO, the standard tax defaulting rules (i.e. Tax ID and Tax Message) for each charge code will apply.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company, true);
				});
			}
		}

		public BooleanRegistryItem OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy
		{
			get
			{
				return GetItem("OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy", delegate
				{
					return new BooleanRegistryItem("OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						ResString.GetMultilingualString("2e0ba968-b023-43a0-9428-daa6ae74db36", "{0} Group Member Billing For Branch Organization Proxy", TheTaxCode),
						ResString.GetMultilingualString("47edcd23-02b4-4e70-b563-c74523d2bd39", "When overridden and set to True, {0} will record Not Reportable Tax ID against each AR and AP charge line when the Debtor/Creditor Organization is a Branch Organization Proxy of the Login Company and has the same GST/VAT registration number as the Charge Line Branch.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company, false);
				});
			}
		}

		public GuidRegistryItem GroupMemberBillingDefaultInvoiceTaxMessage
		{
			get
			{
				return GetItem("GroupMemberBillingDefaultInvoiceTaxMessage", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("GroupMemberBillingDefaultInvoiceTaxMessage",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						ResString.GetMultilingualString("6a9642b2-4a67-4356-acb7-9cb0c68398d1", "{0} Group Member Billing Default Invoice Tax Message", TheTaxCode),
						ResString.GetMultilingualString("d0816fc7-627c-4a99-b68b-744448c5b927", @"This registry is relevant to companies flagged for VAT, GST, Consumption Tax etc.
{0} will record the Invoice Tax Message nominated in this registry against each AR and AP Charge Line WHEN the Organization is considered part of the same VAT/GST Group as the current Login Company/Branch.
An Organization is considered part of the same VAT/GST Group as the current Login Company/Branch WHEN the organization:
1. Has the same Tax Registration details as the current Login Company, OR
2. Is an Organization proxy of the Login Company/Branch, OR
3. Has the Organization Proxy of the current Login Company/Branch listed as an ‘ACG - Accounting VAT/GST Group’ related party.
NOTE: For this registry to work, the Login Company must have the ‘GST/VAT Group Member Billing Default Tax ID' registry set to 'YES'.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccInvMsg);
					return result;
				});
			}
		}

		#region Purchase Order

		public CodeDescriptionPairListRegistryItem APOrderLineStatusCodesList
		{
			get
			{
				CodeDescriptionPairList lookUpList = new CodeDescriptionPairList(OLookUpEditType.PayableOrderLineStatus);

				return GetItem("APOrderLineStatusCodesList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"APOrderLineStatusCodesList",
						Categories.Accounting_PurchaseOrders,
						ResString.GetMultilingualString("80239259-7463-4425-92d6-40c74ff5dfaf", "Order Line Status List"),
						ResString.GetMultilingualString("f1fbc363-6128-4879-864e-1c15da28feab", @"Order Line Status List"),
						3,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						lookUpList
						);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem GoodsReceivedStatusCodesList
		{
			get
			{
				CodeDescriptionPairList lookUpList = new CodeDescriptionPairList(OLookUpEditType.PayableOrderGoodsStatus);

				return GetItem("GoodsReceivedStatusCodesList", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"GoodsReceivedStatusCodesList",
						Categories.Accounting_PurchaseOrders,
						ResString.GetMultilingualString("e9eedce3-7d41-47d7-a42c-98423aa05238", "Goods Received Notes"),
						ResString.GetMultilingualString("f7f514df-9902-4071-8afc-22c8e052b18b", @"The Goods Received Note is a record of goods received at the point of receipt.
This record is used to confirm all goods have been received and compared to the purchase order for payment validation purposes. The default list may be modified when overridden."),
						3,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						lookUpList
						);
				});
			}
		}

		public BooleanRegistryItem AllowEditOrderLineStatus
		{
			get
			{
				return GetItem("AllowEditOrderLineStatus", delegate
				{
					return new BooleanRegistryItem("AllowEditOrderLineStatus", Categories.Accounting_PurchaseOrders,
						ResString.GetMultilingualString("78956a90-323e-4691-9c30-1b6ebaf699c0", "Order Line Status field editable"),
						ResString.GetMultilingualString("78956a90-323e-4691-9c30-1b6ebaf699c0", "Order Line Status field editable"),
						RegistryStorageFlags.Company | RegistryStorageFlags.System, true);
				});
			}
		}

		public BooleanRegistryItem EnableStageAndDispositionOverride
		{
			get
			{
				return GetItem("EnableStageAndDispositionOverride", delegate
				{
					return new BooleanRegistryItem("EnableStageAndDispositionOverride", Categories.Accounting_PurchaseOrders,
						ResString.GetMultilingualString("9a86a2bd-fd23-4e40-b32c-510963844dcb", "Enable Order Status Override"),
						ResString.GetMultilingualString("3f5c6959-473d-4c82-85f8-c40cd4e1039c", @"Purchase Order Status is updated automatically by events and actions.
When this registry is set to 'Yes' the fields will no longer be read only and allow manual overrides, however won't stop the automated status updates."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
				});
			}
		}

		public BooleanRegistryItem APOrderLineQtyRemainingManagement
		{
			get
			{
				return GetItem("APOrderLineQtyRemainingManagement",
					() => new BooleanRegistryItem("APOrderLineQtyRemainingManagement",
								Categories.Accounting_PurchaseOrders,
								ResString.GetMultilingualString("8e9eb8c7-3f3e-4f45-be19-9e3737b6a66b", "Order Line Quantity Remaining Management"),
								ResString.GetMultilingualString("e73e7136-7b5b-4b1a-bfc2-dda2f3bba7f6", @"This registry item configures whether the Quantity Remaining field is based on Quantity Received or Quantity Invoiced.

Set this value to 'Yes' to use Quantity Received to calculate remaining quantity and update Quantity Received when you change Quantity Invoiced.
Set this value to 'No' to use Quantity Invoiced to calculate remaining quantity."),
					RegistryStorageFlags.Company | RegistryStorageFlags.System,
					true));
			}
		}

		public CodeDescriptionBoolDisallowNewRegistryItem PayableOrderApprovalRestriction
		{
			get
			{
				return GetItem("PayableOrderApprovalRestriction", delegate
				{
					return new CodeDescriptionBoolDisallowNewRegistryItem(
						"PayableOrderApprovalRestriction",
						Categories.Accounting_PurchaseOrders,
						ResString.GetMultilingualString("97afb322-dde7-47ee-ad2b-78a1cbd608d6", "Enable Order Approval Restrictions"),
						ResString.GetMultilingualString("acb947c5-1ecf-46ec-a5c2-abb04fa8e09a", @"This registry controls when a Purchase Order can be approved.

Creator(PO initiator) Approval restriction: When this restriction is enabled, a Purchase Order may not be approved by the same user who initiated (created) it. When this restriction is not enabled, the initiator may approve their own Purchase Order as long as it is within their level of authority.

Blank Suppliers restriction: When this restriction is enabled, the Purchase Order may not be approved if the Supplier is blank.

Overridden Supplier Details restriction: When this restriction is enabled, the Purchase Order may not be approved if the Supplier details are manually overridden. A valid Supplier Organization must be selected.

Temporary Organizations restriction: When this restriction is enabled, the Purchase Order may not be approved if it is associated with a Supplier Organization flagged as 'Temporary Account'."),
						RegistryStorageFlags.Company,
						new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("ce4fd4b2-48fd-4608-bea2-c3772dd008de", "Is Enabled"), true, true),
						GetPayableOrderRestrictionList()
						);
				});
			}
		}

		CodeDescriptionBoolDisallowNewCollection GetPayableOrderRestrictionList()
		{
			var result = new CodeDescriptionBoolDisallowNewCollection(new PayableOrderRestrictionList());
			foreach (CodeDescriptionBool item in result)
			{
				item.Bool = item.Code != PayableOrderRestrictionList.Codes.SupplierIsTempOrg;
			}
			return result;
		}

		public PaymentThreeLevelAuthorisationSettingsRegistryItem PayableOrderAuthorizationSettings
		{
			get
			{
				return GetItem("PayableOrderAuthorizationSettings", delegate
				{
					return new PaymentThreeLevelAuthorisationSettingsRegistryItem(
						"PayableOrderAuthorizationSettings",
						Categories.Accounting_PurchaseOrders,
						ResString.GetMultilingualString("b974bf66-d7fd-4a52-b459-a5b54446501a", "Authorization Setting"),
						ResString.GetMultilingualString("84e35aaa-a7d7-43e8-81e1-9342ffad23d7", "This registry item allows you to specify the authorization required to fully approve an unapproved purchase order.  You can set up the authorization required based on the local value of the payment.  When no options are set, all payment values will require three approvals.  The system will allow you to specify required authorization for different ranges. You must specify at least one \"Up to\" line and only one \"Above\" line"),
						RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region Cash Book Defaults

		public CodePairRegistryItem DefaultCashBookReceiptType
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() => new CodeDescriptionPairList(OLookUpEditType.ReceiptMethod));

				return GetItem("DefaultCashBookReceiptType", delegate
				{
					return new CodePairRegistryItem(
						"DefaultCashBookReceiptType",
						Categories.Accounting_CashBookDefaults,
						ResString.GetMultilingualString("8dc59829-563a-4dbf-8326-81c8eda2f931", "Default Cashbook Receipt Type"),
						ResString.GetMultilingualString("639c3c4a-2fe5-4bc7-8f3c-507845ed2fbd", @"This registry item provides you with the ability to nominate the default Receipt Type for any new direct or opening receipt created in the Cashbook. 

By default the Receipt Type is CHQ – cheque"),
						listProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ReceiptTypes.Cheque);
				});
			}
		}

		public CodePairRegistryItem DefaultCashBookPaymentType
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() =>
				{
					var list = new CodeDescriptionPairList(OLookUpEditType.PaymentMethod);
					list.RemoveCode(ReceiptTypes.EPayment);
					return list;
				});

				return GetItem("DefaultCashBookPaymentType", delegate
				{
					return new CodePairRegistryItem(
						"DefaultCashBookPaymentType",
						Categories.Accounting_CashBookDefaults,
						ResString.GetMultilingualString("e1772955-a628-4f99-b130-eb85db0dde7c", "Default Cashbook Payment Type"),
						ResString.GetMultilingualString("fc292c7b-a7e4-4f60-bcb8-61f847c366bd", @"This registry item provides you with the ability to nominate the default Payment Type for any new direct or opening payment created in the Cashbook. 

By default the Payment Type is CHQ – cheque "),
						listProvider,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						ReceiptTypes.Cheque);
				});
			}
		}

		public DirectDebitFileCreationURLRegistryItem DirectDebitFileCreationURLs
		{
			get
			{
				return GetItem("DirectDebitFileCreationURLs", delegate
				{
					return new DirectDebitFileCreationURLRegistryItem(
						"DirectDebitFileCreationURLs",
						Categories.Accounting_CashBookDefaults,
						ResString.GetMultilingualString("8911f977-d85e-48af-9d82-833ba3520f45", "URLs for Direct Debit File Creation"),
						ResString.GetMultilingualString("b77c70ca-5596-4bfb-a743-f5627f8b2a74", "Use this setting to specify the URL of the Bank's web page. If this registry has been set up for the bank account, the users will be prompted whether they would like to open the Bank's web page after the Direct Debit Batch has been created."),
						RegistryStorageFlags.Company
						);
				});
			}
		}

		public GuidRegistryItem BankTransactionGLAccount
		{
			get
			{
				return GetItem("BankTransactionGLAccount", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"BankTransactionGLAccount",
						Categories.Accounting_CashBookDefaults,
						ResString.GetMultilingualString("55a14b8a-5a55-498e-8588-5e7c01d5ea9d", "Bank Transaction GL Account"),
						ResString.GetMultilingualString("55a14b8a-5a55-498e-8588-5e7c01d5ea9d", "Bank Transaction GL Account"),
						RegistryStorageFlags.System,
						RegistryOptions.IsValueMandatory);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSH);
					return result;
				});
			}
		}

		public BooleanRegistryItem AutomaticallySaveReconciliationReportWhenSavingBankReconciliation
		{
			get
			{
				return GetItem("AutomaticallySaveReconciliationReportWhenSavingBankReconciliation",
					() => new BooleanRegistryItem(
						"AutomaticallySaveReconciliationReportWhenSavingBankReconciliation",
						Categories.Accounting_CashBookDefaults,
						ResString.GetMultilingualString("5c9be839-1af4-49f3-b8d0-ab044054711a", "Automatically Save Reconciliation Report When Saving Bank Reconciliation"),
						ResString.GetMultilingualString("e60d6eef-93f7-40b1-9127-eee3cdfeb089", "Set this registry to 'No' to disable saving the reconciliation history report when you save the Bank Reconciliation."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true));
			}
		}

		public BooleanRegistryItem EnableChequeManagementFunctionality
		{
			get
			{
				return GetItem("EnableChequeManagementFunctionality",
					() => new BooleanRegistryItem(
						"EnableChequeManagementFunctionality",
						Categories.Accounting_CashBookDefaults,
						(NoResString)"Enable Cheque Management Functionality (CargoWiseOne Support Only)",
						(NoResString)@"It's used to control the Cheque Management functionality. By default, this is set to 'No', when enabled Cheque transactions and tracking module will be available.

**PLEASE DO NOT ENABLE THIS REGISTRY FOR ANY CUSTOMER**",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public BooleanRegistryItem AllowFuturePostingOfCashBookTransactions
		{
			get
			{
				return GetItem("AllowFuturePostingOfCashBookTransactions",
					() => new BooleanRegistryItem(
						"AllowFuturePostingOfCashBookTransactions",
						Categories.Accounting_CashBookDefaults,
						ResString.GetMultilingualString("37DB64C8-3B1C-4ea6-9137-EC76DCAF5094", "Allow Future Posting of Cash Book Transactions"),
						ResString.GetMultilingualString("991D24E1-28F9-40b1-97E7-FF142FF6EC20", @"When this registry item is set to 'yes', users with appropriate security permissions can future post the following types of transactions: 

AR Payment, AR Receipt, AR Receipt Batch, AP Payment, AP Receipt, AP Payment Batch, CB Direct Receipt, CB Direct Payment, CB Direct Debit Batch. "),
						RegistryStorageFlags.Company,
						false));
			}
		}

		public BooleanRegistryItem AllowManualEntryOfStatementDateWhenEnteringBankStatement
		{
			get
			{
				return GetItem("AllowManualEntryOfStatementDateWhenEnteringBankStatement",
					() => new BooleanRegistryItem(
						"AllowManualEntryOfStatementDateWhenEnteringBankStatement",
						Categories.Accounting_CashBookDefaults,
						(NoResString)"Allow Manual Entry of Statement Date when entering Bank Statement (CargoWiseOne Support Only)",
						(NoResString)@"When this registry item is set to 'yes', users can enter their own statement dates on the 'Edit Bank Statement' screen.
Please consult with the Accounting Product Team before changing the value of this registry setting.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		#endregion

		public AccountingRegistryItem APMatchingSessionControlAccount
		{
			get
			{
				return GetItem("APMatchingSessionControlAccount", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"APMatchingSessionControlAccount",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("FFADA9EE-5216-45A7-BBDF-480DCDC8C0C1", "AP Matching Session Control Account"),
						ResString.GetMultilingualString("76FE403D-2667-4276-BEA1-2523B974243E", "When the registry ‘Automatically Create Balancing Journals When Importing Remittance File’ is set to ‘Yes’, {0} will use this general ledger account when creating balancing journals creating during the remittance file import.", BrandingFactory.Instance.ProductName));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public AccountingRegistryItem ARMatchingSessionControlAccount
		{
			get
			{
				return GetItem("ARMatchingSessionControlAccount", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"ARMatchingSessionControlAccount",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("FA62999F-7A0C-4FD8-9C52-2DF7283EDFA5", "AR Matching Session Control Account"),
						ResString.GetMultilingualString("76FE403D-2667-4276-BEA1-2523B974243E", "When the registry ‘Automatically Create Balancing Journals When Importing Remittance File’ is set to ‘Yes’, {0} will use this general ledger account when creating balancing journals creating during the remittance file import.", BrandingFactory.Instance.ProductName));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public AccountingRegistryItem GLJournalClearingAccount
		{
			get
			{
				return GetItem("GL_JOURNAL_CLEARING_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("b04a2373-a95b-43aa-aa5d-24380f672476", "GL Journal Clearing Account");
					AccountingRegistryItem result = new AccountingRegistryItem(
						"GL_JOURNAL_CLEARING_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						captionAndHint);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public AccountingRegistryItem ARJournalAccount
		{
			get
			{
				return GetItem("GL_AR_JOURNAL_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("48ad1dac-706a-4c5a-96c2-0321ba2c41e0", "AR Journal Clearing Account");
					AccountingRegistryItem result = new AccountingRegistryItem(
						"GL_AR_JOURNAL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						captionAndHint);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public AccountingRegistryItem APJournalAccount
		{
			get
			{
				return GetItem("GL_AP_JOURNAL_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("768bc264-a2f6-45d3-bfa0-7c90b7076ccd", "AP Journal Clearing Account");
					AccountingRegistryItem result = new AccountingRegistryItem(
						"GL_AP_JOURNAL_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						captionAndHint);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public AccountingRegistryItem PeriodApportionmentARClearingAccount
		{
			get
			{
				return GetItem(nameof(PeriodApportionmentARClearingAccount), () =>
				{
					var result = new AccountingRegistryItem(
						nameof(PeriodApportionmentARClearingAccount),
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("B5001FC0-D08A-4F2C-A2C8-4266BA489214", "Multi-Period Apportionment Revenue Clearing Account"),
						ResString.GetMultilingualString("7D00D83C-30FD-4C59-B577-A61E16E6CA9F", @"This account is used when you apportion the invoice line revenue across multiple GL periods.
For example, if your Receivable Invoice covers revenue that is recognized across multiple periods (known as service periods), then:
* The total revenue amount is posted against this Clearing account in the posting period of the invoice;
* A separate journal is created in each service period, which reduces the balance of the clearing account in the Balance Sheet and recognizes the actual revenue in the Profit & Loss Statement."));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl_AllowDirectPost, ClrAccErrMsg);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		static IMultilingualString ClrAccErrMsg => ResString.GetMultilingualString("7AA58C9E-D58B-432D-B16B-793E23DACFEE", "Clearing Account must be a Balance Sheet account and must allow Direct Posting.");

		public AccountingRegistryItem PeriodApportionmentAPClearingAccount
		{
			get
			{
				return GetItem(nameof(PeriodApportionmentAPClearingAccount), () =>
				{
					var result = new AccountingRegistryItem(
						nameof(PeriodApportionmentAPClearingAccount),
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("2BDB1B8A-A9EA-44A6-A589-7F88C65ACB6D", "Multi-Period Apportionment Cost Clearing Account"),
						ResString.GetMultilingualString("51BC6807-7946-4BCF-9150-6B06C73B20AA", @"This account is used when you apportion the invoice line cost across multiple GL periods.
For example, if your Payable Invoice covers cost that is recognized across multiple periods (known as service periods), then:
* The total cost amount is posted against this Clearing account in the posting period of the invoice;
* A separate journal is created in each service period, which reduces the balance of the clearing account in the Balance Sheet and recognizes the actual cost in the Profit & Loss Statement."));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl_AllowDirectPost, ClrAccErrMsg);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public GuidRegistryItem ClearingJournalClearingAccount
		{
			get
			{
				return GetItem("ClearingJournalClearingAccount", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"ClearingJournalClearingAccount",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						ResString.GetMultilingualString("E2608DDC-1741-4B16-9484-CDE670656ED1", "Clearing Journal Clearing Account"),
						ResString.GetMultilingualString("E1E637D8-4928-4EA5-BD4C-ECDE544CA14A", @"This link account is required when the related 'Clearing Journal Configuration' registry is not 'STD'.
When the Clearing Journal Configuration registry is something other than 'STD' {0} will automatically create AR and AP Match Session Clearing Journals. The GL Account nominated here will be used when creating those journals.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public AccountingRegistryItem FinanceChargesAccount
		{
			get
			{
				return GetItem("GL_FINANCE_CHG_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("d8762485-d8d0-4feb-a367-0c84757696e4", "Finance Charges Account");
					AccountingRegistryItem result = new AccountingRegistryItem(
						"GL_FINANCE_CHG_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						captionAndHint);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandL);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public AccountingRegistryItem BadDebtWriteOffAccount
		{
			get
			{
				return GetItem("GL_BAD_DEBT_WRITE_OFF_ACCOUNT", delegate
				{
					MultilingualString captionAndHint = ResString.GetMultilingualString("995fdec8-24f2-4e11-afdc-2bda30432d0a", "Bad Debt Write-Off Account");
					AccountingRegistryItem result = new AccountingRegistryItem(
						"GL_BAD_DEBT_WRITE_OFF_ACCOUNT",
						Categories.Accounting_GeneralLedgerDefaults_LinkAccount,
						captionAndHint,
						captionAndHint);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.PandLOrBSH);
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		public AccountingRegistryItem DiscrepancyGLAccount
		{
			get
			{
				return GetItem("Discrepancy_GL_Account", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"Discrepancy_GL_Account",
						Categories.Accounting_PayableDefaults_DefaultSettings_BulkAPInvoicePosting,
						ResString.GetMultilingualString("38f375d0-8e1b-4394-b734-79cdd08eb9c6", "Discrepancy GL Account"),
						ResString.GetMultilingualString("3374b67b-39a2-4e26-81ac-a5a26060fbb2", @"Enter the account that discrepancies between accrued cost and actual cost will be posted to from the Bulk AP Invoice Posting option.
The maximum value of discrepancy tolerated is defined in the registry 'Max Accrual vs Actual Discrepancies'."));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl);
					return result;
				});
			}
		}

		public AccountingRegistryItem PLAppropriationAccount
		{
			get
			{
				return GetItem("GL_PL_APPROPRIATION_ACCOUNT", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"GL_PL_APPROPRIATION_ACCOUNT",
						Categories.Accounting_Framework,
						ResString.GetMultilingualString("2c2c8267-af22-4c46-8eb5-d26f6e2d4f2e", "PL Appropriation Account"),
						ResString.GetMultilingualString("2c2c8267-af22-4c46-8eb5-d26f6e2d4f2e", "PL Appropriation Account"),
						PLAppropriationAccountDefaultValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndControl);
					result.Options = RegistryOptions.IsValueMandatory;
					result.DataType = new WithoutCheckExistAccountDataType();
					return result;
				});
			}
		}

		Guid PLAppropriationAccountDefaultValue
		{
			get
			{
				var gl = RegistryFactory.Instance.LoadFromNaturalKey<AccGLHeader>(
					AccGLHeaderSchema.AG_AccountNum, AccountingConstants.RegistryDefaultValues.PLAppropriationAccount);
				return gl == null ? Guid.Empty : gl.PK.ToGuid();
			}
		}

		public AccountingRegistryItem BSAccountStartAccount
		{
			get
			{
				return GetItem("GL_BS_ACCOUNT_START", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"GL_BS_ACCOUNT_START",
						Categories.Accounting_Framework_ReportOrder_Global,
						ResString.GetMultilingualString("f1ee842b-c569-4c65-b723-8bd2bcd582c3", "Balance Sheet Account Starts At"),
						ResString.GetMultilingualString("f269d815-1ba5-4d6e-84d9-4989c389a4c8", "Please select the first GL Account that Balance Sheet reports start from."));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.HDR);
					return result;
				});
			}
		}

		public AccountingRegistryItem GrossProfitTotalAccount
		{
			get
			{
				return GetItem("GL_GROSS_PROFIT_TOTAL_ACCOUNT", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"GL_GROSS_PROFIT_TOTAL_ACCOUNT",
						Categories.Accounting_Framework_ReportOrder_Global,
						ResString.GetMultilingualString("b1c74fc3-6f4e-4922-a369-a3d9773137fe", "Gross Profit Total Account"),
						ResString.GetMultilingualString("0505b4ac-1f52-4da1-bb6e-4886b5da1e73", "Please select the GL Account representing the Gross Profit figure."));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.TTL);
					return result;
				});
			}
		}

		public AccountingRegistryItem OverheadTotalAccount
		{
			get
			{
				return GetItem("GL_OVERHEAD_TOTAL_ACCOUNT", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"GL_OVERHEAD_TOTAL_ACCOUNT",
						Categories.Accounting_Framework_ReportOrder_Global,
						ResString.GetMultilingualString("64858af5-0f4f-4fee-ae18-5bf4f63a7c4d", "Overhead Total Account"),
						ResString.GetMultilingualString("19501dd4-471b-4c2a-9beb-ab49336d3808", "Please select the GL Account representing the Overhead figure."));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.TTL);
					return result;
				});
			}
		}

		public AccountingRegistryItem NetProfitTotalAccount
		{
			get
			{
				return GetItem("GL_NET_PROFIT_TOTAL_ACCOUNT", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"GL_NET_PROFIT_TOTAL_ACCOUNT",
						Categories.Accounting_Framework_ReportOrder_Global,
						ResString.GetMultilingualString("8acb914d-1fc1-4c71-bf84-90eaf0d90b2b", "Net Profit Total Account"),
						ResString.GetMultilingualString("bbe2c1cd-6063-4a62-a432-4eb76389440f", "Please select the GL Account representing the Net Profit figure."));
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.TTL);
					return result;
				});
			}
		}

		public GuidRegistryItem GLJournalsApprovalNotifyGroup
		{
			get
			{
				return GetItem("GLJournalsApprovalNotifyGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"GLJournalsApprovalNotifyGroup",
						Categories.Accounting_EmailNotification,
						ResString.GetMultilingualString("A9C1FEED-C2CD-43AB-AAAB-1C3E81D373D7", "GL Journals Approval Notify Group"),
						ResString.GetMultilingualString("02760B48-AA00-4B65-9239-F3AB4E5F07EB", "Notify Party whenever GL Journal Approval Requests are raised by the users."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem ComplianceInvoiceBookAllocaltionFailureNotificationGroup
		{
			get
			{
				return GetItem("ComplianceInvoiceBookAllocaltionFailureNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("ComplianceInvoiceBookAllocaltionFailureNotificationGroup", Categories.Accounting_EmailNotification, ResString.GetMultilingualString("903c389a-5773-4cc8-8360-fa4154feb220", "Compliance Invoice Book Allocation Failure Notification Group"),
						ResString.GetMultilingualString("1233a67e-0acf-44e0-93e5-40aea832d571", "Email Notifications advising when a Compliance Invoice Book needs to be configured because an unsuccessful attempt was made to assign a Compliance Government Invoice number. This notification will identify when a Compliance Invoice Book a particular Sub Type and Branch needs to be added."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem DebtorCreditLimitNotifyGroup
		{
			get
			{
				return GetItem("DebtorCreditLimitNotifyGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("DebtorCreditLimitNotifyGroup", Categories.Accounting_EmailNotification, ResString.GetMultilingualString("fea1fc26-30c4-4186-93c9-46d586c7d681", "AR Control Breach Notify Group"),
						ResString.GetMultilingualString("516dd570-0dde-4eea-9af8-13cafb2e0092", "Notify Party when any of the following controls breach in relation to Account Receivables Transaction has occurred.\r\nThese include:\r\n    - Exceeding Credit Limit granted.\r\n    - Deviation from Credit Term granted"),
						RegistryStorageFlags.Company | RegistryStorageFlags.System | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem CreditorCreditLimitNotifyGroup
		{
			get
			{
				return GetItem("CreditorCreditLimitNotifyGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CreditorCreditLimitNotifyGroup", Categories.Accounting_EmailNotification, ResString.GetMultilingualString("a679bab8-725d-4179-aa0a-d23d25a4ece2", "AP Control Breach Notify Group"),
						ResString.GetMultilingualString("4ecfa3e8-089f-437a-b841-f7b927149980", "Notify Party when any of the following controls breach in relation to Account Payables Transaction has occurred.\r\nThese include:\r\n    - Exceeding Credit Limit granted"),
						RegistryStorageFlags.Company | RegistryStorageFlags.System | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem JobReopenNotifyGroup
		{
			get
			{
				return GetItem("JobReopenNotifyGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("JobReopenNotifyGroup", Categories.Accounting_EmailNotification, ResString.GetMultilingualString("fa159618-b1ee-4cd9-a9ad-6bab9974cc5a", "Job Reopen Notify Group"), ResString.GetMultilingualString("6fa34265-dd6a-44bf-9d33-cb85cdf75ac5", "Notify Party when Closed Job is re-opened"), RegistryStorageFlags.Company | RegistryStorageFlags.System | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem TransactionReverseNotifyGroup
		{
			get
			{
				return GetItem("TransactionReverseNotifyGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("TransactionReverseNotifyGroup", Categories.Accounting_EmailNotification, ResString.GetMultilingualString("befa860f-c4c6-453c-9283-210ac7830b63", "Transaction Reverse Notify Group"), ResString.GetMultilingualString("13C69D78-28E8-4633-8CD4-18F7B384D8FC", "Notify Party when Transaction is being reversed"), RegistryStorageFlags.Company | RegistryStorageFlags.System | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem TransactionUnmatchedNotifyGroup
		{
			get
			{
				return GetItem("TransactionUnmatchedNotifyGroup", delegate
				{
					MultilingualString transactionUnmatchedNotifyGroupHint = ResString.GetMultilingualString("4fbee6e3-f24e-40a2-8fff-707a83994abf", "Enter or Click to select from Notification Group when Match Transaction is being unmatched.\r\nAddressee will be informed of the Transaction Date, Type, Number, Paid Date and Paid Amount together with the match number.");

					GuidRegistryItem result = new GuidRegistryItem("TransactionUnmatchedNotifyGroup", Categories.Accounting_EmailNotification, ResString.GetMultilingualString("28089289-6978-4dbe-b26c-311e08c7105e", "Transaction Unmatched Notify Group"), transactionUnmatchedNotifyGroupHint, RegistryStorageFlags.Company | RegistryStorageFlags.System | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem IntercompanyTransactionsImportNotifyGroup
		{
			get
			{
				return GetItem("IntercompanyTransactionsImportNotifyGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("IntercompanyTransactionsImportNotifyGroup", Categories.Accounting_EmailNotification, ResString.GetMultilingualString("00713c6e-e4b9-4014-9e40-d0e93da4cb68", "Intercompany Transactions Import Notify Group"), ResString.GetMultilingualString("a63ad2e8-1287-41cd-9c0b-ef7a672cfa7a", "Notify Party in the event of an intercompany invoice failing to import. The email will advise transaction details and summary of the error encountered. Note: This is only applicable to an intercompany invoice import failure via the 'ISI' workflow trigger."), RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem RevenueRecognitionNotificationGroup
		{
			get
			{
				return GetItem("RevenueRecognitionNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("RevenueRecognitionNotificationGroup", Categories.Accounting_EmailNotification, ResString.GetMultilingualString("0AB8AAA2-4E3A-422E-A2A3-92A507DF28F2", "Revenue Recognition Notification Group"), ResString.GetMultilingualString("7A175E98-72DC-4C3D-943F-DE7E71C1C24C", "Notify Party for Revenue Recognition"), RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem JobPostingNotificationGroup
		{
			get
			{
				return GetItem("JobPostingNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("JobPostingNotificationGroup", Categories.Accounting_EmailNotification, ResString.GetMultilingualString("912c8775-7bcd-4df2-bdc9-14802a5b6884", "Job Posting Notification Group"), ResString.GetMultilingualString("4a48c2fe-9a02-4cd9-94c2-af37c0ea6614", "Notify Party for Job Posting"), RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem ARCreditControlledDocumentsApprovalNotifyGroup
		{
			get
			{
				return GetItem("ARCreditControlledDocumentsApprovalNotifyGroup", delegate
				{
					var result = new GuidRegistryItem("ARCreditControlledDocumentsApprovalNotifyGroup",
						Categories.Accounting_EmailNotification,
						ResString.GetMultilingualString("001e47b6-b4ef-417d-be87-1ab4355433b1", "AR Credit Controlled Documents Approval Notify Group"),
						ResString.GetMultilingualString("f0c05686-2557-4f74-8e2a-99162f9eb5a1", "Notify Party whenever AR Credit Control Documents Approval Requests are raised by the users."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System | RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public GuidRegistryItem InvoiceDateIncrementingSuspensionNotifyGroup
		{
			get
			{
				return GetItem("InvoiceDateIncrementingSuspensionNotifyGroup", delegate
				{
					var result = new GuidRegistryItem("InvoiceDateIncrementingSuspensionNotifyGroup",
						Categories.Accounting_EmailNotification,
						ResString.GetMultilingualString("e52d0573-3607-49e2-9f7d-2c6d35a6dc8d", "Invoice Date Incrementing Suspension Notify Group"),
						ResString.GetMultilingualString("2ffa8f1f-fc7e-47bc-ae0a-364a083b18b2", @"Notify Party when automatic incrementing of the current Invoice Date is suspended.
This registry is observed when the 'Invoice and Post Dates Defaulting Behavior' registry is set to 'MTH'."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public BooleanRegistryItem ReceivableAllowUserToModifyGSTId
		{
			get
			{
				return GetItem("ReceivableAllowUserToModifyGSTId", delegate
				{
					return new BooleanRegistryItem("ReceivableAllowUserToModifyGSTId", Categories.Accounting_ReceivableDefaults_DefaultSettings, ResString.GetMultilingualString("aff88905-dc6a-45cf-80d1-b02b48fc3131", "Allow user to modify {0}/TAX Id", TheTaxCode), ResString.GetMultilingualString("aff88905-dc6a-45cf-80d1-b02b48fc3131", "Allow user to modify {0}/TAX Id", TheTaxCode), RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
				});
			}
		}

		public BooleanRegistryItem ReceivableAllowUserToModifyTaxMessage
		{
			get
			{
				return GetItem("ReceivableAllowUserToModifyTaxMessage", delegate
				{
					return new BooleanRegistryItem(
						"ReceivableAllowUserToModifyTaxMessage",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("5f5fa8aa-b665-4f03-9ee9-0c45ed7958df", "Allow user to modify Tax Message"),
						ResString.GetMultilingualString("d96cd167-aec9-4a9a-9142-59a095698b48", "When this registry is set to 'Yes', authorized users will be able to override Invoice Sell Tax Message / AR Invoice Tax Message defaulted."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						true);
				});
			}
		}

		public BooleanRegistryItem AllowModifyInvoiceTerm
		{
			get
			{
				return GetItem("ReceivableAllowModifyInvoiceTerm", delegate
				{
					return new BooleanRegistryItem("ReceivableAllowModifyInvoiceTerm",
												Categories.Accounting_ReceivableDefaults_DefaultSettings,
												ResString.GetMultilingualString("d6abb739-cbcf-420a-993e-48ee81222133", "Allow Invoice Term to be modified"),
												ResString.GetMultilingualString("374bfc82-9daf-4e05-8287-a4b8cb143110", @"Use this registry item to control whether users can modify Invoice Term on receivable invoices, credit notes and adjustments. If you want to restrict all users from being able to modify the debtor’s default invoice terms on accounting documents, then set this registry to ‘No’. However, if you wish to restrict this setting only for certain users and documents, then set this registry to ‘Yes’ and then review the following Allow Override AR Invoice Term setting at the following checkpoints within the Security Rights tree:

Manage > Receivables > New Transactions > Invoice.
Manage > Receivables > New Transactions > Credit Note.
Manage > Receivables > New Transactions > Adjustment Note.
Manage > Receivables > New Transactions > Periodic Invoice.
… > Billing > AR Invoices > Amend Transaction > Amend with Invoice.
… > Billing > AR Invoices > Amend Transaction > Amend with Credit Note.

Note that you can only change invoice terms in the New AR Invoice, New AR Credit Note, New AR Adjustment Note and New Periodic Invoice windows and not when posting revenue from jobs."),
												RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
				});
			}
		}

		public BooleanRegistryItem AmendingTransactionLocalTotalBehavior
		{
			get
			{
				return GetItem("AmendingTransactionLocalTotalBehavior", delegate
				{
					return new BooleanRegistryItem("AmendingTransactionLocalTotalBehavior",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("7F5E85B3-945E-43f9-88B9-6600B3847D86", "Amending Transaction Local Total Behavior"),
						ResString.GetMultilingualString("C0C582BE-DAC7-486e-AAF6-3A286132D857", @"By default, this registry will be set to 'No' and users will be warned when creating an amending transaction that will, in Local Total Inclusive of Tax, credit the customer more than what was originally billed.
When this registry is set to 'Yes', users will be prevented from posting an amending transaction that will result in the Receivables Organization being credited more than what was originally invoiced.

This registry is relevant when posting Amending Receivable Transactions.
The Local Currency Equivalent Value of all amounts (including tax) recorded in the Amending Transaction, its Parent Transaction and any related Amending Transactions already posted are all taken into account.
A Warning Message or Error Message (determined by the No/YES setting of this registry) will triggered when a user attempts to post an Amending Transaction that will produce a Net result of crediting the Receivables Organization more than what was invoiced in the related transaction/s.

Note:  
This registry is relevant to the creation of Amending Invoice and Amending Credit Note transactions.
It does NOT affect the creation of standalone Credit Notes. 
It is a company level registry configuration."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem AmendingTransactionTaxBehavior
		{
			get
			{
				return GetItem("AmendingTransactionTaxBehavior", delegate
				{
					return new BooleanRegistryItem(
						new AmendingTransactionTaxBehaviorRegistryItemImpl(
						"AmendingTransactionTaxBehavior",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("974a0d2d-aa84-4231-9a5a-639c735ab77f", "Amending Transaction Tax Behavior"),
						ResString.GetMultilingualString("e480cd7b-5650-4bd8-87dd-b8cbcdf3ec68", @"When set to YES, users will be prevented from posting an amending transaction that will result in the Receivables organization being credited more tax than was originally invoiced.
When set to NO, users will be warned when creating an amending transaction that will, in total, credit the customer more tax than was originally billed.

This registry is relevant when posting Amending Receivables Transactions.
The Local Currency Equivalent Value of all Tax Amounts recorded in the Amending Transaction, its Parent Transaction and any related Amending Transactions already posted are all taken into account.
A Warning Message or Error Message (determined by the NO/YES setting of this registry) will be triggered when a user attempts to post an Amending Transaction that will produce a Net Tax result of crediting the Receivables Organization more tax than was  invoiced in the related transaction/s.

Note:  
This registry is relevant to the creation of Amending Invoice and Amending Credit Note transactions.
It does NOT affect the creation of standalone Credit Notes. 
It is a company level registry configuration."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default));
				});
			}
		}

		public BooleanRegistryItem AmendingTransactionCopyExchangeRateFromOriginalTransaction
		{
			get
			{
				return GetItem("AmendingTransactionCopyExchangeRateFromOriginalTransaction", delegate
				{
					return new BooleanRegistryItem("AmendingTransactionCopyExchangeRateFromOriginalTransaction",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						ResString.GetMultilingualString("5b05e4b8-32ee-4757-a834-e3a6bca11cf7", "Amending Transaction Copy Exchange Rate from Original Transaction"),
						ResString.GetMultilingualString("e4dc3457-680a-4de6-a47f-06ebd33a2d39", @"By default, this registry is set to 'Yes' and the default exchange rate on foreign currency amending transactions is the exchange rate recorded on the original parent transaction.
When this registry is set to 'No', the default exchange rate on foreign currency amending transactions is the exchange rate configured according to the AR/AP Invoice Posting Exchange Rate Option and exchange rate source (rate type) according to the debtor's Job Billing Exchange Rates configuration.

This registry is relevant when posting Amending Receivable Transactions.

Note:  
It does NOT affect the creation of standalone Credit Notes or Reversals.
It is a company level registry configuration."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem AmendingTransactionLocalTotalBehaviorForAP
		{
			get
			{
				return GetItem("AmendingTransactionLocalTotalBehaviorForAP", delegate
				{
					return new BooleanRegistryItem("AmendingTransactionLocalTotalBehaviorForAP",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("AB474746-134C-4A4C-8C77-00D36FCFC5D4", "Amending AP Transaction Local Total Behavior"),
						ResString.GetMultilingualString("E70F3D49-FDDC-467F-9EA1-88A83E340893", @"This registry controls the ability to post amending Payables transactions where Local Total Amount (including tax) exceed the total amount owed in the original transaction.
By default, this registry is set to “No”.
This means the system will display a warning when creating an amending Payables transaction where the total local amount (including tax) exceeds the original amount owed but can still be posted.
When overridden to “Yes”, the system will prevent posting such transactions.
NOTE:
This function applies to Amending AP Invoices and AP Credit Notes (which are linked to parent).
When an amending Payables transaction debits more than what is owed, the system will recognize the Payables Organization as in debt."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem AmendingTransactionTaxBehaviorForAP
		{
			get
			{
				return GetItem("AmendingTransactionTaxBehaviorForAP", delegate
				{
					return new BooleanRegistryItem(
						new AmendingTransactionTaxBehaviorRegistryItemImpl(
						"AmendingTransactionTaxBehaviorForAP",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("EC3EF7FE-2DC4-48E1-9852-D97D6A1E3588", "Amending AP Transaction Tax Behavior"),
						ResString.GetMultilingualString("93608F63-2CB2-4902-AD42-222A377D7977", @"This registry controls the ability to post amending Payables transactions where Tax amount exceed the total tax owed in the original transaction.
By default, this registry is set to “No”.
This means the system will display a warning when creating an amending Payables transaction where the tax amount exceeds the original tax owed but can still be posted.
When overridden to “Yes”, the system will prevent posting such transactions.
NOTE:
This function applies to Amending AP Invoices and AP Credit Notes (which are linked to parent).
When an amending Payables transaction debits more tax than what is owed, the system will recognize the Payables Organization as in debt."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default));
				});
			}
		}

		public BooleanRegistryItem AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP
		{
			get
			{
				return GetItem("AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP", delegate
				{
					return new BooleanRegistryItem("AmendingTransactionCopyExchangeRateFromOriginalTransactionForAP",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("16d705ae-b372-42a4-9ae0-7ca147098d55", "Amending AP Transaction Copy Exchange Rate from Original Transaction"),
						ResString.GetMultilingualString("8db7e26b-ffbd-43fc-a464-717a64c0b742", @"By default, this registry is set to 'Yes' and the default exchange rate on foreign currency amending transactions is the exchange rate recorded on the original parent transaction.
When this registry is set to 'No', the default exchange rate on foreign currency amending transactions is the exchange rate configured according to the AR/AP Invoice Posting Exchange Rate Option and exchange rate source (rate type) according to the creditor's Job Billing Exchange Rates configuration.

This registry is relevant when posting Amending AP Credit Notes.

Note:  
It does NOT affect the creation of standalone Credit Notes or Reversals.
It is a company level registry configuration."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem EnablePaymentApprovalFullMatchingValidation
		{
			get
			{
				return GetItem("EnablePaymentApprovalFullMatchingValidation", () =>
					new BooleanRegistryItem("EnablePaymentApprovalFullMatchingValidation",
						Categories.Accounting_Matching,
						(NoResString)"Enable Payment Approval Full Matching Validation (CargoWiseOne Support Only)",
						(NoResString)"Enables full matching session validation before Payment Approval posting.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true));
			}
		}

		public BooleanRegistryItem EnablePayablesInvoiceProcessingPortal
		{
			get
			{
				return GetItem("EnablePayablesInvoiceProcessingPortal", () =>
					new BooleanRegistryItem("EnablePayablesInvoiceProcessingPortal",
						Categories.Accounting_PayablesInvoiceProcessingPortal,
						(NoResString)"Enable Payables Invoice Processing Portal (CargoWise Support Only)",
						(NoResString)"This registry can be used to enable the new Payables Invoice Processing Portal for internal testing only.\r\nDo not enable this functionality for any client.\r\n\r\nProject reference: PRJ00042427",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public BooleanRegistryItem EnableImportingUniversalTransactionIntoPayableDraftInvoices
		{
			get
			{
				return GetItem("EnableImportingUniversalTransactionIntoPayableDraftInvoices", () =>
					new BooleanRegistryItem("EnableImportingUniversalTransactionIntoPayableDraftInvoices",
						Categories.Accounting_PayablesInvoiceProcessingPortal,
						(NoResString)"Import Universal Transactions into Payables Draft Invoices (CargoWise Support Only)",
						(NoResString)"This registry can be used to enable the incoming XUTs to be saved as draft invoices for internal testing only.\r\nIn addition to enabling this registry item, [Enable Payables Invoice Processing Portal (CargoWise Support Only)] should also be enabled to save incoming XUTs as draft invoices.\r\nDo not enable this functionality for any client.\r\n\r\nProject reference: PRJ00042427",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public BooleanRegistryItem EnableNativeVueLandingPageForPayablesInvoiceProcessingPortal
		{
			get
			{
				return GetItem("EnableNativeVueLandingPageForPayablesInvoiceProcessingPortal", () =>
					new BooleanRegistryItem("EnableNativeVueLandingPageForPayablesInvoiceProcessingPortal",
						Categories.Accounting_PayablesInvoiceProcessingPortal,
						(NoResString)"Enable Native Vue Landing Page For Payables Invoice Processing Portal (CargoWise Support Only)",
						(NoResString)@"This registry can be used to enable Native Vue Landing Page menu items of Payables Invoice Processing Portal for internal testing only.
Do not enable this functionality for any client.

Project reference: PRJ00047884",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false));
			}
		}

		public DraftTransactionStatusReasonCodeRegistryItem DraftTransactionStatusReasons
		{
			get
			{
				return GetItem("DraftTransactionStatusReasons", () =>
					new DraftTransactionStatusReasonCodeRegistryItem("DraftTransactionStatusReasons",
						Categories.Accounting_PayablesInvoiceProcessingPortal,
						ResString.GetMultilingualString("2BFB97AB-7AF6-4094-9490-B13B8F1E5634", "Status Reasons"),
						ResString.GetMultilingualString("BFCC8828-09D4-4D62-BC0B-B57216BAF30A", @"This registry is used to maintain a set of default Status Reasons and determine to which of the following Statuses they can be applied.
ANL - Analyzing.
DFT - Draft.
DSC - Discarded.
DIS - In Dispute.
AFP - Approved for Posting.
AWA - Awaiting Approval.
PRS - Processed.
Tick the statuses to which the reason may be applied.

Reason Code OTH will allow users to enter a status reason description as free text. 
If it is a requirement that a pre-defined reason code and description is always used, ensure that all status checkboxes are unticked."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						Instance.EnablePayablesInvoiceProcessingPortal.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden));
			}
		}

		public CodeDescriptionBoolRegistryItem StatusReasonMandatory
		{
			get
			{
				var editorInfo = new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("C1179D78-ABF6-4C79-8B77-1A7C44C0E1E9", "Mandatory"),
					ResString.GetMultilingualString("6C94D9F2-E387-4240-9A84-F04EE2E33C91", "Status"), null, true, true, true);

				var defaultValue = new CodeDescriptionBoolDisallowNewCollection
				{
					{ AccDraftInvoiceHeaderStatus.AwaitingApproval,  AccDraftInvoiceHeaderStatusList.GetMultilingualDescriptionFromCode(AccDraftInvoiceHeaderStatus.AwaitingApproval) , false },
					{ AccDraftInvoiceHeaderStatus.ApprovedForPosting,  AccDraftInvoiceHeaderStatusList.GetMultilingualDescriptionFromCode(AccDraftInvoiceHeaderStatus.ApprovedForPosting) , false },
					{ AccDraftInvoiceHeaderStatus.Discarded,  AccDraftInvoiceHeaderStatusList.GetMultilingualDescriptionFromCode(AccDraftInvoiceHeaderStatus.Discarded) , false },
					{ AccDraftInvoiceHeaderStatus.InDispute,  AccDraftInvoiceHeaderStatusList.GetMultilingualDescriptionFromCode(AccDraftInvoiceHeaderStatus.InDispute) , false }
				};

				return GetItem("StatusReasonMandatory", () =>
					new CodeDescriptionBoolRegistryItem("StatusReasonMandatory",
						Categories.Accounting_PayablesInvoiceProcessingPortal,
						ResString.GetMultilingualString("7C7D97A8-1D10-4F62-B518-C5659E1E97AA", "Status Reason Mandatory"),
						ResString.GetMultilingualString("EB6196CA-8C8A-408C-9436-F3ED4FA7DAAA", @"This registry determines whether it is mandatory to enter a reason or comment when changing the status of a draft transaction in the invoice processing portal.
By default, a Status Reason is not mandatory (checkbox unticked).
If it is mandatory to have a reason specified for the change to a particular Status, tick the checkbox for that status. 
A ticked checkbox will ensure that a draft transaction cannot be changed to this Status without a reason being selected or free text comment being entered."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						Instance.EnablePayablesInvoiceProcessingPortal.Value ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsHidden,
						editorInfo,
						defaultValue));
			}
		}

		class AmendingTransactionTaxBehaviorRegistryItemImpl : RegistryItemImpl
		{
			public AmendingTransactionTaxBehaviorRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
				: base(name, category, caption, hint, new BooleanRegistryDataType(), storage, option)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				bool result;

#if DEBUG
				if (Cache != null && Globals.IsTest)
				{
					Cache = null;
				}
#endif

				if (Cache == null)
				{
					Cache = new Dictionary<Guid, bool>();
					UpdateCache();
				}

				if (!Cache.TryGetValue(companyPK, out result))
				{
					result = false;
				}

				return result;
			}

			Dictionary<Guid, bool> Cache;

			void UpdateCache()
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				ZQuery companyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Cache.Keys);
				GlbCompany[] companies = factory.Load<GlbCompany>(companyQuery);

				foreach (GlbCompany company in companies)
				{
					bool defaultValue = false;
					ZString countryCode = company.GC_RN_NKCountryCode;
					switch (countryCode)
					{
						case Core.Constants.CountryCodes.Chile:
							defaultValue = true;
							break;
					}
					Cache.Add(company.PK.ToGuid(), defaultValue);
				}
			}
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem EnableMiscInvoiceInPeriodicInvoice
		{
			get
			{
				return GetItem("ReceivableEnableMiscInvoiceInPeriodicInvoice", delegate
				{
					return new BooleanRegistryItem("ReceivableEnableMiscInvoiceInPeriodicInvoice",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						(NoResString)"Enable Miscellaneous Invoices in Periodic Invoicing System (CargoWiseOne Support Only)",
						(NoResString)"Enable Miscellaneous Invoices in Periodic Invoicing System",
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		ResourceString BuildProFormaTitleLiteral(MultilingualString title)
		{
			return ResString.GetMultilingualString("9ecf3cc5-af05-4897-bdab-04f545a5427f", "PRO FORMA {0}", title);
		}

		public ParameterizedStringRegistryItem ProFormaNonTaxAdjustmentNoteTitle
		{
			get
			{
				return GetItem("ProFormaNonTaxAdjustmentNoteTitle", delegate
				{
					return new ParameterizedStringRegistryItem("ProFormaNonTaxAdjustmentNoteTitle",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("e9960c74-d0c9-4ed0-aa67-e5f7bb431b54", "Pro Forma Non Tax Adjustment Note Title"),
						ResString.GetMultilingualString("bbb81aac-b6ff-48ff-b984-6f97043d6391", @"Pro Forma Non Tax Adjustment Note Title

Note: If the value of 'Non Tax Adjustment Note' registry is modified please close and reopen the  registry form to see the effect on the preview."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildProFormaTitleLiteral(NonTaxAdjustmentNoteTitle.Value),
						((IMultilingualRegistryItem)NonTaxAdjustmentNoteTitle).CaptionMultilingual
						);
				});
			}
		}

		public ParameterizedStringRegistryItem ProFormaNonTaxCreditNoteTitle
		{
			get
			{
				return GetItem("ProFormaNonTaxCreditNoteTitle", delegate
				{
					return new ParameterizedStringRegistryItem("ProFormaNonTaxCreditNoteTitle",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("944e4555-e388-4bb2-8865-c7fbcab74afd", "Pro Forma Non Tax Credit Note Title"),
						ResString.GetMultilingualString("9a1d96e0-45ad-435e-89a0-45f9fd0119bb", @"Pro Forma Non Tax Credit Note Title
					
Note: If the value of 'Non Tax Credit Note' registry is modified please close and reopen the  registry form to see the effect on the preview."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildProFormaTitleLiteral(NonTaxCreditNoteTitle.Value),
						((IMultilingualRegistryItem)NonTaxCreditNoteTitle).CaptionMultilingual
						);
				});
			}
		}

		public ParameterizedStringRegistryItem ProFormaNonTaxDisbursementCreditNoteTitle
		{
			get
			{
				return GetItem("ProFormaNonTaxDisbursementCreditNoteTitle", delegate
				{
					return new ParameterizedStringRegistryItem("ProFormaNonTaxDisbursementCreditNoteTitle",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("0aad35dd-0a61-4b0c-b285-1cd931992892", "Pro Forma Non Tax Disbursement Credit Note Title"),
						ResString.GetMultilingualString("b9a34c2a-484e-48f8-893d-8d7566bcd535", @"Pro Forma Non Tax Disbursement Credit Note Title
					
Note: If the value of 'Non Tax Disbursement Credit Note' registry is modified please close and reopen the  registry form to see the effect on the preview."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildProFormaTitleLiteral(NonTaxDisbursementCreditNoteTitle.Value),
						((IMultilingualRegistryItem)NonTaxDisbursementCreditNoteTitle).CaptionMultilingual
						);
				});
			}
		}

		public ParameterizedStringRegistryItem ProFormaNonTaxDisbursementInvoiceTitle
		{
			get
			{
				return GetItem("ProFormaNonTaxDisbursementInvoiceTitle", delegate
				{
					return new ParameterizedStringRegistryItem("ProFormaNonTaxDisbursementInvoiceTitle",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("60330372-c507-43cd-957b-5f4c50ea72b6", "Pro Forma Non Tax Disbursement Invoice Title"),
						ResString.GetMultilingualString("0bdb234f-af00-4f37-b79e-492bed18c8b3", @"Pro Forma Non Tax Disbursement Invoice Title

Note: If the value of 'Non Tax Disbursement Invoice' registry is modified please close and reopen the  registry form to see the effect on the preview."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildProFormaTitleLiteral(NonTaxDisbursementInvoiceTitle.Value),
						((IMultilingualRegistryItem)NonTaxDisbursementInvoiceTitle).CaptionMultilingual
						);
				});
			}
		}

		public ParameterizedStringRegistryItem ProFormaNonTaxInvoiceTitle
		{
			get
			{
				return GetItem("ProFormaNonTaxInvoiceTitle", delegate
				{
					return new ParameterizedStringRegistryItem("ProFormaNonTaxInvoiceTitle",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("26d76c94-2d5e-43da-8451-2fb293b674ee", "Pro Forma Non Tax Invoice Title"),
						ResString.GetMultilingualString("3c9c18b7-ec4b-4cab-bfbd-5c10e3773df3", @"Pro Forma Non Tax Invoice Title

Note: If the value of 'Non Tax Invoice' registry is modified please close and reopen the  registry form to see the effect on the preview."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildProFormaTitleLiteral(NonTaxInvoiceTitle.Value),
						((IMultilingualRegistryItem)NonTaxInvoiceTitle).CaptionMultilingual
						);
				});
			}
		}

		public ParameterizedStringRegistryItem ProFormaTaxAdjustmentNoteTitle
		{
			get
			{
				return GetItem("ProFormaTaxAdjustmentNoteTitle", delegate
				{
					return new ParameterizedStringRegistryItem("ProFormaTaxAdjustmentNoteTitle",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("49ffc232-6c14-4ea1-962f-dc6b9392c8c9", "Pro Forma Tax Adjustment Note Title"),
						ResString.GetMultilingualString("42b4e28e-c960-4b71-a419-1d6a5cdb957f", @"Pro Forma Tax Adjustment Note Title

Note: If the value of 'Tax Adjustment Note' registry is modified please close and reopen the  registry form to see the effect on the preview."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildProFormaTitleLiteral(TaxAdjustmentNoteTitle.Value),
						((IMultilingualRegistryItem)TaxAdjustmentNoteTitle).CaptionMultilingual
						);
				});
			}
		}

		public ParameterizedStringRegistryItem ProFormaTaxCreditNoteTitle
		{
			get
			{
				return GetItem("ProFormaTaxCreditNoteTitle", delegate
				{
					return new ParameterizedStringRegistryItem("ProFormaTaxCreditNoteTitle",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("3df5a2e9-905f-4ceb-8520-0118157488a3", "Pro Forma Tax Credit Note Title"),
						ResString.GetMultilingualString("73409dda-f3a3-4049-9308-2de07a59d348", @"Pro Forma Tax Credit Note Title

Note: If the value of 'Tax Credit Note' registry is modified please close and reopen the  registry form to see the effect on the preview."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildProFormaTitleLiteral(TaxCreditNoteTitle.Value),
						((IMultilingualRegistryItem)TaxCreditNoteTitle).CaptionMultilingual
						);
				});
			}
		}

		public ParameterizedStringRegistryItem ProFormaTaxDisbursementCreditNoteTitle
		{
			get
			{
				return GetItem("ProFormaTaxDisbursementCreditNoteTitle", delegate
				{
					return new ParameterizedStringRegistryItem("ProFormaTaxDisbursementCreditNoteTitle",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("4e75f156-39d9-4771-9628-8c2915194666", "Pro Forma Tax Disbursement Credit Note Title"),
						ResString.GetMultilingualString("41b32d77-9583-4bfd-a436-d6a4fce2086e", @"Pro Forma Tax Disbursement Credit Note Title

Note: If the value of 'Tax Credit Note Disbursement' registry is modified please close and reopen the  registry form to see the effect on the preview."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildProFormaTitleLiteral(TaxDisbursementCreditNoteTitle.Value),
						((IMultilingualRegistryItem)TaxDisbursementCreditNoteTitle).CaptionMultilingual
						);
				});
			}
		}

		public ParameterizedStringRegistryItem ProFormaTaxDisbursementInvoiceTitle
		{
			get
			{
				return GetItem("ProFormaTaxDisbursementInvoiceTitle", delegate
				{
					return new ParameterizedStringRegistryItem("ProFormaTaxDisbursementInvoiceTitle",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("c027925b-e5f0-4626-bf7c-e37642435995", "Pro Forma Tax Disbursement Invoice Title"),
						ResString.GetMultilingualString("c2188765-92c9-4112-82e7-36491e726ed6", @"Pro Forma Tax Disbursement Invoice Title

Note: If the value of 'Tax Disbursement Invoice' registry is modified please close and reopen the  registry form to see the effect on the preview."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildProFormaTitleLiteral(TaxDisbursementInvoiceTitle.Value),
						((IMultilingualRegistryItem)TaxDisbursementInvoiceTitle).CaptionMultilingual
						);
				});
			}
		}

		public ParameterizedStringRegistryItem ProFormaTaxInvoiceTitle
		{
			get
			{
				return GetItem("ProFormaTaxInvoiceTitle", delegate
				{
					return new ParameterizedStringRegistryItem("ProFormaTaxInvoiceTitle",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("a276d7bc-3c8f-4f37-a0f3-b373f7adf501", "Pro Forma Tax Invoice Title"),
						ResString.GetMultilingualString("51c439c1-1545-48c2-bb5a-7518342ec035", @"Pro Forma Tax Invoice Title

Note: If the value of 'Tax Invoice' registry is modified please close and reopen the  registry form to see the effect on the preview."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildProFormaTitleLiteral(TaxInvoiceTitle.Value),
						((IMultilingualRegistryItem)TaxInvoiceTitle).CaptionMultilingual
						);
				});
			}
		}

		public MultilingualStringRegistryItem ProFormaAdjustmentNoteMessage
		{
			get
			{
				return GetItem("ProFormaAdjustmentNoteMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("ProFormaAdjustmentNoteMessage",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("9c3c068a-c197-4f91-a323-6a63963722df", "Pro Forma Adjustment Note Message"),
						ResString.GetMultilingualString("9c3c068a-c197-4f91-a323-6a63963722df", "Pro Forma Adjustment Note Message"),
						RegistryStorageFlags.Company | RegistryStorageFlags.System, AdjustmentNoteMessage.Value);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem ProFormaCreditNoteMessage
		{
			get
			{
				return GetItem("ProFormaCreditNoteMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("ProFormaCreditNoteMessage",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("bb9881c2-1e36-4db7-b9cc-956a822502a5", "Pro Forma Credit Note Message"),
						ResString.GetMultilingualString("bb9881c2-1e36-4db7-b9cc-956a822502a5", "Pro Forma Credit Note Message"),
						RegistryStorageFlags.Company | RegistryStorageFlags.System, CreditNoteMessage.Value);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem ProFormaDisbursementMessage
		{
			get
			{
				return GetItem("ProFormaDisbursementMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("ProFormaDisbursementMessage",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("050cc58d-df04-4f4e-b731-50a78c85cb64", "Pro Forma Disbursement Message"),
						ResString.GetMultilingualString("050cc58d-df04-4f4e-b731-50a78c85cb64", "Pro Forma Disbursement Message"),
						RegistryStorageFlags.Company | RegistryStorageFlags.System, DisbursementMessage.Value);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem ProFormaInvoiceMessage
		{
			get
			{
				return GetItem("ProFormaInvoiceMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("ProFormaInvoiceMessage",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_ProFormaInvoice,
						ResString.GetMultilingualString("a9d4e8dc-c8af-4589-af49-ffa77b30244b", "Pro Forma Invoice Message"),
						ResString.GetMultilingualString("a9d4e8dc-c8af-4589-af49-ffa77b30244b", "Pro Forma Invoice Message"),
						RegistryStorageFlags.Company | RegistryStorageFlags.System, InvoiceMessage.Value);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem PrintWatermarkForTransactionAwaitingApproval
		{
			get
			{
				return GetItem("PrintWatermarkForTransactionAwaitingApproval", () =>
				{
					var result = new MultilingualStringRegistryItem(
						new CountrySpecificDefaultValueRegistryItemImpl<string>(
							"PrintWatermarkForTransactionAwaitingApproval",
							Categories.Accounting_ReceivableDefaults,
							ResString.GetMultilingualString("08df6e94-86e5-4181-ad78-500e27b3a668",
								@"This Registry setting allows users to print a Watermark text on the Transactions Awaiting Approval/Clearance from the Government. It is strongly recommended that the Watermark be prominent and unambiguous for this purpose. If this registry is empty, no watermark will be printed."),
							RegistryDataTypes.StringType,
							RegistryOptions.CacheExpensiveDefaultValue,
							FactoryForCountryDefaultValues,
							new PrintWatermarkForTransactionAwaitingApproval_RegistryDescriptor()));

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);

					return result;
				});
			}
		}

		public MultilingualStringRegistryItem NonTaxInvoiceTitle
		{
			get
			{
				return GetItem("NonTaxInvoiceTitle", delegate
				{
					return new MultilingualStringRegistryItem("NonTaxInvoiceTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_Invoice, ResString.GetMultilingualString("042d00cd-e347-49a9-8afc-cce652728e98", "Non Tax Invoice"), ResString.GetMultilingualString("384eab56-fc7f-4446-ae42-6b4200ee99a1", "Non Tax Invoice Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("9a93a141-a7ba-4ed1-b016-b45c5bd411b9", "INVOICE"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxInvoiceTitle
		{
			get
			{
				return GetItem("TaxInvoiceTitle", delegate
				{
					return new MultilingualStringRegistryItem("TaxInvoiceTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_Invoice, ResString.GetMultilingualString("09b93922-fba6-4bad-868c-284e05b6fd3c", "Tax Invoice"), ResString.GetMultilingualString("b5cc0df8-bed7-47a5-ab5e-e20dc1da6665", "Tax Invoice Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("bafb0efb-e9db-4197-ad02-4609c9ba7447", "TAX INVOICE"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxInvoiceAmendmentTitle
		{
			get
			{
				return GetItem("TaxInvoiceAmendmentTitle", delegate
				{
					return new AmendmentAndReversalTitleRegistryItem(
						new AmendmentAndReversalTitleRegistryItemImpl(
							TaxInvoiceTitle,
							"TaxInvoiceAmendmentTitle",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_Invoice,
							ResString.GetMultilingualString("4421d04c-5066-402a-bc14-690f9ebb0bee", "Tax Invoice Amendment"),
							ResString.GetMultilingualString("469b491b-b7d0-4338-8598-4ed5d97625d6", @"This registry controls the Document Title of a Receivables Amending Invoice transaction.
	Amending transactions are a specific class of transaction. They record additional charges correcting an error or omission on a previously posted parent transaction."),
							RegistryStorageFlags.Company | RegistryStorageFlags.System));
				});
			}
		}

		public MultilingualStringRegistryItem TaxInvoiceReversalTitle
		{
			get
			{
				return GetItem("TaxInvoiceReversalTitle", delegate
				{
					return new AmendmentAndReversalTitleRegistryItem(
						new AmendmentAndReversalTitleRegistryItemImpl(
							TaxInvoiceTitle,
							"TaxInvoiceReversalTitle",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_Invoice,
							ResString.GetMultilingualString("f7012af4-dff9-4355-bdaa-23c319027e03", "Tax Invoice Reversal"),
							ResString.GetMultilingualString("2b8b1027-1e37-46ed-9993-1a86f13657ac", @"This registry controls the Document Title of a Receivables Invoice Reversal transaction.
	Reversals are a specific class of transaction. They are created by “Reversing” a previously posted transaction."),
							RegistryStorageFlags.Company | RegistryStorageFlags.System));
				});
			}
		}

		public MultilingualStringRegistryItem NonTaxCreditNoteTitle
		{
			get
			{
				return GetItem("NonTaxCreditNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("NonTaxCreditNoteTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_CreditNote, ResString.GetMultilingualString("75e8ce56-9596-4b00-895c-a8ffeefeed94", "Non Tax Credit Note"), ResString.GetMultilingualString("6d4a144f-641e-4ad8-b486-a33d4290b671", "Non Tax Credit Note Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("13de81e2-e36b-42a3-a304-e54fddce6ad3", "CREDIT NOTE"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxCreditNoteTitle
		{
			get
			{
				return GetItem("TaxCreditNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("TaxCreditNoteTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_CreditNote, ResString.GetMultilingualString("d6b1d723-bdaf-4737-996b-502ab4e338ed", "Tax Credit Note"), ResString.GetMultilingualString("209c6c28-002f-496d-b183-c1fb2a2cf955", "Tax Credit Note Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("cc5ba718-54dd-4908-8f9e-5e151fd536a8", "TAX CREDIT NOTE"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxCreditNoteAmendmentTitle
		{
			get
			{
				return GetItem("TaxCreditNoteAmendmentTitle", delegate
				{
					return new AmendmentAndReversalTitleRegistryItem(
						new AmendmentAndReversalTitleRegistryItemImpl(
							TaxCreditNoteTitle,
							"TaxCreditNoteAmendmentTitle",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_CreditNote,
							ResString.GetMultilingualString("3339f78a-0c15-454e-8d57-e5f94eb01f94", "Tax Credit Note Amendment"),
							ResString.GetMultilingualString("b482b8d1-258a-4a69-9742-75856a1413b9", @"This registry controls the Document Title of a Receivables Amending Credit Note transaction.
	Amending transactions are a specific class of transaction. They record additional charges correcting an error or omission on a previously posted parent transaction."),
							RegistryStorageFlags.Company | RegistryStorageFlags.System));
				});
			}
		}

		public MultilingualStringRegistryItem TaxCreditNoteReversalTitle
		{
			get
			{
				return GetItem("TaxCreditNoteReversalTitle", delegate
				{
					return new AmendmentAndReversalTitleRegistryItem(
						new AmendmentAndReversalTitleRegistryItemImpl(
							TaxCreditNoteTitle,
							"TaxCreditNoteReversalTitle",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_CreditNote,
							ResString.GetMultilingualString("db018977-be71-4c30-8794-284550952d75", "Tax Credit Note Reversal"),
							ResString.GetMultilingualString("dc73ae9d-0ec7-4f8d-abf2-2aa5c56870de", @"This registry controls the Document Title of a Receivables Credit Note Reversal transaction.
	Reversals are a specific class of transaction. They are created by “Reversing” a previously posted transaction."),
							RegistryStorageFlags.Company | RegistryStorageFlags.System));
				});
			}
		}

		public MultilingualStringRegistryItem NonTaxAdjustmentNoteTitle
		{
			get
			{
				return GetItem("NonTaxAdjustmentNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("NonTaxAdjustmentNoteTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_AdjustmentNote, ResString.GetMultilingualString("0fe8b8d9-7475-4eaa-86ce-634b71eaa6ed", "Non Tax Adjustment Note"), ResString.GetMultilingualString("13573ba8-623a-4a04-b629-dc6cc053a799", "Non Tax Adjustment Note Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("6a88b6d2-22a5-49e7-97cb-609e45dbbde9", "ADJUSTMENT NOTE"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxAdjustmentNoteTitle
		{
			get
			{
				return GetItem("TaxAdjustmentNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("TaxAdjustmentNoteTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_AdjustmentNote, ResString.GetMultilingualString("cb4debcb-4e30-4db2-828e-d217c502bd78", "Tax Adjustment Note"), ResString.GetMultilingualString("59c3c8b4-d1c0-4eaa-bf71-dcb5e4adc338", "Tax Adjustment Note Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("f9a61d0c-20f9-4ced-93a5-a7619fab0942", "TAX ADJUSTMENT NOTE"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxAdjustmentNoteReversalTitle
		{
			get
			{
				return GetItem("TaxAdjustmentNoteReversalTitle", delegate
				{
					return new AmendmentAndReversalTitleRegistryItem(
						new AmendmentAndReversalTitleRegistryItemImpl(
							TaxAdjustmentNoteTitle,
							"TaxAdjustmentNoteReversalTitle",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_AdjustmentNote,
							ResString.GetMultilingualString("610891cb-4275-4671-b4a1-c1fe73aff011", "Tax Adjustment Note Reversal"),
							ResString.GetMultilingualString("b141d3a1-7d3c-44a2-9cef-78dcb772f5ed", @"This registry controls the Document Title of a Receivables Adjustment Note Reversal transaction.
	Reversals are a specific class of transaction. They are created by “Reversing” a previously posted transaction."),
							RegistryStorageFlags.Company | RegistryStorageFlags.System));
				});
			}
		}

		public MultilingualStringRegistryItem NonTaxDisbursementInvoiceTitle
		{
			get
			{
				return GetItem("NonTaxDisbursementInvoiceTitle", delegate
				{
					return new MultilingualStringRegistryItem("NonTaxDisbursementInvoiceTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_InvoiceDisbursement, ResString.GetMultilingualString("634c3016-1255-4825-9452-b48cb654c6a3", "Non Tax Disbursement Invoice"), ResString.GetMultilingualString("6c25737d-c3ae-485c-b0be-1c95ee2e8e1b", "Non Tax Disbursement Invoice Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("c0852555-78ee-4c54-bc1e-d1ba9dca9f1d", "DISBURSEMENT INVOICE"));
				});
			}
		}

		public MultilingualStringRegistryItem NonTaxDisbursementCreditNoteTitle
		{
			get
			{
				return GetItem("NonTaxDisbursementCreditNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("NonTaxDisbursementCreditNoteTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_CreditNoteDisbursement, ResString.GetMultilingualString("954976db-65b9-41f0-8ed4-673e6ad7606c", "Non Tax Credit Note Disbursement"), ResString.GetMultilingualString("aedd1c24-591d-4b13-800e-60159e2059a9", "Non Tax Disbursement Credit Note Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("1d49bbe2-5081-463e-8f87-db5491c030fd", "CREDIT NOTE"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxDisbursementCreditNoteTitle
		{
			get
			{
				return GetItem("TaxDisbursementCreditNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("TaxDisbursementCreditNoteTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_CreditNoteDisbursement, ResString.GetMultilingualString("67807bc2-b3bc-4267-9500-287187ed289a", "Tax Credit Note Disbursement"), ResString.GetMultilingualString("d9676e23-7e4f-4c4c-b586-ba661e117bfb", "Tax Disbursement Credit Note Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("b9b3da21-23e5-4727-9aec-338088a1e44c", "TAX CREDIT NOTE"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxDisbursementCreditNoteAmendmentTitle
		{
			get
			{
				return GetItem("TaxDisbursementCreditNoteAmendmentTitle", delegate
				{
					return new AmendmentAndReversalTitleRegistryItem(
						new AmendmentAndReversalTitleRegistryItemImpl(
							TaxDisbursementCreditNoteTitle,
							"TaxDisbursementCreditNoteAmendmentTitle",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_CreditNoteDisbursement,
							ResString.GetMultilingualString("57340515-ca27-4576-a52c-3feddd215000", "Tax Credit Note Disbursement Amendment"),
							ResString.GetMultilingualString("62e1fc86-b6c5-4061-89dc-24e8c8c298a1", @"This registry controls the Document Title of a Receivables Amending Credit Note Disbursement transaction.
	Amending transactions are a specific class of transaction. They record additional charges correcting an error or omission on a previously posted parent transaction."),
							RegistryStorageFlags.Company | RegistryStorageFlags.System));
				});
			}
		}

		public MultilingualStringRegistryItem TaxDisbursementCreditNoteReversalTitle
		{
			get
			{
				return GetItem("TaxDisbursementCreditNoteReversalTitle", delegate
				{
					return new AmendmentAndReversalTitleRegistryItem(
						new AmendmentAndReversalTitleRegistryItemImpl(
							TaxDisbursementCreditNoteTitle,
							"TaxDisbursementCreditNoteReversalTitle",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_CreditNoteDisbursement,
							ResString.GetMultilingualString("8f9c5dd7-0d27-4881-a30a-27ff4e6698f3", "Tax Credit Note Disbursement Reversal"),
							ResString.GetMultilingualString("c8127138-3b80-4f15-9c90-044451c2eb2a", @"This registry controls the Document Title of a Receivables Credit Note Disbursement Reversal transaction.
	Reversals are a specific class of transaction. They are created by “Reversing” a previously posted transaction."),
							RegistryStorageFlags.Company | RegistryStorageFlags.System));
				});
			}
		}

		public MultilingualStringRegistryItem TaxDisbursementInvoiceTitle
		{
			get
			{
				return GetItem("TaxDisbursementInvoiceTitle", delegate
				{
					return new MultilingualStringRegistryItem("TaxDisbursementInvoiceTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_InvoiceDisbursement, ResString.GetMultilingualString("80bcf4f4-1d52-4b33-a90a-2b41718802ac", "Tax Disbursement Invoice"), ResString.GetMultilingualString("a5cc185a-17ea-4456-acbb-2fb7c4ad5ea0", "Tax Disbursement Invoice Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("409758ef-066c-4a7f-95db-2d43c80e2e4b", "TAX DISBURSEMENT INVOICE"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxDisbursementInvoiceAmendmentTitle
		{
			get
			{
				return GetItem("TaxDisbursementInvoiceAmendmentTitle", delegate
				{
					return new AmendmentAndReversalTitleRegistryItem(
						new AmendmentAndReversalTitleRegistryItemImpl(
							TaxDisbursementInvoiceTitle,
							"TaxDisbursementInvoiceAmendmentTitle",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_InvoiceDisbursement,
							ResString.GetMultilingualString("29eb52d3-8bc7-4195-b976-d905c7b316db", "Tax Invoice Disbursement Amendment"),
							ResString.GetMultilingualString("599993ae-bb2e-4d90-bb68-ac48c06878cd", @"This registry controls the Document Title of a Receivables Amending Invoice Disbursement transaction.
	Amending transactions are a specific class of transaction. They record additional charges correcting an error or omission on a previously posted parent transaction."),
							RegistryStorageFlags.Company | RegistryStorageFlags.System));
				});
			}
		}

		public MultilingualStringRegistryItem TaxDisbursementInvoiceReversalTitle
		{
			get
			{
				return GetItem("TaxDisbursementInvoiceReversalTitle", delegate
				{
					return new AmendmentAndReversalTitleRegistryItem(
						new AmendmentAndReversalTitleRegistryItemImpl(
							TaxDisbursementInvoiceTitle,
							"TaxDisbursementInvoiceReversalTitle",
							Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice_InvoiceDocumentTitles_InvoiceDisbursement,
							ResString.GetMultilingualString("85359504-dfe1-4723-8eb9-289fc15e6422", "Tax Invoice Disbursement Reversal"),
							ResString.GetMultilingualString("2638dd47-1867-449c-a6d0-7186d9aabb3d", @"This registry controls the Document Title of a Receivables Invoice Disbursement Reversal transaction.
	Reversals are a specific class of transaction. They are created by “Reversing” a previously posted transaction."),
							RegistryStorageFlags.Company | RegistryStorageFlags.System));
				});
			}
		}

		class AmendmentAndReversalTitleRegistryItem : MultilingualStringRegistryItem
		{
			public AmendmentAndReversalTitleRegistryItem(AmendmentAndReversalTitleRegistryItemImpl inner)
				: base(inner)
			{
			}

			public override string ResourceStringsKeyPrefix
			{
				get
				{
					return ((AmendmentAndReversalTitleRegistryItemImpl)Inner).parentRegistryItem.ResourceStringsKeyPrefix;
				}
			}

			public override IEnumerable<ResourceString> DefaultStrings
			{
				get
				{
					return ((AmendmentAndReversalTitleRegistryItemImpl)Inner).parentRegistryItem.DefaultStrings;
				}
			}
		}

		class AmendmentAndReversalTitleRegistryItemImpl : MultilingualStringRegistryItemImpl
		{
			public AmendmentAndReversalTitleRegistryItemImpl(MultilingualStringRegistryItem parentRegistryItem, string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new StringRegistryDataType(), storage)
			{
				this.parentRegistryItem = parentRegistryItem;
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return parentRegistryItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty).GetUnresolvedString();
			}

			internal MultilingualStringRegistryItem parentRegistryItem;
		}

		public MultilingualStringRegistryItem DisbursementMessage
		{
			get
			{
				return GetItem("DisbursementMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("DisbursementMessage", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("3ea9ebcc-fc8a-4e89-b8a8-1071555053df", "Disbursement Message"), ResString.GetMultilingualString("f51ba9d6-42db-4a4c-a779-6de4dd0be970", "This message will appear on the Disbursement Invoice"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("276c1ed2-bae4-4695-861c-57f6292dd585", "This Invoice includes payment made on your behalf, please pay immediately."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem CreditNoteMessage
		{
			get
			{
				return GetItem("CreditNoteMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("CreditNoteMessage", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("cf250b93-933d-4c96-8ca5-a299452019c7", "Credit Note Message"), ResString.GetMultilingualString("53869dff-c4b4-4f8b-8fea-31230c31a469", "This message will appear on the Credit Note"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("531073b8-4feb-4919-9572-fa56620f66ed", "Please contact us within 7 days should there be any discrepancies."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem AdjustmentNoteMessage
		{
			get
			{
				return GetItem("AdjustmentNoteMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("AdjustmentNoteMessage", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("625ac526-2813-4e46-950d-7f37d4436f29", "Adjustment Note Message"), ResString.GetMultilingualString("8782b01b-d2e7-48bd-8746-5ad749cf90c5", "This message will appear on the Adjustment Note"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("531073b8-4feb-4919-9572-fa56620f66ed", "Please contact us within 7 days should there be any discrepancies."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem InvoiceMessage
		{
			get
			{
				return GetItem("InvoiceMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("InvoiceMessage", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("f53c0d48-031e-4b09-9014-88bcc9df4c3d", "Invoice Message"), ResString.GetMultilingualString("c6f68a9e-a8e3-4769-8bbb-e239fba2c7a2", "This message will appear on the Invoice"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("531073b8-4feb-4919-9572-fa56620f66ed", "Please contact us within 7 days should there be any discrepancies."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem NonTaxInvoiceBatchTitle
		{
			get
			{
				return GetItem("NonTaxInvoiceBatchTitle", delegate
				{
					return new MultilingualStringRegistryItem("NonTaxInvoiceBatchTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_BatchInvoice, ResString.GetMultilingualString("3b3e2ce9-0619-4017-b199-49e4404e315d", "Non Tax Invoice Batch Title"), ResString.GetMultilingualString("0473a3d4-1727-47fe-b1ac-e272b28889aa", "Non Tax Invoice Batch Summary Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("889cdac3-1526-44a2-9448-a2c61b15fb13", "Invoice Batch"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxInvoiceBatchTitle
		{
			get
			{
				return GetItem("TaxInvoiceBatchTitle", delegate
				{
					return new MultilingualStringRegistryItem("TaxInvoiceBatchTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_BatchInvoice, ResString.GetMultilingualString("5b0ae33c-b5a7-41c9-9593-134ac5743d2b", "Tax Invoice Batch Title"), ResString.GetMultilingualString("1c870262-f37b-4e98-863a-65b12a3dd90b", "Tax Invoice Batch Summary Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("889cdac3-1526-44a2-9448-a2c61b15fb13", "Invoice Batch"));
				});
			}
		}

		public MultilingualStringRegistryItem NonTaxInvoiceBatchDetailsTitle
		{
			get
			{
				return GetItem("NonTaxInvoiceBatchDetailsTitle", delegate
				{
					return new MultilingualStringRegistryItem("NonTaxInvoiceBatchDetailsTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_BatchInvoice, ResString.GetMultilingualString("7c5d962f-b0ee-4df0-80d1-9ff30aa1e3da", "Non Tax Invoice Batch Details Title"), ResString.GetMultilingualString("7c5d962f-b0ee-4df0-80d1-9ff30aa1e3da", "Non Tax Invoice Batch Details Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("a309c089-7ac5-49ad-a7eb-ca3258fe3575", "Detailed Analysis of Invoice Batch"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxInvoiceBatchDetailsTitle
		{
			get
			{
				return GetItem("TaxInvoiceBatchDetailsTitle", delegate
				{
					return new MultilingualStringRegistryItem("TaxInvoiceBatchDetailsTitle", Categories.Accounting_ReceivableDefaults_FormConfigurations_BatchInvoice, ResString.GetMultilingualString("70029c62-c028-4e3b-ace0-73e6179ed8c2", "Tax Invoice Batch Details Title"), ResString.GetMultilingualString("70029c62-c028-4e3b-ace0-73e6179ed8c2", "Tax Invoice Batch Details Title"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("a309c089-7ac5-49ad-a7eb-ca3258fe3575", "Detailed Analysis of Invoice Batch"));
				});
			}
		}

		public StringRegistryItem StatementStandardMessage
		{
			get
			{
				return GetItem("StatementStandardMessage", delegate
				{
					StringRegistryItem result = new StringRegistryItem("StatementStandardMessage", Categories.Accounting_ReceivableDefaults_FormConfigurations_StatementDefault, ResString.GetMultilingualString("106ec3e2-a405-4598-a49c-cb1c1cb8a54c", "Standard Message"), ResString.GetMultilingualString("106ec3e2-a405-4598-a49c-cb1c1cb8a54c", "Standard Message"), RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public BooleanRegistryItem PayableAllowUserToModifyGSTId
		{
			get
			{
				return GetItem("PayableAllowUserToModifyGSTId", delegate
				{
					return new BooleanRegistryItem("PayableAllowUserToModifyGSTId", Categories.Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("aff88905-dc6a-45cf-80d1-b02b48fc3131", "Allow user to modify {0}/TAX Id", TheTaxCode), ResString.GetMultilingualString("aff88905-dc6a-45cf-80d1-b02b48fc3131", "Allow user to modify {0}/TAX Id", TheTaxCode), RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
				});
			}
		}

		public BooleanRegistryItem PayableAllowUserToModifyTaxMessage
		{
			get
			{
				return GetItem("PayableAllowUserToModifyTaxMessage", delegate
				{
					return new BooleanRegistryItem("PayableAllowUserToModifyTaxMessage",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("803FD2AA-A6E9-4f70-BB27-4728ECCCAD0E", "Allow user to modify Tax Message"),
						ResString.GetMultilingualString("B7D7C8D6-400D-438d-A138-9E8E9595A6A9", "When this registry is set to 'Yes', authorized users will be able to override Invoice Cost Tax Message / AP Invoice Tax Message defaulted."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						true);
				});
			}
		}

		public MultilingualStringRegistryItem SelfBilledInvoiceMessage
		{
			get
			{
				return GetItem("SelfBilledInvoiceMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("SelfBilledInvoiceMessage", Categories.Accounting_PayableDefaults_SelfBillingInvoice, ResString.GetMultilingualString("f53c0d48-031e-4b09-9014-88bcc9df4c3d", "Invoice Message"), ResString.GetMultilingualString("5f5d7645-daff-479f-beb7-8ebc1ac84ac8", "This message will appear on the Self Billed AP Invoice"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("757bc8f3-a6c5-4c72-89bb-89aa4e3cb7e6", "This is a Self Billed / Recipient Issued Invoice."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem SelfBilledCreditNoteMessage
		{
			get
			{
				return GetItem("SelfBilledCreditNoteMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("SelfBilledCreditNoteMessage", Categories.Accounting_PayableDefaults_SelfBillingInvoice, ResString.GetMultilingualString("cf250b93-933d-4c96-8ca5-a299452019c7", "Credit Note Message"), ResString.GetMultilingualString("7a940825-b9eb-4978-9b32-44a84bf69207", "This message will appear on the Self Billed AP Credit Note"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("1077d843-bfe4-42d4-b662-13a1df0a7c89", "This is a Self Billed / Recipient Issued Credit Note."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem SelfBilledAdjustmentNoteMessage
		{
			get
			{
				return GetItem("SelfBilledAdjustmentNoteMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("SelfBilledAdjustmentNoteMessage", Categories.Accounting_PayableDefaults_SelfBillingInvoice, ResString.GetMultilingualString("fd3f2c47-b6e3-4981-b13b-6d91ca4e3690", "Adjustment Note Message"), ResString.GetMultilingualString("5e5fa44e-0bc8-437e-93b0-2be96194ba79", "This message will appear on the Self Billed AP Adjustment Note"), RegistryStorageFlags.Company | RegistryStorageFlags.System, ResString.GetMultilingualString("7c4c22ba-0fd3-4b87-8436-fb2927379f3f", "This is a Self Billed / Recipient Issued Adjustment Note."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem NonTaxSelfBilledInvoiceTitle
		{
			get
			{
				return GetItem("NonTaxSelfBilledInvoiceTitle", delegate
				{
					return new MultilingualStringRegistryItem("NonTaxSelfBilledInvoiceTitle",
						Categories.Accounting_PayableDefaults_SelfBillingInvoice,
						ResString.GetMultilingualString("384eab56-fc7f-4446-ae42-6b4200ee99a1", "Non Tax Invoice Title"),
						ResString.GetMultilingualString("d5845ab1-e5c7-4881-a66f-0a2a5ad96d5c", "This registry controls the Title printed in a Non Tax Self Billed Payables Invoice Transaction Document."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						ResString.GetMultilingualString("d94b6ea6-aead-4d3c-a081-3a3af2a3b003", "Recipient Created / Self Billed Invoice"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxSelfBilledInvoiceTitle
		{
			get
			{
				return GetItem("TaxSelfBilledInvoiceTitle", delegate
				{
					return new MultilingualStringRegistryItem("TaxSelfBilledInvoiceTitle",
						Categories.Accounting_PayableDefaults_SelfBillingInvoice,
						ResString.GetMultilingualString("b5cc0df8-bed7-47a5-ab5e-e20dc1da6665", "Tax Invoice Title"),
						ResString.GetMultilingualString("07ff92d8-3ad5-4538-b04f-3f3babe8f273", "This registry controls the Title printed in a Tax Self Billed Payables Invoice Transaction Document."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						ResString.GetMultilingualString("b252ae60-e477-40f9-b8c8-5a7b9cec0d64", "Recipient Created / Self Billed Tax Invoice"));
				});
			}
		}

		public MultilingualStringRegistryItem NonTaxSelfBilledCreditNoteTitle
		{
			get
			{
				return GetItem("NonTaxSelfBilledCreditNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("NonTaxSelfBilledCreditNoteTitle",
						Categories.Accounting_PayableDefaults_SelfBillingInvoice,
						ResString.GetMultilingualString("6d4a144f-641e-4ad8-b486-a33d4290b671", "Non Tax Credit Note Title"),
						ResString.GetMultilingualString("ff007231-3c53-49f6-af53-de97cd294cc0", "This registry controls the Title printed in a Non Tax Self Billed Payables Credit Note Transaction Document."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						ResString.GetMultilingualString("00b8ca57-1609-4b2a-90ee-a681a86d06ba", "Recipient Created / Self Billed Credit Note"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxSelfBilledCreditNoteTitle
		{
			get
			{
				return GetItem("TaxSelfBilledCreditNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("TaxSelfBilledCreditNoteTitle",
						Categories.Accounting_PayableDefaults_SelfBillingInvoice,
						ResString.GetMultilingualString("209c6c28-002f-496d-b183-c1fb2a2cf955", "Tax Credit Note Title"),
						ResString.GetMultilingualString("7aa9646a-6d8c-4e01-a76c-eb459cb8426a", "This registry controls the Title printed in a Tax Self Billed Payables Credit Note Transaction Document."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						ResString.GetMultilingualString("1d89077e-b881-45b6-9fe1-df2415226c9c", "Recipient Created / Self Billed Tax Credit Note"));
				});
			}
		}

		public MultilingualStringRegistryItem NonTaxSelfBilledAdjustmentNoteTitle
		{
			get
			{
				return GetItem("NonTaxSelfBilledAdjustmentNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("NonTaxSelfBilledAdjustmentNoteTitle",
						Categories.Accounting_PayableDefaults_SelfBillingInvoice,
						ResString.GetMultilingualString("549979b1-e4c0-45b3-a4be-da8d423d92dc", "Non Tax Adjustment Note Title"),
						ResString.GetMultilingualString("3cc21f39-a3e6-434c-b130-4fb6fbb8a340", "This registry controls the Title printed in a Non Tax Self Billed Payables Adjustment Note Transaction Document."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						ResString.GetMultilingualString("7fa859e5-83cc-40cc-8e48-a1f37c3824ab", "Recipient Created / Self Billed Adjustment Note"));
				});
			}
		}

		public MultilingualStringRegistryItem TaxSelfBilledAdjustmentNoteTitle
		{
			get
			{
				return GetItem("TaxSelfBilledAdjustmentNoteTitle", delegate
				{
					return new MultilingualStringRegistryItem("TaxSelfBilledAdjustmentNoteTitle",
						Categories.Accounting_PayableDefaults_SelfBillingInvoice,
						ResString.GetMultilingualString("cdc34008-e348-476b-9e98-5c30157af72b", "Tax Adjustment Note Title"),
						ResString.GetMultilingualString("c6172b10-83cc-47db-9549-3d36a1490ddb", "This registry controls the Title printed in a Tax Self Billed Payables Adjustment Note Transaction Document."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						ResString.GetMultilingualString("adf79d96-e248-4c86-99c8-7506301d4bb2", "Recipient Created / Self Billed Tax Adjustment Note"));
				});
			}
		}

		#region AccountingRegistryItem Class

		public class AccountingRegistryItem : RegistryItemImpl
		{
			public AccountingRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint)
				: this(name, subCategory, caption, hint, new GuidRegistryDataType())
			{
			}

			public AccountingRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, Guid defaultValue)
				: this(name, subCategory, caption, hint, new GuidRegistryDataType(), defaultValue)
			{
			}

			public AccountingRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, GuidRegistryDataType dataType)
				: base(name, subCategory, caption, hint, dataType, RegistryStorageFlags.System)
			{
				EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader);
			}

			public AccountingRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, GuidRegistryDataType dataType, RegistryStorageFlags storage)
				: base(name, subCategory, caption, hint, dataType, storage)
			{
				EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader);
			}

			public AccountingRegistryItem(string name, MultilingualString subCategory, MultilingualString caption, MultilingualString hint, GuidRegistryDataType dataType, Guid defaultValue)
				: base(name, subCategory, caption, hint, dataType, RegistryStorageFlags.System, defaultValue)
			{
				EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader);
			}
		}

		#endregion

		public StringRegistryItem TransactionReportFilters
		{
			get
			{
				return GetItem("TransactionReportFilters", delegate
				{
					return new StringRegistryItem("TransactionReportFilters", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden);
				});
			}
		}

		public BooleanRegistryItem ReceivableAllowUserToModifyWHTId
		{
			get
			{
				return GetItem("ReceivableAllowUserToModifyWHTId", delegate
				{
					return new BooleanRegistryItem("ReceivableAllowUserToModifyWHTId", Categories.Accounting_ReceivableDefaults_DefaultSettings, ResString.GetMultilingualString("dea6b7d2-93ab-4ee4-947b-08a2cdb91d4a", "Allow user to modify Withholding Tax ID"), ResString.GetMultilingualString("4BDC3AA1-C4FF-49CB-8306-C0FF1D6E20A9", @"This registry is no longer relevant in most Login Companies. This registry refers to obsolete Withholding Tax features that recorded WHT Tax IDs on individual charge lines similar to GST/VAT features. 

Please do not use this registry."), RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
				});
			}
		}

		public BooleanRegistryItem PayableAllowUserToModifyWHTId
		{
			get
			{
				return GetItem("PayableAllowUserToModifyWHTId", delegate
				{
					return new BooleanRegistryItem("PayableAllowUserToModifyWHTId", Categories.Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("dea6b7d2-93ab-4ee4-947b-08a2cdb91d4a", "Allow user to modify Withholding Tax ID"), ResString.GetMultilingualString("0240CBA8-89DF-4603-B9CF-41DC9F9056BB", @"This registry is no longer relevant in most Login Companies. This registry refers to obsolete Withholding Tax features that recorded WHT Tax IDs on individual charge lines similar to GST/VAT features. 

Please do not use this registry."), RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
				});
			}
		}

		public BooleanRegistryItem PayableFinalFlag
		{
			get
			{
				return GetItem("PayableFinalFlag", delegate
				{
					return new BooleanRegistryItem("PayableFinalFlag", Categories.Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("83d73b36-61e5-43ec-b536-aa9559d4bb6f", "Final Flag"), ResString.GetMultilingualString("32383683-6d46-4d44-872f-c70d69de2b29", "Default value for Final Flag when entering AP Invoice.\r\nNote: When Final Flag is set to Yes, all accruals relating to the charge line will be reversed."), RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
				});
			}
		}

		public BooleanRegistryItem PayableFinalFlagForConsolCost
		{
			get
			{
				return GetItem("PayableFinalFlagForConsolCost", delegate
				{
					return new BooleanRegistryItem("PayableFinalFlagForConsolCost", Categories.Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("72382ebd-527f-4e87-bf9f-8433ef29c7b2", "Final Flag Default for Consol Costs on AP Invoice Screen"), ResString.GetMultilingualString("3625c247-5f62-47fe-9480-fca87f0713b7", "When this registry is set to 'Yes', {0} will default the 'Final' flag to true for Consol Costs posted through the Accounts Payable Invoice screen.", BrandingFactory.Instance.ProductName), RegistryStorageFlags.Company | RegistryStorageFlags.System, true);
				});
			}
		}

		public BooleanRegistryItem AutoImportIntercompanyInvoices
		{
			get
			{
				return GetItem("AutoImportIntercompanyInvoices", delegate
				{
					return new BooleanRegistryItem("AutoImportIntercompanyInvoices", Categories.Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("0381c2fb-1743-46fa-bfe1-7b73558192c4", "Auto Import Sister Company AR Invoices as AP Invoices"), ResString.GetMultilingualString("a362651f-ed10-4232-b34f-ade40658e699", "When this flag is set to 'Yes', the batch processor will automatically import sister company AR invoices as AP invoices."), RegistryStorageFlags.Company, false);
				});
			}
		}

		public BooleanRegistryItem EnableValidationWhenAutoImportIntercompanyInvoices
		{
			get
			{
				return GetItem("EnableValidationWhenAutoImportIntercompanyInvoices", delegate
				{
					return new BooleanRegistryItem("EnableValidationWhenAutoImportIntercompanyInvoices",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						(NoResString)"Enable Validation When Auto Import Sister Company AR Invoices as AP Invoices",
						(NoResString)"When this flag is set to 'Yes', the Validation will be enabled when the batch processor will automatically import sister company AR invoices as AP invoices.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public BooleanRegistryItem ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber
		{
			get
			{
				return GetItem("ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber", delegate
				{
					return new BooleanRegistryItem("ImportSisterCompanyInvoiceComplianceNumberAsInvoiceNumber",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("9097f296-8ae9-44c6-b06e-1bbe7091a1b8", "Import Sister Company Invoice Compliance Number As Invoice Number"),
						ResString.GetMultilingualString("5d316206-13b5-4ef7-9ebf-b94733ee44da", "When this registry is set to 'Yes', importing sister company invoice with compliance number will set receiving company 'Invoice Number’ as the sending company ‘Compliance Number’ and 'Supplier Cost Reference' as the sending company ‘Invoice Number’."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem PayableAllowUserToStoreARDoc
		{
			get
			{
				return GetItem("PayableAllowUserToStoreARDoc", delegate
				{
					return new BooleanRegistryItem("PayableAllowUserToStoreARDoc", Categories.Accounting_PayableDefaults_DefaultSettings, ResString.GetMultilingualString("e1ece882-f318-4cbb-80d8-a4930731150a", "Store AR Invoice Documents when Importing Intercompany AP Invoices"), ResString.GetMultilingualString("51bbf7dd-1cc2-4a48-b980-13159778a822", "When this flag is set to 'Yes', AR Invoice documents will be stored against the Intercompany AP Invoices' eDocs tab when importing."), RegistryStorageFlags.Company, false);
				});
			}
		}

		public BooleanRegistryItem ShowPaymentRemittancePrintDialogue
		{
			get
			{
				return GetItem("ShowPaymentRemittancePrintDialogue", delegate
				{
					return new BooleanRegistryItem("ShowPaymentRemittancePrintDialogue", Categories.Accounting_PayableDefaults_DefaultSettings_PaymentPrintDefaults, ResString.GetMultilingualString("7580b2e6-9577-46a3-9f8d-e0014247815f", "Show Payment Print Dialogue"), ResString.GetMultilingualString("7aecb152-bff4-4676-83f9-ac17a50c7398", "Check this to show Payment Print options"), RegistryStorageFlags.Company | RegistryStorageFlags.System, true);
				});
			}
		}

		public BooleanRegistryItem PrintLogoOnCheque
		{
			get
			{
				return GetItem("PrintLogoOnCheque", delegate
				{
					return new BooleanRegistryItem("PrintLogoOnCheque", Categories.Accounting_PayableDefaults_DefaultSettings_PaymentPrintDefaults, ResString.GetMultilingualString("027ef005-913c-4883-9c6d-d377e1d5a03f", "Print Letterhead On Cheque"), ResString.GetMultilingualString("07f85f31-5901-4fb5-aacf-214652ea055f", "This registry item controls the printing of a letterhead on your cheque stock.\r\n - Select 'Yes' if {0} should print the company letterhead on the top of the remittance section of your Cheque Stock.\r\n - Select 'No' if {0} should not print the company letterhead on the top of the remittance section of your Cheque Stock (Use this option if you have the letterhead pre-printed on the cheque stock).", BrandingFactory.Instance.ProductName), RegistryStorageFlags.Company | RegistryStorageFlags.System, true);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public CodePairRegistryItem DefaultPaymentRemittancePrintOption
		{
			get
			{
				return GetItem("DefaultPaymentRemittancePrintOption", delegate
				{
					return new CodePairRegistryItem("DefaultPaymentRemittancePrintOption", Categories.Accounting_PayableDefaults_DefaultSettings, (NoResString)"Default Payment Voucher / Remittance Advice Print Option", (NoResString)"Select the default Payment Voucher / Remittance Advice print option", OLookUpEditType.PaymentRemittancePrintOption, RegistryStorageFlags.Company | RegistryStorageFlags.System, RegistryOptions.IsHidden, Constants.PaymentRemittancePrintOption.PrintRemittanceAdvice);
				});
			}
		}

		#endregion

		public BranchLevelPostingConfigurationRegistryItem PayableEnforceBranchLevelPosting
		{
			get
			{
				return GetItem("PayableEnforceBranchLevelPosting", delegate
				{
					var item = new BranchLevelPostingConfigurationRegistryItem(
						"PayableEnforceBranchLevelPosting"
						, Categories.Accounting_PayableDefaults_DefaultSettings
						, ResString.GetMultilingualString("9052fb54-9e7e-4ea6-8a23-fe4d2a413667", "Enforce Posting at Branch Level")
						, ResString.GetMultilingualString("ec820b08-8d12-46c2-a8ee-dc4095f09b9c", @"This registry affects the posting behaviors of Payables INV, CRD, ADJ and Cash Book DPY transactions only.
By default this registry is set to No and charge lines with any mix of branches are permitted within each transaction.
This registry should be set to NO when tax collection and reporting is a Company level registration shared by all branches in the one company.
This registry should be set to YES when tax registration, collection and reporting is a Branch level registration.
When set to YES:
- Posting will restrict the mix of line branches permitted within a single transaction.
- Unless defined in the grid below, lines for separate Branches will post in separate transactions.
- Branch Posting Groups can be defined in the grid below to allow mixed transactions.
- The transaction header branch is set from the Line branch, then Job Header Branch, falling back to the Permitted Posting Group check box.
Note:  This registry does NOT affect General Ledger Journals, Job Revenue Journals or other accounting entries.")
						, RegistryStorageFlags.Company
						, RegistryOptions.Default
						, new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false });

					return item;
				});
			}
		}

		public BooleanRegistryItem PayableEnforceUniqueTransactionNumber
		{
			get
			{
				return GetItem("EnforceUniqueAPTransactionNumber", delegate
				{
					var item = new BooleanRegistryItem(
						"EnforceUniqueAPTransactionNumber",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("F95E58EF-DFB1-45F2-9F3A-7A5EBBBE0B35", "Enforce Unique Transaction Number"),
						ResString.GetMultilingualString("135AC18F-7790-4481-AC0D-FC6C2E1CB252", @"By default, the system will allow you to post AP Invoice, AP Credit Note and AP Adjustment Note with the same Transaction Number.
For example:
	AP INV 00001001
	AP CRD 00001001
	AP ADJ 00001001

If required, this registry can be used to enforce unique transaction number across different transaction type.
For example:
	AP INV 00001001
	AP CRD 00001002
	AP ADJ 00001003

This is useful if you need to align the validation rule in CargoWiseOne and your external ERP system."),
						RegistryStorageFlags.Company,
						false);

					item.OnBuildLogReference += (args) => Res.GetString("C9730954-AEB1-4D80-B089-9D0D264E8D29", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

					return item;
				});
			}
		}

		public EnforceZeroBalanceDisbursementsRegistryItem EnforceZeroBalanceDisbursements
		{
			get
			{
				return GetItem("EnforceZeroBalanceDisbursements", delegate
				{
					var item = new EnforceZeroBalanceDisbursementsRegistryItem(
						"EnforceZeroBalanceDisbursements"
						, Categories.Accounting_JobInvoicing
						, ResString.GetMultilingualString("A11D9034-32B1-41F3-AE4B-D86B00A4BEBF", "Enforce Zero Balance Disbursements")
						, ResString.GetMultilingualString("0FCDD591-CDD9-4D0D-9946-09B9F6F38A91", @"This registry determines whether a zero balance between cost and sell amounts on disbursements is enforced.
When enabling this registry, staff and group security rights should be checked to ensure only authorized users are granted rights to “Save Non-zero Balance Disbursements”.
The available options are:
DEF – No Validation (this is the registry default).
CUR – Validate OS Currency and Amounts are Equal.
This option enforces both the cost and sell OS amount and currency are equal. It does not take into account the local cost and sell amounts.
LOC – Validate Local Amounts are Equal.
This option enforces the local cost is equal to the local sell less the value of any CFX Journal that applies to the transaction line.
Please note: If you use Currency Exchange Uplift (CFX) and wish to use this option, then Local Cost and Local Sell amount will only balance if the CFX Journal functionality is enabled for this company. Please refer to registry - Job Invoicing > Enable CFX.
EIT – Validate Either OS Currency and Amounts or Local Amounts are Equal (recommended method).
This option will enforce that either local cost and sell (minus CFX Journal amount) are equal or OS currency and amounts are equal.
BTH – Validate Both OS Currency and Amounts and Local Amounts are Equal.
This is the strictest validation and will ensure that both OS amount and currency are equal and Local Amount and currency (minus CFX Journal amount) are equal.")
						, RegistryStorageFlags.Company
						, RegistryOptions.Default);

					return item;
				});
			}
		}

		public CustomDefaultDepartmentConfigurationRegistryItem CustomDefaultDepartmentConfiguration
		{
			get
			{
				return GetItem("CustomDefaultDepartmentConfiguration", delegate
				{
					var item = new CustomDefaultDepartmentConfigurationRegistryItem(
						"CustomDefaultDepartmentConfiguration"
						, Categories.Accounting_JobInvoicing_DefaultDepartments
						, ResString.GetMultilingualString("5B54F82F-8958-4FB5-A27B-4624F7B06910", "Custom Department Defaulting Configuration")
						, ResString.GetMultilingualString("B8CB5880-5B7F-4846-83E0-D56B4BF296FF", @"This registry can no longer be edited as it has been replaced by the new GLOW Production Rules Engine. 
The new engine is accessed by enabling the “Custom Department Defaulting Rules Engine Configuration” registry and visiting the GLOW portal at the link “Set Department Defaulting Rules” below.

If the new engine is not enabled, existing registry configurations will apply.

Please contact support if you still require changes to this registry.

This registry is for developers only.

This registry allows you to configure additional rules for defaulting a department into job header. These custom rules are assessed in conjunction with the other department defaulting configurations in this registry group. For more information about using this feature, please search MyAccount for resources on Custom Department Defaulting Configuration.

To configure custom department defaulting rules, you need to supply a valid expression using IronPython programming language. Please note: Only IT personnel with a strong understanding of programming concepts and data structures should attempt this.

At run-time, your custom script will be appended to this method signature and will be executed in IronPython to determine the appropriate department:
getDefaultDepartment(obj, company, branch, department).

Job parameters:
obj - represents the Generic Freight Wrapper context.
You can access any of the properties on the operational jobs available from the GenericFreightWrapper used for DocBuilder documents. For example, to access Service Level on a shipment job, use obj.BaseShipment.ServiceLevel.RS_Code.

Environmental values:
company - This is the login company of the user.
branch - This is the login branch of the user.
department - This is the login department of the user.
You can access any properties on the login company, branch and department of the user creating the invoicing job. For example, for the country of the login company, use company.AccountingCountry.

Tip: To find the property name for any field on a form, click into that field and press CRTL+SHIFT+R and note down the Binding Member. Once you have the property name, you can find the corresponding macro using the Common Data Source Maps for GenericFreightJob wrapper. To do this, from the relevant form (e.g. a Shipment job), navigate to Documents > Customize > Common Data Source Maps > GenericFreightJob and locate the required Data Field and its Property Information/Macro. For example, Forwarding Shipment’s Destination is expressed as a macro <BaseShipment.Destination.RL_Code>. You can then construct a Python expression as obj.BaseShipment.Destination.RL_Code.

Your code must return a valid department code in order to be accepted into the job header. If the code returns an invalid or inactive department, then the department will be left blank – user will have to manually select the department. If the code returns no (or blank) value, or if the expression is incorrect, then the system will default the value as setup for each operation in other department defaulting registries.

As a guide, your expression should generally contain an “if” and “else”, as well as “return” statements. For example, you may wish to attribute all Air Export freight with Express service level to a specific department, say FEX, otherwise set the department to the user’s login department. Your expression would be:

if obj.BaseShipment.InvoicingSupporter.IsExport and obj.BaseShipment.TransportMode == 'AIR' and obj.BaseShipment.ServiceLevel.RS_Code == 'I2':
    return 'FEX'
else:
    return department.GE_Code

Note: It is important that your expression is indented appropriately with tabs just like any IronPython code block, for the system to be able to interpret it.

Once you have defined your expression, you can evaluate it against any existing billing job. To do this, enter or select a job number and click Evaluate below the expression box.")
						, RegistryStorageFlags.System | RegistryStorageFlags.Company
						, RegistryOptions.Default);

					return item;
				});
			}
		}

		public CustomDefaultBranchConfigurationRegistryItem CustomDefaultBranchConfiguration
		{
			get
			{
				return GetItem("CustomDefaultBranchConfiguration", delegate
				{
					var item = new CustomDefaultBranchConfigurationRegistryItem(
						"CustomDefaultBranchConfiguration"
						, Categories.Accounting_JobInvoicing
						, ResString.GetMultilingualString("C0B02F57-24F3-452F-AE60-5317F623ADAC", "Custom Branch Defaulting Configuration")
						, ResString.GetMultilingualString("B1B77987-6A32-40FC-89CA-96A6A9E0FA5B", @"This registry can no longer be edited as it has been replaced by the new GLOW Production Rules Engine. 
The new engine is accessed by enabling the “Custom Branch Defaulting Rules Engine Configuration” registry and visiting the GLOW portal at the link “Set Branch Defaulting Rules” below.

If the new engine is not enabled, existing registry configurations will apply.

Please contact support if you still require changes to this registry.

This registry is for developers only.

This registry allows you to configure additional rules for defaulting a Branch into job header. These custom rules are assessed in conjunction with the other Branch defaulting configurations. For more information about using this feature, please search MyAccount for resources on Custom Branch Defaulting Configuration.

To configure custom Branch defaulting rules, you need to supply a valid expression using IronPython programming language. Please note: Only IT personnel with a strong understanding of programming concepts and data structures should attempt this.

At run-time, your custom script will be appended to this method signature and will be executed in IronPython to determine the appropriate Branch:
getDefaultBranch(obj, company, branch, department).

Job parameters:
obj – represents the Generic Freight Wrapper context.
You can access any of the properties on the operational jobs available from the GenericFreightWrapper used for DocBuilder documents. For example, to access Delivery Agent on a shipment job, use obj.BaseShipment.DeliveryAgent.OH_Code.

Environmental values:
company – This is the login company of the user.
branch – This is the login branch of the user.
department – This is the login department of the user.
You can access any properties on the login company, branch and department of the user creating the invoicing job. For example, for the country of the login company, use company.AccountingCountry.

Tip: To find the property name for any field on a form, click into that field and press CTRL+SHIFT+R and note down the Binding Member. Once you have the property name, you can find the corresponding macro using the Common Data Source Maps for GenericFreightJob expressed as a macro <BaseShipment.DeliveryAgent.OH_Code>. You can then construct a Python expression as obj.BaseShipment.DeliveryAgent.OH_Code.

Your code must return a valid Branch code in order to be accepted into the job header. If the code returns an invalid or inactive Branch, then the Branch will be left blank – user will have to manually select the Branch. If the code returns no (or blank) value, or if the expression is incorrect, then the system will default the value as setup in other Branch defaulting registries.

As a guide, your expression should generally contain an “if” and “else”, as well as “return” statements.

For example, you may wish to attribute all Air Import freight with Delivery Agent 'DELAGTB12' to a specific Branch, say 'B12', otherwise set the branch to the user’s login branch. Your expression would be:
if obj.BaseShipment.InvoicingSupporter.IsImport and obj.BaseShipment.TransportMode == 'AIR' and obj.BaseShipment.DeliveryAgent.OH_Code == 'DELAGTB12':
    return 'B12'
else:
    return branch.GB_Code

Note: It is important that your express is indented appropriately with tabs just like any IronPython code block, for the system to be able to interpret it.

Once you have defined your expression, you can evaluate it against any existing billing job. To do this, enter or select a job number and click Evaluate below the expression box.")
						, RegistryStorageFlags.System | RegistryStorageFlags.Company
						, RegistryOptions.Default);

					return item;
				});
			}
		}

		public BranchLevelPostingConfigurationRegistryItem ReceivableEnforceBranchLevelPosting
		{
			get
			{
				return GetItem("ReceivableEnforceBranchLevelPosting", delegate
				{
					var item = new BranchLevelPostingConfigurationRegistryItem(
						"ReceivableEnforceBranchLevelPosting"
						, Categories.Accounting_ReceivableDefaults_DefaultSettings
						, ResString.GetMultilingualString("E1C2E9EB-7FE4-4BB2-94D3-A0DAB2F8C25E", "Enforce Posting at Branch Level")
						, ResString.GetMultilingualString("0E3EF062-648F-414B-A102-B918B7826E7C", @"This registry affects the posting behaviors of Receivables INV, CRD, ADJ and Cash Book DRC transactions only.
By default this registry is set to No and charge lines with any mix of branches are permitted within each transaction.
This registry should be set to NO when tax collection and reporting is a Company level registration shared by all branches in the one company.
This registry should be set to YES when tax registration, collection and reporting is a Branch level registration.
When set to YES:
- Posting will restrict the mix of line branches permitted within a single transaction.
- Unless defined in the grid below, lines for separate Branches will post in separate transactions.
- Branch Posting Groups can be defined in the grid below to allow mixed transactions.
- The transaction header branch is set from the Line branch, then Job Header Branch, falling back to the Permitted Posting Group check box.
Note:  This registry does NOT affect General Ledger Journals, Job Revenue Journals or other accounting entries.")
						, RegistryStorageFlags.Company
						, RegistryOptions.Default
						, new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = false });

					return item;
				});
			}
		}

		public IntercompanyEventConfigurationRegistryItem AutoImportIntercompanyEventConfiguration
		{
			get
			{
				return GetItem("AutoImportIntercompanyEventConfiguration", delegate
				{
					var item = new IntercompanyEventConfigurationRegistryItem(
						"AutoImportIntercompanyEventConfiguration"
						, Categories.Accounting_PayableDefaults_DefaultSettings
						, ResString.GetMultilingualString("BB6D705C-5243-4A23-B903-13A278FBDA8F", "Auto Import Intercompany Event Configuration")
						, ResString.GetMultilingualString("D202F9E5-24AA-4EDD-873F-7A0CD4DC6BBE", @"You can optionally restrict the auto-import of job related sister companies' invoices when specific event after a specific (start) date is present on the related operation record's Workflow & Tracking > Events tab.

Use this registry to specify one or more events and the start date for each event.

Rules:
	1.	When the related operation record contains at least one event that meets the specified start date, the sister company invoice will be auto imported.
		For all job-related invoices other than Freight Consol Invoice (i.e. Shipment, Port Transport, etc.), the system check the corresponding Operation record > Workflow & Tracking > Events.
		For job related invoices relating to Freight Consol Invoice, the system will check the corresponding Consolidation > Workflow & Tracking > Events.

	2.	This configuration only applies to auto import of intercompany invoices when the 'Auto Import Sister Company AR Invoices as AP Invoices' registry is enabled. 
		It does not apply to manual import via action menu, intercompany invoice approval module or 'ISI' workflow trigger action.

	3.	This configuration only applies to auto import of job related invoices. It does not apply to miscellaneous (non-job related) invoices and periodic invoices.")
						, RegistryStorageFlags.Company | RegistryStorageFlags.System
						, RegistryOptions.Default
						, new IntercompanyEventConfiguration());

					item.OnBuildLogReference += BuildAutoImportIntercompanyEventConfigurationLogReference;
					return item;
				});
			}
		}

		string BuildAutoImportIntercompanyEventConfigurationLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var result = string.Empty;

			var orginalEnabled = ((IntercompanyEventConfiguration)args.OriginalValue).EnableEventConfiguration;
			var newEnabled = ((IntercompanyEventConfiguration)args.NewValue).EnableEventConfiguration;

			if (orginalEnabled != newEnabled)
			{
				result += Res.GetString("B5116B02-3B2E-48CD-8D86-26129B5076C3", "Event Configuration {0}.", newEnabled ? (NoResString)"Enabled" : (NoResString)"Disabled") + "\r\n";
			}

			var originalElements = ((IntercompanyEventConfiguration)args.OriginalValue).IntercompanyEventSettingCollection.Cast<IntercompanyEventSetting>();
			var newElements = ((IntercompanyEventConfiguration)args.NewValue).IntercompanyEventSettingCollection.Cast<IntercompanyEventSetting>();

			foreach (var setting in newElements.Where(added => !originalElements.Any(original => original.StmEventCode == added.StmEventCode)))
			{
				result += Res.GetString("ECDA4A08-8658-4EF0-B373-A8AD051A16BC", "New Event Added '{0}' with Start Date '{1}'.", setting.StmEventCode, setting.StartDate.ToShortDateString()) + "\r\n";
			}

			foreach (var setting in originalElements)
			{
				var updatedElement = newElements.FirstOrDefault(x => x.StmEventCode == setting.StmEventCode && x.StartDate != setting.StartDate);
				if (updatedElement != null)
				{
					result += Res.GetString("F966D698-3F8B-474A-834C-424745648A67", "Event '{0}' Start Date change from '{1}' to '{2}'.", setting.StmEventCode, setting.StartDate.ToShortDateString(), updatedElement.StartDate.ToShortDateString()) + "\r\n";
				}

				if (!newElements.Any(x => x.StmEventCode == setting.StmEventCode))
				{
					result += Res.GetString("BF02B08D-2E76-4EE2-9D29-82D5936F1583", "Existing Event '{0}' deleted.", setting.StmEventCode) + "\r\n";
				}
			}

			return result;
		}

		public BooleanRegistryItem PrintCheque
		{
			get
			{
				return GetItem("PrintCheque", delegate
				{
					return new BooleanRegistryItem("PrintCheque",
													Categories.Accounting_PayableDefaults_DefaultSettings_PaymentPrintDefaults_DefaultPaymentPrintOption,
													ResString.GetMultilingualString("3e109382-6e8b-4ad7-b54f-561983110bf8", "Print Cheque"),
													ResString.GetMultilingualString("077eeb90-8250-423d-81a4-c04c90543ebd", @"When set to Yes, the 'Print Cheque' check box is automatically ticked in the Print Options for Payment form."),
													RegistryStorageFlags.Company | RegistryStorageFlags.System,
													false);
				});
			}
		}

		public BooleanRegistryItem PrintRemittanceAdvice
		{
			get
			{
				return GetItem("PrintRemittanceAdvice", delegate
				{
					return new BooleanRegistryItem("PrintRemittanceAdvice",
													Categories.Accounting_PayableDefaults_DefaultSettings_PaymentPrintDefaults_DefaultPaymentPrintOption,
													ResString.GetMultilingualString("70f71279-0354-4ade-8ecb-79d02dfd6bf9", "Print Remittance Advice"),
													ResString.GetMultilingualString("ebf89800-a09a-4843-b120-f66ee77033cf", @"When set to Yes, the 'Print Remittance Advice' check box is automatically ticked in the Print Options for Payment form."),
													RegistryStorageFlags.Company | RegistryStorageFlags.System,
													true);
				});
			}
		}

		public BooleanRegistryItem PrintPaymentVoucher
		{
			get
			{
				return GetItem("PrintPaymentVoucher", delegate
				{
					return new BooleanRegistryItem("PrintPaymentVoucher",
													Categories.Accounting_PayableDefaults_DefaultSettings_PaymentPrintDefaults_DefaultPaymentPrintOption,
													ResString.GetMultilingualString("7af8b43a-06d6-4ae1-a660-10bbd7414bf7", "Print Payment Voucher"),
													ResString.GetMultilingualString("c1f5e83d-a315-4ced-9e31-3b7f1e27eb80", "When set to Yes, the 'Print Payment Voucher' check box is automatically ticked in the Print Options for Payment form."),
													RegistryStorageFlags.Company | RegistryStorageFlags.System,
													false);
				});
			}
		}

		public BooleanRegistryItem PrintPaymentBatchListing
		{
			get
			{
				return GetItem(nameof(PrintPaymentBatchListing), delegate
				{
					return new BooleanRegistryItem(nameof(PrintPaymentBatchListing),
													Categories.Accounting_PayableDefaults_DefaultSettings_PaymentPrintDefaults_DefaultPaymentPrintOption,
													ResString.GetMultilingualString("084F4A9E-80A2-4C02-8836-7B701D5A5BBA", "Print Payment Batch Listing"),
													ResString.GetMultilingualString("48F3E9BD-8241-4BD3-8A35-A2DE050757F6", "When set to Yes, the 'Print Payment Batch Listing' check box is automatically ticked in the Print Options for Payment form. The Payment Batch Listing document lists the payments included in the batch, as well as the transactions each payment relates to. The document can be used to review and approve the payments for processing."),
													RegistryStorageFlags.Company | RegistryStorageFlags.System,
													false);
				});
			}
		}

		public StringRegistryItem InvoiceDetentionDemurrageStatements
		{
			get
			{
				RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetter = (companyPK, branchPK, departmentPK) => InvoiceDetentionDemurrageStatementsHelper.GetDefaultValue(companyPK, branchPK, departmentPK);

				return GetItem("InvoiceDetentionDemurrageStatements", delegate
				{
					var result = new StringRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue(
						"InvoiceDetentionDemurrageStatements",
						Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
						ResString.GetMultilingualString("cc7e15dc-c99e-4cb6-bb85-3e704012b906", "Detention/Demurrage Statement"),
						ResString.GetMultilingualString("6ce0f5fa-46ff-4b01-ad4d-a468a473c08a", "The statements below are displayed in the Doc Strip \"Container Penalties\". Override the registry if you wish to change these statements."),
						RegistryDataTypes.StringType,
						RegistryStorageFlags.Company,
						valueGetter));

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		static class InvoiceDetentionDemurrageStatementsHelper
		{
			[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
			[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Default string value")]
			static internal object GetDefaultValue(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var result = string.Empty;
				if (usCompanyPKs == null)
				{
					usCompanyPKs = new BusinessObjectFactory().Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.UnitedStates)).Select(x => x.PK.ToGuid()).ToHashSet();
				}

				if (usCompanyPKs.Contains(companyPK))
				{
					result = @"These charges are consistent with Federal Maritime Commission rules with respect to detention and demurrage as per ""Ocean Shipping Reform Act of 2022"", title 46 of the United States Code.
The common carrier's performance did not cause or contribute to the underlying invoiced charges.";
				}

				return result;
			}

			[ThreadStatic]
			static HashSet<Guid> usCompanyPKs;
		}

		public StringRegistryItem InvoicingDepartmentMapping
		{
			get
			{
				return GetItem("InvoicingDepartmentMappingFreightCustoms", delegate
				{
					string defaultFreightCustomsMapping = "FEA|CEA;FEL|CEL;FER|CER;FES|CES;FIA|CIA;FIL|CIL;FIR|CIR;FIS|CIS";
					StringRegistryItem result = new StringRegistryItem("InvoicingDepartmentMappingFreightCustoms",
																		Categories.Accounting_JobInvoicing_DefaultDepartments_DepartmentMapping,
																		ResString.GetMultilingualString("d4d97a33-e172-4eb0-ad82-cab74b177e69", "Customs Charges on Freight Job"),
																		ResString.GetMultilingualString("4adc300d-2da2-41b3-9074-7f64053e2885", "This registry enables mapping of default departments for Customs Charges on Freight Jobs.  The Customs Department will default at the line level on charge lines where the charge code belongs to the BON, BRK, CDS, OBO or OBR Charge Groups."),
																		RegistryStorageFlags.Company,
																		defaultFreightCustomsMapping);
					result.EditorInfo = new DepartmentMappingEditorInfo();
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingExportContainerised
		{
			get
			{
				return GetItem("ShippingExportContainerised", delegate
				{
					Guid sEC = new Guid("9C30B56B-2043-42BF-BBA8-665FE223C4B1");
					GuidRegistryItem result = new GuidRegistryItem("ShippingExportContainerised",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("2666d6bc-5f23-4270-919f-72c6fdf60306", "Liner & Agency Export Containerized"),
																	ResString.GetMultilingualString("74c8e0da-7a21-42d6-9948-9b0dca3709dc", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Export job having 'FCL' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sEC);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingExportBreakBulk
		{
			get
			{
				return GetItem("ShippingExportBreakBulk", delegate
				{
					Guid sEB = new Guid("F97C6160-F847-4D0D-8E30-65657D3B8680");
					GuidRegistryItem result = new GuidRegistryItem("ShippingExportBreakBulk",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	 ResString.GetMultilingualString("34e010d9-71f0-4e34-aa94-b1923bd4d420", "Liner & Agency Export Break Bulk"),
																	 ResString.GetMultilingualString("7730F874-75AC-4926-B8E8-6867E3A739D4", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Export job having 'BBK' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 sEB);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingExportRoRo
		{
			get
			{
				return GetItem("ShippingExportRoRo", delegate
				{
					Guid sEV = new Guid("4C2B1F2A-3AB1-4EBF-9842-F6E2F5528DA9");
					GuidRegistryItem result = new GuidRegistryItem("ShippingExportRoRo",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("7fe28d3a-23ec-4771-9dc4-11072e289cd7", "Liner & Agency Export RORO"),
																	ResString.GetMultilingualString("fe07d62f-b264-4b2f-822c-758d8991b95b", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Export job having 'ROR' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sEV);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingExportBulk
		{
			get
			{
				return GetItem("ShippingExportBulk", delegate
				{
					Guid sEU = new Guid("F85D4264-9A8A-4CC3-8463-B19919D5EECF");
					GuidRegistryItem result = new GuidRegistryItem("ShippingExportBulk",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("6059a48b-75fc-44d1-91d2-891b9a20f1cc", "Liner & Agency Export Bulk"),
																	ResString.GetMultilingualString("27a1d0ba-f5c1-4955-bb19-01969a5b9232", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Export job having 'BLK' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sEU);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingExportLiquid
		{
			get
			{
				return GetItem("ShippingExportLiquid", delegate
				{
					Guid sEL = new Guid("1A3065F3-5FCF-4CD7-91ED-0D15957E66B7");
					GuidRegistryItem result = new GuidRegistryItem("ShippingExportLiquid",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("a3584018-fff4-410e-aab9-25f278c641ed", "Liner & Agency Export Liquid"),
																	ResString.GetMultilingualString("07843070-0c8b-4ebe-a046-ec49ce8633b6", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Export job having 'LQD' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sEL);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingImportContainerised
		{
			get
			{
				return GetItem("ShippingImportContainerised", delegate
				{
					Guid sIC = new Guid("F7E302F9-1311-4DE3-9AA1-9355D18EE5F0");
					GuidRegistryItem result = new GuidRegistryItem("ShippingImportContainerised",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("1ec1329e-856f-4155-845e-4b32a2d5466b", "Liner & Agency Import Containerized"),
																	ResString.GetMultilingualString("f763fa19-3013-495e-8ad1-68db4d36399d", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Import job having 'FCL' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sIC);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingImportBreakBulk
		{
			get
			{
				return GetItem("ShippingImportBreakBulk", delegate
				{
					Guid sIB = new Guid("26BBC697-D062-4D27-9B6B-776157BDBB25");
					GuidRegistryItem result = new GuidRegistryItem("ShippingImportBreakBulk",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("c9f636fe-9a63-4069-a2e3-79fc90312e5b", "Liner & Agency Import Break Bulk"),
																	ResString.GetMultilingualString("46a88f3c-85d0-4d4a-ad4a-c7785fd3f3e4", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Import job having 'BBK' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sIB);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingImportRoRo
		{
			get
			{
				return GetItem("ShippingImportRoRo", delegate
				{
					Guid sIV = new Guid("2946BCD7-EBF5-4676-BFE2-3CC4985744DB");
					GuidRegistryItem result = new GuidRegistryItem("ShippingImportRoRo",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("8c08f406-9fa2-4f31-b007-7dec92665d8b", "Liner & Agency Import RORO"),
																	ResString.GetMultilingualString("6415409b-7a5c-496a-907b-5a32632e8c3e", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Import job having 'ROR' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sIV);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingImportBulk
		{
			get
			{
				return GetItem("ShippingImportBulk", delegate
				{
					Guid sIU = new Guid("71FA6168-E487-4895-9F21-5BAC568884BF");
					GuidRegistryItem result = new GuidRegistryItem("ShippingImportBulk",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("6b630bdd-6c88-494f-b62b-aeb26352a8d0", "Liner & Agency Import Bulk"),
																	ResString.GetMultilingualString("978ea5b9-e72c-45c6-a330-3d0f04d95015", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Import job having 'BLK' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sIU);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingImportLiquid
		{
			get
			{
				return GetItem("ShippingImportLiquid", delegate
				{
					Guid sIL = new Guid("5C6089C8-95F4-4F96-94A3-D5FEE94F2AB1");
					GuidRegistryItem result = new GuidRegistryItem("ShippingImportLiquid",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("bfe151c1-5bbd-4eae-9a15-197f89db475d", "Liner & Agency Import Liquid"),
																	ResString.GetMultilingualString("768e175f-2476-4237-b441-feee2d3745d9", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Import job having 'LQD' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sIL);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingDomesticContainerised
		{
			get
			{
				return GetItem("ShippingDomesticContainerised", delegate
				{
					Guid sDC = new Guid("C9B0B975-89FF-4351-8B59-5B87BEAC2516");
					GuidRegistryItem result = new GuidRegistryItem("ShippingDomesticContainerised",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("dd5af630-9044-43b7-857f-2983e89203e8", "Liner & Agency Domestic Containerized"),
																	ResString.GetMultilingualString("57411f09-5202-4028-8695-c43fef611a97", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Domestic job having 'FCL' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sDC);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingDomesticBreakBulk
		{
			get
			{
				return GetItem("ShippingDomesticBreakBulk", delegate
				{
					Guid sDB = new Guid("2FDA89A7-B99A-4838-90A5-8824AE76D993");
					GuidRegistryItem result = new GuidRegistryItem("ShippingDomesticBreakBulk",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	 ResString.GetMultilingualString("97391562-f508-4f3b-96be-442b019c6419", "Liner & Agency Domestic Break Bulk"),
																	 ResString.GetMultilingualString("530c97d0-2911-4674-acaa-0949087b8c9a", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Domestic job having 'BBK' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 sDB);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingDomesticRoRo
		{
			get
			{
				return GetItem("ShippingDomesticRoRo", delegate
				{
					Guid sDV = new Guid("38226EAF-6C85-4C73-A140-D6C65D72E8B5");
					GuidRegistryItem result = new GuidRegistryItem("ShippingDomesticRoRo",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	 ResString.GetMultilingualString("32a23cd0-b9b4-4fe4-b608-19401583199c", "Liner & Agency Domestic RORO"),
																	 ResString.GetMultilingualString("9a823306-0e61-47dc-94e5-551d666a1cbb", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Domestic job having 'ROR' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 sDV);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingDomesticBulk
		{
			get
			{
				return GetItem("ShippingDomesticBulk", delegate
				{
					Guid sDU = new Guid("E5ABFBD9-3C8D-4036-BB98-23B4C12FE17F");
					GuidRegistryItem result = new GuidRegistryItem("ShippingDomesticBulk",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("98247bef-d4fe-4b91-9952-e95169bd07fa", "Liner & Agency Domestic Bulk"),
																	ResString.GetMultilingualString("caa304a7-9bd4-48e2-b3d2-43fa7d401b08", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Domestic job having 'BLK' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sDU);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingDomesticLiquid
		{
			get
			{
				return GetItem("ShippingDomesticLiquid", delegate
				{
					Guid sDL = new Guid("201F93D3-A930-44E8-9D72-8D514F49D016");
					GuidRegistryItem result = new GuidRegistryItem("ShippingDomesticLiquid",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	 ResString.GetMultilingualString("414f6e20-91fa-46c5-b3df-894211a2bcb4", "Liner & Agency Domestic Liquid"),
																	 ResString.GetMultilingualString("5d9561a9-50f6-44e1-ac82-d3e2259c225b", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading Domestic job having 'LQD' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 sDL);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingOtherContainerised
		{
			get
			{
				return GetItem("ShippingOtherContainerised", delegate
				{
					Guid sOC = new Guid("1C1AF1E4-A03E-4316-8EB5-7F6CEFF17D60");
					GuidRegistryItem result = new GuidRegistryItem("ShippingOtherContainerised",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	 ResString.GetMultilingualString("806f2ff1-9629-4982-b012-2e978689e377", "Liner & Agency Other Containerized"),
																	 ResString.GetMultilingualString("5c0860c8-fd36-4478-a031-c6b44a462005", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading job, only if it is not an Export/Import/Domestic Job and has 'FCL' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 sOC);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingOtherBreakBulk
		{
			get
			{
				return GetItem("ShippingOtherBreakBulk", delegate
				{
					Guid sOB = new Guid("98B58AA9-C29E-49F3-86EA-0C2A1E3D5720");
					GuidRegistryItem result = new GuidRegistryItem("ShippingOtherBreakBulk",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	 ResString.GetMultilingualString("ba088cd7-811f-49e3-ad0c-a0b1b602f27c", "Liner & Agency Other Break Bulk"),
																	 ResString.GetMultilingualString("feda0caa-fb75-4d2f-a3a1-84021cd13c10", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading job, only if it is not an Export/Import/Domestic Job and has 'BBK' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 sOB);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingOtherRoRo
		{
			get
			{
				return GetItem("ShippingOtherRoRo", delegate
				{
					Guid sOV = new Guid("370D8B19-6A58-4D77-9E82-E1A3563D6E52");
					GuidRegistryItem result = new GuidRegistryItem("ShippingOtherRoRo",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("cb12b06f-35a0-4e27-9428-5c43d39d8949", "Liner & Agency Other RORO"),
																	ResString.GetMultilingualString("0a7ecdd6-acfd-4fa6-8287-9b87553192e0", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading job, only if it is not an Export/Import/Domestic Job and has 'ROR' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sOV);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingOtherBulk
		{
			get
			{
				return GetItem("ShippingOtherBulk", delegate
				{
					Guid sOU = new Guid("04F905DF-8D72-49F2-9AE1-754C0F7A5874");
					GuidRegistryItem result = new GuidRegistryItem("ShippingOtherBulk",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("7e568dda-ca64-4690-b5fd-8acab2c664cb", "Liner & Agency Other Bulk"),
																	ResString.GetMultilingualString("e7ea247e-0bc9-4549-93c3-59d661dfc393", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading job, only if it is not an Export/Import/Domestic Job and has 'BLK' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sOU);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingOtherLiquid
		{
			get
			{
				return GetItem("ShippingOtherLiquid", delegate
				{
					Guid sOL = new Guid("B43C603F-0C56-41E6-B41F-81866E2A5448");
					GuidRegistryItem result = new GuidRegistryItem("ShippingOtherLiquid",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("b47733dc-b030-4116-bc75-e13c0e0507e4", "Liner & Agency Other Liquid"),
																	ResString.GetMultilingualString("4ea0d5f1-45d3-4670-b9fe-8c52f8adae75", @"When creating an Accounting Job Header on a Liner & Agency Booking/Bill Of Lading job, only if it is not an Export/Import/Domestic Job and has 'LQD' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	sOL);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingVoyageAccounting
		{
			get
			{
				return GetItem("ShippingVoyageAccounting", delegate
				{
					Guid sOA = new Guid("CEAD4CDC-AC8D-4268-AD21-A2764993E341");
					GuidRegistryItem result = new GuidRegistryItem("ShippingVoyageAccounting",
						Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("7eedf986-71ae-47eb-95eb-d64134e7d9b6", "Liner & Agency Voyage Accounting"),
																	ResString.GetMultilingualString("fcf057a2-5d0d-4750-af92-d5910e091fa2", @"When creating an Accounting Job Header on a Liner & Agency Voyage Accounting job, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						sOA);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem ShippingSundryCharges
		{
			get
			{
				return GetItem("ShippingSundryCharges", delegate
				{
					Guid sOA = new Guid("CEAD4CDC-AC8D-4268-AD21-A2764993E341");
					GuidRegistryItem result = new GuidRegistryItem("ShippingSundryCharges",
						Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
																	ResString.GetMultilingualString("a7128830-3272-484e-9718-c3caf149237a", "Liner & Agency Sundry Charges"),
																	ResString.GetMultilingualString("980dd1bd-90ec-4748-87a6-77fcd32b4a55", @"When creating an Accounting Job Header on a Liner & Agency Sundry Charges job, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						sOA);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public BooleanRegistryItem ShippingDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem("ShippingDefaultToCurrentLoginDept", () =>
					 new BooleanRegistryItem("ShippingDefaultToCurrentLoginDept",
							Categories.Accounting_JobInvoicing_DefaultDepartments_ShippingManager,
							ResString.GetMultilingualString("d77843dc-77ad-4a49-b672-798a09fab9bd", "Default to Current Login Dept."),
												ResString.GetMultilingualString("84fc4179-4ce2-4368-96bc-9a6586b06880", "When the value is set to Yes, the Job department will be defaulted to the department selected during login when creating new Shipping jobs. Otherwise, the system will default department as setup for each operation Activity, Direction and Cargo Type."),
												RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
												false));
			}
		}

		public BooleanRegistryItem ForwardingDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem("ForwardingDefaultToCurrentLoginDept", delegate
				{
					return new BooleanRegistryItem("ForwardingDefaultToCurrentLoginDept",
													Categories.Accounting_JobInvoicing_DefaultDepartments_Forwarding,
													ResString.GetMultilingualString("8e3d7074-ff04-4f80-9871-cc6c94dabe77", "Default to Current Login Dept."),
													ResString.GetMultilingualString("7d98e3d2-f51f-4fd5-9a9a-0cb490868644", "When the value is set to Yes, the Job department will be defaulted to the department selected during login when creating new Forwarding job. Otherwise, the system will default department as setup for each operation Activity, Direction and Mode.\r\n\r\nNote: For spot quotes created via WebTracker, the Job department will always be determined based on Direction and Mode."),
													RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
													false);
				});
			}
		}

		public GuidRegistryItem NCTSDepartureAir
		{
			get
			{
				return GetItem("NCTSDepartureAir", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NCTSDepartureAir",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS,
																	ResString.GetMultilingualString("DAE41357-9C2B-4FEF-AC91-6C03C36B381C", "NCTS Departure Air"),
																	ResString.GetMultilingualString("0D2F2137-CF44-4DAB-93B1-121A010DE323", @"When creating an Accounting Job Header on an NCTS Departure job having 'Air' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

		Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	CustomsExportAirUld.DefaultValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem NCTSDeparturePost
		{
			get
			{
				return GetItem("NCTSDeparturePost", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NCTSDeparturePost",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS,
																	ResString.GetMultilingualString("F3C83A92-38CA-47A5-B93B-B46CBE6D7DA5", "NCTS Departure Post"),
																	ResString.GetMultilingualString("9AF81A33-6202-4704-834B-40090A202A1D", @"When creating an Accounting Job Header on an NCTS Departure job having 'Mail/Post' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

		Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	CustomsExportPost.DefaultValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem NCTSDepartureRail
		{
			get
			{
				return GetItem("NCTSDepartureRail", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NCTSDepartureRail",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS,
																	ResString.GetMultilingualString("4065F967-B266-4BB1-A6F4-5ED3043D0941", "NCTS Departure Rail"),
																	ResString.GetMultilingualString("ADBCA193-3828-4FE8-9CEC-43C23EB31A08", @"When creating an Accounting Job Header on an NCTS Departure job having 'Rail' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

		Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	CustomsExportRail.DefaultValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem NCTSDepartureRoad
		{
			get
			{
				return GetItem("NCTSDepartureRoad", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NCTSDepartureRoad",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS,
																	ResString.GetMultilingualString("7A8A7C36-021C-4D8B-AD12-48560BA937F9", "NCTS Departure Road"),
																	ResString.GetMultilingualString("2D5A6773-084E-438C-909E-1BCCC1098846", @"When creating an Accounting Job Header on an NCTS Departure job having 'Road' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

		Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	CustomsExportRoad.DefaultValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem NCTSDepartureSeaFcl
		{
			get
			{
				return GetItem("NCTSDepartureSeaFcl", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NCTSDepartureSeaFcl",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS,
																	ResString.GetMultilingualString("9225C14F-C11C-43D1-A44A-00E646690132", "NCTS Departure Sea FCL"),
																	ResString.GetMultilingualString("74FDCF22-C680-44E0-A5A3-436D5BC11390", @"When creating an Accounting Job Header on an NCTS Departure job having 'Sea' as the Transport Mode and 'FCL' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

		Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	CustomsExportSeaFcl.DefaultValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem NCTSDepartureSeaLcl
		{
			get
			{
				return GetItem("NCTSDepartureSeaLcl", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NCTSDepartureSeaLcl",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS,
																	ResString.GetMultilingualString("02089CBB-7974-43B2-97E4-7DE2CEEEE2FA", "NCTS Departure Sea LCL"),
																	ResString.GetMultilingualString("50684BC0-2BCE-4332-A94A-F804FA94166C", @"When creating an Accounting Job Header on an NCTS Departure job having 'Sea' as the Transport Mode and 'LCL' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

		Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	CustomsExportSeaLcl.DefaultValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem NCTSDepartureOther
		{
			get
			{
				return GetItem("NCTSDepartureOther", delegate
				{
					var result = new GuidRegistryItem("NCTSDepartureOther",
														Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS,
														ResString.GetMultilingualString("D0DD43E4-989F-4C73-A722-F4856F8C782F", "NCTS Departure Other"),
														ResString.GetMultilingualString("5CCA1D42-B151-469C-A392-1B117D3A21C0", "The default Department to use when creating a new billing job."),
														RegistryStorageFlags.Company | RegistryStorageFlags.System,
														CustomsOther.DefaultValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem NCTSArrival
		{
			get
			{
				return GetItem("NCTSArrival", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NCTSArrival",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS,
																	ResString.GetMultilingualString("6FB5934D-1636-4EB6-ABB4-64E95182CAD3", "NCTS Arrival"),
																	ResString.GetMultilingualString("7DB9ED53-92BF-424C-B8F2-79B9E0218D5C", "The default Department to use when creating a new billing job on an NCTS Arrival."),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	CustomsImportOther.DefaultValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public BooleanRegistryItem NCTSDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem("NCTSDefaultToCurrentLoginDept", delegate
				{
					return new BooleanRegistryItem("NCTSDefaultToCurrentLoginDept",
													Categories.Accounting_JobInvoicing_DefaultDepartments_NCTS,
													ResString.GetMultilingualString("4E9440A7-6C4C-4560-99FD-F4D4F5F04CC8", "Default to Current Login Dept."),
													ResString.GetMultilingualString("41487637-932F-42F7-A7A8-15461A8DA17E", "When the value is set to Yes, the Job department will be defaulted to the department selected during login when creating new NCTS Phase 5. Otherwise, the system will default department as setup for each operation Activity, Direction and Mode."),
													RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
													CustomsDefaultToCurrentLoginDept.DefaultValue);
				});
			}
		}

		public GuidRegistryItem CustomsExportSeaFcl
		{
			get
			{
				return GetItem("CustomsExportSeaFcl", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsExportSeaFcl",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	ResString.GetMultilingualString("136cb2f6-a82f-467b-99b1-8d93e3df71d8", "Customs Export Sea FCL"),
																	ResString.GetMultilingualString("605b0786-6958-443f-a5f3-9d776e274845", @"When creating an Accounting Job Header on a Declaration Export job having 'Sea' as the Transport Mode and 'FCL' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.ClearanceExportSea);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsExportSeaLcl
		{
			get
			{
				return GetItem("CustomsExportSeaLcl", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsExportSeaLcl",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	 ResString.GetMultilingualString("c5b7a2d7-7c72-4778-b5ac-56aeb59295b2", "Customs Export Sea LCL"),
																	 ResString.GetMultilingualString("a5933dfe-a1f4-421f-a767-1c9c999d2b61", @"When creating an Accounting Job Header on a Declaration Export job having 'Sea' as the Transport Mode and 'LCL' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.ClearanceExportSea);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsExportAirUld
		{
			get
			{
				return GetItem("CustomsExportAirUdl", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsExportAirUdl",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	ResString.GetMultilingualString("0d564515-ab9a-4d51-92e5-d498b9ae2074", "Customs Export Air ULD"),
																	ResString.GetMultilingualString("df4e0525-ee65-4cf2-b4ba-e617c90097e9", @"When creating an Accounting Job Header on a Declaration Export job having 'Air' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.ClearanceExportAir);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsExportRail
		{
			get
			{
				return GetItem("CustomsExportRail", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsExportRail",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	ResString.GetMultilingualString("e7717711-1ed5-42ae-97e9-a49568030975", "Customs Export Rail"),
																	ResString.GetMultilingualString("a0fc8d78-7872-42d0-a585-e2ab6d3a79ee", @"When creating an Accounting Job Header on a Declaration Export job having 'Rail' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.ClearanceExportRail);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsExportRoad
		{
			get
			{
				return GetItem("CustomsExportRoad", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsExportRoad",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	 ResString.GetMultilingualString("e365dd0a-2df5-4c1b-b01b-6c279fc9662c", "Customs Export Road"),
																	 ResString.GetMultilingualString("1afb2225-8cf5-4492-8290-acdc071ea58a", @"When creating an Accounting Job Header on a Declaration Export job having 'Road' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.ClearanceExportRoad);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsExportPost
		{
			get
			{
				return GetItem("CustomsExportPost", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsExportPost",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	 ResString.GetMultilingualString("a1d05cb2-4e75-4d59-bcd4-38b36986337f", "Customs Export Post"),
																	 ResString.GetMultilingualString("a97721d9-9d9b-40de-befd-15a9aac86d99", @"When creating an Accounting Job Header on a Declaration Export job having 'Mail/Post' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.ClearancePost);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsImportSeaFcl
		{
			get
			{
				return GetItem("CustomsImportSeaFcl", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsImportSeaFcl",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	 ResString.GetMultilingualString("3f715843-e088-4f0f-a04f-1dabf7e73f59", "Customs Import Sea FCL"),
																	 ResString.GetMultilingualString("f87682ee-c1b7-45b0-98af-1dfe3df6bb0e", @"When creating an Accounting Job Header on a Declaration Import job having 'Sea' as the Transport Mode and 'FCL' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.ClearanceImportSea);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsImportSeaLcl
		{
			get
			{
				return GetItem("CustomsImportSeaLcl", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsImportSeaLcl",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	ResString.GetMultilingualString("1ffa17dd-317f-44cc-a313-a47f17a93de7", "Customs Import Sea LCL"),
																	ResString.GetMultilingualString("4fc287f0-52f5-4d82-8cc8-2a3aa628c565", @"When creating an Accounting Job Header on a Declaration Import job having 'Sea' as the Transport Mode and 'LCL' as the Container Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.ClearanceImportSea);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsImportAirUld
		{
			get
			{
				return GetItem("CustomsImportAirUld", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsImportAirUld",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	 ResString.GetMultilingualString("45dff2b8-22de-4440-a899-bbebe7521266", "Customs Import Air ULD"),
																	 ResString.GetMultilingualString("f98a9d72-2014-4bef-8b74-5c71343b0b40", @"When creating an Accounting Job Header on a Declaration Import job having 'Air' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.ClearanceImportAir);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsImportRail
		{
			get
			{
				return GetItem("CustomsImportRail", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsImportRail",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	ResString.GetMultilingualString("bec2124d-215a-4988-88ef-0589ae14f079", "Customs Import Rail"),
																	ResString.GetMultilingualString("809b124f-6523-44d7-8eae-0a1686a3c749", @"When creating an Accounting Job Header on a Declaration Import job having 'Rail' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.ClearanceImportRail);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsImportRoad
		{
			get
			{
				return GetItem("CustomsImportRoad", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsImportRoad",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	ResString.GetMultilingualString("02d5edb3-596d-45f8-bfe9-dfeb8a40e735", "Customs Import Road"),
																	ResString.GetMultilingualString("c322586e-28b1-44c4-b5e9-59a5e2a8251e", @"When creating an Accounting Job Header on a Declaration Import job having 'Road' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.ClearanceImportRoad);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsImportPost
		{
			get
			{
				return GetItem("CustomsImportPost", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsImportPost",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	 ResString.GetMultilingualString("1895b2fe-9e9f-45ee-b31b-1bf21498454f", "Customs Import Post"),
																	 ResString.GetMultilingualString("26da17c3-dc37-4079-aa94-52e6021ac83d", @"When creating an Accounting Job Header on a Declaration Import job having 'Mail/Post' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.ClearancePost);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsImportOther
		{
			get
			{
				return GetItem("CustomsImportOther", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsImportOther",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
						ResString.GetMultilingualString("92FA422E-13D7-4E46-9192-B5F879F1AADB", "Customs Import Other"),
						ResString.GetMultilingualString("1A9A4F17-9441-4CBA-8CE5-B8299FFD8233", "The default Department to use when creating a new billing job on an Import Declaration."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ClearanceImportOther);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsOther
		{
			get
			{
				return GetItem("CustomsOther", delegate
				{
					var result = new GuidRegistryItem("CustomsOther",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
						ResString.GetMultilingualString("e634eb69-35e7-4bb2-8cf9-d489c53086a0", "Customs Other"),
						ResString.GetMultilingualString("d78195dc-51f7-45ce-9fc6-0fbbcab443cc", "The default Department to use when creating a new billing job."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryConstants.DepartmentPKs.ClearanceOther);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CustomsExWarehouse
		{
			get
			{
				return GetItem("CustomsExWarehouse", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomsExWarehouse",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
																	 ResString.GetMultilingualString("d787994d-57b3-4a5b-ba2c-2c1429829967", "Customs Ex Warehouse"),
																	 ResString.GetMultilingualString("d78195dc-51f7-45ce-9fc6-0fbbcab443cc", "The default Department to use when creating a new billing job."),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.ClearanceExBond);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public BooleanRegistryItem CustomsDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem("CustomsDefaultToCurrentLoginDept", delegate
				{
					return new BooleanRegistryItem("CustomsDefaultToCurrentLoginDept",
													 Categories.Accounting_JobInvoicing_DefaultDepartments_Customs,
													 ResString.GetMultilingualString("8e3d7074-ff04-4f80-9871-cc6c94dabe77", "Default to Current Login Dept."),
													 ResString.GetMultilingualString("b1532730-5adc-47a6-b46e-33d10ea43c65", "When the value is set to Yes, the Job department will be defaulted to the department selected during login when creating new Custom job. Otherwise, the system will default department as setup for each operation Activity, Direction and Mode."),
													 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
													 false);
				});
			}
		}

		public GuidRegistryItem CfsPackSea
		{
			get
			{
				return GetItem("CfsPackSea", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CfsPackSea",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	 ResString.GetMultilingualString("1acd6d55-11f8-4f5d-bbd2-16d947ff186c", "CFS Pack Sea"),
																	 ResString.GetMultilingualString("341aae47-6a57-40d5-b5be-2bc3a75c65b3", @"When creating an Accounting Job Header on a CFS Load List/CFS Shipment Export job having 'Sea' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.CfsPackSea);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CfsPackAir
		{
			get
			{
				return GetItem("CfsPackAir", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CfsPackAir",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	ResString.GetMultilingualString("3d589f3a-afd2-4988-83d1-63d761369fe7", "CFS Pack Air"),
																	ResString.GetMultilingualString("fd7b556e-34c1-4f6f-b51c-9eae14a83344", @"When creating an Accounting Job Header on a CFS Load List/CFS Shipment Export job having 'Air' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.CfsPackAir);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CfsPackRail
		{
			get
			{
				return GetItem("CfsPackRail", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CfsPackRail",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	 ResString.GetMultilingualString("a5dcfb68-e375-43f7-8760-fa71f3ab3f5b", "CFS Pack Rail"),
																	 ResString.GetMultilingualString("e327a85a-3579-4d9f-ab1b-c6ce302c3392", @"When creating an Accounting Job Header on a CFS Load List/CFS Shipment Export job having 'Rail' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.CfsPackRail);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CfsPackRoad
		{
			get
			{
				return GetItem("CfsPackRoad", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CfsPackRoad",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	ResString.GetMultilingualString("d6bdb5c2-f9f3-4d11-b3d6-4d2fd0e46633", "CFS Pack Road"),
																	ResString.GetMultilingualString("279a1703-04cb-48c8-aabe-2013f69a97d5", @"When creating an Accounting Job Header on a CFS Load List/CFS Shipment Export job having 'Road' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.CfsPackRoad);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CfsUnpackSea
		{
			get
			{
				return GetItem("CfsUnpackSea", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CfsUnpackSea",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	 ResString.GetMultilingualString("e79aebc7-1516-4928-a213-053efd87943f", "CFS Unpack Sea"),
																	 ResString.GetMultilingualString("503b9a1a-18da-4ded-826f-47b1a4f71cb4", @"When creating an Accounting Job Header on a CFS Load List/CFS Shipment non-Export job having 'Sea' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.CfsUnpackSea);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CfsUnpackAir
		{
			get
			{
				return GetItem("CfsUnpackAir", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CfsUnpackAir",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	 ResString.GetMultilingualString("54e823a3-04d6-499d-b031-df8d541ac0a3", "CFS Unpack Air"),
																	 ResString.GetMultilingualString("ebc78a0a-73a0-49ee-98e0-683b3aba7595", @"When creating an Accounting Job Header on a CFS Load List/CFS Shipment non-Export job having 'Air' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.CfsUnpackAir);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CfsUnpackRail
		{
			get
			{
				return GetItem("CfsUnpackRail", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CfsUnpackRail",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	 ResString.GetMultilingualString("d99eb086-7e91-4005-9fce-351e3105afd6", "CFS Unpack Rail"),
																	 ResString.GetMultilingualString("dee44595-cbe2-4cc4-a15e-3a44c940cd8d", @"When creating an Accounting Job Header on a CFS Load List/CFS Shipment non-Export job having 'Rail' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.CfsUnpackRail);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem CfsUnpackRoad
		{
			get
			{
				return GetItem("CfsUnpackRoad", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CfsUnpackRoad",
																	 Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	 ResString.GetMultilingualString("e24f8441-b8c9-4268-a090-82f12b524202", "CFS Unpack Road"),
																	 ResString.GetMultilingualString("3caf9bc2-3629-4720-be90-0fcd32bb4880", @"When creating an Accounting Job Header on a CFS Load List/CFS Shipment non-Export job having 'Road' as the Transport Mode, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	 RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	 RegistryConstants.DepartmentPKs.CfsUnpackRoad);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem AirCargo
		{
			get
			{
				return GetItem("AirCargo", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("AirCargo",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	ResString.GetMultilingualString("58826c84-307c-42f6-8145-f2c2f62f5345", "Air Cargo"),
																	ResString.GetMultilingualString("712ecec5-1b8b-4773-857e-9f11d59ac7a7", @"When creating an Accounting Job Header on a CFS/CTO job, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled", BrandingFactory.Instance.ProductName),
							RegistryStorageFlags.Company | RegistryStorageFlags.System, RegistryConstants.DepartmentPKs.CfsUnpackAir);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem AirCargoOutturns
		{
			get
			{
				return GetItem("AirCargoOutturns", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("AirCargoOutturns",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	ResString.GetMultilingualString("96d2dc4b-0645-4c0a-a274-6c09913ab776", "Air Cargo Outturns"),
																	ResString.GetMultilingualString("d55b930c-7fef-4ce1-9fc7-1a32c0d56219", @"When creating an Accounting Job Header on a CFS/CTO job, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.CfsUnpackAir);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public GuidRegistryItem AirCargoCTO
		{
			get
			{
				return GetItem("AirCargoCTO", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("AirCargoCTO",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_CFS,
																	ResString.GetMultilingualString("bef6e460-f90b-4f78-ac06-a1ba8e512392", "Air Cargo CTO"),
																	ResString.GetMultilingualString("712ecec5-1b8b-4773-857e-9f11d59ac7a7", @"When creating an Accounting Job Header on a CFS/CTO job, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.CfsUnpackAir);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public BooleanRegistryItem CfsDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem("CfsDefaultToCurrentLoginDept", delegate
				{
					return new BooleanRegistryItem(
							"CfsDefaultToCurrentLoginDept", Categories.Accounting_JobInvoicing_DefaultDepartments_CFS, ResString.GetMultilingualString("8e3d7074-ff04-4f80-9871-cc6c94dabe77", "Default to Current Login Dept."), ResString.GetMultilingualString("9ce8660f-100f-4445-b58b-049d2756a341", "When the value is set to Yes, the Job department will be defaulted to the department selected during login when creating new CFS job. Otherwise, the system will default department as setup for each operation Activity, Direction and Mode."),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		public GuidRegistryItem WarehouseDefaultDept
		{
			get
			{
				return GetItem((NoResString)"Warehouse", delegate
				{
					Guid warehouseDepartment = new Guid("8EEB6773-D15C-47B2-AD51-2F499919A420"); //	Warehouse Free Store
					GuidRegistryItem result = new GuidRegistryItem((NoResString)"Warehouse",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_Warehouse,
																	ResString.GetMultilingualString("C20128DA-C41B-4464-9B3A-7C6451469A82", "Warehouse"),
																	ResString.GetMultilingualString("59d66235-830f-474b-ab27-41934e1df099", @"When creating an Accounting Job Header on a Warehouse Receive/Warehouse Release/Warehouse Periodic/Warehouse Stocktake and Warehouse Ad Hoc Service Job, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	warehouseDepartment);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public BooleanRegistryItem WarehouseDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem("WarehouseDefaultToCurrentLoginDept", delegate
				{
					return new BooleanRegistryItem(
							"WarehouseDefaultToCurrentLoginDept", Categories.Accounting_JobInvoicing_DefaultDepartments_Warehouse, ResString.GetMultilingualString("8e3d7074-ff04-4f80-9871-cc6c94dabe77", "Default to Current Login Dept."), ResString.GetMultilingualString("872dd24e-017b-4935-a690-9dbe81b4f06b", "When the value is set to Yes, the Job department will be defaulted to the department selected during login when creating new Warehouse job. Otherwise, the system will default department as setup for each operation Activity, Direction and Mode."),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch, false);
				});
			}
		}

		public GuidRegistryItem ProjectDefaultDept
		{
			get
			{
				return GetItem("ProjectDefaultDept", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("ProjectDefaultDept",
						Categories.Accounting_JobInvoicing_DefaultDepartments_Project,
																	ResString.GetMultilingualString("1095f37f-61ab-42b1-910a-dfbbae50c72e", "Project"),
																	ResString.GetMultilingualString("b4636850-afe9-4212-bb89-a51cec636080", @"When creating an Accounting Job Header on a Project job, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public BooleanRegistryItem ProjectDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem("ProjectDefaultToCurrentLoginDept", delegate
				{
					return new BooleanRegistryItem(
							"ProjectDefaultToCurrentLoginDept",
							Categories.Accounting_JobInvoicing_DefaultDepartments_Project,
							ResString.GetMultilingualString("DD59BF64-9E69-471C-BA8E-208EDFA8C617", "Default to Current Login Dept."),
							ResString.GetMultilingualString("847DFB19-21B6-4033-A9EF-0C2486E818C4", "When the value is set to Yes, the Job department will be defaulted to the department selected during login when creating new Project job. Otherwise, the system will default department as setup for Projects."),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch, true);
				});
			}
		}

		public GuidRegistryItem WorkitemDefaultDept
		{
			get
			{
				return GetItem("WorkitemDefaultDept", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("WorkitemDefaultDept",
						Categories.Accounting_JobInvoicing_DefaultDepartments_WorkItem,
																	ResString.GetMultilingualString("6359CFDC-B470-40F0-99C7-754129BC44EE", "Work Item"),
																	ResString.GetMultilingualString("d442def6-deb2-4ea1-b057-995d71ad3eed", @"When creating an Accounting Job Header on a Work Item, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public BooleanRegistryItem WorkItemDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem("WorkItemDefaultToCurrentLoginDept", delegate
				{
					return new BooleanRegistryItem(
							"WorkItemDefaultToCurrentLoginDept",
							Categories.Accounting_JobInvoicing_DefaultDepartments_WorkItem,
							ResString.GetMultilingualString("8e3d7074-ff04-4f80-9871-cc6c94dabe77", "Default to Current Login Dept."),
							ResString.GetMultilingualString("1F95110B-AF94-42B9-8F28-836D27D6ADFA", "When the value is set to Yes, the Job department will be defaulted to the department selected during login when creating new Work Item job. Otherwise, the system will default department as setup for Work Items."),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch, true);
				});
			}
		}

		public GuidRegistryItem CustomerServiceTicketDefaultDept
		{
			get
			{
				return GetItem("CustomerServiceTicketDefaultDept", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("CustomerServiceTicketDefaultDept",
						Categories.Accounting_JobInvoicing_DefaultDepartments_CustomerServiceTicket,
						ResString.GetMultilingualString("89BCBEC2-0284-4593-98B1-3E257CF4800E", "Customer Service Ticket"),
						ResString.GetMultilingualString("C45B606F-58F5-4F9C-8E35-B7272C7B6F4D", @"When creating an Accounting Job Header on a Customer Service Ticket, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public BooleanRegistryItem CustomerServiceTicketDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem("CustomerServiceTicketDefaultToCurrentLoginDept", delegate
				{
					return new BooleanRegistryItem(
							"CustomerServiceTicketDefaultToCurrentLoginDept",
							Categories.Accounting_JobInvoicing_DefaultDepartments_CustomerServiceTicket,
							ResString.GetMultilingualString("567E2970-72AB-4B9B-84FE-B6F43BB6F271", "Default to Current Login Dept."),
							ResString.GetMultilingualString("F503981-71C6-4911-AAB1-03F92632AD9A", "When the value is set to Yes, the Job department will be defaulted to the department selected during login when creating a new Customer Service Ticket job. Otherwise, the system will default department as setup for Customer Service Ticket."),
							RegistryStorageFlags.Company | RegistryStorageFlags.Branch, true);
				});
			}
		}

		public GuidRegistryItem TransportBookingJobsDefaultDept
		{
			get
			{
				return GetItem("TransportBookingJobsDefaultDept", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("TransportBookingJobsDefaultDept",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_TransportBooking,
																	ResString.GetMultilingualString("2E0422A3-ACEA-4337-B393-100242DE7B37", "Transport Booking Jobs"),
																	ResString.GetMultilingualString("C88A269E-9599-4027-BAAE-17793CB2CF84", @"When creating an Accounting Job Header on standalone Transport Booking Jobs, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.TransportBookingDefaultDepartment);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public BooleanRegistryItem TransportBookingDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem(nameof(TransportBookingDefaultToCurrentLoginDept), delegate
				{
					return new BooleanRegistryItem(nameof(TransportBookingDefaultToCurrentLoginDept),
													Categories.Accounting_JobInvoicing_DefaultDepartments_TransportBooking,
													ResString.GetMultilingualString("F627C761-7AA4-408D-85DC-8582DAFEF9A8", "Default to Current Login Dept."),
													ResString.GetMultilingualString("75AD9116-DBEE-496D-9841-AAB458FC255F", "When the value is set to Yes, Transport Booking’s Job Department defaults to the department the current user is currently logged in. Otherwise, the system will default department as setup for each operation Activity, Direction and Consol Type."),
													RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
													false);
				});
			}
		}

		public GuidRegistryItem LandTransportJobsDefaultDept
		{
			get
			{
				return GetItem("LandTransportJobsDefaultDept", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("LandTransportJobsDefaultDept",
																	Categories.Accounting_JobInvoicing_DefaultDepartments_LandTransport,
																	ResString.GetMultilingualString("AC2F402E-6604-4E67-9B6A-61D62E63F04E", "Land Transport Jobs"),
																	ResString.GetMultilingualString("38BD7C3D-C120-4557-A857-EADA36426DAF", @"When creating an Accounting Job Header on standalone Land Transport Jobs, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
																	RegistryStorageFlags.Company | RegistryStorageFlags.System,
																	RegistryConstants.DepartmentPKs.TransportBookingDefaultDepartment);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public BooleanRegistryItem LandTransportDefaultToCurrentLoginDept
		{
			get
			{
				return GetItem(nameof(LandTransportDefaultToCurrentLoginDept), delegate
				{
					return new BooleanRegistryItem(nameof(LandTransportDefaultToCurrentLoginDept),
													Categories.Accounting_JobInvoicing_DefaultDepartments_LandTransport,
													ResString.GetMultilingualString("67C53DF5-58FF-416B-BDDE-60A30BB21058", "Default to Current Login Dept."),
													ResString.GetMultilingualString("3FDBF9B8-63E3-444E-A8C1-DAE41BA72760", "When the value is set to Yes, Land Transport’s Job Department defaults to the department the current user is currently logged in. Otherwise, the system will default department as setup in the 'Land Transport Jobs' registry"),
													RegistryStorageFlags.Company | RegistryStorageFlags.System,
													true);
				});
			}
		}

		public BooleanRegistryItem JobInvoicingCFXEnabled
		{
			get
			{
				return GetItem("AccEnableCFX", delegate
				{
					MultilingualString jobInvoicingCFXEnabledHint = ResString.GetMultilingualString("5f0658e5-6fb7-49f6-b120-97fd8813d07c", "This registry automates the removal of any CFX uplift revenue from Job Profit.\r\nWhen enabled (flagged 'Yes') all CFX uplift revenue will be posted to the CFX reserve account.\r\nWhen not enabled (flagged 'No') all CFX uplift revenue will be posted as revenue against the relevant charge code and remain included in the Job Profit.");
					return new BooleanRegistryItem("AccEnableCFX", Categories.Accounting_JobInvoicing, ResString.GetMultilingualString("578cbe8b-b51d-46ab-af7b-f359e77f0366", "Enable CFX"), jobInvoicingCFXEnabledHint, RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
				});
			}
		}

		public IntRegistryItem JobLockedRetryIntervalInMinutes
		{
			get
			{
				return GetItem("JobLockedRetryIntervalInMinutes", delegate
				{
					return new IntRegistryItem(
						"JobLockedRetryIntervalInMinutes",
						Categories.Accounting_JobInvoicing,
						(NoResString)"Job Locked Retry Interval In Minutes",
						(NoResString)"Job Locked Retry Interval In Minutes.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						5, 1, 10);
				});
			}
		}

		public BooleanRegistryItem CreateWIPWhenCostIsGreaterThanAccrual
		{
			get
			{
				return GetItem("CreateWIPWhenCostIsGreaterThanAccrual", delegate
				{
					return new BooleanRegistryItem("CreateWIPWhenCostIsGreaterThanAccrual", Categories.Accounting_JobCostingDefaults, ResString.GetMultilingualString("217ea85a-b60f-4a67-ab61-7029b9f6f859", "Create WIP Where CST > ACR"), ResString.GetMultilingualString("ffe94f1c-8869-4018-b3a3-642af64e5231", "Create WIP When Cost Is Greater Than Accrual"), RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
				});
			}
		}

		public BooleanRegistryItem CreateWIPWhenCostIsFinal
		{
			get
			{
				return GetItem("CreateWIPWhenCostIsFinal", delegate
				{
					return new BooleanRegistryItem("CreateWIPWhenCostIsFinal", Categories.Accounting_JobCostingDefaults, ResString.GetMultilingualString("fcdd65bc-1396-4fd9-9602-852020db48b4", "Create WIP Where Cost Is Final"), ResString.GetMultilingualString("fcdd65bc-1396-4fd9-9602-852020db48b4", "Create WIP Where Cost Is Final"), RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
				});
			}
		}

		public BooleanRegistryItem RestrictPostingOfSellChargesVisibleToLoginUserOnly
		{
			get
			{
				var item = GetItem("RestrictPostingOfSellChargesVisibleToLoginUserOnly", delegate
				{
					return new BooleanRegistryItem(
						"RestrictPostingOfSellChargesVisibleToLoginUserOnly",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("1655D89E-3185-4D64-A59F-FAC6EE854643", "Restrict Posting Of Sell Charges Visible To Login User Only"),
						ResString.GetMultilingualString("94B67D16-37CC-4E62-A958-9B080A1C1CC9", @"When this registry is set to 'Yes', CargoWise will restrict posting of SELL charges visible to the login user only.

Note: Users can be restricted from viewing charges outside their branch/department login permission.
For more information, kindly refer to update note on 'Hide Financial outside Login Branch/Department on Billing Jobs'."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System, false);
				});

				item.OnBuildLogReference += (args) => Res.GetString("1C7C0F23-AA01-4A81-B065-5D3769071799", "Registry value set to [{0}].", args.NewValue);

				return item;
			}
		}

		public BooleanRegistryItem CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency
		{
			get
			{
				return GetItem("CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency", delegate
				{
					return new BooleanRegistryItem("CarryForwardAccrualBasedOnOSAmountWhereCSTCurrencyEqualsACRCurrency",
						Categories.Accounting_JobCostingDefaults,
						ResString.GetMultilingualString("4aa830c1-949b-4bb6-ae50-689e9daaf44a", "Carry Forward Accrual Based On OS Amount Where CST Currency = ACR Currency"),
						ResString.GetMultilingualString("fa0626d8-c320-45f3-8383-11eaf3037465", @"By default this registry is set to 'No'.
When this registry is set to 'Yes', the following system behavior will be applied.

The system will carry forward accrual based on (Accrual's OS Amount minus Cost's OS Amount ) x Cost's Exchange Rate on the following conditions:
1. The Accrual is imported. 
2. The Cost and Accrual Currency (Billing Tab > OS Cost Currency) is the same.
3. The OS Cost Amount < OS Accrual Amount
4. The Final flag is not ticked. 

The system will create WIP based on (Cost's OS Amount minus Accrual's OS Amount ) x Job Exchange Rate on the following conditions:
1. The Accrual is imported. 
2. The Cost and Accrual Currency (Billing Tab > OS Cost Currency) is the same.
3. The OS Cost Amount > OS Accrual Amount
4. The 'Create WIP Where CST > ACR' system registry is set to 'Yes' and Final flag is not ticked
     OR the 'Create WIP Where Cost Is Final' system registry is set to 'Yes' and Final flag is ticked.

If any one of the conditions is not fulfilled, the current logic will apply. 
Based on current logic, the accrual will be carried forward in foreign currency when the  currency and exchange rate of the cost and accrual matches.
Otherwise, the accrual will be carried forward in local currency."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem BringForwardAgainstCreditor
		{
			get
			{
				return GetItem("BringForwardAgainstCreditor", delegate
				{
					return new BooleanRegistryItem("BringForwardAgainstCreditor", Categories.Accounting_JobCostingDefaults,
						ResString.GetMultilingualString("3604a58f-450a-4d2e-9c3a-f50d198b6429", "Accrual Reversal Behavior When Allocated To Creditor"),
						ResString.GetMultilingualString("61ea8962-ee45-4398-802b-09c79b79deba", @"By default this registry is set to ‘Yes’.
When overridden and set to ‘No’, the Creditor code allocated against an Accrual is always ignored when posting an AP Invoice.
Posting an AP Invoice will reverse all outstanding Accruals for the same charge code + branch + department, irrespective of the Accrual’s allocated Creditor.
Any amounts re-accrued will be re-accrued without a Creditor code.

When set to ‘Yes’, the Creditor recorded against each Accrual will be respected.
In addition:
- Accruals attributed to ANOTHER Creditor will be ignored.  Accruals for another creditor will NOT be reversed.
- Only Accruals for the relevant Creditor or Accruals with no Creditor will be considered for reversal.
- Accruals for the AP Invoice Creditor will be reversed first, then, when those accruals have been exhausted, accruals with no Creditor for the same charge code + branch + department will be used.

Note: This registry must be set to 'Yes' in order to allow multiple unposted cost lines for a given charge code on Consol Cost.
The above accrual reversal rules apply. Any re-accrue will be done on the shipment level (where applicable)."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company, true);
				});
			}
		}

		public BooleanRegistryItem PreventOperatorFromChangingRatedLine
		{
			get
			{
				return GetItem("PreventOperatorFromChangingRatedLine", delegate
				{
					return new BooleanRegistryItem("PreventOperatorFromChangingRatedLine",
													Categories.Accounting_JobInvoicing,
													ResString.GetMultilingualString("feaccfcf-85db-4a84-abb9-2e38e832c2ce", "Prevent Operator From Changing Rated Line"),
													ResString.GetMultilingualString("b78ddb64-c697-4295-b3dd-0f10a533d714", @"This registry setting allows the restriction of users from editing Autorated Charges within the billing job.When enabled autorated charges will be un-editable"),
													RegistryStorageFlags.System | RegistryStorageFlags.Company,
													false);
				});
			}
		}

		public BooleanRegistryItem PrintBranchAddressInFooter
		{
			get
			{
				return GetItem("PrintBranchAddressInFooter", delegate
				{
					return new BooleanRegistryItem("PrintBranchAddressInFooter", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("2032c5e2-af86-4b36-a594-10756de7240e", "Print Branch Address In Footer"), ResString.GetMultilingualString("f6249782-32a1-4c37-8aa4-73e968a337ef", "Print current branch address in the Invoice footer, otherwise print the current company address."), RegistryStorageFlags.Branch, false);
				});
			}
		}

		public BooleanRegistryItem UseJobNumberBasedInvoiceNumbers
		{
			get
			{
				return GetItem("UseJobNumberBasedInvoiceNumbers", delegate
				{
					return new BooleanRegistryItem("UseJobNumberBasedInvoiceNumbers", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("0f81ab7c-582d-4ae4-9fa7-b06922e12877", "Use Job Number Based Invoice Numbers"), ResString.GetMultilingualString("4def1f9f-c637-4926-9860-8d6818570142", "Should invoices be printed using the Job number as the main piece of information rather than the invoice number"), RegistryStorageFlags.Company | RegistryStorageFlags.System, true);
				});
			}
		}

		public GuidRegistryItem UseThisBranchLetterheadOnARInvoice
		{
			get
			{
				return GetItem("UseThisBranchLetterheadOnARInvoice", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("UseThisBranchLetterheadOnARInvoice", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("bb9a07ba-ea52-4430-8561-44dcc34087c2", "Use This Branch's Logo on AR Invoice"), ResString.GetMultilingualString("bb9a07ba-ea52-4430-8561-44dcc34087c2", "Use This Branch's Logo on AR Invoice"), RegistryStorageFlags.Branch);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbBranch);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.China;
					return result;
				});
			}
		}

		public BooleanRegistryItem DisplayTaxRegistrationNumber
		{
			get
			{
				return GetItem("DisplayTaxRegistrationNumber", delegate
				{
					return new BooleanRegistryItem("DisplayTaxRegistrationNumber", Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice, ResString.GetMultilingualString("d7dd8984-f2a5-4bb2-a745-b4faff30016d", "Display Tax Registration Number"), ResString.GetMultilingualString("e139612a-e24f-4f58-8b75-a8c6102a75a4", @"This registry is relevant to login companies enabled for GST/VAT. 
When set to ‘Yes’ the VAT/GST tax registration details of the login company will print in the Invoice document.  The GST/VAT registration number will be drawn from the tax registration number recorded against the login company’s license key. 
A login company would set this registry to ‘No’ when their VAT registration details have been included in their letterhead graphic."), RegistryStorageFlags.Company | RegistryStorageFlags.System, true);
				});
			}
		}

		string TheTaxCode
		{
			get
			{
				ICompany currentCompany = EnvProxy.Instance.CurrentCompany;
				return (currentCompany == null) ? string.Empty : currentCompany.Country.ConsumptionTaxDescription;
			}
		}

		#region AccNextNumbers

		#region SuppressResourceStringsCheckRegion

		public AccNextNoRegistryItem AccNextInvoice
		{
			get
			{
				return GetItem("AccNextInvoice", delegate
				{
					return new AccNextNoRegistryItem("AccNextInvoice", Categories.Accounting_AccountingNextNumbers_ReceivableNumbers, (NoResString)"Next Invoice");
				});
			}
		}

		public AccNextNoRegistryItem AccNextCreditNote
		{
			get
			{
				return GetItem("AccNextCreditNote", delegate
				{
					return new AccNextNoRegistryItem("AccNextCreditNote", Categories.Accounting_AccountingNextNumbers_ReceivableNumbers, (NoResString)"Next Credit Note");
				});
			}
		}

		public AccNextNoRegistryItem AccNextAdjNote
		{
			get
			{
				return GetItem("AccNextAdjNote", delegate
				{
					return new AccNextNoRegistryItem("AccNextAdjNote", Categories.Accounting_AccountingNextNumbers_ReceivableNumbers, (NoResString)"Next Adjustment Note");
				});
			}
		}

		public AccNextNoRegistryItem AccNextJournal
		{
			get
			{
				return GetItem("AccNextJournal", delegate
				{
					return new AccNextNoRegistryItem("AccNextJournal", Categories.Accounting_AccountingNextNumbers_ReceivableNumbers, (NoResString)"Next Journal");
				});
			}
		}

		public AccNextNoRegistryItem AccNextTransfer
		{
			get
			{
				return GetItem("AccNextTransfer", delegate
				{
					return new AccNextNoRegistryItem("AccNextTransfer", Categories.Accounting_AccountingNextNumbers_ReceivableNumbers, (NoResString)"Next Transfer");
				});
			}
		}

		public AccNextNoRegistryItem AccNextContra
		{
			get
			{
				return GetItem("AccNextContra", delegate
				{
					return new AccNextNoRegistryItem("AccNextContra", Categories.Accounting_AccountingNextNumbers_ReceivableandPayable, (NoResString)"Next Contra");
				});
			}
		}

		public AccNextNoRegistryItem AccNextReceipt
		{
			get
			{
				return GetItem("AccNextReceipt", delegate
				{
					return new AccNextNoRegistryItem("AccNextReceipt", Categories.Accounting_AccountingNextNumbers_ReceivableandPayable, (NoResString)"Next Receipt");
				});
			}
		}

		public AccNextNoRegistryItem AccNextPayment
		{
			get
			{
				return GetItem("AccNextPayment", delegate
				{
					return new AccNextNoRegistryItem("AccNextPayment", Categories.Accounting_AccountingNextNumbers_ReceivableandPayable, (NoResString)"Next Payment");
				});
			}
		}

		public AccNextNoRegistryItem AccNextOverpayment
		{
			get
			{
				return GetItem("AccNextOverpayment", delegate
				{
					return new AccNextNoRegistryItem("AccNextOverpayment", Categories.Accounting_AccountingNextNumbers_ReceivableandPayable, (NoResString)"Next Overpayment");
				});
			}
		}

		public AccNextNoRegistryItem AccNextDiscount
		{
			get
			{
				return GetItem("AccNextDiscount", delegate
				{
					return new AccNextNoRegistryItem("AccNextDiscount", Categories.Accounting_AccountingNextNumbers_ReceivableandPayable, (NoResString)"Next Discount");
				});
			}
		}

		public AccNextNoRegistryItem AccNextExchangeDiff
		{
			get
			{
				return GetItem("AccNextExchangeDiff", delegate
				{
					return new AccNextNoRegistryItem("AccNextExchangeDiff", Categories.Accounting_AccountingNextNumbers_ReceivableandPayable, (NoResString)"Next Exchange Difference");
				});
			}
		}

		public AccNextNoRegistryItem AccNextBankBatch
		{
			get
			{
				return GetItem("AccNextBankBatch", delegate
				{
					return new AccNextNoRegistryItem("AccNextBankBatch", Categories.Accounting_AccountingNextNumbers_CashBookNumbers, (NoResString)"Next Bank Batch");
				});
			}
		}

		public AccNextNoRegistryItem AccNextDDRBatchNo
		{
			get
			{
				return GetItem("AccNextDDRBatchNo", delegate
				{
					return new AccNextNoRegistryItem("AccNextDDRBatchNo", Categories.Accounting_AccountingNextNumbers_CashBookNumbers, (NoResString)"Next DDR Batch");
				});
			}
		}

		public class AccNextNoRegistryItem : RegistryItemImpl
		{
			public AccNextNoRegistryItem(string name, MultilingualString category, MultilingualString caption)
				: base(name, category, caption, caption, RegistryDataTypes.IntType, RegistryStorageFlags.Company)
			{
				Options = Options | RegistryOptions.IsHidden; // Remove this line when this registry item is enabled
			}

			// This functionality will be enabled at a later stage.
			public override bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				return false;
			}
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public IntRegistryItem AccGovtComplianceNumberLength
		{
			get
			{
				return GetItem("AccGovtComplianceNumberLength", delegate
				{
					return new IntRegistryItem("AccGovtComplianceNumberLength", Categories.Accounting_AccountingNextNumbers_GovtTaxInvoiceReference, (NoResString)"Government Compliance Number Length (CargoWiseOne Support Only)", (NoResString)"When defined, this registry will be used to pad out the Government Compliance Number with leading zero's to make the 'number' the correct length.  This length excludes the Serial Number.", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, 8);
				});
			}
		}

		public CodePairRegistryItem AccGovtComplianceDocumentExchangeRateType
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() =>
				{
					CodeDescriptionPairList list = new CodeDescriptionPairList();
					list.AddPair("SEL", "Sell Rate");
					list.AddPair("BUY", "Buy Rate");
					list.AddPair("CUS", "Customs Rate");
					list.AddPair("PER", "Period End Rate");
					return list;
				});

				return GetItem("AccGovtComplianceDocumentExchangeRateType", delegate
				{
					return new CodePairRegistryItem(
						"AccGovtComplianceDocumentExchangeRateType",
						Categories.Accounting_AccountingNextNumbers_GovtTaxInvoiceReference,
						(NoResString)"Compliance Document Exchange Rate Type (CargoWiseOne Support Only)",
						(NoResString)@"This registry is used when the Login Company's local currency is NOT the local currency of Login Country AND a Government Compliance Document must be printed in the country's local currency.
This registry defines which exchange rate type will be used to convert transaction values to the mandated Compliance Document's currency.
This registry will only be required when neither OS Currency or Local Currency on the transaction is the Country's local currency.",
						listProvider,
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						"SEL");
				});
			}
		}

		public CodePairRegistryItem AccGovtComplianceDocumentExchangeRateDate
		{
			get
			{
				var listProvider = new CodeDescriptionPairListProvider(() =>
				{
					var list = new CodeDescriptionPairList();
					list.AddPair("PST", "Post Date");
					list.AddPair("INV", "Invoice Date");
					return list;
				});

				return GetItem("AccGovtComplianceDocumentExchangeRateDate", delegate
				{
					return new CodePairRegistryItem(
						"AccGovtComplianceDocumentExchangeRateDate",
						Categories.Accounting_AccountingNextNumbers_GovtTaxInvoiceReference,
						(NoResString)"Compliance Document Exchange Rate Date (CargoWiseOne Support Only)",
						(NoResString)@"This registry is used when the Login Company's local currency is NOT the local currency of Login Country AND a Government Compliance Document must be printed in the country's local currency.
This registry defines which date on a transaction will be used when selecting the exchange rate when converting transaction values to the mandated Compliance Document's currency.
This registry will only be required when neither OS Currency or Local Currency on the transaction is the Country's local currency.",
						listProvider,
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						"PST");
				});
			}
		}

		#endregion

		#endregion

		public BooleanRegistryItem EnableGlobalChargesDetail
		{
			get
			{
				return GetItem("EnableGlobalChargesDetail", delegate
				{
					return new BooleanRegistryItem("EnableGlobalChargesDetail", Categories.Accounting_JobCostingDefaults, ResString.GetMultilingualString("53573a83-f056-4b5f-bbf6-6a345ec5e177", "Enable Global Charges Detail"), ResString.GetMultilingualString("cda09e80-37ac-4280-bd79-10122258c65f", @"This registry enables the viewing of a 'Global Job Costing' tab when viewing a job's Profit/Loss.
When this registry is overridden and set to 'Yes' the Global Job Costing tab is enabled. The additional tab will be visible when viewing a job's Profit and Loss tabs through the Job Management module and through the Job Billing screens.
By default this tab is not enabled.  Only the login company specific Profit and Loss Summary and Detail tabs are visible.
When overridden, the third 'Global Job Costing' tab becomes visible in the Job Profit and Loss screens."), RegistryStorageFlags.System | RegistryStorageFlags.Company, false);
				});
			}
		}

		public GuidRegistryItem PeriodReopenNotifyGroup
		{
			get
			{
				return GetItem("PeriodReopenNotifyGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"PeriodReopenNotifyGroup",
						Categories.Accounting_EmailNotification,
						ResString.GetMultilingualString("6C46DB46-371D-48aa-AABE-3AB20E9F8FB8", "Period Reopen Notify Group"),
						ResString.GetMultilingualString("2527B4D1-93A0-4f76-89AB-4E9B7FCC46C9", "Notify Party when Closed Period is re-opened"),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem CollectConstructorCallStackDetailsToReportInCriticalValidationErrors
		{
			get
			{
				return GetItem("CollectConstructorCallStackDetailsToReportInCriticalValidationErrors", delegate
				{
					return new BooleanRegistryItem(
						"CollectConstructorCallStackDetailsToReportInCriticalValidationErrors",
						Categories.Accounting_CriticalValidation,
						(NoResString)"Collect Constructor Call Stack Details",
						(NoResString)"This Registry Item enables collecting constructor Call Stack information for some accounting business objects to be reported in the critical validation errors. It is set to No by default. Set it to Yes only when investigating the critical validation reported issues.",
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForDevelopers, false);
				});
			}
		}

		#endregion

		#region Web

		public BooleanRegistryItem TransactionPaymentStatusWebServiceUsesSettlementGroup
		{
			get
			{
				return GetItem("TransactionPaymentStatusWebServiceUsesSettlementGroup", delegate
				{
					return new BooleanRegistryItem(
						 "TransactionPaymentStatusWebServiceUsesSettlementGroup",
						 Categories.Accounting_Web,
						 ResString.GetMultilingualString("78859b55-736b-4ba5-a8c8-c218d3b7d051", "Transaction Payment Status and Invoice Payment Services use Settlement Group"),
						 ResString.GetMultilingualString("633aa34e-4016-47c3-9b0c-459742847dd2", "This setting defines whether Transaction Payment Status and Invoice Payment Web Services use organization Settlement Group when looking for the specified transaction number for given organization.\r\nSet 'Yes' to allow using Settlement Groups."),
						 RegistryStorageFlags.System,
						 false);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem ShowInvoicePaymentWebServiceRegistryItem
		{
			get
			{
				return GetItem("ShowInvoicePaymentWebServiceRegistryItem", delegate
				{
					return new BooleanRegistryItem(
						 "ShowInvoicePaymentWebServiceRegistryItem",
						 Categories.Accounting_Web,
						 (NoResString)"Show Invoice Payment Web Service Registry Item",
						 (NoResString)"This CargoWiseOne Support Only accessible setting defines visibility of the 'Enable Invoice Payment Web Service' registry item (default - No).",
						 RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport,
						 false);
				});
			}
		}

		public DateTimeRegistryItem AccountingTransactionExportServiceHighWaterMark
		{
			get
			{
				return GetItem("AccountingTransactionExportServiceHighWaterMark", delegate
				{
					var result = new DateTimeRegistryItem(
						"AccountingTransactionExportServiceHighWaterMark",
						Categories.Accounting_Web,
						(NoResString)"Accounting Transaction Export Service High Water Mark",
						(NoResString)"To aid performance of the Accounting Transaction Export Service, the system will only search for un-batched transactions that were posted after this date.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long), RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, DateTime.MinValue, false);
					result.DataType = new AccountingTransactionExportHighWaterMarkDataType();
					return result;
				});
			}
		}

		public DateTimeRegistryItem ExportTransactionBatchStartPostDate
		{
			get
			{
				return GetItem("ExportTransactionBatchStartPostDate", delegate
				{
					var result = new DateTimeRegistryItem(
						"ExportTransactionBatchStartPostDate",
						Categories.Accounting_Web,
						(NoResString)"Export transaction batch start postdate",
						(NoResString)@"When this registry value is set, only transactions posted/reversed on this or after this date will be exported into batch.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport);
					result.DataType = new DateTimeTypeRequireOtherRegistryItemNonDefaultValueToOverrideOwnValue(AccountingTransactionExportServiceHighWaterMark);
					return result;
				});
			}
		}

		public BooleanRegistryItem EnableInvoicePaymentWebService
		{
			get
			{
				return GetItem("EnableInvoicePaymentWebService", delegate
				{
					return new BooleanRegistryItem(
						 "EnableInvoicePaymentWebService",
						 Categories.Accounting_Web,
						 (NoResString)"Enable Invoice Payment Web Service",
						 (NoResString)"This setting defines whether Invoice Payment Web Service is enabled (default - No). This web service implements paying the AR/AP Invoices and Credit Notes without creating and matching a Payment transaction.\r\nSet 'Yes' to enable Invoice Payment Web Service.",
						 RegistryStorageFlags.Company, ShowInvoicePaymentWebServiceRegistryItem.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						 false);
				});
			}
		}

		#endregion

		public StringRegistryItem AccountingWebServiceUserName
		{
			get
			{
				return GetItem("AccountingWebServiceUserName", delegate
				{
					return new StringRegistryItem("AccountingWebServiceUserName",
						 Categories.Accounting_Web,
						 ResString.GetMultilingualString("e2030162-fd62-4058-93c6-d2221b5d18e6", "Accounting Web Service User Name"),
						 ResString.GetMultilingualString("d31ed69c-e0a6-4e0f-a7f6-510758e5540e", "This setting defines User Name to allow access to the Accounting Web Service.\r\nBoth Accounting Web Service User Name and Password should be specified."),
						 RegistryStorageFlags.System,
						 RegistryOptions.PreserveTestValue,
						 string.Empty);
				});
			}
		}

		public StringRegistryItem AccountingWebServicePassword
		{
			get
			{
				return GetItem("AccountingWebServicePassword", delegate
				{
					return new StringRegistryItem("AccountingWebServicePassword",
						 Categories.Accounting_Web,
						 ResString.GetMultilingualString("be89b2f4-5818-4801-9319-2aed06d0c27a", "Accounting Web Service Password"),
						 ResString.GetMultilingualString("a48d82e7-1591-4271-82bf-529a12c46f9d", "This setting defines password to allow access to the Accounting Web Service.\r\nBoth Accounting Web Service User Name and Password should be specified."),
						 RegistryStorageFlags.System,
						 RegistryOptions.PreserveTestValue | RegistryOptions.IsPasswordVisibleForControllerUser,
						 string.Empty)
					{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password) };
				});
			}
		}

		public StringRegistryItem AccountingWebServiceRemarks
		{
			get
			{
				return GetItem("AccountingWebServiceRemarks", delegate
				{
					var item = new StringRegistryItem("AccountingWebServiceRemarks",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_China,
						ResString.GetMultilingualString("AB610C5A-D4D2-4838-8C09-12069C240F4B", "China's Golden Tax Invoice Remark Configuration"),
						ResString.GetMultilingualString("E484C0E5-2DFA-4546-B848-326176732768",
		@"You can include free text and <macro> in the invoice remark. (i.e. MBL: <ConsolMasterBill>)

Further, you can define sections using square brackets. (i.e. [section] or [MBL: <ConsolMasterBill>]) and when the macro within the section returns empty value (i.e. no <ConsolMasterBill> value is found), then the entire section [MBL: <ConsolMasterBill>] will be excluded from the export. If you want a text to be always shown, add it outside a section. (i.e. Please pay by due date. [Section])

The available data fields for inclusion are:
1.	Consol Master Bill <ConsolMasterBill>
2.	Consol Vessel <ConsolVessel>
3.	Consol Voyage / Flight <ConsolVoyFlt>
4.	Consol Load Port <ConsolLoadPort>
5.	Consol Discharge Port <ConsolDischargePort>
6.	Consol ETD <ConsolETD>
7.	Consol ETA <ConsolETA>
8.	Shipment House Bill <ShipmentHouseBill>
9.	Job Invoice Number <JobInvNumber>
10.	Transaction Number <TransNumber>
11.	Invoice Currency <InvoiceCurrency>
12.	Invoice Amount <InvoiceAmount>
13.	Invoice Exchange Rate <InvoiceExchangeRate> 
14.	Job Operator Full Name <JobOperatorFullName>
15.	Job Operator Preferred Name <JobOperatorPreferredName>
16.	Job Sales Rep Full Name <JobSalesRepFullName>
17.	Job Sales Rep Preferred Name <JobSalesRepPreferredName>
18.	Transaction Description <TransDescription>

Note: 
a.	For Forwarding and CFS Shipment Job Invoices, all data values will be included (if entered).
b.	For Forwarding Consol and Other Job Invoices, only Job Invoice Number, Transaction Number, Foreign Currency, Amount and Exchange Rate will be included (where applicable).
c.	For Periodic and Non-Job Invoices, only Transaction Number, Foreign Currency, Amount and Exchange Rate will be included (where applicable)."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(NoResString)"[MBL: <ConsolMasterBill> ][HBL: <ShipmentHouseBill> ][Vessel: <ConsolVessel> ][Voyage/Flight: <ConsolVoyFlt> ][ETD: <ConsolETD> ][ETA: <ConsolETA> ][LoadPort: <ConsolLoadPort> ][DischargePort: <ConsolDischargePort> ][JobInvoiceNumber: <JobInvNumber>]");

					((TextRegistryEditorInfo)item.EditorInfo).EditorType = TextEditorType.Memo;

					return item;
				});
			}
		}

		#endregion

		#region Credit Limit Check

		public IntRegistryItem CreditLimitWarningThreshold
		{
			get
			{
				return GetItem("AllowCreditLimitWarningThreshold", delegate
				{
					return new IntRegistryItem(
						"AllowCreditLimitWarningThreshold",
						Categories.Accounting_CreditLimitCheck_Local,
						ResString.GetMultilingualString("188ce337-efcf-46d1-b4d9-6c5cd5544c21", "Credit Limit Warning Threshold"),
						ResString.GetMultilingualString("ef8983ba-e3b0-4d1b-b444-66f73b9efcca", "Threshold expressed as a percentage of AR Credit Limit. If the threshold is exceeded a warning email will be sent to the notification group specified in the system registry at Accounting > Email Notification > AR Control Breach Notify Group. A value of zero disables the warning."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						0, 0, 100);
				});
			}
		}

		public IntRegistryItem GlobalCreditLimitWarningThreshold
		{
			get
			{
				return GetItem("AllowGlobalCreditLimitWarningThreshold", delegate
				{
					return new IntRegistryItem(
						"AllowGlobalCreditLimitWarningThreshold",
						Categories.Accounting_CreditLimitCheck_Global,
						ResString.GetMultilingualString("14e0de1d-4994-40a4-9f6d-5b838d5563e9", "Global Credit Limit Warning Threshold"),
						ResString.GetMultilingualString("15303399-3456-4bff-9eb1-6aeacfabb483", "Threshold expressed as a percentage of AR Global Credit Limit. If the threshold is exceeded a warning email will be sent to the notification group specified in the system registry at Accounting > Email Notification > Global AR Control Breach Notify Group. A value of zero disables the warning."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						0, 0, 100);
				});
			}
		}

		public IntRegistryItem UseWebServiceTimeout
		{
			get
			{
				return GetItem("UseWebServiceTimeout", delegate
				{
					return new IntRegistryItem(
						"UseWebServiceTimeout",
						Categories.Accounting_CreditLimitCheck,
						ResString.GetMultilingualString("a322abde-46ec-4759-9f32-04559efd3dd4", "Use Web Service Timeout"),
						ResString.GetMultilingualString("c4fab54f-b932-4e6f-a468-a92aff14cb21", "Number of seconds to wait for Web Service before timing out."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						5, 1, 100);
				});
			}
		}

		public IntRegistryItem MaxTimeoutCountBeforeSuspendingWebService
		{
			get
			{
				return GetItem("MaxTimeoutCountBeforeSuspendingWebService", delegate
				{
					return new IntRegistryItem(
						"MaxTimeoutCountBeforeSuspendingWebService",
						Categories.Accounting_CreditLimitCheck,
						ResString.GetMultilingualString("78c8d04d-3a4f-4cae-b4ca-2667397607e9", "Max Web Service Timeouts before suspending"),
						ResString.GetMultilingualString("03ce7c5b-c842-4a7b-a1a0-33f161b81f5e", "Maximum number of continuous Credit Limit Web Service timeouts before suspending attempts to call it."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						5, 5, 100);
				});
			}
		}

		public IntRegistryItem WebServiceCallSuspendingPeriodInMinutes
		{
			get
			{
				return GetItem("WebServiceCallSuspendingPeriodInMinutes", delegate
				{
					return new IntRegistryItem(
						"WebServiceCallSuspendingPeriodInMinutes",
						Categories.Accounting_CreditLimitCheck,
						ResString.GetMultilingualString("d3922368-ec91-4069-aba2-79ff67f780ab", "Web Service call suspending period"),
						ResString.GetMultilingualString("a86aec95-76a6-4ca7-9a68-3c186d2001fc", "Credit Limit Web Service call suspending period in minutes. Suspending will be applied after continuous timeouts to prevent slowing down the system."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						15, 1, 60);
				});
			}
		}

		public BooleanRegistryItem ShowCreditLimitWarningOnJobPosting
		{
			get
			{
				return GetItem("ShowCreditLimitWarningOnJobPosting", delegate
				{
					return new BooleanRegistryItem(
						"ShowCreditLimitWarningOnJobPosting",
						Categories.Accounting_CreditLimitCheck,
						ResString.GetMultilingualString("d8b0bb0d-368f-49cf-bd6b-6de6e81ce865", "Show Credit Limit Warning on Job Posting"),
						ResString.GetMultilingualString("bc49e0f9-c225-4b69-bd18-406b65c903f9", @"If this registry is set to 'Yes', a Credit Limit Warning dialog will be shown on the job related revenue posting (for example via Job Invoicing menu) when one of the Debtors exceeds its Credit Limit. 
With this dialog user can choose to continue or to cancel posting.
The default values is 'No'."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public CodePairRegistryItem IncludeUnpostedRevenueInCreditLimitCalculation
		{
			get
			{
				return GetItem("IncludeUnpostedRevenueInCreditLimitCalculation", delegate
				{
					return new CodePairRegistryItem(
						"IncludeUnpostedRevenueInCreditLimitCalculation",
						Categories.Accounting_CreditLimitCheck_Local,
						ResString.GetMultilingualString("02315a9a-20cd-4dd8-a302-1cc4d38adde2", "Include Unposted Revenue in Credit Limit Calculation"),
						ResString.GetMultilingualString("c5e5f7a2-56f9-4d4a-9048-d5318f7acf93", "This setting determines whether Unposted Recognized and Unrecognized Revenue is included in the Organization Credit Limit checking.\r\nSet to 'Posted Revenue Only' to exclude Unposted Recognized and Unrecognized Revenue from the Credit Limit calculation.\r\nSet to 'Posted and Unposted Recognized Revenue' to include Unposted Recognized Revenue in the Credit Limit calculation.\r\nSet to 'Posted, Unposted Recognized and Unrecognized Revenue' to include Unposted Recognized and Unrecognized Revenue in the Credit Limit calculation."),
						new CodeDescriptionPairListProvider(() => GetIncludeUnpostedRevenueInCreditLimitCalculationOptions()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						Constants.CreditLimitChecking.Posted);
				});
			}
		}

		public CodePairRegistryItem IncludeUnpostedRevenueInGlobalCreditLimitCalculation
		{
			get
			{
				return GetItem("IncludeUnpostedRevenueInGlobalCreditLimitCalculation", delegate
				{
					return new CodePairRegistryItem(
						"IncludeUnpostedRevenueInGlobalCreditLimitCalculation",
						Categories.Accounting_CreditLimitCheck_Global,
						ResString.GetMultilingualString("8ce7be88-2b28-49e7-9c6d-e2df379d477b", "Include Unposted Revenue in Global Credit Limit Calculation"),
						ResString.GetMultilingualString("62209770-4f10-4eee-81d0-1c4547d6977b", "This setting determines whether Unposted Recognized and Unrecognized Revenue is included in the Organization Global Credit Limit checking.\r\nSet to 'Posted Revenue Only' to exclude Unposted Recognized and Unrecognized Revenue from the Global Credit Limit calculation.\r\nSet to 'Posted and Unposted Recognized Revenue' to include Unposted Recognized Revenue in the Global Credit Limit calculation.\r\nSet to 'Posted, Unposted Recognized and Unrecognized Revenue' to include Unposted Recognized and Unrecognized Revenue in the Global Credit Limit calculation."),
						new CodeDescriptionPairListProvider(() => GetIncludeUnpostedRevenueInCreditLimitCalculationOptions()),
						RegistryStorageFlags.System,
						Constants.CreditLimitChecking.Posted);
				});
			}
		}

		CodeDescriptionPairList GetIncludeUnpostedRevenueInCreditLimitCalculationOptions()
		{
			var lookUpList = new CodeDescriptionPairList();
			lookUpList.AddPair(Constants.CreditLimitChecking.Posted, ResString.GetMultilingualString("f49e6422-62cc-4f7a-a0df-3d1d39d5bf47", "Posted Revenue Only"));
			lookUpList.AddPair(Constants.CreditLimitChecking.PostedAndRecognized, ResString.GetMultilingualString("523ec22e-2c94-4ad1-876a-4c016cf2c53e", "Posted and Unposted Recognized Revenue"));
			lookUpList.AddPair(Constants.CreditLimitChecking.PostedRecognizedAndUnrecognized, ResString.GetMultilingualString("69d3001f-8836-4953-ae7b-8d29df5813c8", "Posted, Unposted Recognized and Unrecognized Revenue"));

			return lookUpList;
		}

		public BooleanRegistryItem UseWebServiceForCreditLimit
		{
			get
			{
				return GetItem("UseWebServiceForCreditLimit", delegate
				{
					return new BooleanRegistryItem(
						 "UseWebServiceForCreditLimit",
						 Categories.Accounting_CreditLimitCheck,
						 ResString.GetMultilingualString("139efea1-60cb-467d-8654-e66f58f2f277", "Use Web Service for Credit Limit"),
						 ResString.GetMultilingualString("6adb4733-6a9b-4574-a51c-0f44fa6fd56f", "This setting defines whether a third-party Web Service is used for Organization Credit Limit checking instead of checking on the {0} database.\r\nSet 'Yes' to allow using the Web Service.\r\nYou must specify the Credit Limit Check Web Service URL, User Name and Password before its enabling.", BrandingFactory.Instance.ProductName),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 false);
				});
			}
		}

		public BooleanRegistryItem UseWebServiceForOutstandingBalance
		{
			get
			{
				return GetItem("UseWebServiceForOutstandingBalance", delegate
				{
					return new BooleanRegistryItem(
						 "UseWebServiceForOutstandingBalance",
						 Categories.Accounting_CreditLimitCheck,
						 ResString.GetMultilingualString("0c613d9a-e43c-4ab2-89d2-df6863c81ffa", "Use Web Service for Outstanding Balance"),
						 ResString.GetMultilingualString("90cb2add-806e-4acd-a176-e0060edfcb01", "This setting defines whether a third-party Web Service is used for Organization Outstanding Balance checking instead of checking on the {0} database.\r\nSet 'Yes' to allow using the Web Service.\r\nYou must specify the Credit Limit Check Web Service URL, User Name and Password before its enabling.", BrandingFactory.Instance.ProductName),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 false);
				});
			}
		}

		public BooleanRegistryItem UseWebServiceForUnpostedRevenue
		{
			get
			{
				return GetItem("UseWebServiceForUnpostedRevenue", delegate
				{
					return new BooleanRegistryItem(
						"UseWebServiceForUnpostedRevenue",
						Categories.Accounting_CreditLimitCheck,
						ResString.GetMultilingualString("e315078a-a3c8-43c2-90a8-03a2526984e7", "Use Web Service for Unposted Revenue"),
						ResString.GetMultilingualString("4b5b642a-48e4-4dab-8507-e5cf8198d105", "This setting defines whether a third-party Web Service is used for Organization Unposted Revenue checking instead of checking on the {0} database.\r\nSet 'Yes' to allow using the Web Service.\r\nYou must specify the Credit Limit Check Web Service URL, User Name and Password before its enabling.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService
		{
			get
			{
				return GetItem("EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService", delegate
				{
					return new BooleanRegistryItem(
						"EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService",
						Categories.Accounting_CreditLimitCheck,
						ResString.GetMultilingualString("d4ef41a5-19bf-4889-96d8-24ab8ece5089", "Enable Background Validation on Billing Tab"),
						ResString.GetMultilingualString("a8d82cef-088b-43a4-98cd-5c950096babe", "This setting enables background Credit Limit Validation on Billing tab when a third-party Web Service is used for any part of Organization Credit Limit checking.\r\nSet 'No' to disable background validation."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public StringRegistryItem CreditLimitCheckWebServiceUrl
		{
			get
			{
				return GetItem("CreditLimitCheckWebServiceUrl", delegate
				{
					return new StringRegistryItem("CreditLimitCheckWebServiceUrl",
						 Categories.Accounting_CreditLimitCheck,
						 ResString.GetMultilingualString("8140e71d-0432-4d4a-884c-9ecb21ed190a", "Credit Limit Check Web Service URL"),
						 ResString.GetMultilingualString("4c4fad9b-c01b-406b-a9a9-ad8e1835beb6", "This setting defines the URL of a third-party Organization Credit Limit Check Web Service."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 string.Empty);
				});
			}
		}

		public StringRegistryItem CreditLimitCheckWebServiceUserName
		{
			get
			{
				return GetItem("CreditLimitCheckWebServiceUserName", delegate
				{
					return new StringRegistryItem("CreditLimitCheckWebServiceUserName",
						 Categories.Accounting_CreditLimitCheck,
						 ResString.GetMultilingualString("b6bd3a7b-adf6-4279-aefe-dd9770e631f2", "Credit Limit Check Web Service User Name"),
						 ResString.GetMultilingualString("23b14389-0dd7-4675-9006-fc06da14de9b", "This setting defines User Name to access a third-party Organization Credit Limit Check Web Service."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 string.Empty);
				});
			}
		}

		public StringRegistryItem CreditLimitCheckWebServicePassword
		{
			get
			{
				return GetItem("CreditLimitCheckWebServicePassword", delegate
				{
					return new StringRegistryItem("CreditLimitCheckWebServicePassword",
						 Categories.Accounting_CreditLimitCheck,
						 ResString.GetMultilingualString("f2e2d54f-e1f6-43fb-a2a1-56a6df87fc0e", "Credit Limit Check Web Service Password"),
						 ResString.GetMultilingualString("fc47832b-06f2-41be-85f6-6db981b3a6c6", "This setting defines Password to access a third-party Organization Credit Limit Check Web Service."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 RegistryOptions.IsPasswordVisibleForControllerUser,
						 string.Empty)
					{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password) };
				});
			}
		}

		public CodePairRegistryItem ResubmitCreditApprovalRequestsBasedOnBillingValues
		{
			get
			{
				return GetItem("ResubmitCreditApprovalRequestsBasedOnBillingValues", () =>
				{
					return new CodePairRegistryItem(
						"ResubmitCreditApprovalRequestsBasedOnBillingValues"
						, Categories.Accounting_CreditLimitCheck
						, ResString.GetMultilingualString("7FDF686A-EFF3-48C0-BD7C-3D681CF7E424", "Resubmit Credit Approval Requests Based on Billing Values") //caption
						, ResString.GetMultilingualString("CC051578-E734-4FED-BD89-83B1CD4C59BF", @"This registry allows {0} to compare the current billing values to the billing values that were recorded at the point of making a credit control request.
The default behavior of this registry is to not compare current billing values when checking for credit control request.
If the registry value is overridden to a non-default value then when the sell values Increase and/or Decrease or there is a change in the organizations subjected to credit control evaluation, previous request will be canceled and a new one will be created.
The available options are:
DEF - No comparison made (this is the registry default).
INC - Create new credit control request only when sell values have increased.
BTH - Create new credit control request when sell values have both increased or decreased.", BrandingFactory.Instance.ProductName), // hint
						new CodeDescriptionPairListProvider(() => new ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingConstants.ResubmitCreditApprovalRequestsBasedOnBillingValuesTypes.Default.Code);
				});
			}
		}

		public CreditTemporaryIncreaseAuthorisationSettingsRegistryItem CreditLimitCheckTemporaryCreditLimitIncreaseThreshold
		{
			get
			{
				return GetItem("CreditLimitCheckTemporaryCreditLimitIncreaseThreshold", delegate
				{
					return new CreditTemporaryIncreaseAuthorisationSettingsRegistryItem("CreditLimitCheckTemporaryCreditLimitIncreaseThreshold",
						 Categories.Accounting_CreditLimitCheck,
						 ResString.GetMultilingualString("1d14da78-1426-448a-ba60-006ea157bb1e", "Temporary Credit Limit Increase Threshold"),
						 ResString.GetMultilingualString("5b5dea45-bf73-43d4-b7be-8287dc1bad0b", @"Use this registry to set a percentage or amount threshold at which an authorized user can temporary increase the credit limit for AR Organizations.
In addition, the temporary increase in credit limit can be set to expire after a specific number of days (from the day of adjustment).

The authorization levels (up to 3) restrict the setting of the temporary increase to users with specific authorization levels that is set up in the security settings.
The authorization requirement “None” allows users with access to change the credit limit amount up to the specified threshold level."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		public CreditControlledDocumentsCheckConfigurationRegistryItem CreditLimitCheckOverdueInvoicesStatusCheck
		{
			get
			{
				return GetItem("CreditLimitCheckOverdueInvoicesStatusCheck", delegate
				{
					return new CreditControlledDocumentsCheckConfigurationRegistryItem("CreditLimitCheckOverdueInvoicesStatusCheck",
						 Categories.Accounting_CreditLimitCheck_Local,
						 ResString.GetMultilingualString("e741be52-ede4-49e6-bcc1-160a4470f3c9", "Overdue Invoices Status Check"),
						 ResString.GetMultilingualString("777ba35b-8b7b-4fd0-97f8-8a69b0b33419", @"Use this registry to restrict the delivery of credit controlled documents based on the overdue statuses of Standard and Disbursement Transactions.
Users will not be allowed to generate any credit controlled documents (unless overridden by authorized credit controllers) if the total outstanding amount of the AR Transactions is above this registry setting, even if the AR Debtor had not exceeded credit limit, or put on credit hold."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		public BooleanRegistryItem ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation
		{
			get
			{
				return GetItem("ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation", delegate
				{
					return new BooleanRegistryItem(
						"ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation",
						Categories.Accounting_CreditLimitCheck_Local,
						ResString.GetMultilingualString("C0857EE7-81A8-4508-9EBB-C9C9BB5EEBAE", "Exclude Open Claims Amounts From Overdue Credit Checking Calculation"),
						ResString.GetMultilingualString("E8FF3989-AFFB-4BE7-9A2F-2F1F0EBF74FF", @"By default, all AR Outstanding Invoices will be taken into consideration during the local credit limit check calculation.
When this registry is set to 'Yes', claim amounts relating to AR Outstanding Invoices attach to Open Claims will be excluded from the local credit limit check calculation.

Open Claims refer to claims with 'OPN', 'WRK', 'REJ', 'CCR' and 'CCA' statuses located under Manage > Receivables > Claims and Queries.
AR Invoices refer to invoices attach the these claims."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public CreditControlledDocumentsCheckConfigurationRegistryItem GlobalCreditLimitCheckOverdueInvoicesStatusCheck
		{
			get
			{
				return GetItem("GlobalCreditLimitCheckOverdueInvoicesStatusCheck", delegate
				{
					return new CreditControlledDocumentsCheckConfigurationRegistryItem("GlobalCreditLimitCheckOverdueInvoicesStatusCheck",
						Categories.Accounting_CreditLimitCheck_Global,
						ResString.GetMultilingualString("9f6a8c55-889e-4ed6-a42c-ffbc700c03a6", "Global Overdue Invoices Status Check"),
						ResString.GetMultilingualString("bcddb15f-0bb7-40bc-8d82-6595f086787f", @"Use this registry to restrict the delivery of credit controlled documents based on the overdue statuses of Standard and Disbursement Invoices across all systems companies.
Users will not be allowed to generate any credit controlled documents (unless overridden by authorized credit controllers) if the total outstanding amount of the AR Invoices is above this registry setting, even if the Global Credit Control Group had not exceeded global credit limit, or put on credit hold."),
						RegistryStorageFlags.System);
				});
			}
		}

		public BooleanRegistryItem GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation
		{
			get
			{
				return GetItem("GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation", delegate
				{
					return new BooleanRegistryItem(
						"GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation",
						Categories.Accounting_CreditLimitCheck_Global,
						ResString.GetMultilingualString("B064B336-6B5D-49F6-8484-444CAED0191F", "Global Exclude Open Claims Amounts From Overdue Credit Checking Calculation"),
						ResString.GetMultilingualString("C96D92D9-88B6-43A9-8461-12D75B6AC596", @"By default, all AR Outstanding Invoices will be taken into consideration during the global credit limit check calculation.
When this registry is set to 'Yes', claim amounts relating to AR Outstanding Invoices attach to Open Claims will be excluded from the global credit limit check calculation.

Open Claims refer to claims with 'OPN', 'WRK', 'REJ', 'CCR' and 'CCA' statuses located under Manage > Receivables > Claims and Queries.
AR Invoices refer to invoices attach the these claims."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Transaction Payment Status

		public BooleanRegistryItem UseWebServiceForTransactionPaymentStatus
		{
			get
			{
				return GetItem("UseWebServiceForTransactionPaymentStatus", delegate
				{
					return new BooleanRegistryItem(
						 "UseWebServiceForTransactionPaymentStatus",
						 Categories.Accounting_TransactionPaymentStatus,
						 ResString.GetMultilingualString("56aece28-1868-4f77-a9e9-705b70ef7634", "Use Web Service for Transaction Payment Status"),
						 ResString.GetMultilingualString("f221156a-1fdd-41f1-8416-fdc4d66d28b0", "This setting defines whether a third-party Web Service is used for Transaction Payment Status instead of using the {0} database.\r\nSet 'Yes' to allow using the Web Service.\r\nYou must specify the Transaction Payment Status Web Service URL, User Name and Password before its enabling.", BrandingFactory.Instance.ProductName),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 false);
				});
			}
		}

		public StringRegistryItem TransactionPaymentStatusWebServiceUrl
		{
			get
			{
				return GetItem("TransactionPaymentStatusWebServiceUrl", delegate
				{
					return new StringRegistryItem("TransactionPaymentStatusWebServiceUrl",
						 Categories.Accounting_TransactionPaymentStatus,
						 ResString.GetMultilingualString("1fdb57ee-317a-4b2f-aded-167212997cde", "Transaction Payment Status Web Service URL"),
						 ResString.GetMultilingualString("1124547b-169b-4c34-a950-e8a928b4da5b", "This setting defines the URL of a third-party Transaction Payment Status Web Service."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 string.Empty);
				});
			}
		}

		public StringRegistryItem TransactionPaymentStatusWebServiceUserName
		{
			get
			{
				return GetItem("TransactionPaymentStatusWebServiceUserName", delegate
				{
					return new StringRegistryItem("TransactionPaymentStatusWebServiceUserName",
						 Categories.Accounting_TransactionPaymentStatus,
						 ResString.GetMultilingualString("c348831b-964f-48b3-8009-d781aec20aa2", "Transaction Payment Status Web Service User Name"),
						 ResString.GetMultilingualString("002b2b51-b268-4aff-a9fb-5d129074e62d", "This setting defines the User Name to access a third-party Transaction Payment Status Web Service."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 string.Empty);
				});
			}
		}

		public StringRegistryItem TransactionPaymentStatusWebServicePassword
		{
			get
			{
				return GetItem("TransactionPaymentStatusWebServicePassword", delegate
				{
					return new StringRegistryItem("TransactionPaymentStatusWebServicePassword",
						 Categories.Accounting_TransactionPaymentStatus,
						 ResString.GetMultilingualString("4d73ff58-21c4-4eea-a4d8-e2f2632e3c4e", "Transaction Payment Status Web Service Password"),
						 ResString.GetMultilingualString("09691e91-2b26-43e1-8e4b-c9570378fa4a", "This setting defines the Password to access a third-party Transaction Payment Status Web Service."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company,
						 RegistryOptions.IsPasswordVisibleForControllerUser,
						 string.Empty)
					{ EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password) };
				});
			}
		}

		#endregion

		#region Matching

		public CodePairRegistryItem ClearingJournalConfiguration
		{
			get
			{
				return GetItem("ClearingJournalConfiguration", delegate
				{
					return new CodePairRegistryItem(
						"ClearingJournalConfiguration",
						Categories.Accounting_Matching,
						ResString.GetMultilingualString("A1AD0DC6-8CC8-444A-8A48-6447032F0FF8", "Clearing Journal Configuration"),
						ResString.GetMultilingualString("3176883C-2C0A-4A90-92AC-68D8DB99ECFF",
		@"This setting changes the behavior of {0} when AR and / or AP transactions belonging to multiple branches, ledgers and organizations are matched together in a single match session.
The default setting is 'STD'.
When 'STD' {0} automatically creates Transfer and Contra transactions when matching AR and AP transactions across different organizations and ledgers.
The 'HBR' setting creates AR and AP Clearing journals instead. It will create a Clearing Journal for each transaction in the match session. Transaction Header Branch will be used when creating each Clearing Journal.
The 'LBR' setting creates AR and AP Clearing journals for each transaction line in the match session. A Clearing Journal is also created for those transaction types with no lines (like receipt). Transaction Line Branch is used when creating each Journal.
The 'LBX' setting creates summary AR and AP Clearing Journals. Instead of creating Journals for each individual matched line/header, the LBX setting creates summary Clearing Journals. A separate Journal is created for each combination of Branch + Organization + Ledger matched.
This  registry is relevant to login companies that want to manage and report Accounts Receivables and Accounts Payable balances by Branch.", BrandingFactory.Instance.ProductName),
						new CodeDescriptionPairListProvider(() => new ClearingJournalConfigurationTypes()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingConstants.ClearingJournalConfigurationTypes.Standard.Code);
				});
			}
		}

		public IntRegistryItem MaximumResultsInMatchingSearch
		{
			get
			{
				return GetItem("MaximumResultsInMatchingSearch", delegate
				{
					return new IntRegistryItem(
						"MaximumResultsInMatchingSearch",
						Categories.Accounting_Matching,
						ResString.GetMultilingualString("5b9e7948-6bf0-4c0b-a63a-80d3205f4ffb", "Max Results in Matching Search"),
						ResString.GetMultilingualString("4ec48c56-d9e4-4645-8b5a-067b0a4a1d96", @"This setting limits the maximum number of  Unmatched Transactions returned returned by transaction search on the Matching screen.
Increasing this number can affect performance of your matching functionality."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						1000);
				});
			}
		}

		public BooleanRegistryItem PopupUnMatchTransactionDescriptionOverrideOnUnMatching
		{
			get
			{
				return GetItem("PopupUnMatchTransactionDescriptionOverrideOnUnMatching", delegate
				{
					return new BooleanRegistryItem(
						"PopupUnMatchTransactionDescriptionOverrideOnUnMatching",
						Categories.Accounting_Matching,
						ResString.GetMultilingualString("0600F7AA-7C63-4F83-880B-079D646E3B5A", "Popup Un-Match Transaction Description Override on Un-Matching"),
						ResString.GetMultilingualString("9AD59786-CED0-4775-B45A-25462BF63E87", @"When this registry is set to 'YES', {0} will pop-up a form allowing users to override the description of the reversed miscellaneous matching transaction created during un-matching.", BrandingFactory.Instance.ProductName, false),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Cash Basis Tax Configuration

		public TaxRecognitionDefaultingRulesRegistryItem TaxRecognitionDefaultingRules
		{
			get
			{
				return GetItem("TaxRecognitionDefaultingRules",
								() => new TaxRecognitionDefaultingRulesRegistryItem(
										"TaxRecognitionDefaultingRules",
										Categories.Accounting_CashBasisTaxConfiguration,
										ResString.GetMultilingualString("EF1A8D8B-577B-4F1A-A8C8-1169C77B420D", "Cash Basis Tax Configuration"),
										ResString.GetMultilingualString("C10549DC-6797-418A-BF8C-AA0CA24EE604", @"This registry defines the default Tax Recognition Behavior of a Login Company.
By default, all Tax recorded in {0} are recognized as reportable on an Accrual Basis (Post date of the transaction).
Override of this registry is only permitted for Login Companies Licensed to use Cash Basis GST/VAT features.
This registry is ONLY relevant to Login Companies enabled for Cash Basis GST/VAT features.", BrandingFactory.Instance.ProductName),
										RegistryStorageFlags.Company));
			}
		}

		public CodePairRegistryItem PartPaymentTaxRealizationRule
		{
			get
			{
				return GetItem("PartPaymentTaxRealizationRule",
					() => new CodePairRegistryItem(
						"PartPaymentTaxRealizationRule",
						Categories.Accounting_CashBasisTaxConfiguration,
						ResString.GetMultilingualString("298A8FDB-7D31-4380-A022-0F1A03C905D5", "Part Payment Tax Realization Rule"),
						ResString.GetMultilingualString("ABC6C441-BF86-4754-A977-6E4E1B7E14C5", @"This registry is ONLY relevant to Login Companies enabled for Cash Basis GST/VAT features.
The registry determines how Cash Basis Tax amounts will be recognized as reportable when transactions are Part Paid.
Note:  This registry is NOT Relevant to Accrual Basis Tax amounts.  Accrual Basis Tax transaction lines are always recognized as reportable on the transaction Post Date."),
						new CodeDescriptionPairListProvider(() => new PartPaymentTaxRealizationRuleTypes()),
						RegistryStorageFlags.Company,
						AccountingConstants.PartPaymentTaxRealizationRuleTypes.Proportionally.Code));
			}
		}

		#endregion

		#region Cash Advance

		#region Receivables

		public BooleanRegistryItem EnableReceivablesCashAdvanceFunctionality
		{
			get
			{
				var defaultValue = false;
				var options = RegistryOptions.IsOnlyForSupport;
				if (FeatureControlHelper.IsAdvancePaymentFeatureEnabled)
				{
					defaultValue = true;
					options = RegistryOptions.Default;
				}
				return GetItem("EnableReceivablesCashAdvanceFunctionality", delegate
				{
					return new BooleanRegistryItem(
						"EnableReceivablesCashAdvanceFunctionality",
						Categories.Accounting_CashAdvance_Receivables,
						(NoResString)"Enable Advance Payment Functionality",
						(NoResString)@"Advance Payment functionality allows the request of an advance payment for nominated charges on a job prior to work commencing on the job and prior to an AR Invoice being raised.
Set to Yes to enable this functionality.",
						RegistryStorageFlags.System, options,
						defaultValue);
				});
			}
		}

		public AccountingRegistryItem CashAdvanceClearingAccount
		{
			get
			{
				return GetItem("CashAdvanceClearingAccount", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"CashAdvanceClearingAccount",
						Categories.Accounting_CashAdvance_Receivables,
						ResString.GetMultilingualString("FCC31A32-D778-45F7-8577-6BD1518622E6", "Advance Payment Clearing Account"),
						ResString.GetMultilingualString("67C859BC-1C84-488D-8E3A-61D86C5D6774", "Enter the GL account where received Advance Payments will be held until the work paid in advance is invoiced."),
						new GuidRegistryDataType(),
						RegistryStorageFlags.System);
					result.Options = EnableReceivablesCashAdvanceFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden;
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl);
					return result;
				});
			}
		}

		public BooleanRegistryItem IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation
		{
			get
			{
				return GetItem("IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation", delegate
				{
					return new IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItem(
						new IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationRegistryItemImpl(
							EnableReceivablesCashAdvanceFunctionality,
							"IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation",
							Categories.Accounting_CashAdvance_Receivables,
							ResString.GetMultilingualString("4b8c1dbf-5000-4adb-a422-c1c868504a24", "Include Advance Payment Requests in Credit Controlled Document Evaluation"),
							ResString.GetMultilingualString("b3af4984-2963-48c7-be6e-73007d4d838d", @"When set to Yes, Advance Payment Requests will be included when evaluating if Credit Controlled Documents on a job can be produced. Credit Controlled Documents will not be produced if there is any outstanding Advance Payment amount on a job.
Set this registry to No in order to allow Credit Controlled Documents to be produced on a job when there are still unpaid Advance Payment Requests."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							EnableReceivablesCashAdvanceFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden));
				});
			}
		}

		public BooleanRegistryItem AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid
		{
			get
			{
				return GetItem("AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid", delegate
				{
					return new BooleanRegistryItem(
						"AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid",
						Categories.Accounting_CashAdvance_Receivables,
						ResString.GetMultilingualString("e27e880b-0a6a-444b-af51-eae151e3cb39", "Allow Manual Setting of Advance Payment Request Status to Paid"),
						ResString.GetMultilingualString("7a04b992-90c7-45a9-8d4c-d6e74daa9231", @"Set this registry to Yes to allow the status of a Advance Payment Request to be manually set to PAI - Paid in Full.
This registry should only be set to Yes when Receivables receipts are not recorded in CargoWise. 
When receivables receipts are recorded in CargoWise for Advance Payments, receipting of the Advance Payment will set the Advance Payment status to reflect that it is paid, create AR journals and the appropriate general ledger postings."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						EnableReceivablesCashAdvanceFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						false);
				});
			}
		}

		public MultilingualStringRegistryItem CashAdvanceRequestDocumentTitle
		{
			get
			{
				return GetItem("CashAdvanceRequestDocumentTitle", delegate
				{
					return new MultilingualStringRegistryItem(
						"CashAdvanceRequestDocumentTitle",
						Categories.Accounting_CashAdvance_Receivables,
						ResString.GetMultilingualString("975B69B7-4D11-4BA9-8D1C-258567CABA6D", "Advance Payment Request Document Title"),
						ResString.GetMultilingualString("33F4DD7F-15DB-4B86-80DA-E6A6AE91DB55", "This title will display in the heading banner of the Advance Payment Request Document"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						EnableReceivablesCashAdvanceFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						ResString.GetMultilingualString("EE02A74B-F7A0-4014-8A27-145EAD019C08", "Advance Payment Request"));
				});
			}
		}

		public MultilingualStringRegistryItem CashAdvanceRequestDocumentMessage
		{
			get
			{
				return GetItem("CashAdvanceRequestDocumentMessage", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"CashAdvanceRequestDocumentMessage",
						Categories.Accounting_CashAdvance_Receivables,
						ResString.GetMultilingualString("7F7B12CD-26AF-4BFA-B02A-4E43BA5E8545", "Advance Payment Request Document Message"),
						ResString.GetMultilingualString("8AA2B0BC-4B29-41C3-9FF1-6B5C676321A1", "This message will appear in the footer of the Advance Payment Request Document"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						EnableReceivablesCashAdvanceFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region Payables

		public BooleanRegistryItem EnablePayablesCashAdvanceFunctionality
		{
			get
			{
				var defaultValue = false;
				var options = RegistryOptions.IsOnlyForSupport;
				if (FeatureControlHelper.IsAdvancePaymentFeatureEnabled)
				{
					defaultValue = true;
					options = RegistryOptions.Default;
				}
					return GetItem("EnablePayablesCashAdvanceFunctionality", delegate
				{
					return new BooleanRegistryItem(
						"EnablePayablesCashAdvanceFunctionality",
						Categories.Accounting_CashAdvance_Payables,
						(NoResString)"Enable Payables Advance Payment Functionality",
						(NoResString)@"Payables Advance Payment functionality allows you to record and action requests for advance payment for nominated job costs prior to receiving an AP invoice for the costs. 
Set to Yes to enable this functionality.",
						RegistryStorageFlags.System, options,
						defaultValue);
				});
			}
		}

		public BooleanRegistryItem AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid
		{
			get
			{
				return GetItem("AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid", delegate
				{
					return new BooleanRegistryItem(
						"AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid",
						Categories.Accounting_CashAdvance_Payables,
						ResString.GetMultilingualString("FB490546-B747-4793-B592-B79B525EE45C", "Allow Manual Setting of Payables Advance Payment Request Status to Paid"),
						ResString.GetMultilingualString("59DE34FB-F1E2-45B5-B47D-B7826A3AF4F7", @"Set this registry to Yes to allow the status of a Payables Advance Payment Request to be manually set to PAI - Paid in Full.
This registry should only be set to Yes when payables payments are not recorded in CargoWise. 
When payables payments are recorded in CargoWise for Advance Payments, payment of the Advance Payment will set the Advance Payment status to reflect that it is paid, create AP journals and the appropriate general ledger postings."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						EnablePayablesCashAdvanceFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						false);
				});
			}
		}

		public AccountingRegistryItem PayablesCashAdvanceClearingAccount
		{
			get
			{
				return GetItem("PayablesCashAdvanceClearingAccount", delegate
				{
					AccountingRegistryItem result = new AccountingRegistryItem(
						"PayablesCashAdvanceClearingAccount",
						Categories.Accounting_CashAdvance_Payables,
						ResString.GetMultilingualString("30C7B507-A677-4B01-90D8-34613EBC5103", "Payables Advance Payment Clearing Account"),
						ResString.GetMultilingualString("E5E29293-01F5-4296-B305-0028C2566F84", "Enter the GL account where funds paid out in advance of receipt of an AP invoice will be recorded until the invoice is received."),
						new GuidRegistryDataType(),
						RegistryStorageFlags.System);
					result.Options = EnablePayablesCashAdvanceFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden;
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccGLHeader, RegistryFindBoxFilter.BSHAndNonControl);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#region InvoicePostingExchangeRateOption

		public string GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum ledger, bool isLocalCurrency, ZGuid companyPK)
			=> GetInvoicePostingExchangeRateOptionRegistryValue(ledger, isLocalCurrency, companyPK)?.ExRateOption ?? InvoicePostingExchangeRateOption.Default.Code;

		public int GetInvoicePostingExchangeRateOptionOffSet(ExchangeRateValidLedgerEnum ledger, bool isLocalCurrency, ZGuid companyPK)
		{
			var registryValue = GetInvoicePostingExchangeRateOptionRegistryValue(ledger, isLocalCurrency, companyPK);
			return registryValue != null && !registryValue.ExRateOption.Equals(InvoicePostingExchangeRateOption.Default.Code) ? registryValue.OffSet : ZInt.Zero;
		}

		InvoicePostingExRateOption GetInvoicePostingExchangeRateOptionRegistryValue(ExchangeRateValidLedgerEnum ledger, bool isLocalCurrency, ZGuid companyPK)
		{
			var currencyType = isLocalCurrency ? Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local : Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign;
			return GetInvoicePostingExchangeRateRegistryItem(ledger)?.GetFallBackValueAtAllLevels(companyPK.IsValid ? companyPK.ToGuid() : GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
				.Cast<InvoicePostingExRateOption>().FirstOrDefault(x => x.InvoiceCurrencyType == currencyType);
		}

		public string GetInvoicePostingExchangeRateRegistryItemCaption(ExchangeRateValidLedgerEnum ledger)
			=> GetInvoicePostingExchangeRateRegistryItem(ledger).Caption;

		public StronglyTypedRegistryItem<InvoicePostingExRateOptionCollection> GetInvoicePostingExchangeRateRegistryItem(ExchangeRateValidLedgerEnum ledger)
		{
			switch (ledger)
			{
				case ExchangeRateValidLedgerEnum.AR:
					return InvoicePostingExchangeRateOptionAR;

				case ExchangeRateValidLedgerEnum.AP:
				case ExchangeRateValidLedgerEnum.UA:
					return InvoicePostingExchangeRateOptionAP;

				default:
					ErrorReporter.ReportOnce(Invariant($"Unsupported ledger {Enum.GetName(typeof(ExchangeRateValidLedgerEnum), ledger)}"));
					return null;
			}
		}

		public InvoicePostingExRateOptionRegistryItem InvoicePostingExchangeRateOptionAR
		{
			get
			{
				return GetItem(nameof(InvoicePostingExchangeRateOptionAR), delegate
				{
					return new InvoicePostingExRateOptionRegistryItem(nameof(InvoicePostingExchangeRateOptionAR),
							Categories.Accounting_ReceivableDefaults_DefaultSettings,
							ResString.GetMultilingualString("30029b9c-fac4-4b6a-8fb3-2d93606cf242", "AR Invoice Posting Exchange Rate Option"),
							ResString.GetMultilingualString("3c67743b-6b86-4181-be2c-c67b9503c849", @"Use this registry to configure the exchange rate option to be used during the posting of job and non-job related Revenues.
NOTE: The configuration does not affect the application of job exchange rate during the creation of WIP's.

By default, this registry is set to DEF - Default behavior.

All Job level charges are posted as per exchange rate entered when posting in job invoicing module (including periodic invoices, etc).
All Agent Invoices are posted as per the exchange rate 'Calculation Method' selected during posting of overseas agent charges at consol level.
All Non-Job related charges posted via Receivables module are posted using the exchange rate entered during transaction entry.

To update the exchange rate during transaction posting, you can override the default behavior based on whether the the invoice is posted in LOC (Local) or FOR (Foreign) currency.
In the Exchange Rate Options, you can select;

TOD - Today's Exchange Rate.
Both Job and Non-Job related charges are posted using today's (creation date) exchange rate with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

INV - Exchange Rate based on Invoice Date.
Both Job and Non-Job related charges are posted using the exchange rate set for Invoice Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

PST - Exchange Rate based on Post Date.
Both Job and Non-Job related charges are posted using the exchange rate set for Post Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

EIT - Exchange Rate based on Earliest of Invoice Date and Tax Date.
Both Job and Non-Job related charges are posted using the exchange rate set on whichever is earliest of Invoice Date and Tax Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

You can also use the Offset column to offset exchange rates from a specific date selection (TOD, INV, PST or EIT) by the specified number of days. It can be before (-) or after (+) number of days. This field only accept integers.
When DEF option is used, exchange rate date and offset are determined based on the settings in the Job Billing Exchange Rate Configuration.

NOTE: In all cases, This registry only determines the Date of the exchange rate. Exchange rate TYPE is determined by the Job Billing Exchange Rate Configuration module or the relevant Organization."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company
						);
				});
			}
		}

		public InvoicePostingExRateOptionRegistryItem InvoicePostingExchangeRateOptionAP
		{
			get
			{
				return GetItem(nameof(InvoicePostingExchangeRateOptionAP), delegate
				{
					return new InvoicePostingExRateOptionRegistryItem(nameof(InvoicePostingExchangeRateOptionAP),
							Categories.Accounting_PayableDefaults_DefaultSettings,
							ResString.GetMultilingualString("cdd55d4b-5ede-4402-9c82-6ba7fa16c005", "AP Invoice Posting Exchange Rate Option"),
							ResString.GetMultilingualString("309f083c-9d63-4614-86af-78be0d8447c0", @"Use this registry to configure the exchange rate option to be used during the posting of job and non-job related Costs.
NOTE: The configuration does not affect the application of job exchange rate during the creation of Accruals.

By default, this registry is set to DEF - Default behavior.

All Job level costs are posted as per exchange rate entered when posting in job invoicing module.
All Job and Non-Job related costs posted via Payables module are posted according to the 'Use Job Exchange Rate' setting.

To update the exchange rate during transaction posting, you can override the default behavior based on whether the the invoice is posted in LOC (Local) or FOR (Foreign) currency.
In the Exchange Rate Options, you can select;

TOD - Today's Exchange Rate.
Both Job and Non-Job related costs are posted using today's (creation date) exchange rate with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

INV - Exchange Rate based on Invoice Date.
Both Job and Non-Job related costs are posted using the exchange rate set for Invoice Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

PST - Exchange Rate based on Post Date.
Both Job and Non-Job related costs are posted using the exchange rate set for Post Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

EIT - Exchange Rate based on Earliest of Invoice Date and Tax Date.
Both Job and Non-Job related costs are posted using the exchange rate set on whichever is earliest of Invoice Date and Tax Date with fall back to previous exchange rate if 'Fall Back to Previous Exchange Rate' registry is set to 'Yes'.

You can also use the Offset column to offset exchange rates from a specific date selection (TOD, INV, PST or EIT) by the specified number of days. It can be before (-) or after (+) number of days. This field only accept integers.
When DEF option is used, exchange rate date and offset are determined based on the settings in the Job Billing Exchange Rate Configuration.

NOTE: In all cases, This registry only determines the Date of the exchange rate. Exchange rate TYPE is determined by the Job Billing Exchange Rate Configuration module or the relevant Organization."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company
						);
				});
			}
		}

		#endregion

		#region Job Costing Report
		public JCDServiceTaskControllerRegistryItem JCDServiceTaskController
		{
			get
			{
				return GetItem("JCDServiceTaskController", delegate
				{
					return new JCDServiceTaskControllerRegistryItem(
						"JCDServiceTaskController",
						Categories.Accounting_JobCostingReports,
						ResString.GetMultilingualString("27e0958d-3572-4757-aa9f-e13a40e69e51", "Job Costing Data Queue Service task controller"),
						ResString.GetMultilingualString("360c5b74-04e8-401e-847f-a9193e959ebb", "This registry is used to Initialize, Remove or Re-initialize Job Costing Data Queue Service task"),
						RegistryStorageFlags.System,
						RegistryOptions.Default);
				});
			}
		}

		public IntRegistryItem JobCostingQueueProcessBatchSize
		{
			get
			{
				return GetItem("JobCostingQueueProcessBatchSize", delegate
				{
					return new IntRegistryItem(
						"JobCostingQueueProcessBatchSize",
						Categories.Accounting_JobCostingReports,
						(NoResString)"Job Costing Queue process batch size (CargoWiseOne Support Only)",
						(NoResString)@"This registry is used to configure the number of records processed by the 'JCD' service task every time it runs.

By default this registry is set to 20000.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport,
						20000, 0, 99999999);
				});
			}
		}

		public IntRegistryItem TransactionLineToJobCostingRecordTransformationBatchSize
		{
			get
			{
				return GetItem("TransactionLineToJobCostingRecordTransformationBatchSize", delegate
				{
					return new IntRegistryItem(
						"TransactionLineToJobCostingRecordTransformationBatchSize",
						Categories.Accounting_JobCostingReports,
						(NoResString)"Transaction Line To JobCosting record Transformation Batch Size (CargoWiseOne Support Only)",
						(NoResString)@"This registry is used to configure the number of AccTransactionLine record transferred to JobCostingQueue table in each iteration. This transformation is done by the 'ODT' service task.

By default this registry is set to 1000.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport,
						1000, 0, int.MaxValue);
				});
			}
		}

		public IntRegistryItem JCDQueueHighWaterMark
		{
			get
			{
				return GetItem("JCDQueueHighWaterMark",
								() => new IntRegistryItem(
										"JCDQueueHighWaterMark",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										1));
			}
		}

		public IntRegistryItem JCDReportDataCollectionStartingPeriod
		{
			get
			{
				return GetItem("JCDReportDataCollectionStartingPeriod",
								() => new IntRegistryItem(
										"JCDReportDataCollectionStartingPeriod",
										null,
										null,
										null,
										RegistryStorageFlags.System,
										RegistryOptions.IsHidden,
										0));
			}
		}

		public IntRegistryItem MaximumNumberOfJCDPartitionKeys
		{
			get
			{
				return GetItem("MaximumNumberOfJCDPartitionKeys",
								() => new IntRegistryItem(
										"MaximumNumberOfJCDPartitionKeys",
										Categories.Accounting_JobCostingReports,
										(NoResString)"Maximum number of partition keys (CargoWiseOne Support Only)",
										(NoResString)@"This registry is used to set the maximum number of partitions that are allowed to be created for JCD report table. Oldest partition will be dropped before creating a new partition, if number of partitions reaches this value.

By default this registry is set to 14500.",
										RegistryStorageFlags.System,
										RegistryOptions.IsOnlyForSupport,
										14500,
										1,
										15000));
			}
		}

		public BooleanRegistryItem RemoveLineThatDoesnotHaveAccountingPeriodFromJobCostingDataQueueTable
		{
			get
			{
				return GetItem("RemoveLineThatDoesnotHaveAccountingPeriodFromJobCostingDataQueueTable", delegate
				{
					return new BooleanRegistryItem(
						"RemoveLineThatDoesnotHaveAccountingPeriodFromJobCostingDataQueueTable",
						Categories.Accounting_JobCostingReports,
						ResString.GetMultilingualString("704d0285-d87c-477b-88bb-011d60c6ba4c", "Remove Line form Queue table if it is posted or reversed on a date that is outside of any accounting period"),
						ResString.GetMultilingualString("1fa20980-6666-4e4d-b220-f0dcdedaf3a3", "When this is set to 'YES', Job Costing Data Queue service task removes a line form Queue table if it is posted or reversed on a date that is outside of any accounting period"),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public IntRegistryItem JobCostingReportRelatedDBObjectVersion
		{
			get
			{
				return GetItem("JobCostingReportRelatedDBObjectVersion",
					() => new IntRegistryItem(
						"JobCostingReportRelatedDBObjectVersion",
						Categories.Accounting_JobCostingReports,
						(NoResString)"Current version Number of Job Costing Data Queue service task related Database Objects (CargoWiseOne Support Only)",
						(NoResString)"Shows current version number of Job Costing Data Queue service task related Database Objects (Tables, Stored Procedure, Functions etc). These Database Objects are created/updated by Job Costing Data Queue service task and used by the Job profit Reports. Job Costing Data Queue service task updates the value of this Registry",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						-1));
			}
		}

		public IntRegistryItem JobCostingQueueDBObjectVersion
		{
			get
			{
				return GetItem("JobCostingQueueDBObjectVersion",
					() => new IntRegistryItem(
						"JobCostingQueueDBObjectVersion",
						Categories.Accounting_JobCostingReports,
						(NoResString)"Current version Number of Temporary Database Objects used to process existing transaction record (CargoWiseOne Support Only)",
						(NoResString)"Shows the current version number of Temporary Database Objects used by Job Costing Data Queue service task to process existing (i.e. transactions created before the initialisation of Job Costing Data Queue service task) transaction records",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						-1));
			}
		}

		#endregion

		public GuidRegistryItem SecurityRightsForTransactionAllocateAndPostTrigger
		{
			get
			{
				return GetItem("SecurityRightsForTransactionAllocateAndPostTrigger", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"SecurityRightsForTransactionAllocateAndPostTrigger",
						Categories.Accounting_PayableDefaults,
						ResString.GetMultilingualString("6E58DB46-271D-48FF-A359-0EC78E9F8FB8", "Security for trigger allocation and post"),
						ResString.GetMultilingualString("2527B0D1-93A4-48F6-834B-4EA07F274539", "Set user for security rights when allocating and posting via TPA trigger"),
						RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbStaff);
					return result;
				});
			}
		}

		public StringRegistryItem HMRCOAuthTokens
		{
			get
			{
				return GetItem("HMRCOAuthTokens", delegate
				{
					return new StringRegistryItem(
						"HMRCOAuthTokens",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"HMRC authorization tokens",
						(NoResString)"HMRC authorization tokens",
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden | RegistryOptions.NotCached,
						string.Empty);
				});
			}
		}

		public BooleanRegistryItem IsMTDProductionMode
		{
			get
			{
				return GetItem("IsMTDProductionMode", delegate
				{
					return new BooleanRegistryItem(
						"IsMTDProductionMode",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"Is MTD in Production Mode (CargoWiseOne Support Only)",
						(NoResString)@"This registry determines if application is accessing MTD APIs in HMRC's production environment. When this registry is set to No, the application will access MTD APIs in HMRC's sandbox environment.
By default, this registry is configured to access HMRC's production environment if the current installation is using a production licence, and sandbox environment otherwise.
You can override this behaviour by overriding the registry and setting the value explicitly. If you override this registry, make sure to confirm the MTD Client ID and MTD Client secret registry items are configured to use the correct credentials.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						Env.Instance.IsProductionSystem);
				});
			}
		}

		public StringRegistryItem MTDClientID
		{
			get
			{
				return GetItem("MTDClientID", delegate
				{
					return new StringRegistryItem(
						"MTDClientID",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"MTD Client ID (CargoWiseOne Support Only)",
						(NoResString)@"HMRC uses client ID to identify CW1 during each API call.
By default, this registry is configured to use the Production Client ID when the ""Is MTD in Production Mode"" Registry is 'Yes'. Else, its configured to use the Sandbox Client ID.
If you have overriden the ""Is MTD in Production Mode"", ensure that the correct Client ID is being used.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						IsMTDProductionMode.Value ? "CGSgE2X6EyCrgfVHHFi5dUOVxcEa" : "dnTztKp2F1nNvEhHuR8mxyiWOYMa");
				});
			}
		}

		public StringRegistryItem MTDClientSecret
		{
			get
			{
				return GetItem("MTDClientSecret", delegate
				{
					var result = new StringRegistryItem(
						"MTDClientSecret",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"MTD Client Secret (CargoWiseOne Support Only)",
						(NoResString)@"HMRC uses Client Secret to identify CW1 during each API call.
By default, this registry is configured to use the Production Client Secret when the ""Is MTD in Production Mode"" Registry is 'Yes'. Else, its configured to use the Sandbox Client Secret.
If you have overriden the ""Is MTD in Production Mode"", ensure that the correct Client Secret is being used.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						IsMTDProductionMode.Value ? "e867b76e-c2e1-4879-84ad-4cb8c060c7b1" : "b5d69a47-ce66-45cf-b410-97a84248ebdd")
					{
						EditorInfo = new TextRegistryEditorInfo(TextEditorType.Password),
					};
					return result;
				});
			}
		}
		public StringRegistryItem MTDProductionWebServiceUrl
		{
			get
			{
				return GetItem("MTDProductionWebServiceUrl", delegate
				{
					return new StringRegistryItem(
						"MTDProductionWebServiceUrl",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"MTD Production Web Service Url (CargoWiseOne Support Only)",
						(NoResString)"Kindly enter the Url to access UK HMRC's MTD for VAT API endpoints in production mode",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"https://api.service.hmrc.gov.uk");
				});
			}
		}

		public StringRegistryItem MTDTestWebServiceUrl
		{
			get
			{
				return GetItem("MTDTestWebServiceUrl", delegate
				{
					return new StringRegistryItem(
						"MTDTestWebServiceUrl",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"MTD Test Web Service Url (CargoWiseOne Support Only)",
						(NoResString)"Kindly enter the Url to access UK HMRC's MTD for VAT API endpoints in test mode",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"https://test-api.service.hmrc.gov.uk");
				});
			}
		}

		public StringRegistryItem MTDScenarioToSimulateForObligationRequest
		{
			get
			{
				return GetItem("MTDScenarioToSimulateForObligationRequest", delegate
				{
					return new StringRegistryItem(
						"MTDScenarioToSimulateForObligationRequest",
						AccChargeCodeRegistry.Categories.Accounting_TaxConfigurations,
						(NoResString)"MTD Scenario to simulate for Obligation request(CargoWiseOne Support Only)",
						(NoResString)"Kindly enter the key of the scenario that you want to simulate when the 'Obligations' request is sent to UK HMRC's MTD for VAT API in test mode. List of valid keys is available here: https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0#_retrieve-vat-obligations_get_accordion",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						string.Empty);
				});
			}
		}

		public List<IRegistryItem> GetRegistryItemsByCategoryName(params string[] categoryName)
		{
			List<IRegistryItem> items = new List<IRegistryItem>();

			foreach (var item in GetAllItems())
			{
				if (categoryName.Contains(item.Category))
				{
					items.Add(item);
				}
			}

			return items;
		}

		public GuidRegistryItem ContainerYardJobsDefaultDept
		{
			get
			{
				return GetItem("ContainerYardJobs", delegate
				{
					var containerYardJobsDepartment = new Guid("E64F8690-E35E-40B7-8FEE-D0D873970975");
					var result = new GuidRegistryItem("ContainerYardJobs",
						Categories.Accounting_JobInvoicing_DefaultDepartments_ContainerYardJobs,
						ResString.GetMultilingualString("9B3B4AAF-BB44-4E20-9FBA-B18290C2254C", "Container Yard Jobs"),
						ResString.GetMultilingualString("419990A8-4DCC-49E3-853C-7A57FBCBB71D", @"When creating an Accounting Job Header on Container Yard Jobs, {0} will use this registry to determine the Job Header’s Department.

Note: This is NOT used when the 'Default to Current Login Department' Registry option has been enabled.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						containerYardJobsDepartment);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbDepartment, RegistryFindBoxFilter.NonMiscDepartment);
					return result;
				});
			}
		}

		public IntRegistryItem MaximumNumberOfInvoicesAllowedOnJob
		{
			get
			{
				return GetItem("MaximumNumberOfInvoicesAllowedOnJob", delegate
				{
					return new IntRegistryItem(
						"MaximumNumberOfInvoicesAllowedOnJob",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("1F6DA2B1-5240-44AF-A92F-B64AB2C5F623", "Maximum Number of Invoices Allowed on Job"),
						ResString.GetMultilingualString("46868A8A-0A6F-4E78-B66C-D10D8FCC18B8", "This registry is used to configure the maximum number of invoices allowed on the Job."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport,
						703, 0, MaxNumberRepresentation);
				});
			}
		}

		public BooleanRegistryItem DisplayAccumulativeTotalAmountsInMultipageInvoices
		{
			get
			{
				return GetItem("DisplayAccumulativeTotalAmountsInMultipageInvoices",
						delegate
						{
							return new BooleanRegistryItem("DisplayAccumulativeTotalAmountsInMultipageInvoices",
								Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
								ResString.GetMultilingualString("4c2e1221-9eaf-4059-b6f3-42498288f972", "Include Carry Over Amounts in Invoices that have Multiple Pages"),
								ResString.GetMultilingualString("f3b7eead-9433-4479-a6c3-b8bc9cad9427", @"Set this registry to 'YES', to add carry over amounts to the invoice body header and footer of invoices that are more than one page long.

A subtotal of charges will be printed on the footer of each page of the invoice, except the last page of the invoice.

The invoice carry forward subtotal from the footer on one page will be brought forward to the header of the next page."),
								RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default, false);
						});
			}
		}

		public BooleanRegistryItem PrintQuanityInInvoiceDocument
		{
			get
			{
				return GetItem("PrintQuanityInInvoiceDocument",
						delegate
						{
							return new BooleanRegistryItem("PrintQuanityInInvoiceDocument",
								Categories.Accounting_ReceivableDefaults_FormConfigurations_Invoice,
								ResString.GetMultilingualString("409fa98d-dd0a-4839-9ee3-060e92bd4ec4", "Print quantity in AR invoice charge lines"),
								ResString.GetMultilingualString("15726066-0edd-4933-82cd-1be627a6f915", @"This registry controls the ability to include a quantity per charge line in printed 'DocBuilder' invoices and credit notes.

Set this registry to 'Yes' to include a quantity column in your printed invoices and credit notes.

Note: The quantity per charge line will only print when no summarization or roll up options are configured for printing invoices including periodic and warehouse periodic invoices."),
								RegistryStorageFlags.Company, RegistryOptions.Default, false);
						});
			}
		}

		#region Share Sequential Invoice Reference Numbers

		public ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem ShareSequentialARComplianceDocumentsReferenceNumbers
		{
			get
			{
				return GetItem("ShareSequentialARComplianceDocumentsReferenceNumbers", delegate
				{
					return new ShareSequentialARComplianceDocumentsReferenceNumbersRegistryItem(
						"ShareSequentialARComplianceDocumentsReferenceNumbers",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						(NoResString)"Share Sequential Internal Reference For A/R Compliance Documents",
						(NoResString)@"This registry is only relevant to system companies with the new Compliance Document Module enabled.
	Use this registry to control how Internal References are allocated to A/R Invoice and Credit Note Compliance Documents.
	By default, each transaction type (INV and CRD) will have its own separate number sequence.
	When this registry is set to 'Yes', A/R Invoice and Credit Note Compliance Documents will all be assigned a sequential reference number from a single number sequence.

	Important Note: A change in the registry value could result in two compliance document having the same Internal Reference Number. Please consult the Accounting Product team before a change is made. 

	For instance, the registry was originally set to 'Yes' and a CRD compliance document has been saved with Internal Reference 00001000.
	If the registry is changed to 'No', the next CRD compliance document will be assigned Internal Reference 00001000 and trigger the unique constraint error.",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		#endregion

		#region Original Invoice Details Mandatory On Credit Notes

		public BooleanRegistryItem OriginalInvoiceDetailsMandatoryOnARCreditNotes
		{
			get
			{
				return GetItem(nameof(OriginalInvoiceDetailsMandatoryOnARCreditNotes),
						delegate
						{
							return new BooleanRegistryItem(nameof(OriginalInvoiceDetailsMandatoryOnARCreditNotes),
								Categories.Accounting_ReceivableDefaults_DefaultSettings,
								ResString.GetMultilingualString("C582ED6B-947D-4F9D-AC6C-00894D755F8C", "Original Invoice Details Mandatory on AR Credit Notes"),
								ResString.GetMultilingualString("135764B7-6868-434B-B94E-69FD20A5B916", @"This registry allows you to enforce that users must enter the Original Invoice details when creating new Credit Notes.
By default, this registry is set to No and the Original Invoice details are not mandatory on Credit Notes.
When you set this registry to Yes, when adding new Credit Notes in the Receivables Transactions module, the users must either select the Original Invoice Reference or manually enter the Original Invoice Number and the Original Invoice Date."),
								RegistryStorageFlags.System | RegistryStorageFlags.Company,
								RegistryOptions.Default,
								false);
						});
			}
		}

		public BooleanRegistryItem OriginalInvoiceDetailsMandatoryOnARDebitNotes
		{
			get
			{
				return GetItem(nameof(OriginalInvoiceDetailsMandatoryOnARDebitNotes),
						delegate
						{
							return new BooleanRegistryItem(nameof(OriginalInvoiceDetailsMandatoryOnARDebitNotes),
								Categories.Accounting_ReceivableDefaults_DefaultSettings,
								ResString.GetMultilingualString("dc7d7494-de60-45bb-a8f2-0471f6e21e7f", "Original Invoice Details Mandatory on AR Debit Notes"),
								ResString.GetMultilingualString("2af1fcdc-3f73-49cf-b8a4-b622f55d71a7", @"This registry allows you to enforce that users must enter the Original Invoice details when creating new Debit Notes.
By default, this registry is set to No and the Original Invoice details are not mandatory on Debit Notes.
When you set this registry to Yes, when adding new Debit Notes in the Receivables Transactions module, the users must either select the Original Invoice Reference or manually enter the Original Invoice Number and the Original Invoice Date."),
								RegistryStorageFlags.System | RegistryStorageFlags.Company,
								RegistryOptions.Default,
								false);
						});
			}
		}

		public BooleanRegistryItem OriginalInvoiceDetailsMandatoryOnAPCreditNotes
		{
			get
			{
				return GetItem(nameof(OriginalInvoiceDetailsMandatoryOnAPCreditNotes),
						delegate
						{
							return new BooleanRegistryItem(nameof(OriginalInvoiceDetailsMandatoryOnAPCreditNotes),
								Categories.Accounting_PayableDefaults_DefaultSettings,
								ResString.GetMultilingualString("22091695-6F7C-46B3-A448-68D1DA23E3ED", "Original Invoice Details Mandatory on AP Credit Notes"),
								ResString.GetMultilingualString("3B32CD47-848B-49AB-9B9A-5C7D8D05E6BC", @"This registry allows you to enforce that users must enter the Original Invoice details when creating new Credit Notes.
By default, this registry is set to No and the Original Invoice details are not mandatory on Credit Notes.
When you set this registry to Yes, when adding new Credit Notes in the Payables Transactions module, the users must either select the Original Invoice Reference or manually enter the Original Invoice Number and the Original Invoice Date."),
								RegistryStorageFlags.System | RegistryStorageFlags.Company,
								RegistryOptions.Default,
								false);
						});
			}
		}

		#endregion

		#region Fixed Place of Supply Configuration

		public BooleanRegistryItem EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions
		{
			get
			{
				return GetItem("EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions", delegate
				{
					return new BooleanRegistryItem(
						new CountryEnabledBooleanRegistryItemImpl("EnforcePostingAtFixedPlaceOfSupplyLevelForReceivableTransactions",
						Categories.Accounting_FixedPlaceOfSupplyConfiguration,
						ResString.GetMultilingualString("bf5b304d-17b5-4edd-a1cc-770dee0743ed", "Enforce Posting at Fixed Place of Supply level (Receivable)"),
						ResString.GetMultilingualString("b600b1fa-826a-4f12-b807-5acba5f0c7f1", @"This registry affects the posting behaviors of Receivables INV, CRD, ADJ and Cash Book DRC transactions only.
By default this registry is set to No and charge lines with any mix of Fixed Place of Supply are permitted within each transaction.
You should set this registry to NO when tax rules do not require you to post separate invoices for a mix of places of supply within the invoice.
You should set this registry to YES, when you are required to post separate invoices for a mix of places of supply in the invoice.
When set to YES:
- Posting will not allow mixing different places of supply within a single transaction.
- Transaction lines for separate places of supply will post in separate transactions.
- You will not be able to select a separate Place of supply for a Transaction Line for a Non Job Related invoice."),
						RegistryStorageFlags.Company,
						Core.Constants.CountryCodes.India)
						);
				});
			}
		}

		public BooleanRegistryItem EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions
		{
			get
			{
				return GetItem("EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions", delegate
				{
					return new BooleanRegistryItem(
						new CountryEnabledBooleanRegistryItemImpl("EnforcePostingAtFixedPlaceOfSupplyLevelForPayableTransactions",
						Categories.Accounting_FixedPlaceOfSupplyConfiguration,
						ResString.GetMultilingualString("76f103c7-1e92-467a-b6f1-1b4ec63d9a07", "Enforce Posting at Fixed Place of Supply level (Payable)"),
						ResString.GetMultilingualString("de3c1562-93ff-4231-ae75-bbc4dfdc879d", @"This registry affects the posting behaviors of Payables INV, CRD, ADJ and Cash Book DPY transactions only.
By default this registry is set to No and charge lines with any mix of Fixed Place of Supply are permitted within each transaction.
You should set this registry to NO when tax rules do not require you to post separate invoices for a mix of places of supply within the invoice.
You should set this registry to YES, when you are required to post separate invoices for a mix of places of supply in the invoice.
When set to YES:
- Posting will not allow mixing different places of supply within a single transaction.
- Transaction lines for separate places of supply will post in separate transactions.
- You will not be able to select a separate Place of supply for a Transaction Line on a Non Job Related invoice."),
						RegistryStorageFlags.Company,
						Core.Constants.CountryCodes.India)
						);
				});
			}
		}

		#endregion

		public GuidRegistryItem SplitIntercompanyInvoiceTaxAmountIntoSeparateLine
		{
			get
			{
				return GetItem("SplitIntercompanyInvoiceTaxAmountIntoSeparateLine", delegate
				{
					var result = new GuidRegistryItem(
						(NoResString)"SplitIntercompanyInvoiceTaxAmountIntoSeparateLine",
						Categories.Accounting_PayableDefaults_DefaultSettings,
						ResString.GetMultilingualString("030a7202-7112-4fad-be15-e817d4019709", "Split Intercompany Invoice Tax Amount Into Separate Line"),
						ResString.GetMultilingualString("5a423952-a932-4910-b804-c6cd74686616", @"This feature is useful if you are want to reconcile the AR Invoice Ex Tax Amount (in the Issuing Company) to the AP Invoice Ex Tax Amount (in the Receiving Company).

When a charge code is specified, the Invoice's Tax Amount will be posted on a separate transaction line with this charge code when intercompany invoices received from a sister company located in different country/region are imported.

Note: This charge code must be 'NON' or 'OVR' charge type."),
						RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.NonJobRelatedChargeCode);

					result.OnBuildLogReference += (args) =>
					{
						var factory = new BusinessObjectFactory();
						var originalChargeCode = factory.Load<AccChargeCode>(new ZGuid(args.OriginalValue));
						var newChargeCode = factory.Load<AccChargeCode>(new ZGuid(args.NewValue));
						return Res.GetString("35f3b543-f3bb-438b-8e7b-3220a2f2f49a", "Registry value changed from [{0}] to [{1}].", originalChargeCode?.AC_Code ?? ZString.Empty, newChargeCode?.AC_Code ?? ZString.Empty);
					};

					return result;
				});
			}
		}

		#region CommentChargeLineARInvoiceWarning

		public CodePairRegistryItem CommentChargeLineARInvoiceWarning
		{
			get
			{
				return GetItem("CommentChargeLineARInvoiceWarning", delegate
				{
					return new CodePairRegistryItem(
						new CommentChargeLineARInvoiceWarningImpl(
						"CommentChargeLineARInvoiceWarning",
						Categories.Accounting_ReceivableDefaults_DefaultSettings,
						(NoResString)"Comment Charge Line Validation",
						(NoResString)@"Set the severity level relating to the Comment Charge on a Receivables Invoice. 
The default value for most login companies with be NON - No Action, meaning there will be no warning or error on validation of the Comment charge line.
Countries where the Comment charge line cannot be included in the transaction XML exported as part of the electronic invoicing requirements will set this to WRN (Warning Validation) or ERR (Error Validation).",
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport)
						);
				});
			}
		}

		class CommentChargeLineARInvoiceWarningImpl : RegistryItemImpl
		{
			public CommentChargeLineARInvoiceWarningImpl(string name, MultilingualString category, NoResString caption, NoResString hint, RegistryStorageFlags storage, RegistryOptions option)
				: base(name, category, caption, hint, new CodePairRegistryDataType(new CodeDescriptionPairListProvider(() => AccountingConstants.CommentChargeLineARInvoiceWarningOptions.CodeList), false, true), storage, option)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var company = companyPK == GlbCompany.CurrentCompany.PK ? GlbCompany.CurrentCompany : new BusinessObjectFactory().Load<GlbCompany>(companyPK);
				return company?.GC_RN_NKCountryCode.ToString() == Constants.CountryCodes.Turkey
					? AccountingConstants.CommentChargeLineARInvoiceWarningOptions.WarningValidation
					: AccountingConstants.CommentChargeLineARInvoiceWarningOptions.NoAction;
			}
		}

		#endregion

		#region Note GL Account

		public BooleanRegistryItem EnablePostDateGLJournal =>
			GetItem("EnablePostDateGLJournal", () =>
				new BooleanRegistryItem("EnablePostDateGLJournal",
					Categories.Accounting_GeneralLedgerDefaults,
					ResString.GetMultilingualString("456334E1-F245-4F53-B975-19B710253988", "Enable Post Date in GL Journal Entry"),
					ResString.GetMultilingualString("DB3AFB98-5484-46DB-88B7-7A2472505CD5", @"This feature allows you to specify the Post Date in General Ledger Journals.
This is allowed in the journal types 'GJL - General Journal', 'NJL - Note Journal' and 'RJL - Reversing Journals'. This functionality is not available in the journal type 'AJL - Automatic Journal'.

When this registry is set to 'No', the Post Date is not visible and users can only set the Post Period.
The standard behavior in CargoWise is for the 'Post Date' to be set to the end of the selected period. Your periods are defined in Manage > General Ledger > Period Management.
The Period Management function called 'Edit Period End Date' automatically updates the Post Date of relevant Journals whenever the Period End Date is modified. This keeps the Journal Post Date within the intended period range.

When this registry is set to 'Yes', the Post Date can be explicitly set by the user (as can Reverse Date for RJL Journals).
The default values for Post Dates are retained when the user sets the Period value (i.e. End Date of that period), but the user can explicitly override the default value to a specific date within the month.
A key difference here is that CargoWise will prevent users from editing the Period End Date if a Journal's (GJL/RJL/NJL only) Post Date overlaps with the old and new dates."),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					false));

		public CodeDescriptionPairListRegistryItem NoteGLAccountsStatisticalUnitsofMeasurement
		{
			get
			{
				return GetItem("NoteGLAccountsStatisticalUnitsofMeasurement", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"NoteGLAccountsStatisticalUnitsofMeasurement",
						Categories.Accounting_GeneralLedgerDefaults,
						ResString.GetMultilingualString("4ED93AA4-6943-45A1-9C92-8C84CD4EC612", "Note GL Account's Statistical Units of Measurement"),
						ResString.GetMultilingualString("4714C729-4AC9-4930-9706-6F92A01450D1", @"CargoWiseOne supports the recording of non-financial 'NTE - Note' general ledger journals (i.e. statistical purpose).

This journal type enables you to record Total Carbon Emissions, Total TEU, Total M3, Total Tonnage, etc. and present them in the general ledger reports, using 'NTE - Note' GL Accounts.
Each 'NTE - Note' GL Account will require you to specify the statistical unit of measurement.

This registry enables you to define the list of statistical units that will be available for selection when creating 'NTE - Note' GL Account in Manage > Account > GL Accounts.
Below are couple of statistical units that have been defaulted to give you an idea of the statistical units that you can create.
You can re-configure the list below according to your needs."),
						3,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DefaultStatisticalUnits
						);
				});
			}
		}

		CodeDescriptionPairList fDefaultStatisticalUnits;
		CodeDescriptionPairList DefaultStatisticalUnits
		{
			get
			{
				if (fDefaultStatisticalUnits == null)
				{
					fDefaultStatisticalUnits = new CodeDescriptionPairList();
					fDefaultStatisticalUnits.AddPair("KWH", ResString.GetMultilingualString("D5F0AA76-5BCD-4E5D-992A-47B074D9EC2C", "Kilowatt Hours"));
					fDefaultStatisticalUnits.AddPair("KG", ResString.GetMultilingualString("D82AC39C-795B-457E-B9F6-628ABA13BBE0", "Kilograms"));
					fDefaultStatisticalUnits.AddPair("TON", ResString.GetMultilingualString("0AB7C2A4-DDCC-448F-9B49-F9F64EE855E3", "Ton"));
					fDefaultStatisticalUnits.AddPair("TEU", ResString.GetMultilingualString("931587C1-7051-41E6-B9C8-D50F4F5FD716", "Twenty-foot Equivalent Unit"));
					fDefaultStatisticalUnits.AddPair("HCT", ResString.GetMultilingualString("1ADA71E1-A33C-486F-AA92-7274334F8070", "Headcount"));
				}

				return fDefaultStatisticalUnits;
			}
		}

		#endregion

		public BooleanRegistryItem EnableBulkDisbursementJobsClosure
		{
			get
			{
				return GetItem("EnableBulkDisbursementJobsClosure", () =>
				{
					return new BooleanRegistryItem("EnableBulkDisbursementJobsClosure",
						Categories.Accounting_JobInvoicing_DisbursementChargeManagement,
						(NoResString)"Enable Bulk Disbursement Jobs Closure (CargoWiseOne Support Only)",
						(NoResString)@"This is a temporary registry to hide the progressive feature change for bulk disbursement jobs closure feature.
This temporary registry will be removed on completion of the feature changes.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public DisbursementJobsClosureConfigurationRegistryItem DisbursementJobsClosureConfiguration
		{
			get
			{
				return GetItem("DisbursementJobsClosureConfiguration", () =>
				{
					var result = new DisbursementJobsClosureConfigurationRegistryItem("DisbursementJobsClosureConfiguration",
						Categories.Accounting_JobInvoicing_DisbursementChargeManagement,
						ResString.GetMultilingualString("9AFA3455-90B0-4C60-9F1F-2FB4065DA772", "Disbursement Jobs Closure Configuration"),
						ResString.GetMultilingualString("F3ED338E-6F88-4496-9ABB-EB1263A2A2FF", @"This registry configuration affects the closure of jobs with a disbursement surplus/shortfall balance.

You can specify a job level and an aggregated level threshold.
The Job level threshold, when specified, will be used to evaluate if disbursement jobs should be included in Disbursement Job Close Batch. Jobs that do not meet the threshold will not be batched.
The Aggregated threshold, when specified, will be used to determine if the Disbursement Job Close Batch is subjected to approval or can be automatically close.

Note:
1. Disbursement surplus/shortfall is calculated based on the values of REV and CST line types.
2. If all values are zero, then disbursement clearing balance will not be taken into consideration during job closure."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						Instance.EnableBulkDisbursementJobsClosure.Value ? RegistryOptions.Default : RegistryOptions.IsHidden);
					result.OnBuildLogReference += BuildDisbursementJobsClosureConfigurationReference;
					return result;
				});
			}
		}

		string BuildDisbursementJobsClosureConfigurationReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var oldValue = args.OriginalValue as DisbursementJobsClosureConfiguration;
			var newValue = args.NewValue as DisbursementJobsClosureConfiguration;

			var surplusAggregated = newValue?.AggregatedLevelOfSurplusUpTo ?? 0;
			var shortfallAggregated = newValue?.AggregatedLevelOfShortfallUpTo ?? 0;

			var surplusJobLevel = newValue?.JobLevelOfSurplusUpTo ?? 0;
			var shortfallJobLevel = newValue?.JobLevelOfShortfallUpTo ?? 0;

			var aggregatedHasChange = (surplusAggregated - oldValue?.AggregatedLevelOfSurplusUpTo ?? 0) != 0 || (shortfallAggregated - oldValue?.AggregatedLevelOfShortfallUpTo ?? 0) != 0;
			var shortfallHasChange = (surplusJobLevel - oldValue?.JobLevelOfSurplusUpTo ?? 0) != 0 || (shortfallJobLevel - oldValue?.JobLevelOfShortfallUpTo ?? 0) != 0;

			var builder = new StringBuilder();

			if (shortfallHasChange)
			{
				builder.Append(Res.GetString("121F2B5D-2816-41fb-AE04-11A8B0D809EF", "Job Level: Shortfall {0} Surplus {1}", shortfallJobLevel, surplusJobLevel));
			}

			if (shortfallHasChange && aggregatedHasChange)
			{
				builder.Append(", ");
			}

			if (aggregatedHasChange)
			{
				builder.Append(Res.GetString("72CB9210-E7A5-4aba-8CB9-1E3E323E04AB", "Aggregate Level: Shortfall {0} Surplus {1}", shortfallAggregated, surplusAggregated));
			}

			return builder.ToString();
		}

		#region Job Ready for Financial Closure Feature

		public JobStatusUpdateRestrictionRuleRegistryItem JobStatusUpdateRestrictionRule
		{
			get
			{
				return GetItem("JobStatusUpdateRestrictionRule", delegate
				{
					var item = new JobStatusUpdateRestrictionRuleRegistryItem(
						"JobStatusUpdateRestrictionRule",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("520a4ea8-65cd-47dd-9189-287bd51e9927", "Job Status Update Restriction Rule"),
						ResString.GetMultilingualString("b97c3ca3-4ce1-4535-8e35-a8a172cfff95", @"This registry defines the Job Status Update Restriction Rules for transitioning of job status prior to job closure.

The grid below is pre-populated with the current job status update restriction rule.
If required, you can override the restriction rule and configure according to your internal control policy.

Each row represents the 'From' job status and the related security right to update the job status when restriction applies.
Each column represents the 'To' job status.
Each cell indicates if security right is required to update the job status.
If the value is set to 'No, then no restriction will be applied. All user will be able to update the job status to the specific 'To' job status.
If the value is set to ‘Yes’, then the login user will need to have the related security right to be able to update the job status to the specific 'To' job status.

Note:
The system will always enforce restriction when a job is closed (i.e. job status is changed from 'JFC' to 'CLS'). Login user must have the 'Close Single Job' or 'Close Multiple Jobs' security right depending on whether one or more jobs are closed at the same time.
The system will always enforce restriction when a job is reopened (i.e. job status is changed from 'CLS - Closed' to other job status). Login user must have the 'Reopen Jobs' or 'Allow Reopen Jobs Past Allowed Reopen Period' security right depending on whether re-open restriction is applicable.
Reopen restriction is defined under Auto Job Closure Configuration."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						JobStatusUpdateRestrictionRuleCollection.GetDefault(),
						RegistryOptions.Default);

					item.OnBuildLogReference += BuildJobStatusUpdateRestrictionRuleLogReference;

					return item;
				});
			}
		}

		string BuildJobStatusUpdateRestrictionRuleLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var result = string.Empty;

			var changeLogBuilder = new ZStringBuilder();
			var oldJobStatusUpdateRestrictionRuleCollection = ((JobStatusUpdateRestrictionRuleCollection)args.OriginalValue).Cast<JobStatusUpdateRestrictionRule>();
			var newJobStatusUpdateRestrictionRuleCollection = ((JobStatusUpdateRestrictionRuleCollection)args.NewValue).Cast<JobStatusUpdateRestrictionRule>();
			foreach (var newJobStatusUpdateRestrictionRule in newJobStatusUpdateRestrictionRuleCollection)
			{
				var changeLogList = new List<string>();
				var oldJobStatusUpdateRestrictionRule = oldJobStatusUpdateRestrictionRuleCollection.FirstOrDefault(x => x.JobStatus == newJobStatusUpdateRestrictionRule.JobStatus);
				if (oldJobStatusUpdateRestrictionRule != null)
				{
					newJobStatusUpdateRestrictionRule.Lookups.JobStatusList.GetAllCodes().ForEach(jobStatus => AddChangeLogItem(jobStatus));

					if (changeLogList.Any())
					{
						var changeLogItemBuilder = new ZStringBuilder();
						changeLogList.ForEach(changeLog => changeLogItemBuilder.Append(changeLog));
						changeLogBuilder.Append(Invariant($"{newJobStatusUpdateRestrictionRule.JobStatusDescription}: {changeLogItemBuilder.ToStringWithDelimiterBetweenAppends(", ")}"));
					}

					void AddChangeLogItem(string jobStatus)
					{
						var oldValue = oldJobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(jobStatus);
						var newValue = newJobStatusUpdateRestrictionRule.GetRestricteValueFromJobStatus(jobStatus);
						if (oldValue != newValue)
						{
							changeLogList.Add(Res.GetString("5724d175-7e6d-4ed6-aadb-c0e6f5f0cf7c", "{0} changed from [{1}] to [{2}]", jobStatus, oldValue, newValue));
						}
					}
				}
			}

			return changeLogBuilder.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
		}

		public BooleanRegistryItem AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC
		{
			get
			{
				var item = GetItem("AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC", () =>
				{
					var result = new BooleanRegistryItem("AllowPostingAPInvoiceRelativeToExistingDSBChargeAgainstJFC",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("320EB607-ADBC-41AD-852D-C25BCFE914FC", "Allow Posting of Payables Invoices Related to Existing Disbursement Charge against Jobs Ready for Financial Closure"),
						ResString.GetMultilingualString("034B3E51-A634-475E-8237-A40CBCDA5498", @"By default, this registry will be set to 'No' in which case users who do not have the ‘Allow Posting Charges of Ready for Financial Closure Job’ security right will not be able to post costs against jobs with Ready for Financial Closure (“JFC”) status.

When this registry is set to 'Yes', all users will be able to post Payables Invoices/Credit Notes against JFC jobs when the following conditions are fulfilled:
1. The charge type is Disbursement (“DSB”). 
2. The charge is imported."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false);

					result.OnBuildLogReference += (args) =>
					{
						return Res.GetString("AE2EED7E-3182-4FC1-B791-7D76FCB58C8D", "Registry has been set to [{1}] from [{0}].", args.OriginalValue, args.NewValue);
					};

					return result;
				});
				return item;
			}
		}

		#endregion

		#region Auto Period Closure

		public GuidRegistryItem AutoPeriodClosureNotifyGroup
		{
			get
			{
				return GetItem("AutoPeriodClosureNotifyGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"AutoPeriodClosureNotifyGroup",
						Categories.Accounting_EmailNotification,
						ResString.GetMultilingualString("319b4b07-7c0f-4ce6-8faa-769b8fd62dd7", "Auto Period Closure Notify Group"),
						ResString.GetMultilingualString("ad99e920-a000-4e03-a99f-6596c56aed49", "Notify Party when a validation error prevent a Period Ledger Type from being closed."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					result.OnBuildLogReference += BuildAutoPeriodClosureNotifyGroupLogReference;
					return result;
				});
			}
		}

		string BuildAutoPeriodClosureNotifyGroupLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var group = factory.Load<GlbGroup>(new ZGuid(args.NewValue));
			return Res.GetString("175bb9ac-719f-484e-84b5-6daa8b5c550c", "Notify Group set to '{0}'", group?.GG_Code);
		}

		public PeriodClosureConfigurationRegistryItem AutoPeriodClosureConfiguration
		{
			get
			{
				return GetItem("AutoPeriodClosureConfiguration", delegate
				{
					var result = new PeriodClosureConfigurationRegistryItem(
											"AutoPeriodClosureConfiguration",
											Categories.Accounting,
											ResString.GetMultilingualString("f36b1215-1299-44de-bfb8-4ad72028d8e8", "Auto Period Closure Configuration"),
											ResString.GetMultilingualString("8fb8b214-7866-4a13-9ec4-737d991e52f4", @"This registry enables you to configure the auto closure of period by ledger types after a specified time interval from the relevant period's end date. 
You can configure the system to auto close: 
· All ledger types.
· Sub ledger and general ledger only.
· Sub ledger only.
You can specify the interval in minutes, hours, or days from the relevant period's end date. 

Example:
Assume the relevant period's end date is 31-Mar-21 and the interval has been specified as follows:
· Sub ledger = 3 days.
· General ledger = 5 days.
· Adjustment ledger = 0 day.
The sub ledger will be auto closed on 3-Apr-21 and the general ledger will be auto closed on 5-Apr-21. The adjustment ledger will remain open. 

Note: 
1. The above configuration will be processed by the 'PCS - Period Closure Service Task'.
2. Period for general ledger type can only be closed after sub ledger type. Likewise, the adjustment ledger type can only be closed after the general ledger type has been closed. Thus, the interval setting for general ledger type must be the same or greater than the sub ledger, and the interval setting of the adjustment ledger type  must be the same or greater than the general ledger. An interval setting of 0 means that period will not be auto closed.
3. In the event a ledger type cannot be auto closed due to validation error, an email notification will be sent to the notify party specified in the Accounting > Email Notification > Auto Period Closure Notify Group system registry. "),
											RegistryStorageFlags.System | RegistryStorageFlags.Company,
											RegistryOptions.Default,
											new PeriodClosureConfiguration(AccountingUtils.PeriodClosureConfigurationIntervalType.Minutes));
					result.OnBuildLogReference += BuildAutoPeriodClosureConfigurationLogReference;
					return result;
				});
			}
		}

		string BuildAutoPeriodClosureConfigurationLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var config = args.NewValue as PeriodClosureConfiguration;
			return Res.GetString("1ad54cec-3fd9-4534-96ad-c79bd07a0301", "Interval Type: {0}, Sub Ledger: {1}, General Ledger: {2}, Adjustment Ledger: {3}", config.IntervalType, config.SubLedgerInterval, config.GeneralLedgerInterval, config.AdjustmentLedgerInterval);
		}

		#endregion

		#region Compliance Settings

		public BooleanRegistryItem SuppressWarningWhenThereIsNoComplianceBookSetups
		{
			get
			{
				return GetItem("SuppressWarningWhenThereIsNoComplianceBookSetups", delegate
				{
					return new BooleanRegistryItem(
						"SuppressWarningWhenThereIsNoComplianceBookSetups",
						Categories.Accounting_GovernmentComplianceInvoiceDocument,
						ResString.GetMultilingualString("885E3D86-174C-43C8-BBB7-91B1C65E307F",
								"Suppress Warning when there is no Compliance Book Setups"),
						ResString.GetMultilingualString("71B3112D-8AAC-4749-962C-940A33818212",
								@"By default CargoWise will warn user when they attempt to use Compliance Numbers but have not configured an applicable Compliance Book. You can suppress this warning message by setting this registry value to 'Yes'."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Tax Branch Rules

		public BooleanRegistryItem CustomJobTaxBranchDefaultingRulesEngineConfiguration
		{
			get
			{
				return GetItem("CustomJobTaxBranchDefaultingRulesEngineConfiguration", delegate
				{
					var item = new BooleanRegistryItem(
						"CustomJobTaxBranchDefaultingRulesEngineConfiguration",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("70d2bfc5-cc91-4e9a-949a-929e057fd3e2", "Custom Job Tax Branch Defaulting Rules Engine Configuration"),
						ResString.GetMultilingualString("dc7013ba-0eb7-4f70-99d3-1fbd2cc26979", @"This registry will enable setting up Rules for Job Tax Branch Defaulting in Rules Engine."),
						RegistryStorageFlags.System,
						AccountingMasterFilesRegistry.Instance.EnableTaxBranchFeature.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						false);

					item.OnBuildLogReference += (args) => Res.GetString("E437B15B-B009-4AEB-8460-F62003F677B3", "Registry value changed from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

					return item;
				});
			}
		}

		#endregion

		#region Supply Type Configuration by Charge Group

		public SupplyTypeConfigurationByChargeGroupRegistryItem SupplyTypeConfigurationByChargeGroup
		{
			get
			{
				var result = GetItem("SupplyTypeConfigurationByChargeGroup", delegate
				{
					var defaultCollection = new SupplyTypeConfigurationByChargeGroupCollection();
					var chargeCodeGroups = new ChargeCodeGroupList();
					foreach (CodeDescriptionPair group in chargeCodeGroups)
					{
						var defaultValue = defaultCollection.AddNew();
						defaultValue.ChargeGroup = group.Code;
						defaultValue.ChargeGroupDescription = group.MultilingualDescription;
					}
					return new SupplyTypeConfigurationByChargeGroupRegistryItem(
											"SupplyTypeConfigurationByChargeGroup",
											Categories.Accounting_TaxConfigurations_SupplyType,
											ResString.GetMultilingualString("31813CCA-C60D-46C2-B267-50AF11BBBAF8", "Supply Type Configuration By Charge Group"),
											ResString.GetMultilingualString("6AD187F0-62A5-439D-AE0D-5BCF49953AAF", @"This registry is used to configure Supply Type Defaulting Behavior by Charge Code Rating Group."),
											RegistryStorageFlags.Company,
											AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
											defaultCollection);
				});

				result.OnBuildLogReference += BuildSupplyTypeConfigurationChangeLogReference;
				return result;
			}
		}

		string BuildSupplyTypeConfigurationChangeLogReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			string result = "";
			var newConfigurationCollection = ((SupplyTypeConfigurationByChargeGroupCollection)args.NewValue).Cast<SupplyTypeConfigurationByChargeGroup>();
			var originalConfigurationCollection = ((SupplyTypeConfigurationByChargeGroupCollection)args.OriginalValue).Cast<SupplyTypeConfigurationByChargeGroup>();

			foreach (var configuration in originalConfigurationCollection)
			{
				var configurationSettings = configuration.ChargeGroupSettings;
				foreach (SupplyTypeConfiguration configurationSetting in configurationSettings)
				{
					var newConfigurationCollectionSettingInSameGroup = newConfigurationCollection.FirstOrDefault(x => x.ChargeGroup == configuration.ChargeGroup).ChargeGroupSettings.Cast<SupplyTypeConfiguration>();
					if (!newConfigurationCollectionSettingInSameGroup.Any(x => x.JobType == configurationSetting.JobType && x.DirectionCode == configurationSetting.DirectionCode && x.Mode == configurationSetting.Mode && x.Incoterm == configurationSetting.Incoterm))
					{
						result += GetSupplyTypeConfigurationChangeLog((NoResString)"Deleted", configuration.ChargeGroup, configurationSetting);
					}
				}
			}

			foreach (var newConfiguration in newConfigurationCollection)
			{
				var newConfigurationSettings = newConfiguration.ChargeGroupSettings;

				foreach (SupplyTypeConfiguration newConfigurationSetting in newConfigurationSettings)
				{
					var originalConfigurationCollectionSettingInSameGroup = originalConfigurationCollection.FirstOrDefault(x => x.ChargeGroup == newConfiguration.ChargeGroup).ChargeGroupSettings.Cast<SupplyTypeConfiguration>();

					if (!originalConfigurationCollectionSettingInSameGroup.Any(x => x.JobType == newConfigurationSetting.JobType && x.DirectionCode == newConfigurationSetting.DirectionCode && x.Mode == newConfigurationSetting.Mode && x.Incoterm == newConfigurationSetting.Incoterm))
					{
						result += GetSupplyTypeConfigurationChangeLog((NoResString)"Added", newConfiguration.ChargeGroup, newConfigurationSetting);
					}

					if (originalConfigurationCollectionSettingInSameGroup.Any(x => x.JobType == newConfigurationSetting.JobType && x.DirectionCode == newConfigurationSetting.DirectionCode && x.Mode == newConfigurationSetting.Mode && x.Incoterm == newConfigurationSetting.Incoterm && (x.LineDepartmentPK != newConfigurationSetting.LineDepartmentPK || x.SupplyType != newConfigurationSetting.SupplyType)))
					{
						var originConfigurationSetting = originalConfigurationCollectionSettingInSameGroup.FirstOrDefault(x => x.JobType == newConfigurationSetting.JobType && x.DirectionCode == newConfigurationSetting.DirectionCode && x.Mode == newConfigurationSetting.Mode && x.Incoterm == newConfigurationSetting.Incoterm && (x.LineDepartmentPK != newConfigurationSetting.LineDepartmentPK || x.SupplyType != newConfigurationSetting.SupplyType));

						result += GetSupplyTypeConfigurationChangeLog((NoResString)"Deleted", newConfiguration.ChargeGroup, originConfigurationSetting);
						result += GetSupplyTypeConfigurationChangeLog((NoResString)"Added", newConfiguration.ChargeGroup, newConfigurationSetting);
					}
				}
			}

			return result;
		}

		string GetSupplyTypeConfigurationChangeLog(string changeType, ZString chargeGroup, SupplyTypeConfiguration configuration)
		{
			return Res.GetString("8923E1A8-489A-4FE1-9AC2-0E035754DD4D", "{0}: Charge Group: {1}, Job Type:{2}, Direction:{3}, Transport Mode:{4}, Incoterm:{5}, Line Department:{6}, Supply Type: {7}", changeType, chargeGroup, configuration.JobType, configuration.DirectionCode, configuration.Mode, configuration.Incoterm, configuration.LineDepartment?.GE_Code ?? ZString.Empty, configuration.SupplyType) + "\r\n";
		}

		public BooleanRegistryItem AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB
		{
			get
			{
				var item = GetItem("AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB",
						delegate
						{
							return new BooleanRegistryItem("AlwaysSetSupplyTypetoDSBIfChargeTypeIsDSB",
								Categories.Accounting_TaxConfigurations_SupplyType,
								ResString.GetMultilingualString("5503CC37-0EF9-4823-8680-D6D30F6C3F66", "Always set Supply Type to 'DSB' if Charge Type is 'DSB'"),
								ResString.GetMultilingualString("4DF97741-EF3A-4D4A-8FDA-86C3FD23F7D5", @"When this registry is set to 'Yes', the system will always set the charge's supply type to 'DSB' where the charge type of job charge is 'DSB'."),
								RegistryStorageFlags.Company,
								AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
								false);
						});

				item.OnBuildLogReference += (args) => Res.GetString("04B90760-2B73-425F-BEF7-9B28DF7E7C9A", "Registry value change from [{0}] to [{1}].", args.OriginalValue, args.NewValue);

				return item;
			}
		}

		#endregion

		#region E-Invoicing Registries (Mexico specific)

		public CodeDescriptionPairListRegistryItem MexicoEInvoicingReversalStatusCode
		{
			get
			{
				var reversalStatusCodeList = new CodeDescriptionPairList();
				reversalStatusCodeList.AddPair("02", ResString.GetMultilingualString("5DDD8568-F7E9-403E-A523-756A4A883E79", "Transactions issued with unrelated errors"));
				reversalStatusCodeList.AddPair("03", ResString.GetMultilingualString("A925E9C2-C33B-43FC-ADB9-91C997974E4F", "The operation was not carried out"));

				return GetItem(nameof(MexicoEInvoicingReversalStatusCode), () =>
				new CodeDescriptionPairListRegistryItem(nameof(MexicoEInvoicingReversalStatusCode),
					Categories.Accounting_EReportingAndEInvoicingConfigurations_Mexico,
					(NoResString)"Mexico Cancelations Reversal Status Code (CargoWiseOne Support Only)",
					(NoResString)"This registry is used to configure valid reversal reason codes for Mexico Cancelations within the E-Invoicing module.",
					2,
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					reversalStatusCodeList));
			}
		}

		public MexicoNotificationRemainingFolioConfigurationRegistryItem MexicoRemainingFolioNotification
		{
			get
			{
				return GetItem(nameof(MexicoRemainingFolioNotification),
					() => new MexicoNotificationRemainingFolioConfigurationRegistryItem(
						name: nameof(MexicoRemainingFolioNotification),
						category: Categories.Accounting_EReportingAndEInvoicingConfigurations_Mexico,
						caption: ResString.GetMultilingualString("E492FF6C-0458-4A22-A3F7-FA571FA5239E", "Mexico Remaining Quantity of 'Timbres/Folios' Notification"),
						hint: ResString.GetMultilingualString("6EE21C3D-1FFF-408E-9324-4D47666DDE10", @"This registry lets you define when you want to receive notification e-mails and reminders to alert you that the number of 'Folios/Timbres' to be used in E-Invoicing transactions is running low for your Mexico Login Company.

An e-mail notification will be sent to the group configured against the '{0}' registry when the number of available 'Folios/Timbres' reaches the value set in the Quantity of remaining 'Folios/Timbres' field of this registry.

After the first e-mail notification is triggered, reminder e-mails will be sent after a certain amount of timbres has been consumed.
Use the 'Interval' field to set how often you want to receive reminders after the first e-mail notification is triggered.

Example of use of this registry:
'Quantity of remaining Folios/Timbres' value: 100.
'Interval' value: 10.
The first notification email will be sent when 100 'timbres' are left to be used in the Electronic Invoicing process.
After that, a reminder e-mail will be sent each time 10 'timbres' are consumed.
Each reminder will contain the updated number of available 'timbres'.

The e-mail notifications will stop when a new pack of 'timbres' is assigned to the Login Company.

Note: The number of available 'timbres' is shown in the 'E-Reporting Error/ Warning/ Status' column of the Receivables Transactions and Consolidation modules and all operational jobs.", ((IMultilingualRegistryItem)EInvoicingErrorNotificationGroup).LocationMultilingual),
						storage: RegistryStorageFlags.Company,
						options: RegistryOptions.Default,
						defaultValue: new MexicoNotificationRemainingFolioConfiguration()
						{
							FoliosQuantity = 100,
							Interval = 10
						})
					{ CountryFilterPKs = CountryFilterPKs.Mexico }
					);
			}
		}

		#endregion

		#region E-Invoicing Registries (China specific)

		public EInvoicingReceivingFileTypeConfigurationRegistryItem ChinaEInvoicingReceivingFileType
		{
			get
			{
				var item = GetItem("ChinaEInvoicingReceivingFileType", delegate
				{
					return new EInvoicingReceivingFileTypeConfigurationRegistryItem("ChinaEInvoicingReceivingFileType",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_China,
						ResString.GetMultilingualString("9EB0BC72-85C4-4628-8F34-B87C64A1E176", "E-Invoicing Receiving File Type"),
						ResString.GetMultilingualString("B11A43E4-D641-424A-914A-98C8C4FD8876", @"This registry is relevant to China Login Company Only.
By default, the system only attaches a copy of the e-invoice in PDF format to the invoice's eDocs tab.
If required, you can specify debtors and/or debtor groups to also download copies of the e-invoice in OFD and/or XML format to the invoice's eDocs tab at the same time."),
						RegistryStorageFlags.Company,
						RegistryOptions.Default)
					{ CountryFilterPKs = CountryFilterPKs.China };
				});

				return item;
			}
		}

		public ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItem DoNotSendTheseInfomationForFullyDigitalizedEInvoice
		{
			get
			{
				var item = GetItem("DoNotSendTheseInfomationForFullyDigitalizedEInvoice", delegate
				{
					return new ExcludedFullyDigitalizedElectronicInvoiceDataRegistryItem(
						"DoNotSendTheseInfomationForFullyDigitalizedEInvoice",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_China,
						ResString.GetMultilingualString("7B368683-728D-40E9-B407-B064DA0E4022", "Do Not Send These Information For Fully Digitalized E-Invoice"),
						ResString.GetMultilingualString("A565FAE5-60BE-497D-B414-3F4DA1A23AD8", @"This registry enables you to exclude certain optional data from being transmitted in relation to Fully Digitalized Electronic Invoices.

By default, the 'Buyer's Address' will be excluded.
You can adjust the configuration as required."),
						RegistryStorageFlags.Company,
						new ExcludedFullyDigitalizedElectronicInvoiceData() { BuyerAddress = true });
				});
				item.CountryFilterPKs = CountryFilterPKs.China;
				item.OnBuildLogReference += BuildDoNotSendTheseInfomationForFullyDigitalizedEInvoice;

				return item;
			}
		}

		string BuildDoNotSendTheseInfomationForFullyDigitalizedEInvoice(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var newValue = args.NewValue as ExcludedFullyDigitalizedElectronicInvoiceData;

			return Res.GetString("45A6FFDD-001A-485A-9752-6223B45DE1C8", "Buyer's Address: {0}, Buyer's Phone Number: {1}, Buyer's Bank Account: {2}", CheckTicked(newValue.BuyerAddress), CheckTicked(newValue.BuyerPhoneNumber), CheckTicked(newValue.BuyerBankAccount));

			string CheckTicked(bool checkedValue)
			{
				return checkedValue ? Res.GetString("F3E0D5D1-A4A8-4A3E-BD5B-9358C2F94DDB", "Ticked") : Res.GetString("CCA5A30F-F499-409D-9FB2-B191F47D6C7A", "Un-ticked");
			}
		}

		public class DoNotQueueInvoicesContainingSpecificChargesForTransmissionRegistryItem : RegistryItemImpl
		{
			public DoNotQueueInvoicesContainingSpecificChargesForTransmissionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new GenericChargeConfigurationRegistryDataType(), storage, options)
			{
			}

			public override bool IsVisible(Guid companyPK, Guid branchPK, Guid departmentPK, IGlbDepartment department, IRegistryItemVisibility registryItemVisibility)
			{
				var factory = new ReadOnlyBusinessObjectFactory();
				var branch = factory.Load<GlbBranch>(branchPK);
				return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.China && (companyPK == GlbCompany.CurrentCompany.PK || branch?.Company.PK == GlbCompany.CurrentCompany.PK);
			}
		}

		public GenericChargeConfigurationRegistryItem DoNotQueueInvoicesContainingSpecificChargesForTransmission
		{
			get
			{
				var item = GetItem("DoNotQueueInvoicesContainingSpecificChargesForTransmission", delegate
				{
					return new GenericChargeConfigurationRegistryItem(
						new DoNotQueueInvoicesContainingSpecificChargesForTransmissionRegistryItem("DoNotQueueInvoicesContainingSpecificChargesForTransmission",
						Categories.Accounting_EReportingAndEInvoicingConfigurations_China,
						(NoResString)"Do Not Queue Invoices Containing Specific Charges For Transmission",
						(NoResString)@"By default, all eligible receivables invoices will be queued and transmitted to RongJin's Tax Easy portal for fapiao issuance.

With this registry configuration, we will allow clients to optionally choose not to queue and transmit invoices containing specific charges to Tax Easy portal due to confidentiality reason.
Instead, they will issue the relative fapiao directly through the E-Tax China portal and manually update the compliance number and date back into CargoWise.

Note:
1.	Please consult the Accounting Asia Product Team before sharing or enabling this feature.
2.	This registry is configurable at branch level for China Login Companies Only and will only show the Current Login Company's Branches only. Please login to the required China Login Company to adjust the configuration.",
						RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForSupport));
				});
				item.OnBuildLogReference += BuildDoNotQueueInvoicesContainingSpecificChargesForTransmissionReference;

				return item;
			}
		}

		string BuildDoNotQueueInvoicesContainingSpecificChargesForTransmissionReference(RegistryItemWrapper.BuildLogReferenceArgs args)
		{
			var oldValue = args.OriginalValue as GenericChargeConfigurationCollection;
			var newValue = args.NewValue as GenericChargeConfigurationCollection;
			var builder = new StringBuilder();

			foreach (GenericChargeConfiguration oldCharge in oldValue)
			{
				if (!newValue.Cast<GenericChargeConfiguration>().Any(x => x.ChargePK == oldCharge.Charge.PK))
				{
					builder.Append(Res.GetString("EBE38997-FE27-462E-8BD6-9E40FA6CC972", "{0} is deleted.", oldCharge.Charge.VC_Code));
				}
			}

			foreach (GenericChargeConfiguration newCharge in newValue)
			{
				if (!oldValue.Cast<GenericChargeConfiguration>().Any(x => x.ChargePK == newCharge.Charge.PK))
				{
					builder.Append(Res.GetString("60BFFFCB-3CBB-4C18-9F15-7BC50908A7C0", "{0} is added.", newCharge.Charge.VC_Code));
				}
			}

			return builder.ToString();
		}

		#endregion

		public BooleanRegistryItem EnableTransactionLineMonitor
		{
			get
			{
				return GetItem("EnableTransactionLineMonitor",
						delegate
						{
							return new BooleanRegistryItem("EnableTransactionLineMonitor",
								Categories.Accounting_Temp,
								(NoResString)"Enable Transaction Line Monitor (CargoWiseOne Support Only)",
								(NoResString)"Enable the feature to track transaction line changes.",
								RegistryStorageFlags.System,
								RegistryOptions.IsOnlyForSupport,
								false);
						});
			}
		}

		public MultilingualStringRegistryItem ARExceedCreditLimitGrantedEmailNotificationNote
		{
			get
			{
				return GetItem("ARExceedCreditLimitGrantedEmailNotificationNote", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("ARExceedCreditLimitGrantedEmailNotificationNote",
						Categories.Accounting_EmailNotification,
						ResString.GetMultilingualString("7D7F7998-93BD-4B0F-BE5B-E7BF6405A3C4", "AR Exceed Credit Limit Granted Email Notification Note"),
						ResString.GetMultilingualString("CF895E1D-54EA-4217-85BE-3D6124B3790E", "When specified, the note will be included in the Exceeded Credit Limit Granted Notification Email. \r\n\r\nThis note can be used to provide instruction to AR Control Breach Notify Group in relation to exceeding of credit limit granted."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System | RegistryStorageFlags.Branch);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public CrossTradeDebtorConfigurationRegistryItem CrossTradeDebtorDefaultingConfiguration
		{
			get
			{
				return GetItem("CrossTradeDebtorDefaultingConfiguration", delegate
				{
					var defaultValue = new CrossTradeDebtorConfigurationHeader();

					var item = new CrossTradeDebtorConfigurationRegistryItem(
						"CrossTradeDebtorDefaultingConfiguration",
						Categories.Accounting_JobInvoicing,
						ResString.GetMultilingualString("7C111934-03DC-498D-8F2B-A8D3AEB40268", "Cross Trade Debtor Defaulting Configuration"),
						ResString.GetMultilingualString("8558C5DD-8376-48D5-932B-61DEF4540918", @"This registry enables you to configure the charge line debtor to default on a Cross Trade job based on whether the charge is Prepaid or Collect.
These rules only apply to Cross Trade Forwarding Shipments, Quick Bookings and Bookings with Quotes.
If Default Debtor is Job's Controlling Customer, and the controlling customer has an IFT Party, then the PIC IFT will default for prepaid charges and DLV IFT will default for collect charges. 
If there is no IFT, the Controlling Customer will default.

 

Note: When determining the Debtor to default, the Charge Agent Always Charge Code and Charge Local Client Always Charge Code registries will be taken into account and respected above the defaults set in this registry."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						defaultValue);

					return item;
				});
			}
		}

		public CodeDescriptionPairListRegistryItem EInvoicingAmendmentCodes
		{
			get
			{
				return GetItem("EInvoicingAmendmentCodes",
					() => new CodeDescriptionPairListRegistryItem(
						new CountrySpecificDefaultValueRegistryItemImpl<ReadOnlyCodeDescriptionPairList>(
							"EInvoicingAmendmentCodes",
							Categories.Accounting_EReportingAndEInvoicingConfigurations,
							(NoResString)"This registry is used to configure the Amendment Codes according to the tax authorities of each country within the E-Invoicing module",
							new CodeDescriptionPairListRegistryDataType(2),
							RegistryOptions.CacheExpensiveDefaultValue | RegistryOptions.IsOnlyForSupport,
							FactoryForCountryDefaultValues,
							new EInvoicingAmendmentCodes_RegistryDescriptor(),
							editorInfo: new CodeDescriptionPairListEditorInfo()),
						false,
						new CodeDescriptionPairList())
					);
			}
		}

		#region E-Invoicing Portal Base URL

		public StringRegistryItem MalaysiaEInvoicingPortalBaseUrl
		{
			get
			{
				return GetItem("MalaysiaEInvoicingPortalBaseUrl", delegate
				{
					var item = new StringRegistryItem(
							"MalaysiaEInvoicingPortalBaseUrl",
							Categories.Accounting_EReportingAndEInvoicingConfigurations_Malaysia,
							ResString.GetMultilingualString("C85FED02-AA39-4C02-981D-88FB5214974D", "E-Invoicing Portal Base URL"),
							ResString.GetMultilingualString("2C2C9AF4-5880-4158-9E89-30191C32C4A5", "This registry defines the e-Invoice portal Base URL used by Malaysia e-Invoicing."),
							RegistryStorageFlags.System,
							RegistryOptions.Default,
							(NoResString)(Env.Instance.IsProductionSystem ? "https://myinvois.hasil.gov.my" : "https://preprod.myinvois.hasil.gov.my"));

					item.OnBuildLogReference += args => ResString.GetMultilingualString("4AFD5A96-0F93-47E1-81F0-0E1DDA70CA8B", "The value changed from [{0}] to [{1}]", args.OriginalValue, args.NewValue);
					return item;
				});
			}
		}

		#endregion

		public JournalEntriesNumberCustomisationSettingRegistryItem JournalEntriesNumberCustomisation
		{
			get
			{
				return GetItem("JournalEntriesNumberCustomisation", () => new JournalEntriesNumberCustomisationSettingRegistryItem
					(
						"JournalEntriesNumberCustomisation",
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						ResString.GetMultilingualString("7C16A76D-130F-46B8-909B-703F0E2C0543", "Journal Entries Number Customization"),
						ResString.GetMultilingualString("067F8F1C-CC85-4794-BBC8-FE96FFE6D32F", @"This registry controls the auto allocation of unique reference number to each set of Journal Entries generated.

Before configuring the unique reference number format, please ensure that 'Generate and Store Journal Entries for Posted Accounting Transactions' has been enabled. 

By default, no reference number will be assigned to journal entries.

Note:
1. For 'GEN' allocation option, the system will start allocating number from the first journal entries in the first accounting period order by Post Date, Ledger + (Transaction Type + Transaction Number) / (Job Number). Once the number allocation is up-to-date, the system will allocate number as journal entries are generated. 
2. GL Journal can no longer be edited once unique reference number has been allocated. GL Journal can be reversed and re-entered if required.
3. IMPORTANT: Please configure Number Rule and Allocation Option as desired. Both Number Rule and Allocation Option cannot be edited once Allocation Option has been set to GEN."),
						RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value ? RegistryOptions.Default : RegistryOptions.IsHidden)
				);
			}
		}

		public BooleanRegistryItem EnableAJLandRJLForChinaCompany
		{
			get
			{
				return GetItem("EnableAJLandRJLForChinaCompany", delegate
				{
					return new BooleanRegistryItem(
						"EnableAJLandRJLForChinaCompany",
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						(NoResString)"Enable AJL and RJL for China Company (CWSupport Only)",
						(NoResString)@"By default, the registry is set to No.
When this registry is overridden to Yes, the system will display the registry 'Enable AJL and RJL Creation'.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						GetEnableAJLandRJLForChinaCompanyDefaultValue());
				});
			}
		}

		bool GetEnableAJLandRJLForChinaCompanyDefaultValue()
		{
			var result = false;
			var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingGeneralLedgerDataFeature);
			if (featureData != null
				&& featureData.TryDeserializeParameterAsJson<GeneralLedgerDataFeatureControlModel>(out var aJLAndRJLForChinaFeatureControlData)
				&& aJLAndRJLForChinaFeatureControlData.EnableAJLRJLForCN)
			{
				result = aJLAndRJLForChinaFeatureControlData.EnableAJLRJLForCN;
			}

			return result;
		}

		public BooleanRegistryItem EnableAJLandRJL
		{
			get
			{
				return GetItem("EnableAJLandRJL", delegate
				{
					return new BooleanRegistryItem(
						"EnableAJLandRJL",
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						ResString.GetMultilingualString("B7BECF11-1063-4C6C-B823-24AEA8303665", "Enable AJL and RJL Creation"),
						ResString.GetMultilingualString("D3C1C8A7-6B02-4232-8E4C-D8A548BD4E02", @"By default, the registry is set to No.
You can change it to Yes to enable the creation of AJL and RJL for your China company.
Note: Please ensure you have set 'Allocation Option' to 'GEN' in the registry 'Journal Entries Number Customization'."),
						RegistryStorageFlags.Company,
						AccountingConfigurationRegistry.Instance.EnableAJLandRJLForChinaCompany.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) ? RegistryOptions.Default : RegistryOptions.IsHidden,
						false,
						new EnableAJLandRJLRegistryDataType())
					{
						CountryFilterPKs = new[] { Constants.CountryGuids.China }
					};
				});
			}
		}

		public JournalEntriesClassificationGroupRegistryItem JournalEntriesClassificationGroup
		{
			get
			{
				return GetItem("JournalEntriesClassificationGroup", () => new JournalEntriesClassificationGroupRegistryItem
					(
						"JournalEntriesClassificationGroup",
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						ResString.GetMultilingualString("C062752B-CAD9-4275-9225-B90F54B37B81", "Journal Entries Classification Group"),
						ResString.GetMultilingualString("DB9C7224-FC0B-4531-899D-A86D351FABC5", @"By default, all journal entries will share one sequence number and there will be no classification group. 

If required, you can group transaction types and assign a 'Journal Entries Classification Code to each group, and include this code when configuring the 'Journal Entries Number Customization' with the 'Fountain' check box ticked.

Note:
1. AP CTR will have the same 'Group Code' as AR CTR.
2. If a 'Group Code' is assigned to one ledger and transaction type, a Group Code must be assigned to all ledgers and transaction types. 
3. You can create a separate number fountain for each Journal Entries Classification Group by ticking the 'Fountain' check box in the 'Journal Entries Number Customization' registry. Then all transaction types within each group will share the same number sequence.
4. The Journal Entries Classification Group cannot be edited any more once the Allocation Option has been set to GEN in 'Journal Entries Number Customization' Registry."),
						RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value ? RegistryOptions.Default : RegistryOptions.IsHidden)
				);
			}
		}

		public JournalEntriesClassificationGroupCodeRegistryItem JournalEntriesClassificationGroupCode
		{
			get
			{
				return GetItem("JournalEntriesClassificationGroupCode", () => new JournalEntriesClassificationGroupCodeRegistryItem
					(
						"JournalEntriesClassificationGroupCode",
						Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
						ResString.GetMultilingualString("3F3B8F42-2EA8-4A67-A546-DA9F1A096A9D", "Journal Entries Classification Group Code"),
						ResString.GetMultilingualString("E447CDEA-2B34-4AB0-8967-A1018F3EF605", @"The Journal Entries Classification Group Code is used to group multiple ledgers and transaction types together for the classification of journal entries and number sequencing.

Note: The Journal Entries Classification Group Code cannot be edited any more once the Allocation Option has been set to GEN in 'Journal Entries Number Customization' Registry."),
						RegistryStorageFlags.Company,
						AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value ? RegistryOptions.Default : RegistryOptions.IsHidden)
				);
			}
		}

		#region Allocate Journal Entries Number - Start Date

		public DateTimeRegistryItem AllocateJournalEntriesNumberStartDate
		{
			get
			{
				return GetItem("AllocateJournalEntriesNumberStartDate", () => new DateTimeRegistryItem
						(
							"AllocateJournalEntriesNumberStartDate",
							Categories.Accounting_GeneralLedgerDefaults_GenerateJournalEntries,
							(NoResString)"Allocate Journal Entries Number - Start Date (CWSupport Only)",
							(NoResString)@"By default, this registry value will be empty.
When a value is set, the system will commence to allocate unique reference number to journal entries posted from this date when the allocation option of the ""Journal Entries Number Customization"" registry is set to 'GEN'.

Note: The system will set this date to be the 'Start Date' of period 1 of the current fiscal year when the allocation option is set to 'GEN'.",
							RegistryStorageFlags.Company,
							AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value ? RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly : RegistryOptions.IsHidden
						)
				);
			}
		}

		#endregion
	}
}
