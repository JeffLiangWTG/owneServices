using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	[DependentBusinessObject(typeof(LicenceDatabase), "Connections")]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class LicenceConnection : AutoLicenceConnection
	{
		public LicenceConnection(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("Lookups.Companies")]
		public override ZGuid LK_LC_Company
		{
			get { return base.LK_LC_Company; }
			set { base.LK_LC_Company = value; }
		}

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				ZString logReference = "Connection - ";
				logReference += "Remote Access Method: " + LK_RemoteAccessMethod + (LK_RemoteAccessMethodInfo.HasChanges ? "(" + LK_RemoteAccessMethodInfo.OriginalValue + ")" : "");
				logReference += " Address: " + LK_RemoteAccessAddress + (LK_RemoteAccessAddressInfo.HasChanges ? "(" + LK_RemoteAccessAddressInfo.OriginalValue + ")" : "");

				return logReference;
			}
		}

		#endregion

		#region Properties

		#region Database

		public LicenceDatabase Database
		{
			get { return Factory.Load<LicenceDatabase>(LK_LD); }
		}

		#endregion

		#region LK_RemoteAccessMethod

		[List("Lookups.RemoteAccessMethodsList")]
		public override ZString LK_RemoteAccessMethod
		{
			get { return base.LK_RemoteAccessMethod; }
			set
			{
				base.LK_RemoteAccessMethod = value;
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public string BuildRemoteDesktopBatchCommands(out string validationErrorText)
		{
			validationErrorText = null;
			string result = null;

			if (!LK_RemoteAccessUserName.IsEmpty && !LK_RemoteAccessPassWord.IsEmpty && !LK_RemoteAccessAddress.IsEmpty)
			{
				string server = LK_RemoteAccessAddress;
				int consoleOrAdminIndex = server.IndexOf("/console", StringComparison.OrdinalIgnoreCase);
				if (consoleOrAdminIndex < 0)
				{
					consoleOrAdminIndex = server.IndexOf("/admin", StringComparison.OrdinalIgnoreCase);
				}
				bool isAdmin = consoleOrAdminIndex >= 0;
				if (isAdmin)
				{
					server = server.Substring(0, consoleOrAdminIndex).Trim();
				}

				int portIndex = server.IndexOf(':');
				string serverWithoutPort = portIndex >= 0 ? server.Substring(0, portIndex) : server;
				if (portIndex >= 0)
				{
					string portText = server.Substring(portIndex + 1);
					int port = 0;
					if (!int.TryParse(portText, out port))
					{
						validationErrorText = "The IP address specified appears to be invalid.";
					}
				}

				if (validationErrorText == null)
				{
					result = string.Format(CultureInfo.CurrentCulture, "@echo off\r\ncmdkey /generic:{0} /user:{1} /pass:{2} > nul\r\nstart mstsc /v:{3}{4}",
						serverWithoutPort,
						LK_RemoteAccessUserName,
						LK_RemoteAccessPassWord,
						server,
						isAdmin ? " /admin" : "");
				}
			}
			else
			{
				validationErrorText = Res.GetString("4fc9fb27-5f28-4d31-834f-c0c26d7c01de", "You must specify a username, password and connection address before attempting connection using Remote Desktop.");
			}

			return result;
		}

		#region RemoteAccessMethodDescription

		public ZString RemoteAccessMethodDescription
		{
			get { return Lookups.RemoteAccessMethodsList.GetDescriptionFromCode(LK_RemoteAccessMethod); }
		}

		public ZPropertyInfo RemoteAccessMethodDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RemoteAccessMethodDescription)); }
		}

		#endregion

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return !EDISecurityCheckpoints.OrgLicenceModifyConnectionDetails.IsAllowed || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}

