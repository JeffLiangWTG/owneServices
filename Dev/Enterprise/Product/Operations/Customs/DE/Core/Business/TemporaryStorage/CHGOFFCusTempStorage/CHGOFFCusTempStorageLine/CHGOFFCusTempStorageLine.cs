using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CUSPRLCusTempStorageDec), "CusTempStorageLines")]
	public class CHGOFFCusTempStorageLine : CusTempStorageLine
	{
		public CHGOFFCusTempStorageLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Dec

		public new CHGOFFCusTempStorageDec Dec => (CHGOFFCusTempStorageDec)base.Dec;

		#endregion

		#region Properties

		[ResourceStringData("89B9E047-13D6-40F6-A4EB-2AD3033DA2A0", Caption = "New Entitled Trader Address")]
		public override ZGuid TSL_OA_GoodsOwner { get => base.TSL_OA_GoodsOwner; set => base.TSL_OA_GoodsOwner = value; }

		[ResourceStringData("9D29210F-9D3A-4D7D-819E-DCC72C4B76DB", Caption = "New Entitled Trader EORI")]
		public override ZString TSL_GoodsOwnerIdentifier { get => base.TSL_GoodsOwnerIdentifier; set => base.TSL_GoodsOwnerIdentifier = value; }

		[ResourceStringData("A52C6EBE-387C-4785-AB64-5D6226BEC5BF", Caption = "New Entitled Trader Branch")]
		public override ZString TSL_GoodsOwnerIdentifierBranchNo { get => base.TSL_GoodsOwnerIdentifierBranchNo; set => base.TSL_GoodsOwnerIdentifierBranchNo = value; }

		protected override bool ReadOnlyTSL_LineNo => false;

		protected override bool SequenceNumberEnabledCore => false;

		#endregion

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new CHGOFFCusTempStorageLineLookups(this);

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new CHGOFFCusTempStorageLineValidation(this);

		#endregion

	}
}
