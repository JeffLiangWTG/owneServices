using System.Globalization;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.DocBuilder.SectionEditing
{
	public class CustomizedDocumentElementsTemplateCreator
	{
		public CustomizedDocumentElementsTemplateCreator(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public StmTemplateBase Create()
		{
			return Create(Enterprise.Core.Constants.Languages.English);
		}

		public StmTemplateBase Create(ZString language)
		{
			var result = factory.New<StmTemplateBase>();

			result.SO_Name = GetName(language);
			result.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.GenericFreightJob);
			result.SO_Template = GetBaseDocBuilderTemplate();
			result.SO_ExcelTemplatePath = result.SO_Name + "." + Core.Constants.FileFormats.XLS;

			SetNameInsideTemplate(result);

			return result;
		}

		void SetNameInsideTemplate(StmTemplateBase template)
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);

				var workSheet = excelInterface.WorkSheets.Find(SectionRepository.DocumentWorkSheetName);

				for (var row = 0; row < workSheet.RowCount; row++)
				{
					var cell = workSheet.GetCell(row, 0);

					if (cell.ValueSourceText.StartsWith(Constants.ConfigAreaParameters.TemplateNameSignature, System.StringComparison.InvariantCultureIgnoreCase))
					{
						var templateNameSignature = Constants.ConfigAreaParameters.TemplateNameSignature.ToLowerInvariant();
						templateNameSignature = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(templateNameSignature);

						cell.ValueSourceText = string.Format("{0}{1}", templateNameSignature, template.SO_Name);
						break;
					}
				}

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}
		}

		ZString GetName(ZString language)
		{
			var result = SectionRepositoryTemplateNames.User;

			if (!language.IsEmpty && !language.Equals(Enterprise.Core.Constants.Languages.English))
			{
				result = SectionRepositoryTemplateNames.GetLanguageSpecificTemplateName(SectionRepositoryTemplateNames.User, language);
			}

			return result;
		}

		internal static ZBlob GetBaseDocBuilderTemplate()
		{
			var assembly = typeof(StmTemplateBase).Assembly;
			var resourceName = "Enterprise.DocumentEngine.DocBuilder.SectionEditing.BaseDocBuilderTemplate.xls";
			using (var stream = assembly.GetManifestResourceStream(resourceName))
			{
				return stream.CopyToByteArray();
			}
		}
	}
}
