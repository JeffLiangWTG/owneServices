using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECusHAWBFlagDetailsForTest))]
	internal class UPECusHAWBFlagDetailsTest_CoreFunctionality : UPECusHAWBFlagDetailsTestCase
	{
		public void TestValidateAll()
		{
			AssertEquals("Before validated, should have no errors", false, FlagDetails.HasErrors);
			FlagDetails.ValidateAll();
			AssertEquals(2, FlagDetails.Notifications.GetErrors().Count());
			AssertMandatoryValidationError(FlagDetails.PersonAuthorisedInfo, true);
			AssertMandatoryValidationError(FlagDetails.AuthorisationReceivedByInfo, true);
		}

		public void TestNoteRtf()
		{
			AssertEquals("Has to be courier new", "COURIER NEW", FlagDetails.RtfString.DefaultFont.Name.ToUpper());
			AssertEquals(8f, FlagDetails.RtfString.DefaultFont.Size);
		}

		#region Implementation
		protected override string ExpectedNoteReference
		{
			get
			{
				return "YADAYADAYADA";
			}
		}

		protected override string ExpectedNoteDescription
		{
			get
			{
				return "RARARARA Note";
			}
		}

		protected override UPECusHAWBFlagDetails GetNewUPECusHAWBFlagDetails(UPECusHAWB uPECusHAWB)
		{
			return new UPECusHAWBFlagDetailsForTest(uPECusHAWB);
		}

		new UPECusHAWBFlagDetailsForTest FlagDetails
		{
			get
			{
				return (UPECusHAWBFlagDetailsForTest)base.FlagDetails;
			}
		}

		#region UPECusHAWBFlagDetailsForTest
		class UPECusHAWBFlagDetailsForTest : UPECusHAWBFlagDetails
		{
			public UPECusHAWBFlagDetailsForTest(UPECusHAWB uPECusHAWB) : base(uPECusHAWB)
			{
			}

			public override string FlagName
			{
				get
				{
					return "RARARARA";
				}
			}

			public new FormattedRtfString RtfString
			{
				get
				{
					return base.RtfString;
				}
			}

			protected override string NoteReference
			{
				get
				{
					return "YADAYADAYADA";
				}
			}
		}
		#endregion
		#endregion
	}
}
