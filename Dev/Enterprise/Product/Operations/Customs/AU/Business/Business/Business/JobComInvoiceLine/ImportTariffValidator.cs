using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ImportTariffValidator
	{
		public void Validate(ZPropertyInfo tariffInfo, DutyCalculator dutyCalculator, ZString origin, ZString preference, ZDateTime dateOfValuation)
		{
			ZString tariffValue = tariffInfo.Value.ToString();
			if (tariffValue.IsEmpty)
			{
				tariffInfo.AddMessageError("Tariff may not be empty.");
			}
		}
	}
}
