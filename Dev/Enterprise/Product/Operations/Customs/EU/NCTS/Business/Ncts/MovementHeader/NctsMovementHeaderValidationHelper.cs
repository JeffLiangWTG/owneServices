using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class NctsMovementHeaderValidationHelper
	{
		public static void CheckCustomsOffices(this NctsCommonMovementHeader nctsMovementHeader, ZPropertyInfo info)
		{
			var nctsHeader = nctsMovementHeader.Header;
			if (nctsHeader.IsDepartureMovement)
			{
				var errors = nctsMovementHeader.CustomsOfficeRequirementHelper.Validate();
				errors.ForEach(e => info.AddMessageError(e));
			}
		}

		public static ICustomsOffice OfficeOfTransit(NctsCommonMovementHeader nctsMovementHeader) => nctsMovementHeader.TransitCustomsOfficeCodeList.FirstOrDefault();
	}
}
