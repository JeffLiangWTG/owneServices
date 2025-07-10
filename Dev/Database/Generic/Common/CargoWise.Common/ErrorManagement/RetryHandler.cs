using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CargoWise.Common.ErrorManagement
{
	public delegate void RetryMethod();

	public delegate T RetryMethod<out T>();

	/// <summary>
	///     Class for providing retry semantics to an operation (a method call).
	/// </summary>
	public class RetryHandler
	{
		#region Private Members

		readonly List<string> errorMessages = new List<string>();

		readonly List<TimeSpan> retries;
		readonly TimeSpan maxDurationTimeout;
		static readonly Regex ReferenceRegex = new Regex(@"(-?\d+(?:\.\d+)?)([smhd])", RegexOptions.IgnoreCase);

		#endregion

		#region Construction

		/// <summary>
		///     Constructor taking a string containing a list of time span values in basic form.
		///     e.g. "30s" means 30 seconds (m=minutes, h=hours, d=days)
		///     You can combine values like: 1m30s (one minute and thirty seconds).
		///     List items are comma or semicolon separated.
		///     Unrecognised entries are ignored.
		/// </summary>
		/// <param name="retryTimeouts">List of time values, representing timeouts between invocation attempts. If no timeouts will be specified, one attempt will be made.</param>
		/// <param name="maxDurationTimeout">Optional specification of max timeout for duration of all retry attempts in total. However at least one attempt will be made.</param>
		public RetryHandler(string retryTimeouts, string maxDurationTimeout = "")
		{
			Argument.NotNull(maxDurationTimeout, nameof(maxDurationTimeout)); // Suggested By ReviewBot 
														  // Parse string into list of retry periods.
			this.retries = ParseTimeSpanList(retryTimeouts);
			TryParseTimeSpan(maxDurationTimeout, out this.maxDurationTimeout);
		}

		/// <summary>
		///     Constructor taking a collection of time values.
		/// </summary>
		/// <param name="retryTimeouts">Ordered collection of time values, representing timeouts between invocation attempts. If no timeouts will be specified, one attempt will be made.</param>
		/// <param name="maxDurationTimeout">Optional specification of max timeout for duration of all retry attempts in total. However, at least one attempt will be made.</param>
		public RetryHandler(IEnumerable<TimeSpan> retryTimeouts, TimeSpan maxDurationTimeout = new TimeSpan())
		{
														  // Take a copy of the retries list.
			this.retries = new List<TimeSpan>(retryTimeouts);
			this.maxDurationTimeout = maxDurationTimeout;
		}

		#endregion

		#region Public Interface
		/// <summary>
		/// List of errors appeared during execution of the provided delegate
		/// </summary>
		public List<string> ErrorMessages
		{
			get { return errorMessages; }
		}

		/// <summary>
		///     Invokes the specified method, with retry capability. The method must be
		///     simple and return no value. If the method fails all available retries
		///     then the exception will propagate out of this call too.
		///     If you specify N timeouts
		/// </summary>
		/// <param name="method">The method delegate.</param>
		public void Invoke(RetryMethod method)
		{
			Argument.NotNull(method, nameof(method));
			Invoke<Exception>(method);
		}

		/// <summary>
		///     Invokes the specified method, with retry capability. The method must be
		///     simple and return no value. If the method fails all available retries
		///     with supplied exception type then the exception will propagate out of this call too.
		/// </summary>
		/// <typeparam name="TException">Specification of Exception type on which to retry</typeparam>
		/// <param name="method">The method delegate.</param>
		/// <param name="exceptionHandler">The delagate to run when the exception is thrown. Handy if you need to do a reconnect on failure</param>
		/// 
		public void Invoke<TException>(RetryMethod method, Action exceptionHandler = null) where TException : Exception
		{
			Argument.NotNull(method, nameof(method));

			// wrap up in another delegate as we can't do RetryMethod<void>
			RetryMethod<bool> wrap = delegate
			{
				method();
				return true;
			};
			Invoke<bool, TException>(wrap, exceptionHandler);
		}

		/// <summary>
		///     Invokes the specified method, with retry capability. The return value
		///     from the method is captured. If the method fails all available retries
		///     then the exception will propagate out of this call too.
		/// </summary>
		/// <typeparam name="T">Return value type of the method</typeparam>
		/// <param name="method">The method delegate</param>
		/// <returns>The method's return value</returns>
		public T Invoke<T>(RetryMethod<T> method)
		{
			Argument.NotNull(method, nameof(method));
			return Invoke<T, Exception>(method);
		}

		/// <summary>
		///     Invokes the specified method, with retry capability. The return value
		///     from the method is captured. If the method fails all available retries
		///     with supplied exception type then the exception will propagate out of this call too.
		/// </summary>
		/// <typeparam name="T">Return value type of the method</typeparam>
		/// <typeparam name="TException">Specification of Exception type on which to retry</typeparam>
		/// <param name="method">The method delegate</param>
		/// <param name="exceptionHandler">The delagate to run when the exception is thrown. Handy if you need to do a reconnect on failure</param>
		/// <param name="shouldCatch">Predicate as to whether a given exception should be caught</param>
		/// <returns>The method's return value</returns>
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="It doesnt catch generic exceptions, it catches TException")]
		public T Invoke<T, TException>(RetryMethod<T> method, Action exceptionHandler = null, Predicate<TException> shouldCatch = null) where TException : Exception
		{
			Argument.NotNull(method, nameof(method));

			// Get a mutable copy of the retries list.
			var mutableRetries = new List<TimeSpan>(retries);

			var stopwatch = new Stopwatch();
			if (maxDurationTimeout > TimeSpan.Zero)
			{
				stopwatch.Start();
			}

			// will return out of the loop on success or throw on a failure
			while (true)
			{
				try
				{
					// Invoke the delegate
					return method();
				}
				catch (TException e) when (shouldCatch == null || shouldCatch(e))
				{
					// Error occurred within the delegate method.
					// If there are no retries left then we have failed to execute method.
					if (mutableRetries.Count == 0 || (stopwatch.IsRunning && stopwatch.Elapsed >= maxDurationTimeout))
					{
						errorMessages.Add(String.Format(CultureInfo.InvariantCulture, "Failed to execute delegate \"{0}.{1}\" after {2} attempt{3}: {4} . Stack trace: {5}", // exception text
							method.Method.DeclaringType.FullName,
							method.Method.Name,
							retries.Count + 1, // count first attempt as well
							(retries.Count == 0 ? "" : "s"), // exception text
							e.Message, e.StackTrace));
						// re-throw the final exception
						throw;
					}
					errorMessages.Add(
						String.Format(CultureInfo.InvariantCulture, "Caught exception trying to execute delegate \"{0}.{1}\", retrying in {2}: {3} . Stack trace: {4}", // exception text
							method.Method.DeclaringType.FullName,
							method.Method.Name,
							FormatTimeSpan(mutableRetries[0]),
							e.Message, e.StackTrace));

					if (exceptionHandler != null)
					{
						//If there was a problem in our exception handler, we'd prefer to throw the original issue 
						//since they are most likely related, and the original issue will be closer to the truth
						bool shouldThrow = false;
						try
						{
							exceptionHandler();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							ErrorReporter.ReportOnce("An exception was thrown in the exception handler", ex);
							shouldThrow = true;
						}

						if (shouldThrow)
						{
							throw;
						}
					}

					// sleep for the specified period before the next try
					Thread.Sleep(mutableRetries[0]);
					mutableRetries.RemoveAt(0);
				}
			}
		}

		/// <summary>
		///     Parse a string containing a list of time span values in basic form.
		///     e.g. "30s" means 30 seconds, "1m30s" means 90 seconds (m=minutes, h=hours, d=days)
		///     List items are comma or semicolon separated.
		///     Unrecognised and invalid entries are ignored, but logged out.
		/// </summary>
		/// <param name="list">List of time values</param>
		/// <returns>List of timespan objects containing all parsed items</returns>
		public static List<TimeSpan> ParseTimeSpanList(string list)
		{
			var result = new List<TimeSpan>();
			if (!string.IsNullOrEmpty(list))
			{
				foreach (string spec in list.Split(';', ','))
				{
					TimeSpan span;
					// if we got a time span value for this item then add it to our list,
					// otherwise log out a warning
					if (TryParseTimeSpan(spec, out span) && span >= TimeSpan.Zero)
					{
						result.Add(span);
					}
					else
					{
						ErrorReporter.ReportOnce("RetryHandler|SpecificationInvalid", String.Format("TimeSpan specification string is not valid and was ignored: {0}", spec));
					}
				}
			}
			return result;
		}

		static bool TryParseTimeSpan(string spec, out TimeSpan timespan)
		{
			Argument.NotNull(spec, nameof(spec)); // Suggested By ReviewBot 
			timespan = TimeSpan.Zero;
			bool match = false;

			if (spec.StartsWith("-"))
			{
				ErrorReporter.ReportOnce("RetryHandler|NegativeValue", String.Format("TimeSpan specification contains negative value: {0}", spec));
				return false;
			}

			try
			{
				foreach (Match m in ReferenceRegex.Matches(spec.ToUpperInvariant()))
				{
					// first group is the count (including second fraction group)
					double count = double.Parse(m.Groups[1].Value);
					// second capturing group is the unit (second, minute, hour, day)
					string unit = m.Groups[2].Value;

					// add the amount to the timespan value
					switch (unit)
					{
						case "S":
							timespan = timespan.Add(TimeSpan.FromSeconds(count));
							match = true;
							break;
						case "M":
							timespan = timespan.Add(TimeSpan.FromMinutes(count));
							match = true;
							break;
						case "H":
							timespan = timespan.Add(TimeSpan.FromHours(count));
							match = true;
							break;
						case "D":
							timespan = timespan.Add(TimeSpan.FromDays(count));
							match = true;
							break;
					}
				}
			}
			catch (OverflowException)
			{
				ErrorReporter.ReportOnce("RetryHandler|SpecificationOverflow", String.Format("TimeSpan specification overflow: {0}", spec));
				return false;
			}
			return match;
		}

		/// <summary>
		///     Formats a timespan value into a string.
		///     e.g. "30s" means 30 seconds (m=minutes, h=hours, d=days)
		/// </summary>
		/// <param name="tspan">TimeSpan value to format</param>
		/// <returns>Formatted string representation</returns>
		public static string FormatTimeSpan(TimeSpan tspan)
		{
			var buffer = new StringBuilder();
			if (tspan.Days != 0)
			{
				buffer.AppendFormat("{0}d", tspan.Days); // time span format
			}

			if (tspan.Hours != 0)
			{
				buffer.AppendFormat("{0}{1}h", (buffer.Length > 0 ? " " : ""), tspan.Hours); // time span format
			}

			if (tspan.Minutes != 0)
			{
				buffer.AppendFormat("{0}{1}m", (buffer.Length > 0 ? " " : ""), tspan.Minutes); // time span format
			}

			if (tspan.Seconds != 0)
			{
				buffer.AppendFormat("{0}{1}s", (buffer.Length > 0 ? " " : ""), tspan.Seconds); // time span format
			}

			return buffer.ToString();
		}

		#endregion
	}
}
