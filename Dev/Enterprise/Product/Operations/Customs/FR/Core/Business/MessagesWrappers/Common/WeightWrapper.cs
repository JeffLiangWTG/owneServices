using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class WeightWrapper : IWeight
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public WeightWrapper(SupportingDocument supportingDocument)
		{
			this.supportingDocumentItem = Argument.NotNull(supportingDocument, "SupportingDocument cannot be null ");
		}
		public ZDecimal Weight => supportingDocumentItem?.CSI_Quantity2.Round(6) ?? ZDecimal.Zero;
		public ZString Unit => supportingDocumentItem?.CSI_UnitOfQuantity2 ?? ZString.Empty;

		readonly SupportingDocument supportingDocumentItem;
	}
}
