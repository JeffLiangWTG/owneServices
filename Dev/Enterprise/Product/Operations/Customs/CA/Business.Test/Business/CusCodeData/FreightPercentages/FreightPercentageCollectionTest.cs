using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(FreightPercentageCollection))]
	sealed class FreightPercentageCollectionTest : CusCodeDataCollectionTest<FreightPercentage>
	{
		protected override CusCodeDataCollection<FreightPercentage> GetCusCodeDataCollection()
		{
			return new FreightPercentageCollection(OrgHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<FreightPercentage>();
			result.CY_ParentID = OrgHeader.PK;
			result.CY_ParentTableCode = OrgHeader.TablePrefix;
			return result;
		}

		OrgHeader OrgHeader
		{
			get { return orgHeader ?? (orgHeader = Factory.New<OrgHeader>()); }
		}
		OrgHeader orgHeader;
	}
}
