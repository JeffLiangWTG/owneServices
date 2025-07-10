using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal class ContainerItemWrapper : IItem
	{
		internal ContainerItemWrapper(AsycudaContainer container)
		{
			this.container = CargoWise.Common.Argument.NotNull(container, "AsycudaContainer cannot be null");
			header = container.Header;
		}
		readonly AsycudaContainer container;
		readonly AsycudaManifestHeader header;

		string IItem.BulkType => COWrappersHelper.GetLoadType(header);

		string IItem.ContainerIDNumber => container?.ACN_ContainerNumber;

		string IItem.Size
		{
			get
			{
				var query = new ZQuery(RefContainerCodeMapSchema.RCM_RC_Container, container.ACN_RC_ContainerType);
				query.AddToFilter(RefContainerCodeMapSchema.RCM_RN_NKCountry, Core.Constants.CountryCodes.Colombia);
				var containercodemap = container?.Factory.LoadTop1<RefContainerCodeMap>(query);

				return containercodemap?.RCM_Code ?? ZString.Empty;
			}
		}

		string IItem.EquipmentType
		{
			get
			{
				var containerType = container?.Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.PK, container.ACN_RC_ContainerType));
				return containerType != null ? COWrappersHelper.GetContainerType(containerType.RC_ContainerType) : ZString.Empty;
			}
		}

		string IItem.SealNumber => container?.ACN_Seal1;

		decimal IItem.GrossWeight => header.Bills.Cast<AsycudaBill>().SelectMany(b => b.Packs.Cast<AsycudaPack>())
			.Where(p => p.Container?.PK == container.PK)
			.Sum(p => Core.Constants.Weight.ConvertSafe(p.APA_Weight, p.APA_WeightUQ, Core.Constants.Weight.Kilograms));

		int IItem.BulkQty => container.ACN_NumberOfPackages;

		decimal IItem.Volume => header.Bills.Cast<AsycudaBill>().SelectMany(b => b.Packs.Cast<AsycudaPack>())
			.Where(p => p.Container?.PK == container.PK)
			.Sum(p => Core.Constants.Volume.ConvertSafe(p.APA_Volume, p.APA_VolumeUQ, Core.Constants.Volume.CubicMetres))
			.Round(2);

		IReadOnlyCollection<IPack> IItem.Packs
		{
			get
			{
				var result = new List<IPack>();
				var packs = new List<AsycudaPack>();

				foreach (AsycudaBill bill in header.Bills)
				{
					packs.AddRange(from AsycudaPack pack in bill.Packs
								   where pack.ContainerPK == container.PK
								   select pack);
				}
				result.AddRange(from AsycudaPack pack in packs
								select new PackWrapper(pack));

				return result.ToArray();
			}
		}
	}
}
