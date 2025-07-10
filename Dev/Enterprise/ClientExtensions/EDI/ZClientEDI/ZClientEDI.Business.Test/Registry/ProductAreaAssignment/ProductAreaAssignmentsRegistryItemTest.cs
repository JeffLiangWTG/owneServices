using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ProductAreaAssignmentsRegistryItem))]
	class ProductAreaAssignmentsRegistryItemTest : StronglyTypedRegistryItemTestCase<ProductAreaAssignmentCollection, ProductAreaAssignmentCollection>
	{
		public void TestDefaultValue()
		{
			ProductAreaAssignmentCollection collection = new ProductAreaAssignmentCollection();
			ProductAreaAssignment assignment1 = collection.AddNew("ARC", "SCW");
			ProductAreaAssignment assignment2 = collection.AddNew("CUS", "TST");

			ProductAreaAssignmentsRegistryItem item = new ProductAreaAssignmentsRegistryItem("", null, null, null, RegistryStorageFlags.System, collection);
			AssertEquals(2, item.DefaultValue.Count);
		}

		protected override StronglyTypedRegistryItem<ProductAreaAssignmentCollection, ProductAreaAssignmentCollection> GetNewRegistryItem()
		{
			return new ProductAreaAssignmentsRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(ProductAreaAssignmentsRegistryDataType))]
	class ProductAreaAssignmentsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ProductAreaAssignmentsRegistryDataType>
	{
		#region Implementation

		protected override ProductAreaAssignmentsRegistryDataType GetNewDataType()
		{
			return new ProductAreaAssignmentsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ProductAreaAssignmentsRegistryEditor"; }
		}

		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			ProductAreaAssignment lhsParent = (ProductAreaAssignment)lhs;
			ProductAreaAssignment rhsParent = (ProductAreaAssignment)rhs;

			AssertEquals(lhsParent.ProductArea, rhsParent.ProductArea);
			AssertEquals(lhsParent.Staff, rhsParent.Staff);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			var first = GetValidSample(factory, new Guid("50FBAAE2-A31E-47AD-ACA0-C539AF2CC93F"), new Guid("7857FD79-D179-4BCC-8D03-1BAF16998393"), new byte[]
			{
				60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102,
				0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 65, 0, 114, 0, 114, 0, 97, 0, 121, 0, 79, 0, 102, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101,
				0, 110, 0, 116, 0, 32, 0, 120, 0, 109, 0, 108, 0, 110, 0, 115, 0, 58, 0, 120, 0, 115, 0, 105, 0, 61, 0, 34, 0, 104, 0, 116, 0, 116, 0, 112, 0, 58, 0, 47, 0, 47, 0, 119, 0, 119, 0, 119, 0, 46, 0, 119, 0, 51, 0, 46, 0, 111, 0, 114, 0, 103, 0, 47, 0, 50,
				0, 48, 0, 48, 0, 49, 0, 47, 0, 88, 0, 77, 0, 76, 0, 83, 0, 99, 0, 104, 0, 101, 0, 109, 0, 97, 0, 45, 0, 105, 0, 110, 0, 115, 0, 116, 0, 97, 0, 110, 0, 99, 0, 101, 0, 34, 0, 32, 0, 120, 0, 109, 0, 108, 0, 110, 0, 115, 0, 58, 0, 120, 0, 115, 0, 100,
				0, 61, 0, 34, 0, 104, 0, 116, 0, 116, 0, 112, 0, 58, 0, 47, 0, 47, 0, 119, 0, 119, 0, 119, 0, 46, 0, 119, 0, 51, 0, 46, 0, 111, 0, 114, 0, 103, 0, 47, 0, 50, 0, 48, 0, 48, 0, 49, 0, 47, 0, 88, 0, 77, 0, 76, 0, 83, 0, 99, 0, 104, 0, 101, 0, 109,
				0, 97, 0, 34, 0, 62, 0, 60, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101, 0, 110, 0, 116, 0, 62, 0, 60, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99,
				0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 62, 0, 65, 0, 82, 0, 67, 0, 60, 0, 47, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 62, 0, 60, 0, 83, 0, 116, 0, 97, 0, 102, 0, 102, 0, 62, 0, 102, 0, 98, 0, 97,
				0, 60, 0, 47, 0, 83, 0, 116, 0, 97, 0, 102, 0, 102, 0, 62, 0, 60, 0, 47, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101, 0, 110, 0, 116, 0, 62, 0, 60,
				0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101, 0, 110, 0, 116, 0, 62, 0, 60, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101,
				0, 97, 0, 62, 0, 67, 0, 85, 0, 83, 0, 60, 0, 47, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 62, 0, 60, 0, 83, 0, 116, 0, 97, 0, 102, 0, 102, 0, 62, 0, 102, 0, 100, 0, 100, 0, 60, 0, 47, 0, 83, 0, 116,
				0, 97, 0, 102, 0, 102, 0, 62, 0, 60, 0, 47, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101, 0, 110, 0, 116, 0, 62, 0, 60, 0, 47, 0, 65, 0, 114, 0, 114,
				0, 97, 0, 121, 0, 79, 0, 102, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101, 0, 110, 0, 116, 0, 62, 0
			});

			var second = GetValidSample(factory, new Guid("96DE3CFB-841A-41F2-95C3-2642A61A7415"), new Guid("FF263BFC-BB46-4F54-9227-C8150DB06914"), new byte[]
			{
				60, 0, 63, 0, 120, 0, 109, 0, 108, 0, 32, 0, 118, 0, 101, 0, 114, 0, 115, 0, 105, 0, 111, 0, 110, 0, 61, 0, 34, 0, 49, 0, 46, 0, 48, 0, 34, 0, 32, 0, 101, 0, 110, 0, 99, 0, 111, 0, 100, 0, 105, 0, 110, 0, 103, 0, 61, 0, 34, 0, 117, 0, 116, 0, 102,
				0, 45, 0, 49, 0, 54, 0, 34, 0, 63, 0, 62, 0, 60, 0, 65, 0, 114, 0, 114, 0, 97, 0, 121, 0, 79, 0, 102, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101,
				0, 110, 0, 116, 0, 32, 0, 120, 0, 109, 0, 108, 0, 110, 0, 115, 0, 58, 0, 120, 0, 115, 0, 105, 0, 61, 0, 34, 0, 104, 0, 116, 0, 116, 0, 112, 0, 58, 0, 47, 0, 47, 0, 119, 0, 119, 0, 119, 0, 46, 0, 119, 0, 51, 0, 46, 0, 111, 0, 114, 0, 103, 0, 47, 0, 50,
				0, 48, 0, 48, 0, 49, 0, 47, 0, 88, 0, 77, 0, 76, 0, 83, 0, 99, 0, 104, 0, 101, 0, 109, 0, 97, 0, 45, 0, 105, 0, 110, 0, 115, 0, 116, 0, 97, 0, 110, 0, 99, 0, 101, 0, 34, 0, 32, 0, 120, 0, 109, 0, 108, 0, 110, 0, 115, 0, 58, 0, 120, 0, 115, 0, 100,
				0, 61, 0, 34, 0, 104, 0, 116, 0, 116, 0, 112, 0, 58, 0, 47, 0, 47, 0, 119, 0, 119, 0, 119, 0, 46, 0, 119, 0, 51, 0, 46, 0, 111, 0, 114, 0, 103, 0, 47, 0, 50, 0, 48, 0, 48, 0, 49, 0, 47, 0, 88, 0, 77, 0, 76, 0, 83, 0, 99, 0, 104, 0, 101, 0, 109,
				0, 97, 0, 34, 0, 62, 0, 60, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101, 0, 110, 0, 116, 0, 62, 0, 60, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99,
				0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 62, 0, 65, 0, 82, 0, 67, 0, 60, 0, 47, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 62, 0, 60, 0, 83, 0, 116, 0, 97, 0, 102, 0, 102, 0, 62, 0, 100, 0, 101, 0, 99,
				0, 60, 0, 47, 0, 83, 0, 116, 0, 97, 0, 102, 0, 102, 0, 62, 0, 60, 0, 47, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101, 0, 110, 0, 116, 0, 62, 0, 60,
				0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101, 0, 110, 0, 116, 0, 62, 0, 60, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101,
				0, 97, 0, 62, 0, 67, 0, 85, 0, 83, 0, 60, 0, 47, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 62, 0, 60, 0, 83, 0, 116, 0, 97, 0, 102, 0, 102, 0, 62, 0, 102, 0, 102, 0, 98, 0, 60, 0, 47, 0, 83, 0, 116,
				0, 97, 0, 102, 0, 102, 0, 62, 0, 60, 0, 47, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101, 0, 110, 0, 116, 0, 62, 0, 60, 0, 47, 0, 65, 0, 114, 0, 114,
				0, 97, 0, 121, 0, 79, 0, 102, 0, 80, 0, 114, 0, 111, 0, 100, 0, 117, 0, 99, 0, 116, 0, 65, 0, 114, 0, 101, 0, 97, 0, 65, 0, 115, 0, 115, 0, 105, 0, 103, 0, 110, 0, 109, 0, 101, 0, 110, 0, 116, 0, 62, 0
			});

			return new[] { first, second };
		}

		ValidSampleAndBinaryValueInDB GetValidSample(BusinessObjectFactory factory, Guid staffPk1, Guid staffPk2, byte[] binaryValue)
		{
			var staff1 = GetOrNew<GlbStaff>(factory, staffPk1, item => item.GS_Code = CodeFromGuid(staffPk1));
			var staff2 = GetOrNew<GlbStaff>(factory, staffPk2, item => item.GS_Code = CodeFromGuid(staffPk2));

			var collection = new ProductAreaAssignmentCollection();
			collection.AddNew("ARC", staff1.GS_Code);
			collection.AddNew("CUS", staff2.GS_Code);

			return new ValidSampleAndBinaryValueInDB(collection, binaryValue);
		}

		#endregion
	}
}
