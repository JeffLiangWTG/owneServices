using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace NUnit.Framework
{
	public abstract class XmlAssertion<TXmlObject>
			where TXmlObject : XObject
	{
		protected TXmlObject Object { get; set; }
		protected readonly bool withDelayedChecks;
		readonly List<Func<(bool IsSuccessful, IXmlValidationError validationError)>> delayedChecks = new List<Func<(bool IsSuccessful, IXmlValidationError validationError)>>();
		readonly string message;

		protected XmlAssertion(TXmlObject @object, bool withDelayedChecks, string message)
		{
			Object = @object;
			this.withDelayedChecks = withDelayedChecks;
			this.message = string.IsNullOrEmpty(message) ? string.Empty : (message + Environment.NewLine);
		}

		protected void QueueOrDo(Func<(bool IsSuccessful, IXmlValidationError ValidationError)> predicate)
		{
			if (withDelayedChecks)
			{
				delayedChecks.Add(predicate);
				return;
			}

			var result = predicate();
			if (!result.IsSuccessful)
			{
				var errorMessage = $"{message}{result.ValidationError.GetMessage()}";
				Assertion.Fail(errorMessage);
			}
		}

		public (bool IsSuccessful, IXmlValidationError ValidationMessage) Check()
		{
			foreach (var check in delayedChecks)
			{
				var result = check();
				if (!result.IsSuccessful)
				{
					return result;
				}
			}

			return (true, null);
		}
	}
}
