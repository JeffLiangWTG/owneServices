using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSRegistrationNumber : CusCodeData
	{
		public AIRSRegistrationNumber(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AIRSRegistrationNumberLookups Lookups
		{
			get { return (AIRSRegistrationNumberLookups)base.Lookups; }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new AIRSRegistrationNumberLookups(this);
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return base.IsValidationEnabledCore(propertyInfo) && (((ICADeclarationProvider)Parent)?.IsValidationEnabled ?? ZBool.True);
		}

		public new AIRSRegistrationNumberValidation Validation
		{
			get { return (AIRSRegistrationNumberValidation)base.Validation; }
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new AIRSRegistrationNumberValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.AIRSNumber;
		}

		[List(nameof(Lookups) + "." + nameof(AIRSRegistrationNumberLookups.AIRSRegTypes))]
		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set
			{
				var oldValue = base.CY_Code;
				base.CY_Code = value;
				ActionWhenDataIsChangedAndIsNotCopying(oldValue, value, (old, newValue) =>
				{
					if (!string.IsNullOrEmpty(value))
					{
						var code = RegistrationNumberHelper.LoadCFIAAirRegistrationType(Factory, value);
						if (code != null && code.Attributes.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CFIAFormat, RegistrationNumberHelper.Confirmation))
						{
							CY_Data = YesNoList.Codes.Yes;
						}
						else if (CY_Data.IsEmpty)
						{
							RegistrationNumberHelper.DefaultSafeFoodLicence(value, (x) => CY_Data = x, Lookups.CY_DataList);
						}
					}
				});
			}
		}

		[MaxLength(35)]
		public override ZString CY_Data
		{
			get { return base.CY_Data; }
			set { base.CY_Data = value; }
		}

		public ZString CY_Description
		{
			get
			{
				if (descriptionCached == null)
				{
					descriptionCached = new CachedProperty<ZString>(Factory, () =>
					{
						var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, CY_Code, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CFIAAIRSRegistrationType, ZDateTime.Today);
						return cusCode?.ZZD_Description ?? ZString.Empty;
					});
				}

				return descriptionCached.Value;
			}
		}
		CachedProperty<ZString> descriptionCached;

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CFIAPGAHeader)); }
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new string[] { Schema.CY_ParentID, Schema.CY_ParentTableCode };
		}
	}
}
