using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(FreightWrapperFromIncidentManagementGroup))]
	internal class FreightWrapperFromIncidentManagementGroupTest : FreightWrapperEDITest<IncidentManagementGroup>
	{
		protected override GenericWrapper GetNewFreightWrapperCore()
		{
			return new FreightWrapperFromIncidentManagementGroup(ediBusinessObject, Factory);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string> {
					{ "JobNumber", $"INGABCDEF{NextNumber - 1}" },
					{ "JobNumberHeading", "Job Number" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", $"ÈINGABCDEF{NextNumber - 1}^Ê" },
					{ "JobNumberBarcodeText", "^ING=INGABCDEF0;;|" },
					{ "JobNumberBarcodeTextForFont", "È^ING=INGABCDEF0;;|7Ê" }, };
			}
		}

		public int NextNumber { get; set; }

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var bizo = Factory.New<IncidentManagementGroup>();
			bizo.ING_IncidentGroupNumber = $"INGABCDEF{NextNumber}";
			NextNumber++;
			bizo.ING_BusinessImpact = IncidentManagementGroupConstants.BusinessImpactCodes.HighImpact;
			bizo.ING_Urgency = IncidentManagementGroupConstants.UrgencyCodes.VeryHigh;
			bizo.ING_Product = ProductTypes.Codes.Enterprise;
			bizo.ING_GS_NKGroupOwner = "E";

			return bizo;
		}

		protected override string ExpectedBarcodeText() => "È^ING=INGABCDEF1;CAD;|\"Ê";
	}
}
