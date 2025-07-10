using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class SetEntryStatusDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestFieldsMandatory()
		{
			SetUpEntryStatusDetail();
			ValidationTestHelper.AssertErrorIfNotEntered(setEntryStatusDetail.CusEntryHeaderPKInfo);
			ValidationTestHelper.AssertErrorIfNotEntered(setEntryStatusDetail.EventTimeInfo);
			ValidationTestHelper.AssertErrorIfNotEntered(setEntryStatusDetail.EntryStatusInfo);
		}

		public void TestCusEntryHeaderPKInList()
		{
			SetUpEntryStatusDetail();
			setEntryStatusDetail.CusEntryHeaderPK = Guid.NewGuid();
			AssertHasErrorContaining("Entry in list", setEntryStatusDetail.CusEntryHeaderPKInfo, ListValidation.InvalidCodeError);
			setEntryStatusDetail.CusEntryHeaderPK = entryHeader.PK;
			AssertNoErrorContaining("Entry in list", setEntryStatusDetail.CusEntryHeaderPKInfo, ListValidation.InvalidCodeError);
		}

		public void TestEntryStatusCodeInList()
		{
			Factory.SetupEntryStatusList();
			SetUpEntryStatusDetail();
			setEntryStatusDetail.EntryStatus = "CT0";
			AssertHasErrorContaining("status code not in list", setEntryStatusDetail.EntryStatusInfo, ListValidation.InvalidCodeError);
			setEntryStatusDetail.EntryStatus = "ST1";
			AssertNoErrorContaining("status code in list", setEntryStatusDetail.EntryStatusInfo, ListValidation.InvalidCodeError);
		}

		public void TestEntryStatusCodeMustBeChanged()
		{
			const string message = "Entry Status must be changed.";
			CombineAssertions(() =>
			{
				Factory.SetupEntryStatusList();
				SetUpEntryStatusDetail();
				setEntryStatusDetail.Validation.ValidateEntryStatus();
				AssertHasError("status not change", setEntryStatusDetail.EntryStatusInfo, message);
				setEntryStatusDetail.EntryStatus = "ST2";
				AssertNoError("status changed", setEntryStatusDetail.EntryStatusInfo, message);

				setEntryStatusDetail.EntryStatus = "ST1";
				AssertHasError("no error message for status not changed", setEntryStatusDetail.EntryStatusInfo, message);
				AssertNoWarning("no warning message for status not changed", setEntryStatusDetail.EntryStatusInfo, message);
				var log = entryHeader.Logs.AddNew(Events.CustomsEntryStatus, "ST1", ZDateTimeOffset.Now);
				log.IsCancelled = true;
				setEntryStatusDetail.Validation.ValidateEntryStatus();
				AssertNoError("no error message for status not changed and has log", setEntryStatusDetail.EntryStatusInfo, message);
				AssertHasWarning("warning message for status changed and has log", setEntryStatusDetail.EntryStatusInfo, message);
			});
		}

		public void TestCheckEventTime()
		{
			const string message = "Event time should not be in the future.";
			CombineAssertions(() =>
			{
				SetUpEntryStatusDetail();
				setEntryStatusDetail.EventTime = ZDateTime.Now.AddDays(2);
				AssertHasError("In the future.", setEntryStatusDetail.EventTimeInfo, message);
				setEntryStatusDetail.EventTime = ZDateTime.Now.AddDays(-2);
				AssertNoError("Not in the future.", setEntryStatusDetail.EventTimeInfo, message);
			});
		}

		void SetUpEntryStatusDetail()
		{
			declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "ST1";
			setEntryStatusDetail = new SetEntryStatusDetail(declaration);
		}

		SetEntryStatusDetail setEntryStatusDetail;
		JobDeclaration declaration;
		CusEntryHeader entryHeader;
	}
}
