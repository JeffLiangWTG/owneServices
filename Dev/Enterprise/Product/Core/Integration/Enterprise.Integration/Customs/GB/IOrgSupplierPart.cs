using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class GB
		{
			public interface IOrgSupplierPart
			{
				ZGuid PK { get; }

				IBusinessObjectCollection<ICusClassPartPivot> PivotsForBinding { get; }

				BusinessObjectFactory Factory { get; }

				void AddNote(ZString noteText, ZString noteType);
			}
		}
	}
}
