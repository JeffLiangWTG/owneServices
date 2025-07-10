using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container
{
	[TestedType(typeof(DocLoadListPackLine))]
	sealed class DocLoadListPackLineTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocLoadListPackLine.New(LoadListPackLine, Factory) };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			PackLine packLine = Factory.New<PackLine>();
			CommonContainer container = Factory.New<CommonContainer>();
			packLine.JL_F3_NKPackType = Constants.PkgUnit.Bag;
			DocPackLines packLineWrapper = DocPackLines.New(packLine, Factory);
			DocContainer containerWrapper = DocContainer.New(container, Factory);
			return DocLoadListPackLine.New(new LoadListPackLine(packLineWrapper, containerWrapper), Factory);
		}

		public void TestWeightVolumeAndPacks()
		{
			PackLine.JL_PackageCount = 9;
			PackLine.JL_F3_NKPackType = "PLT";
			PackLine.JL_ActualVolume = 10M;
			PackLine.JL_ActualVolumeUQ = "L";
			PackLine.JL_ActualWeight = 30M;
			PackLine.JL_ActualWeightUQ = "T";

			PackLineWrapper = DocPackLines.New(PackLine, Factory);
			LoadListPackLine = new LoadListPackLine(PackLineWrapper, ContainerWrapper);
			DocLoadListPackLine loadListPackLineWrapper = DocLoadListPackLine.New(LoadListPackLine, Factory);
			AssertEquals("Using actual weight and volume", "30 T\n10 L\n9 Pallet", loadListPackLineWrapper.WeightVolumeAndPacks);

			var pack2 = Factory.New<PackLine>();
			pack2.JL_PackageCount = 1;
			pack2.JL_F3_NKPackType = "CNT";
			pack2.JL_ActualVolume = 1M;
			pack2.JL_ActualVolumeUQ = "M3";
			pack2.JL_ActualWeight = 3M;
			pack2.JL_ActualWeightUQ = "KG";

			DocPackLines lineToAdd = DocPackLines.New(pack2, Factory);
			LoadListPackLine.AmendExistingPackLine(lineToAdd);
			loadListPackLineWrapper = DocLoadListPackLine.New(LoadListPackLine, Factory);
			AssertEquals("Using converted & totalled weight and volume", "30.003 T\n1010 L\n10 Packages", loadListPackLineWrapper.WeightVolumeAndPacks);
		}

		public void TestPackLocations()
		{
			PackLine.PackLocations.RemoveAll();
			AssertEquals(0, PackLineWrapper.PackLocations.Count);

			PackLine.PackLocations.AddNew();
			PackLine.PackLocations.AddNew();
			AssertEquals(2, PackLineWrapper.PackLocations.Count);
		}

		public void TestPackingOrder()
		{
			PackLine = Factory.New<PackLine>();
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "111";
			container.JC_ContainerCount = 2;
			PackLine.JL_ContainerPackingOrder = 6;
			PackLine.JL_F3_NKPackType = Constants.PkgUnit.Bag;
			PackLineWrapper = DocPackLines.New(PackLine, Factory);
			ContainerWrapper = DocContainer.New(container, Factory);
			LoadListPackLine = new LoadListPackLine(PackLineWrapper, ContainerWrapper);
			DocLoadListPackLine loadListPackLineWrapper = DocLoadListPackLine.New(LoadListPackLine, Factory);

			AssertEquals("ContainerPackingOrder should be 6", 6, loadListPackLineWrapper.ContainerPackingOrder);
		}

		#region LoadList Document

		public void TestContainerCode()
		{
			PackLine = Factory.New<PackLine>();
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "111";
			container.JC_ContainerCount = 2;
			var containerType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			container.JC_RC = containerType.PK;
			PackLine.JL_F3_NKPackType = Constants.PkgUnit.Bag;
			PackLineWrapper = DocPackLines.New(PackLine, Factory);
			ContainerWrapper = DocContainer.New(container, Factory);
			LoadListPackLine = new LoadListPackLine(PackLineWrapper, ContainerWrapper);
			DocLoadListPackLine loadListPackLineWrapper = DocLoadListPackLine.New(LoadListPackLine, Factory);

			AssertEquals("ContainerCode should be 111", "111", loadListPackLineWrapper.ContainerCode);

			container.JC_ContainerNum = "";
			AssertEquals("ContainerCode should be 111", "20GP (2)", loadListPackLineWrapper.ContainerCode);
		}

		public void TestConsignorAndConsigneeForLoadList()
		{
			PackLineWrapper.ConsignorAndConsigneeForLoadList = "Consignor\nConsignee";
			AssertEquals("ConsignorAndConsignee should be ", "Consignor\nConsignee", PackLineWrapper.ConsignorAndConsigneeForLoadList);
		}

		public void TestCustomsBrokerForLoadList()
		{
			PackLineWrapper.CustomsBrokerForLoadList = "Customs Broker";
			AssertEquals("Customs broker should be ", "Customs Broker", PackLineWrapper.CustomsBrokerForLoadList);
		}

		#endregion

		PackLine PackLine;
		LoadListPackLine LoadListPackLine;
		DocPackLines PackLineWrapper;
		DocContainer ContainerWrapper;
		protected override void SetUp()
		{
			PackLine = Factory.New<PackLine>();
			CommonContainer container = Factory.New<CommonContainer>();
			PackLine.JL_F3_NKPackType = Constants.PkgUnit.Bag;
			PackLineWrapper = DocPackLines.New(PackLine, Factory);
			ContainerWrapper = DocContainer.New(container, Factory);
			LoadListPackLine = new LoadListPackLine(PackLineWrapper, ContainerWrapper);
			base.SetUp();
		}
	}
}
