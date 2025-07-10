using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public class NctsMessageNumberStrategy : IMessageNumberStrategy
	{
		public NctsMessageNumberStrategy(BusinessObjectFactory factory, string applicationCode)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.applicationCode = Argument.NotNull(applicationCode, nameof(applicationCode));
		}

		public string GetMessageReferenceNumber() => Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", applicationCode).GetNextFormatted(factory);

		readonly ZString applicationCode;
		readonly BusinessObjectFactory factory;
	}
}
