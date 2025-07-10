using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.BuildTools;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocumentTemplatesTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTemplatesExist()
		{
			ZQuery query = new ZQuery(StmTemplateSchema.SO_DataContext, SQLComparisonOperator.NotEqual, "None");

			StmTemplate[] templates = Factory.Load<StmTemplate>(query);

			StringBuilder incorrectTemplates = new StringBuilder();
			foreach (StmTemplate template in templates)
			{
				if (template.SO_ExcelTemplatePath == ZString.Empty)
				{
					incorrectTemplates.AppendLine(String.Format("\"{0}\" has an empty SO_ExcelTemplatePath.", template.SO_Name));
				}
				else if (!File.Exists(BuildConstants.GetLocalPath(template.SO_ExcelTemplatePath)))
				{
					incorrectTemplates.AppendLine(String.Format("\"{0}\" doesn't have an xls file at {1}.", template.SO_Name, BuildConstants.GetLocalPath(template.SO_ExcelTemplatePath)));
				}
			}
			if (string.IsNullOrEmpty(incorrectTemplates.ToString()))
			{
				Assert(true);
			}
			else
			{
				incorrectTemplates.Insert(0, "The following StmTemplate(s) have error(s):" + System.Environment.NewLine);
				Fail(incorrectTemplates.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLatestTemplatesAreLoaded()
		{
			ZQuery query = new ZQuery(StmTemplateSchema.SO_DataContext, SQLComparisonOperator.NotEqual, "None");
			StmTemplate[] templates = Factory.Load<StmTemplate>(query);

			SortedDictionary<string, IList<string>> result = new SortedDictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

			foreach (StmTemplate template in templates)
			{
				string groupKey = string.Format("DataContext: {0}", template.SO_DataContext);
				StmTemplateBase templateOnDisk = Factory.New<StmTemplateBase>();
				ZBlob templateBlob = StmTemplateBase.GetTemplateBlobFromFile(Path.Combine(BuildConstants.LocalEnterprisePath, template.SO_ExcelTemplatePath));

				if (template.SO_Template != templateBlob)
				{
					IList<string> errors;

					if (!result.TryGetValue(groupKey, out errors))
					{
						errors = new List<string>();
						result.Add(groupKey, errors);
					}

					errors.Add(String.Format("{0} ({1})", template.SO_Name, template.SO_ExcelTemplatePath));
				}
			}

			AssertGroupedErrorList("The following template(s) stored in DB differ from checked in templates(s):", result);
		}
	}
}
