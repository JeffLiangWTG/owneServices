using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Export5ASItemWrapper : NonPersistentBusinessObject
	{
		public Export5ASItemWrapper(Export5ASItem amendmentItem, BusinessObjectFactory factory) : base(factory)
		{
			AmendmentItem = amendmentItem;
		}
		public IExport5ASItem AmendmentItem { get; }

		public ZString EntryLineNoPadded => AmendmentItem.EntryLineNo;
		[ResourceStringData("FD4E04DF-175C-4A0C-9528-832EA62E22E7", Caption = "Type")]
		public ZString AmendTypeDescription => AmendTypeCodeList.GetAmendTypeDescription(new ZInt(AmendmentItem.EntryLineNo) == ZInt.Zero ? AmendTypeCodeList.Codes._03 : AmendmentItem.LineAmendType);
		[ResourceStringData("6B3E173A-A2F0-4440-B1F8-186F2AACAE8B", Caption = "Amended Field")]
		public ZString DataItemDescription => Factory.GetCachedValue<ExportAmendmentDataItemIDList>().GetDescriptionFromCode(AmendmentItem.AmendDataItemID) ?? ZString.Empty;
		public ZString LineDetailNoPadded => AmendmentItem.LineDetailNo;
		[ResourceStringData("402BA5E7-FC51-4C62-B1A0-F16E06FF9058", Caption = "Data Item ID")]
		public ZString DataItemNo => AmendmentItem.AmendDataItemID;
		[ResourceStringData("0417368A-EA21-4FD1-AF4B-3B93347EE04E", Caption = "Previous Value")]
		public ZString BeforeValue => AmendmentItem.BeforeDescription;
		[ResourceStringData("7880B6CC-C5A3-4AEA-A1B2-9F664BD53EBF", Caption = "Amended Value")]
		public ZString AfterValue => AmendmentItem.AfterDescription;
		[ResourceStringData("AC6F0670-E0B4-4C8A-B35D-AE4A225F920E", Caption = "Changed Item")]
		public ZString ID
		{
			get
			{
				if (!id.HasValue)
				{
					id = IDsInList.GetID();
				}
				return id.Value;
			}
		}
		ZString? id;

		public IEnumerable<IDInList> IDsInList
		{
			get
			{
				var result = new List<IDInList>();
				if (ExportAmendmentDataItemIDList.IsHeaderDataItem(AmendmentItem.AmendDataItemID))
				{
					if (OrganisationDataItemIDProvider.IsOrganisationDataItemID(ElectronicDocumentTypeList.Codes._830, AmendmentItem.AmendDataItemID))
					{
						result.Add(new IDInList() { IDType = nameof(IOrganization), IDValue = "" });
					}
					else
					{
						result.Add(new IDInList() { IDType = nameof(IExportEntryHeader), IDValue = "" });
					}
				}
				else
				{
					if (IsNotZero(AmendmentItem.EntryLineNo))
					{
						result.Add(new IDInList() { IDType = nameof(IExportEntryLine), IDValue = AmendmentItem.EntryLineNo });
					}
					if (IsNotZero(AmendmentItem.LineDetailNo))
					{
						result.Add(new IDInList() { IDType = nameof(IExportInvoiceLine), IDValue = AmendmentItem.LineDetailNo });
					}
					if (IsNotZero(AmendmentItem.ContainerSequenceNo))
					{
						result.Add(new IDInList() { IDType = nameof(IExportContainer), IDValue = AmendmentItem.ContainerSequenceNo });
					}
					if (IsNotZero(AmendmentItem.VINSequenceNo))
					{
						result.Add(new IDInList() { IDType = nameof(IExportVehicleNo), IDValue = AmendmentItem.VINSequenceNo });
					}
					if (IsNotZero(AmendmentItem.RegulationCategorySequnceNo))
					{
						result.Add(new IDInList() { IDType = nameof(IExportGAApprovalDocument), IDValue = AmendmentItem.RegulationCategorySequnceNo });
					}
				}
				return result;
			}
		}

		static bool IsNotZero(ZString stringNo)
		{
			ZDecimal result;
			ZDecimal.TryParse(stringNo, out result);
			return result != ZDecimal.Zero;
		}
	}
}
