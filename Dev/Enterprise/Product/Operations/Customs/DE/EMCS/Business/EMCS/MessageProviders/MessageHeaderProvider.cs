using System;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public abstract class MessageHeaderProvider<T> : IEMCSMessageHeader
		where T : IEMCSHeader
	{
		protected MessageHeaderProvider(EMCSJobDeclaration emcsJobDeclaration)
		{
			this.emcsJobDeclaration = Argument.NotNull(emcsJobDeclaration, nameof(emcsJobDeclaration));
		}
		protected readonly EMCSJobDeclaration emcsJobDeclaration;

		public IDateAndTime PreparationDateAndTimeCET => preparationDateTimeCET ?? (preparationDateTimeCET = new CentralEuropeanStandardDateAndTimeProvider());
		IDateAndTime preparationDateTimeCET;

		public string Sender => SenderAndBinDetails.Sender;

		public string Recipient => emcsJobDeclaration.JE_DeclarantType == EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor
									? emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch)
									: emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival);

		public string Bin => SenderAndBinDetails.Bin;

		public string InterchangeControlReference => EDIInterchange.InterchangeNumberPlaceHolder;

		public string MessageIdentifier => EDIMessage.SendersReferencePlaceHolder;

		public IEMCSHeader Header => header ?? (header = (IEMCSHeader)Activator.CreateInstance(typeof(T), emcsJobDeclaration));
		protected IEMCSHeader header;

		(string Sender, string Bin) SenderAndBinDetails
		{
			get
			{
				if (!senderAndBinDetails.HasValue)
				{
					var sender = (ZString)DECustomsDataRegistry.Instance.EMCSExciseTraderNumber.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					var bin = (ZString)DECustomsDataRegistry.Instance.EMCSParticipantIdentificationNumber.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (sender.IsEmpty || bin.IsEmpty)
					{
						var isConsignor = emcsJobDeclaration.JE_DeclarantType == EU.EMCS.Business.EMCSEntryTypeList.Codes.Consignor;
						var organisation = isConsignor ? emcsJobDeclaration.Consignor : emcsJobDeclaration.Consignee;
						bin = organisation?.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EMCSParticipantIdentificationNumber) ?? ZString.Empty;
						sender = organisation?.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber) ?? ZString.Empty;
						if (sender.IsEmpty || bin.IsEmpty)
						{
							var warehouseAddress = isConsignor ? emcsJobDeclaration.DispatchWarehouseDocumentaryAddress : emcsJobDeclaration.DestinationWarehouseDocumentaryAddress;
							if (warehouseAddress != null && warehouseAddress.IsValidAddress)
							{
								var warehouseOrgAddress = warehouseAddress.Address;
								bin = warehouseOrgAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber);
								sender = warehouseOrgAddress.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID);
							}
						}
					}
					senderAndBinDetails = (sender, bin);
				}
				return senderAndBinDetails.Value;
			}
		}
		(string Sender, string Bin)? senderAndBinDetails;
	}
}
