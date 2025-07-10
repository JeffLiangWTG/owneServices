using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class DOABuilder
	{
		public DOABuilder(NctsHeader header)
		{
			this.nctsHeader = Argument.NotNull(header, nameof(header));
			context = ObjectFactory.Get<IContext>(nameof(IContext), nctsHeader.Factory);
		}

		readonly NctsHeader nctsHeader;

		readonly IContext context;

		public DOADataObject Build()
		{
			var doa = new DOADataObject();

			PopulateAddresses(doa);
			PopulateDeclaration(doa);

			AddValidationsMandatoryFields(doa);
			doa.ValidateAllIncludingChildren();

			return doa;
		}

		void PopulateAddresses(DOADataObject doa)
		{
			PopulatePortSystem(doa);
			PopulateCurrentUser(doa);
		}

		void PopulatePortSystem(DOADataObject doa)
		{
			var port = (nctsHeader.IsDepartureAndArrivalMovement || nctsHeader.IsDepartureMovement) ? nctsHeader.BH_RL_NKImportLoadPort : ZString.Empty;
			doa.PortSystem = PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent
								.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
								.OfType<CommunitySystemCodesOfForwarderAndAgent>()
								.FirstOrDefault(x => x.Port == port)?.PCS ?? ZString.Empty;
			doa.SICCodeType = doa.PortSystem == PortSystemCodeList.Codes.MGI ? OrgCusCode.FranceCodeTypes.CI5 : OrgCusCode.FranceCodeTypes.SOA;
		}

		void PopulateCurrentUser(DOADataObject doa)
		{
			doa.CurrentUser = AddressBuilder.CreateForCurrentUser(context);
			var sendingParty = GetSendingParty();
			if (sendingParty != null)
			{
				doa.SendingPartySICCode = GetDefaultSendingPartySICCode();
				doa.SendingPartyID = GetRegistrationNumber(sendingParty, doa.SICCodeType);
			}

			var cCSCode = GetCCSCode();
			doa.RecipientID = cCSCode;
			doa.RecipientSICCode = cCSCode;
		}

		ZString GetCCSCode()
		{
			return ZZRefCusMapCombined.MapCW1CodeToCustomsCode(nctsHeader.Factory, Core.Constants.CountryCodes.France, RefCusMapTypeList.Codes.FRCCS, PortOfDispatch, ZDateTime.Today);
		}

		ZString GetDefaultSendingPartySICCode()
		{
			return PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent
								.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)
								.OfType<CommunitySystemCodesOfForwarderAndAgent>()
								.FirstOrDefault(x => x.Port == PortOfDispatch)?.ForwarderCode ?? ZString.Empty;
		}

		ZString PortOfDispatch => nctsHeader.PortOfDispatch;

		void PopulateDeclaration(DOADataObject doa)
		{
			PopulateReferenceNumbers(doa);
			PopulateTypeAndStatus(doa);
			PopulateOffices(doa);
			PopulateGoodsItems(doa);
			PopulatePortDues(doa);
			PopulateOtherFields(doa);
		}

		void PopulateReferenceNumbers(DOADataObject doa)
		{
			doa.JobNumber = nctsHeader.JobNumber;
			doa.DeclarationNumber = nctsHeader.MovementReferenceNumber;
			doa.DeclarationReference = nctsHeader.LocalReferenceNumber;

			var containers = nctsHeader.DepartureHeaderContainers.Cast<EU.NCTS.Business.NctsDepartureHeaderContainer>().Where(x => !x.BC_ContainerNum.IsEmpty).Select(x => x.BC_ContainerNum).ToList();
			if (containers.Count == 1)
			{
				doa.EquipmentRef = containers.First();
			}
			else if (containers.Count > 1)
			{
				doa.Containers = containers.Select(x => new Container(x)).ToList();
			}
		}

		void PopulateTypeAndStatus(DOADataObject doa)
		{
			if (nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				doa.DeclarationType = movementHeader.BM_InBondEntryType;
				if ((nctsHeader.IsPhase4 && movementHeader.BM_CustomsStatus == NctsTransitStatusList.Codes.GoodsReleasedForTransitAtDeparture) || movementHeader.BM_CustomsStatus == NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit)
				{
					doa.DeclarationStatus = bAEConstant;
				}
			}
		}

		void PopulateOffices(DOADataObject doa)
		{
			var customsOffices = nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.CustomsOffices : nctsHeader.CustomsOffices;
			var departureOffice = customsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			var destinationOffice = customsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);

			var departureOfficeCodes = departureOffice?.Lookups.OfficeCodeList ?? EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(context.Factory, EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			var destinationOfficeCodes = destinationOffice?.Lookups.OfficeCodeList ?? EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(context.Factory, EuOfficeCodesTypes.Codes.OfficeOfDestination);

			doa.CustomsOfficeCodeOfDeparture = new CodeDescription(departureOfficeCodes)
			{
				Code = departureOffice?.CY_Data ?? ZString.Empty
			};

			doa.CustomsOfficeCodeOfDestination = new CodeDescription(destinationOfficeCodes)
			{
				Code = destinationOffice?.CY_Data ?? ZString.Empty
			};
		}

		void PopulateGoodsItems(DOADataObject doa)
		{
			PopulatePackages(doa);
			PopulateWeight(doa);
			PopulateTariffs(doa);
		}

		void PopulatePackages(DOADataObject doa)
		{
			if (nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				var goodsItems = nctsHeader.IsPhase4 ? movementHeader.GoodsItems : nctsHeader.Bills.SelectMany(x => x.GoodsItems);
				var packages = goodsItems.SelectMany(x => x.Packages).Cast<NctsPackage>();
				doa.TotalNumberOfPacks = packages.Sum(y => (int)y.B5_UnitCount);
				var package = packages.FirstOrDefault();
				if (package != null)
				{
					var unitType = !nctsHeader.IsPhase4 && packages.Any(p => p.B5_UnitType != package.B5_UnitType) ? (ZString)"MLT" : package.B5_UnitType;
					doa.PackageType = new CodeDescription(package.Lookups.UnitTypeList)
					{
						Code = unitType
					};
				}
			}
		}

		void PopulateWeight(DOADataObject doa)
		{
			if (nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				doa.TotalGrossWeightInKilograms = nctsHeader.IsPhase4 ? movementHeader.GoodsItems.Sum(x => x.GrossMassInKilograms) : new ZWeight(movementHeader.BM_GrossWeight, movementHeader.BM_GrossWeightUQ).InKilogramsSafe;
				var goodsItems = nctsHeader.IsPhase4 ? movementHeader.GoodsItems : nctsHeader.Bills.SelectMany(x => x.GoodsItems);
				doa.TotalNetWeightInKilograms = goodsItems.Sum(x => x.NetMassInKilograms);
			}
		}

		void PopulateTariffs(DOADataObject doa)
		{
			if (nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				var tariffs = new List<ICodeDescription>();

				var goodsItems = nctsHeader.IsPhase4 ? movementHeader.GoodsItems : nctsHeader.Bills.SelectMany(x => x.GoodsItems);
				foreach (var goodsItem in goodsItems)
				{
					if (!goodsItem.BY_HarmonisedTariff.IsEmpty)
					{
						tariffs.Add(new CodeDescription(goodsItem.Lookups.Tariffs)
						{
							Code = goodsItem.BY_HarmonisedTariff
						});
					}
				}
				doa.Tariffs = tariffs;
			}
		}

		void PopulatePortDues(DOADataObject doa)
		{
			doa.PortDuesCurrency = new CodeDescription(new RefCurrencyCollection(context.Factory))
			{
				Code = Core.Constants.CurrencyCodes.EuropeanUnion
			};
			doa.Port = nctsHeader.BH_RL_NKImportLoadPort;
			doa.PortDuesAmount = PortDuesAmount;
		}

		ZDecimal PortDuesAmount
		{
			get
			{
				var amount = 0m;

				if (nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader)
				{
					var goodsItems = nctsHeader.IsPhase4 ? movementHeader.GoodsItems : nctsHeader.Bills.SelectMany(x => x.GoodsItems);
					amount = goodsItems.Sum(x => x.HarbourTaxAmountInLocalCurrency);
				}

				return amount;
			}
		}

		void PopulateOtherFields(DOADataObject doa)
		{
			if (nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				doa.IsPrelodged = movementHeader.PreLodgedForAgreedLocationOfGoodsCode;
				doa.HasSeal = movementHeader.BM_SealQty > 0;
				doa.CommonAccessRef = CommonAccessRef;
			}
		}

		ZString CommonAccessRef
		{
			get
			{
				var result = ZString.Empty;

				if (nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader)
				{
					if (nctsHeader.IsPhase4)
					{
						result = movementHeader.GoodsItems.Cast<NctsDepartureCargoDesc>().SelectMany(x => x.PreviousDocuments.Cast<NctsPreviousDocument>()).FirstOrDefault(x => x.CSI_Code == PreviousDocumentCodeList.Codes.ZZZ)?.CSI_ReferenceNumber ?? ZString.Empty;
					}
					else
					{
						result = movementHeader.SupportingDocuments.FirstOrDefault(x => x.CSI_Code == FRConstants.CAED.CommonAccessDocumentCode)?.CSI_ReferenceNumber ?? ZString.Empty;
					}
				}

				return result;
			}
		}

		OrgAddress GetSendingParty()
		{
			return GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
		}

		ZString GetRegistrationNumber(OrgAddress address, string codeType)
		{
			var number = address.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.France);
			if (number.IsEmpty)
			{
				number = address.Header.CustomsCodes.GetCustomsRegNo(codeType, Core.Constants.CountryCodes.France);
			}
			return number;
		}

		void AddValidationsMandatoryFields(DOADataObject doa)
		{
			doa.SendingPartyIDInfo.AddMessageErrorIfEmpty(Res.GetString("8D984FBD-DF71-43EA-A142-DF7979BC9DBC", "Sender's ID is required."));

			doa.SendingPartySICCodeInfo.AddError(
				() => doa.SendingPartySICCode.IsEmpty && GetDefaultSendingPartySICCode().IsEmpty,
				Res.GetString("4B6C0D24-9D04-4D93-8AF5-A11A7301DE88", "There is no Forwarder code available in Registry \"Port Community System Codes of Forwarder and Agent\"."));

			doa.SendingPartySICCodeInfo.AddMessageErrorIfEmpty(Res.GetString("0CE8347A-C18E-4FDB-B626-F3852C3E9D74", "Sender's SIC code is required."));

			doa.RecipientIDInfo.AddMessageErrorIfEmpty(Res.GetString("C2B60D80-C68B-4723-ABC7-3B1E49C64EBB", "Recipient's ID is required."));
			doa.RecipientSICCodeInfo.AddMessageErrorIfEmpty(Res.GetString("A7A16486-2722-4D84-9483-797706B55607", "Recipient's SIC code is required."));

			doa.CTOPartyIDInfo.AddMessageErrorIfEmpty(Res.GetString("B2B00F76-0759-48B6-BC73-A51E90BA4D9C", "CTO address's ID is required."));
			doa.CTOPartySICCodeInfo.AddMessageErrorIfEmpty(Res.GetString("37B2B09A-69DE-42E4-9A20-BD12B9895C99", "CTO address's SIC code is required."));

			if (nctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader && movementHeader.BM_SealType == EU.NCTS.Business.SealTypeList.Codes.ContainerSeal)
			{
				doa.ContainerMessageInfo.AddMessageError(() => doa.EquipmentRef.IsEmpty && (!doa.Containers?.Any() ?? true), Res.GetString("6B23688F-083E-47DF-9CE4-81F92203E1F7", "At least one container is required."));
			}

			doa.DeclarationTypeInfo.AddMessageErrorIfEmpty(Res.GetString("CC0CB1F9-09B0-46DA-B29B-27914B5BCC55", "Declaration type is required."));

			doa.JobNumberInfo.AddMessageErrorIfEmpty(Res.GetString("F697AF3B-BBAA-46AB-B2BC-1A6FC9801797", "Job number is required."));

			doa.DeclarationNumberInfo.AddMessageErrorIfEmpty(Res.GetString("4E8C599E-1E9F-4F58-9DFA-7114BC694736", "Declaration number is required."));

			doa.DeclarationReferenceInfo.AddMessageErrorIfEmpty(Res.GetString("EFF992C0-4050-4035-8FFD-CD287DD964A7", "Declaration reference is required."));

			doa.TotalNumberOfPacksInfo.AddMessageError(
				() => doa.TotalNumberOfPacks < 1,
				Res.GetString("34B713F0-CDF8-43F7-B257-193C6EA3003E", "At least one pack should be entered."));

			doa.TotalGrossWeightInKilogramsInfo.AddMessageError(
				() => doa.TotalGrossWeightInKilograms.IsEmpty,
				Res.GetString("A0C1C749-F619-4665-BC11-1E98457D117C", "Gross weight cannot be zero."));
		}

		readonly ZString bAEConstant = new ZString("BAE");
	}
}
