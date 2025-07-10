using CargoWise.Types;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebUserDefaultSettingsForDocAddress
	{
		#region Constructors

		public WebUserDefaultSettingsForDocAddress(ZGuid organisationPK, ZGuid addressPK, ZGuid contactPK, ZString docAddressType)
		{
			OrganisationPK = organisationPK;
			AddressPK = addressPK;
			ContactPK = contactPK;
			DocAddressType = docAddressType;
		}

		internal WebUserDefaultSettingsForDocAddress(string value)
		{
			Parse(value);
		}

		#endregion

		#region Properties

		public ZGuid OrganisationPK { get; set; }
		public ZGuid AddressPK { get; set; }
		public ZGuid ContactPK { get; set; }
		public ZString DocAddressType { get; set; }

		public bool IsEmpty
		{
			get
			{
				return OrganisationPK.IsEmpty &&
					AddressPK.IsEmpty &&
					ContactPK.IsEmpty &&
					DocAddressType.IsEmpty;
			}
		}

		#endregion

		#region Methods

		public override string ToString()
		{
			return string.Format("{0};{1};{2};{3}", OrganisationPK, AddressPK, ContactPK, DocAddressType);
		}

		#endregion

		#region Implementation

		void Parse(string value)
		{
			bool succeed = false;

			if (!string.IsNullOrEmpty(value))
			{
				string[] args = value.Split(new char[] { ';' });
				if (args.Length == 4)
				{
					try
					{
						OrganisationPK = new ZGuid(args[0]);
						AddressPK = new ZGuid(args[1]);
						ContactPK = new ZGuid(args[2]);
						DocAddressType = args[3];

						succeed = true;
					}
					catch (ZTypeValueException) { }
				}
			}

			if (!succeed)
			{
				OrganisationPK = ZGuid.Empty;
				AddressPK = ZGuid.Empty;
				ContactPK = ZGuid.Empty;
				DocAddressType = ZString.Empty;
			}
		}

		#endregion
	}
}
