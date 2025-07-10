
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCharacteristic : AutoCMRCharacteristic
	{
		public CMRCharacteristic(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRCharacteristic New(BusinessObjectFactory factory)
		{
			return factory.New<CMRCharacteristic>();
		}
	}
}
