using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class ExpressionPlaceholderCollection : NonPersistentBusinessObjectCollection<ExpressionPlaceholder>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ExpressionPlaceholder(0);
		}

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
