using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public partial class CusClassification : Customs.Business.BaseCusClassification
	{
		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString FormatTariffForSaving(ZString unformattedTariff) => unformattedTariff.KeepNumericCharacters();
	}
}
