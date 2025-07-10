using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(CusSeal))]
	sealed class CusSealTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBK_SealNumber()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(CusSeal), nameof(CusSeal.BK_SealNumber), false, x =>
			x.Caption == "Seal Number" && x.MediumCaption == "Seal Number" && x.ShortCaption == "Seal No.");
		}

		public void TestBK_UnloadingState()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(CusSeal), nameof(CusSeal.BK_UnloadingState), false, x => x.ListDataSourceMember == "Lookups.UnloadedStates");
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
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var seal1 = container.AdditionalSeals.AddNew();
			var seal2 = container.AdditionalSeals.AddNew();
			AssertEquals("Sequence when added.", (short)1, seal1.BK_SequenceNumber);
			AssertEquals("Sequence when added.", (short)2, seal2.BK_SequenceNumber);
			seal2.Delete();
			var seal3 = container.AdditionalSeals.AddNew();
			AssertEquals("Sequence when added.", (short)2, seal3.BK_SequenceNumber);
		}

		public void TestBK_SealType()
		{
			AssertEquals(1, Seal.BK_SealTypeInfo.MaxLength);
			AssertHasCustomAttribute<ListAttribute>(typeof(CusSeal), nameof(CusSeal.BK_SealType), false, x => x.ListDataSourceMember == "Lookups.SealTypeList");
		}

		public void TestValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var seal = container.AdditionalSeals.AddNew();
			AssertType<CusSealValidation>(seal.Validation);
		}

		public void TestLookups()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container = header.Containers.AddNew();
			var seal = container.AdditionalSeals.AddNew();
			AssertType<CusSealLookups>(seal.Lookups);
		}

		public void TestBK_SealingPartyType()
		{
			AssertEntity<CusSeal>()
			.HasProperty(p => p.BK_SealingPartyType)
			.WithCaption("Seal Party")
			.WithMediumCaption("Seal Party")
			.WithShortCaption("Seal Party")
			.WithList("Lookups.SealingPartyList");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => Seal;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => Seal;

		CusSeal Seal
		{
			get
			{
				if (fSeal == null)
				{
					var header = Factory.New<AsycudaManifestHeader>();
					var container = header.Containers.AddNew();
					var seals = new CusSealCollection(container);
					fSeal = seals.AddNew();
					fSeal.BK_SealNumber = "S1";
				}

				return fSeal;
			}
		}
		CusSeal fSeal;
	}
}
