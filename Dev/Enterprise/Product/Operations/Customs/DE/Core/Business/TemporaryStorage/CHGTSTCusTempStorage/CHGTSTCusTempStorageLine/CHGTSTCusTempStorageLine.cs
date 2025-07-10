using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CUSPRLCusTempStorageDec), "CusTempStorageLines")]
	public class CHGTSTCusTempStorageLine : CusTempStorageLine
	{
		public CHGTSTCusTempStorageLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Dec

		public new CHGTSTCusTempStorageDec Dec => (CHGTSTCusTempStorageDec)base.Dec;

		#endregion

		#region Properties

		[ResourceStringData("7EFF29FD-34C6-4ADB-8D18-44370A2F971B", Caption = "New Goods Location", ShortCaption = "New Goods Loc.")]
		public override ZString TSL_LocationOfGoods { get => base.TSL_LocationOfGoods; set => base.TSL_LocationOfGoods = value; }

		protected override bool ReadOnlyTSL_LineNo => false;

		#endregion

		#region Sequence Number Generator

		protected override bool SequenceNumberEnabledCore => false;

		#endregion

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new CHGTSTCusTempStorageLineLookups(this);

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation() => new CHGTSTCusTempStorageLineValidation(this);

		#endregion
	}
}
