using Enterprise.Messaging.Business;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class LinkedeNettEDIMessageTest : eNettEDIMessageTest
	{
		public void TestTypeDecider()
		{
			Assert(LinkedeNettEDIMessage.TypeDecider is eNettEDIMessageTypeDecider);
		}

		#region Implementation

		protected override EDIMessage GetNewMessage()
		{
			EDIMessage result = base.GetNewMessage();
			result.EM_MessageText = GetMessageText();
			return result;
		}

		protected abstract string GetMessageText();

		#endregion
	}
}
