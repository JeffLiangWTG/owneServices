using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmActivityLog))]
	sealed class StmActivityLogTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPopulateFromStatistics()
		{
			FormUserStatistics stats = new FormUserStatistics();
			stats.NotifyFormShownUtc("Hello very very very very very very very very very very long description keepo going and goingHello very very very very very very very very very very long description keepo going and going this is the end", "heaps long module more and more and then there's more and heaps long module more and more and then there's more and heaps long module more and more and then there's more and this is the end ok.", ZDateTime.BrettsBirthday.AddYears(100).ToDateTime());
			stats.NotifyFormClosed(Guid.Empty, "");

			StmActivityLog log = Factory.New<StmActivityLog>();
			log.PopulateFromStatistics(null); // ensure does not throw null reference exception
			log.PopulateFromStatistics(stats);
			AssertEquals(0, log.S7_ActiveTime);
			AssertEquals("Hello very very very very very very very very very very long description keepo going and goingHello very very very very very very ve", log.S7_FormCaption);
			AssertEquals("heaps long module more and more and then there's more and heaps long module more and more and then there's more and heaps long modul", log.S7_ControllerID);
		}

		public void TestDurations()
		{
			StmActivityLog log = Factory.New<StmActivityLog>();
			AssertEquals(0m, log.ActiveDurationMinutes);
			AssertEquals(0m, log.InactiveDurationMinutes);
			log.S7_ActiveTime = 100;
			log.S7_InactiveTime = 3470;

			AssertEquals(1.7m, log.ActiveDurationMinutes);
			AssertEquals(57.8m, log.InactiveDurationMinutes);
		}

		public void TestRelatedBusinessObjectButtonText()
		{
			StmActivityLog log = Factory.New<StmActivityLog>();
			AssertEquals("Open Record", log.RelatedBusinessObjectButtonText);

			log.S7_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals("Open Record", log.RelatedBusinessObjectButtonText);

			log.S7_ControllerID = "Organisation";
			AssertEquals("Open " + DataBoundResourceStrings.GetTableDescriptiveName(OrgHeaderSchema.Constants.TableName) + " Record", log.RelatedBusinessObjectButtonText);

			log.S7_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			AssertEquals("Open " + DataBoundResourceStrings.GetTableDescriptiveName(GlbCompanySchema.Constants.TableName) + " Record", log.RelatedBusinessObjectButtonText);
		}

		public void TestUser()
		{
			StmActivityLog log = Factory.New<StmActivityLog>();
			log.S7_GS_NKUser = EnvProxy.Instance.CurrentUser.Initials;
			AssertNotNull(log.User);
		}

		public void TestLogging()
		{
			StmActivityLog log = Factory.New<StmActivityLog>();
			AssertEquals(false, log.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		public void TestReadOnly()
		{
			StmActivityLog log = Factory.New<StmActivityLog>();
			AssertEquals(true, log.ReadOnly);
		}

		public void TestCannotSaveInvalidRow_OpenDateTimeIsNull()
		{
			StmActivityLog log = Factory.New<StmActivityLog>();
			log.S7_FormCaption = "01010011 01101000 01110010 01100101 01101011 00100000 00110010";
			log.S7_OpenDateTimeUtc = ZDateTime.Empty;

			CombineAssertions("Precondition: Only the OpenDateTime is empty", () =>
			{
				Assert(!log.S7_FormCaption.IsEmpty);
				Assert(!log.S7_OpenDateTimeUtc.IsValid);
			});

			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		public void TestCannotSaveInvalidRow_RowIsEmpty()
		{
			StmActivityLog log = Factory.New<StmActivityLog>();
			log.S7_OpenDateTimeUtc = ZDateTime.Now;

			CombineAssertions("Precondition: S7_FormCaption, S7_ControllerID and S7_ParentTableCode are all null/empty", () =>
			{
				Assert(log.S7_FormCaption.IsEmpty);
				Assert(log.S7_ControllerID.IsEmpty);
				Assert(log.S7_ParentTableCode.IsEmpty);
			});

			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		public void TestStaffFullName()
		{
			IGlbStaff staff1 = Factory.New<IGlbStaff>();
			staff1.GS_Code = "S1";
			staff1.GS_FullName = "Staff Van";

			IGlbStaff staff2 = Factory.New<IGlbStaff>();
			staff2.GS_Code = "S2";
			staff2.GS_FullName = "Staff Tu";

			StmActivityLog log = Factory.New<StmActivityLog>();

			log.S7_GS_NKUser = "S2";
			AssertEquals("Staff Tu", log.UserFullName);

			log.S7_GS_NKUser = "XX";
			Assert(log.UserFullName.IsEmpty);
		}

		[ExpectNoExceptions]
		public void TestSafeParentTableCode()
		{
			StmActivityLog log = Factory.New<StmActivityLog>();
			log.S7_ParentTableCode = "BZZ";
			log.S7_ControllerID = "1";
			ZString test = log.RelatedBusinessObjectButtonText;
		}
	}
}
