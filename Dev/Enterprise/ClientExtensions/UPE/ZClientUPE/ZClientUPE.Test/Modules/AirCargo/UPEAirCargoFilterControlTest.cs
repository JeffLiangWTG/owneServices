using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class UPEAirCargoFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			using (TestUPEAirCargoModule module = new TestUPEAirCargoModule())
			using (UPEAirCargoFilterControl filterControl = (UPEAirCargoFilterControl)module.GetNewFilterControl())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		class TestUPEAirCargoModule : UPEAirCargoModule
		{
			public new IFilterControl GetNewFilterControl()
			{
				return base.GetNewFilterControl();
			}
		}
	}
}
