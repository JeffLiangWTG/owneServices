using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.Testing
{
	[TestedType(typeof(CusEntryNumStatus))]
	class CusEntryNumStatusTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestStatus()
		{
			CusEntryNumStatus status = (CusEntryNumStatus)GetNewBusinessObject();

			NUnit.Framework.Assert.That(status.Code, Is.EqualTo(TestCodeDescriptionPairList.Codes.Value1).Using(CustomComparers.TypeComparison), "Code should be TestCodeDescriptionPairList.Codes.Value1");
		}

		[ExpectNoExceptions]
		public void TestCodeDescription()
		{
			CusEntryNumStatus status = (CusEntryNumStatus)GetNewBusinessObject();

			status.Code = TestCodeDescriptionPairList.Codes.Value2;
			NUnit.Framework.Assert.That(status.Code, Is.EqualTo(TestCodeDescriptionPairList.Codes.Value2).Using(CustomComparers.TypeComparison), "Code should be TestCodeDescriptionPairList.Codes.Value2");
			NUnit.Framework.Assert.That(status.Description, Is.EqualTo(TestCodeDescriptionPairList.Descriptions.Value2).Using(CustomComparers.TypeComparison), "Description should be TestCodeDescriptionPairList.Descriptions.Value2");
		}

		[ExpectNoExceptions]
		public void TestCodeEmpty()
		{
			CusEntryNumStatus status = (CusEntryNumStatus)GetNewBusinessObject();

			status.Code = ZString.Empty;

			NUnit.Framework.Assert.That(status.Code, Is.EqualTo(ZString.Empty), "Code should be empty");
			NUnit.Framework.Assert.That(status.Description, Is.EqualTo(ZString.Empty), "Description should be empty");
		}

		[ExpectNoExceptions]
		public void TestReadOnlyProperties()
		{
			CusEntryNumStatus status = (CusEntryNumStatus)GetNewBusinessObject();

			NUnit.Framework.Assert.That(status.CodeInfo.ReadOnly, Is.EqualTo(true), "Code should be readonly");
			NUnit.Framework.Assert.That(status.DescriptionInfo.ReadOnly, Is.EqualTo(true), "Description should be readonly");
		}

		[ExpectNoExceptions]
		public void TestDefaultValue()
		{
			CusEntryNumStatus status = new CusEntryNumStatus(Factory.New(typeof(DummyBusinessObject)), new TestCodeDescriptionPairList(), TestCodeDescriptionPairList.Codes.Value1, "ABC", "ER");
			NUnit.Framework.Assert.That(status.Code, Is.EqualTo(TestCodeDescriptionPairList.Codes.Value1).Using(CustomComparers.TypeComparison), "Code");
			status.Code = TestCodeDescriptionPairList.Codes.Value1;
			NUnit.Framework.Assert.That(status.Code, Is.EqualTo(TestCodeDescriptionPairList.Codes.Value1).Using(CustomComparers.TypeComparison), "Code");
			status.Code = TestCodeDescriptionPairList.Codes.Value2;
			NUnit.Framework.Assert.That(status.Code, Is.EqualTo(TestCodeDescriptionPairList.Codes.Value2).Using(CustomComparers.TypeComparison), "Code");
		}

		[ExpectNoExceptions]
		public void TestStatusList()
		{
			CodeDescriptionPairList pairList = new TestCodeDescriptionPairList();
			CusEntryNumStatus status = new CusEntryNumStatus(Factory.New(typeof(DummyBusinessObject)), pairList, TestCodeDescriptionPairList.Codes.Value1, "ABC", "ER");
			NUnit.Framework.Assert.That(status.List, Is.SameAs(pairList), "List should be set");
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			DummyBusinessObject bizObj = Factory.New<DummyBusinessObject>();
			CusEntryNumber entryNumber = CusEntryNumber.New(bizObj, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			entryNumber.CE_EntryStatus = TestCodeDescriptionPairList.Codes.Value1;

			return new CusEntryNumStatus(bizObj, new TestCodeDescriptionPairList(), ZString.Empty, "~~~", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		class TestCodeDescriptionPairList : CodeDescriptionPairList
		{
			public static class Codes
			{
				public const string Value1 = "TE1";
				public const string Value2 = "TE2";
			}

			public static class Descriptions
			{
				public const string Value1 = "Test Description1";
				public const string Value2 = "Test Description2";
			}

			public TestCodeDescriptionPairList()
			{
				AddPair(Codes.Value1, Descriptions.Value1);
				AddPair(Codes.Value2, Descriptions.Value2);
			}
		}

		#endregion
	}
}
