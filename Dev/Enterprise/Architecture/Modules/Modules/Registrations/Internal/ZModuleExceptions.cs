using System;

namespace Enterprise.ZArchitecture.Modules
{
	[Serializable]
	public abstract class ModuleFeatureNotSupportedException : NotSupportedException
	{
		public ModuleFeatureNotSupportedException(string message, Exception innerException = null)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected ModuleFeatureNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class ModuleGuiNotSupportedException : ModuleFeatureNotSupportedException
	{
		public ModuleGuiNotSupportedException(string message, Exception innerException = null)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected ModuleGuiNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class ModuleCannotFindControllerException : ModuleFeatureNotSupportedException
	{
		public ModuleCannotFindControllerException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ModuleCannotFindControllerException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class ModuleNotAssignedIDException : ModuleFeatureNotSupportedException
	{
		public ModuleNotAssignedIDException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ModuleNotAssignedIDException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class ModuleTemplateCopyNotSupportedException : ModuleFeatureNotSupportedException
	{
		public ModuleTemplateCopyNotSupportedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ModuleTemplateCopyNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class ModuleIDIsNullException : ModuleFeatureNotSupportedException
	{
		public ModuleIDIsNullException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ModuleIDIsNullException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class ControllerShowNewFormNotSupportedException : ModuleFeatureNotSupportedException
	{
		public ControllerShowNewFormNotSupportedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ControllerShowNewFormNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class ControllerShowEditFormNotSupportedException : ModuleFeatureNotSupportedException
	{
		public ControllerShowEditFormNotSupportedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ControllerShowEditFormNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class ControllerShowViewFormNotSupportedException : ModuleFeatureNotSupportedException
	{
		public ControllerShowViewFormNotSupportedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ControllerShowViewFormNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}

	[Serializable]
	public class ControllerShowDeleteFormNotSupportedException : ModuleFeatureNotSupportedException
	{
		public ControllerShowDeleteFormNotSupportedException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ControllerShowDeleteFormNotSupportedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
