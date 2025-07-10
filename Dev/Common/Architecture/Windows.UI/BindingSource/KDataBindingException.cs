using System;
using System.Collections.Generic;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// An exception that provides details as to an error binding a control. This exception could also be a
	/// list of other KDataBindingExceptions that have accumulated.
	/// </summary>
	[Serializable]
	public class KDataBindingException : Exception
	{
		public KDataBindingException()
		{
		}

		/// <summary>
		/// Constructs a new exception without an initial message. This is typically used when inner
		/// KDataBindingException are added to list.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public KDataBindingException(object dataSource)
		{ this.DataSourceTypeName = dataSource.GetType().FullName; }

		/// <summary>
		/// Constructs a new exception without an initial message. This is typically used when inner
		/// KDataBindingException are added to list.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public KDataBindingException(object dataSource, Exception innerException)
			: base(innerException.Message, innerException)
		{ this.DataSourceTypeName = dataSource.GetType().FullName; }

		/// <summary>
		/// Constructs a new exception with a message.
		/// </summary>
		public KDataBindingException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Constructs a new exception with a message and inner exception.
		/// </summary>
		public KDataBindingException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Constructs a new exception with the bindingMember and a reason for the binding error.
		/// </summary>
		public KDataBindingException(string bindingMember, string reason)
			: base(bindingMember + ": " + reason)
		{
		}

		/// <summary>
		/// Constructs a new exception with a message and inner exception.
		/// </summary>
		public KDataBindingException(string bindingMember, string reason, Exception innerException)
			: base("\n'" + bindingMember + "': " + reason, innerException)
		{
		}

#if NETFRAMEWORK
		protected KDataBindingException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
#endif

		/// <summary>
		/// Add a single KDataBindingException to the list. When one or more exceptions are added to this
		/// list, the message will be an aggregation of the listed exceptions and any message that was
		/// passed into the constructor will be ignored.
		/// </summary>
		public void Add(KDataBindingException ex)
		{ ExceptionList.Add(ex); }

		public override string Message
		{
			get
			{
				string result;
				if (ExceptionList.Count == 0)
				{
					result = base.Message;
				}
				else
				{
					result = (NoResString)"While binding to object of type " + DataSourceTypeName + ":\n\n";
					foreach (KDataBindingException e in ExceptionList)
					{
						result += e.Message + "\n";
					}
				}
				return result;
			}
		}

		#region Implementation

		string DataSourceTypeName
		{
			get { return (string)Data["DataSourceTypeName"]; }
			set { Data["DataSourceTypeName"] = value; }
		}

		List<Exception> ExceptionList
		{
			get
			{
				var result = (List<Exception>)Data["ExceptionList"];
				if (result == null)
				{
					result = new List<Exception>();
					ExceptionList = result;
				}
				return result;
			}
			set { Data["ExceptionList"] = value; }
		}

		#endregion
	}
}
