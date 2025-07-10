using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ModulePlugIn;
using Enterprise.ZArchitecture.ModulePlugIn.Testing;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	class DummyController1 : DummyController
	{
		public static Disposable GetPlugInThrowsException() => new GetPlugInThrowsExceptionDisposable();

		class GetPlugInThrowsExceptionDisposable : Disposable
		{
			public GetPlugInThrowsExceptionDisposable()
			{
				throwException = true;
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					throwException = false;
				}
			}
		}

		public override ControllerID ID => DummyControllerIDs.Dummy1;

		static bool throwException;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			if (throwException)
			{
				throw new Exception();
			}

			return new DummyPlugIn1(businessEntity);
		}

		protected override ZModulePlugin GetModulePluginCore(ZFilterGridModule module)
		{
			if (throwException)
			{
				throw new Exception();
			}

			return new DummyModulePlugin1();
		}
	}
}
