using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportOfficeCodeValidation : CommonExportOfficeCodeValidation
	{
		public ExportOfficeCodeValidation(OfficeCode parent) : base(parent)
		{
		}

		protected override bool WasDataChanged(JobDeclaration declaration, ZString code)
		{
			bool result;
			if (code == EuOfficeCodesTypes.Codes.OfficeOfPresentation)
			{
				result = IsOfficeCodeChanged(declaration.OriginalPresentationOffice);
			}
			else if (code == EuOfficeCodesTypes.Codes.SupervisingOffice)
			{
				result = IsOfficeCodeChanged(declaration.OriginalSupervisingOffice);
			}
			else
			{
				result = base.WasDataChanged(declaration, code);
			}
			return result;
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (Parent.CY_Code.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.SupervisingOffice)
				&& Parent.CY_Data is ZString officeCode
				&& !officeCode.IsEmpty
				&& Parent.Parent is JobDeclaration declaration
				&& declaration.JE_CustomsOffice.EqualsIgnoringCase(officeCode))
			{
				Parent.CY_DataInfo.AddMessageError(Res.GetString("BF270503-0314-4158-8790-9FA62029ABF3", "Supervising Customs Office cannot be the same as Office of Export"));
			}
		}
	}
}
