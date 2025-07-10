using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Builder.Generator.Cache;
using Enterprise.ZArchitecture.GUI;
using static System.FormattableString;

namespace Enterprise.Builder.Generator
{
	/// <summary>
	/// Business Object source file generator, based on the database tables or .XSD files.
	/// </summary>
	public class BizObjGenerator : AbstractGenerator
	{
		public BizObjGenerator(GeneratorOutputDirectory outputDirectory)
			: base(outputDirectory)
		{
		}

		public override void Generate()
		{
			var modelViewTask = Task.Run(() =>
			{
				ModelViewObjectGenerator.Generate();
			});

			var schemaTask = Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					SchemaGenerator.GenerateSchemaForNonBusinessObjectTables();
					SchemaGenerator.Generate();
				}
			});

			var singleBizoGenerators = AllSingleBizObjGenerators.Cast<SingleBizObjGenerator>().Where(generator => !generator.IsClientSpecific);
			Parallel.ForEach(singleBizoGenerators, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, generator =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					generator.Generate();
				}
			});

			/* don't generate client-specific tables for now, this can be done by running -Bizobj command line option
			foreach (var generator in AllSingleBizObjGenerators.Cast<SingleBizObjGenerator>().Where(generator => generator.IsClientSpecific))
			{
				generator.Generate();
			}*/

			Task.WaitAll(modelViewTask, schemaTask);
		}

		#region Hooking up Schema Generator Events

		public override void AddErrorLineEvent(GeneratorEvent gE)
		{
			base.AddErrorLineEvent(gE);
			SchemaGenerator.AddErrorLine += gE;
			ModelViewObjectGenerator.AddErrorLine += gE;
		}

		public override void AddReportLineEvent(GeneratorEvent gE)
		{
			base.AddReportLineEvent(gE);
			SchemaGenerator.AddReportLine += gE;
			ModelViewObjectGenerator.AddReportLine += gE;
		}

		public override void AddFileGeneratedLineEvent(GeneratorEvent gE)
		{
			base.AddFileGeneratedLineEvent(gE);
			SchemaGenerator.AddFileGeneratedLine += gE;
			ModelViewObjectGenerator.AddFileGeneratedLine += gE;
		}

		public override void AddSkippedFileEvent(GeneratorEvent gE)
		{
			base.AddSkippedFileEvent(gE);
			SchemaGenerator.AddSkippedFile += gE;
			ModelViewObjectGenerator.AddSkippedFile += gE;
		}

		#endregion

		#region Generating Business Objects

		public override void GenerateAllFilesForSolution(string solutionName)
		{
			var generators = MatchingSingleBizObjectGeneratorsForSolution(solutionName);

			if (generators.Count == 0)
			{
				OnAddErrorLine("No Business Objects were found in the '" + solutionName + "' solution!");
			}
			else
			{
				GenerateBusinessObjects(generators);
			}
		}

		public override void GenerateSpecificFile(string commandLineFileName) => GenerateSpecificFile(commandLineFileName, excludeClientSpecific: false, excludeAutoEnterpriseSchema: false, exactMatch: false);

		public bool GenerateSpecificFile(string commandLineFileName, bool excludeClientSpecific, bool excludeAutoEnterpriseSchema, bool exactMatch)
		{
			commandLineFileName = commandLineFileName.Trim();

			if (commandLineFileName.Length == 0)
			{
				OnAddErrorLine("No Business Object name was supplied!");
				return false;
			}

			var bizObjNameOnly = NormalizeCommandLineName(commandLineFileName);
			var generators = MatchingSingleBizObjectGeneratorsForBizObj(bizObjNameOnly, exactMatch: exactMatch);

			if (generators.Count == 0)
			{
				OnAddErrorLine("No Business Objects matching '" + bizObjNameOnly + "' were found in " + BuildXmlFileName + " or you do not have sufficient Team Foundation access. \nThis program will now terminate.");
				return false;
			}

			generators = generators.Where(g => !excludeClientSpecific || !g.IsClientSpecific).ToList();

			if (generators.Count == 0)
			{
				OnAddReportLine($"Skipping client-specific business object: {bizObjNameOnly}");
				return true;
			}

			GenerateBusinessObjects(generators, excludeAutoEnterpriseSchema);
			return true;
		}

		string NormalizeCommandLineName(string commandLineName)
		{
			var start = 0;
			var end = commandLineName.Length;

			if (commandLineName.StartsWith("Auto"))
			{
				start += 4;
			}

			if (commandLineName.EndsWith(".cs"))
			{
				end -= 3;
			}

			return commandLineName.Substring(start, end - start);
		}

		void GenerateBusinessObjects(List<SingleBizObjGenerator> generators, bool excludeAutoEnterpriseSchema = false)
		{
			string message = System.Environment.NewLine + "About to generate files for " + generators.Count + " business object" + (generators.Count == 1 ? "" : "s") + ":" + System.Environment.NewLine + System.Environment.NewLine;

			foreach (var generator in generators)
			{
				message += "[" + Path.GetFileNameWithoutExtension(generator.FileNameOfBusinessObject) + "]" + System.Environment.NewLine;

				foreach (string fileName in generator.ListOfFilesToBeGenerated)
				{
					message += "   " + fileName + System.Environment.NewLine;
				}

				message += System.Environment.NewLine;
			}

			message += "[Z AutoSchema]" + System.Environment.NewLine;
			message += SchemaGenerator.FileName + System.Environment.NewLine;
			message += System.Environment.NewLine + "Continue with generation?";

			if (NUnit.Framework.TestingState.IsRunningTests ||
				ZFormModaliser.ShowDialogAndDispose(new ZMessageBoxWithFixedSizeWithoutMultilingualString(message, "Generating Business Objects..", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) == DialogResult.Yes)
			{
				foreach (SingleBizObjGenerator generator in generators)
				{
					generator.Generate();
				}

				if (!excludeAutoEnterpriseSchema)
				{
					SchemaGenerator.Generate();
				}
			}
		}

		List<SingleBizObjGenerator> MatchingSingleBizObjectGeneratorsForBizObj(string nameToMatch, bool exactMatch = false)
		{
			nameToMatch = nameToMatch.ToLower();
			var result = new List<SingleBizObjGenerator>();

			foreach (BuildXmlBizOEntry entry in BuildXml.Instance.AllBusinessObjects) // we could iterate through AllSingleBizObjGenerators but this is faster
			{
				var tableName = entry.TableName.ToLower();
				if (exactMatch)
				{
					if (tableName.Equals(nameToMatch))
					{
						var generators = CreateGeneratorsForBizObjOrXsdOrXML(entry);
						result.AddRange(generators);
						break;
					}
				}
				else if (tableName.IndexOf(nameToMatch) != -1)
				{
					var generators = CreateGeneratorsForBizObjOrXsdOrXML(entry);
					result.AddRange(generators);
				}
			}

			return result;
		}

		protected List<SingleBizObjGenerator> MatchingSingleBizObjectGeneratorsForSolution(string nameToMatch)
		{
			nameToMatch = nameToMatch.ToLower();
			var result = new List<SingleBizObjGenerator>();

			foreach (BuildXmlBizOEntry entry in BuildXml.Instance.AllBusinessObjects) // we could iterate through AllSingleBizObjGenerateors but this is faster
			{
				if (entry.SolutionName.ToLower() == nameToMatch)
				{
					var generators = CreateGeneratorsForBizObjOrXsdOrXML(entry);
					result.AddRange(generators);
				}
			}

			return result;
		}

		#endregion

		#region ModelView Object Generation

		protected ModelViewObjectGenerator ModelViewObjectGenerator => modelViewObjectGenerator ??= new ModelViewObjectGenerator(outputDirectory);
		ModelViewObjectGenerator modelViewObjectGenerator;

		#endregion

		#region Schema Column List Generator

		protected AutoSchemaGenerator SchemaGenerator
		{
			get
			{
				if (fSchemaGenerator == null)
				{
					fSchemaGenerator = new AutoSchemaGenerator(outputDirectory);
				}
				return fSchemaGenerator;
			}
		}

		AutoSchemaGenerator fSchemaGenerator;

		#endregion

		#region Single BusinessObject Generators

		List<SingleBizObjGenerator> AllSingleBizObjGenerators
		{
			get
			{
				if (fAllSingleBizObjGenerators == null)
				{
					fAllSingleBizObjGenerators = new List<SingleBizObjGenerator>();
					var processedBizObjNames = new ConcurrentDictionary<string, List<SingleBizObjGenerator>>();

					var bizoEntries = BuildXml.Instance.AllBusinessObjects.Cast<BuildXmlBizOEntry>();
					Parallel.ForEach(bizoEntries, entry =>
					{
						if (!string.IsNullOrEmpty(entry.TableName))
						{
							var bizObjNameWithoutXsdExtension = Path.GetFileNameWithoutExtension(entry.TableName);
							if (!processedBizObjNames.TryAdd(bizObjNameWithoutXsdExtension, CreateGeneratorsForBizObjOrXsdOrXML(entry)))
							{
								OnAddReportLine("\tERROR: Business Object <" + bizObjNameWithoutXsdExtension + "> is mentioned more than once in " + BuildXmlFileName);
							}
						}
					});

					fAllSingleBizObjGenerators.AddRange(processedBizObjNames.SelectMany(s => s.Value.Cast<SingleBizObjGenerator>()).ToList());
				}

				return fAllSingleBizObjGenerators;
			}
		}

		List<SingleBizObjGenerator> CreateGeneratorsForBizObjOrXsdOrXML(BuildXmlBizOEntry entry)
		{
			List<SingleBizObjGenerator> result;

			try
			{
				if (IsXsdFile(entry.TableName))
				{
					result = CreateSingleBizObjXsdGenerators(entry);
				}
				else if (IsXmlFile(entry.TableName))
				{
					var generator = CreateSingleBizObjXMLGenerator(entry);
					result = new List<SingleBizObjGenerator> { generator };
				}
				else
				{
					var generator = CreateSingleBizObjGenerator(entry);
					result = new List<SingleBizObjGenerator>(1);
					if (generator != null)
					{
						result.Add(generator);
					}
				}

				foreach (var addInfoEntry in entry.AddInfoEntries)
				{
					var generator = CreateAddInfoBizObjGenerator(entry, addInfoEntry);
					if (generator != null)
					{
						result.Add(generator);
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("{0}\r\nSolution: {1}, BizObj: {2}", ex.Message, entry.SolutionName, entry.TableName), ex);
			}

			return result;
		}

		AddInfoBizObjGenerator CreateAddInfoBizObjGenerator(BuildXmlBizOEntry entry, BuildXmlAddInfoEntry addInfoEntry)
		{
			AddInfoBizObjGenerator generator = null;

			var solutionName = addInfoEntry.SolutionName;
			var bizObjFileName = CSharpFileName("Auto" + addInfoEntry.ViewName);
			var bizObjFullFileName = FullFileNameInSolution(bizObjFileName, solutionName);

			if (bizObjFullFileName != null && bizObjFullFileName.Length > 0)
			{
				generator = new AddInfoBizObjGenerator(bizObjFullFileName, addInfoEntry.ViewName, entry.TableName, outputDirectory, addInfoEntry.OldPrefix, addInfoEntry.ParentSchema, addInfoEntry.ChildTableName, addInfoEntry.ChildForeignKey);

				generator.AddErrorLine = AddErrorLine;
				generator.AddReportLine = AddReportLine;
				generator.AddSkippedFile = AddSkippedFile;
				generator.AddFileGeneratedLine = AddFileGeneratedLine;
			}
			return generator;
		}

		SingleBizObjGenerator CreateSingleBizObjXMLGenerator(BuildXmlBizOEntry entry)
		{
			string bizObjName = entry.TableName;
			string solutionName = entry.SolutionName;

			if (IsZArchitectureSolution(solutionName))
			{
				throw new Exception("The BusinessObjectGenerator does not support generating .XML files into the ZArchitecture solution.");
			}

			string xmlFullFileName = FullFileNameInSolution(bizObjName, solutionName);
			DataTable table;
			string bizObjFullFileName;
			if (!xmlFullFileName.IsNullOrEmpty() && File.Exists(xmlFullFileName))
			{
				string directoryName = Path.GetDirectoryName(xmlFullFileName);
				table = TableFromXMLFile(xmlFullFileName);
				bizObjFullFileName = Path.Combine(directoryName, CSharpFileName("Auto" + table.TableName));
			}
			else
			{
				bizObjFullFileName = GetObjFullFileName(bizObjName, solutionName);
				table = GetTableFromCustomsAddInfoSchema(bizObjName, solutionName);
			}

			SingleBizObjGeneratorFromTable generatorFromXML = new SingleBizObjGeneratorFromTable(bizObjFullFileName, table, outputDirectory, entry.ConvertZStringToWesternEuropeanCharacters);
			generatorFromXML.AddErrorLine = AddErrorLine;
			generatorFromXML.AddReportLine = AddReportLine;

			return generatorFromXML;
		}

		string GetObjFullFileName(string bizObjName, string solutionName)
		{
			int schemaIndex = bizObjName.LastIndexOf("Schema.xml", StringComparison.OrdinalIgnoreCase);
			if (schemaIndex > 0)
			{
				var fileNameWithoutSchema = bizObjName.Substring(0, schemaIndex);
				var fileNameIndex = fileNameWithoutSchema.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);
				if (fileNameIndex > 0)
				{
					fileNameWithoutSchema = fileNameWithoutSchema.Substring(fileNameIndex + 1);
				}
				string bizObjFileName = CSharpFileName("Auto" + fileNameWithoutSchema);
				return FullFileNameInSolution(bizObjFileName, solutionName);
			}
			else
			{
				throw new ArgumentException(Invariant($"The XML Schema file name {bizObjName} should end with 'Schema.xml'."));
			}
		}

		DataTable GetTableFromCustomsAddInfoSchema(string bizObjName, string solutionName)
		{
			var xml = CargoWise.CustomsAddInfoSchema.XMLExtractor.Extract(bizObjName);
			if (xml != null)
			{
				return TableFromXMLString(xml);
			}
			else
			{
				throw new ArgumentException(Invariant($"The XML Schema {bizObjName} cannot be found in either the solution {solutionName} or from CargoWise.CustomsAddInfoSchema.dll."));
			}
		}

		SingleBizObjGenerator CreateSingleBizObjGenerator(BuildXmlBizOEntry entry)
		{
			SingleBizObjGenerator generator = null;

			string solutionName = entry.SolutionName;
			string bizObjName = entry.TableName;
			string bizObjFileName = CSharpFileName("Auto" + bizObjName.Substring(bizObjName.LastIndexOf('.') + 1));
			var fileInfo = new BusinessObjectFileNamesInfo();
			string bizObjFullFileName = FullFileNameInSolution(bizObjFileName, solutionName, fileInfo);

			if (bizObjFullFileName != null && bizObjFullFileName.Length > 0)
			{
				if (IsZArchitectureSolution(solutionName))
				{
					generator = new SingleBizObjLivesInZArchitectureGenerator(bizObjFullFileName, outputDirectory);
				}
				else
				{
					generator = new SingleBizObjGenerator(bizObjFullFileName, outputDirectory, entry.MasterFileReference);
				}

				generator.AddErrorLine = AddErrorLine;
				generator.AddReportLine = AddReportLine;
				generator.AddSkippedFile = AddSkippedFile;
				generator.AddFileGeneratedLine = AddFileGeneratedLine;
				generator.BusinessObjectFilesInfo = fileInfo;
			}
			return generator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		protected List<SingleBizObjGenerator> CreateSingleBizObjXsdGenerators(BuildXmlBizOEntry entry)
		{
			var result = new List<SingleBizObjGenerator>();

			string bizObjName = entry.TableName;
			string solutionName = entry.SolutionName;
			string xsdFullFileName = FullFileNameInSolution(bizObjName, solutionName);
			string directoryName;
			DataTableCollection tables;

			if (IsXmlFile(bizObjName))
			{
				DataTable table;
				if (!xsdFullFileName.IsNullOrEmpty() && File.Exists(xsdFullFileName))
				{
					directoryName = Path.GetDirectoryName(xsdFullFileName);
					table = TableFromXMLFile(xsdFullFileName);
				}
				else
				{
					var bizObjFullFileName = GetObjFullFileName(bizObjName, solutionName);
					directoryName = Path.GetDirectoryName(bizObjFullFileName);
					table = GetTableFromCustomsAddInfoSchema(bizObjName, solutionName);
				}

				DataSet dataSet = new DataSet();
				dataSet.Tables.Add(table);
				tables = dataSet.Tables;
			}
			else
			{
				directoryName = Path.GetDirectoryName(xsdFullFileName);
				tables = TablesFromXsdFile(xsdFullFileName);
			}

			if (IsZArchitectureSolution(solutionName))
			{
				throw new Exception("The BusinessObjectGenerator does not support generating .XSD files into the ZArchitecture solution.");
			}

			foreach (DataTable table in tables)
			{
				string bizObjFullFileName = Path.Combine(directoryName, CSharpFileName("Auto" + table.TableName));

				var generatorFromXsd = new SingleBizObjGeneratorFromTable(bizObjFullFileName, table, outputDirectory);
				generatorFromXsd.AddErrorLine = AddErrorLine;
				generatorFromXsd.AddReportLine = AddReportLine;
				result.Add(generatorFromXsd);
			}

			return result;
		}

		bool IsZArchitectureSolution(string solutionName)
		{
			return
				solutionName == "CargoWise.EntityFramework" ||
				solutionName == "Enterprise.ZArchitecture.Business" ||
				solutionName == "Enterprise.ZArchitecture.GUI";
		}

		List<SingleBizObjGenerator> fAllSingleBizObjGenerators;

		#endregion

		#region FileName in Solution

		string FullFileNameInSolution(string fileName, string solution, BusinessObjectFileNamesInfo fileInfo = null)
		{
			string result = "";

			if (string.IsNullOrEmpty(solution))
			{
				OnAddReportLine("\tERROR: " + fileName + " must specify a Solution in " + BuildXmlFileName);
				OnAddSkippedFile(fileName);
			}
			else
			{
				string solutionFileName = Path.Combine(outputDirectory.DevSourceDirectory, BuildXml.Instance.GetFileNameOfSolution(solution));

				if (string.IsNullOrEmpty(solutionFileName))
				{
					OnAddReportLine("\tERROR: " + fileName + " requires Solution " + solution + " to be specified in " + BuildXmlFileName);
					OnAddSkippedFile(fileName);
				}
				else
				{
					result = FileNameInSolution(fileName, solutionFileName, fileInfo);

					if (string.IsNullOrEmpty(result))
					{
						if (IsXsdFile(fileName))
						{
							OnAddReportLine("\tWARNING: " + fileName + " does not exist in Solution " + solutionFileName);
						}
						else if (!IsXmlFile(fileName))
						{
							result = Path.Combine(Path.GetDirectoryName(solutionFileName), fileName);

							string message =
								"\tWARNING: " +
								fileName + " does not exist uniquely in Solution " + solutionFileName + ". " +
								"Its presumed file name is \"" + result + "\".";

							OnAddReportLine(message);
						}
					}
				}
			}

			return result;
		}

		string FileNameInSolution(string fileName, string solutionFileName, BusinessObjectFileNamesInfo fileInfo)
		{
			List<string> filesFound = new List<string>();
			bool isCSharp = IsCSharpFile(fileName);

			foreach (string projectPath in SolutionCache.Instance.GetSolution(solutionFileName).Projects)
			{
				if (IsProjectFile(projectPath))
				{
					string projectBasePath = Path.GetDirectoryName(projectPath);
					var nodes = ProjectFilesCache.Instance.GetFilesInProject(projectPath);

					foreach (var node in nodes)
					{
						bool isXmlFile = NodeIsXmlFile(node);
						if (NodeIsCSharpFile(node) || isXmlFile || NodeIsXsdFile(node))
						{
							string attributeTag = "Include";

							string relativeFileName = node.Attributes[attributeTag].Value;

							if (Path.GetFileName(relativeFileName) == fileName)
							{
								string fullFileName = Path.Combine(projectBasePath, relativeFileName);
								filesFound.Add(fullFileName);
							}

							if (fileInfo != null && isCSharp)
							{
								SetBusinessFileInfo(node, fileName, projectBasePath, fileInfo);
							}
						}
					}
				}
			}

			if (filesFound.Count > 1)
			{
				OnAddReportLine("\tWARNING: " + fileName + " occurs several times in " + solutionFileName + ": " + string.Join(", ", filesFound));
				return "";
			}

			return filesFound.Count > 0 ? filesFound[0] : "";
		}

		void SetBusinessFileInfo(XmlNode node, string fileName, string projectBasePath, BusinessObjectFileNamesInfo fileInfo)
		{
			string attributeTag = "Include";
			string concreteClassFileName = fileName.StartsWith("Auto") ? fileName.Remove(fileName.LastIndexOf("Auto"), 4) : fileName;
			string validationFileName = concreteClassFileName.Insert(concreteClassFileName.LastIndexOf(".cs"), "Validation");
			string lookupsFileName = concreteClassFileName.Insert(concreteClassFileName.LastIndexOf(".cs"), "Lookups");
			string relativeFilePath = node.Attributes[attributeTag].Value;
			string relativeFileName = Path.GetFileName(relativeFilePath);

			if (relativeFileName == validationFileName)
			{
				fileInfo.BusinessObjectValidationConcreteClass = Path.Combine(projectBasePath, relativeFilePath);
			}
			else if (relativeFileName == lookupsFileName)
			{
				fileInfo.BusinessObjectLookupsConcreteClass = Path.Combine(projectBasePath, relativeFilePath);
			}
		}

		#endregion

		#region TableFromXMLFile

		protected DataTable TableFromXMLFile(string fileName)
		{
			return new DataTableGenerator().FromXMLFile(fileName);
		}

		protected DataTable TableFromXMLString(string xml)
		{
			return new DataTableGenerator().FromXMLString(xml);
		}

		#endregion

		#region Tables from XSD File

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		protected DataTableCollection TablesFromXsdFile(string fileName)
		{
			DataSet data = new DataSet();
			data.ReadXmlSchema(fileName);

			return data.Tables;
		}

		#endregion
	}
}
