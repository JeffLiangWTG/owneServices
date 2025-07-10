using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyBizoForLoggerTests : DummyBusinessObject
	{
		public int OnSavingCallsCount { get; private set; }
		public DummyBizoForLoggerTests(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void OnSaving()
		{
			OnSavingCallsCount++;
		}
	}
}
