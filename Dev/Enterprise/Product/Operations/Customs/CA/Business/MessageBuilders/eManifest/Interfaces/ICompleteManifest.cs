namespace Enterprise.Customs.CA.Business.MessageBuilders.eManifest
{
	using System.Collections.Generic;

	[WTG.StaticAnalysis.Annotation.CodeAlive("There are future possible usages, similar to the usages in US solution")]
	interface ICompleteManifest : ITrip
	{
		IEnumerable<IShipment> Shipments { get; }
	}
}
