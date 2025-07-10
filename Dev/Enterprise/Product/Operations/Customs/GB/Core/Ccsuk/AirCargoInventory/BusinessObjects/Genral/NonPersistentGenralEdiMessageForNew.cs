using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral
{
	public class NonPersistentGenralEdiMessageForNew : AutoNonPersistentGenralEdiMessageForNew
		, LicenceAndPimaHelper.ILicenceLoginProvider
	{
		public NonPersistentGenralEdiMessageForNew(BusinessObjectFactory factory)
			: base(factory)
		{ }

		[ReadOnlyMember(nameof(ShedOrBadgeReadOnly))]
		public override ZString ShedOrBadge
		{
			get { return base.ShedOrBadge; }
			set
			{
				base.ShedOrBadge = value;
				GeneratePima();
			}
		}

		[ReadOnlyMember(nameof(AirportReadOnly))]
		public override ZString Airport
		{
			get { return base.Airport; }
			set
			{
				base.Airport = value;
				GeneratePima();
			}
		}

		ZString recipientType;
		ZString RecipientType
		{
			get { return recipientType; }
			set
			{
				recipientType = value;
				GeneratePima();
			}
		}

		[MaxLength(20)] // not 14, needs to accomodate the HMRC suffix 
		[ReadOnlyMember(nameof(PimaReadOnly))]
		public override ZString Pima
		{
			get { return base.Pima; }
			set { base.Pima = value; }
		}

		[List(nameof(SendingProfileList))]
		[MaxLength(CcsukConstants.PimaMaxLength)]
		public override ZString SendingProfile
		{
			get { return base.SendingProfile; }
			set { base.SendingProfile = value; }
		}

		public CodeDescriptionPairList SendingProfileList
		{
			get { return RegistryPimaAndBadgeHelper.GetProfilesKnownToChiefList(GlbBranch.CurrentBranch, Factory, LicenceAndPimaHelper.ShedEnabled, LicenceAndPimaHelper.DEPEnabled, false); }
		}

		public CodeDescriptionPairList RecipientTypesList
		{
			get { return new CcsukParticipantTypesToWhomSendingGenralMessageIsAllowed(); }
		}

		public CodeDescriptionPairList RecipientPimasList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				if (IsRecipientCustoms)
				{
					list = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsPimaOrTerminalAddress, ZDateTime.Today);
				}
				else
				{
					list.AddPairIfNotExist(CalculatePima(), Res.GetString("a637e55c-30c3-48f5-8849-3359e83c38ac", "Calculated Recipient"));
				}
				list.SortByDescription();
				return list;
			}
		}

		public ZBool IsValid
		{
			get
			{
				Validation.ValidateAll();
				RefreshBinding();
				return !(ShedOrBadgeInfo.HasNotifications() ||
							SendingProfileInfo.HasNotifications() ||
							AirportInfo.HasNotifications());
			}
		}

		void GeneratePima()
		{
			Pima = CalculatePima();
			PimaInfo.RefreshBinding();
			Validation.ValidateShedOrBadge();
			Validation.ValidateAirport();
		}

		ZString CalculatePima()
		{
			string airport = Airport.PadRight(3);
			string partyIdentifier = ShedOrBadge.PadRight(3);
			string receipientType = RecipientType.ToUpper();

			if (IsRecipientAgent)
			{
				airport = "000";
			}
			return string.Format("CUK{0}98{1}{2}", receipientType, airport, partyIdentifier);
		}

		public override ZBool IsRecipientAgent
		{
			get { return base.IsRecipientAgent; }
			set
			{
				base.IsRecipientAgent = value;
				if (value)
				{
					RecipientType = CcsukParticipantTypesToWhomSendingGenralMessageIsAllowed.Codes.FreightForwarder;
				}
			}
		}

		public override ZBool IsRecipientCustoms
		{
			get { return base.IsRecipientCustoms; }
			set
			{
				base.IsRecipientCustoms = value;
				if (value)
				{
					RecipientType = CcsukParticipantTypesToWhomSendingGenralMessageIsAllowed.Codes.Customs;
				}
			}
		}

		public override ZBool IsRecipientRollYourOwn
		{
			get { return base.IsRecipientRollYourOwn; }
			set
			{
				base.IsRecipientRollYourOwn = value;
				if (value)
				{
					RecipientType = "";
				}
			}
		}

		public override ZBool IsRecipientShed
		{
			get { return base.IsRecipientShed; }
			set
			{
				base.IsRecipientShed = value;
				if (value)
				{
					RecipientType = CcsukParticipantTypesToWhomSendingGenralMessageIsAllowed.Codes.AirlineOrShed;
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IsRecipientShed = true;
		}

		bool ShedOrBadgeReadOnly
		{
			get { return IsRecipientCustoms || IsRecipientRollYourOwn; }
		}

		bool AirportReadOnly
		{
			get { return IsRecipientAgent || IsRecipientCustoms || IsRecipientRollYourOwn; }
		}

		bool PimaReadOnly
		{
			get { return !(IsRecipientCustoms || IsRecipientRollYourOwn); }
		}

		public void PrepareNewMessageSentFromAwb(ICcsukCusAwb awb)
		{
			if (LicenceAndPimaHelper.IsFullShed(awb) || LicenceAndPimaHelper.IsFallbackShed(awb))
			{
				IsRecipientShed = false;
				IsRecipientAgent = true;
				ShedOrBadge = awb.AgentBadge;
			}
			else if (LicenceAndPimaHelper.IsSimpleAgentProfile(awb))
			{
				ShedOrBadge = awb.CargoTerminalOperator;
				Airport = awb.CargoTerminalOperatorAirport;
			}
			SendingProfile = awb.Profile;
			Payload = "Regarding " + awb.HumanReadableName + "...\r\n";
		}

		public event Customs.Business.LicenceLoginEventHandler CcsukLicenceLoginHandler;

		public void RaiseCcsukLicenceLogin(LicenceAndPimaHelper.CcsukLicenceLoginEventArgs e)
		{
			if (CcsukLicenceLoginHandler != null)
			{
				CcsukLicenceLoginHandler(this, e);
			}
		}
	}
}
