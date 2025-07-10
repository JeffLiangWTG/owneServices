using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebThemeRegistryDataType : RegistryDataTypeWithJsonSerializer<WebTrackerTheme[]>
	{
		public WebThemeRegistryDataType()
			: base(RegistryDataTypes.Codes.Binary, Array.Empty<WebTrackerTheme>())
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new WebThemeEditorInfo();
		}

		protected override WebTrackerTheme[] CloneValue(WebTrackerTheme[] value)
		{
			var clonedThemeArray = new WebTrackerTheme[value.Length];
			for (int i = 0; i < value.Length; ++i)
			{
				clonedThemeArray[i] = new WebTrackerTheme(value[i].Url, value[i].Code);
			}
			return clonedThemeArray;
		}
	}

	[Serializable]
	public class WebTrackerTheme
	{
		public WebTrackerTheme(string url, string code)
		{
			Url = url;
			Code = code;
		}

		public string Url { get; set; }
		public string Code { get; set; }

		public override bool Equals(object obj)
		{
			var o = obj as WebTrackerTheme;
			return o != null && o.Url == Url && o.Code == Code;
		}

		public override int GetHashCode()
		{
			return Url.GetHashCode() ^ Code.GetHashCode();
		}
	}
}
