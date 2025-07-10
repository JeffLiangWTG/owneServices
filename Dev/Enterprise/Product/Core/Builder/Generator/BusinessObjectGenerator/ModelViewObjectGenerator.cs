using System;
using System.Collections.Generic;
using System.IO;
using Enterprise.BusinessObjectGenerator;
using Enterprise.BusinessObjectGenerator.ModelView;

namespace Enterprise.Builder.Generator
{
	public class ModelViewObjectGenerator : AbstractGenerator
	{
		public ModelViewObjectGenerator(GeneratorOutputDirectory outputDirectory) : base(outputDirectory)
		{
			projectRootPath = Path.Combine(outputDirectory.CWSharedSourceDirectory, ModelViewConstants.RelativeProjectRootPath);
		}

		public override void Generate()
		{
			if (AllModelFiles.Count == 0)
			{
				OnAddReportLine($"\tERROR: Cannot find any {ModelViewConstants.ModelFileSuffix} files.");
				return;
			}

			deferModelViewGeneration = true;
			OnAddReportLine("\tAll ModelView objects will be generated.");

			foreach (var modelFilename in AllModelFiles.Keys)
			{
				OnAddReportLine($"\t{modelFilename}");
				GenerateSpecificFile(modelFilename);
				OnAddReportLine($"\tModelView objects generated - {modelFilename}");
				OnAddFileGeneratedLine(modelFilename);
			}

			var codeCollection = new ModelViewCodeCollection(null, AllContexts);
			outputDirectory.WriteToFile(FilePathOfModelViews, codeCollection.ModelViews.SourceCode);
			deferModelViewGeneration = false;
		}

		public override void GenerateSpecificFile(string fileName)
		{
			string filenameWithExt;
			if (fileName.EndsWith(ModelViewConstants.ModelFileSuffix, StringComparison.InvariantCultureIgnoreCase))
			{
				filenameWithExt = fileName;
			}
			else
			{
				filenameWithExt = fileName + ModelViewConstants.ModelFileSuffix;
			}

			if (!AllModelFiles.TryGetValue(filenameWithExt, out var filepath))
			{
				throw new FileNotFoundException($"ModelView file cannot be found in {outputDirectory.CWSharedSourceDirectory}", fileName);
			}

			var context = ContextGenerator.GetCodeGeneratorContext(filepath);

			var codeCollection = new ModelViewCodeCollection(context, AllContexts);

			if (!deferModelViewGeneration)
			{
				outputDirectory.WriteToFile(FilePathOfModelViews, codeCollection.ModelViews.SourceCode);
				OnAddReportLine("\t\tModelViews class created");
			}

			outputDirectory.WriteToFile(context.FilePathOfModelDefinition, codeCollection.ModelViewDefinition.SourceCode);
			OnAddReportLine("\t\tModelDefinition class created");

			outputDirectory.WriteToFile(context.FilePathOfSqlView, codeCollection.SqlView.SourceCode);
			OnAddReportLine("\t\tSqlView script created");

			if (context.HasIndex)
			{
				outputDirectory.WriteToFile(context.FilePathOfSqlIndexView, codeCollection.SqlIndexedView.SourceCode);
				OnAddReportLine("\t\tSqlIndexView script created");
			}
			else
			{
				OnAddSkippedFile($"No index found, skipping SqlIndexView - {fileName}");
			}
		}

		public override void GenerateAllFilesForSolution(string solutionName)
		{
			Generate();
		}

		public string FilePathOfModelViews => Path.Combine(projectRootPath, "ModelViews.cs");

		public virtual ModelViewContextGenerator ContextGenerator => contextGenerator ??= new ModelViewContextGenerator(outputDirectory.CWSharedSourceDirectory);
		ModelViewContextGenerator contextGenerator;

		public List<ModelViewContext> AllContexts => allContexts ??= ContextGenerator.GetAllCodeGeneratorContexts(projectRootPath);
		List<ModelViewContext> allContexts;

		Dictionary<string, string> AllModelFiles => ContextGenerator.AllModelFiles;

		readonly string projectRootPath;
		bool deferModelViewGeneration;
	}
}
