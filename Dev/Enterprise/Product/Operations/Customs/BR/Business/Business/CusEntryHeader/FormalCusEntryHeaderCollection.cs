using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class FormalCusEntryHeaderCollection : BusinessObjectCollectionView<CusEntryHeader>
	{
		public FormalCusEntryHeaderCollection(JobDeclaration declaration) : base(declaration.CustomsEntryHeaders as BusinessObjectCollection)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return element is CusEntryHeader header && header.IsFormalEntry;
		}
	}
}
