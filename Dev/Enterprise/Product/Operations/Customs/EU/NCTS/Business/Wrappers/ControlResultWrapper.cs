using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class ControlResultWrapper : IControlResult
	{
		public ControlResultWrapper(ResultsOfControlAddInfo resultsOfControl)
		{
			this.resultsOfControl = Argument.NotNull(resultsOfControl, nameof(resultsOfControl));
		}

		public ZString CorrectedValue => resultsOfControl.G9_CorrectedValue;
		public ZString Description => resultsOfControl.G9_Description.Left(140);
		public ZString DescriptionLNG => ZString.Empty;
		public ZString PointerToTheAttribute => resultsOfControl.G9_PointerToTheAttribute;
		public ZString ControlIndicator => resultsOfControl.G9_ControlIndicator;

		readonly ResultsOfControlAddInfo resultsOfControl;
	}
}
