using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class AWBSendChileWrapperItem : IDocItems
	{
		internal AWBSendChileWrapperItem(AsycudaPack pack, ZBool isPartial, AsycudaArrivalLine arrivalDetail)
		{
			this.pack = Argument.NotNull(pack, nameof(pack));
			this.isPartial = isPartial;
			this.arrivalDetail = arrivalDetail;
		}
		readonly AsycudaPack pack;
		readonly bool isPartial;
		readonly AsycudaArrivalLine arrivalDetail;

		string IDocItems.Number => pack.APA_LineNo.ToString();

		bool IDocItems.DangerousGoods => pack.UNDGs?.Count > 0;

		string IDocItems.PackageQty
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

		string IDocItems.GrossWeight
		{
			get
			{
				if (isPartial)
				{
					return arrivalDetail == null ? CLMessageHelper.WeightConvertion(pack.APA_WeightUQ, pack.APA_Weight) : CLMessageHelper.WeightConvertion(arrivalDetail.ATL_WeightUQ, arrivalDetail.ATL_Weight);
				}
				else
				{
					return CLMessageHelper.WeightConvertion(pack.APA_WeightUQ, pack.APA_Weight);
				}
			}
		}

		string IDocItems.WeightUQ => CLMessageHelper.WeightUnitCodeCalculator(pack.APA_WeightUQ);

		string IDocItems.Volume => CLMessageHelper.VolumeConvertion(pack.APA_VolumeUQ, pack.APA_Volume);

		string IDocItems.VolumeUQ => CLMessageHelper.VolumeUnitCodeCalculator(pack.APA_VolumeUQ);

		IReadOnlyCollection<IDocProdItems> IDocItems.DocProdItems => new List<IDocProdItems> { new AWBSendChileWrapperProdItem(pack, isPartial, arrivalDetail) };
	}
}
