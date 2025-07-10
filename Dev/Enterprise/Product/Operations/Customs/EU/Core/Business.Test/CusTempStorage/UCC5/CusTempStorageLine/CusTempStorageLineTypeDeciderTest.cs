using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class CusTempStorageLineTypeDeciderTest : CountrySpecificWtihEUTypeDeciderTest
	{
		protected override void SetBizOCountryForLoadTestCore(BusinessObject bizO, ZString countryCode)
		{
			var line = bizO as CusTempStorageLine;
			if (line != null)
			{
				line.Dec.StorageHeader.Company.GC_RN_NKCountryCode = countryCode;
			}
		}

		protected override BusinessObject GetNewBusinessObjectForLoadTest()
		{
			var jobHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var dec = jobHeader.CusTempStorageDecs.AddNew();
			dec.STH_DeclarationType = "IST";
			var line = dec.CusTempStorageLines.AddNew();
			line.TSL_LineNo = 1;
			return line;
		}

		protected override Dictionary<ZGuid, Type> GetTestCountryPKsAndExpectedTypesForLoad()
		{
			var countryTypes = new Dictionary<ZGuid, Type>
			{
				{ Core.Constants.CountryGuids.France, Type.GetType("Enterprise.Customs.FR.Business.CusTempStorage.ISTCusTempStorageLine, Enterprise.Customs.FR.Business") },
			};
			return countryTypes;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForNew()
		{
			var countryTypes = new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.EU.ICusTempStorageLine>() }
			};
			return countryTypes;
		}

		protected override Dictionary<string, Type> GetTestCountryCodesAndExpectedTypesForBinding()
		{
			var countryTypes = new Dictionary<string, Type>
			{
				{ Core.Constants.CountryCodes.France, ObjectFactory.GetType<Integration.Customs.FR.ICusTempStorageLine>() }
			};
			return countryTypes;
		}

		protected override Type BaseTypeDecidedType => typeof(CusTempStorageLine);

		protected override Type EUType => ObjectFactory.GetType<Integration.Customs.EU.ICusTempStorageLine>();
	}
}
