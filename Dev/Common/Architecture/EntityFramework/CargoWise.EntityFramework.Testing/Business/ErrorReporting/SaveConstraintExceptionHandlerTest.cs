using System.Data;
using CargoWise.Common;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SaveConstraintExceptionHandlerTest : TestCaseWithFactory
	{
		public void TestHandleSaveConstraintExceptionByFullValidateAndSave_WithValidationErrors()
		{
			DummyBusinessObjectWithValidation dummy = Factory.New<DummyBusinessObjectWithValidation>();
			MockSaveInitiator initiator = new MockSaveInitiator(dummy);

			AssertEquals(false, initiator.SaveExceptionCaughtAlready);

			AssertEquals(0, dummy.ManualCallsToMarkAsNeedingValidation_CountForTesting);
			bool result = SaveConstraintExceptionHandler.HandleSaveConstraintExceptionByFullValidateAndSave("Test exception", initiator);
			AssertEquals(true, initiator.SaveExceptionCaughtAlready);
			AssertEquals(1, dummy.ManualCallsToMarkAsNeedingValidation_CountForTesting);

			AssertEquals(true, dummy.HasErrors);
			AssertEquals(true, result);
			ErrorReporter.Clear();
		}

		#region Test Classes

		public class MockSaveInitiator : ISaveInitiator
		{
			readonly IBusiness entity;

			public MockSaveInitiator(IBusiness entity)
			{
				this.entity = entity;
			}

			public bool SaveExceptionCaughtAlready { get; set; }

			public IBusiness BusinessEntityForValidation
			{
				get { return entity; }
			}

			public void ShowErrorsDialog()
			{
			}
		}

		public class DummyBusinessObjectWithValidation : DummyBusinessObject
		{
			public DummyBusinessObjectWithValidation(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void RunPreSaveValidationCore()
			{
				base.RunPreSaveValidationCore();
				Z0_AnotherNumberInfo.AddError("Error on this field");
			}
		}

		#endregion
	}
}
