using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business
{
	public class FRCusAuthorizationUsageUpdater : CusAuthorizationUsageUpdater
	{
		public FRCusAuthorizationUsageUpdater(JobDeclaration jobDeclaration) : base(jobDeclaration)
		{
			PopulateSimplifiedDeclarationAuthorization();
			PopulateOutwardProcessingAuthorization();
			PopulateOtherThanOpoAuthorization();
			PopulateTemporaryExportationAuthorization();
			PopulateInwardProcessingAuthorization();
			PopulateEndUseAuthorization();
			PopulateTemporaryAdmissionAuthorization();
			PopulateCustomsWarehousingCWPAuthorization();
		}

		public static readonly ImmutableHashSet<string> OPORelatedCodeList = ImmutableHashSet.Create(new[]
		{
			DeltaGExportDeclarationTypeList.Codes.ExportationForOutwardProcessing
		});

		public static readonly ImmutableHashSet<string> OTORelatedCodeList = ImmutableHashSet.Create(new[]
		{
			DeltaGExportDeclarationTypeList.Codes.TemporaryExportOtherThanUnderCode21
		});

		public static readonly ImmutableHashSet<string> TEERelatedCodeList = ImmutableHashSet.Create(new[]
		{
			DeltaGExportDeclarationTypeList.Codes.TemporaryExportWithEI
		});

		public static readonly ImmutableHashSet<string> CWPRelatedCodeList = ImmutableHashSet.Create(new[]
		{
			DeltaGExportDeclarationTypeList.Codes.PlacingGoodsUnderBW,
			DeltaGExportDeclarationTypeList.Codes.ManufacturingOfGoodsUnderSupervision,
			DeltaGImportDeclarationTypeList.Codes.PlacingGoodsUnderBW,
			DeltaGImportDeclarationTypeList.Codes.EntryOfGoodsForAFreeZone,
			DeltaGImportDeclarationTypeList.Codes.EntryOfGoodsForAFreeZoneWithEI
		});

		public static IEnumerable<string> GetValidAuthorizationTypes(string declarationType)
		{
			if (OPORelatedCodeList.Contains(declarationType))
			{
				yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			}
			else if (OTORelatedCodeList.Contains(declarationType))
			{
				yield return CusAuthorizationHeaderTypeList.Codes.OtherThanOpo;
			}
			else if (TEERelatedCodeList.Contains(declarationType))
			{
				yield return CusAuthorizationHeaderTypeList.Codes.TemporaryExportation;
			}
			else if (declarationType == DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing)
			{
				yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			}
			else if (declarationType == DeltaGImportDeclarationTypeList.Codes.EntryForEndUse || declarationType == DeltaGImportDeclarationTypeList.Codes.EntryForEndUseWithEI)
			{
				yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse;
			}
			else if (declarationType == DeltaGImportDeclarationTypeList.Codes.TemporaryImportation)
			{
				yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission;
			}
			else if (CWPRelatedCodeList.Contains(declarationType))
			{
				yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
			}
			else if (declarationType == DeltaIEImportDeclarationTypeList.Codes.I1)
			{
				yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			}
			else
			{
				yield break;
			}
		}

		public static IEnumerable<string> GetAuthorizationTypesForCustomsWarehousing()
		{
			yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			yield return Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2;
		}

		void PopulateSimplifiedDeclarationAuthorization()
		{
			var headerType = Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			ForAuthorizationType(headerType)
				.When(p => p.CEI_Style == DeltaIEImportDeclarationTypeList.Codes.I1)
				.ReturnHolderAndNumber(p => GetHolderAndNumber(headerType, p));
		}

		void PopulateOutwardProcessingAuthorization()
		{
			var headerType = Customs.Business.CusAuthorizationHeaderTypeList.Codes.OutwardProcessing;
			ForAuthorizationType(headerType)
				.When(p => OPORelatedCodeList.Contains(p.CEI_Style))
				.ReturnHolderAndNumber(p => GetHolderAndNumber(headerType, p));
		}

		void PopulateOtherThanOpoAuthorization()
		{
			var headerType = CusAuthorizationHeaderTypeList.Codes.OtherThanOpo;
			ForAuthorizationType(headerType)
				.When(p => OTORelatedCodeList.Contains(p.CEI_Style))
				.ReturnHolderAndNumber(p => GetHolderAndNumber(headerType, p));
		}

		void PopulateTemporaryExportationAuthorization()
		{
			var headerType = CusAuthorizationHeaderTypeList.Codes.TemporaryExportation;
			ForAuthorizationType(headerType)
				.When(p => TEERelatedCodeList.Contains(p.CEI_Style))
				.ReturnHolderAndNumber(p => GetHolderAndNumber(headerType, p));
		}

		void PopulateInwardProcessingAuthorization()
		{
			var headerType = Customs.Business.CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
			ForAuthorizationType(headerType)
				.When(p => p.CEI_Style == DeltaGImportDeclarationTypeList.Codes.ImportationForInwardProcessing)
				.ReturnHolderAndNumber(p => GetHolderAndNumber(headerType, p));
		}

		void PopulateTemporaryAdmissionAuthorization()
		{
			var headerType = Customs.Business.CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission;
			ForAuthorizationType(headerType)
				.When(p => p.CEI_Style == DeltaGImportDeclarationTypeList.Codes.TemporaryImportation)
				.ReturnHolderAndNumber(p => GetHolderAndNumber(headerType, p));
		}

		void PopulateEndUseAuthorization()
		{
			var headerType = Customs.Business.CusAuthorizationHeaderTypeList.Codes.EndUse;
			ForAuthorizationType(headerType)
				.When(p => p.CEI_Style == DeltaGImportDeclarationTypeList.Codes.EntryForEndUse || p.CEI_Style == DeltaGImportDeclarationTypeList.Codes.EntryForEndUseWithEI)
				.ReturnHolderAndNumber(p => GetHolderAndNumber(headerType, p));
		}

		void PopulateCustomsWarehousingCWPAuthorization()
		{
			var headerType = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
			ForAuthorizationType(headerType)
				.When(p => CWPRelatedCodeList.Contains(p.CEI_Style))
				.ReturnHolderAndNumber(p => GetHolderAndNumber(headerType, p));
		}

		(ZGuid holder, ZString number) GetHolderAndNumber(string authorizationType, Customs.Business.CusEntryInstruction cusEntry)
		{
			var declaration = cusEntry.JobDeclaration as JobDeclaration;
			var country = declaration?.CountryCode ?? GlbCompany.CurrentCompany.Country.Code;
			var authorisationHeaderQuery = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Authorisation);
			authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, country);
			authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_IsActive, true);
			authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_IsAdHoc, false);
			authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_Type, authorizationType);
			authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, SQLComparisonOperator.LessThanOrEqualTo, ZDate.Today);
			authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_IsSingleUse, false);

			var endDateQuery = new ZQuery(CusPermitHeaderSchema.CPH_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);
			endDateQuery.AddToFilter(JoinCondition.Or, CusPermitHeaderSchema.CPH_EndDate, null);
			authorisationHeaderQuery.AddToFilter(endDateQuery);

			var number = ZString.Empty;
			var holder = ZGuid.Empty;
			if (declaration?.IsUCC5 ?? true)
			{
				holder = declaration?.DeltaAccountOrgHeader?.PK ?? ZGuid.Empty;
				authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, holder);
				number = cusEntry.Factory.LoadTop1<CusAuthorisationHeader>(authorisationHeaderQuery)?.CPH_Number ?? ZString.Empty;
			}
			else
			{
				holder = declaration.IsImport ? declaration.Importer?.PK ?? ZGuid.Empty : declaration.Supplier?.PK ?? ZGuid.Empty;
				if (!holder.IsEmpty)
				{
					var query = authorisationHeaderQuery.DeepClone();
					query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, holder);
					var numbers = cusEntry.Factory.Load<CusAuthorisationHeader>(query);
					if (numbers.Length > 1)
					{
						holder = ZGuid.Empty;
					}
					else
					{
						number = numbers.FirstOrDefault()?.CPH_Number ?? ZString.Empty;
						if (number.IsEmpty)
						{
							holder = declaration.JE_DeclarantType == RepresentationTypeList.Codes.IND ? declaration.Declarant?.Header?.PK ?? ZGuid.Empty
								: declaration.JE_DeclarantType == RepresentationTypeList.Codes.DIR ? declaration.Representative?.Header?.PK ?? ZGuid.Empty : ZGuid.Empty;
							if (!holder.IsEmpty)
							{
								authorisationHeaderQuery.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, holder);
								numbers = cusEntry.Factory.Load<CusAuthorisationHeader>(authorisationHeaderQuery);
								if (numbers.Length > 1)
								{
									holder = ZGuid.Empty;
								}
								else
								{
									number = numbers.FirstOrDefault()?.CPH_Number ?? ZString.Empty;
									if (number.IsEmpty)
									{
										holder = ZGuid.Empty;
									}
								}
							}
						}
					}
				}
			}

			return (holder, number);
		}
	}
}
