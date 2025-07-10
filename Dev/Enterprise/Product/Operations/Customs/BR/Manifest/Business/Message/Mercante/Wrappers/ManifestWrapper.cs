using System.Collections.Generic;
using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class ManifestWrapper : IManifest
	{
		public ManifestWrapper(AsycudaManifestHeader header)
		{
			this.header = header;
		}
		readonly AsycudaManifestHeader header;

		IHeader IManifest.Header => iHeader ?? (iHeader = new HeaderWrapper(header));
		IHeader iHeader;

		IReadOnlyCollection<IHouse> IManifest.Bills
		{
			get
			{
				var result = new List<IHouse>();

				foreach (AsycudaBill bill in header.Bills)
				{
					result.Add(new HouseWrapper(bill));
				}

				return result.ToArray();
			}
		}
	}
}
