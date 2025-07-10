using CargoWise.Customs.GB.MessageDefinitions.NCTS.Phase4.cc044a;
using CargoWise.Types;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging
{
	class ResultsOfControlHelper<T> where T : IZType
	{
		public ResultsOfControlHelper(T expected, T actual, string pointerID, System.Func<int> incrementCounter = null)
		{
			this.expected = expected;
			this.actual = actual;
			this.pointerID = pointerID;
			this.incrementCounter = incrementCounter;
		}

		public Resofcon534Type GetResultOfControlDifference()
		{
			Resofcon534Type roc = null;
			if (!expected.Equals(actual))
			{
				var pointer = pointerID;
				if (incrementCounter != null)
				{
					pointer += incrementCounter().ToString();
				}
				roc = new Resofcon534Type()
				{
					ConInd424 = EU.NCTS.Business.ResultOfCOntrol.ResultOfControlCodes.Codes.Different, // DI
					PoiToTheAttToc5 = pointer,
					CorValToc4 = actual.ToString()
				};
			}
			return roc;
		}

		readonly System.Func<int> incrementCounter;
		readonly T expected;
		readonly T actual;
		readonly string pointerID;
	}
}
