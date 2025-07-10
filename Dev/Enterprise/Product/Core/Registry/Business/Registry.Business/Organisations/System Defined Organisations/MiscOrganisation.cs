using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class MiscOrganisation : SystemDefinedOrganisation
	{
		public MiscOrganisation()
			: this(null)
		{
		}

		public MiscOrganisation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override string Code
		{
			get { return "MISC"; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MiscOrganisation(factory);
		}

		protected override ZGuid GetOrganisationPK()
		{
			ZGuid result = base.GetOrganisationPK();
			if (result.IsEmpty)
			{
				CreateMISCOrgScripts scr = new CreateMISCOrgScripts();
				result = scr.Create();
			}
			return result;
		}
	}
}
