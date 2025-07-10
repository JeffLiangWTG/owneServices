using CargoWise.Types;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusHAWBProcessTaskCollection : ProcessTaskCollection<CusHAWBProcessTask, CusHAWB>
	{
		public CusHAWBProcessTaskCollection(CusHAWB cusHAWB)
			: base(cusHAWB)
		{
		}

		public override ZString DestinationCountry
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		public override bool IsCondition1Met(ZString conditionCode)
		{
			return CusMAWBProcessTaskCollection.IsConditionMet(conditionCode);
		}

		protected override bool IsCondition2MetCore(ZString conditionCode, ZString value)
		{
			return CusMAWBProcessTaskCollection.IsConditionMet(conditionCode);
		}
	}
}
