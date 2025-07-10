using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	public class NonPersistentStandAloneFsrEnquiryForNew : AutoNonPersistentStandAloneFsrEnquiryForNew
		, IFSR
		, LicenceAndPimaHelper.ILicenceLoginProvider
	{
		public NonPersistentStandAloneFsrEnquiryForNew(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ZString AwbNumberFormatted
		{
			get
			{
				var sb = new ZStringBuilder();
				var mawbFormatted = ZString.Format("{0}-{1}", MAWB.Left(3), MAWB.KeepAlphanumericCharacters().Right(8));
				sb.Append(mawbFormatted);
				if (!HAWB.IsEmpty)
				{
					sb.Append("-");
					sb.Append(HAWB);
				}
				if (!SRF.IsEmpty)
				{
					sb.Append("/");
					sb.Append(SRF);
				}
				return sb.ToString();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			if (Profiles.Count == 1)
			{
				PIMA = Profiles[0].Code;
			}
			DatabaseToQuery = FsrRequestType.Codes.FsaEnquiry;
		}

		public ZBool IsValid
		{
			get
			{
				Validation.ValidateAll();
				RefreshBinding(); // to make a red error on the PIMA field show up to the user even if they have not touched the control
				return !(MAWBInfo.HasNotifications() ||
					HAWBInfo.HasNotifications() ||
					SRFInfo.HasNotifications() ||
					ShedInfo.HasNotifications() ||
					AirportInfo.HasNotifications() ||
					DatabaseToQueryInfo.HasNotifications() ||
					PIMAInfo.HasNotifications());
			}
		}

		[List(nameof(Profiles))]
		public override ZString PIMA
		{
			get { return base.PIMA; }
			set { base.PIMA = value; }
		}

		[List(nameof(Databases))]
		[MaxLength(3)]
		public override ZString DatabaseToQuery
		{
			get { return base.DatabaseToQuery; }
			set { base.DatabaseToQuery = value; }
		}

		public CodeDescriptionPairList Profiles
		{
			get { return CusHAWBLookups.GetProfilesList(null, Factory); }
		}

		public CodeDescriptionPairList Databases
		{
			get { return new FsrRequestType(); }
		}

		ZString IFSR.AirwaybillPrefixAndAirwaybillNumber
		{
			get { return MAWB.KeepAlphanumericCharacters(); }
		}

		ZString IFSR.HousewaybillNumber
		{
			get { return HAWB; }
		}

		ZString IFSR.SplitReference
		{
			get { return SRF; }
		}

		ZString IFSR.ResponseRequiredIndicator
		{
			get { return DatabaseToQuery; }
		}

		ZString IFSR.Airport
		{
			get { return Airport; }
		}

		ZString IFSR.ShedOperatorIdentity
		{
			get { return Shed; }
		}

		ZString IFSR.RecipientID
		{
			get { return LicenceAndPimaHelper.ShedProfilePrefix + Airport + Shed; }  // used for type=FSN (for retransmission of FSN)
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
