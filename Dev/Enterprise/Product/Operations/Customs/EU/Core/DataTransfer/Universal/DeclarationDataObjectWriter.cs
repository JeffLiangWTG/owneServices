using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using BaseAddInfo = Enterprise.Customs.Business.BaseAddInfo;
using GoodsLocationQualifierCodes = Enterprise.Customs.Business.CusGoodsLocationQualifierList.Codes;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class DeclarationDataObjectWriter : UniversalShipment.DeclarationDataObjectWriter
	{
		public DeclarationDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		public static List<AddInfoGroup> CreateOldSupportingInfoDataIfNeeded(List<AddInfoGroup> result, Integration.Customs.ICusSupportingInfoTypeSupporter supporter, UniversalDataObjectWriterHelper helper)
		{
			if (supporter != null && Registry.EUCustomsDataRegistry.Instance.ExportOldSuportingInfoSchemaInUniversalXML.Value)
			{
				result = result ?? new List<AddInfoGroup>();
				result.AddRange(CreateOldAdditionalInfoIfNeeded(supporter));
				result.AddRange(CreateOldPreviousDocumentIfNeeded(supporter));
				result.AddRange(CreateOldSupportingDocumentIfNeeded(supporter, helper));
			}
			return result;
		}

		protected new UniversalDataObjectWriterHelper helper => (UniversalDataObjectWriterHelper)base.helper;

		protected override UniversalShipment.UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
		{
			var factory = declarationBO.Factory;
			var countryCode = declarationBO.CountryCode;
			var result = GetUniversalDataObjectWriterHelper(factory, countryCode);
			if (declarationBO is JobDeclaration declaration)
			{
				var configuration = declaration.Configuration;
				var cacheKey = UniversalDataObjectWriterHelper.GetCusSupportingInfoCSI_TypeListCacheKey(countryCode, CusEntryInstructionSchema.Constants.Prefix, string.Empty);
				var cachedList = result.GetCusSupportingInfoCSI_TypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty);
				var entryInstructionList = new CodeDescriptionPairList();
				if (cachedList != null)
				{
					entryInstructionList.AddRange(cachedList);
				}
				var instructionConfiguration = configuration.InstructionConfiguration;
				if (entryInstructionList.ContainsCode(CusSupportingInfoTypeList.Codes.SupportingDocument))
				{
					if (!instructionConfiguration.SupportingDocumentsSupport(declaration))
					{
						entryInstructionList.RemoveCode(CusSupportingInfoTypeList.Codes.SupportingDocument);
					}
				}
				else if (instructionConfiguration.SupportingDocumentsSupport(declaration))
				{
					entryInstructionList.AddPair(CusSupportingInfoTypeList.Codes.SupportingDocument, CusSupportingInfoTypeList.Descriptions.SupportingDocument);
				}

				if (entryInstructionList.ContainsCode(CusSupportingInfoTypeList.Codes.PreviousDocument))
				{
					if (!instructionConfiguration.PreviousDocumentsSupport(declaration))
					{
						entryInstructionList.RemoveCode(CusSupportingInfoTypeList.Codes.PreviousDocument);
					}
				}
				else if (instructionConfiguration.PreviousDocumentsSupport(declaration))
				{
					entryInstructionList.AddPairIfNotExist(CusSupportingInfoTypeList.Codes.PreviousDocument, CusSupportingInfoTypeList.Descriptions.PreviousDocument);
				}

				if (entryInstructionList.ContainsCode(CusSupportingInfoTypeList.Codes.InstructionRequestedDocument))
				{
					if (!instructionConfiguration.RequestedDocumentsSupport(declaration))
					{
						entryInstructionList.RemoveCode(CusSupportingInfoTypeList.Codes.InstructionRequestedDocument);
					}
				}
				else if (instructionConfiguration.RequestedDocumentsSupport(declaration))
				{
					entryInstructionList.AddPairIfNotExist(CusSupportingInfoTypeList.Codes.InstructionRequestedDocument, CusSupportingInfoTypeList.Descriptions.InstructionRequestedDocument);
				}

				if (entryInstructionList.ContainsCode(CusSupportingInfoTypeList.Codes.AdditionalInfo))
				{
					if (!declaration.CustomsEntryInstructions.Cast<Business.Declaration.CusEntryInstruction>().Any(x => instructionConfiguration.AdditionalInfosSupport(declaration, x)))
					{
						entryInstructionList.RemoveCode(CusSupportingInfoTypeList.Codes.AdditionalInfo);
					}
				}
				else if (declaration.CustomsEntryInstructions.Cast<Business.Declaration.CusEntryInstruction>().Any(x => instructionConfiguration.AdditionalInfosSupport(declaration, x)))
				{
					entryInstructionList.AddPair(CusSupportingInfoTypeList.Codes.AdditionalInfo, CusSupportingInfoTypeList.Descriptions.AdditionalInfo);
				}

				if (cachedList == null && entryInstructionList.Count > 0)
				{
					factory.ClearCachedValue<ICodeDescriptionPairList>(cacheKey);
					cachedList = factory.GetCachedValue<ICodeDescriptionPairList>(cacheKey, () => entryInstructionList);
				}
				factory.ClearCachedValue<ZString[]>(UniversalDataObjectWriterHelper.GetCusSupportingInfoCSI_TypeListCacheKey(countryCode, CusEntryInstructionSchema.Constants.Prefix, string.Empty));
			}
			return result;
		}

		protected virtual UniversalDataObjectWriterHelper GetUniversalDataObjectWriterHelper(BusinessObjectFactory factory, ZString countryCode)
		{
			return new UniversalDataObjectWriterHelper(factory, countryCode);
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(JobDocAddressSchema.E2_ParentID, invoiceLinePK);
			yield return new FetchHint(CusLineTariffDetailSchema.BZ_ParentID, invoiceLinePK);
			yield return new FetchHint(CusAuthorizationUsageSchema.AGC_ParentID, invoiceLinePK);
			yield return new FetchHint(CusReferenceSchema.CFR_ParentID, invoiceLinePK);
		}

		protected override IEnumerable<IFetchHint> GetEntryInstructionRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetEntryInstructionRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var entryInstructionPK = row.GetValue(CusEntryInstructionSchema.PK);
			yield return new FetchHint(CusAuthorizationUsageSchema.AGC_ParentID, entryInstructionPK);
			yield return new FetchHint(CusReferenceSchema.CFR_ParentID, entryInstructionPK);
		}

		protected override UniversalShipment.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.Business.CusEntryHeader relatedEntry)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		protected override List<AddInfoGroup> GetInvoiceGroupAddInfoGroupCollection(BaseJobComInvoiceGroupHeader invoiceGroup)
		{
			return CreateOldSupportingInfoDataIfNeeded(base.GetInvoiceGroupAddInfoGroupCollection(invoiceGroup), invoiceGroup.JobDeclaration as Integration.Customs.ICusSupportingInfoTypeSupporter, helper);
		}

		protected override void MergeCountrySpecificRelatedData(ZBool isTopLevelContextCustomsDeclaration, BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			base.MergeCountrySpecificRelatedData(isTopLevelContextCustomsDeclaration, declarationBO, declarationData);

			var euDeclaration = (JobDeclaration)declarationBO;
			if (euDeclaration.Guarantees != null)
			{
				declarationData.SetGuaranteeCollection(() => ProcessCollection(euDeclaration.Guarantees, new GuaranteeLineDataObjectWriter(writeManager, helper)));
			}
		}

		protected override void PopulateCountrySpecificData(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			base.PopulateCountrySpecificData(declarationData, declarationBO);
			PopulateEquipments(declarationData, declarationBO);
			if (declarationBO is JobDeclaration declaration)
			{
				if (declaration.AllowUCC6PropertiesWithUCC5)
				{
					PopulateDeclarationInlandTransportMeansCollection(declaration, declarationData);
				}
				if (declaration.IsUCC6)
				{
					PopulateLocationOfGoodsCollection(declaration.GoodsLocation, declarationData);
				}
				declarationData.AgreedPlaceCode = declaration.EUD_AgreedPlaceCode;

				PopulateDV1Data(declarationData, declarationBO);
			}
		}

		void PopulateDV1Data(Shipment declarationData, BaseJobDeclaration declarationBo)
		{
			var euDeclaration = (JobDeclaration)declarationBo;
			var customsValueInformationCollection = new List<CustomsValueInformation>();

			foreach (var dv1Details in euDeclaration.DV1Details.Cast<CusDV1Detail>())
			{
				var customsValueInformation = new CustomsValueInformation(writeManager.WriterStrategy);
				customsValueInformation.Link = helper.AllocateDv1DetailsLink(dv1Details.PK);
				customsValueInformation.SetCustomsValueDetailCollection(() => new List<CustomsValueDetail>
				{
					new() { Type = Constants.CustomsValueTypes.Relationship, Code = dv1Details.DV1_Relationship },
					new() { Type = Constants.CustomsValueTypes.PriceInfluence, Code = dv1Details.DV1_PriceInfluence },
					new() { Type = Constants.CustomsValueTypes.RelationDetails, Details = dv1Details.DV1_RelationDetails },
					new() { Type = Constants.CustomsValueTypes.Restrictions, Code = dv1Details.DV1_Restrictions },
					new() { Type = Constants.CustomsValueTypes.Consideration, Code = dv1Details.DV1_Consideration },
					new() { Type = Constants.CustomsValueTypes.RestrictionConsiderationDetails, Details = dv1Details.DV1_RestrictionConsiderationDetails },
					new() { Type = Constants.CustomsValueTypes.RoyaltiesLicence, Code = dv1Details.DV1_RoyaltiesLicence, Details = dv1Details.DV1_RoyaltiesLicenceDetails },
					new() { Type = Constants.CustomsValueTypes.Resale, Code = dv1Details.DV1_Resale, Details = dv1Details.DV1_ResaleDetails },
					new() { Type = Constants.CustomsValueTypes.DecisionNumber, Details = dv1Details.DV1_CustomsDecisionNumber },
				});
				customsValueInformationCollection.Add(customsValueInformation);
			}

			if (customsValueInformationCollection.Count > 0)
			{
				declarationData.SetCustomsValueInformationCollection(() => customsValueInformationCollection);
			}
		}

		protected override void PopulateExtraOrganisation(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			base.PopulateExtraOrganisation(declarationBO, declarationData, keepExistingData);

			if (declarationBO is JobDeclaration declaration)
			{
				if (declaration.IsUCC6)
				{
					PopulateGoodsLocationAddress(declaration.GoodsLocation, declarationData);
				}
				if (declaration.UseDutyPayerAndDefermentPartyInUXML)
				{
					declarationData.AddOrgAddress(writeManager, declaration.DefermentPartyDocAddress, MasterFiles.Integration.DocAddressType.DefermentParty);
					declarationData.AddOrgAddress(writeManager, declaration.DutyPayer, MasterFiles.Integration.DocAddressType.DutyPayer);
				}
			}
		}

		void PopulateGoodsLocationAddress(Business.CusGoodsLocation goodsLocation, Shipment declarationData)
		{
			const MasterFiles.Integration.DocAddressType goodsLocationType = MasterFiles.Integration.DocAddressType.LocationOfGoods;
			var qualifier = goodsLocation.CGL_Qualifier;

			if (qualifier == GoodsLocationQualifierCodes.Address)
			{
				declarationData.AddOrgAddress(writeManager, goodsLocation.Address, goodsLocationType);
				return;
			}

			if (qualifier == GoodsLocationQualifierCodes.AuthorizationNumber)
			{
				declarationData.AddOrgAddress(writeManager, goodsLocation.Address.IdentificationHolder, goodsLocationType);
			}
		}

		void PopulateLocationOfGoodsCollection(Business.CusGoodsLocation goodsLocation, Shipment declarationData)
		{
			var locationOfGoodsCollection = declarationData.LocationOfGoodsCollection ?? new List<LocationOfGoods>();

			locationOfGoodsCollection.Add(new LocationOfGoods
			{
				Qualifier = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(goodsLocation.CGL_Qualifier, goodsLocation.Lookups.QualifierList),
				LocationType = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(goodsLocation.CGL_Type, goodsLocation.Lookups.TypeList),
				AuthorizationNumber = goodsLocation.Address.AuthorisationNumber,
				AdditionalIdentifier = goodsLocation.CGL_AdditionalIdentifier,
				Contact = GetContactFromGoodsLocationAddress(goodsLocation.Address),
			});

			declarationData.SetLocationOfGoodsCollection(() => locationOfGoodsCollection);
		}

		Contact GetContactFromGoodsLocationAddress(Business.CusGoodsLocationAddress address)
		{
			return new Contact
			{
				Name = address.E2_Contact,
				PhoneNumber = address.E2_Phone,
				Email = address.E2_Email,
			};
		}

		void PopulateDeclarationInlandTransportMeansCollection(JobDeclaration declaration, Shipment declarationData)
		{
			var transports = declarationData.TransportMeansCollection ?? new List<TransportMeans>();
			var declarationBOLookups = declaration.Lookups;
			transports.Add(new TransportMeans()
			{
				TransportType = TransportTypeCode.Inland,
				Order = 0,
				IdentificationNumber = declaration.JE_TransportIDInland,
				Nationality = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(declaration.JE_RN_NKTransportNationalityInland, declarationBOLookups.TransportNationalityInlands),
				TypeOfIdentification = GetCodeDescriptionPairForTypeOfIdentification(declaration.JE_TransportModeInland, declaration.JE_TransportMeans),
			});
			if (declaration.IsRoadInland)
			{
				transports.Add(new TransportMeans()
				{
					TransportType = TransportTypeCode.Inland,
					Order = 1,
					IdentificationNumber = declaration.JE_Trailer1RegNo,
					Nationality = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(declaration.JE_RN_NKTrailer1Nationality, declarationBOLookups.Trailer1Nationalities),
					TypeOfIdentification = new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, Description = TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer },
				});

				transports.Add(new TransportMeans()
				{
					TransportType = TransportTypeCode.Inland,
					Order = 2,
					IdentificationNumber = declaration.JE_Trailer2RegNo,
					Nationality = ListHelper.GetWithDescription<CodeDescriptionPair2Char>(declaration.JE_RN_NKTrailer2Nationality, declarationBOLookups.Trailer2Nationalities),
					TypeOfIdentification = new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, Description = TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer },
				});
			}
			declarationData.SetTransportMeansCollection(() => transports);
		}

		CodeDescriptionPair2Char GetCodeDescriptionPairForTypeOfIdentification(string modeOfTransportInland, string typeOfId)
		{
			switch (modeOfTransportInland)
			{
				case Core.Constants.TransportModes.Road:
					return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, Description = TransportMeansList.Descriptions.RegistrationNumberOfTheRoadVehicle };
				case Core.Constants.TransportModes.Air:
					return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.RegistrationNumberOfTheAircraft, Description = TransportMeansList.Descriptions.RegistrationNumberOfTheAircraft };
				case Core.Constants.TransportModes.FixedTransportInstallations:
				case Core.Constants.TransportModes.InlandWaterwayTransport:
				case Core.Constants.TransportModes.OwnPropulsion:
				case Core.Constants.TransportModes.Mail:
				case Core.Constants.TransportModes.Rail:
				case Core.Constants.TransportModes.Sea:
					return GetFromTypeOfId();
				default:
					return null;
			}

			CodeDescriptionPair2Char GetFromTypeOfId()
			{
				switch (typeOfId)
				{
					case TransportMeansList.Codes.ImoShipIdentificationNumber:
						return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.ImoShipIdentificationNumber, Description = TransportMeansList.Descriptions.ImoShipIdentificationNumber };
					case TransportMeansList.Codes.NameOfTheSeaGoingVessel:
						return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.NameOfTheSeaGoingVessel, Description = TransportMeansList.Descriptions.NameOfTheSeaGoingVessel };
					case TransportMeansList.Codes.WagonNumber:
						return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.WagonNumber, Description = TransportMeansList.Descriptions.WagonNumber };
					case TransportMeansList.Codes.TrainNumber:
						return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.TrainNumber, Description = TransportMeansList.Descriptions.TrainNumber };
					case TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle:
						return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, Description = TransportMeansList.Descriptions.RegistrationNumberOfTheRoadVehicle };
					case TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer:
						return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer, Description = TransportMeansList.Descriptions.RegistrationNumberOfTheRoadTrailer };
					case TransportMeansList.Codes.IataFlightNumber:
						return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.IataFlightNumber, Description = TransportMeansList.Descriptions.IataFlightNumber };
					case TransportMeansList.Codes.RegistrationNumberOfTheAircraft:
						return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.RegistrationNumberOfTheAircraft, Description = TransportMeansList.Descriptions.RegistrationNumberOfTheAircraft };
					case TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode:
						return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, Description = TransportMeansList.Descriptions.EuropeanVesselIdentificationNumberEniCode };
					case TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel:
						return new CodeDescriptionPair2Char() { Code = TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, Description = TransportMeansList.Descriptions.NameOfTheInlandWaterwaysVessel };
					default:
						return null;
				}
			}
		}

		protected override UniversalShipment.CustomsEntryInstructionDataObjectWriter GetNewCustomsEntryInstructionDataObjectWriter()
		{
			return new CustomsEntryInstructionDataObjectWriter(writeManager, helper);
		}

		protected override void PopulateCountrySpecificContainerValue(Container containerData, BaseCusContainer containerBO, BaseJobDeclaration declarationBO)
		{
			if (containerBO is CusContainer container && declarationBO is JobDeclaration declaration && declaration.AdditionalSealsRequired)
			{
				containerData.SetAdditionalSealNumberCollection(() => CreateSortedSealNumbers(container.AdditionalSeals));
			}
		}

		protected override UniversalShipment.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter()
		{
			return new CustomsEntryHeaderDataObjectWriter(writeManager, helper);
		}

		void PopulateEquipments(Shipment declarationData, BaseJobDeclaration declarationBO)
		{
			if (declarationBO is JobDeclaration declaration && declaration.EquipmentsRequired)
			{
				var equipmentList = new List<Equipment>();
				foreach (var equipment in declaration.Equipments.OrderBy(x => x.CEQ_IdentificationNumber))
				{
					var listItem = new Equipment(writeManager.WriterStrategy) { IdentificationNumber = equipment.CEQ_IdentificationNumber };
					listItem.SetSealNumberCollection(() => CreateSortedSealNumbers(equipment.Seals));
					equipmentList.Add(listItem);
				}
				declarationData.SetTransportEquipmentCollection(() => { return equipmentList; });
			}
		}

		static List<SealNumber> CreateSortedSealNumbers(CusSealCollection seals) => seals.OrderBy(x => x.BK_SequenceNumber).ThenBy(x => x.BK_SealNumber).Select(s => new SealNumber() { Number = s.BK_SealNumber }).ToList();

		static IEnumerable<AddInfoGroup> CreateOldAdditionalInfoIfNeeded(Integration.Customs.ICusSupportingInfoTypeSupporter supporter)
		{
			foreach (var supportingDocument in LoadCusSupportingInfo<AdditionalInfo>(supporter.Factory, supporter.IsInDatabase, supporter.PK, CusSupportingInfoTypeList.Codes.AdditionalInfo))
			{
				yield return new AddInfoGroup()
				{
					Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = Constants.OldCusAddInfoTypes.AdditionalInfo.Type, Description = CusSupportingInfoTypeList.Descriptions.AdditionalInfo },
					AddInfoCollection = new List<AddInfo>(new[]
					{
						AddInfo.New(Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.Description, BaseAddInfo.GetStringRepresentation(supportingDocument.CSI_Description)),
						AddInfo.New(Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.NctsExportFromCountry, BaseAddInfo.GetStringRepresentation(supportingDocument.CSI_RN_NKCountryCode)),
						AddInfo.New(Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.NctsExportFromEC, BaseAddInfo.GetStringRepresentation(supportingDocument.CSI_NctsExportFromEC)),
						AddInfo.New(Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.TypeCode, BaseAddInfo.GetStringRepresentation(supportingDocument.CSI_Code))
					})
				};
			}
		}

		static IEnumerable<AddInfoGroup> CreateOldPreviousDocumentIfNeeded(Integration.Customs.ICusSupportingInfoTypeSupporter supporter)
		{
			foreach (var supportingDocument in LoadCusSupportingInfo<PreviousDocument>(supporter.Factory, supporter.IsInDatabase, supporter.PK, CusSupportingInfoTypeList.Codes.PreviousDocument))
			{
				yield return new AddInfoGroup()
				{
					Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = Constants.OldCusAddInfoTypes.PreviousDocument.Type, Description = CusSupportingInfoTypeList.Descriptions.PreviousDocument },
					AddInfoCollection = new List<AddInfo>(new[]
					{
						AddInfo.New(Constants.OldCusAddInfoTypes.PreviousDocument.Fields.Class, BaseAddInfo.GetStringRepresentation(supportingDocument.CSI_SubType)),
						AddInfo.New(Constants.OldCusAddInfoTypes.PreviousDocument.Fields.DateOfIssue, BaseAddInfo.GetStringRepresentation(supportingDocument.CSI_DateOfIssue)),
						AddInfo.New(Constants.OldCusAddInfoTypes.PreviousDocument.Fields.MoreInfo, BaseAddInfo.GetStringRepresentation(supportingDocument.CSI_Description)),
						AddInfo.New(Constants.OldCusAddInfoTypes.PreviousDocument.Fields.Reference, BaseAddInfo.GetStringRepresentation(supportingDocument.CSI_ReferenceNumber)),
						AddInfo.New(Constants.OldCusAddInfoTypes.PreviousDocument.Fields.TypeCode, BaseAddInfo.GetStringRepresentation(supportingDocument.CSI_Code))
					})
				};
			}
		}

		static IEnumerable<AddInfoGroup> CreateOldSupportingDocumentIfNeeded(Integration.Customs.ICusSupportingInfoTypeSupporter supporter, UniversalDataObjectWriterHelper helper)
		{
			foreach (var supportingDocument in LoadCusSupportingInfo<SupportingDocument>(supporter.Factory, supporter.IsInDatabase, supporter.PK, CusSupportingInfoTypeList.Codes.SupportingDocument))
			{
				yield return new AddInfoGroup()
				{
					Type = new UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair() { Code = Constants.OldCusAddInfoTypes.SupportingDocument.Type, Description = CusSupportingInfoTypeList.Descriptions.SupportingDocument },
					AddInfoCollection = CreateOldSupportingDocumentAddInfoCollection(helper.GetOldSupportingDocumentAddInfo(supportingDocument))
				};
			}
		}

		static List<AddInfo> CreateOldSupportingDocumentAddInfoCollection(IEnumerable<KeyValuePair<ZString, IZType>> addInfos)
		{
			var result = new List<AddInfo>();
			foreach (var addInfo in addInfos)
			{
				result.Add(AddInfo.New(addInfo.Key, BaseAddInfo.GetStringRepresentation(addInfo.Value)));
			}
			return result;
		}

		static T[] LoadCusSupportingInfo<T>(BusinessObjectFactory factory, bool isInDatabase, ZGuid pk, ZString type)
			where T : ImportExportAwareSupportingInfo
		{
			var query = new ZQuery(CusSupportingInfoSchema.CSI_ParentID, pk);
			query.AddToFilter(CusSupportingInfoSchema.CSI_Type, type);
			query.FetchOnlyFromLocalCache = !isInDatabase;
			return factory.Load<T>(query);
		}
	}
}
