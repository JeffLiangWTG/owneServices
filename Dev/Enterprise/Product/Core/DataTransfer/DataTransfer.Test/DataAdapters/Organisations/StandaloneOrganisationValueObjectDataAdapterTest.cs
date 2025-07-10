using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(StandaloneOrganisationValueObjectDataAdapter))]
	class StandaloneOrganisationValueObjectDataAdapterTest : OrganisationValueObjectDataAdapterTest
	{
		public void TestEDIMappingsNotExportedWhenRegistryItemSetToFalse()
		{
			SystemDataRegistry.Instance.ExportEDICodeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			OrgHeader orgToExport = Factory.New<OrgHeader>();
			orgToExport.OH_Code = "TEMPORG";
			OrgHeader orgToMatch = Factory.LoadTop1<OrgHeader>(new ZQuery());
			SetEDICodeMapping(orgToExport, orgToMatch.PK, "ABC", Constants.OrgPatternMatchOverrideRelationships.Organisation);

			StandaloneOrganisationValueObjectDataAdapter adapter = new StandaloneOrganisationValueObjectDataAdapter();
			Xsd.Organisation xmlOrganisation = adapter.ExportToValueObject(orgToExport, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("No edi mappings should have been exported", 0, xmlOrganisation.OrganisationDetails.EDICodeMappings.Count);
		}

		public void TestExportOfEDICodeMappings()
		{
			SystemDataRegistry.Instance.ExportEDICodeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OrgHeader orgToMatch = Factory.LoadTop1<OrgHeader>(new ZQuery());
			RefUNLOCO port = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery());
			RefContainer container = Factory.LoadTop1<RefContainer>(new ZQuery());
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery());

			OrgHeader orgToExport = Factory.New<OrgHeader>();
			orgToExport.OH_Code = "TEMPORG";
			SetEDICodeMapping(orgToExport, orgToMatch.PK, "ABC", Constants.OrgPatternMatchOverrideRelationships.Organisation);
			SetEDICodeMapping(orgToExport, country.PK, "UNI", Constants.OrgPatternMatchOverrideRelationships.Country);
			SetEDICodeMapping(orgToExport, container.PK, "CONT", Constants.OrgPatternMatchOverrideRelationships.ContainerType);
			SetEDICodeMapping(orgToExport, currency.PK, "123", Constants.OrgPatternMatchOverrideRelationships.Currency);
			SetEDICodeMapping(orgToExport, ZGuid.Empty, "ABC", Constants.OrgPatternMatchOverrideRelationships.IncoTerm);
			SetEDICodeMapping(orgToExport, port.PK, "PORT", Constants.OrgPatternMatchOverrideRelationships.Port);

			StandaloneOrganisationValueObjectDataAdapter adapter = new StandaloneOrganisationValueObjectDataAdapter();
			Xsd.Organisation xmlOrganisation = adapter.ExportToValueObject(orgToExport, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("6 edi mappings should have been exported", 6, xmlOrganisation.OrganisationDetails.EDICodeMappings.Count);
			AssertExportedMappings(xmlOrganisation.OrganisationDetails.EDICodeMappings, orgToMatch.OH_Code, "ABC", Constants.OrgPatternMatchOverrideRelationships.Organisation);
			AssertExportedMappings(xmlOrganisation.OrganisationDetails.EDICodeMappings, container.RC_Code, "CONT", Constants.OrgPatternMatchOverrideRelationships.ContainerType);
			AssertExportedMappings(xmlOrganisation.OrganisationDetails.EDICodeMappings, country.RN_Code, "UNI", Constants.OrgPatternMatchOverrideRelationships.Country);
			AssertExportedMappings(xmlOrganisation.OrganisationDetails.EDICodeMappings, currency.RX_Code, "123", Constants.OrgPatternMatchOverrideRelationships.Currency);
			AssertExportedMappings(xmlOrganisation.OrganisationDetails.EDICodeMappings, Constants.IncoTerms.FreeCarrier, "ABC", Constants.OrgPatternMatchOverrideRelationships.IncoTerm);
			AssertExportedMappings(xmlOrganisation.OrganisationDetails.EDICodeMappings, port.RL_Code, "PORT", Constants.OrgPatternMatchOverrideRelationships.Port);

			AssertNotNull("A DEX event should have been created", orgToExport.Logs.MostRecentLogByEventTime(Events.DataExport));
		}

		void AssertExportedMappings(Xsd.EDICodeMappingCollection xsdMappings, ZString eDICode, ZString foreignCode, ZString relationship)
		{
			bool result = false;
			foreach (Xsd.EDICodeMapping mapping in xsdMappings)
			{
				if (mapping.Relationship.CompareTo(relationship) == 0
					&& mapping.EDICode.CompareTo(eDICode) == 0
					&& mapping.ForeignCode.CompareTo(foreignCode) == 0)
				{
					result = true;
				}
			}
			AssertEquals(relationship + " code mapping should have been properly exported", true, result);
		}

		void SetEDICodeMapping(OrgHeader header, ZGuid localGuid, ZString foreignCode, ZString relationship)
		{
			OrgPatternMatchOverride match = header.CreatePatternMatchOverrideForTest();
			match.OO_ForeignCode = foreignCode;
			match.OO_Relationship = relationship;
			if (relationship == Constants.OrgPatternMatchOverrideRelationships.IncoTerm)
			{
				match.OO_LocalCode = Constants.IncoTerms.FreeCarrier;
			}
			else
			{
				match.OO_LocalGuid = localGuid;
			}
		}
	}
}
