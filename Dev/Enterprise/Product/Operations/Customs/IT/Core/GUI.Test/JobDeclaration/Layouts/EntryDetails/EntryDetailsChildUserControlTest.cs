using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

abstract class EntryDetailsChildUserControlTest<TUserControl> : TestCaseWithFactory
	where TUserControl : ZUserControl, IDisposable
{
	public void TestControls()
	{
		var controlNames = ControlNames ?? Array.Empty<(string, Type)>();
		if (!controlNames.Any())
		{
			Assert("No control names", false);
		}

		CombineAssertions(() =>
		{
			foreach (var (controlName, controlType) in ControlNames)
			{
				var childControl = Control.FindSingleOrDefault<Control>(controlName);
				AssertNotNull($"Control - {controlName}", childControl);
				AssertType($"Control Type for {controlName}", controlType, childControl);
			}
		});
	}

	protected override void TearDown()
	{
		base.TearDown();
		Control.Dispose();
	}

	protected override void SetUp()
	{
		base.SetUp();
		Control = CreateNewControl();
	}

	protected virtual TUserControl CreateNewControl()
	{
		return Activator.CreateInstance<TUserControl>();
	}

	protected abstract IEnumerable<(string, Type)> ControlNames { get; }

	protected TUserControl Control { get; private set; }
}
