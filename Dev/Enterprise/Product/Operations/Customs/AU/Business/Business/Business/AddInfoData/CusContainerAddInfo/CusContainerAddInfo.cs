
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class CusContainerAddInfo : AUAddInfo
	{
		public CusContainerAddInfo(CusContainer parent, ZPropertyInfo addInfoProperty)
			: base(parent, addInfoProperty)
		{
		}

		public new CusContainer Parent
		{
			get { return base.Parent as CusContainer; }
		}

		protected override AUAddInfoValidation GetNewValidation()
		{
			return new CusContainerAddInfoValidation(this);
		}
	}
}
