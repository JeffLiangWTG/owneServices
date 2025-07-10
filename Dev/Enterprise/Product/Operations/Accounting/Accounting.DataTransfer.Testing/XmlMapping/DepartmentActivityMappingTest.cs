using System;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.XmlMapping.Testing
{
	[TestedType(typeof(DepartmentActivityXmlMapping))]
	public class DepartmentActivityMappingTest : EnterpriseCodeExternalCodeMappingsTest
	{
		public void TestMapping()
		{
			NotificationBuffer notify = new NotificationBuffer();
			DepartmentActivityXmlMapping instance = DepartmentActivityXmlMapping.Instance;
			AssertEquals(Xsd.DepartmentActivity.Customs, instance.GetExternalCode(Constants.DepartmentActivityTypes.Customs, String.Empty, notify));
			AssertEquals(Xsd.DepartmentActivity.DepotCFS, instance.GetExternalCode(Constants.DepartmentActivityTypes.DepotCFS, String.Empty, notify));
			AssertEquals(Xsd.DepartmentActivity.Forwarding, instance.GetExternalCode(Constants.DepartmentActivityTypes.Forwarding, String.Empty, notify));
			AssertEquals(Xsd.DepartmentActivity.Linehaul, instance.GetExternalCode(Constants.DepartmentActivityTypes.Linehaul, String.Empty, notify));
			AssertEquals(Xsd.DepartmentActivity.Cartage, instance.GetExternalCode(Constants.DepartmentActivityTypes.Cartage, String.Empty, notify));
			AssertEquals(Xsd.DepartmentActivity.Warehouse, instance.GetExternalCode(Constants.DepartmentActivityTypes.Warehouse, String.Empty, notify));
			AssertEquals(Xsd.DepartmentActivity.Miscellaneous, instance.GetExternalCode(Constants.DepartmentActivityTypes.Miscellaneous, String.Empty, notify));
			AssertEquals(Xsd.DepartmentActivity.Shipping, instance.GetExternalCode(Constants.DepartmentActivityTypes.Shipping, String.Empty, notify));
			AssertEquals(Xsd.DepartmentActivity.Gateway, instance.GetExternalCode(Constants.DepartmentActivityTypes.Gateway, String.Empty, notify));
		}

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(Constants.DepartmentActivityTypes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return false; }
		}

		protected override bool ShouldExcludeMapping(EnterpriseCodeExternalCodeMappings.Mapping mapping)
		{
			return (mapping.ExternalCode == nameof(Xsd.DepartmentActivity.NotDefined));
		}

		public void TestName()
		{
			AssertEquals("Department Activity", DepartmentActivityXmlMapping.Instance.NameForTesting);
		}
	}
}
