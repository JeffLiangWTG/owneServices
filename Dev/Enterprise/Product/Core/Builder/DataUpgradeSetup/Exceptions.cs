using System;

namespace Enterprise.Builder.DataUpgradeSetup
{
	[Serializable]
	public class DataUpgradeSetupException : Exception
	{
		public DataUpgradeSetupException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected DataUpgradeSetupException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class ControllerNotCheckedOutByMeException : DataUpgradeSetupException
	{
		public ControllerNotCheckedOutByMeException()
			: base(DefaultMessage)
		{
		}

#if NETFRAMEWORK
		protected ControllerNotCheckedOutByMeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		protected const string DefaultMessage = "DataVersionFile is not checked out by you";
	}

	[Serializable]
	public class ControllerHasUncheckedSetupTasksException : DataUpgradeSetupException
	{
		public ControllerHasUncheckedSetupTasksException()
			: base(DefaultMessage)
		{
		}

#if NETFRAMEWORK
		protected ControllerHasUncheckedSetupTasksException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		protected const string DefaultMessage = "Controller has unchecked tasks. Cannot check-in / undo-check-out";
	}
}
