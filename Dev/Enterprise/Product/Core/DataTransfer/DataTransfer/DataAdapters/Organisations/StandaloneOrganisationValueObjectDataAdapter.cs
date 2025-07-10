using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class StandaloneOrganisationValueObjectDataAdapter : OrganisationValueObjectDataAdapter
	{
		protected override void ExportToValueObjectCore(OrgHeader bizObj, Xsd.Organisation constructedValueObject, IValueObjectExportContext context)
		{
			base.ExportToValueObjectCore(bizObj, constructedValueObject, context);

			var xsdOrg = constructedValueObject;
			var organisation = bizObj;

			ExportEDICodeMapping(bizObj, xsdOrg.OrganisationDetails);
			AddExportEvent(constructedValueObject, bizObj, context);
		}

		void ExportEDICodeMapping(OrgHeader organisation, Xsd.OrganisationDetail xsdOrganisationDetail)
		{
			if (SystemDataRegistry.Instance.ExportEDICodeMapping.Value)
			{
				Xsd.EDICodeMappingCollection codes = new Xsd.EDICodeMappingCollection();
				foreach (OrgPatternMatchOverride patternMatch in organisation.PatternMatchOverrides_ForBinding)
				{
					Xsd.EDICodeMapping codeMapping = codes.AddNew();
					codeMapping.ForeignCode = patternMatch.OO_ForeignCode;
					codeMapping.Relationship = patternMatch.OO_Relationship;

					BusinessObjectFactory factory = organisation.Factory;
					ZString eDICode = ZString.Empty;
					switch (patternMatch.OO_Relationship)
					{
						case Constants.OrgPatternMatchOverrideRelationships.Organisation:
							OrgHeader mappedOrg = factory.Load<OrgHeader>(patternMatch.OO_LocalGuid);
							eDICode = (mappedOrg != null) ? mappedOrg.OH_Code : ZString.Empty;
							break;
						case Constants.OrgPatternMatchOverrideRelationships.Port:
							RefUNLOCO mappedPort = factory.Load<RefUNLOCO>(patternMatch.OO_LocalGuid);
							eDICode = (mappedPort != null) ? mappedPort.RL_Code : ZString.Empty;
							break;
						case Constants.OrgPatternMatchOverrideRelationships.Currency:
							RefCurrency mappedCurrency = factory.Load<RefCurrency>(patternMatch.OO_LocalGuid);
							eDICode = (mappedCurrency != null) ? mappedCurrency.RX_Code : ZString.Empty;
							break;
						case Constants.OrgPatternMatchOverrideRelationships.Country:
							RefCountry mappedCountry = factory.Load<RefCountry>(patternMatch.OO_LocalGuid);
							eDICode = (mappedCountry != null) ? mappedCountry.RN_Code : ZString.Empty;
							break;
						case Constants.OrgPatternMatchOverrideRelationships.ContainerType:
							RefContainer mappedContainer = factory.Load<RefContainer>(patternMatch.OO_LocalGuid);
							eDICode = (mappedContainer != null) ? mappedContainer.RC_Code : ZString.Empty;
							break;
						case Constants.OrgPatternMatchOverrideRelationships.IncoTerm:
							eDICode = patternMatch.OO_LocalCode;
							break;
					}
					codeMapping.EDICode = eDICode;
				}
				xsdOrganisationDetail.EDICodeMappings = codes;
			}
		}
	}
}
