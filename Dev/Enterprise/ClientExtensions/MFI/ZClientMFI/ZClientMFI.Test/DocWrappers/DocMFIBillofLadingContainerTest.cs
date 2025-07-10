using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentWrappers;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Client.MFI.DocWrappers.Testing
{
	[TestedType(typeof(DocMFIBillofLadingContainer))]
	public class DocMFIBillofLadingContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGrossWeight()
		{
			AssertEquals("Gross weight", 0M, HBLContainer.GrossWeight);
			FreightContainer.JC_GrossWeight = 13M;
			AssertEquals("Gross weight", 13M, HBLContainer.GrossWeight);
		}

		#region Implementation
		CommonContainer FreightContainer;
		DocContainer DocContainer;
		DocMFIBillofLadingContainer HBLContainer;
		protected override void SetUp()
		{
			FreightContainer = Factory.NewWithValidTestData<CommonContainer>();
			DocContainer = DocContainer.New(FreightContainer, Factory);
			HBLContainer = DocMFIBillofLadingContainer.New(DocContainer);
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			FreightContainer = Factory.NewWithValidTestData<CommonContainer>();
			DocContainer = DocContainer.New(FreightContainer, Factory);
			return DocMFIBillofLadingContainer.New(DocContainer);
		}
		#endregion
	}
}
