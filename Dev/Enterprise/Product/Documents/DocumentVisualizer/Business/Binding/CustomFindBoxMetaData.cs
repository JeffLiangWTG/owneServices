using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class CustomFindBoxMetaData : DynamicMetaData
	{
		public CustomFindBoxMetaData(Func<object> customFindBoxProvider, bool showDescription = true)
			: base(Identifier, customFindBoxProvider)
		{
			this.CustomFindBoxProvider = customFindBoxProvider;
			this.ShowDescription = showDescription;
		}

		public const string Identifier = "CustomFindBox";
		public Func<object> CustomFindBoxProvider { get; private set; }
		public bool ShowDescription { get; private set; }
	}
}
