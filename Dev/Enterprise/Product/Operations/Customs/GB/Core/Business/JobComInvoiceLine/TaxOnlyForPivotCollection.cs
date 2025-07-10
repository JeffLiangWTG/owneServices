using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.GB.Business
{
	public class TaxOnlyForPivotCollection : CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
		, Integration.Customs.GB.ITaxOnlyForPivotCollection
	{
		public TaxOnlyForPivotCollection(BusinessObject master) : base(master)
		{
		}

		public new GBTaxOnlyForPivot this[int index] => (GBTaxOnlyForPivot)Elements[index];

		public new GBTaxOnlyForPivot AddNew()
		{
			return (GBTaxOnlyForPivot)base.AddNew();
		}

		protected new GBTaxOnlyForPivot AddNew(Type type)
		{
			return (GBTaxOnlyForPivot)base.AddNew(type);
		}

		protected override BusinessObject AddNewCore()
		{
			return AddNew(typeof(GBTaxOnlyForPivot));
		}

		#region ITaxOnlyForPivotCollection Members

		Integration.Customs.GB.IGBTaxOnlyForPivot Integration.Customs.GB.ITaxOnlyForPivotCollection.this[int index] => this[index];

		Integration.Customs.GB.IGBTaxOnlyForPivot Integration.Customs.GB.ITaxOnlyForPivotCollection.AddNew()
		{
			return AddNew();
		}

		#endregion
	}
}
