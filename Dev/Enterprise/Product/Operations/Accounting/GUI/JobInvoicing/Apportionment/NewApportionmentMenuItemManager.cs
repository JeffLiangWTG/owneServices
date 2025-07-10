using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class NewApportionmentMenuItemManager
	{
		public NewApportionmentMenuItemManager(ZGrid grid, ApportionmentListing apportionmentListing)
		{
			Grid = grid;
			Apportionments = apportionmentListing;
		}

		readonly ZGrid Grid;
		readonly ApportionmentListing Apportionments;

		public void AddMenuItem()
		{
			if (Apportionments.CostsCollection.IsUsedForGateway)
			{
				var deleteGwSellApportionmentMenuItem = new ZMenuItem(ResString.GetMultilingualString("NewApportionmentUserControl|EDBB17E2-1D2E-44F7-9FE7-36C131DB3A3D", "Reverse/Delete Gateway Sell Apportionment"), DeleteGwSellApportionment_Click);
				Grid.ContextMenu.MenuItems.Add(deleteGwSellApportionmentMenuItem);
			}
		}

		void DeleteGwSellApportionment_Click(object sender, EventArgs args)
		{
			if (!Env.Security.GatewayConsolJobInvoicingReverseSellApportionment.IsAllowed)
			{
				Env.Security.GatewayConsolJobInvoicingReverseSellApportionment.ShowError();
				return;
			}

			var jobConsolCost = Grid?.ListManager?.GetCurrent() as JobConsolCost;

			if (jobConsolCost != null && !jobConsolCost.IsDeleted && !jobConsolCost.IsDeleting && jobConsolCost.IsGatewayConsolCost)
			{
				var jobCharges = jobConsolCost.Factory.Load<JobCharge>(new ZQuery(JobChargeSchema.JR_E6_GatewaySellHeader, jobConsolCost.PK));
				var aJRJPKs = jobCharges.Where(x => x.IsRevenuePostedWithAutoJobRevenueJournal).Select(x => x.ARLine.TransactionHeader.PK).Distinct().ToArray();
				JobRevenueJournal[] aJRJs = Array.Empty<JobRevenueJournal>();

				if (aJRJPKs.Any())
				{
					var newFactory = new BusinessObjectFactory();
					var aJRJQuery = new ZQuery(AccTransactionHeaderSchema.PK, aJRJPKs);
					aJRJQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
					aJRJs = newFactory.Load<JobRevenueJournal>(aJRJQuery);
				}

				if (aJRJs.Any()) //the reverser will clean up the charges
				{
					var controller = AccountingControllerCreator.GetNewController(aJRJs[0]);
					controller.SetFormsModalTo(Grid.FindForm());

					if (aJRJs.Length == 1)
					{
						controller.ShowDeleteForm(aJRJs[0]);
					}
					else
					{
						var multipleReversingProvider = new MultipleReversingProviderForHeader();
						multipleReversingProvider.BizObjectsForReversing.AddRange(aJRJs);

						foreach (BusinessObject bizo in multipleReversingProvider) //the enumeration is used to make the DeleteMultiple() work properly
						{
							if (bizo != null)
							{
								controller.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
							}
						}
					}
				}
				else //clean up charges
				{
					foreach (var jobCharge in jobCharges)
					{
						if (jobCharge.JR_E6.IsEmpty) //not a split charge
						{
							jobCharge.JR_E6_GatewaySellHeader = ZGuid.Empty;
							jobCharge.JR_OSSellAmt = 0m;
							jobCharge.JR_LocalSellAmt = 0m;
						}
					}

					jobConsolCost.ApportionmentCharges.RemoveAndDeleteAll();
					jobConsolCost.Delete();
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("1D5A3186-51B8-4D8A-8DCC-6A5D94C325AD", "Please click on a row before performing Reverse/Delete."));
			}
		}
	}
}
