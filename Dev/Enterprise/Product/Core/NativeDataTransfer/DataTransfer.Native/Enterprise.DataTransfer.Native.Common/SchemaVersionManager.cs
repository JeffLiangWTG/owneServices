using System;
using CargoWise.Common;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using UniversalSchemaVersionManager = Enterprise.UniversalDataBuss.DataObjects.Core.SchemaVersionManager;

namespace Enterprise.DataTransfer.Native.Common
{
	public class SchemaVersionManager
	{
		public SchemaVersionManager(string nameSpace)
		{
			this.Namespace = nameSpace;
			switch (nameSpace)
			{
				case NativeXmlInfo.Namespace_2011_11:
					this.Version = NativeXmlInfo.Version_2011_11;
					this.UniversalNamespace = UniversalXmlInfo.Namespace_2011_11;
					this.UniversalVersion = UniversalXmlInfo.Version_2011_11;
					break;
				case NativeXmlInfo.Namespace_2012_11:
					this.Version = NativeXmlInfo.Version_2012_11_DO_NOT_USE;
					this.UniversalNamespace = UniversalXmlInfo.Namespace_2012_11;
					this.UniversalVersion = UniversalXmlInfo.Version_2012_11_DO_NOT_USE;
					break;
				default:
					throw new NativeXMLUserVisibleException(string.Format("Invalid namespace [{0}] - Please use a valid Native Namespace.", nameSpace ?? "(null)"));
			}
		}

		public readonly string Namespace;
		public readonly string Version;
		public readonly string UniversalNamespace;
		public readonly string UniversalVersion;

		[ThreadStatic]
		static SchemaVersionManager instance;
		public static SchemaVersionManager Instance
		{
			get
			{
				if (instance == null)
				{
					if (eAdaptorRegistry.Instance.UseDate2012_11NamespaceAndFormat)
					{
						instance = new SchemaVersionManager(NativeXmlInfo.Namespace_2012_11);
					}
					else
					{
						instance = new SchemaVersionManager(NativeXmlInfo.Namespace_2011_11);
					}
				}

				return instance;
			}
		}

#if DEBUG
		public static IDisposable SetNamespaceForTesting(string nameSpace)
		{
			var instance = SchemaVersionManager.instance;

			SchemaVersionManager.instance = new SchemaVersionManager(nameSpace);

			var universalNameSpaceSetter = UniversalSchemaVersionManager.SetNamespaceForTesting(SchemaVersionManager.instance.UniversalNamespace);

			return new DisposableAction(delegate
			{
				try
				{
					universalNameSpaceSetter.Dispose();
				}
				finally
				{
					SchemaVersionManager.instance = instance;
				}
			});
		}
#endif
	}
}
