using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(FtpJobConfigControl))]
	sealed class FtpJobConfigControlTest : ZFormBasherTest
	{
		public override Type FormToBashType
		{
			get { return typeof(ZEmptyFormForBasherTest); }
		}

		protected override Form GetFormToBashCore()
		{
			var result = new ZEmptyFormForBasherTest();

			result.MinimumSize = new Size(1024, 600);
			result.Size = new Size(1024, 600);
			result.CaptionRenderingEnabled = true;

			var control = GetNewControl();
			control.CaptionRenderingEnabled = true;
			result.Controls.Add(control);

			var businessEntity = GetNewBusinessEntity();
			control.SetDataBinding(businessEntity, "");

			return result;
		}

		ZUserControl GetNewControl()
		{
			return (ZUserControl)Activator.CreateInstance(typeof(FtpJobConfigControl), null);
		}

		IBusiness GetNewBusinessEntity()
		{
			if (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
			{
				return new DummyStmServiceTask(Factory);
			}

			return new DummyServiceTaskSchedule(Factory);
		}
	}
}
