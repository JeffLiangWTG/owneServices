using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Customs
{
	public class EntryChargeType : CodeDescriptionPair
	{
		public EntryChargeType(EntryChargeTypeList parentList, string code, string description, bool isPaidWhenMessageClears, ZString parentCodeForGSTOnARInvoice)
			: this(parentList, code, (NoResString)description, isPaidWhenMessageClears, parentCodeForGSTOnARInvoice)
		{ }

		public EntryChargeType(EntryChargeTypeList parentList, string code, MultilingualString description, bool isPaidWhenMessageClears, ZString parentCodeForGSTOnARInvoice)
			: base(code, description)
		{
			ParentList = parentList;
			IsPaidWhenMessageClears = isPaidWhenMessageClears;
			ParentCodeForGSTOnARInvoice = parentCodeForGSTOnARInvoice;
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Exceptional case")]
		protected readonly EntryChargeTypeList ParentList;
		public readonly bool IsPaidWhenMessageClears;
		public readonly ZString ParentCodeForGSTOnARInvoice;

		public bool IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing => !ParentCodeForGSTOnARInvoice.IsEmpty;

		public EntryChargeTypeSetting GetChargeTypeSpecificRegistrySettings() => ParentList?.GetChargeTypeSpecificRegistrySetting(ChargeCodeForRating);

		public string ChargeCodeForRating => IsRolledUpIntoAnotherChargeCodeAsGSTAtTimeOfInvoicing ? ParentCodeForGSTOnARInvoice.ToString() : Code;
	}
}
