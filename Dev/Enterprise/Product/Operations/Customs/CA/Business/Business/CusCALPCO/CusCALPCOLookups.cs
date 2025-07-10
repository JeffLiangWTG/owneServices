using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCALPCOLookups : AutoCusCALPCOLookups
	{
		public CusCALPCOLookups(AutoCusCALPCO parent)
			: base(parent)
		{
		}

		protected new CusCALPCO Parent => (CusCALPCO)base.Parent;

		protected IPGAProgramRequirementProvider PGAHeader
		{
			get { return Parent.Parent as IPGAProgramRequirementProvider; }
		}

		public CodeDescriptionPairList HolderPartyTypeCodes
		{
			get
			{
				if (PGAHeader != null)
				{
					return Factory.GetCachedValue<LPCOHolderPartyTypeCodes>();
				}

				return Factory.GetCachedValue("HolderPartyTypeCodes_MISC", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(LPCOHolderPartyTypeCodes.Codes.Importer, LPCOHolderPartyTypeCodes.Descriptions.Importer);
					result.AddPair(LPCOHolderPartyTypeCodes.Codes.Supplier, LPCOHolderPartyTypeCodes.Descriptions.Supplier);
					result.AddPair(LPCOHolderPartyTypeCodes.Codes.ImporterOfRecord, LPCOHolderPartyTypeCodes.Descriptions.ImporterOfRecord);
					result.AddPair(LPCOHolderPartyTypeCodes.Codes.Other, LPCOHolderPartyTypeCodes.Descriptions.Other);
					return result;
				});
			}
		}

		public BusinessObjectCollection DIFDocumentIDList
		{
			get
			{
				var declarationCompanyPK = Parent.Declaration?.CompanyPK ?? GlbCompany.CurrentCompany.PK;
				var collection = new JobRequiredDocumentAddInfoCollection(Factory, JobRequiredDocumentAddInfoLoadHelper.GetJobRequiredDocumentAddInfoFilter(Parent.DocsAndCartageParentPK, Parent.HolderOrgHeadersPks, declarationCompanyPK, Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF, PGAHeader?.GovAgencyIDCode ?? ZString.Empty));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Application Code", "Property", new ZString(Core.Constants.Customs.DocumentImageSystemIDs.CA_DIF)));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Company", "Property", new ZGuid(declarationCompanyPK)));
				return collection;
			}
		}

		public CodeDescriptionPairList LPCOApplicantCodes
		{
			get
			{
				if (PGAHeader != null)
				{
					return Factory.GetCachedValue<LPCOHolderPartyTypeCodes>();
				}

				return Factory.GetCachedValue("ApplicantTypeCodes_MISC", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(LPCOHolderPartyTypeCodes.Codes.Importer, LPCOHolderPartyTypeCodes.Descriptions.Importer);
					result.AddPair(LPCOHolderPartyTypeCodes.Codes.Supplier, LPCOHolderPartyTypeCodes.Descriptions.Supplier);
					result.AddPair(LPCOHolderPartyTypeCodes.Codes.ImporterOfRecord, LPCOHolderPartyTypeCodes.Descriptions.ImporterOfRecord);
					result.AddPair(LPCOHolderPartyTypeCodes.Codes.Other, LPCOHolderPartyTypeCodes.Descriptions.Other);

					return result;
				});
			}
		}

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList DocumentTypeCodes
		{
			get
			{
				var pgaHeader = PGAHeader;
				if (pgaHeader != null)
				{
					if (pgaHeader.GovAgencyIDCode == PGACodes.Codes.CFIA)
					{
						return CFIALPCOTypes;
					}
					return CADocumentTypePGATypes(pgaHeader.GovAgencyIDCode);
				}

				return AllLPCOTypes;
			}
		}

		public CodeDescriptionPairList RefNumbers
		{
			get
			{
				var result = new CodeDescriptionPairList();

				if (PGAHeader != null)
				{
					var documentType = Parent.DocumentType;
					if (documentType != null)
					{
						if (Parent.AgencyIDCode == PGAHeader.GovAgencyIDCode)
						{
							documentType.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.CADocumentTypeValuesAllowed).ForEach(x => result.AddPairIfNotExist(x, ZString.Empty));
						}
					}

					if (PGAHeader.GovAgencyIDCode == PGACodes.Codes.CFIA && Parent.IsTypeSafeFoodForCanadiansLicence)
					{
						result = RegistrationNumberHelper.GetCY_DataListForSafeFoodForCanadiansLicence(Factory, Parent.Declaration?.EffectiveImporter);
					}
				}
				else if (Parent.IsTypeSafeFoodForCanadiansLicence)
				{
					result = RegistrationNumberHelper.GetCY_DataListForSafeFoodForCanadiansLicence(Factory, Parent.Declaration?.EffectiveImporter);
				}

				return result;
			}
		}

		ZZRefCusCodeListCombinedCollection CFIALPCOTypes
		{
			get
			{
				var date = ZDateTime.UtcToday.Date;
				return Factory.GetCachedValue("CFIALPCOTypes_" + date.ToShortDateString(), () =>
				{
					var result = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIALPCOType, date);
					result.Load();
					return result;
				});
			}
		}

		ZZRefCusCodeListCombinedCollection CADocumentTypePGATypes(string pgaCode)
		{
				var date = ZDateTime.UtcToday.Date;
				return Factory.GetCachedValue("CAPGADocumentTypes_" + date.ToShortDateString() + pgaCode,
					() =>
					{
						var attributeFilters = new RefCusCodeListAttributeFilter[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, JoinCondition.And,
							new ZString[] { pgaCode }) };
						var result = new ZZRefCusCodeListCombinedCollection(Factory, new ZString[] { Core.Constants.CountryCodes.Canada }, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType }, date,
							attributeFilters);
						result.Load();
						return result;
					});
		}

		ZZRefCusCodeListCombinedCollection AllLPCOTypes
		{
			get
			{
				var date = ZDateTime.UtcToday.Date;
				return Factory.GetCachedValue("AllCADocumentTypes_" + date.ToShortDateString(),
					() =>
					{
						var result = new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.Canada, new ZString[2] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIALPCOType }, date, null);
						result.Load();
						return result;
					});
			}
		}

		public CodeDescriptionPairList UQList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (PGAHeader != null && PGAHeader.GovAgencyIDCode == PGACodes.Codes.ECCC)
				{
					result = ((ECCCPGAHeader)PGAHeader).AddInfoLookups.UQList;
				}
				else
				{
					result = Factory.GetCachedValue("UnitOfCountAndMeasureCodeList", () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddRange(Factory.GetCachedValue<IIDUnitOfCountCodeList>());
						list.AddRangeOverwriteIfExists(Factory.GetCachedValue<IIDUnitOfMeasureCodeList>());
						list.Sort();

						return list;
					});
				}

				return result;
			}
		}

		public OrgHeaderCollection LPCOHolders => new OrgHeaderCollection(Parent.Factory);

		public OrgHeaderCollection LPCOApplicants => new OrgHeaderCollection(Parent.Factory);
	}
}
