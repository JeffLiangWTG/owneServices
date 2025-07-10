using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.BillOfLadingNumberCustomisationRegistryItemEditor, Enterprise.Registry.GUI")]
	public class BillCustomisationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BillOfLadingNumberCustomisation>, IBillCustomisationRegistryDataType
	{
		public BillCustomisationRegistryDataType()
			: this(null) { }

		public BillCustomisationRegistryDataType(BillOfLadingNumberCustomisation defaultValue)
			: base(defaultValue ?? new BillOfLadingNumberCustomisation())
		{
			Categories = NumberCustomisationElementCategories.Default;
		}

		public string FountainPrefix { get; set; }
		public MultilingualString GeneratedNumberName { get; set; }
		public MultilingualString SequenceNumberName { get; set; }
		public int MaxLength { get; set; }

		public NumberCustomisationElementCategories Categories
		{
			get { return DefaultValue.Categories; }
			set { DefaultValue.Categories = value; }
		}

		public int PrefixLength
		{
			get { return DefaultValue.PrefixLength; }
			set { DefaultValue.PrefixLength = value; }
		}

		public bool AllowNonAlphanumericCharacters
		{
			get { return DefaultValue.AllowNonAlphanumericCharacters; }
			set { DefaultValue.AllowNonAlphanumericCharacters = value; }
		}

		public bool EnableMacroInsertion
		{
			get { return DefaultValue.EnableMacroInsertion; }
			set { DefaultValue.EnableMacroInsertion = value; }
		}

		public Type MacroType
		{
			get { return macroType; }
			set { macroType = value; }
		}
		Type macroType;

		protected override BillOfLadingNumberCustomisation DeserialiseCore(byte[] value)
		{
			var customisation = base.DeserialiseCore(value);
			customisation.Categories = Categories;
			customisation.PrefixLength = PrefixLength;
			customisation.AllowNonAlphanumericCharacters = AllowNonAlphanumericCharacters;
			customisation.EnableMacroInsertion = EnableMacroInsertion;
			customisation.MaxAllowedLength = MaxLength;
			return customisation;
		}

		protected override bool ValuesAreEqualCore(BillOfLadingNumberCustomisation a, BillOfLadingNumberCustomisation b)
		{
			return Equals(a, b);
		}
	}
}
