using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Gateway
{
	public class SystemUnderMaintananceException : SystemException
	{
		public SystemUnderMaintananceException(Exception ex) : this(defaultMessage, ex) { }

		public SystemUnderMaintananceException(string message, Exception ex) : base(message, ex) { }

		const string defaultMessage = "eHub Gateway under maintenance. Messages will be resubmitted to eHub automatically on next service task run. Please ignore the following message: Server under maintenance and have not a valid ediEnterprise licence code.";
	}
}