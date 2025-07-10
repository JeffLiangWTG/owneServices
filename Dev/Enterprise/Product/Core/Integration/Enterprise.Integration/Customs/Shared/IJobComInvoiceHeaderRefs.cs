using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IJobComInvoiceHeaderRefs
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }

				ZGuid J2_JZ { get; set; }
				ZString J2_ReferenceNumber { get; set; }
				ZString J2_ReferenceNumber2 { get; set; }
				ZString J2_ReferenceType { get; set; }
			}
		}
	}
}