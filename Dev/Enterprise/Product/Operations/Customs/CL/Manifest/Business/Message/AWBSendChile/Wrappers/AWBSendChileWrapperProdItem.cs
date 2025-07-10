using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class AWBSendChileWrapperProdItem : IDocProdItems
	{
		internal AWBSendChileWrapperProdItem(AsycudaPack pack, ZBool isPartial, AsycudaArrivalLine arrivalDetail)
		{
			this.pack = Argument.NotNull(pack, nameof(pack));
			this.isPartial = isPartial;
			this.arrivalDetail = arrivalDetail;
		}
		readonly AsycudaPack pack;
		readonly ZBool isPartial;
		readonly AsycudaArrivalLine arrivalDetail;

		string IDocProdItems.Description => pack.APA_GoodsDescription;

		string IDocProdItems.Quantity
		{
			get
			{
				if (isPartial)
				{
					return arrivalDetail == null ? (ZString)pack.APA_PackQty.ToString() : (ZString)arrivalDetail.ATL_Quantity.ToString();
				}
				else
				{
					return pack.APA_PackQty.ToString();
				}
			}
		}

		string IDocProdItems.MeasureUQ => AWBSendChileHelper.ProdItemUnitCodeCalculator(pack.APA_PackUQ);
	}
}
