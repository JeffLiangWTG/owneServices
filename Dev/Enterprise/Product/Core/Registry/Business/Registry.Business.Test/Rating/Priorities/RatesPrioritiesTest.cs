using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RatesPriorities))]
	sealed class RatesPrioritiesTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultPropertyValues()
		{
			var ratePriority = new RatesPriorities();

			AssertEquals("default value for OrganizationType", ZString.Empty, ratePriority.OrganizationType);
			AssertEquals("default value for UseCompanyTariff", ZBool.True, ratePriority.UseCompanyTariff);
			AssertEquals("default value for JobType", "ALL", ratePriority.JobType);
		}

		public void TestUniqueRowValidation()
		{
			var collection = new RatesPrioritiesCollection();
			var lcPriorityAll = collection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL);
			AssertNoErrors("Row is unique", lcPriorityAll);

			var testPriority = collection.AddNew(RatingDebtorOrgTypes.LC, RatingJobTypes.ALL);
			AssertHasError("Row is unique", testPriority.JobTypeInfo, "Please enter unique values only.");
			AssertHasError("Row is unique", testPriority.OrganizationTypeInfo, "Please enter unique values only.");

			CombineAssertions("[0] OrgType changes, error in orgtype, not job type. Then preseave validation causes jobtype to error too", () =>
			{
				testPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNR);
				testPriority.RunPreSaveValidation();
				AssertNoErrors("OrgType changed, it is cleared of errors", testPriority.OrganizationTypeInfo);
				AssertNoErrors("JobType cleared of errors", testPriority.JobTypeInfo);
			});

			CombineAssertions("[1] OrgType changes, causing no more duplicate, OrgType error cleared. Job type still has error until presavevalidation", () =>
			{
				testPriority.OrganizationType = nameof(RatingDebtorOrgTypes.LC);
				testPriority.RunPreSaveValidation();
				AssertHasError("OrgType changed. It gets validation error", testPriority.OrganizationTypeInfo, "Please enter unique values only.");
				AssertHasError("JobType now has error too", testPriority.JobTypeInfo, "Please enter unique values only.");
			});

			CombineAssertions("JobType changes, it is cleared of error. OrgType still has error until presave validation", () =>
			{
				testPriority.JobType = nameof(RatingJobTypes.CUS);
				testPriority.RunPreSaveValidation();
				AssertNoErrors("JobType changes, no more error", testPriority.JobTypeInfo);
				AssertNoErrors("OrgType no more error", testPriority.OrganizationTypeInfo);
			});

			CombineAssertions("OrgType changes, still unique", () =>
			{
				testPriority.OrganizationType = nameof(RatingDebtorOrgTypes.CNR);
				testPriority.RunPreSaveValidation();
				AssertNoErrors("OrgType changes, remains unique", testPriority);
			});
		}

		public void TestOrganizationType()
		{
			var ratePriority = new RatesPriorities();

			ratePriority.OrganizationType = nameof(RatingDebtorOrgTypes.LC);
			AssertNoErrors("LC is valid", ratePriority.OrganizationTypeInfo);

			ratePriority.OrganizationType = ZString.Empty;
			AssertHasError("Blank not allowed", ratePriority.OrganizationTypeInfo, "Please enter a value.");

			ratePriority.OrganizationType = "Cat";
			AssertHasError("Cat not allowed", ratePriority.OrganizationTypeInfo, "Enter a valid selection.");

			AssertExceptionThrown<MaxLengthExceededException>("Max length exceeded",
				() => ratePriority.OrganizationType = "Potato");
			ErrorReporter.Clear();
		}

		public void TestJobType()
		{
			var ratePriority = new RatesPriorities();

			ratePriority.JobType = nameof(RatingJobTypes.ALL);
			AssertNoErrors("Valid JobType 'ALL'", ratePriority.JobTypeInfo);

			ratePriority.JobType = nameof(RatingJobTypes.CUS);
			AssertNoErrors("Valid JobType 'CUST'", ratePriority.JobTypeInfo);

			ratePriority.JobType = "XXX";
			AssertHasError("Invalid JobType 'XXX'", ratePriority.JobTypeInfo, "Enter a valid selection.");

			ratePriority.JobType = "";
			AssertHasError("Empty JobType", ratePriority.JobTypeInfo, "Please enter a value.");

			AssertExceptionThrown<MaxLengthExceededException>("Max length exceeded",
				() => ratePriority.JobType = "XXXX");
			ErrorReporter.Clear();
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new RatesPriorities();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion
	}
}
