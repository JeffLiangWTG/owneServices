using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;

namespace Enterprise.Customs.NL.Business.Declaration;

public class ImportJobDeclarationValidation : JobDeclarationValidation
{
	public ImportJobDeclarationValidation(JobDeclaration parent) : base(parent)
	{
	}

	protected override void CheckJE_OA_Representative()
	{
		base.CheckJE_OA_Representative();
		var intracomReceiverAddress = Parent.Representative;
		if (intracomReceiverAddress != null)
		{
			var intracomReceiverCountry = intracomReceiverAddress.Country;

			if (!intracomReceiverCountry.IsPartOfEuropeanUnion)
			{
				Parent.JE_OA_RepresentativeInfo.AddMessageError(Res.GetString("6539E3B1-1414-4BF5-BB64-4222BDE068BC", "The fiscal rep should be based in an EU country."));
			}

			if ((intracomReceiverAddress.Header?.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, ZString.Empty) ?? ZString.Empty) == ZString.Empty)
			{
				Parent.JE_OA_RepresentativeInfo.AddMessageError(Res.GetString("831CC6B8-53B6-4A30-BC77-EDA53587F60F", "EORI number from representative is required."));
			}
		}
	}

	protected override void CheckJE_TransportModeInland()
	{
		base.CheckJE_TransportModeInland();
		ListValidation.MessageErrorIfInvalidCode(Parent.JE_TransportModeInlandInfo);
	}

	protected override void CheckJE_ShipmentIncoTerm()
	{
		base.CheckJE_ShipmentIncoTerm();
		if (Parent.JE_ShipmentIncoTerm.IsEmpty && Parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x != null && x.JI_ValuationCode.Equals(ValuationMethodList.Codes._1)))
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_ShipmentIncoTermInfo);
		}
	}

	protected override void CheckJE_OH_ControllingAgent()
	{
		base.CheckJE_OH_ControllingAgent();
		if (Parent.JE_OH_ControllingAgent.IsEmpty && Parent.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(x => (x.CEI_SubStyle.Equals(DeclarationSubTypeList.Codes.V) || x.CEI_SubStyle.Equals(DeclarationSubTypeList.Codes.Z))))
		{
			Parent.JE_OH_ControllingAgentInfo.AddMessageError(Res.GetString("FE378796-B248-4B5F-BCBA-D0811CAA40F6", "Agent details required for this procedure type"));
		}
	}
}
