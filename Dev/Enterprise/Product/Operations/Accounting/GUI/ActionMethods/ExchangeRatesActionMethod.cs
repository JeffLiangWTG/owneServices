using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Integration;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Accounting.GUI
{
	public sealed class ExchangeRatesActionMethod : OperationalActionMethod
	{
		public ExchangeRatesActionMethod(ExRateSourceType[] rateSources)
			: base(new ZGuid("E5521A08-7D88-4388-BD5A-E9AD208A4257"))
		{
			this.rateSources = rateSources;
		}

		public override string Name
		{
			get { return Res.GetString("10294cfb-29ce-4ee7-9015-e8e3c0d2fdba", "Update Ex Rates as per Defined Rules"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public override string Description
		{
			get
			{
				return
					Res.GetString(
					"ExchangeRateActionMethod|Description",
					"Updates the job exchange rates on all targets on which the " +
					"operational action is run. The new exchange rates can come from " +
					"the current sell rates or from various other module dependent " +
					"sources such as the related voyage exchange rates.");
			}
		}

		public IEnumerable<ExRateSourceType> RateSources
		{
			get { return rateSources; }
		}

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ExchangeRatesActionMethodApplicator((ExchangeRatesSettings)settings);
		}

		public override bool HasSettings
		{
			get { return true; }
		}

		public override OperationalActionMethodSettings NewSetting(BusinessObjectFactory factory)
		{
			return new ExchangeRatesSettings(rateSources);
		}

		public override IComponent NewSettingsControl()
		{
			return new ExchangeRatesSettingsControl();
		}

		readonly ExRateSourceType[] rateSources;
	}
}
