
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryCharacteristic : AutoCMRInstrumentCategoryCharacteristic
	{
		public CMRInstrumentCategoryCharacteristic(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentCategoryCharacteristic New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentCategoryCharacteristic>();
		}
	}
}
