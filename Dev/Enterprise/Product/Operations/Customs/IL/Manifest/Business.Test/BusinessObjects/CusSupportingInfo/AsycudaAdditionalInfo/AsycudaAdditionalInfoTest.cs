using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaAdditionalInfo))]
	sealed class AsycudaAdditionalInfoTest : CusSupportingInfoTest<AsycudaAdditionalInfo>
	{
		public void TestReferenceNumberFieldType()
		{
			var addInfo = Factory.New<AsycudaAdditionalInfo>();

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.UNLOCO;
			AssertEquals(nameof(FieldType.TextCodeFindBox), addInfo.ReferenceNumberFieldType);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.ExporterTypeID;
			AssertEquals(nameof(FieldType.TextDropEdit), addInfo.ReferenceNumberFieldType);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.NightStop;
			AssertEquals(nameof(FieldType.TextDropEdit), addInfo.ReferenceNumberFieldType);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.IsCooling;
			AssertEquals(nameof(FieldType.TextDropEdit), addInfo.ReferenceNumberFieldType);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.MultipleDeals;
			AssertEquals(nameof(FieldType.TextDropEdit), addInfo.ReferenceNumberFieldType);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.CargoType;
			AssertEquals(nameof(FieldType.TextDropEdit), addInfo.ReferenceNumberFieldType);

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.ActionCode;
			AssertEquals(nameof(FieldType.TextDropEdit), addInfo.ReferenceNumberFieldType);

			addInfo.CSI_Code = "";
			AssertEquals(nameof(FieldType.Text), addInfo.ReferenceNumberFieldType);
		}

		public void TestDescriptionFieldType()
		{
			var addInfo = Factory.New<AsycudaAdditionalInfo>();

			addInfo.CSI_Code = Constants.AsycudaAdditionalInfoCodes.IsDirectDelivery;
			AssertEquals(nameof(FieldType.TextDropEdit), addInfo.DescriptionFieldType);

			addInfo.CSI_Code = "";
			AssertEquals(nameof(FieldType.Text), addInfo.DescriptionFieldType);
		}

		public void TestSetDefaultValues()
		{
			var addInfo = Factory.New<AsycudaAdditionalInfo>();
			var type = addInfo.CSI_Type;
			AssertEquals("OTH", type);
			var subType = addInfo.CSI_SubType;
			AssertEquals("INF", subType);
		}

		public void TestCaptions()
		{
			AssertCaptions("CSI_Code", "Statement Type");
			AssertCaptions("CSI_ReferenceNumber", "Statement Code");
			AssertCaptions("CSI_Description", "Content");
		}

		public void TestLookups()
		{
			var additionalInfo = Factory.New<AsycudaAdditionalInfo>();
			AssertEquals(GetLookupsType, additionalInfo.Lookups.GetType());
		}

		public Type GetLookupsType => typeof(AsycudaAdditionalInfoLookups);

		public void TestValidation()
		{
			var additionalInfo = Factory.New<AsycudaAdditionalInfo>();
			AssertEquals("Validation", typeof(AsycudaAdditionalInfoValidation), additionalInfo.Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return bill.AdditionalInfos.AddNew();
		}

		protected override IEnumerable<AsycudaAdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var additionalInfo = bill.AdditionalInfos.AddNew();
			Factory.Save();
			yield return additionalInfo;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = "785";
			bill = header.Bills.AddNew();
			disposableAction = ILCustomsDataRegistry.Instance.ILEnableILManifest.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "ALL");
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposableAction?.Dispose();
		}

		void AssertCaptions(string propertyName, string caption)
		{
			AssertEquals($"{propertyName} Caption", caption, DataBoundResourceStrings.GetDataForProperty(typeof(AsycudaAdditionalInfo), propertyName).Caption);
		}

		IDisposable disposableAction;
		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
