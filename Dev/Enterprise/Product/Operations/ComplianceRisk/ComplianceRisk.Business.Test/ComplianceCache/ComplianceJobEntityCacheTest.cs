using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceJobEntityCache))]
	public class ComplianceJobEntityCacheTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var (complianceRisk, _) = Factory.CreateNewComplianceRiskWithShipment();
			return Factory.CreateNewComplianceJobEntityCache(complianceRisk, Factory.NewWithValidTestData<OrgHeader>());
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var (complianceRisk, _) = Factory.CreateNewComplianceRiskWithShipment();
			var cache = Factory.CreateNewComplianceJobEntityCache(complianceRisk, Factory.NewWithValidTestData<OrgHeader>());
			var cachePK = cache.PK;
			cache.Factory.Save();
			Assert("ComplianceJobEntityCache is in DB", cache.IsInDatabase);

			cache.Delete();
			cache.Factory.Save();

			AssertNull("ComplianceJobEntityCache is not in DB", (new BusinessObjectFactory()).Load<ComplianceJobEntityCache>(cachePK));
		}

		public void TestComplianceJobEntityCache_OrgHeaderAndRefVessel()
		{
			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			AssertComplianceJobEntityCache(complianceRisk, shipment, Factory.NewWithValidTestData<OrgHeader>());
			AssertComplianceJobEntityCache(complianceRisk, shipment, Factory.NewWithValidTestData<RefVessel>());
		}

		public void TestComplianceJobEntityCache_FreeTextVessel()
		{
			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			var transport = shipment.AddTransportLeg("AUSYD", "SGSIN", "FREE TEXT VESSEL");
			AssertComplianceJobEntityCache(complianceRisk, shipment, transport);
		}

		public void TestComplianceJobEntityCache_OverrideJobDocAddress()
		{
			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			var jobDocAddress = Factory.CreateNewJobDocAddress(shipment);
			AssertComplianceJobEntityCache(complianceRisk, shipment, jobDocAddress);
		}

		public void TestComplianceJobEntityCache_UniqueIndexViolation()
		{
			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			var entity = Factory.NewWithValidTestData<OrgHeader>();

			try
			{
				AssertComplianceJobEntityCache(complianceRisk, shipment, entity);
				AssertComplianceJobEntityCache(complianceRisk, shipment, entity);
				Fail("Expected unique index violation");
			}
			catch (ZSaveException ex)
			{
				var indexViolationText = @$"Cannot insert duplicate key row in object 'dbo.ComplianceJobEntityCache' with unique index 'NR_UX__CJE_EntityID_CJE_COR_ComplianceRisk'. The duplicate key value is ({entity.PK}, {complianceRisk.PK}).";
				AssertContains("Unique index violation message", indexViolationText, ex.Message);
			}
		}

		void AssertComplianceJobEntityCache(ComplianceRiskStatus complianceRisk, BusinessObject job, BusinessObject entity)
		{
			var cache = Factory.CreateNewComplianceJobEntityCache(complianceRisk, entity);
			Factory.Save();

			CombineAssertions(entity.TableName, () =>
			{
				AssertEquals(complianceRisk.COR_ParentID, job.PK);
				AssertEquals(complianceRisk.COR_ParentTableCode, job.TablePrefix);
				AssertEquals(entity.PK, cache.CJE_EntityID);
				AssertEquals(entity.TablePrefix, cache.CJE_EntityTableCode);
			});
		}
	}
}
