using System;
using System.Net;
using System.Net.Sockets;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.com.enett991;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	public class TestComPayRegisteredOrganisationsForm : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestFindButton_ClickHandleWebException()
		{
			var mock = new Mock<IeNettWebServiceClient> { CallBase = true };
			using (ObjectFactory.Substitute(mock.Object))
			{
				mock.Setup(m => m.DisplayClientList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
					.Throws(new WebException());

				using (ComPayRegisteredOrganisationsControl form = new ComPayRegisteredOrganisationsControl())
				{
					EnettRegistrationCode value = new EnettRegistrationCode();
					value.AuthenticationCode = "53333";
					value.OrganisationPK = Environment.Env.CurrentCompany.OrganisationPK;
					value.RegistrationCode = "4322234";
					AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
					form.FindButton_Click_ForTestOnly(null, null);
					mock.VerifyAll();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestFindButton_ClickHandleSocketException()
		{
			var mock = new Mock<IeNettWebServiceClient> { CallBase = true };
			using (ObjectFactory.Substitute(mock.Object))
			{
				mock.Setup(m => m.DisplayClientList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
					.Throws(new SocketException());

				using (ComPayRegisteredOrganisationsControl form = new ComPayRegisteredOrganisationsControl())
				{
					EnettRegistrationCode value = new EnettRegistrationCode();
					value.AuthenticationCode = "53333";
					value.OrganisationPK = Environment.Env.CurrentCompany.OrganisationPK;
					value.RegistrationCode = "4322234";
					AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
					form.FindButton_Click_ForTestOnly(null, null);
					mock.VerifyAll();
				}
			}
		}

		[ExpectNoExceptions]
		public void TestFindButton_ClickHandleInvalidOperationException()
		{
			var mock = new Mock<IeNettWebServiceClient> { CallBase = true };
			using (ObjectFactory.Substitute(mock.Object))
			{
				mock.Setup(m => m.DisplayClientList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
					.Throws(new InvalidOperationException());

				using (ComPayRegisteredOrganisationsControl form = new ComPayRegisteredOrganisationsControl())
				{
					EnettRegistrationCode value = new EnettRegistrationCode();
					value.AuthenticationCode = "53333";
					value.OrganisationPK = Environment.Env.CurrentCompany.OrganisationPK;
					value.RegistrationCode = "4322234";
					AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
					form.FindButton_Click_ForTestOnly(null, null);
					mock.VerifyAll();
				}
			}
		}

		[TestDate(2018, 03, 10)]
		public void TestFindButton_Click()
		{
			Response_GetClientList[] response = new Response_GetClientList[]
			{
				new Response_GetClientList { ABN = "123", ClientName = "Client Name 1", ECN = 1, RegistrationDate = new DateTime(2009, 01, 11), Address1 = "Addr1", Address2 = "Addr2", Suburb = "Sub", State = "Stat", Postcode = "53422", Country = "Panama", Phone = "+3234223", Fax = "+54334333", TerminalCode = "TERM" },
				new Response_GetClientList { ABN = "456", ClientName = "Client Name 2", ECN = 2 },
			};

			var mock = new Mock<IeNettWebServiceClient>(MockBehavior.Strict) { CallBase = true };
			using (ObjectFactory.Substitute(mock.Object))
			{
				mock.SetupProperty(m => m.Url, null);
				mock.SetupProperty(m => m.Proxy, null);
				mock.Setup(m => m.DisplayClientList(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
					.Returns(response);

				using (ComPayRegisteredOrganisationsControl form = new ComPayRegisteredOrganisationsControl())
				{
					EnettRegistrationCode value = new EnettRegistrationCode();
					value.AuthenticationCode = "53333";
					value.OrganisationPK = Environment.Env.CurrentCompany.OrganisationPK;
					value.RegistrationCode = "432rr2234";
					AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
					form.FindButton_Click_ForTestOnly(null, null);
					AssertEquals(0, form.DataSource_ForTestOnly.ComPayRegisteredOrganisations.Count);
					AssertEquals("Last message", "Registration Code is invalid or empty!", UnitTestUserNotification.Instance.LastMessage.Text);

					OrgCusCode cusCode = Factory.NewWithValidTestData<OrgCusCode>();
					cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
					cusCode.OK_CodeType = "ENE";
					cusCode.OK_CustomsRegNo = "1";
					cusCode.OK_OH = new ZGuid("C3F842EF-3BE5-448C-BED3-0017B232C624");
					Factory.Save();

					value.RegistrationCode = "4322234";
					AccountingConfigurationRegistry.Instance.ENettRegistration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
					form.FindButton_Click_ForTestOnly(null, null);
					AssertEquals(2, form.DataSource_ForTestOnly.ComPayRegisteredOrganisations.Count);
					AssertEquals("123", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].ABN);
					AssertEquals("Client Name 1", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].ClientName);
					AssertEquals(1, form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].ECN);
					AssertEquals(new DateTime(2009, 01, 11), form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].RegistrationDate);
					AssertEquals("Addr1", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].Address1);
					AssertEquals("Addr2", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].Address2);
					AssertEquals("Sub", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].Suburb);
					AssertEquals("Stat", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].State);
					AssertEquals("53422", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].Postcode);
					AssertEquals("Panama", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].Country);
					AssertEquals("+3234223", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].Phone);
					AssertEquals("+54334333", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].Fax);
					AssertEquals("TERM", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].TerminalCode);
					AssertEquals("JAYSCH", form.DataSource_ForTestOnly.ComPayRegisteredOrganisations[0].RelatedOrganisations);
				}
			}
		}
	}
}
