using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.xTMessaging.Business.Test
{
	class Cw1DirectxTMessagingConfigTest : TestCaseWithFactory
	{
		public void TestXTIdleConnectionKeepAliveInSecondsValue_CachesValue()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				// Arrange
				var config = new Cw1DirectxTMessagingConfig();
				var firstAccessConnectionKeepAlive = config.XTIdleConnectionKeepAliveInSecondsValue;

				//Act
				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionKeepAliveInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
				{
					// Assert
					AssertNotEquals(config.XTIdleConnectionKeepAliveInSecondsValue, 1);
				}
			}
		}

		public void TestXTIdleConnectionRetryPauseInSecondsValue_CachesValue()
		{
			using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				// Arrange
				var config = new Cw1DirectxTMessagingConfig();
				var firstAccessRetryPause = config.XTIdleConnectionRetryPauseInSecondsValue;

				//Act
				using (DirectxTMessagingRegistry.Instance.XTIdleConnectionRetryPauseInSeconds.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
				{
					// Assert
					AssertNotEquals(config.XTIdleConnectionRetryPauseInSecondsValue, 1);
				}
			}
		}

		public void TestInterchangeCountPerBatchOnReceivingValue_CachesValue()
		{
			using (DirectxTMessagingRegistry.Instance.InterchangeCountPerBatchOnReceiving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				// Arrange
				var config = new Cw1DirectxTMessagingConfig();
				var firstAccessBatchCount = config.InterchangeCountPerBatchOnReceivingValue;

				//Act
				using (DirectxTMessagingRegistry.Instance.InterchangeCountPerBatchOnReceiving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
				{
					// Assert
					AssertNotEquals(config.InterchangeCountPerBatchOnReceivingValue, 1);
				}
			}
		}

		public void TestXTServerMessageChunkSizeWhenSendingValue_CachesValue()
		{
			using (DirectxTMessagingRegistry.Instance.XTServerMessageChunkSizeWhenSending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				// Arrange
				var config = new Cw1DirectxTMessagingConfig();
				var firstAccessBatchCount = config.XTServerMessageChunkSizeWhenSendingValue;

				//Act
				using (DirectxTMessagingRegistry.Instance.XTServerMessageChunkSizeWhenSending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
				{
					// Assert
					AssertNotEquals(config.XTServerMessageChunkSizeWhenSendingValue, 1);
				}
			}
		}
	}
}
