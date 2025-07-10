using System.Diagnostics;
using CargoWise.Definitions;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Core;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public class ResourceStringsUpdaterTask : AbstractApplicationStartupTask
	{
		protected override bool DoExecute(CommandLineArguments arguments)
		{
			TranslationFeedbackLifecycleManager.UpdateCurrentResourceStrings();
			UndoTradosPreviewStrings();
			return true;
		}

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return (string)arguments[ApplicationArguments.OptionUpgrade] != null;
		}

		public override string TaskDescription => Res.GetString("be2bb9d6-d3a2-46a2-9820-8cf33e431692", "Updating Resource Strings");

		public override int FailureExitCode => ExitCodes.ResourceStringsUpdaterTaskError;

		[Conditional("DEBUG")]
		void UndoTradosPreviewStrings()
		{
			ResourceStringsFactory.UndoCheckout(ResourceStringsFactory.Load(new CargoWise.EntityFramework.ZQuery(HelpDataStringSchema.HD_EditReason, EditReasons.Codes.TradosPreview)));
		}
	}
}
