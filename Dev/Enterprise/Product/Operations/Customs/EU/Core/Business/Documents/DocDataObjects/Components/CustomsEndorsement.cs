using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects
{
	public class CustomsEndorsement : NonPersistentBusinessObject
	{
		public CustomsEndorsement(ICustomsEndorsement customsEndorsement, BusinessObjectFactory factory, CustomsEndorsementMaxLength maxLengthInfo) : base(factory)
		{
			this.customsEndorsement = Argument.NotNull(customsEndorsement, nameof(customsEndorsement));
			this.maxLengthInfo = maxLengthInfo;
		}

		readonly ICustomsEndorsement customsEndorsement;
		readonly CustomsEndorsementMaxLength maxLengthInfo;

		#region Form

		[BusinessObjectMaxLengthTestExclude]
		public ZString Form
		{
			get => (form ?? (form = customsEndorsement.Form)).Value.Substring(0, maxLengthInfo?.FormInDocument ?? 100).ToUpperInvariant();
			set => SetNonPersistentPropertyValue(FormInfo, ref form, value);
		}

		ZString? form;

		public ZPropertyInfo FormInfo => GetZPropertyInfo(nameof(Form));

		#endregion

		#region FormNo

		[BusinessObjectMaxLengthTestExclude]
		public ZString FormNo
		{
			get => (formNo ?? (formNo = customsEndorsement.FormNo)).Value.Substring(0, maxLengthInfo?.NumberInDocument ?? 100).ToUpperInvariant();
			set => SetNonPersistentPropertyValue(FormNoInfo, ref formNo, value);
		}

		ZString? formNo;

		public ZPropertyInfo FormNoInfo => GetZPropertyInfo(nameof(FormNo));

		#endregion

		#region OfDate

		public ZDate OfDate
		{
			get => (ofDate ?? (ofDate = customsEndorsement.Date)).Value;
			set => SetNonPersistentPropertyValue(OfDateInfo, ref ofDate, value);
		}

		ZDate? ofDate;

		public ZPropertyInfo OfDateInfo => GetZPropertyInfo(nameof(OfDate));

		#endregion

		#region Customs Office

		[BusinessObjectMaxLengthTestExclude]
		public ZString CustomsOffice
		{
			get => (customsOffice ?? (customsOffice = customsEndorsement.CustomsOffice)).Value.Substring(0, maxLengthInfo?.CustomsInDocument ?? 100).ToUpperInvariant();
			set => SetNonPersistentPropertyValue(CustomsOfficeInfo, ref customsOffice, value);
		}

		ZString? customsOffice;

		public ZPropertyInfo CustomsOfficeInfo => GetZPropertyInfo(nameof(CustomsOffice));

		#endregion

		#region IssuingCountry

		[BusinessObjectMaxLengthTestExclude]
		public ZString IssuingCountry
		{
			get => (issuingCountry ?? (issuingCountry = customsEndorsement.IssuingCountry)).Value.Substring(0, maxLengthInfo?.IssuingInDocument ?? 100).ToUpperInvariant();
			set => SetNonPersistentPropertyValue(IssuingCountryInfo, ref issuingCountry, value);
		}

		ZString? issuingCountry;

		public ZPropertyInfo IssuingCountryInfo => GetZPropertyInfo(nameof(IssuingCountry));

		#endregion

		#region Place

		[BusinessObjectMaxLengthTestExclude]
		public ZString Place
		{
			get => (place ?? (place = customsEndorsement.Place)).Value.Substring(0, maxLengthInfo?.PlaceInDocument ?? 100).ToUpperInvariant();
			set => SetNonPersistentPropertyValue(PlaceInfo, ref place, value);
		}

		ZString? place;

		public ZPropertyInfo PlaceInfo => GetZPropertyInfo(nameof(Place));

		#endregion

		#region EntryNumber

		[BusinessObjectMaxLengthTestExclude]
		public ZString EntryNumber
		{
			get => (entryNumber ?? (entryNumber = customsEndorsement.EntryNumber)).Value;
			set => SetNonPersistentPropertyValue(EntryNumberInfo, ref entryNumber, value);
		}

		ZString? entryNumber;

		public ZPropertyInfo EntryNumberInfo => GetZPropertyInfo(nameof(EntryNumber));

		[BusinessObjectMaxLengthTestExclude]
		public ZBool ShowEntryNumber
		{
			get => (showEntryNumber ?? (showEntryNumber = customsEndorsement.ShowEntryNumber)).Value;
			set => SetNonPersistentPropertyValue(ShowEntryNumberInfo, ref showEntryNumber, value);
		}

		ZBool? showEntryNumber;

		public ZPropertyInfo ShowEntryNumberInfo => GetZPropertyInfo(nameof(ShowEntryNumber));

		#endregion

		#region EUR1Pg1Box11TextEntryNumber 

		[BusinessObjectMaxLengthTestExclude]
		public ZString EUR1Pg1Box11TextEntryNumber
		{
			get => (eUR1Pg1Box11TextEntryNumber ?? (eUR1Pg1Box11TextEntryNumber = customsEndorsement.EUR1Pg1Box11TextEntryNumber)).Value;
			set => SetNonPersistentPropertyValue(EUR1Pg1Box11TextEntryNumberInfo, ref eUR1Pg1Box11TextEntryNumber, value);
		}

		ZString? eUR1Pg1Box11TextEntryNumber;

		public ZPropertyInfo EUR1Pg1Box11TextEntryNumberInfo => GetZPropertyInfo(nameof(EUR1Pg1Box11TextEntryNumber));

		#endregion

		#region ReferenceDate

		public ZDate ReferenceDate
		{
			get => (referenceDate ?? (referenceDate = customsEndorsement.ReferenceDate)).Value;
			set => SetNonPersistentPropertyValue(ReferenceDateInfo, ref referenceDate, value);
		}

		ZDate? referenceDate;

		public ZPropertyInfo ReferenceDateInfo => GetZPropertyInfo(nameof(ReferenceDate));

		#endregion
	}
}
