using System;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebCustomCssRegistryDataType : RegistryDataTypeWithJsonSerializer<WebTrackerCustomCss[]>
	{
		public WebCustomCssRegistryDataType()
			: base(RegistryDataTypes.Codes.Binary, Array.Empty<WebTrackerCustomCss>())
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new WebCustomCssEditorInfo();
		}

		protected override WebTrackerCustomCss[] CloneValue(WebTrackerCustomCss[] value)
		{
			var clonedCssArray = new WebTrackerCustomCss[value.Length];
			for (int i = 0; i < value.Length; ++i)
			{
				clonedCssArray[i] = new WebTrackerCustomCss(value[i].Url, value[i].Data);
			}
			return clonedCssArray;
		}
	}

	[Serializable]
	public class WebTrackerCustomCss
	{
		public WebTrackerCustomCss(string url, string data)
		{
			Url = url;
			Data = data;
		}

		public string Url { get; set; }
		public string Data { get; set; }

		public override bool Equals(object obj)
			=> obj is WebTrackerCustomCss other && other.Data == Data && other.Url == Url;

		public override int GetHashCode()
			=> Url.GetHashCode() ^ Data.GetHashCode();
	}
}
