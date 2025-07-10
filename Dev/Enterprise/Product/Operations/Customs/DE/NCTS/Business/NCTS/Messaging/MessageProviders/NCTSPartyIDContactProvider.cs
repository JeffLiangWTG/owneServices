using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.OrgConstants;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSPartyIDContactProvider : INCTSPartyIDContact, IEquatable<NCTSPartyIDContactProvider>
	{
		public static NCTSPartyIDContactProvider NewOrNull(JobDocAddress address, GlbStaff staff = null, bool provideContact = true) => address != null ? new NCTSPartyIDContactProvider(address, staff, provideContact) : null;

		public static NCTSPartyIDContactProvider NewOrNull(JobDocAddress docAddress, bool fallback) => docAddress != null ? new NCTSPartyIDContactProvider(docAddress, fallback) : null;

		protected NCTSPartyIDContactProvider(JobDocAddress docAddress, GlbStaff staff, bool provideContact)
		{
			DocAddress = Argument.NotNull(docAddress, nameof(docAddress));
			OrgAddress = DocAddress.Address;
			OrgHeader = OrgAddress?.Header;
			this.staff = staff;
			staffMode = staff != null && provideContact;
			if (!staffMode && provideContact)
			{
				cusOrgContact = OrgHeader?.Contacts.GetContactForAllocation(ContactAllocationType.CUS);
			}
		}

		protected NCTSPartyIDContactProvider(JobDocAddress docAddress, bool fallback)
		{
			DocAddress = Argument.NotNull(docAddress, nameof(docAddress));
			OrgAddress = docAddress.Address;
			OrgHeader = OrgAddress?.Header;
			cusOrgContact = DocAddress.Contact;
			if (fallback && cusOrgContact == null)
			{
				cusOrgContact = OrgHeader?.Contacts.GetContactForAllocation(ContactAllocationType.CUS);
			}

			staff = null;
			staffMode = false;
		}

		public string Name
		{
			get
			{
				if (staffMode)
				{
					return staff.GS_FullName.ValueOrNullIfEmpty();
				}
				else if (DocAddress.E2_AddressOverride)
				{
					return DocAddress.E2_Contact.ValueOrNullIfEmpty();
				}
				else
				{
					return cusOrgContact?.OC_ContactName.ValueOrNullIfEmpty();
				}
			}
		}

		public string PhoneNumber
		{
			get
			{
				if (staffMode)
				{
					return staff.GS_WorkPhone.ValueOrNullIfEmpty();
				}
				else if (DocAddress.E2_AddressOverride)
				{
					return DocAddress.E2_Phone.ValueOrNullIfEmpty();
				}
				else
				{
					return cusOrgContact?.OC_Phone.ValueOrNullIfEmpty();
				}
			}
		}

		public string MailAddress
		{
			get
			{
				if (staffMode)
				{
					return staff.GS_EmailAddress.ValueOrNullIfEmpty();
				}
				else if (DocAddress.E2_AddressOverride)
				{
					return DocAddress.E2_Email.ValueOrNullIfEmpty();
				}
				else
				{
					return cusOrgContact?.OC_Email.ValueOrNullIfEmpty();
				}
			}
		}

		public string EoriNumber => OrgHeader.GetEUEoriDetails().ValueOrNullIfEmpty() ?? OrgHeader?.GetConcatenatedSingleOrgCusCodeIgnoringCountry(EuropeanUnionFriendsThirdCountry.TCU).ValueOrNullIfEmpty();

		public string EoriBranchSuffix => OrgAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix).ValueOrNullIfEmpty();

		#region to be cleaned up
		string INCTSPartyIDContact.Position => throw new NotImplementedException();

		string INCTSPartyIDContact.FacsimileNumber => throw new NotImplementedException();

		string IPartyID.TCUNumber => throw new NotImplementedException();
		#endregion

		readonly bool staffMode;
		protected readonly JobDocAddress DocAddress;
		readonly GlbStaff staff;
		readonly OrgContact cusOrgContact;
		protected readonly OrgHeader OrgHeader;
		protected readonly OrgAddress OrgAddress;

		#region IEquatable implementation
		public override bool Equals(object obj)
		{
			if (obj is NCTSPartyIDContactProvider nCTSPartyIDContactProvider)
			{
				return Equals(nCTSPartyIDContactProvider);
			}

			return base.Equals(obj);
		}

		public static bool operator ==(NCTSPartyIDContactProvider first, NCTSPartyIDContactProvider second)
		{
			if ((object)first == null)
			{
				return (object)second == null;
			}

			return first.Equals(second);
		}

		public static bool operator !=(NCTSPartyIDContactProvider first, NCTSPartyIDContactProvider second)
		{
			return !(first == second);
		}

		public bool Equals(NCTSPartyIDContactProvider other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(Name, other.Name) && Equals(PhoneNumber, other.PhoneNumber) && Equals(MailAddress, other.MailAddress) && Equals(EoriNumber, other.EoriNumber) && Equals(EoriBranchSuffix, other.EoriBranchSuffix);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 47;
				hashCode = (hashCode * 53) ^ Name.GetHashCode();

				if (PhoneNumber != null)
				{
					hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(PhoneNumber);
				}

				if (MailAddress != null)
				{
					hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(MailAddress);
				}

				if (EoriNumber != null)
				{
					hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(EoriNumber);
				}

				if (EoriBranchSuffix != null)
				{
					hashCode = (hashCode * 53) ^ EqualityComparer<string>.Default.GetHashCode(EoriBranchSuffix);
				}

				return hashCode;
			}
		}
		#endregion	}
	}
}
