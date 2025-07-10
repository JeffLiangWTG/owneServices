using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class MergingRule : CusCodeData
	{
		public MergingRule(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = Constants.CusCodeDataTypes.Codes.MergingRule;
		}
	}
}
