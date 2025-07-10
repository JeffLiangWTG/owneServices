using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class CalloutPaymentNote : UPEStmNote
	{
		public CalloutPaymentNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ST_IsCustomDescription = true;
			ST_Description = "Payment Details";
		}

		public override bool ReadOnly
		{
			get { return true; }
		}
	}
}
