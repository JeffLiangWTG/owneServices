using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Core
{
	public delegate string StringModifier(string[] str);

	[WTG.StaticAnalysis.Annotation.Immutable]
	public class ModifiedMultilingualString : MultilingualString
	{
		public ModifiedMultilingualString(StringModifier modifier, params MultilingualString[] strings)
		{
			Argument.NotNull(modifier, nameof(modifier));
			Argument.NotNull(strings, nameof(strings));
			this.strings = strings;
			this.modifier = modifier;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Baseline")]
		readonly MultilingualString[] strings;
		readonly StringModifier modifier;

		public IEnumerable<MultilingualString> Strings
		{
			get { return strings; }
		}

		public override string ToString()
		{
			return ToString(Res.CurrentLanguage);
		}

		public override string ToString(string language)
		{
			return invokeModifier(strings.Select(s => s == null ? null : s.ToString(language)).ToArray());
		}

		public override string GetUnresolvedString()
		{
			return invokeModifier(strings.Select(s => s == null ? null : s.GetUnresolvedString()).ToArray());
		}

		string invokeModifier(string[] str)
		{
			Argument.NotNull(str, nameof(str));
			var result = modifier(str);
			return result;
		}
	}
}
