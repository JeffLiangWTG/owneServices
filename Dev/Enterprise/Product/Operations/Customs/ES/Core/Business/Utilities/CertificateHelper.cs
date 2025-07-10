using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business
{
	public static class CertificateHelper
	{
		public static CodeDescriptionPairList CertificateNames(BusinessObjectFactory factory, GlbStaff broker, ZString cacheCodePrefix)
		{
			var brokerCode = broker?.GS_Code ?? ZString.Empty;
			return factory.GetCachedValue(cacheCodePrefix + ".CertificateNames_" + brokerCode, () =>
			{
				var result = new CodeDescriptionPairList();
				if (broker != null)
				{
					var wrapper = GlbStaffWrapper.Get(broker);
					var certificates = wrapper.ESBPasswordCollection.Cast<GlbExternalPassword>().Where(x => x.GP_PasswordStatus == PasswordStatusList.Codes.Valid).ToList();
					certificates.AddRange(GetAuthorisedCertificates(factory, broker));

					foreach (var (certificateName, brokerPK) in certificates.Select(x => (x.GP_Name.ToUpper(), x.GP_GS)).OrderBy(x => x))
					{
						var description = ZString.Empty;
						if (brokerPK != broker.PK)
						{
							var authCertBroker = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, brokerPK));
							description += authCertBroker?.GS_LoginName;
						}

						result.AddPair(certificateName, description);
					}
				}
				return result;
			});
		}

		public static void CheckCustomsProfile(ZPropertyInfo propertyInfo, ZString property, GlbStaff broker)
		{
			if (property.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(propertyInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(propertyInfo);

				if (!propertyInfo.HasMessageErrors() && !IsStaffAuthorised(propertyInfo, property, broker))
				{
					propertyInfo.AddMessageError(UnauthorisedCertificateMessageError);
				}
			}
		}

		static bool IsStaffAuthorised(ZPropertyInfo propertyInfo, ZString property, GlbStaff broker)
		{
			if (!propertyInfo.HasMessageErrors() && broker != null && !broker.IsCurrentUser)
			{
				var certificate = GetCertificate(broker, property);
				return certificate == null || ListOfAuthorisedUserForCertificate.Contains(GlbStaff.CurrentUser.GS_Code)  || certificate.Authorisations.Any(x => ((GlbExternalPasswordAuthorisation)x).GEA_GS_AuthorisedStaff == GlbStaff.CurrentUser.PK);
			}
			return true;
		}

		static ZString[] ListOfAuthorisedUserForCertificate
		{
			get { return new ZString[] { (NoResString)"~BP", (NoResString)"~AD" }; }
		}

		static GlbExternalPassword[] GetAuthorisedCertificates(BusinessObjectFactory factory, GlbStaff broker)
		{
			var entryNumberFilter = new ZDBOnlySubQuery(typeof(GlbExternalPasswordAuthorisation), GlbExternalPasswordAuthorisationSchema.GEA_GP);
			entryNumberFilter.AddToFilter(GlbExternalPasswordAuthorisationSchema.GEA_GS_AuthorisedStaff, broker.PK);

			var query = new ZDBOnlyQuery(typeof(GlbExternalPassword));
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordStatus, PasswordStatusList.Codes.Valid);
			query.AddSubQuery(entryNumberFilter, JoinCondition.And);
			return factory.Load<GlbExternalPassword>(query);
		}

		public static GlbExternalPassword GetCertificate(GlbStaff broker, ZString certificateName, string certificateTumbPrint = "")
		{
			GlbExternalPassword result = null;
			if (!certificateName.IsEmpty)
			{
				var brokerWrapper = GlbStaffWrapper.Get(broker);

				var certificates = brokerWrapper?.ESBPasswordCollection.Cast<GlbExternalPassword>().Where(x => x.GP_PasswordStatus == PasswordStatusList.Codes.Valid).ToList();
				if (broker != null)
				{
					certificates.AddRange(GetAuthorisedCertificates(broker.Factory, broker));
				}

				result = certificates?.OrderBy(x => x.GP_SystemCreateTimeUtc)
										.FirstOrDefault(x => x.GP_Name.EqualsIgnoringCase(certificateName) && (certificateTumbPrint.IsNullOrEmpty() || x.GP_UserID.EqualsIgnoringCase(certificateTumbPrint)));
			}
			return result;
		}

		static ZString UnauthorisedCertificateMessageError => Res.GetString("3ADD5E90-40F9-4C6B-A99B-7D928E502143", "You are not authorized to use this certificate. Please ask the Broker to authorize your user on the Staff & Resource module, Brokerage tab.");
	}
}
