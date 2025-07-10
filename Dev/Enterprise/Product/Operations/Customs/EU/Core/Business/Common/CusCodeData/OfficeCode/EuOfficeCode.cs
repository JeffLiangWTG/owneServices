using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class EuOfficeCode : CusCodeData, ICustomsOffice
	{
		public EuOfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.OfficeCode;
		}

		public new class Schema : CusCodeData.Schema
		{
			public const string CY_CodeDescription = nameof(EuOfficeCode.CY_CodeDescription);
			public const string CY_OfficeDescription = nameof(EuOfficeCode.CY_OfficeDescription);
			public const string CY_OfficeAddress = nameof(EuOfficeCode.CY_OfficeAddress);
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		public static T LoadOrCreate<T>(BusinessObject parent, ZString purpose)
			where T : EuOfficeCode
		{
			return Load<T>(parent, purpose) ?? New<T>(parent, purpose);
		}

		public static T Load<T>(BusinessObject parent, ZString purpose)
			where T : EuOfficeCode
		{
			var filter = new ZQuery(CusCodeDataSchema.CY_ParentID, SQLComparisonOperator.Equal, parent.PK);
			filter.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.OfficeCode);
			filter.AddToFilter(CusCodeDataSchema.CY_Code, purpose);
			filter.FetchOnlyFromLocalCache = !parent.IsInDatabase;
			filter.OrderBy = CusCodeDataSchema.Constants.CY_SystemCreateTimeUtc;
			var result = parent.Factory.LoadTop1<T>(filter);
			if (result != null)
			{
				result.Parent = parent;
			}
			return result;
		}

		internal static T New<T>(BusinessObject parent, ZString officeType)
			where T : EuOfficeCode
		{
			var result = parent.Factory.New<T>();
			using (result.SuspendSettingHasChanges())
			{
				result.Parent = parent;
				result.CY_Code = officeType;
			}
			return result;
		}

		protected override CusCodeDataLookups GetNewLookups() => new EuOfficeCodeLookups(this);

		public new EuOfficeCodeLookups Lookups => (EuOfficeCodeLookups)base.Lookups;

		protected override CusCodeDataValidation GetNewValidation() => new EuOfficeCodeValidation(this);

		public new EuOfficeCodeValidation Validation => (EuOfficeCodeValidation)base.Validation;

		[MaxLength(3)]
		[ReadOnlyMember(nameof(IsLinkingToReadOnlyEMCSParent))]
		[ResourceStringData("D1F8EF71-B08C-4B60-990E-70C0A0E7850F", Caption = "Purpose")]
		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set
			{
				var oldValue = CY_Code;
				base.CY_Code = value;

				if (CY_Code != oldValue && !IsCopying)
				{
					var parent = Parent;
					if (CY_Code == EuOfficeCodesTypes.Codes.OfficeOfPresentation
						&& parent is JobDeclaration declaration
						&& !declaration.IsUXMLImportingData
						&& declaration.Configuration.IsPopulateAuthorisationsForOfficeOfPresentationEnabled(declaration))
					{
						PopulateAuthorisations(declaration);
					}

					parent?.MarkAsNeedingValidation();
				}
			}
		}

		void PopulateAuthorisations(JobDeclaration declaration)
		{
			var entryInstructionsNeedPopulate = declaration.CustomsEntryInstructions.Where(p => !p.IsCentralisedClearance);
			foreach (var entryInstruction in entryInstructionsNeedPopulate)
			{
				var authorization = entryInstruction.CusAuthorizationUsages.AddNew();
				authorization.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
			}
		}

		public IEnumerable<ZString> CY_RoleCodes => Requirement?.OfficeRolesForLookup ?? new ZString[] { CY_Code };

		public ZString CY_CodeDescription =>
			Factory.GetCachedValue(
				"Enterprise.Customs.EU.Business.EuOfficeCode.CY_Description_" + CY_Code,
				() => Lookups.CY_CodeList[CY_Code, StringComparison.CurrentCultureIgnoreCase]?.Description
			);

		[MaxLength(10)]
		[List(nameof(Lookups) + "." + nameof(EuOfficeCodeLookups.OfficeCodeList))]
		[ResourceStringData("Enterprise.Customs.EU.Business.EuOfficeCode.CusCodeData|CY_Data", Caption = "Office Code")]
		[ReadOnlyMember(nameof(IsLinkingToReadOnlyEMCSParent))]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set
			{
				var oldValue = CY_Data;
				base.CY_Data = value;
				if (oldValue != CY_Data)
				{
					office = null;
					Parent?.MarkAsNeedingValidation();
				}
			}
		}

		public ZString CY_OfficeDescription => Office?.ZZD_Description ?? ZString.Empty;

		public ZZRefCusCodeListCombined Office => OfficeCore;
		protected virtual ZZRefCusCodeListCombined OfficeCore =>
			office ?? (office = ZZRefCusCodeListCombined.Loader.LoadTop1ByParentDataGrouping(Factory, CY_Data, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDate.Today));
		ZZRefCusCodeListCombined office;

		public ZBool IsLinkingToReadOnlyEMCSParent => Factory.GetValue(ref isLinkingToReadOnlyEMCSParent,
			() => CY_ParentID.IsValid && CY_Code != OfficeCodes_EMCS.Codes.OfficeOfDelivery && ((Parent as Integration.Customs.EUEMCS.IJobDeclaration)?.IsMessageStatusSentOrAcknowledged ?? ZBool.False));
		CachedProperty<ZBool> isLinkingToReadOnlyEMCSParent;

		public override bool CanDelete => base.CanDelete && !IsLinkingToReadOnlyEMCSParent;

		public CustomsOfficeRequirement Requirement
		{
			get
			{
				CustomsOfficeRequirement result = null;
				if (Parent is IEuOfficeCodeProvider provider && provider.CustomsOfficeRequirementHelper is CustomsOfficeRequirementHelper requirementHelper)
				{
					if (requirementHelper.MainOffice is CustomsOfficeRequirement mainOfficeRequirement && mainOfficeRequirement.OfficeRole == CY_Code)
					{
						result = mainOfficeRequirement;
					}
					else
					{
						result = requirementHelper.OtherRequirements.FirstOrDefault(x => x.OfficeRole == CY_Code);
					}
				}
				return result;
			}
		}

		public ZString CY_OfficeAddress => ZString.Format("{0}\n{1} {2}", Office?.GetAttribute(UniversalReferenceConstants.CustomsOfficeAttributes.Street) ?? ZString.Empty, Office?.GetAttribute(UniversalReferenceConstants.CustomsOfficeAttributes.PostCode) ?? ZString.Empty, Office?.GetAttribute(UniversalReferenceConstants.CustomsOfficeAttributes.CITY) ?? ZString.Empty);

		public virtual bool OfficeCodesUseDesInsteadOfCaa => false;

		#region ICustomsOffice Members

		ZDateTime ICustomsOffice.ArrivalTime => CY_Date;

		ZString ICustomsOffice.OfficeCode => CY_Data;

		#endregion
	}
}
