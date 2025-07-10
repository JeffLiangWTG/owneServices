using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class CUSCONHeaderProvider : ImportHeaderProvider, ICUSCONHeader
	{
		public CUSCONHeaderProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
			entryInstruction = Argument.NotNull(EntryHeader.EntryInstruction, nameof(EntryHeader.EntryInstruction));
		}
		readonly CusEntryInstruction entryInstruction;

		public string TemporaryReferenceNumber => EntryHeader.MovementReferenceNumber;

		public string GoodsLocation => Declaration.JE_LocationOfGoods;

		public IPartyID PresentationConfirmer
		{
			get
			{
				OrgAddress presentationConfirmersOrgAddress = null;
				var declarantType = Declaration.JE_DeclarantType;
				if (!declarantType.IsEmpty)
				{
					if (declarantType == RepresentationTypeList.Codes._2Direct)
					{
						presentationConfirmersOrgAddress = Declaration.Representative;
					}
					else
					{
						presentationConfirmersOrgAddress = Declaration.Declarant;
					}
				}
				return ImportPartyIDProvider.NewOrNull(presentationConfirmersOrgAddress);
			}
		}

		public IImportPartyContactPerson ContactPerson => ImportPartyContactPersonProvider.NewOrNull(GlbStaff.CurrentUser);

		public string ArrivalTransportMeansIdentity => Declaration.JE_TransportMode == Customs.Business.TransportTypeList.Codes.FixedTransportInstallations ? null : Declaration.ZG_Box18TransportID.ToString();

		public string PreviousAdministrativeReferenceType => previousAdministrativeReferenceType;

		public string PreviousAdministrativeReferenceNumber => entryInstruction.PreviousDocumentMaster.HasPreviousDocuments ? entryInstruction.PreviousDocuments[0].CSI_ReferenceNumber.ToString() : null;

		public ISummaryDeclaration SummaryDeclaration => Factory.GetValue(ref summaryDeclarationCached, () => ProvideSummaryDeclaration ? SummaryDeclarationProvider.NewOrNull(entryInstruction.PreviousDocumentMaster) : null);
		CachedProperty<ISummaryDeclaration> summaryDeclarationCached;

		public ICustomsWarehouse CustomsWarehouse => Factory.GetValue(ref customsWarehouseCached, () => ProvideCustomsWarehouse ? CustomsWarehouseProvider.NewOrNull(entryInstruction.PreviousDocumentMaster) : null);
		CachedProperty<ICustomsWarehouse> customsWarehouseCached;

		public IInwardProcessing InwardProcessing => Factory.GetValue(ref inwardProcessingCached, () => ProvideInwardProcessing ? InwardProcessingProvider.NewOrNull(entryInstruction.PreviousDocumentMaster) : null);
		CachedProperty<IInwardProcessing> inwardProcessingCached;

		ZString previousAdministrativeReferenceType => entryInstruction.PreviousDocumentMaster.CSI_Procedure;
		bool ProvideSummaryDeclaration => previousAdministrativeReferenceType == PreviousProcedureList.Codes._ATNEU;
		bool ProvideCustomsWarehouse => previousAdministrativeReferenceType == PreviousProcedureList.Codes._ATZL;
		bool ProvideInwardProcessing => previousAdministrativeReferenceType == PreviousProcedureList.Codes._ATAV;
	}
}
