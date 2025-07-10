using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Customs.TW.Business;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	class TWCustomsAttachedDeclarationDataCreator : ICustomsAttachedDeclarationDataCreator
	{
		public ZGuid CreateAttachedDeclarationData(ZGuid shipmentPk, bool isCancelled)
		{
			var factory = new BusinessObjectFactory();

			var jobDeclaration = factory.New<JobDeclaration>();
			jobDeclaration.JE_JS = shipmentPk;

			if (isCancelled)
			{
				jobDeclaration.IsCancelled = true;
			}

			var cusEntryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			var cusTWControllingMessageHeader = cusEntryInstruction.ControllingMessageHeaders.AddNew();
			_ = cusTWControllingMessageHeader.ProductLabelRanges.AddNew();

			factory.Save();

			return jobDeclaration.PK;
		}
	}
}
