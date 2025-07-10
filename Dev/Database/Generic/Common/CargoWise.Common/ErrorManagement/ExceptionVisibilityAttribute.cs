using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	/// <summary>
	/// Specifies which party an exception is visible to. The default is 'Developer'.
	/// </summary>
	public enum ExceptionVisibility
	{
		/// <summary>
		/// The exception will be reported to CargoWise as a developer exception.
		/// </summary>
		Developer,

		/// <summary>
		/// The exception will be shown to the user and the application will continue.
		/// </summary>
		User,
	}

	/// <summary>
	/// Apply this attribute to an exception class to specify which party an exception is visible to.
	/// </summary>
	/// <see>CargoWise.Common.ExceptionVisibility</see>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ExceptionVisibilityAttribute : Attribute
	{
		public ExceptionVisibilityAttribute(ExceptionVisibility visibility)
		{
			this.visibility = visibility;
		}

		/// <summary>
		/// Evaluate the ExceptionVisibility of a particular exception.
		/// </summary>
		/// <param name="ex">The exception to evaluate</param>
		/// <returns></returns>
		[SuppressMessage("Microsoft.Contracts", "Nonnull-16-0")] // We can suppress the message here as TypeDescriptor type is not null
		public static ExceptionVisibility Evaluate(Exception ex)
		{
			ExceptionVisibilityAttribute attr = (ExceptionVisibilityAttribute)TypeDescriptor.GetAttributes(ex)[typeof(ExceptionVisibilityAttribute)];
			return attr == null ? ExceptionVisibility.Developer : attr.visibility;
		}

		public ExceptionVisibility Visibility
		{
			get { return visibility; }
		}
		readonly ExceptionVisibility visibility;

		/// <summary>
		/// Search for exception with ExceptionVisibility.User attribure throuh innerExceptions
		/// </summary>
		/// <param name="ex">The exception to evaluate</param>
		/// <returns>Exception with ExceptionVisibility.User attribure otherwise null</returns>
		[SuppressMessage("Microsoft.Contracts", "Nonnull-12-0")]
		public static Exception GetFirstOccurenceOfUserException(Exception ex)
		{
			do
			{
				if (ExceptionVisibilityAttribute.Evaluate(ex) == ExceptionVisibility.User)
				{
					return ex;
				}
				ex = ex.InnerException;
			}
			while (ex != null);

			return null;
		}
	}
}
