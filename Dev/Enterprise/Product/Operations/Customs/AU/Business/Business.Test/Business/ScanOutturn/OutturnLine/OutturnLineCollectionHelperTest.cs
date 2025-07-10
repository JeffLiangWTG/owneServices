using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class OutturnLineCollectionHelperTest : TestCaseWithFactory
	{
		public void TestDeepCopyOfOutturnLineCollection()
		{
			var helper = new OutturnLineCollectionHelperForTest(Factory);
			helper.ValidateFillImportMappingCollection();
			helper.ValidateAddCollectionProperties();
		}

		class OutturnLineCollectionHelperForTest : OutturnLineCollectionHelper
		{
			readonly BusinessObjectFactory factory;

			public OutturnLineCollectionHelperForTest(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public void ValidateFillImportMappingCollection()
			{
				var collection = new AirOutturnLineCollection(factory);
				var impl = new ImportCollectionInfoImpl(collection);
				AddCollectionProperties(impl);

				var wizard = new ImportWizard(impl, null, null);
				var importCollection = wizard.Mapping;
				FillImportMappingCollection(importCollection);
				var mappings = importCollection.Cast<ImportWizardMapping>().ToArray();

				AssertEquals(0, mappings.Where(m => m.Text == OutturnLine.Schema.ConsignmentRef).First().FileColumnIndexOrder.First());
				AssertEquals(1, mappings.Where(m => m.Text == OutturnLine.Schema.Status).First().FileColumnIndexOrder.First());
				AssertEquals(2, mappings.Where(m => m.Text == OutturnLine.Schema.ScannedDateTime).First().FileColumnIndexOrder.First());
				AssertEquals(3, mappings.Where(m => m.Text == OutturnLine.Schema.Count).First().FileColumnIndexOrder.First());
			}

			public void ValidateAddCollectionProperties()
			{
				var collection = new AirOutturnLineCollection(factory);
				var impl = new ImportCollectionInfoImpl(collection);
				AddCollectionProperties(impl);
				var properties = impl.Cast<ImportPropertyInfoImpl<OutturnLine>>().ToArray();
				AssertEquals(4, properties.Length);

				AssertEquals("ConsignmentRef", properties[0].HeaderText);
				AssertEquals("Status", properties[1].HeaderText);
				AssertEquals("ScannedDateTime", properties[2].HeaderText);
				AssertEquals("Count", properties[3].HeaderText);
			}
		}
	}
}
