using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusCNEntryInstruction))]
	class CusCNEntryInstructionTest : IAddInfoChildUniqueIndexFailureHandlerSupporterTestCase<CusCNEntryInstruction>
	{
		public void TestSupportsNotes()
		{
			Assert("AddInfo Leaf table does not support Notes", !Factory.New<CusCNEntryInstruction>().SupportsNotes);
		}

		public void TestICusCNEntryInstructionIsCorrectlySetup()
		{
			var data = (BusinessObject)Factory.New<Integration.Customs.CN.ICusCNEntryInstruction>();
			AssertType<CusCNEntryInstruction>(data);
			AssertType<CusCNEntryInstruction>(Factory.Load(data.TablePrefix, data.PK));
		}

		protected override void SetUpSystemLastEditOnParent(EnterpriseBusinessObject parent, ZString systemLastEditUser, ZDateTime systemLastEditTimeUtc)
		{
			var entryInstruction = (CusEntryInstruction)parent;
			entryInstruction.CEI_SystemLastEditUser = systemLastEditUser;
			entryInstruction.CEI_SystemLastEditTimeUtc = systemLastEditTimeUtc;
		}

		protected override string ExpectedUniqueIndexName => Enterprise.ZArchitecture.Schema.CusCNEntryInstructionSchema.Constants.Indexes.FK_UX__CNE_CEI;
		protected override EnterpriseBusinessObject GetParent(CusCNEntryInstruction bizObj) => bizObj.EntryInstruction;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			return instruction.AddInfoChild;
		}
	}

	[TestedType(typeof(CusCNEntryInstruction))]
	class CusCNEntryInstructionClusterKeyTest : ClusterKeyWorkerMandatoryTest
	{
		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			return instruction.AddInfoChild;
		}

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			return instruction;
		}
	}
}
