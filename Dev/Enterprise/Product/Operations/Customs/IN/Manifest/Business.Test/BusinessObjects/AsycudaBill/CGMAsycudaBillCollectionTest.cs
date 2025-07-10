using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaBillCollection))]
sealed class CGMAsycudaBillCollectionTest : BusinessObjectCollectionTestCase
{
	protected override Type GetExpectedCollectionType()
	{
		return typeof(CGMAsycudaBillCollection);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		return header.Bills;
	}

	public void TestSetDefaultsForNewChildInAir()
	{
		Header.AMA_TransportMode = ZString.Empty;

		CombineAssertions(() =>
		{
			AssertEquals("ABL_ManifestUQ when not Air", ZString.Empty, Bill.ABL_ManifestUQ);
			AssertEquals("ABL_GrossWeightUQ when not Air", ZString.Empty, Bill.ABL_GrossWeightUQ);
			AssertEquals("ABL_VolumeUQ when not Air", ZString.Empty, Bill.ABL_VolumeUQ);
			AssertEquals("ABL_SpecialCargoCode when not Air", ZString.Empty, Bill.ABL_SpecialCargoCode);

			Header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			bill = Header.Bills.AddNew();
			AssertEquals("ABL_ManifestUQ when Air", Core.Constants.PkgUnit.Package, bill.ABL_ManifestUQ);
			AssertEquals("ABL_GrossWeightUQ when Air", Core.Constants.Weight.Kilograms, bill.ABL_GrossWeightUQ);
			AssertEquals("ABL_VolumeUQ when Air", Core.Constants.Volume.CubicMetres, bill.ABL_VolumeUQ);
			AssertEquals("ABL_SpecialCargoCode when Air", ShipmentTypeList.Codes.Total, Bill.ABL_SpecialCargoCode);
		});
	}

	public void TestSetDefaultsValueFromHeaderForNewChild()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.AMA_TransportMode = ZString.Empty;
		header.AMA_ManifestType = ZString.Empty;
		header.AMA_RL_NKOrigin = "TEST";
		header.AMA_RL_NKFinalDestination = "TEST";
		var bill = header.Bills.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("ABL_RL_NKOrigin when not Air", ZString.Empty, bill.ABL_RL_NKOrigin);
			AssertEquals("ABL_RL_NKFinalDestination when not Air", ZString.Empty, bill.ABL_RL_NKFinalDestination);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			bill = header.Bills.AddNew();
			AssertEquals("ABL_RL_NKOrigin when Air", "TEST", bill.ABL_RL_NKOrigin);
			AssertEquals("ABL_RL_NKFinalDestination when Air", "TEST", bill.ABL_RL_NKFinalDestination);

			header.AMA_ParentTableCode = JobConsolSchema.Constants.Prefix;
			bill = header.Bills.AddNew();
			AssertEquals("ABL_RL_NKOrigin when Parent is Consol and not override", ZString.Empty, bill.ABL_RL_NKOrigin);
			AssertEquals("ABL_RL_NKFinalDestination when Parent is Consol and not override", ZString.Empty, bill.ABL_RL_NKFinalDestination);

			header.AMA_OverrideFreightDefaults = true;
			bill = header.Bills.AddNew();
			AssertEquals("ABL_RL_NKOrigin when Parent is Consol and override", "TEST", bill.ABL_RL_NKOrigin);
			AssertEquals("ABL_RL_NKFinalDestination when Parent is Consol and override", "TEST", bill.ABL_RL_NKFinalDestination);
		});
	}

	public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
	{
		return new List<ZString>() { AsycudaBill.Schema.ABL_SpecialCargoCode };
	}

	public void TestSetDefaultsForNewChild_BillStatus()
	{
		CombineAssertions(() =>
		{
			AssertEquals("MessageStatus and RegistrationStatus empty", BillActionList.Codes.Fresh, Bill.ABL_BillStatus);

			Header.AMA_MessageStatus = IN.Business.MessageStatusList.Codes.MessageAccepted;
			bill = Header.Bills.AddNew();
			AssertEquals("MessageStatus 'ACC' and RegistrationStatus empty", ZString.Empty, bill.ABL_BillStatus);

			Header.RegistrationStatus = RegistrationStatusList.Codes.ManifestRegistered;
			bill = Header.Bills.AddNew();
			AssertEquals("MessageStatus 'ACC' and RegistrationStatus 'REG'", BillActionList.Codes.Supplementary, bill.ABL_BillStatus);

			Header.AMA_MessageStatus = ZString.Empty;
			bill = Header.Bills.AddNew();
			AssertEquals("MessageStatus empty and RegistrationStatus 'REG'", ZString.Empty, bill.ABL_BillStatus);

			Header.AMA_MessageStatus = "ABC";
			Header.RegistrationStatus = "XYZ";
			bill = Header.Bills.AddNew();
			AssertEquals("MessageStatus 'XYZ' and RegistrationStatus 'ABC", ZString.Empty, bill.ABL_BillStatus);
		});
	}

	CGMAsycudaBill Bill => bill ??= Header.Bills.AddNew();
	CGMAsycudaBill bill;

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;
}
