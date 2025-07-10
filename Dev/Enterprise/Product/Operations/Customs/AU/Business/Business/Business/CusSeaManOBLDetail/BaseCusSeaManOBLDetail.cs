using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class BaseCusSeaManOBLDetail : Customs.Business.CusSeaManOBLDetail
	{
		public BaseCusSeaManOBLDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZBool BD_SACIndicator
		{
			get { return base.BD_SACIndicator; }
			set
			{
				bool hasChanged = base.BD_SACIndicator != value;
				base.BD_SACIndicator = value;
				if (hasChanged && Header != null && !IsCopying)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		protected override Customs.Business.CusSeaManOBLDetailLookups GetNewLookups()
		{
			return new BaseCusSeaManOBLDetailLookups(this);
		}
	}
}
