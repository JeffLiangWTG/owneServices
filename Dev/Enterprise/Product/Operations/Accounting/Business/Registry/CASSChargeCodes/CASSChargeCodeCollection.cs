using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CASSChargeCodeCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		#region Constructors	
		public CASSChargeCodeCollection()
		{
		}
		public CASSChargeCodeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}
		#endregion

		public new CASSChargeCode this[int i]
		{
			get { return (CASSChargeCode)Elements[i]; }
		}

		public new CASSChargeCode AddNew()
		{
			return (CASSChargeCode)base.AddNew();
		}

		#region Overriddes
		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CASSChargeCodeCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CASSChargeCode(CurrentFallbackLevel, CurrentFactory);
		}

		#endregion

	}
}
