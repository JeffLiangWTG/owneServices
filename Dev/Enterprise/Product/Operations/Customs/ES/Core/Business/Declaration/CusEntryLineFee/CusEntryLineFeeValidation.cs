using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Declaration;

public class CusEntryLineFeeValidation : EUUniversalCusEntryLineFeeValidation
{
	public CusEntryLineFeeValidation(AutoCusEntryLineFee parent) : base(parent)
	{
	}
	protected new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

	const int DecimalPlacesAllowedForBaseValuePrecisionNotImportOrPercentage = 2;
	const int DecimalPlacesAllowedForBaseValuePrecisionImportUCC6 = 6;
	const int DecimalPlacesAllowedForBaseValuePrecisionImportNoUCC6 = 3;

	protected override ZInt DecimalPlacesAllowedForBaseValuePrecision
	{
		get
		{
			var entryHeader = (CusEntryHeader)Parent.EntryLine.Header;
			return !entryHeader.IsImport || Parent.CF_MethodOfCalculation == Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage
							? DecimalPlacesAllowedForBaseValuePrecisionNotImportOrPercentage
							: entryHeader.IsUCC6
									? DecimalPlacesAllowedForBaseValuePrecisionImportUCC6
									: DecimalPlacesAllowedForBaseValuePrecisionImportNoUCC6;
		}
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateMaxMin();
	}

	#region MaxMin

	public void ValidateMaxMin()
	{
		ValidateCalculatedProperty(Parent.MaxMinInfo);
	}

	#endregion
}
