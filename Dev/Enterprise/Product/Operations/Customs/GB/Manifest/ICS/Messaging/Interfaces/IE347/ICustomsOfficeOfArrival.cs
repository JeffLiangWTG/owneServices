using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	[CodeAlive("Will be used in subsequent WI.")]
	public interface ICustomsOfficeOfArrival
	{
		ZString ReferenceNumber { get; }
	}
}
