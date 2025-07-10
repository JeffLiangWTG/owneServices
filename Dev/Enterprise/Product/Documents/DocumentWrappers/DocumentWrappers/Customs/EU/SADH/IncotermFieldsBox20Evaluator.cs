using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.DocumentWrappers.Customs.EU
{
	public interface IIncotermFieldsBox20Evaluator
	{
		IncoTermFields Evaluate();
	}

	public class IncoTermFields
	{
		public IncoTermFields(ZString shipmentIncoTerm, ZString incoTermPlace, ZString agreedPlaceCode, ZString agreedPlaceCode2)
		{
			ShipmentIncoTerm = shipmentIncoTerm;
			IncoTermPlace = incoTermPlace;
			AgreedPlaceCode = agreedPlaceCode;
			AgreedPlaceCode2 = agreedPlaceCode2;
		}

		public ZString ShipmentIncoTerm { get; }
		public ZString IncoTermPlace { get; }
		public ZString AgreedPlaceCode { get; }
		public ZString AgreedPlaceCode2 { get; }
	}

	class IncotermFieldsBox20Evaluator : IIncotermFieldsBox20Evaluator
	{
		public IncotermFieldsBox20Evaluator(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}

		readonly CusEntryHeader entryHeader;

		public IncoTermFields Evaluate()
		{
			var invoiceHeader = entryHeader.RandomHeader;

			ZString shipmentIncoTerm = ZString.Empty;
			ZString incoTermPlace = ZString.Empty;
			ZString agreedPlaceCode = ZString.Empty;
			ZString agreedPlaceCode2 = ZString.Empty;

			if (invoiceHeader != null)
			{
				shipmentIncoTerm = !invoiceHeader.JZ_IncoTerm.IsEmpty ? invoiceHeader.JZ_IncoTerm : entryHeader.Declaration.JE_ShipmentIncoTerm;
				incoTermPlace = !invoiceHeader.JZ_IncoTermPlace.IsEmpty ? invoiceHeader.JZ_IncoTermPlace : entryHeader.Declaration.JE_ShipmentIncoTermPlace;
				agreedPlaceCode2 = !invoiceHeader.ZG_AgreedPlaceCode.IsEmpty ? invoiceHeader.ZG_AgreedPlaceCode : entryHeader.Declaration.ZG_AgreedPlaceCode;
			}

			return new IncoTermFields(shipmentIncoTerm, incoTermPlace, agreedPlaceCode, agreedPlaceCode2);
		}
	}
}
