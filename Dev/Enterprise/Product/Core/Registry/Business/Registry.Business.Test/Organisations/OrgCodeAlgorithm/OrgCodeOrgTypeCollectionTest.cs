using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgCodeOrgTypeCollection))]
	sealed class OrgCodeOrgTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<OrgCodeOrgTypeCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		public static void AssertElements(OrgCodeOrgTypeCollection collection)
		{
			AssertEquals("Count", 12, collection.Count);
			AssertEquals("[0].Description", "Receivables", collection[0].Description);
			AssertEquals("[1].Description", "Payables", collection[1].Description);
			AssertEquals("[2].Description", "Consignor", collection[2].Description);
			AssertEquals("[3].Description", "Consignee", collection[3].Description);
			AssertEquals("[4].Description", "Transport Client", collection[4].Description);
			AssertEquals("[5].Description", "Warehouse", collection[5].Description);
			AssertEquals("[6].Description", "Carrier", collection[6].Description);
			AssertEquals("[7].Description", "Forwarder", collection[7].Description);
			AssertEquals("[8].Description", "Broker", collection[8].Description);
			AssertEquals("[9].Description", "Services", collection[9].Description);
			AssertEquals("[10].Description", "Competitor", collection[10].Description);
			AssertEquals("[11].Description", "Sales", collection[11].Description);
		}

		void AssertElementsSelectedInfoReadOnly(OrgCodeOrgTypeCollection collection, bool readOnly)
		{
			foreach (OrgCodeOrgType element in collection)
			{
				AssertEquals("SelectedInfo.ReadOnly", readOnly, element.SelectedInfo.ReadOnly);
			}
		}

		protected override OrgCodeOrgTypeCollection GetCollectionToTest()
		{
			return new OrgCodeOrgTypeCollection(new OrgCodeAlgorithm());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgCodeOrgType();
		}

		public void TestAllowNew()
		{
			AssertEquals("AllowNew", false, Collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals("AllowRemove", false, Collection.AllowRemove);
		}

		public void TestCopyElementValuesFrom()
		{
			OrgCodeOrgTypeCollection sourceCollection = new OrgCodeOrgTypeCollection();
			sourceCollection.Load();

			Collection.Parent.AlgorithmType = OrgCodeAlgorithmType.Override;
			Collection.Load();

			sourceCollection[0].Selected = true;
			sourceCollection[2].Selected = true;

			Collection.CopyElementValuesFrom(sourceCollection);
			AssertEquals("[0].Selected", true, Collection[0].Selected);
			AssertEquals("[1].Selected", false, Collection[1].Selected);
			AssertEquals("[2].Selected", true, Collection[2].Selected);
			AssertEquals("[3].Selected", false, Collection[3].Selected);
			AssertEquals("[4].Selected", false, Collection[4].Selected);
			AssertEquals("[5].Selected", false, Collection[5].Selected);
			AssertEquals("[6].Selected", false, Collection[6].Selected);
			AssertEquals("[7].Selected", false, Collection[7].Selected);
			AssertEquals("[8].Selected", false, Collection[8].Selected);
			AssertEquals("[9].Selected", false, Collection[9].Selected);

			Collection.Parent.AlgorithmType = OrgCodeAlgorithmType.Default;
			sourceCollection[0].Selected = false;
			sourceCollection[2].Selected = false;

			Collection.CopyElementValuesFrom(sourceCollection);
			AssertEquals("[0].Selected", true, Collection[0].Selected);
			AssertEquals("[1].Selected", false, Collection[1].Selected);
			AssertEquals("[2].Selected", true, Collection[2].Selected);
			AssertEquals("[3].Selected", false, Collection[3].Selected);
			AssertEquals("[4].Selected", false, Collection[4].Selected);
			AssertEquals("[5].Selected", false, Collection[5].Selected);
			AssertEquals("[6].Selected", false, Collection[6].Selected);
			AssertEquals("[7].Selected", false, Collection[7].Selected);
			AssertEquals("[8].Selected", false, Collection[8].Selected);
			AssertEquals("[9].Selected", false, Collection[9].Selected);
		}

		public void TestDefaultAlgorithm()
		{
			AssertEquals("DefaultAlgorithm", false, Collection.DefaultAlgorithm);
			Collection.Parent.AlgorithmType = OrgCodeAlgorithmType.Default;
			AssertEquals("DefaultAlgorithm", true, Collection.DefaultAlgorithm);
		}

		public void TestIndexer()
		{
			Collection.Load();
			AssertNull("[\"Boo\"]", Collection["Boo"]);
			AssertEquals("[\"Forwarder\"]", Collection[7], Collection["Forwarder"]);
		}

		public void TestLoad()
		{
			OrgCodeAlgorithm algorithm = new OrgCodeAlgorithm();
			algorithm.AlgorithmType = OrgCodeAlgorithmType.Override;
			algorithm.SelectableOrgTypes[0].Selected = true;
			algorithm.SelectableOrgTypes[2].Selected = true;
			algorithm.SelectableOrgTypes[6].Selected = true;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Order = 1;
			algorithm.Elements[OrgCodeElementDescription.GloballyUniqueNumber].Length = 1;

			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmOverride.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, algorithm);

			Collection.Load();
			AssertElements(Collection);
			AssertElementsSelectedInfoReadOnly(Collection, false);
			foreach (OrgCodeOrgType element in Collection)
			{
				AssertEquals("Selected", false, element.Selected);
			}

			Collection.Parent.AlgorithmType = OrgCodeAlgorithmType.Default;
			Collection.Load();
			AssertElements(Collection);
			AssertElementsSelectedInfoReadOnly(Collection, true);
			AssertEquals("[0].Selected", false, Collection[0].Selected);
			AssertEquals("[1].Selected", true, Collection[1].Selected);
			AssertEquals("[2].Selected", false, Collection[2].Selected);
			AssertEquals("[3].Selected", true, Collection[3].Selected);
			AssertEquals("[4].Selected", true, Collection[4].Selected);
			AssertEquals("[5].Selected", true, Collection[5].Selected);
			AssertEquals("[6].Selected", false, Collection[6].Selected);
			AssertEquals("[7].Selected", true, Collection[7].Selected);
			AssertEquals("[8].Selected", true, Collection[8].Selected);
			AssertEquals("[9].Selected", true, Collection[9].Selected);
		}

		public void TestSupportsSorting()
		{
			AssertEquals("SupportsSorting", false, ((IBindingList)Collection).SupportsSorting);
		}
	}
}
