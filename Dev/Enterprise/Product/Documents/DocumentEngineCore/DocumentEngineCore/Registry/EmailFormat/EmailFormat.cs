using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class EmailFormat : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string Separator = "Separator";
			public const string Disclaimer = "Disclaimer";
		}

		#endregion

		#region BusinessObject Overrides

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			Separator = " - ";
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateSeparator();
			ValidateEmailSubjectFields();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EmailFormat();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			EmailFormat emailFormatClone = (EmailFormat)clone;
			if (fEmailSubjectFields != null)
			{
				emailFormatClone.fEmailSubjectFields = (EmailSubjectFieldCollection)fEmailSubjectFields.Clone(null, null);
			}
			if (fEmailSignatureFields != null)
			{
				emailFormatClone.fEmailSignatureFields = (EmailSignatureFieldCollection)fEmailSignatureFields.Clone(null, null);
			}
			emailFormatClone.RegisterEmailFields();
		}

		#endregion

		#region Bound Properties

		#region Separator
		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Separator
		{
			get
			{
				return fSeparator;
			}
			set
			{
				CheckMaximumLength(SeparatorInfo, value);
				fSeparator = value;
				SetPropertyValue(SeparatorInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateSeparator();
				}
				SeparatorInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo SeparatorInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.Separator);
			}
		}
		protected void ValidateSeparator()
		{
			SeparatorInfo.ClearAllNotifications();
			if (Separator.IsEmpty)
			{
				SeparatorInfo.AddError(Res.GetString("23fe8f17-c6bd-432d-9bc4-0380c78c243b", "Please enter a separator."));
			}
		}
		ZString fSeparator;
		#endregion

		#region Disclaimer
		[CargoWise.ComponentModel.MaxLength(2000)]
		public ZString Disclaimer
		{
			get
			{
				return fDisclaimer;
			}
			set
			{
				CheckMaximumLength(DisclaimerInfo, value);
				fDisclaimer = value;
				SetPropertyValue(DisclaimerInfo, value);
				DisclaimerInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo DisclaimerInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.Disclaimer);
			}
		}
		ZString fDisclaimer;
		#endregion

		#region EmailSubjectFields

		public EmailSubjectFieldCollection EmailSubjectFields
		{
			get
			{
				if (fEmailSubjectFields == null)
				{
					fEmailSubjectFields = GetDefaultEmailSubjectFields();
					RegisterEditableChildObject(fEmailSubjectFields);
				}
				return fEmailSubjectFields;
			}
		}
		EmailSubjectFieldCollection fEmailSubjectFields;

		protected void ValidateEmailSubjectFields()
		{
			if (EmailSubjectFields.Count == 0 ||
				EmailSubjectFields.Cast<EmailSubjectField>().All(s => string.IsNullOrEmpty(s.Code)))
			{
				this.AddRowError(Res.GetString("52B70E33-87EA-4466-9C1E-47407D3EE431", "Please set at least one element for email subject."));
			}
		}

		#endregion

		#region EmailSignatureFields
		public EmailSignatureFieldCollection EmailSignatureFields
		{
			get
			{
				if (fEmailSignatureFields == null)
				{
					fEmailSignatureFields = GetDefaultEmailSignatureFields();
					RegisterEditableChildObject(fEmailSignatureFields);
				}
				return fEmailSignatureFields;
			}
		}
		EmailSignatureFieldCollection fEmailSignatureFields;
		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Separator, Separator.ToString());
			writer.WriteElementString(Schema.Disclaimer, Disclaimer.ToString());
			ZXmlSerializer.New(typeof(EmailSubjectFieldCollection)).Serialize(writer, EmailSubjectFields);
			ZXmlSerializer.New(typeof(EmailSignatureFieldCollection)).Serialize(writer, EmailSignatureFields);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Separator = new ZString(reader.ReadElementString(Schema.Separator));
			Disclaimer = new ZString(reader.ReadElementString(Schema.Disclaimer));
			fEmailSubjectFields = (EmailSubjectFieldCollection)ZXmlSerializer.New(typeof(EmailSubjectFieldCollection)).Deserialize(reader);
			fEmailSignatureFields = (EmailSignatureFieldCollection)ZXmlSerializer.New(typeof(EmailSignatureFieldCollection)).Deserialize(reader);
			RegisterEmailFields();
		}

		#endregion

		#region Implementation

		void RegisterEmailFields()
		{
			if (fEmailSubjectFields != null)
			{
				RegisterEditableChildObject(fEmailSubjectFields);
			}
			if (fEmailSignatureFields != null)
			{
				RegisterEditableChildObject(fEmailSignatureFields);
			}
		}

		EmailSubjectFieldCollection GetDefaultEmailSubjectFields()
		{
			EmailSubjectFieldCollection result = new EmailSubjectFieldCollection();
			result.Add(new EmailSubjectField("1", Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName));
			result.Add(new EmailSubjectField("2", Core.Constants.EmailFormat.EmailFieldCodes.BranchName));
			result.Add(new EmailSubjectField("3", Core.Constants.EmailFormat.EmailFieldCodes.DocumentName));
			return result;
		}

		EmailSignatureFieldCollection GetDefaultEmailSignatureFields()
		{
			EmailSignatureFieldCollection result = new EmailSignatureFieldCollection();
			result.Add(new EmailSignatureField("1", Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName));
			result.Add(new EmailSignatureField("2", Core.Constants.EmailFormat.EmailFieldCodes.BranchName));
			result.Add(new EmailSignatureField("3", Core.Constants.EmailFormat.EmailFieldCodes.BranchAddress));
			result.Add(new EmailSignatureField("4", Core.Constants.EmailFormat.EmailFieldCodes.BranchPhone));
			result.Add(new EmailSignatureField("5", Core.Constants.EmailFormat.EmailFieldCodes.BranchFax));
			return result;
		}

		#endregion
	}
}
