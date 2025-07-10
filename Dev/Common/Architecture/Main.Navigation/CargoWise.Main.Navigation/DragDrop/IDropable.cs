using System;

namespace CargoWise.Main.Navigation.DragDrop;
public interface IDropable
{
	Type DataType { get; }
	void Drop(object data, int index = -1, bool insertAbove = false);
}
