using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations;

namespace Enterprise.DbUpgrader.Transformation.DataModification
{
	public class DataTransformationUpgrader : BaseUpgrader
	{
		public DataTransformationUpgrader(IUpgradeManager manager, TransformationDirector director, DbConnection upgConnection)
			: base(manager, upgConnection, manager.TransformationVersionBeforeUpgrade)
		{
			this.director = director;
		}

		protected override void DoUpgrade()
		{
			RunTransformations();
			UpdateTransformationVersion();
		}

		public override int EstimatedNumberOfTasks
		{
			get { return 8; }
		}

		public override IEnumerable<string> SecondaryDatabasesToUpgrade
		{
			get { return Array.Empty<string>(); }
		}

		public override string Name
		{
			get { return "Database Transformation Upgrade"; }
		}

		public ITriggerTransformation[] GetTriggerTransformations()
		{
			return director.GetTriggerTransformations();
		}

		protected override VersionLabel LatestVersion
		{
			get { return TransformationVersion.ApplicationNumber; }
		}

		readonly TransformationDirector director;

		[SuppressMessage("CargoWiseOne", "CW1031:DoNotUseGetVersionExOrEnvironmentDotOSVersionRule", Justification = "Printing version in error details")]
		protected void RunTransformations()
		{
			StartTask("Running data transformations");

			try
			{
				director.OfflinePostUpgradeRun();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				var builder = new StringBuilder();
				builder.AppendLine("Failed to run data transformations.");
				builder.AppendLine(e.Message);

				try
				{
					var os = System.Environment.OSVersion;
					builder.AppendLine("------------------");
					builder.AppendLine("Environment Info: ");
					builder.AppendLine("------------------");
					builder.AppendLine("OS: " + os.ToString());
					builder.AppendLine("OS Type: " + os.Platform.ToString());
					builder.AppendLine("OS Version: " + os.Version.ToString());
					builder.AppendLine("CLR Version: " + System.Environment.Version.ToString());
					builder.AppendLine("Process Identity: " + WindowsIdentity.GetCurrent().Name);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					//Failed to get OS details. We still want to propagate original exception
				}

				throw new Exception(builder.ToString(), e);
			}
		}

		protected void UpdateTransformationVersion()
		{
			StartTask("Updating transformation version information");

			try
			{
				DbRegistry.DatabaseMajorTransformationVersion.SaveValue(TransformationVersion.ApplicationNumber.Major, Db.Connection);
				DbRegistry.DatabaseMinorTransformationVersion.SaveValue(TransformationVersion.ApplicationNumber.Minor, Db.Connection);
			}
			catch (Exception e)
			{
				throw new Exception("Failed to update transformation version information.\r\n" + e.Message, e);
			}
		}

		public IUpgradeAction AutomatedTransformations
		{
			get
			{
				var list = new UpgradeActionList("Running automated transformations");
				list.Add(new Internal.AutoTransforms.CleanupStmNumberCache(Manager).Run);

				return list;
			}
		}
	}
}
