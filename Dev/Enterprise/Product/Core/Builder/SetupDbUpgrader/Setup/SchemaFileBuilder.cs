namespace Enterprise.Builder.GenerateDbUpgraderResources
{
	using System;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;

	class SchemaFileBuilder
	{
		public SchemaFileBuilder(string sourceSchemaScriptWindowsPath, string mainDbSchemaFilePath, string docManagerDbSchemaFilePath)
		{
			this.sourceSchemaScriptWindowsPath = sourceSchemaScriptWindowsPath;
			this.mainDbSchemaFilePath = mainDbSchemaFilePath;
			this.docManagerDbSchemaFilePath = docManagerDbSchemaFilePath;
		}

		public void RecreateFile()
		{
			if (!IsSourceBatchesFileValid(sourceSchemaScriptWindowsPath))
			{
				throw new Exception("Latest schema creation script is not valid.");
			}

			WriteToResourceFiles();
		}

		bool IsSourceBatchesFileValid(string sourceFilePath)
		{
			bool result = false;

			if (File.Exists(sourceFilePath))
			{
				string fileContents = File.ReadAllText(sourceFilePath).Trim();

				// To be a valid creation script the file should:
				//   be a text file (no char(0) characters),
				//   contain CREATE TABLE statments,
				//   not contain DROP statements
				result = !((fileContents.IndexOf((char)0) >= 0)
					|| (!Regex.IsMatch(fileContents, @"[\n\s]+CREATE\s+TABLE\s+", RegexOptions.IgnoreCase))
					|| (Regex.IsMatch(fileContents, @"[\n\s]+DROP\s+", RegexOptions.IgnoreCase)));
			}

			return result;
		}

		void WriteToResourceFiles()
		{
			using (TextReader streamReader = new StreamReader(sourceSchemaScriptWindowsPath))
			using (TextWriter mainDbStreamWriter = new StreamWriter(mainDbSchemaFilePath))
			using (TextWriter docManStreamWriter = new StreamWriter(docManagerDbSchemaFilePath))
			{
				string line = "";

				while (line != null)
				{
					while (line != null && !CreateOrAlterRegex.IsMatch(line))
					{
						line = streamReader.ReadLine();
					}

					if (line != null)
					{
						var commandBuilder = new StringBuilder(4096);

						commandBuilder.AppendLine(line);
						while (!line.EndsWith(";", StringComparison.Ordinal))
						{
							line = streamReader.ReadLine();
							if (line == null)
							{
								break;
							}
							commandBuilder.AppendLine(line);
						}
						if (line != null)
						{
							line = streamReader.ReadLine();
						}

						WriteCommandToFiles(commandBuilder.ToString(), mainDbStreamWriter, docManStreamWriter);
					}
				}
			}
		}

		void WriteCommandToFiles(string sqlScript, TextWriter mainDbStreamWriter, TextWriter docManStreamWriter)
		{
			ValidateCreateScript(sqlScript);

			mainDbStreamWriter.WriteLine(sqlScript);

			if (DocManRegex.IsMatch(sqlScript))
			{
				docManStreamWriter.WriteLine(sqlScript);
			}
		}

		/// <summary>
		/// Prevents use of NEWID, GETDATE and some other nondeterministic defaults
		/// </summary>
		void ValidateCreateScript(string sqlScript)
		{
			var nondeterministicDefaultMatch = NondeterministicDefaultRegex.Match(sqlScript);
			if (nondeterministicDefaultMatch.Success)
			{
				throw new InvalidOperationException(String.Format("Use of non-deterministic default [{0}] is not allowed.", nondeterministicDefaultMatch.Value));
			}

			var imageMatch = ImageTypeRegex.Match(sqlScript);
			if (imageMatch.Success)
			{
				throw new InvalidOperationException(string.Format("Use of IMAGE data type is not allowed [{0}]", imageMatch.Value));
			}

			var ntextMatch = NTextTypeRegex.Match(sqlScript);
			if (ntextMatch.Success)
			{
				throw new InvalidOperationException(string.Format("Use of NTEXT data type is not allowed [{0}]", ntextMatch.Value));
			}

			var textMatch = TextTypeRegex.Match(sqlScript);
			if (textMatch.Success)
			{
				throw new InvalidOperationException(string.Format("Use of TEXT data type is not allowed [{0}]", textMatch.Value));
			}

			var createTimeMatch = Regex.Match(sqlScript, "_SystemCreateTime\b");
			if (createTimeMatch.Success)
			{
				throw new InvalidOperationException(string.Format("Use of non UTC createTime is not allowed [{0}]", createTimeMatch.Value));
			}

			var lastEditTimeMatch = Regex.Match(sqlScript, "_SystemLastEditTime\b");
			if (lastEditTimeMatch.Success)
			{
				throw new InvalidOperationException(string.Format("Use of non UTC LastEditTime is not allowed [{0}]", lastEditTimeMatch.Value));
			}
		}

		static readonly Regex ImageTypeRegex = new Regex(@"\bIMAGE\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		static readonly Regex NTextTypeRegex = new Regex(@"\bNTEXT\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		static readonly Regex TextTypeRegex = new Regex(@"\bTEXT\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		static readonly Regex NondeterministicDefaultRegex = new Regex(@"\bDEFAULT\s+((NEWSEQUENTIALID|SYSDATETIME|SYSDATETIMEOFFSET|GETDATE)\(\)|CURRENT_TIMESTAMP\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		static readonly Regex CreateOrAlterRegex = new Regex(@"^\s*(CREATE|ALTER\s+TABLE)\s+", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		static readonly Regex DocManRegex = new Regex(@"^\s*((((CREATE|ALTER)\s+TABLE)\s+(\[)?(dbo\.)?(StorageDocs)\b)|(CREATE\s+(\w+\s+)*INDEX\s+([\[\w\]\s])+\s+ON\s+(\[)?(dbo\.)?(StorageDocs)\b))", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

		readonly string sourceSchemaScriptWindowsPath;
		readonly string mainDbSchemaFilePath;
		readonly string docManagerDbSchemaFilePath;
	}
}
