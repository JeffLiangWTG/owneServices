using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusClassificationLookups : Customs.Business.CusClassificationLookups
	{
		public CusClassificationLookups(CusClassification parent)
			: base(parent)
		{
			CargoWise.Common.Argument.NotNull(parent, "parent");
		}

		public CusClassification Classification
		{
			get { return Parent; }
		}

		public BusinessObjectCollection Tariffs
		{
			get { return Parent.IsHTS ? new CACClassCollection(Factory) : new CACExportTariffCollection(Factory); }
		}

		protected new CusClassification Parent
		{
			get { return (CusClassification)base.Parent; }
		}

		public CACusRulingFindBoxCollection AuthorityNumberList
		{
			get
			{
				return Factory.GetCachedValue(ZString.Format("CusClassificationLookups_{0}", Parent.CCA_AuthorityNumber), () => new CACusRulingFindBoxCollection(Factory, Parent.CCA_AuthorityNumber));
			}
		}

		public OrgHeaderCollection Manufacturers
		{
			get { return new OrgHeaderCollection(Factory); }
		}
	}
}
