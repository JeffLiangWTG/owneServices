using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class TemplateSectionValidation : ZValidation
	{
		public TemplateSectionValidation(TemplateSection templateSection)
			: base(templateSection)
		{
			TemplateSection = templateSection;
		}
		readonly TemplateSection TemplateSection;

		public override Type AutoValidationType
		{
			get { return typeof(TemplateSection); }
		}

		public override void ValidateAll()
		{
			ValidateTypeCode();
			ValidateSectionName();
			ValidateStartingRow();
			ValidateRowCount();
		}

		public void ValidateTypeCode()
		{
			ValidateCalculatedProperty(TemplateSection.TypeCodeInfo);
		}

		protected virtual void CheckTypeCode()
		{
			if (!new ConfigurableSectionTypeList().ContainsCode(TemplateSection.TypeCode))
			{
				TemplateSection.TypeCodeInfo.AddError(ErrorTypeCodeMustBeValid);
			}
		}

		public static string ErrorTypeCodeMustBeValid
		{
			get { return Res.GetString("763e56f0-0e21-46fb-a160-d7ebe498d492", "Type Code must be a valid code from the Configurable Section Type List."); }
		}

		public void ValidateSectionName()
		{
			ValidateCalculatedProperty(TemplateSection.SectionNameInfo);
		}

		protected virtual void CheckSectionName()
		{
			if (TemplateSection.SectionName.IsEmpty)
			{
				TemplateSection.SectionNameInfo.AddError(ErrorMustSpecifySectionName);
			}
		}

		public static string ErrorMustSpecifySectionName
		{
			get { return Res.GetString("6e6a60e7-0977-4912-b79b-66899fa03099", "You must specify a Section Name for each Template Section."); }
		}

		public void ValidateStartingRow()
		{
			ValidateCalculatedProperty(TemplateSection.StartingRowNumberInfo);
		}

		protected virtual void CheckStartingRow()
		{
		}

		public void ValidateRowCount()
		{
			ValidateCalculatedProperty(TemplateSection.RowCountInfo);
		}

		protected virtual void CheckRowCount()
		{
			if (TemplateSection.RowCount.IsEmpty)
			{
				TemplateSection.RowCountInfo.AddError(ErrorMustHaveRows);
			}
		}
		public static string ErrorMustHaveRows
		{
			get { return Res.GetString("cc3c59a8-97b9-4570-b848-7b03d30272f9", "You cannot have a section without any rows."); }
		}
	}
}
