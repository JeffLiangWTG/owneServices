using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class UnmatchedOrganisation : SystemDefinedOrganisation
	{
		public UnmatchedOrganisation()
			: this(null)
		{
		}

		public UnmatchedOrganisation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override string Code
		{
			get { return "UNMATCHED"; }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UnmatchedOrganisation(factory);
		}
		protected override ZGuid GetOrganisationPK()
		{
			ZGuid result = base.GetOrganisationPK();
			if (result.IsEmpty)
			{
				CreateUNMATCHEDOrgScripts scr = new CreateUNMATCHEDOrgScripts();
				result = scr.Create();
			}
			return result;
		}
	}
}
