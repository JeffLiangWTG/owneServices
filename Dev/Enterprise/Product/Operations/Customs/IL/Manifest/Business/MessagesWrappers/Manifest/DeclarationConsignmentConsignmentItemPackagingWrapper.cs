using CargoWise.Common;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business.MessagesWrappers
{
	public class DeclarationConsignmentConsignmentItemPackagingWrapper : IDeclarationConsignmentConsignmentItemPackaging
	{
		DeclarationConsignmentConsignmentItemPackagingWrapper(AsycudaPack asycudaPack)
		{
			this.asycudaPack = Argument.NotNull(asycudaPack, nameof(asycudaPack));
		}

		public static IDeclarationConsignmentConsignmentItemPackaging NewOrNull(AsycudaPack item)
			=> item == null ? null : new DeclarationConsignmentConsignmentItemPackagingWrapper(item);

		public ITextType MarksNumbers => TextTypeWrapper.NewOrNull(asycudaPack.APA_MarksAndNumbers);

		public IQuantityType QuantityQuantity => QuantityTypeWrapper.NewOrNull((ZDecimal)asycudaPack.APA_PackQty);

		public ICodeType TypeCode => CodeTypeWrapper.NewOrNull(asycudaPack.APA_PackUQ);

		readonly AsycudaPack asycudaPack;
	}
}
