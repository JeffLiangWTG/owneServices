using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract partial class InvoiceBulkOperationFilters : FilterStripBusinessObject, IObsoleteValidation
	{
		public InvoiceBulkOperationFilters(BusinessObjectFactory factory)
		{
			Helper = new InvoiceBulkOperationFilterHelper(factory);
		}

		public InvoiceBulkOperationFilters()
		{
		}

		protected readonly InvoiceBulkOperationFilterHelper Helper;

		public ZQuery GetQuery()
		{
			Helper.ResetUniqueParameterNumber();
			return Filter;
		}

		public ZGuid DefaultCreditorFilterValue
		{
			get { return defaultCreditorFilterValue; }
			protected set
			{
				defaultCreditorFilterValue = value;
				DefaultValues.Add(new FilterBusinessObjectDefault(InvoiceBulkOperationFilterHelper.CreditorFilterName, "Property", value));
				SetExternalDefaults(DefaultValues);
			}
		}
		ZGuid defaultCreditorFilterValue;

		public ZString DefaultSupplierCostReferenceFilterValue
		{
			get { return defaultSupplierCostReferenceFilterValue; }
			protected set
			{
				if (value != ZString.Empty)
				{
					defaultSupplierCostReferenceFilterValue = value;
					DefaultValues.Add(new FilterBusinessObjectDefault(InvoiceBulkOperationFilterHelper.SupplierCostReferenceFilterName, "Property", value));
					SetExternalDefaults(DefaultValues);
				}
			}
		}
		ZString defaultSupplierCostReferenceFilterValue;

		public ZGuid DefaultTaxBranchFilterValue
		{
			get { return defaultTaxBranchFilterValue; }
			protected set
			{
				defaultTaxBranchFilterValue = value;
				DefaultValues.Add(new FilterBusinessObjectDefault(InvoiceBulkOperationFilterHelper.TaxBranchFilterName, "Property", value));
				SetExternalDefaults(DefaultValues);
			}
		}
		ZGuid defaultTaxBranchFilterValue;

		#region ModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection { SupportsQueryCaching = false };

			AddOrganizationFilters(result);
			AddDateFilters(result);
			AddOtherOperationsFilters(result);
			AddLineFilters(result);

			return result;
		}

		protected virtual void AddOrganizationFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.CreditorFilterName, ModuleIDs.Organisation, GetCreditorFilter, Helper.Creditors);
			guidfilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.CreditorFilterDescription;
			guidfilter.DefaultProperty = DefaultCreditorFilterValue;
			guidfilter.Category = FilterCategories.Organisations;

			guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.SendingAgentFilterName, ModuleIDs.Organisation, GetSendingAgentFilter, Helper.Forwarders);
			guidfilter.Category = FilterCategories.Organisations;
			guidfilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.SendingAgentFilterDescription;

			guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.ReceivingAgentFilterName, ModuleIDs.Organisation, GetReceivingAgentFilter, Helper.Forwarders);
			guidfilter.Category = FilterCategories.Organisations;
			guidfilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.ReceivingAgentFilterDescription;
		}

		protected virtual void AddDateFilters(ModuleFilterCollection filters)
		{
			AddDateFilter(filters, InvoiceBulkOperationFilterHelper.PostDateFilterName, InvoiceBulkOperationFilterHelper.PostDateFilterDescription, GetPostDateFilter);
			AddDateFilter(filters, InvoiceBulkOperationFilterHelper.ShipmentETA_ETDFilterName, InvoiceBulkOperationFilterHelper.ShipmentETA_ETDFilterDescription, GetShipmentETA_ETDFilter);
		}

		protected virtual void AddOtherOperationsFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter textFilter = filters.AddTextFilter(InvoiceBulkOperationFilterHelper.TransportModeFilterName, GetTransportModeFilter, Helper.TransportModeFilterList);
			textFilter.Category = Helper.OtherOperationsFilters;
			textFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.TransportModeFilterDescription;
			textFilter.MaxLength = JobConsolSchema.JK_TransportMode.MaxLength;

			textFilter = filters.AddTextFilter(InvoiceBulkOperationFilterHelper.ContainerModeFilterName, GetContainerModeFilter, Helper.ContainerModeFilterList);
			textFilter.Category = Helper.OtherOperationsFilters;
			textFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.ContainerModeFilterDescription;
			textFilter.MaxLength = JobConsolSchema.JK_ConsolMode.MaxLength;

			textFilter = filters.AddTextFilter(InvoiceBulkOperationFilterHelper.FlightFilterName, GetFlightFilter);
			textFilter.Category = Helper.OtherOperationsFilters;
			textFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.FlightFilterDescription;
			textFilter.MaxLength = JobVoyageSchema.JV_VoyageFlight.MaxLength;

			textFilter = filters.AddTextFilter(InvoiceBulkOperationFilterHelper.MasterBillFilterName, GetMasterBillFilter);
			textFilter.Category = Helper.OtherOperationsFilters;
			textFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.MasterBillFilterDescription;
			textFilter.MaxLength = JobConsolSchema.JK_MasterBillNum.MaxLength;

			textFilter = filters.AddTextFilter(InvoiceBulkOperationFilterHelper.CoLoadMasterBillFilterName, GetCoLoadMasterBillFilter);
			textFilter.Category = Helper.OtherOperationsFilters;
			textFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.CoLoadMasterBillFilterDescription;
			textFilter.MaxLength = JobConsolSchema.JK_CoLoadMasterBill.MaxLength;

			textFilter = filters.AddTextFilter(InvoiceBulkOperationFilterHelper.ContainerNumberFilterName, GetContainerNumberFilter);
			textFilter.Category = Helper.OtherOperationsFilters;
			textFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.ContainerNumberFilterDescription;
			textFilter.MaxLength = JobContainerSchema.JC_ContainerNum.MaxLength;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			helpers.Add(GetWorkflowFilterStripsHelper());

			return helpers;
		}

		protected virtual IFilterStripsHelper GetWorkflowFilterStripsHelper()
		{
			ZString templateCode = "JOB";
			var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), AccTransactionLinesSchema.AL_JH);
			var helper = ObjectFactory.Get<IInvoiceBulkOperationWorkflowFilterStripsHelper>("IInvoiceBulkOperationWorkflowFilterStripsHelper", templateCode, typeof(AccTransactionLines), Factory, true);
			helper.AddRelatedParentJoiningQuery(subQuery);

			return helper;
		}

		protected virtual void AddLineFilters(ModuleFilterCollection filters)
		{
			ModuleGuidFilter guidfilter = filters.AddGuidFilter(InvoiceBulkOperationFilterHelper.ChargeCodeFilterName, ModuleIDs.NotAssigned, GetChargeCodeFilter, Helper.ChargeCodes);
			guidfilter.Category = Helper.ChargeLineFilters;
			guidfilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.ChargeCodeFilterDescription;

			ModuleNkFilter nkfilter = filters.AddNkFilter(InvoiceBulkOperationFilterHelper.CurrencyFilterName, GetCurrencyFilter, ModuleIDs.RefCurrency, Helper.Currencies);
			nkfilter.Category = Helper.ChargeLineFilters;
			nkfilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.CurrencyFilterDescription;

			ModuleNumberFilter supplierCostReferenceFilter = filters.AddNumberFilter(InvoiceBulkOperationFilterHelper.SupplierCostReferenceFilterName, GetSupplierCostReferenceFilter);
			supplierCostReferenceFilter.MaxLength = JobChargeSchema.JR_CostReference.MaxLength;
			supplierCostReferenceFilter.Category = Helper.ChargeLineFilters;
			supplierCostReferenceFilter.MultilingualDescription = InvoiceBulkOperationFilterHelper.SupplierCostReferenceFilterDescription;
		}

		protected void AddDateFilter(ModuleFilterCollection filters, string name, MultilingualString description, GetDateQuery queryDelegate)
		{
			var dateFilter = filters.AddDateFilter(name, queryDelegate);
			dateFilter.Category = FilterCategories.Dates;
			dateFilter.MultilingualDescription = description;
		}

		#endregion

		#region Filter Methods

		protected abstract ZQuery GetCreditorFilter(ZGuid creditorPK);

		protected abstract ZQuery GetSendingAgentFilter(ZGuid sendingAgentPK);

		protected abstract ZQuery GetReceivingAgentFilter(ZGuid receivingAgentPK);

		protected abstract ZQuery GetPostDateFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2);

		protected abstract ZQuery GetShipmentETA_ETDFilter(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2);

		protected abstract ZQuery GetTransportModeFilter(ZString value);

		protected abstract ZQuery GetContainerModeFilter(ZString value);

		protected abstract ZQuery GetFlightFilter(SQLComparisonOperator comparisonOperator, ZString value);

		protected abstract ZQuery GetMasterBillFilter(SQLComparisonOperator comparisonOperator, ZString value);

		protected abstract ZQuery GetCoLoadMasterBillFilter(SQLComparisonOperator comparisonOperator, ZString value);

		protected abstract ZQuery GetContainerNumberFilter(SQLComparisonOperator comparisonOperator, ZString value);

		protected abstract ZQuery GetChargeCodeFilter(ZGuid chargeCodePK);

		protected abstract ZQuery GetCurrencyFilter(ZString value);

		protected abstract ZQuery GetSupplierCostReferenceFilter(SQLComparisonOperator comparisonOperator, ZString value);

		#endregion

		FilterBusinessObjectDefaults DefaultValues => defaultValues ?? (defaultValues = new FilterBusinessObjectDefaults());
		FilterBusinessObjectDefaults defaultValues;
	}
}
