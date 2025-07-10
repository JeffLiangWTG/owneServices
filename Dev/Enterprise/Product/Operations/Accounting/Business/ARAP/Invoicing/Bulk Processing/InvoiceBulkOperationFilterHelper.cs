using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter ID used only in code")]
	public class InvoiceBulkOperationFilterHelper
	{
		public InvoiceBulkOperationFilterHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		readonly BusinessObjectFactory Factory;

		public const string CreditorFilterName = "Creditor";
		public const string ChargeCodeFilterName = "ChargeCode";
		public const string CurrencyFilterName = "Currency";
		public const string PostDateFilterName = "PostDate";
		public const string AccountingDateFilterName = "Accounting Date";
		public const string ShipmentETA_ETDFilterName = "ShipmentETA_ETD";
		public const string TransportModeFilterName = "TransportMode";
		public const string ContainerModeFilterName = "ContainerMode";
		public const string SendingAgentFilterName = "SendingAgent";
		public const string ReceivingAgentFilterName = "ReceivingAgent";
		public const string FlightFilterName = "Flight";
		public const string MasterBillFilterName = "MasterBill";
		public const string CoLoadMasterBillFilterName = "CoLoadMasterBill";
		public const string ContainerNumberFilterName = "Container Number";
		public const string FixedPlaceOfSupplyFilterName = "Fixed Place of Supply";
		public const string ConsolCostOwnerFilterName = "Consol Cost Owner";

		// InvoicingBaseBulkConsolCostImporterFilters
		public const string ExcludeReverseSignedConsolCostsFilterName = "ExcludeReverseSignedConsolCostsFilter";
		public const string IncludeConsolCostsWithNoCreditorFilterName = "IncludeConsolCostsWithNoCreditor";

		public const string ConsolATA_ATDFilterName = "ConsolATA_ATD";
		public const string ShipmentActualDeliveryDateFilterName = "ShipmentADD";
		public const string ShipmentActualPickupDateFilterName = "ShipmentAPD";
		public const string CustomsClearanceDateFilterName = "CustomsClearanceDate";
		public const string AWBIssueDateFilterName = "AWBIssueDate";
		public const string HAWBIssueDateFilterName = "HAWBIssueDate";
		public const string MasterBillIssueDateFilterName = "MasterBillIssueDate";
		public const string TaxBranchFilterName = "TaxBranch";

		// InvoicingBaseBulkChargeImporterFilters 
		public const string IncludeAccrualsWithNoCreditorFilterName = "IncludeAccrualsWithNoCreditor";
		public const string ExcludeReverseSignedChargesFilterName = "ExcludeReverseSignedChargesFilter";
		public const string JobBranchFilterName = "JobBranch";
		public const string JobDepartmentFilterName = "JobDepartment";
		public const string BranchFilterName = "Branch";
		public const string DepartmentFilterName = "Department";
		public const string HouseBillFilterName = "HouseBill";
		public const string OtherCreditorsFilterName = "OtherCreditors";
		public const string SupplierCostReferenceFilterName = "SupplierCostReference";
		public const string RunSheetNumberFilterName = "RunSheetNumber";
		public const string JobChargeCustomSqlFilterName = "JobChargeCustomSql";
		public const string JobHeaderCustomSqlFilterName = "JobHeaderCustomSql";
		public const string JobTaxBranchFilterName = "JobTaxBranch";

		// APBulkInvoicePosterFilters
		public const string APSettlementGroupFilterName = "APSettlementGroup";

		#region ResourceStrings

		public static MultilingualString ChargeCodeFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|ChargeCode", "Charge Code"); }
		}

		public static MultilingualString CurrencyFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|Currency", "Currency"); }
		}

		public static MultilingualString ExcludePositiveCostsFilterDescription
		{
			get { return ResString.GetMultilingualString("8b0c83ac-d961-4a27-8bf1-d72afa429f43", "Exclude Positive Costs"); }
		}

		public static MultilingualString ExcludeNegativeCostsFilterDescription
		{
			get { return ResString.GetMultilingualString("c489154f-f659-4838-b58e-01f6bdd62b9a", "Exclude Negative Costs"); }
		}

		public static MultilingualString TaxBranchFilterDescription
		{
			get { return ResString.GetMultilingualString("19AD5FEA-EAAC-4677-9A06-0DB4D4268CFD", "Tax Branch"); }
		}

		public static MultilingualString CreditorFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|Creditor", "Creditor"); }
		}

		public static MultilingualString SendingAgentFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|SendingAgent", "Sending Agent"); }
		}

		public static MultilingualString ReceivingAgentFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|ReceivingAgent", "Receiving Agent"); }
		}

		public static MultilingualString ConsolCostOwnerFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|ConsolCostOwner", "Consol Cost Owner"); }
		}

		public static MultilingualString TransportModeFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|TransportMode", "Transport Mode"); }
		}

		public static MultilingualString ContainerModeFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|ContainerMode", "Container Mode"); }
		}

		public static MultilingualString FlightFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|Flight", "Flight"); }
		}

		public static MultilingualString MasterBillFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|MasterBill", "Master Bill"); }
		}

		public static MultilingualString CoLoadMasterBillFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|CoLoadMasterBill", "Co-Load MBL"); }
		}

		public static MultilingualString ContainerNumberFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|ContainerNumber", "Container Number"); }
		}

		public static MultilingualString RunSheetNumberFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|RunSheetNumber", "Consignment Run-sheet Number"); }
		}

		public static MultilingualString LineBranchFilterDerscriptor
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|Branch", "Line Branch"); }
		}

		public static MultilingualString LineDepartmentFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|Department", "Line Department"); }
		}

		public static MultilingualString CostTaxBranchFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|CostTaxBranch", "Cost Tax Branch"); }
		}

		public static string ExcludePositiveCharges
		{
			get { return Res.GetString("fdb4adc3-347e-40f5-a69d-fffaa9b3a50c", "Exclude Positive Charges"); }
		}

		public static string ExcludeNegativeCharges
		{
			get { return Res.GetString("d0842bbb-e0b7-4a08-b1ed-d635328f3218", "Exclude Negative Charges"); }
		}

		public static MultilingualString ExcludePositiveChargesFilterDescription
		{
			get { return ResString.GetMultilingualString("fdb4adc3-347e-40f5-a69d-fffaa9b3a50c", "Exclude Positive Charges"); }
		}

		public static MultilingualString ExcludeNegativeChargesFilterDescription
		{
			get { return ResString.GetMultilingualString("d0842bbb-e0b7-4a08-b1ed-d635328f3218", "Exclude Negative Charges"); }
		}

		public static MultilingualString AccrualPostDateFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkConsolCostImporterFilters|AccrualPostDate", "Accrual Post Date"); }
		}

		public static MultilingualString ConsolETA_ETDFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|APInvoiceBulkConsolCostImporterFilters|ConsolETA_ETD", "Consol ETA/ETD"); }
		}

		public static MultilingualString ShipmentETA_ETDFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|ShipmentETA_ETD", "Shipment ETA/ETD"); }
		}

		public static MultilingualString PostDateFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|PostDate", "Post Date"); }
		}

		public static MultilingualString AccountingDateFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|AccountingDate", "Accounting Date"); }
		}

		public static MultilingualString SupplierCostReferenceFilterDescription
		{
			get { return ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|SupplierCostReference", "Supplier Cost Reference"); }
		}

		public static MultilingualString ConsolATA_ATDFilterDescription
		{
			get { return ResString.GetMultilingualString("6a644f91-d88a-4555-9426-ffc6dc1a859e", "Consol ATA/ATD"); }
		}

		public static MultilingualString ShipmentActualDeliveryDateFilterDescription
		{
			get { return ResString.GetMultilingualString("182c22dc-bb25-439b-9734-08e4d4d221b1", "Delivery Date"); }
		}

		public static MultilingualString ShipmentActualPickupDateFilterDescription
		{
			get { return ResString.GetMultilingualString("87fdedad-20c5-4d94-82fe-ac5408fcdc4b", "Pickup Date"); }
		}

		public static MultilingualString CustomsClearanceDateFilterDescription
		{
			get { return ResString.GetMultilingualString("c45ec94a-262b-4938-922c-3b5f2b98f093", "Customs Clearance Date"); }
		}

		public static MultilingualString AWBIssueDateFilterDescription
		{
			get { return ResString.GetMultilingualString("dbd4ddf4-ff2b-4dd3-8b0c-2f032b21ada4", "AWB Issue Date"); }
		}

		public static MultilingualString HAWBIssueDateFilterDescription
		{
			get { return ResString.GetMultilingualString("59b59242-6fb6-4ad0-8210-30609cd630c9", "HAWB Issue Date"); }
		}

		public static MultilingualString MasterBillIssueDateFilterDescription
		{
			get { return ResString.GetMultilingualString("b81a97e9-8755-43a8-96d4-8f9ae64fa9b6", "Master Bill Issue Date"); }
		}

		public static MultilingualString FixedPlaceOfSupplyFilterDescription
		{
			get { return ResString.GetMultilingualString("2a8e5f85-ede8-46f6-adfc-09ded8832180", "Fixed Place of Supply"); }
		}

		public static MultilingualString JobChargeCustomSqlFilterDescription => ResString.GetMultilingualString("8d09a00e-4def-42db-a361-351c8bfb3858", "Custom SQL Filter - Charge");

		public static MultilingualString JobHeaderCustomSqlFilterDescription => ResString.GetMultilingualString("0d8ee5dc-9362-427e-99b4-541a56c652bc", "Custom SQL Filter - Job");

		#endregion

		#region Filter Categories

		public FilterCategory ChargeLineFilters
		{
			get
			{
				return chargeLineFilters ?? (chargeLineFilters = new FilterCategory(
					ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|LineFilters", "Charge Line")));
			}
		}
		FilterCategory chargeLineFilters;

		public FilterCategory OtherOperationsFilters
		{
			get
			{
				return otherOperationsFilters ?? (otherOperationsFilters = new FilterCategory(
					ResString.GetMultilingualString("Accounting|InvoiceBulkOperationFilters|OtherOperationsFilters", "Other Operations Filters")));
			}
		}
		FilterCategory otherOperationsFilters;

		public FilterCategory InvoiceLineFilters
		{
			get
			{
				return invoiceLineFilters ?? (invoiceLineFilters = new FilterCategory(
							ResString.GetMultilingualString("Accounting|APBulkInvoicePosterFilters|LineFilters", "Invoice Line")));
			}
		}
		FilterCategory invoiceLineFilters;

		public FilterCategory JobHeaderFilters
		{
			get
			{
				return jobHeaderFilters ?? (jobHeaderFilters = new FilterCategory(
					ResString.GetMultilingualString("Accounting|APInvoiceBulkChargeImporterFilters|JobHeaderFilters", "Job Header")));
			}
		}
		FilterCategory jobHeaderFilters;

		#endregion

		#region SubGroups

		#region OrgAddressSubGroup

		public ModuleFilterSubGroup OrgAddressSubGroup
		{
			get { return orgAddressSubGroup ?? (orgAddressSubGroup = new OrgAddressSubGroupImplementation()); }
		}
		ModuleFilterSubGroup orgAddressSubGroup;

		class OrgAddressSubGroupImplementation : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return filter;
			}
		}

		#endregion

		#endregion

		#region Query With Parameters

		public ZQuery GetQueryWithParameter(SchemaColumn column, SQLComparisonOperator comparisonOperator, object filterValue)
		{
			ZQuery result = new ZQuery();
			string paramName = GetUniqueParameterName(column.Name);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection { ZSqlParameter.New(paramName, filterValue, column) };
			result.AddFilterAndZSQLParameterCollection(column.Name + " " + comparisonOperator.ComparisonText(filterValue) + " " + paramName, parameters);
			return result;
		}

		public string GetUniqueParameterName(string columnName)
		{
			string result = "@" + columnName + currentParameterNumber;
			currentParameterNumber++;

			return result;
		}

		public void ResetUniqueParameterNumber()
		{
			currentParameterNumber = 0;
		}

		int currentParameterNumber;

		public void ReplaceAutogeneratedParameters(ref string sql, ZSqlParameterCollection parameters)
		{
			var autogenrated = from ZSqlParameter param in parameters where param.ParameterName.StartsWith(ParameterNameFactory.ParameterPrefix) select param;

			if (autogenrated.Any())
			{
				var toReplace = autogenrated.ToArray();
				foreach (var param in toReplace)
				{
					string paramName = GetUniqueParameterName(param.SchemaColumn.Name);
					sql = sql.Replace(param.ParameterName, paramName);
					parameters.Remove(param);
					parameters.Add(ZSqlParameter.New(paramName, param.Value, param.SchemaColumn, param.ComparisonOperator, param.ComparisonOptions));
				}
			}
		}

		#endregion

		#region Filter Methods

		public ZQuery GetSendingAgentSubQuery(ZGuid sendingAgentPK)
		{
			var result = new ZQuery();
			string paramName = GetUniqueParameterName(OrgAddressSchema.OA_OH.Name);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection { ZSqlParameter.New(paramName, sendingAgentPK, OrgAddressSchema.OA_OH) };
			result.AddFilterAndZSQLParameterCollection(JobConsolSchema.JK_OA_SendingForwarderAddress.Name + "  IN (SELECT " + OrgAddressSchema.PK.Name + " FROM " + OrgAddressSchema.Constants.SqlSchemaName + "." + OrgAddressSchema.Constants.TableName + " WHERE " + OrgAddressSchema.OA_OH.Name + " = " + paramName + ")", parameters);
			return result;
		}

		public ZQuery GetReceivingAgentSubQuery(ZGuid receivingAgentPK)
		{
			var result = new ZQuery();
			string paramName = GetUniqueParameterName(OrgAddressSchema.OA_OH.Name);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection { ZSqlParameter.New(paramName, receivingAgentPK, OrgAddressSchema.OA_OH) };
			result.AddFilterAndZSQLParameterCollection(JobConsolSchema.JK_OA_ReceivingForwarderAddress.Name + "  IN (SELECT " + OrgAddressSchema.PK.Name + " FROM " + OrgAddressSchema.Constants.SqlSchemaName + "." + OrgAddressSchema.Constants.TableName + " WHERE " + OrgAddressSchema.OA_OH.Name + " = " + paramName + ")", parameters);
			return result;
		}

		public ZQuery GetDateFilter(SchemaColumn column, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var result = new ZQuery();

			if (value1.IsValidSqlDateTime)
			{
				result.AddToFilter(column, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, value1);
			}
			if (value2.IsValidSqlDateTime)
			{
				result.AddToFilter(column, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, value2);
			}

			return result;
		}

		#endregion

		#region Lists

		public AccChargeCodeCollection ChargeCodes
		{
			get { return FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		public RefCurrencyCollection Currencies
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}

		public GlbBranchDependentCollection Branches
		{
			get { return FindboxLookupCollections.GetCompanyBranchesCollection(Factory); }
		}

		public GlbDepartmentCollection Departments
		{
			get { return FindboxLookupCollections.GetDepartmentCollection(Factory); }
		}

		public CreditorCollection Creditors
		{
			get { return FindboxLookupCollections.GetCreditorCollection(Factory); }
		}

		public GlbStaffCollection Staffs
		{
			get { return FindboxLookupCollections.GetStaffCollection(Factory); }
		}

		public ForwarderCollection Forwarders
		{
			get { return FindboxLookupCollections.GetForwarderCollection(Factory); }
		}

		public CodeDescriptionPairList TransportModeFilterList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.TransportType); }
		}

		public CodeDescriptionPairList ContainerModeFilterList
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.FreightContainerMode); }
		}

		#endregion

	}
}
