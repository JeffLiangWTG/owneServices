using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class WarehouseCustomsLineAddInfoAuthorization : IWarehouseCustomsLineAddInfo
	{
		public WarehouseCustomsLineAddInfoAuthorization(CustomsReference authorization)
		{
			this.authorization = Argument.NotNull(authorization, nameof(authorization));
		}

		readonly CustomsReference authorization;

		public ZString Type => BondedWarehousingHelper.Constants.EntryInstructionAuthorisationAddInfo.Type;

		public ZString AddInfoData
		{
			get
			{
				if (!addInfoData.HasValue)
				{
					addInfoData = AddInfoParser.Serialise(new[]
					{
						new KeyValuePair<ZString, ZString>(BondedWarehousingHelper.Constants.EntryInstructionAuthorisationAddInfo.AddInfoKeys.Code, authorization.SubType.GetCodeAsUpperCase()),
						new KeyValuePair<ZString, ZString>(BondedWarehousingHelper.Constants.EntryInstructionAuthorisationAddInfo.AddInfoKeys.Number, authorization.Reference.GetValueOrDefault())
					});
				}
				return addInfoData.Value;
			}
		}
		ZString? addInfoData;

		public ZString NAddInfoData => ZString.Empty;
	}
}
