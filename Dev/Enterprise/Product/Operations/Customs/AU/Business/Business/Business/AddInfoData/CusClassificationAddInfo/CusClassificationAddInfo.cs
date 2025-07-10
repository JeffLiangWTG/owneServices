
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[TestedAsNonPersistentBusinessObject]
	public class CusClassificationAddInfo : AUAddInfo
	{
		public CusClassificationAddInfo(Classification parent, ZPropertyInfo addInfoProperty)
			: base(parent, addInfoProperty)
		{
		}

		protected override AUAddInfoValidation GetNewValidation()
		{
			return new CusClassificationAddInfoValidation(this);
		}
	}
}
