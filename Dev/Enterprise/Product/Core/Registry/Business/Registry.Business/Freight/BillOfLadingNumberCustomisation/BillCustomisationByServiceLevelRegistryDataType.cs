using System;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.BillOfLadingNumberCustomisationByServiceLevelRegistryItemEditor, Enterprise.Registry.GUI")]
	public class BillCustomisationByServiceLevelRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BillOfLadingNumberCustomisationsByServiceLevel>, IBillCustomisationRegistryDataType
	{
		public BillCustomisationByServiceLevelRegistryDataType()
			: this(null) { }

		public BillCustomisationByServiceLevelRegistryDataType(BillOfLadingNumberCustomisationsByServiceLevel defaultValue)
			: base(defaultValue ?? new BillOfLadingNumberCustomisationsByServiceLevel())
		{
			Categories = NumberCustomisationElementCategories.Default;
		}

		public string FountainPrefix
		{
			get { return fountainPrefix; }
			set
			{
				fountainPrefix = value;
				DefaultValue.PrefixLength = PrefixLength;
			}
		}
		string fountainPrefix;

		public MultilingualString GeneratedNumberName { get; set; }
		public MultilingualString SequenceNumberName { get; set; }
		public int MaxLength { get; set; }

		public NumberCustomisationElementCategories Categories
		{
			get { return DefaultValue.Categories; }
			set { DefaultValue.Categories = value; }
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

		int PrefixLength => FountainPrefix?.Length ?? 1;

		protected override BillOfLadingNumberCustomisationsByServiceLevel DeserialiseCore(byte[] value)
		{
			var customisation = base.DeserialiseCore(value);
			customisation.Categories = Categories;
			customisation.AllowNonAlphanumericCharacters = AllowNonAlphanumericCharacters;
			customisation.EnableMacroInsertion = EnableMacroInsertion;
			customisation.PrefixLength = PrefixLength;
			customisation.MaxAllowedLength = MaxLength;
			return customisation;
		}

		protected override bool ValuesAreEqualCore(BillOfLadingNumberCustomisationsByServiceLevel a, BillOfLadingNumberCustomisationsByServiceLevel b)
		{
			return a.AllowNonAlphanumericCharacters == b.AllowNonAlphanumericCharacters &&
				   a.EnableMacroInsertion == b.EnableMacroInsertion &&
				   a.PrefixLength == b.PrefixLength &&
				   a.Categories == b.Categories &&
				   a.BillOfLadingNumberCustomisations.ContainsSameElementsInAnyOrder(b.BillOfLadingNumberCustomisations);
		}

		protected override void ValidateCore(IRegistryItem registryItem, BillOfLadingNumberCustomisationsByServiceLevel proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (proposedValue.BillOfLadingNumberCustomisations["ALL"] == null)
			{
				throw new RegistryValidationException(Res.GetString("00383c06-6ce3-4b22-ae20-a113147b1036", "System defined Service Level 'ALL' does not exist. Please create 'ALL' Service Level."));
			}
		}
	}
}
