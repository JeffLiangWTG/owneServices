using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(AbandonDetails))]
	internal class AbandonDetailsTest : UPECusHAWBFlagDetailsTestCase
	{
		protected override UPECusHAWBFlagDetails GetNewUPECusHAWBFlagDetails(UPECusHAWB uPECusHAWB)
		{
			return new AbandonDetails(uPECusHAWB);
		}

		protected override string ExpectedNoteDescription
		{
			get
			{
				return "Abandon Note";
			}
		}

		protected override string ExpectedNoteReference
		{
			get
			{
				return "";
			}
		}
	}
}
