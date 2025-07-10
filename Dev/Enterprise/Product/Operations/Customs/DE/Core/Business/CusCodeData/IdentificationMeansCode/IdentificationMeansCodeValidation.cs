using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class IdentificationMeansCodeValidation : Customs.Business.CusCodeDataValidation
	{
		public IdentificationMeansCodeValidation(IdentificationMeansCode parent)
			: base(parent)
		{
		}

		protected new IdentificationMeansCode Parent => (IdentificationMeansCode)base.Parent;

		protected override void CheckCY_Code()
		{
			if (Parent.Parent is CusEntryInstruction entryInstruction && entryInstruction.EnabledOutwardProcessing)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo);
				if (entryInstruction.IdentificationMeanCodes.Cast<IdentificationMeansCode>().Any(x => x.CY_Code == Parent.CY_Code && x.PK != Parent.PK))
				{
					Parent.CY_CodeInfo.AddMessageError(Res.GetString("d53a3262-4691-448e-b397-89b754c3ecba", "Type must be unique."));
				}

				if (Parent.CY_Code == IdentificationMeansList.Codes.O)
				{
					if (entryInstruction.IdentificationMeanCodes.Count > 1)
					{
						Parent.CY_CodeInfo.AddMessageError(Res.GetString("689E071F-0A47-422D-A7A8-84201629FBCD", "Type 'O' allows only one record of Identification Means."));
					}
					if (entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.ZG_UsualReplacement))
					{
						Parent.CY_CodeInfo.AddMessageError(Res.GetString("7BB69C01-B6B0-47A0-BBDB-68E26EBECEEC", "Type 'O' requires the Usual Replacement Flag to be selected on all Invoice Lines linked to this Entry Instruction."));
					}
				}
			}
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			if (Parent.Parent is CusEntryInstruction entryInstruction
				&& entryInstruction.EnabledOutwardProcessing
				&& Parent.CY_Code == IdentificationMeansList.Codes.S
				&& Parent.CY_Data.IsEmpty)
			{
				Parent.CY_DataInfo.AddMessageError(Res.GetString("40cc8ce3-561b-498d-ab8d-a5c8cf7a1ac4", "Description is mandatory for this Type."));
			}
		}
	}
}
