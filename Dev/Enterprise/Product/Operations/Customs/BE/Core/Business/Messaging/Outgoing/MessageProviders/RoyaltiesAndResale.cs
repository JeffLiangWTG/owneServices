using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used In SADTransit")]
public class RoyaltiesAndResale : ITRoyaltiesAndResale
{
	public RoyaltiesAndResale(CusDV1Detail dv1Detail)
	{
		Argument.NotNull(dv1Detail, CusDV1Detail.Schema.TableName);
		this.dv1Detail = dv1Detail;
	}

	readonly CusDV1Detail dv1Detail;

	//TODO cannot find where populated from
	public ZString Conditions { get => ZString.Empty; }
	public ZString Resale { get => dv1Detail.DV1_Resale; }
	public ZString Royalties { get => dv1Detail.DV1_RoyaltiesLicence; }
}
