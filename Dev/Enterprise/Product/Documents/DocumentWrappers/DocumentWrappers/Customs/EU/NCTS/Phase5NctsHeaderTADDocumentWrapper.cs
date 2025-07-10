using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using CusGoodsLocation = Enterprise.Customs.EU.NCTS.Business.CusGoodsLocation;

namespace Enterprise.DocumentWrappers.Customs.EU.NCTS
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
	public class Phase5NctsHeaderTADDocumentWrapper : NctsHeaderDocBaseWrapper
	{
		public static Phase5NctsHeaderTADDocumentWrapper New(NctsHeader nctsHeader, BusinessObjectFactory factory)
		{
			return new Phase5NctsHeaderTADDocumentWrapper(nctsHeader, factory);
		}

		public static Phase5NctsHeaderTADDocumentWrapper New(NctsEdiMessage ediMessage, BusinessObjectFactory factory)
		{
			return ediMessage.Header != null ? new Phase5NctsHeaderTADDocumentWrapper(ediMessage.Header, factory) : null;
		}

		protected Phase5NctsHeaderTADDocumentWrapper(NctsHeader nctsHeader, BusinessObjectFactory factory) : base(nctsHeader, factory)
		{
		}

		public ZString DeclarationType => MovementHeader?.BM_InBondEntryType ?? ZString.Empty;
		public ZString DeclAdditionalType => MovementHeader?.BM_AdditionalDeclarationType ?? ZString.Empty;
		public ZString SpecificCircumstanceIndicator => MovementHeader?.BM_SpecificCircumstance ?? ZString.Empty;
		public ZString MovementReferenceNumber => base.MOVEMENTREFERENCENUMBER;
		public ZString LocalReferenceNumber => base.LOCALREFERENCENUMBER;
		public ZString Security => GetMappedTypeOfSecurity();
		public ZString UniqueConsignmentReference => MovementHeader?.BM_UniqueConsignmentReference ?? ZString.Empty;
		public ZString TIRCarnetNumber => MovementHeader?.TirCarnetNumber ?? string.Empty;
		public ZString ConsignorId => NctsHeader.Consignor?.GetEuIdentificationNumber() ?? ZString.Empty;
		public ZString ConsignorAddress => NctsHeader.Consignor.FormatOnThreeLines();
		public ZString ConsignorContact => NctsHeader.Consignor.FormatContactOnOneLine();
		public ZString ConsigneeId => NctsHeader.Consignee?.GetEuIdentificationNumber() ?? ZString.Empty;
		public ZString ConsigneeAddress => GetAddressCompanyName(NctsHeader.Consignee);
		public ZString HolderOfTheTransitProcedureId => NctsHeader.Principal?.GetEuIdentificationNumber() ?? ZString.Empty;
		public ZString HolderOfTheTransitProcedureAddress => NctsHeader.Principal.FormatOnThreeLines();
		public ZString HolderOfTheTransitProcedureContact => RepresentativeAddress.IsEmpty ? NctsHeader.Principal.FormatContactOnOneLine(true) : ZString.Empty;
		public ZString RepresentativeId => NctsHeader.MovementHeader?.Representative.GetEuIdentificationNumber() ?? ZString.Empty;
		public ZString RepresentativeAddress => GetAddressCompanyName(NctsHeader.MovementHeader?.Representative);
		public ZString RepresentativeContact => NctsHeader.MovementHeader?.Representative.FormatContactOnOneLine() ?? ZString.Empty;
		public ZString CarrierId => NctsHeader.MovementHeader?.Carrier?.GetEuIdentificationNumber() ?? ZString.Empty;
		public ZString CarrierAddress => GetAddressCompanyName(NctsHeader.MovementHeader.Carrier);
		public ZString CarrierContact => NctsHeader.MovementHeader.Carrier.FormatContactOnOneLine();
		public ZString PlaceOfLoading => NctsHeader.MovementHeader != null ? GetPlaceOfLoading() : ZString.Empty;
		public ZString PlaceOfUnloading => NctsHeader.MovementHeader != null ? GetPlaceOfUnloading() : ZString.Empty;
		public ZString ModeOfTransportAtTheBorder => NctsHeader.MovementHeader?.BM_ExportTransportMode ?? ZString.Empty;
		public ZString InlandModeOfTransport => NctsHeader.MovementHeader?.BM_InlandTransportMode ?? ZString.Empty;
		public ZString LocationOfGoods => GetLocationsOfGoods();
		public ZString LocationOfGoodsContactPerson => GetLocationOfGoodsContactPerson();
		public ZString DepartureTransportMeans => GetDepartureTransportMeans();
		public ZString Seal => GetSeal();
		public ZString AdditionalSupplyChain => GetAdditionalSupplyChain();
		public ZString ActiveBorderTransportMeans => GetActiveBorderTransportMeans();
		public ZString ConveyanceRefNo => GetConveyanceReferenceNumbers();
		public ZString TransportEquipment => GetTransportEquipment();
		public ZString ContainerIndicator => (Containers.FirstOrDefault(y => y.BC_Mode == "CNT") != null) ? EU.DocumentWrapperConstants.Delimiters.CheckedCheckBox : EU.DocumentWrapperConstants.Delimiters.UncheckedCheckBox;
		public ZString PreviousDocument => NctsHeader.PreviousDocuments.Select((item, index) =>	GetLineFromCusSupportingInfo(item, index + 1, needSecondRefNum: true)) is IEnumerable<string> lines ? Tools.ConcatenateWithLengthLimit(lines.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 200) : ZString.Empty;
		public ZString SupportingDocument => MovementHeader.SupportingDocuments.Select((item, index) =>	GetLineFromCusSupportingInfo(item, index + 1, needSecondRefNum: true)) is IEnumerable<string> lines ? Tools.ConcatenateWithLengthLimit(lines.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 200) : ZString.Empty;
		public ZString TransportDocument => NctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).Select((item, index) =>
			GetLineFromCusSupportingInfo(item, index + 1)) is IEnumerable<string> lines ? Tools.ConcatenateWithLengthLimit(lines.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 200) : ZString.Empty;
		public ZString AdditionalReference => NctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).Select((item, index) =>
			GetLineFromCusSupportingInfo(item, index + 1)) is IEnumerable<string> lines ? Tools.ConcatenateWithLengthLimit(lines.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 80) : ZString.Empty;
		public ZString AdditionalInfo => NctsHeader.AdditionalDocuments.Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).Select((item, index) =>
			GetLineFromAdditionalInfo(item, index + 1)) is IEnumerable<string> lines ? Tools.ConcatenateWithLengthLimit(lines.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 80) : ZString.Empty;
		public ZString TransportCharges => MovementHeader?.BM_MethodOfPayment ?? ZString.Empty;
		public ZString ReducedDatasetIndicator => (MovementHeader?.BM_ReducedDatasetIndicator ?? ZBool.False) ? EU.DocumentWrapperConstants.Delimiters.CheckedCheckBox : EU.DocumentWrapperConstants.Delimiters.UncheckedCheckBox;
		public new ZString Guarantees => GetGuarantees();
		public ZString Authorisations => MovementHeader?.CusAuthorizationUsages.Select((item, index) => $"{index + 1}: {item.CustomsCode} - {item.AGC_Number}") is IEnumerable<string> lines ? Tools.ConcatenateWithLengthLimit(lines.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 200) : ZString.Empty;
		public ZString CountryOfRoutingOfConsignment => GetCountryOfRoutingOfConsignment();
		public ZString CustomsOfficesOfTransit => MovementHeader?.TransitCustomsOfficeCodeList.Select((item, index) => $"{index + 1}/ {item.OfficeCode}") is IEnumerable<string> lines ? Tools.ConcatenateWithLengthLimit(lines.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 240) : ZString.Empty;
		public ZString CustomsOfficesOfExitForTransit => MovementHeader?.ExitForTransitCustomsOfficeCodeList.Select((item, index) => $"{index + 1}/ {item.OfficeCode}") is IEnumerable<string> lines ? Tools.ConcatenateWithLengthLimit(lines.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 240) : ZString.Empty;
		public ZString CustomsOfficeOfDeparture => MovementHeader?.DepartureCustomsOffice is ICustomsOffice customsOffice ? customsOffice.OfficeCode : ZString.Empty;
		public ZString CustomsOfficeOfDestination => MovementHeader?.DestinationCustomsOffice is ICustomsOffice customsOffice ? customsOffice.OfficeCode : ZString.Empty;
		public ZString CountryOfDispatch => NctsHeader.MovementHeader?.BM_RN_NKCountryOfDispatch ?? ZString.Empty;
		public ZString CountryOfDestination => NctsHeader.MovementHeader?.BM_RL_NKDestinationPort ?? ZString.Empty;
		public ZString BindingItinerary => NctsHeader.CountriesOfRouting.Any() || IsTypeOfSecurityEntExiBth(TypeOfSecurity) ? EU.DocumentWrapperConstants.Delimiters.CheckedCheckBox : EU.DocumentWrapperConstants.Delimiters.UncheckedCheckBox;
		public ZString TotalItems => GetTotalItemsCore();
		public ZString TotalPackages => GetTotalPackagesCore();
		public ZString TotalGrossMass => GetTotalGrossMassCore();
		public ZString DateLimit => MovementHeader?.BM_ExportDate.ToISO8601ShortDateString() ?? ZString.Empty;

		public DocBaseWrapperCollection<Phase5NctsTADItemWrapper> Lines => itemsLines ??= GetLinesCore();
		DocBaseWrapperCollection<Phase5NctsTADItemWrapper> itemsLines;

		protected virtual DocBaseWrapperCollection<Phase5NctsTADItemWrapper> GetLinesCore()
		{
			return new Phase5NctsTADItemWrapperCollection(NctsHeader, Factory);
		}

		protected override ZString GetLocalReferenceNumber() => MovementHeader?.BM_PaperlessInbondNum ?? ZString.Empty;

		ZString GetCountryOfRoutingOfConsignment()
		{
			var builder = new ZStringBuilder();
			foreach (var country in NctsHeader.CountriesOfRouting)
			{
				builder.Append(country.CY_Order +
					EU.DocumentWrapperConstants.Delimiters.Space +
					country.CY_Data);
			}
			return builder.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.SemiColonAndspace);
		}

		ZString GetGuarantees()
		{
			var resultList = new List<string>();
			var guarantees = NctsHeader.MovementHeader.Guarantees.Cast<NctsGuarantee>().OrderBy(g => g.PW_BondType).ThenBy(g => g.PW_BondNumber).ToArray();

			if (guarantees.Length > 0)
			{
				var maxBondNumberLength = guarantees.Max(g => g.PW_BondNumber.Length);

				var sequence = 0;
				foreach (var guarantee in guarantees)
				{
					var lineBuilder = new ZStringBuilder();
					lineBuilder.Append((++sequence).ToString());
					lineBuilder.Append(": ");
					lineBuilder.Append(guarantee.PW_BondType);
					lineBuilder.Append(DocumentWrapperConstants.Delimiters.Dash);
					lineBuilder.Append(guarantee.PW_BondNumber);
					if (guarantee.PW_BondAmount.NullIfZero().HasValue)
					{
						lineBuilder.Append(DocumentWrapperConstants.Delimiters.Comma);
						lineBuilder.Append(Tools.ConvertNumberToString(guarantee.PW_BondAmount.Round(2).Normalize()));
						lineBuilder.Append(DocumentWrapperConstants.Delimiters.Space);
						lineBuilder.Append(guarantee.PW_RX_NKCurrency);
					}
					resultList.Add(lineBuilder.ToString());
				}
			}

			return Tools.ConcatenateWithLengthLimit(resultList.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 200); 
		}

		public ZString GetTransportEquipment()
		{
			var stringsList = new List<string>();
			foreach (var container in Containers)
			{
				var goodsItemsOnContainer = GoodsItemsForContainer(container);
				stringsList.Add($"{container.BC_SequenceNumber}/{container.BC_ContainerNum}/{container.TotalSealCount}/{goodsItemsOnContainer}");
			}

			return Tools.ConcatenateWithLengthLimit(stringsList.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 200);
		}

		ZString GetActiveBorderTransportMeans()
		{
			var stringsList = new List<string>();
			var sequenceNumber = 1;
			if (!MovementHeader.BM_CustomsOfficeAtBorder.IsEmpty)
			{
				ZStringBuilder sb = new ZStringBuilder();
				sb.Append($"{sequenceNumber++}")
				.Append(MovementHeader.BM_CustomsOfficeAtBorder)
				.AppendIfNotEmpty(MovementHeader.BM_ActiveBorderIdentificationType)
				.AppendIfNotEmpty(MovementHeader.BM_TOLCarrierID)
				.AppendIfNotEmpty(MovementHeader.BM_RN_NKTOLCarrierNationality);

				stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(EU.DocumentWrapperConstants.Delimiters.Comma));
			}

			foreach (var transport in MovementHeader.AdditionalTransportAtBorderList)
			{
				ZStringBuilder sb = new ZStringBuilder();
				sb.Append($"{sequenceNumber++}")
				.AppendIfNotEmpty(transport.TPM_CustomsOffice)
				.AppendIfNotEmpty(transport.TPM_TypeOfIdentification)
				.AppendIfNotEmpty(transport.TPM_IdentificationNumber)
				.AppendIfNotEmpty(transport.TPM_RN_NKTransportNationality);

				stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(EU.DocumentWrapperConstants.Delimiters.Comma));
			}

			return Tools.ConcatenateWithLengthLimit(stringsList.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 40);
		}

		ZString GetConveyanceReferenceNumbers()
		{
			var stringsList = new List<string>();
			if (!MovementHeader.BM_CustomsOfficeAtBorder.IsEmpty)
			{
				stringsList.Add(MovementHeader.BM_ConveyanceNumber);
			}

			foreach (var transport in MovementHeader.AdditionalTransportAtBorderList)
			{
				stringsList.Add(transport.TPM_ReferenceNumber);
			}

			return Tools.ConcatenateWithLengthLimit(stringsList.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 40);
		}

		ZString GetAdditionalSupplyChain()
		{
			var stringsList = new List<string>();
			var cusSupplyChainActors = NctsHeader.MovementHeader.CusSupplyChainActors.OrderBy(c => c.CFR_Code).ThenBy(c => c.CFR_Reference).ToArray();

			foreach (var chain in cusSupplyChainActors)
			{
				stringsList.Add(($"{chain.CFR_Code} {chain.CFR_Reference}"));
			}

			return Tools.ConcatenateWithLengthLimit(stringsList.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 60);
		}

		ZString GetSeal()
		{
			var stringsList = new List<string>();

			foreach (var container in Containers)
			{
				var seals = new ZString[] { container.Seal1, container.Seal2 }.Concat(container.AdditionalSeals.Select(a => a.BK_SealNumber)).ToArray();
				var line = $"{container.BC_SequenceNumber}/{string.Join(DocumentWrapperConstants.Delimiters.Comma, seals.Where(y => !y.IsEmpty))}";
				stringsList.Add(line);
			}

			return Tools.ConcatenateWithLengthLimit(stringsList.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 200);
		}

		ZString GetDepartureTransportMeans()
		{
			var stringsList = new List<string>();
			if (!MovementHeader.TransportTypeAtDeparture.IsEmpty || !MovementHeader.TransportAtDeparture.IsEmpty)
			{
				ZStringBuilder sb = new ZStringBuilder();
				var sequenceNumber = 1;

				sb.Append($"{sequenceNumber++}")
				.Append(MovementHeader.TransportTypeAtDeparture)
				.AppendIfNotEmpty(MovementHeader.TransportAtDeparture)
				.AppendIfNotEmpty(MovementHeader.TransportCountryAtDeparture);

				stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(EU.DocumentWrapperConstants.Delimiters.Comma));

				switch (MovementHeader.BM_InlandTransportMode)
				{
					case ModeOfTransportList.Codes._2_RailTransport:
						AddAdditionalWagons();
						break;
					case ModeOfTransportList.Codes._3_RoadTransport:
						AddTrailers();
						break;
					default:
						break;
				}

				void AddAdditionalWagons()
				{
					foreach (var wagon in MovementHeader.AdditionalWagons)
					{
						ZStringBuilder sb = new ZStringBuilder();
						sb.Append($"{sequenceNumber++}")
						.Append(NctsTransportTypeOfIdList.Codes._20)
						.AppendIfNotEmpty(wagon.WagonNumber)
						.AppendIfNotEmpty(wagon.WagonNationality);

						stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(EU.DocumentWrapperConstants.Delimiters.Comma));
					}
				}

				void AddTrailers()
				{
					if(!MovementHeader.Trailer1IDAtDeparture.IsEmpty)
					{
						ZStringBuilder sb = new ZStringBuilder();
						sb.Append($"{sequenceNumber++}")
						.Append(NctsTransportTypeOfIdList.Codes._31)
						.Append(MovementHeader.Trailer1IDAtDeparture)
						.AppendIfNotEmpty(MovementHeader.Trailer1NationalityAtDeparture);

						stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(EU.DocumentWrapperConstants.Delimiters.Comma));
					}

					if (!MovementHeader.Trailer2IDAtDeparture.IsEmpty)
					{
						ZStringBuilder sb = new ZStringBuilder();
						sb.Append($"{sequenceNumber++}")
						.Append(NctsTransportTypeOfIdList.Codes._31)
						.Append(MovementHeader.Trailer2IDAtDeparture)
						.AppendIfNotEmpty(MovementHeader.Trailer2NationalityAtDeparture);

						stringsList.Add(sb.ToStringWithDelimiterBetweenAppends(EU.DocumentWrapperConstants.Delimiters.Comma));
					}
				}
			}

			return Tools.ConcatenateWithLengthLimit(stringsList.ToArray(), DocumentWrapperConstants.Delimiters.SemiColonAndspace, 60);
		}

		ZString GetLocationsOfGoods()
		{
			var builder = new ZStringBuilder();

			if (CusGoodsLocation != null)
			{
				builder.Append($"{CusGoodsLocation.CGL_Type} - {CusGoodsLocation.CGL_Qualifier}");
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

		public ZString GetLocationOfGoodsContactPerson()
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

		ZString GetPlaceOfLoading()
		{
			var builder = new ZStringBuilder();
			var code = MovementHeader.BM_PortOfPresentationCode;
			if (code.Length > 2)
			{
				builder.Append(code);
			}
			if (code.Length >= 2)
			{
				builder.Append(code.Substring(0,2));
			}
			builder.AppendIfNotEmpty(MovementHeader.BM_PlaceOfLoading);
			return builder.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma);
		}

		public ZString GetPlaceOfUnloading()
		{
			var builder = new ZStringBuilder();
			var code = MovementHeader.BM_ForeignDestPortKCode;
			if (code.Length > 2)
			{
				builder.Append(code);
			}
			if (code.Length >= 2)
			{
				builder.Append(code.Substring(0,2));
			}
			builder.AppendIfNotEmpty(MovementHeader.BM_PlaceOfUnloading);
			return builder.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma);
		}

		ZString GetAddressCompanyName(JobDocAddress address)
		{
			if (address == null)
			{
				return ZString.Empty;
			}
			return address.E2_AddressOverride ? address.E2_CompanyName : address.Organisation?.OH_FullName ?? ZString.Empty;
		}

		ZString GetMappedTypeOfSecurity()
		{
			switch (TypeOfSecurity)
			{
				case NON:
					return "0";
				case ENT:
					return "1";
				case EXI:
					return "2";
				case BTH:
					return "3";
				default:
					return ZString.Empty;
			}
		}

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

		string GetLineFromCusSupportingInfo(AutoCusSupportingInfo info, int index, bool needSecondRefNum = false)
		{
			var sb = new ZStringBuilder();
			sb.Append(info.CSI_Code);
			sb.Append(info.CSI_ReferenceNumber);
			if (needSecondRefNum)
			{
				sb.Append(info.CSI_ReferenceNumber2);
			}
			return $"{index}: {sb.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma)}";
		}

		string GetLineFromAdditionalInfo(AutoCusSupportingInfo info, int index)
		{
			var sb = new ZStringBuilder();
			sb.Append(info.CSI_Code);
			sb.Append(info.CSI_Description);
			return $"{index}: {sb.ToStringWithDelimiterBetweenAppends(DocumentWrapperConstants.Delimiters.Comma)}";
		}

		CusGoodsLocation CusGoodsLocation => Factory.GetCached(ref cusGoodsLocationCache, () => NctsHeader.MovementHeader?.GoodsLocation);
		CachedProperty<CusGoodsLocation> cusGoodsLocationCache;

		NctsDepartureHeaderContainer[] Containers => containersCache
			??= NctsHeader.MovementHeader?.Header?.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>()
				.Where(c => !c.BC_ContainerNum.IsEmpty)
				.OrderBy(c => c.BC_SequenceNumber).ToArray()
				?? Array.Empty<NctsDepartureHeaderContainer>();
		NctsDepartureHeaderContainer[] containersCache;

		ZString TypeOfSecurity => MovementHeader?.BM_TypeOfSecurity ?? ZString.Empty;

		ZString GetTotalItemsCore() => Tools.ConvertNumberToString(NctsHeader?.TotalNumberOfItems);
		ZString GetTotalPackagesCore() => Tools.ConvertNumberToString(NctsHeader?.TotalNumberOfPackages);
		ZString GetTotalGrossMassCore() => Tools.ConvertNumberToString(new ZDecimal(Utilities.Round(MovementHeader.BM_GrossWeight, 6)));

		const string NON = "NON";
		const string ENT = "ENT";
		const string EXI = "EXI";
		const string BTH = "BTH";
	}
}
