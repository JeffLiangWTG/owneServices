using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.ZArchitecture.Core.Utilities;

namespace Enterprise.ZArchitecture.Core
{
	[Flags]
	public enum OFontTypes
	{
		Unknown = 0,

		Small = 1 << 0,
		Medium = 1 << 1,
		Larger = 1 << 2,
		Largest = 1 << 3,

		Bold = 1 << 4,
		SansSerif = 1 << 5,

		Normal = Medium,
		Bolded = Medium | Bold,
		Header = Largest | Bold,
		SubText = Small | Bold,
		RichText = Larger | SansSerif,
	}

	public static class OFont
	{
		public static Font GetFont() => GetFont(OFontTypes.Normal);
		public static Font GetFontBold() => GetFont(OFontTypes.Bolded);
		public static Font GetSubTextFont() => GetFont(OFontTypes.SubText);
		public static Font GetHeaderFont() => GetFont(OFontTypes.Header);
		public static Font GetRichTextBoxFont() => GetFont(OFontTypes.RichText);

		static OFontTypes[] SizeFlags => sizeFlags ?? (sizeFlags = new[] { OFontTypes.Small, OFontTypes.Medium, OFontTypes.Larger, OFontTypes.Largest });
		[ThreadStatic]
		static OFontTypes[] sizeFlags;

		const int SmallFontSize = 6;
		const int NormalFontSize = 8;
		const int LargerFontSize = 10;
		const int LargestFontSize = 12;

		public static Font GetFont(OFontTypes type)
		{
			Font result;
			if (!FontsCache.TryGetValue(type, out result))
			{
				result = CreateFont(type);
				FontsCache.Add(type, result);
			}

			return result;
		}

		public static OFontTypes FindFontType(Font f)
		{
			var type = OFontTypes.Unknown;
			switch ((int)Round((decimal)f.Size, 0))
			{
				case SmallFontSize:
					type |= OFontTypes.Small;
					break;
				case NormalFontSize:
					type |= OFontTypes.Normal;
					break;
				case LargerFontSize:
					type |= OFontTypes.Larger;
					break;
				case LargestFontSize:
					type |= OFontTypes.Largest;
					break;
				default:
					return OFontTypes.Unknown;
			}

			if (f.Bold)
			{
				type |= OFontTypes.Bold;
			}

			if (f.Name == FontFamily.GenericSansSerif.Name)
			{
				type |= OFontTypes.SansSerif;
			}

			return type;
		}

		public static int GetDefaultFontSize(OFontTypes type)
		{
			var typeFlag = GetSizeFlag(type);
			return GetSizeFromSizeFlag(typeFlag);
		}

		public static int GetSizeFromSizeFlag(OFontTypes type)
		{
			switch (type)
			{
				case OFontTypes.Small:
					return SmallFontSize;
				case OFontTypes.Normal:
					return NormalFontSize;
				case OFontTypes.Larger:
					return LargerFontSize;
				case OFontTypes.Largest:
					return LargestFontSize;
				default:
					throw new ArgumentException("Font type does not have defined size. Type: " + type);
			}
		}

		static OFontTypes GetSizeFlag(OFontTypes type)
		{
			try
			{
				return SizeFlags.Single(flag => type.HasFlag(flag));
			}
			catch (InvalidOperationException)
			{
				throw new ArgumentException("Exactly one size should be specified. Type: " + type);
			}
		}

		static Font CreateFont(OFontTypes type)
		{
			var sizeFlag = GetSizeFlag(type);
			var size = GetSizeFromSizeFlag(sizeFlag);
			var style = type.HasFlag(OFontTypes.Bold) ? FontStyle.Bold : FontStyle.Regular;
			var name = type.HasFlag(OFontTypes.SansSerif) ? FontFamily.GenericSansSerif.Name : NormalFontName;

			try
			{
#if DEBUG
				if (Globals.IsTest)
				{
					BeforeNewFontCreationForTesting?.Invoke();
				}
#endif

				return new Font(name, size, style);
			}
			catch (ArgumentException) when (type == OFontTypes.RichText) //Font 'Microsoft Sans Serif' does not support style 'Regular'.
			{
				if (!hasReported)
				{
					string computerName = "_";
					try
					{
						computerName = System.Environment.MachineName;
					}
					catch (InvalidOperationException) { }

					Globals.Message.ShowError(SourceGenerated.ResString.GetMultilingualString("412e08b5-7a57-4736-8d04-e7136e7cfe7e", "The font {0} is corrupt or missing and needs to be re-installed for rich text boxes to correctly function. If this needs to be reported to a system administrator, the name of this computer is {1}.", name, computerName));
					hasReported = true;
				}

				return GetFont(OFontTypes.Normal);
			}
			catch (ArgumentException) when (Globals.IsWeb && (style == FontStyle.Bold))
			{
				return GetFont(OFontTypes.Normal);
			}
		}
		static bool hasReported;

#if DEBUG
		[ThreadSafe]
		public static Action BeforeNewFontCreationForTesting = () => { };
#endif

		[ThreadStatic]
		static Dictionary<OFontTypes, Font> fontsCache;
		static Dictionary<OFontTypes, Font> FontsCache => LazyInitializer.EnsureInitialized(ref fontsCache);

		public static void ResetFonts()
		{
			if (fontsCache != null)
			{
				foreach (var font in FontsCache.Values)
				{
					font.Dispose();
				}

				fontsCache = null;
			}
		}

		public static void UseFontFromReg()
		{
			isRegFontAvailable = true;
			fontsCache = null;
		}

		[ThreadSafe]
		static bool isRegFontAvailable = false;

		public static string NormalFontName
		{
			get
			{
				if (isRegFontAvailable)
				{
					try
					{
						return EnvProxy.Instance.Registry.SystemFontRegItem;
					}
					catch (CommunicationException) { }
				}
				return DefaultFontName;
			}
		}

		public static string NavigationMenuFontName
		{
			get
			{
				if (isRegFontAvailable)
				{
					try
					{
						return EnvProxy.Instance.Registry.NavigationMenuFontRegItem;
					}
					catch (CommunicationException) { }
				}
				return DefaultNavigationMenuFontName;
			}
		}

		public static string NewsAnnouncementFontName
		{
			get
			{
				if (isRegFontAvailable)
				{
					try
					{
						return EnvProxy.Instance.Registry.NewsAnnouncementFontRegItem;
					}
					catch (CommunicationException) { }
				}
				return DefaultNewsAnnouncementFontName;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Font name cannot be translated")]
		public const string DefaultFontName = "Tahoma";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Font name cannot be translated")]
		public const string DefaultNavigationMenuFontName = "Segoe UI";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Font name cannot be translated")]
		public const string DefaultNewsAnnouncementFontName = "Arial";
	}
}
