using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface IOrgSupplierPart
			{
				ZGuid PK { get; }

				IBusinessObjectCollection<ICusClassPartPivot> PivotsForBinding { get; }

				BusinessObjectFactory Factory { get; }
			}
		}
	}
}
