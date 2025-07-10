using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportAmendItemWrapper : NonPersistentBusinessObject
	{
		public LocalExportAmendItemWrapper(ILocalExportAmendItem amendItem, BusinessObjectFactory factory)
			: base(factory)
		{
			this.amendItem = amendItem;
		}
		readonly ILocalExportAmendItem amendItem;
		[ResourceStringData("FE64530C-4975-4761-B192-A8D523C5A259", Caption = "Data Item ID")]
		public ZString DataItemNo => amendItem.DataItemNo;
		[ResourceStringData("56359AAE-F27E-4292-9ED3-E1F1D07BC4AD", Caption = "Amended Field")]
		public ZString DataItemDescription => Factory.GetCachedValue<LocalExportAmendmentDataItemIDList>().GetDescriptionFromCode(amendItem.DataItemNo) ?? ZString.Empty;
		public ZInt ItemSequenceNumber => amendItem.ItemSequenceNumber;
		public ZString AmendType => amendItem.AmendType;
		public ZString AmendTypeDescription => Factory.GetCachedValue<LocalExportAmendmentTypeCodeList>().GetDescriptionFromCode(amendItem.AmendType) ?? ZString.Empty;
		[ResourceStringData("0E5D5D68-0CCC-4D97-9EED-CF751614F1A5", Caption = "Type")]
		public ZString AmendTypeDescriptionForEntry => LocalExportAmendmentTypeCodeList.GetLocalExportAmendTypeForEntry(amendItem.AmendType);
		[ResourceStringData("FDEAF383-1CD2-4E3E-B735-77A6A9BADB0D", Caption = "Previous Value")]
		public ZString BeforeValue => amendItem.BeforeValue;
		[ResourceStringData("4AF3EF68-746B-42B0-9AD0-1242AEF8614A", Caption = "Amended Value")]
		public ZString AfterValue => amendItem.AfterValue;
		[ResourceStringData("762BCD68-BE52-4E13-93F2-857C287275E8", Caption = "Changed Item")]
		public ZString ID => IDsInList.GetID();

		IEnumerable<IDInList> IDsInList
		{
			get
			{
				if (ids == null)
				{
					ids = new List<IDInList>();
					var description = LocalExportAmendmentDataItemIDList.GetDataItemIDDescription(DataItemNo);
					ids.Add(new IDInList() { IDType = description, IDValue = amendItem.ItemSequenceNumber.ToString() });
				}
				return ids;
			}
		}
		List<IDInList> ids;
	}
}
