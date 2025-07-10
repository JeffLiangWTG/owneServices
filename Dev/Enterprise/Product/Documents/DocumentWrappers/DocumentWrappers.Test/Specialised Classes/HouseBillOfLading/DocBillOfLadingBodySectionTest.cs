using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocBillOfLadingBodySection))]
	sealed class DocBillOfLadingBodySectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsEmpty()
		{
			AssertEquals("BodySection: IsEmpty", ZBool.True, BodySection.IsEmpty);

			BodySection.MarksAndNumbers = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.Packages = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.GoodsDescription = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.Weight = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.Volume = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.ContainerNumbers = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.ContainerPackages = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.ContainerTypes = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.ContainerWeights = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.ContainerVolumes = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.ContainerPackages = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);

			ClearProperties();
			BodySection.ContainerModes = "blahblah";
			AssertEquals("BodySection: IsEmpty", ZBool.False, BodySection.IsEmpty);
		}

		public void ClearProperties()
		{
			BodySection.MarksAndNumbers = ZString.Empty;
			BodySection.Packages = ZString.Empty;
			BodySection.GoodsDescription = ZString.Empty;
			BodySection.Weight = ZString.Empty;
			BodySection.Volume = ZString.Empty;
			BodySection.ContainerNumbers = ZString.Empty;
			BodySection.ContainerSeals = ZString.Empty;
			BodySection.ContainerTypes = ZString.Empty;
			BodySection.ContainerWeights = ZString.Empty;
			BodySection.ContainerVolumes = ZString.Empty;
			BodySection.ContainerPackages = ZString.Empty;
			BodySection.ContainerModes = ZString.Empty;
		}

		#region Fields
		public void TestMarksAndNumbers()
		{
			AssertEquals("BodySection: Properties - MarksAndNumbers", ZString.Empty, BodySection.MarksAndNumbers);
			BodySection.MarksAndNumbers = "MarksAndNumbers";
			AssertEquals("BodySection: Properties - MarksAndNumbers", "MarksAndNumbers", BodySection.MarksAndNumbers);
		}
		public void TestPackages()
		{
			AssertEquals("BodySection: Properties - Packages", ZString.Empty, BodySection.Packages);
			BodySection.Packages = "Packages";
			AssertEquals("BodySection: Properties - Packages", "Packages", BodySection.Packages);
		}
		public void TestGoodsDescription()
		{
			AssertEquals("BodySection: Properties - GoodsDescription", ZString.Empty, BodySection.GoodsDescription);
			BodySection.GoodsDescription = "GoodsDescription";
			AssertEquals("BodySection: Properties - GoodsDescription", "GoodsDescription", BodySection.GoodsDescription);
		}
		public void TestWeight()
		{
			AssertEquals("BodySection: Properties - Weight", ZString.Empty, BodySection.Weight);
			BodySection.Weight = "Weight";
			AssertEquals("BodySection: Properties - Weight", "Weight", BodySection.Weight);
		}
		public void TestVolume()
		{
			AssertEquals("BodySection: Properties - Volume", ZString.Empty, BodySection.Volume);
			BodySection.Volume = "Volume";
			AssertEquals("BodySection: Properties - Volume", "Volume", BodySection.Volume);
		}
		public void TestContainerNumbers()
		{
			AssertEquals("BodySection: Properties - ContainerNumbers", ZString.Empty, BodySection.ContainerNumbers);
			BodySection.ContainerNumbers = "ContainerNumbers";
			AssertEquals("BodySection: Properties - ContainerNumbers", "ContainerNumbers", BodySection.ContainerNumbers);
		}
		public void TestContainerSeals()
		{
			AssertEquals("BodySection: Properties - ContainerSeals", ZString.Empty, BodySection.ContainerSeals);
			BodySection.ContainerSeals = "ContainerSeals";
			AssertEquals("BodySection: Properties - ContainerSeals", "ContainerSeals", BodySection.ContainerSeals);
		}
		public void TestContainerTypes()
		{
			AssertEquals("BodySection: Properties - ContainerTypes", ZString.Empty, BodySection.ContainerTypes);
			BodySection.ContainerTypes = "ContainerTypes";
			AssertEquals("BodySection: Properties - ContainerTypes", "ContainerTypes", BodySection.ContainerTypes);
		}
		public void TestContainerWeights()
		{
			AssertEquals("BodySection: Properties - ContainerWeights", ZString.Empty, BodySection.ContainerWeights);
			BodySection.ContainerWeights = "ContainerWeights";
			AssertEquals("BodySection: Properties - ContainerWeights", "ContainerWeights", BodySection.ContainerWeights);
		}
		public void TestContainerVolumes()
		{
			AssertEquals("BodySection: Properties - ContainerVolumes", ZString.Empty, BodySection.ContainerVolumes);
			BodySection.ContainerVolumes = "ContainerVolumes";
			AssertEquals("BodySection: Properties - ContainerVolumes", "ContainerVolumes", BodySection.ContainerVolumes);
		}
		public void TestContainerPackages()
		{
			AssertEquals("BodySection: Properties - ContainerPackages", ZString.Empty, BodySection.ContainerPackages);
			BodySection.ContainerPackages = "ContainerPackages";
			AssertEquals("BodySection: Properties - ContainerPackages", "ContainerPackages", BodySection.ContainerPackages);
		}
		public void TestContainerModes()
		{
			AssertEquals("BodySection: Properties - ContainerModes", ZString.Empty, BodySection.ContainerModes);
			BodySection.ContainerModes = "ContainerModes";
			AssertEquals("BodySection: Properties - ContainerModes", "ContainerModes", BodySection.ContainerModes);
		}
		#endregion

		#region Implementation

		DocBillOfLadingBodySection BodySection;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocBillOfLadingBodySection(Factory);
		}

		protected override void SetUp()
		{
			BodySection = new DocBillOfLadingBodySection(Factory);

			base.SetUp();
		}

		#endregion
	}
}
