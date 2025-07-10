using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class CUSCARGeneratorFRC : CUSCARGeneratorFRI
	{
		public CUSCARGeneratorFRC(ICcsukCusAwb awb, ErrorCollector ec)
			: base(awb, ec)
		{
			// FRC on AWBs requires full shed or airline/fallback profile. 
			// FRC on split requires full shed, airline/normal or airline/fallback profile.
			// Using the FRC, therefore, requires shed to be enabled. 

			if (awb is SplitConsignment)
			{
				if (LicenceAndPimaHelper.IsSimpleAgentProfile(awb))
				{
					ec.AddError(FRcNotAllowedForSimpleAgents, null);
				}
			}
			else
			{
				if (!LicenceAndPimaHelper.IsFallbackShed(awb)
					&& !LicenceAndPimaHelper.IsFullShed(awb))
				{
					ec.AddError(FrcNotAllowedForSimpleAgentsOrAirlinesNotInFallback, null);
				}
			}
			LicenceAndPimaHelper.CheckHasShedLicenceButDoNotDetermineWhetherOneIsNeeded(ec, LicenceAndPimaHelper.ShedFunctionsNeedToBeEnabled);
		}

		protected override string AssociationAssignedCode
		{
			get { return "109503"; }
		}

		protected override string BgmDocumentName
		{
			get { return "FRC"; }
		}

		protected override string BgmDocumentNameHuman
		{
			get { return "Edit Freight Record"; }
		}

		protected override void MakeCommunityHandlingCodes()
		{
			CUSCARGeneratorFRI.MakeCommunityHandlingCodesShared(iCuscar, result.GIS);
		}

		protected override bool AlwaysSendNprEvenIfZero
		{
			get { return true; }  // Otherwise, if you reduce NPR to zero we'd omit this info from the message and NPR would not be amended on CCSUK (would remain as per last non-zero message)
		}

		public const string FrcNotAllowedForSimpleAgentsOrAirlinesNotInFallback = "Amending records using the FRC message is not allowed for simple agents or airline agents not in fallback.\r\nInstead delete with FRX then reinsert with FRI.";
		public const string FRcNotAllowedForSimpleAgents = "Amending records using the FRC message is not allowed for simple agents.\r\nInstead delete with FRX then reinsert with FRI.";
	}
}
