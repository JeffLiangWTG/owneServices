using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Client.UPE.Business.GSSI
{
	public class GSSMessage : EDIMessage
	{
		public GSSMessage(BusinessObjectFactory factory, DataRow row) : base(
			factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = "GSS";
			EM_ReceiveTransmit = "TRX";
			EM_Status = "QUE";
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "UPEGSSi", "UPEGSSi").GetNextFormatted(Factory);
		}
	}
}
