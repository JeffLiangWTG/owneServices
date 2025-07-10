using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class ExitControlConfiguration
{
	public static ExitControlConfiguration GetConfiguration(BusinessObjectFactory factory, string countryOrGrouping)
	{
		return factory.GetCachedValue(FormattableString.Invariant($"ExitControlConfiguration_{countryOrGrouping}"), () =>
		{
			object supporter = null;
			var builders = ObjectFactory.Get<Hashtable>("ExitControl.ExitControlConfiguration");
			if (!string.IsNullOrEmpty(countryOrGrouping))
			{
				var objectHandle = (ObjectHandle)builders[countryOrGrouping];
				supporter = objectHandle?.GetObject();
			}
			if (supporter == null)
			{
				var objectHandle = (ObjectHandle)builders[Core.Constants.CountryCodes.EuropeanUnion];
				supporter = objectHandle.GetObject();
			}
			return (ExitControlConfiguration)supporter;
		});
	}

	public CusExitConsignmentConfiguration CusExitConsignmentConfiguration => cusExitConsignmentConfiguration ??= GetNewCusExitConsignmentConfiguration();
	CusExitConsignmentConfiguration cusExitConsignmentConfiguration;

	public CusExitConsignmentItemConfiguration CusExitConsignmentItemConfiguration => cusExitConsignmentItemConfiguration ??= GetNewCusExitConsignmentItemConfiguration();
	CusExitConsignmentItemConfiguration cusExitConsignmentItemConfiguration;

	public CusExitConsignmentPackageConfiguration CusExitConsignmentPackageConfiguration => cusExitConsignmentPackageConfiguration ??= GetNewCusExitConsignmentPackageConfiguration();
	CusExitConsignmentPackageConfiguration cusExitConsignmentPackageConfiguration;

	public CusExitContainerConfiguration CusExitContainerConfiguration => cusExitContainerConfiguration ??= GetNewCusExitContainerConfiguration();
	CusExitContainerConfiguration cusExitContainerConfiguration;

	public CusExitReportConfiguration CusExitReportConfiguration => cusExitReportConfiguration ??= GetNewCusExitReportConfiguration();
	CusExitReportConfiguration cusExitReportConfiguration;

	public CusExitSealConfiguration CusExitSealConfiguration => cusExitSealConfiguration ??= GetNewCusExitSealConfiguration();
	CusExitSealConfiguration cusExitSealConfiguration;

	protected virtual CusExitConsignmentConfiguration GetNewCusExitConsignmentConfiguration() => new();

	protected virtual CusExitConsignmentItemConfiguration GetNewCusExitConsignmentItemConfiguration() => new();

	protected virtual CusExitConsignmentPackageConfiguration GetNewCusExitConsignmentPackageConfiguration() => new();

	protected virtual CusExitContainerConfiguration GetNewCusExitContainerConfiguration() => new();

	protected virtual CusExitReportConfiguration GetNewCusExitReportConfiguration() => new();

	protected virtual CusExitSealConfiguration GetNewCusExitSealConfiguration() => new();
}
