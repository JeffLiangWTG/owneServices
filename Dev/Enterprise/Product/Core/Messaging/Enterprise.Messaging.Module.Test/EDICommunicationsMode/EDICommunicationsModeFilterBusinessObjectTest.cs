using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Messaging.Module.EDICommunicationsModeFilterBusinessObject;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDICommunicationsModeFilterBusinessObject))]
	sealed class EDICommunicationsModeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new EDICommunicationsModeFilterBusinessObject();
		}

		public void TestFilterByModule()
		{
			var bizo1 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			var lookupValue = "ACA";

			bizo1.EK_Module = lookupValue;

			var bizo2 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo2.EK_Module = "ACI";

			var bizo3 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo3.EK_Module = lookupValue;

			Factory.Save();

			var filterBusinessObject = new EDICommunicationsModeFilterBusinessObject();
			var propertyName = "Module";
			var filter = (ModuleTextFilter)filterBusinessObject[propertyName];
			{
				filter.Property = lookupValue;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				var modes = Factory.Load<EDICommunicationsMode>(filter.Query);
				AssertEquals(2, modes.Length);
				Assert(modes.Contains(bizo1));
				Assert(!modes.Contains(bizo2));
				Assert(modes.Contains(bizo3));
			}
		}

		public void TestFilterByCommsDirection()
		{
			var bizo1 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			var lookupValue = "TRX";

			bizo1.EK_CommsDirection = lookupValue;

			var bizo2 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo2.EK_CommsDirection = "RCV";

			Factory.Save();

			var filterBusinessObject = new EDICommunicationsModeFilterBusinessObject();
			var propertyName = "Comm. Direction";
			var filter = (ModuleTextFilter)filterBusinessObject[propertyName];
			{
				filter.Property = lookupValue;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				var modes = Factory.Load<EDICommunicationsMode>(filter.Query);
				AssertEquals(1, modes.Length);
				Assert(modes.Contains(bizo1));
				Assert(!modes.Contains(bizo2));
			}
		}

		public void TestFilterByFileFormat()
		{
			var bizo1 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			var lookup = new EDICommunicationsModeLookups(Factory.New<EDICommunicationsMode>());
			var lookupValue = "XML";

			bizo1.EK_FileFormat = lookupValue;

			var bizo2 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo2.EK_FileFormat = "SFF";

			Factory.Save();

			var filterBusinessObject = new EDICommunicationsModeFilterBusinessObject();
			var propertyName = "File Format";
			var filter = (ModuleTextFilter)filterBusinessObject[propertyName];
			{
				filter.Property = lookupValue;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				var modes = Factory.Load<EDICommunicationsMode>(filter.Query);
				AssertEquals(1, modes.Length);
				Assert(modes.Contains(bizo1));
				Assert(!modes.Contains(bizo2));
			}
		}

		public void TestFilterByCommunicationsTransport()
		{
			var bizo1 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			var lookupValue = "EMA";

			bizo1.EK_CommunicationsTransport = lookupValue;

			var bizo2 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo2.EK_CommunicationsTransport = "EMT";

			Factory.Save();

			var filterBusinessObject = new EDICommunicationsModeFilterBusinessObject();
			var propertyName = "Comm. Transport";
			var filter = (ModuleTextFilter)filterBusinessObject[propertyName];
			{
				filter.Property = lookupValue;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				var modes = Factory.Load<EDICommunicationsMode>(filter.Query);
				AssertEquals(1, modes.Length);
				Assert(modes.Contains(bizo1));
				Assert(!modes.Contains(bizo2));
			}
		}

		public void TestFilterByOrganisation()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.FillWithValidTestData();
			var lookupValue = orgHeader1.PK;

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.FillWithValidTestData();

			var bizo1 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo1.EK_ParentID = lookupValue;

			var bizo2 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			Factory.Save();

			var filterBusinessObject = new EDICommunicationsModeFilterBusinessObject();
			var propertyName = "Organisation";
			var filter = (ModuleGuidFilter)filterBusinessObject[propertyName];
			{
				filter.Property = lookupValue;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				var modes = Factory.Load<EDICommunicationsMode>(filter.Query);
				AssertEquals(1, modes.Length);
				Assert(modes.Contains(bizo1));
				Assert(!modes.Contains(bizo2));
			}
		}

		public void TestFilterByCommunicationParty()
		{
			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			party.ECP_Name = "n1";

			var partyConfig1 = Factory.NewWithValidTestData<EDICommunicationPartyConfig>();
			partyConfig1.ECC_ECP_Party = party.PK;
			partyConfig1.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Inbound;

			var partyConfig2 = Factory.NewWithValidTestData<EDICommunicationPartyConfig>();
			partyConfig2.ECC_ECP_Party = party.PK;
			partyConfig2.ECC_Direction = EDICommunicationPartyConfigDirectionsList.Codes.Outbound;

			var bizo1 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo1.CommunicationParty = party.PK;
			var bizo2 = Factory.NewWithValidTestData<EDICommunicationsMode>();

			Factory.Save();

			AssertEquals(2, Factory.GetDatabaseCount(typeof(EDICommunicationsMode)));

			var filterBusinessObject = new EDICommunicationsModeFilterBusinessObject();
			var propertyName = "EDI Client";
			var filter = (ModuleGuidFilter)filterBusinessObject[propertyName];
			{
				filter.Property = party.PK;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
				var modes = Factory.Load<EDICommunicationsMode>(filter.Query);
				AssertEquals(1, modes.Length);
				Assert(modes.Contains(bizo1));
				Assert(!modes.Contains(bizo2));
			}
		}

		public void TestFilterDisplaysOnlyOrgHeaderParentRecords()
		{
			var bizo1 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo1.EK_ParentTableCode = "OH";
			bizo1.EK_ParentID = ZGuid.NewZGuid();

			var bizo2 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo2.EK_ParentTableCode = "OH";
			bizo2.EK_ParentID = ZGuid.NewZGuid();

			var bizo3 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo3.EK_ParentTableCode = "OB";
			bizo3.EK_ParentID = ZGuid.NewZGuid();

			var bizo4 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo4.EK_ParentTableCode = "GP";
			bizo4.EK_ParentID = ZGuid.NewZGuid();

			var bizo5 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo5.EK_ParentTableCode = "";
			bizo5.EK_ParentID = ZGuid.NewZGuid();

			var bizo6 = Factory.NewWithValidTestData<EDICommunicationsMode>();
			bizo6.EK_ParentTableCode = "OH";
			bizo6.EK_ParentID = ZGuid.Empty;

			Factory.Save();

			var filterBusinessObject = new EDICommunicationsModeFilterBusinessObject();
			var propertyName = "HiddenFilter";
			var filter = (ModuleTextFilter)filterBusinessObject[propertyName];
			{
				AssertNotNull(filter);
				Assert(filter.Visibility == FilterVisibility.AlwaysAppliedAndHidden);
				AssertMultilineASCIIEquals("SQL query should match", @"EK_ParentTableCode = 'OH' 
AND
EK_ParentID <> CONVERT
(
	'00000000-0000-0000-0000-000000000000', 'System.Guid'
)", filter.Query.LiteralTextADOFormatted);

				var modes = Factory.Load<EDICommunicationsMode>(filter.Query);
				AssertEquals(2, modes.Length);
				Assert(modes.Contains(bizo1));
				Assert(modes.Contains(bizo2));
				Assert(!modes.Contains(bizo3));
			}
		}

		[TestedType(typeof(EDICommunicationsModeModuleTextFilter))]
		sealed class EDICommunicationsModeModuleTextFilterTest : ModuleTextFilterTest
		{
			protected override BusinessObject GetNewBusinessObject()
			{
				return new EDICommunicationsModeModuleTextFilter("Module", EDICommunicationsModeSchema.EK_Module, new List<string>(), (NoResString)"Module");
			}
		}
	}
}
