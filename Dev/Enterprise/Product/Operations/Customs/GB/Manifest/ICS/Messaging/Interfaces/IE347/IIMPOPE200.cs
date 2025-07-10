using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	[CodeAlive("Will be used in subsequent WI.")]
	public interface IIMPOPE200
	{
		ZString MRN { get; }
		ZString OfficeOfFirstEntryCountryCode { get; }
		IGOOITEIMP248 GOOITEIMP248 { get; }
	}
}
