using System;
using System.IO;

namespace Enterprise.DataTransfer.Common.Import
{
	public abstract class DataImportService<T>
	{
		bool isCancel;

		public bool AlwaysUseProvidedPKs { get; set; }

		public Action BeforeProcess;
		public Action AfterProcess;
		public Action<T> BeforeUnitProcess;
		public Action<T> AfterUnitProcess;
		public Action<T, Exception> ErrorOccur;
		public Action<T> UnitProcessSuccess;

		public abstract void Import(Stream stream);

		public void Cancel()
		{
			isCancel = true;
		}

		protected void Initial()
		{
			isCancel = false;
		}

		protected bool IsCancelled()
		{
			return isCancel;
		}
	}
}