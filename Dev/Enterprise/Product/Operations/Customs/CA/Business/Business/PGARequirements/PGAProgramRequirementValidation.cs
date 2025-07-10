using CargoWise.Common;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class PGAProgramRequirementValidation
	{
		public PGAProgramRequirementValidation(PGAProgramRequirement parent)
		{
			Argument.NotNull(parent, nameof(parent));
			this.parent = parent;
		}

		#region Implementation

		readonly PGAProgramRequirement parent;

		#endregion

		#region DeclareYes

		public void CheckDeclareYes(ZPropertyInfo declarationYesInfo)
		{
			declarationYesInfo.ClearAllNotifications();
			ValidateDeclareYes(declarationYesInfo);
		}

		protected void ValidateDeclareYes(ZPropertyInfo propertyInfo)
		{
			if (parent.DeclareYes && parent.ParentRequirement.IsEffective)
			{
				CheckRequiredLPCOs();
				if (!parent.IsProgramRequired)
				{
					var warning = Res.GetString("009b9aff-497b-4a6b-8a3d-9cc8d8996214", "The Tariff does not indicate that this program reporting is required.");
					propertyInfo.AddWarning(warning);
				}

				if (parent.PGAType == PGACodes.Codes.CFIA)
				{
					var header = parent.ProgramRequirementProvider as CFIAPGAHeader;
					if (header.IsBlank)
					{
						propertyInfo.AddMessageError(Res.GetString("a846c2a6-2a8c-4a8d-82c9-d5c73c8a0842", "No CFIA data has been entered. If CFIA does not apply to this invoice line, please tick 'No' for all CFIA programs."));
					}
					else
					{
						header.AddInfoValidation.CheckCFIAAccountNumber(propertyInfo);
					}
				}
			}
		}

		void CheckRequiredLPCOs()
		{
			var messageError = PGAHeaderValidationHelper.ValidateRequiredLPCOs(parent.ProgramRequirementProvider, parent.ProgramCode);
			if (!messageError.IsEmpty)
			{
				parent.DeclareYesInfo.AddMessageError(messageError);
			}
		}

		#endregion

		#region DeclareNo

		public void CheckDeclareNo(ZPropertyInfo declarationNoInfo)
		{
			declarationNoInfo.ClearAllNotifications();
			ValidateDeclareNo(declarationNoInfo);
		}

		protected void ValidateDeclareNo(ZPropertyInfo propertyInfo)
		{
			if (parent.DeclareNo && parent.ParentRequirement.IsEffective && parent.IsProgramRequired)
			{
				var warning = Res.GetString("84644fc1-2efd-42fd-b49d-edcda26407b0", "The Classification number may require {0}: {1}. Tick Yes if {0}: {1} applies, otherwise leave as No.", parent.ParentRequirement.AgencyCode, parent.ProgramCodeDescription);
				propertyInfo.AddWarning(warning);
			}
		}

		#endregion

		#region DeclareNotApplicable

		public void CheckDeclareNotApplicable(ZPropertyInfo declareNotApplicableInfo)
		{
			declareNotApplicableInfo.ClearAllNotifications();
			ValidateDeclareNotApplicable(declareNotApplicableInfo);
		}

		protected void ValidateDeclareNotApplicable(ZPropertyInfo propertyInfo)
		{
			if (parent.DeclareNotApplicable && parent.ParentRequirement.IsEffective && parent.IsProgramRequired)
			{
				var message = Res.GetString("5A706E84-7B71-454C-A6CA-3F0586D51271", "The classification is flagged for this PGA and Program. Tick Yes to declare the PGA data or tick No to indicate that the PGA and Program do not apply in this case.");
				propertyInfo.AddMessageError(message);
			}
		}

		#endregion
	}
}
