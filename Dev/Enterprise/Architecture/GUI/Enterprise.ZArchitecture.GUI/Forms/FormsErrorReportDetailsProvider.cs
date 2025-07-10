using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	class FormsErrorReportDetailsProvider : IFormsErrorReportDetailsProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception string")]
		public string LastActiveFormInfo
		{
			get
			{
				var lastActiveForm = OpenedFormCache.LastActiveForm.Target as Form;
				return lastActiveForm != null ? lastActiveForm.GetType().ToString() : "Unknown";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message to be sent to us")]
		public string GetDetails()
		{
			var stringWriter = new StringWriter();
			using (var xtw = new CustomXmlWriter(stringWriter))
			{
				xtw.WriteStartElement("OpenedForms");

				try
				{
					OpenedFormCache openForms = OpenedFormCache.GetInstance();

					foreach (var openForm in openForms.GetAllOpenForms())
					{
						xtw.WriteStartElement("Form");

						try
						{
							xtw.WriteString(openForm.GetType().ToString());

							if (openForm is IDisplayModeAware)
							{
								xtw.WriteString(" ");
								xtw.WriteString(((IDisplayModeAware)openForm).DisplayMode.ToString());
							}
							xtw.WriteString(" (" + openForm.Text + ")");
						}
						finally
						{
							xtw.WriteEndElement();
						}
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					xtw.WriteString("Failed to get open form information:" + System.Environment.NewLine + ex.Message);
				}

				xtw.WriteEndElement();
			}

			return stringWriter.GetStringBuilder().ToString();
		}

		public string GetSystemResourcesUsageElements()
		{
			#if WINZOR
			return string.Empty;
			#else
			var stringWriter = new StringWriter();
			using (var xtw = new CustomXmlWriter(stringWriter))
			{
				xtw.WriteElementString("GDIObjectsCount", UIResources.Instance.GdiObjectsCount.ToString());
				xtw.WriteElementString("USERObjectsCount", UIResources.Instance.UserObjectsCount.ToString());
				xtw.WriteElementString("UserWindowHandlesCount", UIResources.Instance.UserWindowHandlesCount.ToString());
			}
			return stringWriter.GetStringBuilder().ToString();
			#endif
		}

		public string GetSystemResourcesUsageText()
		{
			#if WINZOR
			return string.Empty;
			#else
			return $@"GDIObjectsCount: {UIResources.Instance.GdiObjectsCount}
USERObjectsCount: {UIResources.Instance.UserObjectsCount}
UserWindowHandlesCount: {UIResources.Instance.UserWindowHandlesCount}
";
			#endif
		}
	}
}
