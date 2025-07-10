using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.GB.Business.GBCommonConstants;
#pragma warning disable CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
using biz = Enterprise.Customs.Business;
#pragma warning restore CS8981 // The type name only contains lower-cased ascii characters. Such names may become reserved for the language.
using CusAuthorizationHeaderTypeCodes = Enterprise.Customs.GB.Business.CodeDescriptionPairLists.CusAuthorizationHeaderTypeList.Codes;
using GBCodeList = Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using LocationQualifierCodes = Enterprise.Customs.GB.Business.CodeDescriptionPairLists.LocationQualifierList.Codes;

namespace Enterprise.Customs.GB.Business.Declaration
{
	[CodeProperty(Schema.CEI_Style), DescriptionProperty(Schema.CalculatedDescription)]
	public class CusEntryInstruction : AutoCusEntryInstruction,
		Integration.Customs.GB.ICusEntryInstruction,
		EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport,
		IShortSequenceNumberLine
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void DefaultPackageCount()
		{
			ZInt packageCount = (JobDeclaration?.JE_TotalNoOfPacks ?? 0) - JobDeclaration?.CustomsEntryInstructions?.Where(x => x.PK != PK)?.Sum(x => x.CEI_PackageCount) ?? 0;
			CEI_PackageCount = packageCount < ZInt.Zero ? ZInt.Zero : packageCount;
		}

		public new class Schema : EU.Business.Declaration.CusEntryInstruction.Schema
		{
			public new const int CEI_SubStyleMaxLength = 1;
			public const int CEI_SplitReferenceMaxLength = 2;
			public new const int CEI_StyleMaxLength = 3;
			public const string CEI_SplitReference = "CEI_SplitReference";
			public const string CurrentLocationFor523 = "CurrentLocationFor523";
			public const string CEI_PackageCount = "CEI_PackageCount";
			public const string CalculatedDescription = nameof(CusEntryInstruction.CalculatedDescription);
			public const string IsPostponedVatViaFiscalReference = nameof(CusEntryInstruction.IsPostponedVatViaFiscalReference);
		}

		[ResourceStringData("C93BC250-6A68-41E3-A4C2-78FD624642DB", ShortCaption = "PVA", Caption = "Postponed VAT Accounting", FullDescription = "The presence of an FR1 fiscal reference for this instruction indicates that Postponed VAT Accounting (PVA) is in use")]
		public ZBool IsPostponedVatViaFiscalReference => FiscalReferences.Cast<EU.Business.Declaration.CusFiscalReference>().Any(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR1_Importer && !f.CFR_Reference.IsEmpty);

		public ZString PVAFiscalReference => FiscalReferences.FirstOrDefault(f => f.CFR_Code == FiscalReferenceCodeList.Codes.FR1_Importer)?.CFR_Reference ?? ZString.Empty;

		public ZBool HasAnyChangeOfOwnershipProcedure => (HasIntoWarehouseProcedure && HasOutOfWarehouseProcedure)
					|| (HasIntoOutwardProcessingProcedure && HasOutOfOutwardProcessingProcedure)
					|| (HasIntoInwardProcessingProcedure && HasOutOfInwardProcessingProcedure)
					|| (HasIntoTemporaryImportProcedure && HasOutOfTemporaryImportProcedure);

		public IEnumerable<CusAuthorizationUsage> CusAuthorizationUsagesExceptOldOwnerForChangeOfOwner
		{
			get
			{
				var cusAuthorizationUsagesExceptOldOwnerForChangeOfOwner = CusAuthorizationUsages.AsEnumerable();

				if (HasAnyChangeOfOwnershipProcedure && !CEI_OH_Owner.IsEmpty && OldOwner != null)
				{
					cusAuthorizationUsagesExceptOldOwnerForChangeOfOwner = cusAuthorizationUsagesExceptOldOwnerForChangeOfOwner.Where(x => x.AGC_OH_Owner != OldOwner.PK);
				}
				return cusAuthorizationUsagesExceptOldOwnerForChangeOfOwner;
			}
		}

