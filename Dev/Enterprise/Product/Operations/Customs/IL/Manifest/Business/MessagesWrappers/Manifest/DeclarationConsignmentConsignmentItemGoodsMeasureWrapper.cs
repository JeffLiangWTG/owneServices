using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignmentItemGoodsMeasureWrapper : IDeclarationConsignmentConsignmentItemGoodsMeasure
	{
		DeclarationConsignmentConsignmentItemGoodsMeasureWrapper(AsycudaPack asycudaPack)
		{
			this.asycudaPack = Argument.NotNull(asycudaPack, nameof(asycudaPack));
		}

		public static IDeclarationConsignmentConsignmentItemGoodsMeasure NewOrNull(AsycudaPack asycudaPack)
			=> asycudaPack == null ? null : new DeclarationConsignmentConsignmentItemGoodsMeasureWrapper(asycudaPack);

		public IMeasureType GrossMassMeasure => MeasureTypeWrapper.NewOrNull(asycudaPack.APA_Weight, ZString.Empty);

		readonly AsycudaPack asycudaPack;
	}
}
