using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class ErrorCollector
	{
		public ErrorCollector()
		{
			errors = new Dictionary<Error, ErrorInfo>();
		}
		Dictionary<Error, ErrorInfo> errors;

		public void AddError(ZString element, bool isCritical = true)
		{
			AddError(element, null, isCritical);
		}

		public void AddError(ZString element, ErrorInfo errorInfo, bool isCritical = true)
		{
			if (!element.IsEmpty)
			{
				var error = new Error(element, isCritical);
				errors[error] = errorInfo;
			}
		}

		public IEnumerable<string> GetErrors(ErrorType errorType = ErrorType.CRITICAL)
		{
			var errors = errorType == ErrorType.CRITICAL ? this.errors.Where(x => x.Key.IsCritical)
								: errorType == ErrorType.NONCRTICAL ? this.errors.Where(x => !x.Key.IsCritical)
								: this.errors;

			foreach (KeyValuePair<Error, ErrorInfo> kvp in errors)
			{
				if (kvp.Value != null)
				{
					yield return kvp.Value.Introduction + kvp.Key.ErrorText + (!kvp.Value.Box.IsEmpty ? (NoResString)" (Box " + kvp.Value.Box + (NoResString)")" : "");
				}
				else
				{
					yield return kvp.Key.ErrorText;
				}
			}
		}

		public string GetErrorsAsString(ErrorType errorType = ErrorType.CRITICAL)
		{
			var sb = new ZStringBuilder();
			foreach (var msg in GetErrors(errorType))
			{
				sb.AppendIfNotEmpty(msg);
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}

		public int ErrorCount
		{
			get { return errors.Count(x => x.Key.IsCritical); }
		}

		public void WipeErrors()
		{
			errors = new Dictionary<Error, ErrorInfo>();
		}

		public enum ErrorType
		{
			CRITICAL,
			NONCRTICAL,
			ALL
		}
	}
}
