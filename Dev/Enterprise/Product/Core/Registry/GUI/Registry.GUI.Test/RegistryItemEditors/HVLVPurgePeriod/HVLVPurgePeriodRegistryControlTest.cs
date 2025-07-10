using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(HVLVPurgePeriodRegistryControl))]
	sealed class HVLVPurgePeriodRegistryControlTest : Testing.RegistryZUserControlTestCase
	{
		public void TestControlBindings()
		{
			using (ZForm form = new ZForm())
			using (RegistryZUserControl control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				var isEnableCheckBox = form.Controls.Find("zCheckBox1", true).First() as ZCheckBox;
				var purgePeriodCalBox = form.Controls.Find("zCalcEdit1", true).First() as ZCalcEdit;

				CombineAssertions(() =>
				{
					AssertEquals("IsEnable Binded", "IsEnabled", control.BindingSource.GetBindingMember(isEnableCheckBox));
					AssertEquals("PurgePeriod Binded", "PurgePeriod", control.BindingSource.GetBindingMember(purgePeriodCalBox));
				});
			}
		}

		#region Implementation

		protected override IBusiness GetNewBusinessEntity()
		{
			return new HVLVPurgePeriod();
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((HVLVPurgePeriodRegistryControl)control).ReadOnly;
		}

		#endregion
	}
}
