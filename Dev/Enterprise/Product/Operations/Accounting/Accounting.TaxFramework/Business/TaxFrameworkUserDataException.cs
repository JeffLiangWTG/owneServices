using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.TaxFramework.Business
{
	[Serializable]
	public abstract class TaxFrameworkUserDataException : Exception
	{
		protected TaxFrameworkUserDataException(MultilingualString message) : base(message.GetUnresolvedString())
		{
			this.message = message;
		}

#if NETFRAMEWORK
		protected TaxFrameworkUserDataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public string LocalLanguageMessage => message.GetLocalizedValue(Res.CurrentLanguage).ToString();

		[NonSerialized]
		readonly MultilingualString message;
	}

	[Serializable]
	public class TaxFrameworkUnknownConfigurationValueException : TaxFrameworkUserDataException
	{
		public TaxFrameworkUnknownConfigurationValueException(MultilingualString message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected TaxFrameworkUnknownConfigurationValueException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class TaxFrameworkConfigurationValueException : TaxFrameworkUserDataException
	{
		public TaxFrameworkConfigurationValueException(MultilingualString message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected TaxFrameworkConfigurationValueException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class TaxFrameworkInvalidDataException : TaxFrameworkUserDataException
	{
		public TaxFrameworkInvalidDataException(MultilingualString message) : base(message)
		{
		}

#if NETFRAMEWORK
		protected TaxFrameworkInvalidDataException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
