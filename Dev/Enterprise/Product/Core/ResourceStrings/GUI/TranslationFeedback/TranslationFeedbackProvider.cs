using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public class TranslationFeedbackProvider : ITranslationFeedbackProvider
	{
		public void Feedback(Control control, MultilingualString multilingualString)
		{
			ShowForm(control, TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Res.CurrentLanguage, multilingualString));
		}

		public void Feedback(Control control, ResourceStringData resourceStringData)
		{
			ShowForm(control, TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Res.CurrentLanguage, resourceStringData));
		}

		public void Feedback(Control control, ResourceStringData resourceStringData, string renderedCaption)
		{
			ShowForm(control, TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Res.CurrentLanguage, resourceStringData, renderedCaption));
		}

		public void Feedback(Control control, ResourceStringData resourceStringData, string renderedCaption, bool hasAccelerators)
		{
			ShowForm(control, TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Res.CurrentLanguage, resourceStringData, renderedCaption, hasAccelerators));
		}

		public void Feedback(Control control, ICodeDescription codeDescription)
		{
			ShowForm(control, TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Res.CurrentLanguage, codeDescription));
		}

		public void Feedback(Control control, string caption)
		{
			ShowForm(control, TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Res.CurrentLanguage, caption));
		}

		public void Feedback(string language, string caption, IEnumerable<string> keys)
		{
			ShowForm(null, TranslationFeedbackFactory.GetForWeb(new BusinessObjectFactory(), language, caption, keys));
		}

		void ShowForm(Control control, TopLevelTranslationFeedbackCollection matches)
		{
			string unsupportedModule;
			if (!matches.ContainsExactMatch() && control != null && (unsupportedModule = UnsupportedModule(control)) != null)
			{
				Globals.Message.Show(string.Format("Translation of the {0} Module is not supported.", unsupportedModule), "Translation Feedback", MessageBoxButtons.OK, DialogResult.OK);
			}
			else if (matches.Count > 0)
			{
#if DEBUG
				if (matches[0].XT_Language == Res.DefaultLanguage && matches.Count == 1 && !string.IsNullOrEmpty(matches[0].ResourceStringKey))
				{
					ShowResourceStringForm(matches[0]);
				}
				else
#endif
				{
					if (!TranslationFeedbackConfiguration.IsMasterDatabase)
					{
						if (control != null)
						{
							var controlForm = control.FindForm();
							if (controlForm != null)
							{
								var screenShot = new ScreenShotStreamSource(ZScreenShotGrabber.CaptureWindow(controlForm.Handle));
								foreach (StmTranslationFeedback item in matches)
								{
									using (item.SuspendSettingHasChanges())
									{
										item.SetXT_ScreenshotSource(screenShot);
									}
								}
							}
						}
					}
					var form = GetOpenedTranslationForm(matches) ?? new TranslationFeedbackCreateForm(matches, control);
					form.Show();
					ApplicationDispatcher.Current.BeginInvoke(new Action(form.Activate)); // Call Activate() after this event handler to ensure the form is displayed in front
				}
			}
			else
			{
				Globals.Message.Show("No matching resource strings found.", "Translation Feedback", MessageBoxButtons.OK, DialogResult.OK);
			}
		}

		static TranslationFeedbackCreateForm GetOpenedTranslationForm(TopLevelTranslationFeedbackCollection matches)
		{
			foreach (var openForm in CargoWise.Windows.UI.ZApplication.GetOpenForms())
			{
				var translationForm = openForm as TranslationFeedbackCreateForm;
				if (translationForm != null && CompareMatches((TopLevelTranslationFeedbackCollection)translationForm.BusinessEntity, matches))
				{
					return translationForm;
				}
			}

			return null;
		}

		static bool CompareMatches(TopLevelTranslationFeedbackCollection lhs, TopLevelTranslationFeedbackCollection rhs)
		{
			if (lhs.Count != rhs.Count)
			{
				return false;
			}

			for (var i = 0; i < lhs.Count; ++i)
			{
				if ((lhs[i].ResourceStringKey != rhs[i].ResourceStringKey) || (lhs[i].XT_Language != rhs[i].XT_Language))
				{
					return false;
				}
			}

			return true;
		}

		class ScreenShotStreamSource : IStreamSource
		{
			public ScreenShotStreamSource(Bitmap bitmap)
			{
				this.bitmap = bitmap;
			}

			public Stream GetStream()
			{
				var stream = new MemoryStream();
				this.bitmap.Save(stream, ImageFormat.Png);
				stream.Position = 0;
				return stream;
			}

			readonly Bitmap bitmap;
		}

		string UnsupportedModule(Control control)
		{
			TranslationFileModuleMapping.Node mappingNode = TranslationFileModuleMapping.Instance.Lookup(control.GetType().Namespace);
			while (mappingNode.ModuleType != TranslationFileModuleMapping.ModuleTypes.DoNotTranslate && control.Parent != null)
			{
				control = control.Parent;
				mappingNode = TranslationFileModuleMapping.Instance.Lookup(control.GetType().Namespace);
			}
			return mappingNode.ModuleType != TranslationFileModuleMapping.ModuleTypes.DoNotTranslate ? null : mappingNode.ModuleName;
		}

#if DEBUG

		internal void ShowResourceStringForm(StmTranslationFeedback feedback)
		{
			var helpDataString = ResourceStringsFactory.Lookup(Res.CurrentLanguage, feedback.ResourceStringKey);
			if (helpDataString == null)
			{
				helpDataString = new HelpDataString();
				helpDataString.HD_Language = Res.CurrentLanguage;
				helpDataString.HD_Code = feedback.ResourceStringKey;
				helpDataString.HD_Caption = feedback.XT_OriginalTranslation;
			}

			var form = GetOpenedHelpDataStringForm(helpDataString) ?? new HelpDataStringForm(helpDataString);
			form.Show();
		}

		HelpDataStringForm GetOpenedHelpDataStringForm(HelpDataString helpDataString)
		{
			foreach (var openForm in CargoWise.Windows.UI.ZApplication.GetOpenForms())
			{
				var helpDataStringForm = openForm as HelpDataStringForm;
				if (helpDataStringForm != null)
				{
					var lhs = (HelpDataString)helpDataStringForm.BusinessEntity;
					if ((lhs.HD_Language == helpDataString.HD_Language) && (lhs.HD_Code == helpDataString.HD_Code) && (lhs.HD_Caption == helpDataString.HD_Caption))
					{
						return helpDataStringForm;
					}
				}
			}

			return null;
		}
#endif

		public void OpenTranslationSearchForm()
		{
			TranslationSearchForm.Open();
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Customs.AU
{
	class TestForm : ZForm
	{
		public TestForm()
		{
			label1 = new ZLabel();
			label1.Text = "One";
			label2 = new ZLabel();
			label2.Text = "Two";

			this.Controls.Add(label1);
			this.Controls.Add(label2);
		}

		public ZLabel label1;
		public ZLabel label2;
	}
}

namespace Enterprise.Freight.Forwarding
{
	class TestForm : ZForm
	{
		public TestForm()
		{
			label1 = new ZLabel();
			label1.Text = "One";
			label2 = new ZLabel();
			label2.Text = "Two";

			this.Controls.Add(label1);
			this.Controls.Add(label2);
		}

		public ZLabel label1;
		public ZLabel label2;
	}
}

#endif
#endregion
