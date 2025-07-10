using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Documents
{
	public abstract class CAEDBuilder
	{
		public CAEDBuilder(IFRMessagesOwner header)
		{
			this.header = Argument.NotNull(header, nameof(header));
			context = ObjectFactory.Get<IContext>(nameof(IContext), this.header.Factory);
		}

		protected readonly IFRMessagesOwner header;

		protected readonly IContext context;

		public CAEDDataObject Build()
		{
			var caed = new CAEDDataObject();

			PopulateAddresses(caed);
			PopulateDeclaration(caed);

			AddValidationsMandatoryFields(caed);
			caed.ValidateAllIncludingChildren();

			return caed;
		}

		void PopulateAddresses(CAEDDataObject caed)
		{
			PopulatePortSystem(caed);
			PopulateCurrentUser(caed);
		}

		void PopulatePortSystem(CAEDDataObject caed)
		{
			caed.PortSystem = PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent
								.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
								.OfType<CommunitySystemCodesOfForwarderAndAgent>()
								.FirstOrDefault(x => x.Port == Port)?.PCS ?? ZString.Empty;
			caed.SICCodeType = caed.PortSystem == PortSystemCodeList.Codes.MGI ? OrgCusCode.FranceCodeTypes.CI5 : OrgCusCode.FranceCodeTypes.SOA;

			var cTOPartySICCode = CTOPartySICCode;
			caed.CTOPartyID = cTOPartySICCode;
			caed.CTOPartySICCode = cTOPartySICCode;
		}

		void PopulateCurrentUser(CAEDDataObject caed)
		{
			caed.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
			var sendingParty = GetSendingParty();
			if (sendingParty != null)
			{
				caed.SendingPartySICCode = GetDefaultSendingPartySICCode();
				caed.SendingPartyID = GetRegistrationNumber(sendingParty, caed.SICCodeType);
			}

			var cCSCode = GetCCSCode();
			caed.RecipientID = cCSCode;
			caed.RecipientSICCode = cCSCode;
		}

		ZString GetCCSCode()
		{
			return ZZRefCusMapCombined.MapCW1CodeToCustomsCode(header.Factory, Core.Constants.CountryCodes.France, RefCusMapTypeList.Codes.FRCCS, PortOfDispatch, ZDateTime.Today);
		}

		ZString GetDefaultSendingPartySICCode()
		{
			return PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent
								.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
								.OfType<CommunitySystemCodesOfForwarderAndAgent>()
								.FirstOrDefault(x => x.Port == PortOfDispatch)?.ForwarderCode ?? ZString.Empty;
		}

		protected abstract ZString PortOfDispatch { get; }

		void PopulateDeclaration(CAEDDataObject caed)
		{
			PopulateReferenceNumbers(caed);
			PopulateTypeAndStatus(caed);
			PopulateOffices(caed);
			PopulateGoodsItems(caed);
			PopulatePortDues(caed);
			PopulateOtherFields(caed);
		}

		void PopulateReferenceNumbers(CAEDDataObject caed)
		{
			caed.JobNumber = JobNumber;
		}

		protected abstract ZString JobNumber { get; }

		void PopulateTypeAndStatus(CAEDDataObject caed)
		{
			caed.DeclarationType = DeclarationType;
			caed.DeclarationStatus = DeclarationStatus;
		}

		protected abstract ZString DeclarationType { get; }
		protected abstract ZString DeclarationStatus { get; }

		void PopulateOffices(CAEDDataObject caed)
		{
			caed.CustomsOfficeCodeOfDeparture = CustomsOfficeCodeOfDeparture;
		}
		protected abstract ICodeDescription CustomsOfficeCodeOfDeparture { get; }

		void PopulateGoodsItems(CAEDDataObject caed)
		{
			PopulatePackages(caed);
		}

		void PopulatePackages(CAEDDataObject caed)
		{
			caed.TotalNumberOfPacks = TotalNumberOfPacks;
			caed.PackageType = PackageType;
		}

		protected abstract ICodeDescription PackageType { get; }
		protected abstract ZInt TotalNumberOfPacks { get; }

		void PopulatePortDues(CAEDDataObject caed)
		{
			caed.PortDuesCurrency = new CodeDescription(Currencies)
			{
				Code = Core.Constants.CurrencyCodes.EuropeanUnion
			};

			caed.Port = Port;
			caed.PortDuesAmount = PortDuesAmount;
		}

		void PopulateOtherFields(CAEDDataObject caed)
		{
			caed.Containers = Containers;
			caed.ContainerNumbers = ContainerNumbers;
			caed.DeclarantsSIRETNumber = DeclarantsSIRETNumber;
			caed.CommonAccessRef = CommonAccessRef;
		}

		protected abstract List<ZString> Containers { get; }
		protected abstract ZString ContainerNumbers { get; }
		protected abstract ZString DeclarantsSIRETNumber { get; }
		protected abstract ZString CommonAccessRef { get; }
		protected abstract ZString Port { get; }
		protected abstract ZDecimal PortDuesAmount { get; }
		protected abstract ZString CTOPartySICCode { get; }

		OrgAddress GetSendingParty() => GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;

		ZString GetRegistrationNumber(OrgAddress address, string codeType)
		{
			var number = address.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.France);
			if (number.IsEmpty)
			{
				number = address.Header.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.France);
			}
			return number;
		}

		void AddValidationsMandatoryFields(CAEDDataObject caed)
		{
			caed.SendingPartyIDInfo.AddMessageErrorIfEmpty(Res.GetString("8D984FBD-DF71-43EA-A142-DF7979BC9DBC", "Sender's ID is required."));

			caed.SendingPartySICCodeInfo.AddError(
				() => caed.SendingPartySICCode.IsEmpty && GetDefaultSendingPartySICCode().IsEmpty,
				Res.GetString("11823560-A1CC-4635-85C8-BAC680AFBC28", "There is no Forwarder code available in Registry \"Port Community System Codes of Forwarder and Agent\"."));
			caed.SendingPartySICCodeInfo.AddMessageErrorIfEmpty(Res.GetString("0CE8347A-C18E-4FDB-B626-F3852C3E9D74", "Sender's SIC code is required."));

			caed.RecipientIDInfo.AddMessageErrorIfEmpty(Res.GetString("C2B60D80-C68B-4723-ABC7-3B1E49C64EBB", "Recipient's ID is required."));

			caed.RecipientSICCodeInfo.AddMessageErrorIfEmpty(Res.GetString("A7A16486-2722-4D84-9483-797706B55607", "Recipient's SIC code is required."));

			caed.CTOPartyIDInfo.AddMessageErrorIfEmpty(Res.GetString("C7AE4224-8606-4E98-B17F-7CBF7BC7B0D0", "CTO address's ID is required."));

			caed.CTOPartySICCodeInfo.AddMessageErrorIfEmpty(Res.GetString("0A17B840-E08A-4695-8C92-928D0EFDA55A", "CTO address's SIC code is required."));

			caed.DeclarantsSIRETNumberInfo.AddMessageErrorIfEmpty(Res.GetString("E0B30E0E-8DD8-4AA0-BE00-BF8FD307DFFA", "Declarant SIRET Number is required."));

			caed.CommonAccessRefInfo.AddMessageErrorIfEmpty(Res.GetString("F18D4803-C65A-401C-85AE-D4BA219FFFE2", "Common Access Reference is required."));

			caed.DeclarationTypeInfo.AddMessageErrorIfEmpty(Res.GetString("CC0CB1F9-09B0-46DA-B29B-27914B5BCC55", "Declaration type is required."));

			caed.JobNumberInfo.AddMessageErrorIfEmpty(Res.GetString("F697AF3B-BBAA-46AB-B2BC-1A6FC9801797", "Job number is required."));
			caed.ContainerNumbersInfo.AddMessageErrorIfEmpty(Res.GetString("9B3D9C2F-AF2B-42BB-8F46-22755DCF82FA", "At least one container is required."));
			caed.TotalNumberOfPacksInfo.AddMessageError(() => caed.TotalNumberOfPacks < 1,
				Res.GetString("34B713F0-CDF8-43F7-B257-193C6EA3003E", "At least one pack should be entered."));
		}

		RefCurrencyCollection Currencies
		{
			get
			{
				if (currencies == null)
				{
					currencies = new RefCurrencyCollection(context.Factory);
				}
				return currencies;
			}
		}
		RefCurrencyCollection currencies;
	}
}