		// DE 5/23
		public
#if DEBUG
		virtual
#endif
		ZString CurrentLocationFor523
		{
			get
			{
				if (CEI_SubStyle == EntrySubStyleListImport.Codes.FinalSupplementaryDeclaration)
				{
					var result = JobDeclaration.FullLocationOfGoods;
					const string prefix = Core.Constants.CountryCodes.UnitedKingdom + JobDeclarationLookups.BY_AuthorisedPlaceAuthorisationNumber + ImportDeclarationType.FSD;
					var sdeAuth = CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Where(x => x.AGC_Code == AuthorisationTypeCodes.SDE).OrderBy(x => x.AGC_Number).FirstOrDefault();

					if (sdeAuth != null && sdeAuth.RelatedAuthorisationHeaderIgnoringReferenceNumber != null)
					{
						result = prefix + sdeAuth.RelatedAuthorisationHeaderIgnoringReferenceNumber?.CPH_Number;
					}
					else
					{
						var eirAuth = CusAuthorizationUsages.Cast<CusAuthorizationUsage>().Where(x => x.AGC_Code == AuthorisationTypeCodes.EIR).OrderBy(x => x.AGC_Number).FirstOrDefault();

						if (eirAuth != null && eirAuth.RelatedAuthorisationHeaderIgnoringReferenceNumber != null)
						{
							result = prefix + eirAuth.RelatedAuthorisationHeaderIgnoringReferenceNumber?.CPH_Number;
						}
					}

					return result;
				}
				else
				{
					return GetCurrentLocationFor523ForNonFSDs();
				}
			}
		}

		ZString GetCurrentLocationFor523ForNonFSDs()
		{
			var isOutOfWarehouse = HasOutOfWarehouseProcedure;
			var isOutOfInwardProcessing = HasOutOfInwardProcessingProcedure;
			var isOutOfOutwardProcessing = HasOutOfOutwardProcessingProcedure;
			var isOutOfTemporaryImportProcedure = HasOutOfTemporaryImportProcedure;
			var currentLocationFor523ForNonFSDs = string.Empty;
			var isArrivedExport = (JobDeclaration?.IsExport ?? false) && JobDeclaration.IsSubStyleGoodsArrived(CEI_SubStyle);

			if (isOutOfWarehouse) //xx71xxx or xx78xxx
			{
				if (Warehouse != null)
				{
					if (isArrivedExport)
					{
						currentLocationFor523ForNonFSDs = JobDeclaration.FullLocationOfGoods;
					}
					else
					{
						currentLocationFor523ForNonFSDs = Warehouse.OA_RN_NKCountryCode // GB etc
														+ JobDeclarationLookups.BY_AuthorisedPlaceAuthorisationNumber // BY
														+ GetWarehouseLocationQualifier()
														+ FromWarehouseCode;
					}
				}
			}
			else
			{
				if (!isArrivedExport)
				{
					if (isOutOfInwardProcessing) //xx51xxx
					{
						currentLocationFor523ForNonFSDs = GetAuthorisationNumberOfTypeConsideringOwnersAndRegime(CusAuthorizationHeaderTypeCodes.InwardProcessing, LocationQualifierCodes.InwardProcessing);
					}
					else if (isOutOfOutwardProcessing) //xx21xxx or xx22xxx
					{
						currentLocationFor523ForNonFSDs = GetAuthorisationNumberOfTypeConsideringOwnersAndRegime(CusAuthorizationHeaderTypeCodes.OutwardProcessing, LocationQualifierCodes.OutwardProcessing);
					}
					else if (isOutOfTemporaryImportProcedure) //xx53xxx
					{
						currentLocationFor523ForNonFSDs = GetAuthorisationNumberOfTypeConsideringOwnersAndRegime(CusAuthorizationHeaderTypeCodes.TemporaryAdmission, LocationQualifierCodes.TemporaryAdmission);
					}
				}

				if (string.IsNullOrEmpty(currentLocationFor523ForNonFSDs))
				{
					currentLocationFor523ForNonFSDs = JobDeclaration?.FullLocationOfGoods;
				}
			}
			return currentLocationFor523ForNonFSDs;
		}

