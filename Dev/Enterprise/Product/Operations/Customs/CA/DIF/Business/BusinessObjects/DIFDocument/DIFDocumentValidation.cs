using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.MasterFiles.Business.DIS;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.Business
{
	public sealed class DIFDocumentValidation : AutoDIFDocumentValidation
	{
		public DIFDocumentValidation(AutoDIFDocument bizObj)
			: base(bizObj)
		{
		}

		new DIFDocument Parent
		{
			get { return (DIFDocument)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRequiredDocumentPK();
			ValidateURN();
		}

		protected override void CheckPGA()
		{
			base.CheckPGA();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PGAInfo);
		}

		protected override void CheckDocumentType()
		{
			base.CheckDocumentType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.DocumentTypeInfo);
		}

		protected override void CheckDocumentNumber()
		{
			base.CheckDocumentNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DocumentNumberInfo);
		}

		protected override void CheckBusinessNumber()
		{
			base.CheckBusinessNumber();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.BusinessNumberInfo);
		}

		public void ValidateRequiredDocumentPK()
		{
			ValidateCalculatedProperty(Parent.RequiredDocumentPKInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This method is implicitly referenced by ZAttribute")]
		void CheckRequiredDocumentPK()
		{
			MandatoryValidation.CheckEntered(Parent.RequiredDocumentPKInfo);
			ListValidation.ErrorIfInvalidPK(Parent.RequiredDocumentPKInfo);
		}

		public void ValidateURN()
		{
			ValidateCalculatedProperty(Parent.URNInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "This method is implicitly referenced by ZAttribute")]
		void CheckURN()
		{
			if (Parent.URN.IsEmpty)
			{
				var difHost = Parent.HostWrapper?.DISHost as IDISHost;
				var numberFountainStrategy = difHost?.DISReferenceNumberFountainStrategy;

				var state = numberFountainStrategy.CheckStateForAddingNewRecord();

				if (state.Severity == Integration.Customs.Shared.DISReferenceNumberFountainStrategyStateSeverity.Warning)
				{
					Parent.URNInfo.AddWarning(state.Message);
				}
				else if (state.Severity == Integration.Customs.Shared.DISReferenceNumberFountainStrategyStateSeverity.Error)
				{
					Parent.URNInfo.AddError(state.Message);
				}
			}
		}

		protected override void CheckEDocsDocumentPK()
		{
			base.CheckEDocsDocumentPK();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.EDocsDocumentPKInfo);

			if (!Parent.Lookups.EDocsList.Cast<CodeElement>().Any(x => x.PK.Equals(Parent.EDocsDocumentPK)))
			{
				Parent.EDocsDocumentPKInfo.AddMessageError(SelectValidEDocs);
			}
			else if (HasInvalidCharacters(Parent.EDoc.FileName))
			{
				Parent.EDocsDocumentPKInfo.AddMessageError(FileNameHasInvalidCharacters);
			}
			else
			{
				var eDoc = Parent.EDoc;
				if (eDoc != null)
				{
					if (eDoc.ImageData.Length > MaxmimumFileSizeInMB * 1024 * 1024 - 100000)
					{
						Parent.EDocsDocumentPKInfo.AddMessageError(FileSizeTooLarge);
					}
					if (!AvailableFileExtensions.Any(ext => eDoc.FileName.ToUpper().EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
					{
						Parent.EDocsDocumentPKInfo.AddMessageError(InvalidFileExtension);
					}
				}
			}
		}

		internal static bool HasInvalidCharacters(string fileName)
		{
			return !Regex.IsMatch(fileName.ToUpper(CultureInfo.CurrentCulture), NoneValidCharactersMatchPattern);
		}

		const string NoneValidCharactersMatchPattern = @"^[!@\#\$%\^&\*\(\)-_=\+\[\{\]}\\\|;:'"",<\.>/\?`~¢\ ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789]*$";

		internal static string SelectValidEDocs => Res.GetString("2E395D3A-A09F-481C-922E-0F8280955F84", "Please enter a valid eDocs.");

		internal static string FileNameHasInvalidCharacters => Res.GetString("410A5CAD-0FD5-494B-ACEA-958B751A0180", "This eDoc file name has invalid characters. You will not be able to send a message. Please rename the file, attach it to eDocs and use the file instead.");

		internal const int MaxmimumFileSizeInMB = 4;

		internal static string FileSizeTooLarge => Res.GetString("1B98AB43-EECE-49C0-81D4-48117B620FD2", "This eDoc file has to be less than 3.9 MB. ");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static string[] AvailableFileExtensions = new string[] { "DOC", "DOCX", "GIF", "PDF", "JPG", "PNG", "RTF", "BMP" };

		internal static string InvalidFileExtension => Res.GetString("E4259650-CD37-41A5-81EB-DF2287607625", "This eDoc file can only be the following file extensions: {0}", string.Join(", ", AvailableFileExtensions));

		protected override void CheckEffectiveDate()
		{
			base.CheckEffectiveDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.EffectiveDateInfo);
			if (IsExpiryDateLessThanEffectiveDate())
			{
				Parent.EffectiveDateInfo.AddMessageError(ExpiryDateCanNotBeLessThanEffectiveDate);
			}

			if (Parent.HostWrapper.DISHost.DateOfArrival.IsValid
				&& !StatusList.HasBeenLodgedAtCustoms(Parent.Status)
				&& Parent.EffectiveDate.Date > Parent.HostWrapper.DISHost.DateOfArrival
			)
			{
				Parent.EffectiveDateInfo.AddMessageError(EffectiveDateShouldBeEarlierThanOrSameDay);
			}

			Parent.Validation.ValidateExpiryDate();
		}

		protected override void CheckExpiryDate()
		{
			base.CheckExpiryDate();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ExpiryDateInfo);
			if (IsExpiryDateLessThanEffectiveDate())
			{
				Parent.ExpiryDateInfo.AddMessageError(ExpiryDateCanNotBeLessThanEffectiveDate);
			}

			if (Parent.HostWrapper.DISHost.DateOfArrival.IsValid
				&& !StatusList.HasBeenLodgedAtCustoms(Parent.Status)
				&& Parent.ExpiryDate.Date < Parent.HostWrapper.DISHost.DateOfArrival
			)
			{
				Parent.ExpiryDateInfo.AddMessageError(ExpiryDateShouldBeLaterThanOrSameDay);
			}
			Parent.Validation.ValidateEffectiveDate();
		}

		internal static string EffectiveDateShouldBeEarlierThanOrSameDay => Res.GetString(
			"D475904C-85A2-4E94-9CF3-7C21D41594AA",
			"Effective Date should be earlier than or equal to Declaration’s arrival date."
		);

		internal static string ExpiryDateShouldBeLaterThanOrSameDay => Res.GetString(
			"2FB8A79A-640E-4847-A86E-4CE9B895E3B8",
			"Expiry Date should be later than or equal to Declaration’s arrival date."
		);

		bool IsExpiryDateLessThanEffectiveDate() => Parent.EffectiveDate.IsValid && Parent.ExpiryDate.IsValid && Parent.ExpiryDate.Date < Parent.EffectiveDate.Date;

		internal static string ExpiryDateCanNotBeLessThanEffectiveDate => Res.GetString("909EB08D-954A-470E-8D17-8EA55B256D33", "The expiry date cannot be less than the effective date.");
	}
}
