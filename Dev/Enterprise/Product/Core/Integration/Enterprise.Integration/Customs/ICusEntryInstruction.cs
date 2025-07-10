using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusEntryInstruction
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }

			ZGuid CEI_JE { get; set; }
		}
	}
}
