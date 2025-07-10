using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ElectronicProcessingChargeDescriptionOverride))]
	public class ElectronicProcessingChargeDescriptionOverrideTest : RegistryBusinessObjectTemplateTestCase<ElectronicProcessingChargeDescriptionOverride>
	{
		public void TestDescriptionOverrideMaxLength()
		{
			AssertEquals(30, BizObj.TextInfo.MaxLength);
		}

		public void TestValidateTransport()
		{
			BizObj.Transport = ZString.Empty;
			AssertEquals("Precondition", ZString.Empty, BizObj.Transport);
			AssertHasError(BizObj.TransportInfo, "Please enter a Transport.");

			BizObj.Transport = "XXX";
			Assert("Precondition", !BizObj.Lookups.Transports.ContainsCode(BizObj.Transport));
			AssertHasError(BizObj.TransportInfo, "Enter a valid Transport.");

			BizObj.Transport = "All";
			Assert("Precondition", BizObj.Lookups.Transports.ContainsCode(BizObj.Transport));
			AssertNoErrors(BizObj.TransportInfo);

			BizObj.Container = "All";
			BizObj.ShipmentType = "All";
			BizObj.Origin = "SCC";
			BizObj.Destination = "SCC";
			AssertValidateDubplicated(BizObj.TransportInfo);
		}

		public void TestValidateContainer()
		{
			BizObj.Container = ZString.Empty;
			AssertEquals("Precondition", ZString.Empty, BizObj.Container);
			AssertHasError(BizObj.ContainerInfo, "Please enter a Container.");

			BizObj.Container = "XXX";
			Assert("Precondition", !BizObj.Lookups.Containers.ContainsCode(BizObj.Container));
			AssertHasError(BizObj.ContainerInfo, "Enter a valid Container.");

			BizObj.Container = "All";
			Assert("Precondition", BizObj.Lookups.Containers.ContainsCode(BizObj.Container));
			AssertNoErrors(BizObj.ContainerInfo);

			BizObj.Transport = "All";
			BizObj.ShipmentType = "All";
			BizObj.Origin = "SCC";
			BizObj.Destination = "SCC";
			AssertValidateDubplicated(BizObj.ContainerInfo);
		}

		public void TestValidateShipmentType()
		{
			BizObj.ShipmentType = ZString.Empty;
			AssertEquals("Precondition", ZString.Empty, BizObj.ShipmentType);
			AssertHasError(BizObj.ShipmentTypeInfo, "Please enter a Shipment Type.");

			BizObj.ShipmentType = "XXX";
			Assert("Precondition", !BizObj.Lookups.ShipmentTypes.ContainsCode(BizObj.ShipmentType));
			AssertHasError(BizObj.ShipmentTypeInfo, "Enter a valid Shipment Type.");

			BizObj.ShipmentType = "All";
			Assert("Precondition", BizObj.Lookups.ShipmentTypes.ContainsCode(BizObj.ShipmentType));
			AssertNoErrors(BizObj.ShipmentTypeInfo);

			BizObj.Transport = "All";
			BizObj.Container = "All";
			BizObj.Origin = "SCC";
			BizObj.Destination = "SCC";
			AssertValidateDubplicated(BizObj.ShipmentTypeInfo);
		}

		public void TestValidateOrigin()
		{
			BizObj.Origin = ZString.Empty;
			AssertEquals("Precondition", ZString.Empty, BizObj.Origin);
			AssertHasError(BizObj.OriginInfo, "Please enter an Origin.");

			BizObj.Origin = "XXX";
			Assert("Precondition", !BizObj.Lookups.Origins.ContainsCode(BizObj.Origin));
			AssertHasError(BizObj.OriginInfo, "Enter a valid Origin.");

			BizObj.Origin = "SCC";
			Assert("Precondition", BizObj.Lookups.Origins.ContainsCode(BizObj.Origin));
			AssertNoErrors(BizObj.OriginInfo);

			BizObj.Destination = "SUJ";
			BizObj.Origin = "SCC";
			Assert("Precondition", AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList.ContainsCode(BizObj.Origin));
			Assert("Precondition", AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareJobBranchList.ContainsCode(BizObj.Destination));
			AssertHasError(BizObj.OriginInfo, "Both the Origin and Destination Rule Option must be set based on current login company or job header branch value.");

			BizObj.Destination = "NSC";
			BizObj.Origin = "SCC";
			Assert("Precondition", AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList.ContainsCode(BizObj.Origin));
			Assert("Precondition", AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList.ContainsCode(BizObj.Destination));
			AssertNoErrors(BizObj.OriginInfo);

			BizObj.Transport = "All";
			BizObj.Container = "All";
			BizObj.ShipmentType = "All";
			BizObj.Destination = "SCC";
			AssertValidateDubplicated(BizObj.OriginInfo);
		}

		public void TestValidateDestination()
		{
			BizObj.Destination = ZString.Empty;
			AssertEquals("Precondition", ZString.Empty, BizObj.Destination);
			AssertHasError(BizObj.DestinationInfo, "Please enter a Destination.");

			BizObj.Destination = "XXX";
			Assert("Precondition", !BizObj.Lookups.Destinations.ContainsCode(BizObj.Destination));
			AssertHasError(BizObj.DestinationInfo, "Enter a valid Destination.");

			BizObj.Destination = "SCC";
			Assert("Precondition", BizObj.Lookups.Destinations.ContainsCode(BizObj.Destination));
			AssertNoErrors(BizObj.DestinationInfo);

			BizObj.Origin = "SUJ";
			BizObj.Destination = "SCC";
			Assert("Precondition", AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList.ContainsCode(BizObj.Destination));
			Assert("Precondition", AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareJobBranchList.ContainsCode(BizObj.Origin));
			AssertHasError(BizObj.DestinationInfo, "Both the Origin and Destination Rule Option must be set based on current login company or job header branch value.");

			BizObj.Origin = "NSC";
			BizObj.Destination = "SCC";
			Assert("Precondition", AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList.ContainsCode(BizObj.Destination));
			Assert("Precondition", AccountingConstants.ElectronicProcessingChargeDescriptionOverrideOriginDestinationCompareCurrentCompanyList.ContainsCode(BizObj.Origin));
			AssertNoErrors(BizObj.DestinationInfo);

			BizObj.Transport = "All";
			BizObj.Container = "All";
			BizObj.ShipmentType = "All";
			BizObj.Origin = "SCC";
			AssertValidateDubplicated(BizObj.DestinationInfo);
		}

		public void TestValidatePrefixSuffix()
		{
			BizObj.PrefixSuffix = ZString.Empty;
			AssertEquals("Precondition", ZString.Empty, BizObj.PrefixSuffix);
			AssertHasError(BizObj.PrefixSuffixInfo, "Please enter a Prefix/Suffix.");

			BizObj.PrefixSuffix = "XXX";
			Assert("Precondition", !BizObj.Lookups.PrefixSuffix.ContainsCode(BizObj.PrefixSuffix));
			AssertHasError(BizObj.PrefixSuffixInfo, "Enter a valid Prefix/Suffix.");

			BizObj.PrefixSuffix = "PRE";
			Assert("Precondition", BizObj.Lookups.PrefixSuffix.ContainsCode(BizObj.PrefixSuffix));
			AssertNoErrors(BizObj.PrefixSuffixInfo);
		}

		public void TestValidateText()
		{
			BizObj.Text = ZString.Empty;
			AssertEquals("Precondition", ZString.Empty, BizObj.Text);
			AssertHasError(BizObj.TextInfo, "Please enter a Text.");

			BizObj.Text = "XXX";
			AssertNoErrors(BizObj.TextInfo);
		}

		public void AssertValidateDubplicated(ZPropertyInfo propInfo)
		{
			var currentItem = (ElectronicProcessingChargeDescriptionOverride)propInfo.BizObj;
			var duplicatedItem = currentItem.ParentCollection.AddNew();
			duplicatedItem.Transport = currentItem.Transport;
			duplicatedItem.Container = currentItem.Container;
			duplicatedItem.ShipmentType = currentItem.ShipmentType;
			duplicatedItem.Origin = currentItem.Origin;
			duplicatedItem.Destination = currentItem.Destination;

			currentItem.ClearRowNotifications();
			var prop = typeof(ElectronicProcessingChargeDescriptionOverride).GetProperty(propInfo.Name);
			prop.SetValue(currentItem, propInfo.Value);

			AssertEquals(1, currentItem.RowErrors.Count());
			AssertHasRowError(currentItem, "This row has been duplicated and must be unique.");
		}

		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override ElectronicProcessingChargeDescriptionOverride GetBusinessObjectToClone()
		{
			return (ElectronicProcessingChargeDescriptionOverride)GetNewBusinessObject();
		}

		protected override ElectronicProcessingChargeDescriptionOverride GetBusinessObjectToSerialise()
		{
			return (ElectronicProcessingChargeDescriptionOverride)GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return RegistryValue.AddNew();
		}

		#endregion

		ElectronicProcessingChargeDescriptionOverrideCollection RegistryValue
		{
			get
			{
				var result = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDescriptionOverride.Value;
				result.CurrentFallbackLevel = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				return result;
			}
		}
	}
}
