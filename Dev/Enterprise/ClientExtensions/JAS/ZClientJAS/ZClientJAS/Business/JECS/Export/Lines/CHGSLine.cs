
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class CHGSLine : MessageLine
	{
		public CHGSLine(JobCharge jobCharge)
		{
			this.JobCharge = jobCharge;
		}

		protected override int FieldCount
		{
			get { return JXCConstants.CHGSFieldCount; }
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.CHGS; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.CHGSFieldPositions.ChargeCode, JobCharge.ChargeCode.AC_Code, JXCConstants.CHGSFieldBoundaries.ChargeCode);
			dataRow.SetField(JXCConstants.CHGSFieldPositions.ChargeDescription, JobCharge.JR_Desc, JXCConstants.CHGSFieldBoundaries.ChargeDescription);
			dataRow.SetField(JXCConstants.CHGSFieldPositions.ChargeAmount, JobCharge.JR_OSSellAmt);
			dataRow.SetField(JXCConstants.CHGSFieldPositions.PrepaidOrCollect, PrepaidOrCollect);
			dataRow.SetField(JXCConstants.CHGSFieldPositions.Currency, JobCharge.JR_RX_NKSellCurrency);
		}

		ZString PrepaidOrCollect
		{
			get
			{
				ZString result = DefaultPaymentType;
				if (JobHeader != null)
				{
					if (JobHeader.LocalCharges != null && JobCharge.JR_OH_SellAccount == JobHeader.LocalCharges.PK)
					{
						result = "P";
					}
				}
				return result;
			}
		}

		JobHeader JobHeader
		{
			get { return JobCharge.Job; }
		}

		const string DefaultPaymentType = "C";
		readonly JobCharge JobCharge;
	}
}
