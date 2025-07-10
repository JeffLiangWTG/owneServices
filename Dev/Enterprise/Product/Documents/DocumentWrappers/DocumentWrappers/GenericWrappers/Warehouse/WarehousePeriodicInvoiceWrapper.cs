using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehousePeriodicInvoiceWrapper : WarehouseJobGenericWrapper
	{
		#region Constructors

		public WarehousePeriodicInvoiceWrapper(DocARInvoice invoiceWrapper, WhsInvoice whsInvoice, BusinessObjectFactory factory)
			: base(whsInvoice, factory)
		{
			this.invoiceWrapper = invoiceWrapper;
		}

		#endregion

		#region Properties for Warehouse Invoice Detail document

		public override ZString InvoiceNumber
		{
			get { return (InvoicingBase != null) ? InvoicingBase.InvoiceNumber : ZString.Empty; }
		}

		public override ZString ConsolidatedInvoiceRef
		{
			get
			{
				if (consolidatedInvoiceRef.IsEmpty && JobChargeLines != null && JobChargeLines.Count > 0)
				{
					return JobChargeLines[0].ConsolidatedInvoiceRef;
				}
				else
				{
					return consolidatedInvoiceRef;
				}
			}
		}

		public override ZString DebtorCodeAndName
		{
			get
			{
				if (debtorCodeAndName.IsEmpty && JobChargeLines != null && JobChargeLines.Count > 0)
				{
					return JobChargeLines[0].DebtorCodeAndName;
				}
				else
				{
					return debtorCodeAndName;
				}
			}
		}

		ZString consolidatedInvoiceRef = ZString.Empty;
		ZString debtorCodeAndName = ZString.Empty;

		public override ZString AccountCode
		{
			get { return (InvoicingBase != null) ? InvoicingBase.Header.OH_Code : ZString.Empty; }
		}

		public override ZDateTime FromDate
		{
			get { return WhsInvoice != null ? WhsInvoice.ET_StorageFromDate : ZDateTime.Empty; }
		}

		public override ZDateTime ToDate
		{
			get { return WhsInvoice != null ? WhsInvoice.ET_StorageToDate : ZDateTime.Empty; }
		}

		public override LabelValuePairWrapper WarehouseName
		{
			get
			{
				if (WhsInvoice != null)
				{
					return new LabelValuePairWrapper(Res.GetString("b98bc925-e321-4cf6-8d00-169fbf274a75", "WAREHOUSE:"), WhsInvoice.WarehouseName, Factory);
				}
				else
				{
					return LabelValuePairWrapper.Empty;
				}
			}
		}

		public override ZString CurrencyCode
		{
			get { return (InvoicingBase != null) ? InvoicingBase.AH_RX_NKTransactionCurrency : ZString.Empty; }
		}

		public override MultilingualString ReportDescription
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("505893c7-1dd5-449c-8a38-ab869264c9c5", "Invoice");
				if (WhsInvoice != null && WhsInvoice.Client != null)
				{
					var sort1 = WhsInvoice.Client.MiscServ.OM_IMInvoiceDetailReportSort;
					var sort2 = WhsInvoice.Client.MiscServ.OM_IMInvoiceDetailReportSort2;
					var sort3 = WhsInvoice.Client.MiscServ.OM_IMInvoiceDetailReportSort3;

					if (sort1 == "DEF")
					{
						sort1 = WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.Value.Group1;
					}

					if (sort2 == "DEF")
					{
						sort2 = WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.Value.Group2;
					}

					if (sort3 == "DEF")
					{
						sort3 = WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.Value.Group3;
					}

					if (!sort1.IsEmpty)
					{
						result = MultilingualString.Join(" / ", result, GetBreakdownDesc(sort1));
					}

					if (!sort2.IsEmpty)
					{
						result = MultilingualString.Join(" / ", result, GetBreakdownDesc(sort2));
					}

					if (!sort3.IsEmpty)
					{
						result = MultilingualString.Join(" / ", result, GetBreakdownDesc(sort3));
					}
				}
				return ResString.GetMultilingualString("CCF4C0E1-FA21-425a-9268-B9EF14C296AE", "Breakdown of Invoice charges by {0}. Sorted by Breakdown + Job Date + Reference + Product Code.", result);
			}
		}

		#endregion

		MultilingualString GetBreakdownDesc(string code)
		{
			var miscServ = WhsInvoice.Client.MiscServ;
			MultilingualString result = (NoResString)miscServ.Lookups.InvoiceDetailReportSortList1.GetDescriptionFromCode(code);
			if (code == JobChargeAttribTypeList.Codes.Attrib1)
			{
				result = miscServ.OM_IMPartAttrib1NameMultilingual;
			}

			if (code == JobChargeAttribTypeList.Codes.Attrib2)
			{
				result = miscServ.OM_IMPartAttrib2NameMultilingual;
			}

			if (code == JobChargeAttribTypeList.Codes.Attrib3)
			{
				result = miscServ.OM_IMPartAttrib3NameMultilingual;
			}

			if (code == JobChargeAttribTypeList.Codes.SerialNumber)
			{
				result = ResString.GetMultilingualString("c7b657c3-f6ee-4c15-bdd3-39ae584c79c5", "Serial Number");
			}

			return result;
		}

		#region JobChargeLines

		protected override DocWhsJobChargeCollection NewWarehouseJobChargeLineWrapperCollection() =>
			DocWhsJobChargeCollection.GetCollection(this, nameof(NewWarehouseJobChargeLineWrapperCollection),
				(jobChargeLines) =>
				{
					foreach (JobCharge charge in GetChargesForInvoiceDetailReport())
					{
						DocWhsJobCharge docJobCharge = DocWhsJobCharge.New(charge, Factory);
						FormatDataForReport(docJobCharge);
						jobChargeLines.Add(docJobCharge);
					}

					jobChargeLines.Sort("Description", ListSortDirection.Ascending);
					jobChargeLines.Sort("OrderReference", ListSortDirection.Ascending);
					jobChargeLines.Sort("JobDate", ListSortDirection.Ascending);
					jobChargeLines.Sort("GroupDesc3", ListSortDirection.Ascending);
					jobChargeLines.Sort("GroupDesc2", ListSortDirection.Ascending);
					jobChargeLines.Sort("GroupDesc", ListSortDirection.Ascending);
				});

		List<JobCharge> GetChargesForInvoiceDetailReport()
		{
			List<JobCharge> result = new List<JobCharge>();

			if (JobHeader != null)
			{
				foreach (JobCharge charge in JobHeader.Charges)
				{
					if (charge.IsRevenuePosted)
					{
						if (charge.ARLine.AL_AH == InvoicingBase.PK)
						{
							result.Add(charge);
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region GetJobLines

		// When DocWhsJobChargeCollection is implemented into Doc Strips then it should be used in there in place of NewWarehouseJobChargeLineWrapperCollection
		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			return new WarehouseEmptyWrapperCollection(Factory);
		}

		#endregion

		#region Implementation

		WhsInvoice whsInvoice;
		WhsInvoice WhsInvoice
		{
			get
			{
				if (whsInvoice == null)
				{
					whsInvoice = WhsInvoiceBO;
				}
				return whsInvoice;
			}
		}

		Job JobHeader
		{
			get { return (InvoicingBase != null) ? Factory.Load<Job>(InvoicingBase.AH_JH) : null; }
		}

		void FormatDataForReport(DocWhsJobCharge docCharge)
		{
			if (WhsInvoice != null && WhsInvoice.Client != null)
			{
				ZString sort1 = WhsInvoice.Client.MiscServ.OM_IMInvoiceDetailReportSort;
				ZString sort2 = WhsInvoice.Client.MiscServ.OM_IMInvoiceDetailReportSort2;
				ZString sort3 = WhsInvoice.Client.MiscServ.OM_IMInvoiceDetailReportSort3;
				docCharge.GroupType = sort1 != "DEF" ? sort1 : WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.Value.Group1;
				docCharge.GroupType2 = sort2 != "DEF" ? sort2 : WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.Value.Group2;
				docCharge.GroupType3 = sort3 != "DEF" ? sort3 : WarehouseDataRegistry.Instance.InvoiceDetailGroupBy.Value.Group3;
			}

			docCharge.GroupDesc = GetDesc(docCharge, docCharge.GroupType);
			docCharge.GroupDesc2 = GetDesc(docCharge, docCharge.GroupType2);
			docCharge.GroupDesc3 = GetDesc(docCharge, docCharge.GroupType3);
		}

		string GetDesc(DocWhsJobCharge docCharge, string groupBy)
		{
			string result = "";

			if (WhsInvoice != null && WhsInvoice.Client != null)
			{
				switch (groupBy)
				{
					case JobChargeAttribTypeList.Codes.Attrib1:
						result = GroupDescWithEmptyCheck(WhsInvoice.Client.MiscServ.OM_IMPartAttrib1NameMultilingual, docCharge.PartAttrib1);
						break;

					case JobChargeAttribTypeList.Codes.Attrib2:
						result = GroupDescWithEmptyCheck(WhsInvoice.Client.MiscServ.OM_IMPartAttrib2NameMultilingual, docCharge.PartAttrib2);
						break;

					case JobChargeAttribTypeList.Codes.Attrib3:
						result = GroupDescWithEmptyCheck(WhsInvoice.Client.MiscServ.OM_IMPartAttrib3NameMultilingual, docCharge.PartAttrib3);
						break;

					case JobChargeAttribTypeList.Codes.Commodity:
						result = GroupDescWithEmptyCheck(JobChargeAttribTypeList.Descriptions.Commodity, docCharge.Commodity);
						break;

					case JobChargeAttribTypeList.Codes.DocketReference:
						result = GroupDescWithEmptyCheck(JobChargeAttribTypeList.Descriptions.DocketReference, docCharge.DocketReference);
						break;

					case JobChargeAttribTypeList.Codes.LocationDesc:
						result = GroupDescWithEmptyCheck(JobChargeAttribTypeList.Descriptions.LocationDesc, docCharge.LocationDesc);
						break;

					case JobChargeAttribTypeList.Codes.LocationType:
						result = GroupDescWithEmptyCheck(JobChargeAttribTypeList.Descriptions.LocationType, docCharge.LocationType);
						break;

					case JobChargeAttribTypeList.Codes.Product:
						result = GroupDescWithEmptyCheck(JobChargeAttribTypeList.Descriptions.Product, docCharge.Product);
						break;

					case InvoiceDetailReportSortList.Codes.ChargeCode:
						result = docCharge.ChargeCode.Code + " (" + docCharge.ChargeCode.Desc + ")";
						break;

					case InvoiceDetailReportSortList.Codes.JobType:
						{
							if (docCharge.ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.WHSInwards)
							{
								result = Res.GetString("f5776da7-9096-4d4f-bd6b-289f3d2db104", "INWARDS");
							}
							else if (docCharge.ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.WHSOutwards)
							{
								result = Res.GetString("ff0291ae-5833-4973-92c8-1e25d8268104", "ORDERS");
							}
							else if (docCharge.ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.WHSStorage)
							{
								result = Res.GetString("9bbdef8e-c7a8-4fe6-a686-11a3a6f23a3a", "STORAGE");
							}
							break;
						}
				}
			}

			return result;
		}

		ZString GroupDescWithEmptyCheck(ZString desc, ZString value)
		{
			return desc.ToUpper() + ": " + (value.IsEmpty ? new ZString(Res.GetString("8695bd65-05bd-4791-822e-407121252048", "[None]")) : value);
		}

#if DEBUG
		internal InvoicingBase MockInvoicingBase
		{
			get { return invoicingBase; }
			set { invoicingBase = value; }
		}

		internal ZString MockConsolidatedInvoiceRef
		{
			get { return consolidatedInvoiceRef; }
			set { consolidatedInvoiceRef = value; }
		}

		internal ZString MockDebtorCodeAndName
		{
			get { return debtorCodeAndName; }
			set { debtorCodeAndName = value; }
		}
#endif

		WhsInvoice WhsInvoiceBO
		{
			get { return whsInvoiceBO ?? (whsInvoiceBO = WrappedBO as WhsInvoice); }
		}
		WhsInvoice whsInvoiceBO;

		InvoicingBase InvoicingBase
		{
			get { return invoicingBase ?? (invoicingBase = (invoiceWrapper != null) ? invoiceWrapper.WrappedObject as InvoicingBase : null); }
		}
		InvoicingBase invoicingBase;

		readonly DocARInvoice invoiceWrapper;

		#endregion
	}
}
