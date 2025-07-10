using System;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Client.Wow
{
	public class MITrailerCsvRecord : CsvRecord
	{
		internal MITrailerCsvRecord(string line) : base(line, -1)
		{
			try
			{
				NumberOfRecords = int.Parse(FieldValues[1]);
			}
			catch (FormatException)
			{
				NumberOfRecords = -1;
			}
		}

		public readonly int NumberOfRecords;

		public override string DisplayIdentifier
		{
			get { return "Trailer"; }
		}

		public override bool SupportsUpdateBusinessData()
		{
			return false;
		}

		protected override void OnUpdateBusinessData(BusinessObjectFactoryProvider factoryProvider, INotifications notify)
		{
			throw new NotSupportedException();
		}
	}
}
