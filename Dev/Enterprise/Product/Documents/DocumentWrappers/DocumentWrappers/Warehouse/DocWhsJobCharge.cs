using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsJobCharge : DocJobCharge
	{
		#region Contructors

		protected DocWhsJobCharge(JobCharge jobCharge, BusinessObjectFactory factoryToWrap)
			: base(jobCharge, factoryToWrap)
		{
		}

		public static new DocWhsJobCharge New(JobCharge jobCharge, BusinessObjectFactory factoryToWrap)
		{
			return jobCharge != null ? new DocWhsJobCharge(jobCharge, factoryToWrap) : null;
		}

		#endregion

		#region Related Business Objects

		WhsDocket Docket
		{
			get
			{
				if (docket == null)
				{
					if (JobCharge != null)
					{
						if (!JobCharge.JR_OrderReference.IsEmpty)
						{
							docket = Factory.LoadTop1<WhsDocket>(new ZQuery(WhsDocketSchema.WD_DocketID, JobCharge.JR_OrderReference));
						}
						else if (JobCharge.Job != null)
						{
							docket = Factory.Load<WhsDocket>(JobCharge.Job.JH_ParentID);
						}
					}
				}
				return docket;
			}
		}
		WhsDocket docket;

		#endregion

		#region Properties

		TransactionLine ARLine
		{
			get
			{
				return Factory.Load<TransactionLine>(JobCharge.JR_AL_ARLine);
			}
		}

		public ZDecimal SellAmountFromAccTransLine
		{
			get { return ARLine != null ? ARLine.AL_OSExTaxAmount : ZDecimal.Zero; }
		}

		public ZGuid DocketPK
		{
			get { return Docket != null ? Docket.PK : ZGuid.Empty; }
		}

		public override ZString Description
		{
			get
			{
				var result = base.Description;
				if (GroupType == InvoiceDetailReportSortList.Codes.ChargeCode && result.StartsWith(ChargeCode.Desc))
				{
					var separators = new char[] { ' ', '-', ':', '.', ',', '/' };
					// remove chargecode desc + separators that follow it from description string
					result = result.Right(result.Length - ChargeCode.Desc.Length);
					result = result.TrimStart(separators);
				}
				return result;
			}
		}

		public ZString StorageDescription
		{
			get
			{
				ZString result = "";
				if (Docket == null)
				{
					result = Description;
				}
				return result;
			}
		}

		public ZString OrderReference
		{
			get
			{
				ZString result = "";
				if (Docket != null)
				{
					result = Docket.WD_ExternalReference;
				}
				return result;
			}
		}

		#region ConsigneeName

		public ZString ConsigneeName
		{
			get
			{
				var pickableDocket = Docket as WhsPickableDocket;
				return pickableDocket?.ConsigneeDocAddress.E2_CompanyName ?? ZString.Empty;
			}
		}

		#endregion

		#region GoodsBillToName

		public ZString GoodsBillToName
		{
			get { return (Docket != null) ? Docket.GoodsBillToDocAddress.E2_CompanyName : ZString.Empty; }
		}

		#endregion

		public ZDateTimeOffset JobDate => Docket?.WD_FinalisedDate ?? ZDateTimeOffset.Empty;

		public ZString ConsolidatedInvoiceRef
		{
			get
			{
				ZString result = "";
				if (JobCharge != null)
				{
					if (JobCharge.ARLine == null)
					{
						if (Docket != null && Docket.Client != null)
						{
							result = Docket.Client.OH_FullName;
						}
					}
					else if (JobCharge.ARLine.TransactionHeader != null)
					{
						result = JobCharge.ARLine.TransactionHeader.AH_ConsolidatedInvoiceRef;
					}
				}
				return result;
			}
		}

		public ZString DebtorCodeAndName
		{
			get
			{
				ZString result = "";
				if (Debtor != null)
				{
					result = Debtor.OH_FullName + " (" + Debtor.OH_Code + ")";
				}
				return result;
			}
		}

		OrgHeader Debtor
		{
			get
			{
				OrgHeader result = null;
				if (JobCharge != null && JobCharge.ARLine != null && JobCharge.ARLine.TransactionHeader != null)
				{
					result = JobCharge.ARLine.TransactionHeader.Header;
				}
				else if (Docket != null)
				{
					result = Docket.Client;
				}
				return result;
			}
		}

		public ZString PartAttrib1
		{
			get { return JobCharge.JobChargeAttrib_PartAttrib1; }
		}

		public ZString PartAttrib2
		{
			get { return JobCharge.JobChargeAttrib_PartAttrib2; }
		}

		public ZString PartAttrib3
		{
			get { return JobCharge.JobChargeAttrib_PartAttrib3; }
		}

		public ZString Commodity
		{
			get { return JobCharge.JobChargeAttrib_Commodity; }
		}

		public ZString DocketReference
		{
			get { return JobCharge.JobChargeAttrib_DocketReference; }
		}

		public ZString LocationDesc
		{
			get { return JobCharge.JobChargeAttrib_LocationDesc; }
		}

		public ZString LocationType
		{
			get { return JobCharge.JobChargeAttrib_LocationType; }
		}

		public ZString Product
		{
			get { return JobCharge.JobChargeAttrib_Product; }
		}

		public ZString GroupDesc
		{
			get { return groupDesc; }
			set { groupDesc = value; }
		}

		public ZString GroupType
		{
			get { return groupType; }
			set { groupType = value; }
		}

		public ZString GroupDesc2
		{
			get { return groupDesc2; }
			set { groupDesc2 = value; }
		}

		public ZString GroupType2
		{
			get { return groupType2; }
			set { groupType2 = value; }
		}

		public ZString GroupDesc3
		{
			get { return groupDesc3; }
			set { groupDesc3 = value; }
		}

		public ZString GroupType3
		{
			get { return groupType3; }
			set { groupType3 = value; }
		}

		ZString groupDesc;
		ZString groupDesc2;
		ZString groupDesc3;
		ZString groupType;
		ZString groupType2;
		ZString groupType3;

		#endregion
	}
}
