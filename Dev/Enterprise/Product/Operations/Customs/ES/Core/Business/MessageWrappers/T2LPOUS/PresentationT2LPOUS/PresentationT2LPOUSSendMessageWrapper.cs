using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class PresentationT2LPOUSSendMessageWrapper : T2LPOUSCommonSendMessageWrapper, IPresentationT2LMessageDataProvider
	{
		public PresentationT2LPOUSSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
		}

		public ZString PreviousMRN => entryHeader.MovementReferenceNumber;

		public ZString LocationOfGoods
		{
			get
			{
				var location = declaration.JE_LocationOfGoods;
				return location.Length > 10 ? location.SubstringSafe(4) : location;
			}
		}

		public IT2LPOUSCommonContainerIndicator ContainerIndication => containerIndication ?? (containerIndication = new T2LPOUSCommonContainerIndicatorWrapper(entryHeader.IsContainerised()));
		T2LPOUSCommonContainerIndicatorWrapper containerIndication;

		public IReadOnlyCollection<IT2LPOUSTransportEquipment> TransportEquipment => transportEquipment ?? (transportEquipment = T2LPOUSTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader));
		IReadOnlyCollection<T2LPOUSTransportEquipmentWrapper> transportEquipment;

		public IReadOnlyCollection<IT2LPOUSPresentationGoodItem> GoodItems => lines ?? (lines = entryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new T2LPOUSPresentationGoodItemWrapper(x)).ToList().AsReadOnly());
		ReadOnlyCollection<T2LPOUSPresentationGoodItemWrapper> lines;

		protected override OrgAddress OrgAddressForPersonReqPresCommon
		{
			get
			{
				var personReqPres = declaration.RepresentativeOrgAddress ?? declaration.DeclarantOrgAddress;
				return personReqPres ?? (declaration.ImporterDocumentaryAddress?.Address);
			}
		}
	}
}
