using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DocumentEmailFormatterTest : TestCaseWithFactory
	{
		public void TestGetEmailSubjectLine()
		{
			var formatter = new DocumentEmailFormatter();

			AssertEquals("GetEmailSubjectLine",
				"LBL - Los Branchos Locos - doc", formatter.GetEmailSubjectLine("doc"));
		}

		public void TestGetEmailSignature()
		{
			var formatter = new DocumentEmailFormatter();

			AssertEquals("GetEmailSignature",
@"Los Branchos Locos
Ronald McKwack", formatter.GetEmailSignature("doc"));
		}

		IDisposable tempUserContext;

		protected override void SetUp()
		{
			base.SetUp();

			var user = Factory.New<GlbStaff>();
			user.GS_FullName = "Ronald McKwack";
			user.GS_Code = "RMK";

			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_BranchName = "Los Branchos Locos";
			branch.GB_Code = "LBL";

			Factory.Save();

			tempUserContext = Env.SetTemporaryUserContext(
				user.PK.ToGuid(),
				branch.PK.ToGuid(),
				GlbDepartment.CurrentDepartment.PK.ToGuid());

			EmailFormat emailFormat = new EmailFormat();

			emailFormat.EmailSubjectFields.RemoveAndDeleteAll();
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("1", Enterprise.Core.Constants.EmailFormat.EmailFieldCodes.BranchCode));
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("2", Enterprise.Core.Constants.EmailFormat.EmailFieldCodes.BranchName));
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("3", Enterprise.Core.Constants.EmailFormat.EmailFieldCodes.DocumentName));

			emailFormat.EmailSignatureFields.RemoveAndDeleteAll();
			emailFormat.EmailSignatureFields.Add(new EmailSignatureField("1", Enterprise.Core.Constants.EmailFormat.EmailFieldCodes.BranchName));
			emailFormat.EmailSignatureFields.Add(new EmailSignatureField("2", Enterprise.Core.Constants.EmailFormat.EmailFieldCodes.UserName));

			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);
		}

		protected override void TearDown()
		{
			base.TearDown();
			tempUserContext?.Dispose();
		}
	}
}