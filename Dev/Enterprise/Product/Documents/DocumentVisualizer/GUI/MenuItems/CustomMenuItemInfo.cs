using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.GUI
{
	public sealed class CustomMenuItemInfo : IMenuItemInfo
	{
		public string Name { get; set; }
		public Action<BusinessObject> OnClick { get; set; }
		public Func<BusinessObject, bool> IsApplicable { get; set; }
	}
}
