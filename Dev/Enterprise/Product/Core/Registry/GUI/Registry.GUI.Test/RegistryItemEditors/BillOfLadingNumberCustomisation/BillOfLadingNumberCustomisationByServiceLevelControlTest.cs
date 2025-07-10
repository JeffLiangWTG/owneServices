using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BillOfLadingNumberCustomisationByServiceLevelControl))]
	sealed class BillOfLadingNumberCustomisationByServiceLevelControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		public void TestServiceLevelALL()
		{
			ServiceLevelWordingTest("Bill Of Lading House Number Customization For ALL Service Levels", "ALL", "House");
		}

		public void TestServiceLevelSTD()
		{
			ServiceLevelWordingTest("Bill Of Lading House Number Customization For Service Level STD", "STD", "House");
		}

		public void TestServiceLevel_Category_WarehouseOrder()
		{
			ServiceLevelWordingTest("Warehouse Order ID Customization For ALL Service Levels", "ALL", "Order", NumberCustomisationElementCategories.WarehouseOrder);
		}

		public void TestServiceLevel_Category_WarehouseReceive()
		{
			ServiceLevelWordingTest("Warehouse Receive ID Customization For ALL Service Levels", "ALL", "Receive", NumberCustomisationElementCategories.WarehouseReceive);
		}

		public void TestHideUseShipmentCheckBox_Domestic()
		{
			var dataType = new BillCustomisationByServiceLevelRegistryDataType();
			dataType.FountainPrefix = "XX";
			dataType.Categories = NumberCustomisationElementCategories.Default;

			using (var control = new BillOfLadingNumberCustomisationByServiceLevelControlTestClass(dataType))
			{
				AssertEquals("Default Visibility", true, control.TestCustomisationControl.useShipmentSequenceNumberCheckBox.Visible);
				AssertNotEquals("Default Location", control.TestCustomisationControl.useShipmentSequenceNumberCheckBox.Location, control.TestCustomisationControl.removeFountainPrefixCheckBox.Location);
			}

			dataType.Categories = NumberCustomisationElementCategories.Domestic | NumberCustomisationElementCategories.Consol | NumberCustomisationElementCategories.LinerAgency;
			using (var control = new BillOfLadingNumberCustomisationByServiceLevelControlTestClass(dataType))
			{
				AssertEquals("Domestic Visibility", false, control.TestCustomisationControl.useShipmentSequenceNumberCheckBox.Visible);
				AssertEquals("Domestic Location", control.TestCustomisationControl.useShipmentSequenceNumberCheckBox.Location, control.TestCustomisationControl.removeFountainPrefixCheckBox.Location);
			}
		}
		public void TestHideUseShipmentCheckBox_Warehouse()
		{
			var dataType = new BillCustomisationByServiceLevelRegistryDataType();
			dataType.FountainPrefix = "XX";
			dataType.Categories = NumberCustomisationElementCategories.Default;

			using (var control = new BillOfLadingNumberCustomisationByServiceLevelControlTestClass(dataType))
			{
				AssertEquals("Default Visibility", true, control.TestCustomisationControl.useShipmentSequenceNumberCheckBox.Visible);
				AssertNotEquals("Default Location", control.TestCustomisationControl.useShipmentSequenceNumberCheckBox.Location, control.TestCustomisationControl.removeFountainPrefixCheckBox.Location);
			}

			dataType.Categories = NumberCustomisationElementCategories.WarehouseJob;
			using (var control = new BillOfLadingNumberCustomisationByServiceLevelControlTestClass(dataType))
			{
				AssertEquals("Warehouse Visibility", false, control.TestCustomisationControl.useShipmentSequenceNumberCheckBox.Visible);
				AssertEquals("Warehouse Location", control.TestCustomisationControl.useShipmentSequenceNumberCheckBox.Location, control.TestCustomisationControl.removeFountainPrefixCheckBox.Location);
			}
		}

		public void TestHideAutoAllocateMasterBillNumberCheckBox()
		{
			var dataType = new BillCustomisationByServiceLevelRegistryDataType();
			dataType.FountainPrefix = "XX";
			dataType.Categories = NumberCustomisationElementCategories.Consol;

			using (var control = new BillOfLadingNumberCustomisationByServiceLevelControlTestClass(dataType))
			{
				AssertEquals("Auto Road Master Bill Number Visible", true, control.TestCustomisationControl.autoAllocateMasterBillNumbersToConsolsCheckBox.Visible);
			}

			dataType.Categories = NumberCustomisationElementCategories.FreightStandard | NumberCustomisationElementCategories.SupplierBooking;
			using (var control = new BillOfLadingNumberCustomisationByServiceLevelControlTestClass(dataType))
			{
				AssertEquals("Auto Road Master Bill Number Not Visible", false, control.TestCustomisationControl.autoAllocateMasterBillNumbersToConsolsCheckBox.Visible);
			}
		}

		public void ServiceLevelWordingTest(string expected, string serviceLevel, string generatedNumberName, NumberCustomisationElementCategories categories = NumberCustomisationElementCategories.Default)
		{
			var dataType = new BillCustomisationByServiceLevelRegistryDataType();
			dataType.GeneratedNumberName = (NoResString)generatedNumberName;
			dataType.Categories = categories;

			using (var control = new BillOfLadingNumberCustomisationByServiceLevelControl(dataType))
			{
				control.ServiceLevel = serviceLevel;
				AssertControlText(control, "HouseBillNumberByServiceLevelGroupBox", expected);
			}
		}

		#region Implementation

		void AssertControlText(BillOfLadingNumberCustomisationByServiceLevelControl control, string name, string expectedText)
		{
			AssertEquals(name, expectedText, GetControl(control, name).Text);
		}

		Control GetControl(BillOfLadingNumberCustomisationByServiceLevelControl control, string name)
		{
			return (Control)typeof(BillOfLadingNumberCustomisationByServiceLevelControl).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control);
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new BillOfLadingNumberCustomisationsByServiceLevel();
		}

		protected override RegistryZUserControl GetNewControl()
		{
			BillCustomisationByServiceLevelRegistryDataType dataType = new BillCustomisationByServiceLevelRegistryDataType();
			return new BillOfLadingNumberCustomisationByServiceLevelControl(dataType);
		}

		#endregion

		class BillOfLadingNumberCustomisationByServiceLevelControlTestClass : BillOfLadingNumberCustomisationByServiceLevelControl
		{
			public BillOfLadingNumberCustomisationByServiceLevelControlTestClass(BillCustomisationByServiceLevelRegistryDataType dataType)
				: base(dataType)
			{ }
			public BillOfLadingNumberCustomisationControl TestCustomisationControl
			{
				get { return CustomisationControl; }
			}
		}
	}
}
