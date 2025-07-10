using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class DeactivationWrapperBase : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected DeactivationWrapperBase(BusinessObjectFactory factory) : base(factory)
		{
		}

		public abstract ZString LicenceType { get; }

		public abstract ZString Organisation { get; }

		public abstract ZString SystemInfo { get; }

		public abstract int ReferenceNumber { get; }
	}
}
