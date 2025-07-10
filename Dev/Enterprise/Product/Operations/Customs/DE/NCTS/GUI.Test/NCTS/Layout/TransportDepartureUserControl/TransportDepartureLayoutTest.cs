using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(TransportDepartureLayout))]
	public sealed class TransportDepartureLayoutTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (TransportDepartureControlBag.Instance.InlandTransportModeDropEdit, ControlWidthClass.Long);
				yield return (TransportDepartureControlBag.Instance.TransportAtDepartureTypeDropEdit, ControlWidthClass.Long);
				yield return (TransportDepartureControlBag.Instance.TransportAtDepartureTextBox, ControlWidthClass.Auto);
				yield return (TransportDepartureControlBag.Instance.VesselCodeFindBox, ControlWidthClass.Auto);
				yield return (TransportDepartureControlBag.Instance.TransportAtDepartureTrailer1RegNoTextBox, ControlWidthClass.Auto);
				yield return (TransportDepartureControlBag.Instance.TransportAtDepartureTrailer2RegNoTextBox, ControlWidthClass.Auto);
				yield return (TransportDepartureControlBag.Instance.AdditionalWagonNumbersButton, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (TransportDepartureControlBag.Instance.TransportAtDepartureCountryCodeFindBox, ControlWidthClass.LongNoCaption);
				yield return (TransportDepartureControlBag.Instance.VesselCountryCodeFindBox, ControlWidthClass.LongNoCaption);
				yield return (TransportDepartureControlBag.Instance.TransportAtDepartureTrailer1NationalityCodeFindBox, ControlWidthClass.LongNoCaption);
				yield return (TransportDepartureControlBag.Instance.TransportAtDepartureTrailer2NationalityCodeFindBox, ControlWidthClass.LongNoCaption);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransportDepartureLayoutBuilder<NctsDepartureMovementHeader>();
	}
}
