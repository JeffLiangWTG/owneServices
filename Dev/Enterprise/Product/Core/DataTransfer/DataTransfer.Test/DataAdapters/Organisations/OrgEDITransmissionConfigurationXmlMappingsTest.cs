using System;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(OrgEDITransmissionConfigurationXmlMappings))]
	sealed class OrgEDITransmissionConfigurationXmlMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		public void TestName()
		{
			TestOrgEDITransmissionConfigurationXmlMappings mappings = new TestOrgEDITransmissionConfigurationXmlMappings();
			AssertEquals("Transmission Configuration", mappings.Name);
		}

		public void TestGetExternalCode()
		{
			INotifications notifications = new NotificationBuffer();
			OrgEDITransmissionConfigurationXmlMappings mappings = OrgEDITransmissionConfigurationXmlMappings.Instance;
			AssertEquals(Xsd.OrganisationDetailEDITransmissionDetailsType.EMA, mappings.GetExternalCode(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment, "", notifications));
			AssertEquals(Xsd.OrganisationDetailEDITransmissionDetailsType.EMT, mappings.GetExternalCode(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText, "", notifications));
		}

		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(EDICommunicationsModeCommunicationsTransportList) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}

		class TestOrgEDITransmissionConfigurationXmlMappings : OrgEDITransmissionConfigurationXmlMappings
		{
			public new string Name
			{
				get { return base.Name; }
			}
		}
	}
}
