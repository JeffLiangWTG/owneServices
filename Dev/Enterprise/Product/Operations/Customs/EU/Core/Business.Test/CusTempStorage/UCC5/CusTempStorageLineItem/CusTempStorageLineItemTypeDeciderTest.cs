using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempStorageLineItemTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var lineItem = bizO as CusTempStorageLineItem;
			if (lineItem != null)
			{
				lineItem.Line.Dec.StorageHeader.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var jobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var dec = jobHeader.CusTempStorageDecs.AddNew();
			dec.STH_DeclarationType = "IST";
			var line = dec.CusTempStorageLines.AddNew();
			line.TSL_LineNo = 1;
			var lineItem = line.CusTempStorageLineItems.AddNew();
			return lineItem;
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			var countryTypes = new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.France, ObjectFactory.GetType<Integration.Customs.FR.ICusTempStorageLineItem>() }
			};
			return countryTypes;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew() => GetTestCountryCodesForNewAndBindingTests();

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding() => GetTestCountryCodesForNewAndBindingTests();

		static Dictionary<string, Type> GetTestCountryCodesForNewAndBindingTests()
		{
			var countryTypes = new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusTempStorageLineItem>() }
			};
			return countryTypes;
		}

		protected override Type BaseTypeDecidedType => typeof(CusTempStorageLineItem);

		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.ICusTempStorageLineItem>();
	}
}
