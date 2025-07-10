using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using GoodsLocationQualifierCodes = Enterprise.Customs.Business.CusGoodsLocationQualifierList.Codes;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsEntryInstructionDataObjectReader : Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectReader
	{
		public CustomsEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, UniversalObjectFactory factory, Customs.Business.BaseJobDeclaration declaration)
			: base(entryInstructionDataObject, logger, helper, factory, declaration)
		{
		}

		protected override void FillCustomsReferences(ZGuid entryInstructionPK, bool entryInstructionIsInDatabase)
			=> new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(entryInstructionPK, CusEntryInstructionSchema.Constants.Prefix, entryInstructionIsInDatabase, dataObject);

		protected override void FillCountrySpecificDetails(Customs.Business.CusEntryInstruction targetBO)
		{
			base.FillCountrySpecificDetails(targetBO);
			if (targetBO is CusEntryInstruction entryInstruction && declaration is JobDeclaration euDeclaration)
			{
				var instructionConfiguration = euDeclaration.Configuration.InstructionConfiguration;
				if (instructionConfiguration.AdditionalSupplyChainActorSupport(euDeclaration))
				{
					CreateNewCusSupplyChainActorReferencesDataObjectReader().Read(entryInstruction.CusSupplyChainActorReferences, CustomsReferenceGroupByType);
				}
				PopulateGoodsLocationInformation(entryInstruction.GoodsLocation);
			}

			var instructionRow = GetColumnIndexer(targetBO);
			var addInfoCollection = dataObject.AddInfoCollection;
			PopulateTotalInnerPackages(instructionRow, addInfoCollection);
			MarkDv1DetailAsSelected(targetBO, addInfoCollection);

			void PopulateTotalInnerPackages(IColumnIndexer row, List<UniversalAddInfo> addInfoList)
			{
				if (addInfoList?.FirstOrDefault(info => info.Key.Equals(Constants.AddInfoKeys.EntryInstruction.TotalInnerPackages)) is UniversalAddInfo addInfo)
				{
					if (ZInt.TryParse(addInfo.Value, out var value))
					{
						SetValue(row, CusEntryInstructionSchema.CEI_TotalInnerPackages, value);
					}
				}
			}

			void MarkDv1DetailAsSelected(Customs.Business.CusEntryInstruction targetBO, List<UniversalAddInfo> addInfoList)
			{
				var customsValueLinkAddInfo = addInfoList?.FirstOrDefault(info =>
					info.Key.GetValueOrDefault() == Constants.AddInfoKeys.EntryInstruction.CustomsValueInformationLink);

				if (targetBO is CusEntryInstruction entryInstruction && customsValueLinkAddInfo != null)
				{
					if (ZShort.TryParse(customsValueLinkAddInfo.Value, out var linkValue))
					{
						var euHelper = helper as UniversalDataObjectReaderHelper;
						var dv1DetailsPK = euHelper.GetDv1DetailsPK(linkValue);
						if (dv1DetailsPK.HasValue)
						{
							var selectedDv1Detail = entryInstruction.DV1DetailsPivots.FirstOrDefault(x => x.DV1Detail.PK == dv1DetailsPK);
							if (selectedDv1Detail != null)
							{
								selectedDv1Detail.IsForEntryInstruction = true;
							}
						}
					}
				}
			}
		}

		protected override CustomsSupportingInformationCollectionDataObjectReader.GetMatchingDataPredicate GetMatchingCusSupportingInfo()
		{
			if (declaration is JobDeclaration euDeclaration && euDeclaration.AllowUCC6PropertiesWithUCC5)
			{
				return GetMatchingCusSupportingInfo;
			}
			return null;
		}

		protected virtual Customs.Business.CusSupportingInfo GetMatchingCusSupportingInfo(IEnumerable<Customs.Business.CusSupportingInfo> existingSupportingInfos, ZString type, CustomsSupportingInformation customsSupportingInformation)
		{
			Customs.Business.CusSupportingInfo result = null;
			switch (type)
			{
				case Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument:
					result = GetMatchingPreviousDocument(existingSupportingInfos.OfType<PreviousDocument>(), customsSupportingInformation);
					break;
				case Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument:
					result = GetMatchingSupportingDocument(existingSupportingInfos.OfType<SupportingDocument>(), customsSupportingInformation);
					break;
				case Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo:
					result = GetMatchingAdditionalInfo(existingSupportingInfos.OfType<AdditionalInfo>(), customsSupportingInformation);
					break;
				case Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument:
					result = GetMatchingRequestedDocument(existingSupportingInfos.OfType<RequestedDocument>(), customsSupportingInformation);
					break;
			}
			return result;
		}

		Customs.Business.CusSupportingInfo GetMatchingRequestedDocument(IEnumerable<RequestedDocument> existingRequestedDocuments, CustomsSupportingInformation customsSupportingInformation)
		{
			var type = customsSupportingInformation.Type.GetCodeAsUpperCase();
			var description = customsSupportingInformation.Description.GetValueOrDefault();
			var dateOfIssue = customsSupportingInformation.DateOfIssue.GetValueOrDefault();
			var dateOfExpiry = customsSupportingInformation.DateOfExpiry.GetValueOrDefault();
			var status = customsSupportingInformation.Status.GetCodeAsUpperCase();
			return existingRequestedDocuments.FirstOrDefault(x => x.CSI_Code.EqualsIgnoringCase(type) && x.CSI_Description.EqualsIgnoringCase(description) && x.CSI_DateOfIssue.Equals(dateOfIssue) && x.CSI_DateOfExpiry.Equals(dateOfExpiry) && x.CSI_Status.EqualsIgnoringCase(status));
		}

		protected virtual PreviousDocument GetMatchingPreviousDocument(IEnumerable<PreviousDocument> existingPreviousDocuments, CustomsSupportingInformation customsSupportingInformation)
		{
			var type = customsSupportingInformation.Type.GetCodeAsUpperCase();
			var referenceNumber = customsSupportingInformation.ReferenceNumber.GetValueOrDefault();
			return existingPreviousDocuments.FirstOrDefault(x => x.CSI_Code.EqualsIgnoringCase(type) && x.CSI_ReferenceNumber.EqualsIgnoringCase(referenceNumber));
		}

		protected virtual SupportingDocument GetMatchingSupportingDocument(IEnumerable<SupportingDocument> existingSupportingDocuments, CustomsSupportingInformation customsSupportingInformation)
		{
			var type = customsSupportingInformation.Type.GetCodeAsUpperCase();
			var referenceNumber = customsSupportingInformation.ReferenceNumber.GetValueOrDefault();
			var dateOfExpiry = customsSupportingInformation.DateOfExpiry.GetValueOrDefault();
			var additionalDescription = customsSupportingInformation.AdditionalDescription.GetValueOrDefault();
			var itemNumber = customsSupportingInformation.ItemNumber.GetValueOrDefault();
			return existingSupportingDocuments.FirstOrDefault(x => x.CSI_Code.EqualsIgnoringCase(type) && x.CSI_ReferenceNumber.EqualsIgnoringCase(referenceNumber) && x.CSI_DateOfExpiry.Equals(dateOfExpiry) && x.CSI_AdditionalDescription.EqualsIgnoringCase(additionalDescription) && x.CSI_ItemNumber.Equals(itemNumber));
		}

		protected virtual AdditionalInfo GetMatchingAdditionalInfo(IEnumerable<AdditionalInfo> existingAdditionalInfos, CustomsSupportingInformation customsSupportingInformation)
		{
			var subType = customsSupportingInformation.SubType.GetCodeAsUpperCase();
			var type = customsSupportingInformation.Type.GetCodeAsUpperCase();
			var referenceNumber = customsSupportingInformation.ReferenceNumber.GetValueOrDefault();
			var description = customsSupportingInformation.Description.GetValueOrDefault();
			return existingAdditionalInfos.FirstOrDefault(x => x.CSI_SubType.EqualsIgnoringCase(subType) && x.CSI_Code.EqualsIgnoringCase(type) && x.CSI_ReferenceNumber.EqualsIgnoringCase(referenceNumber) && x.CSI_Description.EqualsIgnoringCase(description));
		}

		protected override ZString[] GetSupportedCusSupportingInfoCSI_Types(Customs.Business.CusEntryInstruction targetBO)
		{
			var list = helper.GetSupportedCusSupportingInfoCSI_TypesFor(CusEntryInstructionSchema.Constants.Prefix, string.Empty)?.ToList() ?? new List<ZString>();
			if (targetBO is CusEntryInstruction entryInstruction && declaration is JobDeclaration euDeclaration && euDeclaration.AllowUCC6PropertiesWithUCC5)
			{
				var configuration = euDeclaration.Configuration;
				var instructionConfiguration = configuration.InstructionConfiguration;
				if (list.Contains(CusSupportingInfoTypeList.Codes.SupportingDocument))
				{
					if (!instructionConfiguration.SupportingDocumentsSupport(euDeclaration))
					{
						list.Remove(CusSupportingInfoTypeList.Codes.SupportingDocument);
					}
				}
				else if (instructionConfiguration.SupportingDocumentsSupport(euDeclaration))
				{
					list.Add(CusSupportingInfoTypeList.Codes.SupportingDocument);
				}

				if (list.Contains(CusSupportingInfoTypeList.Codes.PreviousDocument))
				{
					if (!instructionConfiguration.PreviousDocumentsSupport(euDeclaration))
					{
						list.Remove(CusSupportingInfoTypeList.Codes.PreviousDocument);
					}
				}
				else if (instructionConfiguration.PreviousDocumentsSupport(euDeclaration))
				{
					list.Add(CusSupportingInfoTypeList.Codes.PreviousDocument);
				}

				if (list.Contains(CusSupportingInfoTypeList.Codes.InstructionRequestedDocument))
				{
					if (!instructionConfiguration.RequestedDocumentsSupport(euDeclaration))
					{
						list.Remove(CusSupportingInfoTypeList.Codes.InstructionRequestedDocument);
					}
				}
				else if (instructionConfiguration.RequestedDocumentsSupport(euDeclaration))
				{
					list.Add(CusSupportingInfoTypeList.Codes.InstructionRequestedDocument);
				}

				if (list.Contains(CusSupportingInfoTypeList.Codes.AdditionalInfo))
				{
					if (!instructionConfiguration.AdditionalInfosSupport(euDeclaration, entryInstruction))
					{
						list.Remove(CusSupportingInfoTypeList.Codes.AdditionalInfo);
					}
				}
				else if (instructionConfiguration.AdditionalInfosSupport(euDeclaration, entryInstruction))
				{
					list.Add(CusSupportingInfoTypeList.Codes.AdditionalInfo);
				}
			}
			return list.ToArray();
		}

		protected virtual CusSupplyChainActorReferencesDataObjectReader CreateNewCusSupplyChainActorReferencesDataObjectReader() => new CusSupplyChainActorReferencesDataObjectReader(logger, factory);

		void PopulateGoodsLocationInformation(CusGoodsLocation goodsLocationBO)
		{
			var goodsLocation = dataObject.LocationOfGoodsCollection?.FirstOrDefault();
			if (goodsLocation is null)
			{
				return;
			}

			var goodsLocationRow = GetColumnIndexer(goodsLocationBO);
			SetValue(goodsLocationRow, CusGoodsLocationSchema.CGL_Qualifier, goodsLocation.Qualifier);
			SetValue(goodsLocationRow, CusGoodsLocationSchema.CGL_Type, goodsLocation.LocationType);
			SetValue(goodsLocationRow, CusGoodsLocationSchema.CGL_AdditionalIdentifier, GetAdditionalIdentifier(goodsLocation));
			SetValue(goodsLocationRow, CusGoodsLocationSchema.CGL_CustomsOffice, goodsLocation.CustomsOffice);

			var addressRow = GetColumnIndexer(goodsLocationBO.Address);

			FillGoodsLocationAddress(addressRow);

			SetValue(addressRow, JobDocAddressSchema.E2_GovRegNum, goodsLocation.AuthorizationNumber);
			SetValue(addressRow, JobDocAddressSchema.E2_Contact, goodsLocation.Contact?.Name);
			SetValue(addressRow, JobDocAddressSchema.E2_Phone, goodsLocation.Contact?.PhoneNumber);
			SetValue(addressRow, JobDocAddressSchema.E2_Email, goodsLocation.Contact?.Email);
		}

		protected virtual ZString? GetAdditionalIdentifier(LocationOfGoods goodsLocation) => goodsLocation.AdditionalIdentifier;

		void FillGoodsLocationAddress(IColumnIndexer addressRow)
		{
			var qualifier = dataObject.LocationOfGoodsCollection?.FirstOrDefault()?.Qualifier?.Code ?? ZString.Empty;
			var goodsLocationAddressFromDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(x => (x.AddressType ?? ZString.Empty) == nameof(MasterFiles.Integration.DocAddressType.LocationOfGoods));

			if (qualifier.IsEmpty || goodsLocationAddressFromDataObject is null)
			{
				return;
			}

			if (this.TryGetMatchedOrganisation(out var addressBO, dataObject, declaration, nameof(MasterFiles.Integration.DocAddressType.LocationOfGoods), MasterFiles.Integration.OrganisationTypes.None))
			{
				if (qualifier == GoodsLocationQualifierCodes.Address)
				{
					if (goodsLocationAddressFromDataObject.AddressOverride ?? false)
					{
						SetGoodsLocationAddressManually(addressRow, goodsLocationAddressFromDataObject);
						return;
					}

					SetValue(addressRow, JobDocAddressSchema.E2_OA_Address, addressBO?.PK);
					return;
				}

				if (qualifier == GoodsLocationQualifierCodes.AuthorizationNumber)
				{
					SetValue(addressRow, JobDocAddressSchema.E2_AdditionalAddressInformation, addressBO?.OA_OH.ToString());
				}
			}
		}

		void SetGoodsLocationAddressManually(IColumnIndexer addressRow, OrganizationAddress goodsLocationAddress)
		{
			SetValue(addressRow, JobDocAddressSchema.E2_AddressOverride, goodsLocationAddress.AddressOverride);
			SetValue(addressRow, JobDocAddressSchema.E2_CompanyName, goodsLocationAddress.CompanyName);
			SetValue(addressRow, JobDocAddressSchema.E2_Address1, goodsLocationAddress.Address1);
			SetValue(addressRow, JobDocAddressSchema.E2_Address2, goodsLocationAddress.Address2);
			SetValue(addressRow, JobDocAddressSchema.E2_City, goodsLocationAddress.City);
			SetValue(addressRow, JobDocAddressSchema.E2_Postcode, goodsLocationAddress.Postcode);
			SetValue(addressRow, JobDocAddressSchema.E2_RN_NKCountryCode, goodsLocationAddress.Country);
		}
	}
}
