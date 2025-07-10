using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CC044CConsignmentWrapper : ConsignmentWrapper
	{
		protected CC044CConsignmentWrapper(NctsArrivalMovementHeader movementHeader, OrgHeader actualConsignee) : base(movementHeader, actualConsignee)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		}

		protected readonly NctsArrivalMovementHeader movementHeader;

		public static CC044CConsignmentWrapper New(NctsArrivalMovementHeader movementHeader, OrgHeader actualConsignee = null) => movementHeader == null ? null : new CC044CConsignmentWrapper(movementHeader, actualConsignee);

		NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = (NctsHeader)movementHeader.Header);
		NctsHeader nctsHeader;

		public override decimal? GrossMass => grossMass ?? (grossMass = movementHeader.EffectiveGrossWeightUnloaded);
		decimal? grossMass;

		protected override ICollection<ITransportEquipment> GetTransportEquipments()
		{
			var result = new Collection<ITransportEquipment>();
			NctsHeader.ArrivalHeaderContainers.Cast<EU.NCTS.Business.NctsArrivalHeaderContainer>().Where(c => c.BC_UnloadedState.In(new ZString[] { EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW, EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS, EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF })
					|| (c.BC_UnloadedState == EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC && c.Seals.Cast<CusSeal>().Any(s => s.BK_UnloadingState.In(new ZString[] { EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW, EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS, EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF })))).ForEach(x => result.Add(CC044CTransportEquipmentWrapper.New(x, GetArrivalLinkedGoodsItemNumbers(x.BC_ContainerNum))));
			return result;
		}

		protected override ICollection<string> GetArrivalLinkedGoodsItemNumbers(ZString containerNumber)
		{
			return NctsHeader.Bills.SelectMany(x => x.ArrivalGoodsItems)
			.Where(x => x.Packages.Cast<EU.NCTS.Business.NctsPackage>().SelectMany((EU.NCTS.Business.NctsPackage x) => x.ContainersSelected).Contains(containerNumber))
			.Select(x => x.BY_DeclarationGoodsItemNumber.ToString())
			.ToHashSet();
		}

		protected override ICollection<IDepartureTransportMeans> GetDepartureTransportMeans()
		{
			var result = new Collection<IDepartureTransportMeans>();
			movementHeader.ArrivalTransportInfos.Cast<EU.NCTS.Business.ArrivalCusTransportMeans>().Where(t => t.TPM_TransportState.In(new ZString[] { EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW, EU.NCTS.Business.NctsUnloadedStateList.Codes.MIS })).ForEach(transportMeans => result.Add(CC044CArrivalTransportMeansWrapper.New(transportMeans)));
			return result;
		}

		protected override ICollection<ISupportingDocument> GetSupportingDocuments()
		{
			var result = new Collection<ISupportingDocument>();
			movementHeader.SupportingDocuments.Cast<EU.NCTS.Business.NctsSupportingDocument>().Where(s => s.CSI_Status != EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC).ForEach(document => result.Add(CC044CSupportingDocumentWrapper.New(document)));
			return result;
		}

		protected override ICollection<IDocument> GetTransportDocuments()
		{
			var result = new Collection<IDocument>();
			movementHeader.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument && x.CSI_Status != EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC).ForEach(document => result.Add(CC044CDocumentWrapper.New(document)));
			return result;
		}

		protected override ICollection<IDocument> GetAdditionalReferences()
		{
			var result = new Collection<IDocument>();
			movementHeader.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference && x.CSI_Status != EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC).ForEach(document => result.Add(CC044CDocumentWrapper.New(document)));
			return result;
		}

		protected override ICollection<IHouseConsignment> GetHouseConsignments()
		{
			var result = new Collection<IHouseConsignment>();
			NctsHeader.Bills.Cast<EU.NCTS.Business.NctsBill>().OrderBy(bill => bill.SequenceNumber).Where(x => x.UnloadedStatus != EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC).ForEach(bill => result.Add(CC044CHouseConsignmentWrapper.New(bill)));
			return result;
		}
	}
}
