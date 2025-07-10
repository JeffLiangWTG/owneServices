using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyIOrgHeaderBusinessObjectCollection : DummyBusinessObjectCollection, IOrgHeaderCollection
	{
		public DummyIOrgHeaderBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region IOrgHeaderCollection Members

		bool IOrgHeaderCollection.AllowNewTemporaryOrganisations
		{
			get { return true; }
		}

		void IOrgHeaderCollection.SetDefaultsForNewChild(object bizObj)
		{
		}

		#endregion
	}
}
