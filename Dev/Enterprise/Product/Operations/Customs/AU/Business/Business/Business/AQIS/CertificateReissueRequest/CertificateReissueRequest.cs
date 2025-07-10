using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CertificateReissueRequest : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CertificateReissueRequest(BusinessObjectFactory factory, CodeDescriptionPairList certificates)
			: base(factory)
		{
			Certificates = certificates;
		}

		#region Schema
		public static class Schema
		{
			public const string CertificateNumber = "CertificateNumber";
			public const string ReissueReason = "ReissueReason";
			public const int CertificateNumberMaxLength = 50;
			public const int ReissueReasonMaxLength = 500;
		}
		#endregion

		public bool IsValid => !CertificateNumber.IsEmpty && !ReissueReason.IsEmpty;

		#region CertificateNumber

		[MaxLength(Schema.CertificateNumberMaxLength)]
		[List(nameof(Certificates))]
		public ZString CertificateNumber
		{
			get { return fCertificateNumber; }
			set
			{
				SetNonPersistentPropertyValue(CertificateNumberInfo, ref fCertificateNumber, value);
				if (!IsValidationSuspended)
				{
					ValidateCertificateNumber();
				}
				CertificateNumberInfo.RefreshBinding();
			}
		}
		ZString fCertificateNumber;

		public ZPropertyInfo CertificateNumberInfo
		{
			get { return GetZPropertyInfo(Schema.CertificateNumber); }
		}

		void ValidateCertificateNumber()
		{
			CertificateNumberInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CertificateNumberInfo, Res.GetString("C0455107-D117-4572-ABEF-3DB574253753", "Certificate Number"));

			if (!CertificateNumber.IsEmpty)
			{
				ListValidation.WarnIfInvalidCode(CertificateNumberInfo, Certificates, (IMultilingualString)ResString.GetMultilingualString("3045E689-8BAD-4251-B038-2B817316B50F", "A Certificate matching this Number could not be found."));

				if (!Regex.Match(CertificateNumber, "^[a-zA-Z0-9]+$").Success)
				{
					CertificateNumberInfo.AddError(Res.GetString("8715A3A7-A657-4B3D-B3F1-617E996B85EA", "Certificate Number contains invalid character(s). Use only A-Z and 0-9, no spaces."));
				}

				var docInstances = ParentCollection?.Cast<CertificateReissueRequest>().Count(x => x.CertificateNumber == CertificateNumber) ?? 0;
				if (docInstances > 1)
				{
					CertificateNumberInfo.AddError(Res.GetString("18BD543F-2560-4FD1-A98A-BC783671665E", "Only one of each Certificate can be selected."));
				}
			}
		}

		CertificateReissueRequestCollection ParentCollection => ParentCollections.OfType<CertificateReissueRequestCollection>().FirstOrDefault();

		public CodeDescriptionPairList Certificates { get; private set; }

		#endregion

		#region ReissueReason

		[MaxLength(Schema.ReissueReasonMaxLength)]
		public ZString ReissueReason
		{
			get { return fReissueReason; }
			set
			{
				var shortenedReasonText = value.Left(ReissueReasonInfo.MaxLength);
				CheckMaximumLength(ReissueReasonInfo, shortenedReasonText);
				SetNonPersistentPropertyValue(ReissueReasonInfo, ref fReissueReason, shortenedReasonText);
				ValidateReissueReason();
				RefreshBinding();
			}
		}
		ZString fReissueReason;

		public ZPropertyInfo ReissueReasonInfo
		{
			get { return GetZPropertyInfo(Schema.ReissueReason); }
		}

		public void ValidateReissueReason()
		{
			if (!IsValidationSuspended)
			{
				ReissueReasonInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(ReissueReasonInfo, Res.GetString("D069B8C7-5517-4AE8-987D-19A1221603BF", "Reason"));
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAll();
		}

		void ValidateAll()
		{
			ValidateCertificateNumber();
			ValidateReissueReason();
		}

		#endregion
	}
}
