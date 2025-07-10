using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used In SADTransit")]
public class Contract : ITContract
{
	public Contract(CusDV1Detail dv1Detail)
	{
		Argument.NotNull(dv1Detail, CusDV1Detail.Schema.TableName);
		this.dv1Detail = dv1Detail;
	}

	readonly CusDV1Detail dv1Detail;

	public DateTime ContractDate { get => dv1Detail.DV1_ContractDate.ToDateTime(); }
	public ZString ContractNumber { get => dv1Detail.DV1_ContractNumber; }
}
