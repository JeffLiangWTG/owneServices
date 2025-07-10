using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Registry.Business
{
	public static class DepotAddressColorSoundValidation
	{
		public static void ValidateRGBValue(ZPropertyInfo color)
		{
			var rgbValue = (ZString)color.Value;
			if (!rgbValue.IsEmpty)
			{
				if (!HasCorrectFormat(rgbValue))
				{
					color.AddError(Res.GetString("5f2b9ebb-dfe5-4a6d-a4b4-aff56d0e45a1", "RGB value should be in the format of R,G,B"));
				}

				if (!color.HasErrors())
				{
					if (!IsValidColor(rgbValue))
					{
						color.AddError(Res.GetString("f0db1d4b-38b7-468b-876e-8387f8f6ca22", "Invalid color"));
					}
				}
			}
		}

		public static void ValidateOrgHasDepot(ZPropertyInfo organisation, IOrgHeader orgHeader)
		{
			if (!orgHeader.OH_IsPackDepot && !orgHeader.OH_IsRoadFreightDepot && !orgHeader.OH_IsUnpackDepot)
			{
				organisation.AddError(Res.GetString("8198b86d-b528-45e4-aa8c-91240664aa43", "Organization does not have a depot"));
			}
		}

		public static void ValidateSoundFileContent(ZPropertyInfo soundContent)
		{
			if (((ZBlob)soundContent.Value).IsEmpty)
			{
				soundContent.AddError(Res.GetString("d78a83fb-3152-402f-a7e0-c800154cf07c", "Sound file cannot be empty"));
			}
		}

		public static bool IsValidRGBValue(ZString rgb)
		{
			return HasCorrectFormat(rgb) && IsValidColor(rgb);
		}

		static bool HasCorrectFormat(ZString rgb)
		{
			return rgb.ToString().Count(x => x == ',') == 2;
		}

		static bool IsValidColor(ZString rgb)
		{
			var rgbArray = rgb.Split(new char[] { ',' });
			var r = rgbArray[0];
			var g = rgbArray[1];
			var b = rgbArray[2];
			return ValidColorValue(r) && ValidColorValue(g) && ValidColorValue(b);
		}

		static bool ValidColorValue(string value)
		{
			int i;
			if (int.TryParse(value, out i))
			{
				return i >= 0 && i <= 255;
			}
			return false;
		}
	}
}