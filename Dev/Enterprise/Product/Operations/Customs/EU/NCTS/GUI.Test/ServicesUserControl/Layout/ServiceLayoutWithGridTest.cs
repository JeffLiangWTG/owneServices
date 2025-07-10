using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(ServiceLayoutWithGrid))]
	sealed class ServiceLayoutWithGridTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		protected override Type ExpectedGridUserControlType => typeof(ServicesGridUserControl);

		protected override int ControlBagCount => 1;

		static IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (ServiceControlBag.Instance.ServiceTypeDropEdit, ControlWidthClass.Long);
				yield return (ServiceControlBag.Instance.ContractorFindBox, ControlWidthClass.Long);
				yield return (ServiceControlBag.Instance.ServiceLocationAddressControl, ControlWidthClass.Auto);
				yield return (ServiceControlBag.Instance.SubLocationTextBox, ControlWidthClass.Auto);
				yield return (ServiceControlBag.Instance.BookedDateEdit, ControlWidthClass.Auto);
				yield return (ServiceControlBag.Instance.CompletedDateEdit, ControlWidthClass.Auto);
				yield return (ServiceControlBag.Instance.RateAndCurrencyCalcFindBox, ControlWidthClass.Auto);
				yield return (ServiceControlBag.Instance.MeasurementBasisDropEdit, ControlWidthClass.Auto);
				yield return (ServiceControlBag.Instance.NotesTextBox, ControlWidthClass.Long);
			}
		}

		static IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (ServiceControlBag.Instance.ServiceCountCalcEdit, ControlWidthClass.Auto);
				yield return (ServiceControlBag.Instance.DurationTimeEdit, ControlWidthClass.Auto);
				yield return (ServiceControlBag.Instance.ReferenceTextBox, ControlWidthClass.Auto);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new ServiceLayoutBuilder<NctsHeader>();
	}
}
