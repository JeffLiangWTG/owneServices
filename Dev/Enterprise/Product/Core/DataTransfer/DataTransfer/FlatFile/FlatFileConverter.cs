using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.DataTransfer.Business
{
	public abstract class FlatFileConverter<TValueObject> : IFlatFileConverter, ITransferDataConsumer
		where TValueObject : IValueObject
	{
		protected FlatFileConverter(INotifications notification, BusinessObjectFactory factory)
		{
			this.Notification = notification;
			this.Factory = factory;
		}

		#region Export

		public void ExportFlatFile(TValueObject valueObject, IFlatFileFormat flatFileFormat, TextWriter writer)
		{
			if (writer == null)
			{
				throw new ArgumentNullException(nameof(writer));
			}
			CheckArguments(valueObject, flatFileFormat);
			FlatFileDataRowCollection flatFileRows = new FlatFileDataRowCollection();
			Gather(flatFileRows, valueObject, flatFileFormat);
			Write(flatFileRows, writer, flatFileFormat);
		}

		void Gather(FlatFileDataRowCollection flatFileRows, TValueObject valueObject, IFlatFileFormat fileFormat)
		{
			if (((ITransferDataConsumer)this).DataCollector.BOF)
			{
				CreateHeader(flatFileRows);
			}

			flatFileRows.Add(MapExport(valueObject));

			if (((ITransferDataConsumer)this).DataCollector.EOF)
			{
				CreateFooter(flatFileRows);
			}
		}

		protected void Write(FlatFileDataRowCollection flatFileRows, TextWriter flatFileWriter, IFlatFileFormat fileFormat)
		{
			foreach (FlatFileDataRow rowObject in flatFileRows)
			{
				string flatFileLineString = FormatLine(rowObject, fileFormat);
				flatFileWriter.WriteLine(flatFileLineString);
			}
			flatFileWriter.Flush();
		}

		/// <summary>
		/// Maps the data from a value object XSD to a flat file
		/// </summary>
		protected virtual FlatFileDataRowCollection MapExport(TValueObject valueObject)
		{
			return new FlatFileDataRowCollection();
		}

		protected virtual string FormatLine(FlatFileDataRow row, IFlatFileFormat fileFormat)
		{
			return fileFormat.ConvertToLine(row);
		}

		protected virtual void CreateHeader(FlatFileDataRowCollection document)
		{
		}

		protected virtual void CreateFooter(FlatFileDataRowCollection document)
		{
		}

		#endregion

		#region Import

		public virtual void ImportFlatFile(TValueObject valueObject, IFlatFileFormat flatFileFormat, TextReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException(nameof(reader));
			}
			CheckArguments(valueObject, flatFileFormat);
			ImportFlatFileCore(valueObject, flatFileFormat, reader);
		}

		protected virtual void ImportFlatFileCore(TValueObject valueObject, IFlatFileFormat fileFormat, TextReader flatFileReader)
		{
			FlatFileDataRowCollection fileLines = new FlatFileDataRowCollection();
			string dataLine;
			while ((dataLine = flatFileReader.ReadLine()) != null)
			{
				FlatFileDataRow flatFileLine = ExtractRowFromFormat(fileFormat, dataLine);
				if (flatFileLine != null)
				{
					fileLines.Add(flatFileLine);
				}
			}

			MapImport(valueObject, fileLines);
		}

		protected void ImportFlatFile(TValueObject valueObject, IFlatFileFormat fileFormat, TextReader flatFileReader, ZInt bytesToRead)
		{
			FlatFileDataRowCollection fileLines = new FlatFileDataRowCollection();
			if (bytesToRead > 0)
			{
				char[] buffer = null;
				while (flatFileReader.Peek() >= 0)
				{
					buffer = new char[bytesToRead];
					if (flatFileReader.Read(buffer, 0, buffer.Length) != 0)
					{
						FlatFileDataRow flatFileLine = ExtractRowFromFormat(fileFormat, new string(buffer));
						if (flatFileLine != null)
						{
							fileLines.Add(flatFileLine);
						}
					}
				}
				MapImport(valueObject, fileLines);
			}
			else
			{
				ImportFlatFileCore(valueObject, fileFormat, flatFileReader);
			}
		}

		protected virtual FlatFileDataRow ExtractRowFromFormat(IFlatFileFormat fileFormat, ZString rawRow)
		{
			return fileFormat.ConvertToRow(rawRow);
		}

		/// <summary>
		/// Maps the data from a flat file into the given ValueObject XSD.
		/// </summary>
		protected virtual void MapImport(TValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
		}

		#endregion

		#region ITransferDataConsumer Members

		ITransferDataCollector ITransferDataConsumer.DataCollector
		{
			get
			{
				if (fDataCollector == null)
				{
					fDataCollector = (ITransferDataCollector)Activator.CreateInstance(DataCollectorType);
				}
				return fDataCollector;
			}
		}
		ITransferDataCollector fDataCollector;

		void ITransferDataConsumer.ResetDataCollector()
		{
			fDataCollector = null;
		}

		#endregion

		#region IFlatFileConverter Members

		void IFlatFileConverter.ExportFlatFile(IValueObject valueObject, IFlatFileFormat fileFormat, TextWriter flatFileWriter)
		{
			CheckValueObject(valueObject);
			this.ExportFlatFile((TValueObject)valueObject, fileFormat, flatFileWriter);
		}

		void IFlatFileConverter.ImportFlatFile(IValueObject valueObject, IFlatFileFormat fileFormat, TextReader flatFileReader)
		{
			CheckValueObject(valueObject);
			this.ImportFlatFile((TValueObject)valueObject, fileFormat, flatFileReader);
		}

		void CheckValueObject(IValueObject valueObject)
		{
			if (valueObject == null)
			{
				throw new ArgumentNullException(nameof(valueObject));
			}

			if (!(valueObject is TValueObject))
			{
				throw new ArgumentException("ValueObject must be of type " + typeof(TValueObject).FullName, nameof(valueObject));
			}
		}

		#endregion

		#region Implementation

		protected virtual Type DataCollectorType
		{
			get { return typeof(TransferDataCollector); }
		}

		#region CheckArguments

		protected virtual void CheckArguments(TValueObject valueObject, IFlatFileFormat flatFileFormat)
		{
			if (valueObject == null)
			{
				throw new ArgumentNullException(nameof(valueObject));
			}

			if (flatFileFormat == null)
			{
				throw new ArgumentNullException(nameof(flatFileFormat));
			}
		}

		#endregion

		protected readonly BusinessObjectFactory Factory;
		protected readonly INotifications Notification;

		#endregion
	}

	/// <summary>
	/// Please use the generic FlatFileConverter instead of this class.
	/// </summary>
	public abstract class FlatFileConverter : FlatFileConverter<IValueObject>
	{
		protected FlatFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}
	}
}
