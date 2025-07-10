using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(ServiceLevel))]
	class ServiceLevelTest : DataObjectTestCase<ServiceLevel>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ nameof(ServiceLevel.Code), OrgCarrierServiceLevelSchema.PL_Code.MaxLength },
				{ nameof(ServiceLevel.Description), OrgCarrierServiceLevelSchema.PL_ServicePrintDescription.MaxLength },
				{ nameof(ServiceLevel.CarrierServiceCode), OrgCarrierServiceLevelSchema.PL_CarrierServiceCode.MaxLength },
				{ nameof(ServiceLevel.CarrierProductCode), OrgCarrierServiceLevelSchema.PL_ProductCode.MaxLength },
				{ nameof(ServiceLevel.CarrierChargeCode), OrgCarrierServiceLevelSchema.PL_ChargeCode.MaxLength },
				{ nameof(ServiceLevel.CarrierProfileID), OrgCarrierServiceLevelSchema.PL_APProfileID.MaxLength },
			};
		}
	}
}

