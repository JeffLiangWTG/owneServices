using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Licensing;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory
{
	/// <summary>
	/// This does not provide any business rule logic about whether a profile requires a shed/airline/etc profile. The consumer must do that.  	
	/// </summary>
	public static class LicenceAndPimaHelper
	{
		public const string ShedProfilePrefix = "CUKAIR98";
		public const string AgentProfilePrefix = "CUKFFW98";

		public static void CheckHasShedLicenceButDoNotDetermineWhetherOneIsNeeded(ZPropertyInfo property, string explanationForWhyLicenceRequired)
		{
			if (!ShedEnabled)
			{
				property.AddError(explanationForWhyLicenceRequired);
			}
		}

		public static void CheckHasShedLicenceButDoNotDetermineWhetherOneIsNeeded(ErrorCollector ec, string explanationForWhyLicenceRequired)
		{
			if (!ShedEnabled)
			{
				ec.AddError(explanationForWhyLicenceRequired, null);
			}
		}

		public static bool DEPEnabled
		{
			get { return GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.Value; }
		}

		public static bool ShedEnabled
		{
			get { return true; }
		}

		public static bool AgentEnabled
		{
			get { return true; }
		}

		/// <summary>
		/// Profile is that of a full shed (CUKAIR98...)
		/// </summary>
		public static bool IsFullShed(ICcsukCusAwb awb)
		{
			return IsShedPIMA(awb.Profile) && GetShedTypeFromRegistry(awb) == ShedProfileType.FullShed;
		}

		/// <summary>
		/// Profile is that of an agent in shed fallback
		/// </summary>
		public static bool IsFallbackShed(ICcsukCusAwb awb)
		{
			return IsShedPIMA(awb.Profile) && GetShedTypeFromRegistry(awb) == ShedProfileType.AgentFallbackForShed;
		}

		public static bool IsShedPIMA(ZString pima)
		{
			return pima.StartsWith(LicenceAndPimaHelper.ShedProfilePrefix, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsAgentPIMA(ZString pima)
		{
			return pima.StartsWith(LicenceAndPimaHelper.AgentProfilePrefix, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Profile is that of an agent (CUKFFW98...) that is NOT ticked as an airline in fallback
		/// </summary>		
		public static bool IsSimpleAgentProfile(ICcsukCusAwb awb)
		{
			return IsAgentPIMA(awb.Profile);
		}

		static ShedProfileType GetShedTypeFromRegistry(ICcsukCusAwb awb)
		{
			var result = ShedProfileType.FullShed;
			if (awb != null && awb.Branch != null)
			{
				var credentials = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(awb.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				foreach (CredentialsSetting cred in credentials)
				{
					if (cred.PIMA == awb.Profile)
					{
						result = cred.FallbackForShed.IsEmpty ? ShedProfileType.FullShed : ShedProfileType.AgentFallbackForShed;
						break;
					}
				}
			}
			return result;
		}

		public static void SetBranchFromPimaForNewAwb(CusMAWB mawb)
		{
			// By default mawbs are created using the "current branch" which is that of the service task.  Update this to reflect the recipient PIMA as per the registry. 			
			mawb.CM_GB = RegistryPimaAndBadgeHelper.GetPrimaryBranchPkFromRegistryBasedOnPima(mawb.Profile, mawb.Factory);
		}

		enum ShedProfileType
		{
			AgentFallbackForShed,
			FullShed
		}

		public enum CcsukLicenceType
		{
			Base,
			Agent,
			ErtsShed,
			DEP
		}

		public const string PostArrivalRequiresShedFunctionsEnabled = "When NPR=0 and arrival date is missing, the consignment is deemed a pre-arrival record, otherwise it is a post-arrival. Only sheds (and airline agents in fallback) may create and edit post-arrival records.";
		public const string ShedFunctionsNeedToBeEnabled = "Your company is not enabled to perform this function.  Please enable shed functions in the registry";

		internal static void RecordCcsukLicenceLogin(CusHAWB hawbOrWorkerHawb, CcsukLicenceType type)
		{
			var checkPoint = Env.Licence.AirCcsukBase;
			if (type == CcsukLicenceType.Agent)
			{
				checkPoint = Env.Licence.AirCcsuk;
			}
			else if (type == CcsukLicenceType.ErtsShed)
			{
				checkPoint = Env.Licence.AirCcsukShed;
			}
			var e = new CcsukLicenceLoginEventArgs(checkPoint, type);
			hawbOrWorkerHawb.RaiseCcsukLicenceLogin(e);
			if (e.LoginHasBeenAttempted && !checkPoint.IsLoggedIn)
			{
				hawbOrWorkerHawb.CS_FolioReferenceInfo.AddError(string.Format("This PIMA requires the {0} licence.\r\n{1}", checkPoint.DisplayName, checkPoint.LastReasonForNotAllowing));
			}
		}

		public class CcsukLicenceLoginEventArgs : Customs.Business.LicenceLoginEventArgs
		{
			public CcsukLicenceLoginEventArgs(LicenceCheckpoint checkPoint, CcsukLicenceType type)
				: base(checkPoint)
			{
				this.CcsukLicenceType = type;
			}
			public CcsukLicenceType CcsukLicenceType { get; private set; }
		}

		public static void RecordLicenceLoginBasedOnProfileOfAncillaryJob(ZString pima, ZPropertyInfo zPropertyInfo, ILicenceLoginProvider ancillaryJob)
		{
			var checkPoint = Env.Licence.AirCcsukBase;
			var licenceType = LicenceAndPimaHelper.CcsukLicenceType.Base;
			if (IsShedPIMA(pima))
			{
				checkPoint = Env.Licence.AirCcsukShed;
				licenceType = LicenceAndPimaHelper.CcsukLicenceType.ErtsShed;
			}
			else if (IsAgentPIMA(pima))
			{
				licenceType = LicenceAndPimaHelper.CcsukLicenceType.Agent;
				checkPoint = Env.Licence.AirCcsuk;
			}

			if (!zPropertyInfo.HasErrors()) // includes a security check on the shed right, so doesn't record hit if user has no right
			{
				var eventArg = new CcsukLicenceLoginEventArgs(checkPoint, licenceType);
				ancillaryJob.RaiseCcsukLicenceLogin(eventArg);
			}
		}

		public interface ILicenceLoginProvider
		{
			void RaiseCcsukLicenceLogin(CcsukLicenceLoginEventArgs e);
			event Customs.Business.LicenceLoginEventHandler CcsukLicenceLoginHandler;
		}

		internal static bool IsEtsfShed(ICcsukCusAwb awb)
		{
			return awb.Profile.EndsWith("X", StringComparison.OrdinalIgnoreCase);
		}
	}
}
