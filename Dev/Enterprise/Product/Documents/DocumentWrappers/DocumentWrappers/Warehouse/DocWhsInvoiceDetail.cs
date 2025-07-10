using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsInvoiceDetail : DocBaseWrapper
	{
		#region Constructors

		protected DocWhsInvoiceDetail(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
			: base(invoicingBase, factoryToWrap)
		{
		}

		public static DocWhsInvoiceDetail New(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
		{
			return new DocWhsInvoiceDetail(invoicingBase, factoryToWrap);
		}

		public static DocWhsInvoiceDetail New(AccTransactionHeader transactionHeader, BusinessObjectFactory factoryToWrap)
		{
			DocWhsInvoiceDetail result = null;

			if (transactionHeader != null)
			{
				InvoicingBase invoice = (InvoicingBase)factoryToWrap.Load<TransactionHeader>(transactionHeader.PK);
				result = DocWhsInvoiceDetail.New(invoice, factoryToWrap);
			}

			return result;
		}

		#endregion

		#region Related Business Objects

		public DocBranch Branch
		{
			get { return DocBranch.New(InvoicingBase.Branch, Factory); }
		}

		public DocWhsJobChargeCollection JobChargeLines =>
			DocWhsJobChargeCollection.GetCollection(this, nameof(JobChargeLines),
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

		#region Properties

		public override string ToString()
		{
			return JobNumber;
		}

		public ZString CurrencyCode
		{
			get { return InvoicingBase.AH_RX_NKTransactionCurrency; }
		}

		public ZString JobNumber
		{
			get { return InvoicingBase.InvoiceNumber; }
		}

		public ZString AccountCode
		{
			get { return InvoicingBase.Header.OH_Code; }
		}

		public ZString InvoiceTitle
		{
			get { return Res.GetString("505893c7-1dd5-449c-8a38-ab869264c9c5", "Invoice"); }
		}

		public ZDateTime FromDate
		{
			get { return WhsInvoice != null ? WhsInvoice.ET_StorageFromDate : ZDateTime.Empty; }
		}

		public ZDateTime ToDate
		{
			get { return WhsInvoice != null ? WhsInvoice.ET_StorageToDate : ZDateTime.Empty; }
		}

		public ZString WarehouseName
		{
			get { return WhsInvoice != null ? WhsInvoice.WarehouseName : ZString.Empty; }
		}

		public ZString ReportDescription
		{
			get
			{
				string result = Res.GetString("505893c7-1dd5-449c-8a38-ab869264c9c5", "Invoice");
				if (WhsInvoice != null && WhsInvoice.Client != null)
				{
					ZString sort1 = WhsInvoice.Client.MiscServ.OM_IMInvoiceDetailReportSort;
					ZString sort2 = WhsInvoice.Client.MiscServ.OM_IMInvoiceDetailReportSort2;
					ZString sort3 = WhsInvoice.Client.MiscServ.OM_IMInvoiceDetailReportSort3;

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
						result += " / " + GetBreakdownDesc(sort1);
					}

					if (!sort2.IsEmpty)
					{
						result += " / " + GetBreakdownDesc(sort2);
					}

					if (!sort3.IsEmpty)
					{
						result += " / " + GetBreakdownDesc(sort3);
					}
				}
				return Res.GetString("991b411d-965b-45b2-bd62-26cdcffceb15", "Breakdown of Invoice charges by {0}. Sorted by Breakdown + Job Date + Reference + Product Code.", result);
			}
		}

		string GetBreakdownDesc(string code)
		{
			OrgMiscServ miscServ = WhsInvoice.Client.MiscServ;
			string result = miscServ.Lookups.InvoiceDetailReportSortList1.GetDescriptionFromCode(code);
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

			return result;
		}

		#endregion

		#region Implementation

		protected override ZString DocManagerUniqueID
		{
			get { return JobNumber; }
		}

		WhsInvoice whsInvoice;
		WhsInvoice WhsInvoice
		{
			get
			{
				if (whsInvoice == null && JobHeader != null)
				{
					whsInvoice = Factory.Load<WhsInvoice>(JobHeader.JH_ParentID);
				}
				return whsInvoice;
			}
		}

		InvoicingBase InvoicingBase
		{
			get { return (InvoicingBase)WrappedObject; }
		}

		Job JobHeader
		{
			get { return Factory.Load<Job>(InvoicingBase.AH_JH); }
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
						// warehouse storage charges should never be job specific.
						ZString docketReference = (docCharge.ChargeCode.ChargeGroup != ChargeCodeGroupList.Codes.WHSStorage) ? docCharge.DocketReference : ZString.Empty;
						result = GroupDescWithEmptyCheck(JobChargeAttribTypeList.Descriptions.DocketReference, docketReference);
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
								result = "INWARDS";
							}
							else if (docCharge.ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.WHSOutwards)
							{
								result = "ORDERS";
							}
							else if (docCharge.ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.WHSStorage)
							{
								result = "STORAGE";
							}
							break;
						}
				}
			}

			return result;
		}

		ZString GroupDescWithEmptyCheck(ZString desc, ZString value)
		{
			return desc.ToUpper() + ": " + (value.IsEmpty ? new ZString((NoResString)"[None]") : value);
		}

		#endregion
	}
}
