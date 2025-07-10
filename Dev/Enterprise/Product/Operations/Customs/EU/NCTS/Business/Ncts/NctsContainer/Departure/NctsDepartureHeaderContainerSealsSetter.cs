using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsDepartureHeaderContainerSealsSetter
	{
		public NctsDepartureHeaderContainerSealsSetter(NctsDepartureHeaderContainer headerContainer)
		{
			this.headerContainer = Argument.NotNull(headerContainer, nameof(headerContainer));
		}

		public void SetInFirstAvailableSlot(ZString seal)
		{
			if (seal.IsEmpty || headerContainer.AllSeals.Contains(seal))
			{
				return;
			}

			if (headerContainer.BC_Seal1.IsEmpty)
			{
				headerContainer.BC_Seal1 = seal;
			}
			else if (headerContainer.BC_Seal2.IsEmpty)
			{
				headerContainer.BC_Seal2 = seal;
			}
			else
			{
				var additionalSeal = headerContainer.AdditionalSeals.AddNew();
				additionalSeal.BK_SealNumber = seal;
			}
		}

		readonly NctsDepartureHeaderContainer headerContainer;
	}
}
