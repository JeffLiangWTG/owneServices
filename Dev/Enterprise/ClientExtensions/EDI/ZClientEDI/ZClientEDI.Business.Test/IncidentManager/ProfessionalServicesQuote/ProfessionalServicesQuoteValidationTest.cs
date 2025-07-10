using System;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	internal class ProfessionalServicesQuoteValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckIM_EstimatedHours()
		{
			TestPropertyCannotBeNegative(BizObj.IM_EstimatedHoursInfo);
		}

		public void TestCheckIM_CloseTimeUtc()
		{
			Assert("Precondition: IM_CloseTimeUtc should be empty", BizObj.IM_CloseTimeUtc.IsEmpty);
			Assert("Precondition: IM_CloseTimeUtc should not have errors", !BizObj.IM_CloseTimeUtcInfo.HasErrors());

			BizObj.Validation.ValidateIM_CloseTimeUtc();
			Assert("IM_CloseTimeUtc should not have errors", !BizObj.IM_CloseTimeUtcInfo.HasErrors());

			BizObj.IM_SystemCreateTimeUtc = ZDateTime.Now;
			BizObj.IM_CloseTimeUtc = BizObj.IM_SystemCreateTimeUtc.AddHours(-1);
			Assert("IM_CloseTimeUtc should have an error if it is smaller than IM_SystemCreateTimeUtc",
				BizObj.IM_CloseTimeUtcInfo.HasError("The Close Date must be greater than the Start Date."));

			BizObj.IM_CloseTimeUtc = ZDateTime.Now.AddHours(+1);
			Assert("IM_CloseTimeUtc should not have errors", !BizObj.IM_CloseTimeUtcInfo.HasErrors());
		}

		public void TestCheckIM_Description()
		{
			Assert("Precondition: IM_Description should be empty", BizObj.IM_Description.IsEmpty);

			BizObj.Validation.ValidateIM_Description();
			Assert("IM_Description should have an error if it is less than 10 characters",
				BizObj.IM_DescriptionInfo.HasError("You must enter a Description of more than 10 characters."));

			BizObj.IM_Description = "abcd";
			Assert("IM_Description should have an error if it is less than 10 characters",
				BizObj.IM_DescriptionInfo.HasError("You must enter a Description of more than 10 characters."));

			BizObj.IM_Description = "abcdefghijk";
			Assert("IM_Details should not have errors", !BizObj.IM_DescriptionInfo.HasErrors());

			BizObj.IM_Description = "";
			Assert("IM_Description should have an error if it is less than 10 characters",
				BizObj.IM_DescriptionInfo.HasError("You must enter a Description of more than 10 characters."));
		}

		public void TestCheckIM_Status()
		{
			AssertNoErrors("Precondition: IM_Status should not have errors.", BizObj.IM_StatusInfo);
			AssertNotNullOrEmpty("Precondition: StatusList[0].Code should not be empty.", BizObj.Lookups.StatusList[0].Code);

			BizObj.IM_Status = "";
			AssertHasError(BizObj.IM_StatusInfo, (GetMissingFieldErrorMessage(BizObj.IM_StatusInfo)));

			BizObj.IM_Status = BizObj.Lookups.StatusList[0].Code;
			AssertNoErrors(BizObj.IM_StatusInfo);

			BizObj.IM_Status = "@$%";
			AssertHasError(BizObj.IM_StatusInfo, (GetInvalidFieldErrorMessage(BizObj.IM_StatusInfo)));
		}

		public void TestCheckIM_GG_Team()
		{
			AssertNoErrors("Precondition: IM_GG_Team should not have errors.", BizObj.IM_GG_TeamInfo);

			BizObj.IM_GG_Team = ZGuid.Empty;
			AssertHasError("IM_GG_Team should have an error because it is mandatory.", BizObj.IM_GG_TeamInfo, GetMissingFieldErrorMessage(BizObj.IM_GG_TeamInfo));

			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = ValidGroupCode;

			BizObj.IM_GG_Team = group.PK;
			AssertNoErrors("IM_GG_Team should not have errors.", BizObj.IM_GG_TeamInfo);

			GlbStaff newStaff1 = Factory.New<GlbStaff>();
			GlbStaff newStaff2 = Factory.New<GlbStaff>();

			newStaff1.GS_Code = "AAA";
			newStaff1.GS_LoginName = "NewStaff1";
			newStaff1.Groups.Add(group);

			newStaff2.GS_Code = "BBB";
			newStaff2.GS_LoginName = "NewStaff2";

			BizObj.IM_GG_Team = group.PK;
			BizObj.IM_GS_NKAssignedToCurrent = newStaff1.GS_Code;
			AssertNoNotifications("IM_GG_Team should not have warnings if the assigned staff is a member of the group.", BizObj.IM_GG_TeamInfo);

			BizObj.IM_GS_NKAssignedToCurrent = newStaff2.GS_Code;
			AssertNoErrors(BizObj.IM_GG_TeamInfo);
			AssertHasWarning("IM_GG_Team should have a warning if the assigned staff isn't a member of the group.",
				BizObj.IM_GG_TeamInfo, "The Assigned User 'BBB' is not a member of the Team '" + group.GG_Code + "'.");

			BizObj.IM_GG_Team = ZGuid.NewZGuid();
			AssertHasError(BizObj.IM_GG_TeamInfo, "Enter a valid Team.");
		}

		public void TestCheckIM_Priority()
		{
			AssertEquals("Precondition: IM_Priority should not have errors", false, BizObj.IM_PriorityInfo.HasErrors());
			AssertNotNullOrEmpty("Precondition: IM_Priority_List[0].Code should not be empty", BizObj.Lookups.PriorityList[0].Code);

			BizObj.IM_Priority = "XXX";
			AssertEquals("IM_Priority should have an error if it is invalid", true, BizObj.IM_PriorityInfo.HasError(GetInvalidFieldErrorMessage(BizObj.IM_PriorityInfo)));

			BizObj.IM_Priority = BizObj.Lookups.PriorityList[0].Code;
			AssertEquals("IM_Priority should not an error if it is valid", false, BizObj.IM_PriorityInfo.HasErrors());

			BizObj.IM_Priority = "";
			AssertEquals("IM_Priority should not an error if it is empty", false, BizObj.IM_PriorityInfo.HasErrors());
		}

		public void TestCheckIM_WorkItemType()
		{
			AssertNoErrors("Precondition: IM_WorkItemType should not have errors.", BizObj.IM_WorkItemTypeInfo);
			AssertNotNullOrEmpty("Precondition: WorkItemTypeList[0].Code should not be empty.", BizObj.Lookups.WorkItemTypeList[0].Code);

			BizObj.IM_WorkItemType = ")_+";
			AssertHasError(BizObj.IM_WorkItemTypeInfo, GetInvalidFieldErrorMessage(BizObj.IM_WorkItemTypeInfo));

			BizObj.IM_WorkItemType = BizObj.Lookups.WorkItemTypeList[0].Code;
			AssertNoErrors(BizObj.IM_WorkItemTypeInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = GetNewBusinessObject();
			BizObj.IM_Status = ProfessionalServicesQuoteLookups.ProfessionalServiceQuoteStatusCodes.Quote;
		}

		protected string GetMissingFieldErrorMessage(ZPropertyInfo propertyInfo)
		{
			return MandatoryValidation.MustBeEnteredMessage(propertyInfo.Description);
		}

		protected string GetInvalidFieldErrorMessage(ZPropertyInfo propertyInfo)
		{
			return "Enter a valid " + propertyInfo.Description + ".";
		}

		protected virtual ZString ValidGroupCode
		{
			get { return "GRP"; }
		}

		protected virtual ProfessionalServicesQuote GetNewBusinessObject()
		{
			return Factory.New<ProfessionalServicesQuote>();
		}

		protected void AssertMustHaveValue(ZPropertyInfo propertyInfo)
		{
			propertyInfo.Value = ZGuid.Empty;
			AssertHasError(propertyInfo, string.Format(CultureInfo.CurrentCulture, "Please enter a {0}.", propertyInfo.Description));

			propertyInfo.Value = ZGuid.NewZGuid();
			AssertNoErrors(propertyInfo);
		}

		protected void AssertMustBeEmpty(ZPropertyInfo propertyInfo)
		{
			propertyInfo.Value = ZGuid.NewZGuid();
			AssertHasError(propertyInfo, string.Format(CultureInfo.CurrentCulture, "Please do not enter a {0}.", propertyInfo.Description));

			propertyInfo.Value = ZGuid.Empty;
			AssertNoErrors(propertyInfo);
		}

		protected void TestPropertyCannotBeNegative(ZPropertyInfo propertyInfo)
		{
			AssertNoErrors("Precondition: " + propertyInfo.Name + " should not have errors.", propertyInfo);

			propertyInfo.Value = GetNumericZType(propertyInfo, -1);
			AssertHasError(propertyInfo, "Please enter " + Grammar.Instance.IndefiniteArticlePrefix(propertyInfo.Description) + "'" + propertyInfo.Description + "' greater than or equal to 0.");

			propertyInfo.Value = GetNumericZType(propertyInfo, 0);
			AssertNoErrors(propertyInfo);

			propertyInfo.Value = GetNumericZType(propertyInfo, 1);
			AssertNoErrors(propertyInfo);
		}

		INumericZType GetNumericZType(ZPropertyInfo propertyInfo, Int16 value)
		{
			return (INumericZType)Activator.CreateInstance(propertyInfo.PropertyType, new object[] { value });
		}

		protected ProfessionalServicesQuote BizObj;

		#endregion

		public void TestCheckIM_GS_CurrentlyAssignedTo()
		{
			BizObj.IM_GS_NKAssignedToCurrent = ZString.Empty;
			AssertNoErrors(BizObj.IM_GS_NKAssignedToCurrentInfo);
		}

		public void TestCheckIM_GS_CustomerServiceContact()
		{
			BizObj.IM_GS_NKCustServiceContact = ZString.Empty;
			AssertHasErrors(BizObj.IM_GS_NKCustServiceContactInfo);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			BizObj.IM_GS_NKCustServiceContact = staff.GS_Code;
			AssertNoErrors(BizObj.IM_GS_NKCustServiceContactInfo);
		}
	}
}
