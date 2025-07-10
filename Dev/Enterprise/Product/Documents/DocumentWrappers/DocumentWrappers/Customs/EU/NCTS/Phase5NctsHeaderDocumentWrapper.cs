using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using CusGoodsLocation = Enterprise.Customs.EU.NCTS.Business.CusGoodsLocation;
using NctsDepartureHeaderContainer = Enterprise.Customs.EU.NCTS.Business.NctsDepartureHeaderContainer;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class Phase5NctsHeaderDocumentWrapper : NctsHeaderDocBaseWrapper
	{
		public static Phase5NctsHeaderDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factory)
		{
			return new Phase5NctsHeaderDocumentWrapper(nctsHeader, factory);
		}

		protected Phase5NctsHeaderDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		new NctsHeader NctsHeader => (NctsHeader)WrappedObject;
		new BusinessObjectFactory Factory => NctsHeader.Factory;

		public ZString MRN => !NctsHeader.MovementReferenceNumber.IsEmpty ? NctsHeader.MovementReferenceNumber : NctsHeader.MovementHeader.MovementReferenceNumber;

		public new ZString Guarantees => Factory.GetCached(ref guaranteesCache, () =>
		{
			var builder = new ZStringBuilder();

			var guarantees = NctsHeader.MovementHeader.Guarantees.Cast<NctsGuarantee>().OrderBy(g => g.PW_BondType).ThenBy(g => g.PW_BondNumber).ToArray();

			if (guarantees.Length > 0)
			{
				var maxBondNumberLength = guarantees.Max(g => g.PW_BondNumber.Length);

				foreach (var guarantee in guarantees)
				{
					var lineBuilder = new ZStringBuilder();
					lineBuilder.Append(guarantee.PW_BondType.PadRight(NctsGuarantee.Schema.PW_BondTypeMaxLength));
					lineBuilder.Append(DocumentWrapperConstants.Delimiters.Hyphen);
					lineBuilder.Append(guarantee.PW_BondNumber.PadRight(maxBondNumberLength));
					builder.Append(lineBuilder.ToString());
				}
			}

			return builder.ToStringWithNewLineBetweenAppends().Trim();
		});
		CachedProperty<ZString> guaranteesCache;

		public ZString CountryOfDispatch => NctsHeader.MovementHeader?.BM_RN_NKCountryOfDispatch ?? ZString.Empty;

		public ZString CountryOfDestination => NctsHeader.MovementHeader?.BM_RL_NKDestinationPort ?? ZString.Empty;

		public ZString ContainerIndicator => Containers.Length > 0 ? EU.DocumentWrapperConstants.Delimiters.CheckedCheckBox : EU.DocumentWrapperConstants.Delimiters.CrossedCheckBox;

		public ZString InlandModeOfTransport => NctsHeader.MovementHeader?.BM_InlandTransportMode ?? ZString.Empty;

		public ZString ModeOfTransportAtTheBorder => NctsHeader.MovementHeader?.BM_ExportTransportMode ?? ZString.Empty;

		public ZString GrossMass => NctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader ? (movementHeader.BM_GrossWeight - 0.00m).ToString(Culture.Invariant) : ZString.Empty;

		public ZString ReferenceNumberUCR => NctsHeader.MovementHeader.BM_UniqueConsignmentReference;

		public ZString CarrierId => GetEORIOrEmpty(NctsHeader.MovementHeader?.Carrier);

		public ZString CarrierAddressFormatted => Carrier is ITrader carrier ? carrier.GetWrappedAddressSummary(GetStreetAndNumber(NctsHeader.MovementHeader?.Carrier)) : ZString.Empty;

		public ZString CarrierContactPersonFormatted => GetCusContact(NctsHeader.MovementHeader?.Carrier);

		public ZString ConsignorId => GetEORIOrEmpty(NctsHeader.Consignor);

		public ZString ConsignorAddressFormatted => Consignor is ITrader consignor ? consignor.GetWrappedAddressSummary(GetStreetAndNumber(NctsHeader.Consignor)) : ZString.Empty;

		public ZString ConsignorContactPersonFormatted => GetCusContact(NctsHeader.Consignor);

		public ZString ConsigneeId => GetEORIOrEmpty(NctsHeader.Consignee);

		public ZString ConsigneeAddressFormatted => Consignee is ITrader consignee ? consignee.GetWrappedAddressSummary(GetStreetAndNumber(NctsHeader.Consignee)) : ZString.Empty;

		public ZString AdditionalSupplyChain => Factory.GetCached(ref additionalSupplyChainCache, GetAdditionalSupplyChain);
		CachedProperty<ZString> additionalSupplyChainCache;

		ZString GetAdditionalSupplyChain()
		{
			var builder = new ZStringBuilder();

			var cusSupplyChainActors = NctsHeader.MovementHeader.CusSupplyChainActors.OrderBy(c => c.CFR_Code).ThenBy(c => c.CFR_Reference).ToArray();
			foreach (var chain in cusSupplyChainActors)
			{
				builder.Append($"{chain.CFR_Code} {chain.CFR_Reference}");
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		public ZString TransportEquipment
		{
			get
			{
				var builder = new ZStringBuilder();

				foreach (var container in Containers)
				{
					var goodsItemsOnContainer = GoodsItemsForContainer(container);
					builder.Append($"{container.BC_SequenceNumber}/{container.BC_ContainerNum}/{container.TotalSealCount}/{goodsItemsOnContainer}");
				}

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		ZString GoodsItemsForContainer(NctsDepartureHeaderContainer container)
		{
			var builder = new ZStringBuilder();
			foreach (var bill in NctsHeader.Bills)
			{
				foreach (var item in bill.GoodsItems)
				{
					foreach (var package in item.Packages)
					{
						foreach (var pivot in package.ContainersPivot)
						{
							if (pivot.Relation2ID == container.PK)
							{
								builder.Append(item.BY_DeclarationGoodsItemNumber.ToString());
							}
						}
					}
				}
			}
			return builder.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Dash);
		}

		public ZString Seal
		{
			get
			{
				var builder = new ZStringBuilder();

				foreach (var container in Containers)
				{
					var seals = new ZString[] { container.Seal1, container.Seal2 }.Concat(container.AdditionalSeals.Select(a => a.BK_SealNumber)).ToArray();
					var line = $"{container.BC_SequenceNumber}/{string.Join(DocumentWrapperConstants.Delimiters.Comma, seals)}";
					builder.Append(line);
				}

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString LocationOfGoods
		{
			get
			{
				var builder = new ZStringBuilder();

				if (CusGoodsLocation != null)
				{
					builder.Append(CusGoodsLocation.CGL_Type);
					builder.Append(CusGoodsLocation.CGL_Qualifier);
					switch (CusGoodsLocation.CGL_Qualifier)
					{
						case CusGoodsLocationQualifierList.Codes.AuthorizationNumber:
							builder.Append(CusGoodsLocation.Address?.AuthorisationNumber ?? string.Empty);
							builder.Append(CusGoodsLocation.CGL_AdditionalIdentifier);
							break;
						case CusGoodsLocationQualifierList.Codes.UnLocode:
							builder.Append(CusGoodsLocation.Unlocode);
							break;
						case CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier:
							builder.Append(CusGoodsLocation.CGL_CustomsOffice);
							break;
						case CusGoodsLocationQualifierList.Codes.EoriNumber:
							builder.Append(CusGoodsLocation.Address.E2_GovRegNum);
							break;
						case CusGoodsLocationQualifierList.Codes.PostcodeAddress:
							builder.Append(CusGoodsLocation.AdditionalIdentifier);
							builder.Append(CusGoodsLocation.Address.E2_Postcode);
							builder.Append(CusGoodsLocation.Address.E2_RN_NKCountryCode);
							break;
						case CusGoodsLocationQualifierList.Codes.Address:
							builder.Append(CusGoodsLocation.Address.E2_Address1AndE2_Address2);
							builder.Append(CusGoodsLocation.Address.E2_City);
							builder.Append(CusGoodsLocation.Address.E2_Postcode);
							builder.Append(CusGoodsLocation.Address.E2_RN_NKCountryCode);
							break;
						case CusGoodsLocationQualifierList.Codes.GnssCoordinates:
							builder.Append(CusGoodsLocation.Address.E2_Latitude.ToString());
							builder.Append(CusGoodsLocation.Address.E2_Longitude.ToString());
							break;
					}
				}

				return builder.ToStringWithNewLineBetweenAppends().Trim();
			}
		}

		public ZString LocationOfGoodsContactPerson
		{
			get
			{
				var builder = new ZStringBuilder();

				if (CusGoodsLocation?.Address is JobDocAddress docAddress)
				{
					builder.Append(docAddress.E2_Contact);
					builder.Append(docAddress.E2_Phone);
					builder.Append(docAddress.E2_Email);
				}

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		CachedProperty<CusGoodsLocation> cusGoodsLocationCache;
		CusGoodsLocation CusGoodsLocation => Factory.GetCached(ref cusGoodsLocationCache, () => NctsHeader.MovementHeader?.GoodsLocation);

		NctsDepartureHeaderContainer[] Containers => containersCache
			??= NctsHeader.MovementHeader?.Header?.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>()
				.Where(c => !c.BC_ContainerNum.IsEmpty)
				.OrderBy(c => c.BC_SequenceNumber).ToArray()
				?? Array.Empty<NctsDepartureHeaderContainer>();
		NctsDepartureHeaderContainer[] containersCache;

		static ZString GetEORIOrEmpty(JobDocAddress address) => address?.Organisation?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) ?? ZString.Empty;

		static ZString GetCusContact(JobDocAddress address)
		{
			static bool HasCusAllocation(OrgContact contact) => contact.Allocations.Find(a => a.PC_Type == OrgConstants.ContactAllocationType.CUS).Any();

			static ZString FirstNonEmptyPhone(OrgContact contact) => new ZString[] {
				contact.OC_Phone,
				contact.OC_Mobile,
				contact.OC_HomePhone,
				contact.OC_OtherPhone
			}.FirstOrDefault(p => !p.IsEmpty);

			if (address?.Organisation?.FilteredContacts.Where(HasCusAllocation).FirstOrDefault() is OrgContact contact)
			{
				return new ZStringBuilder()
					.Append(contact.Name)
					.Append(FirstNonEmptyPhone(contact))
					.Append(contact.Email)
					.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.CommaAndSpace);
			}
			else
			{
				return ZString.Empty;
			}
		}

		public ZString DeclarationType => MovementHeader?.BM_InBondEntryType ?? ZString.Empty;

		public ZString AdditionalDeclarationType => MovementHeader?.BM_AdditionalDeclarationType ?? ZString.Empty;

		public ZString TirCarnetNumber => MovementHeader?.TirCarnetNumber ?? ZString.Empty;

		public ZString TypeOfSecurity => MovementHeader?.BM_TypeOfSecurity ?? ZString.Empty;

		public ZString Security => TypeOfSecurity.ToString() switch
		{
			NctsTypeOfSecurityList.Codes.ENT => "1",
			NctsTypeOfSecurityList.Codes.EXI => "2",
			NctsTypeOfSecurityList.Codes.BTH => "3",
			NctsTypeOfSecurityList.Codes.NON => "0",
			_ => ZString.Empty
		};

		public ZString ReducedDatasetIndicator => (MovementHeader?.BM_ReducedDatasetIndicator ?? ZBool.False) ? EU.DocumentWrapperConstants.Delimiters.CheckedCheckBox : EU.DocumentWrapperConstants.Delimiters.CrossedCheckBox;

		public ZString SpecificCircumstanceIndicator => MovementHeader?.BM_SpecificCircumstance ?? ZString.Empty;

		public ZString BindingItinerary => NctsHeader.CountriesOfRouting.Any() || IsTypeOfSecurityEntExiBth(TypeOfSecurity) ? EU.DocumentWrapperConstants.Delimiters.CheckedCheckBox : EU.DocumentWrapperConstants.Delimiters.CrossedCheckBox;

		public ZString LimitDate => MovementHeader?.BM_ExportDate.ToISO8601ShortDateString() ?? ZString.Empty;

		public ZString AuthorisationsFormatted => MovementHeader?.CusAuthorizationUsages.Select((item, index) => $"{index + 1}: {item.CustomsCode} - {item.AGC_Number}") is IEnumerable<string> lines ?
			string.Join(System.Environment.NewLine, lines) : ZString.Empty;

		public ZString CustomsOfficeOfDeparture => MovementHeader?.DepartureCustomsOffice is Enterprise.Customs.EU.Business.ICustomsOffice customsOffice ? customsOffice.OfficeCode : ZString.Empty;

		public ZString CustomsOfficeOfDestination => MovementHeader?.DestinationCustomsOffice is Enterprise.Customs.EU.Business.ICustomsOffice customsOffice ? customsOffice.OfficeCode : ZString.Empty;

		public ZString CustomsOfficesOfTransitFormatted => MovementHeader?.TransitCustomsOfficeCodeList.Select((item, index) => $"{index + 1}/ {item.OfficeCode}") is IEnumerable<string> lines ?
			string.Join(System.Environment.NewLine, lines) : ZString.Empty;

		public ZString CustomsOfficesOfExitForTransitFormatted => MovementHeader?.ExitForTransitCustomsOfficeCodeList.Select((item, index) => $"{index + 1}/ {item.OfficeCode}") is IEnumerable<string> lines ?
			string.Join(System.Environment.NewLine, lines) : ZString.Empty;

		public ZString HolderOfTheTransitProcedureId
		{
			get
			{
				var tin = Principal?.TIN ?? ZString.Empty;
				return tin.IsEmpty ? NctsHeader.Principal?.Organisation?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU) ?? ZString.Empty : tin;
			}
		}

		public ZString HolderOfTheTransitProcedureTirNameAddressFormatted => Principal is ITrader principal ? string.Join(System.Environment.NewLine, principal.HolderIDTIR, principal.GetWrappedAddressSummary(GetStreetAndNumber(NctsHeader.Principal))) : ZString.Empty;

		public ZString HolderOfTheTransitProcedureContactPersonFormatted => GetFormattedContact(NctsHeader.Principal);

		ITrader Representative => CachedValueHelper.GetValue(ref representative, () => TraderWrapper.New(MovementHeader?.Representative, true, IsAddressExtended));
		CachedValue<ITrader> representative;

		public ZString RepresentativeId => Representative?.TIN ?? ZString.Empty;

		public ZString RepresentativeStatus
		{
			get
			{
				if (MovementHeader?.Representative is JobDocAddress repDocAddress)
				{
					var representativePK = repDocAddress.OrganisationPK;
					var principalPK = NctsHeader.Principal.OrganisationPK;
					if (representativePK.IsValid || principalPK.IsValid)
					{
						return representativePK == principalPK ? RepresentativeStatusCode2 : RepresentativeStatusCode3;
					}
				}
				return ZString.Empty;
			}
		}

		public ZString RepresentativeContactPersonFormatted => GetFormattedContact(MovementHeader?.Representative);

		public DocBaseWrapperCollection<Phase5NctsDepartureCargoDescWrapper> Lines => lines ??= GetLinesCore();
		DocBaseWrapperCollection<Phase5NctsDepartureCargoDescWrapper> lines;

		public ZString DepartureTransportMeans => new ZStringBuilder()
			.AppendIfNotEmpty(MovementHeader.TransportTypeAtDeparture)
			.AppendIfNotEmpty(MovementHeader.TransportAtDeparture)
			.AppendIfNotEmpty(MovementHeader.TransportCountryAtDeparture)
			.ToStringWithDelimiterBetweenAppends(EU.DocumentWrapperConstants.Delimiters.Space);

		public ZString CountryOfRoutingOfConsignment
		{
			get
			{
				var builder = new ZStringBuilder();
				foreach (var country in NctsHeader.CountriesOfRouting)
				{
					builder.Append(country.CY_Order +
						EU.DocumentWrapperConstants.Delimiters.Space +
						country.CY_Data);
				}
				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		public ZString ActiveBorderTransportMeans => new ZStringBuilder()
			.AppendIfNotEmpty(MovementHeader.BM_CustomsOfficeAtBorder)
			.AppendIfNotEmpty(MovementHeader.BM_ActiveBorderIdentificationType)
			.AppendIfNotEmpty(MovementHeader.BM_TOLCarrierID)
			.AppendIfNotEmpty(MovementHeader.BM_RN_NKTOLCarrierNationality)
			.ToStringWithDelimiterBetweenAppends(EU.DocumentWrapperConstants.Delimiters.Space);

		public ZString ConveyanceRefNo => MovementHeader.BM_ConveyanceNumber;

		public ZString PlaceOfLoading
		{
			get
			{
				var code = MovementHeader.BM_PortOfPresentationCode;
				if (code.Length is 2)
				{
					var builder = new ZStringBuilder();
					builder.Append(COUNTRY + code)
						.AppendIfNotEmpty(LOCATION + MovementHeader.BM_PlaceOfLoading);
					return builder.ToStringWithNewLineBetweenAppends();
				}
				else
				{
					return code.Length > 0 ? UNLOCODE + code : ZString.Empty;
				}
			}
		}

		public ZString PlaceOfUnloading
		{
			get
			{
				var code = MovementHeader.BM_ForeignDestPortKCode;
				if (code.Length is 2)
				{
					var builder = new ZStringBuilder();
					builder.Append(COUNTRY + code)
						.AppendIfNotEmpty(LOCATION + MovementHeader.BM_PlaceOfUnloading);
					return builder.ToStringWithNewLineBetweenAppends();
				}
				else
				{
					return code.Length > 0 ? UNLOCODE + code : ZString.Empty;
				}
			}
		}

		public ZString PreviousDocument => NctsHeader.PreviousDocuments.Select((item, index) =>
			GetLineFromCusSupportingInfo(item, index + 1, needSecondRefNum: true)) is IEnumerable<string> lines ? string.Join(System.Environment.NewLine, lines) : ZString.Empty;

		public ZString SupportingDocument => NctsHeader.MovementHeader.SupportingDocuments.Select((item, index) =>
			GetLineFromCusSupportingInfo(item, index + 1, needSecondRefNum: true)) is IEnumerable<string> lines ? string.Join(System.Environment.NewLine, lines) : ZString.Empty;

		public ZString TransportDocument => NctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).Select((item, index) =>
			GetLineFromCusSupportingInfo(item, index + 1)) is IEnumerable<string> lines ? string.Join(System.Environment.NewLine, lines) : ZString.Empty;

		public ZString AdditionalReference => NctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).Select((item, index) =>
			GetLineFromCusSupportingInfo(item, index + 1)) is IEnumerable<string> lines ? string.Join(System.Environment.NewLine, lines) : ZString.Empty;

		public ZString AdditionalInfo => NctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).Select((item, index) =>
			GetLineFromCusSupportingInfo(item, index + 1)) is IEnumerable<string> lines ? string.Join(System.Environment.NewLine, lines) : ZString.Empty;

		public ZString TransportCharges => NctsHeader.Bills.FirstOrDefault()?.B0_TransportPaymentMethod ?? ZString.Empty;

		static string GetLineFromCusSupportingInfo(AutoCusSupportingInfo info, int index, bool needSecondRefNum = false)
		{
			var ref2 = needSecondRefNum ? info.CSI_ReferenceNumber2 : ZString.Empty;
			return $"{index}: {info.CSI_Code} {info.CSI_ReferenceNumber} {ref2}".Trim();
		}
		public ZString TotalItems => GetTotalItemsCore();
		protected virtual ZString GetTotalItemsCore() => ConvertNumberToString(NctsHeader?.TotalNumberOfItems);

		public ZString TotalPackages => GetTotalPackagesCore();
		protected virtual ZString GetTotalPackagesCore() => ConvertNumberToString(NctsHeader?.TotalNumberOfPackages);

		protected virtual DocBaseWrapperCollection<Phase5NctsDepartureCargoDescWrapper> GetLinesCore() => new Phase5NctsDepartureCargoDescWrapperCollection(NctsHeader, Factory);

		ZString GetStreetAndNumber(JobDocAddress address) => address == null ? ZString.Empty : new ZString(string.Join(DocumentWrapperConstants.Delimiters.Space, address.Address1, address.Address2).Trim());

		ZString GetFormattedContact(JobDocAddress address) => address == null ? ZString.Empty : new ZString(string.Join(DocumentWrapperConstants.Delimiters.Space, address.E2_Contact, address.E2_Phone, address.E2_Email).Trim());

		bool IsTypeOfSecurityEntExiBth(ZString typeOfSecurity)
		{
			switch (typeOfSecurity)
			{
				case ENT:
				case EXI:
				case BTH:
					return true;
				default:
					return false;
			}
		}

		protected virtual ZString ConvertNumberToString(IZType number) => number?.ToString() ?? ZString.Empty;

		const string COUNTRY = "COUNTRY" + DocumentWrapperConstants.Delimiters.ColonAndSpace;
		const string LOCATION = "LOCATION" + DocumentWrapperConstants.Delimiters.ColonAndSpace;
		const string UNLOCODE = "UNLOCODE" + DocumentWrapperConstants.Delimiters.ColonAndSpace;

		const string ENT = "ENT";
		const string EXI = "EXI";
		const string BTH = "BTH";

		const string RepresentativeStatusCode2 = "2";
		const string RepresentativeStatusCode3 = "3";
	}
}
