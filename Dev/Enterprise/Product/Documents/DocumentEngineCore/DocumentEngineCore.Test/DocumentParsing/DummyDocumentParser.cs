using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	sealed class DummyDocumentParser : DocumentParser
	{
		public DummyDocumentParser(Type sourceDocWrapperType, BusinessObjectFactory factory)
			: base(sourceDocWrapperType, factory)
		{
		}

		public static void RegisterThisSubTypeOverride()
		{
			NewDocumentParser = GetNewDummyDocumentParser;
		}

		public static void UnregisterThisSubTypeOverride()
		{
			NewDocumentParser = null;
		}

		static DummyDocumentParser GetNewDummyDocumentParser(Type sourceDocWrapperType, BusinessObjectFactory factory)
		{
			return new DummyDocumentParser(sourceDocWrapperType, factory);
		}
	}
}
