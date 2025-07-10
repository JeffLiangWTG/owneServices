using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public abstract class G3CommonHeaderWrapper : IG3CommonHeader
	{
		protected G3CommonHeaderWrapper(IEnumerable<AsycudaBill> bills, string localReferenceNumber)
		{
			this.bills = Argument.NotNull(bills, nameof(bills));
			this.localReferenceNumber = Argument.NotNull(localReferenceNumber, nameof(localReferenceNumber));

			header = Argument.NotNull(bills?.FirstOrDefault()?.Header, "bills[0].Header");
		}

		protected readonly IEnumerable<AsycudaBill> bills;

		protected readonly AsycudaManifestHeader header;

		protected readonly string localReferenceNumber;

		public ZString LRN => localReferenceNumber;

		public ZString CustomsOffice => header.AMA_CustomsOffice;

		public ZString PersonPresentingGoods => header.Presenter?.GetEOROrNIFCode() ?? ZString.Empty;

		public IG3Declarant Declarant => CachedValueHelper.GetValue(ref declarant, () => G3DeclarantWrapper.New(header.Declarant));
		CachedValue<IG3Declarant> declarant;

		public IG3Representative Representative => CachedValueHelper.GetValue(ref representative, () => G3RepresentativeWrapper.New(header.Representative?.Header));
		CachedValue<IG3Representative> representative;

		public ZDateTime DeclarationDate => DateTime.MinValue;

		public ZDateTime PresentationDate => DateTime.MinValue;
	}
}
