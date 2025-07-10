using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Testing
{
	public class CASSChargeCodeRegistryExtractorTest : TestCaseWithFactory
	{
		public void TestGetChargeCodesByCASSCostComponent_Default()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var regValue = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				var cassChargeCode1 = regValue.AddNew();
				cassChargeCode1.CASSComponentCode = CASSChargeCodeLookups.ALL;
				cassChargeCode1.ChargeCodePK = ObjetCreator.CC1.PK;
				cassChargeCode1.CASSType = CASSChargeCodeLookups.ALL;
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

				var chargeCodePKs = CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code);
				AssertEquals("Charge Code Count", 1, chargeCodePKs.Length);
				AssertEquals("Charge Code: CC1", ObjetCreator.CC1.PK, chargeCodePKs.Cast<ZGuid>().First());

				chargeCodePKs = CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.CASSComponents.WeightOrValuationCharge.Code);
				AssertEquals("Charge Code Count", 1, chargeCodePKs.Length);
				AssertEquals("Charge Code: CC1", ObjetCreator.CC1.PK, chargeCodePKs.Cast<ZGuid>().First());
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestGetChargeCodesByCASSCostComponent_SpecificCASSType()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var regValue = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

				var cassChargeCode1 = regValue.AddNew();
				cassChargeCode1.CASSComponentCode = CASSChargeCodeLookups.ALL;
				cassChargeCode1.ChargeCodePK = ObjetCreator.CC1.PK;
				cassChargeCode1.CASSType = CASSChargeCodeLookups.CASSTypes.Export.Code;

				var cassChargeCode2 = regValue.AddNew();
				cassChargeCode2.CASSComponentCode = CASSChargeCodeLookups.ALL;
				cassChargeCode2.ChargeCodePK = ObjetCreator.CC2.PK;
				cassChargeCode2.CASSType = CASSChargeCodeLookups.CASSTypes.Import.Code;

				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

				var chargeCodePKs = CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code);
				AssertEquals("Charge Code Count", 1, chargeCodePKs.Length);
				AssertEquals("Charge Code: CC1", ObjetCreator.CC1.PK, chargeCodePKs.Cast<ZGuid>().First());

				chargeCodePKs = CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code);
				AssertEquals("Charge Code Count", 1, chargeCodePKs.Length);
				AssertEquals("Charge Code: CC1", ObjetCreator.CC1.PK, chargeCodePKs.Cast<ZGuid>().First());

				chargeCodePKs = CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.CASSComponents.WeightOrValuationCharge.Code);
				AssertEquals("Charge Code Count", 1, chargeCodePKs.Length);
				AssertEquals("Charge Code: CC2", ObjetCreator.CC2.PK, chargeCodePKs.Cast<ZGuid>().First());

				chargeCodePKs = CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.CASSComponents.OtherCharge1.Code);
				AssertEquals("Charge Code Count", 1, chargeCodePKs.Length);
				AssertEquals("Charge Code: CC2", ObjetCreator.CC2.PK, chargeCodePKs.Cast<ZGuid>().First());
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestGetChargeCodesByCASSCostComponent_Mixed()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var regValue = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

				var cassChargeCode1 = regValue.AddNew();
				cassChargeCode1.CASSComponentCode = CASSChargeCodeLookups.ALL;
				cassChargeCode1.ChargeCodePK = ObjetCreator.CC1.PK;
				cassChargeCode1.CASSType = CASSChargeCodeLookups.CASSTypes.Export.Code;

				var cassChargeCode2 = regValue.AddNew();
				cassChargeCode2.CASSComponentCode = CASSChargeCodeLookups.ALL;
				cassChargeCode2.ChargeCodePK = ObjetCreator.CC2.PK;
				cassChargeCode2.CASSType = CASSChargeCodeLookups.CASSTypes.Import.Code;

				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

				var chargeCodePKs = CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code);
				AssertEquals("Charge Code Count", 1, chargeCodePKs.Length);
				AssertEquals("Charge Code: CC1", ObjetCreator.CC1.PK, chargeCodePKs.Cast<ZGuid>().First());

				chargeCodePKs = CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code);
				AssertEquals("Charge Code Count", 1, chargeCodePKs.Length);
				AssertEquals("Charge Code: CC1", ObjetCreator.CC1.PK, chargeCodePKs.Cast<ZGuid>().First());

				chargeCodePKs = CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.CASSComponents.WeightOrValuationCharge.Code);
				AssertEquals("Charge Code Count", 1, chargeCodePKs.Length);
				AssertEquals("Charge Code: CC2", ObjetCreator.CC2.PK, chargeCodePKs.Cast<ZGuid>().First());

				chargeCodePKs = CASSChargeCodeRegistryExtractor.GetChargeCodesByCASSCostComponent(CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.CASSComponents.OtherCharge1.Code);
				AssertEquals("Charge Code Count", 1, chargeCodePKs.Length);
				AssertEquals("Charge Code: CC2", true, chargeCodePKs.Cast<ZGuid>().Contains(ObjetCreator.CC2.PK));
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestGetCASSCompnentsByChargeCode()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var regValue = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				var cassChargeCode1 = regValue.AddNew();
				cassChargeCode1.CASSComponentCode = CASSChargeCodeLookups.ALL;
				cassChargeCode1.ChargeCodePK = ObjetCreator.CC1.PK;
				cassChargeCode1.CASSType = CASSChargeCodeLookups.ALL;
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

				var components = CASSChargeCodeRegistryExtractor.GetCASSCompnentsByChargeCode(CASSChargeCodeLookups.CASSTypes.Export.Code, ObjetCreator.CC1.PK);
				AssertEquals("Components Count", CASSChargeCodeLookups.CASSExportLineComponentList.Count, components.Length);
				AssertEquals("Export Components", true, components.All(x => CASSChargeCodeLookups.CASSExportLineComponentList.ContainsCode(x)));

				components = CASSChargeCodeRegistryExtractor.GetCASSCompnentsByChargeCode(CASSChargeCodeLookups.CASSTypes.Import.Code, ObjetCreator.CC1.PK);
				AssertEquals("Components Count", CASSChargeCodeLookups.CASSImportLineComponentList.Count, components.Length);
				AssertEquals("Import Components", true, components.All(x => CASSChargeCodeLookups.CASSImportLineComponentList.ContainsCode(x)));
			}
			finally
			{
				prevValue.Cast<CASSChargeCode>().ForEach(x => x.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		TestObjectCreator ObjetCreator
		{
			get
			{
				if (objetCreator == null)
				{
					objetCreator = new TestObjectCreator(Factory);
				}
				return objetCreator;
			}
		}
		TestObjectCreator objetCreator;
	}
}
