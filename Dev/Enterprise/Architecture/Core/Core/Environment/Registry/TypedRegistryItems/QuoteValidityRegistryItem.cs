using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	//The QuoteValidityRegistryItem represents the default time period a quote can be valid for.
	//This is stored as an integer for backwards compatibility
	public class QuoteValidityRegistryItem : StronglyTypedRegistryItem<int>
	{
		public const int BlankValue = int.MaxValue - 10;
		public QuoteValidityRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public QuoteValidityRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, int defaultValue)
			: this(name, category, caption, hint, null, storage, options, defaultValue)
		{
		}

		public QuoteValidityRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, NumericRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, int defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new IntRegistryDataType(), editorInfo, storage, options, defaultValue))
		{
		}

		//When HasExpiry is false, the default quote validity period has no end date.Otherwise, the date is given by Value number of months from today
		public bool HasExpiry
		{
			get { return ValidMonthsFromToday != BlankValue; }
		}

		//When true, the default quote validity period will continue to the end of the month Value specifies.
		//For example, today is 10 of June.Value = 0, TillEndOfMonth = True means it is valid until the end of June.
		//Value = -1, TillEndOfMonth = True, means valid until end of July.
		//Finally, Value = 1, TillEndOFMonth = False means valid until 10 of July.
		public bool TillEndOfMonth
		{
			get { return Value <= 0; }
		}

		//This returns a number of months a period of a quote would be valid
		public int ValidMonthsFromToday
		{
			get { return Math.Abs(Value); }
		}
	}
}
