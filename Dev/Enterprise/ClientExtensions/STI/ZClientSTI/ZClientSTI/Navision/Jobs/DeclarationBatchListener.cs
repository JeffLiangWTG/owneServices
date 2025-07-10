using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.STI.Navision
{
	public class DeclarationBatchListener : JobBatchListener
	{
		public DeclarationBatchListener(ZDateTime dateTimeExportStarted) : base(dateTimeExportStarted)
		{
		}

		protected override Type BusinessObjectCollectionType
		{
			get { return typeof(BaseJobDeclarationCollection); }
		}

		public override string BusinessObjectTableName
		{
			get { return JobDeclarationSchema.Constants.TableName; }
		}

		public override Type BusinessObjectType
		{
			get { return typeof(BaseJobDeclaration); }
		}

		protected override NavisionFlatFileExporter Exporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new DeclarationFlatFileExporter(Factory, Instructions, FileName, ZString.Empty);
				}
				return fExporter;
			}
		}
		NavisionFlatFileExporter fExporter;
	}
}
