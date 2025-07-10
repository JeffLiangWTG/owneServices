using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public interface ITranslationFeedbackProvider
	{
		void Feedback(Control control, ResourceStringData resourceStringData, string renderedCaption, bool hasAccelerators);
		void Feedback(Control control, ResourceStringData resourceStringData, string renderedCaption);
		void Feedback(Control control, ResourceStringData resourceStringData);
		void Feedback(Control control, MultilingualString multilingualString);
		void Feedback(Control control, string caption);
		void Feedback(Control control, ICodeDescription codeDescription);
		void Feedback(string language, string caption, IEnumerable<string> keys);
		void OpenTranslationSearchForm();
	}
}

namespace Enterprise.ZArchitecture.GUI
{
	public class MockTranslationFeedbackProvider : ITranslationFeedbackProvider
	{
		public void Feedback(Control control, ResourceStringData resourceStringData, string renderedCaption, bool hasAccelerators)
		{
			Feedback(control, resourceStringData, renderedCaption);
		}

		public void Feedback(Control control, ResourceStringData resourceStringData, string renderedCaption)
		{
			LastFeedbackKey = resourceStringData.Key;
			LastFeedbackCaption = renderedCaption;
			LastFeedbackControl = control;
			LastFeedbackDescription = resourceStringData.FullDescription;
		}

		public void Feedback(Control control, ResourceStringData resourceStringData)
		{
			LastFeedbackKey = resourceStringData.Key;
			LastFeedbackCaption = resourceStringData.Caption;
			LastFeedbackControl = control;
			LastFeedbackDescription = resourceStringData.FullDescription;
		}

		public void Feedback(Control control, MultilingualString multilingualString)
		{
			LastFeedbackKey = multilingualString is ResourceString ? ((ResourceString)multilingualString).ResourceKey : null;
			LastFeedbackCaption = multilingualString;
			LastFeedbackControl = control;
		}

		public void Feedback(Control control, string caption)
		{
			LastFeedbackKey = null;
			LastFeedbackCaption = caption;
			LastFeedbackControl = control;
		}

		public void Feedback(Control control, ICodeDescription codeDescription)
		{
			LastFeedbackKey = codeDescription is IMultilingualDescription && ((IMultilingualDescription)codeDescription).MultilingualDescription is ResourceString ?
				((ResourceString)((IMultilingualDescription)codeDescription).MultilingualDescription).ResourceKey : null;
			LastFeedbackCaption = codeDescription.Description;
			LastFeedbackControl = control;
		}

		public void Feedback(string language, string caption, IEnumerable<string> keys)
		{
			LastFeedbackKey = null;
			LastFeedbackCaption = caption;
			LastFeedbackControl = null;
		}

		public void OpenTranslationSearchForm()
		{
			throw new System.NotImplementedException();
		}

		public string LastFeedbackKey;
		public string LastFeedbackCaption;
		public string LastFeedbackDescription;
		public Control LastFeedbackControl;
	}
}
