using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDAManifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestShowBDEGM()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			AssertEquals("Not BD country", false, header.ShowExportGeneralManifest);
			header.AMA_RN_NKCountry = "BD";
			AssertEquals("Not export manifest", false, header.ShowExportGeneralManifest);
			header.AMA_Nature = "EXP";
			AssertEquals("BD country and export manifest", true, header.ShowExportGeneralManifest);
		}

		public void TestBills()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(header.Bills);
		}

		public void TestGetArrivalHeaderType()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(typeof(AsycudaArrivalHeader), header.GetArrivalHeaderType());
		}

		public void TestAMA_MessageStatus_ReadOnly()
		{
			AssertEquals(false, ((AsycudaManifestHeader)GetNewBusinessObject()).AMA_MessageStatusInfo.ReadOnly);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = (AsycudaManifestHeader)GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestTransportMeansType()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<TransportMeanCollection>(header.TransportMeans);
			var iTransportParent = header as ITransportParent;
			AssertNotNull(iTransportParent);
			AssertType<TransportMeanCollection>(iTransportParent.Transports);
		}

		public void TestShowTransportMeansTab()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertEquals(false, header.ShowTransportMeansTab);
		}

		public void TestITransportParentTypeCode()
		{
			var header = (ITransportParent)GetNewBusinessObject();
			AssertEquals("ASY", header.TypeCode);
		}

		public void TestITransportParentTransportSupporter()
		{
			var header = (ITransportParent)GetNewBusinessObject();
			AssertNotNull(header.TransportSupporter);
			AssertType<AsycudaManifestHeaderTransportSupporter>(header.TransportSupporter);
		}

		public void TestIsImport()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			Assert(header.IsImport);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			Assert(!header.IsImport);
		}

		public void TestIsExport()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			Assert(!header.IsExport);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			Assert(header.IsExport);
		}

		public void TestEnableBillsLock_OnlyWhenSupportBillLockIsTrue()
		{
			var headerSupportBillLock = Factory.New<AsycudaManifestHeaderForTest>();
			headerSupportBillLock.SetSupportBillLock(true);

			headerSupportBillLock.EnableBillsLock(true);
			Assert(headerSupportBillLock.IsBillLockEnabled);

			var headerNotSupportBillLock = Factory.New<AsycudaManifestHeaderForTest>();
			headerNotSupportBillLock.SetSupportBillLock(false);

			headerNotSupportBillLock.EnableBillsLock(true);
			Assert(!headerNotSupportBillLock.IsBillLockEnabled);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "C1234";
			return header;
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);

		class AsycudaManifestHeaderForTest : AsycudaManifestHeader
		{
			public AsycudaManifestHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void SetSupportBillLock(bool supportBillLock)
			{
				this.supportBillLock = supportBillLock;
			}

			bool supportBillLock { get; set; }

			protected override bool SupportBillLock() => supportBillLock;

			protected override AutologState AutoLoggingState => AutologState.NotLogged;
		}
	}
}
