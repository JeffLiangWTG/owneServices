using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class JXCWarningInfoCollectorTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestNullReference()
		{
			new JXCWarningInfoCollector(null);
		}

		public void TestIteratingJXCWarningInfoCollector()
		{
			DummyForTest dummy = Factory.New<DummyForTest>();
			DummyForTest dummy2 = Factory.New<DummyForTest>();
			dummy.RegisterEditableChildObject(dummy.Collection);
			DummyChildBusinessObject dummyChild = dummy.Collection.AddNew();
			using (dummy.SuspendValidationTesting())
			using (dummy2.SuspendValidationTesting())
			using (dummyChild.SuspendValidationTesting())
			using (dummy.AnotherEditableChildObject.SuspendValidationTesting())
			using (dummy.NonChildObject.SuspendValidationTesting())
			{
				ValidationHelper validationHelper = new ValidationHelper();
				validationHelper.AddJXCWarning(dummy.Z0_AnotherDateInfo, "AnotherDateWarning from Dummy");
				validationHelper.AddJXCWarning(dummy.AnotherEditableChildObject.Z0_AnotherDecimalInfo, "AnotherDecimalWarning from Dummy.AnotherEditableChildObject");
				validationHelper.AddJXCWarning(dummyChild.Z0_BoolInfo, "BoolWarning from DummyChild");
				validationHelper.AddJXCWarning(dummy2.Z0_VarBinaryMaxInfo, "ByteArrayWarning from Dummy2");
				validationHelper.AddJXCWarning(dummy.NonChildPropertyWrapperInfo, "NonChildPropertyWrapperWarning from Dummy");
				List<JXCWarningInfo> warningInfoList = new List<JXCWarningInfo>(new JXCWarningInfoCollector(dummy));
				SortWarningInfoList(warningInfoList);
				AssertEquals("There should be 4 JXC WarningInfo", 4, warningInfoList.Count);
				AssertJXCWarningInfo(warningInfoList[0], dummy.Z0_AnotherDateInfo, "AnotherDateWarning from Dummy");
				AssertJXCWarningInfo(warningInfoList[1], dummy.AnotherEditableChildObject.Z0_AnotherDecimalInfo, "AnotherDecimalWarning from Dummy.AnotherEditableChildObject");
				AssertJXCWarningInfo(warningInfoList[2], dummyChild.Z0_BoolInfo, "BoolWarning from DummyChild");
				AssertJXCWarningInfo(warningInfoList[3], dummy.NonChildPropertyWrapperInfo, "NonChildPropertyWrapperWarning from Dummy");
				dummy.RegisterEditableChildObject(dummy2);
				warningInfoList = new List<JXCWarningInfo>(new JXCWarningInfoCollector(dummy));
				AssertEquals("There should be 5 JXC WarningInfo. Should Include the warning from Dummy2", 5, warningInfoList.Count);
				SortWarningInfoList(warningInfoList);
				AssertJXCWarningInfo(warningInfoList[0], dummy.Z0_AnotherDateInfo, "AnotherDateWarning from Dummy");
				AssertJXCWarningInfo(warningInfoList[1], dummy.AnotherEditableChildObject.Z0_AnotherDecimalInfo, "AnotherDecimalWarning from Dummy.AnotherEditableChildObject");
				AssertJXCWarningInfo(warningInfoList[2], dummyChild.Z0_BoolInfo, "BoolWarning from DummyChild");
				AssertJXCWarningInfo(warningInfoList[3], dummy2.Z0_VarBinaryMaxInfo, "ByteArrayWarning from Dummy2");
				AssertJXCWarningInfo(warningInfoList[4], dummy.NonChildPropertyWrapperInfo, "NonChildPropertyWrapperWarning from Dummy");
			}
		}

		public void TestIsEmpty()
		{
			DummyForTest dummy = Factory.New<DummyForTest>();
			using (dummy.SuspendValidationTesting())
			{
				Assert("No JXC Warnings, should be empty", new JXCWarningInfoCollector(dummy).IsEmpty);
				new ValidationHelper().AddJXCWarning(dummy.Z0_AnotherDateInfo, "BLAH");
				Assert("JXC warning exists. Should not be empty", !new JXCWarningInfoCollector(dummy).IsEmpty);
			}
		}

		[ExpectNoExceptions]
		public void TestIsEmptyDoesNotChangeEnumeratorPosition()
		{
			var dummy = Factory.New<DummyForTest>();
			dummy.RegisterEditableChildObject(dummy.Collection);
			var child1 = dummy.Collection.AddNew();
			var child2 = dummy.Collection.AddNew();
			var child3 = dummy.Collection.AddNew();
			var collector = new JXCWarningInfoCollector(dummy.Collection);
			using (child1.SuspendValidationTesting())
			{
				new ValidationHelper().AddJXCWarning(child1.Z0_AnotherDateInfo, "BLAH");
				AssertEquals(false, collector.IsEmpty);
				AssertNoExceptionThrown(() =>
				{
					dummy.Collection.Remove(child2);
					dummy.Collection.Remove(child3);
				});
			}
		}

		void SortWarningInfoList(List<JXCWarningInfo> warningInfoList)
		{
			warningInfoList.Sort(delegate(JXCWarningInfo left, JXCWarningInfo right)
			{
				return left.WarningMessage.CompareTo(right.WarningMessage);
			});
		}

		void AssertJXCWarningInfo(JXCWarningInfo warningInfo, ZPropertyInfo expectedInfo, string expectedWarningMessage)
		{
			AssertEquals(expectedInfo, warningInfo.Info);
			AssertEquals(expectedWarningMessage, warningInfo.WarningMessage);
		}

		class DummyForTest : DummyBusinessObject
		{
			public DummyForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZDecimal ChildPropertyWrapper
			{
				get
				{
					return AnotherEditableChildObject.Z0_AnotherDecimal;
				}
			}

			public ZWrappedPropertyInfo ChildPropertyWrapperInfo
			{
				get
				{
					return GetWrappedZPropertyInfo(nameof(ChildPropertyWrapper), x => AnotherEditableChildObject.Z0_AnotherDecimalInfo);
				}
			}

			public ZDecimal NonChildPropertyWrapper
			{
				get
				{
					return NonChildObject.Z0_AnotherDecimal;
				}
			}

			public ZWrappedPropertyInfo NonChildPropertyWrapperInfo
			{
				get
				{
					return GetWrappedZPropertyInfo(nameof(NonChildPropertyWrapper), x => NonChildObject.Z0_AnotherDecimalInfo);
				}
			}

			public DummyChildBusinessObject AnotherEditableChildObject
			{
				get
				{
					if (fAnotherChildDummy == null)
					{
						fAnotherChildDummy = Factory.New<DummyChildBusinessObject>();
						RegisterEditableChildObject(fAnotherChildDummy);
					}

					return fAnotherChildDummy;
				}
			}

			public DummyChildBusinessObject NonChildObject
			{
				get
				{
					if (fNonChildObject == null)
					{
						fNonChildObject = Factory.New<DummyChildBusinessObject>();
					}

					return fNonChildObject;
				}
			}

			DummyChildBusinessObject fAnotherChildDummy;
			DummyChildBusinessObject fNonChildObject;
		}
	}
}
