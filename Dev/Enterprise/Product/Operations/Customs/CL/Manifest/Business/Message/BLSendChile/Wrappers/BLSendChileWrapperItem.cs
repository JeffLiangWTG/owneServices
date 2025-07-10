using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CL.Manifest.Business
{
	internal class BLSendChileWrapperItem : IDocumentItem
	{
		internal BLSendChileWrapperItem(AsycudaPack pack)
		{
			this.pack = Argument.NotNull(pack, nameof(pack));
		}
		readonly AsycudaPack pack;

		string IDocumentItem.Number => pack.APA_LineNo.ToString();

		string IDocumentItem.GoodMarks => pack.APA_MarksAndNumbers;

		bool IDocumentItem.DangerousGoods => pack.UNDGs?.Count > 0;

		string IDocumentItem.BulkType
		{
			get
			{
				var query = new ZQuery(RefPacksSchema.RP_CommercialPack, this.pack.APA_PackUQ);
				query.AddToFilter(RefPacksSchema.RP_CustomsCountry, Core.Constants.CountryCodes.Chile);
				var pack = this.pack.Factory.LoadTop1<CusRefPacks>(query);

				return pack?.RP_CustomsPack ?? this.pack.APA_PackUQ;
			}
		}

		string IDocumentItem.GoodDescription => pack.APA_GoodsDescription;

		string IDocumentItem.PackageQty => pack.APA_PackQty.ToString();

		string IDocumentItem.GrossWeight => CLMessageHelper.WeightConvertion(pack.APA_WeightUQ, pack.APA_Weight);

		string IDocumentItem.WeightUQ => CLMessageHelper.WeightUnitCodeCalculator(pack.APA_WeightUQ);

		string IDocumentItem.Volume => CLMessageHelper.VolumeConvertion(pack.APA_VolumeUQ, pack.APA_Volume);

		string IDocumentItem.VolumeUQ => CLMessageHelper.VolumeUnitCodeCalculator(pack.APA_VolumeUQ);

		bool IDocumentItem.Containerized => pack.Container != null;

		IReadOnlyCollection<IIMO> IDocumentItem.ItemsIMO
		{
			get
			{
				var result = new List<IIMO>();

				foreach (UNDGDataItem undg in pack.UNDGs)
				{
					result.Add(new BLSendChileWrapperIMO(undg));
				}

				return result.ToArray();
			}
		}

		IReadOnlyCollection<IItemContainer> IDocumentItem.Containers
		{
			get
			{
				var result = new List<IItemContainer>();

				if (pack.Container != null)
				{
					result.Add(new BLSendChileWrapperContainer(pack.Container, pack.APA_WeightUQ, pack.UNDGs));
				}

				return result.ToArray();
			}
		}
	}
}
