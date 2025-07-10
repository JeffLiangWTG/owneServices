using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentScanning.Business
{
	public static class Constants
	{
		public static string AllocateDocumentsFormName
		{
			get { return Res.GetString("0bed7a43-db17-4923-a724-12695b204cc7", "Allocate eDocs"); }
		}

		#region Context Menu

		public static MultilingualString ViewMenuText
		{
			get { return ResString.GetMultilingualString("366c26aa-773c-4675-af81-c8c2526e8d59", "&View"); }
		}
		public const string SeparatorMenuText = "-";
		public static MultilingualString CutMenuText
		{
			get { return ResString.GetMultilingualString("c343880e-5093-48a6-a338-1dcfe8df4af9", "Cu&t"); }
		}
		public static MultilingualString CopyMenuText
		{
			get { return ResString.GetMultilingualString("3ad98c86-22ae-4c7f-9af2-473da77bfccc", "C&opy"); }
		}
		public static MultilingualString CopyLinkMenuText
		{
			get { return ResString.GetMultilingualString("43dcea12-3556-4d50-b90e-c83c619eb028", "Cop&y Link"); }
		}
		public static MultilingualString PasteMenuText
		{
			get { return ResString.GetMultilingualString("79238d4c-1007-49f1-91b6-b63468ceaa9f", "&Paste"); }
		}
		public static MultilingualString DeleteMenuText
		{
			get { return ResString.GetMultilingualString("d180ab6e-6565-43b3-a55e-22337ec426ab", "De&lete"); }
		}
		public static MultilingualString DeletePermanentlyMenuText
		{
			get { return ResString.GetMultilingualString("b5455ad7-c4f5-4b57-9933-0cc83ac51365", "Delete Perma&nently"); }
		}
		public static MultilingualString RestoreMenuText
		{
			get { return ResString.GetMultilingualString("3226bf40-82c1-4111-93a3-1d7be16ff24a", "&Restore"); }
		}
		public static MultilingualString AllocateMenuText
		{
			get { return ResString.GetMultilingualString("0d2a53a4-89e4-4b7d-b8f9-ae0591958cde", "&Allocate"); }
		}
		public static MultilingualString SelectAllMenuText
		{
			get { return ResString.GetMultilingualString("475e963f-4c76-4d6f-9cdf-0a2831411363", "&Select All"); }
		}
		public static MultilingualString UnallocateMenuText
		{
			get { return ResString.GetMultilingualString("880d1152-459b-4513-bbb6-a4b34efa0810", "&Unallocate"); }
		}
		public static MultilingualString EditPropertiesMenuText
		{
			get { return ResString.GetMultilingualString("cdaedcf3-7d7a-4a05-8656-6c9b3e480298", "&Edit Properties"); }
		}
		public static MultilingualString DeliverDocumentMenuText
		{
			get { return ResString.GetMultilingualString("5c70adbf-fcd4-4a9a-b202-865914d9b78f", "&Deliver eDoc"); }
		}
		public static MultilingualString SaveFileAsMenuText
		{
			get { return ResString.GetMultilingualString("d49b7bea-4e49-4c0e-bc8e-65d3aa0cf155", "Save &File As..."); }
		}
		public static MultilingualString SplitDocumentMenuText
		{
			get { return ResString.GetMultilingualString("2F307EE2-8F36-4B97-B81A-BCACB741A954", "&Split Document"); }
		}
		public static MultilingualString ParseDocumentMenuText
		{
			get { return ResString.GetMultilingualString("d7cade67-5085-4225-b570-48169c47bd96", "&Parse"); }
		}

		#endregion

		#region Importing / Scanning

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public const string Automatic = "Automatic";
		public const string AutomaticSingle = "AutomaticSingle";
		public const string NewFileBatch = "NewFileBatch";
		public const string NewFileSingle = "NewFileSingle";

		#endregion

		#region Virus Scan Result
		public static class VirusScanResult
		{
			public const int NotScan = 0;
			public const int NotDetected = 1;
			public const int Detected = 100;
		}
		#endregion

		#region Shipamax integration

		public static MultilingualString ReviewParsedResultsMenuText
		{
			get { return ResString.GetMultilingualString("07322222-E085-440E-80AB-0D4D9B0EC3FC", "&Review Parsed Results"); }
		}

		#endregion
	}
}
