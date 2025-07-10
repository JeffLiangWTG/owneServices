using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class GlowTempOrgRequiredFieldCollection : CodeDescriptionBoolDisallowNewCollection
	{
		public GlowTempOrgRequiredFieldCollection()
			: base()
		{
		}

		public GlowTempOrgRequiredFieldCollection(int codeMaxLength)
			: base(codeMaxLength)
		{
		}

		public new GlowTempOrgRequiredField this[int i]
		{
			get { return (GlowTempOrgRequiredField)base[i]; }
		}

		public new GlowTempOrgRequiredField AddNew()
		{
			return (GlowTempOrgRequiredField)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new GlowTempOrgRequiredField();
		}

		protected override CodeDescriptionBoolCollection GetNewCollection()
		{
			return new GlowTempOrgRequiredFieldCollection();
		}

		public GlowTempOrgRequiredField Add(ZString code, MultilingualString description, bool enabled, bool isMandatory)
		{
			var result = (GlowTempOrgRequiredField)base.Add(code, description, enabled);
			result.IsMandatory = isMandatory;
			return result;
		}

		public GlowTempOrgRequiredField AddSystemDefined(ZString code, MultilingualString description, bool enabled, bool isMandatory)
		{
			var result = (GlowTempOrgRequiredField)base.AddSystemDefined(code, description, enabled);
			result.IsMandatory = isMandatory;
			return result;
		}
	}
}
