using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing;

public abstract class ExitSummaryUserControlForVirtualPropertiesTest<T> : TestCaseWithFactory
	where T : ExitSummaryUserControl, new()
{
	[RequiresSTA]
	public void TestNewTopPanel()
	{
		var exitHeader = Factory.New<CusExitControlHeader>();
		using (var form = new ZForm(exitHeader))
		using (var control = new T())
		{
			form.Controls.Add(control);
			form.Show();

			var newTopPanel = control.NewTopPanel;

			CombineAssertions(() =>
			{
				AssertEquals("DynamicLayoutApplied", DefaultDynamicLayoutApplied, typeof(T).GetProperty("DynamicLayoutApplied", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control));
				if (DefaultDynamicLayoutApplied)
				{
					AssertEquals($"{DefaultDynamicLayoutApplied}-NewTopPanel", true, newTopPanel.Visible);
				}
				else
				{
					AssertEquals($"{DefaultDynamicLayoutApplied}-NewTopPanel", false, newTopPanel.Visible);
				}
			});
		}
	}

	protected virtual ZBool DefaultDynamicLayoutApplied => ZBool.False;
}
