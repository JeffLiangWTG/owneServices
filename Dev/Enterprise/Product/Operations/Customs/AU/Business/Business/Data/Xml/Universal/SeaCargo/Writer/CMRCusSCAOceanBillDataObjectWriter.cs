using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class CMRCusSCAOceanBillDataObjectWriter : CusSCAOceanBillDataObjectWriter<CusSCAOceanBill, CusSCAHouse, CusSCAContainer, CusSCAPivot>
	{
		public CMRCusSCAOceanBillDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override CusSCAHouseDataObjectWriter<CusSCAHouse, CusSCAPivot> GetNewCusSCAHouseDataObjectWriter()
		{
			return new CMRCusSCAHouseDataObjectWriter(writeManager);
		}

		protected override CusSCAContainerDataObjectWriter<CusSCAContainer> GetNewCusSCAContainerDataObjectWriter()
		{
			return new CusSCAContainerDataObjectWriter(writeManager);
		}

		protected override bool ShouldKeepExistingData(CusSCAOceanBill oceanBill)
		{
			return !HasRecipientRole(RecipientRoleType.HSA);
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(CusSCAOceanBill sourceBO)
		{
			return sourceBO.GetUserDefinedValues();
		}
	}
}
