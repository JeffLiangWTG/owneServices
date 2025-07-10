using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class NZCustomsAPInvoiceJobWrapper
	{
		public NZCustomsAPInvoiceJobWrapper(JobHeader header)
		{
			Header = header;
	}

		public ZString BranchCode
		{
			get
			{
				return Header.Branch.GB_Code;
			}
		}

		public ZString JobNumber
		{
			get
			{
				return Header.JH_JobNum;
			}
		}

		public ZString DepartmentCode
		{
			get
			{
				return Header.Department.GE_Code;
			}
		}

		JobCharge[] fValidChargeLines;

		public JobCharge[] ValidChargeLines
		{
			get
			{
				if (fValidChargeLines == null)
				{
					ZQuery jobChargeLinesQuery = new ZQuery(JobChargeSchema.JR_JH, Header.PK);
					fValidChargeLines = Array.FindAll(Header.Factory.Load<JobCharge>(jobChargeLinesQuery), c => !c.IsCostPosted);
				}

				return fValidChargeLines;
			}
		}

		//protected readonly BusinessObjectFactory Factory;
		protected readonly JobHeader Header;

		protected readonly AccChargeCode RegistryDutyChargeCode;
		protected readonly AccChargeCode RegistryEntryFeeChargeCode;
		protected readonly AccChargeCode RegistryGSTChargeCode;
	}
}
