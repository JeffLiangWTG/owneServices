using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class IncompleteImportH1SendMessageWrapper : ImportH1CommonSendMessageWrapper, IIncompleteImportH1MessageDataProvider
{
	public IncompleteImportH1SendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate) : base(entryHeader, certificate)
	{
	}

	public IIncompleteImportH1ImportOperation ImportOperation => importOperation ??= new IncompleteImportH1ImportOperationWrapper(entryHeader);
	IncompleteImportH1ImportOperationWrapper importOperation;

	public ZString CountryOfDispatch => declaration.JE_GoodsOrigin;

	public IReadOnlyCollection<ICommonTransportEquipment> TransportEquipments => transportEquipment ??= transportEquipment = CommonTransportEquipmentWrapper.GetTransportEquipmentList(entryHeader);
	IReadOnlyCollection<CommonTransportEquipmentWrapper> transportEquipment;

	public IReadOnlyCollection<IIncompleteImportH1GoodsShipmentItem> GoodsShipmentItems => goodsShipmentItems ??= goodsShipmentItems = entryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new IncompleteImportH1LineWrapper(x)).ToList().AsReadOnly();
	IReadOnlyCollection<IncompleteImportH1LineWrapper> goodsShipmentItems;
}
