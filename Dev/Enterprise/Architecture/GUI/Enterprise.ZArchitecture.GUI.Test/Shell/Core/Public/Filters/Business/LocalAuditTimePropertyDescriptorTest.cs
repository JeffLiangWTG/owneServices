using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class LocalAuditTimePropertyDescriptorTest : TestCaseWithFactory
	{
		public void TestPropertyName()
		{
			var descriptor = new LocalAuditTimePropertyDescriptor(DummyBizoSchema.Z0_Date);
			AssertEquals("Z0_Date", descriptor.Name);
		}

		public void TestPropertyType()
		{
			var descriptor = new LocalAuditTimePropertyDescriptor(DummyBizoSchema.Z0_Date);
			AssertEquals(typeof(ZDateTime), descriptor.PropertyType);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestGetValue()
		{
			var descriptor = new LocalAuditTimePropertyDescriptor(DummyBizoSchema.Z0_Date);
			var dummyBizObj = Factory.New<DummyEnterpriseBusinessObject>();

			dummyBizObj.Z0_Date = new ZDateTime(2013, 1, 1, 0, 0, 0);
			AssertEquals(new ZDateTime(2013, 1, 1, 10, 0, 0), descriptor.GetValue(dummyBizObj));

			dummyBizObj.Z0_Date = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, descriptor.GetValue(dummyBizObj));

			dummyBizObj.Z0_Date = ZDateTime.Invalid;
			AssertEquals(ZDateTime.Invalid, descriptor.GetValue(dummyBizObj));
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestGetValue_OnChildProperty()
		{
			var descriptor = new LocalAuditTimePropertyDescriptor("Child.Z0_Date");
			var dummyBizObjParent = Factory.New<DummyEnterpriseBusinessObjectForTestingParent>();

			AssertEquals(null, descriptor.GetValue(dummyBizObjParent));

			dummyBizObjParent.Child = Factory.New<DummyEnterpriseBusinessObject>();
			dummyBizObjParent.Child.Z0_Date = new ZDateTime(2013, 1, 1, 0, 0, 0);

			AssertEquals(new ZDateTime(2013, 1, 1, 10, 0, 0), descriptor.GetValue(dummyBizObjParent));
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestSetValue()
		{
			var descriptor = new LocalAuditTimePropertyDescriptor(DummyBizoSchema.Z0_Date);
			var dummyBizObj = Factory.New<DummyEnterpriseBusinessObject>();

			descriptor.SetValue(dummyBizObj, new ZDateTime(2013, 1, 1, 0, 0, 0));
			AssertEquals(new ZDateTime(2012, 12, 31, 14, 0, 0), dummyBizObj.Z0_Date);

			descriptor.SetValue(dummyBizObj, ZDateTime.Empty);
			AssertEquals(ZDateTime.Empty, dummyBizObj.Z0_Date);

			descriptor.SetValue(dummyBizObj, ZDateTime.Invalid);
			AssertEquals(ZDateTime.Invalid, dummyBizObj.Z0_Date);
		}

		#region Implementation

		class DummyEnterpriseBusinessObjectForTestingParent : DummyEnterpriseBusinessObject
		{
			public DummyEnterpriseBusinessObjectForTestingParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DummyEnterpriseBusinessObject Child { get; set; }
		}

		#endregion
	}
}
