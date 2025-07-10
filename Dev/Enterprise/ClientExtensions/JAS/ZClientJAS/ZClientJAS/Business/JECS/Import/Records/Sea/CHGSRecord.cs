
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class CHGSRecord : JXCRecord, IJobChargeData
	{
		public CHGSRecord(ZString lineType, ZString lineContent)
			: base(lineType, lineContent)
		{
		}

		ZDecimal ChargeAmount
		{
			get { return Fields.GetDecimalFieldValue(JXCConstants.CHGSFieldPositions.ChargeAmount); }
		}

		ZString ChargeCode
		{
			get { return Fields.GetFieldValue(JXCConstants.CHGSFieldPositions.ChargeCode); }
		}

		ZString ChargeDescription
		{
			get { return Fields.GetFieldValue(JXCConstants.CHGSFieldPositions.ChargeDescription); }
		}

		ZString Currency
		{
			get { return Fields.GetFieldValue(JXCConstants.CHGSFieldPositions.Currency); }
		}

		ZString PrepaidOrCollect
		{
			get { return Fields.GetFieldValue(JXCConstants.CHGSFieldPositions.PrepaidOrCollect); }
		}

		#region IJobChargeData Members

		ZString IJobChargeData.ChargeCode
		{
			get { return ChargeCode; }
		}

		ZString IJobChargeData.ChargeDescription
		{
			get { return ChargeDescription; }
		}

		ZString IJobChargeData.Currency
		{
			get { return Currency; }
		}

		ZDecimal IJobChargeData.ChargeAmount
		{
			get { return ChargeAmount; }
		}

		bool IJobChargeData.IsCollect
		{
			get { return PrepaidOrCollect.ToUpper() == "C"; }
		}

		#endregion
	}
}
