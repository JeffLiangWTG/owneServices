using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.DataTransfer
{
	public class CATCPLineCollection : NonPersistentBusinessObjectCollection<CATCPLine>
	{
		public CATCPLineCollection()
			: base()
		{
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return false;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CATCPLine();
		}
	}
}
