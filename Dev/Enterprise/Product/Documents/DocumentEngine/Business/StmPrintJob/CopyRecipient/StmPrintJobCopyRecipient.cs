using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class StmPrintJobCopyRecipient : AutoStmPrintJobCopyRecipient
	{
		public StmPrintJobCopyRecipient(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region PrintJob

		public virtual StmPrintJob PrintJob
		{
			get { return Factory.Load<StmPrintJob>(SPR_SP); }
		}

		[EmailAddress]
		public override ZString SPR_EmailAddress
		{
			get
			{
				return base.SPR_EmailAddress;
			}
			set
			{
				base.SPR_EmailAddress = value;
			}
		}

		[RelatedBusinessObject("PrintJob")]
		public override ZGuid SPR_SP
		{
			get { return base.SPR_SP; }
			set { base.SPR_SP = value; }
		}

		#endregion
	}
}
