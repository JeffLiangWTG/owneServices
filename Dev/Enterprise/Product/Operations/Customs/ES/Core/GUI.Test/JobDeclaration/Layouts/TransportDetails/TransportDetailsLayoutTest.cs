using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(TransportDetailsLayout))]
	sealed class TransportDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
			}
		}

		protected override int ControlBagCount => 3;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDetailsLayoutBuilder();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, ControlWidthClass.Long);
				yield return (TransportDetailsControlBag.Instance.MasterBillAndIATAUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, ControlWidthClass.Auto);
				yield return (EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandModeAndTypeOfIdUserControl, ControlWidthClass.Auto);
				yield return (EU.GUI.TransportDetailsControlBag.Instance.InlandTransportDetailsUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.TransportInlandRailUserControl, ControlWidthClass.Auto);
				yield return (EU.GUI.TransportDetailsControlBag.Instance.AdditionalWagonNumbersUserControl, ControlWidthClass.Auto);
			}
		}

		public void TestAddControlBehaviour()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new JobDeclarationUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var transportUserControl = control.TransportDetailsLayoutPanel.FindSingle<Control>(Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl.ControlName);
				var transportIDTextBox = (ZTextBox)transportUserControl.Controls.Find("TransportIDTextBox", true).Single();

				var vesselUserControl = control.TransportDetailsLayoutPanel.FindSingle<Control>(EU.GUI.TransportDetailsControlBag.Instance.VesselUserControl.ControlName);
				var vesselCodeFindBox = (ZCodeFindBox)vesselUserControl.Controls.Find("VesselCodeFindBox", true).Single();

				var additionalWagonNumbersUserControl = control.TransportDetailsLayoutPanel.FindSingle<Control>(EU.GUI.TransportDetailsControlBag.Instance.AdditionalWagonNumbersUserControl.ControlName);
				var additionalWagonNumbersButton = (ZButton)additionalWagonNumbersUserControl.Controls.Find("AdditionalWagonNumbersButton", true).Single();

				CombineAssertions(() =>
				{
					AssertEquals("TransportIDTextBox Caption", "[21] Transport ID", transportIDTextBox.CaptionResourceString.Caption);
					AssertEquals("VesselCodeFindBox Caption", "[21] Vessel", vesselCodeFindBox.CaptionResourceString.Caption);
					AssertEquals("AdditionalWagonNumbersButton Alignement", DockStyle.Right, additionalWagonNumbersButton.Dock);
				});
			}
		}
	}
}
