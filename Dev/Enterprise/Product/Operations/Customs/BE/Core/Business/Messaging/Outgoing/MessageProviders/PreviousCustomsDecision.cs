using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BE.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used In SADTransit")]
public class PreviousCustomsDecision : ITPreviousCustomsDecision
{
	public PreviousCustomsDecision(CusDV1Detail dv1Detail)
	{
		Argument.NotNull(dv1Detail, CusDV1Detail.Schema.TableName);
		// load the last DV1 form which has its decision made
		this.dv1Detail = dv1Detail.Factory.LoadTop1<CusDV1Detail>(new ZQuery(CusDV1DetailSchema.DV1_JE, dv1Detail.DV1_JE)
		{
			OrderBy = string.Join(" ", new[] { CusDV1Detail.Schema.DV1_CustomsDecisionDate, "DESC" }),
		});
	}

	readonly CusDV1Detail dv1Detail;

	public bool HasPreviousDecision { get => dv1Detail != null; }
	public DateTime PrevCustDecisionDate { get => dv1Detail.DV1_CustomsDecisionDate.ToDateTime(); }
	public ZBool PrevCustDecisionDateSpecified { get => dv1Detail.DV1_CustomsDecisionDate.IsValid && !dv1Detail.DV1_CustomsDecisionDate.IsEmpty; }
	public ZString PrevCustDecisionNumber { get => dv1Detail.DV1_CustomsDecisionNumber; }
}
