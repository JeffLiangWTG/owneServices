using System;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class StmDocumentDeliveryManagerForTest : StmDocumentDeliveryManager
	{
		public override void ProcessDocumentDelivery(StmDocumentDelivery stmDocumentDelivery)
		{
			if (stmDocumentDelivery.SDL_RetryAttempts.ToZInt() >= 1)
			{
				throw new Exception("Test exception");
			}
		}
	}
}
