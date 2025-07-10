using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.Export
{
	class DeclarationInvoiceFlatFileDataExporter : FlatFileDataExporter
	{
		public DeclarationInvoiceFlatFileDataExporter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Override

		protected override IValueObjectDataAdapter DataAdapter
		{
			get { return DeclarationValueObjectDataAdapter.New(); }
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new DeclarationInvoiceExportFlatFileFormat(); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new DeclarationInvoiceExportFlatFileConverter(notificationSubscriber, new BusinessObjectFactory());
		}

		protected override void PopulateExportInstructions(ExportInstructions instructions, IFlatFileConverter converter)
		{
			instructions.MethodOfExport = ExportType.File;
			instructions.SpecifiedFilename = "krkipo_" + ZDateTime.Now.ToString("ddMMyyyyHHmm");
			instructions.BasePath = Path.GetFullPath(WowDataRegistry.Instance.DeclarationInvoiceExportDirectory);
		}

		public override ZString EnglishDescription
		{
			get { return "WoolWorths Declaration Invoice Automated Data Export"; }
		}

		protected override ZString DeliverFile(ZString tempFile, INotifications notifications)
		{
			var outPutFile = base.DeliverFile(tempFile, notifications);
			var bodyLines = Contents(outPutFile);
			if (bodyLines.Count == 0)
			{
				try
				{
					File.Delete(outPutFile);
				}
				catch (IOException) { }

				notifications.Notify(new InfoNotification(Constants.NothingToExport));
				return ZString.Empty;
			}
			else
			{
				outPutFile = AddHeaderToOutputFile(outPutFile, bodyLines);
				return outPutFile;
			}
		}

		#endregion

		#region Implementation

		List<String> Contents(ZString outPutFile)
		{
			List<string> contents = new List<string>();
			if (!outPutFile.IsEmpty && File.Exists(outPutFile))
			{
				using (StreamReader sr = new StreamReader(outPutFile))
				{
					string? line;
					while ((line = sr.ReadLine()) != null)
					{
						contents.Add(line);
					}
				}
			}

			return contents;
		}

		ZString AddHeaderToOutputFile(ZString outPutFile, List<String> contents)
		{
			using (StreamWriter sw = new StreamWriter(outPutFile))
			{
				sw.WriteLine(CreateHeaderRecord(contents.Count));
				foreach (string s in contents)
				{
					sw.WriteLine(s);
				}
			}

			return outPutFile;
		}

		string CreateHeaderRecord(ZInt numberOfRecords)
		{
			string s = String.Format(numberOfRecords.ToString("D8"));
			return "0krkipo             " + ZDate.Today.ToDateTime().ToString("ddMMyyyy") + s;
		}

		#endregion
	}
}
