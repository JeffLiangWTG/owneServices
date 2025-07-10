using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CC044CConsignmentItemWrapper : ConsignmentItemWrapper
	{
		CC044CConsignmentItemWrapper(NctsCommonCargoDesc item) : base(item)
		{
		}

		public new static CC044CConsignmentItemWrapper New(NctsCommonCargoDesc item) => item == null ? null : new CC044CConsignmentItemWrapper(item);

		protected override string GetGoodsItemNumber() => item.Bill.MovementDetail.B9_SeqNo;

		public override ICommodity Commodity => commodity ?? (commodity = item.BY_UnloadedState == NctsUnloadedStateList.Codes.NEW || item.BY_UnloadedState == NctsUnloadedStateList.Codes.DIF
		? CC044CCommodityWrapper.New(UnloadedItemIfExists)
		: null);
		ICommodity commodity;

		protected override ICollection<IPackaging> GetPackagings()
		{
			var result = new Collection<IPackaging>();
			item.Packages.Cast<NctsPackage>().Where(p => p.B5_TypeOfDifference == NctsUnloadedStateList.Codes.NEW || p.B5_TypeOfDifference == NctsUnloadedStateList.Codes.MIS).ForEach(package => result.Add(CC044CPackagingWrapper.New(package)));
			item.Packages.Cast<NctsPackage>().Where(p => p.B5_TypeOfDifference == NctsUnloadedStateList.Codes.DIF).ForEach(package => result.Add(CC044CPackagingWrapper.New(package.PackDifference)));
			return result;
		}

		protected override ICollection<ISupportingDocument> GetSupportingDocuments()
		{
			var result = new Collection<ISupportingDocument>();
			item.SupportingDocuments.Cast<NctsSupportingDocument>().Where(s => s.CSI_Status == NctsUnloadedStateList.Codes.NEW || s.CSI_Status == NctsUnloadedStateList.Codes.MIS).ForEach(document => result.Add(CC044CSupportingDocumentWrapper.New(document)));
			return result;
		}

		protected override ICollection<IDocument> GetTransportDocuments()
		{
			var result = new Collection<IDocument>();
			item.AdditionalInfos.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument && (x.CSI_Status == NctsUnloadedStateList.Codes.NEW || x.CSI_Status == NctsUnloadedStateList.Codes.MIS)).ForEach(document => result.Add(CC044CDocumentWrapper.New(document)));
			return result;
		}

		protected override ICollection<IDocument> GetAdditionalReferences()
		{
			var result = new Collection<IDocument>();
			item.AdditionalInfos.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && (x.CSI_Status == NctsUnloadedStateList.Codes.NEW || x.CSI_Status == NctsUnloadedStateList.Codes.MIS)).ForEach(document => result.Add(CC044CDocumentWrapper.New(document)));
			return result;
		}

		NctsCommonCargoDesc UnloadedItemIfExists => unloadedItemIfExists ??= item.BY_BY_Commodity.IsEmpty ? item : NctsUnloadedCargoDesc.Load((NctsArrivalCargoDesc)item);
		NctsCommonCargoDesc unloadedItemIfExists;
	}
}
