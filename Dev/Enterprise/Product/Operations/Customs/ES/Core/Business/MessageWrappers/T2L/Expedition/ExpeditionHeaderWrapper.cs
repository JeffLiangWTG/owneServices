using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ExpeditionHeaderWrapper : IExpeditionHeader
	{
		public ExpeditionHeaderWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			jobDeclaration = Argument.NotNull(entryHeader.Declaration, "entryHeader.Declaration");
		}
		protected readonly CusEntryHeader entryHeader;
		readonly JobDeclaration jobDeclaration;

		public ZString ExpeditionCustomsOffice => jobDeclaration.JE_CustomsOffice.SubstringSafe(jobDeclaration.JE_CustomsOffice.Length - 6);

		public ZString ExpeditionCountry => jobDeclaration.JE_GoodsOrigin;

		public ZString DestinationCountry => jobDeclaration.JE_GoodsDestination;

		public ZInt TotalLinesNum => entryHeader.MergedLines.Count;

		public ZInt TotalPackagesQty => entryHeader.PackagesCount;

		public ZBool ContainersIndicator => entryHeader.IsContainerised();

		public ZString ConveyanceId => jobDeclaration.JE_TransportMode == Core.Constants.TransportModes.Air ? jobDeclaration.JE_VoyageFlightNo : jobDeclaration.JE_VesselName;

		public IPartyProvider Sender => CachedValueHelper.GetValue(ref sender, () => PartyWrapper.New(jobDeclaration.SupplierDocumentaryAddress));
		CachedValue<IPartyProvider> sender;

		public IPartyProvider Consignee => CachedValueHelper.GetValue(ref consignee, () => ExpeditionImporterWrapper.New(jobDeclaration.ImporterDocumentaryAddress));
		CachedValue<IPartyProvider> consignee;

		public IPartyNameProvider Declarant => CachedValueHelper.GetValue(ref declarant, () => PartyNameWrapper.New(jobDeclaration.Declarant));
		CachedValue<IPartyNameProvider> declarant;

		public IT2LCommunicationsCommon Communications => communications ?? (communications = new T2LCommunicationsCommonWrapper(jobDeclaration));
		T2LCommunicationsCommonWrapper communications;
	}
}
