using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Business;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AE.Manifest.Business;

public sealed class CUSCARMessageProvider : EDIFACTMessageProviderBase<AsycudaBill>, ICUSCARMessageProvider
{
	public CUSCARMessageProvider(MessageChooserItem messageItem)
		: base(messageItem.Bill)
	{
		MessageItem = messageItem;
		Bill = messageItem.Bill;
		Header = messageItem.Bill.Header;
	}
	AsycudaBill Bill { get; }
	AsycudaManifestHeader Header { get; }
	MessageChooserItem MessageItem { get; }

	public override IMessageHeaderProvider MessageHeader => messageHeaderProvider ??= new CUSCARMessageHeaderProvider(MessageTypeList.CustomsCargoReportMessage);
	IMessageHeaderProvider messageHeaderProvider;

	public IMessageDetailsProvider MessageDetails => messageDetails ??= new MessageDetailsProvider(MessageItem);
	IMessageDetailsProvider messageDetails;

	public IDateTimePeriodProvider BillIssueDate => CachedValueHelper.GetValue(ref billIssueDate, () => GetBillIssueDate());
	CachedValue<IDateTimePeriodProvider> billIssueDate;

	public ILocationProvider BillIssueLocation => billIssueLocation
				??= new LocationProvider(LocationFunctionCodeQualifierList.PlaceOfDocumentIssue, Header.AMA_RL_NKPortOfLoading);
	ILocationProvider billIssueLocation;

	public IReadOnlyCollection<IReferenceProvider> References => references ??= GetReferences();
	IReadOnlyCollection<IReferenceProvider> references;

	public IReadOnlyCollection<IPartyProvider> RelatedParties => relatedParties ??= GetRelatedParties();
	IReadOnlyCollection<IPartyProvider> relatedParties;

	public IFreeTextProvider DocumentUpdates => CachedValueHelper.GetValue(ref documentUpdates, () => GetDocumentUpdates());
	CachedValue<IFreeTextProvider> documentUpdates;

	public bool IsNegotiable => isNegotiable ??= Bill.Negotiable == NegotiableList.Codes.Yes;
	bool? isNegotiable;

	public string Payer => payer ??= Bill.Payer;
	string payer;

	public IReadOnlyCollection<ITransportEquipmentInfoProvider> ContainerInfos => containerInfos ??= GetContainerInfos();
	IReadOnlyCollection<ITransportEquipmentInfoProvider> containerInfos;

	public IConsignmentInfoProvider BillInfo => billInfo ??= new ConsignmentInfoProvider(Bill);
	IConsignmentInfoProvider billInfo;

	DateTimePeriodProvider GetBillIssueDate()
	{
		var billIssueDate = Header.AMA_MasterBillIssueDate;
		if (billIssueDate.IsValid)
		{
			var functionCode = DateOrTimeOrPeriodFunctionCodeQualifierList.BillOfLadingDate;
			var format = DateOrTimeOrPeriodFormatCodeList.Ccyymmdd;
			var dateText = billIssueDate.ToString("yyyyMMdd");
			return new DateTimePeriodProvider(functionCode, dateText, format);
		}
		return null;
	}

	List<ReferenceProvider> GetReferences()
	{
		var result = new List<ReferenceProvider>
		{
			new (ReferenceCodeQualifierList.HouseBillOfLadingNumber, Header.AMA_MasterBill),
			new (ReferenceCodeQualifierList.BillOfLadingNumber, Bill.ABL_BillNumber),
		};
		if (!Bill.ABL_SplitBillNumber.IsEmpty)
		{
			result.Add(new ReferenceProvider(ReferenceCodeQualifierList.RelatedDocumentNumber, Bill.ABL_SplitBillNumber));
		}
		return result;
	}

	List<PartyProvider> GetRelatedParties()
	{
		var result = new List<PartyProvider>();
		if (!Header.AMA_OA_Carrier.IsEmpty && Header.Carrier?.Header is OrgHeader carrierOrg)
		{
			result.Add(new PartyProvider(PartyFunctionCodeQualifierList.Carrier, GetOrgMPCI(carrierOrg)));
		}
		if (!Bill.ABL_OA_Forwarder.IsEmpty && Bill.Forwarder?.Header is OrgHeader forwarderOrg && Bill.ABL_BolType == ShipmentTypes.CoLoadMaster)
		{
			result.Add(new PartyProvider(PartyFunctionCodeQualifierList.ConsigneesFreightForwarder, GetOrgMPCI(forwarderOrg)));
		}
		return result;
	}

	FreeTextProvider GetDocumentUpdates()
	{
		var subjectCode = MessageItem.SubjectCode;
		var subject = MessageItem.Subject;
		return subjectCode.IsEmpty && subject.IsEmpty
				? null
				: new FreeTextProvider(subjectCode, subject);
	}

	List<TransportEquipmentInfoProvider> GetContainerInfos()
	{
		return Bill.Packs.Cast<AsycudaPack>()
			.Where(x => x.Container is AsycudaContainer)
			.Select(x => x.Container)
			.Distinct()
			.Select(x => new TransportEquipmentInfoProvider(x, Bill))
			.ToList();
	}

	string GetOrgMPCI(OrgHeader org)
	{
		return org?.CustomsCodes.GetCustomsRegNo(OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber, CountryCodes.UnitedArabEmirates);
	}
}
