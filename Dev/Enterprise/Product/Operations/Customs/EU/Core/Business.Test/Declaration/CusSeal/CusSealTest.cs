using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CusSeal))]
	class CusSealTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBK_SealNumber()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusSeal), nameof(CusSeal.BK_SealNumber), false, x => x.Caption == "Seal Number");
		}

		public void TestISequenceNumberLineMembers()
		{
			Assert("Interface should be implemented on ", typeof(ISequenceNumberLine<ZShort>).IsAssignableFrom(typeof(CusSeal)));
			var interfaceInstance = Seal as ISequenceNumberLine<ZShort>;

			Seal.BK_SequenceNumber = 9;
			AssertEquals("SequenceNumber should return BK_SequenceNumber.", (short)9, interfaceInstance.SequenceNumber);
			interfaceInstance.SequenceNumber = 8;
			AssertEquals("SequenceNumber should set BK_SequenceNumber.", (short)8, Seal.BK_SequenceNumber);

			AssertEquals("FKToHeader should return BK_ParentID.", Seal.BK_ParentID, interfaceInstance.FKToHeader);
		}

		public void TestSequence()
		{
			var declaration = Factory.New<JobDeclaration>();
			var equipment = declaration.Equipments.AddNew();
			var seal1 = equipment.Seals.AddNew();
			var seal2 = equipment.Seals.AddNew();
			AssertEquals("Sequence when added.", (short)1, seal1.BK_SequenceNumber);
			AssertEquals("Sequence when added.", (short)2, seal2.BK_SequenceNumber);
			seal2.Delete();
			var seal3 = equipment.Seals.AddNew();
			AssertEquals("Sequence when added.", (short)2, seal3.BK_SequenceNumber);
		}

		public void TestSupportsClone()
		{
			AssertEquals("SupportsClone", true, GetNewBusinessObject().SupportsClone());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Seal;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Seal;

		CusSeal Seal
		{
			get
			{
				if (fSeal == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					var equipment = declaration.Equipments.AddNew();
					equipment.CEQ_IdentificationNumber = "E1";
					var seals = new CusSealCollection(equipment);
					fSeal = seals.AddNew();
					fSeal.BK_SealNumber = "S1";
				}

				return fSeal;
			}
		}
		CusSeal fSeal;
	}
}
