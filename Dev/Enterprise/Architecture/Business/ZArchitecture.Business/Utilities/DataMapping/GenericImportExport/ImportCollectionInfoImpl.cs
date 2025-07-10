using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ImportCollectionInfoImpl : IImportCollectionInfo, IExportCollectionInfo, IEnumerable
	{
		public ImportCollectionInfoImpl(IBusinessObjectCollection collection)
		{
			this.collection = collection;
		}

		public void Add(IImportPropertyInfo property)
		{
			properties.Add(property);
		}

		IBusinessObjectCollection IImportCollectionInfo.Collection
		{
			get { return collection; }
		}

		IEnumerable<BusinessObject> IExportCollectionInfo.BusinessObjects
		{
			get { return collection.Cast<BusinessObject>(); }
		}

		IEnumerable<IImportPropertyInfo> IImportCollectionInfo.Properties
		{
			get { return properties; }
		}

		IEnumerable<RowType> IExportCollectionInfo.RowTypes
		{
			get
			{
				yield return new RowType((NoResString)"", typeof(object), properties);
			}
		}

		BusinessObjectFactory IExportCollectionInfo.Factory
		{
			get { return collection.Factory; }
		}

		public bool ValidateAndSave { get; protected set; }

		public EventHandler Started;
		void IImportCollectionInfo.OnImportStarted()
		{
			if (Started != null)
			{
				Started(this, EventArgs.Empty);
			}
		}

		public EventHandler<ImportCompletedEventArgs> Completed;
		void IImportCollectionInfo.OnImportCompleted(bool success)
		{
			if (Completed != null)
			{
				Completed(this, new ImportCompletedEventArgs(success));
			}
		}

		readonly IBusinessObjectCollection collection;
		readonly ImportPropertyInfoCollection properties = new ImportPropertyInfoCollection();

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)properties).GetEnumerator();
		}

		#endregion
	}

	public class ImportCompletedEventArgs : EventArgs
	{
		public ImportCompletedEventArgs(bool success)
		{
			Success = success;
		}

		public bool Success { get; private set; }
	}
}
