using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using ICustomsOffice = CargoWise.Customs.CH.MessageContracts.ICustomsOffice;

namespace Enterprise.Customs.CH.NCTS.Business;

public class BaseTransitDeclarationDataProvider : BaseNctsMessageDataProvider<NctsHeaderDepartureMessageSendingObject>
{
	protected BaseTransitDeclarationDataProvider(NctsHeaderDepartureMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public ITransitOperation TransitOperation => transitOperation ?? (transitOperation = TransitOperationDataProvider.New(sendingObject));
	ITransitOperation transitOperation;

	public ICustomsOffice CustomsOfficeOfDestination => customsOfficeOfDestination ?? (customsOfficeOfDestination = CustomsOfficeDataProvider.New(nctsHeader.MovementHeader.CustomsOffices, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination));
	ICustomsOffice customsOfficeOfDestination;

	public IReadOnlyCollection<ICustomsOffice> CustomsOfficesOfTransit => customsOfficesOfTransit ?? (customsOfficesOfTransit = CustomsOfficeDataProvider.NewCollection(nctsHeader.MovementHeader.CustomsOffices, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit)?.ToCollection());
	IReadOnlyCollection<ICustomsOffice> customsOfficesOfTransit;

	public IReadOnlyCollection<ICustomsOffice> CustomsOfficesOfExitForTransit => customsOfficesOfExitForTransit ?? (customsOfficesOfExitForTransit = CustomsOfficeDataProvider.NewCollection(nctsHeader.MovementHeader.CustomsOffices, OfficeCodes_NCTS.Codes.NCTSOfficeOfExitForTransit)?.ToCollection());
	IReadOnlyCollection<ICustomsOffice> customsOfficesOfExitForTransit;

	public IReadOnlyCollection<IGuarantee> Guarantees => guarantees ??= GuaranteeDataProvider.NewCollection(nctsHeader.MovementHeader.Guarantees).ToArray();
	IReadOnlyCollection<IGuarantee> guarantees;

	public IConsignment Consignment => consignment ?? (consignment = ConsignmentDataProvider.New(nctsHeader));
	IConsignment consignment;

	public IHolderOfTransitProcedure HolderOfTransitProcedure => holderOfTransitProcedure ?? (holderOfTransitProcedure = HolderOfTransitProcedureDataProvider.New(nctsHeader.Principal));
	IHolderOfTransitProcedure holderOfTransitProcedure;

	public IPerson Representative => representative ?? (representative = PersonDataProvider.New(nctsHeader.MovementHeader?.Representative?.Organisation, GlbStaff.CurrentUser));
	IPerson representative;
}
