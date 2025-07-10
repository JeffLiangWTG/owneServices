using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(QualityIterationAssignmentRegistryDataType))]
	public class QualityIterationAssignmentRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<QualityIterationAssignmentRegistryDataType>
	{
		#region Implementation

		protected override QualityIterationAssignmentRegistryDataType GetNewDataType()
		{
			return new QualityIterationAssignmentRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "QualityIterationAssignmentRegistryEditor"; }
		}

		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			var lhsParent = (QualityIterationAssignmentHeader)lhs;
			var rhsParent = (QualityIterationAssignmentHeader)rhs;

			AssertEquals(lhsParent.IsDefaultOptionSelected, rhsParent.IsDefaultOptionSelected);
			AssertEquals(lhsParent.AssignmentCollection.Count, rhsParent.AssignmentCollection.Count);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var newReleaseGroupsCreated = false;

			var releaseGroup1 = factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, SQLComparisonOperator.Equal, "PMG"));
			if (releaseGroup1 == null)
			{
				releaseGroup1 = factory.NewWithValidTestData<GlbGroup>();
				releaseGroup1.GG_Code = "PMG";
				newReleaseGroupsCreated = true;
			}

			var releaseGroup2 = factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, SQLComparisonOperator.Equal, "ALL"));
			if (releaseGroup2 == null)
			{
				releaseGroup2 = factory.NewWithValidTestData<GlbGroup>();
				releaseGroup2.GG_Code = "ALL";
				newReleaseGroupsCreated = true;
			}

			if (newReleaseGroupsCreated)
			{
				factory.Save();
			}

			var header = new QualityIterationAssignmentHeader { IsDefaultOptionSelected = true };
			header.AddNewAssignment(releaseGroup1.GG_Code, ZBool.False);
			header.AddNewAssignment(releaseGroup2.GG_Code, ZBool.True);

			byte[] byteArrayValue = {
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,0,45,0,49,0,54,0,34,
				0,63,0,62,0,60,0,81,0,117,0,97,0,108,0,105,0,116,0,121,0,73,0,116,0,101,0,114,0,97,0,116,0,105,0,111,0,110,0,65,0,115,0,115,0,105,0,103,0,110,0,109,0,101,0,110,0,116,0,72,0,101,0,97,0,100,0,101,0,114,0,
				62,0,60,0,73,0,115,0,68,0,101,0,102,0,97,0,117,0,108,0,116,0,79,0,112,0,116,0,105,0,111,0,110,0,83,0,101,0,108,0,101,0,99,0,116,0,101,0,100,0,62,0,116,0,114,0,117,0,101,0,60,0,47,0,73,0,115,0,68,0,101,0,
				102,0,97,0,117,0,108,0,116,0,79,0,112,0,116,0,105,0,111,0,110,0,83,0,101,0,108,0,101,0,99,0,116,0,101,0,100,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,81,0,117,0,97,0,108,0,105,0,116,0,121,0,73,
				0,116,0,101,0,114,0,97,0,116,0,105,0,111,0,110,0,65,0,115,0,115,0,105,0,103,0,110,0,109,0,101,0,110,0,116,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,
				0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,
				32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,
				0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,0,81,0,117,0,97,0,108,0,105,0,116,0,121,0,73,0,116,0,101,0,114,0,97,0,116,0,105,0,111,0,110,0,65,0,115,0,115,0,105,0,103,0,110,0,109,0,101,0,110,0,116,
				0,62,0,60,0,82,0,101,0,108,0,101,0,97,0,115,0,101,0,71,0,114,0,111,0,117,0,112,0,62,0,80,0,77,0,71,0,60,0,47,0,82,0,101,0,108,0,101,0,97,0,115,0,101,0,71,0,114,0,111,0,117,0,112,0,62,0,60,0,73,0,115,0,81,
				0,105,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,102,0,97,0,108,0,115,0,101,0,60,0,47,0,73,0,115,0,81,0,105,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,60,0,47,0,81,0,117,0,97,0,108,0,105,0,116,0,121,
				0,73,0,116,0,101,0,114,0,97,0,116,0,105,0,111,0,110,0,65,0,115,0,115,0,105,0,103,0,110,0,109,0,101,0,110,0,116,0,62,0,60,0,81,0,117,0,97,0,108,0,105,0,116,0,121,0,73,0,116,0,101,0,114,0,97,0,116,0,105,0,
				111,0,110,0,65,0,115,0,115,0,105,0,103,0,110,0,109,0,101,0,110,0,116,0,62,0,60,0,82,0,101,0,108,0,101,0,97,0,115,0,101,0,71,0,114,0,111,0,117,0,112,0,62,0,65,0,76,0,76,0,60,0,47,0,82,0,101,0,108,0,101,0,
				97,0,115,0,101,0,71,0,114,0,111,0,117,0,112,0,62,0,60,0,73,0,115,0,81,0,105,0,69,0,110,0,97,0,98,0,108,0,101,0,100,0,62,0,116,0,114,0,117,0,101,0,60,0,47,0,73,0,115,0,81,0,105,0,69,0,110,0,97,0,98,0,108,
				0,101,0,100,0,62,0,60,0,47,0,81,0,117,0,97,0,108,0,105,0,116,0,121,0,73,0,116,0,101,0,114,0,97,0,116,0,105,0,111,0,110,0,65,0,115,0,115,0,105,0,103,0,110,0,109,0,101,0,110,0,116,0,62,0,60,0,47,0,65,0,114,
				0,114,0,97,0,121,0,79,0,102,0,81,0,117,0,97,0,108,0,105,0,116,0,121,0,73,0,116,0,101,0,114,0,97,0,116,0,105,0,111,0,110,0,65,0,115,0,115,0,105,0,103,0,110,0,109,0,101,0,110,0,116,0,62,0,60,0,47,0,81,0,117,
				0,97,0,108,0,105,0,116,0,121,0,73,0,116,0,101,0,114,0,97,0,116,0,105,0,111,0,110,0,65,0,115,0,115,0,105,0,103,0,110,0,109,0,101,0,110,0,116,0,72,0,101,0,97,0,100,0,101,0,114,0,62,0
			};

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(header, byteArrayValue)
			};
		}

		#endregion
	}
}
