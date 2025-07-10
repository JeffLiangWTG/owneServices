using System;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;

namespace CargoWise.ResourceStrings.Cache
{
	public interface ITranslatableDataTransformHelper
	{
		void AddResourceString(ResourceStringData data, string language);
		void DeleteResourceString(string key, string language);
		void SaveChanges();

#if DEBUG
		IDisposable MockResourceStringSources();
		IMockResourceStringCache GetMockSource(string language);
#endif
	}
}
