using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	class BillingDataAdapterTest : TestCaseWithFactory
	{
		public void TestOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible()
		{
			IValueObjectDataAdapter billingAdapter = new BillingDataAdapter();
			Assert(!billingAdapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible);
		}

		public void TestOnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked()
		{
			IValueObjectDataAdapter billingAdapter = new BillingDataAdapter();
			Assert(!billingAdapter.OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked);
		}

		public void TestSpecifiedCharges_AllCharges()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.AllCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 200, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 50, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertJobCharges("Replace Charges", job,
				"ZZAAA: 200.0000",
				"ZZBBB: 50.0000"
				);
		}

		public void TestSpecifiedCharges_UpdatedCharges()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.UpdatedCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 200, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 50, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertJobCharges("Update Charges", job,
				"ZZAAA: 200.0000",
				"ZZBBB: 50.0000",
				"ZZCCC: 60.0000"
				);
		}

		public void TestSpecifiedCharges_UpdatedChargesWithOSSellCurrencyMapping()
		{
			OrgHeader orgWithOverrides = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GlbCompany.CurrentCompany.OrgProxy.PK));

			RefCurrency aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			RefCurrency sGDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "SGD");
			RefCurrency kRWCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "KRW");
			RefCurrency jPYCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");
			RefCurrency hKDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "HKD");

			OrgPatternMatchOverride matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = aUDCurrency.PK;
			matchOverride.OO_ForeignCode = "AD";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = sGDCurrency.PK;
			matchOverride.OO_ForeignCode = "SG";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = kRWCurrency.PK;
			matchOverride.OO_ForeignCode = "KR";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = jPYCurrency.PK;
			matchOverride.OO_ForeignCode = "JP";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = hKDCurrency.PK;
			matchOverride.OO_ForeignCode = "HK";
			Factory.Save();

			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.UpdatedCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AD", Value = 200, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "KR", Value = 50, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZCCC",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "XX", Value = 100, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZDDD",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "USD", Value = 300, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");
			AccChargeCode ddd = GetChargeCode(GlbBranch.CurrentBranch, "DDD", "DDD Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30);
			AddCharge(job, ddd, 80, 40);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			string[] expected = { "ZZAAA: AUD",
				"ZZBBB: KRW",
				"ZZCCC: AUD",
				"ZZDDD: USD"
				};
			AssertContainsExactElementsInAnyOrder("Update Charges", expected, job.Charges.ToArray<Charge>().Select(c => FormatValuesToTestForCurrency(c.ChargeCode, c.JR_OSSellCurrencyCode)));
		}

		public void TestSpecifiedCharges_UpdatedChargesWithOSCostCurrencyMapping()
		{
			OrgHeader orgWithOverrides = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GlbCompany.CurrentCompany.OrgProxy.PK));

			RefCurrency aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			RefCurrency sGDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "SGD");
			RefCurrency kRWCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "KRW");
			RefCurrency jPYCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");
			RefCurrency hKDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "HKD");

			OrgPatternMatchOverride matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = aUDCurrency.PK;
			matchOverride.OO_ForeignCode = "AD";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = sGDCurrency.PK;
			matchOverride.OO_ForeignCode = "SG";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = kRWCurrency.PK;
			matchOverride.OO_ForeignCode = "KR";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = jPYCurrency.PK;
			matchOverride.OO_ForeignCode = "JP";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = hKDCurrency.PK;
			matchOverride.OO_ForeignCode = "HK";
			Factory.Save();

			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.UpdatedCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSCostAmount = new Xsd.FinancialValue() { CurrencyCode = "SG", Value = 900, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSCostAmount = new Xsd.FinancialValue() { CurrencyCode = "JP", Value = 80, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZCCC",
				OSCostAmount = new Xsd.FinancialValue() { CurrencyCode = "YY", Value = 400, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZDDD",
				OSCostAmount = new Xsd.FinancialValue() { CurrencyCode = "HKD", Value = 500, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");
			AccChargeCode ddd = GetChargeCode(GlbBranch.CurrentBranch, "DDD", "DDD Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30);
			AddCharge(job, ddd, 80, 40);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			string[] expected = { "ZZAAA: SGD",
				"ZZBBB: JPY",
				"ZZCCC: AUD",
				"ZZDDD: HKD"
				};
			AssertContainsExactElementsInAnyOrder("Update Charges", expected, job.Charges.ToArray<Charge>().Select(c => FormatValuesToTestForCurrency(c.ChargeCode, c.JR_OSCostCurrencyCode)));
		}

		public void TestSpecifiedCharges_NewCharges()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.NewCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 200, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 50, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertJobCharges("Add Charges", job,
				"ZZAAA: 180.0000",
				"ZZAAA: 10.0000",
				"ZZCCC: 60.0000",
				"ZZAAA: 200.0000",
				"ZZBBB: 50.0000"
				);
		}

		public void TestSpecifiedCharges_NewChargesWithOSSellCurrencyMapping()
		{
			OrgHeader orgWithOverrides = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GlbCompany.CurrentCompany.OrgProxy.PK));

			RefCurrency aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			RefCurrency sGDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "SGD");
			RefCurrency kRWCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "KRW");
			RefCurrency jPYCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");
			RefCurrency hKDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "HKD");

			OrgPatternMatchOverride matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = aUDCurrency.PK;
			matchOverride.OO_ForeignCode = "AD";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = sGDCurrency.PK;
			matchOverride.OO_ForeignCode = "SG";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = kRWCurrency.PK;
			matchOverride.OO_ForeignCode = "KR";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = jPYCurrency.PK;
			matchOverride.OO_ForeignCode = "JP";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = hKDCurrency.PK;
			matchOverride.OO_ForeignCode = "HK";
			Factory.Save();

			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.NewCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AD", Value = 200, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "KR", Value = 50, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZCCC",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "XX", Value = 100, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZDDD",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "USD", Value = 150, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");
			AccChargeCode ddd = GetChargeCode(GlbBranch.CurrentBranch, "DDD", "DDD Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			string[] expected = { "ZZAAA: AUD",
				"ZZAAA: AUD",
				"ZZCCC: AUD",
				"ZZAAA: AUD",
				"ZZBBB: KRW",
				"ZZDDD: USD"
				};
			AssertContainsExactElementsInAnyOrder("Add Charges", expected, job.Charges.ToArray<Charge>().Select(c => FormatValuesToTestForCurrency(c.ChargeCode, c.JR_OSSellCurrencyCode)));
		}

		public void TestSpecifiedCharges_NewChargesWithOSCostCurrencyMapping()
		{
			OrgHeader orgWithOverrides = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GlbCompany.CurrentCompany.OrgProxy.PK));

			RefCurrency aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			RefCurrency sGDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "SGD");
			RefCurrency kRWCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "KRW");
			RefCurrency jPYCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "JPY");
			RefCurrency hKDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "HKD");

			OrgPatternMatchOverride matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = aUDCurrency.PK;
			matchOverride.OO_ForeignCode = "AD";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = sGDCurrency.PK;
			matchOverride.OO_ForeignCode = "SG";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = kRWCurrency.PK;
			matchOverride.OO_ForeignCode = "KR";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = jPYCurrency.PK;
			matchOverride.OO_ForeignCode = "JP";

			matchOverride = orgWithOverrides.CreatePatternMatchOverrideForTest();
			matchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			matchOverride.OO_LocalGuid = hKDCurrency.PK;
			matchOverride.OO_ForeignCode = "HK";
			Factory.Save();

			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.NewCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSCostAmount = new Xsd.FinancialValue() { CurrencyCode = "SG", Value = 900, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSCostAmount = new Xsd.FinancialValue() { CurrencyCode = "JP", Value = 80, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZCCC",
				OSCostAmount = new Xsd.FinancialValue() { CurrencyCode = "YY", Value = 400, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZDDD",
				OSCostAmount = new Xsd.FinancialValue() { CurrencyCode = "HKD", Value = 500, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");
			AccChargeCode ddd = GetChargeCode(GlbBranch.CurrentBranch, "DDD", "DDD Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			string[] expected = { "ZZAAA: AUD",
				"ZZAAA: AUD",
				"ZZCCC: AUD",
				"ZZAAA: SGD",
				"ZZBBB: JPY",
				"ZZDDD: HKD"
				};
			AssertContainsExactElementsInAnyOrder("Add Charges", expected, job.Charges.ToArray<Charge>().Select(c => FormatValuesToTestForCurrency(c.ChargeCode, c.JR_OSCostCurrencyCode)));
		}

		public void TestSpecifiedCharges_UpdateIgnoringPosted()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.UpdatedCharges;
			billing.PostedChargesHandling = Xsd.PostedChargeHandling.Ignore;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 200, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 50, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertJobCharges("Update Charges", job,
				"ZZAAA: 200.0000",
				"ZZBBB: 50.0000",
				"ZZCCC: 60.0000"
				);
		}

		public void TestSpecifiedCharges_AllCharges_WithPosted()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.AllCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 200, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 50, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30, true);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertJobCharges("Dont Replace Charges", job,
				"ZZAAA: 180.0000",
				"ZZAAA: 10.0000",
				"ZZCCC: 60.0000"
				);
		}

		public void TestCreateJobHeaderConcurrentlyUsingImport()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipment = factory1.New<CommonShipment>();
			factory1.Save();

			Job job = new Job.Loader(factory1, shipment).TryLoadOrCreateWithMutex();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			var aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AddCharge(job, aaa, 180, 10);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			var billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.UpdatedCharges;
			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 200, },
			});
			try
			{
				using (Env.SetTemporaryUserContext("CWSupport", Env.CurrentBranchPK, Env.CurrentUserPK))
				{
					IValueObjectDataAdapter adapter = new BillingDataAdapter();
					var context = new ValueObjectImportContext(factory2, new NotificationBuffer());
					adapter.ImportFromValueObject(shipment2, billing, context);
					AssertEquals(true, context.NotificationsHasErrors);

					var message = string.Format(@"Error: You have created the job {0} on another form, but haven't saved it yet.
Please close or save other forms that use job {0} to continue.
", shipment.JobNumber);
					AssertEquals(message, (context.Notifications as NotificationBuffer).AsString);
				}
			}
			finally
			{
				job.Dispose();
			}
		}

		public void TestSpecifiedCharges_UpdatedCharges_WithPosted()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.UpdatedCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 200, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 50, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30, true);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertJobCharges("Dont Update Charges", job,
				"ZZAAA: 180.0000",
				"ZZAAA: 10.0000",
				"ZZCCC: 60.0000"
				);
		}

		public void TestSpecifiedCharges_NewCharges_WithPosted()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.NewCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 200, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 50, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, aaa, 10, 20);
			AddCharge(job, ccc, 60, 30, true);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertJobCharges("Add Charges", job,
				"ZZAAA: 180.0000",
				"ZZAAA: 10.0000",
				"ZZCCC: 60.0000",
				"ZZAAA: 200.0000",
				"ZZBBB: 50.0000"
				);
		}

		public void TestSpecifiedCharges_UpdateIgnoringPosted_WithPosted()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.UpdatedCharges;
			billing.PostedChargesHandling = Xsd.PostedChargeHandling.Ignore;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 200, },
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 50, },
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");
			AccChargeCode ccc = GetChargeCode(GlbBranch.CurrentBranch, "CCC", "CCC Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 10, 10, true);
			AddCharge(job, ccc, 60, 20);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertJobCharges("Dont Update Charges", job,
				"ZZAAA: 10.0000",
				"ZZCCC: 60.0000",
				"ZZAAA: 200.0000",
				"ZZBBB: 50.0000"
				);
		}

		public void TestSpecifiedCharges_NewCharges_WithSellRatingOverride()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.NewCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				SellRatingOverride = "Y"
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				SellRatingOverride = "N"
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, bbb, 10, 20, false, true);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertSellRatingOverride("Add Charges", job,
				"ZZAAA: N",
				"ZZBBB: Y",
				"ZZAAA: Y",
				"ZZBBB: N"
				);
		}

		public void TestSpecifiedCharges_UpdatedCharges_WithSellRatingOverride()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.UpdatedCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				SellRatingOverride = "Y"
			});

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZBBB",
				SellRatingOverride = "N"
			});

			AccChargeCode aaa = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");
			AccChargeCode bbb = GetChargeCode(GlbBranch.CurrentBranch, "BBB", "BBB Charge");

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			AddCharge(job, aaa, 180, 10);
			AddCharge(job, bbb, 10, 20, false, true);
			Factory.Save();

			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);

			AssertSellRatingOverride("Update Charges", job,
				"ZZAAA: Y",
				"ZZBBB: N"
				);
		}

		public void TestImportDuplicatedCharges()
		{
			JobBranchDefaultOrderRule rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = 1;
			rule.DefaultToBranchOfOrganisation = 0;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			rule.DefaultToLoginUserDefault = 0;
			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			var branch1 = GetBranch("C1", "B1");
			var branch2 = GetBranch("C2", "B2");
			var branch3 = GetBranch("C3", "B3");

			var b1_CC1 = GetChargeCode(branch1, "B1_CC1", "Charge Code B1_1");
			var b1_CC2 = GetChargeCode(branch1, "B1_CC2", "Charge Code B1_2");
			var b1_CC3 = GetChargeCode(branch1, "B1_CC3", "Charge Code B1_3");

			var b2_CC1 = GetChargeCode(branch2, "B2_CC1", "Charge Code B2_1");
			var b2_CC2 = GetChargeCode(branch2, "B2_CC2", "Charge Code B2_2");

			var b3_CC1 = GetChargeCode(branch3, "B3_CC1", "Charge Code B3_1");
			var b3_CC2 = GetChargeCode(branch3, "B3_CC2", "Charge Code B3_2");
			var b3_CC3 = GetChargeCode(branch3, "B3_CC3", "Charge Code B3_3");

			var shipment = Factory.New<CommonShipment>();
			var loader = new Job.Loader(shipment);

			var job = loader.TryCreateWithoutMutexForTestOnly(branch1);
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = branch1.PK;
			AddCharge(job, b1_CC1, 111, 10);
			AddCharge(job, b1_CC2, 121, 20);
			AddCharge(job, b1_CC2, 122, 30);
			AddCharge(job, b1_CC2, 123, 40);

			job = loader.TryCreateWithoutMutexForTestOnly(branch3);
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = branch3.PK;
			AddCharge(job, b3_CC1, 311, 10);
			AddCharge(job, b3_CC2, 321, 20);
			AddCharge(job, b3_CC2, 322, 30, true);
			AddCharge(job, b3_CC2, 323, 40);

			CombineAssertions(delegate
			{
				AssertJobCharges("Precondition: should be a Job for branch1", loader, branch1, new string[]
				{
					FormatValuesToTest(b1_CC1, 111),
					FormatValuesToTest(b1_CC2, 121),
					FormatValuesToTest(b1_CC2, 122),
					FormatValuesToTest(b1_CC2, 123),
				});

				AssertJobCharges("Precondition: should be NO Job for branch2", loader, branch2, null);

				AssertJobCharges("Precondition: should be a Job for branch3", loader, branch3, new string[]
				{
					FormatValuesToTest(b3_CC1, 311),
					FormatValuesToTest(b3_CC2, 321),
					FormatValuesToTest(b3_CC2, 322),
					FormatValuesToTest(b3_CC2, 323),
				});
			});

			var adapter = (IValueObjectDataAdapter)new BillingDataAdapter();

			TestForBranch1();
			TestForBranch2();
			TestForBranch3();

			void TestForBranch1()
			{
				var billing = NewBilling();
				AddBillingLine(billing, branch1, b1_CC2, 1210);
				AddBillingLine(billing, branch1, b1_CC2, 1220);
				AddBillingLine(billing, branch1, b1_CC3, 1300);

				TestImportFromValueObject(billing, "A Job for branch1 has been updated", branch1, new string[]
				{
				FormatValuesToTest(b1_CC2, 1210),
				FormatValuesToTest(b1_CC2, 1220),
				FormatValuesToTest(b1_CC3, 1300),
				});
			}

			void TestForBranch2()
			{
				var billing = NewBilling();
				AddBillingLine(billing, branch2, b2_CC1, 2100);
				AddBillingLine(billing, branch2, b2_CC2, 2200);

				TestImportFromValueObject(billing, "New Job has been created for branch2", branch2, new string[]
				{
					FormatValuesToTest(b2_CC1, 2100),
					FormatValuesToTest(b2_CC2, 2200),
				});
			}

			void TestForBranch3()
			{
				var billing = NewBilling();
				AddBillingLine(billing, branch3, b3_CC2, 3210);
				AddBillingLine(billing, branch3, b3_CC2, 3220);
				AddBillingLine(billing, branch3, b3_CC3, 3300);

				TestImportFromValueObject(billing, "A Job for branch3 has been skipped as it has posted charges", branch3, new string[]
				{
					FormatValuesToTest(b3_CC1, 311),
					FormatValuesToTest(b3_CC2, 321),
					FormatValuesToTest(b3_CC2, 322),
					FormatValuesToTest(b3_CC2, 323),
				});
			}

			void TestImportFromValueObject(Xsd.Billing billing, string message, GlbBranch branch, string[] expect)
			{
				var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
				
				try
				{
					using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
					{
						adapter.ImportFromValueObject(shipment, billing, context);
					}

					CombineAssertions(delegate
					{
						AssertJobCharges(message, loader, branch, expect);
					});

					if (branch.PK == branch3.PK)
					{
						AssertEquals("Should be a warning for a skipped Job",
						"Warning: The Invoicing Job for company C3 was skipped during the Import process as it has posted charges.",
						context.LastNotificationMessage);
					}
				}
				finally
				{
					(adapter as BillingDataAdapter).LastHeaderCreated?.Dispose();
				}
			}
		}

		public void TestDefaultDepartmentChargesNotDeleted()
		{
			GlbDepartment department = Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK);

			var cAFChargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CAF");
			cAFChargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			department.DeptCharges.AddNew().GD_AC = Env.Registry.FreightChargeCode;
			department.DeptCharges.AddNew().GD_AC = Factory.LoadTop1<AccChargeCode>(cAFChargeCodeQuery).PK;
			Factory.Save();

			Xsd.Billing billing = NewBilling();
			Xsd.ChargeLine chargeLine1 = billing.ChargeLines.AddNew();
			chargeLine1.Department = GlbDepartment.CurrentDepartment.GE_Code;
			chargeLine1.ChargeCode = "BAF";
			chargeLine1.OSSellAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)300m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			chargeLine1.OSCostAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode((ZDecimal)500m, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);

			BillingDataAdapter billingAdapter = new BillingDataAdapter();
			AssertNull(billingAdapter.LastHeaderCreated);
			try
			{
				((IValueObjectDataAdapter)billingAdapter).ImportFromValueObject(new JobHeaderParentImplementation(Factory), billing, new ValueObjectImportContext(Factory, new NotificationBuffer()));
				AssertNotNull(billingAdapter.LastHeaderCreated);
				AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, billingAdapter.LastHeaderCreated.Department.GE_Code);

				AssertContainsExactElementsInAnyOrder(
					new ZString[] { "FRT", "CAF", "BAF" },
					billingAdapter.LastHeaderCreated.Charges.ToArray<Charge>().Select(c => c.ChargeCode.AC_Code));
			}
			finally
			{
				billingAdapter.LastHeaderCreated?.Dispose();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[DisableZeroExchangeRateOverriding]
		public void TestFullyPopulatedBizObj()
		{
			string expected = GetTextFromFile(PopulatedBillingFileName);
			Job initial = CreateFullyPopulatedJob();
			string export1 = ExportJob(initial);

			this.AssertXMLEqualsByDiff("First Import", expected, export1);

			Job imported = ImportJob(export1);
			string export2 = ExportJob(imported);

			this.AssertXMLEqualsByDiff("Second Import", expected, export2);
		}

		public void TestErrorIfAmoutSpecifiedAndCurrencyNotSpecified()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "HouseBill";

			BillingDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			Xsd.Billing billingValue = NewBilling();
			Xsd.ChargeLine line = billingValue.ChargeLines.AddNew();
			line.ChargeCode = "FRT";
			line.Description = "Description";
			line.Collect = true;
			line.CollectSpecified = true;
			line.OSSellAmount.CurrencyCode = "USD";
			line.OSSellAmount.Value = 500;
			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billingValue, context);
				AssertEquals("No errors on import", false, context.NotificationsHasErrors);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}
			shipment = Factory.New<CommonShipment>();
			line.OSSellAmount.CurrencyCode = "XXX";
			line.OSSellAmount.Value = 500;
			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billingValue, context);
				AssertEquals("Error on import currency", true, context.NotificationsHasErrors);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}
		}

		public void TestCurrentBranch()
		{
			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			JobBranchDefaultOrderRule rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = 0;
			rule.DefaultToBranchOfOrganisation = 0;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			rule.DefaultToLoginUserDefault = 1;
			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			BillingDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			Xsd.Billing billingValue = NewBilling();
			Xsd.ChargeLine line = billingValue.ChargeLines.AddNew();
			line.ChargeCode = "FRT";
			line.Description = "Random Crap";
			line.OSSellAmount.CurrencyCode = "USD";
			line.OSSellAmount.Value = 1000m;

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "S0001";

			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billingValue, context);
				AssertEquals("JH_GB Should be Current Branch", GlbBranch.CurrentBranch.PK, shipment.Job.JH_GB);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}

			try
			{
				Job shipmentJob = new Job.Loader(shipment).Load();
				shipmentJob.JH_GB = branch2.PK;
				shipmentJob.Charges.RemoveAndDeleteAll();
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billingValue, context);
				JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, shipment.Job.PK));
				AssertEquals("Should have 1 charge", 1, charges.Length);
				AssertEquals("JR_GB should be taken from the job header", branch2.PK, charges[0].JR_GB);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}
		}

		public void TestCurrentDepartment()
		{
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();

			BillingDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			Xsd.Billing billingValue = NewBilling();
			Xsd.ChargeLine line = billingValue.ChargeLines.AddNew();
			line.ChargeCode = "FRT";
			line.Description = "Random Crap";
			line.OSSellAmount.CurrencyCode = "USD";
			line.OSSellAmount.Value = 1000m;

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_HouseBill = "S0001";

			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billingValue, context);
				AssertEquals("JH_GE should be the current department", GlbDepartment.CurrentDepartment.PK, shipment.Job.JH_GE);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}

			JobCharge[] charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, shipment.Job.PK));
			AssertEquals("Should have 1 charge", 1, charges.Length);
			AssertEquals("JR_GE should be the current department", GlbDepartment.CurrentDepartment.PK, charges[0].JR_GE);

			Job shipmentJob = new Job.Loader(shipment).Load();
			shipmentJob.JH_GE = department2.PK;
			shipmentJob.Charges.RemoveAndDeleteAll();
			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billingValue, context);
				charges = Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, shipment.Job.PK));
				AssertEquals("Should have 1 charge", 1, charges.Length);
				AssertEquals("JR_GE should be taken from the job header", department2.PK, charges[0].JR_GE);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}
		}

		public void TestWarningIfWrongBranch()
		{
			var shipment = Factory.New<CommonShipment>();
			var adapter = new BillingDataAdapter();

			Xsd.Billing billing = NewBilling();
			Xsd.ChargeLine line = billing.ChargeLines.AddNew();
			line.ChargeCode = "FRT";
			line.Description = "Description";
			line.OSSellAmount.CurrencyCode = "USD";
			line.OSSellAmount.Value = 500;

			line.Branch = "XXX";
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billing, context);
				AssertEquals("Warning and skip line",
					"Warning: Unable to match the branch 'XXX'",
					context.LastNotificationMessage);
				AssertEquals("Job should NOT be created", null, shipment.Job);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}

			line.Branch = GlbBranch.CurrentBranch.GB_Code;
			context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billing, context);
				AssertEquals("No warnings on import", ZString.Empty, context.LastNotificationMessage);
				AssertEquals("Job should be created", GlbBranch.CurrentBranch.GB_Code, adapter.LastHeaderCreated.Branch.GB_Code);
				AssertEquals("Charge should be created", GlbBranch.CurrentBranch.GB_Code, adapter.LastHeaderCreated.Charges[0].Branch.GB_Code);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}
		}

		public void TestImportChargeWithNonCurrentJobCompany()
		{
			var shipment = Factory.New<CommonShipment>();
			var adapter = new BillingDataAdapter();
			var billing = NewBilling();
			var line = billing.ChargeLines.AddNew();
			line.ChargeCode = "FRT";
			line.Description = "Description";
			line.OSSellAmount.CurrencyCode = "USD";
			line.OSSellAmount.Value = 500;
			var nonCurrentBranch = GetBranch("CAU", "SYY");
			line.Branch = nonCurrentBranch.GB_Code;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				try
				{
					((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billing, context);
					AssertEquals("Error and skip line",
						$"Warning: The imported branch 'SYY' in the <ChargeLine> doesn't belong to the system company '{Env.CurrentCompany.Code}' processing the XML import.\r\nPlease ensure that the correct branch is specified",
						context.LastNotificationMessage);
					var zquery = new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK);
					Assert("Job should NOT be created", !Factory.Exists(typeof(JobHeader), zquery));

					line.Branch = ObjectCreator.NonCurrentBranch.GB_Code;
					context = new ValueObjectImportContext(Factory, new NotificationBuffer());

					((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billing, context);
					AssertEquals("No warnings or errors on import", ZString.Empty, context.LastNotificationMessage);
					AssertEquals("Job should be created", ObjectCreator.NonCurrentBranch.GB_Code, adapter.LastHeaderCreated.Branch.GB_Code);
					AssertEquals("Charge should be created", ObjectCreator.NonCurrentBranch.GB_Code, adapter.LastHeaderCreated.Charges[0].Branch.GB_Code);
				}
				finally
				{
					adapter.LastHeaderCreated?.Dispose();
				}
			}
		}

		public void TestErrorIfJobIsNotCreated()
		{
			var consol = ObjectCreator.CreateConsol();
			var adapter = new BillingDataAdapter();

			var billing = NewBilling();
			var line = billing.ChargeLines.AddNew();
			line.ChargeCode = "FRT";
			line.Description = "Description";
			line.OSSellAmount.CurrencyCode = "USD";
			line.OSSellAmount.Value = 500;

			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			((IValueObjectDataAdapter)adapter).ImportFromValueObject(consol, billing, context);
			AssertEquals("Warning and skip line",
				"Error: Operational job 'C001' does not support creation of invoicing job.",
				context.LastNotificationMessage);
			AssertNull("Job should NOT be created", consol.Job);

			consol = ObjectCreator.CreateGatewayConsol(consolNum: "C002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(consol, billing, context);
				AssertEquals("No warnings on import", ZString.Empty, context.LastNotificationMessage);
				AssertNotNull("Job should be created", consol.Job);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}
		}

		public void TestWarningIfWrongDepartment()
		{
			var shipment = Factory.New<CommonShipment>();
			var adapter = new BillingDataAdapter();

			Xsd.Billing billing = NewBilling();
			Xsd.ChargeLine line = billing.ChargeLines.AddNew();
			line.ChargeCode = "FRT";
			line.Description = "Description";
			line.OSSellAmount.CurrencyCode = "USD";
			line.OSSellAmount.Value = 500;

			line.Department = "XXX";
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billing, context);
				AssertEquals("Warning and skip line",
					"Warning: Unable to match the department 'XXX'",
					context.LastNotificationMessage);
				AssertEquals("Job should NOT be created", null, shipment.Job);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}

			line.Department = GlbDepartment.CurrentDepartment.GE_Code;
			context = new ValueObjectImportContext(Factory, new NotificationBuffer());

			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billing, context);
				AssertEquals("No warnings on import", ZString.Empty, context.LastNotificationMessage);
				AssertEquals("Job should be created", GlbDepartment.CurrentDepartment.GE_Code, adapter.LastHeaderCreated.Department.GE_Code);
				AssertEquals("Charge should be created", GlbDepartment.CurrentDepartment.GE_Code, adapter.LastHeaderCreated.Charges[0].Department.GE_Code);
			}
			finally
			{
				adapter.LastHeaderCreated?.Dispose();
			}
		}

		public void TestExportShipmentWithBillingInfoAndDeclaration()
		{
			bool oldValue = CustomsDataRegistry.Instance.IncludeBillingInformationInXMLFile.Value;
			CustomsDataRegistry.Instance.IncludeBillingInformationInXMLFile.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Job job = CreateFullyPopulatedJob();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = job.PlugInData.PK;

			Xsd.Billing billing = NewBilling();
			BillingDataAdapter adapter = new BillingDataAdapter();
			NotificationBuffer notify = new NotificationBuffer();
			((IValueObjectDataAdapter)adapter).ExportToValueObject(declaration, billing, new ValueObjectExportContext(notify));

			AssertEquals("No errors should occur on export", false, notify.HasErrors);

			CustomsDataRegistry.Instance.IncludeBillingInformationInXMLFile.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, oldValue);
		}

		public void TestLocalClientNotOverridenWhenNotSpecified()
		{
			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.AllCharges;
			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSSellAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 200, },
			});

			var jobHeaderParent = new JobHeaderParentImplementation(Factory);
			Job job = new Job.Loader(jobHeaderParent).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			OrgHeader localClient = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = localClient.PK;

			Factory.Save();

			AssertEquals("Precondition", localClient.PK, job.LocalCharges.PK);
			AssertEquals("Precondition", false, billing.LocalClient.IsSpecified);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			adapter.ImportFromValueObject(jobHeaderParent, billing, context);
			AssertEquals("Local client not specified in xml => not overriden on job", localClient.PK, job.LocalCharges.PK);

			OrgHeader anotherClient = Factory.NewWithValidTestData<OrgHeader>();
			anotherClient.OH_Code = "TRUEBLOOD";
			anotherClient.OH_FullName = "True Blood Pty Ltd";
			anotherClient.MainAddress.OA_Address1 = "Bon Temps, Louisiana";

			Factory.Save();

			Xsd.Organisation xsdLocalClient = new Xsd.Organisation();
			xsdLocalClient.OwnerCode = "TRUEBLOOD";
			xsdLocalClient.OrganisationDetails.Name = "True Blood Pty Ltd";
			xsdLocalClient.OrganisationDetails.Addresses.GetOrCreateMainAddress().AddressLine1 = "Bon Temps, Louisiana";

			billing.LocalClient = xsdLocalClient;

			AssertEquals("Precondition", localClient.PK, job.LocalCharges.PK);
			AssertEquals("Precondition", true, billing.LocalClient.IsSpecified);

			adapter.ImportFromValueObject(jobHeaderParent, billing, context);
			AssertEquals("Local client specified in xml => overriden on job", anotherClient.PK, job.LocalCharges.PK);
		}

		public void TestChargeShouldNotChangeIfApportioned()
		{
			AccChargeCode chargeCode = GetChargeCode(GlbBranch.CurrentBranch, "AAA", "AAA Charge");

			var consol = ObjectCreator.CreateConsol();
			ApportionmentListing apps = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = apps.CostsCollection.TryAddNew();
			cost.E6_AC_ChargeCode = chargeCode.PK;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_OSCostAmount = 180M;

			CommonShipment shipment = Factory.New<CommonShipment>();
			Job job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			var charge = AddCharge(job, chargeCode, 180, 10);
			charge.JR_E6 = cost.PK;
			charge.JR_OH_CostAccount = Guid.Empty;
			Factory.Save();

			Xsd.Billing billing = NewBilling();
			billing.SpecifiedCharges = Xsd.ChargesSpecified.UpdatedCharges;

			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSCostAmount = new Xsd.FinancialValue() { CurrencyCode = "AUD", Value = 0 },
			});
			IValueObjectDataAdapter adapter = new BillingDataAdapter();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			adapter.ImportFromValueObject(shipment, billing, context);
			Factory.Save();
			AssertEquals("Error: Validation Warning (Unable to modify the cost currency 'AUD' or the cost amount '0' when charge apportioned)", context.LastNotificationMessage);

			billing.ChargeLines.Clear();
			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				OSCostAmount = new Xsd.FinancialValue() { CurrencyCode = "CNY", Value = 180 },
			});
			adapter.ImportFromValueObject(shipment, billing, context);
			Factory.Save();
			AssertEquals("Error: Validation Warning (Unable to modify the cost currency 'CNY' or the cost amount '180' when charge apportioned)", context.LastNotificationMessage);

			billing.ChargeLines.Clear();
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.OrganisationDetails.Name = "splaty";
			billing.ChargeLines.Add(new Xsd.ChargeLine()
			{
				ChargeCode = "ZZAAA",
				Creditor = organisation,
			});
			adapter.ImportFromValueObject(shipment, billing, context);
			Factory.Save();
			AssertEquals("Error: Validation Warning (Unable to modify the cost account 'splaty' when charge apportioned)", context.LastNotificationMessage);
		}

		#region Implementation

		class JobHeaderParentImplementation : NonPersistentBusinessObject, IJobHeaderParent
		{
			public JobHeaderParentImplementation(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			string IJobNumber.JobNumber
			{
				get { return "JOB"; }
			}

			void IJobHeaderParent.OnJobCreating(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobCreated(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleting(JobHeader job)
			{
			}

			void IJobHeaderParent.OnJobDeleted(JobHeader job)
			{
			}

			void IJobHeaderParent.SetJobNumberFieldOnSaving()
			{
			}

			bool IJobHeaderParent.AllowInvoiceDeletion
			{
				get { return true; }
			}
		}

		GlbBranch GetBranch(ZString companyCode, ZString branchCode)
		{
			var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode)
						  ?? ObjectCreator.CreateNewCompany(companyCode);
			var branch = ObjectCreator.CreateBranch(branchCode, company);
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			Factory.Save();

			return branch;
		}

		AccChargeCode GetChargeCode(GlbBranch branch, ZString code, ZString description)
		{
			var chargeCode = ObjectCreator.CreateChargeCode(code, description, Core.Constants.ChargeType.Disbursement, 100, ObjectCreator.GST1, ObjectCreator.WHT1);
			chargeCode.AC_GC = branch.Company.PK;

			return chargeCode;
		}

		Charge AddCharge(Job job, AccChargeCode accChargeCode, ZDecimal osSellAmount, ZDecimal osCostAmount, bool isPosted = false, bool sellRatingOverride = false)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = accChargeCode.PK;
			charge.JR_OSCostAmt = osCostAmount;
			charge.JR_OSSellAmt = osSellAmount;
			charge.JR_SellRatingOverride = sellRatingOverride;

			if (isPosted)
			{
				PostCharge(charge);
			}

			return charge;
		}

		void PostCharge(Charge charge)
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = TransactionTypes.Invoice;
			header.AH_JH = charge.JR_JH;
			header.AH_GC = charge.Branch.GB_GC;
			header.AH_GB = charge.JR_GB;
			header.AH_GE = charge.JR_GE;
			header.AH_InvoiceDate = ZDateTime.Today;

			AccTransactionLines revLine = Factory.New<AccTransactionLines>();
			revLine.AL_AH = header.PK;
			revLine.AL_GB = charge.JR_GB;
			revLine.AL_GE = charge.JR_GE;
			revLine.AL_LineType = TransactionLineTypes.Revenue;
			revLine.AL_RX_NKTransactionCurrency = charge.JR_RX_NKSellCurrency;
			revLine.AL_LineAmount = charge.JR_LocalSellAmt;
			revLine.AL_OSAmount = charge.JR_OSSellAmt;
			charge.JR_AL_ARLine = revLine.PK;
			revLine.AL_OSAmount = revLine.AL_LineAmount = charge.JR_OSSellAmt;
			revLine.AL_AG = ObjectCreator.GLHeader1.PK;

			AssertEquals(true, charge.IsRevenuePosted);
		}

		Xsd.ChargeLine AddBillingLine(Xsd.Billing xsdBilling, GlbBranch branch, AccChargeCode accChargeCode, ZDecimal osSellAmount)
		{
			var xsdLine = xsdBilling.ChargeLines.AddNew();
			xsdLine.ChargeCode = accChargeCode.AC_Code;
			xsdLine.OSSellAmount = Xsd.FinancialValue.FromAmountAndCurrencyCode(osSellAmount, "AUD");
			xsdLine.Branch = (branch != null) ? branch.GB_Code : ZString.Empty;

			return xsdLine;
		}

		string FormatValuesToTest(AccChargeCode accChargeCode, ZDecimal osSellAmount)
		{
			return string.Format("{0}: {1:N4}", accChargeCode.AC_Code, osSellAmount);
		}

		string FormatValuesToTestForCurrency(AccChargeCode accChargeCode, ZString osCurrency)
		{
			return string.Format("{0}: {1}", accChargeCode.AC_Code, osCurrency);
		}

		string FormatValuesToTestForSellRatingOverride(AccChargeCode accChargeCode, ZBool sellRatingOverride)
		{
			return string.Format("{0}: {1}", accChargeCode.AC_Code, sellRatingOverride);
		}

		void AssertJobCharges(ZString message, Job.Loader loader, GlbBranch branch, params string[] expected)
		{
			AssertJobCharges(message, loader.Load(false, branch.Company), expected);
		}

		void AssertJobCharges(ZString message, Job job, params string[] expected)
		{
			if (job == null)
			{
				AssertEquals(message, expected, null);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(message, expected, job.Charges.ToArray<Charge>().Select(c => FormatValuesToTest(c.ChargeCode, c.JR_OSSellAmt)));
			}
		}

		void AssertSellRatingOverride(ZString message, Job job, params string[] expected)
		{
			if (job == null)
			{
				AssertEquals(message, expected, null);
			}
			else
			{
				AssertContainsExactElementsInAnyOrder(message, expected, job.Charges.ToArray<Charge>().Select(c => FormatValuesToTestForSellRatingOverride(c.ChargeCode, c.JR_SellRatingOverride)));
			}
		}

		protected virtual Xsd.Billing NewBilling()
		{
			return new Xsd.Billing();
		}

		protected Job CreateFullyPopulatedJob()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();

			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_JX = voyage.Sailings[0].PK;

			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.Parent = shipment;
			job.FillWithValidTestData(TestBusinessObjectKind.All & ~TestBusinessObjectKind.PopulateAllDependentAndRelatedObjectsDeeply, Array.Empty<PropertyDescriptor>());
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_Status = JobHeaderStatus.Working.Code;
			job.JH_Direction = string.Empty;

			ChargeCodeCC1.AC_Code = "CC1";
			ChargeCodeCC3.AC_Code = "CC3";
			ChargeCodeCC4.AC_Code = "CC4";

			OrgHeader debtor1 = Factory.NewWithValidTestData<OrgHeader>();
			debtor1.OH_Code = "DEBT1";
			debtor1.OH_FullName = "debtor1";
			OrgHeader creditor1 = Factory.NewWithValidTestData<OrgHeader>();
			creditor1.OH_Code = "CRED1";
			creditor1.OH_FullName = "creditor1";
			OrgHeader creditor2 = Factory.NewWithValidTestData<OrgHeader>();
			creditor2.OH_Code = "CRED2";
			creditor2.OH_FullName = "creditor2";

			job.JH_OA_LocalChargesAddr = debtor1.Addresses.MainAddress.PK;
			Factory.Save();

			Charge charge1 = job.Charges.AddNew();
			charge1.JR_AC = ChargeCodeCC1.PK;
			charge1.JR_RX_NKSellCurrency = "AUD";
			charge1.JR_OSSellAmt = 500;
			charge1.JR_RX_NKCostCurrency = "USD";
			charge1.JR_OSCostAmt = 200;
			charge1.JR_OH_SellAccount = debtor1.PK;
			charge1.JR_OH_CostAccount = creditor1.PK;
			charge1.JR_InvoiceType = AgencyInvoiceTypesList.Codes.LocalCollect;

			Charge charge2 = job.Charges.AddNew();
			charge2.JR_AC = ChargeCodeCC3.PK;
			charge2.JR_RX_NKSellCurrency = "USD";
			charge2.JR_OSSellAmt = 1000;
			charge2.JR_RX_NKCostCurrency = "USD";
			charge2.JR_OSCostAmt = 800;
			charge2.JR_OH_SellAccount = debtor1.PK;
			charge2.JR_OH_CostAccount = creditor2.PK;
			charge2.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;

			Charge charge3 = job.Charges.AddNew();
			charge3.JR_AC = ChargeCodeCC4.PK;
			charge3.JR_RX_NKSellCurrency = "SGD";
			charge3.JR_OSSellAmt = 750;
			charge3.JR_RX_NKCostCurrency = "AUD";
			charge3.JR_OSCostAmt = 300;
			charge3.JR_InvoiceType = AgencyInvoiceTypesList.Codes.ForeignCollect;

			return job;
		}

		protected virtual string ExportJob(Job job)
		{
			Xsd.Billing billing = NewBilling();
			BillingDataAdapter adapter = new BillingDataAdapter();
			adapter.Export(job, billing, new ValueObjectExportContext(new NotificationBuffer()));

			using (StringWriter stream = new StringWriter())
			using (XmlTextWriter writer = new XmlTextWriter(stream))
			{
				writer.Formatting = Formatting.Indented;

				XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(typeof(Xsd.Billing));
				serialiser.Serialize(writer, billing);

				writer.Flush();

				return stream.ToString();
			}
		}

		protected virtual Job ImportJob(string xml)
		{
			Xsd.Billing billing = NewBilling();

			using (StringReader stream = new StringReader(xml))
			using (XmlTextReader reader = new XmlTextReader(stream))
			{
				XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(typeof(Xsd.Billing));
				billing = (Xsd.Billing)serialiser.Deserialize(reader);
			}

			AgencyShipment shipment = Factory.New<AgencyShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "SGSIN";

			BillingDataAdapter adapter = new BillingDataAdapter();

			try
			{
				((IValueObjectDataAdapter)adapter).ImportFromValueObject(shipment, billing, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			}
			finally
			{
				adapter.LastHeaderCreated.Dispose();
			}
			return (Job)shipment.Job;
		}

		protected string[] ChargesAsStringArray(Job job)
		{
			return Array.ConvertAll(
				job.Charges.ToArray<Charge>(),
				(c) => string.Format(
					"Charge Code: {0}\r\nInvoice Type: {1}",
					c.ChargeCode == null ? "<null>" : c.ChargeCode.AC_Code.ToString(),
					c.JR_InvoiceType)
				);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void SetUp()
		{
			base.SetUp();
			ExchangeRateReader.GetReaderInstance().ClearCache();

			new AccountingPeriodTestHelper().PostPeriodsForEntireYear(PostDate.Year, GlbCompany.CurrentCompany.PK);

			originalUserContext = Env.CurrentUserContext;

			GlbStaff newStaffUser = Factory.NewWithValidTestData<GlbStaff>();
			newStaffUser.GS_LoginName = "NewStaffUser";
			newStaffUser.GS_FullName = "New Staff User";
			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = newStaffUser.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";
			var security2 = Factory.NewWithValidTestData<GlbSecurity>();
			security2.GU_GS = newStaffUser.PK;
			security2.GU_SecurityItemIsAllowed = true;
			security2.GU_SecurityRight = "SpecializedRights";
			var security3 = Factory.NewWithValidTestData<GlbSecurity>();
			security3.GU_GS = newStaffUser.PK;
			security3.GU_SecurityItemIsAllowed = true;
			security3.GU_SecurityRight = "DocumentsReports";
			var security4 = Factory.NewWithValidTestData<GlbSecurity>();
			security4.GU_GS = newStaffUser.PK;
			security4.GU_SecurityItemIsAllowed = true;
			security4.GU_SecurityRight = "Notes";
			Factory.Save();
			Env.SetUserContext(new UserContext("NewStaffUser", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void TearDown()
		{
			Env.SetUserContext(originalUserContext);
			ExchangeRateReader.GetReaderInstance().ClearCache();
			base.TearDown();
		}

		protected ZDateTime PostDate
		{
			get { return new ZDateTime(2005, 01, 01, 10, 30, 0); }
		}

		protected AccChargeCode ChargeCodeCC1
		{
			get
			{
				if (chargeCodeCC1 == null)
				{
					chargeCodeCC1 = ObjectCreator.CC1;

					chargeCodeCC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
					chargeCodeCC1.AC_ChargeSubGroup = "SUB";

					chargeCodeCC1.AC_AG_RevenueAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.10")).PK;
					chargeCodeCC1.AC_AG_WIPAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.20")).PK;
					chargeCodeCC1.AC_AG_AccrualAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.10")).PK;
					chargeCodeCC1.AC_AG_CostAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.20")).PK;

					AccGroups salesGroup1 = Factory.NewWithValidTestData<AccGroups>();
					salesGroup1.AR_Code = "SAL";
					salesGroup1.AR_Desc = "Sales Group 1";
					chargeCodeCC1.AC_AR_SalesGroup = salesGroup1.PK;

					AccGroups expenseGroup1 = Factory.NewWithValidTestData<AccGroups>();
					expenseGroup1.AR_Code = "EXP";
					expenseGroup1.AR_Desc = "Expense Group 1";
					chargeCodeCC1.AC_AR_ExpenseGroup = expenseGroup1.PK;

					Factory.Save();
				}

				return chargeCodeCC1;
			}
		}
		AccChargeCode chargeCodeCC1;

		protected AccChargeCode ChargeCodeCC3
		{
			get
			{
				if (chargeCodeCC3 == null)
				{
					chargeCodeCC3 = ObjectCreator.CC3;

					chargeCodeCC3.AC_AG_RevenueAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.10")).PK;
					chargeCodeCC3.AC_AG_WIPAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.20")).PK;
					chargeCodeCC3.AC_AG_AccrualAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.10")).PK;
					chargeCodeCC3.AC_AG_CostAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.20")).PK;

					Factory.Save();
				}

				return chargeCodeCC3;
			}
		}
		AccChargeCode chargeCodeCC3;

		protected AccChargeCode ChargeCodeCC4
		{
			get
			{
				if (chargeCodeCC4 == null)
				{
					chargeCodeCC4 = ObjectCreator.CC4;

					chargeCodeCC4.AC_AG_RevenueAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.10")).PK;
					chargeCodeCC4.AC_AG_WIPAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.10.20")).PK;
					chargeCodeCC4.AC_AG_AccrualAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.10")).PK;
					chargeCodeCC4.AC_AG_CostAccount = Factory.LoadFromUniqueKey(typeof(AccGLHeader), AccGLHeaderSchema.AG_AccountNum, new ZString("1010.20.20")).PK;

					Factory.Save();
				}

				return chargeCodeCC4;
			}
		}
		AccChargeCode chargeCodeCC4;

		protected TestObjectCreator ObjectCreator
		{
			get
			{
				if (objectCreator == null)
				{
					objectCreator = new TestObjectCreator(Factory);
				}
				return objectCreator;
			}
		}
		TestObjectCreator objectCreator;

		IUserContext originalUserContext;

		protected string GetTextFromFile(string filename)
		{
			using (TextReader stream = new StreamReader(filename))
			{
				return stream.ReadToEnd();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected string BaseTestFilePath
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\"; }
		}

		protected virtual string PopulatedBillingFileName
		{
			get { return BaseBillingTestPath + "PopulatedBilling.xml"; }
		}

		protected string BaseBillingTestPath
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\Billing\Testing\"; }
		}

		protected void SetExchangeRate(Job job, string currencyCode, decimal rate)
		{
			ExchangeRate found = null;

			foreach (ExchangeRate exRate in job.ExchangeRates)
			{
				if (exRate.JF_RX_NKRateCurrency == currencyCode)
				{
					found = exRate;
					found.JF_BaseRate = rate;
				}
			}

			if (found == null)
			{
				found = job.ExchangeRates.AddNew();
				found.JF_RX_NKRateCurrency = currencyCode;
				found.JF_BaseRate = rate;
			}
		}
		protected void SetExchangeRate(JobVoyage voyage, string currencyCode, decimal rate)
		{
			VoyageExRate found = null;

			foreach (VoyageExRate exRate in voyage.ExRates)
			{
				if (exRate.E8_RX_NKExCurrency == currencyCode)
				{
					found = exRate;
					break;
				}
			}

			if (found == null)
			{
				found = voyage.ExRates.AddNew();
				found.E8_RX_NKExCurrency = currencyCode;
			}

			found.E8_VoyageExchangeRate = rate;
		}

		#endregion
	}
}
