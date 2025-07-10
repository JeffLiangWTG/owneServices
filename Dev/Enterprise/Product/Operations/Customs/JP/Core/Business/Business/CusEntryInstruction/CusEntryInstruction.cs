using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.JP.Business.DeclarationCargoTypeList;
using static Enterprise.Customs.JP.Business.JPImportDeclarationTypeList;
using static Enterprise.Integration.Customs.JP;

namespace Enterprise.Customs.JP.Business
{
	[CodeProperty(nameof(CodeProperty)), DescriptionProperty(nameof(DescriptionProperty))]
	public class CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : AutoJPCusEntryInstruction(factory, row),
		ICusEntryInstruction,
		Integration.Customs.ICusSupportingInfoTypeSupporter,
		IShortSequenceNumberLine,
		ISequenceNumberHeader,
		ICusOtherLawReferenceParent,
		ISupportMultipleResourceStringData
	{
		public new class Schema : AutoCusEntryInstruction.Schema
		{
			public const string DeleteExportControlEntryNumEnabled = nameof(CusEntryInstruction.DeleteExportControlEntryNumEnabled);
		}

		#region ExportControlNumber

		[ResourceStringData("03DA4B40-291C-4F77-93C5-E32E2C50E73A", Caption = "Export Control Number", ShortCaption = "ECN")]
		[ReadOnlyMember(nameof(ExportControlNumber_ReadOnly))]
		[BusinessObjectTestExclude()]
		public ZString ExportControlNumber
		{
			get => ExportControlEntryNum?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (ExportControlNumber != value)
				{
					var entryNumber = ExportControlEntryNum;
					if (value.IsEmpty)
					{
						entryNumber?.Delete();
					}
					else
					{
						if (entryNumber == null)
						{
							exportControlEntryNum = CreateNewEntryNumber(CusEntryNumberTypes.JP.ExportControlNumber, value);
							RegisterEditableChildObject(exportControlEntryNum);
						}
						else
						{
							entryNumber.CE_EntryNum = value;
							entryNumber.CE_EntryIsSystemGenerated = false;
						}
					}
				}

				ExportControlNumberInfo.RefreshBinding();
			}
		}

		bool ExportControlNumber_ReadOnly => IsExportControlEntryNumSystemGenerated;

		public bool IsExportControlEntryNumSystemGenerated => ExportControlEntryNum?.CE_EntryIsSystemGenerated ?? false;

		public ZBool DeleteExportControlEntryNumEnabled => Factory.GetCached(ref deleteExportControlEntryNumEnabledCached, () => HasExportControlNumber && IsExportControlEntryNumSystemGenerated);
		CachedProperty<ZBool> deleteExportControlEntryNumEnabledCached;

		public ZPropertyInfo ExportControlNumberInfo => GetZPropertyInfo(nameof(ExportControlNumber));

		CusEntryNumber ExportControlEntryNum
		{
			get
			{
				if (exportControlEntryNum == null || exportControlEntryNum.IsDeleted || exportControlEntryNum.CE_EntryType != CusEntryNumberTypes.JP.ExportControlNumber)
				{
					exportControlEntryNum = CusEntryNumber.Load(this, CusEntryNumberTypes.JP.ExportControlNumber, CountryCode);

					if (exportControlEntryNum != null)
					{
						RegisterEditableChildObject(exportControlEntryNum);
					}
				}

				return exportControlEntryNum;
			}
		}
		CusEntryNumber exportControlEntryNum;

		#endregion

