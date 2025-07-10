using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EntryInstructionBasicDetailsControlBag))]
	sealed class EntryInstructionBasicDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override ControlBag GetControlBagForTesting() => EntryInstructionBasicDetailsControlBag.Instance;
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(EntryInstructionBasicDetailsControlBag.LocationOfGoodsUserControl);
				yield return nameof(EntryInstructionBasicDetailsControlBag.ToWarehouseUserControl);
				yield return nameof(EntryInstructionBasicDetailsControlBag.ToWarehouseLabel);
				yield return nameof(EntryInstructionBasicDetailsControlBag.FromWarehouseLabel);
				yield return nameof(EntryInstructionBasicDetailsControlBag.FromWarehouseUserControl);
				yield return nameof(EntryInstructionBasicDetailsControlBag.ToWarehouseTypeTextBox);
				yield return nameof(EntryInstructionBasicDetailsControlBag.ToWarehouseCodeTextBox);
				yield return nameof(EntryInstructionBasicDetailsControlBag.FromWarehouseTypeTextBox);
				yield return nameof(EntryInstructionBasicDetailsControlBag.FromWarehouseCodeTextBox);
				yield return nameof(EntryInstructionBasicDetailsControlBag.NewOwnerOrganisationControl);
				yield return nameof(EntryInstructionBasicDetailsControlBag.AcceptanceDateEdit);
				yield return nameof(EntryInstructionBasicDetailsControlBag.RequestedDocumentsGroupBox);
				yield return nameof(EntryInstructionBasicDetailsControlBag.ToWarehouseAddressControl);
				yield return nameof(EntryInstructionBasicDetailsControlBag.FromWarehouseAddressControl);
			}
		}
	}
}
