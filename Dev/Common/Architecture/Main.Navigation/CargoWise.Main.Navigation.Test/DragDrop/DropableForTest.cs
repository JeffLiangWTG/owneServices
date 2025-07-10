using System;
using System.Collections.ObjectModel;

namespace CargoWise.Main.Navigation.DragDrop.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
class DropableForTest : IDropable
{
	public DropableForTest()
	{
		Items = new Collection<DragableForTest> { new DragableForTest { Name = "Item 1", Value = 1 }, new DragableForTest { Name = "Item 2", Value = 2 }, new DragableForTest { Name = "Item 3", Value = 3 } };
	}

	public Collection<DragableForTest> Items { get; set; }

	public bool DropHappened { get; internal set; }

	public Type DataType
	{
		get
		{
			return typeof(DragableForTest);
		}
	}

	public void Drop(object data, int index = -1, bool insertAbove = false)
	{
		DropHappened = true;
	}
}
