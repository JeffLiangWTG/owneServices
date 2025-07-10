using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusEntryNumberWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstructor_DefaultCountry()
		{
			var wrapperWithDefaultCountry = new CusEntryNumberWrapper(parent, EntryType);
			CreateCusEntryNumber(parent, EntryType, CountryCode, "ITMRN123");
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(wrapperWithDefaultCountry.EntryNumber, Is.EqualTo(ZString.Empty), "DE CusEntryNumber doesn't exist");

				CreateCusEntryNumber(parent, EntryType, Core.Constants.CountryCodes.Germany, "DEMRN123");
				NUnit.Framework.Assert.That(wrapperWithDefaultCountry.EntryNumber, Is.EqualTo("DEMRN123").Using(CustomComparers.TypeComparison), "DE CusEntryNumber exists");
			});
		}

		[ExpectNoExceptions]
		public void TestExistsCusEntryNumber()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				CreateCusEntryNumber(parent, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode, "ITLRN123");
				CreateCusEntryNumber(parent, EntryType, Core.Constants.CountryCodes.Germany, "DEMRN123");
				NUnit.Framework.Assert.That(wrapper.ExistsCusEntryNumber, Is.EqualTo(ZBool.False), "CusEntryNumber doesn't exist");
				NUnit.Framework.Assert.That(CusEntryNumber.Load(parent, EntryType, CountryCode), Is.EqualTo(default(CusEntryNumber)), "Getter doesn't create CusEntryNumber - should be [null]");

				var cusEntryNumber = CreateCusEntryNumber(parent, EntryType, CountryCode, "ITMRN123");
				NUnit.Framework.Assert.That(wrapper.ExistsCusEntryNumber, Is.EqualTo(ZBool.True), "CusEntryNumber exists");

				cusEntryNumber.Delete();
				NUnit.Framework.Assert.That(wrapper.ExistsCusEntryNumber, Is.EqualTo(ZBool.False), "CusEntryNumber deleted");
			});
		}

		[ExpectNoExceptions]
		public void TestEntryNumber_Getter()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				CreateCusEntryNumber(parent, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode, "ITLRN123");
				CreateCusEntryNumber(parent, EntryType, Core.Constants.CountryCodes.Germany, "DEMRN123");
				NUnit.Framework.Assert.That(wrapper.EntryNumber, Is.EqualTo(ZString.Empty), "CusEntryNumber doesn't exist");
				NUnit.Framework.Assert.That(CusEntryNumber.Load(parent, EntryType, CountryCode), Is.EqualTo(default(CusEntryNumber)), "Getter doesn't create CusEntryNumber - should be [null]");

				var cusEntryNumber = CreateCusEntryNumber(parent, EntryType, CountryCode, "ITMRN123");
				NUnit.Framework.Assert.That(wrapper.EntryNumber, Is.EqualTo("ITMRN123").Using(CustomComparers.TypeComparison), "CusEntryNumber exists");

				cusEntryNumber.Delete();
				NUnit.Framework.Assert.That(wrapper.EntryNumber, Is.EqualTo(ZString.Empty), "CusEntryNumber deleted");
			});
		}

		[ExpectNoExceptions]
		public void TestEntryNumber_Setter()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber, Is.EqualTo(default(CusEntryNumber)), "Initially CusEntryNumber doesn't exist - should be [null]");

				wrapper.SetEntryNumber("ITMRN123", parent.TestPropertyInfo);
				cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber.CE_EntryNum, Is.EqualTo("ITMRN123").Using(CustomComparers.TypeComparison), "CusEntryNumber created");

				var cusEntryNumberPK = cusEntryNumber.PK;
				wrapper.SetEntryNumber("ITMRN789", parent.TestPropertyInfo);
				cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber.PK, Is.EqualTo(cusEntryNumberPK), "Same CusEntryNumber kept instead of creating a new one");
				NUnit.Framework.Assert.That(cusEntryNumber.CE_EntryNum, Is.EqualTo("ITMRN789").Using(CustomComparers.TypeComparison), "CE_EntryNum updated");

				cusEntryNumber.Delete();
				wrapper.SetEntryNumber("ITMRN555", parent.TestPropertyInfo);
				cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber.PK, NUnit.Framework.Is.Not.EqualTo(cusEntryNumberPK), "New CusEntryNumber is created");
				NUnit.Framework.Assert.That(cusEntryNumber.CE_EntryNum, Is.EqualTo("ITMRN555").Using(CustomComparers.TypeComparison), "CE_EntryNum of new CusEntryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestEntryNumber_Setter_HasChanges()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(parent.HasChanges, Is.EqualTo(false), "Before EntryNumber is updated");
				wrapper.SetEntryNumber("123", parent.TestPropertyInfo);
				NUnit.Framework.Assert.That(parent.HasChanges, Is.EqualTo(true), "After EntryNumber is updated");
			});
		}

		[ExpectNoExceptions]
		public void TestEntryNumber_Setter_MaxLength()
		{
			wrapper.SetEntryNumber(new ZString('a', 35), parent.TestPropertyInfo);
			NUnit.Framework.Assert.That(LoadCusEntryNumber().CE_EntryNum, Is.EqualTo(new ZString('a', 10)), "Stored value is truncated to MaxLength");
		}

		[ExpectNoExceptions]
		public void TestExpiryDate_Getter()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				CreateCusEntryNumber(parent, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode, "ITLRN123", new DateTime(2020, 02, 15));
				CreateCusEntryNumber(parent, EntryType, Core.Constants.CountryCodes.Germany, "DEMRN123", new DateTime(2020, 02, 15));
				NUnit.Framework.Assert.That(wrapper.ExpiryDate, Is.EqualTo(ZDateTime.Empty), "CusEntryNumber doesn't exist");
				NUnit.Framework.Assert.That(CusEntryNumber.Load(parent, EntryType, CountryCode), Is.EqualTo(default(CusEntryNumber)), "Getter doesn't create CusEntryNumber - should be [null]");

				var cusEntryNumber = CreateCusEntryNumber(parent, EntryType, CountryCode, "ITMRN123", new DateTime(2020, 02, 15));
				NUnit.Framework.Assert.That(wrapper.ExpiryDate, Is.EqualTo(new ZDateTime(2020, 02, 15)), "CusEntryNumber exists");

				cusEntryNumber.Delete();
				NUnit.Framework.Assert.That(wrapper.ExpiryDate, Is.EqualTo(ZDateTime.Empty), "CusEntryNumber deleted");
			});
		}

		[ExpectNoExceptions]
		public void TestExpiryDate_Setter()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber, Is.EqualTo(default(CusEntryNumber)), "Initially CusEntryNumber doesn't exist - should be [null]");

				wrapper.SetExpiryDate(new ZDateTime(2020, 02, 15), parent.TestPropertyInfo);
				cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber.CE_ExpiryDate, Is.EqualTo(new ZDateTime(2020, 02, 15)), "CusEntryNumber created");

				var cusEntryNumberPK = cusEntryNumber.PK;
				wrapper.SetExpiryDate(new ZDateTime(2021, 05, 20), parent.TestPropertyInfo);
				cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber.PK, Is.EqualTo(cusEntryNumberPK), "Same CusEntryNumber kept instead of creating a new one");
				NUnit.Framework.Assert.That(cusEntryNumber.CE_ExpiryDate, Is.EqualTo(new ZDateTime(2021, 05, 20)), "CE_ExpiryDate updated");

				cusEntryNumber.Delete();
				wrapper.SetExpiryDate(new ZDateTime(2021, 12, 18), parent.TestPropertyInfo);
				cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber.PK, NUnit.Framework.Is.Not.EqualTo(cusEntryNumberPK), "New CusEntryNumber is created");
				NUnit.Framework.Assert.That(cusEntryNumber.CE_ExpiryDate, Is.EqualTo(new ZDateTime(2021, 12, 18)), "CE_ExpiryDate of new CusEntryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestExpiryDate_Setter_HasChanges()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(parent.HasChanges, Is.EqualTo(false), "Before ExpiryDate is updated");
				wrapper.SetExpiryDate(ZDateTime.Today, parent.TestPropertyInfo);
				NUnit.Framework.Assert.That(parent.HasChanges, Is.EqualTo(true), "After ExpiryDate is updated");
			});
		}

		[ExpectNoExceptions]
		public void TestIssueDate_Getter()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				CreateCusEntryNumber(parent, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode, "ITLRN123", issueDate: new DateTime(2020, 02, 15));
				CreateCusEntryNumber(parent, EntryType, Core.Constants.CountryCodes.Germany, "DEMRN123", issueDate: new DateTime(2020, 02, 15));
				NUnit.Framework.Assert.That(wrapper.IssueDate, Is.EqualTo(ZDateTime.Empty), "CusEntryNumber doesn't exist");
				NUnit.Framework.Assert.That(CusEntryNumber.Load(parent, EntryType, CountryCode), Is.EqualTo(default(CusEntryNumber)), "Getter doesn't create CusEntryNumber - should be [null]");

				var cusEntryNumber = CreateCusEntryNumber(parent, EntryType, CountryCode, "ITMRN123", issueDate: new DateTime(2020, 02, 15));
				NUnit.Framework.Assert.That(wrapper.IssueDate, Is.EqualTo(new ZDateTime(2020, 02, 15)), "CusEntryNumber exists");

				cusEntryNumber.Delete();
				NUnit.Framework.Assert.That(wrapper.IssueDate, Is.EqualTo(ZDateTime.Empty), "CusEntryNumber deleted");
			});
		}

		[ExpectNoExceptions]
		public void TestEntryStatus_Getter()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				CreateCusEntryNumber(parent, CusEntryNumberTypes.Standard.LocalReferenceNumber, CountryCode, "ITLRN123", entryStatus: "PRS");
				CreateCusEntryNumber(parent, EntryType, Core.Constants.CountryCodes.Germany, "DEMRN123", entryStatus: "PRS");
				NUnit.Framework.Assert.That(wrapper.EntryStatus, Is.EqualTo(ZString.Empty), "CusEntryNumber doesn't exist");
				NUnit.Framework.Assert.That(CusEntryNumber.Load(parent, EntryType, CountryCode), Is.EqualTo(default(CusEntryNumber)), "Getter doesn't create CusEntryNumber - should be [null]");

				var cusEntryNumber = CreateCusEntryNumber(parent, EntryType, CountryCode, "ITMRN123", entryStatus: "PRS");
				NUnit.Framework.Assert.That(wrapper.EntryStatus, Is.EqualTo("PRS").Using(CustomComparers.TypeComparison), "CusEntryNumber exists");

				cusEntryNumber.Delete();
				NUnit.Framework.Assert.That(wrapper.EntryStatus, Is.EqualTo(ZString.Empty), "CusEntryNumber deleted");
			});
		}

		[ExpectNoExceptions]
		public void TestEntryStatus_Setter()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber, Is.EqualTo(default(CusEntryNumber)), "Initially CusEntryNumber doesn't exist - should be [null]");

				wrapper.SetEntryStatus("OPN", parent.TestPropertyInfo);
				cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber.CE_EntryStatus, Is.EqualTo("OPN").Using(CustomComparers.TypeComparison), "CusEntryNumber created");

				var cusEntryNumberPK = cusEntryNumber.PK;
				wrapper.SetEntryStatus("PRS", parent.TestPropertyInfo);
				cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber.PK, Is.EqualTo(cusEntryNumberPK), "Same CusEntryNumber kept instead of creating a new one");
				NUnit.Framework.Assert.That(cusEntryNumber.CE_EntryStatus, Is.EqualTo("PRS").Using(CustomComparers.TypeComparison), "CE_EntryStatus updated");

				cusEntryNumber.Delete();
				wrapper.SetEntryStatus("CAN", parent.TestPropertyInfo);
				cusEntryNumber = LoadCusEntryNumber();
				NUnit.Framework.Assert.That(cusEntryNumber.PK, NUnit.Framework.Is.Not.EqualTo(cusEntryNumberPK), "New CusEntryNumber is created");
				NUnit.Framework.Assert.That(cusEntryNumber.CE_EntryStatus, Is.EqualTo("CAN").Using(CustomComparers.TypeComparison), "CE_EntryStatus of new CusEntryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestEntryStatus_Setter_HasChanges()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(parent.HasChanges, Is.EqualTo(false), "Before EntryStatus is updated");
				wrapper.SetEntryStatus("PRS", parent.TestPropertyInfo);
				NUnit.Framework.Assert.That(parent.HasChanges, Is.EqualTo(true), "After EntryStatus is updated");
			});
		}

		CusEntryNumber CreateCusEntryNumber(BusinessObject parent, string entryType, string countryCode, string entryNum, DateTime expiryDate = default, DateTime issueDate = default, string entryStatus = "")
		{
			var cusEntryNumber = CusEntryNumber.New(parent, entryType, countryCode);
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_ExpiryDate = expiryDate;
			cusEntryNumber.CE_IssueDate = issueDate;
			cusEntryNumber.CE_EntryStatus = entryStatus;
			return cusEntryNumber;
		}

		CusEntryNumber LoadCusEntryNumber()
		{
			return CusEntryNumber.Load(parent, EntryType, CountryCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			parent = Factory.New<CusEntryNumberWrapperParentForTest>();
			wrapper = new CusEntryNumberWrapper(parent, EntryType, CountryCode);
		}
		CusEntryNumberWrapperParentForTest parent;
		CusEntryNumberWrapper wrapper;

		const string EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
		const string CountryCode = Core.Constants.CountryCodes.Italy;
	}

	class CusEntryNumberWrapperParentForTest : DummyBusinessObject
	{
		public CusEntryNumberWrapperParentForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[MaxLength(10)]
		public ZString TestProperty => ZString.Empty;

		public ZPropertyInfo TestPropertyInfo => GetZPropertyInfo(nameof(TestProperty));
	}
}
