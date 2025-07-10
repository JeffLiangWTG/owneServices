using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAContainerOutturnReportLineInformation : SeaCusOutturnOutturnReportLineInformation, ISeaOutturnReportLineInformation
	{
		public CusSCAContainerOutturnReportLineInformation(CusSCAContainer container, CusOutturn outturn)
			: base(outturn)
		{
			this.Container = container;
		}

		protected override ZString GetContainerNumber()
		{
			return Container.CN_ContainerNumber;
		}

		protected override ZString GetSealNumber()
		{
			return Container.CN_SealNumber;
		}

		protected override ZString GetHouseBillOfLading()
		{
			return ZString.Empty;
		}

		protected override ZString GetOceanBillOfLading()
		{
			return ZString.Empty;
		}

		protected override ZString GetImportCargoType()
		{
			return Core.Constants.ContainerModes.FCL;
		}

		protected override ZString GetPackageType()
		{
			return Container.MostPrevelantPackageType;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return ZString.Empty;
		}

		protected override ZString GetOutturnStatus()
		{
			return ZString.Empty;
		}

		protected readonly CusSCAContainer Container;
	}
}
