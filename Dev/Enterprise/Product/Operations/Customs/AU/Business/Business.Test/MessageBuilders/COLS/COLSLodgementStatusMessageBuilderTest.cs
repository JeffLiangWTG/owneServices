using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSLodgementStatusMessageBuilderTest : COLSMessageBuilderTest
	{
		public override void TestCreateNewMessage()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "TST";
			branch.GB_GC = company.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_GC = company.PK;
			declaration.JE_GB = branch.PK;
			Env.Registry.AUCustoms.SetLocalCustomsBranchIdentifierForBranch(branch.PK.ToGuid(), "AA33HF");

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = "123456";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = cusEntryHeader.PK;
			var entryNumObject = CusEntryNumber.New<CusEntryNumber>(colsHeader, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Australia);
			entryNumObject.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumObject.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumObject.CE_EntryNum = "CE1234";
			declaration.Factory.Save();

			var messageBuilder = new COLSLodgementStatusMessageBuilder(colsHeader);
			var message = messageBuilder.CreateNewMessage();

			CombineAssertions(() =>
			{
				AssertEquals("Message count", 1, colsHeader.Messages.Count);
				AssertEquals("Message type", AUCOLSMessageTypeList.Codes.LodgementStatus, message.EM_MessageType);
				AssertEquals("Message EM_LinkUniqueID", colsHeader.PK, message.EM_LinkUniqueID);
				AssertEquals("Message EM_LinkTable", colsHeader.TableName, message.EM_LinkTable);
				AssertEquals("Message text", "{}", message.EM_MessageText);
			});
		}
	}
}
