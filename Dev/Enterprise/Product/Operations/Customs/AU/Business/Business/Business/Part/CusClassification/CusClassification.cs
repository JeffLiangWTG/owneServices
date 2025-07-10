using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class Classification : BaseCusClassification, IAddInfo, IAddInfoManager, IDutyDataWrapper, ISelfHoldingLineAttachee, ITariffNumberProvider, Integration.Customs.AU.IClassification
	{
		public Classification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : BaseCusClassification.Schema
		{
			public const string TreatmentCode = "TreatmentCode";
			public const string InstrumentType = "InstrumentType";
			public const string InstrumentCode = "InstrumentCode";
		}

		public static Classification New(BusinessObjectFactory factory)
		{
			return factory.New<Classification>();
		}

		#region New Properties

		public RefCountryCollection CountryList
		{
			get
			{
				return new RefCountryCollection(Factory);
			}
		}

		public ZDecimal ImportDutyPercentage
		{
			get
			{
				Common.AU.ICMRDutyData dutyData = new DutyDataWrapper(this);
				return new CMRDutyCalculator(dutyData).Duty.Percent;
			}
		}

		#region Treatment Code / List
		public ZString TreatmentCode
		{
			get
			{
				return AddInfo.ZA_TreatmentCode_Hidden;
			}
			set
			{
				if (AddInfo.ZA_TreatmentCode_Hidden != value)
				{
					AddInfo.ZA_TreatmentCode_Hidden = value;
					HasChanges = true;
				}
			}
		}
		public ZPropertyInfo TreatmentCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.TreatmentCode, x => AddInfo.ZA_TreatmentCode_HiddenInfo); }
		}

		public CodeDescriptionPairList TreatmentCode_List
		{
			get { return AddInfo.Lookups.TreatmentCodeList; }
		}

		#endregion

		#region InstrumentType / List
		public ZString InstrumentType
		{
			get
			{
				return AddInfo.ZA_InstrumentType_Hidden;
			}
			set
			{
				if (AddInfo.ZA_InstrumentType_Hidden != value)
				{
					AddInfo.ZA_InstrumentType_Hidden = value;
					HasChanges = true;
				}
			}
		}
		public ZPropertyInfo InstrumentTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.InstrumentType, x => AddInfo.ZA_InstrumentType_HiddenInfo); }
		}

		public CodeDescriptionPairList InstrumentType_List
		{
			get { return new CustomsInstrumentTypeList(); }
		}

		#endregion

		#region Instrument Code / List
		public ZString InstrumentCode
		{
			get
			{
				return AddInfo.ZA_InstrumentCode_Hidden;
			}
			set { AddInfo.ZA_InstrumentCode_Hidden = value; }
		}
		public ZPropertyInfo InstrumentCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.InstrumentCode, x => AddInfo.ZA_InstrumentCode_HiddenInfo); }
		}

		#endregion

		#endregion

		#region Override

		public override void Delete()
		{
			if (this.IsImport)
			{
				this.Questions.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		[BusinessObjectTestExclude()]
		public override ZString CC_TariffNum
		{
			get { return base.CC_TariffNum; }
			set
			{
				LineAttacheeHolderWrapper.NeedToGenerateQuestions = base.CC_TariffNum != value;
				base.CC_TariffNum = value;
				SetSecondUQFromTariffNum();
				if (CC_Description.IsEmpty)
				{
					SetDescriptionFromTariffDescription();
				}
			}
		}

		protected override ZString FormatTariffForSaving(ZString unformattedTariff) => ((IAUTariffFormatter)CurrentTariffFormatter).FormatDotted(unformattedTariff);

		protected override TariffFormatter GetTariffFormatter()
		{
			if (IsExport)
			{
				return new AUExportTariffUniversalFormatter();
			}
			else
			{
				return new AUImportTariffUniversalFormatter();
			}
		}

		#region IDocManagerSupport

		public override DocManagerInfo DocManagerInfo
		{
			get
			{
				string docManagerCode = (CC_ClassificationType == ClassificationType.IMP) ? "IMC" : "EXC";
				return new DocManagerInfo(this, docManagerCode);
			}
		}

		#endregion

		#endregion

		#region IAddInfo Members

		public override void OnLoaded()
		{
			base.OnLoaded();
			AddInfo.LoadPropertiesFromString(CC_AddInfo);
		}

		public AUAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new CusClassificationAddInfo(this, CC_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
				}
				return fAddInfo;
			}
		}
		protected AUAddInfo fAddInfo;

		public AUAddInfo EffectiveAddInfo
		{
			get
			{
				return AddInfo;
			}
		}

		bool IAggregatedAddInfo.IsCopying
		{
			get
			{
				return IsCopying;
			}
		}

		#endregion

		[BusinessObjectTestExclude()]
		public override ZString CC_AddInfo
		{
			get { return base.CC_AddInfo; }
			set
			{
				value = value.Trim(AUAddInfo.SeperationCharacter);
				if (CC_AddInfo != value)
				{
					base.CC_AddInfo = value;
					using (AddInfo.GetValidationSuspender())
					{
						AddInfo.LoadPropertiesFromString(CC_AddInfo);
					}
					LineAttacheeHolderWrapper.NeedToGenerateQuestions = true;
				}
			}
		}

		protected override Customs.Business.CusClassificationValidation GetNewValidation()
		{
			return new CusClassificationValidation(this);
		}

		#region Implementation

		protected void SetSecondUQFromTariffNum()
		{
			if (ImportTariff != null)
			{
				AddInfo.ZA_UQ2 = ImportTariff.ZZ1_ZZ8_UQ2.ToUpper();
			}
		}

		protected void SetDescriptionFromTariffDescription()
		{
			if (ExportTariff != null)
			{
				base.CC_Description = ExportTariff.ZZ1_Description.SubstringSafe(0, 80);
			}
			else if (ImportTariff != null)
			{
				base.CC_Description = ImportTariff.ZZ1_Description.SubstringSafe(0, 80);
			}
		}

		protected internal ITariffView ExportTariff
		{
			get
			{
				return Factory.GetCachedValue($"Enterprise.Customs.AU.Declaration.Business.Classification_EXP_{CC_TariffNum}_{DateOfValuation}",
					() => AUCAHECCWrapper.Load(Factory, CC_TariffNum, DateOfValuation));
			}
		}

		protected internal ITariffView ImportTariff
		{
			get
			{
				return Factory.GetCachedValue($"Enterprise.Customs.AU.Declaration.Business.Classification_IMP_{CC_TariffNum}_{DateOfValuation}",
					() => AUCClassWrapper.Load(Factory, CC_TariffNum, DateOfValuation));
			}
		}

		#endregion

		#region IAggregatedAddInfo Members

		public ZDateTime EffectiveDutyDate
		{
			get { return ZDateTime.Today; }
		}

		public ZDateTime DateOfValuation
		{
			get { return ZDateTime.Today; }
		}

		ZString IAggregatedAddInfo.AggregatedZA_ORG
		{
			get { return AddInfo.ZA_ORG; }
		}

		ZString IAggregatedAddInfo.AggregatedZA_PRF
		{
			get { return AddInfo.ZA_PRF; }
		}

		public IZType AggregatedValue(string propertyName)
		{
			return (IZType)AddInfo[propertyName];
		}

		#endregion

		#region IDutyDataWrapper Members

		public ZString Preference
		{
			get
			{
				if (IsImport && AddInfo != null)
				{
					return AddInfo.ZA_PRF;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZString ImportTariffCode
		{
			get { return IsImport ? CC_TariffNum : ZString.Empty; }
		}

		ZString IDutyDataWrapper.TreatmentCode
		{
			get
			{
				if (IsImport && AddInfo != null)
				{
					return AddInfo.ZA_TreatmentCode_Hidden;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		#region ICPQAAttachee Members

		SchemaGuidColumn ICPQAAttachee.FKColumnInCusEntryCPDecTable
		{
			get { return CPQAAttacheeWrapper.FKColumnInCusEntryCPDecTable; }
		}

		[ChildEditable(true)]
		public CMRCusEntryCPDecCollection Questions
		{
			get { return CPQAAttacheeWrapper.Questions; }
		}

		ZDateTime ICPQAAttachee.SelectionDate
		{
			get { return CPQAAttacheeWrapper.SelectionDate; }
		}

		CPQAAttacheeWrapper CPQAAttacheeWrapper
		{
			get
			{
				if (fCPQAAttacheeWrapper == null)
				{
					fCPQAAttacheeWrapper = new CPQAAttacheeWrapper(this);
				}
				return fCPQAAttacheeWrapper;
			}
		}
		CPQAAttacheeWrapper fCPQAAttacheeWrapper;

		#endregion

		#region ICPQALineAttachee Members

		public
#if DEBUG
 virtual
#endif
 void GenerateQuestion()
		{
			LineAttacheeHolderWrapper.GenerateQuestion();
		}

		public void RefreshQuestions()
		{
			LineAttacheeHolderWrapper.NeedToGenerateQuestions = true;
			GenerateQuestion();
		}

		CPQuestionKeys ICPQALineAttachee.CPQuestionKey
		{
			get
			{
				CPQuestionKeys result = new CPQuestionKeys();
				ZString tariffStatNumber = CC_TariffNum.Replace(".", "").Replace(" ", "");
				result.TariffNumber = tariffStatNumber.SubstringSafe(0, 8);
				result.StatCode = tariffStatNumber.SubstringSafe(8, 2);
				result.OriginCode = AddInfo.ZA_ORG;
				result.HasValidOriginOrNatureOrModeOfTransport = false;
				return result;
			}
		}

		LineDefaultQuestions ICPQALineAttachee.DefaultUniqueQuestions
		{
			get { return LineAttacheeHolderWrapper.DefaultUniqueQuestions; }
		}

		ICPQALineAttachee[] ICPQALineAttachee.SourcesToDefault
		{
			get { return System.Array.Empty<ICPQALineAttachee>(); }
		}

		ZString ICPQALineAttachee.TableCode
		{
			get { return Enterprise.ZArchitecture.Schema.CusClassificationSchema.Constants.Prefix; }
		}

		ZBool ICPQALineAttachee.IsRiskCalculatedFromTariff
		{
			get { return true; }
		}

		ZBool ICPQALineAttachee.IsRiskHistorySupported
		{
			get { return true; }
		}

		#endregion

		#region ICPQAAttacheeHolder Members

		ICPQAHeaderAttachee[] ICPQAAttacheeHolder.Headers
		{
			get { return LineAttacheeHolderWrapper.Headers; }
		}

		ICPQALineAttachee[] ICPQAAttacheeHolder.Lines
		{
			get { return LineAttacheeHolderWrapper.Lines; }
		}

		CachedAnsweredQuestions ICPQAAttacheeHolder.CachedQuestions
		{
			get { return LineAttacheeHolderWrapper.CachedQuestions; }
		}

		internal CPQASelfHoldingLineAttacheeWrapper LineAttacheeHolderWrapper
		{
			get
			{
				if (fLineAttacheeHolderWrapper == null)
				{
					fLineAttacheeHolderWrapper = new CPQASelfHoldingLineAttacheeWrapper(this);
				}
				return fLineAttacheeHolderWrapper;
			}
		}
		CPQASelfHoldingLineAttacheeWrapper fLineAttacheeHolderWrapper;
		#endregion

		#region ITariffNumberProvider Members

		ZString ITariffNumberProvider.TariffAndStatNumber
		{
			get
			{
				return CC_TariffNum;
			}
		}

		#endregion

		protected override ZString GetDutyRateForCurrentCountry()
		{
			if (CC_ClassificationType == Classification.ClassificationType.IMP
				&& !CC_TariffNum.IsEmpty)
			{
				Common.AU.ICMRDutyData dutyData = new DutyDataWrapper(this);
				ZDecimal rate = new CMRDutyCalculator(dutyData).Duty.Percent;
				return rate.ToString(5);
			}
			return ZString.Empty;
		}

		#region IAddInfoManager Members

		Customs.Business.IAddInfo IAddInfoManager.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion
	}
}
