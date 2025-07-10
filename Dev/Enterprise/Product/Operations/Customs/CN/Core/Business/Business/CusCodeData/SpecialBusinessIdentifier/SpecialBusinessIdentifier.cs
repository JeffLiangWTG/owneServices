using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class SpecialBusinessIdentifier : CusCodeData
	{
		public SpecialBusinessIdentifier(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(CusEntryInstruction));

		protected override CusCodeDataValidation GetNewValidation() => new SpecialBusinessIdentifierValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = Constants.CusCodeDataTypes.Codes.SpecialBusinessIdentifier;
		}
	}
}
