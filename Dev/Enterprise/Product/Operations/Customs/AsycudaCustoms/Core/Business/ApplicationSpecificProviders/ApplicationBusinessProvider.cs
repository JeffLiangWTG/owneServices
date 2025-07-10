using CargoWise.Types;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class ApplicationBusinessProvider : BaseApplicationBusinessProvider
	{
		protected override ZString GetUniversalTariffTypeCore() => Universal.Constants.TariffTypes.HarmonizedSystem;

		public override ZString UniversalRefDataSource => Core.Constants.Customs.Universal.DataSetTypes.OWNData;
	}
}
