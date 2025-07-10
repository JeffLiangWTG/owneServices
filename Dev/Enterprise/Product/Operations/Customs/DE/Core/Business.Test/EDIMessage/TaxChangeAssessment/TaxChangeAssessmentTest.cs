using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(TaxChangeAssessment))]
	class TaxChangeAssessmentTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCodeAndDescriptionProperty()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CodeProperty", TaxChangeAssessment.Schema.ReferenceNumber, CodePropertyAttribute.CodePropertyNameFromType(typeof(TaxChangeAssessment)));
				AssertEquals("DescriptionProperty", TaxChangeAssessment.Schema.ReferenceNumber, DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(TaxChangeAssessment)));
			});
		}

		public void TestType()
		{
			taxChangeAssessment.CreateStmNote("Absehen", TaxChangeAssessment.Schema.TaxChangeAssessmentType);
			AssertEquals("Absehen", taxChangeAssessment.Type);
		}

		public void TestType_Caption()
		{
			AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(taxChangeAssessment.TypeInfo).Caption);
		}

		public void TestReferenceNumber()
		{
			CreateCusEntryNumber("DEMRN123");
			AssertEquals("DEMRN123", taxChangeAssessment.ReferenceNumber);
		}

		public void TestReferenceNumber_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(taxChangeAssessment.ReferenceNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Reference Number", resourceStringData.Caption);
				AssertEquals("ShortCaption", "Reference", resourceStringData.ShortCaption);
			});
		}

		public void TestLocalReferenceNumber()
		{
			taxChangeAssessment.CreateStmNote("B00000001", TaxChangeAssessment.Schema.LocalReferenceNumber);
			AssertEquals("B00000001", taxChangeAssessment.LocalReferenceNumber);
		}

		public void TestLocalReferenceNumber_Caption()
		{
			AssertEquals("LRN", DataBoundResourceStrings.GetDataForProperty(taxChangeAssessment.LocalReferenceNumberInfo).Caption);
		}

		public void TestIssueDate()
		{
			CreateCusEntryNumber("DEMRN123", issueDate: new DateTime(2020, 02, 15));
			AssertEquals(new ZDateTime(2020, 02, 15), taxChangeAssessment.IssueDate);
		}

		public void TestIssueDate_Caption()
		{
			AssertEquals("Issue Date", DataBoundResourceStrings.GetDataForProperty(taxChangeAssessment.IssueDateInfo).Caption);
		}

		public void TestMaturityDate()
		{
			CreateCusEntryNumber("DEMRN123", expiryDate: new DateTime(2021, 02, 15));
			AssertEquals(new ZDateTime(2021, 02, 15), taxChangeAssessment.MaturityDate);
		}

		public void TestMaturityDate_Caption()
		{
			AssertEquals("Maturity Date", DataBoundResourceStrings.GetDataForProperty(taxChangeAssessment.MaturityDateInfo).Caption);
		}

		public void TestEntryStatus()
		{
			CreateCusEntryNumber("DEMRN123", entryStatus: "PRS");
			AssertEquals("PRS", taxChangeAssessment.EntryStatus);
		}

		public void TestEntryStatus_Caption()
		{
			AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(taxChangeAssessment.EntryStatusInfo).Caption);
		}

		public void TestEntryStatus_MaxLength()
		{
			AssertEquals(3, taxChangeAssessment.EntryStatusInfo.MaxLength);
		}

		public void TestBranchCode_Caption()
		{
			AssertEquals("Branch", DataBoundResourceStrings.GetDataForProperty(typeof(TaxChangeAssessment), nameof(TaxChangeAssessment.BranchCode)).Caption);
		}

		public void TestBranchCode()
		{
			AssertEquals(taxChangeAssessment.Branch.GB_Code, taxChangeAssessment.BranchCode);
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("EM_ApplicationCode", EDIInterchange.ApplicationCodes.DECustomsAtlasSystem, taxChangeAssessment.EM_ApplicationCode);
				AssertEquals("EM_MessageType", EDIMessageTypeList.Codes.Import, taxChangeAssessment.EM_MessageType);
				AssertEquals("EM_MessageSubType", ImportMessageSubTypeList.Codes.SubsequentRaiseRefundOrAbatement, taxChangeAssessment.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", EDIInterchange.Direction.Receive, taxChangeAssessment.EM_ReceiveTransmit);
			});
		}

		public void TestLookups()
		{
			AssertType<TaxChangeAssessmentLookups>(taxChangeAssessment.Lookups);
		}

		public void TestValidation()
		{
			AssertType<TaxChangeAssessmentValidation>(taxChangeAssessment.Validation);
		}

		public void TestIRelatedJobMembers()
		{
			CreateCusEntryNumber("DEMRN123");
			CombineAssertions(() =>
			{
				AssertEquals("JobNumber", "DEMRN123", ((IRelatedJob)taxChangeAssessment).JobNumber);
				AssertEquals("JobDescription", ZString.Empty, ((IRelatedJob)taxChangeAssessment).JobDescription);
				AssertEquals("JobStatus", ZString.Empty, ((IRelatedJob)taxChangeAssessment).JobStatus);
				AssertEquals("ControllerID", ControllerIDs.Customs.DE.TaxChangeAssessment, ((IControllerIDProvider)taxChangeAssessment).ControllerID);
				AssertEquals("BusinessObjectPK", taxChangeAssessment.PK, ((IControllerIDProvider)taxChangeAssessment).BusinessObjectPK);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			taxChangeAssessment = Factory.New<TaxChangeAssessment>();
		}
		TaxChangeAssessment taxChangeAssessment;

		CusEntryNumber CreateCusEntryNumber(string entryNum, DateTime expiryDate = default, DateTime issueDate = default, string entryStatus = "")
		{
			var cusEntryNumber = CusEntryNumber.New(taxChangeAssessment, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_ExpiryDate = expiryDate;
			cusEntryNumber.CE_IssueDate = issueDate;
			cusEntryNumber.CE_EntryStatus = entryStatus;
			return cusEntryNumber;
		}
	}
}
