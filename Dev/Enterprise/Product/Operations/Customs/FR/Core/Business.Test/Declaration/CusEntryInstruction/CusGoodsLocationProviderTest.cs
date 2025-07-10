using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	partial class CusEntryInstructionTest
	{
		public void TestGoodsLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusGoodsLocation = entryInstruction.GoodsLocation;
			CombineAssertions(() =>
			{
				AssertEquals("create with parent PK", entryInstruction.PK, cusGoodsLocation.CGL_ParentID);
				AssertEquals("create with parent table code", entryInstruction.TablePrefix, cusGoodsLocation.CGL_ParentTableCode);
			});
		}

		public void TestLoadGoodsLocation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusGoodsLocation = entryInstruction.GoodsLocation;
			cusGoodsLocation.CGL_Qualifier = "X";
			AssertEquals("X", entryInstruction.GoodsLocationDescription);
			Factory.Save();

			var type = typeof(EU.Business.Declaration.CusEntryInstruction);
			var goodsLocation = type.GetField("goodsLocation", BindingFlags.NonPublic | BindingFlags.Instance);
			goodsLocation.SetValue(entryInstruction, null);
			AssertEquals("X", entryInstruction.GoodsLocationDescription);
		}

		public void TestGoodsLocationDescription()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var cusGoodsLocation = entryInstruction.GoodsLocation;
			cusGoodsLocation.CGL_Qualifier = "X";
			AssertEquals("X", entryInstruction.GoodsLocationDescription);
		}

		public void TestGoodsLocationMutex()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var entryInstruction1 = factory1.Load<CusEntryInstruction>(entryInstruction.PK);
			var cusGoodsLocation = entryInstruction.GoodsLocation;

			var cusGoodsLocation1 = entryInstruction1.GoodsLocation;
			CombineAssertions(() =>
			{
				AssertNull("only one goods location per CusEntryInstruction", cusGoodsLocation1);
				Factory.Save();
				cusGoodsLocation1 = entryInstruction1.GoodsLocation;
				AssertEquals("load from entryInstruction after save (UnlockGoodsLocationManagementMutex released the mutex)", entryInstruction.PK, entryInstruction1.PK);
			});
			entryInstruction.DisposeMutex();
			entryInstruction1.DisposeMutex();
		}

		public void TestICusGoodsLocationProvider_ProviderKey()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("FRDECL", (entryInstruction as ICusGoodsLocationProvider).ProviderKey);
		}
	}
}
