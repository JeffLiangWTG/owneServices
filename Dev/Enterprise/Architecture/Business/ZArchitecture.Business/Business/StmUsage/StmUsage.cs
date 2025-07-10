using System.Data;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public sealed class StmUsage : AutoStmUsage
	{
		public StmUsage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public Statistics.Xml.IUsages UsageActionsSettings
		{
			get
			{
				if (usageActionsSettings == null)
				{
					usageActionsSettings = DataMapping.XmlSerializableSetting.FromXml<Statistics.Xml.Usages>(Encoding.UTF8.GetString(XW_UsageActions));
					if (usageActionsSettings == null)
					{
						usageActionsSettings = new Statistics.Xml.Usages();
					}
				}
				return usageActionsSettings;
			}
			set
			{
				string xml = value.AsXml();
				XW_UsageActions = Encoding.UTF8.GetBytes(xml);
				usageActionsSettings = value;
			}
		}

		Statistics.Xml.IUsages usageActionsSettings;

		public override ZBlob XW_UsageActions
		{
			get
			{
				return base.XW_UsageActions;
			}
			set
			{
				usageActionsSettings = null;
				base.XW_UsageActions = value;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			XW_MachineName = System.Environment.MachineName;
			XW_ExeVersion = ReleaseInfo.Instance.VersionNumber.ToString();
			XW_StartTimeUtc = ZDateTime.UtcNow;
		}

		protected override void ReloadCore()
		{
			usageActionsSettings = null;
			base.ReloadCore();
		}

		public override bool CanDelete
		{
			get { return false; }
		}

		public override void Delete()
		{
			base.Delete();

			ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Attempted to delete {0} record. That's not supported.", GetType().Name));
		}
	}
}
