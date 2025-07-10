using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class LicenceEnterpriseKey : AutoLicenceEnterpriseKey
	{
		public LicenceEnterpriseKey() { }

		public LicenceEnterpriseKey(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public new BusinessObjectFactory CurrentFactory
		{
			get { return base.CurrentFactory; }
		}

		#region Properties

		[List("Lookups.InternalEnterpriseList")]
		public override ZGuid LE_PK
		{
			get { return base.LE_PK; }
			set { base.LE_PK = value; }
		}

		#endregion

		#region Lookups

		public LicenceEnterpriseKeyLookups Lookups
		{
			get { return lookups ?? (lookups = new LicenceEnterpriseKeyLookups(this)); }
		}
		LicenceEnterpriseKeyLookups lookups;

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LicenceEnterpriseKey(fallbackLevel, factory);
		}

		#endregion
	}
}

