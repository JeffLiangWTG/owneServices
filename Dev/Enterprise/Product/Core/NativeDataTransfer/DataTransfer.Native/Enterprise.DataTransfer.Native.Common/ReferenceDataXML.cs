#region SuppressResourceStringsCheckRegion

using System;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.Common
{
	public static class ReferenceDataXMLForDeSerialize
	{
		public const string NameSpace_Universal = "http://www.cargowise.com/Schemas/Universal";
		public const string RootElementName_Universal = "ReferenceData";

		public const string NameSpace_Unversioned_Native = "http://www.cargowise.com/Schemas/Native";

		public const string Version = NativeXmlInfo.Version_2011_11;
		public const string RootElementName = NativeXmlInfo.RootElementName;
		public const string HeaderElementName = NativeXmlInfo.HeaderElementName;
		public const string BodyElementName = NativeXmlInfo.BodyElementName;
	}

	public class ReferenceDataXMLForSerialize
	{
		public static ReferenceDataXMLForSerialize Instance
		{
			get { return instance ?? (instance = GetNewReferenceData()); }
		}

		static ReferenceDataXMLForSerialize GetNewReferenceData()
		{
			// BG - This commented code is here as a template "Just in case" we have a need to reinstate an option to output using the old namespaces.
			//if (eHubMessagingRegistry.Instance.UseLegacyNativeXMLNamespace.Value)
			//{

			return new ReferenceDataXMLForSerialize(NativeXmlInfo.Namespace_2011_11, ReferenceDataXMLForDeSerialize.RootElementName);

			//}
			//return new ReferenceDataXMLForSerialize(ReferenceDataXMLForDeSerialize.NameSpace_Universal, ReferenceDataXMLForDeSerialize.RootElementName_Universal);
			// BG - This commented code is here as a template "Just in case" we have a need to reinstate an option to output using the old namespaces.
		}

		[ThreadStatic]
		static ReferenceDataXMLForSerialize instance;

#if DEBUG
		public static void ResetInstanceForTesting()
		{
			instance = null;
		}
#endif

		ReferenceDataXMLForSerialize(string nameSpace, string rootElementName)
		{
			this.NameSpace = nameSpace;
			this.RootElementName = rootElementName;
		}

		public readonly string NameSpace;
		public readonly string RootElementName;

		public readonly string HeaderElementName = ReferenceDataXMLForDeSerialize.HeaderElementName;
		public readonly string BodyElementName = ReferenceDataXMLForDeSerialize.BodyElementName;
	}
}

#endregion
