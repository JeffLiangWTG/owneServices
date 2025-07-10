using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class WarehouseCustomsLineAddInfoPreviousDocument : IWarehouseCustomsLineAddInfo
	{
		public WarehouseCustomsLineAddInfoPreviousDocument(CustomsSupportingInformation document)
		{
			this.previousDocument = Argument.NotNull(document, nameof(document));
		}

		readonly CustomsSupportingInformation previousDocument;

		public ZString Type => BondedWarehousingHelper.Constants.PreviousDocumentAddInfo.Type;

		public ZString AddInfoData
		{
			get
			{
				if (!addInfoData.HasValue)
				{
					addInfoData = AddInfoParser.Serialise(new[]
					{
						new KeyValuePair<ZString, ZString>(BondedWarehousingHelper.Constants.PreviousDocumentAddInfo.AddInfoKeys.Type, previousDocument.Type.GetCodeAsUpperCase()),
						new KeyValuePair<ZString, ZString>(BondedWarehousingHelper.Constants.PreviousDocumentAddInfo.AddInfoKeys.Reference, previousDocument.ReferenceNumber.GetValueOrDefault())
					});
				}
				return addInfoData.Value;
			}
		}
		ZString? addInfoData;

		public ZString NAddInfoData => ZString.Empty;
	}
}
