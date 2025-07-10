using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TGE.Business
{
	internal class CSSExporter : FlatFileDataExporter
	{
		public CSSExporter(BusinessObjectFactory factory) : base(factory)
		{
		}

		public void Export(IList<BusinessObject> bizoList, INotifications notifications)
		{
			this.bizoList = bizoList;
			this.notifications = notifications;

			ZString tempFile = ExportToFile();
			if (!File.Exists(ExportedFile))
			{
				if (IsValidToDeliver())
				{
					ExportedFile = DeliverFile(tempFile);
					if (IsExportOK)
					{
						SetDataExportEvent();
						notifications.Notify(new InfoNotification(ZString.Format("Export File {0} has been created.", ExportedFile)));
						SaveFactory();
					}

					ExportedFile = "";
				}
				else if (File.Exists(tempFile))
				{
					File.Delete(tempFile);
				}
			}
		}

		public override ZString EnglishDescription
		{
			get { return englishDescription; }
		}
		const string englishDescription = "TGE Air Cargo Data Exporter";

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return flatFileFormat ?? (flatFileFormat = new CSSPipeDelimitedFileFormat()); }
		}
		CSSPipeDelimitedFileFormat flatFileFormat;

		protected override IValueObjectDataAdapter DataAdapter
		{
			get { throw new NotSupportedException(); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			throw new NotImplementedException();
		}

		protected override void PopulateExportInstructions(ExportInstructions instructions, IFlatFileConverter converter)
		{
			instructions.BasePath = TGEDataRegistry.Instance.CSSExportDirectory;
			instructions.SpecifiedFilename = GetNewFilenameNumber();
			instructions.UseUpperCaseFileExtension = true;
		}

		ZString ExportToFile()
		{
			fileSequenceNumber = TGECSSFountain.PeekPreliminaryFormatted(Db.Connection);
			ImportConverter.FileSequenceNumber = fileSequenceNumber;
			ExportConverter.FileSequenceNumber = fileSequenceNumber;
			string tempFileName = Env.GetTempFileName();
			try
			{
				using (TextWriter writer = new StreamWriter(tempFileName, AppendToFile))
				{
					int i = 0;
					writer.WriteLine("<BOF>");
					foreach (BusinessObject bizObj in bizoList)
					{
						ExportToFileCore(bizObj, i, writer);
						i++;
					}
					writer.WriteLine("<EOF>");
					writer.Flush();
					writer.Close();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				TryDeleteFile(tempFileName);
				throw;
			}
			return tempFileName;
		}

		void ExportToFileCore(BusinessObject bizObj, int i, TextWriter writer)
		{
			OnProgressChanged(i, bizoList.Count);

			if (bizObj.GetType() == typeof(CusHAWB))
			{
				ImportConverter.ExportFlatFile(bizObj, FlatFileFormat, writer);
				SetIsExportOK(!ImportConverter.HasErrors);
			}
			else if (bizObj.GetType() == typeof(JobDeclaration) || bizObj.GetType() == typeof(ForwardingShipment))
			{
				ExportConverter.ExportFlatFile(bizObj, FlatFileFormat, writer);
				SetIsExportOK(!ExportConverter.HasErrors);
			}
		}
		ZString fileSequenceNumber;

		bool IsValidToDeliver()
		{
			return IsExportOK;
		}

		ZString DeliverFile(ZString tempFile)
		{
			ZString result = base.DeliverFile(tempFile, notifications);
			RenewInstructions = true;
			return result;
		}

		void TryDeleteFile(string tempFileName)
		{
			base.TryDeleteFile(tempFileName, notifications);
		}

		void SetDataExportEvent()
		{
			foreach (BusinessObject bizObj in bizoList)
			{
				if (bizObj.GetType() == typeof(CusHAWB))
				{
					CusHAWB cusHawb = bizObj as CusHAWB;
					cusHawb.Logs.AddNew(Events.DataExport, String.Format(DEXEventReferenceFormat, "Export", cusHawb.CS_CustomsStatus));
				}
				else if (bizObj.GetType() == typeof(JobDeclaration))
				{
					JobDeclaration jobDec = bizObj as JobDeclaration;
					jobDec.Logs.AddNew(Events.DataExport, String.Format(DEXEventReferenceFormat, "Export", "CES_" + jobDec.JE_EntryStatus));
				}
				else if (bizObj.GetType() == typeof(ForwardingShipment))
				{
					ForwardingShipment shipment = bizObj as ForwardingShipment;
					shipment.Logs.AddNew(Events.DataExport, String.Format(DEXEventReferenceFormat, "Export", shipment.CustomsEntryNumberType + " " + shipment.CustomsEntryNumber));
				}
			}
		}
		internal const string DEXEventReferenceFormat = "TGE - {0} of {1} Status Event";

		INumberFountainProxy TGECSSFountain
		{
			get { return new FormattedNumberFountainFactory("TGECSS").New(); }
		}

		string GetNewFilenameNumber()
		{
			using (var manager = Db.Connection.BeginTransactionWithManager())
			{
				var result = FilenamePrefix + TGECSSFountain.GetNextFormatted(Db.Connection) + FilenameSuffix;
				manager.CommitTransaction();
				return result;
			}
		}

		string FilenamePrefix
		{
			get { return Path.Combine(TGEDataRegistry.Instance.CSSExportDirectory, filenamePrefix); }
		}
		const string filenamePrefix = "Cargowise_AU_";

		string FilenameSuffix
		{
			get { return "." + CSSPipeDelimitedFileFormat.CSSFileExtension; }
		}

		CSSAirImportConverter ImportConverter
		{
			get { return importConverter ?? (importConverter = new CSSAirImportConverter(notifications, Factory)); }
		}
		CSSAirImportConverter importConverter;

		CSSAirExportConverter ExportConverter
		{
			get { return exportConverter ?? (exportConverter = new CSSAirExportConverter(notifications, Factory)); }
		}
		CSSAirExportConverter exportConverter;

		IList<BusinessObject> bizoList;
		INotifications notifications;
	}
}
