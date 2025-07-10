using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAContainerUnderbondMovementRequestHeader : CusSCAOceanBillUnderbondMovementRequestHeader
	{
		public CusSCAContainerUnderbondMovementRequestHeader(CusUnderbond underbond, CusSCAContainer container)
			: base(underbond, container.OceanBill)
		{
			this.container = container;
		}

		public override ZString TranshipmentOverseasDestinationPort
		{
			get
			{
				return underbond.C4_MovementReason == CMRUnderbondRequestCodes.Codes.Transshipment ?
					underbond.C4_RL_NKTranshipDestPort : ZString.Empty;
			}
		}

		public override IUnderbondMovementRequestLine Line
		{
			get
			{
				if (container.IsBreakBulk || container.IsBulk)
				{
					return new CusSCABulkUnderbondMovementRequestLine(container);
				}
				else
				{
					return new CusSCAContainerUnderbondMovementRequestLine(container);
				}
			}
		}

		public override ZString PackageType
		{
			get { return container.IsBreakBulk || container.IsBulk ? ZString.Empty : base.PackageType; }
		}

		public override ZInt NumberOfPackages
		{
			get { return container.IsBreakBulk || container.IsBulk ? ZInt.Zero : base.NumberOfPackages; }
		}

		#region Implementantion

		readonly CusSCAContainer container;

		#endregion
	}
}
