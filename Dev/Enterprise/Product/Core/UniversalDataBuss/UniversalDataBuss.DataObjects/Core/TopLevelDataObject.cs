using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public abstract partial class TopLevelDataObject : ITopLevelDataObject, IDisposable
	{
		public abstract IDataContextDataObject DataContext { get; set; }

		#region IDisposable Support

		bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				return;
			}

			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void SetWriterStrategy(IDataObjectWriterStrategy newStrategy)
		{
			writerStrategy = newStrategy;
		}
		protected IDataObjectWriterStrategy writerStrategy;

		public bool IsAllowSet(string propertyName)
		{
			return writerStrategy.IsAllowSet(propertyName);
		}

		#endregion
	}
}
