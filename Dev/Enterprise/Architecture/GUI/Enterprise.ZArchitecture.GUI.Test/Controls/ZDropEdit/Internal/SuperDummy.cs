using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class SuperDummy : DummyBusinessObject, ICodeDescription
	{
		public SuperDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		SuperDummyLookups lookups;
		public SuperDummyLookups Lookups => lookups ?? (lookups = new SuperDummyLookups(this));

		public HugeDummy Huge { get; set; }

		object ICodeDescription.PK => PK;
		string ICodeDescription.Code => Z0_FK_Code;
		string ICodeDescription.Description => Z0_FK_Code;

		public override void Delete()
		{
			base.Delete();
			Huge?.RefreshSortedInvoiceList();
		}
	}
}
