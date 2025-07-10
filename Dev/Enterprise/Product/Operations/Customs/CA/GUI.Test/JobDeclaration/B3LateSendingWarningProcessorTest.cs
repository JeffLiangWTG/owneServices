using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing;

sealed class B3LateSendingWarningProcessorTest : TestCaseWithFactory
{
	public void TestCanadianB3LateWarningSentLog()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "T";
		staff.GS_LoginName = "T";
		staff.GS_EmailAddress = "t@wisetechglobal.com";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
		declaration.JE_DeclarationReference = "B00000001";
		declaration.JE_SystemCreateUser = staff.GS_Code;

		var processor = new B3LateSendingWarningProcessor(declaration);
		processor.Process();

		var logs = declaration.Logs;
		ZQuery bwsfilter = new ZQuery(StmALogSchema.SL_Parent, declaration.PK);
		bwsfilter.AddToFilter(StmALogSchema.SL_Reference, "CADWarning Reported");
		var bwsLog = logs.Find(bwsfilter);
		AssertNotNull(bwsLog);
		AssertEquals("BWS", bwsLog[0].SL_SE_NKEvent);
	}

	public void TestB3LateSendingWarningProcess_CON()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "T";
		staff.GS_LoginName = "T";
		staff.GS_EmailAddress = "t@wisetechglobal.com";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
		declaration.JE_DeclarationReference = "B00000001";
		declaration.JE_SystemCreateUser = staff.GS_Code;

		Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		var processor = new B3LateSendingWarningProcessor(declaration);
		processor.Process();

		var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "CAD Late Sending Warning");
		AssertNotNull("CAD Late Sending Warning should be created", email);
		Assert("Email.Body", email.Body.Contains("This LVS job"));
		Assert("Email.Body", email.Body.Contains(ShowEditFormUrlHandler.Instance.Create(ControllerIDs.Customs.JobDeclaration, declaration.PK)));
	}

	[TestDate(2015, 2, 16)]
	public void TestB3LateSendingWarningProcess_LVS()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "T";
		staff.GS_LoginName = "T";
		staff.GS_EmailAddress = "t@wisetechglobal.com";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_DeclarationReference = "B00000001";
		declaration.JE_SystemCreateUser = staff.GS_Code;

		Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		var processor = new B3LateSendingWarningProcessor(declaration);
		processor.Process();

		var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "CAD Late Sending Warning");
		AssertNull("CAD Late Sending Warning should not be created", email);
		declaration.JE_EntryAuthorisationDate = new DateTime(2015, 2, 10);

		Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		processor.Process();

		email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "CAD Late Sending Warning");
		AssertNotNull("CAD Late Sending Warning should be created", email);
		Assert(email.Body, email.Body.Contains("it is now 4 working days since release"));
		Assert("Email.Body", email.Body.Contains(ShowEditFormUrlHandler.Instance.Create(ControllerIDs.Customs.JobDeclaration, declaration.PK)));
	}
}
