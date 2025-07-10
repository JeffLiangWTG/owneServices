using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestedType(typeof(UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>))]
	class UploadDocumentsSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new UploadDocumentsSendingActionParent<UploadDocumentsSendingAction>(Factory.New<AsycudaManifestHeader>());
		}
	}
}
