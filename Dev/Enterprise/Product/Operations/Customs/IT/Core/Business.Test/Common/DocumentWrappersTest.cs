using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class DocumentWrappersTest : TestCase
{
	public void TestLoadTypeSADHDocumentWrapper()
	{
		AssertTypeCorrectlyLoaded(DocumentWrapperConstants.FullNames.SADH);
	}

	public void TestLoadTypeITSadAttachmentDocumentWrapper()
	{
		AssertTypeCorrectlyLoaded(DocumentWrapperConstants.FullNames.ITSadAttachment);
	}

	void AssertTypeCorrectlyLoaded(ZString assemblyQualifiedName)
	{
		AssertNotNull($"Loaded Type from {assemblyQualifiedName}", Type.GetType(assemblyQualifiedName));
	}
}
