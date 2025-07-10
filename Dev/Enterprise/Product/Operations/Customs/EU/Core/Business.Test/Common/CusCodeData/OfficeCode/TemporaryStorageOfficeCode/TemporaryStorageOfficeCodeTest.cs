using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(TemporaryStorageOfficeCode))]
	class TemporaryStorageOfficeCodeTest : Customs.Business.Testing.CusCodeDataTest<TemporaryStorageOfficeCode>
	{
		protected override IEnumerable<TemporaryStorageOfficeCode> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return GetTemporaryStorageOfficeCode(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetTemporaryStorageOfficeCode(factory);

		protected override BusinessObject GetNewBusinessObject() => GetTemporaryStorageOfficeCode(Factory);

		TemporaryStorageOfficeCode GetTemporaryStorageOfficeCode(BusinessObjectFactory factory)
		{
			var storageHeader = factory.New<TemporaryStorageHeader>();
			storageHeader.AMA_JobReference = "123";
			storageHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			return storageHeader.PresentationCustomsOfficeCode;
		}
	}
}
