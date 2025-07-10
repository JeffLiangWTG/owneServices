namespace CargoWise.Bi.Deployment.ReportingServices
{
	using System;
	using System.Linq;
	using CargoWise.Data;
	using Enterprise.Integration.Licensing;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Environment;
	using WTG.StaticAnalysis.Annotation;

	public interface IPowerBiReportLinkBuilderFactory
	{
		PowerBiReportLinkBuilder NewLinkBuilder(IProductRegistration registration, Uri powerBiPortalUrl);
	}

	public abstract class PowerBiReportLinkBuilder
	{
		protected PowerBiReportLinkBuilder(IProductRegistration registration, Uri powerBiPortalUrl)
		{
			this.enterpriseCode = registration.Key.EnterpriseCode;
			this.serverCode = registration.Key.ServerCode;
			this.powerBiPortalUrl = powerBiPortalUrl; // SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value;

#if DEBUG
			if (registration.LocalVerify() != ProductRegistrationVerifyResult.OK)
			{
				this.serverCode += @"_" + Db.DatabaseName;
			}
#endif
		}

		public abstract string GetReportPath(Registration.PowerBi.PowerBiItem report);
		protected readonly string enterpriseCode;
		protected readonly string serverCode;
		protected readonly string currentCompanyCode = (EnvProxy.Instance.CurrentUser as Enterprise.MasterFiles.Business.GlbStaff)?.HomeBranch?.Company.GC_Code;
		protected readonly Uri powerBiPortalUrl;
	}

	[CodeAlive("Instance is created by GlowPowerBiReportLinkBuilder.Factory")]
	public class GlowPowerBiReportLinkBuilder : PowerBiReportLinkBuilder
	{
		public class Factory : IPowerBiReportLinkBuilderFactory
		{
			public PowerBiReportLinkBuilder NewLinkBuilder(IProductRegistration registration, Uri powerBiPortalUrl)
			{
				return new GlowPowerBiReportLinkBuilder(registration, powerBiPortalUrl);
			}
		}

		public GlowPowerBiReportLinkBuilder(IProductRegistration registration, Uri powerBiPortalUrl) : base(registration, powerBiPortalUrl)
		{
			this.powerBiPortal = powerBiPortalUrl.Segments.LastOrDefault();
		}

		public override string GetReportPath(Registration.PowerBi.PowerBiItem report)
		{
			return FormattableString.Invariant($"{glowURL}cw1api/analytics/loadReport/{powerBiPortal}/{report.ResourceType}/{enterpriseCode}/{serverCode}/Analytics/{report.BusinessArea}/{report.Name}?rs:Embed=true");  // reports Path string
		}

		readonly string powerBiPortal;
		readonly string glowURL = GlowRegistry.Instance.GlowServiceUriRegistryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
	}

	[CodeAlive("Instance is created by WinFormPowerBiReportLinkBuilder.Factory")]
	public class WinFormPowerBiReportLinkBuilder : PowerBiReportLinkBuilder
	{
		public class Factory : IPowerBiReportLinkBuilderFactory
		{
			public PowerBiReportLinkBuilder NewLinkBuilder(IProductRegistration registration, Uri powerBiPortalUrl)
			{
				return new WinFormPowerBiReportLinkBuilder(registration, powerBiPortalUrl);
			}
		}

		public WinFormPowerBiReportLinkBuilder(IProductRegistration registration, Uri powerBiPortalUrl) : base(registration, powerBiPortalUrl) { }

		public override string GetReportPath(Registration.PowerBi.PowerBiItem report)
		{
			return FormattableString.Invariant($"{powerBiPortalUrl}/{report.ResourceType}/{enterpriseCode}/{serverCode}/Analytics/{report.BusinessArea}/{report.Name}?rs:Embed=true");  // reports Path string
		}
	}
}
