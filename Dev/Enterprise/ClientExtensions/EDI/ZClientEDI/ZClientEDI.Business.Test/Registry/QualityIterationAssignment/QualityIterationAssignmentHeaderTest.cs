using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(QualityIterationAssignmentHeader))]
	public class QualityIterationAssignmentHeaderTest : RegistryBusinessObjectTemplateTestCase<QualityIterationAssignmentHeader>
	{
		public void TestValidateIsDefaultOptionSelected()
		{
			const string message = "Either true or false is valid for IsDefaultOptionSelected so no validation error should be present.";
			var header = NewPopulatedBusinessObject();

			header.IsDefaultOptionSelected = false;
			AssertNoErrors(message, header.IsDefaultOptionSelectedInfo);

			header.IsDefaultOptionSelected = true;
			AssertNoErrors(message, header.IsDefaultOptionSelectedInfo);
		}

		public void TestSaveWithInvalidAssignents_ShouldThrowException()
		{
			var header = NewPopulatedBusinessObject();
			var assignment = header.AddNewAssignment("ZUG", ZBool.True);

			AssertHasError(assignment.ReleaseGroupInfo, "Enter a valid selection.");
			AssertExceptionThrown<RegistryValidationException>(header.RunPreSaveValidation);
		}

		public void TestIsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled()
		{
			var header = new QualityIterationAssignmentHeader();

			header.IsDefaultOptionSelected = false;
			AssertEquals(false, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("ONE"));
			AssertEquals(false, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("PER"));
			AssertEquals(false, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("CNT"));
			AssertEquals(false, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled(string.Empty));

			header.IsDefaultOptionSelected = true;
			AssertEquals(true, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("ONE"));
			AssertEquals(true, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("PER"));
			AssertEquals(true, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("CNT"));
			AssertEquals(true, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled(string.Empty));

			header.AddNewAssignment("ONE", ZBool.True);
			header.AddNewAssignment("PER", ZBool.False);

			header.IsDefaultOptionSelected = false;
			AssertEquals(true, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("ONE"));
			AssertEquals(false, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("PER"));
			AssertEquals(false, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("CNT"));
			AssertEquals(false, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled(string.Empty));

			header.IsDefaultOptionSelected = true;
			AssertEquals(true, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("ONE"));
			AssertEquals(false, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("PER"));
			AssertEquals(true, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled("CNT"));
			AssertEquals(true, header.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled(string.Empty));
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override QualityIterationAssignmentHeader GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override QualityIterationAssignmentHeader GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		QualityIterationAssignmentHeader NewPopulatedBusinessObject()
		{
			return new QualityIterationAssignmentHeader(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
