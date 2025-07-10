using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class NctsMessageFunctionSetsProvider : EU.NCTS.Business.MessageGeneration.NctsMessageFunctionSetsProvider
	{
		protected override NctsMessageFunctionSet GetMessageFunctionCore(ZString messageCode)
		{
			switch (messageCode)
			{
				case "F15":
					return new FRNctsMessageFunctionSet.PrelodgeValidationMessage();
				default:
					return base.GetMessageFunctionCore(messageCode);
			}
		}
	}
}
