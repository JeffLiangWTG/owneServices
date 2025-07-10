using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module.TranslationFeedback.Testing
{
	[TestedType(typeof(TranslationFeedbackController))]
	internal class TranslationFeedbackControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.TranslationFeedback;
		}

		public override void TestNewForm()
		{
			Assert(true);
		}

		public override void TestDeleteForm()
		{
			AssertNull("DeleteForm should be null", Controller.ShowDeleteForm(GetBusinessObjectThatIsInTheDatabase()));
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var feedback = Factory.New<StmTranslationFeedback>();
			feedback.XT_Language = Core.SharedConstants.Languages.ChineseTraditional;
			Factory.Save();
			return feedback;
		}
	}
}
