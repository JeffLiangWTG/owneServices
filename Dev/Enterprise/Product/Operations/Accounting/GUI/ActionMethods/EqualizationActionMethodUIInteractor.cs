using System;

namespace Enterprise.Accounting.GUI
{
	using Enterprise.Accounting.Business;
	using Enterprise.Integration;
	using Enterprise.Services.OperationalActions.Support;

	public class EqualizationActionMethodUIInteractor : ActionMethodUIInteractor
	{
		public EqualizationActionMethodUIInteractor(IOperationalActionSectionLog log)
			: base(log)
		{ }

		bool hasErrors;

		public override void Log(LogType type, string message, Exception ex)
		{
			switch (type)
			{
				case LogType.Error:
					hasErrors = true;
					break;

				case LogType.Information:
					if (!hasErrors && message.StartsWith(AccountingConstants.VolumeEqualizationMessages.DiscountResults, System.StringComparison.OrdinalIgnoreCase))
					{
						log.Notify(OperationalActionLogErrorLevel.Informational, message);
					}
					break;
			}

			base.Log(type, message, ex);
		}
	}
}
