using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CUSPCSSplitCusTempStorageLine))]
	public class CUSPCSSplitCusTempStorageLineTest : CusTempStorageLineTest<CUSPCSSplitCusTempStorageLine>
	{
		protected override CUSPCSSplitCusTempStorageLine GetNewCusTempStorageLine(BusinessObjectFactory factory)
		{
			var customer = factory.NewWithValidTestData<OrgHeader>();
			customer.OH_Code = "CUSTEST";
			var presenter = factory.NewWithValidTestData<OrgAddress>();
			var representative = factory.NewWithValidTestData<OrgAddress>();

			var storageJobHeader = factory.New<CusTempStorageJobHeader>();
			storageJobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			storageJobHeader.SJH_JobReference = "DECUSPRL001";
			storageJobHeader.SJH_OH_Customer = customer.PK;
			storageJobHeader.SJH_OA_Presenter = presenter.PK;
			storageJobHeader.SJH_OA_Representative = representative.PK;

			var storageDecs = storageJobHeader.CUSPCSCusTempStorageDecs.AddNew();
			storageDecs.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			storageDecs.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationCargo;

			return storageDecs.ConsolidatedCusTempStorageLine.CusTempStorageLinesTo.AddNew();
		}

		protected override Type GetDecType() => typeof(CUSPCSCusTempStorageDec);

		protected override Type GetLookupType() => typeof(CUSPCSSplitCusTempStorageLineLookups);

		protected override Type GetValidationType() => typeof(CUSPCSSplitCusTempStorageLineValidation);
	}
}
