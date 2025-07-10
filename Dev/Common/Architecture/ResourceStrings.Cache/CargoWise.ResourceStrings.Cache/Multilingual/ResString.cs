using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Core
{
	/// <summary>
	/// [STOP] Should only used by Generated Bridge Code as per (TODO: provide file link)
	/// </summary>
	[CodeAlive("Referenced by generated code in \\CargoWise\\Shared\\CargoWiseOne.ResourceStrings\\CargoWiseOne.ResourceStrings.Transform.SourceGen\\GeneratedSourceString.cs")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:Use source generated static methods", Justification = "The implementation of the ResourceString")]
	public static class ResStringBridge
	{
		public static ResourceString _GetMultilingualString(UInt16 asmid, string resourceKey, string englishText, params object[] parameters)
			=> ResString._GetMultilingualString(asmid, resourceKey, englishText, parameters);
	}

	public static class ResString
	{
		public static ResourceString _GetMultilingualString(UInt16 asmid, string resourceKey, string englishText, params object[] parameters)
		{
			Argument.NotNull(resourceKey, nameof(resourceKey));
			Argument.NotNull(englishText, nameof(englishText));
			Argument.NotNull(parameters, nameof(parameters)); // Suggested By ReviewBot 
			return new ResourceString(asmid, resourceKey, englishText, parameters);
		}

		public static ResourceString _GetMultilingualString(UInt16 asmid, string resourceKey, string englishText)
		{
			Argument.NotNull(resourceKey, nameof(resourceKey));
			Argument.NotNull(englishText, nameof(englishText));
			return new ResourceString(asmid, resourceKey, englishText, Array.Empty<object>());
		}

		public static ResourceString GetMultilingualString(string resourceKey, string englishText, params object[] parameters)
		{
			Argument.NotNull(resourceKey, nameof(resourceKey));
			Argument.NotNull(englishText, nameof(englishText));
			Argument.NotNull(parameters, nameof(parameters)); // Suggested By ReviewBot 
			return new ResourceString(0, resourceKey, englishText, parameters);
		}

		public static ResourceString GetMultilingualString(string resourceKey, string englishText)
		{
			Argument.NotNull(resourceKey, nameof(resourceKey));
			Argument.NotNull(englishText, nameof(englishText));
			return new ResourceString(0, resourceKey, englishText, Array.Empty<object>());
		}
	}

	[Immutable]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:Use source generated static methods", Justification = "The implementation of the ResourceString")]
	public class ResourceString : MultilingualString, IResString
	{
		internal ResourceString(UInt16 asmid, string resourceKey, string englishText, object[] parameters)
		{
			Argument.NotNull(parameters, nameof(parameters)); // Suggested By ReviewBot
			Argument.NotNull(englishText, nameof(englishText));
			Argument.NotNull(resourceKey, nameof(resourceKey));

			this.asmid = asmid;
			this.resourceKey = resourceKey;
			this.englishText = englishText;
			this.parameters = parameters.Length == 0 ? Array.Empty<string>() : parameters;
		}

		readonly UInt16 asmid;
		readonly string resourceKey;
		readonly string englishText;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Avoid extra allocations for performance reasons, this array will be passed directly into string.Format()")]
		readonly object[] parameters;

		public UInt16 Asmid
		{
			get
			{
				return asmid;
			}
		}

		public string ResourceKey
		{
			get
			{
				return resourceKey;
			}
		}

		public object[] Parameters
		{
			get
			{
				return parameters;
			}
		}

		public string EnglishText => englishText;

		public override string ToString()
		{
			return string.IsNullOrEmpty(resourceKey) ? englishText : Res._GetString(asmid, resourceKey, englishText, parameters);
		}

		public string ToStringWithParameters()
		{
			return string.IsNullOrEmpty(Res.CurrentLanguage) ? ToStringWithParameters(Res.DefaultLanguage) : ToStringWithParameters(Res.CurrentLanguage);
		}

		public string ToStringWithParameters(string language)
		{
			Argument.NotNullOrEmpty(language, nameof(language));
			if (string.IsNullOrEmpty(resourceKey))
			{
				return englishText;
			}

			var result = Res.GetLanguageInstance(language).GetString(asmid, resourceKey);
			return string.IsNullOrEmpty(result) ? englishText : result;
		}

		public override string ToString(string language)
		{
			return string.IsNullOrEmpty(resourceKey)
				? englishText
				: ResourceStrings.Normalize(language) == Res.CurrentLanguage
					? Res._GetString(asmid, resourceKey, englishText, parameters)
					: Res.GetLanguageInstance(language).GetString(asmid, resourceKey, englishText, Array.ConvertAll(parameters, parameter => parameter is MultilingualString ? ((MultilingualString)parameter).ToString(language) : parameter));
		}

		public bool IsLocalized(string language)
		{
			Argument.NotNullOrEmpty(language, nameof(language));
			return ResourceStrings.Normalize(language) == Res.DefaultLanguage || Res.GetLanguageInstance(language).GetData(asmid, resourceKey) != null;
		}

		public override string GetUnresolvedString()
		{
			object[] parameters = new object[this.parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				parameters[i] = this.parameters[i] is MultilingualString ? ((MultilingualString)this.parameters[i]).GetUnresolvedString() : this.parameters[i];
			}
			return StringExtensions.FormatRobustly(ResourceStringsFormatter.Instance, englishText, parameters);
		}

		public class ResourceKeyEqualityComparer : IEqualityComparer<IResString>
		{
			public bool Equals(IResString x, IResString y)
			{
				if (x == null)
				{
					return y == null;
				}

				if (y == null)
				{
					return false;
				}

				return x.ResourceKey == y.ResourceKey;
			}

			public int GetHashCode(IResString obj)
			{
				if (obj?.ResourceKey == null)
				{
					return 0;
				}

				return obj.ResourceKey.GetHashCode();
			}
		}

		public static string NormalizeNewLines(string s)
		{
			Argument.NotNull(s, nameof(s));
			return Regex.Replace(s, "\r\n|\n\r|\n|\r", "\r\n");
		}
	}
}
