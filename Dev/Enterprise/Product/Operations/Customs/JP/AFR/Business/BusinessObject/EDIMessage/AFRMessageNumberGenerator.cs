using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class AFRMessageNumberGenerator : IMessageNumberStrategy
	{
		public AFRMessageNumberGenerator(BusinessObjectFactory factory)
		{
			this.factory = Argument.NotNull(factory, "factory");
		}
		readonly BusinessObjectFactory factory;

		#region IMessageNumberStrategy Members

		public string GetMessageReferenceNumber()
		{
			return new FormattedNumberFountainFactory("JPAFRMESSAGENUMBER", "JP").New().GetNextFormatted(factory);
		}

		#endregion
	}
}
