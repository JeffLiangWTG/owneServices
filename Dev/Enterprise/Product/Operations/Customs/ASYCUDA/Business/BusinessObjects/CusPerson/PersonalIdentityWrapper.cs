using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ASYCUDA.Business
{
	class PersonalIdentityWrapper
	{
		public PersonalIdentityWrapper(GlbStaff staff)
		{
			name = staff.GS_FullName;
			var pp = staff.Certificates.Find(c => c.XZ_Type == StaffCertificateType.PAS).FirstOrDefault();
			passportNumber = pp?.XZ_RefNumber ?? ZString.Empty;
			passportNationality = pp?.XZ_RN_NKCountryOfIssuance ?? ZString.Empty;
			passportExpires = pp?.XZ_ExpiryOrDueDate ?? ZDateTime.Empty;
			factory = staff.Factory;
		}

		public PersonalIdentityWrapper(OrgContact contact)
		{
			name = contact.OC_ContactName;
			var pp = contact.Certificates.Find(c => c.XZ_Type == StaffCertificateType.PAS).FirstOrDefault();
			passportNumber = pp?.XZ_RefNumber ?? ZString.Empty;
			passportNationality = pp?.XZ_RN_NKCountryOfIssuance ?? ZString.Empty;
			passportExpires = pp?.XZ_ExpiryOrDueDate ?? ZDateTime.Empty;

			factory = contact.Factory;
		}

		public GlbPerson TryReallyHardToGetExistingGlbPerson()
		{
			var qExactName = new ZQuery(GlbPersonSchema.PER_FullName, name);
			qExactName.AddToFilter(GlbPersonSchema.PER_FullName, SQLComparisonOperator.NotEqual, ZString.Empty);
			var qNameAndPassport = new ZQuery(qExactName);
			ZQuery qPassport = null;
			if (!passportNumber.IsEmpty)
			{
				qPassport = new ZQuery(GlbPersonSchema.PER_Passport, passportNumber);
				qPassport.AddToFilter(GlbPersonSchema.PER_PassportPlaceOfIssue, passportNationality);
				qPassport.AddToFilter(GlbPersonSchema.PER_PassportPlaceOfIssue, SQLComparisonOperator.NotEqual, ZString.Empty);
				qNameAndPassport.AddToFilter(qPassport, JoinCondition.And);
			}

			// 1. Try match on exact name and passport if present
			var glbPerson = factory.Load<GlbPerson>(qNameAndPassport).OrderBy(x => x.PER_SystemCreateTimeUtc).FirstOrDefault();

			if (glbPerson == null && !passportNumber.IsEmpty)
			{
				// 2. Nothing for exact name and passport, try just exact name
				glbPerson = factory.Load<GlbPerson>(qExactName).OrderBy(x => x.PER_SystemCreateTimeUtc).FirstOrDefault();
				if (glbPerson != null)
				{
					// We found a match using name, having also tried unsuccessfully using also passport. If we have a source passport, make use of it. 
					if (glbPerson.PER_Passport.IsEmpty)
					{
						glbPerson.PER_Passport = passportNumber;
						glbPerson.PER_PassportPlaceOfIssue = passportNationality;
						glbPerson.PER_PassportExpiryDate = passportExpires.Date;
					}
				}
			}

			if (glbPerson == null)
			{
				var soundexHelper = OrgPatternMatchGenerationHelper.Get("ENG", "", (NoResString)"");
				var soundexForDesiredStaffOrContactName = soundexHelper.CompanyNameSoundexWords(name);

				if (glbPerson == null && qPassport != null)
				{
					// 3. We have a passport on the staff/contact, now get all persons with matching passport and then soundex the names found
					foreach (var matchingPassport in factory.Load<GlbPerson>(qPassport))
					{
						var soundexForPossibleHit = soundexHelper.CompanyNameSoundexWords(matchingPassport.PER_FullName);
						if (SoundicesMatch(soundexForPossibleHit, soundexForDesiredStaffOrContactName))
						{
							glbPerson = matchingPassport;
							break;
						}

						if (glbPerson == null)
						{
							// 4. Still no joy. Those existing records whose passport mathes exaxtly and whose initials match.  If the staff name is Nit Wit...
							var queryForInitials = new ZQuery(GlbPersonSchema.PER_FullName, SQLComparisonOperator.StartsWith, name.Left(1));  // name like N%....
							foreach (var nameFragment in name.Split(' ').Skip(1))
							{
								queryForInitials.AddToFilter(GlbPersonSchema.PER_FullName, SQLComparisonOperator.Contains, " " + nameFragment.Left(1));  // name like  % W%
							}
							var queryForPassportAndInitials = new ZQuery();
							queryForPassportAndInitials.AddToFilter(qPassport);
							queryForPassportAndInitials.AddToFilter(queryForInitials);
							glbPerson = factory.Load<GlbPerson>(queryForPassportAndInitials).OrderBy(x => x.PER_SystemCreateTimeUtc).FirstOrDefault();
						}
					}
				}
			}
			return glbPerson;  // 5.  If this is still null, we'll make a new GlbPerson
		}

		bool SoundicesMatch(string[] soundexForPossibleHit, string[] soundexForDesiredStaffOrContactName)
		{
			var shortestArray = (soundexForDesiredStaffOrContactName.Length > soundexForPossibleHit.Length) ? soundexForPossibleHit : soundexForDesiredStaffOrContactName;
			for (int i = 0; i < shortestArray.Length; i++)
			{
				if (soundexForDesiredStaffOrContactName[i] != soundexForPossibleHit[i])
				{
					return false;
				}
			}
			return true;
		}

		readonly ZString name;
		readonly ZDateTime passportExpires;
		readonly ZString passportNumber;
		readonly ZString passportNationality;
		readonly BusinessObjectFactory factory;
	}
}