		#region BillNumber

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_BillNumberType", Caption = "Bill Number Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BillNumberTypeList))]
		public override ZString CEI_BillNumberType { get => base.CEI_BillNumberType; set => base.CEI_BillNumberType = value; }

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|Air|CEI_BillNumber", Caption = "AWB Number", MultipleKey = AirCaptionKey)]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|Sea|CEI_BillNumber", Caption = "B/L Number", MultipleKey = SeaCaptionKey)]
		public ZString CEI_BillNumber
		{
			get => BillNumberEntryNum?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (CEI_BillNumber != value)
				{
					var entryNumber = BillNumberEntryNum;
					if (value.IsEmpty)
					{
						entryNumber?.Delete();
					}
					else
					{
						if (entryNumber == null)
						{
							billNumberEntryNum = CreateNewEntryNumber(CusEntryNumberTypes.JP.BillNumber, value);
							RegisterEditableChildObject(billNumberEntryNum);
						}
						else
						{
							CheckMaximumLength(CEI_BillNumberInfo, value);
							entryNumber.CE_EntryNum = value;
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateCEI_BillNumber();
					}
				}

				CEI_BillNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CEI_BillNumberInfo => GetZPropertyInfo(nameof(CEI_BillNumber));

		CusEntryNumber BillNumberEntryNum
		{
			get
			{
				if (billNumberEntryNum == null || billNumberEntryNum.IsDeleted || billNumberEntryNum.CE_EntryType != CusEntryNumberTypes.JP.BillNumber)
				{
					billNumberEntryNum = CusEntryNumber.Load(this, CusEntryNumberTypes.JP.BillNumber, CountryCode);

					if (billNumberEntryNum != null)
					{
						RegisterEditableChildObject(billNumberEntryNum);
					}
				}

				return billNumberEntryNum;
			}
		}
		CusEntryNumber billNumberEntryNum;

		#endregion

		public CusEntryNumber CreateNewEntryNumber(string entryType, string entryNumber, bool isSystemGenerated = false)
		{
			return CusEntryNumber.New<CusEntryNumber>(this, entryType, CountryCode, entryNumber, isSystemGenerated);
		}

		#region NSI

		[ResourceStringData("D22F7220-59A7-4CD2-9631-A789BE2AA06F", Caption = "N-S/I")]
		[MaxLength(35)]
		[BusinessObjectTestExclude()]
		public ZString NSI
		{
			get => NSIEntryNum?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (NSI != value)
				{
					var entryNumber = NSIEntryNum;
					if (value.IsEmpty)
					{
						entryNumber?.Delete();
					}
					else
					{
						if (entryNumber == null)
						{
							nsiEntryNum = CreateNewEntryNumber(CusEntryNumberTypes.JP.NSI, value);
							RegisterEditableChildObject(nsiEntryNum);
						}
						else
						{
							entryNumber.CE_EntryNum = value;
						}
					}
				}

				NSIInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NSIInfo => GetZPropertyInfo(nameof(NSI));

		CusEntryNumber NSIEntryNum
		{
			get
			{
				if (nsiEntryNum == null || nsiEntryNum.IsDeleted || nsiEntryNum.CE_EntryType != CusEntryNumberTypes.JP.NSI)
				{
					nsiEntryNum = CusEntryNumber.Load(this, CusEntryNumberTypes.JP.NSI, CountryCode);

					if (nsiEntryNum != null)
					{
						RegisterEditableChildObject(nsiEntryNum);
					}
				}

				return nsiEntryNum;
			}
		}
		CusEntryNumber nsiEntryNum;

		#endregion

		[ChildEditable(true)]
		public CusOtherLawReferenceCollection<CusOtherLawReference> OtherLaws
		{
			get
			{
				if (cusOtherLawReferences == null)
				{
					cusOtherLawReferences = new CusOtherLawReferenceCollection<CusOtherLawReference>(this);
					cusOtherLawReferences.Load();
					RegisterEditableChildObject(cusOtherLawReferences);
				}
				return cusOtherLawReferences;
			}
		}
		CusOtherLawReferenceCollection<CusOtherLawReference> cusOtherLawReferences;

		[ChildEditable(true)]
		public VanningAddressCollection VanningLocations
		{
			get
			{
				if (vanningLocations == null)
				{
					vanningLocations = new VanningAddressCollection(this);
					vanningLocations.Load();
					RegisterEditableChildObject(vanningLocations);
				}

				return vanningLocations;
			}
		}
		VanningAddressCollection vanningLocations;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => new TypedEnumerable<ISequenceNumberLine>(VanningLocations);

		internal ShortSequenceNumberGenerator VanningLocationsSeqGenerator => vanningLocationsSeqGenerator ?? (vanningLocationsSeqGenerator = new ShortSequenceNumberGenerator(this, () => 1, () => byte.MaxValue));
		ShortSequenceNumberGenerator vanningLocationsSeqGenerator;

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;

		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

		public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;

		protected override bool SupportsCloneCore() => true;

		[RelatedBusinessObject(nameof(SpecialCargoCode))]
		[ResourceStringData("A3DA392C-881C-47C0-94A2-9EBEC759D0C2", ShortCaption = "SPC.", Caption = "Special Cargo Code")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.SpecialCargoCodeList))]
		public override ZString CEI_SpecialCargoCode { get => base.CEI_SpecialCargoCode; set => base.CEI_SpecialCargoCode = value; }

		[ResourceStringData("C70F370E-3715-4EAA-8A04-C02C62A5F5B0", ShortCaption = "GD", Caption = "Goods Description")]
		public override ZString CEI_GoodsDescription { get => base.CEI_GoodsDescription; set => base.CEI_GoodsDescription = value; }

		public ZZRefCusCodeListCombined SpecialCargoCode => JPRefCusCodeListTypes.GetSpecialCargoCode(Factory, CEI_SpecialCargoCode);

		protected override ZString HumanReadableNameCore => Res.GetString("2a2bfe91-91c3-4f43-8309-2b655c324beb", "Entry Instruction");

		[ReadOnly(true)]
		[ResourceStringData("638C2CE5-0D19-47C0-AD6A-7177927863A7", ShortCaption = "#", MediumCaption = "Seq #", Caption = "Display Sequence")]
		public override ZShort CEI_DisplaySequence
		{
			get => base.CEI_DisplaySequence;
			set => base.CEI_DisplaySequence = value;
		}

		public override ZGuid CEI_JE
		{
			get => base.CEI_JE;
			set
			{
				if (CEI_JE != value)
				{
					var previousGenerator = SeqNumberGenerator;
					base.CEI_JE = value;

					if (!IsCopying)
					{
						previousGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(this);
						SeqNumberGenerator?.RecalculateWhenAdded(this);

						var declaration = JobDeclaration;
						if (declaration != null)
						{
							var bonedLocationCode = declaration.GetBondedLocationCode();
							var bonedLocationOrg = declaration.WarehouseDocAddress?.Organisation;

							if (!bonedLocationCode.IsEmpty || bonedLocationOrg != null)
							{
								var bonedLocationName = declaration.GetBondedLocationName();
								RefreshBondedLocationIfNeeded(bonedLocationCode, bonedLocationName);
							}

							DefaultBillNumberIfNeeded(declaration);
						}
					}

					JobDeclaration?.MarkAsNeedingValidation();
				}
			}
		}

		void DefaultBillNumberIfNeeded(JobDeclaration declaration)
		{
			if (CEI_BillNumber.IsEmpty && declaration.IsExportAndAir)
			{
				var houseBill = declaration.JE_HouseBill;
				if (!houseBill.IsEmpty)
				{
					CEI_BillNumber = houseBill;
				}
				else
				{
					CEI_BillNumber = declaration.JE_MasterBill;
				}
			}
		}

		internal void RefreshBondedLocationIfNeeded(ZString bondedLocationCode, ZString bonedLocationName)
		{
			if (CEI_BondedLocationCode.IsEmpty || IsUsingBasketBondedLocationCode)
			{
				CEI_BondedLocationCode = bondedLocationCode.IsEmpty ? (ZString)BasketBondedLocationCode : bondedLocationCode;
				CEI_BondedLocationName = bondedLocationCode.IsEmpty || bondedLocationCode == BasketBondedLocationCode
					? bonedLocationName.SubstringSafe(0, CEI_BondedLocationNameInfo.MaxLength)
					: ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ValueTypeList))]
		[ResourceStringData("CusEntryInstruction|CEI_ValueType", Caption = "Value Type", FullDescription = "If the total value of goods is 201,000 yen or more, \"L – Large Value\" will be applied, and if the total value of goods is less than 201,000 yen, \"S – Small Value\" will be applied.")]
		public override ZString CEI_ValueType
		{
			get => base.CEI_ValueType;
			set
			{
				if (base.CEI_ValueType != value)
				{
					base.CEI_ValueType = value;
					InvoiceLines.ForEach(x => x.MarkAsNeedingValidation());
					JobDeclaration?.ActiveEntryHeaders.Cast<CusEntryHeader>().ForEach(header => header.AllEntryLines.ForEach(line => line.MarkAsNeedingValidation()));
				}
			}
		}

		[ResourceStringData("CusEntryInstruction|CEI_DateForDuty", Caption = "Scheduled Declaration Date", ShortCaption = "Decl. Date", MediumCaption = "Declaration Date", FullDescription = "This is the scheduled declaration date. If not empty, this date will be used to select tariffs and rates for duty calculations.")]
		public override ZDateTime CEI_DateForDuty { get => base.CEI_DateForDuty; set => base.CEI_DateForDuty = value; }

		[LightValidationTestExempt]
		public override ZString CEI_AddInfo { get => base.CEI_AddInfo; set => base.CEI_AddInfo = value; }

		[ResourceStringData("CusEntryInstruction|CEI_PreInspectedCargoType", Caption = "Pre-Inspected Cargo Type", ShortCaption = "Pre-Inspected", MediumCaption = "Pre-Inspected Cargo")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.PreInspectedCargoTypeList))]
		public override ZString CEI_PreInspectedCargoType { get => base.CEI_PreInspectedCargoType; set => base.CEI_PreInspectedCargoType = value; }

		[ResourceStringData("CusEntryInstruction|CEI_LoadingConfirmationIsRequired", Caption = "Loading Confirmation", ShortCaption = "Loading Cfm.")]
		public override ZBool CEI_LoadingConfirmationIsRequired { get => base.CEI_LoadingConfirmationIsRequired; set => base.CEI_LoadingConfirmationIsRequired = value; }

		#region CEI_TradeType

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_TradeType", Caption = "Sign of trade form")]
		public override ZString CEI_TradeType { get => base.CEI_TradeType; set => base.CEI_TradeType = value; }

		[MaxLength(1)]
		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.TradeTypeFirstCharList))]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|TradeTypeFirstChar", Caption = "Trade Type First Character", MediumCaption = "Trade Type 1st Char", ShortCaption = "Trade Type 1")]
		public ZString TradeTypeFirstChar
		{
			get
			{
				var originChar = CEI_TradeType.SubstringSafe(0, 1);
				return originChar.Equals("_") ? ZString.Empty : originChar;
			}
			set
			{
				var oldValue = TradeTypeFirstChar;
				if (!value.Equals(oldValue))
				{
					SetTradeType(value, TradeTypeSecondChar, TradeTypeThirdChar);

					if (!IsValidationSuspended)
					{
						Validation.ValidateTradeTypeChar();
					}
					TradeTypeFirstCharInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TradeTypeFirstCharInfo
		{
			get { return GetZPropertyInfo(nameof(TradeTypeFirstChar)); }
		}

		[MaxLength(1)]
		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|TradeTypeSecondChar", Caption = "Trade Type Second Character", MediumCaption = "Trade Type 2nd Char", ShortCaption = "Trade Type 2")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.TradeTypeSecondCharList))]
		public ZString TradeTypeSecondChar
		{
			get
			{
				var originChar = CEI_TradeType.SubstringSafe(1, 1);
				return originChar.Equals("_") ? ZString.Empty : originChar;
			}
			set
			{
				var oldValue = TradeTypeSecondChar;
				if (!value.Equals(oldValue))
				{
					SetTradeType(TradeTypeFirstChar, value, TradeTypeThirdChar);

					if (!IsValidationSuspended)
					{
						Validation.ValidateTradeTypeChar();
					}
					TradeTypeSecondCharInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TradeTypeSecondCharInfo
		{
			get { return GetZPropertyInfo(nameof(TradeTypeSecondChar)); }
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.TradeTypeThirdCharList))]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|TradeTypeThirdChar", Caption = "Trade Type Third Character", MediumCaption = "Trade Type 3rd Char", ShortCaption = "Trade Type 3")]
		[BusinessObjectTestExclude]
		public ZString TradeTypeThirdChar
		{
			get
			{
				var originChar = CEI_TradeType.SubstringSafe(2, 1);
				return originChar.Equals("_") ? ZString.Empty : originChar;
			}
			set
			{
				var oldValue = TradeTypeThirdChar;
				if (!value.Equals(oldValue))
				{
					SetTradeType(TradeTypeFirstChar, TradeTypeSecondChar, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateTradeTypeChar();
					}
					TradeTypeThirdCharInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TradeTypeThirdCharInfo => GetZPropertyInfo(nameof(TradeTypeThirdChar));

		void SetTradeType(ZString firstChar, ZString secondChar, ZString thirdChar)
		{
			var paddingChar = new ZString("_");
			firstChar = firstChar.IsEmpty ? paddingChar : firstChar;
			secondChar = secondChar.IsEmpty ? paddingChar : secondChar;
			thirdChar = thirdChar.IsEmpty ? paddingChar : thirdChar;
			CEI_TradeType = firstChar + secondChar + thirdChar;
		}

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.DeclarationConditionList))]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_DeclarationCondition", Caption = "Declaration Condition", ShortCaption = "Condition")]
		public override ZString CEI_DeclarationCondition { get => base.CEI_DeclarationCondition; set => base.CEI_DeclarationCondition = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|DeclarationConditionDescription", Caption = "Declaration Condition Description", MediumCaption = "Condition Description", ShortCaption = "Condition Desc.", FullDescription = "The description of the Declaration Condition.")]
		public ZString DeclarationConditionDescription => Lookups.DeclarationConditionList.GetDescriptionFromCode(CEI_DeclarationCondition);

		public ZPropertyInfo DeclarationConditionDescriptionInfo => GetZPropertyInfo(nameof(DeclarationConditionDescription));

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_ApprovalCertificateCategory", Caption = "Export Approval Certificate Category", MediumCaption = "Approval Certificate Category", ShortCaption = "Appr. Cert. Cate.")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ApprovalCertificateCategoryList))]
		public override ZString CEI_ApprovalCertificateCategory { get => base.CEI_ApprovalCertificateCategory; set => base.CEI_ApprovalCertificateCategory = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_DutyDrawback", Caption = "Is tax return declaration?", ShortCaption = "Tax return?")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.YesNoList))]
		public override ZString CEI_DutyDrawback { get => base.CEI_DutyDrawback; set => base.CEI_DutyDrawback = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_CommercialValueType", Caption = "Approval certificate type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CommercialValueTypes))]
		public override ZString CEI_CommercialValueType { get => base.CEI_CommercialValueType; set => base.CEI_CommercialValueType = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_ContentInspectionResult", Caption = "Contents inspections results")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ContentInspectionResultList))]
		public override ZString CEI_ContentInspectionResult { get => base.CEI_ContentInspectionResult; set => base.CEI_ContentInspectionResult = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_CustomsInspectionCode", Caption = "Customs Inspection Code", MediumCaption = "Cus. Inspection Code", ShortCaption = "Cus. Ins. Co.")]
		public override ZString CEI_CustomsInspectionCode { get => base.CEI_CustomsInspectionCode; set => base.CEI_CustomsInspectionCode = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_CommonControlNumber", Caption = "Common Control Number", ShortCaption = "Com. Ctrl. No.")]
		public override ZString CEI_CommonControlNumber { get => base.CEI_CommonControlNumber; set => base.CEI_CommonControlNumber = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_FoodHygieneCertificateType", ShortCaption = "Food Cert. Id.", Caption = "Food hygiene certificate identification")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.IDACertificateIdList))]
		public override ZString CEI_FoodHygieneCertificateType { get => base.CEI_FoodHygieneCertificateType; set => base.CEI_FoodHygieneCertificateType = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_PlantProtectionCertificateType", ShortCaption = "Plant Cert. Id.", Caption = "Plant protection certificate identification")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.IDACertificateIdList))]
		public override ZString CEI_PlantProtectionCertificateType { get => base.CEI_PlantProtectionCertificateType; set => base.CEI_PlantProtectionCertificateType = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_AnimalQuarantineCertificateType", ShortCaption = "Animal Cert. Id.", Caption = "Animal quarantine certification identification")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.IDACertificateIdList))]
		public override ZString CEI_AnimalQuarantineCertificateType { get => base.CEI_AnimalQuarantineCertificateType; set => base.CEI_AnimalQuarantineCertificateType = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_BeforePermitApplicationReason", Caption = "BP Application Reason")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BeforePermitApplicationReasonList))]
		public override ZString CEI_BeforePermitApplicationReason { get => base.CEI_BeforePermitApplicationReason; set => base.CEI_BeforePermitApplicationReason = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_TradeControlOrder", Caption = "Import Trade Control Ordinance Article 3", MediumCaption = "Imp. Trade Ctrl. Ord. Art. 3", ShortCaption = "Trade Ctrl. Ord. Art. 3")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ImportTradeControlOrdinanceArticle3CodeList))]
		public override ZString CEI_TradeControlOrder { get => base.CEI_TradeControlOrder; set => base.CEI_TradeControlOrder = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_CustomsOfficeForSpecialDeclarations", ShortCaption = "Cus. Off. (Spe. Decl.)", MediumCaption = "Customs Office (Spe. Decl.)", Caption = "Customs Office (Special Declarations)", FullDescription = "Customs Office for Special Declarations")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CustomsOfficeList))]
		public override ZString CEI_CustomsOfficeForSpecialDeclarations
		{
			get => base.CEI_CustomsOfficeForSpecialDeclarations;
			set
			{
				if (value != CEI_CustomsOfficeForSpecialDeclarations)
				{
					if (!IsCopying)
					{
						CEI_CustomsOfficeDepartmentForSpecialDeclarations = ZString.Empty;
					}
					base.CEI_CustomsOfficeForSpecialDeclarations = value;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_CustomsOfficeDepartmentForSpecialDeclarations", ShortCaption = "Cus. Off. Dept. (Spe. Decl.)", MediumCaption = "Customs Office Dept. (Spe. Decl.)", Caption = "Customs Office Dept. (Special Declarations)", FullDescription = "Customs Office Department for Special Declarations")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CustomsOfficeDepartmentForSpecialDeclarationsList))]
		public override ZString CEI_CustomsOfficeDepartmentForSpecialDeclarations { get => base.CEI_CustomsOfficeDepartmentForSpecialDeclarations; set => base.CEI_CustomsOfficeDepartmentForSpecialDeclarations = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_BondedLocationCode", Caption = "Bonded Location Code")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BondedLocationList))]
		public override ZString CEI_BondedLocationCode
		{
			get => base.CEI_BondedLocationCode;
			set
			{
				base.CEI_BondedLocationCode = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCEI_BondedLocationName();
					JobDeclaration?.WarehouseDocAddress.Validation.ValidateE2_OA_Address();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_BondedLocationName", Caption = "Bonded Location Name")]
		public override ZString CEI_BondedLocationName { get => base.CEI_BondedLocationName; set => base.CEI_BondedLocationName = value; }

		protected bool CEI_BondedLocationName_ReadOnly => !IsUsingBasketBondedLocationCode;

		public bool IsUsingBasketBondedLocationCode => CEI_BondedLocationCode == BasketBondedLocationCode;

		public const string BasketBondedLocationCode = "99999";

		[ResourceStringData("AB6881D3-1CB2-4A36-A7F2-99E7E13ACBDD", Caption = "Gross Weight")]
		[DecimalPlaces(nameof(JPGrossWeightDecimalPlacesNum))]
		public override ZDecimal CEI_GrossWeight
		{
			get => base.CEI_GrossWeight;
			set
			{
				base.CEI_GrossWeight = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCEI_CustomsWeight();
				}
			}
		}

		public int JPGrossWeightDecimalPlacesNum => (JobDeclaration.IsAir && JobDeclaration.IsImport) ? 1 : 3;

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.GrossWeightUnitList))]
		public override ZString CEI_GrossWeightUnit
		{
			get => base.CEI_GrossWeightUnit;
			set
			{
				base.CEI_GrossWeightUnit = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCEI_CustomsWeight();
				}
			}
		}

		[ResourceStringData("8E300B5F-A04D-4BD5-979D-11CE4A6FBBD4", Caption = "Cargo Quantity")]
		public override ZDecimal CEI_CargoQuantity { get => base.CEI_CargoQuantity; set => base.CEI_CargoQuantity = value; }

		[ReadOnlyMember(nameof(CargoQuantityUnit_ReadOnly))]
		public override ZString CEI_CargoQuantityUnit
		{
			get => IsAir ? Constants.NoneUnit : base.CEI_CargoQuantityUnit;
			set => base.CEI_CargoQuantityUnit = value;
		}

		bool CargoQuantityUnit_ReadOnly => IsAir;

		[MaxLength(1)]
		[ResourceStringData("DDDE9415-574F-4C17-8C4C-B605CAFCC733", Caption = "Declaration Type")]
		public override ZString CEI_Style
		{
			get => base.CEI_Style;
			set
			{
				var previousValue = base.CEI_Style;
				base.CEI_Style = value;
				JobDeclaration?.MarkAsNeedingValidation();
				if (previousValue != value)
				{
					if (!IsCopying)
					{
						UpdateInvoiceLinesStorageTypeIfNeeded();
						UpdateInvoiceLineJPNACCSCodeIfNeeded();
						UpdateECRCargoTypeIfNeeded();
					}
				}

				if (IsExport)
				{
					DefualtAddApprovalCertificateInfo(JPExportDeclarationTypeList.Codes.G, ApprovalCertificateInfoCodes.ITNO);
					DefualtAddApprovalCertificateInfo(JPExportDeclarationTypeList.Codes.M, ApprovalCertificateInfoCodes.AEOM);
				}
			}
		}

		internal void UpdateInvoiceLinesStorageTypeIfNeeded()
		{
			InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line =>
			{
				line.UpdateStorageType(IsStorageTypeVisible);
				line.JI_ParentIDInfo.RefreshBinding();
			});
		}

		internal void UpdateInvoiceLineJPNACCSCodeIfNeeded()
		{
			if (IsImport && CEI_Style == JPImportDeclarationTypeList.Codes.Y)
			{
				InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line =>
				{
					line.JI_NACCSCode = ImportNACCSCodeList.Codes.X;
				});
			}
		}

		internal void UpdateECRCargoTypeIfNeeded()
		{
			if (IsExportAndSea && CEI_ECRCargoType.IsEmpty)
			{
				CEI_ECRCargoType = (string)CEI_Style switch
				{
					JPExportDeclarationTypeList.Codes.R or JPExportDeclarationTypeList.Codes.G => CargoTypeList.Codes.R,
					JPExportDeclarationTypeList.Codes.M or JPExportDeclarationTypeList.Codes.N or JPExportDeclarationTypeList.Codes.T => CargoTypeList.Codes.T,
					_ => ZString.Empty
				};
			}
			else
			{
				CEI_ECRCargoType = ZString.Empty;
			}
		}

		internal void DefualtAddApprovalCertificateInfo(string checkDeclarationType, string certificateType)
		{
			if (CEI_Style == checkDeclarationType && ApprovalCertificateInfos.Cast<ApprovalCertificateInfo>().All(c => !c.CSI_Code.Equals(certificateType)))
			{
				var approvalCertificateInfo = ApprovalCertificateInfos.AddNew();
				approvalCertificateInfo.CSI_Code = certificateType;
			}
		}

		public ZBool IsStorageTypeVisible => Factory.GetCached(ref isStorageTypeVisibleProperty, () =>
		{
			var result = false;
			if (!IsImport)
			{
				return result;
			}

			var declarationType = CEI_Style;
			if (IsAir)
			{
				switch (declarationType)
				{
					case JPImportDeclarationTypeList.Codes.S:
					case JPImportDeclarationTypeList.Codes.M:
					case JPImportDeclarationTypeList.Codes.A:
					case JPImportDeclarationTypeList.Codes.G:
						result = true;
						break;
				}
			}
			else if (IsSea)
			{
				switch (declarationType)
				{
					case JPImportDeclarationTypeList.Codes.C:
					case JPImportDeclarationTypeList.Codes.F:
					case JPImportDeclarationTypeList.Codes.S:
					case JPImportDeclarationTypeList.Codes.M:
					case JPImportDeclarationTypeList.Codes.A:
					case JPImportDeclarationTypeList.Codes.G:
					case JPImportDeclarationTypeList.Codes.K:
					case JPImportDeclarationTypeList.Codes.D:
					case JPImportDeclarationTypeList.Codes.U:
					case JPImportDeclarationTypeList.Codes.L:
					case JPImportDeclarationTypeList.Codes.B:
					case JPImportDeclarationTypeList.Codes.E:
					case JPImportDeclarationTypeList.Codes.R:
						result = true;
						break;
				}
			}

			return result;
		});
		CachedProperty<bool> isStorageTypeVisibleProperty;

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CargoTypeList))]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_CargoType", Caption = "Cargo Type (ECR)", ShortCaption = "Cargo Type")]
		public override ZString CEI_CargoType { get => base.CEI_CargoType; set => base.CEI_CargoType = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.DeclarationCargoTypes))]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_DeclarationCargoType", Caption = "Declaration Cargo Type", ShortCaption = "Decl. Cargo Type")]
		public override ZString CEI_DeclarationCargoType
		{
			get => base.CEI_DeclarationCargoType;
			set
			{
				var oldValue = CEI_DeclarationCargoType;
				base.CEI_DeclarationCargoType = value;
				if (!IsCopying && oldValue != CEI_DeclarationCargoType && JobDeclaration is JobDeclaration declaration)
				{
					declaration.IsMailedCargoInfo.RefreshBinding();
				}
			}
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.EntrySubStyleList))]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_SubStyle", Caption = "Additional Declaration Type", ShortCaption = "Add. Decl. Type")]
		public override ZString CEI_SubStyle { get => base.CEI_SubStyle; set => base.CEI_SubStyle = value; }

		[ResourceStringData("CBEDFB21-9F8D-8D21-CE88-76B6AE82D8E6", Caption = "Volume", ShortCaption = "Vol.")]
		public override ZDecimal CEI_Volume
		{
			get => base.CEI_Volume;
			set
			{
				base.CEI_Volume = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCEI_CustomsVolume();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.VolumeUnitList))]
		public override ZString CEI_VolumeUnit
		{
			get => base.CEI_VolumeUnit;
			set
			{
				base.CEI_VolumeUnit = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateCEI_CustomsVolume();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_ContainerCount", Caption = "Container Count")]
		public override ZInt CEI_ContainerCount { get => base.CEI_ContainerCount; set => base.CEI_ContainerCount = value; }

		#region Calculate Fieles
		[DecimalPlaces(3)]
		[ResourceStringData("07866674-5565-4B55-BCF6-4469B1F81A72", Caption = "Customs Weight", ShortCaption = "Cus. Wgt.")]
		public ZDecimal CEI_CustomsWeight
		{
			get => NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(CEI_GrossWeight, CEI_GrossWeightUnit);
			set
			{
				CEI_GrossWeight = NACCSUnitConverter.CalculateCustomsWeightInCW1Unit(value, CEI_CustomsWeightUnit);
				CEI_CustomsWeightInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CEI_CustomsWeightInfo => GetZPropertyInfo(nameof(CEI_CustomsWeight));

		public const decimal CustomsWeightMaxValue = 999999.999m;

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CustomsWeightUnitConditionList))]
		[ResourceStringData("50593338-2E17-4632-98A9-DDCE80BB59B6", Caption = "Customs Weight Unit", MediumCaption = "Cus. Wgt. Unit", ShortCaption = "UQ")]
		[BusinessObjectTestExclude]
		public ZString CEI_CustomsWeightUnit
		{
			get => NACCSUnitConverter.ConvertToCustomsWeightUnit(CEI_GrossWeightUnit);
			set
			{
				CEI_GrossWeightUnit = NACCSUnitConverter.ConvertJPCustomsWeightUnitToCW1WeightUnit(value);
				CEI_CustomsWeightUnitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CEI_CustomsWeightUnitInfo => GetZPropertyInfo(nameof(CEI_CustomsWeightUnit));

		[DecimalPlaces(3)]
		[ResourceStringData("3FF847F5-CB97-44B8-8B5F-892712EC25AC", Caption = "Customs Volume", ShortCaption = "Cus. Vol.")]
		public ZDecimal CEI_CustomsVolume
		{
			get => NACCSUnitConverter.CalculateVolumeInCustomsVolumeUnit(CEI_Volume, CEI_VolumeUnit);
			set
			{
				CEI_Volume = NACCSUnitConverter.CalculateCustomsVolumeInCW1Unit(value, CEI_CustomsVolumeUnit);
				CEI_CustomsVolumeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CEI_CustomsVolumeInfo => GetZPropertyInfo(nameof(CEI_CustomsVolume));

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CustomsVolumeUnitList))]
		[ResourceStringData("5E4FE09E-9405-4EE5-8A27-E8960ABA1B66", Caption = "Customs Volume Unit", MediumCaption = "Cus. Vol. Unit", ShortCaption = "UQ")]
		[BusinessObjectTestExclude]
		public ZString CEI_CustomsVolumeUnit
		{
			get => NACCSUnitConverter.ConvertToCustomsVolumeUnit(CEI_VolumeUnit);
			set
			{
				CEI_VolumeUnit = NACCSUnitConverter.ConvertJPCustomsVolumeUnitToCW1VolumeUnit(value);
				CEI_CustomsVolumeUnitInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CEI_CustomsVolumeUnitInfo => GetZPropertyInfo(nameof(CEI_CustomsVolumeUnit));
		#endregion

		[ChildEditable]
		public MoveInDestinationCollection MoveInDestinationInfos
		{
			get
			{
				if (moveInDestinationInfos == null)
				{
					moveInDestinationInfos = new MoveInDestinationCollection(this);
					moveInDestinationInfos.Load();
					RegisterEditableChildObject(moveInDestinationInfos);
				}

				return moveInDestinationInfos;
			}
		}
		MoveInDestinationCollection moveInDestinationInfos;

		[ChildEditable]
		public ApprovalCertificateInfoCollection ApprovalCertificateInfos
		{
			get
			{
				if (approvalCertificateInfos == null)
				{
					approvalCertificateInfos = new ApprovalCertificateInfoCollection(this);
					approvalCertificateInfos.Load();
					RegisterEditableChildObject(approvalCertificateInfos);
				}

				return approvalCertificateInfos;
			}
		}
		ApprovalCertificateInfoCollection approvalCertificateInfos;

		[ChildEditable]
		public CusGuaranteeReferenceCollection Guarantees
		{
			get
			{
				if (guarantees == null)
				{
					guarantees = new CusGuaranteeReferenceCollection(this);
					guarantees.Load();
					RegisterEditableChildObject(guarantees);
				}

				return guarantees;
			}
		}
		CusGuaranteeReferenceCollection guarantees;

		public ZBool IsOnlyContainSmallValueGoods => Factory.GetCached(ref isOnlyContainSmallValueGoods, () => GetIsOnlyContainSmallValueGoods());

		CachedProperty<ZBool> isOnlyContainSmallValueGoods;

		bool GetIsOnlyContainSmallValueGoods()
		{
			var entryHeader = EntryHeader;
			if (entryHeader != null)
			{
				var lowerThresholdValue = LowerThresholdValueForJapanLargeValueDeclarations;

				var entryLines = entryHeader.AllEntryLines.Cast<CusEntryLine>();
				var entryLinesX = entryLines.Where(x => x.RandomLine.JI_NACCSCode == ExportNACCSCodeList.Codes.X);
				if (entryLinesX.All(x => x.CL_CustomsValue < lowerThresholdValue))
				{
					var entryLinesY = entryLines.Where(x => x.RandomLine.JI_NACCSCode.Equals(ExportNACCSCodeList.Codes.Y));
					var entryLinesY_GroupsByTariff = entryLinesY.GroupBy(x => x.RandomLine.JI_Tariff);
					var sumsOfCustomsValue_EntryLinesY_GroupByTariff = entryLinesY_GroupsByTariff.Select(group => group.Sum(x => x.CL_CustomsValue));
					if (sumsOfCustomsValue_EntryLinesY_GroupByTariff.All(x => x < lowerThresholdValue))
					{
						var entryLinesNotXNotY = entryLines.Where(x => !x.RandomLine.JI_NACCSCode.Equals(ExportNACCSCodeList.Codes.X) && !x.RandomLine.JI_NACCSCode.Equals(ImportNACCSCodeList.Codes.Y));
						var entryLinesNotXNotY_GroupsByTariff = entryLinesNotXNotY.GroupBy(x => x.RandomLine.JI_Tariff);
						var sumsOfCustomsValue_EntryLinesNotXNotY_GroupsByTariff = entryLinesNotXNotY_GroupsByTariff.Select(group => group.Sum(x => x.CL_CustomsValue));
						if (sumsOfCustomsValue_EntryLinesNotXNotY_GroupsByTariff.All(x => x < lowerThresholdValue))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		#region RefSysConfig

		public ZDecimal LowerThresholdValueForJapanLargeValueDeclarations => RefSysConfigLoader.GetDecimalValue(Common.Constants.RefSysConfigCodes.LowerThresholdValueForJapanLargeValueDeclarations, 200100, ZDateTime.Today);

		RefSysConfig.Loader RefSysConfigLoader => refSysConfigLoader ??= new RefSysConfig.Loader(Factory);

		RefSysConfig.Loader refSysConfigLoader;

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				RemoveAndDeleteRelatedEntryHeaders();

				var generator = SeqNumberGenerator;
				generator?.RecalculateWhenAboutToBeDetachedOrDeleted(this);

				using (generator?.GetLineNumberSuspender())
				{
					base.Delete();
				}
			}
		}

		void RemoveAndDeleteRelatedEntryHeaders()
		{
			var entryHeaders = JobDeclaration.ActiveEntryHeaders;
			var linkedHeaders = entryHeaders.Where(x => x.CH_CEI_Instruction == PK).ToList();
			entryHeaders.RemoveRange(linkedHeaders);
			linkedHeaders.DeleteAll();
		}

		public override bool CanDelete => base.CanDelete && !IsLinkedAnyMessages;

		bool IsLinkedAnyMessages => Factory.GetCached(ref isLinkedAnyMessagesProperty, () => JobDeclaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Any(x => x.CH_CEI_Instruction == PK && x.Messages.Count > 0));
		CachedProperty<ZBool> isLinkedAnyMessagesProperty;

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_ECRCargoType", Caption = "ECR Cargo Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CargoTypeList))]
		public override ZString CEI_ECRCargoType { get => base.CEI_ECRCargoType; set => base.CEI_ECRCargoType = value; }

		#region CDB01

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_CDB01CargoType", Caption = "CDB01 Cargo Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.CDB01CargoTypeList))]
		public override ZString CEI_CDB01CargoType { get => base.CEI_CDB01CargoType; set => base.CEI_CDB01CargoType = value; }

		[MaxLength(AutoJPCusEntryInstruction.Schema.CEI_CDB01PermitNoMaxLength)]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_CDB01PermitNo", Caption = "CDB01 Permit No")]
		public override ZString CEI_CDB01PermitNo { get => base.CEI_CDB01PermitNo; set => base.CEI_CDB01PermitNo = value; }

		[ResourceStringData("C654B369-3F21-45DF-8496-E045033BBB2A", Caption = "Move-In Notice")]
		[ReadOnlyMember(nameof(MoveInNotice_ReadOnly))]
		[MaxLength(7)]
		[BusinessObjectTestExclude()]
		public ZString MoveInNotice
		{
			get => MoveInNoticeEntryNum?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (MoveInNotice != value)
				{
					var entryNumber = MoveInNoticeEntryNum;
					if (entryNumber == null)
					{
						moveInNoticeEntryNum = CreateNewEntryNumber(CusEntryNumberTypes.JP.MoveInNotice, value);
						RegisterEditableChildObject(moveInNoticeEntryNum);
					}
					else
					{
						entryNumber.CE_EntryNum = value;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateMoveInNotice();
					}
				}

				MoveInNoticeInfo.RefreshBinding();
			}
		}

		bool MoveInNotice_ReadOnly => RequestMoveInNotice;

		public ZPropertyInfo MoveInNoticeInfo => GetZPropertyInfo(nameof(MoveInNotice));

		[ResourceStringData("5FDE1D0C-219F-4A39-9872-4BD8187B02E6", Caption = "Request Move-In Notice")]
		[ReadOnlyMember(nameof(RequestMoveInNotice_ReadOnly))]
		public ZBool RequestMoveInNotice
		{
			get => MoveInNoticeEntryNum?.CE_EntryIsSystemGenerated ?? true;
			set
			{
				if (RequestMoveInNotice != value)
				{
					var entryNumber = MoveInNoticeEntryNum;
					if (value && entryNumber != null && entryNumber.CE_EntryNum.IsEmpty)
					{
						entryNumber.Delete();
					}
					else
					{
						if (entryNumber == null)
						{
							moveInNoticeEntryNum = CreateNewEntryNumber(CusEntryNumberTypes.JP.MoveInNotice, ZString.Empty, value);
							RegisterEditableChildObject(moveInNoticeEntryNum);
						}
						else
						{
							entryNumber.CE_EntryIsSystemGenerated = value;
						}
					}
				}

				RequestMoveInNoticeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo RequestMoveInNoticeInfo => GetZPropertyInfo(nameof(RequestMoveInNotice));

		bool RequestMoveInNotice_ReadOnly => !MoveInNotice.IsEmpty;

		CusEntryNumber MoveInNoticeEntryNum
		{
			get
			{
				if (moveInNoticeEntryNum == null || moveInNoticeEntryNum.IsDeleted || moveInNoticeEntryNum.CE_EntryType != CusEntryNumberTypes.JP.MoveInNotice)
				{
					moveInNoticeEntryNum = CusEntryNumber.Load(this, CusEntryNumberTypes.JP.MoveInNotice, CountryCode);

					if (moveInNoticeEntryNum != null)
					{
						RegisterEditableChildObject(moveInNoticeEntryNum);
					}
				}

				return moveInNoticeEntryNum;
			}
		}
		CusEntryNumber moveInNoticeEntryNum;

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|MoveInDestination", Caption = "Move-In Date")]
		public ZDateTime MoveInDate
		{
			get => MoveInDestinationInfos.Any() ? MoveInDestinationInfos[0].CSI_DateOfIssue : ZDateTime.Empty;
			set
			{
				SetMoveInDestinationInfos(moveInDestinationInfo => moveInDestinationInfo.CSI_DateOfIssue = value);
			}
		}

		public ZPropertyInfo MoveInDateInfo => GetMoveInDestinationInfosInfo(nameof(MoveInDate), moveInDestinationInfo => moveInDestinationInfo.CSI_DateOfIssueInfo);

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|MoveInDestination", Caption = "Move-In Destination")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.MoveInDestinationCodeList))]
		public ZString MoveInDestination
		{
			get => MoveInDestinationInfos.Any() ? MoveInDestinationInfos[0].CSI_Code : ZString.Empty;
			set
			{
				SetMoveInDestinationInfos(moveInDestinationInfo => moveInDestinationInfo.CSI_Code = value);
			}
		}

		public ZPropertyInfo MoveInDestinationInfo => GetMoveInDestinationInfosInfo(nameof(MoveInDestination), moveInDestinationInfo => moveInDestinationInfo.CSI_CodeInfo);

		[DecimalPlaces(0)]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|MoveInQuantity", Caption = "Move-In Quantity")]
		public ZDecimal MoveInQuantity
		{
			get => MoveInDestinationInfos.Any() ? MoveInDestinationInfos[0].CSI_Quantity : ZDecimal.Zero;
			set
			{
				SetMoveInDestinationInfos(moveInDestinationInfo => moveInDestinationInfo.CSI_Quantity = value);
			}
		}

		public ZPropertyInfo MoveInQuantityInfo => GetMoveInDestinationInfosInfo(nameof(MoveInQuantity), moveInDestinationInfo => moveInDestinationInfo.CSI_QuantityInfo);

		[DecimalPlaces(nameof(MoveInWeightDecimalPlaces))]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|MoveInWeight", Caption = "Move-In Weight")]
		public ZDecimal MoveInWeight
		{
			get => MoveInDestinationInfos.Any() ? MoveInDestinationInfos[0].CSI_Quantity2 : ZDecimal.Zero;
			set
			{
				SetMoveInDestinationInfos(moveInDestinationInfo => moveInDestinationInfo.CSI_Quantity2 = value);
			}
		}

		int MoveInWeightDecimalPlaces => IsAir ? 1 : 3;

		public ZPropertyInfo MoveInWeightInfo => GetMoveInDestinationInfosInfo(nameof(MoveInWeight), moveInDestinationInfo => moveInDestinationInfo.CSI_Quantity2Info);

		void SetMoveInDestinationInfos(Action<MoveInDestination> action)
		{
			var firstMoveInDestinationInfos = MoveInDestinationInfos.Count <= 0 ? MoveInDestinationInfos.AddNew() : MoveInDestinationInfos[0];
			action.Invoke(firstMoveInDestinationInfos);
		}

		ZPropertyInfo GetMoveInDestinationInfosInfo(string propertyName, Func<MoveInDestination, ZPropertyInfo> func) => MoveInDestinationInfos.Count <= 0 ? GetZPropertyInfo(propertyName) : GetWrappedZPropertyInfo(propertyName, x => func.Invoke(MoveInDestinationInfos[0]));

		#endregion

		#region Notes

		#region Customs Notes

		[MaxLength(140)]
		[ResourceStringData("85240694-93ea-44a0-8e97-31b981e5b217", Caption = "Notes (Customs)")]
		public ZString JP_CustomsNotes
		{
			get
			{
				return (CustomsNotes != null && !CustomsNotes.IsDeleted) ? CustomsNotes.ST_NoteText : (CEI_BondedLocationName.IsEmpty ? ZString.Empty : new ZString($"蔵入等先保税地域名: {CEI_BondedLocationName}"));
			}
			set
			{
				if (!value.IsEmpty)
				{
					CheckMaximumLength(JP_CustomsNotesInfo, value);

					if (CustomsNotes == null)
					{
						CustomsNotes = CreateNote(Constants.StmNoteDescriptions.CustomsNotesDescription);
						RegisterEditableChildObject(CustomsNotes);
					}

					CustomsNotes.ST_NoteText = value;
				}
				else
				{
					if (CustomsNotes != null)
					{
						CustomsNotes.Delete();
						CustomsNotes = null;
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateJP_CustomsNotes();
				}

				JP_CustomsNotesInfo.RefreshBinding();
			}
		}

		StmNote CustomsNotes
		{
			get
			{
				if (fCustomsNotes == null)
				{
					fCustomsNotes = LoadNote(Constants.StmNoteDescriptions.CustomsNotesDescription);
					if (fCustomsNotes != null)
					{
						RegisterEditableChildObject(fCustomsNotes);
					}
				}

				return fCustomsNotes;
			}
			set
			{
				fCustomsNotes = value;
			}
		}
		StmNote fCustomsNotes;

		public ZPropertyInfo JP_CustomsNotesInfo
		{
			get { return GetZPropertyInfo(nameof(JP_CustomsNotes)); }
		}

		protected bool JP_CustomsNotes_ReadOnly => !JP_CustomsNotes_Override && CustomsNotes == null && !JP_CustomsNotes.IsEmpty;

		public ZBool JP_CustomsNotes_Override
		{
			get => customsNotes_Override;
			set
			{
				if (customsNotes_Override && customsNotes_Override != value)
				{
					JP_CustomsNotes = ZString.Empty;
				}

				customsNotes_Override = value;
				JP_CustomsNotesInfo.RefreshBinding();
			}
		}
		ZBool customsNotes_Override;

		#endregion

		#region Brokers Notes

		[MaxLength(70)]
		[ResourceStringData("cca6fab7-51f8-47d3-8312-78c883e0cf33", Caption = "Notes (Customs Broker)")]
		public ZString JP_BrokersNotes
		{
			get
			{
				return (BrokersNotes != null && !BrokersNotes.IsDeleted) ? BrokersNotes.ST_NoteText : ZString.Empty;
			}
			set
			{
				if (!value.IsEmpty)
				{
					CheckMaximumLength(JP_BrokersNotesInfo, value);

					if (BrokersNotes == null)
					{
						BrokersNotes = CreateNote(Constants.StmNoteDescriptions.BrokersNotesDescription);
						RegisterEditableChildObject(BrokersNotes);
					}

					BrokersNotes.ST_NoteText = value;
				}
				else
				{
					if (BrokersNotes != null)
					{
						BrokersNotes.Delete();
						BrokersNotes = null;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJP_BrokersNotes();
				}

				JP_BrokersNotesInfo.RefreshBinding();
			}
		}

		StmNote BrokersNotes
		{
			get
			{
				if (fBrokersNotes == null)
				{
					fBrokersNotes = LoadNote(Constants.StmNoteDescriptions.BrokersNotesDescription);
					if (fBrokersNotes != null)
					{
						RegisterEditableChildObject(fBrokersNotes);
					}
				}

				return fBrokersNotes;
			}
			set
			{
				fBrokersNotes = value;
			}
		}
		StmNote fBrokersNotes;

		public ZPropertyInfo JP_BrokersNotesInfo
		{
			get { return GetZPropertyInfo(nameof(JP_BrokersNotes)); }
		}

		#endregion

		#region OwnersNotes

		[MaxLength(70)]
		[ResourceStringData("013f5c55-74a9-43b1-87be-b6cdfb45fda1", Caption = "Notes (Owner)")]
		public ZString JP_OwnersNotes
		{
			get
			{
				return (OwnersNotes != null && !OwnersNotes.IsDeleted) ? OwnersNotes.ST_NoteText : ZString.Empty;
			}
			set
			{
				if (!value.IsEmpty)
				{
					CheckMaximumLength(JP_OwnersNotesInfo, value);

					if (OwnersNotes == null)
					{
						OwnersNotes = CreateNote(Constants.StmNoteDescriptions.OwnersNotesDescription);
						RegisterEditableChildObject(OwnersNotes);
					}

					OwnersNotes.ST_NoteText = value;
				}
				else
				{
					if (OwnersNotes != null)
					{
						OwnersNotes.Delete();
						OwnersNotes = null;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJP_OwnersNotes();
				}

				JP_OwnersNotesInfo.RefreshBinding();
			}
		}

		StmNote OwnersNotes
		{
			get
			{
				if (fOwnersNotes == null)
				{
					fOwnersNotes = LoadNote(Constants.StmNoteDescriptions.OwnersNotesDescription);
					if (fOwnersNotes != null)
					{
						RegisterEditableChildObject(fOwnersNotes);
					}
				}

				return fOwnersNotes;
			}
			set
			{
				fOwnersNotes = value;
			}
		}
		StmNote fOwnersNotes;

		public ZPropertyInfo JP_OwnersNotesInfo
		{
			get { return GetZPropertyInfo(nameof(JP_OwnersNotes)); }
		}

		#endregion

		#region Marks and Numbers

		[MaxLength(140)]
		[ResourceStringData("20A12672-4AF1-446D-8161-EA3DC062921F", Caption = "Marks & Numbers")]
		public ZString JP_MarksAndNumbers
		{
			get
			{
				return (MarksAndNumbers != null && !MarksAndNumbers.IsDeleted) ? MarksAndNumbers.ST_NoteText : ZString.Empty;
			}
			set
			{
				if (!value.IsEmpty)
				{
					CheckMaximumLength(JP_MarksAndNumbersInfo, value);

					if (MarksAndNumbers == null)
					{
						MarksAndNumbers = CreateNote(Constants.StmNoteDescriptions.MarksAndNumbersDescription);
						RegisterEditableChildObject(MarksAndNumbers);
					}

					MarksAndNumbers.ST_NoteText = value;
				}
				else
				{
					if (MarksAndNumbers != null)
					{
						MarksAndNumbers.Delete();
						MarksAndNumbers = null;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJP_MarksAndNumbers();
				}

				JP_MarksAndNumbersInfo.RefreshBinding();
			}
		}

		StmNote MarksAndNumbers
		{
			get
			{
				if (fMarksAndNumbers == null)
				{
					fMarksAndNumbers = LoadNote(Constants.StmNoteDescriptions.MarksAndNumbersDescription);
					if (fMarksAndNumbers != null)
					{
						RegisterEditableChildObject(fMarksAndNumbers);
					}
				}

				return fMarksAndNumbers;
			}
			set
			{
				fMarksAndNumbers = value;
			}
		}
		StmNote fMarksAndNumbers;

		public ZPropertyInfo JP_MarksAndNumbersInfo
		{
			get { return GetZPropertyInfo(nameof(JP_MarksAndNumbers)); }
		}
		#endregion

		#region ECR Notes

		[MaxLength(140)]
		[ResourceStringData("73410A05-253B-491B-AD85-13D3A0E1F5EA", Caption = "ECR Notes")]
		public ZString JP_ECRNotes
		{
			get
			{
				return (ECRNotes != null && !ECRNotes.IsDeleted) ? ECRNotes.ST_NoteText : ZString.Empty;
			}
			set
			{
				if (!value.IsEmpty)
				{
					CheckMaximumLength(JP_ECRNotesInfo, value);

					if (ECRNotes == null)
					{
						ECRNotes = CreateNote(Constants.StmNoteDescriptions.ECRNotesDescription);
						RegisterEditableChildObject(ECRNotes);
					}

					ECRNotes.ST_NoteText = value;
				}
				else
				{
					if (ECRNotes != null)
					{
						ECRNotes.Delete();
						ECRNotes = null;
					}
				}

				JP_ECRNotesInfo.RefreshBinding();
			}
		}

		StmNote ECRNotes
		{
			get
			{
				if (fECRNotes == null)
				{
					fECRNotes = LoadNote(Constants.StmNoteDescriptions.ECRNotesDescription);
					if (fECRNotes != null)
					{
						RegisterEditableChildObject(fECRNotes);
					}
				}

				return fECRNotes;
			}
			set
			{
				fECRNotes = value;
			}
		}
		StmNote fECRNotes;

		public ZPropertyInfo JP_ECRNotesInfo => GetZPropertyInfo(nameof(JP_ECRNotes));
		#endregion

		#region ECR Main Details
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_ExporterCode", Caption = "Exporter Code")]
		public ZString CEI_ExporterCode => SupplierJobDocAddressProvider.Code;

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_ExporterName", Caption = "Exporter Name")]
		public ZString CEI_ExporterName => SupplierJobDocAddressProvider.Name;

		JobDocAddressProvider SupplierJobDocAddressProvider => JobDeclaration.Factory.GetValue(ref supplierJobDocAddressProviderCached, () => new(JobDeclaration.SupplierDocumentaryAddress));
		CachedProperty<JobDocAddressProvider> supplierJobDocAddressProviderCached;

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_DeclarantCode", Caption = "Declarant Code")]
		public ZString CEI_DeclarantCode => JobDeclaration.ExternalBrokerCode;

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_DeclarationReference", Caption = "Internal Reference Number", ShortCaption = "Internal Ref.")]
		public ZString CEI_DeclarationReference => JobDeclaration.JE_DeclarationReference;
		#endregion

		StmNote CreateNote(string description)
		{
			var result = Notes.AddNew();
			result.ST_Description = description;
			result.ST_ParentID = PK;
			result.ST_Table = CusEntryInstructionSchema.Constants.TableName;

			return result;
		}

		StmNote LoadNote(string description) => Notes.FindByDescription(description).SingleOrDefault();

		#endregion

		#region ICusSupportingInfoTypeSupporter

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
		{
			{ CusSupportingInfoTypeList.Codes.ApprovalCertificate, typeof(ApprovalCertificateInfo) },
			{ CusSupportingInfoTypeList.Codes.ExpectedMoveInDestination, typeof(MoveInDestination) },
			{ CusSupportingInfoTypeList.Codes.Inventory, typeof(InventoryInfo) },
		};

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

		#region ISequenceNumberLine

		ZGuid ISequenceNumberLine.FKToHeader => CEI_JE;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => CEI_DisplaySequence; set => CEI_DisplaySequence = value; }

		BaseSequenceNumberGenerator<ZShort, IShortSequenceNumberLine> SeqNumberGenerator => JobDeclaration?.CustomsEntryInstructions?.SeqNumberGenerator;

		#endregion

		#region RCR
		[MaxLength(1)]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_RCRAction", Caption = "RCR Action")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.RCRActionList))]
		public override ZString CEI_RCRAction { get => base.CEI_RCRAction; set => base.CEI_RCRAction = value; }

		[MaxLength(5)]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ViaList))]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_ViaLocation", Caption = "Via", FullDescription = "Enter the Vanning Location Code if the Vanning Location is different from the clearance location.")]
		public override ZString CEI_ViaLocation { get => base.CEI_ViaLocation; set => base.CEI_ViaLocation = value; }

		[MaxLength(35)]
		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|CEI_PreviousBillNumber", Caption = "Previous Bill Number", ShortCaption = "Previous B/L")]
		public override ZString CEI_PreviousBillNumber { get => base.CEI_PreviousBillNumber; set => base.CEI_PreviousBillNumber = value; }

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|ReceiptMode", ShortCaption = "RM", Caption = "Receipt Mode")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ReceiptModeList))]
		[MaxLength(AutoJPJobDeclaration.Schema.JE_ReceiptModeMaxLength)]
		public ZString ReceiptMode
		{
			get => JobDeclaration.JE_ReceiptMode;
			set
			{
				if (value != ReceiptMode && !IsCopying)
				{
					JobDeclaration.JE_ReceiptMode = value;
					ReceiptModeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ReceiptModeInfo => GetZPropertyInfo(nameof(ReceiptMode));

		[ResourceStringData("Enterprise.Customs.JP.Business.CusEntryInstruction|FinalDestination", Caption = "Final Destination")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.FinalDestinations))]
		[MaxLength(JobDeclaration.Schema.JE_RL_NKFinalDestinationMaxLength)]
		public ZString FinalDestination
		{
			get => JobDeclaration.JE_RL_NKFinalDestination;
			set
			{
				if (value != FinalDestination && !IsCopying)
				{
					JobDeclaration.JE_RL_NKFinalDestination = value;
					FinalDestinationInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo FinalDestinationInfo => GetZPropertyInfo(nameof(FinalDestination));
		#endregion

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var result = base.NoteTypesCore;
				result.Add(new PredefinedNoteType((NoResString)Constants.StmNoteDescriptions.CustomsNotesDescription, StmNoteVisibility.INT, true, false, true, true));
				result.Add(new PredefinedNoteType((NoResString)Constants.StmNoteDescriptions.OwnersNotesDescription, StmNoteVisibility.INT, true, false, true, true));
				result.Add(new PredefinedNoteType((NoResString)Constants.StmNoteDescriptions.BrokersNotesDescription, StmNoteVisibility.INT, true, false, true, true));
				result.Add(new PredefinedNoteType((NoResString)Constants.StmNoteDescriptions.MarksAndNumbersDescription, StmNoteVisibility.INT, true, false, true, true));
				result.Add(new PredefinedNoteType((NoResString)Constants.StmNoteDescriptions.ECRNotesDescription, StmNoteVisibility.INT, true, false, true, true));
				return result;
			}
		}

		public ZString CodeProperty => CEI_DisplaySequence.ToString();

		public ZString DescriptionProperty
		{
			get
			{
				var description = new ZStringBuilder();
				description.AppendIfNotEmpty(CEI_Description);
				if (IsSea)
				{
					description.AppendIfNotEmpty(ExportControlNumber);
				}
				else if (IsAir)
				{
					description.AppendIfNotEmpty(CEI_BillNumber);
				}
				description.AppendIfNotEmpty(CEI_Style);
				description.AppendIfNotEmpty(CEI_SubStyle);

				return description.ToStringWithDelimiterBetweenAppends("|");
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CEI_ValueType = ValueTypeList.Codes.L;
		}

		public ZString EntryInstructionDescription => IsExportAndSea ? CEI_GoodsDescription : CEI_Description;

		internal bool IsExportAndSea => JobDeclaration?.IsExportAndSea ?? false;

		internal bool HasExportControlNumber => !ExportControlNumber.IsEmpty;

		internal bool IsImport => JobDeclaration?.IsImport ?? false;

		internal bool IsExport => JobDeclaration?.IsExport ?? false;

		internal bool IsSea => JobDeclaration?.IsSea ?? false;

		internal bool IsAir => JobDeclaration?.IsAir ?? false;

		public bool IsECR => Factory.GetValue(ref isECR, () => IsExport && IsSea && (ExportControlNumber.IsEmpty || IsExportControlEntryNumSystemGenerated));
		CachedProperty<bool> isECR;

		public bool IsSpecialDeclaration => CEI_Style == JPImportDeclarationTypeList.Codes.J || CEI_Style == JPImportDeclarationTypeList.Codes.P || CEI_Style == JPImportDeclarationTypeList.Codes.R;

		public bool IsInbondDeclarationType => CEI_Style == JPImportDeclarationTypeList.Codes.S || CEI_Style == JPImportDeclarationTypeList.Codes.M || CEI_Style == JPImportDeclarationTypeList.Codes.A || CEI_Style == JPImportDeclarationTypeList.Codes.G;

		internal bool IsExportOrReturnedGoodsDeclarationType => CEI_Style == JPExportDeclarationTypeList.Codes.E || CEI_Style == JPExportDeclarationTypeList.Codes.R;

		internal bool IsMailedCargo => Factory.GetCachedValue<MailedCargoList>().ContainsCode(CEI_DeclarationCargoType);

		internal bool IsBondedImportDeclarationType => Factory.GetCachedValue<BondedImportDeclarationTypeList>().ContainsCode(CEI_Style);

		#region ICusOtherLawReferenceParent
		ZBool ICusOtherLawReferenceParent.IsOtherLawReferenceRequired => IsImport;

		ZString ICusOtherLawReferenceParent.MessageType => JobDeclaration?.JE_MessageType ?? ZString.Empty;

		public IEnumerable<ZString> GetTariffAttributesByKey(ZString key)
		{
			var tariffAttributes = new List<ZString>();
			var invoiceLinesTariffAttributes = InvoiceLines.Cast<JobComInvoiceLine>().Select(c => c.GetTariffAttributesByKey(key)).ToList();
			foreach (var invoiceLineTariffAttributes in invoiceLinesTariffAttributes)
			{
				tariffAttributes.AddRange(invoiceLineTariffAttributes);
			}
			return tariffAttributes.Distinct();
		}
		#endregion

		#region ISupportMultipleResourceStringData

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => IsAir ? [AirCaptionKey] : [SeaCaptionKey];

		public const string AirCaptionKey = "B6578E65-CD34-424C-BA76-11CA5EA7B063";

		public const string SeaCaptionKey = "8804C084-12C2-4E15-975B-7BC3F56FC9B6";

		#endregion
	}
}
