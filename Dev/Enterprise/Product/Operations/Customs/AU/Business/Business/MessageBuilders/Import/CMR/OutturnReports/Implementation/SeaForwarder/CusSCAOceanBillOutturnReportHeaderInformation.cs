using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusSCAOceanBillOutturnReportHeaderInformation : CusUnderbondOutturnReportHeaderInformation, ISeaOutturnReportHeaderInformation
	{
		public CusSCAOceanBillOutturnReportHeaderInformation(CusSCAOceanBill oceanBill, CusUnderbond underbond)
			: base(underbond)
		{
			this.OceanBill = oceanBill;
		}

		public ZString VoyageNumber
		{
			get { return OceanBill.CB_Voyage; }
		}

		public ZString VesselID
		{
			get { return OceanBill.CB_LloydsIMO; }
		}

		public IEnumerable<ISeaOutturnReportLineInformation> Lines
		{
			get { return GetLines(Underbond); }
		}

		public IEnumerable<ISeaOutturnReportLineInformation> MessageLines
		{
			get { return Lines; }
		}

		public IEDIMessageCollectionProvider MessagesProvider
		{
			get { return Underbond; }
		}

		#region Implementation

		protected abstract IEnumerable<ISeaOutturnReportLineInformation> GetLines(CusUnderbond underbond);

		protected readonly CusSCAOceanBill OceanBill;

		#endregion
	}
}
