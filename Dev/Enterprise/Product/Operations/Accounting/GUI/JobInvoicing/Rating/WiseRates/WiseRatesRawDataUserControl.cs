using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Rating.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class WiseRatesRawDataUserControl : ZUserControl
	{
		public WiseRatesRawDataUserControl()
		{
			InitializeComponent();
		}

		void WiseRatesDataButton_Click(object sender, EventArgs e)
		{
			BusinessObjectFactory factory = null;
			ZGuid? key = null;
			if (CurrentDataItem is Job job)
			{
				factory = job.Factory;
				key = job.Parent.PK;
			}
			else if (CurrentDataItem is ApportionmentListing apportionmentListing)
			{
				factory = apportionmentListing.Factory;
				key = apportionmentListing.ConsolPK;
			}

			if (factory != null && key != null)
			{
				var cache = factory.GetCachedValue("AutoRatingStarerCore.RateCharges.RawResponse", () => new Dictionary<ZGuid, string>());

				if (cache.TryGetValue(key.Value, out string rawResponses))
				{
					WiseRatesGUIHelper.ViewJSONWithDeveloperAuthentication(rawResponses);
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("7CB124DF-FAEA-4388-8CC0-0DD15C13C18D", "Please perform Autorating in this session before you can see Raw Data from Rates Service. This information is available until the form is closed."));
				}
			}
		}
	}
}
