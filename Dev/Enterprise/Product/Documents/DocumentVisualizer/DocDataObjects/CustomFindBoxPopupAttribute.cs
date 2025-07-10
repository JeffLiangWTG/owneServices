using System;
using CargoWise.Application;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class CustomFindBoxPopupAttribute : Attribute
	{
		public CustomFindBoxPopupAttribute(Type customPopupType, bool showDescription = true)
		{
			this.customPopupType = customPopupType;
			this.ShowDescription = showDescription;
		}

		readonly Type customPopupType;
		public bool ShowDescription { get; private set; }

		public object GetInstance() => ObjectFactory.Get(customPopupType.Name);
	}
}
