using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsCusGoodsLocationLookups : EU.NCTS.Business.CusGoodsLocationLookups
	{
		public NctsCusGoodsLocationLookups(NctsCusGoodsLocation parent) : base(parent)
		{
		}

		protected new NctsCusGoodsLocation Parent => (NctsCusGoodsLocation)base.Parent;

		public override CodeDescriptionPairList QualifierList
		{
			get
			{
				var parentIsMovementHeader = Parent.ParentIsMovementHeader;
				return Factory.GetCachedValue($"ES.NCTS.CusGoodsLocationLookups.QualifierList_{nameof(Parent.ParentIsMovementHeader)}_{parentIsMovementHeader}",
					() =>
					{
						var qualifierList = new CodeDescriptionPairList();
						if (parentIsMovementHeader)
						{
							qualifierList.AddPair(CusGoodsLocationQualifierList.Codes.AuthorizationNumber, CusGoodsLocationQualifierList.Descriptions.AuthorizationNumber);
						}
						else
						{
							qualifierList = base.QualifierList;
						}
						return qualifierList;
					});
			}
		}

		public ICollection AdditionalIdentifierList => LocationsHelper.GetESLocationsCusCodeList(Factory);
	}
}
