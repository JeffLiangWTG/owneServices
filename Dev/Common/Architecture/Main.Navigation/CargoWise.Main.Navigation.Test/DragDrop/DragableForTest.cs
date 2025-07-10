using System;

namespace CargoWise.Main.Navigation.DragDrop.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
class DragableForTest : IDragable
{
	public string Name { get; set; }

	public int Value { get; set; }

	public bool DragHappened { get; internal set; }

	public Type DataType
	{
		get
		{
			return typeof(DragableForTest);
		}
	}

	public void Remove(object i)
	{
		DragHappened = true;
	}
}