		ZString GetAuthorisationNumberOfTypeConsideringOwnersAndRegime(ZString typeCode, ZString qualifier)
		{
			var authorisationReference = string.Empty;

			if (CusAuthorizationUsages.Count > 0)
			{
				if (!HasAnyChangeOfOwnershipProcedure || Owner == null || OldOwner == null)
				{
					authorisationReference = CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Code == typeCode)?.RelatedAuthorisationHeaderIgnoringReferenceNumber?.CPH_Number;
				}
				else
				{
					authorisationReference = CusAuthorizationUsages.FirstOrDefault(x => x.AGC_Code == typeCode && x.Owner == OldOwner)?.RelatedAuthorisationHeaderIgnoringReferenceNumber?.CPH_Number;
				}
			}

			if (!authorisationReference.IsNullOrEmpty())
			{
				authorisationReference = Core.Constants.CountryCodes.UnitedKingdom + JobDeclarationLookups.BY_AuthorisedPlaceAuthorisationNumber + qualifier + authorisationReference;
			}

			return authorisationReference;
		}

		ZString GetWarehouseLocationQualifier()
		{
			var result = LocationQualifierCodes.CustomsWarehouse;
			if (CusAuthorizationUsages.Any(u => u.AGC_Code.StartsWith(LocationQualifierCodes.FreeZone)))
			{
				result = LocationQualifierCodes.FreeZone;
			}
			else if (!CusAuthorizationUsages.Any(u => u.AGC_Code.StartsWith(LocationQualifierCodes.CustomsWarehouse)))
			{
				if (CusAuthorizationUsages.Any(u => u.AGC_Code == CusAuthorizationHeaderTypeCodes.ExciseWarehouse))
				{
					result = CusAuthorizationHeaderTypeCodes.ExciseWarehouse;
				}
				else if (CusAuthorizationUsages.Any(u => u.AGC_Code == CusAuthorizationHeaderTypeCodes.ExciseWarehouseHydrocarbonOils))
				{
					result = CusAuthorizationHeaderTypeCodes.ExciseWarehouseHydrocarbonOils;
				}
			}
			return result;
		}

		public ZPropertyInfo CurrentLocationFor523Info
		{
			get { return GetZPropertyInfo(Schema.CurrentLocationFor523); }
		}

		public ZString WarehouseTypeCode => base.OrgCusCodeTypeForWarehouse;

		public ZPropertyInfo CEI_SplitReferenceInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CEI_SplitReference, x => AddInfo.ZG_HouseSplitReferenceInfo); }
		}

		[UniversalCopyAddInfoPropertyMapping(AutoEUAddInfo.Schema.ZG_HouseSplitReference)]
		[MaxLength(Schema.CEI_SplitReferenceMaxLength)]
		public virtual ZString CEI_SplitReference
		{
			get { return AddInfo.ZG_HouseSplitReference; }

			set
			{
				AddInfo.ZG_HouseSplitReference = value;

				if (null != JobDeclaration)
				{
					var count = CEI_PackageCount;
					var hawbs = Factory.Load<CusHAWB>(new ZQuery(CusHAWBSchema.CS_JE_CustomsFormalEntry, JobDeclaration.PK));
					if (hawbs != null && hawbs.Length > 0)
					{
						var ukHawb = (from CusHAWB h in hawbs where h.MAWB != null && h.MAWB.CM_ApplicationCode == ApplicationCodeList.Codes.GbCcsuk select h).FirstOrDefault();
						if (ukHawb != null)
						{
							var splits = Array.Empty<CusPartShip>();
							if (JobDeclaration.ZG_ShipmentType == ShipmentTypeList.Codes.BasicDirect || ukHawb.CS_IsMasterHouse)
							{
								splits = Factory.Load<CusPartShip>(new ZQuery(CusPartShipSchema.CG_CM_LinkToPartMaster, ukHawb.MAWB.PK));
							}
							else if (JobDeclaration.ZG_ShipmentType == ShipmentTypeList.Codes.HouseConsignment)
							{
								splits = Factory.Load<CusPartShip>(new ZQuery(CusPartShipSchema.CG_CS, ukHawb.PK));
							}
							var split = (from CusPartShip s in splits where s.CG_MessageReference == value select s).FirstOrDefault();
							if (split != null)
							{
								count = split.CG_PiecesManifested; // NPX
							}
						}
					}
					CEI_PackageCount = count;
				}
			}
		}

		public virtual ZInt CEI_PackageCount
		{
			get { return AddInfo.ZG_PackageCount; }

			set { AddInfo.ZG_PackageCount = value; }
		}

		public ZPropertyInfo CEI_PackageCountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CEI_PackageCount, x => AddInfo.ZG_PackageCountInfo); }
		}

		public new JobDeclaration JobDeclaration => base.JobDeclaration as JobDeclaration;

		[MaxLength(Schema.CEI_SubStyleMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.EntrySubStyleList))]
		[ReadOnlyMember(nameof(CEI_SubStyleReadOnly))]
		public override ZString CEI_SubStyle
		{
			get => base.CEI_SubStyle;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_SubStyle))
				{
					var oldValue = CEI_SubStyle;
					base.CEI_SubStyle = value;
					if (oldValue != CEI_SubStyle)
					{
						JobDeclaration?.ApplicationExtender?.DefaultLocationOfGoods(JobDeclaration);
						JobDeclaration?.ApplicationExtender?.GetValueStrategy(JobDeclaration)?.DefaultEntryStyle(this);
					}
				}
			}
		}
		protected virtual bool CEI_SubStyleReadOnly => JobDeclaration != null && (HasAnyPreLodgedOrLodgedHeaders(this) || IsEntryStatusNeitherBlankOrRej(this));

		[MaxLength(Schema.CEI_AddInfoMaxLength)]
		public override ZString CEI_AddInfo
		{
			get => base.CEI_AddInfo;
			set
			{
				CheckMaximumLength(CEI_AddInfoInfo, value);
				base.CEI_AddInfo = value;
			}
		}

		[MaxLength(Schema.CEI_StyleMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.StyleList))]
		[ReadOnlyMember(nameof(CEI_StyleReadOnly))]
		public override ZString CEI_Style
		{
			get => base.CEI_Style;
			set
			{
				if (!SetterSuspender.IsSetterSuspended(CusEntryInstruction.Schema.CEI_Style))
				{
					var oldValue = CEI_Style;
					CheckMaximumLength(CEI_StyleInfo, value);
					base.CEI_Style = value;
					if (oldValue != CEI_Style)
					{
						JobDeclaration?.ApplicationExtender?.GetValueStrategy(JobDeclaration)?.DefaultEntrySubStyle(this);
					}
				}
			}
		}

		protected virtual bool CEI_StyleReadOnly => JobDeclaration != null && (HasAnyPreLodgedOrLodgedHeaders(this) || IsEntryStatusNeitherBlankOrRej(this));

		bool HasAnyPreLodgedOrLodgedHeaders(CusEntryInstruction entryInstruction) => entryInstruction.JobDeclaration.CustomsEntryHeaders.OfType<CusEntryHeader>().Any(x => x.CH_CEI_Instruction == entryInstruction.PK && x.IsPreLodgedOrLodgedWithCustoms);

		bool IsEntryStatusNeitherBlankOrRej(CusEntryInstruction entryInstruction)
		{
			var entryStatus = entryInstruction.EntryHeader?.CH_EntryStatus ?? ZString.Empty;
			return entryStatus != ThreeCharFunctionCode.Codes.REJ && !entryStatus.IsEmpty;
		}

		[MaxLength(Schema.CEI_DescriptionMaxLength)]
		public override ZString CEI_Description
		{
			get => base.CEI_Description;
			set
			{
				CheckMaximumLength(CEI_DescriptionInfo, value);
				base.CEI_Description = value;
			}
		}

		#region public AdditionalInfoCollection AdditionalInfos

		public new AdditionalInfoCollection AdditionalInfos => (AdditionalInfoCollection)base.AdditionalInfos;

		protected override EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection CreateNewAdditionalInfoCollection() => new AdditionalInfoCollection(this);

		#endregion

		string EU.Business.Declaration.MultiLineAddInfos.ICanBeImportOrExport.Level => EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header;

		protected override IDictionary<ZString, Type> GetCusSupportingInfoTypesCore()
		{
			var result = base.GetCusSupportingInfoTypesCore();
			result[Enterprise.Customs.Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo] = typeof(AdditionalInfo);
			return result;
		}

		protected override EU.Business.Declaration.AddInfoCusEntryInstruction GetNewAddInfo()
		{
			return new AddInfoCusEntryInstruction(CEI_AddInfoInfo);
		}

		protected override bool IsDescriptionDefaultedFromStyle => true;

		public new CusEntryInstructionLookups Lookups => new CusEntryInstructionLookups(this) ?? (CusEntryInstructionLookups)base.Lookups;
		protected override biz.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

		public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;
		protected override biz.CusEntryInstructionValidation GetNewValidation() => JobDeclaration?.ApplicationExtender?.GetNewCusEntryInstructionValidation(this) ?? new CusEntryInstructionValidation(this);
		protected override bool IsLookupsCachedInBase => false; // So that CHIEF and CDS DeclarationTypeList lookups work on CusEntryInstruction

		protected override IValueSetStrategy GetValueSetStrategy() => JobDeclaration?.ApplicationExtender?.GetNewCusEntryInstructionValueSetStrategy(this) ?? base.GetValueSetStrategy();

		public bool IsClearanceRequest =>
			StringComparer.OrdinalIgnoreCase.Equals(this.CEI_Style, ImportDeclarationTypeList.Codes.ImportClearanceRequestC21I)
				|| StringComparer.OrdinalIgnoreCase.Equals(this.CEI_Style, ImportDeclarationTypeList.Codes.ImportClearanceRequestC21N)
				|| StringComparer.OrdinalIgnoreCase.Equals(this.CEI_Style, ExportDeclarationTypeList.Codes.ExportClearanceRequestC21E)
				|| StringComparer.OrdinalIgnoreCase.Equals(this.CEI_Style, ExportDeclarationTypeList.Codes.ExportClearanceRequestC21EEIDRNOP);

		protected override EU.Business.Declaration.FiscalRepresentativeDefaulter GetFiscalRepresentativeDefaulterCore() => JobDeclaration?.ApplicationExtender?.GetFiscalRepresentativeDefaulter() ?? new EU.Business.Declaration.FiscalRepresentativeDefaulter();

		#region CalculatedDescription

		public ZString CalculatedDescription
		{
			get
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(CEI_Description);
				if (!CEI_DisplaySequence.IsEmpty)
				{
					result.Append($"({CEI_DisplaySequence})");
				}
				return result.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		public ZPropertyInfo CalculatedDescriptionInfo => GetZPropertyInfo(Schema.CalculatedDescription);

		#endregion

		#region IShortSequenceNumberLine

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => CEI_DisplaySequence; set => CEI_DisplaySequence = value; }

		ZGuid ISequenceNumberLine.FKToHeader => CEI_JE;

		#endregion

		[ResourceStringData("faccb57c-5468-40bb-83a5-4e6846997636", Caption = "Display Sequence")]
		public override ZShort CEI_DisplaySequence
		{
			get => base.CEI_DisplaySequence;
			set
			{
				var oldValue = CEI_DisplaySequence;
				base.CEI_DisplaySequence = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					JobDeclaration?.CusEntryInstructionSequenceNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}

		public override ZGuid CEI_JE
		{
			get => base.CEI_JE;
			set
			{
				var oldValue = CEI_JE;
				var oldJobDeclaration = JobDeclaration;
				base.CEI_JE = value;
				if (oldValue != CEI_JE && !IsCopying)
				{
					SetLineNoOnSettingCEI_JE(oldJobDeclaration);
				}
			}
		}

		public override void Delete()
		{
			JobDeclaration?.CusEntryInstructionSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			base.Delete();
		}

		void SetLineNoOnSettingCEI_JE(JobDeclaration oldJobDeclaration)
		{
			if (!IsCopying)
			{
				oldJobDeclaration?.CusEntryInstructionSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);

				JobDeclaration?.CusEntryInstructionSequenceNumberGenerator.RecalculateWhenAdded(this);
			}
		}

		[ReadOnlyMember(nameof(CEI_DateForDutyReadOnly))]
		public override ZDateTime CEI_DateForDuty
		{
			get => base.CEI_DateForDuty;
			set
			{
				var oldValue = CEI_DateForDuty;
				base.CEI_DateForDuty = value;
				if (!IsCopying && oldValue != CEI_DateForDuty)
				{
					JobDeclaration?.ResumeApportionment();
					JobDeclaration?.MarkInvoicesAsNeedingValidation();
				}
			}
		}

		protected virtual bool CEI_DateForDutyReadOnly => !((CusEntryHeader)EntryHeader)?.DateOfLegalAcceptance.IsEmpty ?? false;

		public bool IsGoodsNotArrivedSubStyle
		{
			get
			{
				switch (CEI_SubStyle.ToUpper())
				{
					case GBCodeList.EntrySubStyleListExport.Codes.C21_GoodsNotArrived_IECR:
					case GBCodeList.EntrySubStyleListExport.Codes.FullDeclarationGoodsNotArrived_IEFD:
					case GBCodeList.EntrySubStyleListExport.Codes.SDP_and_LCP_PSA_GoodsNotArrived_IELP_IESP:
					case GBCodeList.EntrySubStyleListImport.Codes.TransitSfdGoodsNotArrived:
					case EntrySubStyleCodeList.Codes.E:
						return true;
					default:
						return false;
				}
			}
		}

		public void ChangeSubStyleFromNotArrivedToArrived()
		{
			var subStyle = CEI_SubStyle;
			switch (subStyle)
			{
				case GBCodeList.EntrySubStyleListExport.Codes.C21_GoodsNotArrived_IECR:
					CEI_SubStyle = GBCodeList.EntrySubStyleListExport.Codes.C21_GoodsArrived_IECR;
					return;
				case GBCodeList.EntrySubStyleListExport.Codes.FullDeclarationGoodsNotArrived_IEFD:
					CEI_SubStyle = GBCodeList.EntrySubStyleListExport.Codes.FullDeclarationGoodsArrived_IEFD;
					return;
				case GBCodeList.EntrySubStyleListExport.Codes.SDP_and_LCP_PSA_GoodsNotArrived_IELP_IESP:
					CEI_SubStyle = GBCodeList.EntrySubStyleListExport.Codes.SDP_PSA_GoodsArrived_IESP;
					return;
				case GBCodeList.EntrySubStyleListImport.Codes.TransitSfdGoodsNotArrived:
					CEI_SubStyle = GBCodeList.EntrySubStyleListImport.Codes.TransitSfdGoodsArrived;
					return;
				case EntrySubStyleCodeList.Codes.E:
					CEI_SubStyle = EntrySubStyleCodeList.Codes.B;
					return;
			}
		}
	}
}
