using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DeclarationDeferredAmendmentSavingOptions))]
	public class DeclarationDeferredAmendmentSavingOptionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldTakeReasonForSavingWithoutSendingSeparately()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var savingOptions = new DeclarationDeferredAmendmentSavingOptions(declaration);
			savingOptions.SendAmendment = true;
			Assert((savingOptions as IDeferredAmendmentSavingOptions).ShouldTakeReasonForSavingWithoutSendingSeparately);

			savingOptions.SendAmendment = false;
			Assert(!(savingOptions as IDeferredAmendmentSavingOptions).ShouldTakeReasonForSavingWithoutSendingSeparately);

			savingOptions.SaveWithEntryChanges = true;
			Assert(!(savingOptions as IDeferredAmendmentSavingOptions).ShouldTakeReasonForSavingWithoutSendingSeparately);

			savingOptions.SaveWithEntryChanges = false;
			Assert(!(savingOptions as IDeferredAmendmentSavingOptions).ShouldTakeReasonForSavingWithoutSendingSeparately);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
			savingOptions.SendAmendment = true;
			Assert((savingOptions as IDeferredAmendmentSavingOptions).ShouldTakeReasonForSavingWithoutSendingSeparately);

			savingOptions.SendAmendment = false;
			Assert(!(savingOptions as IDeferredAmendmentSavingOptions).ShouldTakeReasonForSavingWithoutSendingSeparately);

			savingOptions.SaveWithEntryChanges = true;
			Assert((savingOptions as IDeferredAmendmentSavingOptions).ShouldTakeReasonForSavingWithoutSendingSeparately);

			savingOptions.SaveWithEntryChanges = false;
			Assert(!(savingOptions as IDeferredAmendmentSavingOptions).ShouldTakeReasonForSavingWithoutSendingSeparately);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new DeclarationDeferredAmendmentSavingOptions(declaration);
		}

		#endregion

	}
}
