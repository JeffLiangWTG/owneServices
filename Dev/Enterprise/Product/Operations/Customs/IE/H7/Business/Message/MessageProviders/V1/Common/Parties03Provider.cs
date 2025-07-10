using System;
using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using IParty = CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class Parties03Provider : IParties03
	{
		public Parties03Provider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}
		readonly AsycudaPackedItem packedItem;

		public IReadOnlyCollection<IAdditionalReference> AdditionalFiscalReference => additionalFiscalReference ??=
			packedItem.Bill.ABL_SellerRegNo.IsDefault ? Array.Empty<IAdditionalReference>() : new[] { new AdditionalFiscalReferenceProvider(packedItem.Bill) };
		IReadOnlyCollection<IAdditionalReference> additionalFiscalReference;

		public IParty Exporter => CachedValueHelper.GetValue(ref exporterCached, () => MessageProviderHelper.GetExporter(packedItem.Bill));
		CachedValue<IParty> exporterCached;
	}
}
