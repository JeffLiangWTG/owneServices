using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using static Enterprise.Freight.Integration.Agency;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskHelperTest : TestCaseWithFactory
	{
		public IForwardingShipment CreateNewShipment => Factory.New<IForwardingShipment>();

		public IForwardingConsol CreateNewConsolidation => Factory.New<IForwardingConsol>();

		public IQuotedBooking CreateNewBookingQuick => ObjectFactory.Get<IQuotedBookingBuilder>().CreateNew(QuoteBookingType.QuickBooking, Factory);

		public IQuotedBooking CreateNewBookingWithQuote => CreateNewBookingWithType(QuoteBookingType.BookingWithQuote);

		public IQuotedBooking CreateNewBookingSpotQuote => CreateNewBookingWithType(QuoteBookingType.SpotQuote);

		public IBaseJobDeclaration CreateNewBaseJobDeclaration => Factory.New<IBaseJobDeclaration>();

		IQuotedBooking CreateNewBookingWithType(QuoteBookingType bookingType) => (IQuotedBooking)ObjectFactory.GetType<IQuotedBooking>().
			InvokeMember("New",
			BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
			null, null, new object[] { bookingType, Factory });

		public IAgencyBooking CreateNewAgencyBooking => Factory.New<IAgencyBooking>();

		public IBillOfLading CreateNewBillOfLading => Factory.New<IBillOfLading>();

		public SecurityCore CreateNewSecurityCore => new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);

		public ComplianceRiskStatus CreateNewComplianceRiskStatus(BusinessObject hostBusinessEntity)
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = hostBusinessEntity.PK;
			complianceRiskStatus.COR_ParentTableCode = hostBusinessEntity.TablePrefix;
			return complianceRiskStatus;
		}

		public ComplianceRiskPlugInBusinessObject CreateComplianceRiskPlugInBusinessObject(IBusiness hostBusinessEntity, ComplianceRiskStatus complianceRiskStatus) => new ComplianceRiskPlugInBusinessObject(hostBusinessEntity, complianceRiskStatus);

		public class ComplianceRiskPlugInForTest : ComplianceRiskPlugIn
		{
			public ComplianceRiskPlugInForTest(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
			{
			}

			public void OverrideComplianceRiskStatusExposed(OverrideComplianceRiskConfirmationModel model) => OverrideComplianceRiskStatus(model);
		}

		public class FilterGridModuleForTest : BaseFilterGridModuleForTest
		{
			public FilterGridModuleForTest(BusinessObject[] bizos)
				: base(typeof(DeniedPartyScreeningFilterModuleStrategyTest.DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider))
			{
				DummyBizos = bizos;
			}

			public override BusinessObject[] GetSelectedBusinessObjects() => DummyBizos;

			BusinessObject[] DummyBizos { get; }
		}

		public class BaseFilterGridModuleForTest : DummyFilterGridModule
		{
			public BaseFilterGridModuleForTest(Type typeForTest)
			{
				typeOfTopLevelBusinessObjectForTest = typeForTest;
			}

			readonly Type typeOfTopLevelBusinessObjectForTest;

			protected override Type TypeOfTopLevelBusinessObjectCore => typeOfTopLevelBusinessObjectForTest;
		}

		public class ComplianceRiskPluginParentFormForTest : ZForm
		{
			public ComplianceRiskPluginParentFormForTest(object dataSource)
				: base(dataSource)
			{
				PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
				Controls.Add(TabControl);
				bwApiResponse = BorderWiseApiHelper.SetResponse(null);
			}

			public readonly ZTemplateTabControl TabControl = new ZTemplateTabControl();
			readonly IDisposable bwApiResponse;

			protected override ZTabControl TopLevelTabControl => TabControl;

			public MainMenu TopLevelMenu
			{
				get { return MainMenu; }
			}

			protected override void Dispose(bool disposing)
			{
				bwApiResponse.Dispose();
				TabControl.Dispose();
				base.Dispose(true);
			}
		}
	}
}
