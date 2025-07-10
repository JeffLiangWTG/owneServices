using System;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	public static class ResourceStringDataLevels
	{
		public static class Codes
		{
			public const string ShortCaption = "SHO";
			public const string MediumCaption = "MED";
			public const string Caption = "CAP";
			public const string FullDescription = "FUL";
		}

		public static CodeDescriptionPairList List
		{
			get
			{
				var levels = new CodeDescriptionPairList();
				levels.AddPair(Codes.ShortCaption, "Short Caption");
				levels.AddPair(Codes.MediumCaption, "Short Caption");
				levels.AddPair(Codes.Caption, "Caption");
				levels.AddPair(Codes.FullDescription, "Full Description");
				return levels;
			}
		}

		public static string GetCaptionAtLevel(this HelpDataString data, string level)
		{
			switch (level)
			{
				case Codes.Caption: return data.HD_Caption;
				case Codes.FullDescription:	return data.HD_FullDescription;
				case Codes.ShortCaption: return data.HD_ShortCaption;
				case Codes.MediumCaption: return data.HD_MidCaption;
				default:
					throw new InvalidOperationException("Unknown level code");
			}
		}

		public static void SetCaptionAtLevel(this HelpDataString data, string level, string caption)
		{
			switch (level)
			{
				case Codes.Caption: data.HD_Caption = caption; break;
				case Codes.FullDescription: data.HD_FullDescription = caption; break;
				case Codes.ShortCaption: data.HD_ShortCaption = caption; break;
				case Codes.MediumCaption: data.HD_MidCaption = caption; break;
				default:
					throw new InvalidOperationException("Unknown level code");
			}
		}

		public static CodeDescriptionPairList GetCaptions(this HelpDataString data)
		{
			var captions = new CodeDescriptionPairList();
			foreach (ICodeDescription level in List)
			{
				var valueAtLevel = data.GetCaptionAtLevel(level.Code);
				if (!string.IsNullOrEmpty(valueAtLevel))
				{
					captions.AddPair(level.Code, valueAtLevel);
				}
			}
			return captions;
		}

		public static string GetMatchingLevel(this HelpDataString data, string caption, bool hasAccelerators)
		{
			if ((hasAccelerators && data.HD_Caption.Replace("&","") == caption) || (!hasAccelerators && data.HD_Caption == caption)) //strip accelerator keys
			{
				return Codes.Caption;
			}
			else if (data.HD_ShortCaption == caption)
			{
				return Codes.ShortCaption;
			}
			else if (data.HD_MidCaption == caption)
			{
				return Codes.MediumCaption;
			}
			else if (data.HD_FullDescription == caption)
			{
				return Codes.FullDescription;
			}
			else
			{
				return null;
			}
		}

		public static string GetLevel(this HelpDataString data)
		{
			return string.IsNullOrEmpty(data.HD_Caption) ? Codes.FullDescription : Codes.Caption;
		}

		public static string GetCaptionOrFullDescription(this HelpDataString data)
		{
			return data.GetCaptionAtLevel(GetLevel(data));
		}
	}
}
