using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public abstract class CountrySpecificTypeDeciderTest : CountrySpecificTestCase
	{
		#region TestGetTypeForNew

		public virtual void TestGetTypeForNew()
		{
			SetAsycudaCustomsCountryCodes();
			AssertNotNull("GetTestCountryCodesAndExpectedTypes should be overriden to return country codes and their decided types", TestCountryCodesAndExpectedTypesForNew);
			Assert("Ensure that TestCountryCodesAndExpectedTypes is overridden to check expected country codes and decided types", TestCountryCodesAndExpectedTypesForNew.Count > 0);
			TestCountryCodesAndExpectedTypesForNew.Add(Enterprise.Core.Constants.CountryCodes.Ethiopia, OverriddenDefaultDecidedType);
			foreach (string countryCode in TestCountryCodesAndExpectedTypesForNew.Keys)
			{
				SetCountryCode(new ZString(countryCode));
				BusinessObject newBizO = Factory.New(BaseTypeDecidedType);
				AssertEquals(string.Format("DecidedType for {0} differs from expected", countryCode), TestCountryCodesAndExpectedTypesForNew[countryCode], newBizO.GetType());
				newBizO = null;
			}
		}
		#endregion

		#region TestGetTypeForLoad

		public virtual void TestGetTypeForLoad()
		{
			SetAsycudaCustomsCountryCodes();
			BusinessObjectFactory cleanFactory = new BusinessObjectFactory();
			IGlbCompany currentCompany = cleanFactory.Load<IGlbCompany>(StaticCurrentFetcher.Instance.CurrentCompany.PK);
			IRefCountry initialCountry = currentCompany.Country;
			try
			{
				AssertNotNull("GetTestCountryPKsAndExpectedTypes should be overriden to return country codes and their decided types", TestCountryPKsAndExpectedTypesForLoad);
				Assert("GetTestCountryPKsAndExpectedTypes should be overriden to return at least one country code and expected types", TestCountryPKsAndExpectedTypesForLoad.Count > 0);
				BusinessObject newBizO = GetNewBusinessObjectForLoadTest();
				foreach (object key in TestCountryPKsAndExpectedTypesForLoad.Keys)
				{
					ZGuid countryPK = new ZGuid(key);
					IRefCountry country = Factory.Load<IRefCountry>(countryPK);
					AssertNotNull("countryPK " + countryPK, country);
					SetBizOCountryForLoadTest(newBizO, country.RN_Code);
					Factory.Save();

					BusinessObject loadedBizO = new BusinessObjectFactory().Load(BaseTypeDecidedType, newBizO.PK);
					AssertNotNull("Loaded BizO should not be null", loadedBizO);
					AssertEquals(ZString.Format("DecidedType for {0} differs from expected", countryPK), TestCountryPKsAndExpectedTypesForLoad[countryPK], loadedBizO.GetType());
					loadedBizO = null;
				}
			}
			finally
			{
				cleanFactory = new BusinessObjectFactory();
				currentCompany = cleanFactory.Load<IGlbCompany>(StaticCurrentFetcher.Instance.CurrentCompany.PK);
				if (initialCountry != currentCompany.Country)
				{
					ZString countryCode = initialCountry.RN_Code;
					((EnterpriseBusinessObject)currentCompany)[Enterprise.ZArchitecture.Schema.GlbCompanySchema.GC_RN_NKCountryCode.Name] = countryCode;
					cleanFactory.Save();
				}
			}
		}
		#endregion

		#region TestGetTypeForBinding

		public void TestGetTypeForBinding()
		{
			SetAsycudaCustomsCountryCodes();
			AssertNotNull("GetTestCountryCodesAndExpectedTypes should be overriden to return country codes and their decided types", TestCountryCodesAndExpectedTypesForBinding);
			Assert("Ensure that TestCountryCodesAndExpectedTypes is overridden to check expected country codes and decided types", TestCountryCodesAndExpectedTypesForBinding.Count > 0);
			TestCountryCodesAndExpectedTypesForBinding.Add(Enterprise.Core.Constants.CountryCodes.Ethiopia, OverriddenDefaultDecidedType);
			foreach (string countryCode in TestCountryCodesAndExpectedTypesForBinding.Keys)
			{
				SetCountryCode(new ZString(countryCode));
				AssertEquals(ZString.Format("DecidedType for {0} differs from expected", countryCode), TestCountryCodesAndExpectedTypesForBinding[countryCode], TypeDecider.GetTypeForBinding(this.BaseTypeDecidedType));
			}
		}
		#endregion

		#region Helper Methods

		void SetBizOCountryForLoadTest(BusinessObject bizO, ZString countryCode)
		{
			if (bizO.GetType() != OverriddenDefaultDecidedType)
			{
				throw new InvalidCastException(ZString.Format("Invalid type for test business object. Expected {0} but was {1}", OverriddenDefaultDecidedType, bizO.GetType()));
			}

			SetBizOCountryForLoadTestCore(bizO, countryCode);
		}

		protected Dictionary<ZGuid, Type> TestCountryPKsAndExpectedTypesForLoad
		{
			get
			{
				if (fTestCountryKeysAndExpectedTypesForLoad == null)
				{
					fTestCountryKeysAndExpectedTypesForLoad = GetTestCountryPKsAndExpectedTypesForLoad();
				}
				return fTestCountryKeysAndExpectedTypesForLoad;
			}
		}

		Dictionary<ZGuid, Type> fTestCountryKeysAndExpectedTypesForLoad;

		protected Dictionary<string, Type> TestCountryCodesAndExpectedTypesForNew
		{
			get
			{
				if (fTestCountryCodesAndExpectedTypesForNew == null)
				{
					fTestCountryCodesAndExpectedTypesForNew = GetTestCountryCodesAndExpectedTypesForNew();
				}
				return fTestCountryCodesAndExpectedTypesForNew;
			}
		}

		Dictionary<string, Type> fTestCountryCodesAndExpectedTypesForNew;

		protected Dictionary<string, Type> TestCountryCodesAndExpectedTypesForBinding
		{
			get
			{
				if (fTestCountryCodesAndExpectedTypesForBinding == null)
				{
					fTestCountryCodesAndExpectedTypesForBinding = GetTestCountryCodesAndExpectedTypesForBinding();
				}
				return fTestCountryCodesAndExpectedTypesForBinding;
			}
		}

		Dictionary<string, Type> fTestCountryCodesAndExpectedTypesForBinding;

		public void SetAsycudaCustomsCountryCodes()
		{
			TestConnection.ExecuteNonQuery($@"
IF NOT EXISTS ( SELECT NULL FROM {RefDataGroupingSchema.Constants.TableName} WHERE {RefDataGroupingSchema.Constants.ZZZ_DataGrouping} = '{Universal.RefDataGrouping.Codes.CommonDataGrouping}')
	INSERT INTO {RefDataGroupingSchema.Constants.TableName} (
		{RefDataGroupingSchema.Constants.PK}, 
		{RefDataGroupingSchema.Constants.ZZZ_DataGrouping}, 
		{RefDataGroupingSchema.Constants.ZZZ_Description}, 
		{RefDataGroupingSchema.Constants.ZZZ_ZZZ_Grouping}
	) VALUES (NEWID(), '{Universal.RefDataGrouping.Codes.CommonDataGrouping}', '{Universal.RefDataGrouping.Codes.CommonDataGrouping}', NULL)

IF NOT EXISTS ( SELECT NULL FROM {RefCusCodeTypeSchema.Constants.TableName} WHERE {RefCusCodeTypeSchema.Constants.ZZK_CodeType} = '{Universal.RefCusCodeListTypes.Codes.Asycuda}')
	INSERT INTO {RefCusCodeTypeSchema.Constants.TableName} (
		{RefCusCodeTypeSchema.Constants.PK}, 
		{RefCusCodeTypeSchema.Constants.ZZK_CodeType}, 
		{RefCusCodeTypeSchema.Constants.ZZK_Description}, 
		{RefCusCodeTypeSchema.Constants.ZZK_IsReadonly},
		{RefCusCodeTypeSchema.Constants.ZZK_ZZZ_NKDataGrouping}
	) VALUES (NEWID(), '{Universal.RefCusCodeListTypes.Codes.Asycuda}', 'FOR TEST', 1, '{Universal.RefDataGrouping.Codes.CommonDataGrouping}')

IF NOT EXISTS ( SELECT NULL FROM {RefCusCodeListSchema.Constants.TableName} 
	WHERE {RefCusCodeListSchema.Constants.ZZD_ZZK_NKCodeType} = '{Universal.RefCusCodeListTypes.Codes.Asycuda}'
	AND {RefCusCodeListSchema.Constants.ZZD_Code} = '{Constants.CountryCodes.Namibia}'
	AND {RefCusCodeListSchema.Constants.ZZD_ZZZ_NKDataGrouping} = '{Universal.RefDataGrouping.Codes.CommonDataGrouping}'
)
	INSERT INTO {RefCusCodeListSchema.Constants.TableName} (
		{RefCusCodeListSchema.Constants.PK}, 
		{RefCusCodeListSchema.Constants.ZZD_ZZK_NKCodeType}, 
		{RefCusCodeListSchema.Constants.ZZD_Code}, 
		{RefCusCodeListSchema.Constants.ZZD_Description}, 
		{RefCusCodeListSchema.Constants.ZZD_StartDate}, 
		{RefCusCodeListSchema.Constants.ZZD_EndDate}, 
		{RefCusCodeListSchema.Constants.ZZD_ZZZ_NKDataGrouping}
		) VALUES (NEWID(), '{Universal.RefCusCodeListTypes.Codes.Asycuda}', '{Constants.CountryCodes.Namibia}', 'Namibia', '2019-03-30 00:00:00', '2079-06-06 23:59:00', '{Universal.RefDataGrouping.Codes.CommonDataGrouping}')
			,(NEWID(), '{Universal.RefCusCodeListTypes.Codes.Asycuda}', '{Constants.CountryCodes.Lesotho}', 'Lesotho', '2019-03-30 00:00:00', '2079-06-06 23:59:00', '{Universal.RefDataGrouping.Codes.CommonDataGrouping}')
			,(NEWID(), '{Universal.RefCusCodeListTypes.Codes.Asycuda}', '{Constants.CountryCodes.Botswana}', 'Botswana', '2019-03-30 00:00:00', '2079-06-06 23:59:00', '{Universal.RefDataGrouping.Codes.CommonDataGrouping}')
			,(NEWID(), '{Universal.RefCusCodeListTypes.Codes.Asycuda}', '{Constants.CountryCodes.Swaziland}', 'Swaziland', '2019-03-30 00:00:00', '2079-06-06 23:59:00', '{Universal.RefDataGrouping.Codes.CommonDataGrouping}')");
		}

		#endregion

		#region Abstract Members

		protected abstract void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode);
		protected abstract BusinessObject GetNewBusinessObjectForLoadTest();
		protected abstract Type BaseTypeDecidedType { get; }
		protected virtual Type OverriddenDefaultDecidedType { get { return BaseTypeDecidedType; } }

		protected abstract Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad();
		protected abstract Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew();
		protected abstract Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding();

		#endregion
	}
}
