
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPreferenceRulePeriodCharacteristic : AutoCMRPreferenceRulePeriodCharacteristic
	{
		public CMRPreferenceRulePeriodCharacteristic(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRPreferenceRulePeriodCharacteristic New(BusinessObjectFactory factory)
		{
			return factory.New<CMRPreferenceRulePeriodCharacteristic>();
		}
	}
}
