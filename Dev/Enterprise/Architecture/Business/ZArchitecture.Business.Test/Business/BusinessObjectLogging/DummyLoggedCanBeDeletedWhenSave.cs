using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyLoggedCanBeDeletedWhenSave : DummyLogged
	{
		public DummyLoggedCanBeDeletedWhenSave(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ShouldFireDelete { get; set; }

		public override void OnSaving()
		{
			if (ShouldFireDelete)
			{
				Delete();
			}

			base.OnSaving();
		}
	}
}
