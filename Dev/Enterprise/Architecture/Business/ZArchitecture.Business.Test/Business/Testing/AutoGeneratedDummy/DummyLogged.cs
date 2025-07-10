using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyLogged : AutoDummyLogged, IDataVersionLoggingSupported
	{
		public DummyLogged(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			RunPreSaveValidationCount++;
		}

		public int RunPreSaveValidationCount;

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => isDataVersionsAutoLogged;
		bool isDataVersionsAutoLogged;

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		public void SetIsDataVersionsAutoLogged(bool value)
		{
			isDataVersionsAutoLogged = value;
		}
	}
}
