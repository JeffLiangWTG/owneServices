using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	[CodeAlive("Will be used in subsequent WI.")]
	public interface ITraderAtEntry
	{
		ZString Name { get; }
		ZString StreetAndNumber { get; }
		ZString City { get; }
		ZString Country { get; }
		ZString Postcode { get; }
		ZString Language { get; }
		ZString EORI { get; }
	}
}
