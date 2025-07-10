using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor
{
	public class FaxConfigObj : AutoFaxConfigObj
	{
		public FaxConfigObj(IServiceTaskSchedule parent)
			: base(((BusinessObject)parent).Factory)
		{
			_parent = parent;

			using (SuspendSettingHasChanges())
			{
				FromXml(_parent.ConfigString);

				if (LocalCountry.IsEmpty)
				{
					LocalCountry = _parent.GetBranchCountryCode();
				}
			}
			((BusinessObject)_parent).RegisterEditableChildObject(this);
		}

		[List("Lookups.Countries")]
		public override ZString LocalCountry
		{
			set
			{
				bool hasChanges = LocalCountry != value;
				base.LocalCountry = value;
				if (hasChanges && Lookups.InternationalCallPrefixes.Count > 0 && !Lookups.InternationalCallPrefixes.ContainsCode(InternationalCallPrefix))
				{
					InternationalCallPrefix = Lookups.InternationalCallPrefixes[0].Code;
				}
			}
		}

		public override ZString LongDistanceCallPrefix
		{
			get { return CountryPhoneInformation == null ? ZString.Empty : (ZString)CountryPhoneInformation.NationalPrefix; }
		}

		[List("Lookups.InternationalCallPrefixes")]
		public override ZString InternationalCallPrefix
		{
			set { base.InternationalCallPrefix = value; }
		}

		#region CountryPhoneInfo

		public PhoneInfo CountryPhoneInformation
		{
			get
			{
				if (_countryPhoneInfo == null || _countryPhoneInfo.CountryIsoCode != LocalCountry)
				{
					_countryPhoneInfo = PhoneInfo.GetPhoneInfoByCountryIsoCode(LocalCountry);
				}

				return _countryPhoneInfo;
			}
		}

		PhoneInfo _countryPhoneInfo;

		#endregion

		#region Ports

		public FaxPortConfigObjCollection Ports
		{
			get
			{
				if (_ports == null)
				{
					_ports = new FaxPortConfigObjCollection(Factory);
					RegisterEditableChildObject(_ports);
				}

				return _ports;
			}
		}

		public FaxPortConfigObjCollection _ports;

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			_parent.ConfigString = ToXml();
		}

		void FromXml(string xml)
		{
			FaxConfigSettings settings = FaxConfigSettings.FromXml<FaxConfigSettings>(xml);
			if (settings != null)
			{
				LocalCountry = settings.LocalCountry;
				LocalAreaCode = settings.LocalAreaCode;
				LocalID = settings.LocalID;
				OutsideLinePrefix = settings.OutsideLinePrefix;
				InternationalCallPrefix = settings.InternationalCallPrefix;
				EnableLogging = settings.EnableLogging;
				settings.Ports.ForEach(port =>
				{
					var portConfigObj = Ports.AddNew();
					using (portConfigObj.SuspendSettingHasChanges())
					{
						portConfigObj.PortName = port.PortName;
					}
				});
			}
		}

		string ToXml()
		{
			var settings = new FaxConfigSettings()
			{
				LocalCountry = this.LocalCountry,
				LocalAreaCode = this.LocalAreaCode,
				LocalID = this.LocalID,
				OutsideLinePrefix = this.OutsideLinePrefix,
				InternationalCallPrefix = this.InternationalCallPrefix,
				EnableLogging = this.EnableLogging
			};
			var portConfiguration = new FaxPortConfigSettings[Ports.Count];
			for (var i = 0; i < Ports.Count; i++)
			{
				portConfiguration[i] = new FaxPortConfigSettings() { PortName = Ports[i].PortName };
			}
			settings.Ports = portConfiguration;

			return settings.AsXml();
		}

		#region Lookups

		public FaxConfigObjLookups Lookups
		{
			get { return lookups ?? (lookups = new FaxConfigObjLookups(this)); }
		}

		FaxConfigObjLookups lookups;

		#endregion

		readonly IServiceTaskSchedule _parent;
	}
}
