using System.Linq;
using CargoWise.Application;
using static Enterprise.Core.Constants;
using static Enterprise.Integration.Customs.AU;

namespace Enterprise.DocumentEngine
{
	public static class DeliveryInstructionsProvider
	{
		public static DeliveryInstructions Get(DocumentPack pack)
		{
			if (pack?.Cast<IDeliverable>()?.Any(c => c.DocumentTypeCode.EqualsIgnoringCase(RefDocTypes.QuarantineRemotePrint)) ?? false)
			{
				return (DeliveryInstructions)ObjectFactory.Get<IQuarantineDeliveryInstructions>(@"AU.IQuarantineDeliveryInstructions", pack);
			}

			return new DeliveryInstructions(pack);
		}
	}
}
