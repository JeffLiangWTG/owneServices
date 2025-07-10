using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class Licence3rdPartySoftwareDependentCollection : DependentBusinessObjectCollection<Licence3rdPartySoftware, LicenceCompany>
	{
		public Licence3rdPartySoftwareDependentCollection(LicenceCompany parent)
			: base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return Licence3rdPartySoftwareSchema.L3_LC_LicenceCompany; }
		}
	}
}

