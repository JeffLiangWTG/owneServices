using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(CommonTransferDetailsControlBag))]
	sealed class CommonTransferDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(CommonTransferDetailsControlBag.DestinationPortCodeFindBox);
				yield return nameof(CommonTransferDetailsControlBag.TransferTypeDropEdit);
				yield return nameof(CommonTransferDetailsControlBag.CarrierAddressControl);
				yield return nameof(CommonTransferDetailsControlBag.CarrierIDTextBox);
				yield return nameof(CommonTransferDetailsControlBag.OnwardCarrierCodeFindBox);
				yield return nameof(CommonTransferDetailsControlBag.DestinationWarehouseAddressControl);
				yield return nameof(CommonTransferDetailsControlBag.DestinationWarehouseIDTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => CommonTransferDetailsControlBag.Instance;
	}
}
