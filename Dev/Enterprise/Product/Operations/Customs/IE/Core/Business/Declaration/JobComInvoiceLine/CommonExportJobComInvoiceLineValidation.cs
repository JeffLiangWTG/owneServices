using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public abstract class CommonExportJobComInvoiceLineValidation : JobComInvoiceLineValidation
	{
		protected CommonExportJobComInvoiceLineValidation(JobComInvoiceLine parent) : base(parent)
		{
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			var parent = Parent;
			if (parent.Declaration is JobDeclaration declaration)
			{
				var maxLength = declaration.IsTransitionPeriodAES30 ? JobComInvoiceLine.JI_DescriptionMaxLength_TransitionPeriodAES30 : JobComInvoiceLine.JI_DescriptionMaxLength_AISUCC5;
				if (parent.JI_Description.Length > maxLength)
				{
					parent.JI_DescriptionInfo.AddWarning(Res.GetString("B50306A3-B78A-4FC7-A293-8B7EAD38EC63", "Goods description can have up to {0} alpha numeric characters.", maxLength));
				}
			}
		}

		protected override void CheckJI_CountryOfOriginMandatoryValidation()
		{
			var additionalProcedurePrefix = Parent.JI_Calc_Concession.ToUpperInvariant().Left(1);
			if (additionalProcedurePrefix == AdditionalProcedurePrefixE)
			{
				var info = Parent.JI_CountryOfOriginInfo;
				MandatoryValidation.MessageErrorIfNotEntered(info, Res.GetString("91D324AD-66B0-4CF5-B437-03D9A8BF5D70", "{0} Origin is mandatory when Additional Procedure begins with 'E'", info.HumanReadableName));
			}
		}
		const string AdditionalProcedurePrefixE = "E";

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();

			if (!Parent.JI_Procedure.IsEmpty
				&& Parent.EntryInstruction is CusEntryInstruction instruction
				&& instruction.CEI_Style == ExportDeclarationTypeList.Codes.B3
				&& Parent.CusProcedure is RefCusProcedure cusProcedure
				&& !cusProcedure.IsIntoWarehouse())
			{
				Parent.JI_ProcedureInfo.AddMessageError(Res.GetString("B333F09C-CCAA-49C7-A462-BF302C3B2947", "Procedure must be into warehouse when Instruction Declaration Type is {0}.", ExportDeclarationTypeList.Codes.B3));
			}
		}

		protected override void CheckJI_CustomsSecondUnitQty()
		{
			base.CheckJI_CustomsSecondUnitQty();
			if (Parent.UniversalTariff is ITariff tariff && !tariff.UQ2.IsEmpty && Parent.JI_CustomsSecondUnitQty != tariff.UQ2)
			{
				var info = Parent.JI_CustomsSecondUnitQtyInfo;
				info.AddMessageError(Res.GetString("C23AF349-AF0A-494F-8BB5-40E79C1F5942", "{0} for this {1} should be {2}", info.HumanReadableName, Parent.JI_TariffInfo.HumanReadableName, tariff.UQ2));
			}
		}
		protected override ZString TariffNoAdditionalCodesMessage => Res.GetString("F76C50FF-E471-4C65-A501-D3153659D356", "TARIC Additional codes exist for this tariff.");
	}
}
