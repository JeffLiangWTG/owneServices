using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportEntryLineWrapper : NonPersistentBusinessObject
	{
		public LocalExportEntryLineWrapper(ILocalExportEntryLine entryLine, BusinessObjectFactory factory)
		{
			EntryLine = entryLine;
			PopulatePreviousTransactionReferenceNoTypeName(factory);
		}
		public ILocalExportEntryLine EntryLine { get; }
		public ZString FormattedHSCode => MessageFunctions.HSCodeFormat(EntryLine.HSCode);
		public ZString PreviousTransactionReferenceNoTypeName { get; set; }
		public ZString PreviousTransactionReferenceWrapperString
		{
			get
			{
				var numberType = EntryLine.PreviousTransactionReferenceNoType;
				if (!numberType.IsEmpty && !PreviousTransactionReferenceNoTypeName.IsEmpty)
				{
					numberType = $"({numberType}:{PreviousTransactionReferenceNoTypeName})";
				}
				var number = EntryLine.PreviousTransactionReferenceNo;
				if (!number.IsEmpty && !numberType.IsEmpty)
				{
					number += " ";
				}
				return number + numberType;
			}
		}

		void PopulatePreviousTransactionReferenceNoTypeName(BusinessObjectFactory factory)
		{
			PreviousTransactionReferenceNoTypeName = factory.GetCachedValue<OriginalStateDocTypeList>().GetDescriptionFromCode(EntryLine.PreviousTransactionReferenceNoType);
		}

		public ZString FormattedEntryLineNo
		{
			get
			{
				ZString result = ZString.Empty;
				if (!EntryLine.EntryLineNo.IsEmpty)
				{
					result = EntryLine.EntryLineNo.ToString("D3");
				}
				return result;
			}
		}

		public ZString DecoratedGoodsNo => EntryLine.GoodsNo.IsEmpty ? "" : $"\n({EntryLine.GoodsNo})";
	}
}
