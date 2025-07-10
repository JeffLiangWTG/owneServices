using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class PlaceWrapper : IPlace
	{
		PlaceWrapper(string unloco,string location)
		{
			this.unloco = Argument.NotNull(unloco, nameof(unloco));
			this.location = location;
		}
		readonly string unloco;

		public static PlaceWrapper New(string unloco, string location) => string.IsNullOrEmpty(unloco) ? null : new PlaceWrapper(unloco, location);

		public string UNLoCode => unloco;

		public string Location => location;
		readonly string location;
	}
}
