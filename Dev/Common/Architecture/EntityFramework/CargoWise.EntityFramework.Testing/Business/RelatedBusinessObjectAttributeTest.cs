using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework.Testing
{
	sealed class RelatedBusinessObjectAttributeTest : TestCaseWithDummy
	{
		public void TestGetRelatedBizObjName()
		{
			TestBusinessObject dummy = new TestBusinessObject();
			PropertyDescriptor property = TypeDescriptor.GetProperties(dummy)["Self+Self+BusinessEntityFK"];
			string relatedBizObjName = RelatedBusinessObjectAttribute.GetRelatedBizObjName(property);
			AssertEquals("Should find the wrapped related business object name", "Self+Self+BusinessEntity", relatedBizObjName);
		}

		public void TestGetRelatedBizObjName_WhenNoRelatedBizObjName()
		{
			TestBusinessObject dummy = new TestBusinessObject();
			PropertyDescriptor property = TypeDescriptor.GetProperties(dummy)["PropertyWithoutRelatedBusinessObjet"];
			string relatedBizObjName = RelatedBusinessObjectAttribute.GetRelatedBizObjName(property);
			AssertEquals("When related business object name doesn't exist it should return null", null, relatedBizObjName);
		}

		public void TestGetCodeForGuid()
		{
			Dummy.Z0_Guid = ZGuid.Invalid;
			AssertEquals("", RelatedBusinessObjectAttribute.GetCodeForGuid(Dummy.Z0_GuidInfo));

			Dummy.Z0_Guid = ZGuid.Empty;
			AssertEquals("", RelatedBusinessObjectAttribute.GetCodeForGuid(Dummy.Z0_GuidInfo));

			Dummy.Z0_Guid = ZGuid.Missing;
			AssertEquals("", RelatedBusinessObjectAttribute.GetCodeForGuid(Dummy.Z0_GuidInfo));

			DummyBusinessObject relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Code = "oppo";
			Dummy.Z0_Guid = relatedDummy.PK;
			AssertEquals("oppo", RelatedBusinessObjectAttribute.GetCodeForGuid(Dummy.Z0_GuidInfo));

			Dummy.Z0_Guid = ZGuid.NewZGuid();
			AssertEquals("", RelatedBusinessObjectAttribute.GetCodeForGuid(Dummy.Z0_GuidInfo)); // no Code found
		}

		public void TestGetDescriptionForGuid()
		{
			Dummy.Z0_Guid = ZGuid.Invalid;
			AssertEquals("", RelatedBusinessObjectAttribute.GetDescriptionForGuid(Dummy.Z0_GuidInfo));

			Dummy.Z0_Guid = ZGuid.Empty;
			AssertEquals("", RelatedBusinessObjectAttribute.GetDescriptionForGuid(Dummy.Z0_GuidInfo));

			Dummy.Z0_Guid = ZGuid.Missing;
			AssertEquals("", RelatedBusinessObjectAttribute.GetDescriptionForGuid(Dummy.Z0_GuidInfo));

			DummyBusinessObject relatedDummy = Factory.New<DummyBusinessObject>();
			relatedDummy.Z0_Code = "oppo";
			relatedDummy.Z0_Description = "Splatty Splatty Blah Blah";
			Dummy.Z0_Guid = relatedDummy.PK;
			AssertEquals("Splatty Splatty Blah Blah", RelatedBusinessObjectAttribute.GetDescriptionForGuid(Dummy.Z0_GuidInfo));

			Dummy.Z0_Guid = ZGuid.NewZGuid();
			AssertEquals("", RelatedBusinessObjectAttribute.GetDescriptionForGuid(Dummy.Z0_GuidInfo)); // no Code found
		}

		public void TestGetMultilingualDescriptionForGuid()
		{
			var dummy = new TestBusinessObject();

			dummy.MultilingualBusinessObject = new TestMultilingualBusinessObject() { ObjectDescription = "description", ObjectDescriptionMultilingual = ResString.GetMultilingualString("0650d99c-d122-448e-a717-e5f7d69f973b", "multilingual description") };
			dummy.MultilingualEntityFK = dummy.MultilingualBusinessObject.PK;

			AssertEquals("multilingual description", RelatedBusinessObjectAttribute.GetDescriptionForGuid(dummy.MultilingualEntityFKInfo));
		}

		#region Implementation

		class TestBusinessObject : NonPersistentBusinessObject
		{
			public TestBusinessObject Self
			{
				get { return null; }
			}

			public DummyBusinessObject BusinessEntity
			{
				get { return null; }
			}

			[RelatedBusinessObject("BusinessEntity")]
			public ZGuid BusinessEntityFK
			{
				get { return ZGuid.Empty; }
			}

			public ZGuid PropertyWithoutRelatedBusinessObjet
			{
				get { return ZGuid.Empty; }
			}

			[RelatedBusinessObject("MultilingualBusinessObject")]
			public ZGuid MultilingualEntityFK { get; set; }

			public ZPropertyInfo MultilingualEntityFKInfo => GetZPropertyInfo(nameof(MultilingualEntityFK));

			public TestMultilingualBusinessObject MultilingualBusinessObject { get; set; }
		}

		[DescriptionProperty("ObjectDescription")]
		class TestMultilingualBusinessObject : NonPersistentBusinessObject
		{
			public ZString ObjectDescription { get; set; }

			public MultilingualString ObjectDescriptionMultilingual { get; set; }
		}

		#endregion
	}
}
