using System;
using System.Text;

namespace Enterprise.DbUpgrader.Data
{
	/// <summary>
	/// A single task in the Data Upgrade process
	/// </summary>
	public class EmbeddedUpgradeTask : UpgradeTask
	{
		public EmbeddedUpgradeTask(EmbeddedDataFile resourceDataFile)
			: base(resourceDataFile)
		{
			ResourceFile = resourceDataFile;
		}

		public override bool IsRequired
		{
			get { return ResourceFile.Version != ResourceFile.VersionInDatabase; }
		}

		public override string TaskNameWhenUpgrading
		{
			get
			{
				if (SuppressVersionsWhenUpgrading)
				{
					return base.TaskNameWhenUpgrading;
				}

				var builder = new StringBuilder();
				builder.Append(base.TaskNameWhenUpgrading);
				builder.Append(" from version ");
				builder.Append(ResourceFile.VersionInDatabase);
				builder.Append(" to ");
				builder.Append(ResourceFile.Version);
				builder.Append(".");
				return builder.ToString();
			}
		}

		protected virtual bool SuppressVersionsWhenUpgrading
		{
			get { return false; }
		}

		protected override void UpdateVersionNumber()
		{
			ResourceFile.VersionInDatabase = ResourceFile.Version;
		}

		protected override ApplicationException GetNewException(Exception e)
		{
			return new ApplicationException("Error encountered while upgrading data for '" + ResourceFile.FileResourceName + "' from version " + ResourceFile.VersionInDatabase + " to version " + ResourceFile.Version + System.Environment.NewLine + e.Message, e);
		}

		public readonly new EmbeddedDataFile ResourceFile;
	}
}
