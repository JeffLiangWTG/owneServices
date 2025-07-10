using System.Windows.Forms;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusTempStorageRegPremises = Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(ZChildForm))]
	public sealed class TSCustomsNumberViewStmNumsUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => Form;

		ZChildForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZChildForm(Premises);
					form.CaptionRenderingEnabled = true;
					form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 712);
					var control = new TSCustomsNumberViewStmNumsUserControl(Provider.CustomsNumberWrappers);
					control.Name = "TSCustomsNumberViewStmNumsUserControl";
					control.Dock = DockStyle.Fill;
					form.Controls.Add(control);
					form.SetDataBinding(Premises, "");
				}
				return form;
			}
		}
		ZChildForm form;

		TSCustomsNumberViewStmNumsBusinessProvider Provider => Premises.NumberProvider;

		CusTempStorageRegPremises Premises => premises ??= Factory.New<CusTempStorageRegPremises>();
		CusTempStorageRegPremises premises;
	}
}
