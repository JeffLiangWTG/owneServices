using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public abstract class REXDISCusTempStorageLine : CusTempStorageLine
	{
		protected REXDISCusTempStorageLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region SequenceNumber

		protected override bool SequenceNumberEnabledCore => false;

		protected override bool SetLineNumOnSettingTSL_STHEnabled => false;

		#endregion

		#region Dec

		public new REXDISCusTempStorageDec Dec => Factory.Load<REXDISCusTempStorageDec>(TSL_STH);

		#endregion
	}
}
