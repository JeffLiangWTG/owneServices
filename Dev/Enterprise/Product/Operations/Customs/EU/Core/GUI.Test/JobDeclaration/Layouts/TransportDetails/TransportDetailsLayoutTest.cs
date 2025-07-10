using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Declaration.Testing
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

		protected override int ControlBagCount => 2;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDetailsLayoutBuilder<JobDeclaration>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.OverrideValuesCheckBox, ControlWidthClass.Long);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.MasterBillTextBox, ControlWidthClass.Medium);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.OceanBillTextBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.VoyageAndNationalityUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportIDAndNationalityUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfLoadingUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportDetailsPortOfLoadingWithIATAUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfFirstArrivalUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.PortOfDischargeUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeparatorUserControl, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.InlandTransportDetailsUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.InlandModeOfTransportDropEdit, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRoadUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandAirUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandInlandWaterwaysUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandOwnPropulsionUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandRailUserControl, ControlWidthClass.Auto);
				yield return (Customs.GUI.TransportDetailsControlBag.Instance.TransportInlandSeaUserControl, ControlWidthClass.Auto);
				yield return (TransportDetailsControlBag.Instance.AdditionalWagonNumbersUserControl, ControlWidthClass.Auto);
			}
		}
	}
}
