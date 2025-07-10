using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public abstract class CommonExportCusEntryInstructionValidation : CusEntryInstructionValidation
	{
		protected CommonExportCusEntryInstructionValidation(CusEntryInstruction parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();

			base.ValidateAll();
			CheckEntryInstructionShouldHaveCentralisedClearanceAuthorizations();
		}

		protected override void CheckCEI_SubStyle()
		{
			base.CheckCEI_SubStyle();

			var targetInfo = Parent.CEI_SubStyleInfo;

			if (Parent.IsSimplifiedDeclarationAuthorizationRequired && Parent.CusAuthorizationUsages.All(p => p.AGC_Code != CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration))
			{
				targetInfo.AddMessageError(Res.GetString("A70EDDC1-5942-4361-912E-B665E9C7E043", "Please enter at least a Authorization of code 'SDE'"));
			}
		}

		protected override void CheckCEI_OA_Warehouse2()
		{
			base.CheckCEI_OA_Warehouse2();

			if (Parent.CEI_OA_Warehouse2.IsEmpty && Parent.CEI_Style == ExportDeclarationTypeList.Codes.B3)
			{
				Parent.CEI_OA_Warehouse2Info.AddMessageError(Res.GetString("D712EC27-1A1E-434B-BC79-7D3ACAF49DBC", "Warehouse To is mandatory when Declaration Type is {0}.", ExportDeclarationTypeList.Codes.B3));
			}
		}

		void CheckEntryInstructionShouldHaveCentralisedClearanceAuthorizations()
		{
			if (Parent.JobDeclaration is JobDeclaration declaration)
			{
				var office = declaration.CustomsOffices.GetPresentationOffice()?.CY_Data ?? ZString.Empty;
				if (!office.IsEmpty && !Parent.IsCentralisedClearance)
				{
					Parent.AddRowMessageError(Res.GetString("53DC2C77-4EA8-4AB1-87FE-56FAF6899C31", "Centralized clearance is indicated when Presentation Office Code is entered. In this case an appropriate authorization for centralized clearance should be entered (CCL)."));
				}
			}
		}
	}
}
