using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using static Enterprise.Customs.DE.Business.BondedWarehousingHelper.Constants;

namespace Enterprise.Customs.DE.DataTransfer.Universal
{
	public class WarehouseCustomsLineAddInfoSupportingInfo : IWarehouseCustomsLineAddInfo
	{
		public WarehouseCustomsLineAddInfoSupportingInfo(CustomsSupportingInformation supportingInfo)
		{
			this.supportingInfo = Argument.NotNull(supportingInfo, nameof(supportingInfo));
		}

		readonly CustomsSupportingInformation supportingInfo;

		public ZString Type => InvoiceSupportingDocumentAddInfoTypes.InvoiceLine;

		public ZString AddInfoData
		{
			get
			{
				if (!addInfoData.HasValue)
				{
					addInfoData = CreateAddInfoData();
				}
				return addInfoData.Value;
			}
		}
		ZString? addInfoData;

		ZString CreateAddInfoData()
		{
			var dictionary = new Dictionary<ZString, ZString>
			{
				{ WarehouseCustomsLineAddInfoSupportingInfoKeys.Type, supportingInfo.Type.GetCodeAsUpperCase() },
				{ WarehouseCustomsLineAddInfoSupportingInfoKeys.Reference, Customs.Business.BaseAddInfo.GetStringRepresentation(supportingInfo.ReferenceNumber.GetValueOrDefault()) },
				{ WarehouseCustomsLineAddInfoSupportingInfoKeys.DateOfIssue, Customs.Business.BaseAddInfo.GetStringRepresentation(supportingInfo.DateOfIssue.GetValueOrDefault()) },
				{ WarehouseCustomsLineAddInfoSupportingInfoKeys.Available, supportingInfo.Status.GetCodeAsUpperCase() },
				{ WarehouseCustomsLineAddInfoSupportingInfoKeys.Quantity, Customs.Business.BaseAddInfo.GetStringRepresentation(supportingInfo.Quantity.GetValueOrDefault()) },
				{ WarehouseCustomsLineAddInfoSupportingInfoKeys.UnitofMeasure, supportingInfo.UnitOfQuantity.GetCodeAsUpperCase() }
			};
			return AddInfoParser.Serialise(dictionary);
		}

		public ZString NAddInfoData => ZString.Empty;
	}
}
