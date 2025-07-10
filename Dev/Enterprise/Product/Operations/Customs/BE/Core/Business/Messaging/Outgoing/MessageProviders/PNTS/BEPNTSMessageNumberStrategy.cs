using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.Business;

public class BEPNTSMessageNumberStrategy : IMessageNumberStrategy
{
	public BEPNTSMessageNumberStrategy(BusinessObjectFactory factory, string applicationCode)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.applicationCode = Argument.NotNull(applicationCode, nameof(applicationCode));
	}
	readonly ZString applicationCode;
	readonly BusinessObjectFactory factory;

	public string GetMessageReferenceNumber() => Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", applicationCode).GetNextFormatted(factory);
}
