using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyCMRDepotMessageLine : ICMRDepotMessageLine
	{
		public DummyCMRDepotMessageLine(DummyCMRDepotMessage depotMessage)
		{
			this.parent = depotMessage;
			depotMessage.lines.Add(this);
		}

		#region ICMRDepotMessageLine Members

		ZInt ICMRDepotMessageLine.NumberOfPackages
		{
			get { return numberOfPackages; }
		}
		public ZInt numberOfPackages;

		ZString ICMRDepotMessageLine.PackageType
		{
			get { return packageType; }
		}
		public ZString packageType;

		ZString ICMRDepotMessageLine.ContainerNumber
		{
			get { return containerNumber; }
		}
		public ZString containerNumber;

		ZString ICMRDepotMessageLine.HouseBillNumber
		{
			get { return houseBillNumber; }
		}
		public ZString houseBillNumber;

		ZString ICMRDepotMessageLine.OceanBillNumber
		{
			get { return oceanBillNumber; }
		}
		public ZString oceanBillNumber;

		ZString ICMRDepotMessageLine.ContainerMode
		{
			get { return containerMode; }
		}
		public ZString containerMode;

		ZString ICMRDepotMessageLine.MarksAndNumbers
		{
			get { return marksAndNumbers; }
		}
		public ZString marksAndNumbers;

		ZString ICMRDepotMessageLine.GoodsDescription
		{
			get { return goodsDescription; }
		}
		public ZString goodsDescription;

		ICMRDepotMessage ICMRDepotMessageLine.Parent
		{
			get { return parent; }
		}
		public ICMRDepotMessage parent;

		ZDecimal ICMRDepotMessageLine.ActualWeight
		{
			get { return ZDecimal.Zero; }
		}

		ZString ICMRDepotMessageLine.WeightUnits
		{
			get { return ZString.Empty; }
		}

		ZDecimal ICMRDepotMessageLine.ActualVolume
		{
			get { return ZDecimal.Zero; }
		}

		ZString ICMRDepotMessageLine.VolumeUnits
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}
