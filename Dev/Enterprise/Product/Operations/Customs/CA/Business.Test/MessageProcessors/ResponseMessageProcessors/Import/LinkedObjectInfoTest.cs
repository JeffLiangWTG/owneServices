using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class LinkedObjectInfoTest : TestCaseWithFactory
	{
		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestNotificationBranch()
		{
			var declaration = (JobDeclaration)JobDeclaration.New(Factory);
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "10006789";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ZString.Empty;
			declaration.JE_IsCancelled = false;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

			var declarationCCN = declaration.AdditionalReferenceNumbers.AddNew();
			declarationCCN.CE_EntryNum = "CCN123856";
			declarationCCN.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			declarationCCN.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			Factory.Save();

			var company = Factory.Load<GlbCompany>(declaration.JE_GC);
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BBB";
			branch.GB_BranchName = "Test Branch";
			Factory.Save();

			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK));
			var info1 = new LinkedObjectInfo(entryHeader, ZString.Empty);
			var info2 = new LinkedObjectInfo(declaration, ZString.Empty);
			var info3 = new LinkedObjectInfo(null, ZString.Empty);
			AssertEquals(entryHeader.Branch.PK, info1.NotificationBranch.PK);
			AssertNotEquals(branch.PK, info1.NotificationBranch.PK);
			AssertEquals(declaration.Branch.PK, info2.NotificationBranch.PK);
			AssertNotEquals(branch.PK, info2.NotificationBranch.PK);
			AssertEquals(branch.PK, info3.NotificationBranch.PK);
		}
	}
}
