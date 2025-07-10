using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUExportTariffBulkChange : TariffBulkChange, IObsoleteValidation
	{
		public AUExportTariffBulkChange(BusinessObjectFactory factory)
			: base(factory, BaseCusClassification.ClassificationType.EXP)
		{
		}

		public override ZString ReferenceKey => "HS2022 AHECC " + lookupType;

		public override ZGuid CountryPK => Core.Constants.CountryGuids.Australia;

		public override ZString CountryCode => Core.Constants.CountryCodes.Australia;

		protected override TariffFormatter GetCurrentTariffFormatter() => new AUExportTariffFormatter();
	}
}
