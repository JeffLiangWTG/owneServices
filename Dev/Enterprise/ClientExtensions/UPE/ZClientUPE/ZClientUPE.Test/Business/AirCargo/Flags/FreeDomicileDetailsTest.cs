using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(FreeDomicileDetails))]
	internal class FreeDomicileDetailsTest : UPECusHAWBFlagDetailsTestCase
	{
		protected override UPECusHAWBFlagDetails GetNewUPECusHAWBFlagDetails(UPECusHAWB uPECusHAWB)
		{
			return new FreeDomicileDetails(uPECusHAWB);
		}

		protected override string ExpectedNoteDescription
		{
			get
			{
				return "Free Domicile Note";
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
