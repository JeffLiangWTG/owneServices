using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public class DocFormedPagesShipment : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Properties

		internal ZString GoodsDescription { get; set; }

		internal ZString MarksAndNumbers { get; set; }

		internal ZString PackageCount { get; set; }

		internal ZString Weight { get; set; }

		internal ZString Volume { get; set; }

		#endregion
	}
}
