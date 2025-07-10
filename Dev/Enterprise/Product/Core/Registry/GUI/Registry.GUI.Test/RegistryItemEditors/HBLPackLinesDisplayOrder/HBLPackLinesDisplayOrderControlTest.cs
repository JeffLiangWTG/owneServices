using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(HBLPackLinesDisplayOrderControl))]
	sealed class HBLPackLinesDisplayOrderControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new HBLPackLinesDisplayOrderDropEditBussinessObject(new CodeDescriptionPairList());
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((HBLPackLinesDisplayOrderControl)control).ReadOnly;
		}

		[RequiresSTA]
		public void TestPresetPackLinesOrderDropEditCaption()
		{
			using (var form = new ZForm())
			using (var control = GetNewControl())
			{
				form.Controls.Add(control);
				form.Show();

				var businessEntity = GetNewBusinessEntity();
				control.SetDataBinding(businessEntity, null);

				AssertEquals("Order Pack Lines by", ((HBLPackLinesDisplayOrderControl)control).PresetPackLinesOrderDropEdit.CaptionResourceString.Caption);
			}
		}
	}
}
