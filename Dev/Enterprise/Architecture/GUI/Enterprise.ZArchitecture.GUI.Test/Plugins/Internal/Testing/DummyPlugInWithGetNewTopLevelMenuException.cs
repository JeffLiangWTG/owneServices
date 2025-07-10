using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	sealed class DummyPlugInWithGetNewTopLevelMenuException : ZPlugIn
	{
		public DummyPlugInWithGetNewTopLevelMenuException(IBusiness hostBusinessEntity)
			: base(hostBusinessEntity)
		{
		}

		public override string Name
		{
			get { return "DummyPlugIn1WithGetNewTopLevelMenuException"; }
		}

		protected internal override ZBool HasUserControl
		{
			get { return false; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return (LicenceCheckpoint)EnvProxy.Instance.Licence.AlwaysAllow; }
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			throw new NullReferenceException();
		}
	}
}
