using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for CusSCAOceanBillActualArrivalReportInformation.
	/// </summary>
	public class CusSCAOceanBillActualArrivalReportInformation : CusSCAOceanBillArrivalReportInformation, ISeaActualArrivalReportInformation
	{
		public CusSCAOceanBillActualArrivalReportInformation(CusSCAOceanBill oceanBill)
			: base(oceanBill)
		{
		}

		#region IActualArrivalReportInformation Members

		public ZDateTime ActualArrivalDateTimeUTC
		{
			get
			{
				return ZDateTime.UtcNow.AddHours(-1);
			}
		}

		public ZString BerthCode
		{
			get
			{
				return oceanBill.BerthCode;
			}
		}

		public ZString DischargeCTOID
		{
			get
			{
				return oceanBill.DischargeCTOID;
			}
		}

		public ZString Voyage
		{
			get
			{
				return oceanBill.CB_Voyage;
			}
		}

		public ZString LloydsNumber
		{
			get
			{
				return oceanBill.CB_LloydsIMO;
			}
		}

		public ZString StevedoreID
		{
			get
			{
				return oceanBill.StevadoreID;
			}
		}

		#endregion
	}
}
