using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocContactPasswordInstructionEmail : DocBaseWrapper, Integration.DocumentWrappers.IDocContactPasswordInstructionEmail
	{
		DocContactPasswordInstructionEmail(IPasswordInstructionEmailSource source, BusinessObjectFactory factory)
			: base(source, factory)
		{
		}

		public static DocContactPasswordInstructionEmail New(IPasswordInstructionEmailSource objectToWrap, BusinessObjectFactory factory)
		{
			return new DocContactPasswordInstructionEmail(objectToWrap, factory);
		}

		[DocumentField("Contact Salutation")]
		public ZString ContactSalutation
		{
			get { return WrappedObject.Salutation; }
		}

		[DocumentField("Password Change Instructions")]
		public ZString PasswordChangeInstructions
		{
			get { return WrappedObject.ExtraInstruction; }
		}

		[DocumentField("Contact's Company Code")]
		public ZString CompanyCode
		{
			get { return WrappedObject.OrgCode; }
		}

		[DocumentField("Contact's Email")]
		public ZString ContactEmail
		{
			get { return WrappedObject.Email; }
		}

		[DocumentField("Your Company's Name")]
		public ZString CurrentCompanyName => CompanyForEmailTemplate?.CompanyName ?? CurrentCompany?.Name ?? ZString.Empty;

		GlbCompany CompanyForEmailTemplate
		{
			get
			{
				if (companyForEmailTemplate == null && WrappedObject.CompanyPKForEmailTemplate != Guid.Empty)
				{
					var factory = Factory ?? new BusinessObjectFactory();
					companyForEmailTemplate = factory.Load<GlbCompany>(WrappedObject.CompanyPKForEmailTemplate);
				}
				return companyForEmailTemplate;
			}
		}
		GlbCompany companyForEmailTemplate;

		[DocumentField("Mailbox Display Name")]
		public ZString MailboxDisplayName => Env.Registry.MailboxDisplayName;

		[DocumentField("Contact's Full Name")]
		public ZString ContactName => WrappedObject.Name;

		[DocumentField("Password Instruction Url")]
		public ZString PasswordInstructionUrl => WrappedObject.PasswordInstructionEmail?.PasswordInstructionUrl ?? ZString.Empty;

		public new IPasswordInstructionEmailSource WrappedObject => (IPasswordInstructionEmailSource)base.WrappedObject;
	}
}
