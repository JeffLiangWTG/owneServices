using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Integration.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NctsHeaderMessageWrapper : INctsHeaderMessageProvider
	{
		public NctsHeaderMessageWrapper(NctsHeader header, ICertificateProvider certificateData)
		{
			nctsHeader = Argument.NotNull(header, nameof(header));
			certificate = Argument.NotNull(certificateData, nameof(certificateData));
		}

		protected readonly NctsHeader nctsHeader;
		readonly ICertificateProvider certificate;

		public ZString BrokerCode => certificate.BrokerCode;

		public ZString CertificateName => certificate.CertificateName;

		public ZString CertificateThumbPrint => certificate.CertificateThumbPrint;

		public ZBlob CertificateBytes => certificate.CertificateBytes;

		public ZString DecryptedCertificatePassphrase => certificate.DecryptedCertificatePassphrase;

		public ZGuid CertificatePK => certificate.CertificatePK;

		public ZString CertificateID => certificate.CertificateID;

		public EDIMessageCollection Messages => messages ?? (messages = nctsHeader.Messages);
		EDIMessageCollection messages;

		public BusinessObjectFactory Factory => nctsHeader.Factory;

		public ZBool IsTest
		{
			get
			{
				var registration = ObjectFactory.Get<IProductRegistration>();
				return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
							&& !registration.IsWiseTechGlobalInternalSystem();
			}
		}

		public ZString DeclarantIdForUNBSegment => CachedValueHelper.GetValue(ref declarantIdForUNBSegment, () => DeclarantIdForUNBSegmentCore);

		CachedValue<ZString> declarantIdForUNBSegment;

		protected virtual ZString DeclarantIdForUNBSegmentCore => OrgHeaderExtension.GetIDCode(nctsHeader.DeclarantAddress?.Header);

		public ZString LocalReferenceNumber => LocalReferenceNumberCore;
		protected virtual ZString LocalReferenceNumberCore => nctsHeader.LocalReferenceNumber;

		public ZString CountryOfDestination => nctsHeader.MovementHeader?.BM_RL_NKDestinationPort ?? ZString.Empty; //Taken form NCTSDeparture declaration

		public IPartyProvider Consignor => CachedValueHelper.GetValue(ref consignor, () => PartyWrapper.New(nctsHeader.Consignor));
		CachedValue<IPartyProvider> consignor;

		public IPartyProvider Consignee => CachedValueHelper.GetValue(ref consignee, () => NctsHeaderConsigneeWrapper.New(nctsHeader.Consignee));
		CachedValue<IPartyProvider> consignee;

		public ZString BusinessObjectReference => nctsHeader.BH_JobReference;

		protected ZLong GetPackageAmountFromDepartureGoodItem(NctsDepartureCargoDesc goodItem) => !goodItem.IsVehicles ? GetPackageAmountFromNonDepartureGoodItem(goodItem) : goodItem.Packages.Count;

		protected ZLong GetPackageAmountFromNonDepartureGoodItem(NctsCommonCargoDesc goodItem)
		{
			var packages = goodItem.Packages;
			return packages.Any()
					? (ZLong)packages.Cast<EU.NCTS.Business.NctsPackage>().Sum(pack => pack.IsBulk ? 1 : pack.B5_UnitCount)
					: ZLong.Zero;
		}
	}
}
