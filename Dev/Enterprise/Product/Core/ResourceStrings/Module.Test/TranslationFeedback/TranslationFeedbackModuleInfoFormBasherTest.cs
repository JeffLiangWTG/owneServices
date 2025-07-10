using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module.TranslationFeedback.Testing
{
	[TestedType(typeof(TranslationFeedbackModuleInfoForm))]
	internal sealed class TranslationFeedbackModuleInfoFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new TranslationFeedbackModuleInfoForm();
		}

		#endregion
	}
}
