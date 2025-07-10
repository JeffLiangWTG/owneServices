using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	[DependentBusinessObject(typeof(JobComInvoiceLine), "LPCOJobComInvLineRefsCollection")]
	public class LPCOJobComInvLineRefs : JobComInvLineRefs
	{
		public LPCOJobComInvLineRefs(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void OnSaving()
		{
			if (JG_ReferenceNumber.IsEmpty)
			{
				Delete();
			}
			base.OnSaving();
		}

		[MaxLength(11)]
		[ResourceStringData("Enterprise.Customs.BR.Business.LPCOJobComInvLineRefs|JG_ReferenceNumber", Caption = "LPCO")]
		public override ZString JG_ReferenceNumber
		{
			get => base.JG_ReferenceNumber;
			set
			{
				var oldValue = JG_ReferenceNumber;
				base.JG_ReferenceNumber = value;
				if (!IsCopying && oldValue != JG_ReferenceNumber)
				{
					InvoiceLine.LPCOJobComInvLineRefsCollection.MarkAsNeedingValidation();
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JG_ReferenceType = JobComInvLineRefsType.Codes.Lpco;
		}

		protected override JobComInvLineRefsValidation GetNewValidation()
		{
			JobComInvLineRefsValidation result;
			if (InvoiceLine?.IsExport ?? ZBool.False)
			{
				result = new LPCOJobComInvLineRefsValidation(this);
			}
			else
			{
				result = base.GetNewValidation();
			}
			return result;
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;
	}
}
