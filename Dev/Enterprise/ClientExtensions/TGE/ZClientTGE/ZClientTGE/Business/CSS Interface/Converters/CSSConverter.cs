using System;
using System.IO;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TGE.Business
{
	internal abstract class CSSConverter : FlatFileConverter
	{
		public CSSConverter(INotifications notification, BusinessObjectFactory factory) : base(notification, factory) { }

		internal void ExportFlatFile(BusinessObject bizObj, IFlatFileFormat flatFileFormat, TextWriter writer)
		{
			HasErrors = false;
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			if (bizObj == null)
			{
				throw new ArgumentNullException(nameof(bizObj));
			}
			if (flatFileFormat == null)
			{
				throw new ArgumentNullException(nameof(flatFileFormat));
			}

			FlatFileDataRowCollection flatFileRows = new FlatFileDataRowCollection();
			flatFileRows.Add(MapExport(bizObj));
			Write(flatFileRows, writer, flatFileFormat);
		}

		internal
		FlatFileDataRowCollection MapExport(BusinessObject bizObj)
		{
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();

			try
			{
				BusinessObject castedBizo = CastBusinessObject(bizObj);
				CSSMapper mapper = GetMapper();
				mapper.FileSequenceNumber = FileSequenceNumber;
				CusHAWB cusHawb = bizObj as CusHAWB;
				if (mapper != null && castedBizo != null)
				{
					dataRows = mapper.Map(castedBizo);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Notification.Notify(new ErrorNotification(ErrorType.Error, "Error(s) occurred while exporting Shipment - " + ex.Message));
				hasErrors = true;
			}
			return dataRows;
		}

		protected abstract BusinessObject CastBusinessObject(BusinessObject bizObj);

		protected abstract CSSMapper GetMapper();

		public bool HasErrors
		{
			get { return hasErrors; }
			set { hasErrors = value; }
		}
		protected bool hasErrors;

		internal ZString FileSequenceNumber { get; set; }
	}
}
