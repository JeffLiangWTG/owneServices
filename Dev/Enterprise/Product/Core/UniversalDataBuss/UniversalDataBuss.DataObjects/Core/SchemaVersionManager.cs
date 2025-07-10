using System;
using CargoWise.Common;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class SchemaVersionManager
	{
		public static IUniversalXmlSchema Current
		{
			get
			{
				if (current == null)
				{
					current = eAdaptorRegistry.Instance.UseDate2012_11NamespaceAndFormat
						? UniversalXmlSchema.Version_2012_11_DO_NOT_USE
						: UniversalXmlSchema.Version_2011_11;
				}

				return current;
			}
		}

		[ThreadStatic]
		static IUniversalXmlSchema current;

#if DEBUG

		public static IDisposable SetNamespaceForTesting(string nameSpace)
		{
			current = nameSpace.GetUniversalXmlSchema();

			return new DisposableAction(() => current = null);
		}

		public static IDisposable SetNamespaceForTesting(IUniversalXmlSchema schema)
		{
			current = schema;

			return new DisposableAction(() => current = null);
		}
#endif
	}
}
