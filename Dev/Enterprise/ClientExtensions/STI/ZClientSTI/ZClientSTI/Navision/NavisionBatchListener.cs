using System;
using System.Globalization;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.BatchProcessor;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.STI.Navision
{
	public abstract class NavisionBatchListener : LogBatchListener
	{
		protected NavisionBatchListener(ZDateTime dateTimeExportStarted)
		{
			this.DateTimeExportStarted = dateTimeExportStarted;
			this.Factory = new BusinessObjectFactory();
		}

		protected virtual NavisionFlatFileExporter Exporter
		{
			get { return null; }
		}

		protected abstract Type BusinessObjectCollectionType { get; }
		protected abstract ZString ExportDirectory { get; }
		protected abstract ZString FileNamePreFix { get; }

		public override string HumanReadableName
		{
			get { return "Navision Batch Exporter"; }
		}

		protected override bool AdditionalMatching(StmALog log)
		{
			return ((log.SL_SE_NKEvent == Events.AddedARecordToTheSystem.Code || log.SL_SE_NKEvent == Events.EditedARecord.Code) &&
					NoDEXEventsHaveAnEventTimeGreaterThanTheCurrentLogsEventTime(log));
		}

		bool NoDEXEventsHaveAnEventTimeGreaterThanTheCurrentLogsEventTime(StmALog log)
		{
			ZQuery query = new ZQuery(StmALogSchema.SL_Parent, log.SL_Parent);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DataExport.Code);
			query.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.GreaterThan, log.SL_EventTime);
			return !Factory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(typeof(StmALog)), query);
		}

		protected override void Process(BusinessObject matchingBusinessObject, StmALog log, INotifications notifications)
		{
			BusinessObjectCollection collection = (BusinessObjectCollection)Activator.CreateInstance(BusinessObjectCollectionType, Factory);
			collection.Add(matchingBusinessObject);
			CollectionWrapperBusinessObjectReader businessObjectReader = new CollectionWrapperBusinessObjectReader(collection);
			Exporter.Export(businessObjectReader, notifications);
		}

		protected ExportInstructions Instructions
		{
			get
			{
				if (fInstructions == null)
				{
					fInstructions = new ExportInstructions();
					fInstructions.BasePath = ExportDirectory;
					fInstructions.FileExtension = FileExtensionType.Csv;
					fInstructions.MethodOfExport = ExportType.File;
					fInstructions.SpecifiedFilename = FileName;
				}
				return fInstructions;
			}
		}
		ExportInstructions fInstructions;

		protected ZString FileName
		{
			get { return Path.Combine(ExportDirectory, FileNamePreFix + DateTimeExportStarted.ToString(Constants.DateTimeFileNameFormat, CultureInfo.CurrentCulture) + ".csv"); }
		}

		readonly ZDateTime DateTimeExportStarted;
	}
}
