using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class ArrivalNewSealsInformationWrapper : IArrivalNewSealsInformation
	{
		public ArrivalNewSealsInformationWrapper(SealContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
		}
		readonly SealContainer container;

		public ZString SealId => container.BC_Seal1;

		public ZString SealIdLanguage => ZString.Empty;
	}
}
