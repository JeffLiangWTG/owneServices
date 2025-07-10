using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public sealed class CustomArrivalCustomerReferenceFormatValidation : AutoCustomArrivalCustomerReferenceFormatValidation
{
	public CustomArrivalCustomerReferenceFormatValidation(AutoCustomArrivalCustomerReferenceFormat parent) : base(parent)
	{
	}

	new CustomArrivalCustomerReferenceFormat Parent => (CustomArrivalCustomerReferenceFormat)base.Parent;

	protected override void CheckAuthorizationLocationCode()
	{
		base.CheckAuthorizationLocationCode();

		var parent = Parent;

		if (parent.CurrentFallbackLevel != null)
		{
			MandatoryValidation.CheckEntered(parent.AuthorizationLocationCodeInfo);
			ListValidation.ErrorIfInvalidCode(parent.AuthorizationLocationCodeInfo);
		}

		PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(parent.AuthorizationLocationCodeInfo);
	}

	protected override void CheckYearOption()
	{
		base.CheckYearOption();
		CheckArrivalCustomerReferenceLength();
		MandatoryValidation.CheckEntered(Parent.YearOptionInfo);
		ListValidation.ErrorIfInvalidCode(Parent.YearOptionInfo);
	}

	protected override void CheckPrefix()
	{
		base.CheckPrefix();
		CheckArrivalCustomerReferenceLength();

		switch (Parent.YearOption)
		{
			case CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix2:
				CheckPrefixOrSuffixLength(Parent.PrefixInfo, 8, () => CustomArrivalCustomerReferenceFormatYearOptionList.Descriptions.Prefix2);
				break;
			case CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix4:
				CheckPrefixOrSuffixLength(Parent.PrefixInfo, 6, () => CustomArrivalCustomerReferenceFormatYearOptionList.Descriptions.Prefix4);
				break;
		}
	}

	protected override void CheckSuffix()
	{
		base.CheckPrefix();
		CheckArrivalCustomerReferenceLength();

		switch (Parent.YearOption)
		{
			case CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Suffix2:
				CheckPrefixOrSuffixLength(Parent.SuffixInfo, 8, () => CustomArrivalCustomerReferenceFormatYearOptionList.Descriptions.Suffix2);
				break;
			case CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Suffix4:
				CheckPrefixOrSuffixLength(Parent.SuffixInfo, 6, () => CustomArrivalCustomerReferenceFormatYearOptionList.Descriptions.Suffix4);
				break;
		}
	}

	void CheckPrefixOrSuffixLength(ZPropertyInfo propertyInfo, int maxLength, Func<MultilingualString> yearOptionDescription)
	{
		if (((ZString)propertyInfo.Value).Length > maxLength)
		{
			propertyInfo.AddError(Res.GetString("87302A61-CDDD-4EEB-B8D2-7BF5A245E799", "{0} must not be longer than {1} characters if '{2}' is selected.", propertyInfo.HumanReadableName, maxLength, yearOptionDescription()));
		}
	}

	protected override void CheckSequenceNumberLength()
	{
		base.CheckSequenceNumberLength();
		CheckArrivalCustomerReferenceLength();

		CompareValidation.CheckWithinRange(Parent.SequenceNumberLengthInfo, 1, 15);
	}

	void CheckArrivalCustomerReferenceLength()
	{
		var messageError = Res.GetString("65DAADAA-D4AB-4DF5-9C80-C6646872013C",
			"Arrival Customer Reference cannot exceed 22 characters.");
		Parent.ClearRowNotificationsContaining(messageError);

		var yearLength = Parent.YearOption.ToString() switch
		{
			CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix2 or CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Suffix2 => 2,
			CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Prefix4 or CustomArrivalCustomerReferenceFormatYearOptionList.Codes.Suffix4 => 4,
			_ => 0
		};

		if (Parent.Prefix.Length + Parent.Suffix.Length + yearLength + Parent.SequenceNumberLength > 22)
		{
			Parent.AddRowError(messageError);
		}
	}
}
