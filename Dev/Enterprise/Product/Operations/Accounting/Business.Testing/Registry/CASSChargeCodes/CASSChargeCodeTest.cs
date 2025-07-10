using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CASSChargeCode))]
	public class CASSChargeCodeTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestCASSTypeValidation()
		{
			AssertNoErrors("Precondition: CASS Type shouldn't have error", BizObj.CASSTypeInfo);

			BizObj.CASSType = "TST";
			AssertHasError(BizObj.CASSTypeInfo, "Enter a valid selection.");

			BizObj.CASSType = ZString.Empty;
			AssertHasError(BizObj.CASSTypeInfo, "Please enter a value.");

			BizObj.CASSType = CASSChargeCodeLookups.CASSTypes.Export.Code;
			AssertNoErrors("Precondition: CASS Type shouldn't have error", BizObj.CASSTypeInfo);
		}

		public void TestCASSComponentCodeValidation()
		{
			AssertNoErrors("Precondition: CASSComponentCode shouldn't have error", BizObj.CASSComponentCodeInfo);

			BizObj.CASSComponentCode = "TST";
			AssertHasError(BizObj.CASSComponentCodeInfo, "Enter a valid selection.");

			BizObj.CASSComponentCode = ZString.Empty;
			AssertHasError(BizObj.CASSComponentCodeInfo, "Please enter a value.");

			BizObj.CASSType = CASSChargeCodeLookups.CASSTypes.Export.Code;
			BizObj.CASSComponentCode = CASSChargeCodeLookups.CASSExportLineComponentList[0].Code;
			AssertNoErrors("Precondition: CASSComponentCode shouldn't have error", BizObj.CASSComponentCodeInfo);

			BizObj.CASSComponentCode = CASSChargeCodeLookups.CASSImportLineComponentList[1].Code;
			AssertHasError(BizObj.CASSComponentCodeInfo, "Enter a valid selection.");

			BizObj.CASSType = CASSChargeCodeLookups.CASSTypes.Import.Code;
			AssertNoErrors("Precondition: CASSComponentCode shouldn't have error", BizObj.CASSComponentCodeInfo);
		}

		public void TestChargeCodeValidation()
		{
			AssertNoErrors("Precondition: ChargeCode shouldn't have error", BizObj.ChargeCodePKInfo);

			BizObj.ChargeCodePK = new ZGuid("39617382-1833-4fa4-b278-71d7081c29c5");
			AssertHasError(BizObj.ChargeCodePKInfo, "Enter a valid selection.");

			BizObj.ChargeCodePK = ObjectCreator.CC1.PK;
			AssertNoErrors("Precondition: ChargeCode shouldn't have error", BizObj.ChargeCodePKInfo);
		}

		public void TestIntegrity_MissingComponent()
		{
			var cASSMaps = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

			var cASSMap1 = cASSMaps.AddNew();
			cASSMap1.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			cASSMap1.CASSType = "ALL";
			cASSMap1.CASSComponentCode = "ALL";
			cASSMap1.ChargeCodePK = ObjectCreator.FRT.PK;
			cASSMap1.RunPreSaveValidation();

			AssertNoErrors(cASSMap1.CASSTypeInfo);
			AssertNoErrors(cASSMap1.CASSComponentCodeInfo);
			AssertNoErrors(cASSMap1.ChargeCodePKInfo);

			cASSMap1.CASSType = CASSChargeCodeLookups.CASSTypes.Export.Code;
			cASSMap1.RunPreSaveValidation();
			AssertHasError(cASSMap1.CASSTypeInfo, "You must add settings for CASS Type: CASS Import.");

			var cASSMap2 = cASSMaps.AddNew();
			cASSMap2.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			cASSMap2.CASSType = CASSChargeCodeLookups.CASSTypes.Import.Code;
			cASSMap2.CASSComponentCode = "ALL";
			cASSMap2.ChargeCodePK = ObjectCreator.FRT.PK;
			cASSMap2.RunPreSaveValidation();
			AssertNoErrors(cASSMap2.CASSTypeInfo);
			AssertNoErrors(cASSMap2.CASSComponentCodeInfo);
			AssertNoErrors(cASSMap2.ChargeCodePKInfo);

			cASSMap2.CASSComponentCode = CASSChargeCodeLookups.CASSComponents.WeightOrValuationCharge.Code;
			cASSMap2.RunPreSaveValidation();
			var missingCASSComponentCodes = string.Join(", ", CASSChargeCodeLookups.CASSImportLineComponentList.ToArray().Where(x => x.Code != "ALL" && x.Code != cASSMap2.CASSComponentCode).Select(x => x.Code).ToArray());
			AssertHasError(cASSMap2.CASSComponentCodeInfo, "You must add settings for CASS Component Code: CCA, CCC, STC, COF, HDC, OC1, OC2, MCA.");

			cASSMap2.CASSComponentCode = "ALL";
			cASSMap2.RunPreSaveValidation();
			AssertNoErrors(cASSMap2.CASSComponentCodeInfo);
		}

		public void TestIntegrity_InvalidRow()
		{
			var cASSMaps = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

			var cASSMap1 = cASSMaps.AddNew();
			cASSMap1.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			cASSMap1.CASSType = "ALL";
			cASSMap1.CASSComponentCode = "ALL";
			cASSMap1.ChargeCodePK = ObjectCreator.FRT.PK;
			cASSMap1.RunPreSaveValidation();

			AssertNoErrors(cASSMap1.CASSTypeInfo);
			AssertNoErrors(cASSMap1.CASSComponentCodeInfo);
			AssertNoErrors(cASSMap1.ChargeCodePKInfo);

			var cASSMap2 = cASSMaps.AddNew();
			cASSMap2.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			cASSMap2.CASSType = CASSChargeCodeLookups.CASSTypes.Export.Code;
			cASSMap2.CASSComponentCode = "ALL";
			cASSMap2.ChargeCodePK = ObjectCreator.CC1.PK;
			cASSMap2.RunPreSaveValidation();
			AssertHasError(cASSMap2.CASSTypeInfo, "Settings for a specific CASS Type is not allowed, as there is a settings for 'ALL' CASS Type.");

			cASSMap1.CASSType = CASSChargeCodeLookups.CASSTypes.Import.Code;
			cASSMap1.RunPreSaveValidation();
			AssertNoErrors(cASSMap1.CASSTypeInfo);
			AssertNoErrors(cASSMap1.CASSComponentCodeInfo);
			AssertNoErrors(cASSMap1.ChargeCodePKInfo);

			cASSMap2.RunPreSaveValidation();
			AssertNoErrors(cASSMap2.CASSTypeInfo);
			AssertNoErrors(cASSMap2.CASSComponentCodeInfo);
			AssertNoErrors(cASSMap2.ChargeCodePKInfo);

			var cASSMap3 = cASSMaps.AddNew();
			cASSMap3.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			cASSMap3.CASSType = CASSChargeCodeLookups.CASSTypes.Export.Code;
			cASSMap3.CASSComponentCode = CASSChargeCodeLookups.CASSExportLineComponentList[1].Code;
			cASSMap3.ChargeCodePK = ObjectCreator.CC2.PK;
			cASSMap3.RunPreSaveValidation();
			AssertHasError(cASSMap3.CASSComponentCodeInfo, "Settings for a specific CASS Component with CASS Type 'CEXP' is not allowed, as there is a settings for 'ALL' CASS Component.");

			cASSMap3.CASSComponentCode = "ALL";
			cASSMap3.RunPreSaveValidation();
			AssertNoErrors(cASSMap3.CASSTypeInfo);
			AssertNoErrors(cASSMap3.CASSComponentCodeInfo);
			AssertNoErrors(cASSMap3.ChargeCodePKInfo);
		}

		public void TestPreSaveValidation()
		{
			var cASSMaps = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

			var cASSMap1 = cASSMaps.AddNew();
			cASSMap1.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			cASSMap1.CASSType = "ALL";
			cASSMap1.CASSComponentCode = "ALL";
			cASSMap1.ChargeCodePK = ObjectCreator.FRT.PK;
			cASSMap1.RunPreSaveValidation();
			Assert("Shouldn't have errors", !cASSMap1.HasErrors());

			cASSMap1.CASSType = "TST";
			cASSMap1.RunPreSaveValidation();
			Assert("CASSTypeInfo Should have error", cASSMap1.CASSTypeInfo.HasError("Enter a valid selection."));

			cASSMap1.CASSType = "ALL";
			cASSMap1.RunPreSaveValidation();
			Assert("Shouldn't have errors", !cASSMap1.HasErrors());

			cASSMap1.CASSComponentCode = "TSTC";
			cASSMap1.RunPreSaveValidation();
			Assert("CASSComponentCodeInfo Should have error", cASSMap1.CASSComponentCodeInfo.HasError("Enter a valid selection."));

			cASSMap1.CASSComponentCode = "ALL";
			cASSMap1.RunPreSaveValidation();
			Assert("Shouldn't have errors", !cASSMap1.HasErrors());

			cASSMap1.ChargeCodePK = new ZGuid("39617382-1833-4fa4-b278-71d7081c29c5");
			cASSMap1.RunPreSaveValidation();
			Assert("ChargeCodeInfo Should have error", cASSMap1.ChargeCodePKInfo.HasError("Enter a valid selection."));

			cASSMap1.ChargeCodePK = ObjectCreator.CC1.PK;
			cASSMap1.RunPreSaveValidation();
			Assert("Shouldn't have errors", !cASSMap1.HasErrors());
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetNewBusinessObject() as RegistryBusinessObjectTemplate;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cASSMaps = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);

			var cassCode = cASSMaps.AddNew();
			cassCode.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			cassCode.CASSType = "ALL";
			cassCode.CASSType = "ALL";
			cassCode.CASSComponentCode = "ALL";
			cassCode.ChargeCodePK = new ZGuid(Env.Registry.FreightChargeCode);
			return cassCode;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected new CASSChargeCode BizObj
		{
			get { return (CASSChargeCode)base.BizObj; }
		}

		TestObjectCreator ObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		TestObjectCreator testObjectCreator;
	}
}
