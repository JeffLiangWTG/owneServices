using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used In SADTransit")]
public class Seal : ITSeal
{
	public Seal(BaseCusContainer container)
	{
		Argument.NotNull(container, BaseCusContainer.Schema.TableName);
		this.container = container;
	}

	readonly BaseCusContainer container;

	// TODO sealaffixed unsure
	public ZString SealAffixed { get => container.JobContainer.JC_SealNum; }
	public ZString Sealingparty { get => container.JobContainer.JC_SealParty; }
}
