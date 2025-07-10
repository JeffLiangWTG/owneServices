using System;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.Builder.Generator
{
	public delegate void SetupNewSchemaEvent(string message);

	/// <summary>
	/// This class controls the file generation on a New Database schema setup.
	/// It's called after the Generation of DbUpgrader files to:
	///   - Run DB Upgrader to update the schema of the developer's database
	///   - Import base data and test data
	///   - Regenerate DisplayLists
	///   - Regenerate DatabaseUtils
	///   - Regenerate Typed DataSets
	///   - Regenerate Business Objects
	/// </summary>
	public class Controller
	{
		public Controller(IProgressLogger logger, GeneratorOutputDirectory outputDirectory)
		{
			this.logger = logger;
			this.outputDirectory = outputDirectory;
		}

		readonly GeneratorOutputDirectory outputDirectory;

		public bool DoGeneration(RegenActions regenType)
		{
			using (Db.DisableSchemaVersionCheck())
			{
				try
				{
					UpgradeDatabase();
					GenerateBusinessObjects();
					GenerateEventTypes();

					outputDirectory.Save();

					return true;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					AddDetailMessage("ERROR: " + e);
					return false;
				}
			}
		}

		#region Implementation

		#region Event Firing

		protected IProgressLogger logger;

		public SetupNewSchemaEvent OnShowStatusMessage;

		protected void AddHeaderMessage(string text) => logger.AddProcessHeaderLine(text);
		protected void AddDetailMessage(string text) => logger.AddProcessDetailLine(text);
		protected void AddSkippedFile(string fileName) => logger.ReportSkippedFile(fileName);
		protected void ShowStatusMessage(string text) => logger.ShowStatusLine(text);

		#endregion // Event Firing

		#region UpgradeDatabase

		void UpgradeDatabase()
		{
			string taskName = "Upgrade Database Schema";
			AddHeaderMessage(taskName + " - Start");

			DbPreparation dbPrep = new DbPreparation();
			dbPrep.OnAddHeaderLine += new GeneratorEvent(AddDetailMessage);
			dbPrep.OnAddDetailLine += new GeneratorEvent(AddDetailMessage);
			dbPrep.OnAddErrorLine += new GeneratorEvent(AddDetailMessage);
			dbPrep.OnAddDetailLine += new GeneratorEvent(ShowStatusMessage);
			dbPrep.UpgradeDbForSetupNewSchema();

			AddDetailMessage(taskName + " - Complete");
		}

		#endregion // UpgradeDatabase

		void GenerateBusinessObjects()
		{
			string taskName = "Business Objects Generation";
			AddHeaderMessage(taskName + " - Start");

			BizObjGenerator generator = new BizObjGenerator(outputDirectory);
			generator.AddFileGeneratedLineEvent(new GeneratorEvent(AddDetailMessage));
			generator.AddErrorLineEvent(new GeneratorEvent(AddDetailMessage));
			generator.AddSkippedFileEvent(new GeneratorEvent(AddSkippedFile));
			generator.AddReportLineEvent(new GeneratorEvent(ShowStatusMessage));
			generator.Generate();

			AddDetailMessage(taskName + " - Complete");
		}

		void GenerateEventTypes()
		{
			string taskName = "Event Types Generation";
			AddHeaderMessage(taskName + " - Start");

			EventGenerator eventGenerator = new EventGenerator(outputDirectory);
			eventGenerator.OnTaskStarted += new GeneratorEvent(AddDetailMessage);
			eventGenerator.OnTaskFailed += new GeneratorEvent(AddDetailMessage);
			eventGenerator.OnSubtaskStarted += new GeneratorEvent(ShowStatusMessage);
			eventGenerator.GenerateStmEventConstants();

			AddDetailMessage(taskName + " - Complete");
		}

		#endregion // Implementation
	}
}
