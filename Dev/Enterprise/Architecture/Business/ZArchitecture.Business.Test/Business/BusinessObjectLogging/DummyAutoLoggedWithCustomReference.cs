using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyAutoLoggedWithCustomReference : DummyAutoLogged
	{
		public DummyAutoLoggedWithCustomReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString CustomLogReferenceSuffix
		{
			get { return "Custom Dummy Reference"; }
		}
	}
}
