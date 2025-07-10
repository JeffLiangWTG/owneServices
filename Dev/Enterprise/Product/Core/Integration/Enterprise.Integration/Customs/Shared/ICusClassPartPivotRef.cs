using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICusClassPartPivotRef
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }
				ZGuid CIR_CI { get; set; }
				ZString CIR_ReferenceNumber { get; set; }
				ZString CIR_ReferenceType { get; set; }
			}
		}
	}
}
