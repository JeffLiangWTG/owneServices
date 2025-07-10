using System;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class TS332MessageProvider : ITS332Header
	{
		public TS332MessageProvider(TemporaryStorageMessageSendingObject sender)
		{
			this.sendingObject = Argument.NotNull(sender, nameof(sender));
			this.header = Argument.NotNull(sender.Header, nameof(sender.Header));
			PreparationDateAndTime = DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.UtcNow, true);
			PresentationOffice = header.PresentationCustomsOffice;
			CustomsOfficeLodgement = header.CustomsOfficeOfLodgement;
		}
		protected readonly TemporaryStorageHeader header;
		protected readonly TemporaryStorageMessageSendingObject sendingObject;

		public IDeclaration09 Declaration => CachedValueHelper.GetValue(ref declarationCached, () => Declaration09Provider.New(header));
		CachedValue<IDeclaration09> declarationCached;

		public IRepresentativeType05 Representative => CachedValueHelper.GetValue(ref representativeCached, () => RepresentativeProvider.New(header));
		CachedValue<IRepresentativeType05> representativeCached;

		public IDeclarant03 Declarant => CachedValueHelper.GetValue(ref declarantCached, () => DeclarantProvider.New(header.Declarant));
		CachedValue<IDeclarant03> declarantCached;

		public string PersonPresentingTheGoodsID => CachedValueHelper.GetValue(ref personPresentingTheGoodsIDCached, () => OrganizationProvider.GetEori(header.Presenter));
		CachedValue<string> personPresentingTheGoodsIDCached;

		public string PresentationOffice { get; }

		public string CustomsOfficeLodgement { get; }

		public IConsignment Consignment => CachedValueHelper.GetValue(ref consignmentCached, () => ConsignmentProvider.New(header));
		CachedValue<IConsignment> consignmentCached;

		public IFallbackProcedure FallbackProcedure => CachedValueHelper.GetValue(ref fallbackProcedureCached, () => FallbackProcedureProvider.New(sendingObject));
		CachedValue<IFallbackProcedure> fallbackProcedureCached;

		public DateTime PreparationDateAndTime { get; }
	}
}
