using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Freight.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Client.MFI.DocWrappers.Testing
{
	[TestedType(typeof(DocMFIBillofLading))]
	public class DocMFIBillofLadingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsMFIBillOfLading()
		{
			AssertEquals("MFI Bill of Lading bill type", ZBool.False, Wrapper.IsMFIBillOfLading);
			Shipment.JS_HouseBillOfLadingType = "MNZ";
			ResetWrappers();
			AssertEquals("MFI Bill of Lading bill type", ZBool.True, Wrapper.IsMFIBillOfLading);
			Shipment.JS_HouseBillOfLadingType = "EAG";
			ResetWrappers();
			AssertEquals("MFI Bill of Lading bill type", ZBool.False, Wrapper.IsMFIBillOfLading);
		}

		public void TestGetBottomSectionLineAt()
		{
			CommonContainer freightContainer = Consol.Containers.AddNew();
			PackLine line = Shipment.OuterPackLines.AddNew();
			line.Containers.Add(freightContainer);
			DocForwardingShipment baseShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			DocBillOfLadingTestClass bOLWrapper = new DocBillOfLadingTestClass(baseShipmentWrapper, baseShipmentWrapper.PK);
			ZString expectedString = bOLWrapper.ContainerColumnsSection[0].ToString();
			Shipment.JS_HouseBillOfLadingType = "FIA";
			ResetWrappers();
			AssertEquals("GetBottomSectionLineAt", expectedString, Wrapper.ContainerColumnsSection[0].ToString());
			expectedString = Wrapper.AlignToWidth("", ShipmentWrapper.ContainerNumberWidth) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap) + Wrapper.AlignToWidth("-", ShipmentWrapper.ContainerSealWidth) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap) + Wrapper.AlignToWidth("-", ShipmentWrapper.ContainerTypeWidth) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap) + Wrapper.AlignToWidth(ShipmentWrapper.FormatNumber(0M, Env.Registry.WeightMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerWeightWidth) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap) + Wrapper.AlignToWidth(ShipmentWrapper.FormatNumber(0M, Env.Registry.VolumeMinimumDecimalPlacesToDisplay), ShipmentWrapper.ContainerVolumeWidth) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap) + Wrapper.AlignToWidth(new ZInt(0).ToString(), ShipmentWrapper.ContainerPackagesWidth) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap) + Wrapper.AlignToWidth("-", ShipmentWrapper.ContainerModeWidth);
			Shipment.JS_HouseBillOfLadingType = "MNZ";
			ResetWrappers();
			AssertEquals("GetBottomSectionLineAt", expectedString, Wrapper.ContainerColumnsSection[0].ToString());
		}

		public void TestContainersColumnHeaders()
		{
			DocForwardingShipment baseShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			ZString expectedString = baseShipmentWrapper.BillOfLading.ContainersColumnHeaders;
			Shipment.JS_HouseBillOfLadingType = "FIA";
			AssertEquals("ContainersColumnHeaders", expectedString, Wrapper.ContainersColumnHeaders);
			expectedString = "\nContainer" + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberWidth - "Container".Length) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerNumberAndSealGap) + "Seal" + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerSealWidth - "Seal".Length) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerSealAndTypeGap) + "Type" + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeWidth - "Type".Length) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerTypeAndWeightGap) + "Weight(KG)" + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightWidth - "Weight(KG)".Length) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerWeightAndVolumeGap) + "Volume(M3)" + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeWidth - "Volume(M3)".Length) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerVolumeAndPackagesGap) + "Packages" + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesWidth - "Packages".Length) + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerPackagesAndModeGap) + "Mode" + Wrapper.FillWithSpaces(ShipmentWrapper.ContainerModeWidth - "Mode".Length);
			Shipment.JS_HouseBillOfLadingType = "MNZ";
			ResetWrappers();
			AssertEquals("ContainersColumnHeaders", expectedString, Wrapper.ContainersColumnHeaders);
		}

		#region SetUp
		ForwardingShipment Shipment;
		DocForwardingShipment ShipmentWrapper;
		ForwardingConsol Consol;
		DocMFIBillofLadingTestClass Wrapper;
		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			Shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Consol = Shipment.Consols.AddNew();
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			Wrapper = new DocMFIBillofLadingTestClass(ShipmentWrapper);
			base.SetUp();
		}

		void ResetWrappers()
		{
			ShipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			Wrapper = new DocMFIBillofLadingTestClass(ShipmentWrapper);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocMFIBillofLading(ShipmentWrapper);
		}

		#endregion
		#region Inner Test Class
		public class DocMFIBillofLadingTestClass : DocMFIBillofLading
		{
			public DocMFIBillofLadingTestClass(DocForwardingShipment shipmentWrapper) : base(shipmentWrapper)
			{
			}

			public new ZBool IsMFIBillOfLading
			{
				get
				{
					return base.IsMFIBillOfLading;
				}
			}

			public new ZString FillWithSpaces(ZInt numberOfTimes)
			{
				return base.FillWithSpaces(numberOfTimes);
			}

			public new ZString AlignToWidth(ZString value, ZInt maxWidth)
			{
				return base.AlignToWidth(value, maxWidth);
			}

			public new DocBillofLadingContainerCollection Containers
			{
				get
				{
					return base.Containers;
				}
			}
		}
		#endregion
	}
}
