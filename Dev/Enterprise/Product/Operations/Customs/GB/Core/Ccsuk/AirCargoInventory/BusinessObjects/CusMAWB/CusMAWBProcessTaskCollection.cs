using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusMAWBProcessTaskCollection : ProcessTaskCollection<CusMAWBProcessTask, CusMAWB>
	{
		public CusMAWBProcessTaskCollection(CusMAWB cusMAWB)
			: base(cusMAWB)
		{
		}

		public override ZString DestinationCountry
		{
			get { return Core.Constants.CountryCodes.UnitedKingdom; }
		}

		public override bool IsCondition1Met(ZString conditionCode)
		{
			return IsConditionMet(conditionCode);
		}

		protected override bool IsCondition2MetCore(ZString conditionCode, ZString value)
		{
			return IsConditionMet(conditionCode);
		}

		internal static bool IsConditionMet(ZString conditionCode)
		{
			switch (conditionCode)
			{
				case JobDeclarationWorkflowCondition1CodeList.Codes.NotExport:
				case JobDeclarationWorkflowCondition1CodeList.Codes.Import:
					return true;
				case JobDeclarationWorkflowCondition1CodeList.Codes.Export:
					return false;
			}
			return false;
		}
	}
}
