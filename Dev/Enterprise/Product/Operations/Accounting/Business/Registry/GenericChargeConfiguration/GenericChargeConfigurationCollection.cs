using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class GenericChargeConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public GenericChargeConfigurationCollection()
		{
		}

		public GenericChargeConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public GenericChargeConfiguration Find(ZGuid chargePK)
		{
			GenericChargeConfiguration result = null;

			foreach (GenericChargeConfiguration current in this)
			{
				if (current.ChargePK == chargePK)
				{
					result = current;
					break;
				}
			}
			return result;
		}

		#region Overriden

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new GenericChargeConfigurationCollection(fallbackLevel, CurrentFactory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GenericChargeConfiguration(CurrentFallbackLevel, CurrentFactory);
		}

		#endregion

		public new GenericChargeConfiguration this[int index]
		{
			get { return (GenericChargeConfiguration)Elements[index]; }
		}

		public new GenericChargeConfiguration AddNew()
		{
			return (GenericChargeConfiguration)base.AddNew();
		}
	}
}
