using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSReassessmentMessageBuilderTest : TestCaseWithFactory
	{
		public void TestBuildMessage()
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
			Env.Registry.AUCustoms.SetLocalCustomsBranchIdentifierForBranch(branch.PK.ToGuid(), "aa33hf");
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "container number";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			colsHeader.QCH_BiosecurityImportConditionURL = "bicon reference";
			colsHeader.QCH_ImportPermitNumber = "permitnum";
			colsHeader.ResponsibleParty.E2_Contact = "123456789012345678901234567890123456789012";
			colsHeader.ResponsibleParty.E2_Phone = "+61 2 9744 8000";
			colsHeader.ResponsibleParty.E2_Mobile = "+61 415 923 947";
			colsHeader.ResponsibleParty.E2_Email = "123456789012345678901234567890123456789012";
			colsHeader.QCH_AlsoNotifyEmail = "thirdparty email address";
			colsHeader.QCH_ApprovedArrangementRefNum = "ref";
			colsHeader.QCH_LateLodgementReason = "lodgement reason";
			colsHeader.QCH_LateLodgementDetails = "lodgement details";
			colsHeader.DeliveryOrUnpack.Address1 = "Address1";
			colsHeader.DeliveryOrUnpack.Address2 = "ABCDEFGHIJKLMNOPQRSTUV";
			colsHeader.DeliveryOrUnpack.City = "City";
			colsHeader.DeliveryOrUnpack.Postcode = "Postcode";
			colsHeader.DeliveryOrUnpack.State = "State";
			colsHeader.QCH_DeliveryClassification = "class";
			var entryNumObject = Factory.New<CusEntryNumber>();
			entryNumObject.CE_ParentID = colsHeader.PK;
			entryNumObject.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			entryNumObject.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumObject.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumObject.CE_EntryNum = "LRN1";
			var direction1 = colsHeader.Directions.AddNew();
			direction1.AAAddress.Address1 = "AA Address1";
			direction1.AAAddress.E2_GovRegNum = "AA Reg number1";
			direction1.QCD_CO_Container = container.PK;
			direction1.QCD_Direction = "direction";
			direction1.QCD_TreatmentType = "treatment type1";
			var direction2 = colsHeader.Directions.AddNew();
			direction2.AAAddress.Address1 = "AA Address2";
			direction2.AAAddress.E2_GovRegNum = "AA Reg number2";
			direction2.QCD_CO_Container = container.PK;
			direction2.QCD_Direction = "";
			direction2.QCD_TreatmentType = "treatment type2";

			var messageBuilder = new COLSReassessmentMessageBuilder(colsHeader, "additionalComment" , "reassessmentReason", true);
			var message = messageBuilder.CreateNewMessage();
			AssertEquals("Message count", 1, colsHeader.Messages.Count);
			AssertEquals("Message type", AUCOLSMessageTypeList.Codes.RequestAReassessment, message.EM_MessageType);
			AssertEquals("Message EM_LinkUniqueID", colsHeader.PK, message.EM_LinkUniqueID);
			AssertEquals("Message EM_LinkTable", colsHeader.TableName, message.EM_LinkTable);
			AssertEquals("Message text", "{\"originalLrn\":\"LRN1\",\"brokerBranchId\":\"AA33HF\",\"reassessmentReason\":\"reassessmentReason\",\"contactName\":\"1234567890123456789012345678901234567890\",\"contactPhone\":\"0297448000\",\"contactEmail\":\"1234567890123456789012345678901234567890\",\"thirdPartyNotificationEmail\":\"thirdparty email address\",\"additionalComments\":\"additionalComment\",\"generalDeclaration\":\"True\",\"directionRequests\":[{\"direction\":\"direction\",\"directionLineContainer\":\"CONTAINER NUMBER\",\"treatmentType\":\"treatment type1\",\"location\":\"AA Address1\",\"aaname\":\"\",\"aanumber\":\"AA Reg number1\"}],\"documentationRequired\":true}", message.EM_MessageText);
		}

		public void TestBuildMessage_RemoveNullField()
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
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var container = Factory.New<CusContainer>();
			container.CO_ContainerNumber = "container number";
			var colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
			colsHeader.QCH_AlsoNotifyEmail = "";
			var entryNumObject = Factory.New<CusEntryNumber>();
			entryNumObject.CE_ParentID = colsHeader.PK;
			entryNumObject.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			entryNumObject.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			entryNumObject.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entryNumObject.CE_EntryNum = "LRN1";
			var direction1 = colsHeader.Directions.AddNew();
			direction1.AAAddress.Address1 = "AA Address1";
			direction1.AAAddress.E2_GovRegNum = "AA Reg number1";
			direction1.QCD_CO_Container = container.PK;
			direction1.QCD_Direction = "direction";
			direction1.QCD_TreatmentType = "treatment type1";

			var messageBuilder = new COLSReassessmentMessageBuilder(colsHeader, "additionalComment", "reassessmentReason", true);
			var message = messageBuilder.CreateNewMessage();
			AssertEquals("Message count", 1, colsHeader.Messages.Count);
			AssertEquals("Message type", AUCOLSMessageTypeList.Codes.RequestAReassessment, message.EM_MessageType);
			AssertEquals("Message EM_LinkUniqueID", colsHeader.PK, message.EM_LinkUniqueID);
			AssertEquals("Message EM_LinkTable", colsHeader.TableName, message.EM_LinkTable);
			AssertEquals("Message text", "{\"originalLrn\":\"LRN1\",\"brokerBranchId\":\"AA33HF\",\"reassessmentReason\":\"reassessmentReason\",\"contactName\":\"\",\"contactPhone\":\"\",\"contactEmail\":\"\",\"additionalComments\":\"additionalComment\",\"generalDeclaration\":\"True\",\"directionRequests\":[{\"direction\":\"direction\",\"directionLineContainer\":\"CONTAINER NUMBER\",\"treatmentType\":\"treatment type1\",\"location\":\"AA Address1\",\"aaname\":\"\",\"aanumber\":\"AA Reg number1\"}],\"documentationRequired\":true}", message.EM_MessageText);
		}
	}
}
