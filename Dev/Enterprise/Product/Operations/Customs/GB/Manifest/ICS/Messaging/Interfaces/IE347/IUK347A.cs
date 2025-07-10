using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	[CodeAlive("Will be used in subsequent WI.")]
	public interface IUK347A
	{
		ZString MesSenMES3 { get; }
		ZString MesRecMES6 { get; }
		ZDateTime DatOfPreMES9 { get; }
		ZDateTime TimOfPreMES10 { get; }
		ZString PriMES15 { get; }
		ZString TesIndMES18 { get; }
		ZString MesIdeMES19 { get; }
		ZString MesTypMES20 { get; }

		IHEAHEA HEAHEA { get; }

		ICustomsOfficeOfArrival CUSTOMSOFFICEOFARRIVAL { get; }

		ITraderAtEntry TRADERATENTRY { get; }

		IIMPOPE200 IMPOPE200 { get; }
	}
}
