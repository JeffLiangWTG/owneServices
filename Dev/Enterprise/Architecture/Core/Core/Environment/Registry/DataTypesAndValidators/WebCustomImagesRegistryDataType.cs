using System;
using System.Linq;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebCustomImagesRegistryDataType : RegistryDataTypeWithJsonSerializer<WebTrackerCustomImage[]>
	{
		public WebCustomImagesRegistryDataType()
			: base(RegistryDataTypes.Codes.Binary, Array.Empty<WebTrackerCustomImage>())
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new WebCustomImagesEditorInfo();
		}

		protected override WebTrackerCustomImage[] CloneValue(WebTrackerCustomImage[] value)
		{
			var clonedImageArray = new WebTrackerCustomImage[value.Length];

			for (var i = 0; i < value.Length; ++i)
			{
				clonedImageArray[i] = new WebTrackerCustomImage(value[i].Name, value[i].Url, value[i].Data);
			}

			return clonedImageArray;
		}
	}

	[Serializable]
	public class WebTrackerCustomImage
	{
		public WebTrackerCustomImage(string name, string url, byte[] data)
		{
			Name = name;
			Url = url;
			Data = data;
		}

		public string Name { get; set; }
		public string Url { get; set; }
		public byte[] Data { get; set; }

		public static readonly string[] SupportedFileTypes = { ".bmp", ".png", ".gif", ".jpg", ".jpeg" };

		public static bool IsSupportedFileType(string fileExtension)
		{
			return SupportedFileTypes.Contains(fileExtension);
		}
	}
}
