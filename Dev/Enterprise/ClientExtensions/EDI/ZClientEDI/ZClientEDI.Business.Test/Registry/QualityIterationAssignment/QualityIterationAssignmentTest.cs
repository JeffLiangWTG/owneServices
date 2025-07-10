using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(QualityIterationAssignment))]
	public class QualityIterationAssignmentTest : RegistryBusinessObjectTemplateTestCase<QualityIterationAssignment>
	{
		public void TestValidateReleaseGroup()
		{
			var assignment = NewPopulatedBusinessObject();
			var lookups = new QualityIterationAssignmentLookups(NewPopulatedBusinessObject());
			var releaseGroup = lookups.ReleaseGroupList.First();

			assignment.ReleaseGroup = releaseGroup.GG_Code;
			AssertNoErrors("A valid release group was used so there should be no validation errors.", assignment.ReleaseGroupInfo);

			assignment.ReleaseGroup = "BLAH BLAH";
			AssertHasError("An invalid release group was used so the relevant error should be present.", assignment.ReleaseGroupInfo, "Enter a valid selection.");

			AssertExceptionThrown<MaxLengthExceededException>(() =>
			{
				assignment.ReleaseGroup = "The Indiana Mole Women";
			});

			ErrorReporter.Clear();
		}

		public void TestValidateIsQiEnabled()
		{
			var assignment = NewPopulatedBusinessObject();
			const string message = "Either true or false is valid for IsQiEnabled so no validation error should be present.";

			assignment.IsQiEnabled = ZBool.True;
			AssertNoErrors(message, assignment.IsQiEnabledInfo);

			assignment.IsQiEnabled = ZBool.False;
			AssertNoErrors(message, assignment.IsQiEnabledInfo);
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

		protected override QualityIterationAssignment GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override QualityIterationAssignment GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		QualityIterationAssignment NewPopulatedBusinessObject()
		{
			return new QualityIterationAssignment(NewFallbackLevel(), Factory);
		}

		#endregion
	}
}
