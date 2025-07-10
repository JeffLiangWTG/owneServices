using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used In SADTransit")]
public class BuyerSellerRelation : ITBuyerSellerRelation
{
	public BuyerSellerRelation(CusDV1Detail dv1Detail)
	{
		Argument.NotNull(dv1Detail, CusDV1Detail.Schema.TableName);
		this.dv1Detail = dv1Detail;
	}

	readonly CusDV1Detail dv1Detail;

	public ZString CloseApproximate { get => dv1Detail.DV1_CloseApproximation; }
	public ZString CloseApprxDetails { get => dv1Detail.DV1_RelationDetails; }
	public ZString Influence { get => dv1Detail.DV1_PriceInfluence; }
	public ZString Relation { get => dv1Detail.DV1_Relationship; }
}
