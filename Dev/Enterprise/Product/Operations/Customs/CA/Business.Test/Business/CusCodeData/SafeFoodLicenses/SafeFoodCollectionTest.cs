using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing;

[TestedType(typeof(SafeFoodLicenseCollection))]
public class SafeFoodCollectionTest : CusCodeDataCollectionTest<SafeFoodLicense>
{
	protected override CusCodeDataCollection<SafeFoodLicense> GetCusCodeDataCollection()
	{
		return new SafeFoodLicenseCollection(OrgHeader);
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var result = Factory.New<SafeFoodLicense>();
		result.CY_ParentID = OrgHeader.PK;
		result.CY_ParentTableCode = OrgHeader.TablePrefix;
		return result;
	}

	OrgHeader OrgHeader
	{
		get { return orgHeader ??= Factory.New<OrgHeader>(); }
	}
	OrgHeader orgHeader;
}
