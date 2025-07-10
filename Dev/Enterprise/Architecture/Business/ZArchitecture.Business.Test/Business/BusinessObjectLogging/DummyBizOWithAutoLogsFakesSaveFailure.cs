using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyBizOWithAutoLogsFakesSaveFailure : DummyBizOWithAutoLogs
	{
		public DummyBizOWithAutoLogsFakesSaveFailure(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnSaving()
		{
			throw new NotSupportedException("Cannot save");
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(false);
		}
	}
}
