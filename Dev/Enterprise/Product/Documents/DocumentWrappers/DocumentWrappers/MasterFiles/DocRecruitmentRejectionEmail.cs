using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentParsing;

namespace Enterprise.DocumentWrappers
{
	public interface IRecruitmentRejectionEmailDataSource
	{
		string CompanyName { get; }
		string FirstName { get; }
		Uri LinkedInURL { get; }
		string CompanySignatureLogoHtml { get; }
	}

	public class DocRecruitmentRejectionEmail : DocBaseWrapper, Integration.DocumentWrappers.IDocRecruitmentRejectionEmail
	{
		protected DocRecruitmentRejectionEmail(IRecruitmentRejectionEmailDataSource source, BusinessObjectFactory factory)
			: base(source, factory)
		{ }

		public static DocRecruitmentRejectionEmail New(IRecruitmentRejectionEmailDataSource objectToWrap, BusinessObjectFactory factory)
			=> new DocRecruitmentRejectionEmail(objectToWrap, factory);

		[DocumentField("Company Name")]
		public string CompanyName => WrappedObject.CompanyName;

		[DocumentField("First Name")]
		public string FirstName => WrappedObject.FirstName;

		[DocumentField("LinkedIn URL")]
		public Uri LinkedInURL => WrappedObject.LinkedInURL;

		[DocumentField("Company Logo HTML")]
		public string CompanySignatureLogoHtml => WrappedObject.CompanySignatureLogoHtml;

		public new IRecruitmentRejectionEmailDataSource WrappedObject => (IRecruitmentRejectionEmailDataSource)base.WrappedObject;
	}
}
