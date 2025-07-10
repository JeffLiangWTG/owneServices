using System.Collections.Generic;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class OptionalTemplateSheetCollectionBuilder
	{
		public OptionalTemplateSheetCollectionBuilder(ExcelWorkSheet optionalTemplateSheet, StringCollection templateSheets, ValidatorPack validators, bool isInRuntime)
		{
			Sheet = optionalTemplateSheet;
			this.TemplateSheets = templateSheets;
			this.Validators = validators;
			IsInRuntime = isInRuntime;
		}
		protected ExcelWorkSheet Sheet;
		readonly StringCollection TemplateSheets;
		readonly ValidatorPack Validators;
		readonly bool IsInRuntime;

		public List<IReportProcessingError> Errors
		{
			get { return errors ?? (errors = new List<IReportProcessingError>()); }
		}
		List<IReportProcessingError> errors;

		public void Build()
		{
			Errors.Clear();
			OptionalTemplateSheets.Clear();

			try
			{
				Root = new StringTreeBuilder(Sheet).GetTree();
				BuildOptionalTemplatesFromTree();
			}
			catch (TemplateDefinitionException ex)
			{
				Errors.Add(new ReportProcessingError(Res.GetString("4535f82e-fd61-4433-bdfe-a6b5e671f7c5", "Error Building Optional Template Sheets: {0}", ex.Message),
									ex.CellReference, ReportProcessingErrorSeverity.Error));
			}
		}

		public OptionalTemplateSheetCollection OptionalTemplateSheets
		{
			get
			{
				if (fOptionalTemplateSheets == null)
				{
					fOptionalTemplateSheets = GetNewOptionalTemplateSheetCollection();
				}
				OptionalTemplateSheetCollection result = fOptionalTemplateSheets;
				if (HasErrors)
				{
					result = GetNewOptionalTemplateSheetCollection();
				}
				return result;
			}
		}
		OptionalTemplateSheetCollection fOptionalTemplateSheets;

		OptionalTemplateSheetCollection GetNewOptionalTemplateSheetCollection()
		{
			return new OptionalTemplateSheetCollection(IsInRuntime ? new BusinessObjectFactory() { NameForDebugging = "Optional Template Sheet" } : null, Validators);
		}

		protected StringTreeNode Root;

		void BuildOptionalTemplatesFromTree()
		{
			if (Sheet != null)
			{
				if (TemplateSheets.Count == 1)
				{
					throw new TemplateDefinitionException("This report only contains 1 template sheet. Can't specify optional templates if there's only 1 template sheet.", new CellReference(Sheet.SheetName, "A1"));
				}
				foreach (StringTreeNode optionalTemplate in Root.Children)
				{
					try
					{
						if (!TemplateSheets.Contains(optionalTemplate.Value))
						{
							throw new TemplateDefinitionException(optionalTemplate.Value + " is not a renderable template sheet.", optionalTemplate.CellReference);
						}
						else
						{
							if (OptionalTemplateSheets.Contains(optionalTemplate.Value))
							{
								throw new TemplateDefinitionException(optionalTemplate.Value + " is specified on the optional templates sheet more than once.", optionalTemplate.CellReference);
							}
							else
							{
								OptionalTemplateSheets.Add(optionalTemplate.Value);
							}
						}
					}
					catch (TemplateDefinitionException ex)
					{
						Errors.Add(new ReportProcessingError(Res.GetString("8749a0c7-9548-4bd9-9e4f-01a3a39e215e", "Error Building Optional Template Sheets from Tree: {0}", ex.Message)
							, ex.CellReference
							, ReportProcessingErrorSeverity.Error));
					}
				}
			}
		}

		bool HasErrors
		{
			get { return Errors.Count > 0; }
		}
	}
}
