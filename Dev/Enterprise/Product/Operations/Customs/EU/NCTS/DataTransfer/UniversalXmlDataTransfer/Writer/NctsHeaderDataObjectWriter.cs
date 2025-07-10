using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4
{
	public class NctsHeaderDataObjectWriter : NctsHeaderCommonDataObjectWriter
	{
		public NctsHeaderDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected sealed override void PopulateDataObjectCore(NctsHeader headerBO, Shipment headerData)
		{
			headerData.MessageSubType = new CodeDescriptionPair() { Code = headerBO.BH_HeaderType };
			headerData.SetPackingLineCollection(() => new DataObjectList<PackingLine>() { Content = CollectionContent.Complete });
			headerData.SetAddInfoCollection(() => new List<UniversalDataBuss.DataObjects.Universal.AddInfo>());
			headerData.CommercialInfo = new UniversalCustoms.CommercialInfo();
			headerData.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>();
			PopulateCustomsProfileIdentifier(headerBO, headerData);
			PopulateCustomsReferences(headerBO, headerData);
			PopulateCarrier(headerBO, headerData);
			PopulateDates(headerBO, headerData);
			headerData.TotalNoOfPacks = headerBO.MovementHeader?.TotalNumberOfPackages.ToZInt();
			headerData.TotalWeight = headerBO.MovementHeader?.TotalGrossMassInKilograms;
			headerData.TotalWeightUnit = new UnitOfWeight() { Code = Weight.Kilograms, Description = Weight.GetDescription(Weight.Kilograms, PluralState.Plural) };

			headerData.SetAddInfoGroupCollection(() => GetNctsHeaderCusAddInfos(headerBO, helper)); // Departure Results of Control, Arrival En-route Events, Arrival En-route Incidents

			ProcessCollection(headerBO.MovementHeaders, GetNewMoveHeaderDataObjectWriter(headerData));
			headerData.SetGuaranteeCollection(() => ProcessCollection(headerBO.Guarantees, new EU.DataTransfer.Universal.GuaranteeLineDataObjectWriter(writeManager, helper)));
			PopulateCountryData(headerBO, headerData);
			if (headerData.AddInfoCollection?.Count == 0)
			{
				headerData.SetAddInfoCollection(() => null);
			}
		}

		protected virtual NctsMoveHeaderDataObjectWriter GetNewMoveHeaderDataObjectWriter(Shipment headerData) => new NctsMoveHeaderDataObjectWriter(writeManager, helper, headerData);

		protected virtual void PopulateCountryData(NctsHeader headerBO, Shipment headerData)
		{
		}

		List<UniversalCustoms.AddInfoGroup> GetNctsHeaderCusAddInfos(NctsHeader headerBO, UniversalDataObjectWriterHelper writerHelper)
		{
			return AddInfoGroupCollectionCreator.CreateCollection(writerHelper, headerBO, writeManager);
		}

		static void PopulateCustomsReferences(NctsHeader headerBO, Shipment headerData)
		{
			headerData.SetCustomsReferenceCollection(() =>
			{
				var list = new List<UniversalCustoms.CustomsReference>();
				AddCustomsReferenceToList(DataObjectWriterConstants.Header.AddInfo.CustomReferenceType, Res.GetString("1D02A50A-5127-41AB-BA98-B47E19B7214B", "Commercial Reference Number"), headerBO.MovementHeader?.BM_AdditionalText ?? ZString.Empty, list);
				list.AddRange(headerBO.CustomsOffices.Cast<NctsEuOfficeCode>()
				.Select(officeCode => new UniversalCustoms.CustomsReference
				{
					Type = ListHelper.GetWithDescription<CodeDescriptionPair>(officeCode.CY_Type, headerBO.Factory.GetCachedValue<EU.Business.CusCodeDataTypeList>()),
					SubType = new CodeDescriptionPair35Char
					{
						Code = officeCode.CY_Code,
						Description = officeCode.Lookups.CY_CodeList.GetDescriptionFromCode(officeCode.CY_Code)
					},
					Reference = officeCode.CY_Data,
					IsOverridden = officeCode.CY_IsOverridden,
					Order = officeCode.CY_Order,
					DateCollection = new List<Date>
					{
						new Date
						{
							Type = DateType.DateAtOffice,
							Value = officeCode.CY_Date
						}
					},
					ReferencedEntityDescription = officeCode.CY_OfficeDescription
				}).ToList());

				return list;
			});
		}

		static void AddCustomsReferenceToList(ZString typeCode, ZString typeDescription, ZString referenceValue, List<UniversalCustoms.CustomsReference> list)
		{
			list.Add(new UniversalCustoms.CustomsReference()
			{
				Type = new CodeDescriptionPair() { Code = typeCode, Description = typeDescription },
				Reference = referenceValue
			});
		}

		void PopulateDates(NctsHeader headerBO, Shipment headerData)
		{
			var datetimeNow = ZDateTime.Now;

			SetDateOnBusinessObjectIfSendingMessage(headerBO, datetimeNow);

			headerData.SetDateCollection(() =>
			{
				var dates = new List<Date>();
				dates.Add(DateType.Departure, ZBool.False, headerBO.MovementHeader?.BM_EntryDate ?? ZDateTime.Empty);
				if (headerBO.MessageFunctionCode == new NctsMessageFunctionSet.DeclarationCancellationRequestMessage("", "").Code)
				{
					dates.Add(DateType.CutOffDate, ZBool.False, datetimeNow);
				}
				var arrivalMovementHeader = headerBO.ArrivalMovementHeader;
				if (arrivalMovementHeader != null)
				{
					dates.Add(DateType.Arrival, ZBool.False, arrivalMovementHeader.BM_EntryDate);
					dates.Add(DateType.Unpack, ZBool.False, headerBO.UnloadingRemark.G9_UnloadingDate);
				}
				return dates;
			});
		}

		void SetDateOnBusinessObjectIfSendingMessage(NctsHeader headerBO, ZDateTime datetimeNow)
		{
			if (headerBO.MessageFunctionCode == new NctsMessageFunctionSet.DeclarationDataMessage().Code)
			{
				headerBO.MovementHeader.BM_EntryDate = datetimeNow;
			}
			else if (headerBO.MessageFunctionCode == new NctsMessageFunctionSet.ArrivalNotificationMessage().Code)
			{
				var arrivalMovementHeader = headerBO.ArrivalMovementHeader;
				if (arrivalMovementHeader != null)
				{
					arrivalMovementHeader.BM_EntryDate = datetimeNow;
				}
			}
			else if (headerBO.MessageFunctionCode == new NctsMessageFunctionSet.UnloadingRemarksMessage().Code)
			{
				headerBO.UnloadingRemark.G9_UnloadingDate = datetimeNow;
			}
		}

		void PopulateCarrier(NctsHeader headerBO, Shipment headerData)
		{
			if (headerBO.CarrierOrgAddress != null)
			{
				var carrierOrgAddress = headerBO.CarrierOrgAddress;

				var orgAddress = new OrganizationAddress(writeManager.WriterStrategy)
				{
					AddressType = nameof(DocAddressType.Carrier),
					Address1 = carrierOrgAddress.Address1,
					City = carrierOrgAddress.City,
					CompanyName = carrierOrgAddress.CompanyName,
					Country = new Country() { Code = carrierOrgAddress.Country.Code },
					OrganizationCode = carrierOrgAddress.Header.OH_Code,
					Postcode = carrierOrgAddress.Postcode
				};

				var eoriType = new RegistrationNumberType() { Code = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Description = Res.GetString("2C35448A-EB85-4A08-917B-69FEE12522F2", "EORI code (to be sent verbatim)") };
				var tirType = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, Description = Res.GetString("E57B7D18-6E98-41F0-AB0D-81AA0A73FB71", "Transports Internationaux Routiers") };

				var eori = carrierOrgAddress.GetEuIdentificationNumber();
				var tir = EuEoriProviderAndValidator.GetRegoCodeOfThisOrg(carrierOrgAddress.Header, "TIR");

				var country = new Country() { Code = Core.Constants.CountryCodes.UnitedKingdom, Name = helper.GetCountryDescriptionFromCode(Core.Constants.CountryCodes.UnitedKingdom) };

				orgAddress.SetRegistrationNumberCollection(() =>
				{
					var collection = new List<UniversalDataBuss.DataObjects.Universal.RegistrationNumber>();
					collection.Add(new UniversalDataBuss.DataObjects.Universal.RegistrationNumber() { Type = eoriType, Value = eori, CountryOfIssue = country });
					collection.Add(new UniversalDataBuss.DataObjects.Universal.RegistrationNumber() { Type = tirType, Value = tir, CountryOfIssue = country });
					return collection;
				});

				if (headerData.OrganizationAddressCollection != null || headerData.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()))
				{
					headerData.OrganizationAddressCollection.Add(orgAddress);
				}
			}
		}

		void PopulateCustomsProfileIdentifier(NctsHeader nctsHeader, Shipment shipment)
		{
			shipment.CustomsProfileIdentifier = new ValueTypePair()
			{
				Type = CustomsProfileIdentifierType.ToString(),
				Value = nctsHeader.BH_CustomsProfile,
			};
		}

		protected override CodeDescriptionPair GetMessagingApplicationCode(NctsHeader headerBO) => new CodeDescriptionPair() { Code = CusInBondApplicationCodeList.Codes.NCTS4, Description = CusInBondApplicationCodeList.Descriptions.NCTS4 };

		protected virtual CustomsProfileType CustomsProfileIdentifierType => CustomsProfileType.UserName;
	}
}
