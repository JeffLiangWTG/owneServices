using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class GBGuaranteeCollection : EU.Business.Declaration.GuaranteeForDeclarationCollection
	{
		public GBGuaranteeCollection(JobDeclaration declaration)
				: base(declaration)
		{
		}

		public new JobDeclaration Master => (JobDeclaration)base.Master;

		public new GBGuarantee this[int index] => (GBGuarantee)Elements[index];

		protected override BusinessObject AddNewCore() => AddNew(typeof(GBGuarantee));

		public new GBGuarantee AddNew() => (GBGuarantee)base.AddNew();

		protected new GBGuarantee AddNew(Type type) => (GBGuarantee)base.AddNew(type);

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var guarantee = child as GBGuarantee;
			if (guarantee != null)
			{
				guarantee.PW_BondType = GuaranteeTypeList.Codes.Guarantee;
			}
		}
	}
}
