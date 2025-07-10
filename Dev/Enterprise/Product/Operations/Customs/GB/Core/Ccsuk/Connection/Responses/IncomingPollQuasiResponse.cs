using System;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class IncomingPollQuasiResponse : ShortMessageResponse
	{
		public IncomingPollQuasiResponse(string bodyText)
			: base(bodyText)
		{ }

		public override bool WasOperationSuccessful
		{
			get
			{
				return true;
			}
		}

		public override string ReasonForFailure
		{
			get
			{
				throw new NotImplementedException("A poll request is not a failure");
			}
		}
	}
}
