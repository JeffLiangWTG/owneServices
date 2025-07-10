using CargoWise.EntityFramework;

namespace Enterprise.ResourceStrings.Business
{
	public class StmTranslationFeedbackCollection : BusinessObjectCollection<StmTranslationFeedback>
	{
		public StmTranslationFeedbackCollection(BusinessObjectFactory factory)
			: base(factory)
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
