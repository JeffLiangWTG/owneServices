using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine
{
	#region Sign By

	public static class DocumentsSignBy
	{
		public const string NON = "NON";
		public const string PFX = "PFX";
		public const string DOS = "DOS";

		public static CodeDescriptionPairList GetSignByList()
		{
			var signByList = new CodeDescriptionPairList();
			signByList.AddPair(DocumentsSignBy.NON, ResString.GetMultilingualString("42489069-4366-4757-9336-2ee1fec5363a", "None"));
			signByList.AddPair(DocumentsSignBy.PFX, ResString.GetMultilingualString("af7da8c2-f701-4570-bfd4-4f763045079d", "PFX File"));
			if (DocumentsDataRegistry.Instance.EnableDocumentSigningService.Value)
			{
				signByList.AddPair(DocumentsSignBy.DOS, ResString.GetMultilingualString("de0f2379-7271-4f95-a373-59d24bea86ab", "Document Signing Service"));
			}
			return signByList;
		}
	}

	#endregion
}
