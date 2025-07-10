using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestsSubclassesOf(typeof(FallbackMergedRegistryBusinessObjectCollectionDataType<>), typeof(ExcludeFromRegistryDataTypeTestAttribute))]
	public abstract class FallbackMergedRegistryBusinessObjectCollectionDataTypeTestCase : NonPersistentBusinessObjectRegistryDataTypeTestCase<IRegistryDataType>
	{
		public void TestIsFallBackMergeValuesImplemented()
		{
			AssertEquals("IsFallBackMergeValuesImplemented should be true", true, DataType.IsFallBackMergeValuesImplemented);
		}

		public void TestFallBackMergeValues()
		{
			RegistryBusinessObjectCollection companyCollection = GetNewCollection();
			RegistryBusinessObjectCollection branchCollection = GetNewCollection();

			RegistryBusinessObject companyElement1 = companyCollection.AddNew();
			RegistryBusinessObject companyElement2 = companyCollection.AddNew();

			RegistryBusinessObject branchElement1 = branchCollection.AddNew();
			RegistryBusinessObject branchElement2 = branchCollection.AddNew();

			companyElement1.Code = "ABC";
			companyElement1.Description = (NoResString)"ABCCompany";

			companyElement2.Code = "XYZ";
			companyElement2.Description = (NoResString)"XYZCompany";

			branchElement1.Code = "ABC";
			branchElement1.Description = (NoResString)"ABCBranch";

			branchElement2.Code = "NFI";
			branchElement2.Description = (NoResString)"NFIBranch";

			RegistryBusinessObjectCollection mergedCollection =
				(RegistryBusinessObjectCollection)DataType.FallBackMergeValues(branchCollection, companyCollection);
			AssertEquals("MergedCollection.Count", 3, mergedCollection.Count);

			RegistryBusinessObject obtainedElement1 = mergedCollection.FindByCode("ABC");
			RegistryBusinessObject obtainedElement2 = mergedCollection.FindByCode("XYZ");
			RegistryBusinessObject obtainedElement3 = mergedCollection.FindByCode("NFI");

			AssertEquals("ObtainedElement1.Description", "ABCBranch", obtainedElement1.Description);
			AssertEquals("ObtainedElement2.Description", "XYZCompany", obtainedElement2.Description);
			AssertEquals("ObtainedElement3.Description", "NFIBranch", obtainedElement3.Description);
		}

		#region Implementation

		RegistryBusinessObjectCollection GetNewCollection()
		{
			Type objectType = DataType.DataType;
			return (RegistryBusinessObjectCollection)Activator.CreateInstance(objectType);
		}

		#endregion
	}
}
