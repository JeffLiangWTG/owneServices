using System;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Environment;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal class HeaderWrapper : IHeader
	{
		internal HeaderWrapper(AsycudaManifestHeader header, ZString action)
		{
			this.header = header;
			isFirstDownload = action == MessageSubTypeCodes.Codes.Original;
			sippingDate = Convert.ToDateTime(Env.Time.CurrentLocalDateTime.ToShortDateString());
		}
		readonly AsycudaManifestHeader header;
		readonly bool isFirstDownload;
		readonly DateTime sippingDate;

		string IHeader.Year => sippingDate.Year.ToString();

		bool IHeader.IsFirstDownload => isFirstDownload;

		uint IHeader.ShippingNumber => uint.TryParse(header.AMA_ManifestNumber, out var shippingNumber) ? shippingNumber : 0;

		DateTime IHeader.ShippingDate => sippingDate;

		DateTime IHeader.InitialDate => new DateTime(sippingDate.Year, 1, 1);

		DateTime IHeader.FinalDate => new DateTime(sippingDate.Year, 12, 31);

		double IHeader.TotalValue => !header.TravelDocumentType.IsEmpty ? double.Parse(header.TravelDocumentType) : 0;
	}
}
