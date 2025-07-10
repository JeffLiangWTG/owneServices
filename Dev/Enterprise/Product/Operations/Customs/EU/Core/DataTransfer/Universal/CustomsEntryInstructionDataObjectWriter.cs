using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using GoodsLocationQualifierCodes = Enterprise.Customs.Business.CusGoodsLocationQualifierList.Codes;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CustomsEntryInstructionDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryInstructionDataObjectWriter
	{
		public CustomsEntryInstructionDataObjectWriter(IDataWritingManager manager, Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper helper) : base(manager, helper)
		{
		}

		protected override EntryInstruction PopulateDataObject(Customs.Business.CusEntryInstruction sourceBO)
		{
			var dataObject = base.PopulateDataObject(sourceBO);
			var entryInstruction = (CusEntryInstruction)sourceBO;
			var declaration = entryInstruction.JobDeclaration;
			var configuration = declaration.Configuration.InstructionConfiguration;
			PopulateCustomsSupportingInformation(configuration, declaration, entryInstruction, dataObject);
			PopulateAdditionalSupplyChainActors(configuration, declaration, entryInstruction, dataObject);
			if (declaration.AllowGoodsLocationFromImport)
			{
				PopulateGoodsLocationInformation(entryInstruction.GoodsLocation, dataObject);
			}
			return dataObject;
		}

		protected override List<AddInfo> GetEntryInstructionAddInfoCollection(Customs.Business.CusEntryInstruction instructionBO)
		{
			var result = base.GetEntryInstructionAddInfoCollection(instructionBO);
			var entryInstruction = (CusEntryInstruction)instructionBO;
			var dv1Detail = entryInstruction.DV1DetailsPivots.SingleOrDefault(x => x.IsForEntryInstruction);
			if (dv1Detail != null)
			{
				var euHelper = helper as UniversalDataObjectWriterHelper;
				var dv1DetailLink = euHelper.AllocateDv1DetailsLink(dv1Detail.DV1Detail.PK);
				result.AddAddInfo(Constants.AddInfoKeys.EntryInstruction.CustomsValueInformationLink, dv1DetailLink);
			}
			result.AddAddInfo(Constants.AddInfoKeys.EntryInstruction.TotalInnerPackages, instructionBO.CEI_TotalInnerPackages);
			return result;
		}

		protected override ZString[] GetSupportedCusSupportingInfoCSI_Types(Customs.Business.CusEntryInstruction entryInstructionBO)
		{
			if (entryInstructionBO is CusEntryInstruction euEntryInstruction && euEntryInstruction.JobDeclaration is JobDeclaration declaration && declaration.AllowUCC6PropertiesWithUCC5)
			{
				var result = helper.GetSupportedCusSupportingInfoCSI_TypesFor(euEntryInstruction.TablePrefix, string.Empty)?.ToHashSet() ?? new HashSet<ZString>();
				if (declaration.Configuration.InstructionConfiguration.AdditionalInfosSupport(declaration, euEntryInstruction))
				{
					if (!result.Contains(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo))
					{
						result.Add(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
					}
				}
				else if (result.Contains(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo))
				{
					result.Remove(Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo);
				}
				return result.ToArray();
			}
			return base.GetSupportedCusSupportingInfoCSI_Types(entryInstructionBO);
		}

		void PopulateCustomsSupportingInformation(InstructionConfiguration configuration, JobDeclaration declaration, CusEntryInstruction entryInstruction, EntryInstruction dataObject)
		{
			if (entryInstruction is Integration.Customs.ICusSupportingInfoTypeSupporter)
			{
				var customsSupportingInformationCollection = dataObject.CustomsSupportingInformationCollection ?? new List<CustomsSupportingInformation>();
				AddEmptyPreviousDocumentIfNeeded(configuration, declaration, customsSupportingInformationCollection);
				AddEmptySupportingDocumentIfNeeded(configuration, declaration, customsSupportingInformationCollection);
				AddEmptyAdditionalInfoIfNeeded(configuration, declaration, entryInstruction, customsSupportingInformationCollection);
				AddEmptyDocumentRequestedIfNeeded(configuration, declaration, customsSupportingInformationCollection);
				dataObject.SetCustomsSupportingInformationCollection(() => customsSupportingInformationCollection);
			}
		}

		void AddEmptyCustomsSupportingInformationIfMissing(List<CustomsSupportingInformation> customsSupportingInformationCollection, ZString code, ZString description)
		{
			if (!customsSupportingInformationCollection.Any(x => x.Category.GetCodeAsUpperCase() == code))
			{
				customsSupportingInformationCollection.Add(new CustomsSupportingInformation()
				{
					Category = new CodeDescriptionPair() { Code = code, Description = description }
				});
			}
		}

		void AddEmptyPreviousDocumentIfNeeded(InstructionConfiguration configuration, JobDeclaration declaration, List<CustomsSupportingInformation> customsSupportingInformationCollection)
		{
			if (configuration.PreviousDocumentsSupport(declaration))
			{
				AddEmptyCustomsSupportingInformationIfMissing(customsSupportingInformationCollection, Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, Common.EU.CusSupportingInfoTypeList.Descriptions.PreviousDocument);
			}
		}

		void AddEmptyAdditionalInfoIfNeeded(InstructionConfiguration configuration, JobDeclaration declaration, CusEntryInstruction entryInstruction, List<CustomsSupportingInformation> customsSupportingInformationCollection)
		{
			if (configuration.AdditionalInfosSupport(declaration, entryInstruction))
			{
				AddEmptyCustomsSupportingInformationIfMissing(customsSupportingInformationCollection, Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, Common.EU.CusSupportingInfoTypeList.Descriptions.AdditionalInfo);
			}
		}

		void AddEmptySupportingDocumentIfNeeded(InstructionConfiguration configuration, JobDeclaration declaration, List<CustomsSupportingInformation> customsSupportingInformationCollection)
		{
			if (configuration.SupportingDocumentsSupport(declaration))
			{
				AddEmptyCustomsSupportingInformationIfMissing(customsSupportingInformationCollection, Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, Common.EU.CusSupportingInfoTypeList.Descriptions.SupportingDocument);
			}
		}

		void AddEmptyDocumentRequestedIfNeeded(InstructionConfiguration configuration, JobDeclaration declaration, List<CustomsSupportingInformation> customsSupportingInformationCollection)
		{
			if (configuration.RequestedDocumentsSupport(declaration))
			{
				AddEmptyCustomsSupportingInformationIfMissing(customsSupportingInformationCollection, Common.EU.CusSupportingInfoTypeList.Codes.InstructionRequestedDocument, Common.EU.CusSupportingInfoTypeList.Descriptions.InstructionRequestedDocument);
			}
		}

		void PopulateAdditionalSupplyChainActors(InstructionConfiguration configuration, JobDeclaration declaration, CusEntryInstruction entryInstruction, EntryInstruction dataObject)
		{
			if (configuration.AdditionalSupplyChainActorSupport(declaration))
			{
				var references = dataObject.CustomsReferenceCollection ?? new List<CustomsReference>();
				UniversalCustomsDataObjectProvider.PopulateCustomsReference(references, entryInstruction.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>(), helper, writeManager);
				dataObject.SetCustomsReferenceCollection(() => references);
			}
		}

		void PopulateGoodsLocationInformation(CusGoodsLocation goodsLocation, EntryInstruction dataObject)
		{
			var locationOfGoodsCollection = dataObject.LocationOfGoodsCollection ?? new List<LocationOfGoods>();

			locationOfGoodsCollection.Add(new LocationOfGoods
			{
				Qualifier = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(goodsLocation.CGL_Qualifier, goodsLocation.Lookups.QualifierList),
				LocationType = ListHelper.GetWithDescription<CodeDescriptionPair1Char>(goodsLocation.CGL_Type, goodsLocation.Lookups.TypeList),
				AuthorizationNumber = goodsLocation.Address.AuthorisationNumber,
				AdditionalIdentifier = goodsLocation.CGL_AdditionalIdentifier,
				CustomsOffice = goodsLocation.CGL_CustomsOffice,
				Contact = GetContactFromGoodsLocationAddress(goodsLocation.Address),
			});

			dataObject.SetLocationOfGoodsCollection(() => locationOfGoodsCollection);
			var qualifier = goodsLocation.CGL_Qualifier;
			if (qualifier == GoodsLocationQualifierCodes.Address || qualifier == GoodsLocationQualifierCodes.AuthorizationNumber || qualifier == GoodsLocationQualifierCodes.PostcodeAddress)
			{
				dataObject.AddOrgAddress(writeManager, goodsLocation.Address, MasterFiles.Integration.DocAddressType.LocationOfGoods);
			}
		}

		Contact GetContactFromGoodsLocationAddress(CusGoodsLocationAddress address)
		{
			return new Contact
			{
				Name = address.E2_Contact,
				PhoneNumber = address.E2_Phone,
				Email = address.E2_Email,
			};
		}
	}
}
