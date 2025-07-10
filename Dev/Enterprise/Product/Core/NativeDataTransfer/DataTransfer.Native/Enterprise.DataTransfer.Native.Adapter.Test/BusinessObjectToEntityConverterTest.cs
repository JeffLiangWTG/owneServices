using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Exceptions;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Adapter
{
	class BusinessObjectToEntityConverterTest : TransactionedTestCase
	{
		public void TestConvert()
		{
			var oH_Pk = TestUtil.PrepareOrgHeaderTableData();
			var oC_Pk = TestUtil.PrepareOrgContactTableData(oH_Pk);

			var factory = new BusinessObjectFactory();
			var org = factory.Load<OrgHeader>(oH_Pk);

			var converter =
				new BusinessObjectToEntityConverter
				{
					DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder()
				};

			var result = converter.GetEntity(org, null);

			AssertEquals("Business Object's primary key should be equal to the generated entity's internalPK", oH_Pk, result.InternalPK);
			AssertEquals("Business Object should have the same amount of children as the generated entity", 1, result.Children.Count());
			AssertEquals(EntityAction.MERGE, result.Action);
			AssertEquals(oC_Pk, result.Children.First().InternalPK);
			AssertEquals(EntityAction.MERGE, result.Children.First().Action);
		}

		public void TestConvertNotExistingBO()
		{
			var fakePK = Guid.NewGuid();
			var org = new RowID(fakePK, "OrgHeader");

			var converter =
				new BusinessObjectToEntityConverter
				{
					DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder()
				};

			AssertExceptionThrown(typeof(RowNotFoundException), String.Format("Cannot find row in table [OrgHeader] using PK [{0}].", fakePK), () => converter.GetEntity(org, null));
		}
	}
}
