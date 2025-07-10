using System;
using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public class CustomsOfficeRequirement
	{
		public CustomsOfficeRequirement()
		{
		}

		public CustomsOfficeRequirement(string officeRole, bool isMandatory, bool isLocalCountryOnly, string friendlyName = "")
			: this(officeRole, isMandatory, isLocalCountryOnly, false, friendlyName)
		{
		}

		public CustomsOfficeRequirement(string officeRole, bool isMandatory, bool isLocalCountryOnly, bool isForeignCountryOnly, string friendlyName = "")
			: this(officeRole, isMandatory, isLocalCountryOnly, false, true, friendlyName)
		{
		}

		public CustomsOfficeRequirement(string officeRole, bool isMandatory, bool isLocalCountryOnly, bool isForeignCountryOnly, bool isRecommended, string friendlyName = "")
		{
			OfficeRole = officeRole;
			IsMandatory = isMandatory;
			IsLocalCountryOnly = isLocalCountryOnly;
			FriendlyName = friendlyName;
			IsForeignCountryOnly = isForeignCountryOnly;
			IsRecommended = isRecommended;
		}

		public ZString OfficeRole { get; set; }

		public ZBool IsMandatory { get; set; }

		public ZBool IsLocalCountryOnly { get; set; }

		public ZBool IsForeignCountryOnly { get; set; }

		public ZBool IsRecommended { get; set; }

		public ZString ValidationMessage { get; set; }

		public ZInt? MaxOfficeCountLimit
		{
			get => maxOfficeCountLimit;
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException(nameof(MaxOfficeCountLimit), "MaxOfficeCountLimit must be positive number or null for unlimited");
				}
				maxOfficeCountLimit = value;
			}
		}
		ZInt? maxOfficeCountLimit = 1;

		public IEnumerable<ZString> OfficeRolesForLookup
		{
			get => officeRolesForLookup ?? (new ZString[] { OfficeRole });
			set => officeRolesForLookup = value;
		}
		IEnumerable<ZString> officeRolesForLookup;

		public ZString FriendlyName
		{
			get
			{
				if (friendlyName.IsEmpty)
				{
					var description = (ZString)OfficeRoleList.GetDescriptionFromCode(OfficeRole);
					if (description.IsEmpty)
					{
						return Res.GetString("70953486-a307-4263-902a-a38317c495f2", "Customs Office");
					}
					return description;
				}
				return friendlyName;
			}
			set => friendlyName = value;
		}
		ZString friendlyName;

		public ICodeDescriptionPairList OfficeRoleList => customsOfficeRoleList ?? (customsOfficeRoleList = GetCustomsOfficeRoleList());
		ICodeDescriptionPairList customsOfficeRoleList;

		protected ICodeDescriptionPairList GetCustomsOfficeRoleList()
		{
			return new EuOfficeCodesTypes();
		}

		public override bool Equals(object obj)
		{
			return obj is CustomsOfficeRequirement other && other.GetType() == GetType() && other.OfficeRole == OfficeRole && other.OfficeRolesForLookup == OfficeRolesForLookup && other.IsMandatory == IsMandatory && other.IsLocalCountryOnly == IsLocalCountryOnly && other.IsRecommended == IsRecommended && other.FriendlyName == FriendlyName && other.IsForeignCountryOnly == IsForeignCountryOnly && other.MaxOfficeCountLimit == MaxOfficeCountLimit;
		}

		public override int GetHashCode()
		{
			return OfficeRole.GetHashCode() ^ OfficeRolesForLookup.GetHashCode() ^ IsMandatory.GetHashCode() ^ IsLocalCountryOnly.GetHashCode() ^ IsRecommended.GetHashCode() ^ FriendlyName.GetHashCode() ^ IsForeignCountryOnly.GetHashCode() ^ MaxOfficeCountLimit.GetHashCode();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public override string ToString()
		{
			return $"Type: {GetType().FullName}, OfficeRole: {OfficeRole}, OfficeRoleForLookup: {OfficeRolesForLookup}, IsMandatory: {IsMandatory}, IsLocalCountryOnly: {IsLocalCountryOnly}, IsRecommended: {IsRecommended}, FriendlyName: {FriendlyName}, IsForeignCountryOnly: {IsForeignCountryOnly}, MaxOfficeCountLimit: {MaxOfficeCountLimit}";
		}
	}
}
