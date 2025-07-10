using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class PrintChargesBilledToLocalClientAtDestAsCollectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public PrintChargesBilledToLocalClientAtDestAsCollectCollection()
		{
		}

		public PrintChargesBilledToLocalClientAtDestAsCollectCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public new PrintChargesBilledToLocalClientAtDestAsCollect this[int index]
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (PrintChargesBilledToLocalClientAtDestAsCollect)Elements[index]; }
		}

		[System.Diagnostics.DebuggerStepThrough]
		public new PrintChargesBilledToLocalClientAtDestAsCollect AddNew()
		{
			return (PrintChargesBilledToLocalClientAtDestAsCollect)base.AddNew();
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PrintChargesBilledToLocalClientAtDestAsCollectCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PrintChargesBilledToLocalClientAtDestAsCollect(CurrentFallbackLevel);
		}

		#endregion
	}
}


