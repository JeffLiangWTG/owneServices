using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class TransshipmentNoticeSubmissionInformationDocumentWrapper(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory)
	: InboundMessageIncludingItemsDocumentWrapper<ITransshipmentNoticeSubmissionInformation, TransshipmentNoticeSubmissionInformationItemDocumentWrapper, HouseBillFieldsResponse>(parseResult, factory)
{
	#region Header Fields

	public ZString H_2 => messageProvider?.SubmitterCode ?? ZString.Empty;

	public ZString H_3 => messageProvider?.VesselCode ?? ZString.Empty;

	public ZString H_4 => messageProvider?.VesselName ?? ZString.Empty;

	public ZString H_5 => messageProvider?.ManifestSubmissionPortCode ?? ZString.Empty;

	public ZString H_6 => messageProvider?.ManifestSubmissionPortSuffix ?? ZString.Empty;

	public ZString H_7 => messageProvider?.ImplementerOfSubmissionProcedure ?? ZString.Empty;

	public ZString H_8 => messageProvider?.MasterBillNumber ?? ZString.Empty;

	#endregion

	protected override DocumentWrapperCollection<TransshipmentNoticeSubmissionInformationItemDocumentWrapper, HouseBillFieldsResponse> GetItemsCore() => new(messageProvider.HouseBillFields, Factory);
}
