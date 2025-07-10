using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.IFC.ConsolExport
{
	public class ConsolXmlExporter
	{
		public ConsolXmlExporter()
		{
		}

		public void Export(BusinessObject @object, INotifications notify)
		{
			if (@object != null)
			{
				var consolExportFactory = new BusinessObjectFactory();
				consolExportFactory.RefreshEnabled = false;

				var consol = consolExportFactory.Load<ForwardingConsol>(@object.PK);
				var consolCollection = new ForwardingConsol[] { consol };

				var fileName = GetFileName(consol.JK_UniqueConsignRef);
				using (var toFile = new FileStream(fileName, FileMode.Create))
				{
					ExportSerlialisation(toFile, consolCollection, notify);
				}

				if (notify != null)
				{
					notify.Notify(new BatchNotification("Exported Consol " + consol.JK_MasterBillNum + " to file " + fileName));
				}

				consol.Factory.Save();
				consolExportFactory.CleanUp();
				consolExportFactory = null;
#if DEBUG
				ExportedFileNameForTesting = fileName;
#endif
			}
		}

		void ExportSerlialisation(Stream file, ForwardingConsol[] selectedBusinessObjects, INotifications notify)
		{
			XmlValueObjectSerializer serialiser = new XmlValueObjectSerializer(ConsolAdapter.ValueObjectType);
			serialiser.ExportXmlData(file, ConsolAdapter, selectedBusinessObjects, new ValueObjectExportContext(notify));
		}

		ForwardingConsolValueObjectDataAdapter ConsolAdapter
		{
			get
			{
				if (fConsolAdapter == null)
				{
					fConsolAdapter = new ForwardingConsolValueObjectDataAdapter();
				}
				return fConsolAdapter;
			}
		}

		ForwardingConsolValueObjectDataAdapter fConsolAdapter;

		ZString GetFileName(ZString consolNumber)
		{
			ZString result = "";
			if (!consolNumber.IsEmpty)
			{
				result = ExportDirectory + consolNumber + ZDateTime.Now.ToString("ddMMyyyyhhmm") + ".xml";
			}

			return result;
		}

		ZString ExportDirectory
		{
			get
			{
				string directory = IFCDataRegistry.Instance.FSCExportDirectory;
				return directory.EndsWith(@"\") ? directory : directory + @"\";
			}
		}

#if DEBUG
		public ZString ExportedFileNameForTesting;
#endif
	}
}
