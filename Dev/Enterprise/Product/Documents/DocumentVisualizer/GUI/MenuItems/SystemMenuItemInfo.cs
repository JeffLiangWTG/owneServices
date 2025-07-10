using System;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.GUI
{
	public sealed class SystemMenuItemInfo : IMenuItemInfo
	{
		public ZGuid ID { get; set; }
		public Func<bool> Precondition { get; set; }
	}
}
