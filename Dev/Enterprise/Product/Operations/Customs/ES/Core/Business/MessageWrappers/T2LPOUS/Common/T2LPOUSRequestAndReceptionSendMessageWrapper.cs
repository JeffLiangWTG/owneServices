using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.ES.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class T2LPOUSRequestAndReceptionSendMessageWrapper : T2LPOUSCommonSendMessageWrapper, IT2LPOUSRequestAndReceptionMessageDataProvider
{
	public T2LPOUSRequestAndReceptionSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
	{
		entryInstruction = entryHeader.EntryInstruction;
	}
	protected readonly CusEntryInstruction entryInstruction;

	public IT2LPOUSAuthorisation Authorisation
	{
		get
		{
			if (authorisation == null)
			{
				var auth = entryInstruction.CusAuthorizationUsages.Cast<CusAuthorizationUsage>().FirstOrDefault(x => x.AGC_Code == CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer);
				authorisation = auth != null && entryInstruction.ZG_RequestType == RequestTypeList.Codes.RegistrationRequest ? new T2LPOUSAuthorisationWrapper(auth) : null;
			}
			return authorisation;
		}
	}
	T2LPOUSAuthorisationWrapper authorisation;

	public IT2LPOUSCommonPersonReqPres Representative => representative ?? (representative = isRepresentativeDeclared ? T2LPOUSCommonPersonReqPresWrapper.New(GetRepresentative, contactEmail) : null);
	T2LPOUSCommonPersonReqPresWrapper representative;

	public IT2LPOUSGoodsShipment GoodsShipment => goodsShipment ?? (goodsShipment = new T2LPOUSGoodsShipmentWrapper(entryHeader));
	T2LPOUSGoodsShipmentWrapper goodsShipment;

	protected override ZBool isRepresentativeDeclared => GetRepresentative != null && GetRepresentative.Header != OrgAddressForPersonReqPres?.Header;

	OrgAddress GetRepresentative => declaration.Representative ?? declaration.DeclarantOrgAddress;

	public override ZString SendEmailU => SendEmailFixed;

	public override ZString SendEmailExp => SendEmailFixed;

	protected override OrgAddress OrgAddressForPersonReqPresCommon => OrgAddressForPersonReqPres;

	protected virtual OrgAddress OrgAddressForPersonReqPres { get; }
}
