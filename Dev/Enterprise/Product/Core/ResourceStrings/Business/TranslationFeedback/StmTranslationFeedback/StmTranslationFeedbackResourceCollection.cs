using CargoWise.EntityFramework;

namespace Enterprise.ResourceStrings.Business
{
	public class StmTranslationFeedbackResourceCollection : DependentBusinessObjectCollection<StmTranslationFeedbackResource, StmTranslationFeedback>
	{
		public StmTranslationFeedbackResourceCollection(StmTranslationFeedback master)
			: base(master)
		{ }

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}
	}
}
