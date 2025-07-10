using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class CurrentThreadBusinessObjectLoader
	{
		public BusinessObject GetOnCurrentThread(BusinessObject maybeUnsafeBizo, [CallerFilePath] string sourceFilePath = "")
		{
			return GetOnCurrentThreadCore(maybeUnsafeBizo, sourceFilePath);
		}

		protected virtual BusinessObject GetOnCurrentThreadCore(BusinessObject maybeUnsafeBizo, string sourceFilePath)
		{
			if (maybeUnsafeBizo?.Factory == null || maybeUnsafeBizo.Factory.IsOwnedByCurrentThread)
			{
				return maybeUnsafeBizo;
			}
			else
			{
				var callerFileName = sourceFilePath != null ? Path.GetFileNameWithoutExtension(sourceFilePath) : null;
				var newFactory = new BusinessObjectFactory { NameForDebugging = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", callerFileName ?? nameof(CurrentThreadBusinessObjectLoader), nameof(GetOnCurrentThread)) };

				return newFactory.Load(maybeUnsafeBizo.GetType(), maybeUnsafeBizo.PK);
			}
		}
	}
}
