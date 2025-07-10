using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing
{
	internal class FormatterForBillTypeLayoutTestClassForShipment : FormatterForBillTypeLayout
	{
		public FormatterForBillTypeLayoutTestClassForShipment()
			: base()
		{
			base.HaveCalculatedMarksAndNumbers = false;
			base.HaveCalculatedDescription = false;
			base.HaveCalculatedContainerColumns = false;
		}

		public ZInt TestMarksAndNumbersAndDescriptionRowHeight;
		public ZInt TestMarksAndNumbersWidth;
		public ZInt TestDescriptionWidth;
		public ZInt TestWeightWidth;
		public ZInt TestVolumeWidth;
		public ZInt TestContainerRowHeight;

		public override ZString PackageTypeDescription
		{
			get
			{
				var freightPkgUnitList = new RefPackTypeCollection(new BusinessObjectFactory());
				return freightPkgUnitList.GetDescriptionFromCode("PLT");
			}
		}

		public override ZInt PackCount
		{
			get { return 100; }
		}

		public override IDocSimpleContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					var factory = new BusinessObjectFactory();
					fContainers = new IDocSimpleContainerCollection(factory);

					var type20FR = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"));
					var type40OT = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40OT"));

					var consol = factory.New<ForwardingConsol>();
					var shipment = consol.Shipments.AddNew();

					var packLineForContainer1 = shipment.OuterPackLines.AddNew();
					packLineForContainer1.JL_PackageCount = 50;
					packLineForContainer1.JL_ActualVolume = 4M;

					var container1 = consol.Containers.AddNew();
					container1.JC_ContainerNum = "1";
					container1.JC_SealNum = "seal 1";
					container1.JC_RC = type20FR.PK;
					container1.JC_ContainerMode = "FCL";
					container1.JC_DeliveryMode = "CY/CY";
					packLineForContainer1.SetContainer(consol, container1);
					fContainers.Add(DocContainer.New(container1, shipment, factory));

					var packLineForContainer2 = shipment.OuterPackLines.AddNew();
					packLineForContainer2.JL_PackageCount = 1;
					packLineForContainer2.JL_ActualWeight = 23M;

					var container2 = consol.Containers.AddNew();
					container2.JC_ContainerNum = "2";
					container2.JC_RC = type20FR.PK;
					container2.JC_ContainerMode = "FCL";
					container2.JC_DeliveryMode = "CY/CY";
					packLineForContainer2.SetContainer(consol, container2);
					fContainers.Add(DocContainer.New(container2, shipment, factory));

					var packLineForContainer3 = shipment.OuterPackLines.AddNew();
					packLineForContainer3.JL_PackageCount = 1;
					packLineForContainer3.JL_ActualWeight = 45M;

					var container3 = consol.Containers.AddNew();
					container3.JC_ContainerNum = "3";
					container3.JC_SealNum = "seal 3";
					container3.JC_RC = type40OT.PK;
					container3.JC_ContainerMode = "FCL";
					packLineForContainer3.SetContainer(consol, container3);
					fContainers.Add(DocContainer.New(container3, shipment, factory));

					var packLineForContainer4 = shipment.OuterPackLines.AddNew();
					packLineForContainer4.JL_PackageCount = 1;
					packLineForContainer4.JL_ActualVolume = 34M;

					var container4 = consol.Containers.AddNew();
					container4.JC_ContainerNum = "4";
					container4.JC_RC = type40OT.PK;
					container4.JC_ContainerMode = "LCL";
					container4.JC_SealNum = "seal 4";
					container4.JC_DeliveryMode = "CY/CY";
					packLineForContainer4.SetContainer(consol, container4);
					fContainers.Add(DocContainer.New(container4, shipment, factory));
				}

				return fContainers;
			}
		}

		public override ZString UnformattedGoodsDescription
		{
			get { return "This is the goods description\nto test this formatter with"; }
		}

		public override ZString UnformattedMarksAndNumbers
		{
			get { return "This is the marks and numbers\nto test this formatter with"; }
		}

		public override ZString ContainerSectionHeading
		{
			get { return "CONTAINER        SEAL                     TYPE         WEIGHT(KG)      VOLUME(M3)           PACKAGE        MODE        "; }
		}

		public override ZBool ShowContainerAdditionalDetails
		{
			get { return ZBool.True; }
		}

		public override ZString UnformattedWeight
		{
			get { return "0.123 KG (123.000 G)"; }
		}

		public override ZString UnformattedVolume
		{
			get { return "3.483 M3 (123.000 CF)"; }
		}

		public override ZString DeliveryModeForContainer(IDocSimpleContainer container)
		{
			return ((DocContainer)container).DeliveryMode;
		}

		public override ZInt MarksAndNumbersAndDescriptionRowHeight
		{
			get { return TestMarksAndNumbersAndDescriptionRowHeight; }
		}

		public override ZInt MarksAndNumbersWidth
		{
			get { return TestMarksAndNumbersWidth; }
		}

		public override ZInt DescriptionWidth
		{
			get { return TestDescriptionWidth; }
		}

		public override ZInt WeightWidth
		{
			get { return TestWeightWidth; }
		}

		public override ZInt VolumeWidth
		{
			get { return TestVolumeWidth; }
		}

		public override ZInt ContainerRowHeight
		{
			get { return TestContainerRowHeight; }
		}

		protected IDocSimpleContainerCollection fContainers;
	}
}
