using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.TGE.Module
{
	internal class TGEShipmentModuleOverride : JobShipmentModule
	{
		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem("Export CSS Data <Debug Menu Only>", new EventHandler(BypassCSSExportServiceTaskForDebugOnly)));
			return result.ToArray();
		}

		void BypassCSSExportServiceTaskForDebugOnly(object sender, EventArgs args)
		{
			LoggerForTesting logger = new LoggerForTesting();
			if (TGEDataRegistry.Instance.CSSInterfaceReadyToGo)
			{
				logger.NotifiedEventList.Add("Registry is OK - so will run log walker.");
				LogWalkerRunner.Master().Process((ICategoryLogger<LogWalkerCategories>)logger, CancellationToken.None);
				LogWalkerRunner.Default().Process((ICategoryLogger<LogWalkerCategories>)logger, CancellationToken.None);
				LogWalkerRunner.Purge().Process((ICategoryLogger<LogWalkerCategories>)logger, CancellationToken.None);
			}
			else
			{
				logger.NotifiedEventList.Add("Registry is NOT OK.  Log walker abandoned.");
			}
			if (logger.NotifiedEventList.Count > 0)
			{
				Globals.Message.Show(logger.ToString(), "Log subscriber Exception Messages", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		[Serializable]
		class LoggerForTesting : ILogger
		{
			void ILogger.Log(LogType logType, string message)
			{
				NotifiedEventList.Add(message);
			}

			void ILogger.Log(LogType logType, string message, Exception ex)
			{
				NotifiedEventList.Add(message);
			}

			public List<string> NotifiedEventList
			{
				get
				{
					return notifiedEventList ?? (notifiedEventList = new List<string>());
				}
			}

			List<string> notifiedEventList;
			public override string ToString()
			{
				StringBuilder result = new StringBuilder();
				foreach (string item in NotifiedEventList)
				{
					result.AppendLine(item);
				}

				return result.ToString();
			}
		}
	}
}
