using System;
using CargoWise.Common;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Core
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	[CLSCompliant(true)]
	public abstract class MultilingualString : ZMultilingual, IMultilingualString
	{
		public static implicit operator string(MultilingualString value)
		{
			return value == null ? null : value.ToString();
		}

		public static implicit operator ZString(MultilingualString value)
		{
			return value == null ? ZString.Empty : (ZString)value.ToString();
		}

		public override bool Equals(object obj)
		{
			return ((obj is string || obj is ZString) && this.ToString() == obj.ToString()) ||
				(obj is MultilingualString && this.GetUnresolvedString() == ((MultilingualString)obj).GetUnresolvedString());
		}

		public bool EqualsUnresolvedOrLocalized(string value, bool ignoreCase)
		{
			var unresolvedString = GetUnresolvedString();
			var localizedString = ToString();
			if (ignoreCase)
			{
				unresolvedString = unresolvedString?.ToUpper();
				localizedString = localizedString?.ToUpper();
				value = value?.ToUpper();
			}

			return unresolvedString == value || localizedString == value;
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public abstract string ToString(string language);

		public abstract string GetUnresolvedString();

		public override IZType GetLocalizedValue(string language)
		{
			return (ZString)ToString(language);
		}

		public override IZType GetUnresolvedValue()
		{
			return (ZString)GetUnresolvedString();
		}

		public MultilingualString Replace(string oldValue, string newValue)
		{
			return new ModifiedMultilingualString((str) =>
			{
				return str[0].Replace(oldValue, newValue);
			},
				this
			);
		}

		public static MultilingualString Join(string seperator, params MultilingualString[] value)
		{
			Argument.NotNull(value, nameof(value));
			return new ModifiedMultilingualString((str) =>
			{
				return string.Join(seperator, str);
			},
				value
			);
		}

		public MultilingualString Trim()
		{
			return new ModifiedMultilingualString((str) =>
			{
				return str[0].Trim();
			},
				this
			);
		}

		public MultilingualString Pluralize()
		{
			return new ModifiedMultilingualString((str) =>
			{
				return Grammar.Instance.Pluralize(str[0]);
			},
				this);
		}
	}
}
