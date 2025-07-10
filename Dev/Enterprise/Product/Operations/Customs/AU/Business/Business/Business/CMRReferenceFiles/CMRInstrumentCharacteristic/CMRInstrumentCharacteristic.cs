
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCharacteristic : AutoCMRInstrumentCharacteristic
	{
		public CMRInstrumentCharacteristic(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentCharacteristic New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentCharacteristic>();
		}

		public static bool IsInstrumentCharacteristic(ZString number, ZShort requiredCharacteristic, BusinessObjectFactory factory)
		{
			if (!number.IsEmpty)
			{
				var query = new ZQuery(CMRInstrumentCharacteristicSchema.IK_InstrumentNumber, number);
				query.AddToFilter(CMRInstrumentCharacteristicSchema.IK_CharacteristicCode, requiredCharacteristic);
				return factory.LoadTop1<CMRInstrumentCharacteristic>(query) != null;
			}
			return false;
		}
	}
}
