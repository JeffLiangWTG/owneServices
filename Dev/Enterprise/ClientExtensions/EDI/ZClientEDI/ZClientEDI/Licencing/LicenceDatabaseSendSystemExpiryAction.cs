using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Licencing.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public class LicenceDatabaseSendSystemExpiryAction
	{
		public LicenceDatabaseSendSystemExpiryAction(LicenceDatabase businessEntity)
		{
			databases = new LicenceDatabase[] { businessEntity };
		}

		public LicenceDatabaseSendSystemExpiryAction(LicenceDatabase[] databaseList)
		{
			databases = databaseList;
		}
		readonly LicenceDatabase[] databases;

		#region SendNewSystemShutdownDate

		public void SendNewSystemShutdownDate()
		{
			if (databases.Length > 0)
			{
				var sendFactory = new BusinessObjectFactory();
				var systemShutDownDate = new SystemShutdownDate(databases[0]);

				Dictionary<LicenceDatabase, ZString> databaseErrorMessages = new Dictionary<LicenceDatabase, ZString>();
				ZInt successCount = 0;
				ZInt failCount = 0;
				ZString errorMessage = string.Empty;

				if (databases.Length > 0)
				{
					using (var form = GetNewShutdownDateForm(systemShutDownDate))
					{
						if (form.ShowDialog() == DialogResult.OK)
						{
							var newDate = systemShutDownDate.NewSystemShutdownDate;
							if (newDate.IsEmpty)
							{
								newDate = ZDateTime.Today.AddDays(Licensing.Licences.DefaultLicenceGracePeriodInDays);
							}

							var databasesInSendFactory = sendFactory.Load<LicenceDatabase>(new ZQuery(LicenceDatabaseSchema.PK, databases.Select(x => x.PK)));

							foreach (var ld in databasesInSendFactory)
							{
								if (ld.LD_Status != DatabaseStatusList.Codes.REG && !ld.LD_HostServerSID.IsValid)
								{
									errorMessage = "Can't send update since server SID is empty. A heartbeat is needed from the client.";
									databaseErrorMessages.Add(ld, errorMessage);
									failCount++;
								}
								else
								{
									try
									{
										systemShutDownDate.ApplyTo(ld);
										if (ld.LD_Status != DatabaseStatusList.Codes.REG)
										{
											ld.SendUpdateForSystemExpiry("", newDate);
										}
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
										ld.Logs.AddNew(Enterprise.ZArchitecture.Business.Events.EditedARecord, String.Format("Send new system shutdown date '{0}' for '{1}' Server.", systemShutDownDate.NewSystemShutdownDate.ToString("MM/dd/yyyy"), ld.LD_ServerCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
										sendFactory.Save();
										successCount++;
									}
									catch (Exception ex) when (!ex.IsCriticalException())
									{
										if (errorMessage.IsEmpty)
										{
											errorMessage = ex.Message;
										}
										databaseErrorMessages.Add(ld, errorMessage);
										failCount++;
									}
								}
							}

							if (databasesInSendFactory.Length == 1)
							{
								if (successCount == 1)
								{
									Globals.Message.ShowInformation("Shutdown date updated successfully");
								}
								else
								{
									Globals.Message.ShowError(errorMessage);
								}
							}
							else
							{
								ZStringBuilder message = new ZStringBuilder();
								if (successCount > 0 || databaseErrorMessages.Values.Count != 1)
								{
									message.AppendLine(string.Format("{0} Database(s) successfully sent", successCount));
								}
								message.AppendLine(string.Format("{0} Database(s) had errors.", failCount));
								if (databaseErrorMessages.Values.Count > 0)
								{
									message.AppendLine("Failures shown below <Enterprise Code>-<Server Code> <Error Message>");
									foreach (KeyValuePair<LicenceDatabase, ZString> x in databaseErrorMessages)
									{
										message.AppendLine(string.Format("{0}-{1} {2}", x.Key.LicEnterprise.LE_EnterpriseCode, x.Key.LD_ServerCode, x.Value));
									}
								}
								Globals.Message.Show(message.ToString(), "Summary message of sent Shutdown dates ", MessageBoxButtons.OK, MessageBoxIcon.None);
							}
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowWarning((NoResString)"Please select a Licence Database(s)");
			}
		}

		protected virtual SendNewSystemShutdownDateForm GetNewShutdownDateForm(SystemShutdownDate systemShutDownDate)
		{
			return new SendNewSystemShutdownDateForm(systemShutDownDate);
		}
	}
	#endregion
}


