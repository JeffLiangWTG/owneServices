using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusOutturnOutturnReportLineInformation : IOutturnReportLineInformation
	{
		public CusOutturnOutturnReportLineInformation(CusOutturn outturn)
		{
			this.Outturn = outturn;
		}

		public bool DamageIndicator
		{
			get { return Outturn.C5_DamageIndicator; }
		}

		public bool PillageIndicator
		{
			get { return Outturn.C5_PillageIndicator; }
		}

		public ZInt NumberOfPackages
		{
			get { return NumberOfPackagesCore(); }
		}

		protected virtual ZInt NumberOfPackagesCore()
		{
			return Outturn.C5_PackagesOutturned;
		}

		public ZString GoodsDescription
		{
			get { return GetGoodsDescription(); }
		}

		public ZString OutturnResultType
		{
			get { return Outturn.C5_OutturnResultType; }
		}

		protected virtual ZString GetGoodsDescription()
		{
			return Outturn.C5_GoodsDescription;
		}

		public ZDateTime LastMessageDate
		{
			get { return Outturn.C5_LastMessageDate; }
		}

		protected readonly CusOutturn Outturn;
	}
}
