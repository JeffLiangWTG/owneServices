using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.Business
{
	public class StmTemplateBaseValidation : StmTemplateValidation
	{
		public StmTemplateBaseValidation(StmTemplateBase validatee)
			: base(validatee)
		{
		}

		protected override void CheckSO_Name()
		{
			base.CheckSO_Name();

			MandatoryValidation.CheckEntered(Parent.SO_NameInfo, Res.GetString("8c4f5979-c171-4622-bcd0-b5db96ba527d", "Template Name"));

			if (!Parent.SO_IsSystemDefined && Parent.SO_Name.EqualsIgnoringCase(SectionRepositoryTemplateNames.System))
			{
				Parent.SO_NameInfo.AddError(Res.GetString("7debd1b5-ecd5-4460-b445-812c10e611fa", "'{0}' is a reserved name for section repository templates. Please enter a different name.", Parent.SO_Name));
			}

			if (Parent.SO_IsUserConfigurable && !SectionRepositoryTemplateNames.NameChecker.IsMatch(Parent.SO_Name))
			{
				Parent.SO_NameInfo.AddError(Res.GetString("994e397a-84d5-4fa3-b251-7e05dc10b0d2", "Cannot use 'Configurable Sections' templates unless they are named as '{0}' or '{1}' with or without a language code suffix.", SectionRepositoryTemplateNames.System, SectionRepositoryTemplateNames.User));
			}

			if (SectionRepositoryTemplateNames.SystemNameChecker.IsMatch(Parent.SO_Name) && !Parent.SO_IsSystemDefined)
			{
				Parent.SO_NameInfo.AddError(Res.GetString("3e24a3da-0dee-4c66-a346-a8305127ea40", "'{0}' can only be saved as System.", Parent.SO_Name));
			}

			if (SectionRepositoryTemplateNames.UserNameChecker.IsMatch(Parent.SO_Name) && Parent.SO_IsSystemDefined)
			{
				Parent.SO_NameInfo.AddError(Res.GetString("4d9b0e17-4208-4df3-a698-139ab6220714", "'{0}' can only be saved as Non-System.", Parent.SO_Name));
			}
		}

		protected override void CheckSO_DataContext()
		{
			base.CheckSO_DataContext();
			var dataContextName = nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob);
			if (SectionRepositoryTemplateNames.UserNameChecker.IsMatch(Parent.SO_Name) && !Parent.SO_DataContext.Equals(dataContextName))
			{
				Parent.SO_DataContextInfo.AddError(Res.GetString("04F2A244-5CB2-4DE0-9F1B-8D4DB113E97E", "The DocBuilder style template's data context must be {0}", dataContextName));
			}
		}

		protected override void CheckSO_TemplateIsValidZBlobSize()
		{
		}

		protected override void CheckSO_Template()
		{
			base.CheckSO_Template();
			if (Parent.SO_TemplateInfo.HasChanges && !Parent.SO_Template.IsEmpty)
			{
				using (var excelInterface = ExcelInterface.GetLoadedExcelInterface(Parent.SO_Template))
				{
					if (Parent.SO_DataContext == nameof(Core.Constants.DataContext.None))
					{
						CheckTemplateFilter(excelInterface);
					}

					if (Parent.SO_IsUserConfigurable && SectionRepositoryTemplateNames.NameChecker.IsMatch(Parent.SO_Name))
					{
						var invalidSectionNames = new List<string>();
						var sourceSections = new TemplateSectionCollection(((StmTemplateBase)Parent).GetExcelTemplate());

						foreach (TemplateSection section in sourceSections)
						{
							if (section.SectionName.Length > AutoStmMenuDocumentConfigItem.Schema.S4_SectionItemNameMaxLength)
							{
								invalidSectionNames.Add(section.SectionName);
							}
						}
						if (invalidSectionNames.Count > 0)
						{
							var message = new StringBuilder();
							message.Append(Res.GetString("BD53F285-E290-4F71-AF1A-5724919F9AC9", "The following section names are too long. They must not exceed {0} characters:",
									AutoStmMenuDocumentConfigItem.Schema.S4_SectionItemNameMaxLength));
							foreach (var invalidSeciontName in invalidSectionNames)
							{
								message.Append("\r\n\r\n" + invalidSeciontName);
							}
							Parent.SO_TemplateInfo.AddError(message.ToString());
						}
					}

					if (!IsWholeTemplateContentInsidePrintArea(excelInterface))
					{
						Parent.SO_TemplateInfo.AddWarning(Res.GetString("41BAA4A7-7088-4A5B-9214-9120C1504FA3", "Some content of this template is out of the print area and will not be printed, you may need to check your modification again."));
					}
				}
			}
		}

		void CheckTemplateFilter(ExcelInterface excelInterface)
		{
			try
			{
				var filterWorkSheet = (ExcelWorkSheet)excelInterface.WorkSheets[Report.FilterSheetNameRegEx].FirstOrDefault();
				var usedFilterNames = new HashSet<string>();
				var root = new StringTreeBuilder(filterWorkSheet).GetTree();

				foreach (var filterNode in root.Children)
				{
					if (usedFilterNames.Contains(filterNode.Value))
					{
						var errorMessage = Res.GetString("42AC8126-88B9-4C9E-8C65-F872EE1E6B6B", "Filter Name Conflict: There are more than one filter using the name [{0}].", filterNode.Value);
						Parent.SO_TemplateInfo.AddError(errorMessage);
					}
					else
					{
						usedFilterNames.Add(filterNode.Value);
					}
				}
			}
			catch (TemplateDefinitionException ex)
			{
				Parent.SO_TemplateInfo.AddError(ex.Message);
			}
		}

		protected override void CheckSO_IsUserConfigurable()
		{
			base.CheckSO_IsUserConfigurable();

			this.ValidateSO_Name();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "System.String.ToUpper")]
		public static bool IsWholeTemplateContentInsidePrintArea(ExcelInterface excelInterface)
		{
			for (int i = 0; i < excelInterface.WorkSheets.Count; i++)
			{
				TXlsNamedRange printArea = excelInterface.Xls.GetNamedRange(((char)InternalNameRange.Print_Area).ToString(), i + 1);
				if (printArea != null)
				{
					var workSheet = excelInterface.WorkSheets[i];
					for (int row = 0; row < workSheet.RowCount; row++)
					{
						var initialCellContent = workSheet[row, 0].ToString().Trim();
						if (initialCellContent.ToUpper() == Constants.AreaIdentifierTags.EndOfReport)
						{
							if (row > printArea.Bottom)
							{
								return false;
							}
						}
					}
				}
			}
			return true;
		}
	}
}
