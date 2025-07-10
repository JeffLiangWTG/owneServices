using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Messaging.Business.Testing
{
	public abstract class SendersMessageReferenceProviderTest : TestCaseWithFactory
	{
		public void TestPopulateSendersReferenceIfNeeded()
		{
			ZString result1;
			ZString result2;
			ISendersMessageReferenceProvider provider = GetSavableProvider();
			AssertEquals("SendersReference", ZString.Empty, provider.SendersReference);
			CargoWise.Data.Db.Connection.BeginTransaction();
			try
			{
				provider.PopulateSendersReferenceIfNeeded();
				result1 = provider.SendersReference;
				provider.PopulateSendersReferenceIfNeeded();
				result2 = provider.SendersReference;
			}
			finally
			{
				CargoWise.Data.Db.Connection.RollbackTransaction();
			}
			Assert("SendersReferenceNotEmpty", !result1.IsEmpty);
			AssertEquals("Result Only Populated Once", result1, result2);
		}

		public void TestSendersReferencePopulatedOnRecordSave()
		{
			ISendersMessageReferenceProvider provider = GetSavableProvider();
			AssertEquals("SendersReference", ZString.Empty, provider.SendersReference);
			Factory.Save();
			Assert("SendersReferenceNotEmpty", !provider.SendersReference.IsEmpty);
		}

		public void TestSendersReferenceRolledBackOnSaveFailure()
		{
			ISendersMessageReferenceProvider provider = GetProviderThatThrowsExceptionWhilstSaving();
			AssertEquals("SendersReference", ZString.Empty, provider.SendersReference);
			bool exceptionThrown = false;
			try
			{
				Factory.Save();
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				exceptionThrown = true;
			}
			Assert("ExceptionThrown", exceptionThrown);
			AssertEquals("SendersReference", ZString.Empty, provider.SendersReference);
		}

		#region Implementation

		protected abstract ISendersMessageReferenceProvider GetSavableProvider();
		protected abstract ISendersMessageReferenceProvider GetProviderThatThrowsExceptionWhilstSaving();

		#endregion
	}
}
