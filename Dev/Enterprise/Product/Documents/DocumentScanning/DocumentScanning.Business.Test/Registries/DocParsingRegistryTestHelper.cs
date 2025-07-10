using System;
using System.Collections.Generic;

namespace Enterprise.DocumentScanning.Business.Test
{
	public static class DocParsingRegistryTestHelper
	{
		public static IDisposable SetTemporaryDocParsingRegistryValues(this DocManagerRegistry instance, bool registryValue)
		{
			var compositeDisposable = new CompositeDisposable();

			try
			{
				compositeDisposable.Add(instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue));
				compositeDisposable.Add(instance.EnableAccountsPayableInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue));

				return compositeDisposable;
			}
			catch
			{
				compositeDisposable.Dispose();
				throw;
			}
		}

		public class CompositeDisposable : IDisposable
		{
			readonly List<IDisposable> disposables = [];
			bool disposed;

			public CompositeDisposable Add(IDisposable disposable)
			{
				disposables.Add(disposable);
				return this;
			}

			public void Dispose()
			{
				if (disposed)
				{
					return;
				}

				 List<Exception> exceptions = null;

				foreach (var disposable in disposables)
				{
					try
					{
						disposable.Dispose();
					}
					catch(Exception ex)
					{
						exceptions ??= new List<Exception>();
						exceptions.Add(ex);
					}
				}

				disposed = true;

				if (exceptions != null)
				{
					throw new AggregateException("One or more disposables failed to clean up.", exceptions);
				}
			}
		}
	}
}
