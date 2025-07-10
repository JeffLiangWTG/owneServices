using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.StmNote;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.StmNote
{
	[TestedType(typeof(vw_StmNoteNonUniqueDescriptions))]
	class vw_StmNoteNonUniqueDescriptionsTest : DbCreateScriptTest
	{
		//The test will covered in Enterprise.ZArchitecture.Business.PredefinedNoteTypes.TestTypesWithUniqueDescriptionAreSynchronizedToViewInDB
	}
}

