using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondHeader : Customs.Business.BaseCusInBondHeader, Integration.Customs.AsycudaCustoms.ICusInBondHeader
	{
		public CusInBondHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type MovementHeaderTypeCore => typeof(CusInBondMoveHeader);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BH_ApplicationCode = AsycudaEntryInstructionType;
		}

		public const string AsycudaEntryInstructionType = "AEI";
	}
}
