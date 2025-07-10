using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.Business.MasterFiles
{
	public partial class CusClassification : AutoCusClassification
		, Integration.Customs.EU.ICusClassification
		, ICusCodeDataTypeSupporter
		, ISupplementaryCodeSupporter
	{
		public CusClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new class Schema : AutoCusClassification.Schema
		{
			public const string CC_EcSupplement1 = "CC_EcSupplement1";
			public const string CC_EcSupplement2 = "CC_EcSupplement2";
			public const string CC_EcAdditionalSupplements = "CC_EcAdditionalSupplements";
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CC_ClassificationType = CusClassification.ClassificationType.Both;
		}

		[List(nameof(Lookups) + "." + nameof(CusClassificationLookups.CPCs))]
		public override ZString CC_ProcedureCode
		{
			get { return base.CC_ProcedureCode; }
			set { base.CC_ProcedureCode = value; }
		}

		public override ZString CC_ClassificationType
		{
			get { return base.CC_ClassificationType; }
			set
			{
				var oldClassificationType = base.CC_ClassificationType;
				base.CC_ClassificationType = value;
				if (oldClassificationType != CC_ClassificationType)
				{
					OnTariffSet(oldClassificationType, CC_TariffNum);
				}
			}
		}

		protected override void OnTariffSet(ZString oldTariff)
		{
			base.OnTariffSet(oldTariff);
			OnTariffSet(CC_ClassificationType, oldTariff);
		}

		protected void OnTariffSet(ZString oldClassificationType, ZString oldTariff)
		{
			if (!CC_Description.IsEmpty)
			{
				var oldDescription = GetTariffDescription(oldClassificationType, oldTariff);
				if (!oldDescription.IsEmpty)
				{
					if (CC_Description == oldDescription.Left(CC_Description.Length))
					{
						CC_Description = ZString.Empty;  // Current description is that of the OLD commodity code, i.e. it's not a custom/user-defined description... wipe it
					}
				}
			}

			if (CC_Description.IsEmpty)
			{
				var tariffDescription = GetTariffDescription(CC_ClassificationType, CC_TariffNum);
				if (!tariffDescription.IsEmpty)
				{
					CC_Description = tariffDescription.Left(CC_DescriptionInfo.MaxLength);
				}
			}
		}

		protected virtual ZString GetTariffDescription(ZString classificationType, ZString tariffCode)
		{
			var description = ZString.Empty;
			if (!tariffCode.IsEmpty)
			{
				var tariffType = TariffFormatter.GetTariffType(classificationType == ClassificationType.EXP);
				description = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(CC_RN_NKCountryCode, tariffType, tariffCode, ZDateTime.Today)?.ZZ1_Description ?? ZString.Empty;
			}

			return description;
		}

		#region SupplementaryCodes

		#region CC_EcSupplement1

		[MaxLength(Customs.EU.Business.SupplementaryCode.Schema.CY_CodeMaxLength)]
		public ZString CC_EcSupplement1
		{
			get { return (Supplement1 != null) ? Supplement1.CY_Code : ZString.Empty; }
			set => SupplementaryCodeHelper.SupplementaryCodeSetter(this, CC_EcSupplement1Info, value, Supplement1, 1);
		}

		public ZPropertyInfo CC_EcSupplement1Info
		{
			get { return Supplement1 != null ? GetWrappedZPropertyInfo(Schema.CC_EcSupplement1, x => Supplement1.CY_CodeInfo) : GetZPropertyInfo(Schema.CC_EcSupplement1); }
		}

		SupplementaryCode Supplement1
		{
			get
			{
				if (supplement1 == null || supplement1.IsDeleted)
				{
					var loader = new BaseSupplementaryCode.Loader(Factory);
					supplement1 = loader.Load<SupplementaryCode, CusClassification>(this, 1);
					if (supplement1 != null)
					{
						RegisterEditableChildObject(supplement1);
					}
				}
				return supplement1;
			}
		}
		SupplementaryCode supplement1;

		#endregion

		#region CC_EcSupplement2

		[MaxLength(Customs.EU.Business.SupplementaryCode.Schema.CY_CodeMaxLength)]
		public ZString CC_EcSupplement2
		{
			get { return (Supplement2 != null) ? Supplement2.CY_Code : ZString.Empty; }
			set => SupplementaryCodeHelper.SupplementaryCodeSetter(this, CC_EcSupplement2Info, value, Supplement2, 2);
		}

		public ZPropertyInfo CC_EcSupplement2Info
		{
			get { return Supplement2 != null ? GetWrappedZPropertyInfo(Schema.CC_EcSupplement2, x => Supplement2.CY_CodeInfo) : GetZPropertyInfo(Schema.CC_EcSupplement2); }
		}

		SupplementaryCode Supplement2
		{
			get
			{
				if (supplement2 == null || supplement2.IsDeleted)
				{
					var loader = new BaseSupplementaryCode.Loader(Factory);
					supplement2 = loader.Load<SupplementaryCode, CusClassification>(this, 2);
					if (supplement2 != null)
					{
						RegisterEditableChildObject(supplement2);
					}
				}
				return supplement2;
			}
		}
		SupplementaryCode supplement2;

		#endregion

		#region CC_EcAdditionalSupplements

		[ReadOnlyMember(nameof(CC_EcAdditionalSupplements_ReadOnly))]
		public ZString CC_EcAdditionalSupplements => AdditionalSupplementaryCodes.AsString;

		public ZPropertyInfo CC_EcAdditionalSupplementsInfo => GetZPropertyInfo(Schema.CC_EcAdditionalSupplements);

		public bool CC_EcAdditionalSupplements_ReadOnly => false;

		#endregion

		[ChildEditable(true)]
		public SupplementaryCodeCollection AdditionalSupplementaryCodes
		{
			get
			{
				if (additionalSupplements == null)
				{
					additionalSupplements = SupplementaryCodeCollection.New(CC_EcAdditionalSupplementsInfo);
					RegisterEditableChildObject(additionalSupplements);
				}

				return additionalSupplements;
			}
		}
		SupplementaryCodeCollection additionalSupplements;

		ICusCodeDataCollection<BaseSupplementaryCode> ISupplementaryCodeSupporter.AdditionalSupplementaryCodes => AdditionalSupplementaryCodes;

		public IEnumerable<SupplementaryCode> SupplementaryCodes => AdditionalSupplementaryCodes.OfType<SupplementaryCode>().Union(new SupplementaryCode[] { Supplement1, Supplement2 }).Where(x => x != null);

		IEnumerable<BaseSupplementaryCode> ISupplementaryCodeSupporter.SupplementaryCodes => SupplementaryCodes;

		ZString ISupplementaryCodeSupporter.SupplementaryCodesFieldType => nameof(FieldType.Text);

		ResourceStringData ISupplementaryCodeSupporter.SupplementaryCodeCaption => null;

		public ZString GetCountryCodeForCodeProvider() => CC_RN_NKCountryCode;

		TariffView ICusCodeDataWithOrderSupporter.Tariff => null;
		IZZRateSelectionCriteria ICusCodeDataWithOrderSupporter.RateSelectionCriteria => null;

		CodeDescriptionPairList ICusCodeDataWithOrderSupporter.CachedListOfAdditionalCodeDescriptions => null;

		void ICusCodeDataWithOrderSupporter.OnCodesChanged()
		{
		}

		#endregion

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoCusClassification.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public CusClassification Load(string lookupCode, string classificationType)
			{
				ZQuery classFilter = new ZQuery(CusClassificationSchema.CC_LookupCode, lookupCode);
				classFilter.AddToFilter(CusClassificationSchema.CC_ClassificationType, CusClassification.ClassificationType.Both);  // Only support BTH since the tariff code itself (8 or 10 digits) tells us the direction
				classFilter.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);  // same as SetDefaultValues for BaseCusClassification bizO, which is how manually-created rows are made.  So fine for the checking during the automatic import. 
				return (CusClassification)Factory.LoadTop1(GetTypeOfBusinessObjectToLoad(), classFilter);
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(CusClassification);
			}
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			foreach (var fetchStrategy in GetAdditionalBusinessObjectFetchStrategies())
			{
				yield return fetchStrategy;
			}
		}

		protected virtual IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}
		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.SupplementaryCode, typeof(SupplementaryCode));
			return result;
		}

		public ZString GetCountryCodeFromAdditionalCode(ZString additionalCode) => ZString.Empty;
	}
}
