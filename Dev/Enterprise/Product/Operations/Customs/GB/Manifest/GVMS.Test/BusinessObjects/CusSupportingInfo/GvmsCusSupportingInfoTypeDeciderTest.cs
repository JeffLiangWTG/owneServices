using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	internal class GvmsCusSupportingInfoTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new GvmsCusSupportingInfoTypeDecider();
			AssertNull(typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new GvmsCusSupportingInfoTypeDecider();
			AssertNull(typeDecider.GetTypeForNew());
		}

		public void TestGetTypeForLoad()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;
			header.Validation.ValidateAMA_JobReference();
			var newFactory = new BusinessObjectFactory();

			var testData = new List<(string code, Type expectedType)>
			{
				(GVMSCustomsReference.Codes.AtaCarnet, typeof(GvmsCustomsReference)),
				(GVMSCustomsReference.Codes.CdsMovementReferenceNumberMrn, typeof(GvmsCustomsReference)),
				(GVMSCustomsReference.Codes.CdsExportDeclarationUniqueConsignmentReferenceDucr, typeof(GvmsCustomsReference)),
				(GVMSCustomsReference.Codes.ChiefImportEntryReferenceNumber, typeof(GvmsCustomsReference)),
				(GVMSCustomsReference.Codes.EntryInDeclarantsRecord, typeof(GvmsEidrReference)),
				(GVMSCustomsReference.Codes.ExemptGoods, typeof(GvmsCustomsReference)),
				(GVMSCustomsReference.Codes.ImportControlSystemEntrySummaryDeclaration, typeof(GvmsCustomsReference)),
				(GVMSCustomsReference.Codes.IndirectExportDeclarationEad, typeof(GvmsCustomsReference)),
				(GVMSCustomsReference.Codes.NctsOrCtcTransitMovementReferenceNumber, typeof(GvmsTransitReference)),
				(GVMSCustomsReference.Codes.OralDeclaration, typeof(GvmsEidrReference)),
				(GVMSCustomsReference.Codes.SSReferenceForEmptyVehicle, typeof(GvmsCustomsReference)),
				(GVMSCustomsReference.Codes.TirCarnet, typeof(GvmsCustomsReference)),
			};

			foreach (var (code, expectedType) in testData)
			{
				var row = Factory.New<Customs.Business.CusSupportingInfo>();
				row.CSI_ParentID = header.PK;
				row.CSI_ParentTableCode = header.TablePrefix;
				row.CSI_Type = GVMSManifestType.Codes.GoodsVehicleMovementSystemGvms;
				row.CSI_Code = code;
				Factory.Save();

				var loaded = newFactory.Load<Customs.Business.CusSupportingInfo>(row.PK);
				AssertNotNull(loaded);
				AssertEquals(expectedType, loaded.GetType());
			}
		}
	}
}
