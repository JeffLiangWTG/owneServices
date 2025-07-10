using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Wow
{
	public class ProductsCsvImportFile : CsvImportFileFromMI
	{
		public ProductsCsvImportFile(BusinessObjectFactoryProvider factoryProvider, StreamReader reader)
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
						result = new ProductCsvRecord(line);
						break;
				}
			}
			return result;
		}

		#region Implementation

		protected override void GenerateHints(IEnumerable<string> lines)
		{
			foreach (string line in lines)
			{
				ProductCsvRecord record = null;
				try
				{
					record = TryNewRecord(line) as ProductCsvRecord;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}

				if (record != null)
				{
					FactoryProvider.Current.AddFetchHint(OrgSupplierPartSchema.OP_PartNum, new ZString(record.ProductNo));
				}
			}
		}

		protected override int? MaximumRecordsPerFactory
		{
			get { return 500; }
		}

		#endregion
	}
}
