using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(DefaultTransferDetailsLayouts))]
	sealed class DefaultTransferDetailsLayoutsTest : LayoutsAbstractTest
	{
		protected override int ControlBagCount => 1;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new TransferDetailsLayoutBuilder<AsycudaTransferHeader>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonTransferDetailsControlBag.Instance.DestinationPortCodeFindBox, ControlWidthClass.Long);
				yield return (CommonTransferDetailsControlBag.Instance.TransferTypeDropEdit, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonTransferDetailsControlBag.Instance.CarrierAddressControl, ControlWidthClass.Long);
				yield return (CommonTransferDetailsControlBag.Instance.CarrierIDTextBox, ControlWidthClass.Long);
				yield return (CommonTransferDetailsControlBag.Instance.OnwardCarrierCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonTransferDetailsControlBag.Instance.DestinationWarehouseAddressControl, ControlWidthClass.Long);
				yield return (CommonTransferDetailsControlBag.Instance.DestinationWarehouseIDTextBox, ControlWidthClass.Long);
			}
		}
	}
}
