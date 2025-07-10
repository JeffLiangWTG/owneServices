using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used In SADTransit")]
public class RestrictionsAndConditions : ITRestrictionsAndConditions
{
	public RestrictionsAndConditions(CusDV1Detail dv1Detail)
	{
		Argument.NotNull(dv1Detail, CusDV1Detail.Schema.TableName);
		this.dv1Detail = dv1Detail;
	}

	readonly CusDV1Detail dv1Detail;

	public ZString Consideration { get => dv1Detail.DV1_Consideration; }
	public ZString RestrictCondDetails { get => dv1Detail.DV1_RestrictionConsiderationDetails; }
	public ZString Restrictions { get => dv1Detail.DV1_Restrictions; }
}
