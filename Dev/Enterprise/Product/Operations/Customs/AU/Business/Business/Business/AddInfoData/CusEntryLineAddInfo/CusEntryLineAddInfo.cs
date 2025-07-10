
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class CusEntryLineAddInfo : AUAddInfo
	{
		public CusEntryLineAddInfo(CusEntryLine parent, ZPropertyInfo addInfoProperty)
			: base(parent, addInfoProperty)
		{
		}

		protected override AUAddInfoValidation GetNewValidation()
		{
			return new CusEntryLineAddInfoValidation(this);
		}

		public new CusEntryLine Parent
		{
			get { return (CusEntryLine)base.Parent; }
		}

		internal CusEntryLine EntryLine
		{
			get { return Parent; }
		}
	}
}
