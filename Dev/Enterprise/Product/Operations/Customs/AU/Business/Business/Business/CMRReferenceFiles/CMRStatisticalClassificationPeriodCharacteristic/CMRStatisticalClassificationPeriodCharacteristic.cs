
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRStatisticalClassificationPeriodCharacteristic : AutoCMRStatisticalClassificationPeriodCharacteristic
	{
		public CMRStatisticalClassificationPeriodCharacteristic(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRStatisticalClassificationPeriodCharacteristic New(BusinessObjectFactory factory)
		{
			return factory.New<CMRStatisticalClassificationPeriodCharacteristic>();
		}

		public static CMRStatisticalClassificationPeriodCharacteristic[] Load(BusinessObjectFactory factory, ZString tariffNumber, ZString statCode, ZShort periodID)
		{
			ZQuery filter = new ZQuery(CMRStatisticalClassificationPeriodCharacteristicSchema.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber, tariffNumber.Replace(" ", "").Replace(".", ""));
			filter.AddToFilter(CMRStatisticalClassificationPeriodCharacteristicSchema.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier, periodID);
			filter.AddToFilter(CMRStatisticalClassificationPeriodCharacteristicSchema.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode, statCode.Replace(" ", ""));

			return (CMRStatisticalClassificationPeriodCharacteristic[])factory.Load(typeof(CMRStatisticalClassificationPeriodCharacteristic), filter);
		}
	}
}
