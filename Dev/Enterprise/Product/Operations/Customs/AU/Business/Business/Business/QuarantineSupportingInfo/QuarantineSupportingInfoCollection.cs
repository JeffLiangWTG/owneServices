using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineSupportingInfoCollection : CusSupportingInfoCollection<QuarantineSupportingInfo>
	{
		public QuarantineSupportingInfoCollection(BusinessObject parent)
			: base(parent, DeclarationConstant)
		{
		}

		public QuarantineExDocHeader Parent => (QuarantineExDocHeader)Master;

		public const string DeclarationConstant = "DEC";

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, CusSupportingInfoSchema.CSI_Code, SQLComparisonOperator.Equal, DeclarationConstant);
			return result;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var supportingInfo = (QuarantineSupportingInfo)child;
			supportingInfo.CSI_Code = DeclarationConstant;
		}
	}
}
