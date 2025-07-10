using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class GoodsReferenceProvider : IGoodsReference
	{
		readonly NctsContainerItem cusCodeData;
		readonly NctsCommonCargoDesc goodItem;

		public GoodsReferenceProvider(NctsContainerItem cusCodeData, int sequence)
		{
			SequenceNumber = sequence;
			this.cusCodeData = Argument.NotNull(cusCodeData, nameof(cusCodeData));
		}

		public GoodsReferenceProvider(NctsCommonCargoDesc goodItem, ZInt sequence)
		{
			SequenceNumber = sequence;
			this.goodItem = Argument.NotNull(goodItem, nameof(goodItem));
		}

		public int SequenceNumber { get; }

		public int DeclarationGoodsItemNumber => cusCodeData != null ? cusCodeData.CY_DataNumeric : goodItem.BY_DeclarationGoodsItemNumber;
	}
}
