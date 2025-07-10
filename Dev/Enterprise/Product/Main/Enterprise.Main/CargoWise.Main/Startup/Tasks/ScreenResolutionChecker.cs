using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class ScreenResolutionChecker : AbstractApplicationStartupTask
	{
		public override string TaskDescription => Res.GetString("D2F1F2BE-0C44-4D0C-ACBB-4338CBCDB0E5", "Checking Screen Resolution");

		public ResourceStringData CheckboxCaption => Res.GetData("A9AFB5DD-8C8E-4007-AC7F-E007FBD35B15", "Do not show this message again.");

		protected override bool GetShouldExecute(CommandLineArguments arguments) => true;

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			var bounds = GetBounds();

			if (bounds.Width < (1366 - 4) || bounds.Height < (768 - 4)) //Be slightly permissive, because for some resolutions (and some drivers?) screen size is underreported by a few pixels.
			{
				if (Globals.CanShowDialogs)
				{
					var screenResolutionCheckerContext = new DialogDefaultContext(new ZGuid("139A9D76-DFDE-4F49-B281-FEE6B8293E6D"), TaskDescription, ZMessageBoxButtons.OK, ZMessageBoxIcon.Warning, true, ZDialogResult.None, true, CheckboxCaption);
					Globals.Message.ShowOrDefault(screenResolutionCheckerContext, WarningMessage);
				}
			}

			return true;
		}

		Rectangle GetBounds()
		{
			return AllScreenBounds.OrderByDescending(bounds => bounds.Width * bounds.Height).First();
		}

		string GetAllScreenResolutions()
		{
			var message = string.Empty;
			var index = 1;

			foreach (var bound in AllScreenBounds)
			{
				message += Res.GetString("B5C35D8B-DEE8-4AE6-856B-AF7E123847C9", "The resolution for monitor {0} is {1} x {2}.{3}", index, bound.Width, bound.Height, System.Environment.NewLine);
				index++;
			}

			return message;
		}

#if DEBUG
		protected virtual
#endif
		IEnumerable<Rectangle> AllScreenBounds => CachedScreenInfo.Instance.BoundsInfos;
		protected string WarningMessage
		{
			get
			{
				var warningMessage = Res.GetString("70DA97F8-EDBD-4EF0-A642-61DCB6E15250", "Your display settings do not meet the minimum requirements. The minimum display requirements are a resolution of at least 1366 x 768 without any scaling.{0}", System.Environment.NewLine);
				warningMessage += GetAllScreenResolutions();
				warningMessage += Res.GetString("D016C8FE-509C-46C7-8662-72DF4840819E", "Please adjust your resolution and scaling so that they meet the minimum requirements.");

				return warningMessage;
			}
		}

		public override int FailureExitCode => ExitCodes.ScreenResolutionCheckerError;
	}
}
