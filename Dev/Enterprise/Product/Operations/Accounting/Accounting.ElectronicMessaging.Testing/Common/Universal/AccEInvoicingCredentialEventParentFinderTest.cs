using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Moq;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing
{
	class AccEInvoicingCredentialEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParents()
		{
			using (ObjectFactory.Substitute(GlobalFactoryMock.Object))
			{
				var logParent = GetLogParents(GlbCompany.CurrentCompany.GC_Code, PasswordTypesList.Codes.EIM);
				AssertNotNull(logParent);
			}
		}

		public void TestGetLogParents_WhenCredentialDoesNotExist()
		{
			using (ObjectFactory.Substitute(GlobalFactoryMock.Object))
			{
				Factory.Load<GlbCompanyEInvoicingCertificateCredential>(new ZQuery()).DeleteAll();

				var logParent = GetLogParents(GlbCompany.CurrentCompany.GC_Code, PasswordTypesList.Codes.EIM);
				AssertNull(logParent);
			}
		}

		public void TestGetLogParents_InvalidCompanyCode()
		{
			using (ObjectFactory.Substitute(GlobalFactoryMock.Object))
			{
				var logParent1 = GetLogParents(string.Empty, PasswordTypesList.Codes.EIM);
				AssertNull(logParent1);

				var logParent2 = GetLogParents("123", PasswordTypesList.Codes.EIM);
				AssertNull(logParent2);
			}
		}

		public void TestGetLogParents_InvalidPasswordType()
		{
			using (ObjectFactory.Substitute(GlobalFactoryMock.Object))
			{
				var logParent1 = GetLogParents(GlbCompany.CurrentCompany.GC_Code, string.Empty);
				AssertNull(logParent1);

				var logParent2 = GetLogParents(GlbCompany.CurrentCompany.GC_Code, PasswordTypesList.Codes.EBD);
				AssertNull(logParent2);
			}
		}

		public void TestGetLogParents_NotImplementICountryEInvoicingObjectFactorySettings()
		{
			var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
			globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.IsAny<ZString>())).Returns((ICountryEInvoicingObjectFactorySettings)null);

			using (ObjectFactory.Substitute(globalFactoryMock.Object))
			{
				var settings = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().GetCountryEInvoicingObjectFactorySettings(string.Empty);
				AssertNull("Precondition", settings);

				var logParent = GetLogParents(GlbCompany.CurrentCompany.GC_Code, PasswordTypesList.Codes.EIM);
				AssertNull(logParent);
			}
		}

		public void TestGetLogParents_NotImplementIEInvoicingCredentialSettings()
		{
			var mockObjectFactorySetting = new Mock<ICountryEInvoicingObjectFactorySettings>();
			mockObjectFactorySetting.Setup(x => x.Credentials).Returns((IEInvoicingCredentialSettings)null);
			var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
			globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.IsAny<ZString>())).Returns(mockObjectFactorySetting.Object);

			using (ObjectFactory.Substitute(globalFactoryMock.Object))
			{
				var settings = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().GetCountryEInvoicingObjectFactorySettings(string.Empty);
				AssertNull("Precondition", settings.Credentials);

				var logParent = GetLogParents(GlbCompany.CurrentCompany.GC_Code, PasswordTypesList.Codes.EIM);
				AssertNull(logParent);
			}
		}

		public void TestGetLogParents_NotImplementIEInvoicingCredentialXUEBehaviorProvider()
		{
			var mockCredentialSettings = new Mock<IEInvoicingCredentialSettings>();
			var mockObjectFactorySetting = new Mock<ICountryEInvoicingObjectFactorySettings>();
			mockObjectFactorySetting.Setup(x => x.Credentials).Returns(mockCredentialSettings.Object);
			var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
			globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.IsAny<ZString>())).Returns(mockObjectFactorySetting.Object);

			using (ObjectFactory.Substitute(globalFactoryMock.Object))
			{
				var settings = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>().GetCountryEInvoicingObjectFactorySettings(string.Empty);
				AssertNotNull("Precondition", settings.Credentials);
				AssertNull("Precondition", settings.Credentials as IEInvoicingCredentialXUEBehaviorProvider);

				var logParent = GetLogParents(GlbCompany.CurrentCompany.GC_Code, PasswordTypesList.Codes.EIM);
				AssertNull(logParent);
			}
		}

		protected override void SetUp()
		{
			var credential = Factory.New<GlbCompanyEInvoicingCertificateCredential>();
			AssertEquals(PasswordTypesList.Codes.EIM, credential.GP_PasswordType);
			Factory.Save();
		}

		BusinessObject GetLogParents(string companyCode, string passwordType)
		{
			var universalEvent = SetupUniversalEvent(companyCode, passwordType);
			return ProcessUniversalEvent(universalEvent, out var logger);
		}

		UniversalEvent SetupUniversalEvent(string companyCode, string passwordType)
		{
			var eventDataObject = new UniversalEvent();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingCredential, string.Empty);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.ContextCollection = new List<Context>
			{
				!companyCode.IsNullOrEmpty() ? new Context() { Type = new ContextType() { Type = "CompanyCode" }, Value = companyCode } : null,
				!passwordType.IsNullOrEmpty() ? new Context() { Type = new ContextType() { Type = "PasswordType" }, Value = passwordType } : null,
			}.WhereNotNull().ToList();
			return eventDataObject;
		}

		BusinessObject ProcessUniversalEvent(UniversalEvent universalEvent, out XmlSessionTracker logger)
		{
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			var subscriber = new AccEInvoicingCredentialEventParentFinder(Factory, new AccEInvoicingCredentialDataContextManager(), logger);
			var logParents = subscriber.GetLogParentsForEvent(universalEvent);
			return logParents != null && logParents.Length > 0 ? logParents[0] : null;
		}

		Mock<IGlobalEInvoicingObjectFactory> GlobalFactoryMock
		{
			get
			{
				if (globalFactoryMock == null)
				{
					var mockCredentialSettings = new Mock<IEInvoicingCredentialSettings>();
					mockCredentialSettings.Setup(x => x.PasswordType).Returns(PasswordTypesList.Codes.EIM);
					mockCredentialSettings.As<IEInvoicingCredentialXUEBehaviorProvider>();

					var mockObjectFactorySetting = new Mock<ICountryEInvoicingObjectFactorySettings>();
					mockObjectFactorySetting.Setup(x => x.Credentials).Returns(mockCredentialSettings.Object);

					globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
					globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.IsAny<ZString>())).Returns(mockObjectFactorySetting.Object);
				}

				return globalFactoryMock;
			}
		}
		Mock<IGlobalEInvoicingObjectFactory> globalFactoryMock;
	}
}
