using CargoWise.Common;
using Enterprise.Core.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	internal class TabPermissionChecker
	{
		public TabPermissionChecker(string formToken, IZSecurity security)
		{
			Argument.NotNullOrEmpty(formToken, "formToken");

			this.formToken = formToken;
			this.security = security;
		}

		readonly string formToken;
		readonly IZSecurity security;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public bool IsEditAllowed(string tabToken)
		{
			return DeterminePermission(tabToken, "Edit");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public bool IsViewAllowed(string tabToken)
		{
			return DeterminePermission(tabToken, "View");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public string ErrorMessageForEditNotAllowed(string tabToken)
		{
			return DetermineCheckpoint(tabToken, "Edit")?.ErrorMessageForNotAllowed ?? security.FindCheckPoint(formToken).ErrorMessageForNotAllowed;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public string ErrorMessageForViewNotAllowed(string tabToken)
		{
			return DetermineCheckpoint(tabToken, "View")?.ErrorMessageForNotAllowed ?? security.FindCheckPoint(formToken).ErrorMessageForNotAllowed;
		}

		SecurityCheckpoint DetermineCheckpoint(string tabToken, string point)
		{
			Argument.NotNullOrEmpty(tabToken, "tabToken");
			if (tabToken != "WorkflowTabPage")
			{
				var checkpoint = FindCheckPoint(tabToken, point);
				return checkpoint as SecurityCheckpoint;
			}
			else
			{
				return null;
			}
		}

		bool DeterminePermission(string tabToken, string point)
		{
			var checkpoint = DetermineCheckpoint(tabToken, point);
			return checkpoint == null || checkpoint.IsAllowed;
		}

		ISecurityCheckpoint FindCheckPoint(string tabToken, string point)
		{
			var key = formToken + "." + point + "." + tabToken;
			return security.FindCheckPoint(key);
		}
	}
}
