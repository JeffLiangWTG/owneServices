using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Client.Wow
{
	public class OrdersCsvImportFile : CsvImportFileFromMI
	{
		public OrdersCsvImportFile(BusinessObjectFactoryProvider factoryProvider, StreamReader reader)
			: base(factoryProvider, reader)
		{
		}

		public override CsvRecord TryNewRecord(string line)
		{
			CsvRecord result = base.TryNewRecord(line);
			if (result == null)
			{
				string[] fieldValues = new OCsvLine(line).FieldValues;
				switch (fieldValues[0])
				{
					case "1":
						result = new OrderCsvRecord(line);
						break;
					case "2":
						result = new OrderLineCsvRecord(line);
						break;
					case "3":
						result = new DeliveryCsvRecord(line);
						break;
				}
			}
			return result;
		}
	}
}
