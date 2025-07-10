using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Module.Testing
{
	sealed class DummyChildBusinessObjectExcludable : DummyChildBusinessObject, ICanBeExcludedFromOperationalActions
	{
		public DummyChildBusinessObjectExcludable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool ShouldExclude
		{
			get;
			set;
		}

		public string ReasonForExclusion
		{
			get
			{
				return "!?!";
			}
		}
	}
}
