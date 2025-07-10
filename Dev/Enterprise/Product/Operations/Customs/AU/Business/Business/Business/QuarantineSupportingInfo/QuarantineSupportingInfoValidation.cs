using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class QuarantineSupportingInfoValidation : CusSupportingInfoValidation
	{
		public QuarantineSupportingInfoValidation(QuarantineSupportingInfo parent)
		: base(parent)
		{
			this.parent = parent;
		}
		readonly QuarantineSupportingInfo parent;

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();

			if (!parent.CSI_Description.IsEmpty && IsCSIDescriptionDuplicated)
			{
				parent.CSI_DescriptionInfo.AddError(ResString.GetMultilingualString("1984A819-6D50-477D-8472-880E2F878543", "This Code already exists in this Job"));
			}
		}

		bool IsCSIDescriptionDuplicated
		{
			get
			{
				ZQuery filter = new ZQuery(CusSupportingInfoSchema.CSI_Description, parent.CSI_Description);
				filter.AddToFilter(CusSupportingInfoSchema.PK, SQLComparisonOperator.NotEqual, parent.PK);
				filter.AddToFilter(CusSupportingInfoSchema.CSI_ParentID, parent.CSI_ParentID);

				var duplicated = parent.Factory.LoadTop1<CusSupportingInfo>(filter);

				return (duplicated != null);
			}
		}
	}
}
