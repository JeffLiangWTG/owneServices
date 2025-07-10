
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class CusEntryHeaderAddInfo : AUAddInfo
	{
		public CusEntryHeaderAddInfo(CusEntryHeader parent, ZPropertyInfo addInfoProperty)
			: base(parent, addInfoProperty)
		{
		}

		protected override AUAddInfoValidation GetNewValidation()
		{
			return new CusEntryHeaderAddInfoValidation(this);
		}

		public new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		internal CusEntryHeader EntryHeader
		{
			get { return Parent; }
		}
	}
}
