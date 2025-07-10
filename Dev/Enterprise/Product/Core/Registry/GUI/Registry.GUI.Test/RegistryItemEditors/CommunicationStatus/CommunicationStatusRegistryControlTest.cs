using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CommunicationStatusRegistryControl))]
	sealed class CommunicationStatusRegistryControlTest : RegistryZUserControlTestCase
	{
		public void TestData()
		{
			var collection = new CommunicationStatusCollection(false, true);
			using (var control = new CommunicationStatusRegistryControl())
			{
				control.Data = collection;
				AssertEquals(collection, control.Data);
			}
		}

		public void TestControlReadOnly()
		{
			using (var dummyForm = new Form())
			using (var control = new CommunicationStatusRegistryControl())
			{
				dummyForm.Controls.Add(control);

				AssertEquals(false, control.ReadOnly);
				control.ReadOnly = true;
				AssertEquals(true, control.ReadOnly);
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new CommunicationStatusCollection(false, true);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, CargoWise.EntityFramework.IBusiness businessEntity)
		{
			return ((CommunicationStatusRegistryControl)control).CodeDescriptionBoolGrid.ReadOnly;
		}

		protected override RegistryZUserControl GetNewControl()
		{
			return new CommunicationStatusRegistryControl();
		}
	}
}
