using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	public abstract class AddInfoBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AddInfo addInfo = (AddInfo)GetNewBusinessObject();
			AssertEquals("Lookups", GetExpectedLookupsType(), addInfo.Lookups.GetType());
		}

		protected abstract Type GetExpectedLookupsType();

		public void TestValidation()
		{
			AddInfo addInfo = (AddInfo)GetNewBusinessObject();
			AssertEquals("Validation", GetExpectedValidationType(), addInfo.Validation.GetType());
		}

		protected abstract Type GetExpectedValidationType();

		protected override List<string> ColumnsToClearValueAfterTested
		{
			get
			{
				List<string> result = new List<string>();
				foreach (SchemaColumn column in CAAddInfoSchema.All)
				{
					result.Add(column.Name);
				}
				return result;
			}
		}
	}
}
