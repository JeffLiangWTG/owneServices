using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[BusinessObjectTestExclude]
	public class DummyChildEnterpriseBusinessObjectCollection : DummyChildBusinessObjectCollection, IOrgHeaderCollection
	{
		public DummyChildEnterpriseBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			AllowNewTemporaryOrganisations = true;
		}

		public DummyChildEnterpriseBusinessObjectCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			AllowNewTemporaryOrganisations = true;
		}

		public new DummyChildEnterpriseBusinessObject this[int i]
		{
			get { return (DummyChildEnterpriseBusinessObject)base[i]; }
		}

		public new DummyChildEnterpriseBusinessObject AddNew()
		{
			return (DummyChildEnterpriseBusinessObject)base.AddNew();
		}

		#region IOrgHeaderCollection Members

		public bool AllowNewTemporaryOrganisations { get; set; }

		void IOrgHeaderCollection.SetDefaultsForNewChild(object bizObj)
		{
		}

		#endregion
	}
}
