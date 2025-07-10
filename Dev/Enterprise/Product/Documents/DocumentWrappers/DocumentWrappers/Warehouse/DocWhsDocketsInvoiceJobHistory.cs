using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Invoicing;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsDocketsInvoiceJobHistory : DocBaseWrapper
	{
		#region Static

		public static DocWhsDocketsInvoiceJobHistory New(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
		{
			return (invoicingBase == null) ? null : new DocWhsDocketsInvoiceJobHistory(invoicingBase, factoryToWrap);
		}

		#endregion

		#region Constructors

		protected DocWhsDocketsInvoiceJobHistory(InvoicingBase invoicingBase, BusinessObjectFactory factoryToWrap)
			: base(invoicingBase, factoryToWrap)
		{
		}

		#endregion

		#region Related Business Objects

		public virtual DocWhsJobChargeCollection JobChargeLines =>
			DocWhsJobChargeCollection.GetCollection(this, nameof(JobChargeLines),
				(jobChargeLines) =>
				{
					if (this.JobHeader != null)
					{
						foreach (JobCharge charge in JobHeader.Charges)
						{
							DocWhsJobCharge docJobCharge = DocWhsJobCharge.New(charge, Factory);
							if (AddChargeCondition(docJobCharge))
							{
								FormatDataForReport(docJobCharge);
								jobChargeLines.Add(docJobCharge);
								if (!AllChargeDescriptions.Contains(docJobCharge.ChargeCode.ToString()))
								{
									AllChargeDescriptions.Add(docJobCharge.ChargeCode.ToString());
								}
							}
						}

						jobChargeLines.Sort("Description", ListSortDirection.Ascending);
						jobChargeLines.Sort("JobDate", ListSortDirection.Ascending);
						jobChargeLines.Sort("OrderReference", ListSortDirection.Ascending);

						for (int c = 1; c <= AllChargeDescriptions.Count && c <= 10; c++)
						{
							this["ChargeHeader" + c.ToString()] = AllChargeDescriptions[c - 1];
						}
					}
				});

		protected virtual bool AddChargeCondition(DocWhsJobCharge docWhsJobCharge)
		{
			return GetDocket(docWhsJobCharge.DocketPK) != null;
		}

		#endregion

		#region Collections

		public DocWhsDocketLineCollection JobDocketLines
		{
			get
			{
				if (docDocketLines == null)
				{
					docDocketLines = GetDocDocketLineWithChargesCollection();
					foreach (WhsDocket docket in Dockets)
					{
						List<ChargeCodeAmmount> groupedCharges = GroupedDocketChargesList(docket);
						foreach (WhsDocketLine docketLine in docket.Lines)
						{
							var docketLineWithCharges = GetDocDocketLineWithCharges(docketLine);
							docDocketLines.Add(docketLineWithCharges);
							foreach (ChargeCodeAmmount chargeCode in groupedCharges)
							{
								for (int c = 1; c <= AllChargeDescriptions.Count && c <= 10; c++)
								{
									if (chargeCode.ChargeCode == this["ChargeHeader" + c.ToString()].ToString())
									{
										docketLineWithCharges["ChargeHeader" + c.ToString()] = groupedCharges.Find(delegate(ChargeCodeAmmount chargeCodeAmount) { return (chargeCodeAmount.ChargeCode == chargeCode.ChargeCode.ToString()); }).ChargeAmmount;
										break;
									}
								}
							}
						}
					}
				}
				return docDocketLines;
			}
		}

		public virtual DocWhsDocketLineCollection GetDocDocketLineWithChargesCollection()
		{
			return new DocWhsDocketLineWithChargesCollection(Factory);
		}

		public virtual DocWhsDocketLine GetDocDocketLineWithCharges(WhsDocketLine docketLine)
		{
			return DocWhsDocketLineWithCharges.New(docketLine, Factory);
		}

		protected List<WhsDocket> Dockets
		{
			get
			{
				List<WhsDocket> list = new List<WhsDocket>();
				foreach (DocWhsJobCharge charge in JobChargeLines)
				{
					WhsDocket docket = GetDocket(charge.DocketPK);
					if (AddDocketCondition(docket, list))
					{
						list.Add(docket);
					}
				}
				return list;
			}
		}

		protected virtual bool AddDocketCondition(WhsDocket docket, List<WhsDocket> list)
		{
			return !list.Exists(delegate(WhsDocket filterDocket)
			{ return (filterDocket == docket); });
		}

		protected List<ChargeCodeAmmount> GroupedDocketChargesList(WhsDocket docket)
		{
			List<ChargeCodeAmmount> list = new List<ChargeCodeAmmount>();
			foreach (DocWhsJobCharge charge in JobChargeLines)
			{
				if (GetDocket(charge.DocketPK).WD_DocketID == docket.WD_DocketID)
				{
					if (!list.Exists(delegate(ChargeCodeAmmount chargeCodeAmount)
					{ return (chargeCodeAmount.ChargeCode == charge.ChargeCode.ToString()); }))
					{
						list.Add(new ChargeCodeAmmount(charge.ChargeCode.ToString(), charge.LocalSellAmount));
					}
					else
					{
						list.Find(delegate(ChargeCodeAmmount chargeCodeAmount)
						{ return (chargeCodeAmount.ChargeCode == charge.ChargeCode.ToString()); }).AddAmmount(charge.LocalSellAmount);
					}
				}
			}

			return list;
		}

		#endregion

		#region Properties

		public override string ToString()
		{
			return JobNumber;
		}

		public ZString JobNumber
		{
			get { return InvoicingBase.JobNumber; }
		}

		public ZString AccountCode
		{
			get { return InvoicingBase.Header.OH_Code; }
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

		public ZString ClientName
		{
			get { return (WhsInvoice != null && WhsInvoice.Client != null) ? WhsInvoice.Client.OH_FullName : ZString.Empty; }
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

		#endregion

		#region Hardcoded Headers

		ZString fChargeHeader1;
		public ZString ChargeHeader1
		{
			get { return fChargeHeader1; }
			set { fChargeHeader1 = value; }
		}

		ZString fChargeHeader2;
		public ZString ChargeHeader2
		{
			get { return fChargeHeader2; }
			set { fChargeHeader2 = value; }
		}

		ZString fChargeHeader3;
		public ZString ChargeHeader3
		{
			get { return fChargeHeader3; }
			set { fChargeHeader3 = value; }
		}

		ZString fChargeHeader4;
		public ZString ChargeHeader4
		{
			get { return fChargeHeader4; }
			set { fChargeHeader4 = value; }
		}

		ZString fChargeHeader5;
		public ZString ChargeHeader5
		{
			get { return fChargeHeader5; }
			set { fChargeHeader5 = value; }
		}

		ZString fChargeHeader6;
		public ZString ChargeHeader6
		{
			get { return fChargeHeader6; }
			set { fChargeHeader6 = value; }
		}

		ZString fChargeHeader7;
		public ZString ChargeHeader7
		{
			get { return fChargeHeader7; }
			set { fChargeHeader7 = value; }
		}

		ZString fChargeHeader8;
		public ZString ChargeHeader8
		{
			get { return fChargeHeader8; }
			set { fChargeHeader8 = value; }
		}

		ZString fChargeHeader9;
		public ZString ChargeHeader9
		{
			get { return fChargeHeader9; }
			set { fChargeHeader9 = value; }
		}

		ZString fChargeHeader10;
		public ZString ChargeHeader10
		{
			get { return fChargeHeader10; }
			set { fChargeHeader10 = value; }
		}

		#endregion

		#region Implementation

		protected WhsDocket GetDocket(ZGuid docketPK)
		{
			return Factory.Load<WhsDocket>(docketPK);
		}

		protected InvoicingBase InvoicingBase
		{
			get { return (InvoicingBase)WrappedObject; }
		}

		Job JobHeader
		{
			get { return Factory.Load<Job>(InvoicingBase.AH_JH); }
		}

		protected void FormatDataForReport(DocWhsJobCharge docCharge)
		{
			docCharge.GroupDesc = docCharge.ChargeCode.Code + " (" + docCharge.ChargeCode.Desc + ")";

			if (WhsInvoice != null && WhsInvoice.Client != null)
			{
				docCharge.GroupType = WhsInvoice.Client.MiscServ.OM_IMInvoiceDetailReportSort;

				if (docCharge.GroupType == InvoiceDetailReportSortList.Codes.JobType)
				{
					if (docCharge.ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.WHSInwards)
					{
						docCharge.GroupDesc = "INWARDS";
					}
					else if (docCharge.ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.WHSOutwards)
					{
						docCharge.GroupDesc = "ORDERS";
					}
					else if (docCharge.ChargeCode.ChargeGroup == ChargeCodeGroupList.Codes.WHSStorage)
					{
						docCharge.GroupDesc = "STORAGE";
					}
				}
			}
		}

		protected DocWhsDocketLineCollection docDocketLines;
		public List<ZString> AllChargeDescriptions = new List<ZString>();

		protected class ChargeCodeAmmount
		{
			public readonly ZString ChargeCode;

			public ZDecimal ChargeAmmount
			{
				get { return chargeAmmount; }
			}

			ZDecimal chargeAmmount;

			public ChargeCodeAmmount(ZString code, ZDecimal ammount)
			{
				ChargeCode = code;
				chargeAmmount = ammount;
			}

			public void AddAmmount(ZDecimal ammount)
			{
				chargeAmmount += ammount;
			}
		}

		#endregion
	}
}
