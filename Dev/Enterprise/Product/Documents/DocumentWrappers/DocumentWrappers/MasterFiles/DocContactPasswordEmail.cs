using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Business;
namespace Enterprise.DocumentWrappers
{
	public class DocContactPasswordEmail : DocBaseWrapper, Integration.DocumentWrappers.IDocContactPasswordEmail
	{
		DocContactPasswordEmail(IPasswordEmailSource source, BusinessObjectFactory factory)
			: base(source, factory)
		{
		}

		public static DocContactPasswordEmail New(IPasswordEmailSource objectToWrap, BusinessObjectFactory factory)
		{
			return new DocContactPasswordEmail(objectToWrap, factory);
		}

		[DocumentField("Contact Salutation")]
		public ZString ContactSalutation
		{
			get { return WrappedObject.Salutation; }
		}

		[DocumentField("Contact's Full Name")]
		public ZString ContactName
		{
			get { return WrappedObject.Name; }
		}

		[DocumentField("WebTracker URL")]
		public ZString WebTrackerURL
		{
			get { return string.Format("<a href=\"{0}\">{0}</a>", WrappedObject.Url); }
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

		[DocumentField("Contact's User Name")]
		public ZString UserName
		{
			get { return WrappedObject.Email; }
		}

		[DocumentField("Contact's Email")]
		public ZString ContactEmail
		{
			get { return WrappedObject.Email; }
		}

		[DocumentField("Contact's Password")]
		public ZString Password
		{
			get { return WrappedObject.Password; }
		}

		[DocumentField("Your Company's Name")]
		public ZString CurrentCompanyName
		{
			get { return base.CurrentCompany.Name; }
		}

		public new IPasswordEmailSource WrappedObject
		{
			get { return (IPasswordEmailSource)base.WrappedObject; }
		}
	}
}
