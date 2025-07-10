using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DataProviders;

namespace Enterprise.DocumentEngine
{
	/// <summary>
	/// Summary description for ReportCommandValidation.
	/// </summary>
	public class ReportCommandValidation : StmMenuItemBaseValidation
	{
		public ReportCommandValidation(ReportCommand parent) : base(parent)
		{
		}

		new ReportCommand Parent
		{
			get { return (ReportCommand)base.Parent; }
		}

		protected override void CheckSU_DocumentDirection()
		{
		}

		protected override void CheckSU_ContactType()
		{
		}

		protected override void CheckSU_Calc_IsWebSupportable()
		{
			base.ValidateSU_Calc_IsWebSupportable();

			using (Report report = Parent.GetReport())
			{
				if (Parent.SU_Calc_IsWebSupportable && (report == null || report.LinkedLookupField == null))
				{
					if (report != null && report.ForceWebPublish)
					{
						Parent.SU_Calc_IsWebSupportableInfo.AddWarning(Res.GetString("121e7718-0367-4515-aee4-7a3eccca7d78", "Report is published on Web, but it is not linked to a client organization."));
					}
					else
					{
						Parent.SU_Calc_IsWebSupportableInfo.AddError(Res.GetString("b81b4687-53d4-4849-9420-9e6d82f4ec97", "Report cannot be published on Web because it cannot be linked to a client organization."));
					}
				}
			}
		}

		#region CheckSU_FilterList

		protected override void CheckSU_FilterList()
		{
			base.CheckSU_FilterList();

			if (!Parent.SU_FilterList.IsEmpty && (!Parent.SU_IsSystemDefined || !Parent.IsInDatabase))
			{
				if (!ZExpressionEvaluator.IsValidFilter(Parent.SU_FilterList, true, out string message))
				{
					Parent.SU_FilterListInfo.AddError(message);
				}
			}
		}

		#endregion

		#region CheckSU_DefaultAttachmentType

		protected override void CheckSU_DefaultAttachmentType()
		{
			base.CheckSU_DefaultAttachmentType();

			ListValidation.ErrorIfInvalidCode(Parent.SU_DefaultAttachmentTypeInfo, Parent.AttachmentTypes, ResString.GetMultilingualString("2DDCE67E-A88F-4CDA-ADD8-9E4FA3D77249", "Default Attachment Type"));
		}

		#endregion
	}
}
