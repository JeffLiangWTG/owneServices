using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business.Declaration;

public partial class JobDeclarationValidation
{
	protected override void CheckJE_SpecificCircumstanceIndicator()
	{
		base.CheckJE_SpecificCircumstanceIndicator();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_SpecificCircumstanceIndicatorInfo);
	}

	protected override void CheckJE_BorderTransportMeans()
	{
		base.CheckJE_BorderTransportMeans();
		var declaration = Parent;
		var transportModes = new IZType[] { (ZString)ModeOfTransportCodeList.Codes._AIR, (ZString)ModeOfTransportCodeList.Codes._SEA, (ZString)ModeOfTransportCodeList.Codes._ROA, (ZString)ModeOfTransportCodeList.Codes._IWT, (ZString)ModeOfTransportCodeList.Codes._OWN };

		if (declaration.IsExport && declaration.HasInvoiceLineWithC9008Procedure)
		{
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(declaration.JE_BorderTransportMeansInfo, Parent.JE_TransportModeInfo, transportModes, Res.GetString("536A84CB-C0C2-4A81-925C-26D763C821FF", "[C9008] {0} is required.", declaration.JE_BorderTransportMeansInfo.HumanReadableName));
		}
	}
}
