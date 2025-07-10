using System;
using System.Globalization;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public class BMSEmailDef : EmailDef
	{
		public BMSEmailDef(string subject, string body, GuidRegistryItem groupRegistryItem = null)
		{
			this.groupRegistryItem = groupRegistryItem ?? BMSRegistry.Instance.NotificationGroup;
			Subject = subject;
			Body = body;
		}

		readonly GuidRegistryItem groupRegistryItem;

		public void Send()
		{
			try
			{
				Env.OutgoingMailManager.CreateAndSave(this, groupRegistryItem.Value, GroupSourceLocator.GetFromRegistryItem(groupRegistryItem));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Exception occurred when sending the following email:\r\n\r\n{0}\r\n{1}", Subject, Body), ex); // Developer exception message
			}
		}
	}
}
