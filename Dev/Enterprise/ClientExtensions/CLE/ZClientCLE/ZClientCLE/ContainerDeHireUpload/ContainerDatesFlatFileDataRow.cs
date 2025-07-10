using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.CLE
{
	internal class ContainerDatesFlatFileDataRow : FlatFileDataRow
	{
		public ContainerDatesFlatFileDataRow(string line)
			: base(new OCsvLine(line).FieldValues)
		{
		}

		public class Schema
		{
			public const int DeliveryDate = 0;
			public const int ContainerNumber = 2;
			public const int DeHireDate = 4;
			public const int CLEReference = 8;
		}

		public ZString DeliveryDateString
		{
			get { return GetField(Schema.DeliveryDate); }
		}

		public ZString ContainerNumber
		{
			get { return GetField(Schema.ContainerNumber); }
		}

		public ZString DeHireDateString
		{
			get { return GetField(Schema.DeHireDate); }
		}

		public ZString CLEReference
		{
			get { return GetField(Schema.CLEReference); }
		}
	}
}
