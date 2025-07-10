using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class InvoiceLine : InvoicingLineBase
	{
		public InvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overriden Properties

		#region AL_AC

		public override ZGuid AL_AC
		{
			get { return base.AL_AC; }
			set
			{
				base.AL_AC = value;
				RecalculateTaxAmounts();
			}
		}

		#endregion

		#endregion

		public Invoice Invoice
		{
			get { return (Invoice)MasterTransactionHeader; }
		}

		protected override bool AL_JH_ReadOnly
		{
			get { return base.AL_JH_ReadOnly || InternalAL_JH_ReadOnly; }
		}

		public bool InternalAL_JH_ReadOnly { get; set; }
	}
}
