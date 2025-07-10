using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class JobComInvoiceLineTaxValidation : EU.Business.Declaration.JobComInvoiceLineTaxValidation
	{
		public JobComInvoiceLineTaxValidation(JobComInvoiceLineTax parent) : base(parent)
		{
		}

		public new JobComInvoiceLineTax Parent => (JobComInvoiceLineTax)base.Parent;

		protected override void CheckJLT_Type()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JLT_TypeInfo);
			var invLine = Parent.InvoiceLine;
			if (invLine.Taxes.Cast<JobComInvoiceLineTax>().Any(x => x.PK != Parent.PK && x.JLT_Type == Parent.JLT_Type))
			{
				Parent.JLT_TypeInfo.AddMessageError(Res.GetString("42e66d86-7341-46b3-861c-d922168708ae", "A row with this Group already exists"));
			}
		}

		protected override void CheckJLT_MethodOfCalculation()
		{
			var info = Parent.JLT_MethodOfCalculationInfo;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(info);

			var invLine = Parent.InvoiceLine;
			var cpc = invLine.JI_Procedure.SubstringSafe(0, 4);
			var in0607AllowedGroups = SpecialCaseGroupList.IsInGroups1to12And20(Parent.Factory, Parent.JLT_Type); //Note 'Group' is JLT_Type

			var type = Parent.JLT_MethodOfCalculation;
			var typeIs6 = type == SpecialCaseTypeList.Codes._06;
			var typeIs7 = type == SpecialCaseTypeList.Codes._07;

			if (typeIs6 || typeIs7)
			{
				var prevProcedure = invLine.JI_Calc_PreviousProcedure;
				if (!in0607AllowedGroups
					|| (typeIs6 && !PermittedPreviousProcedures_06.Any(x => x == prevProcedure))
					|| (typeIs7 && !PermittedPreviousProcedures_07.Any(x => x == prevProcedure)))
				{
					info.AddMessageError(Res.GetString("5262d7f5-75e3-4c24-b894-03bca2b7a2fb", "This Previous Procedure and Group combination is not permitted for Type '{0}'.", type));
				}
			}
			else if ((cpc == UniversalReferenceConstants.CustomsProcedureCodes._4054 || cpc == UniversalReferenceConstants.CustomsProcedureCodes._4254) && in0607AllowedGroups)
			{
				info.AddMessageError(Res.GetString("6124bcb9-01c4-4ba2-acf6-e4a1b28aae8a", "Type '06' or '07' is required for this Previous Procedure and Group combination."));
			}
		}

		protected override void CheckJLT_MethodOfPayment()
		{
		}

		string[] PermittedPreviousProcedures_06 => permittedPreviousProcedures_06 ?? (permittedPreviousProcedures_06 = new string[] { "31", "51", "54" });
		string[] permittedPreviousProcedures_06;
		string[] PermittedPreviousProcedures_07 => permittedPreviousProcedures_07 ?? (permittedPreviousProcedures_07 = new string[] { "51", "54" });
		string[] permittedPreviousProcedures_07;

		protected override void CheckJLT_Rate()
		{
			if (RateMandatoryTypes.Any(x => x == Parent.JLT_MethodOfCalculation))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JLT_RateInfo);
			}
		}

		string[] RateMandatoryTypes => rateMandatoryTypes ?? (rateMandatoryTypes = new string[] { "01", "02", "03", "04", "05", "06", "07" });
		string[] rateMandatoryTypes;
	}
}
