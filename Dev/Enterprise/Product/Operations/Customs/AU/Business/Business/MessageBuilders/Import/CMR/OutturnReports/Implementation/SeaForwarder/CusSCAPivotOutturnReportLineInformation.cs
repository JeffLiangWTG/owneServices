using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotOutturnReportLineInformation : CusSCAContainerOutturnReportLineInformation
	{
		public CusSCAPivotOutturnReportLineInformation(CusSCAPivot pivot, CusOutturn outturn)
			: base(pivot.Container, outturn)
		{
			this.pivot = pivot;
		}

		#region Implementation

		protected override ZString GetGoodsDescription()
		{
			return pivot.CV_GoodsDescription;
		}

		protected override ZString GetHouseBillOfLading()
		{
			CusSCAHouse houseBill = pivot.HouseBill;
			CusSCAOceanBill oceanBill = houseBill.OceanBill;
			return oceanBill.CB_MultiOBLUnpack ? ZString.Empty : houseBill.CA_HouseBill;
		}

		protected override ZString GetOceanBillOfLading()
		{
			CusSCAHouse houseBill = pivot.HouseBill;
			CusSCAOceanBill oceanBill = houseBill.OceanBill;
			return oceanBill.CB_MultiOBLUnpack ? houseBill.CA_HouseBill : oceanBill.CB_OceanBill;
		}

		protected override ZString GetImportCargoType()
		{
			return Container.CN_ContainerMode;
		}

		protected override ZString GetPackageType()
		{
			return pivot.CV_PackageType;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return pivot.CV_MarksAndNumbers;
		}

		readonly CusSCAPivot pivot;

		#endregion
	}
}
