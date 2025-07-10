using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Business;
using Enterprise.Core.Forms;

namespace Enterprise.Client.EDI.TokenAuthenticationOnBoarding.Module.Testing
{
	class TokenAuthenticationOnBoardingFilterControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var collection = new EdiTokenAuthOnBoardingDataCollection(Factory);
			using (var filterControl = new TokenAuthenticationOnBoardingFilterControl(collection, new TokenAuthenticationOnBoardingFilterBusinessObject()))
			{
				var columns = filterControl.Grid.ColumnStyles;
				var columnNames = columns.Cast<ZGridColumnInfo>().Select(column => column.ColumnName);
				AssertContainsExactElementsInExactOrder(
					new string[]
					{
						"TOD_SystemCreateUser",
						"TOD_SystemCreateTimeUtc",
						"TOD_SystemLastEditUser",
						"TOD_SystemLastEditTimeUtc",
						"TOD_SystemUniqueIdentifier",
						"LicenceEnterprise+LE_EnterpriseID",
						"LicenceEnterprise+LE_EnterpriseCode",
						"LicenceEnterprise+Header+OH_FullName",
						"LicenceEnterprise+Header+OH_Code",
						"TOD_VerificationUsername",
						"TOD_ConfigurationIdentifier",
						"TOD_ClaimMappingName",
						"TOD_ClaimMappingIdentifier",
						"TOD_Status",
						"Incident+IM_IncidentNumber",
						"TOD_Retry",
						"TOD_OIDCServer",
						"Tenant+IDT_Name",
						"Tenant+IDT_TenantId",
						"TOD_WinzorOnly"
					},
					columnNames);
			}
		}
	}
}
