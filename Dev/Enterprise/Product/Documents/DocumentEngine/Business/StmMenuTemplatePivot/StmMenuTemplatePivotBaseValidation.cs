using System;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuTemplatePivotBaseValidation : StmMenuTemplatePivotValidation
	{
		public StmMenuTemplatePivotBaseValidation(StmMenuTemplatePivotBase validatee)
			: base(validatee)
		{
			Validatee = validatee;
		}
		readonly StmMenuTemplatePivotBase Validatee;

		protected override void CheckSI_RT_DocType()
		{
			base.CheckSI_RT_DocType();
			if (Validatee.SI_RT_DocType.IsEmpty)
			{
				if (Validatee.Menu != null && Validatee.Menu.SU_IncludeDocInArchive.Equals("YES"))
				{
					Validatee.SI_RT_DocTypeInfo.AddWarning(ErrorMustHaveArchivableDocumentType);
				}

				if (Validatee.SI_IsSystemDefined && Validatee.Menu != null && Validatee.Menu.IsADocumentMenuItem)
				{
					Validatee.SI_RT_DocTypeInfo.AddError(ErrorMustHaveDocumentType);
				}
				else
				{
					Validatee.SI_RT_DocTypeInfo.AddWarning(ErrorMustHaveDocumentType);
				}
			}
		}

		public static string ErrorMustHaveDocumentType => Res.GetString("f56d1c2c-42f0-4620-846b-ee8360383b3a", "When a Doc Type is not specified when delivering or generating the document, it will be allocated to eDocs as an MSC Doc Type.");
		public static string ErrorMustHaveArchivableDocumentType => Res.GetString("E44C182B-9793-434E-8A25-ABFD54457AB0", "When a Doc Type is not specified, the document will not be archived.");

		protected override void CheckSI_IsSystemDefined()
		{
			base.CheckSI_IsSystemDefined();

			if (Validatee.Menu != null && Validatee.Template != null && !IsSystemBillOfLadingMenuItem)
			{
				var errorText = Business.Validation.SystemClientPivotValidator.GetErrorText(Validatee.Menu.SU_IsSystemDefined, Validatee.Menu.SU_IsClientSpecific, Validatee.Template.SO_IsSystemDefined, Validatee.Template.SO_IsClientSpecific, Validatee.SI_IsSystemDefined, Validatee.SI_IsClientSpecific);

				if (!errorText.IsEmpty)
				{
					Validatee.SI_IsSystemDefinedInfo.AddError(errorText);
				}
			}

			ValidateSI_RT_DocType();
		}

		bool IsSystemBillOfLadingMenuItem => Validatee.SI_SU == ForwardingConstants.SystemFormMenuItems.BillOfLadingPK;

		protected override void CheckSI_PrintCopyType()
		{
			base.CheckSI_PrintCopyType();

			if (!Validatee.SI_PrintCopyType.IsEmpty)
			{
				try
				{
					var testType = (PrintCopyType)Enum.Parse(typeof(PrintCopyType), Validatee.SI_PrintCopyType);
				}
				catch (ArgumentException)
				{
					Validatee.SI_PrintCopyTypeInfo.AddError(Res.GetString("ef2f46ec-1302-4c51-a3d3-b25760dbc94a", "Invalid type entered. Please select one from the list."));
				}
			}
		}

		protected override void CheckSI_MenuTemplateFilter()
		{
			base.CheckSI_MenuTemplateFilter();

			if (!Validatee.SI_MenuTemplateFilter.IsEmpty)
			{
				if (!ZExpressionEvaluator.IsValidTemplateFilterName(Validatee.SI_MenuTemplateFilter, false, out var message))
				{
					Validatee.SI_MenuTemplateFilterInfo.AddError(message);
				}
			}
		}

		protected void CheckSO_Name()
		{
			if (Validatee.SO_Name.EqualsIgnoringCase(SectionRepositoryTemplateNames.User) || SectionRepositoryTemplateNames.UserNameChecker.IsMatch(Validatee.SO_Name))
			{
				Validatee.SO_NameInfo.AddError(Res.GetString("4ff54de8-6a2a-405c-96a5-7601fbf9be81", "The '{0}' template is only meant for adding new template sections. Please link this document to the '{1}' template instead.",
					Validatee.SO_Name, SectionRepositoryTemplateNames.System));
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateSO_Name();
		}

		public void ValidateSO_Name()
		{
			ValidateCalculatedProperty(Validatee.SO_NameInfo);
		}
	}
}
