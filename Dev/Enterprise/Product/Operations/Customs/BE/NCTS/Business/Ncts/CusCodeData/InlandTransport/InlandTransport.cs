using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

[SingleObjectAroundARow]
[DependentBusinessObject(typeof(NctsBill), nameof(NctsBill.InlandTransports))]
public class InlandTransport : CusCodeData, IShortSequenceNumberLine
{
	public InlandTransport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public override ZShort CY_Order
	{
		get => base.CY_Order;
		set
		{
			var oldValue = CY_Order;

			base.CY_Order = value;

			if (!IsCopying && Parent != null)
			{
				Parent.InlandTransportLineNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
			}
		}
	}

	public new NctsBill Parent => (NctsBill)base.Parent;

	protected override CusCodeDataValidation GetNewValidation() => new InlandTransportValidation(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		CY_Type = Constants.CusCodeDataTypes.TransportInland;
	}

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(NctsBill));

	ZShort ISequenceNumberLine<ZShort>.SequenceNumber
	{
		get => CY_Order;
		set => CY_Order = value;
	}
	ZGuid ISequenceNumberLine.FKToHeader => CY_ParentID;
}
