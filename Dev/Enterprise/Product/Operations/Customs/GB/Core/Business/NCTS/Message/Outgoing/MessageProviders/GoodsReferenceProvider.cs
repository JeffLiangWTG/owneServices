using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class GoodsReferenceProvider : IGoodsReference
	{
		public GoodsReferenceProvider(NctsContainerItem cusCodeData, int sequence)
		{
			SequenceNumber = sequence;
			this.cusCodeData = Argument.NotNull(cusCodeData, nameof(cusCodeData));
		}

		public GoodsReferenceProvider(NctsCommonCargoDesc goodItem, int sequence)
		{
			SequenceNumber = sequence;
			this.goodItem = Argument.NotNull(goodItem, nameof(goodItem));
		}

		public int SequenceNumber { get; }

		public int DeclarationGoodsItemNumber => cusCodeData != null ? cusCodeData.CY_DataNumeric : goodItem.BY_DeclarationGoodsItemNumber;

		readonly NctsContainerItem cusCodeData;
		readonly NctsCommonCargoDesc goodItem;
	}
}
