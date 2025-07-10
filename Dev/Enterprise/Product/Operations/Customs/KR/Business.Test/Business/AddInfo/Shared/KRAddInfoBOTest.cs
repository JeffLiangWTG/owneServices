using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	public abstract class KRAddInfoBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			KRAddInfo addInfo = (KRAddInfo)GetNewBusinessObject();
			AssertEquals("Lookups", GetExpectedLookupsType(), addInfo.Lookups.GetType());
		}

		protected abstract Type GetExpectedLookupsType();

		public void TestValidation()
		{
			KRAddInfo addInfo = (KRAddInfo)GetNewBusinessObject();
			AssertEquals("Validation", GetExpectedValidationType(), addInfo.Validation.GetType());
		}

		protected abstract Type GetExpectedValidationType();
	}
}
