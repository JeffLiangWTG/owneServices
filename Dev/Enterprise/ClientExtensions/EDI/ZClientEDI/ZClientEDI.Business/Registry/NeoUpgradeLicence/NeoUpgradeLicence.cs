using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class NeoUpgradeLicence : AutoNeoUpgradeLicence
	{
		public NeoUpgradeLicence() { }

		public NeoUpgradeLicence(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public NeoUpgradeLicenceLookups Lookups => lookups ?? (lookups = new NeoUpgradeLicenceLookups(this));
		NeoUpgradeLicenceLookups lookups;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new NeoUpgradeLicence(fallbackLevel, factory);
		}

		#region Properties

		[List("Lookups.LicenceEnterpriseList")]
		public override ZGuid LicencePK
		{
			get { return base.LicencePK; }
			set
			{
				if (base.LicencePK != value)
				{
					base.LicencePK = value;
					EnterpriseCode = LicenceEnterprise?.LE_EnterpriseCode ?? ZString.Empty;
				}
			}
		}

		LicenceEnterprise LicenceEnterprise => Lookups.Factory.Load<LicenceEnterprise>(LicencePK);

		#endregion

		#region Validation

		public override void ValidateLicencePK()
		{
			base.ValidateLicencePK();
			MandatoryValidation.CheckEntered(LicencePKInfo);
			ListValidation.ErrorIfInvalidPK(LicencePKInfo);
		}

		#endregion
	}
}
