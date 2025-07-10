using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(AdditionalIdentificationCollection))]
	class AdditionalIdentificationCollectionTest : CusCodeDataCollectionTest<AdditionalIdentification>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<AdditionalIdentification>();
			result.CY_ParentID = OrgHeader.PK;
			result.CY_ParentTableCode = OrgHeader.TablePrefix;
			return result;
		}

		protected override CusCodeDataCollection<AdditionalIdentification> GetCusCodeDataCollection()
		{
			return new AdditionalIdentificationCollection(OrgHeader);
		}

		OrgHeader OrgHeader => orgHeader ?? Factory.NewWithValidTestData<OrgHeader>();
		readonly OrgHeader orgHeader;
	}
}
