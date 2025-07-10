using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryHeader
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }

			ZGuid CH_JE { get; set; }
			ZString CH_EntryStatus { get; set; }
			ZString CH_Status { get; set; }
			ZString CH_MessageType { get; set; }
			ZString CH_PhaseStatus { get; set; }
		}
	}
}
