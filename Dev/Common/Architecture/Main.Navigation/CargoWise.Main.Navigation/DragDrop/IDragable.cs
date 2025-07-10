using System;

namespace CargoWise.Main.Navigation.DragDrop;

public interface IDragable
{
	Type DataType { get; }
	void Remove(object i);
}
