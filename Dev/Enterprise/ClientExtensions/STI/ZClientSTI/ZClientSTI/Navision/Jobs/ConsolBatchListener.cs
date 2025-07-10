using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.STI.Navision
{
	public class ConsolBatchListener : JobBatchListener
	{
		public ConsolBatchListener(ZDateTime dateTimeExportStarted) : base(dateTimeExportStarted)
		{
		}

		protected override Type BusinessObjectCollectionType
		{
			get { return typeof(MainFormConsolCollection); }
		}

		public override string BusinessObjectTableName
		{
			get { return JobConsolSchema.Constants.TableName; }
		}

		public override Type BusinessObjectType
		{
			get { return typeof(CommonConsol); }
		}

		protected override NavisionFlatFileExporter Exporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new ConsolFlatFileExporter(Factory, Instructions, FileName, ZString.Empty);
				}
				return fExporter;
			}
		}
		NavisionFlatFileExporter fExporter;
	}
}
