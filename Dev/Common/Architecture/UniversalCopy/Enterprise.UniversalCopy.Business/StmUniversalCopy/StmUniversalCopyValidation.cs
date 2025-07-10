//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmUniversalCopyValidation
//
//    This class should be used for overriding validation in AutoStmUniversalCopyValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.UniversalCopy.Business
{
	public class StmUniversalCopyValidation : AutoStmUniversalCopyValidation
	{
		public StmUniversalCopyValidation(AutoStmUniversalCopy parent) : base(parent)
		{
		}

		protected new StmUniversalCopy Parent
		{
			get { return (StmUniversalCopy)base.Parent; }
		}

		protected override void CheckSUC_CopyObjectTableCode()
		{
			base.CheckSUC_CopyObjectTableCode();
			var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(Parent.SUC_CopyObjectTableCode, false);
			if (type == null)
			{
				Parent.SUC_CopyObjectTableCodeInfo.AddError(Res.GetString("854d2828-b623-4714-bc24-ae72edfa2237", "Scheduled Universal Copy is not currently supported on the selected entity type."));
			}
		}

		protected override void CheckSUC_S9_CopyTemplate()
		{
			base.CheckSUC_S9_CopyTemplate();

			if (Parent.SUC_S9_CopyTemplate.IsValid)
			{
				var template = Parent.CopyTemplate;
				if (template != null)
				{
					if (!template.S9_IsPublished)
					{
						Parent.SUC_S9_CopyTemplateInfo.AddError(Res.GetString("77664a65-e9bb-4fa0-8d64-4b670c99464d", "Selected Copy Template is not published and will not be available to other users."));
					}
					else if (!template.S9_GC.IsEmpty && Parent.ScheduleTask != null)
					{
						if (Parent.ScheduleTask.Branch != null && Parent.ScheduleTask.Branch.GB_GC != template.S9_GC)
						{
							Parent.SUC_S9_CopyTemplateInfo.AddError(Res.GetString("2c932564-3b4e-4fa5-8aef-116efaff0b0a", "Selected Copy Template is not published for all Companies and is not available under selected Branch."));
						}
						else if (Parent.ScheduleTask.Branch == null)
						{
							Parent.SUC_S9_CopyTemplateInfo.AddWarning(Res.GetString("6bf5f5c2-62e6-4409-8bde-5d88d396db17", "Selected Copy Template is not published for all Companies and may be not available when this Schedule Task is running."));
						}

						if (template.S9_GC != GlbCompany.CurrentCompany.PK)
						{
							Parent.SUC_S9_CopyTemplateInfo.AddWarning(Res.GetString("e0223e77-b2ad-48a4-a591-859574187d51", "Selected Copy Template is not published for all Companies and is not available under current Company."));
						}
					}

					CheckCopyTemplateProducesValidCopy();
				}
			}
		}

		void CheckCopyTemplateProducesValidCopy()
		{
			if (Parent != null && Parent.CopyObject != null && Parent.Template != null && Parent.Template.CopyTemplateTree != null)
			{
				if (!Parent.CopyObject.IsInDatabase)
				{
					Parent.SUC_S9_CopyTemplateInfo.AddWarning(Res.GetString("c8aac82b-44f9-4ff5-9a5d-90bddd093fd0", "Cannot test selected Copy Template with source object, because source object is not stored in database."));
					return;
				}

				var copyResult = Parent.RunCopyAndGetCopyObjectInNewFactory();
				var copy = copyResult.Copy;
				if (copy != null)
				{
					copy.RunPreSaveValidation();
					if (copy.HasErrors)
					{
						Parent.SUC_S9_CopyTemplateInfo.AddError(GetErrorMesssage(string.Join("\r\n", copy.GetErrors().Select(n => n.Message).Distinct())));
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(copyResult.Error))
					{
						Parent.SUC_S9_CopyTemplateInfo.AddError(GetErrorMesssage(copyResult.Error));
					}
				}
			}

			string GetErrorMesssage(string error) => Res.GetString("fb582dcd-2922-450d-925e-e35127fd405e", "Selected Copy Template produces invalid or incomplete copy record with following validation errors:") + "\r\n" + error;
		}
	}
}
