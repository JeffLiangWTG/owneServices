using System;
using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocContactPasswordEmail))]
	public class DocContactPasswordEmailTest : DocumentWrapperTestCase
	{
		public void TestContactSalutation()
		{
			AssertEquals(WrappedObject.Salutation, Wrapper.ContactSalutation);
		}

		public void TestContactName()
		{
			AssertEquals(WrappedObject.Name, Wrapper.ContactName);
		}

		public void TestWebTrackerURL()
		{
			AssertEquals(string.Format("<a href=\"{0}\">{0}</a>", WrappedObject.Url), Wrapper.WebTrackerURL);
		}

		public void TestPasswordChangeInstructions()
		{
			var expectedMessage = Parser.Parse(WrappedObject as OrgContact, WebDataRegistry.Instance.PasswordResetEmailTemplate.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty).EmailBody);
			AssertEquals(WrappedObject.ExtraInstruction, expectedMessage);
		}

		public void TestCompanyCode()
		{
			AssertEquals(WrappedObject.OrgCode, Wrapper.CompanyCode);
		}

		public void TestUserName()
		{
			AssertEquals(WrappedObject.Email, Wrapper.UserName);
		}

		public void TestPassword()
		{
			AssertEquals(WrappedObject.Password, Wrapper.Password);
		}

		public void TestCurrentCompanyName()
		{
			AssertEquals(GlbCompany.CurrentCompany.GC_Name, Wrapper.CurrentCompanyName);
		}

		#region Implementation

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			var source = Factory.NewWithValidTestData<PasswordEmailSource>();
			return DocContactPasswordEmail.New(source, Factory);
		}

		DocContactPasswordEmail Wrapper
		{
			get { return (DocContactPasswordEmail)base.Wrappers[0]; }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var source = Factory.NewWithValidTestData<PasswordEmailSource>();
			return new DocumentWrapper[] { DocContactPasswordEmail.New(source, Factory) };
		}

		IPasswordEmailSource WrappedObject
		{
			get { return Wrapper.WrappedObject; }
		}

		DocumentParser Parser
		{
			get
			{
				if (parser == null)
				{
					parser = DocumentParser.New(ObjectFactory.GetType<Integration.DocumentWrappers.IDocContactPasswordInstructionEmail>(), Factory);
				}
				return parser;
			}
		}
		DocumentParser parser;

		#endregion

		class PasswordEmailSource : OrgContact, IPasswordEmailSource, IContactBase
		{
			public PasswordEmailSource(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			ZString IPasswordEmailSource.Password => "pass1";
			ZString IPasswordEmailSource.Url => "www.wisetechglobal.com";
			ZString IPasswordEmailSource.OrgCode => "WISGLOSYD";
			ZString IPasswordEmailSource.ExtraInstruction => Parser.Parse(this, WebDataRegistry.Instance.PasswordResetEmailTemplate.GetFallBackValueAtAllLevels(CompanyPKForLogin, Guid.Empty, Guid.Empty).EmailBody);
			ZString IPasswordEmailSource.Salutation => "Dear";
			string IContactBase.Name => "Jenny Nguyen";
			string IContactBase.Email => "Jenny.Nguyen@wisetechglobal.com";

			DocumentParser Parser
			{
				get
				{
					if (parser == null)
					{
						parser = DocumentParser.New(ObjectFactory.GetType<Integration.DocumentWrappers.IDocContactPasswordInstructionEmail>(), Factory);
					}
					return parser;
				}
			}
			DocumentParser parser;
		}
	}
}
