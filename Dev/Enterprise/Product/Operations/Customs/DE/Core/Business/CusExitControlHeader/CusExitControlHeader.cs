using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.DE.Business
{
	public class CusExitControlHeader : EU.Business.CusExitControlHeader, Integration.Customs.DE.ICusExitControlHeader
	{
		public CusExitControlHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("7C4DA2D8-A1D0-497A-B692-73087073A98C", Caption = "Loading Place")]
		public override ZString CEH_LocationOfGoods
		{
			get => base.CEH_LocationOfGoods;
			set => base.CEH_LocationOfGoods = value;
		}

		public new CusExitControlHeaderValidation Validation => (CusExitControlHeaderValidation)base.Validation;

		public new CusExitControlHeaderLookups Lookups => (CusExitControlHeaderLookups)base.Lookups;

		public new CusExitDetailCollection CusExitDetails => (CusExitDetailCollection)base.CusExitDetails;

		protected override EU.Business.CusExitControlHeaderValidation GetNewValidation() => new CusExitControlHeaderValidation(this);

		protected override EU.Business.CusExitControlHeaderLookups GetNewLookups() => new CusExitControlHeaderLookups(this);

		protected override EU.Business.CusExitDetailCollection GetNewCusExitDetailsCollectionCore() => new CusExitDetailCollection(this);
	}
}
