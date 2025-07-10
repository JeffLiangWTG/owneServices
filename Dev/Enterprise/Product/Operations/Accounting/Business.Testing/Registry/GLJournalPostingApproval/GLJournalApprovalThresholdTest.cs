using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GLJournalApprovalThreshold))]
	public class GLJournalApprovalThresholdTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestCloneShouldIncludeReadOnlyStatus()
		{
			var collection = new GLJournalApprovalThresholdCollection();
			var newElement = collection.AddNew();
			newElement.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;
			Assert(newElement.AuthorisationSettings.ReadOnly);

			var clone = (GLJournalApprovalThreshold)newElement.Clone(new FallbackLevel(new Guid(), new Guid(), new Guid()), Factory);
			Assert(clone.AuthorisationSettings.ReadOnly);
		}

		public void TestType()
		{
			var query = new ZQuery(AccGLHeaderSchema.AG_AccountType, "BSH");
			query.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, "P&L");

			var element = new GLJournalApprovalThreshold();
			element.Type = GLJournalApprovalThreshold.TypeCodes.GLAccount;
			element.GLAccount = Factory.LoadTop1<AccGLHeader>(query).PK;
			Assert("GLAccount", !element.GLAccount.IsEmpty);
			Assert("Description", !element.Description.IsEmpty);
			element.Type = GLJournalApprovalThreshold.TypeCodes.ReportSection;
			AssertEquals(GLJournalApprovalThreshold.TypeCodes.ReportSection, element.Type);
			Assert(element.GLAccount.IsEmpty);
			element.ReportSection = AccGLHeader.Constants.SectionTypes.Codes.Overheads;
			Assert("ReportSection", !element.ReportSection.IsEmpty);
			AssertEquals("Description", AccGLHeader.Constants.SectionTypes.Descriptions.Overheads, element.Description);
			element.Type = GLJournalApprovalThreshold.TypeCodes.GLAccount;
			AssertEquals(GLJournalApprovalThreshold.TypeCodes.GLAccount, element.Type);
			Assert(element.ReportSection.IsEmpty);
			var glAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery());
			element.GLAccount = glAccount.PK;
			Assert("GLAccount", !element.GLAccount.IsEmpty);
			AssertEquals("Description", glAccount.AG_DescriptionMultilingual, element.Description);
			element.Type = GLJournalApprovalThreshold.TypeCodes.All;
			AssertEquals(GLJournalApprovalThreshold.TypeCodes.All, element.Type);
			Assert("GLAccount", element.GLAccount.IsEmpty);
			Assert("ReportSection", element.ReportSection.IsEmpty);
			AssertEquals("Description", "All GL Accounts", element.Description);

			element.Type = GLJournalApprovalThreshold.TypeCodes.ReportSection;
			AssertEquals(GLJournalApprovalThreshold.TypeCodes.ReportSection, element.Type);
			Assert(element.GLAccount.IsEmpty);
			element.ReportSection = AccGLHeader.Constants.SectionTypes.Codes.TradingStatement;
			Assert("ReportSection", !element.ReportSection.IsEmpty);
			AssertEquals("Description", AccGLHeader.Constants.SectionTypes.Descriptions.TradingStatement, element.Description);
			element.Type = GLJournalApprovalThreshold.TypeCodes.All;
			AssertEquals(GLJournalApprovalThreshold.TypeCodes.All, element.Type);
			Assert("GLAccount", element.GLAccount.IsEmpty);
			Assert("ReportSection", element.ReportSection.IsEmpty);
			AssertEquals("Description", "All GL Accounts", element.Description);
		}

		public void TestTypeAnyChanges()
		{
			var query = new ZQuery(AccGLHeaderSchema.AG_AccountType, "BSH");
			query.AddToFilter(JoinCondition.Or, AccGLHeaderSchema.AG_AccountType, SQLComparisonOperator.Equal, "P&L");

			var element = new GLJournalApprovalThreshold();
			element.Type = GLJournalApprovalThreshold.TypeCodes.GLAccount;
			element.GLAccount = Factory.LoadTop1<AccGLHeader>(query).PK;
			Assert("GLAccount", !element.GLAccount.IsEmpty);
			Assert("Description", !element.Description.IsEmpty);

			AmountBasedMultiLevelAuthorisationRequirement setting1 = element.AuthorisationSettings.AddNew();
			AmountBasedMultiLevelAuthorisationRequirement setting2 = element.AuthorisationSettings.AddNew();
			AmountBasedMultiLevelAuthorisationRequirement setting3 = element.AuthorisationSettings.AddNew();
			AmountBasedMultiLevelAuthorisationRequirement setting4 = element.AuthorisationSettings.AddNew();

			setting1.Range = "Up to";
			setting1.Amount = 100;
			setting2.Range = "Above";
			setting2.Amount = 300;
			setting3.Range = "Up to";
			setting3.Amount = 300;
			setting4.Range = "Up to";
			setting4.Amount = 200;

			AssertEquals(element.AuthorisationSettings.Count, 4);
			Assert(!element.AuthorisationSettings.ReadOnly);

			element.Type = GLJournalApprovalThreshold.TypeCodes.AnyChanges;

			AssertEquals(element.AuthorisationSettings.Count, 0);
			Assert(element.AuthorisationSettings.ReadOnly);
		}

		public void TestGLAccount()
		{
			GLJournalApprovalThreshold element = new GLJournalApprovalThreshold();
			var glAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery());
			element.GLAccount = glAccount.PK;
			AssertEquals("GLAccount", glAccount.PK, element.GLAccount);
			AssertEquals("Description", glAccount.AG_DescriptionMultilingual, element.Description);
		}

		public void TestReportSection()
		{
			GLJournalApprovalThreshold element = new GLJournalApprovalThreshold();
			element.ReportSection = AccGLHeader.Constants.SectionTypes.Codes.Overheads;
			AssertEquals("ReportSection", AccGLHeader.Constants.SectionTypes.Codes.Overheads, element.ReportSection);
			AssertEquals("Description", AccGLHeader.Constants.SectionTypes.Descriptions.Overheads, element.Description);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new GLJournalApprovalThreshold();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GLJournalApprovalThreshold();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
