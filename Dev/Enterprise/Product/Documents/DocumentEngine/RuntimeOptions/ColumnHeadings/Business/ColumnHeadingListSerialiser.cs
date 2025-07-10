using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	static class ColumnHeadingListSerialiser
	{
		public static ReportColumnSettings Deserialise(string xml, ColumnSettingXMLVersionUpgraderCurrentVersionParameters parameters)
		{
			ColumnSettingXMLVersionUpgrader upgrader = new ColumnSettingXMLVersionUpgraderCurrentVersion(parameters);
			xml = upgrader.Upgrade(xml);
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(ReportColumnSettings));
			return (ReportColumnSettings)serialiser.Deserialize(new StringReader(xml));
		}

		public static ReportColumnSettings MergeDeserialisedAndTemplateHeadings(ReportColumnSettings deserialisedHeadings, DefaultTemplateConfigurationManager templateSetting, Report report)
		{
			ReportColumnSettings resultColumnSettings = templateSetting.GetCopyOfHeadings();
			foreach (Worksheet deserialisedWorkSheet in deserialisedHeadings.Worksheets)
			{
				if (resultColumnSettings.Worksheets.Contains(deserialisedWorkSheet.Name))
				{
					if (deserialisedWorkSheet.ColumnHeadings.Count > 0)
					{
						var resultWorkSheet = resultColumnSettings.Worksheets[deserialisedWorkSheet.Name];
						foreach (ColumnHeading defaultHeading in resultWorkSheet.ColumnHeadings)
						{
							defaultHeading.Hidden = true;
						}

						foreach (ColumnHeading deserialisedHeading in deserialisedWorkSheet.ColumnHeadings)
						{
							if (resultWorkSheet.ColumnHeadings.Contains(deserialisedHeading.DisplayLabel))
							{
								ColumnHeading resultHeading = resultWorkSheet.ColumnHeadings[deserialisedHeading.DisplayLabel];
								if (!deserialisedHeading.Hidden)
								{
									resultHeading.Hidden = false;
									if (deserialisedHeading.CurrentPosition >= 0)
									{
										resultHeading.CurrentPosition = deserialisedHeading.CurrentPosition;
									}
									if (!string.IsNullOrEmpty(deserialisedHeading.Description))
									{
										resultHeading.Description = deserialisedHeading.Description;
									}
									if (!string.IsNullOrEmpty(deserialisedHeading.HeadingText))
									{
										resultHeading.HeadingText = deserialisedHeading.HeadingText;
									}
									if (!string.IsNullOrEmpty(deserialisedHeading.TagName))
									{
										resultHeading.TagName = deserialisedHeading.TagName;
									}
									if (deserialisedHeading.WidthInPixels > 0)
									{
										resultHeading.WidthInPixels = deserialisedHeading.WidthInPixels;
									}
								}
							}
						}

						if (!string.IsNullOrEmpty(deserialisedWorkSheet.Title))
						{
							resultWorkSheet.Title = deserialisedWorkSheet.Title;
						}
					}
				}
				else if (report == null || !report.IsAnalyzed)
				{
					var factory = new BusinessObjectFactory() { NameForDebugging = "ColumnHeadingListSerialiser error reporter" };
					var reportMenuItem = factory.Load<StmMenuItem>(templateSetting.ReportID);
					string errorMessage = Res.GetString("e4c24847-aaba-40bf-8b55-55c2b7f8cddf"
							, @"The Configuration you have selected has Column Configurations in it for a Worksheet [{0}] that no longer exists.

This can happen when the design of the report has been changed since the time that the Configuration you are trying to use was last saved.

These settings have been ignored, please check your Configuration settings then Save them to remove the redundant settings from your Configuration.

Report details:
- Menu Name: {1}
- Business Context: {2}
- Worksheet Name: {0}",
						  deserialisedWorkSheet.Name,
						  reportMenuItem != null ? reportMenuItem.SU_MenuName : ZString.Empty,
						  reportMenuItem != null ? reportMenuItem.SU_BusinessContext : ZString.Empty);

					var unattendedUserNotification = Globals.Message as UnattendedUserNotification;
					if (unattendedUserNotification != null)
					{
						string email = GetEmailOfLastEditingUser(report);
						if (!string.IsNullOrWhiteSpace(email))
						{
							unattendedUserNotification.ShowWarningToEmail(errorMessage, email);
						}
						else
						{
							Globals.Message.ShowWarning(errorMessage);
						}
					}
					else
					{
						Globals.Message.ShowWarning(errorMessage);
					}
				}
			}
			return resultColumnSettings;
		}

		internal static string GetEmailOfLastEditingUser(Report report)
		{
			if (report == null)
			{
				return null;
			}

			if (report.ScheduleTask == null)
			{
				return null;
			}

			return report.ScheduleTask.EmailAddressForReportingErrors;
		}

		public static string Serialise(ReportColumnSettings reportColumnSettings)
		{
			using (StringWriter stream = new StringWriter())
			{
				ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(ReportColumnSettings));
				serialiser.Serialize(stream, reportColumnSettings.CloneVisibleOnly());
				return stream.ToString();
			}
		}
	}
}
